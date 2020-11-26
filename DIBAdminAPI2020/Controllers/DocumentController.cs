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
using System.Text.Encodings;
using System.IO.Compression;
using System.Net;

namespace DIBAdminAPI.Controllers
{
    [Route("/[controller]")]
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
        [HttpGet("searchvariable")]
        public async Task<IActionResult> GetVariable(
                [FromQuery] string resourceId,
                [FromQuery] string segmentId,
                [FromQuery] string search
            )
        {
            search = WebUtility.HtmlDecode(search);
            segmentId = segmentId ?? "";
            string rid = "rid=" + resourceId + ";sid=" + segmentId + ";_document";
            DocumentContainer result = null;
            result = _cache.Get<DocumentContainer>(rid);
            int n = 1;
            string sRegex = search.Split(' ').Select(p => @"(?<v" + (n++).ToString() + ">(" + p + @"))").StringConcatenate();
            List<string> root = new List<string>();
            Dictionary<string, ObjectApi> objects = new Dictionary<string, ObjectApi>();
            await Task.Factory.StartNew(delegate
            {
                //objects = result.objects.Where(p => p.Value.type == "dib-x-var" ? ((string)p.Value.data.name).StringIsMatch(search):false).ToDictionary(p=>p.Key, p=>p.Value);
                objects = result.objects.Where(p => p.Value.type == "dib-x-var").Select(p=>new { rank = ((string)p.Value.data.name).StringIsMatchValue(search), x=p }).Where(p=>p.rank > 0).OrderByDescending(p=>p.rank).ToDictionary(p => p.x.Key, p => p.x.Value);
            });

            TopicPartsAPI tp = new TopicPartsAPI
            {
                root = objects.Select(p => p.Key).ToList(),
                objects = objects
            };

            return Ok(tp);
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
                [FromQuery] IEnumerable<string> labelId

        )
        {

            if ((segmentId ?? "").Trim().ToLower() == "diblink")
            {
                DocumentParts docParts = await _repo.GetDocumentPartDiblink(resourceId, segmentId, Id);
                if (docParts == null)
                {
                    return BadRequest("Missing part");
                }
                return Ok(docParts);
            }

            if ((Id ?? "")!="")
            {
                DocumentParts docParts = await _repo.GetDocumentPart(resourceId, Id);
                if (docParts == null)
                {
                    return BadRequest("Missing part");
                }
                return Ok(docParts);
            }

            var p = new
            {
                session_id = "apitest",//_usrsvc.CurrentUser.session_id,
                id = resourceId,
                segment_id = segmentId ?? "",
                collection_id = collectionId ?? ""
            };



            string rid = "rid=" + resourceId + ";sid=" + segmentId + ";_document";
            DocumentContainer result = null;
            result = _cache.Get<DocumentContainer>(rid);

            if ((labelId.Count() > 0) && result != null)
            {
                if (labelId.Where(p=>p==null ? true :("all;reset;null".Split(';').Contains(p.ToLower()))).Count()>0)
                {
                    return Ok(result);
                }
                DocumentContainer dcselect =  new DocumentContainer(result);

                XElement xDocument = dcselect.GetDocumentContainerXML();
                xDocument = xDocument.GetChecklistElements(labelId, dcselect.GetItemData(), dcselect.GetLabels(), dcselect.GetLabelGroups());
                TocJson tocJson = new TocJson(null, xDocument, result.id, "");
                dcselect.tocroot = tocJson.tocroot;
                dcselect.toc = tocJson.toc;

                string document_id = "document;" + result.id + ";" + "";
                xDocument.SetAttributeValueEx("id", document_id);
                dcselect.root = new List<JsonChild>
                {
                    new JsonChild { id = document_id }
                };
                dcselect.elements = new Dictionary<string, JsonElement>
                {
                    {
                        document_id,
                        new JsonElement
                        {
                            name = "div",
                            attributes = new Dictionary<string, string>() {
                                { "class", "doccontainer" },
                                { "id", document_id },
                            },
                            children = xDocument.Elements().Select(p => new JsonChild { id = (string)p.Attributes("id").FirstOrDefault() }).ToList()
                        }
                    }
                };
                dcselect.elements.AddRange(
                    xDocument
                    .Descendants()
                    .Select(p => p)
                    .ToDictionary(p => (string)p.Attributes("id").FirstOrDefault(), p => new JsonElement(p))
                );
                dcselect.genroot = (
                    from e in xDocument.Descendants()
                    join el in dcselect.elements
                        on ((string)e.Attributes("id").FirstOrDefault() ?? "").ToLower() equals el.Key.ToLower()
                    join eld in dcselect.elementdata
                        on el.Key equals eld.Key
                    from v in eld.Value.Where(p => p.Key == "related").SelectMany(p => p.Value)
                    group v by v into g
                    select g.Key
                ).ToList();
                dcselect.eCount = dcselect.elements.Count();
                return Ok(dcselect);

            }
            if ((Id == null ? "" : Id) == "" && result != null)
            {
                    return Ok(result);
            }

            ResourceHTML5 data = await _repo.GetHTML5("[dbo].[GetResourceHTML]", p, null);

            if (data==null)
            { 
                    return BadRequest("Document missing!");
            }

            if (data.Document==null)
            {
                return BadRequest("Document missing!");
            }
            if (data.Document.Name == "encoding")
            {
                XElement encoding = data.Document;
                string filename = (string)encoding.Attributes("filename").FirstOrDefault();
                filename = Regex.Replace(Regex.Replace(filename, @"[^a-zæøåA-ZÆØÅ0-9\s§]", ""), @"\s+", " ");
                string fileextention = (string)encoding.Attributes("fileextention").FirstOrDefault();
                string base64 = encoding.Elements("base64String").Select(p => p.Value).FirstOrDefault();
                byte[] fileBytes = Convert.FromBase64String(base64);

                Stream filedata = new MemoryStream(fileBytes);   //The original data
                Stream unzippedEntryStream = null; ;         //Unzipped data from a file in the archive
                var archive = new ZipArchive(filedata);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    unzippedEntryStream = entry.Open(); // .Open will return a stream
                }

                string fileName = filename + "." + fileextention;
                return File(unzippedEntryStream, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }

            if ((Id == null ? "" : Id) == "" && result != null)
            {
                
            }
            result = new DocumentContainer(data);
            if ((Id == null ? "" : Id) == "")
            {
                _cache.Set<DocumentContainer>(rid, result);
            }
            //result.itemData = null;
            return Ok(result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody]JsonDocumentCreate jdc)
        {
            string transactionId = Guid.NewGuid().ToString();
            
            string op = jdc.op ?? "";
            XElement result = null;
            
            if (op=="create" || op =="")
            {
                
                if (jdc.topicId == null || jdc.resourcetypeId==null || jdc.name == null)
                {

                }
                int resourcetypeId = jdc.resourcetypeId ?? 0;
                XElement document = resourcetypeId.CreateXMLDocument();
                TocJson tocJson = new TocJson(document);
                XElement index = new XElement("index", tocJson.items.Nodes());
                XElement container = new XElement("container2020",
                        index,
                        document
                    );
                var p = new
                {
                    jdc.topicId,
                    jdc.name,
                    resourcetypeId,
                    session_id = "apitest",
                    container,
                    transactionId
                };
            
                result = await _repo.ExecUpdateResult("[dbo].[CreateRecourceDocument]", p);
            }
            else if (op == "createeditable")
            {
                string segmentId = jdc.segmentId ?? "";
                string resourceId = jdc.resourceId;
                string topicId = jdc.topicId;
                if (resourceId == null || topicId == null)
                {
                    return BadRequest("Missing resourceId or topicId");
                }
                string rid = "rid=" + resourceId + ";sid=" + segmentId + ";_document";

                DocumentContainer documentContainer = null;
                documentContainer = _cache.Get<DocumentContainer>(rid);
                if (documentContainer == null) return BadRequest("Documentcontainer is missing");

                XElement document = documentContainer.GetDocumentContainerXML();
                TocJson tocJson = new TocJson(document, resourceId, segmentId);
                XElement index = new XElement("index", tocJson.items.Nodes());
                XElement container = new XElement("container2020", index, document);
                var p = new
                {
                    topicId,
                    segmentId,
                    resourceId,
                    name = "Kladd - " + documentContainer.name,
                    session_id = "apitest",
                    container,
                    transactionId
                };
                result = await _repo.ExecUpdateResult("[dbo].[CreateRecourceDocumentEditable]", p);
            }

            if (result==null)
            {
                return BadRequest();
            }
            if ((string)result.Attributes("value").FirstOrDefault() != "1")
            {
                return BadRequest((string)result.Attributes("message").FirstOrDefault());
            }
            else
            {
                string id = ((string)result.Attributes("id").FirstOrDefault() ?? "").ToLower();
                var t = new
                {
                    jdc.topicId
                };
                IEnumerable<ResourceDocuments> rd = await _repo.GetResourceDocuments(t);
                ObjectsApi oa = new ObjectsApi(rd);
                TopicPartsAPI tp = new TopicPartsAPI
                {
                    root = new List<string> {id},
                    objects = oa.objects
                                .Where(v => v.Value.transactionId == transactionId)
                                .ToDictionary(v => v.Key, v => v.Value)
                };
                return Ok(tp);
            }
            
        }
        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody]JsonDocumentCreate jdc)
        {
            string transactionId = Guid.NewGuid().ToString();
            string topicId = jdc.topicId;
            string resourceId = jdc.resourceId;
            string segmentId = jdc.segmentId??"";

            if (topicId == null || resourceId == null)
            {
                return BadRequest("Missing input data");
            }

            string rid = "rid=" + resourceId + ";sid=" + segmentId + ";_document";
            
            DocumentContainer documentContainer = null;
            documentContainer = _cache.Get<DocumentContainer>(rid);
            if (documentContainer == null) return BadRequest("Documentcontainer is missing");
            if (documentContainer.Edited == false)
            {
                
            }

            XElement document = documentContainer.GetDocumentContainerXML();
            TocJson tocJson = new TocJson(document, resourceId, segmentId);
            XElement index = new XElement("index", tocJson.items.Nodes());
            XElement container = new XElement("container2020", index, document);
            var p = new
            {
                Id = resourceId,
                segmentId,
                session_id = "apitest",//_usrsvc.CurrentUser.session_id,
                container,
                transactionId
            };
            XElement result = await _repo.SaveResourceDocument(p);
            if (result == null)
            {
                return BadRequest();
            }
            if ((string)result.Attributes("value").FirstOrDefault() != "1")
            {
                return BadRequest((string)result.Attributes("message").FirstOrDefault());
            }
            else
            {
                string id = ((string)result.Attributes("id").FirstOrDefault() ?? "").ToLower();
                var t = new
                {
                    topicId
                };
                IEnumerable<ResourceDocuments> rd = await _repo.GetResourceDocuments(t);
                ObjectsApi oa = new ObjectsApi(rd);
                TopicPartsAPI tp = new TopicPartsAPI
                {
                    root = new List<string> { id },
                    objects = oa.objects
                                .Where(v => v.Value.transactionId == transactionId)
                                .ToDictionary(v => v.Key, v => v.Value)
                };
                return Ok(tp);
            }

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
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(string topicId, IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest("Fil må angis!");

                byte[] fileBytes;
                string name = file.FileName;
                string extention = name.Split('.').LastOrDefault();
                name = name.Split('.').FirstOrDefault();
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }
                byte[] compressedBytes;
                

                using (var outStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
                    {
                        var fileInArchive = archive.CreateEntry(file.FileName, CompressionLevel.Optimal);
                        using (var entryStream = fileInArchive.Open())
                        using (var fileToCompressStream = new MemoryStream(fileBytes))
                        {
                            fileToCompressStream.CopyTo(entryStream);
                        }
                    }
                    compressedBytes = outStream.ToArray();
                }

                string base64File  = Convert.ToBase64String(compressedBytes);

                XElement xml = new XElement("encoding",
                    new XAttribute("filename", name),
                    new XAttribute("fileextention", extention),
                    new XElement("base64String", base64File)
                );
                string transactionId = Guid.NewGuid().ToString();
                var p = new
                {
                    topicId,
                    name,
                    extention,
                    xml,
                    session_id = "apitest",//_usrsvc.CurrentUser.session_id,
                    transactionId
                };

                XElement result = await _repo.ExecXElementQuery("[dbo].[SaveFile]", p);
                if (result == null)
                {
                    return BadRequest();
                }
                if ((string)result.Attributes("value").FirstOrDefault() != "1")
                {
                    return BadRequest((string)result.Attributes("message").FirstOrDefault());
                }
                else
                {
                    string id = ((string)result.Attributes("id").FirstOrDefault() ?? "").ToLower();
                    var t = new
                    {
                        topicId
                    };
                    IEnumerable<ResourceDocuments> rd = await _repo.GetResourceDocuments(t);
                    ObjectsApi oa = new ObjectsApi(rd);
                    TopicPartsAPI tp = new TopicPartsAPI
                    {
                        root = new List<string> { id },
                        objects = oa.objects
                                    .Where(v => v.Value.transactionId == transactionId)
                                    .ToDictionary(v => v.Key, v => v.Value)
                    };
                    return Ok(tp);
                }
            }
            catch (Exception e)
            {
                //_logger.LogError("<DocumentsController/AddDocument/(folder_id: {folder_id}, navn: {navn})> Message: {message}", folder_id, file.FileName, e.Message);
                //return BadRequest(e.Message);
                return BadRequest();
                
            }

        }
        [HttpGet("file")]
        public async Task<IActionResult> File([FromQuery] string resourceId)
        {

            try
            {
                var p = new
                {
                    session_id = "apitest",//_usrsvc.CurrentUser.session_id,
                    resourceId
                };

                XElement encoding = await _repo.ExecXElementQuery("[dbo].[Getfile]", p);
                if (encoding == null)
                    return BadRequest("Error in File: encoding is null!");

                encoding = encoding.DescendantsAndSelf("encoding").FirstOrDefault();
                if (encoding == null)
                    return BadRequest("Error in File: encoding is null!");

                string filename = (string)encoding.Attributes("filename").FirstOrDefault();
                filename = Regex.Replace(Regex.Replace(filename, @"[^a-zæøåA-ZÆØÅ0-9\s§]", ""), @"\s+", " ");
                string fileextention = (string)encoding.Attributes("fileextention").FirstOrDefault();
                string base64 = encoding.Elements("base64String").Select(p => p.Value).FirstOrDefault();
                byte[] fileBytes = Convert.FromBase64String(base64);

                Stream data = new MemoryStream(fileBytes);   //The original data
                Stream unzippedEntryStream = null; ;         //Unzipped data from a file in the archive
                var archive = new ZipArchive(data);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    unzippedEntryStream = entry.Open(); // .Open will return a stream
                }

                string fileName = filename + "." + fileextention;
                return File(unzippedEntryStream, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception e)
            {
                //_logger.LogError("<api/file?id={id}> UserSession: {@userSession}, Message = {@ex}'",
                //        id, _usrsvc.CurrentUser, e);
                return BadRequest("Error in file: " + e.Message);
            }
        }
    }
}