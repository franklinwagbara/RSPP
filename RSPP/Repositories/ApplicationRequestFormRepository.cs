using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using RSPP.Helpers;
using RSPP.Models.DB;
using RSPP.Repositories.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;

namespace RSPP.Repositories
{
    public class ApplicationRequestFormRepository<T> : BaseRepository<ApplicationRequestForm>, IApplicationRequestFormRepository
    {
        /// <summary>
        /// ApplicationRequestFormRepository Constructor
        /// </summary>
        /// <param name="context"> The database context</param>
        /// 
        public ApplicationRequestFormRepository(RSPPdbContext context) : base(context)
        {
        }

        public string GetApplicationCompanyName(string applicationId)
        {

            return (from app in _context.ApplicationRequestForm
                    join users in _context.UserMaster
                    on app.CompanyEmail equals users.UserEmail
                    where app.ApplicationId == applicationId
                    select users.CompanyName).FirstOrDefault();
        }

        //public IEnumerable<string> GetUnPaidApplicationIds()
        //{
        //    return (from app in _context.ApplicationRequestForm
        //            join payments in _context.PaymentLog
        //            on app.ApplicationId equals payments.ApplicationId
        //            where app.Status != AppMessages.REJECTED && app.CurrentStageId < 4
        //            && payments.Status == AppMessages.INIT && payments.Rrreference != null
        //            && payments.Rrreference != AppMessages.RRR
        //            select app.ApplicationId)
        //            .ToList();
        //}
    }
}
