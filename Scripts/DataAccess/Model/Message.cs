using System.Collections.Generic;

namespace DataAccess.Model
{
    public class Message
    {
        public int id;
        public string user_id;
        public string type;
        public string text;
        public Dictionary<string, string> data;
    }
}