using FUNews.Presentation.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FUNews.Presentation.Filters
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;

            var accountRoleId = httpContext.Session.GetInt32("AccountRole");

            // Only accountRoleId = 1 is Admin
            if (accountRoleId != AccountRoles.Admin)
            {
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Auth",
                    null
                );
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
