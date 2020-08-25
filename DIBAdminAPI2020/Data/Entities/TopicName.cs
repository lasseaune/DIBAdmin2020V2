using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Data.Entities
{
    public class TopicNameAPI
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public int typeId { get; set; }
        public bool Default { get; set; }
    }
    public class TopicName
    {
        public Guid topicnameId { get; set; }
        public string name { get; set; }
        public bool isdefault { get; set; }
        public int topic_name_type_id { get; set; }
        public string transactionId { get; set; }


    }
}
