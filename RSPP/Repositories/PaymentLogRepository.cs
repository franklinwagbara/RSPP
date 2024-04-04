using RSPP.Models.DB;
using RSPP.Models.ViewModels;
using System.Linq;
using RSPP.Repositories.Interfaces;

namespace RSPP.Repositories
{
    public class PaymentLogRepository<T> : BaseRepository<PaymentLog>, IPaymentLogRepository
    {
        /// <summary>
        /// PaymentLogRepository Constructor
        /// </summary>
        /// <param name="context"> The database context</param>
        public PaymentLogRepository(RSPPdbContext context) : base(context) { }

        /// <summary>
        /// Get the summary of an application's payment
        /// </summary>
        /// <param name="applicationId"> The application id</param>
        public ChargeSummaryVM GetChargeSummary(string applicationId)
        {
            return (from payments in _context.PaymentLog
                    join applications in _context.ApplicationRequestForm
                    on payments.ApplicationId equals applications.ApplicationId
                    where payments.ApplicationId == applicationId
                    select new ChargeSummaryVM
                    {
                        ApplicationId = applicationId,
                        RRR = payments.Rrreference,
                        AgencyName = applications.AgencyName,
                        Amount = payments.TxnAmount.ToString()
                    }).FirstOrDefault();
        }
    }
}
