using FluentMigrator.Runner.Announcers;
using Serilog;

namespace Ekklesia.Tools
{
    public class MigrationAnnouncer : Announcer
    {
        private readonly ILogger _logger;

        public MigrationAnnouncer()
        {
            _logger = Log.ForContext<MigrationAnnouncer>();
        }

        public override void Write(string message, bool escaped)
        {
            _logger.Debug(message);
        }
    }
}
