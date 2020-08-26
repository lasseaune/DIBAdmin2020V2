using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Data.Entities
{
    public class Topictype
    {
        public string id { get; set; }
        public string name { get; set; }
        public int? assosSort { get; set; }
        public int? sortOrder { get; set; }
        public string categoryId { get; set; }

    }
}
