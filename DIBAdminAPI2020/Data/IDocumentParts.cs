using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DIBAdminAPI.Data.Entities;

namespace DIBAdminAPI.Data
{
    public interface IDocumentParts
    {
        Task<List<DocumentContainer>> GetParts(string resourceId, string segmentId, string id);
    }
}
