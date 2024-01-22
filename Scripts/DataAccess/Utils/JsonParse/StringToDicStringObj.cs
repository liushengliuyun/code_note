using System;
using System.Collections.Generic;
using Core.Extensions;
using Newtonsoft.Json;

namespace DataAccess.Utils.JsonParse
{
    public class StringToDicStringObj : JsonConverter<Dictionary<string, object>>
    {
        public override void WriteJson(JsonWriter writer, Dictionary<string, object> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, object> ReadJson(JsonReader reader, Type objectType,
            Dictionary<string, object> existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var readerValue = reader.Value as string;

            if (readerValue.IsNullOrEmpty())
            {
                return null;
            }
            // readerValue = readerValue.Trim('"');
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(readerValue);
        }
    }
}