using System.Runtime.InteropServices;

namespace IoctlMacos.Abi;

internal static class IoctlAbi
{
    private static readonly Lazy<IIoctlAbi> Detected = new(Detect);

    /// <summary>ABI текущего процесса. Под Rosetta процесс считается x64.</summary>
    public static IIoctlAbi Current => Detected.Value;

    public static IIoctlAbi For(Architecture architecture) => architecture switch
    {
        Architecture.Arm64 => new Arm64Abi(),
        Architecture.X64 => new X64Abi(),
        _ => throw new PlatformNotSupportedException($"Архитектура {architecture} не поддерживается."),
    };

    private static IIoctlAbi Detect() => OperatingSystem.IsMacOS()
        ? For(RuntimeInformation.ProcessArchitecture)
        : throw new PlatformNotSupportedException("Поддерживается только macOS.");
}
