using NodaTime;

namespace Ekklesia.Api.Data.Models
{
    public abstract class CompositeModel<TData> : IHaveId, IHaveAuditData
    {
        public long Id { get; set; }
        public Instant CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public Instant ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public TData Data { get; set; }
    }
}