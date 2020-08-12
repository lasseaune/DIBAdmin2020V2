using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using DIBAdminAPI.Data;
using DIBAdminAPI.Services;
using DIBAdminAPI.Data.Entities;
using System.Xml.Linq;

namespace DIBAdminAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EditController : ControllerBase
    {
        private readonly IRepository _repo;
        private readonly ITempStorage _tempstore;
        private readonly ICacheService _cache;
        public EditController(IRepository repository, ITempStorage tempstore, ICacheService cacheService)
        {
            _repo = repository;
            _tempstore = tempstore;
            _cache = cacheService;
        }
        
        [HttpPost("create")]
        public IActionResult Create([FromBody]JsonCreateElements jc)
        {

            string resourceId = jc.resourceid;
            string segmentId = jc.segmentid ?? "";

            string rid = "rid="+ resourceId + ";sid=" + segmentId + ";_document";
            DocumentContainer result = _cache.Get<DocumentContainer>(rid);
            if (result == null)
            {
                return BadRequest("Document finnes ikke i cache");
            }
            EditResult editResult = result.CreateJsonElement(jc);
            if (editResult == null)
            {
                return BadRequest("Kunne ikke legge til");
            }
            JsonObject json = editResult.json;
            if (json == null)
            {
                return BadRequest("Kunne ikke legge til");
            }
            result = editResult.documentContainer;
            _cache.Set< DocumentContainer>(rid, result);
           
            return Ok(json);
        }
        [HttpPatch("")]
        public IActionResult Patch([FromBody]JsonUpdate ju)
        {
            string resourceId = ju.resourceid;
            string segmentId = ju.segmentid == null ? "" : ju.segmentid;
            string rid = "rid=" + resourceId + ";sid=" + segmentId + ";_document";
            DocumentContainer result = _cache.Get<DocumentContainer>(rid);
            if (result == null)
            {
                return BadRequest("Document finnes ikke i cache");
            }
            EditDocumentContainerResult ecr = result.UpdateJsonElements(ju, resourceId, segmentId);
            if (ecr==null)
            {
                return BadRequest("Kunne ikke oppdatere");
            }
            //bool success = result.elements.UpdateJsonElements(ju);
            //if (!success)
            //{
            //    return BadRequest("Kunne ikke oppdatere");
            //}
            _cache.Set<DocumentContainer>(rid, ecr.documentContainer);
            //return Ok(true);

            return Ok(ecr.json);
        }

        [HttpDelete("")]
        public IActionResult Delete([FromBody]JsonDelete jd)
        {
            string resourceId = jd.resourceid;
            string segmentId = jd.segmentid == null ? "" : jd.segmentid;
            string rid = "rid=" + resourceId + ";sid=" + segmentId + ";_document";
            DocumentContainer result = _cache.Get<DocumentContainer>(rid);
            if (result == null)
            {
                return BadRequest("Document finnes ikke i cache");
            }

            EditDocumentContainerResult ecr = result.DeleteJsonElement(jd, resourceId, segmentId);
            if (ecr == null)
            {
                return BadRequest("Kunne ikke oppdatere");
            }
            _cache.Set<DocumentContainer>(rid, ecr.documentContainer);
            //bool success = result.elements.DeleteJsonElement(jd);
            //if (!success)
            //{
            //    return BadRequest("Kunne ikke slette");
            //}
            //_cache.Set<DocumentContainer>(rid, result);

            return Ok(ecr.json);
        }
    }
}