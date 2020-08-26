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
        public IEnumerable<Resource> Resources { get; set; }

    }
    public class ObjectsApi
    {
        public List<string> objectsList { get; set; }
        public Dictionary<string, ObjectApi> objects { get; set; }
        public ObjectsApi() { }
        public ObjectsApi(IEnumerable<Database> databases)
        {
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
            objectsList = dates
                        .OrderByDescending(p => p.date)
                        .Select(p => p.id.ToString().ToLower())
                        .ToList();
            objects = dates
                .ToDictionary(
                    p => p.id.ToString(),
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
                            dataId = p.Value.dataId.ToLower()
                            
                        }
                    }
                );
        }
        public ObjectsApi(IEnumerable<Resource> resources, string topic_id)
        {
            objectsList = resources
                        .Select(p => 
                            p.resource_id.ToString().Trim().ToLower()== topic_id.Trim().ToLower() 
                            ? "default;"+ p.resource_id.ToLower() 
                            : p.resource_id.ToLower()
                        )
                        .ToList();
            objects = resources
                .ToDictionary(
                    p => p.resource_id.ToString().Trim().ToLower() == topic_id.Trim().ToLower() ? "default;" + p.resource_id.ToLower() : p.resource_id.ToLower(),
                    p => new ObjectApi
                    {
                        type = "resource",
                        id = p.resource_id.ToString().Trim().ToLower() == topic_id.Trim().ToLower() ? "default;" + p.resource_id.ToString() : p.resource_id.ToString(),
                        transactionId = p.transactionId,
                        data = new Dictionary<string, string>
                        {
                            { "resourceId" , p.resource_id.ToLower() },
                            { "resource_type_id" , p.resource_type_id.ToString().ToLower()},
                            { "language" , p.language},
                            { "lastupdate" , p.updatedate.ToString()},
                            { "default" , p.resource_id.ToString().Trim().ToLower() == topic_id.Trim().ToLower() ? "1":"0"},
                        }
                    }
            );
        }
    }
        
    public class ObjectApi
    {
        public string type { get; set; }
        public dynamic id { get; set; }
        public string transactionId { get; set; }
        //public Dictionary<string, string> data { get; set; }
        public dynamic data { get; set; }
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
        public Dictionary<string, ObjectApi> objects { get; set; }
        public List<string> root { get; set; }
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
        public List<string> root { get; set; }
        public TopicDetailAPI(TopicDetail topicDetail)
        {

            root = new List<string>
            {
                topicDetail.topicId.ToString()
            };

            ObjectApi topicObject = new ObjectApi
            {
                type = "topicdata",
                id = topicDetail.topicId,
                transactionId = topicDetail.transactionId,
                data = new TopicDataAPI
                {
                    id = topicDetail.topicId,
                    language = topicDetail.language,
                    publish = topicDetail.publish,
                    supplierId = topicDetail.supplierId,
                    topictypeId = topicDetail.topictypeId,
                    shortDescription = topicDetail.shortDescription,
                    deleted = topicDetail.deleted
                }
            };

            objects.Add(topicDetail.topicId.ToString(), topicObject);



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

            objectsApi = new ObjectsApi(topicDetail.Resources, topicDetail.topicId.ToString());
            resources = objectsApi.objectsList;
            objects.AddRange(objectsApi.objects);

        }
    }
    public class RelatedResources
    {
        public Dictionary<string, Related> related { get; set; }
        public RelatedResources(IEnumerable<TopicBase> Related, IEnumerable<TopicSubElement> SubLink)
        {
            if (Related == null || SubLink == null) return;
            related = (
                from r in Related
                join s in SubLink
                on r.resourceId.ToLower() equals s.resourceId.ToLower()
                orderby r.topictypeId, r.name, s.idx
                select new { r, s }
            )
            .ToDictionary(
                p => p.s.Id.ToLower(),
                p => new Related
                {
                    id = p.s.Id.ToLower(),
                    name = p.r.name,
                    subname = p.s.Name,
                    topictypeId = p.r.topictypeId.ToLower(),
                    dataResourceId = p.r.resourceId.ToLower(),
                    dataId = p.s.relId.ToLower(),
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
        public int idx { get; set; }
        public string transactionId { get; set; }
    }
    public class TopicSubElement
    {
        public string resourceId { get; set; }
        public string Id { get; set; }
        public string relId { get; set; }
        public string Name { get; set; }
        public int idx { get; set; }
        public string transactionId { get; set; }
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
        public IEnumerable<TopicSubElement> rel { get; set; }
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
