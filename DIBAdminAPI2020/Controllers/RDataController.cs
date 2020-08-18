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
                    if (data.op=="delete")
                    {
                        TopicPartsAPI dp = new TopicPartsAPI
                        {
                            root = new List<string>
                            {
                                (string)result.Attributes("id").FirstOrDefault()
                            },
                            objects= null
                        };
                        return Ok(dp);
                    }
                    var t = new
                    {
                        topic_id,
                        data.ob,
                        transactionId
                    };
                    string query = "dbo.GetTopicDetail";
                    TopicDetail topicresult = await _repo.ExecTopicDetail(query, t , null);
                    TopicDetailAPI tdapi = new TopicDetailAPI(topicresult);
                    TopicPartsAPI tp = new TopicPartsAPI {
                        root = tdapi.objects
                                .Where(v => v.Value.type == data.ob && v.Value.transactionId == transactionId)
                                .Select(v => v.Key).ToList(),
                        objects = tdapi.objects
                                .Where(v => v.Value.type == data.ob && v.Value.transactionId == transactionId )
                                .ToDictionary(v => v.Key, v => v.Value) };
                        
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