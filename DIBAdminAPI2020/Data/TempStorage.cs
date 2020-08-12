using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using System.Threading;
using DIBAdminAPI.Data.Entities;

namespace DIBAdminAPI.Data
{
    public class TempStorage : ITempStorage
    {
        private readonly string _temppath;
        public TempStorage()
        {
            string tempfolder = Path.GetTempPath();
            string folder = tempfolder + "dibadmin2020";
            Task<string> x = CreateFolder(folder);
            
            _temppath = x.Result + @"\";
        }
        public async Task<string>CreateFolder(string folder)
        {
            string result = "";
            await Task.Factory.StartNew(delegate
            {
                
                if (!Directory.Exists(folder))
                {
                    DirectoryInfo di = Directory.CreateDirectory(folder);
                    while (!di.Exists)
                    {
                        Thread.Sleep(500);
                    };
                    result = di.FullName;
                }
                else 
                {
                    result = folder;
                }
            });
            return result;
        }
        public async Task<bool> Store(string resourceid, XElement document)
        {
            bool result = false;
            await Task.Factory.StartNew(delegate
            {
                if (!(_temppath != "" ? Directory.Exists(_temppath) : false))
                {
                    result = false;
                }
                else
                {
                    string recourcepath = _temppath + resourceid;

                    if (!Directory.Exists(recourcepath))
                    {
                        Task<string> x = CreateFolder(recourcepath);
                        if (x.Result == "")
                        {
                            result = false;
                            return;
                        }
                    }
                    string filename = recourcepath + @"\" + resourceid + @".xml";
                    document.Save(filename);
                    result = true;
                }
            });
            return result;
        }

        //public async Task<bool> SaveElement(JsonObject jsonObject)
        //{
        //    bool result = false;
        //    await Task.Factory.StartNew(delegate
        //    {
        //        string resourceid = jsonObject.resourceid;
        //        JsonUpdateObject updateObject = jsonObject.GetUpdateXML();

        //        Task<XElement> data = GetDocument(resourceid);
        //        if (data.Result != null)
        //        {
        //            string id = (string)updateObject.updateElement.Attributes("id").FirstOrDefault();

        //            XElement update = data.Result.Elements("docpart").Elements("document").Descendants().Where(p => ((string)p.Attributes("id").FirstOrDefault() ?? "").Trim().ToLower() == id).FirstOrDefault();
        //            if (update!=null)
        //            {
        //                update.ReplaceWith(updateObject.updateElement);
        //                Task<bool> x = Store(resourceid, data.Result);
        //                result = x.Result;
        //            }
        //            else
        //            {
        //                id = updateObject.parentId;
        //                string name = updateObject.parentName;
        //                XElement parent = data.Result.Elements("docpart").Elements("document").Descendants().Where(p => ((string)p.Attributes("id").FirstOrDefault() ?? "").Trim().ToLower() == id && p.Name.LocalName==name).FirstOrDefault();
        //                if (parent != null)
        //                {
        //                    if (updateObject.elementBeforeId != null)
        //                    {
        //                        XElement before = parent.Elements().Where(p => (string)p.Attributes("id").FirstOrDefault() == updateObject.elementBeforeId).FirstOrDefault();
        //                        if (before != null)
        //                        {
        //                            before.AddAfterSelf(updateObject.updateElement);
        //                            Task<bool> x = Store(resourceid, data.Result);
        //                            result = x.Result;
        //                        }
        //                        else
        //                        {
        //                            result = false;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        parent.AddFirst(updateObject.updateElement);
        //                        Task<bool> x = Store(resourceid, data.Result);
        //                        result = x.Result;
        //                    }
        //                }
        //                else
        //                {
        //                    result = false;
        //                }
        //            }

                        
                    
        //        }
        //        else
        //        {
        //            result = false;
        //        }
                
        //    });
        //    return result;
            
        //}

        public async Task<XElement> GetDocument(string resourceid)
        {
            XElement result = null;
            await Task.Factory.StartNew(delegate
            {
                string path = _temppath + resourceid + @"\" + resourceid + ".xml";
                if (File.Exists(path))
                {
                    result = XElement.Load(path);
                }
            });
            return result;
        }
       
    }
}
