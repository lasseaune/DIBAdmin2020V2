using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DIBAdminAPI.Data;
using DIBAdminAPI.Data.Entities;
using DIBAdminAPI.Services;
using DIBAdminAPI.Helpers.Extentions;
using System.IO;
using System.Xml.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DIBAdminAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IRepository _repo;
        private readonly ITempStorage _tempstore;
        private readonly ICacheService _cache;
        public DocumentController(IRepository repository, ITempStorage tempStorage, ICacheService cacheService)
        {
            _repo = repository;
            _tempstore = tempStorage;
            _cache = cacheService;

        }
        [HttpGet("{objectname}", Name = "GetResource")]
        public async Task<IActionResult> GetResource(
                string objectname,
                [FromQuery] string resourceId,
                [FromQuery] string Id
            )
        {
            if (objectname == "tags")
            {
                IEnumerable<Tag> tags = await _repo.GetTags(new { resourceId, Id });
                TagsAPI tagsAPI = new TagsAPI(tags);
                return Ok(tagsAPI);
            }
            else if (objectname == "related")
            {
                RelatedResources related = await _repo.GetRelated(new { resourceId, Id });
                RelatedsAPI relatedAPI = new RelatedsAPI(related);
                return Ok(relatedAPI);
            }
            return BadRequest();
        }
        [HttpGet]
        public async Task<IActionResult> Get(
                [FromQuery] string resourceId,
                [FromQuery] string segmentId,
                [FromQuery] string Id,
                [FromQuery] string collectionId,
                [FromQuery] string Search,
                [FromQuery] string Version = "0"
        )
        {

            if ((segmentId??"").Trim().ToLower() =="diblink")
            {

            }
            
            var p = new
            {
                session_id = "apitest",//_usrsvc.CurrentUser.session_id,
                topic_id = resourceId,
                resource_id = resourceId,
                segment_id = segmentId ?? "",
                collection_id = collectionId ?? "",
                version = Version

            };



            string rid = "rid=" + resourceId + ";sid=" + segmentId + ";_document";
            DocumentContainer result = null;
            result = _cache.Get<DocumentContainer>(rid);

            if ((Id == null ? "" : Id) == "" && result != null)
            {
                    return Ok(result);
            }

            ResourceHTML5 data = await _repo.GetHTML5("[dbo].[GetResourceHTML]", p, null);

            if (data==null)
            { 
                    return BadRequest("Document missing!");
            }

            if ((Id == null ? "" : Id) == "" && result != null)
            {
                
            }

            //if (data.Name.LocalName == "package" && (string)data.Attributes("type").FirstOrDefault() == "8")
            //{
            //    //ChecklistJson result = new ChecklistJson(data);
            //    return BadRequest("Document missing!");
            //}
            //else if (data.Name.LocalName == "packages")
            //{
            //    string objectName = "dibobjects";
            //    IEnumerable<AccountingType> accountingType = _cache.Get<DIBObjects>(objectName).accountingTypes;
            //    IEnumerable<AccountingCode> accountingCodes = _cache.Get<DIBObjects>(objectName).accountingCodes;
            //    IEnumerable<AccountingTax> accountingTax = _cache.Get<DIBObjects>(objectName).accountingTaxes;
            //    DocumentPartsContainer dpc = new DocumentPartsContainer(data, resourceId, segmentId, Id, accountingType, accountingCodes, accountingTax);
            //    return Ok(dpc);
            //}
            //else
            //{
            //string objectName = "dibobjects";

            //result = new DocumentContainer(data, _cache.Get<DIBObjects>(objectName));
            result = new DocumentContainer(data);
            if ((Id == null ? "" : Id) == "")
                {
                    _cache.Set<DocumentContainer>(rid, result);
                }
                return Ok(result);
            //}
            
        }
        [HttpPost("create")]
        public IActionResult Create([FromBody]JsonDocumentCreate jdc)
        {
            string action = jdc.action;
            string name = jdc.name;
            XElement document = null;
            string id = null;
            switch (action.Trim().ToLower())
            {
                case "new7":
                    {
                        id = Guid.NewGuid().ToString();

                        document =
                            new XElement("document",
                                new XElement("section",
                                    new XAttribute("id", id),
                                    new XElement("h1",
                                        new XAttribute("id", Guid.NewGuid().ToString()),
                                        new XText("Overskrift 1")
                                    ),
                                    new XElement("p",
                                        new XAttribute("id", Guid.NewGuid().ToString()),
                                        new XText("Første setning....")
                                    )
                                )
                            );
                        
                    }
                    break;
                default:
                    return BadRequest("Kunne ikke opprette dokument til");
            }

            string resourceId = Guid.NewGuid().ToString();
            DocumentContainer dc = new DocumentContainer(name, document, resourceId);
            string segmentId = "";
            string rid = "rid=" + resourceId + ";sid=" + segmentId + ";_document";

            _cache.Set<DocumentContainer>(rid + "_document", dc);
            return Ok(dc);
        }
        
        [HttpGet("s")]
        public async Task<IActionResult> Search(
                [FromQuery] string search
        )
        {
            string objectName = "dibobjects";
            DIBObjects dibobjects = _cache.Get<DIBObjects>(objectName);

            List<string> searchlist = Regex.Replace(Regex.Replace(search.ToLower(), @"[^a-zæøåA-ZÆØÅ0-9\-§]+", " "), @"\s+", " ").Trim().Split(' ').GroupBy(p=>p).Select(p=>p.Key).ToList();
            int i = 1;
            string test = "(" + searchlist.Select(p => "(?<i" + i++.ToString() + ">(" + p + "))").StringConcatenate("|") + ")+";
            Regex rx = new Regex(test);
            IEnumerable<TopicBase> topics = null;

            await Task.Factory.StartNew(delegate
            {
                topics =
                from n in dibobjects.topicNames//.Where(p => p.name.ToLower().IndexOf(search.ToLower()) > 0)
                where n.name.ToLower().Contains(search.ToLower()) || n.topicId.ToString().ToLower() == search || n.name.ToLower().SearchText(rx, searchlist.Count())
                group n by n.topicId into q
                join t in dibobjects.topics
                on q.Key equals t.topicId
                join nn in dibobjects.topicNames.Where(p => p.isdefault)
                on q.Key equals nn.topicId
                orderby q.Min(p=>p.name.Length) 
                select new TopicBase
                {
                    topicId = t.topicId,
                    topictypeId = t.topictypeId,
                    name = nn.name,
                    language = t.language,
                    publish = t.publish,
                    shortDescription = t.shortDescription,
                    supplierId = t.supplierId

                };
                
            });

            return Ok(topics);
        }
    }
}