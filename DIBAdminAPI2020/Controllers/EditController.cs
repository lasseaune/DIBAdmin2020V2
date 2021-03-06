﻿using System;
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
    [Route("/[controller]")]
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

            string resourceId = jc.resourceId;
            string segmentId = jc.segmentId ?? "";

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
            result.Edited = true;
            _cache.Set< DocumentContainer>(rid, result);
           
            return Ok(json);
        }
        [HttpPatch("")]
        public IActionResult Patch([FromBody]JsonUpdate ju)
        {
            string resourceId = ju.resourceId;
            string segmentId = ju.segmentId == null ? "" : ju.segmentId;
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
            ecr.documentContainer.Edited = true;
            _cache.Set<DocumentContainer>(rid, ecr.documentContainer);

            return Ok(ecr.json);
        }

        [HttpDelete("")]
        public IActionResult Delete([FromBody]JsonDelete jd)
        {
            string resourceId = jd.resourceId;
            string segmentId = jd.segmentId == null ? "" : jd.segmentId;
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
            ecr.documentContainer.Edited = true;
            _cache.Set<DocumentContainer>(rid, ecr.documentContainer);

            return Ok(ecr.json);
        }
    }
}