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
            var p = new
            {
                data.resourceId,
                Id= data.Id??"",
                data.ob,
                data.op,
                values = new XElement("values", data.values.Select(v => new XAttribute(v.Key, v.Value))),
                session_id = "apitest" //_usrsvc.CurrentUser.session_id,
            };
            if (data.ob == "topicdata")
            {
                XElement result = await _repo.ExecRData("[dbo].[Update_RDATA]", p);
                if (result == null)
                {
                    return BadRequest();
                }
                if ((string)result.Attributes("value").FirstOrDefault() == "1")
                {
                    if (Guid.TryParse(data.resourceId, out Guid topicId))
                    {
                        string query = "dbo.GetTopicDetail";
                        TopicDetail topicresult = await _repo.ExecTopicDetail(query, new { topic_id = topicId }, null);
                        TopicDetailAPI tdapi = new TopicDetailAPI(topicresult);
                        TopicPartsAPI tp = new TopicPartsAPI {
                            objects = tdapi.objects
                                    .Where(v => v.Value.type == "topicdata")
                                    .ToDictionary(v => v.Key, v => v.Value) };
                        return Ok(tp);
                    }
                }
                else
                {
                    return BadRequest();
                }
            }
            if (data.ob == "area")
            {

                XElement result = await _repo.ExecRData("[dbo].[Update_RDATA]", p);
                if (result == null)
                {
                    return BadRequest();
                }
                if ((string)result.Attributes("value").FirstOrDefault() == "1")
                {
                    if (Guid.TryParse(data.resourceId, out Guid topicId))
                    {
                        string query = "dbo.GetTopicDetail";
                        TopicDetail topicresult = await _repo.ExecTopicDetail(query, new { topic_id = topicId }, null);
                        TopicDetailAPI tdapi = new TopicDetailAPI(topicresult);
                        return Ok(tdapi);
                    }
                }
                else if ((string)result.Attributes("value").FirstOrDefault() == "0")
                {
                    return BadRequest((string)result.Attributes("message").FirstOrDefault());
                }
                return BadRequest();
            }
            if (data.ob == "topicname")
            {
                XElement result = await _repo.ExecRData("[dbo].[Update_RDATA]", p);
                if (result == null)
                {
                    return BadRequest();
                }
                if ((string)result.Attributes("value").FirstOrDefault() == "1")
                {
                    if (Guid.TryParse(data.resourceId, out Guid topicId))
                    {
                        string query = "dbo.GetTopicDetail";
                        TopicDetail topicresult = await _repo.ExecTopicDetail(query, new { topic_id = topicId }, null);
                        TopicDetailAPI tdapi = new TopicDetailAPI(topicresult);
                        TopicPartsAPI tp = new TopicPartsAPI
                        {
                            root = tdapi.objects
                                    .Where(v => v.Value.type == "name")
                                    .Select(v=>v.Key).ToList(),
                            objects = tdapi.objects
                                    .Where(v => v.Value.type == "name")
                                    .ToDictionary(v => v.Key, v => v.Value)
                        };
                        return Ok(tp);
                    }
                }
                else if((string)result.Attributes("value").FirstOrDefault() == "0")
                {
                    return BadRequest((string)result.Attributes("message").FirstOrDefault());
                }
            }
            return Ok(data);
        }
    }
}