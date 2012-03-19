using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Projections.Services;
using Orchard.Security;
using Orchard.Settings;
using Orchard.ContentManagement;
using Orchard.Tokens;
using JsonProjection.Feeds;
using Orchard.UI.Admin;
using Orchard.Themes;
using System.Net;

namespace JsonProjection.Controllers
{
    public class ServiceController : Controller
    {
        private readonly IJsonFeedService jsonFeed;

        public ServiceController(
            IOrchardServices services,
            IShapeFactory shapeFactory,
            IJsonFeedService jsonFeed)
        {

            this.jsonFeed = jsonFeed;

            Services = services;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        public IOrchardServices Services { get; set; }

        public Localizer T { get; set; }

        public dynamic Shape { get; set; }

        [Admin]
        public ActionResult Index() 
        {
            var viewModel = new ViewModel.AdminIndexViewModel();
            viewModel.Items = this.Services.ContentManager.Query<Orchard.Projections.Models.QueryPart>().List().ToList();

            return View(viewModel);
        }

        public ActionResult Feed(int id) 
        {
            var feed = this.jsonFeed.Execute(id);

            return Content(feed, "JSON", Encoding.UTF8);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Themed]
        public ActionResult Preview(int id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage queries")))
                return new HttpUnauthorizedResult();

            var feed = this.jsonFeed.Execute(id);

            return Content(feed);
        }
    }
}
