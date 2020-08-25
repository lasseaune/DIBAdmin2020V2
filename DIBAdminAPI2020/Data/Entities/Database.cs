using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace DIBAdminAPI.Data.Entities
{
    public class DatabaseAPI
    {
        public int databaseId { get; set; }
        public string name { get; set; }
    }
    public class Database
    {
        public int databaseId { get; set; }
        public string name { get; set; }
        public string transactionId { get; set; }
    }
}
