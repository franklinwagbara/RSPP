using RSPP.Repositories.Interfaces;
using System;

namespace RSPP.UnitOfWorks.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserMasterRepository UserMasterRepository { get; }
        IPaymentLogRepository PaymentLogRepository { get; }
        IApplicationRequestFormRepository ApplicationRequestFormRepository { get; }

        void Complete();
    }
}
