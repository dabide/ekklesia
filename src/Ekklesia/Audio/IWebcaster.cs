using System.Threading;

namespace Ekklesia.Audio
{
    public interface IWebcaster
    {
        void Start(int port, CancellationToken cancellationToken);
    }
}
