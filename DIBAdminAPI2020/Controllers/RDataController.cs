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

namespace DIBAdminAPI.Controllers
{
    [Route("api/[controller]")]
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
        [HttpPatch("")]
        public async Task<IActionResult> Patch([FromBody]JsonRDataPatch data)
        {
            string transactionId = Guid.NewGuid().ToString();
            if (!(Guid.TryParse(data.resourceId, out Guid test)))
            {
                return BadRequest();
            }
            string topic_id = data.resourceId;
            string segment_id = data.segmentId ?? "";
            var p = new
            {
                data.resourceId,
                Id = data.Id ?? "",
                data.ob,
                data.op,
                transactionId,
                values = new XElement("values", data.values.Select(v => new XAttribute(v.Key, v.Value))),
                session_id = "apitest" //_usrsvc.CurrentUser.session_id,
            };

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
                        TopicPartsAPI dp = new TopicPartsAPI
                        {
                            root = new List<string>
                            {
                                (string)result.Attributes("id").FirstOrDefault()
                            },
                            objects = null
                        };
                        return Ok(dp);
                    }
                    var t = new
                    {
                        topic_id,
                        data.ob,
                        transactionId
                    };
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
                else
                {
                    string message = (string)result.Attributes("message").FirstOrDefault();
                    return BadRequest(message);
                }
            }
            else if ("accline;taxline".Split(';').Contains(data.ob) && (data.Id ?? "") != "")
            {
                string rid = "rid=" + topic_id + ";sid=" + segment_id + ";_document";
                DocumentContainer dc = null;
                dc = _cache.Get<DocumentContainer>(rid);
                XElement result = await _repo.ExecRData("[dbo].[Update_RDATA]", p);
                if (result == null)
                {
                    return BadRequest("No data");
                }
                if ((string)result.Attributes("value").FirstOrDefault() == "1")
                {
                    if (data.op == "delete")
                    {
                        string deleteId = (string)result.Attributes("id").FirstOrDefault();
                        TopicPartsAPI dp = new TopicPartsAPI
                        {
                            root = new List<string>
                            {
                                deleteId
                            },
                            objects = null
                        };

                        if (data.ob == "accline")
                        {
                            List<string> s = dc.elementdata.Values.Where(d => d.accounting.Contains(deleteId)).Select(d => d.accounting).FirstOrDefault();
                            if (s!=null)
                                s.Remove(deleteId);
                        }
                        if (data.ob == "taxline")
                        {
                            List<string> s = dc.elementdata
                                .Values
                                .Where(d => d.tax.Contains(deleteId))
                                .Select(d => d.tax).FirstOrDefault();
                            if (s!=null)
                                s.Remove(deleteId);
                        }

                        _cache.Set<DocumentContainer>(rid, dc);
                        return Ok(dp);
                    }
                    var t = new
                    {
                        resource_id = topic_id,
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

                        KeyValuePair<string, JsonElement> oa = dc.elements.Where(d => d.Key == ded.elementdata.FirstOrDefault().Key).Select(d => d).FirstOrDefault();
                        if (oa.Key != null)
                        {
                            KeyValuePair<string, string> accref = new KeyValuePair<string, string>("accref", ded.elementdata.FirstOrDefault().Key);
                            oa.Value.otherprops.Remove("accref");
                            oa.Value.otherprops.Add("accref", ded.elementdata.FirstOrDefault().Key);
                        }
                        _cache.Set<DocumentContainer>(rid, dc);

                    }
                    

                    TopicPartsAPI tp = new TopicPartsAPI
                    {
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