using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace DIBAdminAPI.Data.Entities
{
    public class Database
    {
        public int database_id { get; set; }
        public string name { get; set; }
        public string transactionId { get; set; }
    }
}
