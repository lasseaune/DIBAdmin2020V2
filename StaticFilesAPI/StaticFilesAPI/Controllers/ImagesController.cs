using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StaticFilesAPI.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace StaticFilesAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly ICacheService _cache;
        public ImagesController(ICacheService cacheService)
        {
            _cache = cacheService;
        }
        // GET: api/<ImagesController>
        [HttpGet]
        public FileStream Get(string name)
        {
            return _cache.Get<FileStream>(name);
        }
    }
}
