using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DIBAdminAPI.Data;
using DIBAdminAPI.Data.Entities;
using System.Xml.Linq;

namespace DIBAdminAPI.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class TopicsController : ControllerBase
    {
        private readonly IRepository _repo;
        public TopicsController(IRepository repository)
        {
            _repo = repository;
        }
        // GET: api/Topics
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _repo.ExecTopics("dbo.GetTopics", null, null);
            return Ok(result);
        }
        // GET: api/Topics/5
        [HttpGet("s")]
        public async Task<IActionResult> Search([FromQuery] Topicsearch q)
        {
            XElement filter = q.SearchFilter();
            string query = (string)filter.Attributes("query").FirstOrDefault();
            XElement items = new XElement("items", filter.Elements("item"));

            var result = await _repo.ExecTopics("dbo.GetTopics", new {q=query, filter=items },null);

            return Ok(result);
        }
        // GET: api/Topics/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<IActionResult> Get(Guid id)
        {
            string query = "dbo.GetTopicDetail";
            TopicDetail result = await _repo.ExecTopicDetail(query, new {topic_id = id}, null);
            TopicDetailAPI tdapi = new TopicDetailAPI(result);
            return Ok(tdapi);
        }

        // POST: api/Topics
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TopicCreate data)
        {
            var p = new
            {
                data.name,
                data.ignoreduplicate,
                session_id = "apitest" //_usrsvc.CurrentUser.session_id,
            };

            XElement result = await _repo.ExecRData("[dbo].[Create_Topic]", p);
            if (result==null)
            {
                return BadRequest("Document missing!");
            }
            if ((string)result.Attributes("value").FirstOrDefault() == "400")
            {
                return BadRequest("Name exists");
            }
            else if ((string)result.Attributes("value").FirstOrDefault() == "1")
            {
                string id = (string)result.Attributes("id").FirstOrDefault();
                if (Guid.TryParse(id, out Guid topicId))
                {
                    string query = "dbo.GetTopicDetail";
                    TopicDetail topicresult = await _repo.ExecTopicDetail(query, new { topic_id = topicId }, null);
                    TopicDetailAPI tdapi = new TopicDetailAPI(topicresult);
                    return Ok(tdapi);
                }
            }
            return BadRequest();

        }

        // PUT: api/Topics/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
            
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var p = new
            {
                id,
                session_id = "apitest" //_usrsvc.CurrentUser.session_id,
            };
            XElement result = await _repo.ExecRData("[dbo].[Delete_Topic]", p);
            if (result == null)
            {
                return BadRequest();
            }

            if ((string)result.Attributes("value").FirstOrDefault() == "1")
            {
                return Ok();
            }

            if ((string)result.Attributes("value").FirstOrDefault() == "0")
            {
                return BadRequest();
            }
            return BadRequest();
        }
    }
}
