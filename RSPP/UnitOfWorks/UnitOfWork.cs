using RSPP.Models.DB;
using RSPP.Repositories;
using RSPP.Repositories.Interfaces;
using RSPP.UnitOfWorks.Interfaces;
using System;

namespace RSPP.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly RSPPdbContext _context;
        private UserMasterRepository<UserMaster> _userMasterRepository;
        private PaymentLogRepository<PaymentLog> _paymentLogRepository;
        private ApplicationRequestFormRepository<ApplicationRequestForm> _applicationRequestFormRepository;


        public UnitOfWork(RSPPdbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IUserMasterRepository UserMasterRepository
        {
            get
            {

                if (this._userMasterRepository == null)
                {
                    this._userMasterRepository = new UserMasterRepository<UserMaster>(_context);
                }
                return _userMasterRepository;
            }
        }
        public IPaymentLogRepository PaymentLogRepository
        {
            get
            {

                if (this._paymentLogRepository == null)
                {
                    this._paymentLogRepository = new PaymentLogRepository<PaymentLog>(_context);
                }
                return _paymentLogRepository;
            }
        }

        public IApplicationRequestFormRepository ApplicationRequestFormRepository {
            get
            {

                if (this._applicationRequestFormRepository == null)
                {
                    this._applicationRequestFormRepository = new ApplicationRequestFormRepository<ApplicationRequestForm>(_context);
                }
                return _applicationRequestFormRepository;
            }
        }

        public void Complete()
        {
            _context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
