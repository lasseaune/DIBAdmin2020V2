﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Data.Entities
{
    public class Tag
    {
        public Guid tagId { get; set; }
        public int tag_id { get; set; }
        public string tag { get; set; }
        public int tag_type_id { get; set; }
        public string language { get; set; }
        public string transactionId { get; set; }

    }
}
