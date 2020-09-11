using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Data.Entities
{
    public class DIBLink
    {
        public string resourceId { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public string segmentId { get; set; }
        public int? idx { get; set; }
        public string toid { get; set; }
        public string tosegmentId { get; set; }
        public int? toidx { get; set; }
        public string tag3 { get; set; }
        public string totag3 { get; set; }
    }
}
