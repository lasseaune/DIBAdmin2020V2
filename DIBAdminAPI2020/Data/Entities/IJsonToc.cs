using System.Collections.Generic;

namespace DIBAdminAPI.Data.Entities
{
    public interface IJsonToc
    {
        Dictionary<string, string> attributes { get; set; }
        List<JsonChild> children { get; set; }
        string name { get; set; }
        string type { get; set; }

        bool Equals(object obj);
    }
    public interface IJsonElement
    {
        Dictionary<string, dynamic> otherprops { get; set; }
        Dictionary<string, string> attributes { get; set; }
        List<JsonChild> children { get; set; }
        string name { get; set; }
        string type { get; set; }
        
    }
}