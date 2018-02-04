using System.Collections.Generic;

namespace Ekklesia.Audio
{
    public interface ISingbackScanner
    {
        Dictionary<string, string> Scan();
    }
}
