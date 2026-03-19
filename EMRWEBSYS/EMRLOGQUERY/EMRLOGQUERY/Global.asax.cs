using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace EMRLOGQUERY
{
    public class MvcApplication : System.Web.HttpApplication
    {



        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest()
        {

            string currentHost = Request.Url.Host.ToLower();
            bool isShortHost = currentHost == "intra5";
            // 檢查是否為 HTTP 請求，若是就轉址為 HTTPS
            if (!Request.IsSecureConnection || isShortHost)
            {
                string host = Request.Url.Host;
                string rawUrl = Request.RawUrl;           

                // 若有自訂 Port，可加上 Port: ":" + Request.Url.Port
                string httpsUrl = "https://" + host +".femh.org.tw"+ rawUrl;

                Response.Redirect(httpsUrl, true); // true = 終止後續處理
            }
        }

    }
}
