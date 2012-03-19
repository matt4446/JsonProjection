using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsonProjection.ViewModel
{
    public class AdminIndexViewModel
    {
        public IEnumerable<Orchard.Projections.Models.QueryPart> Items { get; set; }
    }
}
