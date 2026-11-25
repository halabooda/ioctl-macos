using System.Runtime.InteropServices;

namespace IoctlMacos.Abi;

/// <summary>
/// Apple ARM64: variadic-аргументы передаются только через стек. Пустые аргументы
/// занимают регистры x2..x7, и настоящий аргумент становится первым на стеке.
/// </summary>
internal sealed class Arm64Abi : IIoctlAbi
{
    public int Invoke(SafeHandle handle, CULong request, ref byte argument) =>
        Ioctl(handle, request, 0, 0, 0, 0, 0, 0, ref argument);

    [DllImport("/usr/lib/libSystem.dylib", EntryPoint = "ioctl", ExactSpelling = true, SetLastError = true)]
    private static extern int Ioctl(
        SafeHandle handle, CULong request,                  // x0, x1
        nint x2, nint x3, nint x4, nint x5, nint x6, nint x7,
        ref byte argument);                                 // [sp]
}
