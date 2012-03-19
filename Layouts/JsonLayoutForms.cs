using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Core.Contents.Rules;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace JsonProjection.Layouts
{
    public class JsonLayoutForms : Orchard.Forms.Services.IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public JsonLayoutForms(
            IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            //todo: add options:
            //1. Include Query definition in json 
            //2. Require Authorization
            //3. Permission stereotype
            
            Func<IShapeFactory, object> form =
                shape => {

                    var f = 
                        Shape.Form(
                            Id: "JsonLayout",
                            _Options: Shape.Fieldset(
                                Title: T("Fields"),
                                _ValueTrue: 
                                    Shape.Checkbox(
                                        Id: "IncludeQueryDefinition", 
                                        Title: T("Include Query Definition"), 
                                        Checked: true, 
                                        Description: T("Include the title of the query in the json results")
                                    ),
                                _ValueFalse:
                                    Shape.Checkbox(
                                        Id: "RequireAuthorization",
                                        Title: T("Require Authorization"),
                                        Checked: false,
                                        Description: T("Require user to be authorized")
                                    )
                            )
                            //_Options: Shape.Fieldset(
                            //Title: T("Alignment"),
                            //_ValueTrue: Shape.Radio(
                            //    Id: "horizontal", Name: "Alignment",
                            //    Title: T("Horizontal"), Value: "horizontal",
                            //    Checked: true,
                            //    Description: T("Horizontal alignment will place items starting in the upper left and moving right.")
                            //    ),
                            //_ValueFalse: Shape.Radio(
                            //    Id: "vertical", Name: "Alignment",
                            //    Title: T("Vertical"), Value: "vertical",
                            //    Description: T("Vertical alignment will place items starting in the upper left and moving dowmn.")
                            //    ),
                            //_Colums: Shape.TextBox(
                            //    Id: "columns", Name: "Columns",
                            //    Title: T("Number of columns/lines "),
                            //    Value: 3,
                            //    Description: T("How many columns (in Horizontal mode) or lines (in Vertical mode) to display in the grid."),
                            //    Classes: new[] { "small-text", "tokenized" }
                            //    )
                            //),

                        );

                    return f;
                };

            context.Form("JsonLayout", form);

        }
    }

    public class JsonLayoutFormsValitator : FormHandler
    {
        public Localizer T { get; set; }

        public override void Validating(ValidatingContext context) {
            if (context.FormName == "JsonLayout")
            {
                //if (context.ValueProvider.GetValue("Order") == null) {
                //    context.ModelState.AddModelError("Order", T("You must provide an Order").Text);
                //}
            }
        }
    }
}
