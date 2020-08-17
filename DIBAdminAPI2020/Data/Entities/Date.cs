using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Data.Entities
{
    public class DateType
    {
         public int date_type_id { get; set; }
         public string name { get; set; }   
    }

    public class Dates
    {
        public Guid id { get; set; }
        public int date_type_id { get; set; }
        public DateTime date { get; set; }
        public string transactionId { get; set; }
    }
}
