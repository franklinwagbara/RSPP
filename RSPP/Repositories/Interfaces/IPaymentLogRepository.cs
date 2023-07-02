using RSPP.Models.DB;
using RSPP.Models.ViewModels;

namespace RSPP.Repositories.Interfaces
{
    /// <summary>
    /// PaymentLog Repository Interface
    /// </summary>
    public interface IPaymentLogRepository:IBaseRepository<PaymentLog>
    {
        ChargeSummaryVM GetChargeSummary(string applicationId);
    }
}
