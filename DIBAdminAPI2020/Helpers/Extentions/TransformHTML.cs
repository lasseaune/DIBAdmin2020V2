using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using DIBAdminAPI.Data.Entities;

namespace DIBAdminAPI.Helpers.Extentions
{
    public static class TransformHTML
    {
        private class MirrorItem
        {
            public XElement mirrors { get; set; }
            public MirrorItem(XElement m)
            {
                m.DescendantsAndSelf().ToList().ForEach(p => p.SetAttributeValueEx("id", Guid.NewGuid().ToString()));
                mirrors = m;
            }
        }
        private class MirrorsItem
        {
            public XElement mirrors { get; set; }
            public MirrorsItem(XElement ms)
            {
                mirrors = ms;
            }
        }
        private class CommentsItem
        {
            public XElement comments { get; set; }
            public CommentsItem(XElement cs)
            {
                cs.Descendants().ToList().ForEach(p => p.SetAttributeValueEx("id", Guid.NewGuid().ToString()));
                comments = new XElement(cs.Name.LocalName,
                    new XAttribute("id", "cs;" + (string)cs.Attributes("id").FirstOrDefault()),
                    cs.Nodes()
                    );
            }
        }
        private class Accounting
        {
            public string rid { get; set; }
            public Accounting(string id)
            {
                rid = id;  
            }
        }
        private class AccountingItem
        {
            public XElement element { get; set; }

            public AccountingItem(XElement ac)
            {
                ac.Descendants().ToList().ForEach(p => p.SetAttributeValueEx("id", Guid.NewGuid().ToString()));
                element = new XElement(ac.Name.LocalName,
                    new XAttribute("id", "acc;" + (string)ac.Attributes("id").FirstOrDefault()),
                    ac.Nodes()
                    );
            }
        }
        private class VariableInfo
        {
            public DibVariable v { get; set; }
            public DibTrigger t { get; set; }
            public  VariableInfo(DibVariable variable, DibTrigger trigger)
            {
                if (variable == null )
                {

                }
                v = variable;
                t = trigger;
            }
        }
        private class MoreinfoItem
        {
            public bool hasRelations { get; set; }
        }
        private static void AddComment(this XElement item, XElement cs)
        {
            List<string> vals = new List<string>();
            if (cs != null)
            {
                vals = cs.Element("x-c").Attributes("type").Select(p => p.Value).ToList();
            }

            if ((string)item.Attributes("data-comment").FirstOrDefault() == null)
            {
                item.Add(new XAttribute("data-comment", "-1"));
                XElement xcs = new XElement("x-cs", vals.Select(p => new XElement("x-c", new XAttribute("type", p))));
                item.AddAnnotation(new CommentsItem(xcs));
            }
            else
            {
                item.Attributes("data-comment").FirstOrDefault().SetValue("-1");
                vals.Add("0");
                XElement xcs = new XElement("x-cs", vals.Select(p => new XElement("x-c", new XAttribute("type", p))));
                item.AddAnnotation(new CommentsItem(xcs));
            }
        }
        public static XElement ConvertXMLtoHTML5(this XElement document, IEnumerable<LinkData> linkDatas, IEnumerable<DibVariable> variables, IEnumerable<DibTrigger> trigger )
        {
            try
            {
                if (variables.Select(p => p.id).FirstOrDefault() != null)
                {
                    (
                        from x in document.Descendants("x-var").Reverse()
                        join v in variables
                        on ((string)x.Attributes("id").FirstOrDefault() ?? "").Trim() equals v.varId.Trim()
                        select new { x, v }
                    ).ToList()
                    .ForEach(p => p.x.AddAnnotation(new VariableInfo(p.v, trigger.Where(t => (t.name == null ? false : t.name.Trim().ToLower() == p.v.name.Trim().ToLower())).Select(t => t).FirstOrDefault())));

                    //.ForEach(p => p.x.ReplaceWith(
                    //        new XElement("span",
                    //                new XAttribute("id", Guid.NewGuid().ToString()),
                    //                new XAttribute("class", "dib-var" + (p.v.n == 1 ? " -nvar" : "") + (trigger.Where(t => (t.name == null ? false : t.name.Trim().ToLower() == p.v.name.Trim().ToLower())).Count() > 0 ? "" : " -trig")),
                    //                new XAttribute("data-var-id", p.v.varId),
                    //                (string)p.x.Attributes("class").FirstOrDefault() == null ? null : new XAttribute("data-var-area", (string)p.x.Attributes("class").FirstOrDefault()),
                    //                new XAttribute("data-var-text", p.v.name),
                    //                new XText(p.v.name)
                    //        )

                    //    )
                    //);

                }


                document.DescendantsAndSelf().Attributes("idx").Remove();
                
                (
                    from e in document.Descendants()
                    join l in linkDatas
                    on ((string)e.Attributes("id").FirstOrDefault() ?? "").Trim().ToLower() equals l.id.Trim().ToLower()
                    select new { e, l }
                )
                .ToList()
                .ForEach(p => p.e.AddAnnotation(new LinkInfo(p.l)));

                XElement xElement = new XElement("document", document.Transform());
                xElement.Descendants().Where(p => (string)p.Attributes("class").FirstOrDefault() != null).ToList().ForEach(p => p.SetAttributeValueEx("data-class", (string)p.Attributes("class").FirstOrDefault()));

                //XElement first = xElement.Descendants("section").Where(p => ((string)p.Attributes("class").FirstOrDefault() ?? "").Split(" ").Contains("-autocount")).FirstOrDefault();
                //if (first!=null)
                //{
                //    int n = first.Ancestors().Count();
                //    while (first != null)
                //    {
                //        AutoCountProps autoCountProps = new AutoCountProps();
                //        autoCountProps.active = true;
                //        autoCountProps.autocount = true;
                //        autoCountProps.id = (string)first.Attributes("id").FirstOrDefault();
                //        autoCountProps.type = (string)first.Attributes("data-type").FirstOrDefault() ?? "decimal";
                //        first.SetAutoCount(autoCountProps);
                //        first.NodesBeforeSelf().OfType<XElement>().Where(p => p.Name.LocalName == "section").ToList().ForEach(p => p.SetAutoCount(autoCountProps));
                //        first.NodesAfterSelf().OfType<XElement>().Where(p => p.Name.LocalName == "section").ToList().ForEach(p => p.SetAutoCount(autoCountProps));

                //        first = xElement.Descendants("section").Where(p => p.Ancestors().Count() > n && ((string)p.Attributes("class").FirstOrDefault() ?? "").Split(" ").Contains("-autocount")).FirstOrDefault();
                //        if (first != null)
                //        {
                //            n = first.Ancestors().Count();
                //        }

                //    }
                //}
                
                return xElement;
            }
            catch (SystemException e)
            {
                return null;
            }
        }
        //public static XElement ConvertXMLtoHTML5(this XElement document, XElement links)
        //{
        //    document.DescendantsAndSelf().Attributes("idx").Remove();
        //    (
        //        from e in document.Descendants()
        //        join l in links.Elements()
        //        on ((string)e.Attributes("id").FirstOrDefault() ?? "").Trim().ToLower() equals ((string)l.Attributes("id").FirstOrDefault() ?? "").Trim().ToLower()
        //        select new { e, l }
        //    )
        //    .ToList()
        //    .ForEach(p=> p.e.AddAnnotation(new LinkInfo(p.l)));


        //    return new XElement("document", document.Transform());
        //}
        //public static XElement ConvertXMLtoHTML(this XElement document, List<string> aobjects = null, XElement moreinfo = null, XElement topics = null, XElement xlinkgroup = null, XElement comments = null, XElement mirrors = null)
        //{
        //    document.DescendantsAndSelf().Attributes("idx").Remove();
        //    if (mirrors != null)
        //    {
        //        (
        //            from i in document.Descendants("section")
        //            join m in mirrors.Elements("x-mirror")
        //            on (string)i.Attributes("id").FirstOrDefault() equals (string)m.Attributes("id").FirstOrDefault()
        //            select new { item = i, mirror = m }
        //        )
        //        .ToList()
        //        .ForEach(p => p.item.AddAnnotation(new MirrorItem(p.mirror)));
        //    }

        //    if (comments != null)
        //    {
        //        (
        //            from i in document.DescendantsAndSelf().Where(p => p.Attributes("id").FirstOrDefault() != null)
        //            join cm in comments.Elements("x-c")
        //            on (string)i.Attributes("id").FirstOrDefault() equals (string)cm.Attributes("id").FirstOrDefault()
        //            select new { item = i, Comment = cm }
        //        ).ToList()
        //        .GroupBy(p => p.item)
        //        .ToList()
        //        .ForEach(p => p.Key.AddComment(new XElement("x-cs", p.Select(s => s.Comment))));
        //    }
        //    document.Descendants().Where(p => (string)p.Attributes("data-comment").FirstOrDefault() == "0").ToList().ForEach(p => p.AddComment(null));

        //    if (xlinkgroup != null)
        //    {
        //        document.AddLinkInfo(xlinkgroup);
        //    }
        //    //if (AccRoot != null)
        //    //{
        //    //    (
        //    //        from a in AccRoot
        //    //        join e in document.Descendants()
        //    //        on a.Key.ToLower() equals ((string)e.Attributes("id").FirstOrDefault() ?? "").ToLower()
        //    //        select new { a, e }
        //    //    )
        //    //    .ToList()
        //    //    .ForEach(p => p.e.AddAnnotation(new Accounting(p.a.Key)));

        //    //}
        //    //if (accounting != null )
        //    //{
        //    //    (
        //    //        from t in accounting.Elements("topics").Elements("topic")
        //    //        join a in accounting.Elements("x-accounting").Descendants().Where(p => p.Attributes("data-tid").FirstOrDefault() != null)
        //    //        on (string)t.Attributes("topic_id").FirstOrDefault() equals (string)a.Attributes("data-tid").FirstOrDefault()
        //    //        select new { topic = t, xa = a }
        //    //    )
        //    //    .ToList()
        //    //    .ForEach(p => p.xa.Add(new XAttribute("data-tname", (string)p.topic.Attributes("name").FirstOrDefault()), new XAttribute("data-view", (string)p.topic.Attributes("view").FirstOrDefault()), new XAttribute("data-ttype", (string)p.topic.Attributes("tid").FirstOrDefault())));

        //    //    (
        //    //        from ai in accounting.Elements("x-accounting")
        //    //        join i in document.Descendants("section")
        //    //        on (string)ai.Attributes("id").FirstOrDefault() equals (string)i.Attributes("id").FirstOrDefault()
        //    //        select new { acc = ai, item = i }
        //    //    )
        //    //    .ToList()
        //    //    .ForEach(p => p.item.AddAnnotation(new AccountingItem(p.acc)));
        //    //}

        //    if (moreinfo != null)
        //    {
        //        (
        //            from ai in moreinfo.Elements("item")
        //            join i in document.Descendants().Where(p => p.Name.LocalName == "section")
        //            on ((string)ai.Attributes("id").FirstOrDefault() ?? "-1").ToLower() equals ((string)i.Attributes("id").FirstOrDefault() ?? "1").ToLower()
        //            select new { acc = ai, item = i }
        //        )
        //        .ToList()
        //        .ForEach(p => p.item.AddAnnotation(new MoreinfoItem { hasRelations = true }));
        //    }
        //    return new XElement("document", document.Transform());
            
        //}
        //public static string ConvertHTML(this XElement document, List<string> aobjects = null, XElement moreinfo = null, XElement topics = null, XElement xlinkgroup = null, XElement comments = null, XElement mirrors = null)
        //{
        //    XElement documentHTML = document.ConvertXMLtoHTML(aobjects, moreinfo, topics, xlinkgroup, comments, mirrors);
        //    XElement c = new XElement("container", documentHTML.Nodes());
        //    XmlReader r = c.CreateReader();
        //    r.MoveToContent();
        //    return r.ReadInnerXml();
        //}
        private static IEnumerable<XNode> Transform(this XNode n)
        {
            List<XNode> result = new List<XNode>();
            try
            {
                if (n.NodeType == XmlNodeType.Text) result.Add(n);
                if (n.NodeType == XmlNodeType.Element)
                {
                    XElement e = ((XElement)n);
                    switch (e.Name.LocalName)
                    {
                        //satt inn
                        case "x-comment":
                            { 
                                if (e.DescendantNodesAndSelf().OfType<XText>().Select(s=>s.Value).StringConcatenate().Trim()!="" || ((string)e.Attributes("title").FirstOrDefault()??"").Trim()!="")
                                {
                                    result.Add(new XElement("aside",
                                        new XAttribute("id", Guid.NewGuid().ToString()),
                                        new XAttribute("class", "dib-x-comment"),
                                        //new XAttribute("data-gen-object", true),
                                        ((string)e.Attributes("title").FirstOrDefault() ?? "").Trim() != "" ? new XElement("em", new XAttribute("class","dib-x-comment-tag"), new XText(((string)e.Attributes("title").FirstOrDefault() ?? "").Trim())) : null,
                                        new XElement("p",
                                            new XAttribute("id", Guid.NewGuid().ToString()),
                                            e.Nodes().SelectMany(p => p.Transform())
                                        )
                                        )
                                    );
                                }
                            }

                            break;
                        case "x-alternative":
                            //x-alternative title="Redusere pålydende" n="1" id="x45"
                            result.Add(new XElement("section",
                                new XAttribute("id", Guid.NewGuid().ToString()),
                                new XAttribute("class", "dib-x-alternative"),
                                //new XAttribute("data-gen-object", true),
                                new XAttribute("data-var-value", (string)e.Attributes("n").FirstOrDefault()),
                                new XAttribute("data-var-name", (string)e.Attributes("title").FirstOrDefault()),
                                e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "x-alternatives":
                            //x-alternatives id="1_Alternative"
                            result.Add(new XElement("section",
                                new XAttribute("id", Guid.NewGuid().ToString()),
                                new XAttribute("class", "dib-x-alternatives"),
                                //new XAttribute("data-gen-object", true),
                                new XAttribute("data-var-id", (string)e.Attributes("id").FirstOrDefault()),
                                e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "x-letterhead":
                            result.Add(new XElement("section",
                                new XAttribute("id", Guid.NewGuid().ToString()),
                                new XAttribute("class", "dib-x-letterhead"),
                                //new XAttribute("data-gen-object", true),
                                e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "x-list":
                            //header="Antall øvrige styremedlemmer i Hjelpeselskapet:" varname="Antall-S---S-øvrige-S-styremedlemmer-S-i-S-Hjelpeselskapet" defaultcounter="0" oftype="cell-line"
                            result.Add(new XElement("section",
                                new XAttribute("id", Guid.NewGuid().ToString()),
                                new XAttribute("class", "dib-x-list"),
                                //new XAttribute("data-gen-object", true),
                                new XAttribute("data-var-oftype", (string)e.Attributes("oftype").FirstOrDefault()),
                                new XAttribute("data-var-id", (string)e.Attributes("varname").FirstOrDefault()),
                                new XAttribute("data-var-header", (string)e.Attributes("header").FirstOrDefault()),
                                new XAttribute("data-default-counter", (string)e.Attributes("defaultcounter").FirstOrDefault()),
                                e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "x-optional":
                            //keyword="IAS 23" id="IAS - S - 23_FreeElement"
                            result.Add(new XElement("span",
                                new XAttribute("id", Guid.NewGuid().ToString()),
                                new XAttribute("class", "dib-x-optional"),
                                //new XAttribute("data-gen-object", true),
                                new XAttribute("data-var-id", (string)e.Attributes("id").FirstOrDefault()),
                                new XAttribute("data-var-keyword", (string)e.Attributes("keyword").FirstOrDefault()),
                                e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "x-var":
                            {
                                VariableInfo vi = e.Annotations<VariableInfo>().FirstOrDefault();
                                result.Add(new XElement("span",
                                    new XAttribute("id", Guid.NewGuid().ToString()),
                                    new XAttribute("class", "dib-x-var"
                                                        + (vi.v.n == 1 ? " -nvar" : "")
                                                        + ((vi.t == null ? false : vi.t.trigName.Trim().ToLower() != vi.v.name.Trim().ToLower() && vi.t.name.Trim().ToLower() == vi.v.name.Trim().ToLower()) ? "" : " -trigby")
                                                        + ((vi.t == null ? false : vi.t.trigName.Trim().ToLower() == vi.v.name.Trim().ToLower()) ? "" : " -trig")
                                                        ),
                                    (vi.t == null ? false : vi.t.trigName.Trim().ToLower() != vi.v.name.Trim().ToLower() && vi.t.name.Trim().ToLower() == vi.v.name.Trim().ToLower()) ? new XAttribute("data-trigby", vi.t.trigName) : null,
                                    new XAttribute("data-var-id", vi.v.id.ToString().ToLower()),
                                    //new XAttribute("data-var-object", true),
                                    //(string)e.Attributes("class").FirstOrDefault() == null ? null : new XAttribute("data-var-area", (string)e.Attributes("class").FirstOrDefault()),
                                    new XAttribute("data-var-text", vi.v.name),
                                    new XText(vi.v.name)
                                    )
                                );
                                
                                
                            }
                            break;
                        
                        case "docpart":
                        case "document": result.AddRange(e.Document()); break;
                        case "h1":
                        case "h2":
                        case "h3":
                        case "h4":
                        case "h5":
                        case "h6":
                        case "h7":
                        case "h8":
                        case "h9":
                        case "h10":
                        case "h11":
                        case "h12":
                        case "h13":
                        case "h14":
                            result.AddRange(e.h()); break;
                        case "x-box": result.AddRange(e.xbox()); break;
                        case "diblink":
                            result.Add(new XElement("a",
                                 new XAttribute("id", Guid.NewGuid().ToString()),
                                 new XAttribute("class", "diblink"),
                                 new XAttribute("data-id", (string)e.Attributes("rid").FirstOrDefault()),   
                                 e.Nodes().SelectMany(p => p.Transform())
                                 )
                            );
                            break;
                        case "dibparameter":
                            result.Add(new XElement("a",
                                 new XAttribute("id", Guid.NewGuid().ToString()),
                                 new XAttribute("class", "diblink"),
                                 new XAttribute("data-id", (string)e.Attributes("rid").FirstOrDefault()),
                                 new XText((string)e.Attributes("replaceText").FirstOrDefault())
                                 )
                            );
                            break;
                        case "a": result.AddRange(e.a()); break;
                        case "img": result.AddRange(e.img()); break;
                        case "section":
                            result.AddRange(e.Section()); break;
                        case "table": result.AddRange(e.Table()); break;
                        //case "x-acc": result.AddRange(e.XAcc()); break;
                        case "x-index": result.AddRange(e.XIndex()); break;
                        case "x-link-to": result.AddRange(e.XLinkTo()); break;
                        case "item": result.AddRange(e.item()); break;
                        default: result.AddRange(e.Default()); break;
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                result.Add(new XElement("p", "Transformation error:" + e.Message));
                return result;
            }
        }
        #region //ElementStyle
        //private static IEnumerable<XNode> XAcc(this XElement e)
        //{

        //    List<XNode> result = new List<XNode>();
        //    Accounting acci = e.Annotations<Accounting>().FirstOrDefault();
        //    if (acci != null)
        //    {
        //        result.Add(new XElement(e.Name.LocalName, e.Attributes("id"), new XAttribute("aid", acci.rid)));
        //    }

        //    return result;
        //}
        private static IEnumerable<XNode> item(this XElement e)
        {

            List<XNode> result = new List<XNode>();
            int n = e.AncestorsAndSelf("item").Count();
            if (n > 6)
                n = 6;
            result.Add(new XElement("section",
                e.Attributes("id"),
                e.Attributes("type").FirstOrDefault()==null ? null : new XAttribute("data-type", (string)e.Attributes("type").FirstOrDefault()),
                new XAttribute("class", "check-item"),
                (
                    e.Nodes().OfType<XElement>().Where(p=>p.Name.LocalName=="ititle").DescendantNodes().OfType<XText>().Select(p=>p.Value).StringConcatenate().Trim() !="" 
                    ? new XElement("h"+ n.ToString(),
                        e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "ititle").Attributes("id").FirstOrDefault(),
                        new XAttribute("class", "check-title"),
                        e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "ititle").Nodes().SelectMany(p => p.Transform()))
                    : new XElement("h" + n.ToString(),
                            new XAttribute("id", Guid.NewGuid().ToString()),
                            new XAttribute("class", "check-title"),
                            new XText(((string)e.Attributes("type").FirstOrDefault()??"2") == "2" ? "[Infopunkt]" : "[Uten tittel]")
                      )
                ),
                new XElement("section",
                    new XAttribute("class", "check-ingress"),
                    (
                        e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "ingress").Attributes("id").FirstOrDefault() == null
                        ? new XAttribute("id", Guid.NewGuid().ToString())
                        : e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "ingress").Attributes("id").FirstOrDefault()
                    ),
                    e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "ingress").Nodes().SelectMany(p => p.Transform())
                ),
                new XElement("section",
                    new XAttribute("class", "check-description"),
                    (
                        e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "description").Attributes("id").FirstOrDefault() == null
                        ? new XAttribute("id", Guid.NewGuid().ToString())
                        : e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "description").Attributes("id").FirstOrDefault()
                    ),
                    e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "description").Nodes().SelectMany(p => p.Transform())
                ),
                new XElement("section",
                    new XAttribute("class", "check-law"),
                    (
                        e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "law").Attributes("id").FirstOrDefault()==null
                        ? new XAttribute("id", Guid.NewGuid().ToString())
                        : e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "law").Attributes("id").FirstOrDefault()
                    ),
                    e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "law").Nodes().SelectMany(p => p.Transform())
                ),
                new XElement("section",
                    new XAttribute("id", Guid.NewGuid().ToString()),
                    new XAttribute("class", "check-children"),
                    e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "item").SelectMany(p => p.Transform())
                    )
                )
                //e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "item").SelectMany(p => p.Transform()))

            );

            return result;
        }
        private static IEnumerable<XNode> XLinkTo(this XElement e)
        {

            List<XNode> result = new List<XNode>();
            LinkInfo li = e.Annotations<LinkInfo>().FirstOrDefault();
            string Value = "";
            string Id = "";
            if (li != null && e.Ancestors().Where(p=>Regex.IsMatch(p.Name.LocalName,@"h\d+")).Count()==0)
            {
                if (li.rName == "link")
                {
                    Value = li.rText.Split(';').ElementAt(3).Split('#').FirstOrDefault() ?? li.dName;
                    Id = li.rText.Split(';').ElementAt(2)??"";
                    result.Add(new XElement("a",
                        e.Attributes("id"),
                        (li.rTag2 ?? "") == "" ? new XAttribute("class", "diblink-nav") : new XAttribute("class", "diblink-preview"),
                        new XAttribute("data-resource-id", li.dId),
                        (li.rTag2 ?? "") == "" ? null : new XAttribute("data-id", li.rTag2),
                        new XText(Value)
                        )
                    );  ;
                    return result;
                }
                else if (li.rName == "dibid")
                {
                    Value = li.dName;
                    result.Add(new XElement("a",
                        e.Attributes("id"),
                        new XAttribute("class", "diblink-nav"),
                        new XAttribute("data-resource-id", li.dId),
                        new XText(Value)
                        )
                    );
                    return result;
                }
                else if ((li.rTag2==null?"": li.rTag2)=="")
                {
                    Value = li.dName;
                    result.Add(new XElement("a",
                        e.Attributes("id"),
                        new XAttribute("class", "diblink-nav"),
                        new XAttribute("data-resource-id", li.dId),
                        e.Nodes()
                        )
                    );
                    return result;
                }
                else
                {
                    result.Add(new XElement("a",
                        e.Attributes("id"),
                        new XAttribute("class", "diblink-preview"),
                        new XAttribute("data-resource-id", li.dId),
                        new XAttribute("data-segment-id", "diblink"),
                        new XAttribute("data-id", li.Id),
                        e.Nodes()
                        )
                    );
                    return result;
                }
            }
            else
            {
                result.Add(new XElement("span", new XAttribute("class", "x-link-to"),  e.Attributes("id"), e.Nodes()));
                return result;
            }

        
        }
        private static IEnumerable<XNode> XIndex(this XElement e)
        {

            List<XNode> result = new List<XNode>();
            
            
            result.Add(new XElement("span", new XAttribute("class", "x-index"), e.Attributes("id"), e.Nodes()));
            

            return result;
        }
        private static IEnumerable<XNode> Table(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (e.Parent.Name.LocalName + ((string)e.Parent.Attributes("class").FirstOrDefault() ?? "") != "divtable-wrapper")
            {
                result.Add(new XElement("div",
                        new XAttribute("id", Guid.NewGuid().ToString()),
                        new XAttribute("class", "table-wrapper"),
                        new XElement(e.Name.LocalName,
                        e.Attributes(),
                        e.Nodes().SelectMany(p => p.Transform())
                        )
                    )
                );
            }
            else
            {
                result.Add(new XElement(e.Name.LocalName,
                    e.Attributes(),
                    e.Nodes().SelectMany(p => p.Transform())
                    )
                );
            }
            return result;
        }
        private static IEnumerable<XNode> xbox(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("figure",
                e.Attributes("class"),
                e.Attributes("id"),
                e.Elements("x-box-title").FirstOrDefault() == null ? null : new XElement("figcaption", e.Element("x-box-title").Attributes("id"), e.Element("x-box-title").Nodes()),
                new XElement("div",
                    new XAttribute("id", Guid.NewGuid().ToString()),
                    e.Nodes().Where(p=>(p.NodeType==XmlNodeType.Element ? ((XElement)p).Name.LocalName : "")!= "x-box-title")
                    )
                )
            );
            return result;
        }
        private static IEnumerable<XNode> h(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (e == null) return result;
            string name = e.Name.LocalName.ToLower();
            switch (name)
            {
                case "h7":
                case "h8":
                case "h9":
                case "h10":
                case "h11":
                case "h12":
                case "h13":
                case "h14":
                    name = "h6"; break;
            }

            result.Add(new XElement(name,
                   e.Attributes(),
                   e.Nodes().SelectMany(p => p.Transform())
                   )
            );
            CommentsItem cmi = e.Parent.Annotations<CommentsItem>().FirstOrDefault();
            if (cmi != null)
            {
                result.Add(cmi.comments);
            }
            MirrorItem m = e.Parent.Annotations<MirrorItem>().FirstOrDefault();
            if (m != null)
            {
                result.Add(m.mirrors);
            }
            //Accounting acci = e.Parent.Annotations<Accounting>().FirstOrDefault();
            //if (acci != null)
            //{
            //    result.Add(new XElement("x-acc", new XAttribute("id", Guid.NewGuid().ToString()),  new XAttribute("aid", acci.rid)));
            //}

            return result;
        }
        private static IEnumerable<XNode> img(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (e == null) return result;
            string src = ((string)e.Attributes("src").FirstOrDefault() ?? "").Trim();
            if (src == "") return result;
            if (!Regex.IsMatch(src.Trim().ToLower(), @"(\/)?dibimages/uploads"))
            {
                src = src.Split('/').LastOrDefault();
                if (src == "") return result;
                src = @"dibimages/" + (string)e.Ancestors().Attributes("topic_id").LastOrDefault() + @"/" + src;
            }

            result.Add(new XElement("figure",
                    new XAttribute("id", Guid.NewGuid().ToString()),
                    new XAttribute("class","img-no-caption"),
                    new XElement("div",
                        new XAttribute("id", Guid.NewGuid().ToString()),
                        new XElement(e.Name.LocalName.ToString(),
                            new XAttribute("src", src),
                            e.Attributes().Where(p => p.Name.LocalName != "src"),
                            e.Nodes().SelectMany(p => p.Transform())
                        )
                    )
                )
            );

            return result;
        }
        private static IEnumerable<XNode> Document(this XElement e)
        {
            return e.Nodes().SelectMany(p => p.Transform());
        }
        private static IEnumerable<XNode> a(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if ((string)e.Attributes("class").FirstOrDefault() == "diblink")
            {
                TopicInfo ti = e.Annotation<TopicInfo>();
                if (ti != null)
                {
                    if ((ti.type == null ? "" : ti.type) == "link")
                    {
                        if ((ti.bm == null ? "" : ti.bm) == "")
                        {
                            result.Add(new XElement("a",
                                    e.Attributes("id"),
                                    new XAttribute("class", "diblink topic"),
                                    e.Attributes("href"),
                                    new XAttribute("data-topictitle", ti.title),
                                    new XAttribute("data-topictype", ti.topictype),
                                    new XAttribute("data-resourceid", Regex.Replace(ti.id, @"[\{\}]", "")),
                                    ti.segment_id == "" ? null : new XAttribute("data-segmentid", ti.segment_id),
                                    new XAttribute("data-view", ti.view),
                                    new XAttribute("data-authorized", (ti.authorized == null ? "1" : ti.authorized)),
                                    e.Value
                                    )
                                );
                        }
                        else
                        {
                            result.Add(new XElement("a",
                                e.Attributes("id"),
                                new XAttribute("data-authorized", (ti.authorized == null ? "1" : ti.authorized)),
                                new XAttribute("class", "diblink"),
                                new XAttribute("data-id", (string)e.Attributes("data-refid").FirstOrDefault() ?? ""),
                                new XAttribute("data-resourceid", Regex.Replace(((string)e.Ancestors().Attributes("topic_id").FirstOrDefault() ?? ""), @"[\{\}]", "")),
                                new XAttribute("data-segmentid", "diblink"),
                                new XAttribute("data-id", Regex.Replace(((string)e.Attributes("href").FirstOrDefault() ?? ""), @"[\#]", "")),
                                e.Value
                                )
                            );

                        }
                    }
                    else if ((e.Value.Trim().StartsWith("«") && e.Value.Trim().EndsWith("»")) || ((string)e.Attributes("data-replacetext").FirstOrDefault() ?? "") != "")
                    {
                        result.Add(new XElement("a",
                                e.Attributes("id"),
                                new XAttribute("class", "link"),
                                e.Attributes("href"),
                                new XAttribute("data-topictitle", ti.title),
                                new XAttribute("data-topictype", ti.topictype),
                                new XAttribute("data-resourceid", Regex.Replace(ti.id, @"[\{\}]", "")),
                                new XAttribute("data-view", ti.view),
                                new XAttribute("data-language", ti.language),
                                new XAttribute("data-authorized", (ti.authorized == null ? "1" : ti.authorized)),
                                ti.title
                                )
                            );
                    }
                    else
                    {
                        result.Add(new XElement("a",
                            e.Attributes("id"),
                            new XAttribute("class", "link"),
                            e.Attributes("href"),
                            new XAttribute("data-topictitle", ti.title),
                            new XAttribute("data-topictype", ti.topictype),
                            new XAttribute("data-resourceid", Regex.Replace(ti.id, @"[\{\}]", "")),
                            new XAttribute("data-view", ti.view),
                            new XAttribute("data-language", ti.language),
                            new XAttribute("data-authorized", (ti.authorized == null ? "1" : ti.authorized)),
                            e.Value
                            )
                        );
                    }
                }
                else
                {
                    //<a class="diblink" data-refid="m319" data-replacetext="(#link; {812EE247-C54E-4CF2-AF0E-C64922506846};;Gaver til forretningsforbindelser;#)" href="#m319" id="ee279b63-a653-47a0-9903-89b3ef743509">Gaver til forretningsforbindelser</a>
                    //result.Add(e);
                    if (e.Attributes("data-refid").FirstOrDefault() != null)
                    {
                        result.Add(new XElement("a",
                                e.Attributes("id"),
                                new XAttribute("class", "diblink"),
                                new XAttribute("data-segmentid", "diblink"),
                                new XAttribute("data-id", (string)e.Attributes("data-refid").FirstOrDefault()??""),
                                new XAttribute("data-resourceid", Regex.Replace(((string)e.Ancestors().Attributes("topic_id").FirstOrDefault() ?? ""), @"[\{\}]", "")),
                                new XAttribute("data-authorized", "1"),
                                e.Value
                            )
                        );
                    }
                
                
                }
                return result;
            }

            if ((string)e.Attributes("class").FirstOrDefault() == "diblink topic")
            {
                result.Add(new XElement(e));
            }
            else if ((string)e.Attributes("class").FirstOrDefault() == "xref")
            {
                result.Add(new XElement("a",
                    e.Attributes("id"),
                    new XAttribute("class", "diblink"),
                    new XAttribute("href", "#tid=xref//id=" + ((string)e.Attributes("id").FirstOrDefault() ?? "")),
                    e.Nodes().SelectMany(p => p.Transform())
                    )
                );
            }
            else
            {
                result.Add(new XElement("a",
                    e.Attributes().Where(p => "class;id;alt;href;target".Split(';').Contains(p.Name.LocalName) || p.Name.LocalName.StartsWith("data-")),
                    e.Nodes().SelectMany(p => p.Transform())
                    )
                );
            }
            return result;
        }
        private static IEnumerable<XNode> Default(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (e.Name.LocalName == "p" && "(ny side);(new page)".Contains(e.DescendantNodes().OfType<XText>().Select(s=>s.Value).StringConcatenate().Trim().ToLower()))
            {
                result.Add(new XElement(e.Name.LocalName.ToString(),
                    e.Attributes("id"),
                    new XAttribute("class", "dib-x-pagebreak"),
                    e.Nodes().SelectMany(p => p.Transform())
                    )
                );
                return result;
            }
            if (e == null) return result;
            CommentsItem cmi = e.Annotations<CommentsItem>().FirstOrDefault();
            result.Add(new XElement(e.Name.LocalName.ToString(),
                    e.Attributes(),
                    cmi == null ? null : cmi.comments,
                    e.Nodes().SelectMany(p => p.Transform())
                    )
                );

            return result;
        }
        private static IEnumerable<XAttribute> GetSectionAttributes(this XElement e)
        {
            List<XAttribute> result = new List<XAttribute>();
            string dataType = (string)e.Attributes("data-type").FirstOrDefault() ?? "decimal";
            bool dataOptional = ((string)e.Attributes("data-optional").FirstOrDefault() ?? "").Trim().ToLower() == "true";
            bool dataAutocount = ((string)e.Attributes("data-autocount").FirstOrDefault() ?? "").Trim().ToLower() == "true";
            bool bDataAutocount = e.NodesAfterSelf().OfType<XElement>().Where(p => p.IsHeaderName() && p.Name.LocalName == "section" && ((string)e.Attributes("data-autocount").FirstOrDefault() ?? "").Trim().ToLower() == "true").FirstOrDefault() != null;
            bool aDataAutocount = e.NodesBeforeSelf().OfType<XElement>().Where(p => p.IsHeaderName() && p.Name.LocalName == "section" && ((string)e.Attributes("data-autocount").FirstOrDefault() ?? "").Trim().ToLower() == "true").FirstOrDefault() != null;
            
            if (dataOptional || dataAutocount || bDataAutocount || aDataAutocount)
            {
                List<string> classValue = ((string)e.Attributes("class").FirstOrDefault() ?? "").Split(' ').Where(p => p.Trim() != "" && p != "").ToList();
                if (dataType == "decimal") result.Add(new XAttribute("type", "1"));
                if (dataOptional) result.Add(new XAttribute("optional", true));
                if (dataAutocount || bDataAutocount || aDataAutocount) result.Add(new XAttribute("autocount", true));

                //result.Add(new XAttribute("class", "x-section-optional"));
                //result.Add(new XAttribute("data-gen-object", true));
            }
            
            return result;
        }
        private static IEnumerable<XNode> Section(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            MoreinfoItem moreinfo = e.Annotations<MoreinfoItem>().FirstOrDefault();
            CommentsItem cmi = e.Annotations<CommentsItem>().FirstOrDefault();
            result.Add(new XElement("section",
                e.Attributes("id"),
                e.GetSectionAttributes(),
                e.Nodes().SelectMany(p => p.Transform())
                )
            );
            return result;
        }

        #endregion
    }
}
