using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using DIBAdminAPI.Data.Entities;

namespace DIBAdminAPI.Data
{
    public interface ITempStorage
    {
        Task<bool> Store(string resourceid, XElement document);
        //Task<bool> SaveElement(JsonObject jsonObject);
        Task<XElement> GetDocument(string resourceid);
        Task<string> CreateFolder(string folder);
    }
}
