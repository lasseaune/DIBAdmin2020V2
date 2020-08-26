using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Data.Entities
{
    public class DateType
    {
         public string id { get; set; }
         public string name { get; set; }   
    }
    public class DatesAPI
    {
        public string id { get; set; }
        public string datetypeId { get; set; }
        public DateTime date { get; set; }
    }
    public class Dates
    {
        public string id { get; set; }
        public string datetypeId { get; set; }
        public DateTime date { get; set; }
        public string transactionId { get; set; }
    }
}
