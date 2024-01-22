using System.Collections.Generic;

namespace DataAccess.Controller
{
    public interface IRequestSender
    {
        SortedDictionary<string, object> ParamHandler { get; }
    }
}