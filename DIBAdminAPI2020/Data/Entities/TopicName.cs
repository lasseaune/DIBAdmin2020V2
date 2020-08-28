using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Data.Entities
{
    public class TopicNameType
    {
        public string id { get; set; }
        public string name { get; set; }
    }
    public class TopicNameAPI
    {
        public string id { get; set; }
        public string name { get; set; }
        public string nametypeId { get; set; }
        public bool isDefault { get; set; }
    }
    public class TopicName
    {
        public string id { get; set; }
        public string name { get; set; }
        public bool isdefault { get; set; }
        public string nametypeId { get; set; }
        public string transactionId { get; set; }


    }
}
