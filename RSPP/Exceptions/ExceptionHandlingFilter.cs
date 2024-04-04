using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace RSPP.Exceptions
{
    public class ExceptionHandlingFilter : ExceptionFilterAttribute
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void OnException(ExceptionContext context)
        {
            _logger.Error($"OnException() - {context.Exception.Message}");
            context.ExceptionHandled = true;

            context.Result = new ViewResult
            {
                ViewName = "~/Views/Shared/Error.cshtml"
            };

        }
    }
}
