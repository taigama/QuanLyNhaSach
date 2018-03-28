using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;





/////////////////////////////////////////////////////
/// -- template --
///

//public ActionResult GetOrders()
//{
//    var orders = db.Orders;
//    var result = new JsonNetResult
//    {
//        Data = order,
//        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
//        Settings = {
//            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
//            MaxDepth = 3
//                }
//    };
//    return result;
//}

namespace BookStore.Data
{
    /// <summary> Our custom json result
    /// <para> you can set serialize depth
    /// </para>
    /// <para> => mega-boost in performance 
    /// </para>
    /// <para> 50KB => 2.7KB
    /// </para>
    /// </summary>
    public class JsonNetResult : JsonResult
    {
        public static JsonNetResult ParseJson(object data, int depth = 1
            , ReferenceLoopHandling referenceLoopHandling = ReferenceLoopHandling.Ignore)
        {
            var result = new JsonNetResult
            {
                Data = data,
                JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet,
                Settings = {
            ReferenceLoopHandling = referenceLoopHandling,
            MaxDepth = depth// product, productdetail, author
                }
            };
            return result;
        }


        public JsonNetResult()
        {
            Settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        public JsonSerializerSettings Settings { get; private set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (this.JsonRequestBehavior == JsonRequestBehavior.DenyGet && string.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("JSON GET is not allowed");

            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType = string.IsNullOrEmpty(this.ContentType) ? "application/json" : this.ContentType;

            if (this.ContentEncoding != null)
                response.ContentEncoding = this.ContentEncoding;
            if (this.Data == null)
                return;

            var scriptSerializer = JsonSerializer.Create(this.Settings);

            using (var sw = new StringWriter())
            {
                if (Settings.MaxDepth != null)
                {
                    using (var jsonWriter = new JsonNetTextWriter(sw))
                    {
                        Func<bool> include = () => jsonWriter.CurrentDepth <= Settings.MaxDepth;
                        var resolver = new JsonNetContractResolver(include);
                        this.Settings.ContractResolver = resolver;
                        var serializer = JsonSerializer.Create(this.Settings);
                        serializer.Serialize(jsonWriter, Data);
                    }
                    response.Write(sw.ToString());
                }
                else
                {
                    scriptSerializer.Serialize(sw, this.Data);
                    response.Write(sw.ToString());
                }
            }
        }
    }

    public class JsonNetTextWriter : JsonTextWriter
    {
        public JsonNetTextWriter(TextWriter textWriter) : base(textWriter) { }

        public int CurrentDepth { get; private set; }

        public override void WriteStartObject()
        {
            CurrentDepth++;
            base.WriteStartObject();
        }

        public override void WriteEndObject()
        {
            CurrentDepth--;
            base.WriteEndObject();
        }
    }

    public class JsonNetContractResolver : DefaultContractResolver
    {
        private readonly Func<bool> _includeProperty;

        public JsonNetContractResolver(Func<bool> includeProperty)
        {
            _includeProperty = includeProperty;
        }

        protected override JsonProperty CreateProperty(
            MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            var shouldSerialize = property.ShouldSerialize;
            property.ShouldSerialize = obj => _includeProperty() &&
                                              (shouldSerialize == null ||
                                               shouldSerialize(obj));
            return property;
        }
    }
}