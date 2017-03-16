using Izenda.BI.Framework.CustomAttributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Izenda.BI.CacheProvider.RedisCache.Converters
{
    public class DBServerTypeSupportingConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(DBServerTypeSupporting));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            string databaseTypeId = (string)jsonObject["DatabaseTypeId"];
            string shortDbTypeName = (string)jsonObject["ShortDbTypeName"];
            string databaseTypeName = (string)jsonObject["DatabaseTypeName"];

            return new DBServerTypeSupporting(databaseTypeId, shortDbTypeName, databaseTypeName);
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
