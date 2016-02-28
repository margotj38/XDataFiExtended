using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace WebAPI_final
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {


            config.MapHttpAttributeRoutes();

            // Convention-based routing.    
           

            

            

            config.Routes.MapHttpRoute(
                name: "ApiByAction",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { action = "Get" }
            );
        }
    }
}
