using DIBAdminAPI.Helpers.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Data.Entities
{
    public class TopicDataAPI
    {
        public string id { get; set; }
        public string language { get; set; }
        public bool publish { get; set; }
        public string supplierId { get; set; }
        public string topictypeId { get; set; }
        public string shortDescription { get; set; }
        public bool deleted { get; set; }
    }
    public class TopicDetail
    {
        public string topicId { get; set; }
        public string language { get; set; }
        public bool publish { get; set; }
        public string supplierId { get; set; }
        public string topictypeId { get; set; }
        public string shortDescription { get; set; }
        public string description { get; set; }
        public bool deleted { get; set; }
        public string transactionId { get; set; }
        public IEnumerable<Database> Databases { get; set; }
        public IEnumerable<TopicName> TopicNames { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public IEnumerable<Dates> Dates { get; set; }
        public Dictionary<string, Related> related { get; set; }
        public IEnumerable<ResourceDocuments> Resources { get; set; }

    }
    public class ObjectsApi
    {
        public List<string> objectsList { get; set; }
        public Dictionary<string, ObjectApi> objects { get; set; }
        public ObjectsApi() { }
        public ObjectsApi(IEnumerable<Database> databases)
        {
            if (databases.Select(p=>p.id).FirstOrDefault() == null)
            {
                objectsList = new List<string>();
                return;
            }
            objectsList = databases
                        .OrderBy(p => p.name)
                        .Select(p => p.id.ToLower()).ToList();
            objects = databases
                 .ToDictionary(
                     p => p.id.ToLower(),
                     p => new ObjectApi
                     {
                         type = "database",
                         id = p.id.ToLower(),
                         transactionId = p.transactionId,
                         data = new DatabaseAPI {
                             id  = p.id.ToLower(),
                             name = p.name
                         }
                     }
                 );
        }
        public ObjectsApi(IEnumerable<TopicName> topicNames)
        {
            if (topicNames.Select(p => p.id).FirstOrDefault() == null)
            {
                objectsList = new List<string>();
                return;
            }
            objectsList = topicNames
                        .OrderBy(p => p.name)
                        .Select(p => p.id.ToLower()).ToList();
            objects = topicNames
                 .ToDictionary(
                     p => p.id.ToLower(),
                     p => new ObjectApi
                     {
                         type = "name",
                         id = p.id.ToLower(),
                         transactionId = p.transactionId,
                         data = new TopicNameAPI {
                             id = p.id.ToLower(),
                             name = p.name ,
                             nametypeId = p.nametypeId.ToLower(),
                             isDefault =  p.isdefault,
                         }
                     }
                 );
        }
        public ObjectsApi(IEnumerable<Tag> tags)
        {
            if (tags.Select(p => p.id).FirstOrDefault() == null)
            {
                objectsList = new List<string>();
                return;
            }
                
            objectsList = tags
                    .OrderBy(p => p.name)
                    .Select(p => p.id.ToLower())
                    .ToList();
            objects = tags
                .ToDictionary(
                    p => p.id.ToLower(),
                    p => new ObjectApi
                    {
                        type = "tag",
                        id = p.id.ToLower(),
                        transactionId = p.transactionId,
                        data = new TagAPI
                        {
                            id = p.id.ToLower(),
                            name = p.name ,
                            tagtypeId = p.tagtypeId.ToLower(),
                            language= p.language 
                        }
                    }
                );
        }
        public ObjectsApi(IEnumerable<Dates> dates)
        {
            if (dates.Select(p => p.id).FirstOrDefault() == null)
            {
                objectsList = new List<string>();
                return;
            }
            objectsList = dates
                        .OrderByDescending(p => p.date)
                        .Select(p => p.id.ToString().ToLower())
                        .ToList();
            objects = dates
                .ToDictionary(
                    p => p.id.ToString().ToLower(),
                    p => new ObjectApi
                    {
                        type = "date",
                        id = p.id.ToLower(),
                        transactionId = p.transactionId,
                        data = new DatesAPI
                        {
                            id = p.id.ToLower(),
                            date = p.date,
                            datetypeId = p.datetypeId.ToLower(),
                        }
                    }
                );
        }
        public ObjectsApi(Dictionary<string, Related> related)
        {
            if (related == null)
            {
                objectsList = new List<string>();
                return;
            }
            if (related.Select(p => p.Key).FirstOrDefault() == null) return;
            objectsList = related
                .OrderBy(p => p.Value.topictypeId).ThenBy(p => p.Value.idx)
                .Select(p => p.Key.ToString().ToLower())
                .ToList();
            objects = related
                .ToDictionary(
                    p => p.Key.ToLower(),
                    p => new ObjectApi
                    {
                        type = "related",
                        id = p.Key,
                        transactionId = p.Value.transactionId,
                        data = new RelatedAPI
                        {
                            id = p.Value.id.ToLower(),
                            name  = p.Value.name,
                            subname = p.Value.subname,
                            topictypeId = p.Value.topictypeId.ToLower(),
                            dataResourceId = p.Value.dataResourceId.ToLower(),
                            dataId = p.Value.dataId == null ? null : p.Value.dataId.ToLower()
                            
                        }
                    }
                );
        }
        public ObjectsApi(IEnumerable<ResourceDocuments> resources)
        {
            if (resources.Select(p => p.id).FirstOrDefault() == null)
            {
                objectsList = new List<string>();
                return;
            }
            objectsList = resources
                        .Select(p =>p.id.ToString().ToLower())
                        .ToList();
            objects = resources
                .ToDictionary(
                    p => p.id.ToString().ToLower(),
                    p => new ObjectApi
                    {
                        type = "resource",
                        id = p.id.ToString().ToLower(),
                        transactionId = p.transactionId,
                        data = p
                    }
            );
        }
        public ObjectsApi(IEnumerable<TopicBase> topicBases, IEnumerable<TopicSubElement> topicSubElements)
        {
            if (topicSubElements == null) return;
            if (topicBases.Select(p => p.resourceId).FirstOrDefault() == null || topicSubElements.Select(p => p.dataResourceId).FirstOrDefault() == null) return;
            objects = (
                from r in topicBases
                join s in topicSubElements
                on r.resourceId.ToLower() equals s.dataResourceId.ToLower()
                where (s.itemId == null ? "" : s.itemId.Trim()) != ""
                orderby r.topictypeId, r.name, s.idx
                select new { r, s }
            )
            .ToDictionary(
                p => p.s.id.ToLower(),
                p => new ObjectApi
                {
                    type = "related",
                    id = p.s.id.ToString().ToLower(),
                    transactionId = p.s.transactionId,
                    data = new Related
                    {
                        id = p.s.id.ToLower(),
                        name = p.r.name,
                        subname = p.s.name,
                        topictypeId = p.r.topictypeId.ToLower(),
                        dataResourceId = p.r.resourceId.ToLower(),
                        dataId = p.s.dataId == null ? null : p.s.dataId.ToLower(),
                        idx = p.s.idx,
                        transactionId = p.s.transactionId
                    }
                }
                
            );
        }
    }
        
    public class ObjectApi
    {
        public string type { get; set; }
        public string id { get; set; }
        public string transactionId { get; set; }
        public bool? selected { get; set; }
        //public Dictionary<string, string> data { get; set; }
        public dynamic data { get; set; }
        public List<string> children { get; set; }
        //private List<Dictionary<string, ObjectApi>> objects { get; set; }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(ObjectApi other)
        {
            if (other == null) return false;

            if ((other.type ?? "null") != (type ?? "null")) return false;
            if ((other.id ?? "null") != (id ?? "null")) return false;
            if ((other.transactionId ?? "null") != (transactionId ?? "null")) return false;
            if ((other.selected ?? false) != (selected ?? false)) return false;
            switch (type)
            {
                case "dib-x-section":
                    {
                        if ((other.data.name ?? "") != (data.name ?? "")) return false;
                        if ((other.data.autoCount ?? "") != (data.autoCount ?? "")) return false;
                        if ((other.data.optional ?? "") != (data.optional ?? "")) return false;
                        if ((other.data.varId ?? "") != (data.varId ?? "")) return false;
                        if ((other.data.varvalue ?? "") != (data.varvalue ?? "")) return false;
                        if ((other.data.dataType ?? "") != (data.dataType ?? "")) return false;
                    }
                    break;
                case "pointer-x-var":
                    {
                        if ((other.data.varId ?? "") != (data.varId ?? "")) return false;
                    }
                    break;
                case "dib-x-optional":
                    {
                        if ((other.data.varId ?? "") != (data.varId ?? "")) return false;
                        if ((other.data.varvalue ??"") != (data.varvalue ?? "")) return false;
                        if ((other.data.optionKey ?? "") != (data.optionKey ?? "")) return false;
                    }
                    break;
                case "dib-x-list":
                    {
                        if ((other.data.ofType ?? "") != (data.ofType ?? "")) return false;
                        if ((other.data.heading ?? "") != (data.heading ?? "")) return false;
                        if ((other.data.varId ?? "") != (data.varId ?? "")) return false;
                        if ((other.data.defaultCounter ?? "") != (data.defaultCounter ?? "")) return false;
                    }
                    break;
                case "dib-x-letterhead":
                    {
                        if ((other.data.heading ?? "") != (data.heading ?? "")) return false;
                    }
                    break;
                case "dib-x-alternatives":
                    {
                        if ((other.data.heading ?? "") != (data.heading ?? "")) return false;
                    }
                    break;
                case "dib-x-alternative":
                    {
                        if ((other.data.heading ?? "") != (data.heading ?? "")) return false;
                        //if ((other.data.varId ?? "") != (data.varId ?? "")) return false;
                        if ((other.data.varValue ?? "") != (data.varValue ?? "")) return false;
                    }
                    break;
                case "dib-x-comment":
                    {
                        if ((other.data.heading ?? "") != (data.heading ?? "")) return false;
                    }
                    break;
                default:
                    return false; ;

            }
            string leftChild = other.children == null ? "" : other.children.Select(p => p).StringConcatenate(";");
            string rightChild = children == null ? "" : children.Select(p => p).StringConcatenate(";");
            if (leftChild != rightChild) return false;
            return true;
        }

    }
    public class TagsAPI
    {
        public List<string> tags { get; set; }
        public Dictionary<string, ObjectApi> objects = new Dictionary<string, ObjectApi>();
        public TagsAPI(IEnumerable<Tag> Tags)
        {
            ObjectsApi objectsApi = new ObjectsApi(Tags);
            tags = objectsApi.objectsList;
            objects.AddRange(objectsApi.objects);
        }
    }
    public class RelatedsAPI
    {
        public List<string> related { get; set; }
        public Dictionary<string, ObjectApi> objects = new Dictionary<string, ObjectApi>();
        public RelatedsAPI(RelatedResources Related)
        {
            ObjectsApi objectsApi = new ObjectsApi(Related.related);
            related = objectsApi.objectsList;
            objects.AddRange(objectsApi.objects);
        }
    }
    public class TopicRoot
    {
       public string id { get; set; }   
    }
    public class TopicPartsAPI
    {
        public Dictionary<string, Dictionary<string,List<string>>> elementdata { get; set; }
        public Dictionary<string, JsonElement> elements { get; set; }
        public Dictionary<string, ObjectApi> objects { get; set; }
        public List<string> root { get; set; }
        public List<string> viewroot { get; set; }
        public List<string> showroot { get; set; }
        public List<string> genroot { get; set; }
    }
    public class TopicDetailAPI
    {
        public Dictionary<string, ObjectApi> objects = new Dictionary<string, ObjectApi>();
        public List<string> databases { get; set; }
        public List<string> names { get; set; }
        public List<string> tags { get; set; }
        public List<string> dates { get; set; }
        public List<string> related { get; set; }
        public List<string> resources { get; set; }
        public string id { get; set; }
        public TopicDetailAPI(TopicDetail topicDetail)
        {
            ObjectApi topicObject;
            if (topicDetail.topicId != null)
            {
                id = topicDetail.topicId.ToString().ToLower();

                topicObject = new ObjectApi
                {
                    type = "topicdata",
                    id = topicDetail.topicId.ToString().ToLower(),
                    transactionId = topicDetail.transactionId,
                    data = new TopicDataAPI
                    {
                        id = topicDetail.topicId.ToString().ToLower(),
                        language = topicDetail.language,
                        publish = topicDetail.publish,
                        supplierId = topicDetail.supplierId,
                        topictypeId = topicDetail.topictypeId,
                        shortDescription = topicDetail.shortDescription,
                        deleted = topicDetail.deleted
                    }
                };
                objects.Add(topicDetail.topicId.ToString().ToLower(), topicObject);
            }



            ObjectsApi objectsApi = new ObjectsApi(topicDetail.Databases);
            databases = objectsApi.objectsList;
            objects.AddRange(objectsApi.objects);

            objectsApi = new ObjectsApi(topicDetail.TopicNames);
            names = objectsApi.objectsList;
            objects.AddRange(objectsApi.objects);


            objectsApi = new ObjectsApi(topicDetail.Tags);
            tags = objectsApi.objectsList;
            objects.AddRange(objectsApi.objects);


            objectsApi = new ObjectsApi(topicDetail.Dates);
            dates = objectsApi.objectsList;
            objects.AddRange(objectsApi.objects);

            objectsApi = new ObjectsApi(topicDetail.related);
            related = objectsApi.objectsList;
            objects.AddRange(objectsApi.objects);

            objectsApi = new ObjectsApi(topicDetail.Resources);
            resources = objectsApi.objectsList;
            objects.AddRange(objectsApi.objects);

        }
    }
    public class RelatedResources
    {
        public Dictionary<string, Related> related { get; set; }
        public RelatedResources(IEnumerable<TopicBase> Related, IEnumerable<TopicSubElement> SubLink)
        {
            if (Related.Select(p=>p.resourceId).FirstOrDefault()==null || SubLink.Select(p=>p.dataResourceId).FirstOrDefault()==null) return;
            related = (
                from r in Related
                join s in SubLink
                on r.resourceId.ToLower() equals s.dataResourceId.ToLower()
                orderby r.topictypeId, r.name, s.idx
                select new { r, s }
            )
            .ToDictionary(
                p => p.s.id.ToLower(),
                p => new Related
                {
                    id = p.s.id.ToLower(),
                    name = p.r.name,
                    subname = p.s.name,
                    topictypeId = p.r.topictypeId.ToLower(),
                    dataResourceId = p.r.resourceId.ToLower(),
                    dataId = p.s.dataId == null ? null : p.s.dataId.ToLower(),
                    idx = p.s.idx,
                    transactionId = p.s.transactionId
                }
            );

            
        }
    }
    public class RelatedAPI
    {
        public string id { get; set; }
        public string name { get; set; }
        public string subname { get; set; }
        public string topictypeId { get; set; }
        public string dataResourceId { get; set; }
        public string dataId { get; set; }
        public List<string> childeren { get; set; }
    }
    public class Related
    {
        public string id { get; set; }
        public string name { get; set; }
        public string subname { get; set; }
        public string topictypeId { get; set; }
        public string dataResourceId { get; set; }
        public string dataId { get; set; }
        public int? idx { get; set; }
        public string transactionId { get; set; }
    }
    public class TopicSubElement
    {
        public string id { get; set; }
        public string name { get; set; }
        public string dataResourceId { get; set; }
        public string dataId { get; set; }
        public int idx { get; set; }
        public string transactionId { get; set; }
        public string itemId { get; set; }
    }
    
    public class TopicBase
    {
        public string topicId { get; set; }
        public string name { get; set; }
        public string language { get; set; }
        public bool publish { get; set; }
        public string supplierId { get; set; }
        public string topictypeId { get; set; }
        public string shortDescription { get; set; }
        public string resourceId { get; set; }
        public bool deleted { get; set; }
        public int? aSort { get; set; }
        public int? tSort { get; set; }
        //public IEnumerable<TopicSubElement> rel { get; set; }
    }
    public class TopicDetails
    {
        public IEnumerable<Topic> topics { get; set; }
        public IEnumerable<TopicNames> topicNames { get; set; }
        public IEnumerable<TopicDatabase> topicDatabases { get; set; } 
        public IEnumerable<Resources> topicResources { get; set; }
    }
    public class Topic
    {
        public string topicId { get; set; }
        public string language { get; set; }
        public bool publish { get; set; }
        public string supplierId { get; set; }
        public string topictypeId { get; set; }
        public string shortDescription { get; set; }
        public string description { get; set; }
    }
    public class TopicNames
    {
        public string topicId { get; set; }
        public string topinameId { get; set; }
        public string name { get; set; }
        public bool isdefault { get; set; }
        public int nametypeId { get; set; }
    }
    public class TopicDatabase
    {
        public string topicId { get; set; }
        public string databaseId { get; set; }
    }
    public class TopicCreate
    {
        public string name { get; set; }
        public int ignoreduplicate { get; set; }
    }
}
