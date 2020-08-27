using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DIBAdminAPI.Data;
using DIBAdminAPI.Data.Entities;
using DIBAdminAPI.Services;

namespace DIBAdminAPI.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IRepository _repo;
        private readonly ICacheService _cache;
        public HomeController(IRepository repository, ICacheService cacheService)
        {
            _repo = repository;
            _cache = cacheService;
        }
        [HttpGet("api/")]
        public async Task<IActionResult> Get()
        {
            string objectName = "dibobjects";
            DIBObjects dibobjects = _cache.Get<DIBObjects>(objectName);
            if (dibobjects == null ? true : (DateTime.Now - dibobjects.date).Minutes>10)
            {
                var p = new
                {
                    sesssion_id = "apitest",//_usrsvc.CurrentUser.session_id,
                };
                dibobjects = await _repo.ExecDIBObjects("[dbo].[GetDibObjects]", p);
                if (dibobjects==null)
                {
                    return BadRequest("Objects missing!");
                }
                _cache.Set<DIBObjects>(objectName, dibobjects);
            }

            var result = new
            {
                dibobjects.Suppliers,
                dibobjects.TopicTypes,
                dibobjects.Categories,
                dibobjects.Databases,
                dibobjects.TagTypes,
                dibobjects.TopicNameTypes,
                dibobjects.DateTypes,
                dibobjects.ResourceTypes,
                dibobjects.accountingTypes,
                dibobjects.accountingCodes,
                dibobjects.accountingTaxes
            };
            return Ok(result);
        }
    }
}