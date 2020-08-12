using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Data.Entities
{
    public class TopicDetail
    {
        public Guid topic_id { get; set; }
        public string language { get; set; }
        public int publish { get; set; }
        public int supplier_id { get; set; }
        public int topic_type_id { get; set; }
        public string short_description { get; set; }
        public string description { get; set; }
        public IEnumerable<int> Databases { get; set; }
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
        public ObjectsApi(IEnumerable<TopicName> topicNames)
        {
            objectsList = topicNames.OrderBy(p => p.name).Select(p => p.topic_name_id.ToString()).ToList();
            objects = topicNames
                 .ToDictionary(
                     p => p.topic_name_id.ToString(),
                     p => new ObjectApi
                     {
                         type = "name",
                         id = p.topic_name_id.ToString(),
                         data = new Dictionary<string, string> {
                            { "name", p.name },
                            { "name_type_id",p.topic_name_type_id.ToString() },
                            { "default", p.isdefault ? "1" : "0"  }
                         }
                     }
                 );
        }
        public ObjectsApi(IEnumerable<Tag> tags)
        {
            objectsList = tags.OrderBy(p => p.tag).Select(p => p.tag_id.ToString()).ToList();
            objects = tags
                .ToDictionary(
                    p => p.tag_id.ToString(),
                    p => new ObjectApi
                    {
                        type = "tag",
                        id = p.tag_id.ToString(),
                        data = new Dictionary<string, string>
                        {
                            {"tag", p.tag },
                            {"tag_type_id" , p.tag_type_id.ToString() }
                        }
                    }
                );
        }
        public ObjectsApi(IEnumerable<Dates> dates)
        {
            objectsList = dates.OrderByDescending(p => p.date).Select(p => p.id.ToString()).ToList();
            objects = dates
                .ToDictionary(
                    p => p.id.ToString(),
                    p => new ObjectApi
                    {
                        type = "date",
                        id = p.id.ToString(),
                        data = new Dictionary<string, string>
                        {
                            { "date", p.date.ToString() },
                            { "date_type_id" ,p.date_type_id.ToString() }
                        }
                    }
                );
        }
        public ObjectsApi(Dictionary<string, Related> related)
        {
            objectsList = related.OrderBy(p => p.Value.topic_type_id).ThenBy(p => p.Value.idx).Select(p => p.Key.ToString()).ToList();
            objects = related
                .ToDictionary(
                    p => p.Key.ToString(),
                    p => new ObjectApi
                    {
                        type = "related",
                        id = p.Key.ToString(),
                        data = new Dictionary<string, string>
                        {
                            { "resourceId", p.Value.resourceId },
                            { "name" ,p.Value.name },
                            { "topic_type_id" , p.Value.topic_type_id.ToString()},
                            { "Id" , (p.Value.Id == null ? "" : p.Value.Id)},
                            { "subname" , (p.Value.subname==null ? "" :p.Value.subname) }
                        }
                    }
                );
        }
        public ObjectsApi(IEnumerable<Resource> resources, string topic_id)
        {
            objectsList = resources.Select(p => p.resource_id.ToString().Trim().ToLower()== topic_id.Trim().ToLower() ? "default;"+ p.resource_id.ToString() : p.resource_id.ToString()).ToList();
            objects = resources
                .ToDictionary(
                    p => p.resource_id.ToString().Trim().ToLower() == topic_id.Trim().ToLower() ? "default;" + p.resource_id.ToString() : p.resource_id.ToString(),
                    p => new ObjectApi
                    {
                        type = "resource",
                        id = p.resource_id.ToString().Trim().ToLower() == topic_id.Trim().ToLower() ? "default;" + p.resource_id.ToString() : p.resource_id.ToString(),
                        data = new Dictionary<string, string>
                        {
                            { "resourceId" , p.resource_id.ToString() },
                            { "resource_type_id" , p.resource_type_id.ToString()},
                            { "language" , p.language},
                            { "lastupdate" , p.updatedate.ToString()},
                            { "default" , p.resource_id.ToString().Trim().ToLower() == topic_id.Trim().ToLower() ? "1":"0"}
                        }
                    }
            );
        }
    }
        
    public class ObjectApi
    {
        public string type { get; set; }
        public string id { get; set; }
        public Dictionary<string, string> data { get; set; }
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
    public class RelatedAPI
    {
        public List<string> related { get; set; }
        public Dictionary<string, ObjectApi> objects = new Dictionary<string, ObjectApi>();
        public RelatedAPI(RelatedResources Related)
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
    public class TopicDetailAPI
    {
        public Dictionary<string, ObjectApi> objects = new Dictionary<string, ObjectApi>();
        //public Guid topic_id { get; set; }
        //public string language { get; set; }
        //public bool publish { get; set; }
        //public int supplier_id { get; set; }
        //public int topic_type_id { get; set; }
        //public string short_description { get; set; }
        //public string description { get; set; }
        public IEnumerable<int> databases { get; set; }
        public List<string> names { get; set; }
        public List<string> tags { get; set; }
        public List<string> dates { get; set; }
        public List<string> related { get; set; }
        public List<string> resources { get; set; }
        public TopicRoot root { get; set; }
        public TopicDetailAPI(TopicDetail topicDetail)
        {

            root = new TopicRoot
            {
                id = topicDetail.topic_id.ToString()
            };

            ObjectApi topicObject = new ObjectApi
            {
                type = "topicdata",
                id = topicDetail.topic_id.ToString(),
                data = new Dictionary<string, string>{
                    { "language",topicDetail.language },
                    { "publish", topicDetail.publish.ToString() },
                    { "supplier_id",topicDetail.supplier_id.ToString() },
                    { "topic_type_id", topicDetail.topic_type_id.ToString() },
                    { "short_description",topicDetail.short_description },
                    { "description", topicDetail.description }
                }
            };

            objects.Add(topicDetail.topic_id.ToString(), topicObject);
            databases = topicDetail.Databases;

            ObjectsApi objectsApi = new ObjectsApi(topicDetail.TopicNames);
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

            objectsApi = new ObjectsApi(topicDetail.Resources, topicDetail.topic_id.ToString());
            resources = objectsApi.objectsList;
            objects.AddRange(objectsApi.objects);

        }
    }
    public class RelatedResources
    {
        public Dictionary<string, Related> related { get; set; }
        public RelatedResources(IEnumerable<TopicBase> Related, IEnumerable<TopicSubElement> SubLink)
        {
            related = (
                from r in Related
                join s in SubLink
                on r.resourceId equals s.resourceId
                orderby r.topic_type_id, r.name, s.idx
                select new { r, s }
            )
            .ToDictionary(p => p.s.Id.ToString(), p => new Related
            {
                resourceId = p.r.resourceId.ToString(),
                name = p.r.name,
                topic_type_id = p.r.topic_type_id,
                subname = p.s.Name,
                Id = p.s.relId,
                idx = p.s.idx
            });

            
        }
    }
    public class Related
    {
        public string resourceId { get; set; }
        public string name { get; set; }
        public int topic_type_id { get; set; }
        public string subname { get; set; }
        public string Id { get; set; }
        public int idx { get; set; }
    }
    public class TopicSubElement
    {
        public Guid resourceId { get; set; }
        public Guid Id { get; set; }
        public string relId { get; set; }
        public string Name { get; set; }
        public int idx { get; set; }
    }
    
    public class TopicBase
    {
        public Guid topic_id { get; set; }
        public string name { get; set; }
        public string language { get; set; }
        public bool publish { get; set; }
        public int supplier_id { get; set; }
        public int topic_type_id { get; set; }
        public string short_description { get; set; }
        public Guid resourceId { get; set; }
        public int deleted { get; set; }
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
        public Guid topic_id { get; set; }
        public string language { get; set; }
        public bool publish { get; set; }
        public int supplier_id { get; set; }
        public int topic_type_id { get; set; }
        public string short_description { get; set; }
        public string description { get; set; }
    }
    public class TopicNames
    {
        public Guid topic_id { get; set; }
        public Guid topic_name_id { get; set; }
        public string name { get; set; }
        public bool isdefault { get; set; }
        public int topic_name_type_id { get; set; }
    }
    public class TopicDatabase
    {
        public Guid topic_id { get; set; }
        public int database_id { get; set; }
    }
    public class TopicCreate
    {
        public string name { get; set; }
        public int ignoreduplicate { get; set; }
    }
}
