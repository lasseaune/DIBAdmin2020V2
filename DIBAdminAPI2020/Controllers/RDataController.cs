using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DIBAdminAPI.Data;
using DIBAdminAPI.Services;
using DIBAdminAPI.Data.Entities;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace DIBAdminAPI.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class RDataController : ControllerBase
    {
        private readonly IRepository _repo;
        private readonly ITempStorage _tempstore;
        private readonly ICacheService _cache;
        public RDataController(IRepository repository, ITempStorage tempstore, ICacheService cacheService)
        {
            _repo = repository;
            _tempstore = tempstore;
            _cache = cacheService;
        }

        private class DeleteObject
        {
            public string resourceId { get; set; }
            public string segmentId { get; set; }
            public string Id { get; set; }
            public string ob { get; set; }
            public string op { get; set; }
        }

        [HttpDelete("")]
        public async Task<IActionResult> Delete([FromBody] IEnumerable<JsonRDataPatch> data)
        {
            string transactionId = Guid.NewGuid().ToString();
            if ((data == null ? 0 : data.Count()) == 0)
                return BadRequest("Bad input!");
            List<DeleteObject> inputData =
                data
                .GroupBy(p => new { p.resourceId, segmentId =  p.segmentId??"", Id = p.Id ?? "", p.ob, p.op })
                .Select(p => new DeleteObject
                {
                    resourceId = p.Key.resourceId,
                    segmentId = p.Key.segmentId,
                    Id = p.Key.Id ?? "",
                    ob = p.Key.ob,
                    op = p.Key.op
                }
                ).ToList();
            if (inputData.Count() != 1)
            {
                return BadRequest("Mixed operations!");
            }

            DeleteObject deleteData = inputData.FirstOrDefault();

            if (deleteData == null)
                return BadRequest("Missing input!");

            if (deleteData.op != "delete")
            {
                return BadRequest("Only delete operations supported!");
            }

            if (!("accline;taxline;tag;related;accounting".Contains(deleteData.ob)))
            {
                return BadRequest("Object not supported!");
            }

            var p = new
            {
                deleteData.resourceId,
                deleteData.Id,
                deleteData.ob,
                deleteData.op,
                transactionId,
                values = new XElement("values", data.Where(p=>p.values!=null).Select(p => new XElement("value", p.values.Select(v => new XAttribute(v.Key, v.Value))))),
                session_id = "apitest" //_usrsvc.CurrentUser.session_id,
            };

            XElement result = await _repo.ExecRData("[dbo].[Update_RDATA]", p);
            if (result == null)
            {
                return BadRequest("No data");
            }
            if ((string)result.Attributes("value").FirstOrDefault() == "1")
            {
                string rid = "rid=" + deleteData.resourceId + ";sid=" + deleteData.segmentId + ";_document";
                DocumentContainer dc = null;
                dc = _cache.Get<DocumentContainer>(rid);

                Dictionary<string, JsonElement> updatedElements = null;
                List<string> deletedLines = null;
                //List<KeyValuePair<string, AccountingElementApi>> ed = null;
                Dictionary<string, List<string>> ed = null;
                TopicPartsAPI dp = null;
                deletedLines = result.Elements("id").Select(p => (string)p.Attributes("id").FirstOrDefault()).ToList();
                
                if (deleteData.ob == "accounting")
                {
                    foreach (string s in deletedLines)
                    {
                        //AccountingElementApi ace = null;
                        Dictionary<string, List<string>> ace;
                        dc.elementdata.TryGetValue(s, out ace);
                        if (ace != null)
                        {
                            
                            foreach(string o in  ace.Where(p=>p.Key=="accounting").SelectMany(p => p.Value)
                                .Union(ace.Where(p=>p.Key=="tax").SelectMany(p => p.Value))
                            )
                            {
                                dc.objects.Remove(o);
                            }
                            dc.elementdata.Remove(s);
                        }

                        KeyValuePair<string, JsonElement> ae = dc.elements.Where(p=>p.Key==s).FirstOrDefault();
                        if (ae.Value.otherprops!=null)
                        {
                            if (ae.Value.otherprops.ContainsKey("accounting"))
                            {
                                ae.Value.otherprops.Remove("accounting");
                            }
                        }
                        updatedElements = new Dictionary<string, JsonElement>();
                        updatedElements.Add(ae.Key, ae.Value);

                    }
                    dp = new TopicPartsAPI
                    {
                        
                        root = deletedLines,
                        elements = updatedElements,
                        objects = null
                    };
                    return Ok(dp);

                }
                else if (deleteData.ob == "accline" || deleteData.ob == "taxline")
                {
                     ed = 
                        (from Dictionary<string, List<string>> k in dc.elementdata.SelectMany(p=>p.Value).Where(p=>p.Key== "accounting").Select(p=>p)
                         from a in k
                         join d in deletedLines
                         on a.Key equals d
                         select k
                        )
                        .Union(
                            from List<string> k in dc.elementdata.SelectMany(p => p.Value).Where(p => p.Key == "tax").Select(p => p.Value)
                            from a in k
                            join d in deletedLines
                            on a equals d
                            select k
                        )
                        .ToList();

                    updatedElements =
                    (
                        from el in dc.elements
                        join d in ed
                        on el.Key equals d.Key
                        select new { el, d }
                    )
                    .ToList()
                    .SelectMany(p => p.el.UpdateOtherProps(p.d, deletedLines, "accounting"))
                    .ToDictionary(p => p.Key, p => p.Value);
                }

                dp = new TopicPartsAPI
                {
                    root = deletedLines,
                    elementdata = ed.ToDictionary(p=>p.Key, p=>p.Value),
                    elements = updatedElements,
                    objects = null
                };
                return Ok(dp); 
            }
            else
            {
                string message = (string)result.Attributes("message").FirstOrDefault();
                return BadRequest(message);
            }
            
        }
    
        [HttpPatch("")]
        public async Task<IActionResult> Patch([FromBody]JsonRDataPatch data)
        {
            string transactionId = Guid.NewGuid().ToString();
            
            if (!(Guid.TryParse(data.resourceId, out Guid test)))
            {
                return BadRequest();
            }
            string resourceId = data.resourceId;
            string segment_id = data.segmentId ?? "";
            string Id = data.Id ?? "";
            var p = new
            {
                data.resourceId,
                Id,
                data.ob,
                data.op,
                transactionId,
                values = new XElement("values", new XElement("value", data.values.Select(v => new XAttribute(v.Key, v.Value)))),
                session_id = "apitest" //_usrsvc.CurrentUser.session_id,
            };
            Dictionary<string, JsonElement> updatedElements = null;
            string rid = "rid=" + resourceId + ";sid=" + segment_id + ";_document";
            DocumentContainer dc = null;
            dc = _cache.Get<DocumentContainer>(rid);
            List<string> deletedLines = null;
            //if (data.ob == "cllable")
            //{
            //    XElement result = await _repo.ExecRData("[dbo].[Update_RDATA]", p);
            //    if (result == null)
            //    {
            //        return BadRequest("No data");
            //    }
            //    if ((string)result.Attributes("value").FirstOrDefault() == "1")
            //    {
            //        if (data.op == "delete")
            //        {
            //            deletedLines = result.Elements("id").Select(p => (string)p.Attributes("id").FirstOrDefault()).ToList();
            //            dc.objects.Where(p=>p.Value.type=="clgroup")
            //        }
            //    }
            //}
            if ("topicdata;database;name;tag;date;related".Split(';').Contains(data.ob))
            {
                XElement result = await _repo.ExecRData("[dbo].[Update_RDATA]", p);
                if (result == null)
                {
                    return BadRequest("No data");
                }
                if ((string)result.Attributes("value").FirstOrDefault() == "1")
                {
                    if (data.op == "delete")
                    {

                        if (data.ob == "tag" && Id != "")
                        {
                            IEnumerable<Tag> tags = await _repo.GetTags(new { resourceId, Id });
                            TagsAPI tagsAPI = new TagsAPI(tags);

                            KeyValuePair<string, JsonElement> ae = dc.elements.Where(p => p.Key == Id).FirstOrDefault();
                            if (ae.Key != null)
                            {
                                if (tagsAPI.objects.Count() > 0)
                                    updatedElements = ae.UpdateOtherProps("tags").ToDictionary(p => p.Key, p => p.Value);
                                else
                                {
                                    if (ae.Value.otherprops == null ? false : ae.Value.otherprops.ContainsKey("related"))
                                    {
                                        ae.Value.otherprops.Remove("related");
                                        updatedElements.Add(ae.Key, ae.Value);
                                        _cache.Set<DocumentContainer>(rid, dc);
                                    }
                                }
                            }

                            TopicPartsAPI tp = new TopicPartsAPI
                            {
                                root = result.Elements("id").Select(p => (string)p.Attributes("id").FirstOrDefault()).ToList(),
                                elements = updatedElements
                            };
                            return Ok(tp);
                        }
                        else if (data.ob == "related" && Id != "")
                        {
                            RelatedResources related = await _repo.GetRelated(new { resourceId, Id });
                            RelatedsAPI relatedAPI = new RelatedsAPI(related);
                            KeyValuePair<string, JsonElement> ae = dc.elements.Where(p => p.Key == Id).FirstOrDefault();
                            if (ae.Key != null)
                            {
                                if (relatedAPI.objects.Count() > 0 )
                                    updatedElements = ae.UpdateOtherProps("related").ToDictionary(p => p.Key, p => p.Value);
                                else
                                {
                                    if (ae.Value.otherprops==null ? false : ae.Value.otherprops.ContainsKey("related"))
                                    {
                                        ae.Value.otherprops.Remove("related");
                                        updatedElements.Add(ae.Key, ae.Value);
                                        
                                    }
                                }
                                _cache.Set<DocumentContainer>(rid, dc);
                            }

                            TopicPartsAPI tp = new TopicPartsAPI
                            {
                                root = result.Elements("id").Select(p => (string)p.Attributes("id").FirstOrDefault()).ToList(),
                                elements = updatedElements
                            };
                            return Ok(tp);
                        }
                        else
                        {
                            TopicPartsAPI dp = new TopicPartsAPI
                            {
                                root = result.Elements("id").Select(p => (string)p.Attributes("id").FirstOrDefault()).ToList(),
                                objects = null
                            };
                            return Ok(dp);
                        }
                    }
                    var t = new
                    {
                        resourceId,
                        data.ob,
                        transactionId
                    };

                    if (data.ob == "tag" && Id != "")
                    {
                        IEnumerable<Tag> tags = await _repo.GetTags(new { resourceId, Id });
                        TagsAPI tagsAPI = new TagsAPI(tags);

                        KeyValuePair<string, JsonElement> ae = dc.elements.Where(p => p.Key == Id).FirstOrDefault();
                        if (ae.Key != null)
                        { 
                            updatedElements =  ae.UpdateOtherProps("tags").ToDictionary(p=>p.Key, p=>p.Value);
                        }

                        TopicPartsAPI tp = new TopicPartsAPI
                        {
                            root = tagsAPI.objects
                                  .Where(v => v.Value.type == data.ob && v.Value.transactionId == transactionId)
                                  .Select(v => v.Key).ToList(),
                            objects = tagsAPI.objects
                                  .Where(v => v.Value.type == data.ob && v.Value.transactionId == transactionId)
                                  .ToDictionary(v => v.Key, v => v.Value),
                            elements = updatedElements
                        };
                        _cache.Set<DocumentContainer>(rid, dc);
                        return Ok(tp);
                    }
                    else if (data.ob == "related" && Id != "")
                    {
                        RelatedResources related = await _repo.GetRelated(new { resourceId, Id });
                        RelatedsAPI relatedAPI = new RelatedsAPI(related);
                        KeyValuePair<string, JsonElement> ae = dc.elements.Where(p => p.Key == Id).FirstOrDefault();
                        if (ae.Key != null)
                        {
                            updatedElements = ae.UpdateOtherProps("related").ToDictionary(p => p.Key, p => p.Value);
                        }
                        TopicPartsAPI tp = new TopicPartsAPI
                        {
                            root = relatedAPI.objects
                                  .Where(v => v.Value.type == data.ob && v.Value.transactionId == transactionId)
                                  .Select(v => v.Key).ToList(),
                            objects = relatedAPI.objects
                                  .Where(v => v.Value.type == data.ob && v.Value.transactionId == transactionId)
                                  .ToDictionary(v => v.Key, v => v.Value),
                            elements = updatedElements
                        };
                        _cache.Set<DocumentContainer>(rid, dc);
                        return Ok(tp);
                    }
                    else
                    {

                        string query = "dbo.GetTopic";
                        TopicDetail topicresult = await _repo.ExecTopicDetail(query, t, null);
                        TopicDetailAPI tdapi = new TopicDetailAPI(topicresult);
                        TopicPartsAPI tp = new TopicPartsAPI
                        {
                            root = tdapi.objects
                                    .Where(v => v.Value.type == data.ob && v.Value.transactionId == transactionId)
                                    .Select(v => v.Key).ToList(),
                            objects = tdapi.objects
                                    .Where(v => v.Value.type == data.ob && v.Value.transactionId == transactionId)
                                    .ToDictionary(v => v.Key, v => v.Value)
                        };

                        return Ok(tp);
                    }
                }
                else
                {
                    string message = (string)result.Attributes("message").FirstOrDefault();
                    return BadRequest(message);
                }
            }
            else if ("accline;taxline".Split(';').Contains(data.ob) && (data.Id ?? "") != "")
            {
                
                XElement result = await _repo.ExecRData("[dbo].[Update_RDATA]", p);
                if (result == null)
                {
                    return BadRequest("No data");
                }
                if ((string)result.Attributes("value").FirstOrDefault() == "1")
                {
                    
                    
                    if (data.op == "delete")
                    {
                        deletedLines = result.Elements("id").Select(p => (string)p.Attributes("id").FirstOrDefault()).ToList();
                        List<KeyValuePair<string, AccountingElementApi>> ed = null;
                        if (data.ob == "accline" || data.ob == "taxline")
                        {
                            ed =
                                (from KeyValuePair<string, AccountingElementApi> k in dc.elementdata
                                 from a in k.Value.accounting
                                 join d in deletedLines
                                 on a equals d
                                 select k
                                )
                                .Union(
                                    from KeyValuePair<string, AccountingElementApi> k in dc.elementdata
                                    from a in k.Value.tax
                                    join d in deletedLines
                                    on a equals d
                                    select k
                                )
                                .ToList();

                            updatedElements =
                            (
                                from el in dc.elements
                                join d in ed
                                on el.Key equals d.Key
                                select new { el, d }
                            )
                            .ToList()
                            .SelectMany(p => p.el.UpdateOtherProps(p.d, deletedLines, "accounting"))
                            .ToDictionary(p=>p.Key, p=>p.Value);
                        }

                        TopicPartsAPI dp = new TopicPartsAPI
                        {
                            root = deletedLines,
                            elementdata = ed.ToDictionary(p=>p.Key, p=>p.Value),
                            elements = updatedElements,
                            objects = null
                            
                        };

                        _cache.Set<DocumentContainer>(rid, dc);
                        return Ok(dp);
                    }
                    
                    var t = new
                    {
                        resource_id = resourceId,
                        id = data.Id,
                        data.ob,
                        session_id = "apitest"
                    };
                    string query = "[dbo].[GetResourceHTMLElement]";
                    ResourceHTML5Element resel = await _repo.GetRecourceElementdata(query, t, null);
                    if (resel == null)
                    {
                        return BadRequest();
                    }
                    DocumentContainer c = new DocumentContainer(resel);
                    DocumentElementdata ded = new DocumentElementdata
                    
                    {
                        elementdata = c.elementdata,
                        objects = c.objects
                                .Where(v => v.Value.type == data.ob && v.Value.transactionId == transactionId)
                                .ToDictionary(v => v.Key, v => v.Value)
                    };
                    if (dc!=null)
                    {
                        (
                          from e in dc.elementdata
                          join n in ded.elementdata
                          on e.Key equals n.Key
                          select e
                        ).ToList()
                        .ForEach(d => dc.elementdata.Remove(d.Key));

                        dc.elementdata.AddRange(ded.elementdata);
                        (
                            from o in dc.objects
                            join n in ded.objects
                            on o.Key equals n.Key
                            select o
                         ).ToList()
                         .ForEach(d => dc.objects.Remove(d.Key));
                        dc.objects.AddRange(ded.objects);


                        updatedElements =
                           (
                               from el in dc.elements
                               join d in ded.elementdata
                               on el.Key equals d.Key
                               select new { el, d }
                           )
                           .ToList()
                           .SelectMany(p => p.el.UpdateOtherProps(p.d, deletedLines, "accounting"))
                           .ToDictionary(p => p.Key, p => p.Value);

                        _cache.Set<DocumentContainer>(rid, dc);
                    }
                    

                    TopicPartsAPI tp = new TopicPartsAPI
                    {
                        elementdata = c.elementdata,
                        elements = updatedElements,
                        root = c.objects
                                .Where(v => v.Value.type == data.ob && v.Value.transactionId == transactionId)
                                .Select(v => v.Key).ToList(),
                        objects = c.objects
                                .Where(v => v.Value.type == data.ob && v.Value.transactionId == transactionId)
                                .ToDictionary(v => v.Key, v => v.Value)
                    };
                    return Ok(tp);

                }
                else
                {
                    string message = (string)result.Attributes("message").FirstOrDefault();
                    return BadRequest(message);
                }
            }
            else
            {
                return BadRequest("No action");
            }
        }
    }
}