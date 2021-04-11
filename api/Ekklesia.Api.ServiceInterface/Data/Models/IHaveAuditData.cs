using NodaTime;

namespace Ekklesia.Api.Data.Models
{
    public interface IHaveAuditData
    {
        Instant CreatedOn { get; set; }
        string CreatedBy { get; set; }
        Instant ModifiedOn { get; set; }
        string ModifiedBy { get; set; }
    }
}