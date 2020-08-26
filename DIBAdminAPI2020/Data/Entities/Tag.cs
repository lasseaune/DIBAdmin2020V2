using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Data.Entities
{
    public class TagAPI
    {
        public string id { get; set; }
        public string name { get; set; }
        public string tagtypeId {get;set;}
        public string language { get; set; }
    }
    public class Tag
    {
        public string id { get; set; }
        public string name { get; set; }
        public string tagtypeId { get; set; }
        public string language { get; set; }
        public string transactionId { get; set; }

    }
}
