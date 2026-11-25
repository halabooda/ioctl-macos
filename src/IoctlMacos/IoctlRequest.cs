using System.Runtime.InteropServices;

namespace IoctlMacos;

/// <summary>
/// Запрос ioctl, связанный с типом аргумента. Код собирается как в
/// &lt;sys/ioccom.h&gt;, размер берётся из <typeparamref name="T"/>. Порядок и смещения
/// полей должны совпадать с C-структурой, за это отвечает автор типа.
/// </summary>
/// <remarks>Запросы без аргумента (_IO) не поддерживаются.</remarks>
/// <typeparam name="T">Аргумент с раскладкой Sequential или Explicit, в том числе у вложенных структур.</typeparam>
public abstract class IoctlRequest<T> where T : unmanaged
{
    private const int MaxSize = 0x1fff; // IOCPARM_MASK

    private protected IoctlRequest(uint direction, char group, byte number)
    {
        // У enum раскладка формально Auto, но поле одно, поэтому порядок однозначен.
        if (typeof(T).IsAutoLayout && !typeof(T).IsEnum)
            throw new ArgumentException($"{typeof(T)}: нужна раскладка Sequential или Explicit.");

        T probe = default;
        int size = MemoryMarshal.AsBytes(new Span<T>(ref probe)).Length;
        if (size > MaxSize)
            throw new ArgumentException($"{typeof(T)} занимает {size} байт, ioctl допускает до {MaxSize}.");

        ArgumentOutOfRangeException.ThrowIfGreaterThan((int)group, byte.MaxValue, nameof(group));

        Code = direction | ((uint)size << 16) | ((uint)group << 8) | number;
    }

    /// <summary>Код запроса, который получает ядро.</summary>
    public nuint Code { get; }
}

/// <summary>_IOR: ядро заполняет аргумент.</summary>
/// <typeparam name="T">Тип аргумента.</typeparam>
/// <param name="group">Группа запроса, например 't' для терминалов.</param>
/// <param name="number">Номер запроса в группе.</param>
public sealed class IoctlRead<T>(char group, byte number)
    : IoctlRequest<T>(0x4000_0000, group, number) where T : unmanaged;

/// <summary>_IOW: ядро читает аргумент.</summary>
/// <typeparam name="T">Тип аргумента.</typeparam>
/// <param name="group">Группа запроса, например 't' для терминалов.</param>
/// <param name="number">Номер запроса в группе.</param>
public sealed class IoctlWrite<T>(char group, byte number)
    : IoctlRequest<T>(0x8000_0000, group, number) where T : unmanaged;

/// <summary>_IOWR: ядро читает аргумент и записывает результат обратно.</summary>
/// <typeparam name="T">Тип аргумента.</typeparam>
/// <param name="group">Группа запроса, например 'i' для сетевых интерфейсов.</param>
/// <param name="number">Номер запроса в группе.</param>
public sealed class IoctlReadWrite<T>(char group, byte number)
    : IoctlRequest<T>(0xC000_0000, group, number) where T : unmanaged;
