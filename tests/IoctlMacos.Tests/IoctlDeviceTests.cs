using System.Net.Sockets;
using System.Runtime.InteropServices;
using IoctlMacos.Abi;
using IoctlMacos.Requests;
using Microsoft.Win32.SafeHandles;

namespace IoctlMacos.Tests;

/// <summary>Логика IoctlDevice с подставным ABI, без системных вызовов.</summary>
public class IoctlDeviceTests
{
    private static readonly SafeFileHandle Handle = new(-1, ownsHandle: false);
    private static readonly IoctlReadWrite<int> Increment = new('x', 1);

    [Fact]
    public void Get_KernelWritesValue_ReturnsIt()
    {
        var abi = new FakeAbi { Reply = 7 };
        Assert.Equal(7, new IoctlDevice(Handle, abi).Get(FileRequests.BytesAvailable));
        Assert.Equal(FileRequests.BytesAvailable.Code, abi.Request);
    }

    [Fact]
    public void Set_Value_PassesItToKernel()
    {
        var abi = new FakeAbi();
        new IoctlDevice(Handle, abi).Set(new IoctlWrite<int>('x', 2), 99);
        Assert.Equal(99, abi.Received);
    }

    [Fact]
    public void Exchange_Value_PassesItAndReturnsReply()
    {
        var abi = new FakeAbi { Reply = 6 };
        Assert.Equal(6, new IoctlDevice(Handle, abi).Exchange(Increment, 5));
        Assert.Equal(5, abi.Received);
    }

    [Fact]
    public void Get_KernelFails_ThrowsWithCodeAndErrno()
    {
        var device = new IoctlDevice(Handle, new FakeAbi { Errno = 25 });
        var error = Assert.Throws<IoctlException>(() => device.Get(FileRequests.BytesAvailable));
        Assert.Equal(25, error.Errno);
        Assert.Equal(FileRequests.BytesAvailable.Code, error.Code);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(-2)]
    public void Get_ResultIsNotMinusOne_DoesNotThrow(int result) =>
        new IoctlDevice(Handle, new FakeAbi { Result = result }).Get(FileRequests.BytesAvailable);

    [Fact]
    public void AnyMember_NullArgument_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new IoctlDevice((SafeFileHandle)null!));
        Assert.Throws<ArgumentNullException>(() => new IoctlDevice((SafeSocketHandle)null!));
        Assert.Throws<ArgumentNullException>(() => new IoctlDevice(Handle, new FakeAbi()).Get<int>(null!));
    }

    private sealed class FakeAbi : IIoctlAbi
    {
        public int? Reply { get; init; }
        public int Errno { get; init; }
        public int Result { get; init; }
        public nuint Request { get; private set; }
        public int Received { get; private set; }

        public int Invoke(SafeHandle handle, CULong request, ref byte argument)
        {
            Span<byte> bytes = MemoryMarshal.CreateSpan(ref argument, sizeof(int));
            Request = request.Value;
            Received = MemoryMarshal.Read<int>(bytes);
            if (Reply is { } reply)
                MemoryMarshal.Write(bytes, reply);
            if (Errno == 0)
                return Result;

            Marshal.SetLastPInvokeError(Errno);
            return -1;
        }
    }
}
