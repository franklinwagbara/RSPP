using Microsoft.EntityFrameworkCore;
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
