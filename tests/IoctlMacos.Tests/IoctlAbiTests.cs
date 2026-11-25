using System.Runtime.InteropServices;
using IoctlMacos.Abi;
using IoctlMacos.Requests;

namespace IoctlMacos.Tests;

public class IoctlAbiTests
{
    public static bool IsArm64 => RuntimeInformation.ProcessArchitecture == Architecture.Arm64;

    [Fact]
    public void For_Arm64_ReturnsArm64Abi() => Assert.IsType<Arm64Abi>(IoctlAbi.For(Architecture.Arm64));

    [Fact]
    public void For_X64_ReturnsX64Abi() => Assert.IsType<X64Abi>(IoctlAbi.For(Architecture.X64));

    [Fact]
    public void For_OtherArchitecture_Throws() =>
        Assert.Throws<PlatformNotSupportedException>(() => IoctlAbi.For(Architecture.Wasm));

    [Fact]
    public void Current_OnMacOS_MatchesProcess() =>
        Assert.IsType(IoctlAbi.For(RuntimeInformation.ProcessArchitecture).GetType(), IoctlAbi.Current);

    /// <summary>
    /// Зачем нужен Arm64Abi. Взят TIOCSWINSZ: по мусорному указателю ядро только
    /// читает, поэтому, в отличие от FIONREAD, память процесса не портится.
    /// Тест демонстрационный: результат зависит от мусора на стеке.
    /// </summary>
    [Fact(Skip = "Актуально только для arm64", SkipUnless = nameof(IsArm64))]
    public void X64Abi_OnArm64_DoesNotPassArgument()
    {
        var (master, slave) = Posix.CreatePty();
        using (master)
        using (slave)
        {
            var expected = new WindowSize(42, 137, 1111, 2222);
            try
            {
                new IoctlDevice(slave, new X64Abi()).Set(TtyRequests.SetWindowSize, expected);
            }
            catch (IoctlException)
            {
                // Ядро отвергло указатель: тоже подтверждение.
            }

            Assert.NotEqual(expected, new IoctlDevice(slave).Get(TtyRequests.GetWindowSize));
        }
    }
}
