using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IoctlMacos.Requests;

namespace IoctlMacos.Tests;

/// <summary>Эталонные коды получены из заголовков macOS SDK через clang.</summary>
public class IoctlRequestTests
{
    [Fact]
    public void Code_BytesAvailable_IsFIONREAD() =>
        Assert.Equal((nuint)0x4004667f, FileRequests.BytesAvailable.Code);

    [Fact]
    public void Code_GetWindowSize_IsTIOCGWINSZ() =>
        Assert.Equal((nuint)0x40087468, TtyRequests.GetWindowSize.Code);

    [Fact]
    public void Code_SetWindowSize_IsTIOCSWINSZ() =>
        Assert.Equal((nuint)0x80087467, TtyRequests.SetWindowSize.Code);

    [Fact]
    public void Code_ReadWrite_IsSIOCGIFMTU() =>
        Assert.Equal((nuint)0xc0206933, new IoctlReadWrite<InterfaceMtu>('i', 51).Code);

    [Fact]
    public void Constructor_LargestArgument_IsAccepted() =>
        Assert.Equal((nuint)0x5fff7801, new IoctlRead<Bytes8191>('x', 1).Code);

    [Fact]
    public void Constructor_OversizedArgument_Throws() =>
        Assert.Throws<ArgumentException>(() => new IoctlRead<Bytes8192>('x', 1));

    [Fact]
    public void Constructor_GroupOutsideOneByte_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new IoctlRead<int>('ж', 1));

    [Fact]
    public void Constructor_AutoLayout_Throws() =>
        Assert.Throws<ArgumentException>(() => new IoctlRead<AutoLayout>('x', 1));

    [Fact]
    public void Constructor_PrimitiveOrEnum_IsAccepted()
    {
        Assert.Equal((nuint)0x40087801, new IoctlRead<nint>('x', 1).Code);
        Assert.Equal((nuint)0x40047801, new IoctlRead<DayOfWeek>('x', 1).Code);
    }

    [StructLayout(LayoutKind.Auto)]
    private struct AutoLayout
    {
        public byte A;
        public long B;
    }

    [InlineArray(8191)]
    private struct Bytes8191
    {
        private byte _first;
    }

    [InlineArray(8192)]
    private struct Bytes8192
    {
        private byte _first;
    }
}
