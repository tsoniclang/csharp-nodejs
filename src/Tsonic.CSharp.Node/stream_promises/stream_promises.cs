using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591
#pragma warning disable IDE1006

public static class stream_promises
{
    public static Task pipeline(params Stream[] streams)
    {
        return stream.promises.pipeline(streams);
    }

    public static Task finished(Stream streamValue)
    {
        return stream.promises.finished(streamValue);
    }
}
