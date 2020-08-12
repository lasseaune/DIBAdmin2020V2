using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Data.Entities
{
    public class JsonRDataPatch
    {
        public string resourceId { get; set; }
        public string Id { get; set; }
        public string ob { get; set; }
        public string op { get; set; }
        public Dictionary<string, string> values { get; set; }
    }
}
