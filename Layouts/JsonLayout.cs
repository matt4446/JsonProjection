using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JsonProjection.Converter;
using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Models;
using Orchard.Projections.Services;

namespace JsonProjection.Layouts
{
    public class JsonLayout : ILayoutProvider
    {
        private readonly IContentManager _contentManager;
        protected dynamic Shape { get; set; }

        public JsonLayout(IShapeFactory shapeFactory, IContentManager contentManager)
        {
            _contentManager = contentManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeLayoutContext describe)
        {
            describe.For("JSON", T("JSON"), T("JSON Layouts"))
                .Element("List", T("JSON List"), T("Organizes content items inside a json array."),
                    DisplayLayout,
                    RenderLayout,
                    "JsonLayout"
                );
        }

        public LocalizedString DisplayLayout(LayoutContext context)
        {
            string order = context.State.Order;

            switch (order)
            {
                case "ordered":
                    return T("Ordered Html List");
                case "unordered":
                    return T("Unordered Html List");
                default:
                    throw new ArgumentOutOfRangeException("order");
            }
        }

        public dynamic RenderLayout(LayoutContext context, IEnumerable<LayoutComponentResult> layoutComponentResults)
        {
            int columns = 2;//Convert.ToInt32(context.State.Columns);
            bool horizontal = Convert.ToString(context.State.Alignment) != "vertical";
            string rowClass = context.State.RowClass;
            string gridClass = context.State.GridClass;
            string gridId = context.State.GridId;

            IEnumerable<dynamic> shapes =
               //context.LayoutRecord.Display == (int)LayoutRecord.Displays.Content
               //    ? layoutComponentResults.Select(x => _contentManager.BuildDisplay(x.ContentItem, context.LayoutRecord.DisplayType))
               layoutComponentResults.Select(x => x.Properties);

            //ClayJsonConverter converter = new ClayJsonConverter();

            //StringBuilder sb = new StringBuilder();
            //StringWriter sw = new StringWriter(sb);
            //using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            //{
            //    converter.WriteJson(jsonWriter, shapes, new JsonSerializer() {  });
            //}

            return Shape.JsonList(Id: gridId, Horizontal: horizontal, Columns: columns, Items: shapes, Classes: new[] { gridClass }, RowClasses: new[] { rowClass });
        }

        //public dynamic RenderLayout(LayoutContext context, IEnumerable<LayoutComponentResult> layoutComponentResults)
        //{
        //    string order = context.State.Order;
        //    string itemClass = context.State.ItemClass;
        //    string listClass = context.State.ListClass;
        //    string listId = context.State.ListId;

        //    string listTag = order == "ordered" ? "ol" : "ul";

        //    IEnumerable<dynamic> shapes;
        //    if (context.LayoutRecord.Display == (int)LayoutRecord.Displays.Content)
        //    {
        //        shapes = layoutComponentResults.Select(x => _contentManager.BuildDisplay(x.ContentItem, context.LayoutRecord.DisplayType));
        //    }
        //    else
        //    {
        //        shapes = layoutComponentResults.Select(x => x.Properties);
        //    }

        //    var classes = String.IsNullOrEmpty(listClass) ? Enumerable.Empty<string>() : new[] { listClass };
        //    var itemClasses = String.IsNullOrEmpty(itemClass) ? Enumerable.Empty<string>() : new[] { itemClass };

        //    return Shape.List(Id: listId, Items: shapes, Tag: listTag, Classes: classes, ItemClasses: itemClasses);
        //}
    }
}
