using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataAccess.Utils.JsonParse
{
    public class StringToListDic : JsonConverter<Dictionary<int, Dictionary<int, float>>>
    {
        public override void WriteJson(JsonWriter writer, Dictionary<int, Dictionary<int, float>> value,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<int, Dictionary<int, float>> ReadJson(JsonReader reader, Type objectType,
            Dictionary<int, Dictionary<int, float>> existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var readerValue = reader.Value as string;
            // readerValue = readerValue.sTrim('"');
            return JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, float>>>(readerValue);
        }
    }
}