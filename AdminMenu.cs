using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace JsonProjection
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu()
        {
        }

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.AddImageSet("navigation")
                .Add(T("Json Query Feeds"), "9",
                    menu =>
                    {
                        menu.Add(T("Preview"), "0", item => item.Action("Index", "Service", new { area = "JsonProjection" }));
                    });
        }
    }
}
