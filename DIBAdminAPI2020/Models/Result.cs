using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using static DIBAdminAPI.Models.FilterListJson;

namespace DIBAdminAPI.Models
{
    public class Result
    {
        public class Collection
        {
            public string collection_id { get; set; }
            public string name { get; set; }
            public string date { get; set; }
            public bool isowner { get; set; }
            public List<dynamic> collaborators { get; set; }
            public Collection(XElement collection)
            {
                collection_id = (string)collection.Attributes("collection_id").FirstOrDefault();
                name = (string)collection.Attributes("name").FirstOrDefault();
                date = (string)collection.Attributes("date").FirstOrDefault();
                isowner = ((string)collection.Attributes("is_owner").FirstOrDefault() ?? "0") == "1";
                if (collection.Element("collaborators") != null && collection.Element("collaborators").HasElements)
                {
                    var coll = collection.Element("collaborators").Elements("collaborator").Select(
                        e => new {
                            name = (string)e.Attributes("name").FirstOrDefault(),
                            email = (string)e.Attributes("email").FirstOrDefault(),
                            account_id = (string)e.Attributes("account_id").FirstOrDefault(),
                            accountname = (string)e.Attributes("accountname").FirstOrDefault(),
                            isowner = ((string)e.Attributes("is_owner").FirstOrDefault() ?? "0") == "1"
                        }).ToArray();

                    collaborators = new List<dynamic>(coll);
                }
            }
        }

        public class ResultSets
        {
            public int offset { get; set; }
            public int count { get; set; }
            public IEnumerable<ResultSet> items { get; set; }

            public ResultSets(XElement set)
            {
                offset = (string)set.Attributes("offset").FirstOrDefault() == null ? 0 : Convert.ToInt32((string)set.Attributes("offset").FirstOrDefault());
                count = (string)set.Attributes("count").FirstOrDefault() == null ? 0 : Convert.ToInt32((string)set.Attributes("count").FirstOrDefault());
                items = set.Elements("result").Select(s => new ResultSet(s)).ToList();
            }
            public ResultSets(XElement topics, XElement collections)
            {
                if (topics == null)
                    items = null;
                else
                    items = topics.Elements("topic").Any()
                        ? topics.Elements("topic").OrderByDescending(p => (string)p.Attributes("lastuse").FirstOrDefault() ?? "")
                                .Select(s => new ResultSet(s, collections))
                        : null;
            }
        }

        public class ResultSetSingleContainer
        {
            public string uid { get; set; }
            public int column { get; set; }
            public ResultSets set { get; set; }

            public ResultSetSingleContainer(XElement Set)
            {
                if (Set == null) return;
                uid = (string)Set.Attributes("uid").FirstOrDefault();
                column = (string)Set.Attributes("column").FirstOrDefault() == null ? 0 : Convert.ToInt32((string)Set.Attributes("column").FirstOrDefault());
                set = new ResultSets(Set);
            }
        }
        public class ResultSet
        {
            public int score { get; set; }
            public string id { get; set; }
            public string segment_id { get; set; }
            public string paragraph_id { get; set; }
            public string type { get; set; }
            public string supplier { get; set; }
            public string date { get; set; }
            public string lastuse { get; set; }
            public string name { get; set; }
            public string view { get; set; }
            public string language { get; set; }
            public string description { get; set; }
            public bool published { get; set; }
            public IEnumerable<ResultSet> subitems { get; set; }
            public IEnumerable<Collection> collections { get; set; }
            public IEnumerable<string> databases { get; set; }
            public ResultSet() { }
            public ResultSet(XElement topic, XElement Collections)
            {
                if (topic == null) return;
                id = (string)topic.Attributes("topic_id").FirstOrDefault();
                type = (string)topic.Attributes("tid").FirstOrDefault();
                supplier = (string)topic.Attributes("sid").FirstOrDefault();
                date = (string)topic.Attributes("odate").FirstOrDefault();
                lastuse = (string)topic.Attributes("lastuse").FirstOrDefault();
                name = (((string)topic.Attributes("short").FirstOrDefault() ?? "") + " " + ((string)topic.Attributes("name").FirstOrDefault() ?? "")).Trim();
                view = (string)topic.Attributes("view").FirstOrDefault();
                language = (string)topic.Attributes("lang").FirstOrDefault() ?? "no";
                published = ((string)topic.Attributes("pub").FirstOrDefault() ?? "0") == "1" ? true : false;
                description = (string)topic.Attributes("desc").FirstOrDefault();
                if (Collections != null)
                {
                    List<XElement> coll = Collections.Elements("collection").Where(p => (string)p.Attributes("topic_id").FirstOrDefault() == id).ToList();
                    if (coll.Count() != 0)
                        collections = coll.OrderByDescending(p => (string)p.Attributes("date").FirstOrDefault() ?? "").Select(p => new Collection(p));
                }
            }
            public ResultSet(XElement topic)
            {

                if (topic == null) return;
                score = Convert.ToInt32((string)topic.Attributes("rank").FirstOrDefault() ?? "0");
                id = (string)topic.Attributes("topic_id").FirstOrDefault();
                segment_id = (string)topic.Attributes("segment_id").FirstOrDefault();
                paragraph_id = (string)topic.Attributes("paragraf_id").FirstOrDefault();
                type = (string)topic.Attributes("tid").FirstOrDefault();
                supplier = (string)topic.Attributes("sid").FirstOrDefault();
                date = (string)topic.Attributes("odate").FirstOrDefault();
                name = (((string)topic.Attributes("short").FirstOrDefault() ?? "") + " " + ((string)topic.Attributes("name").FirstOrDefault() ?? "")).Trim();
                view = (string)topic.Attributes("view").FirstOrDefault();
                language = (string)topic.Attributes("lang").FirstOrDefault() ?? "no";
                published = ((string)topic.Attributes("pub").FirstOrDefault() ?? "0") == "1" ? true : false;

                if (topic.Descendants("database").Any())
                {
                    databases = topic.Descendants("database").Select(p => (string)p.Attributes("id").FirstOrDefault()).ToList();
                }
                else if (topic.Attributes("db").Any())
                    databases = topic.Attributes("db").FirstOrDefault().Value.Split(",").ToList();

                if ((type == "45" || type == "46") && topic.Descendants("content").Any())
                    description = topic.Element("description").Element("content").Value;
                else
                    description = (string)topic.Attributes("desc").FirstOrDefault();

                if (topic.Descendants("item").Count() != 0)
                {
                    subitems = topic.Descendants("item").Select(p =>
                        new ResultSet
                        {
                            paragraph_id = (string)p.Attributes("id").FirstOrDefault(),
                            segment_id = (string)p.Attributes("segment_id").FirstOrDefault(),
                            name = (string)p.Attributes("title").FirstOrDefault(),
                        });
                }

            }
        }

        public class ResultSetContainer
        {
            public ResultSets index { get; set; }
            public ResultSets tools { get; set; }
            public ResultSets source { get; set; }
            public ResultSetContainer(XElement results)
            {

                index = results.Elements("set").Where(p => (string)p.Attributes("column").FirstOrDefault() == "0").FirstOrDefault() == null ? null : new ResultSets(results.Elements("set").Where(p => (string)p.Attributes("column").FirstOrDefault() == "0").FirstOrDefault());
                tools = results.Elements("set").Where(p => (string)p.Attributes("column").FirstOrDefault() == "1").FirstOrDefault() == null ? null : new ResultSets(results.Elements("set").Where(p => (string)p.Attributes("column").FirstOrDefault() == "1").FirstOrDefault());
                source = results.Elements("set").Where(p => (string)p.Attributes("column").FirstOrDefault() == "2").FirstOrDefault() == null ? null : new ResultSets(results.Elements("set").Where(p => (string)p.Attributes("column").FirstOrDefault() == "2").FirstOrDefault());
            }
        }
        public class ResultSetJson
        {
            public ResultSetSingleContainer container { set; get; }
            public ResultSetJson(XElement e)
            {
                container = new ResultSetSingleContainer(e);
            }

        }
        public class ResultFilter
        {
            public string query { get; set; }
            public List<string> categories { get; set; }
            public List<string> types { get; set; }
            public List<string> suppliers { get; set; }
            public List<string> databases { get; set; }
            public bool autocorrect { get; set; }
            public string org_query { get; set; }
            public ResultFilter(XElement filter)
            {
                if (filter == null) return;
                query = (string)filter.Attributes("query").FirstOrDefault();
                categories = filter.Elements("item").Where(p => (string)p.Attributes("type").FirstOrDefault() == "2")
                           .GroupBy(p => (string)p.Attributes("value").FirstOrDefault())
                           .Select(p => p.Key)
                           .ToList();
                if (categories.Count() == 0) categories = null;

                databases = filter.Elements("item").Where(p => (string)p.Attributes("type").FirstOrDefault() == "1")
                    .GroupBy(p => (string)p.Attributes("value").FirstOrDefault())
                    .Select(p => p.Key)
                    .ToList();
                if (databases.Count() == 0) databases = null;

                types = filter.Elements("item").Where(p => (string)p.Attributes("type").FirstOrDefault() == "3")
                    .GroupBy(p => (string)p.Attributes("value").FirstOrDefault())
                    .Select(p => p.Key)
                    .ToList();
                if (types.Count() == 0) types = null;

                suppliers = filter.Elements("item").Where(p => (string)p.Attributes("type").FirstOrDefault() == "4")
                    .GroupBy(p => (string)p.Attributes("value").FirstOrDefault())
                    .Select(p => p.Key)
                    .ToList();
                if (suppliers.Count() == 0) suppliers = null;
            }
        }
        public class ResultTimespan
        {
            public string first { get; set; }
            public string latest { get; set; }
            public ResultTimespan(XElement Group)
            {
                first = (string)Group.Elements("results").Attributes("first").FirstOrDefault() ?? "0";
                latest = (string)Group.Elements("results").Attributes("latest").FirstOrDefault() ?? "0";
            }
        }
        public class ResultCounter
        {
            public string query { get; set; }
            public int total { get; set; }
            public Dictionary<string, int> categories { get; set; }
            public Dictionary<string, int> types { get; set; }
            public Dictionary<string, int> suppliers { get; set; }
            public Dictionary<string, int> databases { get; set; }
            public Dictionary<string, int> internals { get; set; }

            public ResultCounter(XElement Group)
            {
                query = (string)Group.Elements("results").Attributes("query").FirstOrDefault();
                total = Convert.ToInt32((string)Group.Elements("results").Attributes("count").FirstOrDefault() ?? "0");
                categories = Group.Elements("topictypes").Elements("topictype")
                            .GroupBy(p => (string)p.Attributes("cid").FirstOrDefault())
                            .Select(p =>
                                new KeyValuePair<string, int>(p.Key, p.Select(n => (int)n.Attributes("n").FirstOrDefault()).Sum(n => n))
                            )
                            .ToDictionary(p => p.Key.ToString(), p => p.Value);
                if (categories.Count() == 0) categories = null;

                types = Group.Elements("topictypes").Elements("topictype")
                            .Select(p => new KeyValuePair<string, int>((string)p.Attributes("id").FirstOrDefault(), Convert.ToInt32((string)p.Attributes("n").FirstOrDefault() ?? "0")))
                            .ToDictionary(p => p.Key.ToString(), p => p.Value);
                if (types.Count() == 0) types = null;

                suppliers = Group.Elements("suppliers").Elements("supplier")
                            .Select(p => new KeyValuePair<string, int>((string)p.Attributes("id").FirstOrDefault(), Convert.ToInt32((string)p.Attributes("n").FirstOrDefault() ?? "0")))
                            .ToDictionary(p => p.Key.ToString(), p => p.Value);
                if (suppliers.Count() == 0) suppliers = null;

                databases = Group.Elements("databases").Elements("database")
                            .Select(p => new KeyValuePair<string, int>((string)p.Attributes("id").FirstOrDefault(), Convert.ToInt32((string)p.Attributes("n").FirstOrDefault() ?? "0")))
                            .ToDictionary(p => p.Key.ToString(), p => p.Value);
                if (databases.Count() == 0) databases = null;

                internals = Group.Elements("internals").Elements("internal")
                            .Select(p => new KeyValuePair<string, int>((string)p.Attributes("id").FirstOrDefault(), Convert.ToInt32((string)p.Attributes("n").FirstOrDefault() ?? "0")))
                            .ToDictionary(p => p.Key.ToString(), p => p.Value);
                if (internals.Count() == 0) internals = null;
            }
        }
        public class ResultListContainer
        {
            public string uid { get; set; }
            public string searchword { get; set; }
            public ResultFilter filter { get; set; }
            public ResultCounter count { get; set; }
            //public List<ResultSet> results { get; set; }
            public ResultSetContainer results { get; set; }
            public ResultTimespan timespan { get; set; }

            public ResultListContainer(XElement Group)
            {
                uid = (string)Group.Elements("results").Attributes("uid").FirstOrDefault();
                searchword = (string)Group.Elements("results").Attributes("searchword").FirstOrDefault();
                filter = new ResultFilter(Group.Elements("results").Elements("filter").FirstOrDefault());
                timespan = new ResultTimespan(Group);
                count = new ResultCounter(Group);
                results = new ResultSetContainer(Group.Elements("results").FirstOrDefault());

                //results = Group.Elements("results")
                //        .Elements("result")
                //        .Select(p => new ResultSet(p))
                //        .ToList();


            }
        }

        public class NewsListContainer
        {
            public string uid { get; set; }
            public string searchword { get; set; }
            public ResultFilter filter { get; set; }
            public ResultCounter count { get; set; }
            public ResultSets results { get; set; }
            public ResultTimespan timespan { get; set; }
            public FilterListContainer filterlist { get; set; }


            public NewsListContainer(XElement Group)
            {
                uid = (string)Group.Elements("results").Attributes("uid").FirstOrDefault();
                searchword = (string)Group.Elements("results").Attributes("searchword").FirstOrDefault();
                filter = new ResultFilter(Group.Elements("results").Elements("filter").FirstOrDefault());
                timespan = new ResultTimespan(Group);
                count = new ResultCounter(Group);
                results = new ResultSets(Group.Descendants("set").FirstOrDefault());
                filterlist = new FilterListContainer(Group);
            }
        }


        public class ResultListJson
        {
            public ResultListContainer container { set; get; }
            public ResultListJson(XElement e)
            {
                container = new ResultListContainer(e);
            }






        }
    }
}
