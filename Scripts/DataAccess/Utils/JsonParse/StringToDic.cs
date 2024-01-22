using System;
using System.Collections.Generic;
using Core.Extensions;
using Newtonsoft.Json;

namespace DataAccess.Utils.JsonParse
{
    public class StringToDic : JsonConverter<Dictionary<int, float>>
    {
        public override void WriteJson(JsonWriter writer, Dictionary<int, float> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<int, float> ReadJson(JsonReader reader, Type objectType,
            Dictionary<int, float> existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var readerValue = reader.Value as string;

            if (readerValue.IsNullOrEmpty())
            {
                return null;
            }
            // readerValue = readerValue.Trim('"');

            try
            {
                return JsonConvert.DeserializeObject<Dictionary<int, float>>(readerValue);
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
                return null;
            }
        }
    }
}