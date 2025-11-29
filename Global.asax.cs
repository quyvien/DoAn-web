using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;// thu vien cho FormsAuthentication
using System.Security.Principal;// thu vien cho IPrincipal

namespace DoAn_web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            //  Lấy cookie xác thực
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                try
                {
                    //  Giai lay ma ve (ticket)
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                    if (authTicket != null && !authTicket.Expired)
                    {
                        // lay vai tro da luu trong UserData
                        string[] roles = authTicket.UserData.Split(',');

                        //  tao 1 danh tinh moi voi vai tro
                        IIdentity identity = new FormsIdentity(authTicket);
                        IPrincipal principal = new GenericPrincipal(identity, roles);

                        //  gan danh tinh nay cho user hien tai
                        Context.User = principal;
                    }
                }
                catch
                {
                    /// loi thi thoai
                }
            }
        }
    }
}
