using System.Runtime.InteropServices;

namespace IoctlMacos.Abi;

/// <summary>
/// System V x86-64: variadic-указатель идёт в rdx, как обычный аргумент. Регистр AL
/// (число векторных аргументов) P/Invoke не задаёт, но ioctl их и не принимает.
/// </summary>
internal sealed class X64Abi : IIoctlAbi
{
    public int Invoke(SafeHandle handle, CULong request, ref byte argument) =>
        Ioctl(handle, request, ref argument);

    [DllImport("/usr/lib/libSystem.dylib", EntryPoint = "ioctl", ExactSpelling = true, SetLastError = true)]
    private static extern int Ioctl(SafeHandle handle, CULong request, ref byte argument);
}
