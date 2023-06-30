using Microsoft.EntityFrameworkCore;
using RSPP.Models.DB;
using RSPP.Repositories.Interfaces;

namespace RSPP.Repositories
{
    public class PaymentLogRepository<T>: BaseRepository<PaymentLog>, IPaymentLogRepository
    {
        /// <summary>
        /// PaymentLogRepository Constructor
        /// </summary>
        /// <param name="context"> The database context</param>
        public PaymentLogRepository(RSPPdbContext context):base(context)
        {
            
        }
    }
}
