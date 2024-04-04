using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using RSPP.Controllers;

namespace RSPP.Helpers
{
    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext ctx = filterContext.HttpContext;
            if (ctx.Session != null)
            {
                var userEmail = ctx.Session.Get(AccountController.sessionEmail);
                if (userEmail is null)
                {
                    filterContext.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
