using System.Collections.Generic;
using System.Linq;
using ServiceStack;

namespace Ekklesia
{
    internal class EkklesiaConfiguration : NetCoreAppSettings, IEkklesiaConfiguration
    {        
        public bool DebugMode => Get(nameof(DebugMode), false);
        public string RecordingsFolder => Get<string>(nameof(RecordingsFolder));

        public string SongsFolder => Get<string>(nameof(SongsFolder));

        public int StreamingPort => Get(nameof(StreamingPort), 44100);

        public string IceCastServer => Get<string>(nameof(IceCastServer));

        public int IceCastPort => Get(nameof(IceCastPort), 8000);

        public string IceCastPassword => Get<string>(nameof(IceCastPassword));

        public string IceCastMountPoint => Get<string>(nameof(IceCastMountPoint));

        public EkklesiaConfiguration(Microsoft.Extensions.Configuration.IConfiguration configuration) : base(configuration)
        {
        }
    }
}
