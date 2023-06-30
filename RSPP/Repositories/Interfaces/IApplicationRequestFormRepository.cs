using RSPP.Models.DB;
using System.Collections.Generic;

namespace RSPP.Repositories.Interfaces
{
    public interface IApplicationRequestFormRepository : IBaseRepository<ApplicationRequestForm>
    {
        string GetApplicationCompanyName(string applicationId);
        //IEnumerable<string> GetUnPaidApplicationIds();
    }
}
