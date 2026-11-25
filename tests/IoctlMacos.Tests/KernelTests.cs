using System.Net.Sockets;
using IoctlMacos.Requests;
using Microsoft.Win32.SafeHandles;

namespace IoctlMacos.Tests;

/// <summary>Настоящие вызовы ядра macOS.</summary>
public class KernelTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(300)]
    public void Get_Pipe_ReturnsPendingBytes(int count)
    {
        var (read, write) = Posix.CreatePipe();
        using (read)
        using (write)
        {
            Posix.WriteBytes(write, count);
            Assert.Equal(count, new IoctlDevice(read).Get(FileRequests.BytesAvailable));
        }
    }

    [Fact]
    public void Set_WindowSize_IsVisibleToGet()
    {
        var (master, slave) = Posix.CreatePty();
        using (master)
        using (slave)
        {
            var expected = new WindowSize(42, 137, 1111, 2222);
            new IoctlDevice(slave).Set(TtyRequests.SetWindowSize, expected);
            Assert.Equal(expected, new IoctlDevice(master).Get(TtyRequests.GetWindowSize));
        }
    }

    [Fact]
    public void Exchange_Loopback_ReturnsMtu()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        var request = new IoctlReadWrite<InterfaceMtu>('i', 51); // SIOCGIFMTU
        InterfaceMtu reply = new IoctlDevice(socket.SafeHandle).Exchange(request, InterfaceMtu.For("lo0"));

        int expected = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
            .Single(i => i.Name == "lo0").GetIPProperties().GetIPv4Properties().Mtu;
        Assert.Equal(expected, reply.Mtu);
    }

    [Fact]
    public void Get_InvalidDescriptor_ThrowsEBADF()
    {
        using var handle = new SafeFileHandle(-1, ownsHandle: false);
        var device = new IoctlDevice(handle);
        Assert.Equal(9, Assert.Throws<IoctlException>(() => device.Get(FileRequests.BytesAvailable)).Errno);
    }

    [Fact]
    public void Get_WindowSizeOfPipe_ThrowsENOTTY()
    {
        var (read, write) = Posix.CreatePipe();
        using (read)
        using (write)
        {
            var device = new IoctlDevice(read);
            Assert.Equal(25, Assert.Throws<IoctlException>(() => device.Get(TtyRequests.GetWindowSize)).Errno);
        }
    }

    [Fact]
    public void Get_ClosedHandle_ThrowsObjectDisposed()
    {
        var (read, write) = Posix.CreatePipe();
        write.Dispose();
        read.Dispose();
        Assert.Throws<ObjectDisposedException>(() => new IoctlDevice(read).Get(FileRequests.BytesAvailable));
    }
}
