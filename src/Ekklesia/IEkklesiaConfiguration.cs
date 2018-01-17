using System.Collections.Generic;
using ServiceStack.Configuration;

namespace Ekklesia
{
    public interface IEkklesiaConfiguration : IAppSettings
    {
        bool DebugMode { get; }
        string RecordingsFolder { get; }
        string SongsFolder { get; }
        int StreamingPort { get; }
        string IceCastServer { get; }
        int IceCastPort { get; }
        string IceCastPassword { get; }
        string IceCastMountPoint { get; }
    }
}
