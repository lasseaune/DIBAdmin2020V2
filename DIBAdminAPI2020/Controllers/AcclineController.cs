using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DIBAdminAPI.Data;
using DIBAdminAPI.Services;
using DIBAdminAPI.Data.Entities;
using System.Xml.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DIBAdminAPI.Controllers
{
    [Route("api/[controller]")]
    public class AcclineController : Controller
    {
        private readonly IRepository _repo;
        private readonly ITempStorage _tempstore;
        private readonly ICacheService _cache;
        public AcclineController(IRepository repository, ITempStorage tempstore, ICacheService cacheService)
        {
            _repo = repository;
            _tempstore = tempstore;
            _cache = cacheService;
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody]AccountingJson aj)
        {
            string resourceId = aj.resourceid;
            string segmentId = aj.segmentid == null ? "" : aj.segmentid;
            string rid = "rid=" + resourceId + ";sid=" + segmentId + ";_document";

            DocumentContainer result = _cache.Get<DocumentContainer>(rid);
            if (result == null)
            {
                return BadRequest("Document finnes ikke i cache");
            }



            return Ok();
        }
        [HttpPatch("")]
        public IActionResult Patch([FromBody]JsonUpdate aj)
        {
            return Ok();
        }

        [HttpDelete("")]
        public IActionResult Delete([FromBody]JsonDelete aj)
        {
            return Ok();
        }
    }
}
