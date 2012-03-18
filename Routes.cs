using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace JsonProjection
{
    public class Routes : IRouteProvider
    {
        public string Area 
        {
            get { return "JsonProjection"; } 
        }
        
        public Routes()
        {
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/JsonProjection/Preview/{id}",
                                                         new RouteValueDictionary {
                                                                                      {"area", this.Area},
                                                                                      {"controller", "Service"},
                                                                                      {"action", "Preview"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", this.Area }
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
                         };
        }
    }
}
