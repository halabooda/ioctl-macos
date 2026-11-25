# ioctl-macos

Вызов `ioctl` на macOS из C#: чистый P/Invoke, без `unsafe` и без прослойки на C. Работает в процессах arm64 и x64.

```csharp
using IoctlMacos;
using IoctlMacos.Requests;
using Microsoft.Win32.SafeHandles;

using var stdout = new SafeFileHandle(1, ownsHandle: false);
WindowSize size = new IoctlDevice(stdout).Get(TtyRequests.GetWindowSize);
Console.WriteLine($"{size.Columns}x{size.Rows}");
```

## В чём сложность

В C функция объявлена с переменным числом аргументов:

```c
int ioctl(int, unsigned long, ...);
```

На Apple Silicon такие аргументы передаются через стек, а P/Invoke кладёт третий аргумент в регистр `x2`. `ioctl` берёт со стека мусор, и вызов падает, возвращает ошибку или портит память.

Решение: шесть пустых аргументов занимают регистры `x2`–`x7`, и настоящий оказывается первым на стеке.

```csharp
[DllImport("/usr/lib/libSystem.dylib", EntryPoint = "ioctl", ExactSpelling = true, SetLastError = true)]
private static extern int Ioctl(
    SafeHandle handle, CULong request,                  // x0, x1
    nint x2, nint x3, nint x4, nint x5, nint x6, nint x7,
    ref byte argument);                                 // [sp]
```

На x64 третий аргумент и так передаётся в регистре `rdx`, там вызов обычный. Нужный вариант выбирается по архитектуре процесса.

> Приём официально не документирован. Поддержка таких вызовов в .NET обсуждается в [dotnet/runtime#48796](https://github.com/dotnet/runtime/issues/48796).

## Использование

| Метод | Запрос | Тип |
|---|---|---|
| `Get` | `_IOR` | `IoctlRead<T>` |
| `Set` | `_IOW` | `IoctlWrite<T>` |
| `Exchange` | `_IOWR` | `IoctlReadWrite<T>` |

Запрос знает тип своего аргумента, поэтому перепутать тип или направление не даст компилятор.

Готовые запросы: `TtyRequests.GetWindowSize`, `TtyRequests.SetWindowSize`, `FileRequests.BytesAvailable`.

Свой запрос описывается одной строкой, группа и номер берутся из заголовков macOS SDK:

```csharp
// TIOCOUTQ: _IOR('t', 115, int)
var outputQueue = new IoctlRead<int>('t', 115);
int bytes = new IoctlDevice(handle).Get(outputQueue);
```

Структура должна повторять раскладку C-структуры (`Sequential` или `Explicit`). Библиотека сверяет с лимитом размер и отвергает `LayoutKind.Auto`, но порядок полей проверить не может.

Ошибка ядра приходит как `IoctlException` с полями `Errno` и `Code`.

## Сборка и тесты

Нужны macOS и Docker. .NET на Mac ставить не нужно: контейнер собирает, а тесты запускаются на macOS.

```sh
./test.sh            # архитектура этого Mac
./test.sh osx-x64    # x64, на Apple Silicon через Rosetta
```

Тесты вызывают настоящее ядро (pipe, pty, MTU `lo0`), сверяют коды запросов с SDK и проверяют логику на подставном ABI.

Проверено на macOS 26.2, .NET SDK 10.0.404: arm64 и x64 через Rosetta. На Mac с Intel не проверялось.

## Ограничения

- Запросы без аргумента (`_IO`) и с аргументом по значению не поддерживаются.
- `EINTR` не повторяется: не все запросы безопасно выполнять дважды.
- Неотрицательный результат `ioctl` отбрасывается.
- `LibraryImport` не используется: его генератор требует `AllowUnsafeBlocks`.

## Структура

```
src/IoctlMacos/
  IoctlDevice.cs     Get, Set, Exchange
  IoctlRequest.cs    типизированные запросы
  Abi/               вызов для arm64 и x64
  Requests/          готовые запросы
tests/               xUnit v3
```
