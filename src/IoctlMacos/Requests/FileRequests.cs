namespace IoctlMacos.Requests;

/// <summary>&lt;sys/filio.h&gt;</summary>
public static class FileRequests
{
    /// <summary>FIONREAD</summary>
    public static readonly IoctlRead<int> BytesAvailable = new('f', 127);
}
