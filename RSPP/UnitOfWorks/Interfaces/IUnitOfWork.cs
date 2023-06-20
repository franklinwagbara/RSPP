using RSPP.Repositories.Interfaces;
using System;

namespace RSPP.UnitOfWorks.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserMasterRepository UserMasterRepository { get; }

        void Complete();
    }
}
