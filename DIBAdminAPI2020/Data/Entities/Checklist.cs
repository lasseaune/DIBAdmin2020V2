﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Data.Entities
{
    public class Labels
    {
        public bool Global = false;
        public bool View = false;
    }
    public class ChecklistLabelGroup
    {
        public Guid? labelGroupId { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string transactionId { get; set; }
    }
    public class ChecklistLabel
    {
        public Guid? labelId { get; set; }
        public string name { get; set; }
        public Guid? labelGroupId { get; set; }
        public string transactionId { get; set; }
    }
    public class ChecklistItemData
    {
        public string id { get; set; }
        public Guid? labelId { get; set; }
        public string transactionId { get; set; }
    }

}
