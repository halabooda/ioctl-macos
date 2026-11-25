using System.Net.Sockets;
using System.Runtime.InteropServices;
using IoctlMacos.Abi;
using Microsoft.Win32.SafeHandles;

namespace IoctlMacos;

/// <summary>Выполняет запросы ioctl на дескрипторе. Дескриптором владеет вызывающий код.</summary>
/// <remarks>
/// EINTR не повторяется: не все запросы можно безопасно выполнить дважды.
/// Неотрицательный результат ioctl отбрасывается.
/// </remarks>
public sealed class IoctlDevice
{
    private readonly SafeHandle _handle;
    private readonly IIoctlAbi _abi;

    /// <summary>Создаёт устройство поверх файла, канала или терминала.</summary>
    /// <param name="handle">Открытый дескриптор.</param>
    public IoctlDevice(SafeFileHandle handle) : this(handle, IoctlAbi.Current)
    {
    }

    /// <summary>Создаёт устройство поверх сокета.</summary>
    /// <param name="handle">Открытый сокет.</param>
    public IoctlDevice(SafeSocketHandle handle) : this(handle, IoctlAbi.Current)
    {
    }

    internal IoctlDevice(SafeHandle handle, IIoctlAbi abi)
    {
        ArgumentNullException.ThrowIfNull(handle);
        _handle = handle;
        _abi = abi;
    }

    /// <summary>Читает значение из ядра.</summary>
    /// <typeparam name="T">Тип аргумента.</typeparam>
    /// <param name="request">Запрос _IOR.</param>
    /// <returns>Значение, которое записало ядро.</returns>
    /// <exception cref="IoctlException">Ядро вернуло ошибку.</exception>
    public T Get<T>(IoctlRead<T> request) where T : unmanaged
    {
        T value = default;
        Invoke(request, ref value);
        return value;
    }

    /// <summary>Передаёт значение ядру.</summary>
    /// <typeparam name="T">Тип аргумента.</typeparam>
    /// <param name="request">Запрос _IOW.</param>
    /// <param name="value">Значение для ядра.</param>
    /// <exception cref="IoctlException">Ядро вернуло ошибку.</exception>
    public void Set<T>(IoctlWrite<T> request, T value) where T : unmanaged => Invoke(request, ref value);

    /// <summary>Передаёт значение ядру и возвращает ответ.</summary>
    /// <typeparam name="T">Тип аргумента.</typeparam>
    /// <param name="request">Запрос _IOWR.</param>
    /// <param name="value">Значение для ядра.</param>
    /// <returns>Значение, которое записало ядро.</returns>
    /// <exception cref="IoctlException">Ядро вернуло ошибку.</exception>
    public T Exchange<T>(IoctlReadWrite<T> request, T value) where T : unmanaged
    {
        Invoke(request, ref value);
        return value;
    }

    private void Invoke<T>(IoctlRequest<T> request, ref T value) where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(request);

        Span<byte> bytes = MemoryMarshal.AsBytes(new Span<T>(ref value));
        if (_abi.Invoke(_handle, new CULong(request.Code), ref bytes[0]) == -1)
            throw new IoctlException(request.Code, Marshal.GetLastPInvokeError());
    }
}
