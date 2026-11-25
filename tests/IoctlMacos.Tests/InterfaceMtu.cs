using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace IoctlMacos.Tests;

/// <summary>struct ifreq с полем ifru_mtu, 32 байта.</summary>
[StructLayout(LayoutKind.Sequential, Size = 32)]
internal struct InterfaceMtu
{
    private InterfaceName _name;

    public int Mtu;

    public static InterfaceMtu For(string name)
    {
        var request = new InterfaceMtu();
        Encoding.ASCII.GetBytes(name, request._name);
        return request;
    }

    [InlineArray(16)]
    private struct InterfaceName
    {
        private byte _first;
    }
}
