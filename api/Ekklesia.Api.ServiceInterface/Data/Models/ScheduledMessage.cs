namespace Ekklesia.Api.Data.Models
{
    public class ScheduledMessage : CompositeModel<ScheduledMessageData>
    {
        public string Description { get; set; }
    }

    public class ScheduledMessageData
    {
    }
}