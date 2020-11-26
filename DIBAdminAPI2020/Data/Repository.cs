using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using DIBAdminAPI.Data.Entities;
using DIBAdminAPI.Services;
using System.Linq;

namespace DIBAdminAPI.Data
{
    public class Repository : IRepository
    {
        private readonly ILogger<Repository> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connstring;
        


        public Repository(IConfiguration configuration, ILogger<Repository> logger)
        {
            _configuration = configuration;
            _connstring = "Server=rocky;Database=TM2020;Persist Security Info=True;User ID=dibapp;Password=lau610523;MultipleActiveResultSets=true;";
            _logger = logger;
        }

        private IDbConnection dbConnection
        {
            get
            {
                return new SqlConnection(_connstring);
            }
        }

        public async Task<DocumentParts> GetDocumentPartDiblink(string resourceId, string segmentId, string Id)
        {
            var p = new
            {
                session_id = "apitest",//_usrsvc.CurrentUser.session_id,
                id = Id
            };
            IEnumerable<DIBLinkData> dibLinkDatas = await this.GetResourceDibLinkData(p);

            List<DibLinkSelect> docParts = dibLinkDatas.GroupBy(p => new { p.resourceId, p.tag2SegmentId, p.name, idx = p.tag2Idx.Min() })
                .OrderBy(p => p.Key.idx)
                .Select(p => new DibLinkSelect { resourceId = p.Key.resourceId, segmentId = p.Key.tag2SegmentId, regexName = p.Key.name })
                .ToList();
            if (docParts.Count() == 0) return null;
            if (docParts.Count() == 1 ? "link;temaid".Contains(docParts.FirstOrDefault().regexName) : false)
            {
                string id = dibLinkDatas.Select(p => p.tag2).FirstOrDefault();
                var n = new
                {
                    session_id = "apitest",
                    resourceId = docParts.Select(p => p.resourceId.ToString()).FirstOrDefault(),
                    id
                };

                ResourceNavigation rNav = await this.GetResourceByResourceIdAndId(n);
                if (rNav == null) return null;

                var d = new
                {
                    session_id = "apitest",
                    id = rNav.resourceId,
                    segment_id = rNav.segmentId,

                };
                ResourceHTML5 data = await this.GetHTML5("[dbo].[GetResourceHTML]", d, null);
                if (data == null) return null;
                if ((id ?? "") != "")
                {
                    XElement items =
                    new XElement("items",
                        new XElement("item", new XAttribute("id", rNav.Id))
                    );


                    XElement document = data.Document.GetDocumentParts(items);
                    if (document == null) return null;

                    data.Document = document;
                    data.selectId = rNav.Id;
                }
                DocumentParts result = new DocumentParts
                {
                    parts = new List<DocumentContainer>
                    {
                        new DocumentContainer(data)
                    }
                };
                return result;
            }
            else
            {
                DocumentParts result = new DocumentParts();
                result.parts = new List<DocumentContainer>();
                foreach (DibLinkSelect dp in docParts)
                {
                    var d = new
                    {
                        session_id = "apitest",
                        id = dp.resourceId,
                        segment_id = dp.segmentId,

                    };
                    ResourceHTML5 data = await this.GetHTML5("[dbo].[GetResourceHTML]", d, null);
                    if (data == null) return null;
                    XElement items =
                    new XElement("items",
                        dibLinkDatas
                        .Where(p => p.resourceId == dp.resourceId && p.tag2SegmentId == dp.segmentId)
                        .Select(p => 
                            new XElement("select", 
                            new XAttribute("id", p.tag2Id),
                                p.tag3Id == null ? null : new XElement("mark", p.tag3Id) 
                            )
                        )
                    );

                    XElement document = data.Document.GetDocumentParts(items);
                    if (document == null) return null;

                    data.Document = document;
                    result.parts.Add(new DocumentContainer(data));
                }
                return result;
            }
        }
        public  async Task<DocumentParts> GetDocumentPart( string resourceId, string id)
        {
            var p = new
            {
                session_id = "apitest",
                resourceId,
                id,
            };
            ResourceNavigation rNav = await this.GetResourceByResourceIdAndId(p);
            if (rNav == null) return null;

            var d = new
            {
                session_id = "apitest",
                id = rNav.resourceId,
                segment_id = rNav.segmentId,

            };

            ResourceHTML5 data = await this.GetHTML5("[dbo].[GetResourceHTML]", d, null);
            if (data == null) return null;

            XElement items =
                new XElement("items",
                    new XElement("item", new XAttribute("id", id))
            );
            data.selectId = id;

            XElement document = data.Document.GetDocumentParts(items);
            if (document == null) return null;

            data.Document = document;
            DocumentParts result = new DocumentParts
            {
                parts = new List<DocumentContainer>
                {
                    new DocumentContainer(data)
                }
            };

            return result;
        }
        public async Task<RelatedResources> GetRelated(object p, int? timeOut = null)
        {
            
            string QueryName = "[dbo].[GetResourceRelated]";
            if (!timeOut.HasValue)
                timeOut = 60;
            try
            {
                using (IDbConnection conn = dbConnection)
                {
                    using (var multi = await conn.QueryMultipleAsync(QueryName, p, null, null, CommandType.StoredProcedure))
                    {
                        IEnumerable<TopicBase> topicBases = multi.Read<TopicBase>();
                        IEnumerable<TopicSubElement> topicSubElements = multi.Read<TopicSubElement>();
                        return new RelatedResources(topicBases, topicSubElements);    
                    }
                    
                }
            }
            catch (Exception e)
            {
                _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', p = '{@p}', Message = '{err}'", QueryName, p, e.Message);
            }
            return null;
        }
        public async Task<IEnumerable<Tag>> GetTags(object p, int? timeOut = null)
        {
            IEnumerable<Tag> result;
            string QueryName = "[dbo].[GetResourceTags]";
            if (!timeOut.HasValue)
                timeOut = 60;
            try
            {
                using (IDbConnection conn = dbConnection)
                {
                    result = await conn.QueryAsync<Tag>(QueryName, p, null, null, CommandType.StoredProcedure);
                    return result;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', p = '{@p}', Message = '{err}'", QueryName, p, e.Message);
            }
            return null;
        }
        public async Task<ResourceHTML5> GetHTML5(string QueryName, object p, int? timeOut = null)
        {
            ResourceHTML5 result;
            if (!timeOut.HasValue)
                timeOut = 60;
            try
            {
                using (IDbConnection conn = dbConnection)
                {
                    using (var multi = await conn.QueryMultipleAsync(QueryName, p, null, timeOut, CommandType.StoredProcedure))
                    {
                        result = multi.Read<ResourceHTML5>().FirstOrDefault();
                        result.ResourceMap = multi.Read<XElement>().FirstOrDefault();
                        result.Document = multi.Read<XElement>().FirstOrDefault();
                        result.Links = multi.Read<LinkData>();
                        result.Related = multi.Read<string>();
                        result.Tags = multi.Read<string>();
                        result.AccountLines = multi.Read<AccountLine>();
                        result.TaxLines = multi.Read<TaxLine>();
                        result.TriggerData = multi.Read<XElement>().FirstOrDefault();
                        result.Collections = multi.Read<XElement>().FirstOrDefault();
                        result.XObjects = multi.Read<XElement>().FirstOrDefault();
                        result.DgVariables = multi.Read<XElement>().FirstOrDefault();
                        result.LabelGroups = multi.Read<ChecklistLabelGroup>();
                        result.Labels = multi.Read<ChecklistLabel>();
                        result.itemData = multi.Read<ChecklistItemData>();
                        result.topicBases = multi.Read<TopicBase>();
                        result.topicSubElements = multi.Read<TopicSubElement>();
                        result.dibVariables =  multi.Read<DibVariable>();
                        result.dibTrigger = multi.Read<DibTrigger>();


                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', p = '{@p}', Message = '{err}'", QueryName, p, e.Message);
            }
            return null;
        }
        public async Task<ResourceHTML5Element> GetRecourceElementdata(string QueryName, object p, int? timeOut = null)
        {
            ResourceHTML5Element result = new ResourceHTML5Element();
            if (!timeOut.HasValue)
                timeOut = 60;
            try
            {
                using (IDbConnection conn = dbConnection)
                {
                    using (var multi = await conn.QueryMultipleAsync(QueryName, p, null, null, CommandType.StoredProcedure))
                    {
                        result.AccountLines = multi.Read<AccountLine>();
                        result.TaxLines = multi.Read<TaxLine>();
                        result.LabelGroups = multi.Read<ChecklistLabelGroup>();
                        result.Labels = multi.Read<ChecklistLabel>();
                        result.itemData = multi.Read<ChecklistItemData>();
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', p = '{@p}', Message = '{err}'", QueryName, p, e.Message);
            }
            return null;
        }
        public async Task<IEnumerable<TopicBase>> ExecTopics(string QueryName, object p, int? timeOut = null)
        {
            IEnumerable<TopicBase> result;
            if (!timeOut.HasValue)
                timeOut = 60;
            try
            {
                using (IDbConnection conn = dbConnection)
                {
                    result = await conn.QueryAsync<TopicBase>(QueryName, p, null, null, CommandType.StoredProcedure);
                }
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', p = '{@p}', Message = '{err}'", QueryName, p, e.Message);
            }
            return null;
        }
        public async Task<TopicDetail> ExecTopicDetail(string QueryName, object p, int? timeOut = null)
        {
            TopicDetail result = new TopicDetail();
            if (!timeOut.HasValue)
                timeOut = 60;
            try
            {
                using (IDbConnection conn = dbConnection)
                {
                    using (var multi = await conn.QueryMultipleAsync(QueryName, p,null, null, CommandType.StoredProcedure))
                    {
                        result = multi.Read<TopicDetail>().FirstOrDefault();
                        result.TopicNames = multi.Read<TopicName>().ToList();
                        result.Databases = multi.Read<Database>().ToList();
                        result.Tags = multi.Read<Tag>().ToList();
                        result.description = multi.Read<string>().FirstOrDefault();
                        result.Dates = multi.Read<Dates>().ToList();
                        IEnumerable<TopicBase> topicBases = multi.Read<TopicBase>();
                        IEnumerable<TopicSubElement> topicSubElements = multi.Read<TopicSubElement>();
                        result.related = new RelatedResources(topicBases, topicSubElements).related;
                        result.Resources = multi.Read<ResourceDocuments>().ToList();
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', Message = '{err}'", QueryName, e.Message);
            }
            return null;
        }
        public async Task<DIBObjects> ExecDIBObjects(string QueryName, object p, int? timeOut = null)
        {
            DIBObjects result = new DIBObjects();
            if (!timeOut.HasValue)
                timeOut = 60;
            try
            {
                using (IDbConnection conn = dbConnection)
                {
                    using (var multi = await conn.QueryMultipleAsync(QueryName, p))
                    {
                        result.Suppliers = multi.Read<Supplier>().ToList();             //1
                        result.TopicTypes = multi.Read<Topictype>().ToList();           //2
                        result.Categories = multi.Read<Category>().ToList();            //3
                        result.Databases = multi.Read<Database>().ToList();             //4
                        result.TagTypes = multi.Read<Tagtype>().ToList();               //5
                        result.TopicNameTypes = multi.Read<TopicNameType>().ToList();   //6
                        result.DateTypes = multi.Read<DateType>().ToList();             //7
                        result.ResourceTypes = multi.Read<ResourceType>().ToList();     //8
                        result.topicDatabases = multi.Read<TopicDatabase>();            //9    
                        result.accountingTypes = multi.Read<AccountingType>();          //10
                        result.accountingCodes = multi.Read<AccountingCode>();          //11
                        result.accountingTaxes = multi.Read<AccountingTax>();           //12
                        result.topics = multi.Read<Topic>();                            //13
                        result.topicNames = multi.Read<TopicNames>();                   //14
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', Message = '{err}'", QueryName, e.Message);
            }
            return null;
        }

        public async Task<XElement> ExecRData(string QueryName, object p, int? timeOut = null)
        {
            IEnumerable<XElement> result;
            if (!timeOut.HasValue)
                timeOut = 60;
            try
            {
                using (IDbConnection conn = dbConnection)
                {
                    result = await conn.QueryAsync<XElement>(QueryName, p, null, null, CommandType.StoredProcedure);
                }
                return result.FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', p = '{@p}', Message = '{err}'", QueryName, p, e.Message);
            }
            return null;
        }
        public async Task<IEnumerable<ResourceDocuments>> GetResourceDocuments(object p, int? timeOut = null)
        {
            string QueryName = "[dbo].[GetResources]";
            IEnumerable<ResourceDocuments> result;
            if (!timeOut.HasValue)
                timeOut = 60;
            try
            {
                using (IDbConnection conn = dbConnection)
                {
                    result = await conn.QueryAsync<ResourceDocuments>(QueryName, p, null, null, CommandType.StoredProcedure);
                }
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', p = '{@p}', Message = '{err}'", QueryName, p, e.Message);
            }
            return null;
        }
        public async Task<XElement> CreateResourceDocument(object p, int? timeOut = null)
        {
            string QueryName = "[dbo].[CreateRecourceDocument]";
            IEnumerable<XElement> result;
            if (!timeOut.HasValue)
                timeOut = 60;
            try
            {
                using (IDbConnection conn = dbConnection)
                {
                    result = await conn.QueryAsync<XElement>(QueryName, p, null, null, CommandType.StoredProcedure);
                }
                return result.FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', p = '{@p}', Message = '{err}'", QueryName, p, e.Message);
            }
            return null;
        }
        public async Task<XElement> SaveResourceDocument(object p, int? timeOut = null)
        {
            string QueryName = "[dbo].[SaveRecourceDocument]";
            IEnumerable<XElement> result;
            if (!timeOut.HasValue)
                timeOut = 60;
            try
            {
                using (IDbConnection conn = dbConnection)
                {
                    result = await conn.QueryAsync<XElement>(QueryName, p, null, null, CommandType.StoredProcedure);
                }
                return result.FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', p = '{@p}', Message = '{err}'", QueryName, p, e.Message);
            }
            return null;
        }
        public async Task<XElement> ExecUpdateResult(string QueryName, object p, int? timeOut = null)
        {
            IEnumerable<XElement> result;
            if (!timeOut.HasValue)
                timeOut = 60;
            try
            {
                using (IDbConnection conn = dbConnection)
                {
                    result = await conn.QueryAsync<XElement>(QueryName, p, null, null, CommandType.StoredProcedure);
                }
                return result.FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', p = '{@p}', Message = '{err}'", QueryName, p, e.Message);
            }
            return null;
        }
        public async Task<XElement> ExecXElementQuery(string QueryName, object p, int? timeOut = null)
        {
            IEnumerable<XElement> result;
            if (!timeOut.HasValue)
                timeOut = 60;
            try
            {
                using (IDbConnection conn = dbConnection)
                {
                    result = await conn.QueryAsync<XElement>(QueryName, p, null, null, CommandType.StoredProcedure);
                }
                return result.FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', p = '{@p}', Message = '{err}'", QueryName, p, e.Message);
            }
            return null;
        }
        //[dbo].[GetResourceDibLinkData]
        public async Task<IEnumerable<DIBLinkData>> GetResourceDibLinkData(object p, int? timeOut = null)
        {
            string QueryName = "[dbo].[GetResourceDibLinkData]";
            IEnumerable<DIBLinkData> result;
            if (!timeOut.HasValue)
                timeOut = 60;
            try
            {
                using (IDbConnection conn = dbConnection)
                {
                    result = await conn.QueryAsync<DIBLinkData>(QueryName, p, null, null, CommandType.StoredProcedure);
                }
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', p = '{@p}', Message = '{err}'", QueryName, p, e.Message);
            }
            return null;
        }
        public async Task<ResourceNavigation> GetResourceByResourceIdAndId(object p, int? timeOut = null)
        {
            string QueryName = "GetResourceByResourceIdAndId";
            IEnumerable<ResourceNavigation> result;
            if (!timeOut.HasValue)
                timeOut = 60;
            try
            {
                using (IDbConnection conn = dbConnection)
                {

                    result = await conn.QueryAsync<ResourceNavigation>(QueryName, p, null, null, CommandType.StoredProcedure);
                }
                return result.FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', p = '{@p}', Message = '{err}'", QueryName, p, e.Message);
            }
            return null;
        }
        //============================================================================================================
        //UTKOMMENTERT
        //============================================================================================================
        //public async Task<ResourceDataDocument> ExecDocumentResource(string QueryName, object p, int? timeOut = null)
        //{
        //    IEnumerable<ResourceDataDocument> result;
        //    if (!timeOut.HasValue)
        //        timeOut = 60;
        //    try
        //    {
        //        using (IDbConnection conn = dbConnection)
        //        {
        //            result = await conn.QueryAsync<ResourceDataDocument>(QueryName, p, null, null, CommandType.StoredProcedure);
        //            return result.FirstOrDefault();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', p = '{@p}', Message = '{err}'", QueryName, p, e.Message);
        //    }
        //    return null;
        //}
        //public async Task<XElement> ExecDocument(string QueryName, object p, int? timeOut = null)
        //{
        //    IEnumerable<XElement> result;
        //    if (!timeOut.HasValue)
        //        timeOut = 60;



        //    try
        //    {
        //        using (IDbConnection conn = dbConnection)
        //        {
        //            try
        //            {
        //                result = await conn.QueryAsync<XElement>(QueryName, p, null, null, CommandType.StoredProcedure);
        //                return result.FirstOrDefault();
        //            }
        //            catch(Exception e)
        //            {

        //            }


        //        }




        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', p = '{@p}', Message = '{err}'", QueryName, p, e.Message);
        //    }
        //    return null;
        //}

        //public async Task<TopicDetails> ExecTopicDetails(string QueryName, object p, int? timeOut = null)
        //{
        //    TopicDetails result = new TopicDetails();
        //    if (!timeOut.HasValue)
        //        timeOut = 60;
        //    try
        //    {
        //        using (IDbConnection conn = dbConnection)
        //        {
        //            using (var multi = await conn.QueryMultipleAsync(QueryName, p, null, null, CommandType.StoredProcedure))
        //            {
        //                result.topics = multi.Read<Topic>();
        //                result.topicNames = multi.Read<TopicNames>();
        //                result.topicDatabases = multi.Read<TopicDatabase>();
        //                result.topicResources = multi.Read<Resources>();
        //            }
        //        }

        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', Message = '{err}'", QueryName, e.Message);
        //    }
        //    return null;
        //}

        //public async Task<Home> ExecHome(string QueryName, object p, int? timeOut = null)
        //{
        //    Home result = new Home();
        //    if (!timeOut.HasValue)
        //        timeOut = 60;

        //    try
        //    {
        //        using (IDbConnection conn = dbConnection)
        //        {
        //            //var r = await conn.QueryAsync<Home>(QueryName, null, null, null, CommandType.StoredProcedure);
        //            //result = r.FirstOrDefault();
        //            using (var multi = await conn.QueryMultipleAsync(QueryName, p))
        //            {
        //                result.Suppliers = multi.Read<Supplier>().ToList();
        //                result.Topictypes = multi.Read<Topictype>().ToList();
        //                result.Categories = multi.Read<Category>().ToList();
        //                result.Databases = multi.Read<Database>().ToList();
        //                result.Tagtypes = multi.Read<Tagtype>().ToList();
        //                result.TopicNameTypes = multi.Read<TopicNameType>().ToList();
        //                result.DateTypes = multi.Read<DateType>().ToList();
        //                result.ResourceTypes = multi.Read<ResourceType>().ToList();
        //            }
        //        }



        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', Message = '{err}'", QueryName,  e.Message);
        //    }
        //    return null;
        //}
        //public async Task<IEnumerable<XElement>> ExecQuery(string QueryName, object p, int? timeOut = null)
        //{
        //    IEnumerable<XElement> result;
        //    if (!timeOut.HasValue)
        //        timeOut = 60;
        //    try
        //    {
        //        using (IDbConnection conn = dbConnection)
        //        {
        //            result = await conn.QueryAsync<XElement>(QueryName,p,null,null,CommandType.StoredProcedure);
        //        }
        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError("<Repository/ExecQuery> Query = '{queryName}', p = '{@p}', Message = '{err}'", QueryName, p, e.Message);
        //    }
        //    return null;
        //}
    }
}