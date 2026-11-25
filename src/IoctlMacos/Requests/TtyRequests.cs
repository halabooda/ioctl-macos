using System.Runtime.InteropServices;

namespace IoctlMacos.Requests;

/// <summary>struct winsize</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct WindowSize(ushort Rows, ushort Columns, ushort XPixels, ushort YPixels);

/// <summary>&lt;sys/ttycom.h&gt;</summary>
public static class TtyRequests
{
    /// <summary>TIOCGWINSZ</summary>
    public static readonly IoctlRead<WindowSize> GetWindowSize = new('t', 104);

    /// <summary>TIOCSWINSZ</summary>
    public static readonly IoctlWrite<WindowSize> SetWindowSize = new('t', 103);
}
