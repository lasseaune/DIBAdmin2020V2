using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using DIBAdminAPI.Models;
using static DIBAdminAPI.Models.Result;

namespace DIBAdminAPI.Models
{
    public class FilterListJson
    {
        public UserDataContainer container { get; set; }

        public class UserDataContainer
        {
            public UserDataContainer()
            {

            }
            public UserData userdata { get; set; }
            public FilterListContainer filterlist { get; set; }

            public ResultListContainer historylist { get; set; }
            public ResultSetSingleContainer favorites { get; set; }
            public ResultSetSingleContainer fillings { get; set; }
            public TemplatesContainer templates { get; set; }
            public StopwordsContainer stopwords { get; set; }

            public UserDataContainer(XElement group, UserSession user)
            {
                userdata = new UserData(user);
                filterlist = new FilterListContainer(group);
                historylist = new ResultListContainer(group);
                favorites = new ResultSetSingleContainer(group.Elements("favorite").FirstOrDefault());
                fillings = new ResultSetSingleContainer(group.Elements("fillings").FirstOrDefault());
                templates = new TemplatesContainer(group.Elements("templates").FirstOrDefault());
                stopwords = new StopwordsContainer(group.Elements("stopwords").FirstOrDefault());
            }
        }

        public class TemplatesContainer
        {
            public List<Template> Templates { get; set; }

            public TemplatesContainer(XElement templates)
            {
                Templates = templates.Elements("template").Select(t => new Template(t)).ToList();
            }
        }

        public class Template
        {
            public int id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string type { get; set; }

            public Template(XElement template)
            {
                if (template != null)
                {
                    id = template.Attributes("id").FirstOrDefault() != null ? Convert.ToInt32(template.Attributes("id").FirstOrDefault().Value) : 0;
                    name = template.Attributes("name").FirstOrDefault().Value;
                    description = template.Attributes("description").FirstOrDefault() != null ? template.Attributes("description").FirstOrDefault().Value : null;
                    type = template.Attributes("type").FirstOrDefault() != null ? template.Attributes("type").FirstOrDefault().Value : null;
                }
            }
        }

        public class StopwordsContainer
        {
            public string DefaultLanguageId { get; set; }
            public List<Language> Languages { get; set; }

            public StopwordsContainer(XElement stopwords)
            {
                if (stopwords != null)
                {
                    DefaultLanguageId = stopwords != null ? stopwords.Attributes("default_language_id").Any() ? stopwords.Attribute("default_language_id").Value : string.Empty : string.Empty;
                    Languages = stopwords.Descendants("language").Select(t => new Language(t)).ToList();
                }
            }
        }

        public class Language
        {
            public string Name { get; set; }
            public string LanguageId { get; set; }
            public List<string> Stopwords { get; set; }

            public Language(XElement language)
            {
                Name = language.Attributes("language").Any() ? language.Attribute("language").Value : string.Empty;
                LanguageId = language.Attributes("language_id").Any() ? language.Attribute("language_id").Value : string.Empty;
                Stopwords = language.Descendants("stopword").Select(t => t.Attributes("w").Any() ? t.Attribute("w").Value : string.Empty).ToList();
            }
        }

        public class UserData
        {
            public UserData()
            {

            }
            public string session_id { get; set; }
            public string email { get; set; }
            public string name { get; set; }
            public string companyname { get; set; }
            public string accountname { get; set; }
            public string role { get; set; }
            public string bizarea { get; set; }
            public string licensetype { get; set; }
            public bool isdemo { get; set; }
            public DateTime? license_to { get; set; }
            public string initials { get; set; }
            public bool isactive { get; set; }
            public int permissions { get; set; }
            public int role_id { get; set; }

            public UserData(UserSession user)
            {
                session_id = user.session_id;
                email = user.Epost;
                name = user.Navn;
                role = user.Rolle;
                bizarea = user.Kundegruppe;
                licensetype = user.Produktpakke;
                isdemo = user.Demokonto;
                isactive = user.Aktiv;
                initials = user.Initials;
                accountname = user.KontoNavn;
                companyname = user.KundeNavn;
                license_to = user.LisensTil;
                permissions = user.Rettighet;
                role_id = user.RolleId;
            }

        }
        public class FilterListContainer
        {

            public List<FilterListCategory> categories { get; set; }
            public List<FilterListType> types { get; set; }
            public List<FilterListSupplier> suppliers { get; set; }
            public List<FilterListDatabase> databases { get; set; }
            public List<FilterListInternal> internals { get; set; }
            public FilterTimespan timespan { get; set; }
            public FilterListContainer(XElement group)
            {
                if (group.Elements("timespan").FirstOrDefault() != null)
                    timespan = new FilterTimespan
                    {
                        first = (string)group.Elements("timespan").Attributes("first").FirstOrDefault(),
                        latest = (string)group.Elements("timespan").Attributes("latest").FirstOrDefault(),
                    };
                categories = group
                    .Elements("categorys")
                    .Elements("category")
                    .Select(p => new FilterListCategory
                    {
                        id = (string)p.Attributes("id").FirstOrDefault(),
                        name = (string)p.Attributes("name").FirstOrDefault(),
                        types = group
                            .Elements("topictypes")
                            .Elements("topictype")
                            .Where(t => (string)t.Attributes("cid").FirstOrDefault() == (string)p.Attributes("id").FirstOrDefault())
                            .Select(t => (string)t.Attributes("id").FirstOrDefault()).ToList()
                    }).ToList();
                types = group
                    .Elements("topictypes")
                    .Elements("topictype")
                    .Select(p => new FilterListType
                    {
                        id = (string)p.Attributes("id").FirstOrDefault(),
                        name = (string)p.Attributes("name").FirstOrDefault()
                    }).ToList();
                suppliers = group
                    .Elements("suppliers")
                    .Elements("supplier")
                    .Select(p => new FilterListSupplier
                    {
                        id = (string)p.Attributes("id").FirstOrDefault(),
                        name = (string)p.Attributes("name").FirstOrDefault()
                    }).ToList();
                databases = group
                    .Elements("databases")
                    .Elements("database")
                    .Select(p => new FilterListDatabase
                    {
                        id = (string)p.Attributes("id").FirstOrDefault(),
                        name = (string)p.Attributes("name").FirstOrDefault()
                    }).ToList();
                internals = group
                    .Elements("internals")
                    .Elements("internal")
                    .Select(p => new FilterListInternal
                    {
                        id = (string)p.Attributes("id").FirstOrDefault(),
                        name = (string)p.Attributes("name").FirstOrDefault()
                    }).ToList();
            }
        }
        public class FilterTimespan
        {
            public FilterTimespan()
            {

            }
            public string first { get; set; }
            public string latest { get; set; }
        }
        public class FilterListInternal
        {
            public FilterListInternal()
            {

            }
            public string id { get; set; }
            public string name { get; set; }
        }
        public class FilterListDatabase
        {
            public FilterListDatabase()
            {

            }
            public string id { get; set; }
            public string name { get; set; }
        }
        public class FilterListSupplier
        {
            public FilterListSupplier()
            {

            }
            public string id { get; set; }
            public string name { get; set; }

        }
        public class FilterListType
        {
            public FilterListType()
            {

            }
            public string id { get; set; }
            public string name { get; set; }
        }
        public class FilterListCategory
        {
            public FilterListCategory()
            {

            }
            public string id { get; set; }
            public string name { get; set; }
            public List<string> types { get; set; }

        }
    }
}
