using Microsoft.EntityFrameworkCore;
using RSPP.Models.DB;
using RSPP.Repositories.Interfaces;

namespace RSPP.Repositories
{
    public class UserMasterRepository<T> : BaseRepository<UserMaster>, IUserMasterRepository
    {
        /// <summary>
        /// UserMasterRepository Constructor
        /// </summary>
        /// <param name="context"> The database context</param>
        public UserMasterRepository(DbContext context) : base(context) { }
    }
}
