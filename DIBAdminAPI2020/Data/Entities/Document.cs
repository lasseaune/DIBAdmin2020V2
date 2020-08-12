using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using static DIBAdminAPI.Models.Result;
using DIBAdminAPI.Helpers.Extentions;

namespace DIBAdminAPI.Data.Entities
{
    public class DocumentPartJson
    {
        public DocumentPartsContainer container { get; set; }

        public DocumentPartJson(XElement data, string topicId, string segmentId, string Id, IEnumerable<AccountingType> accountingType,
            IEnumerable<AccountingCode> accountingCodes,
            IEnumerable<AccountingTax> accountingTax)
        {
            container = new DocumentPartsContainer(data, topicId, segmentId, Id, accountingType, accountingCodes, accountingTax);
        }
    }
    public class DocumentPartsContainer
    {

        public string document { get; set; }
        public ResultSetContainer relations { get; set; }
        public ResultSet link { get; set; }
        //public IEnumerable<DocumentPartContainer> parts { get; set; }
        public List<JsonPart> parts { get; set; }
        public DocumentPartsContainer(
            XElement result, 
            string topicId,
            string segmentId,
            string Id,
            IEnumerable<AccountingType> accountingType,
            IEnumerable<AccountingCode> accountingCodes,
            IEnumerable<AccountingTax> accountingTax
            )
        {
            //if (result == null) return;
            //if (result.Name.LocalName == "x-relations" && result.Elements("result").Count() != 0)
            //{
            //    relations = result.GetResultSet();
            //}
            //else if (result.DescendantsAndSelf("package").Count() == 1 && (string)result.DescendantsAndSelf("package").Attributes("type").FirstOrDefault() == "link" && result.DescendantsAndSelf("package").Elements("topics").Elements("topic").Count() == 1)
            //{
            //    link = new ResultSet(result.DescendantsAndSelf("package").Elements("topics").Elements("topic").FirstOrDefault());
            //}
            //else
            if (result.DescendantsAndSelf("package").Count() != 0)
            {
                //parts = result.DescendantsAndSelf("package").Select(p => new DocumentPartContainer(p));
                parts = result.DescendantsAndSelf("package")
                    .Select(p => new DocumentPartContainer(p, accountingType, accountingCodes, accountingTax))
                    .Select(p => new JsonPart
                    {
                        resourceid = p.resourceId,
                        segmentid = p.segmentId,
                        root = p.root,
                        elements = p.elements
                    }
                    ).ToList();
            }
            else
            {
                document = (new XElement("p", "Ikke noe å vise frem for " + topicId + (Id == "" ? "" : @"/" + Id))).ToString();
            }


        }
    }
    public class DocumentPartContainer
    {
        public bool access { get; set; }
        public IEnumerable<DocumentTOC> toc { get; set; }
        public string name { get; set; }
        //public string document { get; set; }
        public List<JsonChild> root { get; set; }
        public Dictionary<string, JsonElement> elements { get; set; }
        public ResultSet metadata { get; set; }
        public IEnumerable<ParentsItem> parents { get; set; }
        public string resourceId  { get; set; }
        public string segmentId { get; set; }

        public Dictionary<string, AccountingElementApi> elementdata { get; set; }
        public Dictionary<string, ObjectApi> objects { get; set; }



        public DocumentPartContainer(
            XElement result,
            IEnumerable<AccountingType> accountingType,
            IEnumerable<AccountingCode> accountingCodes,
            IEnumerable<AccountingTax> accountingTax
        )
        {
            if (result.HasElements)
            {
                resourceId = Regex.Replace(((string)result.Attributes("topic_id").FirstOrDefault()??""),@"[\{\}]", "");
                segmentId = (string)result.Attributes("segment_id").FirstOrDefault()??"";
                access = ((string)result.Attributes("cred").FirstOrDefault() ?? "0") == "1" ? true : false;
                XElement Parents = result.Elements("parents").FirstOrDefault();
                XElement settings = result.Elements("settings").FirstOrDefault();
                XElement map = result.Elements("index").FirstOrDefault();
                XElement docpart = result.Elements("docpart").FirstOrDefault();
                XElement relation = result.Elements("x-relations").FirstOrDefault();

                AccountingObjectsAPI ao = new AccountingObjectsAPI(result.Elements("accroot").FirstOrDefault(), accountingType, accountingCodes, accountingTax);
                elementdata = ao.objectsList;
                objects = ao.objects;
                
                
                XElement desc = result.Elements("topic").Elements("description").FirstOrDefault();
                XElement xmoreinfo = result.Elements("x-more-info").FirstOrDefault();
                name = (string)result.Elements("topic").Attributes("name").FirstOrDefault();
                XElement topic = result.Descendants("topic").FirstOrDefault();
                XElement topics = result.Descendants("topics").FirstOrDefault();

                string select_id = (string)result.Attributes("select_id").FirstOrDefault();
                if (select_id != null && topic != null)
                    topic.Add(new XAttribute("select_id", select_id));
                string segment_id = (string)result.Attributes("segment_id").FirstOrDefault();

                metadata = new ResultSet(topic);
                

                XElement xlinkgroup = result.Elements("x-link-group").FirstOrDefault();

                
                string viewstyle = (result.Elements("settings").Elements("viewstyle").Elements("main").Select(p => p.Value).FirstOrDefault() ?? "").ToLower();
                if (docpart != null && viewstyle != null)
                {
                    switch (viewstyle)
                    {
                        case "11":


                            XElement docparthtml = docpart.ConvertXMLtoHTML(ao.aobjects, xmoreinfo, topics, xlinkgroup);

                            docparthtml
                                .Descendants()
                                .Select(p=>p)
                                .ToList()
                                .ForEach(p => p.SetAttributeValueEx("id", Guid.NewGuid().ToString()));
                            
                            string document_id = "document;" + resourceId + ";" + segmentId;
                            //JsonObject htmlJson = new JsonObject(docpart.Descendants("document").Elements());
                            root = new List<JsonChild>();
                            root.Add(new JsonChild { id = document_id });
                            elements = new Dictionary<string, JsonElement>();
                            elements.Add(document_id,
                                new JsonElement
                                {
                                    name="div",
                                    attributes = new Dictionary<string, string>() { { "class", "doccontainer" } },
                                    children = docparthtml.Elements().Select(p => new JsonChild { id = (string)p.Attributes("id").FirstOrDefault() }).ToList()
                                }
                            );
                            elements.AddRange(
                                docparthtml
                                .Descendants()
                                .Select(p => p)
                                .ToDictionary(p => (string)p.Attributes("id").FirstOrDefault(), p => new JsonElement(p))
                            );
                            if (ao.aobjects != null)
                            {
                                (
                                    from e in elements
                                    join a in ao.aobjects
                                    on e.Key.ToLower() equals a.ToLower()
                                    select e
                                ).ToList()
                                .ForEach(p => p.Value.otherprops = new Dictionary<string, string>() { { "accounting", "true" } });
                            }
                            break;
                    }
                }
            }
        }
    }
    public class DocumentContainer
    {
       
        public bool access { get; set; }
        public bool companylookup { get; set; }
        public string name { get; set; }
        public List<JsonChild> root { get; set; }
        public Dictionary<string, JsonElement> elements { get; set; }
        public List<JsonChild> tocroot { get; set; }
        public Dictionary<string, JsonToc> toc { get; set; }
        public Dictionary<string, AccountingElementApi> elementdata { get; set; }
        public Dictionary<string, ObjectApi> objects { get; set; }
        
        public IEnumerable<XVariable> variables { get; set; }
        public IEnumerable<XObj> xobjects { get; set; }
        
        public DocumentContainer(string Name, XElement Document, string resourceId)
        {
            resourceId = resourceId.ToLower();
            name = Name;
        
            TocJson tocJson = new TocJson(null, Document, resourceId, "");
            tocroot = tocJson.tocroot;
            toc = tocJson.toc;

            string document_id = "document;" + resourceId + ";" + "";
            root = new List<JsonChild>();
            root.Add(new JsonChild { id = document_id });
            elements = new Dictionary<string, JsonElement>();
            elements = new Dictionary<string, JsonElement>();
            elements.Add(document_id,
                new JsonElement
                {
                    name = "div",
                    attributes = new Dictionary<string, string>() { { "class", "doccontainer" } },
                    children = Document.Elements().Select(p => new JsonChild { id = (string)p.Attributes("id").FirstOrDefault() }).ToList()
                }
            );
            elements.AddRange(
                Document
                .Descendants()
                .Select(p => p)
                .ToDictionary(p => (string)p.Attributes("id").FirstOrDefault(), p => new JsonElement(p))
            );

        }
        public DocumentContainer(ResourceDataDocument data, DIBObjects dIBObjects)
        {

            access = data.Accsess == 1 ? true : false;
            XElement vars = null;
            if (data.DGVariables != null)
            {
                vars = data.DGVariables.Elements("variables").FirstOrDefault();
                XElement userdatas = data.DGVariables.Elements("userdatas").FirstOrDefault();
                XElement triggerdata = data.DGVariables.Elements("triggerdata").FirstOrDefault();

                if (vars != null && userdatas != null)
                {
                    (
                        from v in vars.Elements("variable")
                        join ud in userdatas.Elements("userdata")
                        on (v.Elements("id").Select(s => s.Value).FirstOrDefault() ?? "-0").ToLower() equals ((string)ud.Attributes("id").FirstOrDefault() ?? "-1").ToLower()
                        select new { var = v, userdata = ud }
                    ).ToList()
                    .ForEach(p =>
                    {
                        if (p.var.Attributes("changedate").FirstOrDefault() == null)
                            p.var.Add(new XAttribute("changedate", p.userdata.Attribute("changedate").Value));
                        else
                            p.var.Attributes("changedate").FirstOrDefault().Value = p.userdata.Attribute("changedate").Value;

                        if (p.var.Elements("value").FirstOrDefault() == null)
                            p.var.Add(new XElement("value", p.userdata.Value));
                        else
                            p.var.Elements("value").FirstOrDefault().Value = p.userdata.Value;

                    });

                    var c = from v in vars.Elements("variable")
                                .Where(p => Regex.IsMatch(p.Elements("id").Select(s => s.Value).FirstOrDefault(), @"\*N\*", RegexOptions.IgnoreCase))
                            select new
                            {
                                e = v,
                                r = new Regex(Regex.Replace(v.Elements("id").Select(s => s.Value).FirstOrDefault(), @"\*N\*", @"\d+", RegexOptions.IgnoreCase))
                            };

                    foreach (var e in c)
                    {
                        var v = userdatas.Elements("userdata").Where(p => e.r.IsMatch((string)p.Attributes("id").FirstOrDefault()));
                        foreach (XElement ud in v)
                        {
                            e.e.Add(new XElement("variable",
                                new XAttribute("changedate", (string)ud.Attributes("changedate").FirstOrDefault()),
                                new XElement("id", (string)ud.Attributes("id").FirstOrDefault()),
                                new XElement("value", ud.Value)
                                )
                            );
                        }
                    }

                }
                if (vars != null && triggerdata != null)
                {
                    vars = vars.InsertProffData(triggerdata);
                }
            }
            else
            {
                vars = data.DGVariables;
            }

            if (vars != null)
            {
                List<XElement> nvar = vars
                    .Elements("variable").Where(p => Regex.IsMatch(p.Elements("id").Select(v => v.Value).FirstOrDefault(), @"\*N\*"))
                    .ToList();
                nvar.ForEach(p => p.EvalNVariable());

                variables = vars.Elements("variable").Select(p => new XVariable(p));
            }


            XElement xtriggers = data.TriggerData;
            XElement xobjs = data.XObjects;
            if (xobjs != null)
            {
                (
                    from o in xobjs.Descendants("x-var")
                    join v in vars.Elements("variable").Where(p => (string)p.Attributes("counter").FirstOrDefault() == "true")
                    on (string)o.Attributes("id").FirstOrDefault() equals v.Elements("id").Select(p => p.Value).FirstOrDefault()
                    select o
                )
                .ToList().ForEach(p => p.SetAttributeValueEx("counter", "true"));
                (
                    from o in xobjs.Descendants("x-var")
                    join v in xtriggers.Elements("x-trigger")
                    on ((string)o.Attributes("id").FirstOrDefault() ?? "-1").Trim().ToLower() equals ((string)v.Attributes("id").FirstOrDefault() ?? "-0").Trim().ToLower()
                    select o
                )
                .ToList()
                .ForEach(p => p.SetAttributeValueEx("trigger", "true"));

                xobjects = xobjs.Elements().Select(p => new XObj(p, vars));
            }

            XElement map = data.ResourceMap;

            AccountingObjectsAPI ao = new AccountingObjectsAPI(data.Accounting, dIBObjects.accountingTypes, dIBObjects.accountingCodes, dIBObjects.accountingTaxes);

            //accounts = ao.accounts;
            if (ao.objectsList != null)
            {
                elementdata = ao.objectsList;
                objects = ao.objects;
            }

            

            //XElement xlinkgroup = result.Elements("x-link-group").FirstOrDefault();
            //XElement xComments = result.Elements("x-cs").FirstOrDefault();
            //XElement xMirrors = result.Elements("x-mirrors").FirstOrDefault();


            if (data.Document != null)
            {
                data.Document.Descendants().Where(p => (string)p.Attributes("id").FirstOrDefault() == null && p.Nodes().Count()==0).ToList().ForEach(p => p.Remove());
                data.Document.Descendants().Where(p => (string)p.Attributes("id").FirstOrDefault() == null && p.Nodes().Count() != 0).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));

                string lookup = data.Document.Descendants("x-settings").Elements("companylookup").Select(p => p.Value).FirstOrDefault();
                companylookup = false;
                if (vars != null)
                {
                    if (lookup == "off")
                        companylookup = false;
                    else
                        companylookup = true;
                }

                if (data.Document.Attributes("id").FirstOrDefault() == null)
                {
                    data.Document.Add(new XAttribute("id", data.resourceId));
                }
                else
                {
                    data.Document.Attributes("id").FirstOrDefault().SetValue(data.resourceId);
                }

                XElement docparthtml = data.Document.ConvertXMLtoHTML5(data.LinkData);

                TocJson tocJson = new TocJson(map, docparthtml, data.resourceId, data.segmentId);
                tocroot = tocJson.tocroot;
                toc = tocJson.toc;

                string document_id = "document;" + data.resourceId + ";" + data.segmentId;
                root = new List<JsonChild>();
                root.Add(new JsonChild { id = document_id });
                elements = new Dictionary<string, JsonElement>();
                elements.Add(document_id,
                    new JsonElement
                    {
                        name = "div",
                        attributes = new Dictionary<string, string>() { { "class", "doccontainer" } },
                        children = docparthtml.Elements().Select(p => new JsonChild { id = (string)p.Attributes("id").FirstOrDefault() }).ToList()
                    }
                );

                List<string> like = docparthtml.Descendants().GroupBy(p => (string)p.Attributes("id").FirstOrDefault()).Where(p => p.Count() > 1).Select(p => p.Key).ToList();

                List<XElement> elike = docparthtml.Descendants().Where(p => (string)p.Attributes("id").FirstOrDefault() == null).ToList();

                elements.AddRange(
                    docparthtml
                    .Descendants()
                    .Select(p => p)
                    .ToDictionary(p => (string)p.Attributes("id").FirstOrDefault(), p => new JsonElement(p))
                );

                if (ao.aobjects != null)
                {
                    (
                        from e in elements
                        join a in ao.aobjects
                        on e.Key.ToLower() equals a.ToLower()
                        select e
                    ).ToList()
                    .ForEach(p => p.SetOtherProps(elements, "accounting", "accref"));
                }


                if (data.Tags!=null)
                {
                    (
                        from e in elements
                        join t in data.Tags.Elements("tag")
                        on e.Key.ToLower() equals ((string)t.Attributes("id").FirstOrDefault()??"").ToLower()
                        select e
                    ).ToList()
                    .ForEach(p => p.SetOtherProps(elements, "tags", "tagsRef"));
                }
                if (data.Related != null)
                {
                    (
                        from e in elements
                        join t in data.Related.Elements("rel")
                        on e.Key.ToLower() equals ((string)t.Attributes("id").FirstOrDefault() ?? "").ToLower()
                        select e
                    ).ToList()
                    .ForEach(p => p.SetOtherProps(elements, "related", "relatedRef"));
                }
            }
        }
    }
    public class xDocumentContainer
    {
        public string resourceId { get; set; }
        public string topicId { get; set; }
        public string segmentId { get; set; }
        public bool access { get; set; }
        public List<SearchWords> mark { get; set; }
        public bool companylookup { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public ResultSetContainer relations { get; set; }

        //public IEnumerable<DocumentTOC> toc { get; set; }
        //public string document { get; set; }
        public List<JsonChild> root { get; set; }

        public Dictionary<string, JsonElement> elements { get; set; }

        public List<JsonChild> tocroot { get; set; }

        public Dictionary<string, JsonToc> toc { get; set; }
        public ResultSet metadata { get; set; }

        public Dictionary<string, AccountingElementApi> elementdata { get; set; }
        public Dictionary<string, ObjectApi> objects { get; set; }

        public IEnumerable<XObj> xobjects { get; set; }
        public IEnumerable<XVariable> variables { get; set; }

        public xDocumentContainer(string Name, XElement Document, string resourceId)
        {
            resourceId = resourceId.ToLower();
            name = Name;
            //JsonObject htmlJson = new JsonObject(Document.Descendants("document").Elements());
            //root = htmlJson.root;
            //elements = htmlJson.elements;
            TocJson tocJson = new TocJson(null, Document, resourceId, "");
            tocroot = tocJson.tocroot;
            toc = tocJson.toc;

            string document_id = "document;" + resourceId + ";" + "";
            root = new List<JsonChild>();
            root.Add(new JsonChild { id = document_id });
            elements = new Dictionary<string, JsonElement>();
            elements = new Dictionary<string, JsonElement>();
            elements.Add(document_id,
                new JsonElement
                {
                    name = "div",
                    attributes = new Dictionary<string, string>() { { "class", "doccontainer" } },
                    children = Document.Elements().Select(p => new JsonChild { id = (string)p.Attributes("id").FirstOrDefault() }).ToList()
                }
            );
            elements.AddRange(
                Document
                .Descendants()
                .Select(p => p)
                .ToDictionary(p => (string)p.Attributes("id").FirstOrDefault(), p => new JsonElement(p))
            );

        }
        public xDocumentContainer(
            XElement result,
            IEnumerable<AccountingType> accountingType,
            IEnumerable<AccountingCode> accountingCodes,
            IEnumerable<AccountingTax> accountingTax
        )
        {
            if (result == null) return;
            if (result.Name.LocalName == "package")
            {
                

                string topic_id = (string)result.Attributes("topic_id").FirstOrDefault();

                resourceId = topic_id;
                topicId = topic_id;
                segmentId = (string)result.Attributes("segment_id").FirstOrDefault();

                access = ((string)result.Attributes("cred").FirstOrDefault() ?? "0") == "1" ? true : false;
                XElement topic = result.Elements("topic").FirstOrDefault();
                if (topic == null)
                {
                    //document = new XElement("p", "Ingenting å vise").ToString();
                    return;
                }
                metadata = new ResultSet(topic);

                XElement dgvariables = result.Elements("dgvariables").FirstOrDefault();
                XElement vars = null;
                if (dgvariables != null)
                {
                    vars = dgvariables.Elements("variables").FirstOrDefault();
                    XElement userdatas = dgvariables.Elements("userdatas").FirstOrDefault();
                    XElement triggerdata = dgvariables.Elements("triggerdata").FirstOrDefault();

                    if (vars != null && userdatas != null)
                    {
                        (
                            from v in vars.Elements("variable")
                            join ud in userdatas.Elements("userdata")
                            on (v.Elements("id").Select(s => s.Value).FirstOrDefault() ?? "-0").ToLower() equals ((string)ud.Attributes("id").FirstOrDefault() ?? "-1").ToLower()
                            select new { var = v, userdata = ud }
                        ).ToList()
                        .ForEach(p => {
                            if (p.var.Attributes("changedate").FirstOrDefault() == null)
                                p.var.Add(new XAttribute("changedate", p.userdata.Attribute("changedate").Value));
                            else
                                p.var.Attributes("changedate").FirstOrDefault().Value = p.userdata.Attribute("changedate").Value;

                            if (p.var.Elements("value").FirstOrDefault() == null)
                                p.var.Add(new XElement("value", p.userdata.Value));
                            else
                                p.var.Elements("value").FirstOrDefault().Value = p.userdata.Value;

                        });

                        var c = from v in vars.Elements("variable")
                                    .Where(p => Regex.IsMatch(p.Elements("id").Select(s => s.Value).FirstOrDefault(), @"\*N\*", RegexOptions.IgnoreCase))
                                select new
                                {
                                    e = v,
                                    r = new Regex(Regex.Replace(v.Elements("id").Select(s => s.Value).FirstOrDefault(), @"\*N\*", @"\d+", RegexOptions.IgnoreCase))
                                };

                        foreach (var e in c)
                        {
                            var v = userdatas.Elements("userdata").Where(p => e.r.IsMatch((string)p.Attributes("id").FirstOrDefault()));
                            foreach (XElement ud in v)
                            {
                                e.e.Add(new XElement("variable",
                                    new XAttribute("changedate", (string)ud.Attributes("changedate").FirstOrDefault()),
                                    new XElement("id", (string)ud.Attributes("id").FirstOrDefault()),
                                    new XElement("value", ud.Value)
                                    )
                                );
                            }
                        }

                    }
                    if (vars != null && triggerdata != null)
                    {
                        vars = vars.InsertProffData(triggerdata);
                    }
                }
                else
                {
                    vars = result.Elements("variables").FirstOrDefault();
                }

                if (vars != null)
                {
                    List<XElement> nvar = vars
                        .Elements("variable").Where(p => Regex.IsMatch(p.Elements("id").Select(v => v.Value).FirstOrDefault(), @"\*N\*"))
                        .ToList();
                    nvar.ForEach(p => p.EvalNVariable());

                    variables = vars.Elements("variable").Select(p => new XVariable(p));
                }


                XElement xtriggers = result.Elements("x-triggers").FirstOrDefault();
                XElement xobjs = result.Elements("xobjects").FirstOrDefault();
                if (xobjs != null)
                {
                    (
                        from o in xobjs.Descendants("x-var")
                        join v in vars.Elements("variable").Where(p => (string)p.Attributes("counter").FirstOrDefault() == "true")
                        on (string)o.Attributes("id").FirstOrDefault() equals v.Elements("id").Select(p => p.Value).FirstOrDefault()
                        select o
                    )
                    .ToList().ForEach(p => p.SetAttributeValueEx("counter", "true"));
                    (
                        from o in xobjs.Descendants("x-var")
                        join v in xtriggers.Elements("x-trigger")
                        on ((string)o.Attributes("id").FirstOrDefault() ?? "-1").Trim().ToLower() equals ((string)v.Attributes("id").FirstOrDefault() ?? "-0").Trim().ToLower()
                        select o
                    )
                    .ToList()
                    .ForEach(p => p.SetAttributeValueEx("trigger", "true"));

                    xobjects = xobjs.Elements().Select(p => new XObj(p, vars));
                }
                XElement varvalues = result.Elements("varvalues").FirstOrDefault();
                XElement settings = result.Elements("settings").FirstOrDefault();
                XElement map = result.Elements("index").FirstOrDefault();
                XElement docpart = result.Elements("docpart").FirstOrDefault();
                XElement relation = result.Elements("x-relations").FirstOrDefault();

                AccountingObjectsAPI ao = new AccountingObjectsAPI(result.Elements("accroot").FirstOrDefault(), accountingType, accountingCodes, accountingTax);
                elementdata = ao.objectsList;
                objects = ao.objects;

                //AccRoot = new AccountingObjects(result.Elements("accroot").FirstOrDefault(), accountingType, accountingCodes, accountingTax).AccRoot;
                //XElement accounting = result.Elements("accroot").FirstOrDefault();

                XElement desc = result.Elements("topic").Elements("description").FirstOrDefault();
                XElement xmoreinfo = result.Elements("x-more-info").FirstOrDefault();
                name = (string)result.Elements("topic").Attributes("name").FirstOrDefault();
                //string topic_id = (string)result.Elements("topic").Attributes("topic_id").FirstOrDefault();

                XElement topics = result.Elements("topics").FirstOrDefault();
                XElement searchresult = result.Elements("searchresult").FirstOrDefault();

                XElement xlinkgroup = result.Elements("x-link-group").FirstOrDefault();
                XElement xComments = result.Elements("x-cs").FirstOrDefault();
                XElement xMirrors = result.Elements("x-mirrors").FirstOrDefault();
                if (map != null)
                {
                    if ((searchresult == null ? 0 : searchresult.Elements("item").Count()) != 0)
                    {

                        mark = searchresult.Elements("sw")
                            .Elements("w")
                            .Select(p => new SearchWords(p))
                            .ToList();

                        (
                            from m in map.Descendants("item")
                            join s in searchresult.Elements("item")
                            on (string)m.Attributes("id").FirstOrDefault() equals (string)s.Attributes("id").FirstOrDefault()
                            select new { me = m, se = s }
                        )
                        .ToList()
                        .ForEach(p => p.me.AddAnnotation(new DocumentIntemScore(p.se)));
                       
                    }

                }

                string viewstyle = (result.Elements("settings").Elements("viewstyle").Elements("main").Select(p => p.Value).FirstOrDefault() ?? "").ToLower();

                if (docpart != null && viewstyle != null)
                {
                    string lookup = docpart.Descendants("x-settings").Elements("companylookup").Select(p => p.Value).FirstOrDefault();
                    companylookup = false;
                    if (vars != null)
                    {
                        if (lookup == "off")
                            companylookup = false;
                        else
                            companylookup = true;
                    }

                    if (docpart.Attributes("id").FirstOrDefault() == null)
                    {
                        docpart.Add(new XAttribute("id", topic_id));
                    }
                    else
                    {
                        docpart.Attributes("id").FirstOrDefault().SetValue(topic_id);
                    }

                    switch (viewstyle)
                    {
                        case "11":
                            {
                                XElement docparthtml = docpart.ConvertXMLtoHTML(ao.aobjects, xmoreinfo, topics, xlinkgroup);
                                //TocJson tocJson = new TocJson(map);
                                TocJson tocJson = new TocJson(map, docparthtml, topic_id, segmentId);
                                tocroot = tocJson.tocroot;
                                toc = tocJson.toc;

                                string document_id = "document;" + topic_id + ";" + segmentId;
                                //JsonObject htmlJson = new JsonObject(docpart.Descendants("document").Elements());
                                root = new List<JsonChild>();
                                root.Add(new JsonChild { id = document_id });
                                elements = new Dictionary<string, JsonElement>();
                                elements.Add(document_id,
                                    new JsonElement
                                    {
                                        name = "div",
                                        attributes = new Dictionary<string, string>() { { "class", "doccontainer" } },
                                        children = docparthtml.Elements().Select(p => new JsonChild { id = (string)p.Attributes("id").FirstOrDefault() }).ToList()
                                    }
                                );

                                elements.AddRange(
                                    docparthtml
                                    .Descendants()
                                    .Select(p => p)
                                    .ToDictionary(p => (string)p.Attributes("id").FirstOrDefault(), p => new JsonElement(p))
                                );

                                if (ao.aobjects != null)
                                {
                                    (
                                        from e in elements
                                        join a in ao.aobjects
                                        on e.Key.ToLower() equals a.ToLower()
                                        select e
                                    ).ToList()
                                    .ForEach(p => p.Value.otherprops = new Dictionary<string, string>() { { "accounting", "true" } } );
                                }
                            }
                            break;
                            //document = docpart.ConvertHTML(accounting, xmoreinfo, topics, xlinkgroup, xComments, xMirrors); break;
                    }
                }

                if (desc != null)
                {
                    if ((viewstyle == null ? "" : viewstyle) == "lovdata.xsl")
                    {
                        desc.Descendants().Where(p => "diblink;dibparameter".Split(';').Contains(p.Name.LocalName)).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
                    }
                    description = desc.ConvertHTML(null, null, null, xlinkgroup);
                }

                if (relation != null) relations = relation.GetResultSet();

            }
        }


    }
    public class SearchWords
    {
        public string word { get; set; }
        public List<string> extensions { get; set; }
        public SearchWords(XElement w)
        {
            word = w.Nodes().OfType<XText>().Select(s => s.Value).StringConcatenate();
            extensions = w.Elements("ws").Select(p => p.Value).ToList();

        }
    }
    public class DocumentSearch
    {
        public Dictionary<string, DocumentIntemScore> searchresult { get; set; }
        //public List<string> mark { get; set; }
        public List<SearchWords> mark { get; set; }
        public DocumentSearch(XElement result)
        {
            mark = result.Elements("sw")
                .Elements("w")
                .Select(p => new SearchWords(p))
                .ToList();

            searchresult = result
                .Elements("item")
                 .Select(p => new KeyValuePair<string, DocumentIntemScore>(
                        (string)p.Attributes("id").FirstOrDefault(),
                        new DocumentIntemScore(p)
                    ))
                    .ToDictionary(p => p.Key, p => p.Value);
        }
    }
    public class DocumentIntemScore
    {
        public string id { get; set; }
        public int hitcount { get; set; }
        public int score { get; set; }
        public int max { get; set; }
        //public Dictionary<string, DocumentIntemScore> hits { get; set; }
        public List<DocumentIntemScore> hits { get; set; }
        public DocumentIntemScore(XElement item)
        {
            id = (string)item.Attributes("id").FirstOrDefault();
            hitcount = Convert.ToInt32((string)item.Attributes("n").FirstOrDefault() ?? "0"); //  item.NodesBeforeSelf().OfType<XElement>().Count();
            max = Convert.ToInt32((string)item.Attributes("max").FirstOrDefault() ?? "0");
            score = Convert.ToInt32((string)item.Attributes("score").FirstOrDefault() ?? "0");
            if (item.Elements("item").Count() > 0)
            {
                hits = item
                    .Elements("item")
                     .Select(p => new DocumentIntemScore(p))
                     .ToList();
            }
        }
    }
    public class DocumentTOC
    {
        public string title { get; set; }
        public string id { get; set; }
        public bool segment { get; set; }
        public string segmentId { get; set; }
        public int hitcount { get; set; }
        public int score { get; set; }
        public int max { get; set; }
        public IEnumerable<DocumentTOC> children { get; set; }
        //public Dictionary<string, DocumentIntemScore> hits { get; set; }
        public List<DocumentIntemScore> hits { get; set; }
        public DocumentTOC(XElement item, string segment_id)
        {
            DocumentIntemScore ds = item.Annotation<DocumentIntemScore>();
            title = (string)item.Attributes("title").FirstOrDefault();
            if (title == null) title = "Ingen tittel";
            //if (item.Attributes("n").FirstOrDefault() != null)
            //    hitcount = Convert.ToInt32((string)item.Attributes("n").FirstOrDefault()); //  item.NodesBeforeSelf().OfType<XElement>().Count();
            //if (item.Attributes("max").FirstOrDefault() != null)
            //    max = Convert.ToInt32((string)item.Attributes("max").FirstOrDefault());
            //if (item.Attributes("score").FirstOrDefault() != null)
            //    score = Convert.ToInt32((string)item.Attributes("score").FirstOrDefault());
            if (ds != null)
            {
                hitcount = ds.hitcount; //  item.NodesBeforeSelf().OfType<XElement>().Count();
                max = ds.max;
                score = ds.score;
                hits = ds.hits;
            }

            id = (string)item.Attributes("id").FirstOrDefault() ?? "-1";
            segmentId = segment_id;

            if ((string)item.Attributes("s").FirstOrDefault() == "1" || segment_id == id)
            {
                segment = true;
            }
            if (item.Elements("item").Count() != 0)
            {
                children = item.Elements("item").Select(p => new DocumentTOC(p, segment_id));
            }
        }
        public DocumentTOC(XElement item)
        {
            DocumentIntemScore ds = item.Annotation<DocumentIntemScore>();
            title = (string)item.Attributes("title").FirstOrDefault();
            if (title == null) title = "Ingen tittel";
            if (ds != null)
            {
                hitcount = ds.hitcount; //  item.NodesBeforeSelf().OfType<XElement>().Count();
                max = ds.max;
                score = ds.score;
                hits = ds.hits;
            }
            //if (item.Attributes("n").FirstOrDefault() != null) 
            //    hitcount = Convert.ToInt32((string)item.Attributes("n").FirstOrDefault()); //  item.NodesBeforeSelf().OfType<XElement>().Count();
            //if (item.Attributes("max").FirstOrDefault() != null)
            //    max = Convert.ToInt32((string)item.Attributes("max").FirstOrDefault());
            //if (item.Attributes("score").FirstOrDefault() != null)
            //    score = Convert.ToInt32((string)item.Attributes("score").FirstOrDefault());

            if ((string)item.Attributes("s").FirstOrDefault() == "1")
            {
                segment = true;
            }
            id = (string)item.Attributes("id").FirstOrDefault() ?? "-1";
            if (item.Elements("item").Count() != 0)
            {
                children = item.Elements("item").Select(p => new DocumentTOC(p));
            }
        }
    }
    public class ParentsItem
    {
        public string name { get; set; }
        public string id { get; set; }
        public string segmentId { get; set; }
    }

}
