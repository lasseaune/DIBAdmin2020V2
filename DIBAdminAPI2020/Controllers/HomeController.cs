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
            if (dibobjects == null)
            {
                var p = new
                {
                    sesssion_id = "apitest",//_usrsvc.CurrentUser.session_id,
                };
                dibobjects = await _repo.ExecDIBObjects("dbo.DibObjects", p);
                if (dibobjects==null)
                {
                    return BadRequest("Objects missing!");
                }
                _cache.Set<DIBObjects>(objectName, dibobjects);
            }
            //Home home = await _repo.ExecHome("dbo.Home", null);
            //if (home == null)
            //{
            //    return BadRequest("Document missing!");
            //}

            Dictionary<int, AccountingType> accountingTypes = new Dictionary<int, AccountingType>();
            accountingTypes = dibobjects.accountingTypes
                .OrderBy(p=>p.GuiOrder)
                .ToDictionary(p => p.AccId, p => p);

            Dictionary<int, IEnumerable<AccountingCode>> accountingCode = new Dictionary<int, IEnumerable<AccountingCode>>();
            accountingCode = dibobjects.accountingCodes
                .GroupBy(p => p.Type)
                .ToDictionary(p => p.Key, p => p.OrderBy(l => l.GuiOrder).Select(l => l));

            Dictionary<int, AccountingTax> accountingTaxes = new Dictionary<int, AccountingTax>();
            accountingTaxes = dibobjects.accountingTaxes
                .OrderBy(p => p.TaxId)
                .ToDictionary(p => p.TaxId, p => p);
            var result = new
            {
                dibobjects.Suppliers,
                dibobjects.Topictypes,
                dibobjects.Categories,
                dibobjects.Databases,
                dibobjects.Tagtypes,
                dibobjects.TopicNameTypes,
                dibobjects.DateTypes,
                dibobjects.ResourceTypes,
                //dibobjects.accountingTypes,
                accountingTypes,
                //dibobjects.accountingCodes,
                accountingCode,
                //dibobjects.accountingTaxes
                accountingTaxes
            };
            return Ok(result);
        }
    }
}