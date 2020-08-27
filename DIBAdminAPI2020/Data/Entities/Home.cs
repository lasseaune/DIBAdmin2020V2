using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Data.Entities
{
    //public class Home
    //{
    //    public IEnumerable<Supplier> Suppliers { get; set; }
    //    public IEnumerable<Topictype> Topictypes { get; set; }
    //    public IEnumerable<Category> Categories { get; set; }
    //    public IEnumerable<Database> Databases { get; set; }
    //    public IEnumerable<Tagtype> Tagtypes { get; set; }
    //    public IEnumerable<TopicNameType> TopicNameTypes { get; set; }
    //    public IEnumerable<DateType> DateTypes { get; set; }
    //    public IEnumerable<ResourceType> ResourceTypes { get; set; }
    //}
    public class DIBObjects
    {
        public DateTime date = DateTime.Now;
        public IEnumerable<Supplier> Suppliers { get; set; }
        public IEnumerable<Topictype> TopicTypes { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Database> Databases { get; set; }
        public IEnumerable<Tagtype> TagTypes { get; set; }
        public IEnumerable<TopicNameType> TopicNameTypes { get; set; }
        public IEnumerable<DateType> DateTypes { get; set; }
        public IEnumerable<ResourceType> ResourceTypes { get; set; }
        public IEnumerable<Topic> topics { get; set; }
        public IEnumerable<TopicNames> topicNames { get; set; }
        public IEnumerable<TopicDatabase> topicDatabases { get; set; }
        //public IEnumerable<Resources> topicResources { get; set; }
        public IEnumerable<AccountingType> accountingTypes { get; set; }
        public IEnumerable<AccountingCode> accountingCodes { get; set; }
        public IEnumerable<AccountingTax> accountingTaxes { get; set; }
    }
}
