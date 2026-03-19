using System;
using System.Web;
using System.Web.Mvc;

namespace EMRLOGQUERY.Filters
{
    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string _role;

        public RoleAuthorizeAttribute(string role)
        {
            _role = role;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var sessionRole = httpContext.Session["UserRole"] as string;
            return sessionRole == _role;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // 若未授權，導向登入頁面
            filterContext.Result = new RedirectResult("~/Login/Login");
        }
    }
}
