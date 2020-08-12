using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Data.Entities
{
    public class Topictype
    {
        public int topic_type_id { get; set; }
        public string name { get; set; }
        public int assosiation_sort { get; set; }
        public int sortorder { get; set; }
        public int category_id { get; set; }

    }
}
