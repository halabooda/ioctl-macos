using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace IoctlMacos.Tests;

/// <summary>Настоящие дескрипторы для тестов. Все функции не variadic.</summary>
internal static class Posix
{
    private const string LibSystem = "/usr/lib/libSystem.dylib";

    public static (SafeFileHandle Read, SafeFileHandle Write) CreatePipe()
    {
        var fds = new int[2];
        Check(Pipe(fds));
        return (Own(fds[0]), Own(fds[1]));
    }

    public static (SafeFileHandle Master, SafeFileHandle Slave) CreatePty()
    {
        Check(OpenPty(out int master, out int slave, 0, 0, 0));
        return (Own(master), Own(slave));
    }

    public static void WriteBytes(SafeFileHandle handle, int count) =>
        Assert.Equal(count, (int)Write(handle, new byte[count], (nuint)count));

    private static SafeFileHandle Own(int fd) => new(fd, ownsHandle: true);

    private static void Check(int result)
    {
        if (result == -1)
            throw new IOException(Marshal.GetLastPInvokeErrorMessage());
    }

    [DllImport(LibSystem, EntryPoint = "pipe", ExactSpelling = true, SetLastError = true)]
    private static extern int Pipe([Out] int[] fds);

    [DllImport(LibSystem, EntryPoint = "openpty", ExactSpelling = true, SetLastError = true)]
    private static extern int OpenPty(out int master, out int slave, nint name, nint termios, nint winsize);

    [DllImport(LibSystem, EntryPoint = "write", ExactSpelling = true, SetLastError = true)]
    private static extern nint Write(SafeHandle handle, byte[] buffer, nuint count);
}
