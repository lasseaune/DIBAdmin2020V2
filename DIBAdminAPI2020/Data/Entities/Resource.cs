using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using DIBAdminAPI.Helpers.Extentions;

namespace DIBAdminAPI.Data.Entities
{
    public class ResourceNavigation
    {
        public Guid resourceId { get; set; }
        public string segmentId { get; set; }
        public string Id { get; set; }
        public string parentId { get; set; }
    }
    public class ResourceHTML5Element
    {
        public IEnumerable<AccountLine> AccountLines { get; set; }
        public IEnumerable<TaxLine> TaxLines { get; set; }
    }
    public class ResourceHTML5
    {
        public string topicId { get; set; }
        public string resourceId { get; set; }
        public string segmentId { get; set; }
        public string Name { get; set; }
        public int Accsess { get; set; }
        public int ResourceTypeId { get; set; }
        public XElement ResourceMap { get; set; }
        public XElement Document { get; set; }
        public IEnumerable<LinkData> Links { get; set; }
        public IEnumerable<string> Related { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<AccountLine> AccountLines { get; set; }
        public IEnumerable<TaxLine> TaxLines { get; set; }
        public XElement TriggerData { get; set; }
        public XElement Collections { get; set; }
        public XElement XObjects { get; set; }
        public XElement DgVariables { get; set; }

    }
    public class ResourceDataDocument
    {
        public string topicId { get; set; }
        public string resourceId { get; set; }
        public string segmentId { get; set; }
        public string Name { get; set; }
        public int Accsess { get; set; }
        public int ResourceTypeId { get; set; }
        public XElement ResourceMap { get; set; }
        public XElement Document { get; set; }
        public XElement Variables { get; set; }
        public XElement XObjects { get; set; }
        public XElement LinkData { get; set; }
        public XElement DGVariables { get; set; }
        public XElement TriggerData { get; set; }
        public XElement Collections { get; set; }
        public XElement Accounting { get; set; }
        public XElement Related { get; set; }
        public XElement Tags { get; set; }
    }

    public class Resources
    {
        public Guid topic_id { get; set; }
        public Guid resource_id { get; set; }
        public int resource_type_id { get; set; }
        public DateTime updatedate { get; set; }
        public string username { get; set; }
        public string language { get; set; }
    }
    public class Resource
    {
        public Guid resource_id { get; set; }
        public int resource_type_id { get; set; }
        public DateTime updatedate { get; set; }
        public string username { get; set; }
        public string language { get; set; }
        public string transactionId { get; set; }
    }

    public class ResourceType
    {
        public int resource_type_id { get; set; }
        public string name { get; set; }
        public string data_type { get; set; }
        public int storage_id { get; set; }

    }
    //public class ResourceData
    //{
    //    public Dictionary<string, AccountingSection> accounting { get; set; }
    //}
}

