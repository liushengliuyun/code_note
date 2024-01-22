using System;
using System.Collections.Generic;
using DataAccess.Model;
using Newtonsoft.Json;

namespace DataAccess.Utils.JsonParse
{
    public class NameToItem : JsonConverter<List<Item>>
    {
        public override void WriteJson(JsonWriter writer, List<Item> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override List<Item> ReadJson(JsonReader reader, Type objectType, List<Item> existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            reader.Read();
            var result = new List<Item>();
            while (reader.Value != null)
            {
                try
                {
                    var name = reader.Value as string;
                    var item = new Item(name);
                    reader.Read();
                    item.Count = Convert.ToSingle(reader.Value);
                    result.Add(item);
                    reader.Read();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return result;
        }
    }
}