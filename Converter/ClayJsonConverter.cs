using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClaySharp;
using Newtonsoft.Json;
using Orchard.DisplayManagement;

namespace JsonProjection.Converter
{
    //public class ClayJsonConverter : JsonConverter
    //{
    //    public override bool CanRead
    //    {
    //        get
    //        {
    //            // TODO: read Clay objects
    //            return false;
    //        }
    //    }

    //    public override void WriteJson(JsonWriter writer, IShape value, JsonSerializer serializer)
    //    {
    //        //var clay = (IClayBehaviorProvider)value;

    //        var members = new Dictionary<string, object>();
    //        ((IClayBehaviorProvider)value).Behavior.GetMembers(() => null, value, members);


    //        // Parse Clay Array
    //        var e = ((dynamic)value).GetEnumerator();
    //        if (e != null && e.MoveNext())
    //        {
    //            writer.WriteStartArray();
    //            do
    //            {
    //                serializer.Serialize(writer, e.Current);
    //            } while (e.MoveNext());
    //            writer.WriteEndArray();
    //        }

    //        var members = new Dictionary<string, object>();
    //        clay.Behavior.GetMembers(() => null, clay, members);

    //        var memberKeys = members.Keys.Where(key => !key.StartsWith("_")).ToList();

    //        if (memberKeys.Count > 0)
    //        {
    //            writer.WriteStartObject();
    //            foreach (var key in memberKeys)
    //            {
    //                writer.WritePropertyName(key);
    //                serializer.Serialize(writer, members[key]);
    //            }

    //            writer.WriteEndObject();
    //        }
    //    }

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override bool CanConvert(Type objectType)
    //    {
    //        return typeof(Clay).IsAssignableFrom(objectType);
    //    }
    //}
}
