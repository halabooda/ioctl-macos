using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace IoctlMacos;

/// <summary>Ядро вернуло ошибку на запрос ioctl.</summary>
/// <param name="code">Код запроса.</param>
/// <param name="errno">Значение errno.</param>
[SuppressMessage("Design", "CA1032", Justification = "Исключение без кода запроса и errno не имеет смысла.")]
public sealed class IoctlException(nuint code, int errno)
    : IOException($"ioctl 0x{code:x}: {Marshal.GetPInvokeErrorMessage(errno)} (errno {errno})")
{
    /// <summary>Код запроса.</summary>
    public nuint Code { get; } = code;

    /// <summary>Значение errno.</summary>
    public int Errno { get; } = errno;
}
