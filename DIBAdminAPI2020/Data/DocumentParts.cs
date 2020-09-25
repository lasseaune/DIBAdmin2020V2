using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DIBAdminAPI.Services;
using DIBAdminAPI.Data.Entities;

namespace DIBAdminAPI.Data
{
    public class DocumentParts
    { 
        public List<DocumentContainer> parts { get; set; }
    }
}
