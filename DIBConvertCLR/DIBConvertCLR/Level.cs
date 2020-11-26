using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace DIBConvertCLR
{
    public static class LevelTransformations
    {
        private class MoreinfoItem
        {
            public bool hasRelations { get; set; }
        }
        private class AccountingItem
        {
            public XElement element { get; set; }

            public AccountingItem(XElement ac)
            {
                element = new XElement(ac);
            }
        }

        public class DGLink
        {
            public string topic_id { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public string name1 { get; set; }
            public int view { get; set; }
            public string language { get; set; }
            public int publish { get; set; }
            public DGLink(XElement topic, string id)
            {
                topic_id = (string)topic.Attributes("topic_id").FirstOrDefault();
                name = (string)topic.Attributes("name").FirstOrDefault();
                view = Convert.ToInt32((string)topic.Attributes("view").FirstOrDefault() ?? "0");
                language = (string)topic.Attributes("lang").FirstOrDefault();
                publish = Convert.ToInt32((string)topic.Attributes("pub").FirstOrDefault() ?? "0");
                if (id != null)
                {
                    XElement bookmark = topic.Descendants("bookmark").Where(p => ((string)p.Attributes("id").FirstOrDefault() ?? "").Trim().ToLower() == id.Trim().ToLower()).FirstOrDefault();
                    if (bookmark != null)
                    {
                        id = (string)bookmark.Attributes("id").FirstOrDefault();
                        name1 = (string)bookmark.Attributes("name").FirstOrDefault();
                    }
                }
            }
        }

        public static XElement TransformLevel(this XElement document)
        {
            document.DescendantsAndSelf().Attributes("idx").Remove();
            document.Descendants().Attributes("class").ToList().ForEach(p => p.Value = p.Value.Trim()); // CA: Fikser bl.a. en del class=" tbNoBorder" som har sneket seg inn, og som ikke fanges opp...

            XElement c = new XElement("document", document.Transform());
            return c;

        }
        private static IEnumerable<XNode> Transform(this XNode n)
        {
            try
            {
                List<XNode> result = new List<XNode>();
                if (n.NodeType == XmlNodeType.Text) result.Add(n);
                if (n.NodeType == XmlNodeType.Element)
                {
                    XElement e = ((XElement)n);
                    switch (e.Name.LocalName)
                    {
                        case "Iref": result.AddRange(e.Iref()); break;
                        case "img":
                            result.AddRange(e.img()); break;
                        case "Bilde":
                        case "bilde":
                            result.AddRange(e.bilde()); break;
                        case "container":
                        case "document":
                        case "docpart":
                        case "bookcontent":
                        case "text": result.AddRange(e.Nodes().SelectMany(p => p.Transform())); break;

                        //case "document": result.AddRange(e.Document()); break;
                        case "forside":
                        case "level": result.AddRange(e.Level()); break;
                        case "title": result.AddRange(e.Title()); break;

                        case "div": result.Add(e.HtmlWithId()); break;
                        case "p": result.Add(e.HtmlWithId()); break;
                        case "em": result.Add(e.Em()); break;
                        case "diblink": result.AddRange(e.Diblink()); break;
                        case "dibparameter": result.AddRange(e.Diblink()); break;
                        case "table": result.Add(e.Table()); break;
                        case "thead":
                        case "tbody": result.Add(e.TableElements()); break;
                        case "td": result.AddRange(e.Td()); break;
                        case "tr": result.AddRange(e.Tr()); break;
                        case "th": result.Add(e.TableElements()); break;
                        case "h2":
                        case "h3":
                        case "h4": result.Add(e.H()); break;
                        case "span":
                        case "strong":
                        case "area":
                        case "hr":
                        case "colgroup":
                        case "col":
                            result.Add(e.Default()); break;
                        case "br":
                            result.Add(new XElement("br")); break;
                        case "base":
                        case "command":
                        case "embed":
                        case "input":
                        case "keygen":
                        case "link":
                        case "meta":
                        case "param":
                        case "source":
                        case "track":
                        case "navigation":
                        case "wbr": break;
                        case "x-list": result.Add(e.xlist()); break;
                        case "x-box": result.Add(e.xbox()); break;
                        case "x-var": result.AddRange(e.xvar()); break;
                        case "x-alternatives": result.AddRange(e.xalternatives()); break;
                        case "x-alternative": result.AddRange(e.xalternative()); break;
                        case "x-optional": result.AddRange(e.xoptional()); break;
                        case "x-letterhead": result.Add(e.xletterhead()); break;
                        case "x-comment": result.Add(e.xcomment()); break;
                        case "x-fn": result.AddRange(e.xfn()); break;
                        case "x-pagebreak": result.AddRange(e.xpagebreak()); break;
                        case "item": result.AddRange(e.LItem()); break;
                        case "description": result.AddRange(e.Description()); break;
                        case "references": result.AddRange(e.References()); break;
                        case "topicinfo": result.AddRange(e.TopicInfo()); break;
                        case "footnote": result.AddRange(e.footnote()); break;
                        case "a": result.AddRange(e.a()); break;
                        case "sup": result.AddRange(e.sup()); break;
                        default: result.Add(e.Default()); break;
                    }
                }
                return result;
            }
            catch(SystemException err)
            {
                throw new SystemException(((XElement)n).Name.LocalName + " " + err.Message);
            }
        }

        private static IEnumerable<XNode> sup(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (
                e.Nodes().OfType<XText>().Select(p => p.Value).StringConcatenate().Trim() == ""
                && (e.Elements().Count() == 1 ? e.Elements().First().Name.LocalName == "a" && ((string)e.Elements().First().Attributes("title").FirstOrDefault() ?? "").Trim() != "" : false)
            )
            {
                result.Add(new XElement("sup",
                    new XAttribute("class", "footnote"),
                    e.Attributes("id"),
                    new XAttribute("title", (string)e.Elements().First().Attributes("title").FirstOrDefault())
                    )
                );
            } else if (e.DescendantNodes().OfType<XText>().Select(p => p.Value).StringConcatenate().Trim() == "") {
                return result;
            }
            else
            {
                result.Add(e.Default());
            }
            return result;
        }
        private static IEnumerable<XNode> Iref(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("a",
                e.Attributes("id"),
                new XAttribute("class", "iref"),
                new XAttribute("data-id", e.Nodes().OfType<XText>().Select(p => p.Value).StringConcatenate().Trim()),
                e.Nodes().SelectMany(p => p.Transform())
                )
            );
            return result;
        }
        private static IEnumerable<XNode> img(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            string src = ((string)e.Attributes("src").FirstOrDefault() ?? "").Trim();

            if ((src == null ? "" : src) == "") return result;
            if (src.IndexOf('/') != -1 || src.IndexOf('\\') != -1)
            {
                result.Add(new XElement("img",
                    e.Attributes().Where(p => p.Name.LocalName != "src"),
                    new XAttribute("src", src)
                    )
                );
            }
            else
            {
                src = Regex.Split(src, @"(\\|\/)").LastOrDefault();
                if ((src == null ? "" : src) == "") return result;
                result.Add(new XElement("img",
                    e.Attributes().Where(p => p.Name.LocalName != "src"),
                    new XAttribute("src", src)
                    )
                );
            }
            return result;
        }

        private static IEnumerable<XNode> bilde(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            string src = (string)e.Attributes("src").FirstOrDefault();
            if (src == null) return result;
            result.Add(new XElement("img",
                new XAttribute("src", "dibimages/" + (string)e.Ancestors().Attributes("id").LastOrDefault() + "/" + src)
                )
            );
            return result;
        }
        private static IEnumerable<XNode> a(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if ((string)e.Attributes("class").FirstOrDefault() == "diblink")
            {
                TopicInfo ti = e.Annotation<TopicInfo>();
                if (ti != null)
                {
                    if ((e.Value.Trim().StartsWith("«") && e.Value.Trim().EndsWith("»")) || ((string)e.Attributes("data-replacetext").FirstOrDefault() ?? "") != "")
                    {
                        result.Add(new XElement("a",
                                new XAttribute("class", "diblink topic"),
                                e.Attributes("href"),
                                new XAttribute("data-topictitle", ti.title),
                                new XAttribute("data-topictype", ti.topictype),
                                new XAttribute("data-topicid", ti.id),
                                new XAttribute("data-view", ti.view),
                                new XAttribute("data-language", ti.language),
                                ti.title
                                )
                            );
                    }
                    else
                    {
                        result.Add(new XElement("a",
                            new XAttribute("class", "diblink topic"),
                            e.Attributes("href"),
                            new XAttribute("data-topictitle", ti.title),
                            new XAttribute("data-topictype", ti.topictype),
                            new XAttribute("data-topicid", ti.id),
                            new XAttribute("data-view", ti.view),
                            new XAttribute("data-language", ti.language),
                            e.Value
                            )
                        );
                    }
                }
                else
                {
                    result.Add(e);
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
                    new XAttribute("class", "diblink"),
                    new XAttribute("href", "#tid=xref//id=" + ((string)e.Attributes("id").FirstOrDefault() ?? "")),
                    e.Nodes().SelectMany(p => p.Transform())
                    )
                );
            }
            else
            {
                result.Add(new XElement("a",
                    e.Attributes().Where(p => "id;alt;href;target;title".Split(';').Contains(p.Name.LocalName) || p.Name.LocalName.StartsWith("data-")),
                    e.Nodes().SelectMany(p => p.Transform())
                    )
                );
            }
            return result;
        }
        private static IEnumerable<XNode> footnote(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("sup",
                    new XElement("a",
                        new XAttribute("class", "diblink"),
                        new XAttribute("href", "#tid=footnote//id=" + ((string)e.Attributes("id").FirstOrDefault() ?? "")),
                        e.Nodes().SelectMany(p => p.Transform())
                    )
                )
            );
            return result;
        }
        private static IEnumerable<XNode> TopicInfo(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            DGLink dgl = e.Annotation<DGLink>();
            if (dgl != null)
            {

                if (dgl.view == 1 || dgl.view == 2)
                {
                    result.Add(new XElement("li",
                        new XElement("input",
                            new XAttribute("type", "checkbox"),
                            new XAttribute("name", "topicinfo"),
                            new XAttribute("value", "0")
                        ),
                        new XElement("label",
                            dgl.name == null ? "Name missing" : dgl.name
                            )
                        )
                    );
                }
                else
                {
                    result.Add(new XElement("li",
                        dgl.name == null ? "Name missing" : dgl.name
                        )
                    );
                }

            }


            return result;
        }
        private static IEnumerable<XNode> References(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (e.Descendants("topicinfo").Where(p => (p.Annotation<DGLink>() == null ? "" : p.Annotation<DGLink>().id) != "").Count() == 0) return result;
            result.Add(new XElement("ul",
                    new XAttribute("class", "dgreferences"),
                    e.Nodes().SelectMany(p => p.Transform())
                )
            );
            return result;
        }
        private static IEnumerable<XNode> Description(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.AddRange(e.Nodes().SelectMany(p => p.Transform()));
            return result;
        }
        private static IEnumerable<XNode> LItem(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("section",
                    e.Attributes("id"),
                    new XAttribute("class", "dgitem"),
                    e.Nodes().SelectMany(p => p.Transform())
                )
            );
            return result;
        }
        private static IEnumerable<XNode> Item(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("section",
                    e.Attributes("id"),
                    new XAttribute("class", "dgitem"),
                    e.Nodes().SelectMany(p => p.Transform())
                )
            );
            return result;
        }
        private static IEnumerable<XNode> xoptional(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("x-optional",
                    e.Attributes(),
                    new XElement("section",
                        e.Nodes().SelectMany(p => p.Transform())
                    )
                )
            );
            return result;
        }
        private static IEnumerable<XNode> xalternative(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("x-alternative",
                    e.Attributes(),
                    e.Nodes().SelectMany(p => p.Transform())
                )
            );
            return result;
        }
        private static IEnumerable<XNode> xalternatives(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("x-alternatives",
                    e.Attributes(),
                    e.Nodes().SelectMany(p => p.Transform())
                )
            );
            return result;
        }
        private static IEnumerable<XNode> xfn(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (e.Nodes().Count() == 0) return result;
            if (e.Value == null) return result;
            result.Add(new XElement("sup",
                    new XAttribute("class", "footnote"),
                    new XAttribute("title", e.DescendantNodes().OfType<XText>().Select(s => s.Value).StringConcatenate())
                )
            );
            if (e.NodesAfterSelf().FirstOrDefault() == null ? false : e.NodesAfterSelf().FirstOrDefault().NodeType == XmlNodeType.Text)
            {
                XText test = (XText)e.NodesAfterSelf().FirstOrDefault();
                if (!(test.Value.StartsWith(" ") || test.Value.StartsWith(".")))
                {
                    result.Add(new XText(" "));
                }
            }
            else if (e.NodesAfterSelf().FirstOrDefault() == null ? false : e.NodesAfterSelf().FirstOrDefault().NodeType == XmlNodeType.Element)
            {
                XElement test = (XElement)e.NodesAfterSelf().FirstOrDefault();
                if (!(test.DescendantNodes().OfType<XText>().Select(p => p.Value).StringConcatenate().StartsWith(" ") || test.DescendantNodes().OfType<XText>().Select(p => p.Value).StringConcatenate().StartsWith(".")))
                {
                    result.Add(new XText(" "));
                }
            }
            return result;
        }
        private static XElement xcomment(this XElement e)
        {
            if (e.Nodes().Count() == 0) return null;
            return new XElement("x-comment",
                e.Attributes(),
                e.Nodes().SelectMany(p => p.Transform())
            );
        }
        private static XElement xletterhead(this XElement e)
        {
            if (e.Nodes().Count() == 0) return null;
            return new XElement("x-letterhead",
                e.Attributes(),
                e.Nodes().SelectMany(p => p.Transform())
            );
        }
        private static XElement xlist(this XElement e)
        {
            if (e.Nodes().Count() == 0) return null;
            return new XElement("x-list",
                e.Attributes(),
                e.Nodes().SelectMany(p => p.Transform())
            );
        }
        private static IEnumerable<XNode> xpagebreak(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("hr",
                 e.Attributes(),
                new XAttribute("class", "page-break")
                )
            );
            return result;
        }
        private static IEnumerable<XNode> xvar(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if ((e.NodesBeforeSelf().LastOrDefault() == null ? XmlNodeType.None : e.NodesBeforeSelf().LastOrDefault().NodeType) == XmlNodeType.Element)
            {
                XElement test = (XElement)e;
                if (test.Name.LocalName == "x-var")
                {
                    result.Add(new XText(" "));
                }
            }

            result.Add(new XElement("x-var",
                    e.Attributes("id"),
                    (string)e.Attributes("type").FirstOrDefault() == "global" ? new XAttribute("class", "global") : new XAttribute("class", "local"),
                    new XText(((string)e.Attributes("id").FirstOrDefault() ?? "DEFAULT").DecodeVariableId())
                )
            );
            return result;
        }
        private static IEnumerable<XNode> Td(this XElement e)
        {
            string elementName = "td";
            List<XNode> result = new List<XNode>();
            XElement parent = e.Parent;
            if (parent.Name.LocalName == "thead")
            {
                elementName = "th";
            }

            result.Add(new XElement(elementName,
                    e.Attributes("id"),
                    e.Attributes("colspan"),
                    e.Attributes("rowspan"),
                    e.Attributes("style"),
                    e.Nodes().SelectMany(p => p.Transform())
                )
            );
            return result;
        }
        private static IEnumerable<XNode> Tr(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            XElement loop = e.Parent.Parent;
            int n = 1;
            if ((loop == null ? "" : loop.Name.LocalName) == "x-list" && loop.Descendants("x-var").Count() != 0)
            {
                n = Convert.ToInt32((string)loop.Attributes("defaultcounter").FirstOrDefault() ?? "1");
            }
            for (int i = 1; i <= n; i++)
            {
                result.Add(new XElement(e.Name.LocalName.ToString(),
                        e.Attributes(),
                        e.Nodes().SelectMany(p => p.Transform())
                    )
                );
            }

            return result;
        }
        private static XElement xbox(this XElement e)
        {

            if (e.Nodes().Count() == 0) return null;
            return new XElement(e.Name.LocalName,
                e.Attributes("id"),
                e.Attributes("class"),
                e.Attributes("title").FirstOrDefault() == null ? null : new XElement("x-box-title", (string)e.Attributes("title").FirstOrDefault()),
                e.Nodes().SelectMany(p => p.Transform())
            );
        }
        private static XElement H(this XElement e)
        {
            if (e.Nodes().Count() == 0) return null;
            return new XElement("p",
                new XElement("strong",
                    e.Nodes().SelectMany(p => p.Transform())
                )
            );
        }
        private static XElement TableElements(this XElement e)
        {
            if (e.Nodes().Count() == 0)
            {
                return new XElement(e.Name.LocalName,
                    e.Attributes().Where(p => !"style;border".Split(';').Contains(p.Name.LocalName)),
                    new XComment(" ")
                );
            }
            else if (e.Name.LocalName == "td" && e.Parent.Parent.Name.LocalName == "thead")
            {
                return new XElement("th",
                    e.Attributes().Where(p => !"style;border".Split(';').Contains(p.Name.LocalName)),
                    e.Nodes().SelectMany(p => p.Transform())
                );
            }
            else
            {
                return new XElement(e.Name.LocalName,
                    e.Attributes().Where(p => !"style;border".Split(';').Contains(p.Name.LocalName)),
                    e.Nodes().SelectMany(p => p.Transform())
                );
            }

        }
        private static XElement Table(this XElement e)
        {
            return new XElement("div",
                new XAttribute("class", "table-wrapper"),
                new XElement("table",
                    ((string)e.Attributes("class").FirstOrDefault() ?? "").ToLower() == "tbnoborder" ? e.Attributes("class") : null,
                     e.Nodes().SelectMany(p => p.Transform())
                )
            );
        }
        private static XElement Default(this XElement e)
        {
            if (e.Nodes().Count() == 0 && !("br;x-footnote;dl;dt;dd").Split(';').Contains(e.Name.LocalName)) return null;
            return new XElement(e.Name.LocalName.ToString(),
                e.Attributes("id"),
                e.Attributes("style"),
                e.Name.LocalName == "ol" ? e.Attributes("type") : null,
                e.Attributes().Where(p => p.Name.LocalName.StartsWith("data-")),
                e.Nodes().SelectMany(p => p.Transform())
            );
        }
        private static XElement HtmlWithId(this XElement e)
        {
            if (e.Nodes().Count() == 0) return null;
            return new XElement(e.Name.LocalName.ToString(),
                e.Attributes("id").FirstOrDefault() == null ? null : new XAttribute("id", (string)e.Attributes("id").FirstOrDefault()),
                e.Nodes().SelectMany(p => p.Transform())
            );
        }
        private static XElement Em(this XElement e)
        {
            if (e.Nodes().Count() == 0) return null;
            return new XElement("i",
                e.Nodes().SelectMany(p => p.Transform())
            );
        }
        private static IEnumerable<XElement> Title(this XElement e)
        {
            List<XElement> l = new List<XElement>();
            AccountingItem acci = e.Parent.Annotations<AccountingItem>().FirstOrDefault();
            MoreinfoItem moreinfo = e.Parent.Annotations<MoreinfoItem>().FirstOrDefault();
            int n = 0;
            if (e.Ancestors("x-alternative").FirstOrDefault() == null)
            {
                n = e.Ancestors().TakeWhile(p => p.Name.LocalName == "level").Count();
            }
            else
            {
                n = e.Ancestors().Where(p => p.Name.LocalName == "level").Count()-1;
                if (n == 1) n = 2;
            }
            if (((string)e.Parent.Attributes("hiddentitle").FirstOrDefault() ?? "") != "1")
            {
                l.Add(new XElement("h" + n.ToString(),
                    //new XAttribute("id", (string)e.Parent.Attributes("id").FirstOrDefault()),
                    //moreinfo == null ? null : new XAttribute("data-hasrelations", moreinfo.hasRelations),
                    e.Nodes().SelectMany(p => p.Transform())
                    )
                );
                if (acci != null)
                {
                    l.Add(acci.element);
                }
            }
            else
            {
                l.Add(new XElement("p"));
            }

            return l;
        }
        private static IEnumerable<XNode> Level(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if ((string)e.Attributes("segment").FirstOrDefault() == "true")
            {
                result.Add(new XElement("nav",
                        new XAttribute("class", "front-toc"),
                            new XElement("ul",
                            new XElement("li",
                                new XElement("a",
                                    e.Attributes("id"),
                                    new XAttribute("class", "dibsegment"),
                                    new XAttribute("href", "#" + (string)e.Attributes("id").FirstOrDefault()),
                                             new XText(e.Elements("title").Select(p => p.Value).FirstOrDefault())
                                 )
                            )
                        )
                    )
                );
                result.AddRange(e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == e.Name.LocalName).SelectMany(p => p.Transform()));
            }
            else
            {
                MoreinfoItem moreinfo = e.Annotations<MoreinfoItem>().FirstOrDefault();
                int n = e.AncestorsAndSelf().TakeWhile(p => p.Name.LocalName != "document").Count();
                result.Add(new XElement("section",
                        e.Attributes("id"),
                        e.Attributes().Where(a => a.Name.LocalName.StartsWith("data")),
                        moreinfo == null ? null : new XAttribute("data-hasrelations", moreinfo.hasRelations),
                        e.Nodes().SelectMany(p => p.Transform())
                    )
                );
            }
            return result;
        }
        private static IEnumerable<XNode> Document(this XElement e)
        {
            return e.Nodes().SelectMany(p => p.Transform());
        }
    }
}
