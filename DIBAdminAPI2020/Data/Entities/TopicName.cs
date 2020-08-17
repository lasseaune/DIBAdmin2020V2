using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Data.Entities
{
    public class TopicName
    {
        public Guid topic_name_id { get; set; }
        public string name { get; set; }
        public bool isdefault { get; set; }
        public int topic_name_type_id { get; set; }
        public string transactionId { get; set; }


    }
}
