using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace VstsDemoBuilder.Controllers
{
    public class SessonTimeoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext ctx = HttpContext.Current;
            if (HttpContext.Current.Session["visited"] == null)
            {
                //filterContext.Result = new RedirectResult("../account/SessionOutReturn", true);
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "account", action = "SessionOutReturn" }));
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}