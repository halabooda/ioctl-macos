using System.Runtime.InteropServices;

namespace IoctlMacos.Abi;

/// <summary>Соглашение о вызове variadic-функции ioctl на конкретной архитектуре.</summary>
internal interface IIoctlAbi
{
    /// <summary>Возвращает результат ioctl; при -1 errno лежит в Marshal.GetLastPInvokeError().</summary>
    int Invoke(SafeHandle handle, CULong request, ref byte argument);
}
