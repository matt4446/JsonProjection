using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;
using Orchard.Core.Feeds.StandardBuilders;
using Orchard.DisplayManagement;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Descriptors.Property;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Services;
using Orchard.Tokens;

namespace JsonProjection.Feeds
{
    public interface IJsonFeedService : IDependency
    {
        bool Authorize(int queryId);
        string Execute(int queryId);
    }

    public class JsonFeedService : IJsonFeedService
    {
        private readonly IProjectionManager _projectionManager;
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;
        private readonly ITokenizer tokenizer;
        private readonly IDisplayHelperFactory displayHelperFactory;
        private readonly IWorkContextAccessor workContextAccessor;

        public JsonFeedService(IOrchardServices services,
            IProjectionManager projectionManager,
            IEnumerable<IHtmlFilter> htmlFilters,
            ITokenizer tokenizer,
            IWorkContextAccessor workContextAccessor,
            IDisplayHelperFactory displayHelperFactory)
        {
            this.tokenizer = tokenizer;
            this.displayHelperFactory = displayHelperFactory;
            this.workContextAccessor = workContextAccessor;
            Services = services;

            _projectionManager = projectionManager;
            _htmlFilters = htmlFilters;
        }

        public IOrchardServices Services { get; private set; }

        //public void WriteItem(JsonWriter jsonWriter, IList<string> properties, IEnumerable<ContentItem> items) 
        //{
        //    Func<int, int, int> seekItem = (row, col) => (row * properties.Count) + col;
        //    var rows = shapes.Count / properties.Count; 
        //    jsonWriter.WriteStartObject();

        //    for (var i = 0; i < rows; i++) 
        //    {
        //        jsonWriter.WriteStartObject();
        //        for (var c = 0; c < properties.Count; c++) 
        //        {
        //            jsonWriter.WritePropertyName(properties[c]);
        //            jsonWriter.WriteNull();
        //            //jsonWriter.WriteValue(shapes.
        //        }
        //        jsonWriter.WriteEndObject();
        //    }

        //    jsonWriter.WriteEndArray();
        //}

        //public void QueryProperties(JsonWriter jsonWriter, ProjectionPart projection, IEnumerable<ContentItem> items) 
        //{
        //    var shapes = items.Select(contentItem => this._contentManager.BuildDisplay(contentItem, "Summary", null)).ToList();

        //    if (shapes.Count == 0)
        //    {
        //        jsonWriter.WriteStartArray();
        //        jsonWriter.WriteEndArray();

        //        return;
        //    }

        //    jsonWriter.WriteStartArray();
            
        //    WriteItem(jsonWriter, new List<string>(){ "Title", "Description" }, shapes);
            
        //    jsonWriter.WriteEndArray();

        //    //for (int row = 0; row < maxRows; row++)
        //    //{
        //    //    Output.Write(rowTag.ToString(TagRenderMode.StartTag));
        //    //    for (int col = 0; col < maxCols; col++)
        //    //    {
        //    //        int index = seekItem(row, col);
        //    //        Output.Write(cellTag.ToString(TagRenderMode.StartTag));
        //    //        if (index < itemsCount)
        //    //        {
        //    //            Output.Write(Display(items[index]));
        //    //        }
        //    //        else
        //    //        {
        //    //            // fill an empty cell
        //    //            Output.Write("&nbsp;");
        //    //        }

        //    //        Output.Write(cellTag.ToString(TagRenderMode.EndTag));
        //    //    }
        //    //    Output.Write(rowTag.ToString(TagRenderMode.EndTag));
        //    //}


        //    //return string.Empty;
        //}
        public bool Authorize(int queryId)
        {
            var projection = this.Services.ContentManager.Query<ProjectionPart>()
                .Where<ProjectionPartRecord>(e => e.QueryPartRecord.Id == queryId)
                .List()
                .FirstOrDefault();

            return true;
        }

        public string Execute(int queryId) 
        {
            var projection = this.Services.ContentManager.Query<ProjectionPart>()
                .Where<ProjectionPartRecord>(e => e.QueryPartRecord.Id == queryId)
                .List()
                .FirstOrDefault();

            var layout = projection.Record.LayoutRecord;

            LayoutDescriptor layoutDescriptor = layout == null ? null : 
                _projectionManager.DescribeLayouts().SelectMany(x => x.Descriptors).FirstOrDefault(x => x.Category == layout.Category && x.Type == layout.Type);

            var tokens = new Dictionary<string, object> { { "Content", projection.ContentItem } };
            var allFielDescriptors = _projectionManager.DescribeProperties().ToList();
            var fieldDescriptors = layout.Properties.OrderBy(p => p.Position).Select(p => allFielDescriptors.SelectMany(x => x.Descriptors).Select(d => new { Descriptor = d, Property = p }).FirstOrDefault(x => x.Descriptor.Category == p.Category && x.Descriptor.Type == p.Type)).ToList();
            var tokenizedDescriptors = fieldDescriptors.Select(fd => new { fd.Descriptor, fd.Property, State = FormParametersHelper.ToDynamic(tokenizer.Replace(fd.Property.State, tokens)) }).ToList();

            var limit = projection.Record.MaxItems;
            var items = _projectionManager.GetContentItems(queryId, 0, limit).ToList();

            //var container = _contentManager.Get<Orchard.Projections.Models.QueryPart>(projection.Record.QueryPartRecord.Id);

            var inspector = new ItemInspector(projection, Services.ContentManager.GetItemMetadata(projection), _htmlFilters);

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            var display = displayHelperFactory.CreateHelper(new ViewContext { HttpContext = workContextAccessor.GetContext().HttpContext }, new ViewDataContainer());

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;

                jsonWriter.WriteStartObject();

                jsonWriter.WritePropertyName("title");
                jsonWriter.WriteValue(inspector.Title);
                jsonWriter.WritePropertyName("description");
                jsonWriter.WriteValue(inspector.Description);

                jsonWriter.WritePropertyName("Items");
                jsonWriter.WriteStartArray();
                foreach (var contentItem in items) 
                {
                    var contentItemMetadata = Services.ContentManager.GetItemMetadata(contentItem);
                    var propertyDescriptors = tokenizedDescriptors.Select(d =>
                                {
                                    var fieldContext = new PropertyContext
                                    {
                                        State = d.State,
                                        Tokens = tokens
                                    };

                                    return new { d.Property, Shape = d.Descriptor.Property(fieldContext, contentItem) };
                                });

                    var properties = Services.New.Properties(
                            Items: propertyDescriptors.Select(
                                pd => Services.New.PropertyWrapper(
                                    Item: pd.Shape,
                                    Property: pd.Property,
                                    ContentItem: contentItem,
                                    ContentItemMetadata: contentItemMetadata
                                )
                            )
                        );

                    var itemInspector = new ItemInspector(contentItem, contentItemMetadata, _htmlFilters);
                    jsonWriter.WriteStartObject();
                    //jsonWriter.WriteComment("title");
                    //jsonWriter.WritePropertyName("title");
                    //jsonWriter.WriteValue(itemInspector.Title);
                    //jsonWriter.WritePropertyName("description");
                    //jsonWriter.WriteValue(itemInspector.Description);

                    var propertyShapes = ((IEnumerable<dynamic>)properties.Items)
                        .Select(e => new { 
                            Property = ((PropertyRecord)e.Property),
                            Description = ((PropertyRecord)e.Property).Description,
                            Proxy = e,
                            Content = Convert.ToString(display(e))
                        })
                        .ToList();

                    foreach (var field in propertyShapes) 
                    {
                        jsonWriter.WritePropertyName(field.Description);
                        jsonWriter.WriteValue(field.Content);
                    }

                    jsonWriter.WriteEndObject();
                }

                jsonWriter.WriteEndArray();

                jsonWriter.WriteEndObject();

            }

            return sb.ToString();
            //        var projectionId = context.ValueProvider.GetValue("projection");
            //        if (projectionId == null)
            //            return;

            //        var limitValue = context.ValueProvider.GetValue("limit");
            //        var limit = 20;
            //        if (limitValue != null)
            //            limit = (int)limitValue.ConvertTo(typeof(int));

            //        var containerId = (int)projectionId.ConvertTo(typeof(int));
            //        var container = _contentManager.Get<ProjectionPart>(containerId);

            //        if (container == null)
            //        {
            //            return;
            //        }

            //        var inspector = new ItemInspector(container, _contentManager.GetItemMetadata(container), _htmlFilters);

        }

    }

    

    //public class QueryFeedQuery : IFeedQueryProvider, IFeedQuery
    //{
    //    private readonly IContentManager _contentManager;
    //    private readonly IProjectionManager _projectionManager;
    //    private readonly IEnumerable<IHtmlFilter> _htmlFilters;

    //    public QueryFeedQuery(
    //        IContentManager contentManager,
    //        IProjectionManager projectionManager,
    //        IEnumerable<IHtmlFilter> htmlFilters)
    //    {
    //        _contentManager = contentManager;
    //        _projectionManager = projectionManager;
    //        _htmlFilters = htmlFilters;
    //    }

    //    public FeedQueryMatch Match(FeedContext context)
    //    {
    //        var containerIdValue = context.ValueProvider.GetValue("projection");
    //        if (containerIdValue == null)
    //            return null;

    //        return new FeedQueryMatch { FeedQuery = this, Priority = 0 };
    //    }

    //    public void Execute(FeedContext context)
    //    {
    //        var projectionId = context.ValueProvider.GetValue("projection");
    //        if (projectionId == null)
    //            return;

    //        var limitValue = context.ValueProvider.GetValue("limit");
    //        var limit = 20;
    //        if (limitValue != null)
    //            limit = (int)limitValue.ConvertTo(typeof(int));

    //        var containerId = (int)projectionId.ConvertTo(typeof(int));
    //        var container = _contentManager.Get<ProjectionPart>(containerId);

    //        if (container == null)
    //        {
    //            return;
    //        }

    //        var inspector = new ItemInspector(container, _contentManager.GetItemMetadata(container), _htmlFilters);
    //        if (context.Format.ToLower() == "xml")
    //        {
    //            StringBuilder sb = new StringBuilder();
    //            StringWriter sw = new StringWriter(sb);

    //            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
    //            {
    //              jsonWriter.Formatting = Formatting.Indented;

    //              jsonWriter.WriteStartObject();
    //              jsonWriter.WritePropertyName("CPU");
    //              jsonWriter.WriteValue("Intel");
    //              jsonWriter.WritePropertyName("PSU");
    //              jsonWriter.WriteValue("500W");
    //              jsonWriter.WritePropertyName("Drives");
    //              jsonWriter.WriteStartArray();
    //              jsonWriter.WriteValue("DVD read/writer");
    //              jsonWriter.WriteComment("(broken)");
    //              jsonWriter.WriteValue("500 gigabyte hard drive");
    //              jsonWriter.WriteValue("200 gigabype hard drive");
    //              jsonWriter.WriteEnd();
    //              jsonWriter.WriteEndObject();

    //            }

    //            context.Response.Element = new System.Xml.Linq.XElement(

               
    //            var link = new XElement("link");
    //            context.Response.Element.SetElementValue("title", inspector.Title);
    //            context.Response.Element.Add(link);
    //            context.Response.Element.SetElementValue("description", inspector.Description);

    //            context.Response.Contextualize(requestContext =>
    //            {
    //                var urlHelper = new UrlHelper(requestContext);
    //                var uriBuilder = new UriBuilder(urlHelper.RequestContext.HttpContext.Request.ToRootUrlString()) { Path = urlHelper.RouteUrl(inspector.Link) };
    //                link.Add(uriBuilder.Uri.OriginalString);
    //            });
    //        }
    //        else
    //        {
    //            context.Builder.AddProperty(context, null, "title", inspector.Title);
    //            context.Builder.AddProperty(context, null, "description", inspector.Description);
    //            context.Response.Contextualize(requestContext =>
    //            {
    //                var urlHelper = new UrlHelper(requestContext);
    //                context.Builder.AddProperty(context, null, "link", urlHelper.RouteUrl(inspector.Link));
    //            });
    //        }

    //        var items = _projectionManager.GetContentItems(container.Record.QueryPartRecord.Id, 0, limit).ToList();

    //        foreach (var item in items)
    //        {
    //            context.Builder.AddItem(context, item);
    //        }
    //    }
    //}

    public class ViewDataContainer : IViewDataContainer
    {
        public ViewDataDictionary ViewData { get; set; }
    }
}


