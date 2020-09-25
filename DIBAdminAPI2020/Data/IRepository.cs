using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using DIBAdminAPI.Data.Entities;
namespace DIBAdminAPI.Data
{
    public interface IRepository
    {
        Task<RelatedResources> GetRelated(object p, int? timeOut = null);
        Task<IEnumerable<Tag>> GetTags(object p, int? timeOut = null);
        Task<ResourceHTML5> GetHTML5(string QueryName, object p, int? timeOut = null);
        Task<ResourceHTML5Element> GetRecourceElementdata(string QueryName, object p, int? timeOut = null);
        Task<DIBObjects> ExecDIBObjects(string QueryName, object p, int? timeOut = null);
        Task<TopicDetail> ExecTopicDetail(string QueryName, object p, int? timeOut = null);
        Task<IEnumerable<TopicBase>> ExecTopics(string QueryName, object p, int? timeOut = null);
        Task<XElement> ExecRData(string QueryName, object p, int? timeOut = null);
        Task<IEnumerable<ResourceDocuments>> GetResourceDocuments(object p, int? timeOut = null);
        Task<XElement> CreateResourceDocument(object p, int? timeOut = null);
        Task<XElement> SaveResourceDocument(object p, int? timeOut = null);
        Task<XElement> ExecUpdateResult(string QueryName, object p, int? timeOut = null);
        Task<XElement> ExecXElementQuery(string QueryName, object p, int? timeOut = null);
        Task<IEnumerable<DIBLinkData>> GetResourceDibLinkData(object p, int? timeOut = null);
        Task<ResourceNavigation> GetResourceByResourceIdAndId(object p, int? timeOut = null);
        //Task<XElement> ExecDocument(string QueryName, object p, int? timeOut = null);
        //Task<ResourceDataDocument> ExecDocumentResource(string QueryName, object p, int? timeOut = null);
        //Task<IEnumerable<XElement>> ExecQuery(string QueryName, object p, int? timeOut = null);
        //Task<Home> ExecHome(string QueryName, object p, int? timeOut = null);
        //Task<IEnumerable<XElement>> ExecQuery(string QueryName, object p, int? timeOut = null);
        //Task<XElement> GetTopic();
        //Task<TopicDetails> ExecTopicDetails(string QueryName, object p, int? timeOut = null);
        Task<DocumentParts> GetDocumentPartDiblink(string resourceId, string segmentId, string Id);
        Task<DocumentParts> GetDocumentPart(string resourceId, string id);

    }
}
