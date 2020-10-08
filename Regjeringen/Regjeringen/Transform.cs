using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Regjeringen
{
    public class HeaderClassType
    {
        public string type { get; set; }
        public string heading { get; set; }
        public HeaderClassType (XElement h)
        {
            type = h.ElementClassName();
            heading = h.GetHeaderText().Trim().Split(' ').FirstOrDefault();
        }
    }
    public class TopicInfo
    {
        public string topictype { get; set; }
        public string title { get; set; }
        public string id { get; set; }
        public string segment_id { get; set; }
        public string paragraph_id { get; set; }
        public string view { get; set; }
        public string language { get; set; }
        public string bm { get; set; }
        public string type { get; set; }
        public TopicInfo(XElement topic)
        {
            id = (string)topic.Attributes("topic_id").FirstOrDefault();
            topictype = (string)topic.Attributes("tid").FirstOrDefault();
            title = (((string)topic.Attributes("short").FirstOrDefault() ?? "") + " " + ((string)topic.Attributes("name").FirstOrDefault() ?? "")).Trim();
            segment_id = (string)topic.Attributes("segment_id").FirstOrDefault();
            paragraph_id = (string)topic.Attributes("paragraf_id").FirstOrDefault();
            view = (string)topic.Attributes("view").FirstOrDefault();
            language = (string)topic.Attributes("lang").FirstOrDefault();
        }
        public TopicInfo(XElement topic, string Bm)
        {
            id = (string)topic.Attributes("topic_id").FirstOrDefault();
            topictype = (string)topic.Attributes("tid").FirstOrDefault();
            title = (((string)topic.Attributes("short").FirstOrDefault() ?? "") + " " + ((string)topic.Attributes("name").FirstOrDefault() ?? "")).Trim();
            segment_id = (string)topic.Attributes("segment_id").FirstOrDefault();
            paragraph_id = (string)topic.Attributes("paragraf_id").FirstOrDefault();
            view = (string)topic.Attributes("view").FirstOrDefault();
            language = (string)topic.Attributes("lang").FirstOrDefault();
            bm = Bm == null ? null : (Bm.Trim() == "" ? null : Bm.Trim().ToLower());

        }
        public TopicInfo(XElement topic, string Bm, string Type)
        {
            //default
            id = (string)topic.Attributes("topic_id").FirstOrDefault();
            topictype = (string)topic.Attributes("tid").FirstOrDefault();
            title = (((string)topic.Attributes("short").FirstOrDefault() ?? "") + " " + ((string)topic.Attributes("name").FirstOrDefault() ?? "")).Trim();
            segment_id = (string)topic.Attributes("segment_id").FirstOrDefault();
            paragraph_id = (string)topic.Attributes("paragraf_id").FirstOrDefault();
            view = (string)topic.Attributes("view").FirstOrDefault();
            language = (string)topic.Attributes("lang").FirstOrDefault();
            bm = Bm == null ? null : (Bm.Trim() == "" ? null : Bm.Trim());
            type = Type == null ? null : (Type.Trim() == "" ? null : Type.Trim().ToLower());
        }
    }
    public static class TranformRegjeringen
    {
        public static string ConvertToHtmlRegj(this XElement document, XElement xlinkgroup = null)
        {
            document.DescendantsAndSelf().Attributes("idx").Remove();
            if (xlinkgroup != null)
            {
                document.AddLinkInfo(xlinkgroup);
            }

            document.DescendantsAndSelf().Attributes("idx").Remove();
            string segment_id = (string)document.Attributes("segment_id").FirstOrDefault();

            XElement part = null;
            if ((segment_id == null ? "_top" : segment_id) != "_top")
                part = document.Descendants().Where(p => ((string)p.Attributes("id").FirstOrDefault() ?? "").ToLower() == segment_id.ToLower()).FirstOrDefault();
            if (part != null)
                document = part;

            XElement c = new XElement("container", document.Transform());
            XmlReader r = c.CreateReader();
            r.MoveToContent();
            return r.ReadInnerXml();
        }

        private static IEnumerable<XNode> Transform(this XNode n)
        {
            List<XNode> result = new List<XNode>();
            if (n.NodeType == XmlNodeType.Text) result.Add(n);


            if (n.NodeType == XmlNodeType.Element)
            {
                XElement e = ((XElement)n);
                switch (e.Name.LocalName)
                {
                    //case "diblink":
                    //case "dibparameter": result.AddRange(e.Diblink()); break;
                    case "docpart":
                    case "navpoint":
                        result.AddRange(e.Nodes().SelectMany(p => p.Transform())); break;
                    case "body":
                    case "div":
                        result.AddRange(e.section()); break;
                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6":
                    case "h7":
                        result.AddRange(e.title()); break;
                    case "img":
                        result.AddRange(e.img()); break;
                    case "br": result.Add(new XElement("br")); break;
                    case "a": result.AddRange(e.a()); break;
                    case "b": result.AddRange(e.b()); break;
                    case "i": result.AddRange(e.i()); break;
                    default:
                        result.AddRange(e.Default()); break;

                }
            }
            return result;
        }
        private static IEnumerable<XNode> _Prototype(this XElement e)
        {
            List<XNode> result = new List<XNode>();


            return result;
        }
        #region //Transform elementer

        private static IEnumerable<XNode> i(this XElement e)
        {
            List<XNode> result = new List<XNode> {
                new XElement("em",
                        e.Nodes().SelectMany(p => p.Transform())
                    )
            };

            return result;
        }
        private static IEnumerable<XNode> b(this XElement e)
        {
            List<XNode> result = new List<XNode> {
                new XElement("strong",
                        e.Nodes().SelectMany(p => p.Transform())
                    )
            };

            return result;
        }
        private static IEnumerable<XNode> a(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (((string)e.Parent.Descendants("span").Attributes("class").FirstOrDefault() ?? "").ToLower() == "K-NOTETEXT".ToLower())
            {
                result.Add(new XElement("a",
                        e.GetAttributeToLower("id"),
                        new XAttribute("class", "diblink"),
                        new XAttribute("href", "#tid=xref//id=" + (((string)e.Attributes("href").FirstOrDefault() ?? "").Split('#').LastOrDefault() ?? "")),
                        e.Nodes().SelectMany(p => p.Transform())
                    )
                );
            }
            else if (((string)e.Attributes("footnote").FirstOrDefault() ?? "") != "")
            {
                result.Add(new XElement("a",
                        e.GetAttributeToLower("id"),
                        new XAttribute("class", "diblink"),
                        new XAttribute("href", "#tid=footnote//id=" + (((string)e.Attributes("href").FirstOrDefault() ?? "").Split('#').LastOrDefault() ?? "")),
                        e.Nodes().SelectMany(p => p.Transform())
                    )
                );
            }
            else
            {
                result.Add(new XElement("a",
                        e.GetAttributeToLower("id"),
                        e.GetAttributeToLower("href"),
                        new XElement("target", "_blank"),
                        e.Nodes().SelectMany(p => p.Transform())
                    )
                );
            }

            return result;
        }
        private static IEnumerable<XNode> img(this XElement e)
        {
            List<XNode> result = new List<XNode> {
                new XElement("img",
                    new XAttribute("src", "dibimages/" + (string)e.Ancestors().Attributes("id").LastOrDefault() + "/" + (string)e.Attributes("src").FirstOrDefault())
                )
            };

            return result;
        }
        private static IEnumerable<XNode> Default(this XElement e)
        {
            List<XNode> result = new List<XNode> {
                new XElement(e.Name.LocalName,
                    e.GetAttributeToLower("id"),
                    "p" == e.Name.LocalName.ToLower() ? e.GetAttributeToLower("class") : null,
                    "td;th".Split(';').Contains(e.Name.LocalName.ToLower()) ? e.Attributes().Where(p => "colspan;rowspan;align;valign".Split(';').Contains(p.Name.LocalName.ToLower())).Select(p => e.GetAttributeToLower(p.Name.LocalName)) : null,
                    "table".Split(';').Contains(e.Name.LocalName.ToLower()) ? e.Attributes().Where(p => "border;frame".Split(';').Contains(p.Name.LocalName.ToLower())).Select(p => e.GetAttributeToLower(p.Name.LocalName)) : null,
                    "ol".Split(';').Contains(e.Name.LocalName.ToLower()) ? e.Attributes().Where(p => "type;class".Split(';').Contains(p.Name.LocalName.ToLower())).Select(p => e.GetAttributeToLower(p.Name.LocalName)) : null,
                    "li".Split(';').Contains(e.Name.LocalName.ToLower()) ? e.Attributes().Where(p => "class".Split(';').Contains(p.Name.LocalName.ToLower())).Select(p => e.GetAttributeToLower(p.Name.LocalName)) : null,
                    e.Nodes().SelectMany(p => p.Transform())
                )
            };
            return result;
        }
        private static IEnumerable<XNode> title(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            XElement parent = e.Parent;
            if (parent.Nodes().OfType<XElement>().Where(p => p.DescendantNodes().OfType<XText>().Select(s => s.Value).StringConcatenate().Trim() != "").FirstOrDefault() == e)
            {
                int n = e.Ancestors().TakeWhile(p => p.Name.LocalName != "navpoint").Count();
                result.Add(new XElement("h" + n.ToString(),
                    e.GetAttributeToLower("id"),
                    e.GetAttributeToLower("class"),

                    e.Nodes().SelectMany(p => p.Transform())
                    )
                );
            }
            else
            {
                result.Add(new XElement("p",
                        e.GetAttributeToLower("id"),
                        e.GetAttributeToLower("class"),
                        new XElement("strong",
                            e.Nodes().SelectMany(p => p.Transform())
                        )
                    )
                );
            }
            return result;
        }

        private static IEnumerable<XNode> section(this XElement e)
        {
            List<XNode> result = new List<XNode> {
                new XElement("section",
                    e.Attributes("id"),
                    e.Nodes().SelectMany(p => p.Transform())
                )
            };
            return result;
        }


        #endregion
    }

    public class NavPointDataEx
    {
        public string id { get; set; }
        public string filename { get; set; }
        public string text { get; set; }
        public string srcId { get; set; }
        public int playOrder { get; set; }
        public NavPointDataEx(XElement navPoint)
        {
            id = ((string)navPoint.Attributes("id").FirstOrDefault()??"").Trim().ToLower();
            string src = navPoint.navPointContentSrc() ?? "";
            filename = src.Split('#').FirstOrDefault();
            srcId = src.Split('#').LastOrDefault();
            text = navPoint.navLabelText();
            playOrder = Convert.ToInt32(((string)navPoint.Attributes("playOrder").FirstOrDefault()??"0"));
        }
    }
    public class NavPointData
    {
        public string segmentname { get; set; }
        public string id { get; set; }
        public string src { get; set; }
        public NavPointData(string filename)
        {
            src = filename;
            segmentname = src.Split('.').FirstOrDefault();
            id = src.Split('#').Count() < 2 ? "" : src.Split('#').LastOrDefault();
        }
    }

    public static class TranformXHTML
    {

        private static IEnumerable<XNode> Transform(this XNode n)
        {
            List<XNode> result = new List<XNode>();
            if (n.NodeType == XmlNodeType.Text) result.Add(n);


            if (n.NodeType == XmlNodeType.Element)
            {
                XElement e = ((XElement)n);
                switch (e.Name.LocalName)
                {
                    case "document":
                        result.AddRange(e.Nodes().SelectMany(p => p.Transform())); break;
                    case "section":
                    case "div":
                        result.AddRange(e.Default()); break;
                    default:
                        result.AddRange(e.Default()); break;
                }
            }
            return result;
        }
        private static IEnumerable<XNode> Default(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement(e.Name.LocalName, e.Attributes("id").FirstOrDefault()));
            return result;
        }
        private static IEnumerable<XNode> _Prototype(this XElement e)
        {
            List<XNode> result = new List<XNode>();


            return result;
        }
        
}
    public static class Extention
    {
        public static void AddLinkInfo(this XElement document, XElement xlinkgroup)
        {
            (
                from dl in document.Descendants("a").Where(s => (string)s.Attributes("class").FirstOrDefault() == "diblink")
                join rf in xlinkgroup.Descendants("ref")
                on ((string)dl.Attributes("href").FirstOrDefault() ?? "") equals "#" + ((string)rf.Attributes("id").FirstOrDefault() ?? "")
                join to in xlinkgroup.Descendants("topic")
                on (string)rf.Parent.Attributes("topic_id").FirstOrDefault() equals (string)to.Attributes("topic_id").FirstOrDefault()
                select new { link = dl, topicinfo = to, r = rf }
                )
                .Union(
                from dl in document.Descendants().Where(p => "diblink;dibparameter".Split(';').Contains(p.Name.LocalName))
                join rf in xlinkgroup.Descendants("ref")
                on (string)dl.Attributes().Where(p => "refid;rid".Split(';').Contains(p.Name.LocalName)).FirstOrDefault() equals (string)rf.Attributes("id").FirstOrDefault()
                join to in xlinkgroup.Descendants("topic")
                on (string)rf.Parent.Attributes("topic_id").FirstOrDefault() equals (string)to.Attributes("topic_id").FirstOrDefault()
                select new { link = dl, topicinfo = to, r = rf }
                ).ToList()
                .ForEach(p => p.link.AddAnnotation(
                        new TopicInfo(
                            p.topicinfo,
                            (string)p.r.Attributes("bm").FirstOrDefault() ?? ""
                            , (string)p.r.Attributes("type").FirstOrDefault() ?? ""
                        )
                    )
                );
        }
        public static XAttribute GetAttributeToLower(this XElement e, string name)
        {
            string value = (string)e.Attributes().Where(p => p.Name.LocalName.ToLower() == name.ToLower()).FirstOrDefault();
            return value == null ? null : new XAttribute(name.ToLower(), value.ToLower());
        }
        public static IEnumerable<XElement> GetItem(this XElement e)
        {
            List<XElement> result = new List<XElement>();
            string search = "";
            switch (e.Name.LocalName)
            {
                case "li":
                    search = e.Nodes()
                        .TakeWhile(p =>
                            p.NodeType == XmlNodeType.Text
                            || !"ol;ul;p".Split(';').Contains((p.NodeType == XmlNodeType.Element ? ((XElement)p).Name.LocalName : ""))
                        )
                        .OfType<XText>()
                        .Select(p => p.ToString())
                        .StringConcatenate();

                    if (search == "") search = null;

                    result.Add(new XElement("item",
                        e.Attributes("id"),
                        new XAttribute("text_type", "0"),
                        search == null ? null : new XAttribute("search", search),
                        e.Elements().Where(p => "ol;ul;p".Split(';').Contains(p.Name.LocalName)).Count() == 0
                            ? null
                            : e.Elements().Where(p => "ol;ul;p".Split(';').Contains(p.Name.LocalName)).SelectMany(p => p.GetItem())
                        )
                    );

                    break;

                case "a":
                    result.Add(new XElement("item",
                        e.Attributes("id"),
                        new XAttribute("text_type", "0")
                        )
                    );
                    break;
                case "span":
                case "table":
                case "th":
                case "tr":
                case "ol":
                case "ul":
                    result.AddRange(
                        e.Elements().SelectMany(p => p.GetItem())
                    );
                    break;
                default:
                    search = e.DescendantNodes().OfType<XText>().Select(s => s.ToString()).StringConcatenate();
                    result.Add(new XElement("item",
                        e.Attributes("id"),
                        new XAttribute("text_type", "0"),
                        search == null ? null : new XAttribute("search", search),
                        e.Elements().Where(p => "span;a".Split(';').Contains(p.Name.LocalName)).Count() == 0
                        ? null
                        : e.Elements().Where(p => "span;a".Split(';').Contains(p.Name.LocalName)).SelectMany(p => p.GetItem())

                        )
                    );
                    break;

            }
            return result;
        }
        public static IEnumerable<XElement> GetPart(this XElement e)
        {
            if (e.Elements().Where(p => p.IsPart("TIT")).Count() != 0)
            {
                return e.Elements()
                .Where(p => p.IsPart("TIT"))
                .Select(p =>
                    new XElement("item",
                        p.Parent.Attributes("id"),
                        new XAttribute("title", p.DescendantNodes().OfType<XText>().Select(s => s.ToString()).StringConcatenate()),
                        new XAttribute("search", p.DescendantNodes().OfType<XText>().Select(s => s.ToString()).StringConcatenate()),
                        new XAttribute("text_type", "1"),
                        p.NodesAfterSelf()
                        .OfType<XElement>()
                        .TakeWhile(s => !s.IsPart("DEL"))
                        .SelectMany(s => s.GetItem()),
                        p.NodesAfterSelf()
                        .OfType<XElement>()
                        .SkipWhile(s => !s.IsPart("DEL"))
                        .TakeWhile(s => s.IsPart("DEL"))
                        .SelectMany(s => s.GetParts())

                    )
               );

            }
            else
            {
                return e.Elements().SelectMany(p => p.GetParts());
            }
        }

        public static bool IsPart(this XElement e, string token)
        {
            return (((string)e.Attributes("class").FirstOrDefault() ?? "").Split('-').Count() > 1
                ? ((string)e.Attributes("class").FirstOrDefault() ?? "").Split('-').ElementAt(1)
                : "") == token
                ? true
                : false;
        }
        public static IEnumerable<XElement> GetParts(this XElement e)
        {
            if ((string)e.Ancestors("navpoint").Attributes("id").FirstOrDefault() == "fot")
            {
                return e.Elements("p").Select(p => new XElement("item",
                    p.Attributes("id"),
                    new XAttribute("title", p.Elements("a").Select(s => s.Value.ToString()).StringConcatenate())

                    )
                );
            }
            else if (e.Name.LocalName == "body")
            {
                return e.Elements().SelectMany(p => p.GetParts());
            }

            else
            {
                return e.GetPart();
            }

        }
        public static IEnumerable<XElement> GetBookcontentItems(this XElement bookcontent)
        {
            return bookcontent.Descendants("body").SelectMany(p => p.GetParts());


        }
        public static string ReplaceTocText(this string s)
        {
            s = Regex.Replace(s, @"\r\n", " ");
            s = Regex.Replace(s, @"\s+", " ");

            return s;
        }

        public static IEnumerable<XElement> GetNavpiontData(this XElement e)
        {
            return e.Elements("navPoint")
                    .Select(p => new XElement(p.Name.LocalName.ToLower(),
                        p.Attributes(),
                        new XAttribute("title", p.Elements("navLabel").Elements("text").DescendantNodes().OfType<XText>().Select(s => s.ToString()).StringConcatenate().ReplaceTocText().Trim()),
                        p.Element("navPoint") == null ? null : p.GetNavpiontData(),
                        p.Elements("content").Attributes("src")
                        )
                    );
        }
        public static string StringConcatenate(this IEnumerable<string> source)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in source)
                sb.Append(s);
            return sb.ToString();
        }
        public static string StringConcatenate(this IEnumerable<string> source, string separate)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in source)
            {
                if (sb.ToString() == "")
                {
                    sb.Append(s);
                }
                else if (!sb.ToString().EndsWith(" ") && !s.StartsWith(" "))
                {
                    sb.Append(separate + s);
                }
                else
                {
                    sb.Append(s);
                }
            }
            return sb.ToString();
        }

        public static string StringConcatenate<T>(
            this IEnumerable<T> source,
            Func<T, string> projectionFunc)
        {
            return source.Aggregate(
                new StringBuilder(),
                (s, i) => s.Append(projectionFunc(i)),
                s => s.ToString());
        }

    }
    public static class TransformEpubRegjeringen
    {
        public static IEnumerable<XElement> K_DEL_LEDD(this XElement e, int niv)
        {
            List<XElement> result = new List<XElement>();
            XElement endringstittel = e.Elements().Where(p => p.ElementClassName() == "h2-k-tit-endringer").FirstOrDefault();
            if (endringstittel != null)
            {
                while (e.Elements().Count()>0)
                {
                    List<XElement> before = e.Elements().TakeWhile(p => p.ElementClassName() != "h2-k-tit-endringer").ToList();
                    if (before.Count()>0)
                        result.Add(new XElement("section", new XAttribute("class", "lov-ledd"), before.Nodes()));

                    before.ForEach(p => p.Remove());
                    XElement heading = e.Elements().FirstOrDefault();
                    if ((heading==null ? "" : heading.ElementClassName())== "h2-k-tit-endringer")
                    {
                        List<XElement> after = heading.NodesAfterSelf().OfType<XElement>().TakeWhile(p => p.ElementClassName() != "h2-k-tit-endringer").ToList();
                        result.Add(
                            new XElement("section",
                                e.Attributes("id"),
                                new XElement("h" + (niv + 1).ToString(), heading.Nodes()),
                                new XElement("section", new XAttribute("class", "lov-ledd"), after)
                            )
                        );
                        heading.Remove();
                        after.ForEach(p => p.Remove());
                    }
                }
            }
            else
            {
                result.Add(new XElement("section", new XAttribute("class", "lov-ledd"), e.Nodes()));
            }
            
            
            return result;
        }
        public static IEnumerable<XElement> K_DEL_LOVAVSNITT(this XElement e, int niv)
        {
            List<XElement> result = new List<XElement>();
            XElement tittel = e.Elements().Where(p => p.ElementClassName() == "h2-k-tit-lovavsnitt").FirstOrDefault();
            if (tittel != null)
            {
                result.Add(
                    new XElement("section",
                        e.Attributes("id"),
                        new XElement("h" + (niv + 1).ToString(), tittel.Nodes()),
                        e.Elements().Where(p => "div-k-del-paragraf".Split(';').Contains(p.ElementClassName())).SelectMany(p => p.K_DEL_LOVDELER(niv + 1))
                    )
                );
            }
            else
            {
                result.AddRange(e.Elements().Where(p => "div-k-del-paragraf".Split(';').Contains(p.ElementClassName())).SelectMany(p => p.K_DEL_LOVDELER(niv)));
            }
            return result;
        }
        public static IEnumerable<XElement> K_DEL_LOVKAP(this XElement e, int niv)
        {
            List<XElement> result = new List<XElement>();
            XElement tittel = e.Elements().Where(p => p.ElementClassName() == "h2-k-tit-lovkap").FirstOrDefault();
            if (tittel != null)
            {
                result.Add(
                    new XElement("section",
                        e.Attributes("id"),
                        new XElement("h" + (niv + 1).ToString(), tittel.Nodes()),
                        e.Elements().Where(p => "div-k-del-lovavsnitt;div-k-del-paragraf".Split(';').Contains(p.ElementClassName())).SelectMany(p => p.K_DEL_LOVDELER(niv + 1))
                    )
                );
            }
            else
            {
                result.AddRange(e.Elements().Where(p => "div-k-del-lovavsnitt;div-k-del-paragraf".Split(';').Contains(p.ElementClassName())).SelectMany(p => p.K_DEL_LOVDELER(niv)));
            }
            return result;
        }
        public static IEnumerable<XElement> K_DEL_PARAGRAF(this XElement e, int niv)
        {
            List<XElement> result = new List<XElement>();
            XElement endringstittel = e.Elements().Where(p => "h2-k-tit-paragraf;h2-k-tit-endringer".Split(';').Contains(p.ElementClassName())).FirstOrDefault();
            if (endringstittel != null)
            {
                result.Add(
                    new XElement("section",
                        e.Attributes("id"),
                        new XElement("h" + (niv + 1).ToString(), endringstittel.Nodes()),
                        e.Elements().Where(p => p.ElementClassName() == "div-k-del-ledd").SelectMany(p => p.K_DEL_LEDD(niv + 1))
                    )
                );
            }
            else
            {
                result.AddRange(e.Elements().Where(p => p.ElementClassName() == "div-k-del-ledd").SelectMany(p => p.K_DEL_LEDD(niv)));
            }
            return result;
        }
        public static IEnumerable<XElement> K_DEL_LOVDELER(this XElement e, int niv)
        {
            List<XElement> result = new List<XElement>();

            
            switch (e.ElementClassName())
            {
                case "div-k-del-paragraf":
                    result.AddRange(e.K_DEL_PARAGRAF(niv));
                    break;
                case "div-k-del-lovkap":
                    result.AddRange(e.K_DEL_LOVKAP(niv));
                    break;
                case "div-k-del-lovavsnitt":
                    result.AddRange(e.K_DEL_LOVAVSNITT(niv));
                    break;
            }
            
            return result;
        }
        public static IEnumerable<XElement> K_DEL_LOVDEL(this XElement e, int niv)
        {
            List<XElement> result = new List<XElement>();

            foreach (XElement del in e.Elements())
            {
                switch (del.ElementClassName())
                {
                    case "div-k-del-paragraf":
                        result.AddRange(del.K_DEL_PARAGRAF(niv));
                        break;
                    case "div-k-del-lovkap":
                        result.AddRange(del.K_DEL_LOVKAP(niv));
                        break;
                    case "div-k-del-lovavsnitt":
                        result.AddRange(del.K_DEL_LOVAVSNITT(niv));
                        break;
                }
            }
            return result;
        }
        public static void K_DEL_LOVER(this XElement e, int niv)
        {
            XElement endringstittel = e.Elements().Where(p => p.ElementClassName() == "h2-k-tit-endringer").FirstOrDefault();
            if (endringstittel != null)
            {
                e.ReplaceWith(
                    new XElement("section", 
                        e.Attributes("id"),
                        new XElement("h" + (niv+1).ToString(), endringstittel.Nodes()),
                        e.Elements().Where(p => p.ElementClassName() == "div-k-del-lovdel").SelectMany(p => p.K_DEL_LOVDEL(niv + 1))
                    )
                );
            }
            else
            {
                e.ReplaceWith(e.Elements().Where(p => p.ElementClassName() == "div-k-del-lovdel").SelectMany(p => p.K_DEL_LOVDEL(niv)));
            }
        }

        public static void MakeSectionsFrom_MUTIT_UNDERTIT(this XElement section , string niv1, string niv2, string titlestart)
        {
            List<XElement> vedlegg = section
                                            .Descendants()
                                            .Where(p =>
                                                p.ElementClassName() == niv1
                                                && Regex.IsMatch(p.GetHeaderText().Trim().ToLower(), @"^" + titlestart.ToLower())
                                                && p.NodesAfterSelf().OfType<XElement>().Take(1).FirstOrDefault().ElementClassName() == niv2
                                            )
                                            .ToList();
            if (vedlegg.Count() > 0)
            {
                foreach (XElement v in vedlegg)
                {
                    XElement undertittel = v.NodesAfterSelf().OfType<XElement>().Take(1).FirstOrDefault();
                    List<XElement> vl = new List<XElement>();
                    vl.AddRange(v.NodesAfterSelf().OfType<XElement>().Skip(1).TakeWhile(p => p.Name.LocalName != "section" && !vedlegg.Contains(p)));
                    XElement newvedlegg = new XElement("section",
                        new XElement("h" + (v.Ancestors("section").Count() + 1).ToString(), v.GetHeaderText()),
                        new XElement("p", new XElement("strong", undertittel.GetHeaderText())),
                        vl
                    );
                    undertittel.Remove();
                    vl.ForEach(p => p.Remove());
                    v.ReplaceWith(newvedlegg);
                }
            }
        }
        public static void ResetHeaderClassData(this XElement document)
        {
            document
                .Descendants()
                .Where(a => a.Annotations<HeaderClassType>().FirstOrDefault() != null)
                .ToList()
                .ForEach(p => p.RemoveAnnotations<HeaderClassType>());

            document
                .Descendants()
                .Where(p =>
                    Regex.IsMatch(p.Name.LocalName, @"h\d")
                    && p.Parent.Name == "section"
                    && "h2-k-tit-undertit;h2-k-tit-mutit;h2-k-tit-mutit2;h2-k-tit-mtit".Split(';').Contains(p.ElementClassName())
                )
                .Select(p => p)
                .ToList().ForEach(p => p.AddAnnotation(new HeaderClassType(p)));
        }
        public static XElement MakeSubSections(this XElement document)
        {
            document.ResetHeaderClassData();

            List<XElement> sections = null;

            sections = document
                .Descendants()
                .Where(p =>
                    Regex.IsMatch(p.Name.LocalName, @"h\d")
                    && p.Parent.Name == "section"
                    && p == p.Parent.Elements().FirstOrDefault()
                    && p.NodesAfterSelf()
                        .OfType<XElement>()
                        .Where(a => a.Annotations<HeaderClassType>().FirstOrDefault() != null)
                        .GroupBy(a => a.Annotations<HeaderClassType>().Select(r=>r.type).FirstOrDefault())
                        .Select(a => a.Key)
                        .Count() > 1
                )
                .Select(p => p.Parent)
                .ToList();


            foreach (XElement section in sections)
            {
                section.MakeSectionsFrom_MUTIT_UNDERTIT("h2-k-tit-mutit", "h2-k-tit-undertit", "vedlegg");
                section.MakeSectionsFrom_MUTIT_UNDERTIT("h2-k-tit-mutit", "h2-k-tit-undertit", "kapittel");
                section.MakeSectionsFrom_MUTIT_UNDERTIT("h2-k-tit-mutit", "h2-k-tit-undertit", "kapitel");
                section.MakeSectionsFrom_MUTIT_UNDERTIT("h2-k-tit-mutit", "h2-k-tit-undertit", "chapter");
                section.MakeSectionsFrom_MUTIT_UNDERTIT("h2-k-tit-mutit", "h2-k-tit-undertit", "artikkel");
                section.MakeSectionsFrom_MUTIT_UNDERTIT("h2-k-tit-mutit", "h2-k-tit-undertit", "artikel");
                section.MakeSectionsFrom_MUTIT_UNDERTIT("h2-k-tit-mutit", "h2-k-tit-undertit", "article");
            }

            document.ResetHeaderClassData();


            string subelementClassName = "";
            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0: subelementClassName = "h2-k-tit-undertit"; break;
                    case 1: subelementClassName = "h2-k-tit-mutit"; break;
                    case 2: subelementClassName = "h2-k-tit-mutit2"; break;
                    case 3: subelementClassName = "h2-k-tit-mtit"; break;
                    default: subelementClassName = ""; break;
                }
                
                if (subelementClassName != "")
                {
                    document.ResetHeaderClassData();
                    sections = document
                        .Descendants()
                        .Where(p =>
                            Regex.IsMatch(p.Name.LocalName, @"h\d")
                            && p.Parent.Name == "section"
                            && p == p.Parent.Elements().FirstOrDefault()
                            && p.NodesAfterSelf()
                                .OfType<XElement>()
                                .Where(a => a.Annotations<HeaderClassType>().Select(t => t.type).FirstOrDefault() == subelementClassName)
                                .Count() > 0

                        )
                        .Select(p => p.Parent)
                        .Reverse()
                        .ToList();
                    foreach (XElement section in sections)
                    {
                        section.MakeSubSection(subelementClassName);
                    }

                }
            }

            return document;
        }
        public static void MakeSubSection(this XElement e, string elementClassName)
        {
            List<XElement> result = new List<XElement>();
            while (e.Elements().Count()>0)
            {
                List<XElement> before = e.Elements().TakeWhile(p => p.ElementClassName() != elementClassName).ToList();
                result.AddRange(before);
                before.ForEach(p => p.Remove());
                XElement header = e.Elements().FirstOrDefault();
                if ((header == null ? "" : header.ElementClassName())== elementClassName)
                {
                    XElement section = new XElement("section", header.Attributes("id"), new XElement("h" + (header.Ancestors("section").Count() + 1).ToString(), header.Nodes()));
                    List<XElement> after = header.NodesAfterSelf().OfType<XElement>().TakeWhile(p => p.ElementClassName() != elementClassName).ToList();
                    header.Remove();
                    section.Add(after);
                    after.ForEach(p => p.Remove());
                    result.Add(section);
                }
            }
            if (result.Count()>0)
                e.Add(result);
        }
        public static void ReplaceID(this XElement e)
        {
            XAttribute a = e.Attributes("ID").FirstOrDefault();
            e.Attributes("ID").Remove();
            if (e.Attributes("id").FirstOrDefault()==null)
            {
                e.Add(new XAttribute("id", a.Value.ToLower().Trim()));
            }
            else
            {
                e.Attributes("id").FirstOrDefault().SetValue(a.Value.ToLower().Trim());
            }

        }
        public static IEnumerable<XElement> TransformFotnoter(this XElement e)
        {
            List<XElement> result = new List<XElement>();
            foreach (XElement p in e.Elements("p"))
            {
                XElement footnote = new XElement("footnote", p.Attributes("id"));
                List<XElement> el = p.Descendants().Where(n => n.ElementClassName() == "span-k-notetext").Elements().ToList();
                foreach (XElement t in el)
                {
                    footnote.Add(new XElement(t.Name.LocalName, t.Nodes()));
                }
                result.Add(footnote);
            }

            return result;
        }
        public static void SetTDClass(this XElement e)
        {
            XAttribute c = e.Attributes("class").FirstOrDefault();
            if (c == null) return;
            string classValue = c.Value.ToLower();
            if (Regex.IsMatch(classValue, "right"))
            {
                c.SetValue("numeric");
            }
            else
            {
                e.Attributes("class").Remove();
            }

        }
        public static XElement K_Section(this XElement d)
        {
            XElement first = d.Elements().FirstOrDefault();
            if (first == null)
            {
                return new XElement("section",d.Attributes("id"), d.Nodes());
            }
            else
            {
                if (Regex.IsMatch(first.Name.LocalName, @"h\d"))
                {
                    return new XElement("section"
                        , d.Attributes("id")
                        , new XElement("h" + (d.Ancestors("section").Count()+ 1).ToString(), first.Nodes())
                        , d.Elements().Skip(1)
                    );
                }
                else
                {
                    return new XElement("section", d.Attributes("id"), d.Nodes());
                }
            }
        }
        public static XElement ListClassReplace (this XElement e)
        {
            string className = ((string)e.Attributes("class").FirstOrDefault() ?? "").Trim().ToLower();
            if (!("ol;ul".Split(';').Contains(e.Name.LocalName) && className != "")) return e;
            string newName = "";
            switch (className)
            {
                case "k-alfa": newName = "list-lower-alpha"; break;
                case "k-strek": newName = "list-disc"; break;
                case "k-annet": newName = "list-enumeration"; break;
                case "k-bombe": newName = "list-disc"; break;
                case "k-ingen": newName = "list-enumeration"; break;
                case "k-num": newName = "list-decimal"; break;
                case "k-opprams": newName = "list-enumeration"; break;
                case "k-rom": newName = "list-lower-roman"; break;
                default:
                    newName = ""; break;

            }
            if (newName != "")
            {
                e.Attributes("class").FirstOrDefault().SetValue(newName);
            }
            return e;

        }
        public static IEnumerable<XElement> K_RAMME(this XElement e)
        {
            List<XElement> result = new List<XElement>();
            List<XElement> l = e.Elements().ToList();
            XElement figure = new XElement("figure", new XAttribute("role", "group"));
            foreach (XElement n in l)
            {
                if ("h2-k-tit-ramme".Split(';').Contains(n.ElementClassName()))
                {
                    figure.Add(new XElement("figcapiton", n.GetHeaderText()));
                }
                else if (n.ElementClassName() == "figure")
                {
                    figure.Add(n);
                }
                else if (((string)n.Attributes("class").FirstOrDefault()??"").Trim()=="")
                {
                    figure.Add(n);
                }
                else
                {

                }
            }

            result.Add(figure);
            return result;

        }
        public static IEnumerable<XElement> K_FIGGRP(this XElement e)
        {
            List<XElement> result = new List<XElement>();
            List<XElement> l = e.Elements().ToList();
            XElement figure = new XElement("figure");
            foreach (XElement n in l)
            {
                if ("h2-k-tit-figgrp".Split(';').Contains(n.ElementClassName()))
                {
                    figure.Add(new XElement("figcapiton", n.GetHeaderText()));
                }
                else if (n.ElementClassName()== "div-k-fig")
                {
                    foreach (XElement s in n.Elements())
                    {
                        if (s.ElementClassName()== "span-k-figname")
                        {
                            s.Elements("img").ToList().ForEach(p=> figure.Add(new XElement("img", p.Attributes("src"), p.Attributes("alt"))));
                        }
                    }
                }
                else if (n.ElementClassName() == "p-k-kilde")
                {
                    figure.Add(new XElement("p", new XElement("em", n.Nodes())));
                }
                else if (n.ElementClassName() == "span-k-note-fignote")
                {
                    List<XElement> notetext = n.Elements().Where(p => p.ElementClassName() == "span-k-notetext").ToList();
                    if (notetext.Count() > 0)
                    {
                        if (notetext.Elements("p").Count() > 0)
                        {
                            XElement dl = new XElement("dl", new XAttribute("class", "footnote-definitions"));
                            foreach (XElement p in notetext.Elements("p"))
                            {
                                XElement sup = p.Elements("sup").FirstOrDefault();
                                if (sup == null)
                                {
                                    XText first = p.DescendantNodes().OfType<XText>().FirstOrDefault();
                                    if (first != null)
                                    {
                                        string dltext = first.Value.TrimStart().Split(' ').FirstOrDefault();
                                        if (Regex.IsMatch(dltext.Trim(), @"^(\()?(\*+|\d\|[a-z])(\))?"))
                                        {
                                            first.ReplaceWith(new XText(first.Value.TrimStart().Substring(dltext.Length).TrimStart()));
                                            dl.Add(new XElement("dt", dltext.Trim()));
                                            dl.Add(new XElement("dd", p.Nodes()));
                                        }
                                        else
                                        {
                                            dl.Add(new XElement("dt", ""));
                                            dl.Add(new XElement("dd", p.Nodes()));
                                        }
                                    } 
                                }
                                else
                                { 
                                    if (sup != null)
                                        sup.Remove();
                                    XText t = p.DescendantNodes().OfType<XText>().FirstOrDefault();
                                    if (t != null)
                                        t.ReplaceWith(new XText(p.Value.TrimStart()));

                                    t = p.DescendantNodes().OfType<XText>().LastOrDefault();
                                    if (t != null)
                                        t.ReplaceWith(new XText(p.Value.TrimEnd()));
                                    dl.Add(new XElement("dt", sup.DescendantNodes().OfType<XText>().Select(s => s.Value).StringConcatenate().Trim()));
                                    dl.Add(new XElement("dd", p.Nodes()));
                                }
                            }
                            figure.Add(dl);
                        }
                    }

                }
                else
                {

                }
            }

            result.Add(figure);
            return result;
        }
        public static IEnumerable<XElement> K_TBL(this XElement e)
        {
            List<XElement> result = new List<XElement>();
            List<XElement> l = e.Elements().ToList();
            

            XElement figure = new XElement("figure");
            XElement before = e.NodesBeforeSelf().OfType<XElement>().Reverse().Take(1).FirstOrDefault();
            if (before == null ? false : "h2-k-tit-mutit;h2-k-tit-mtit".Split(';').Contains(before.ElementClassName()))
            {
                XElement figCapt = new XElement("figcapiton", before.GetHeaderText());
                figure.Add(figCapt);
                before.Remove();
            }

            foreach (XElement n in l)
            {
                if ("h2-k-title;h3-k-title".Split(';').Contains(n.ElementClassName()))
                {
                    figure.Add(new XElement("figcapiton", n.GetHeaderText()));
                }
                else if (n.Name.LocalName == "table")
                {

                    figure.Add(n);
                }
                else if (n.ElementClassName() == "span-k-note-tblnoter")
                {
                    List<XElement> notetext = n.Elements().Where(p => p.ElementClassName() == "span-k-notetext").ToList();
                    if (notetext.Count() > 0)
                    {
                        if (notetext.Elements("p").Count() > 0)
                        {
                            XElement dl = new XElement("dl", new XAttribute("class", "footnote-definitions"));
                            foreach (XElement p in notetext.Elements("p"))
                            {
                                XElement sup = p.Elements("sup").FirstOrDefault();
                                if (sup != null)
                                    sup.Remove();
                                XText t = p.DescendantNodes().OfType<XText>().FirstOrDefault();
                                if (t != null)
                                    t.ReplaceWith(new XText(p.Value.TrimStart()));

                                t = p.DescendantNodes().OfType<XText>().LastOrDefault();
                                if (t != null)
                                    t.ReplaceWith(new XText(p.Value.TrimEnd()));
                                dl.Add(new XElement("dt", sup==null ? "" : sup.DescendantNodes().OfType<XText>().Select(s => s.Value).StringConcatenate().Trim()));
                                dl.Add(new XElement("dd", p.Nodes()));
                                
                            }

                            figure.Add(dl);
                        }
                    }    
                }
                else if (n.ElementClassName() == "p-k-kilde")
                {
                    figure.Add(new XElement("p", new XElement("em", n.Nodes())));
                }
                else
                {
                    figure.Add(n);
                }
                    
            }
            result.Add(figure);
            return result;

        }
        public static string GetElementClass(this XElement e)
        {
            string className = (string)e.Attributes("class").FirstOrDefault() ?? "";
            if (className.Trim() == "") return "";
            string result = (className.Split(' ').TakeWhile(p => !p.StartsWith("krnl_")).FirstOrDefault() ?? "").Trim().ToLower();
            return result;
        }
        public static string ElementClassName(this XElement e)
        {
            string className = e.GetElementClass();
            return e.Name.LocalName + (className == "" ? "" : "-" + className);
        }
        public static IEnumerable<XElement> GetRegSection(this List<XElement> n)
        {
            List<XElement> result = new List<XElement>();

            XElement e = n.FirstOrDefault();
            XElement section = null;
            while (e != null)
            {
                string name = e.ElementClassName();
                if (name == "div-k-del-seksjon")
                {
                    section = new XElement("section", e.Attributes("id"), e.Nodes());
                    result.Add(section);
                }
                else
                {
                    result.Add(e);
                }
                e = n.SkipWhile(p => p != e).Skip(1).Take(1).FirstOrDefault();
            }
            return result;

           
        }
        
        public static IEnumerable<XElement> GetRegKapittel(this List<XElement> n)
        {
            List<XElement> result = new List<XElement>();
            XElement e = n.FirstOrDefault();
            XElement section = null;
            while (e != null)
            {
                string name = e.ElementClassName();
                if (name == "h2-k-tit-kapittel")
                {
                    section = new XElement("section", e.Attributes("id"), new XAttribute("segment", "true"), new XElement("h2", e.Nodes()));
                    List<XElement> l = n
                        .SkipWhile(p=>p!=e)
                        .Skip(1)
                        .TakeWhile(p => p.ElementClassName() != "h2-k-tit-kapittel")
                        .ToList();
                    section.Add(l.GetRegSection());
                    result.Add(section);
                    if(l.Count() != 0 )
                    {
                        e = n.SkipWhile(p => p != l.Last()).Skip(1).Take(1).FirstOrDefault();
                    }
                    else
                    {
                        e= n.SkipWhile(p => p != e).Skip(1).Take(1).FirstOrDefault();
                    }
                }
                else
                {
                    if (section != null)
                    {
                        section.Add(e);
                    }
                    else
                    {
                        result.Add(e);
                    }
                    e = n.SkipWhile(p => p != e).Skip(1).Take(1).FirstOrDefault();
                }
                
            }
            return result;
        }
        public static string GetHeaderText(this XElement h)
        {
            return Regex.Replace(
                    Regex.Replace(
                    h.DescendantNodes().Select(p =>
                        p.NodeType == XmlNodeType.Text
                        ? ((XText)p).Value
                        : (
                            p.NodeType == XmlNodeType.Element ? ((XElement)p).Name.LocalName : "") == "br"
                            ? " "
                            : ""
                        )
                        .StringConcatenate()
                    , @"\n", " ")
                , @"\s+", " ").Trim();
        }
        public static XElement MakeRegjeringenHierarky(this XElement root)
        {
            List<XElement> divblokksit = root.Descendants().Where(p => 
                p.ElementClassName() == "div-k-blokksit" 
                && (p.NodesBeforeSelf().OfType<XElement>().LastOrDefault() == null ? "" : p.NodesBeforeSelf().OfType<XElement>().LastOrDefault().ElementClassName()) != "div-k-blokksit"
                && (p.NodesAfterSelf().OfType<XElement>().FirstOrDefault() == null ? "" : p.NodesAfterSelf().OfType<XElement>().FirstOrDefault().ElementClassName()) == "div-k-blokksit"
            ).ToList(); 
            foreach (XElement d in divblokksit)
            {
                List<XElement> follow = d.NodesAfterSelf().OfType<XElement>().TakeWhile(p => p.ElementClassName() == "div-k-blokksit").ToList();
                XElement sitat = new XElement("section", new XAttribute("class", "sitat"));
                sitat.Add(new XElement("div", d.Nodes()));
                follow.ForEach(p =>sitat.Add(new XElement("div", p.Nodes())));
                follow.ForEach(p => p.Remove());
                d.ReplaceWith(sitat);
            }


            XElement document = new XElement("document");
            List<string> elementClassName = document.Descendants().Select(p => p.ElementClassName()).GroupBy(p => p).Select(p => p.Key).OrderBy(p => p).ToList();
            elementClassName.ForEach(p => Debug.Print(p));
            foreach (XElement s in root.Elements("section"))
            {
                XElement result = new XElement(s.Name.LocalName, s.Attributes());
                XElement test = s.Elements().FirstOrDefault();


                List<XElement> n = null;
                XElement section = null;
                string id = "";
                while (test != null)
                {

                    string name = test.ElementClassName();

                    switch (name)
                    {
                        case "h1":

                            result.Add(test);
                            test.Remove();
                            break;
                        case "p-k-a":
                            if (test.DescendantNodes().OfType<XElement>().Count() == 0 && test.DescendantNodes().OfType<XText>().Select(s => s.Value).StringConcatenate().Trim() == "")
                            {
                                test.Remove();
                            }
                            break;
                        case "h2-k-tit-refliste":
                            id = "refliste";
                            List<XElement> follow = test.NodesAfterSelf().OfType<XElement>().TakeWhile(p => p.ElementClassName() == "p-k-refliste").ToList();
                            test.Remove();
                            result.Add(new XElement("section",
                                new XAttribute("segment", "true"),
                                new XAttribute("default", "true"),
                                new XAttribute("id", id),
                                new XElement("h1", test.Nodes()),
                                 follow.Select(p => new XElement(p.Name.LocalName, p.Nodes()))
                                )
                            );
                            follow.ForEach(p => p.Remove());
                            break;
                        case "div-k-del-avsldel":
                        case "div-k-del-hoveddel":
                        case "div-k-ny-noukapnr":
                            if (test.Nodes().OfType<XElement>().Count() == 0)
                            {
                                test.Remove();

                            }
                            else
                            {

                            }
                            break;

                        case "div-k-del-forside":
                            id = "forside";
                            result.Add(new XElement("section",
                                new XAttribute("segment", "true"),
                                new XAttribute("default", "true"),
                                new XAttribute("id", id),
                                new XElement("h1", "Omslag"),
                                test.Nodes()
                                )
                            );
                            test.Remove();
                            break;
                        case "div-k-del-innldel":
                            id = "innldel";
                            if ((test.Nodes().OfType<XElement>().FirstOrDefault() == null ? "" : test.Nodes().OfType<XElement>().FirstOrDefault().ElementClassName()) == "div-k-del-innldel")
                            {
                                result.Add(new XElement("section",
                                    new XAttribute("segment", "true"),
                                    test.Attributes("id"),
                                    new XAttribute("id", id),
                                    new XElement("h1", "Innledning"),
                                    new XElement("table",
                                        new XElement("thead",
                                            new XElement("tr",
                                                new XElement("th", test.Nodes().OfType<XElement>().FirstOrDefault().Elements().Select(s => new XElement(s.Name.LocalName, s.Nodes())))
                                            )
                                        )
                                    )
                                    )
                                );
                            }
                            else
                            {
                                result.Add(new XElement("section",
                                    new XAttribute("segment", "true"),
                                    test.Attributes("id"),
                                    new XAttribute("id", id),
                                    new XElement("h1", "Innledning"),
                                    new XElement("table",
                                        new XElement("thead",
                                            new XElement("tr",
                                                new XElement("th", test.Elements().Select(s => new XElement(s.Name.LocalName, s.Nodes())))
                                            )
                                        )
                                    )
                                    )
                                );
                            }
                            test.Remove();
                            break;
                        case "h2-k-tit-oversendelse":
                            n = test
                               .NodesAfterSelf()
                               .OfType<XElement>()
                               .TakeWhile(p =>
                                   !(

                                        "h2-k-tit-kapittel"
                                       + ";h2-k-tit-refliste"
                                       + ";div-k-ny-noukapnr"
                                       + ";h2-k-tit-tilrstad"
                                       + ";h3-k-tit-tilrstad"
                                       + ";div-k-del-tilrpost"
                                       + ";div-k-del-kongeside"
                                       + ";h2-k-tit-vedtak"
                                       + ";h2-k-tit-vedlegg"
                                       + ";div-k-del-vedtakdeler"
                                       + ";h2-k-tit-vedlegg"
                                       + ";div-k-del-fotnoter"
                                   ).Split(';')
                                   .Contains(p.ElementClassName())
                               )
                               .ToList();


                            section = new XElement("section",
                                new XAttribute("segment", "true"),
                                new XAttribute("id", "oversendelse"),
                                new XElement("h1", test.GetHeaderText()
                                )
                            );
                            section.Add(n);
                            result.Add(section);
                            section = null;
                            test.Remove();
                            n.ForEach(p => p.Remove());
                            break;
                        case "h2-k-tit-del":
                            n = test
                                   .NodesAfterSelf()
                                   .OfType<XElement>()
                                   .TakeWhile(p =>
                                       !(
                                             "h2-k-tit-del"
                                           + ";h2-k-tit-refliste"
                                           + ";h2-k-tit-kapittel"
                                           + ";div-k-ny-noukapnr"
                                           + ";h2-k-tit-tilrstad"
                                           + ";h3-k-tit-tilrstad"
                                           + ";div-k-del-tilrpost"
                                           + ";div-k-del-kongeside"
                                           + ";h2-k-tit-vedtak"
                                           + ";h2-k-tit-vedlegg"
                                           + ";div-k-del-vedtakdeler"
                                           + ";h2-k-tit-vedlegg"
                                           + ";div-k-del-fotnoter"
                                       ).Split(';')
                                       .Contains(p.ElementClassName())
                                   )
                                   .ToList();
                            string del_id = Regex.Match(test.GetHeaderText(), @"Del\s(?<nr>(\d+))").Groups["nr"].Value;

                            section = new XElement("section",
                                new XAttribute("segment", "true"),
                                (del_id != "" ? new XAttribute("id", "del-" + del_id) : test.Attributes("id").FirstOrDefault()),
                                new XElement("h1", test.GetHeaderText()
                                )
                            );
                            section.Add(n.GetRegKapittel());
                            result.Add(section);
                            section = null;
                            test.Remove();
                            n.ForEach(p => p.Remove());
                            break;
                        case "h2-k-tit-kapittel":
                            n = test
                                    .NodesAfterSelf()
                                    .OfType<XElement>()
                                    .TakeWhile(p =>
                                        !(
                                              "h2-k-tit-kapittel"
                                            + ";h2-k-tit-refliste"
                                            + ";h2-k-tit-del"
                                            + ";div-k-ny-noukapnr"
                                            + ";h2-k-tit-tilrstad"
                                            + ";h3-k-tit-tilrstad"
                                            + ";div-k-del-tilrpost"
                                            + ";div-k-del-kongeside"
                                            + ";h2-k-tit-vedtak"
                                            + ";h2-k-tit-vedlegg"
                                            + ";div-k-del-vedtakdeler"
                                            + ";h2-k-tit-vedlegg"
                                            + ";div-k-del-fotnoter"
                                        ).Split(';')
                                        .Contains(p.ElementClassName())
                                    )
                                    .ToList();
                            string kap_id = Regex.Match(test.GetHeaderText().Trim(), @"(?<nr>(\d+))\s").Groups["nr"].Value;

                            section = new XElement("section",
                                new XAttribute("segment", "true"),
                                new XAttribute("id", "kap-" + kap_id),
                                new XElement("h1", test.GetHeaderText()),
                                n

                            );
                            result.Add(section);
                            section = null;
                            test.Remove();
                            n.ForEach(p => p.Remove());
                            break;
                        case "h2-k-tit-tilrstad":
                        case "h3-k-tit-tilrstad":
                            id = "tilrstad";
                            n = test
                                    .NodesAfterSelf()
                                    .OfType<XElement>()
                                    .TakeWhile(p =>
                                        !(
                                              "h2-k-tit-del"
                                            + ";h2-k-tit-refliste"
                                            + ";h2-k-tit-vedtak"
                                            + ";h2-k-tit-vedlegg"
                                            + ";div-k-del-vedtakdeler"
                                            + ";h2-k-tit-vedlegg"
                                            + ";div-k-del-fotnoter"
                                        ).Split(';')
                                        .Contains(p.ElementClassName())
                                    )
                                    .ToList();
                            section = new XElement("section",
                                new XAttribute("segment", "true"),
                                new XAttribute("id", id),
                                new XElement("h1", test.Attributes("id"),
                                test.GetHeaderText()));
                            section.Add(n);
                            result.Add(section);
                            section = null;
                            test.Remove();
                            n.ForEach(p => p.Remove());
                            break;

                        case "h2-k-tit-vedtak":
                            XElement next = test.NodesAfterSelf().OfType<XElement>().FirstOrDefault();
                            if (next != null)
                            {
                                if (next.ElementClassName() == "h2-k-tit-vedtak")
                                {
                                    id = "vedtak_" + (test.GetHeaderText().Split(' ').FirstOrDefault() ?? "").ToLower();
                                    section = new XElement("section",
                                        new XAttribute("id", id),
                                        new XAttribute("type", "vedtak"),
                                        new XAttribute("segment", "true"),
                                        new XElement("h1", test.GetHeaderText())
                                    );

                                    result.Add(section);
                                    test.Remove();
                                    section = null;
                                }
                                else
                                {
                                    section = new XElement("section",
                                        test.Attributes("id"),
                                        new XElement("h2", test.GetHeaderText())
                                    );
                                    n = test
                                    .NodesAfterSelf()
                                    .OfType<XElement>()
                                    .TakeWhile(p =>
                                        !(
                                              "h2-k-tit-del"
                                            + ";h2-k-tit-refliste"
                                            + ";h2-k-tit-vedtak"
                                            + ";h2-k-tit-vedlegg"
                                            + ";div-k-del-fotnoter"
                                        ).Split(';')
                                        .Contains(p.ElementClassName())
                                    )
                                    .ToList();
                                    section.Add(n);
                                    XElement parent = result.Elements("section").LastOrDefault();
                                    if ((parent == null ? "" : ((string)parent.Attributes("type").FirstOrDefault() ?? "")) == "vedtak")
                                    {
                                        parent.Add(section);
                                    }
                                    else
                                    {
                                        result.Add(section);
                                    }
                                    section = null;
                                    test.Remove();
                                    n.ForEach(p => p.Remove());
                                }
                            }
                            break;
                        case "h2-k-tit-vedlegg":
                            section = null;
                            id = "vedlegg_" + Regex.Match(test.GetHeaderText(), @"Vedlegg\s(?<nr>(\d+|[A-Z]))\s").Groups["nr"].Value.ToLower();
                            section = new XElement("section",
                                new XAttribute("segment", "true"),
                                new XAttribute("id", id),
                                new XAttribute("type", "vedlegg"),
                                new XElement("h1", test.GetHeaderText()
                                )
                            );
                            n = test
                            .NodesAfterSelf()
                            .OfType<XElement>()
                            .TakeWhile(p =>
                                !(
                                      "h2-k-tit-del"
                                    + ";h2-k-tit-refliste"
                                    + ";h2-k-tit-vedtak"
                                    + ";h2-k-tit-vedlegg"
                                    + ";div-k-del-fotnoter"
                                ).Split(';')
                                .Contains(p.ElementClassName())
                            )
                            .ToList();
                            section.Add(n.GetRegSection());
                            result.Add(section);
                            test.Remove();
                            n.ForEach(p => p.Remove());
                            break;
                        case "div-k-del-fotnoter":
                            section = null;
                            section = new XElement("footnotes", test.TransformFotnoter());
                            result.Add(section);
                            test.Remove();
                            break;
                        default:
                            Debug.Print(test.ElementClassName());

                            break;
                    }

                    test = s.Elements().FirstOrDefault();
                }
                document.Add(result);
            }
            
            return document;
        }
        public static void RemoveNameSpace(this XDocument d)
        {
            foreach (XElement e in d.Root.DescendantsAndSelf())
            {
                if (e.Name.Namespace != XNamespace.None)
                {
                    e.Name = XNamespace.None.GetName(e.Name.LocalName);
                }
                if (e.Attributes().Where(a => a.IsNamespaceDeclaration || a.Name.Namespace != XNamespace.None).Any())
                {
                    e.ReplaceAttributes(e.Attributes().Select(a => a.IsNamespaceDeclaration ? null : a.Name.Namespace != XNamespace.None ? new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value) : a));
                }
            }
        }
        public static string navPointContentSrc(this XElement navPoint)
        {
            return navPoint.Elements("content").Attributes("src").Select(p => p.Value).FirstOrDefault();
        }
            
        public static string navLabelText(this XElement navPoint)
        {
            return  Regex.Replace(Regex.Replace(navPoint.Elements("navLabel").Elements("text").Select(p => p.DescendantNodes().OfType<XText>().Select(s => s.Value).StringConcatenate().Trim()).FirstOrDefault(), "\n", " "), @"\s+", " ").Trim();
        }
        public static XElement LoadBodyElement(string filepath)
        {
            XDocument sd = XDocument.Load(filepath);
            sd.RemoveNameSpace();
            sd.Root.DescendantNodesAndSelf().OfType<XComment>().Remove();
            return sd.Root.Descendants("body").FirstOrDefault();
        }
        public static XElement ReplaceMarkupElementsEnd(this XElement document)
        {
            
            document.Descendants().Where(p => p.ElementClassName() == "div-k-defliste").ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
            document.Descendants().Where(p => p.ElementClassName() == "div-k-del-seksjon" && p.Parent.Name.LocalName == "section").ToList().ForEach(p => p.ReplaceWith(p.K_Section()));
            document.Descendants().Where(p => p.ElementClassName() == "div-k-del-subsek1" && p.Parent.Name.LocalName == "section").ToList().ForEach(p => p.ReplaceWith(p.K_Section()));
            document.Descendants().Where(p => p.ElementClassName() == "div-k-del-subsek2" && p.Parent.Name.LocalName == "section").ToList().ForEach(p => p.ReplaceWith(p.K_Section()));
            document.Descendants().Where(p => p.ElementClassName() == "div-k-del-subsek3" && p.Parent.Name.LocalName == "section").ToList().ForEach(p => p.ReplaceWith(p.K_Section()));
            document.Descendants().Where(p => p.ElementClassName() == "div-k-del-subsek4" && p.Parent.Name.LocalName == "section").ToList().ForEach(p => p.ReplaceWith(p.K_Section()));
            document.Descendants().Where(p => "div-k-del-tilrpost;div-k-del-kongeside".Split(';').Contains(p.ElementClassName())).ToList().ForEach(p => p.ReplaceWith(new XElement("section", p.Nodes())));
            document.Descendants().Where(p => "p-k-tilrdep;p-k-tilrar;p-k-konge;p-k-stadfester".Split(';').Contains(p.ElementClassName())).ToList().ForEach(p => p.ReplaceWith(new XElement("p", p.Nodes())));

            document.Descendants().Where(p => "div-k-del-vedtakdeler".Split(';').Contains(p.ElementClassName())).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
            document.Descendants().Where(p => "div-k-del-vedtakdel".Split(';').Contains(p.ElementClassName())).ToList()
                .ForEach(p => p.ReplaceWith(
                        new XElement("section",
                            Regex.IsMatch(p.Elements().FirstOrDefault().Name.LocalName, @"h\d") ? new XElement("h" + (p.Ancestors("section").Count() + 1).ToString(), p.Elements().FirstOrDefault().Nodes()) : null,
                            Regex.IsMatch(p.Elements().FirstOrDefault().Name.LocalName, @"h\d") ? p.Elements().Skip(1) : p.Elements()
                        )
                    )
                );

            document.Descendants().Where(p => p.ElementClassName() == "div-k-del-innldel").ToList().ForEach(p => p.ReplaceWith(new XElement("table", new XElement("thead", new XElement("tr", new XElement("th", p.Elements().Select(s => new XElement(s.Name.LocalName, s.Nodes()))))))));

            document
                .Descendants("section")
                .Where(p => p.Elements().Where(s => s.ElementClassName() == "div-k-del-lover").Count() > 0)
                .Elements()
                .Where(p => p.ElementClassName() == "div-k-del-lover")
                .Reverse()
                .ToList()
                .ForEach(p => p.K_DEL_LOVER(p.Ancestors("section").Count()));

            document.Descendants().Attributes("class").Where(p => p.Value.Trim().ToLower() == "k-ledd").ToList().ForEach(p => p.Remove());
            document.Descendants().Attributes("class").Where(p => p.Value.Trim().ToLower() == "k-lovpunktum").ToList().ForEach(p => p.Remove());
            document.Descendants().Attributes("class").Where(p => p.Value.Trim().ToLower() == "center").ToList().ForEach(p => p.Remove());
            document.Descendants().Attributes("class").Where(p => p.Value.Trim().ToLower() == "right").ToList().ForEach(p => p.Remove());
            document.Descendants().Attributes("class").Where(p => p.Value.Trim().ToLower() == "k-tbody").ToList().ForEach(p => p.Remove());
            document.Descendants().Attributes("class").Where(p => p.Value.Trim().ToLower() == "k-sup").ToList().ForEach(p => p.Remove());
            document.Descendants().Attributes("class").Where(p => p.Value.Trim().ToLower() == "k-term").ToList().ForEach(p => p.Remove());
            document.Descendants().Attributes("class").Where(p => p.Value.Trim().ToLower() == "k-def").ToList().ForEach(p => p.Remove());

            document.DescendantNodes().OfType<XText>().ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"\s+", " "))));

            document.Descendants().Where(p => p.ElementClassName() == "a-footnote").ToList().ForEach(p => p.ReplaceWith(
                    new XElement("a",
                        p.Attributes("id"),
                        p.Attributes("title"),
                        new XAttribute("class", "iref ifrs-footnote"),
                        new XAttribute("data-segment", "footnote"),
                        new XAttribute("data-id", ((string)p.Attributes("href").FirstOrDefault() ?? "").Split('#').LastOrDefault()),
                        new XElement("sup",
                            new XText(p.DescendantNodes().OfType<XText>().Select(s => s.Value).StringConcatenate().Trim())
                            )
                    )
                )
            );

            List<XElement> refliste = document.Descendants().Where(p =>
                p.ElementClassName() == "p-k-refliste"
                && (p.NodesBeforeSelf().OfType<XElement>().LastOrDefault() == null ? "" : p.NodesBeforeSelf().OfType<XElement>().LastOrDefault().ElementClassName()) != "p-k-refliste"
                && (p.NodesAfterSelf().OfType<XElement>().FirstOrDefault() == null ? "" : p.NodesAfterSelf().OfType<XElement>().FirstOrDefault().ElementClassName()) == "p-k-refliste"
            ).ToList();
            foreach (XElement r in refliste)
            {
                List<XElement> follow = r.NodesAfterSelf().OfType<XElement>().TakeWhile(p => p.ElementClassName() == "p-k-refliste").ToList();
                XElement refl = new XElement("ul", new XAttribute("class", "list-enumeration"));
                refl.Add(new XElement("li", r.Nodes()));
                follow.ForEach(p => refl.Add(new XElement("li", p.Nodes())));
                follow.ForEach(p => p.Remove());
                r.ReplaceWith(refl);
            }
            //husk å fjerne P i Li
            document.Descendants("p").Where(p =>
                "li;td".Split(';').Contains(p.Parent.Name.LocalName)
                && p.Parent.Nodes().OfType<XElement>().Where(a => a.Name.LocalName == "p").Count() == 1
                && p.Parent.Nodes().OfType<XElement>().Count() == (p.Parent.Nodes().OfType<XElement>().Where(a => a.Name.LocalName == "p").Count() + p.Parent.Nodes().OfType<XElement>().Where(a => a.Name.LocalName == "li").Count() + p.Parent.Nodes().OfType<XElement>().Where(a => a.Name.LocalName == "table").Count())
            ).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));


            return document;

        }
        public static XElement ReplaceMarkupElements(this XElement document)
        {

            List<XElement> divblokksit = document.Descendants().Where(p =>
                p.ElementClassName() == "div-k-blokksit"
                && (p.NodesBeforeSelf().OfType<XElement>().LastOrDefault() == null ? "" : p.NodesBeforeSelf().OfType<XElement>().LastOrDefault().ElementClassName()) != "div-k-blokksit"
                && (p.NodesAfterSelf().OfType<XElement>().FirstOrDefault() == null ? "" : p.NodesAfterSelf().OfType<XElement>().FirstOrDefault().ElementClassName()) == "div-k-blokksit"
            ).ToList();
            foreach (XElement d in divblokksit)
            {
                List<XElement> follow = d.NodesAfterSelf().OfType<XElement>().TakeWhile(p => p.ElementClassName() == "div-k-blokksit").ToList();
                XElement sitat = new XElement("section", new XAttribute("class", "sitat"));
                sitat.Add(new XElement("div", d.Nodes()));
                follow.ForEach(p => sitat.Add(new XElement("div", p.Nodes())));
                follow.ForEach(p => p.Remove());
                d.ReplaceWith(sitat);
            }


            document.Descendants().Where(p => !"img;iframe;br".Split(';').Contains(p.Name.LocalName) && p.Nodes().Count() == 0).ToList().ForEach(p => p.Remove());
            document.Descendants().Where(p => !"img;iframe;br".Split(';').Contains(p.Name.LocalName) && p.Descendants().Count() == 0 && p.DescendantNodes().OfType<XText>().Select(t=>t.Value).StringConcatenate().Trim()=="").ToList().ForEach(p => p.Remove());

            document.Descendants().Where(p => p.Attributes("ID").FirstOrDefault() != null).ToList().ForEach(p => p.ReplaceID());
            document.Descendants().Attributes("id").ToList().ForEach(p => p.SetValue(p.Value.ToLower().Trim()));

            document
            .Descendants().Where(p =>
                "span-k-note-fotnote;span-k-text".Split(';').Contains(p.ElementClassName())
            ).Reverse()
            .ToList()
            .ForEach(p => p.ReplaceWith(p.Nodes()));

            document
            .Descendants().Where(p =>
                "span-k-sperret;i-k-i;span-k-inline-endring".Split(';').Contains(p.ElementClassName())
            ).Reverse()
            .ToList()
            .ForEach(p => p.ReplaceWith(new XElement("em", p.Nodes())));

            document
            .Descendants().Where(p =>
                "b-k-b;span-k-tit-htit".Split(';').Contains(p.ElementClassName())
            ).Reverse()
            .ToList()
            .ForEach(p => p.ReplaceWith(new XElement("strong", p.Nodes())));

            

            document
            .Descendants().Where(p =>
                "div-k-pdflink".Split(';').Contains(p.ElementClassName())
            ).Reverse()
            .ToList()
            .ForEach(p => p.ReplaceWith(new XElement("p", p.Nodes())));


            document
            .Descendants().Where(p =>
                "div-k-blokksit".Split(';').Contains(p.ElementClassName())
            ).Reverse()
            .ToList()
            .ForEach(p => p.ReplaceWith(new XElement("blockquote", p.Nodes())));

            document
            .Descendants().Where(p =>
                "div-k-ny-a-leder".Split(';').Contains(p.ElementClassName())
            ).Reverse()
            .ToList()
            .ForEach(p => p.ReplaceWith(new XElement("p", new XElement("strong", p.Nodes()))));

            document
            .Descendants().Where(p =>
                "p-k-forfatter".Split(';').Contains(p.ElementClassName())
            ).Reverse()
            .ToList()
            .ForEach(p => p.ReplaceWith(new XElement("p", new XElement("em", p.Nodes()))));


            document.Descendants("table").Attributes("class").ToList().ForEach(p => p.Remove());
            document.Descendants("thead").Attributes("class").ToList().ForEach(p => p.Remove());
            document.Descendants("tr").Attributes("class").ToList().ForEach(p => p.Remove());
            document.Descendants().Where(p => "th;td".Split(';').Contains(p.Name.LocalName)).ToList().ForEach(p => p.SetTDClass());
            document.Descendants().Where(p =>
                      "span;p".Split(';').Contains(p.Name.LocalName)
                    && ("li;th;td".Split(';').Contains(p.Parent.Name.LocalName))
                    && (
                        p.Parent.Nodes().Count() == 1
                        ? (
                            p.Parent.Nodes().First().NodeType == XmlNodeType.Element
                            ? "span;p".Split(';').Contains(((XElement)p.Parent.Nodes().First()).Name.LocalName)
                            : false
                        )
                        : false)
                    )
                    .ToList().ForEach(p => p.ReplaceWith(p.Nodes()));

            document

            .Descendants().Where(p =>
                "li-k-normal;li-k-opprams".Split(';').Contains(p.ElementClassName())
            ).Reverse()
            .ToList()
            .ForEach(p => p.ReplaceWith(new XElement(p.Name.LocalName, p.Nodes())));

            document.Descendants().Attributes("alt").ToList().ForEach(p => p.SetValue(Regex.Replace(p.Value, @"\s+", " ").Trim()));
            document.Descendants("img").Attributes("src").ToList().ForEach(p => p.SetValue(Regex.Split(p.Value, @"[\\\/]").LastOrDefault().Trim()));


            document.Descendants().Where(p => p.ElementClassName() == "p-k-a").Reverse().ToList().ForEach(p => p.ReplaceWith(new XElement("p", p.Nodes())));
            document.Descendants("p").Where(p => p.DescendantNodes().OfType<XText>().Count() == 1).Select(p => p.DescendantNodes().OfType<XText>().FirstOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"[\r\n\s+]", " ").Trim())));
            document.Descendants("p").Where(p => p.DescendantNodes().OfType<XText>().Count() > 1).Select(p => p.DescendantNodes().OfType<XText>().FirstOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"[\r\n\s+]", " ").TrimStart())));
            document.Descendants("p").Where(p => p.DescendantNodes().OfType<XText>().Count() > 1).Select(p => p.DescendantNodes().OfType<XText>().LastOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"[\r\n\s+]", " ").TrimEnd())));

            document.Descendants("li").Where(p => p.DescendantNodes().OfType<XText>().Count() == 1).Reverse().Select(p => p.DescendantNodes().OfType<XText>().FirstOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"[\r\n\s+]", " ").Trim())));
            document.Descendants("li").Where(p => p.DescendantNodes().OfType<XText>().Count() > 1).Reverse().Select(p => p.DescendantNodes().OfType<XText>().FirstOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"[\r\n\s+]", " ").TrimStart())));
            //document.Descendants("li").Where(p => p.DescendantNodes().OfType<XText>().Count() > 1).Reverse().Select(p => p.DescendantNodes().OfType<XText>().LastOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"[\r\n\s+]", " ").TrimEnd())));

            document.Descendants("td").Where(p => p.DescendantNodes().OfType<XText>().Count() == 1).Select(p => p.DescendantNodes().OfType<XText>().FirstOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"[\r\n\s+]", " ").Trim())));
            document.Descendants("td").Where(p => p.DescendantNodes().OfType<XText>().Count() > 1).Select(p => p.DescendantNodes().OfType<XText>().FirstOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"[\r\n\s+]", " ").TrimStart())));
            document.Descendants("td").Where(p => p.DescendantNodes().OfType<XText>().Count() > 1).Select(p => p.DescendantNodes().OfType<XText>().LastOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"[\r\n\s+]", " ").TrimEnd())));

            document.Descendants("th").Where(p => p.DescendantNodes().OfType<XText>().Count() == 1).Select(p => p.DescendantNodes().OfType<XText>().FirstOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"[\r\n\s+]", " ").Trim())));
            document.Descendants("th").Where(p => p.DescendantNodes().OfType<XText>().Count() > 1).Select(p => p.DescendantNodes().OfType<XText>().FirstOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"[\r\n\s+]", " ").TrimStart())));
            document.Descendants("th").Where(p => p.DescendantNodes().OfType<XText>().Count() > 1).Select(p => p.DescendantNodes().OfType<XText>().LastOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"[\r\n\s+]", " ").TrimEnd())));

            document.Descendants().Where(p => "ol;ul".Split(';').Contains(p.Name.LocalName)).ToList().ForEach(p => p.ReplaceWith(p.ListClassReplace()));
            document.Descendants().Where(p => p.ElementClassName() == "div-k-tbl").ToList().ForEach(p => p.ReplaceWith(p.K_TBL()));
            document.Descendants().Where(p => p.ElementClassName() == "div-k-figgrp").ToList().ForEach(p => p.ReplaceWith(p.K_FIGGRP()));
            document.Descendants().Where(p => p.ElementClassName() == "div-k-ramme").ToList().ForEach(p => p.ReplaceWith(p.K_RAMME()));

            document.Descendants().Where(p => p.ElementClassName() == "div-k-del-medlemsliste").ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
            return document;
        }
        public static XElement NavPointIdentify(this NavPointDataEx npd, XElement container)
        {
            XElement h = container
                            .Descendants()
                            .Where(p => (string)p.Attributes("id").FirstOrDefault() == npd.id)
                            .FirstOrDefault();

            if (h == null)
            {
                h = container
                    .Descendants()
                    .Where(p =>
                        ((string)p.Parent.Attributes("id").FirstOrDefault() ?? "") == ""
                        && Regex.IsMatch(p.Name.LocalName, @"h\d")
                        && p.GetHeaderText().ToLower() == npd.text.ToLower()
                    ).FirstOrDefault();
            }
            return h;
        }

        public static void AddNavpointElements(this XElement section, IEnumerable<XElement> navPoint, XElement container)
        {
            foreach (XElement np in navPoint)
            {
                NavPointDataEx npd = np.Annotations<NavPointDataEx>().Select(p=>p).FirstOrDefault();
                if (npd == null) return;
                XElement e = npd.NavPointIdentify(container);
                IEnumerable<XElement> before = e.NodesBeforeSelf().OfType<XElement>();
                if (before.Count() > 0)
                {
                    section.Add(before);
                    before.ToList().ForEach(p => p.Remove());
                }
                XElement subSection = null;
                if (Regex.IsMatch(e.Name.LocalName,@"h\d"))
                {
                    subSection = new XElement("section", new XAttribute("id", npd.id), e);
                    string elementClassName = e.ElementClassName(); 
                    e.Remove();
                    if (navPoint.Elements("navPoint").Count()>0)
                    {
                        subSection.AddNavpointElements(navPoint.Elements("navPoint"), container);
                    }
                    if (container.Elements().Count()>0)
                    {
                        subSection.Add(container.Elements());
                        container.Elements().ToList().ForEach(p => p.Remove());
                    }
                    if (elementClassName == "h2-k-tit-kapittet")
                    {
                        subSection.Add(new XAttribute("segment", "true"));
                        List<XElement> vedlegg = subSection
                                           .Descendants()
                                           .Where(p =>
                                               p.ElementClassName() == "h2-k-tit-undertit" &&
                                               Regex.IsMatch(p.GetHeaderText().Trim().ToLower(), @"^vedlegg")
                                           )
                                           .ToList();
                        if (vedlegg.Count() > 0)
                        {
                            foreach (XElement v in vedlegg)
                            {
                                List<XElement> vl = v.NodesAfterSelf().OfType<XElement>().Where(p => !vedlegg.Contains(p)).ToList();
                                XElement newvedlegg = new XElement("section", new XElement("h2", v.GetHeaderText()), vl);
                                v.Remove();
                                vl.ForEach(p => p.Remove());
                                subSection.Add(newvedlegg);
                            }
                        }
                    }

                    section.Add(subSection);
                }
                else if (Regex.IsMatch(e.Name.LocalName, @"div"))
                {
                    subSection = new XElement("section", new XAttribute("id", npd.id));
                    XElement subContainer = new XElement("container", e.Nodes());
                    e.Remove();
                    if (navPoint.Elements("navPoint").Count() > 0)
                    {
                        subSection.AddNavpointElements(navPoint.Elements("navPoint"), subContainer );
                    }
                    if (subContainer.Elements().Count() > 0)
                    {
                        subSection.Add(subContainer.Elements());
                        subContainer.Elements().ToList().ForEach(p => p.Remove());
                    }
                    section.Add(subSection);
                }

            }
        }
        
        public static XElement TransformEpubRegEx(this string path)
        {
            XElement result = new XElement("root");
            XDocument toc = XDocument.Load(path + @"toc.ncx");
            toc.RemoveNameSpace();
            toc.Root.DescendantNodesAndSelf().OfType<XComment>().Remove();
            XElement navMap = toc.Root.Elements("navMap").FirstOrDefault();


            
            navMap.Descendants("navPoint").ToList().ForEach(p => p.AddAnnotation(new NavPointDataEx(p)));

            List<IGrouping<string, XElement>> navpoints = navMap
                    .Descendants("navPoint")
                    .GroupBy(p => p.Annotations<NavPointDataEx>().Select(a=>a.filename).FirstOrDefault())
                    .ToList();

            string HeaderClassName = "";
            string LastDel = "";

            foreach (IGrouping<string, XElement> n in navpoints)
            {
                string filename = n.Key;
                XElement body = LoadBodyElement(path + filename);
                XElement container = null;
                if (body != null)
                {
                    new XElement("html", body.Nodes()).Save(path.Replace(@"OEBPS\", @"xml\") + filename);
                    container = new XElement("container", body.Nodes());
                    container.ReplaceMarkupElements();
                }

                XElement first = n.Select(p => p).FirstOrDefault();
                
                XElement s = navMap
                    .Elements("navPoint")
                    .Where(p=>n.Contains(p))
                    .OrderBy(p=>p.Annotations<NavPointDataEx>().Select(a=>a.playOrder).FirstOrDefault())
                    .FirstOrDefault();

                s.Add(
                    s
                    .NodesAfterSelf().OfType<XElement>()
                    .Where(p =>
                        n.Contains(p)
                        && p.Name.LocalName == "navPoint"
                        && p.Annotations<NavPointDataEx>().Select(a => a.filename).FirstOrDefault() == filename
                        && Convert.ToInt32((string)p.Attributes("playOrder").FirstOrDefault() ?? "0") > Convert.ToInt32((string)s.Attributes("playOrder").FirstOrDefault() ?? "0")
                    )
                    .OrderBy(p => p.Annotations<NavPointDataEx>().Select(a => a.playOrder).FirstOrDefault())
                );
                s.Descendants("navPoint").ToList().ForEach(p => p.AddAnnotation(new NavPointDataEx(p)));
                if (s!=null)
                {

                    NavPointDataEx npd = s.Annotations<NavPointDataEx>().FirstOrDefault();
                    XElement h = npd.NavPointIdentify(container);

                    if (h==null)
                    {
                        if (npd.id.ToLower() == "omslag")
                        {
                            result.Add(new XElement("section", 
                                new XAttribute("default", "true"),
                                new XAttribute("segment", "true"), 
                                new XAttribute("id", npd.id),
                                new XElement("h1", npd.text),
                                container.Nodes()
                                )
                            );
                            container.Nodes().Remove();
                        }
                        else if (npd.id.ToLower() == "")
                        {
                        }
                    }
                    else
                    {
                        if (Regex.IsMatch(h.Name.LocalName, @"h\d") && h.Parent.Name.LocalName=="div" && h.Parent.ElementClassName()=="div-k-del-innldel")
                        {
                            XElement section = new XElement("section", new XAttribute("segment", "true"), new XAttribute("id", npd.id), new XElement("h2", npd.text));
                            section.Add(new XElement("table", new XElement("thead", new XElement("tr", new XElement("th", h.Parent.Elements().Select(p => new XElement(p.Name.LocalName, p.Nodes())))))));
                            h.Parent.Remove();
                            result.Add(section);
                        }
                        else if (Regex.IsMatch(h.Name.LocalName, @"h\d") && npd.id == "fot")
                        {
                            LastDel = "";
                            XElement footnotes = new XElement("footnotes",
                                container.TransformFotnoter()
                            );
                            result.Add(footnotes);
                        }

                        else if (Regex.IsMatch(h.Name.LocalName, @"h\d") && npd.id != "fot")
                        {

                            HeaderClassName = h.ElementClassName().Split('-').LastOrDefault();
                            
                            XElement section = new XElement("section", new XAttribute("segment", "true"), new XAttribute("id", npd.id), h);
                            h.Remove();
                            section.AddNavpointElements(s.Elements("navPoint"), container);
                            if (container.Elements().Count()>0)
                            {
                                section.Add(container.Elements());
                            }

                            switch (HeaderClassName)
                            {
                                case "del":
                                    LastDel = HeaderClassName;
                                    section.Add(new XAttribute("doc-type", HeaderClassName));
                                    result.Add(section);
                                    break;
                                case "vedtak":
                                    if (Regex.IsMatch((npd.text.Split(' ', '-', '–').FirstOrDefault()??"").Trim(),@"^[AB]$"))
                                    {
                                        LastDel = HeaderClassName;
                                        section.Add(new XAttribute("doc-type", HeaderClassName));
                                        result.Add(section);
                                    } 
                                    else
                                    {
                                        XElement parent = result.Elements("section").Where(p => (string)p.Attributes("doc-type").FirstOrDefault() == "vedtak").LastOrDefault();
                                        if (parent == null)
                                        {
                                            result.Add(section);
                                        }
                                        else
                                        {
                                            parent.Add(section);
                                        }
                                    }
                                    break;
                                case "kapittel":
                                    List<XElement> vedlegg = section
                                            .Descendants()
                                            .Where(p =>
                                                p.ElementClassName() == "h2-k-tit-undertit" &&
                                                Regex.IsMatch(p.GetHeaderText().Trim().ToLower(), @"^vedlegg")
                                            )
                                            .ToList();
                                    if (vedlegg.Count() > 0)
                                    {
                                        foreach (XElement v in vedlegg)
                                        {
                                            List<XElement> vl = v.NodesAfterSelf().OfType<XElement>().Where(p => !vedlegg.Contains(p)).ToList();
                                            XElement newvedlegg = new XElement("section", new XElement("h2", v.GetHeaderText()), vl);
                                            v.Remove();
                                            vl.ForEach(p => p.Remove());
                                            section.Add(newvedlegg);
                                        }
                                    }

                                    if (LastDel == "del")
                                    {
                                        XElement parent = result.Elements("section").Where(p => (string)p.Attributes("doc-type").FirstOrDefault() == "del").LastOrDefault();

                                        

                                        if (parent == null)
                                        {
                                            result.Add(section);
                                        }
                                        else
                                        {
                                            parent.Add(section);
                                        }
                                    }
                                    else
                                    {
                                        result.Add(section);
                                    }
                                    break;
                                case "refliste":
                                case "oversendelse":
                                case "vedlegg":
                                case "tilrstad":
                                    LastDel = "";
                                    if (section.Descendants().Where(p => p.ElementClassName() == "div-k-del-vsubsek").Count() > 0)
                                    {
                                        List<XElement> vv = section.Descendants().Where(p => p.ElementClassName() == "div-k-del-vsubsek").ToList();
                                        vv.ForEach(p => p.Remove());
                                        result.Add(section);
                                        int nv = 1;
                                        foreach (XElement xv in vv)
                                        {
                                            XElement x = xv.K_Section();
                                            x.Add(new XAttribute("segment", "true"));
                                            x.Add(new XAttribute("id", "addon" + nv.ToString()));
                                            result.Add(x);
                                        }
                                    }
                                    else
                                    {
                                        result.Add(section);
                                    }
                                    
                                    break;
                                case "budkap":
                                    LastDel = "";
                                    section.Descendants().Where(p => p.ElementClassName() == "div-k-del-post" && p.Parent.Name.LocalName == "section").ToList().ForEach(p => p.ReplaceWith(p.K_Section()));
                                    result.Add(section);
                                    break;
                                case "ordforkl":
                                    LastDel = "";
                                    result.Add(section);
                                    break;
                                default:
                                    LastDel = "";
                                    result.Add(section);
                                    break;
                            }

                            
                        }
                        else if (Regex.IsMatch(h.Name.LocalName, @"div"))
                        {
                            XElement section = new XElement("section", new XAttribute("id", npd.id));
                            XElement subContainer = new XElement("container", h.Nodes());
                            h.Remove();
                            section.AddNavpointElements(s.Elements(), subContainer);
                            if (subContainer.Elements().Count() > 0)
                            {
                                section.Add(subContainer.Elements());
                                subContainer.Elements().ToList().ForEach(p => p.Remove());
                            }
                            result.Add(section);
                        }
                        else
                        {

                        }
                        
                    }
                }
            }

            result = result.ReplaceMarkupElementsEnd();
            result.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d") && p == p.Parent.Elements().FirstOrDefault()).ToList().ForEach(p => p.ReplaceWith(new XElement("h" + p.Ancestors("section").Count(), p.GetHeaderText().Trim())));
            result = result.MakeSubSections();

            return new XElement("document", result.Nodes());
        }
        public static XElement TransformEpubReg(this string path)
        {
            XDocument toc = XDocument.Load(path + @"toc.ncx");
            foreach (XElement e in toc.Root.DescendantsAndSelf())
            {
                if (e.Name.Namespace != XNamespace.None)
                {
                    e.Name = XNamespace.None.GetName(e.Name.LocalName);
                }
                if (e.Attributes().Where(a => a.IsNamespaceDeclaration || a.Name.Namespace != XNamespace.None).Any())
                {
                    e.ReplaceAttributes(e.Attributes().Select(a => a.IsNamespaceDeclaration ? null : a.Name.Namespace != XNamespace.None ? new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value) : a));
                }
            }

            XElement navMap = toc.Descendants("navMap").FirstOrDefault();

            XElement nav = new XElement("toc", navMap.GetNavpiontData());
            if (!Directory.Exists(path + @"\xml\")) Directory.CreateDirectory(path + @"\xml\");

            nav.Descendants("navpoint").ToList().ForEach(p => p.AddAnnotation(new NavPointData((string)p.Attributes("src").FirstOrDefault())));

            XElement segments = new XElement("segments",

                nav.Descendants().Where(p => p.Annotation<NavPointData>() != null)
                .Where(p => p.Annotation<NavPointData>().id == "")
                .GroupBy(p => p.Annotation<NavPointData>().segmentname)
                .Select(p => p.LastOrDefault())
                .Select(p => new XElement(p.Name.LocalName, p.Attributes()))
            );

            XElement result = new XElement("segments");


            foreach (XElement segment in segments.Elements())
            {
                XElement s = new XElement("segment", new XElement("bookcontent", segments.Nodes()));
                string src = (string)segment.Attributes("src").FirstOrDefault();
                string id = (string)segment.Attributes("id").FirstOrDefault();
                string playOrder = (string)segment.Attributes("playOrder").FirstOrDefault();
                string title = (string)segment.Attributes("title").FirstOrDefault();
                if (playOrder == "1")
                {
                    s.Add(new XAttribute("id", "_top"));
                }
                else
                {
                    s.Add(segment.Attributes("id"));
                }

                XDocument sd = XDocument.Load(path + src);
                sd.Save(path + src);

                foreach (XElement e in sd.Root.DescendantsAndSelf())
                {
                    if (e.Name.Namespace != XNamespace.None)
                    {
                        e.Name = XNamespace.None.GetName(e.Name.LocalName);
                    }
                    if (e.Attributes().Where(a => a.IsNamespaceDeclaration || a.Name.Namespace != XNamespace.None).Any())
                    {
                        e.ReplaceAttributes(e.Attributes().Select(a => a.IsNamespaceDeclaration ? null : a.Name.Namespace != XNamespace.None ? new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value) : a));
                    }
                }
                sd.Root.DescendantNodesAndSelf().OfType<XComment>().Remove();
                XElement ee = s.Descendants().Where(p => (string)p.Attributes("id").FirstOrDefault() == id).FirstOrDefault();
                XElement body = sd.Root.Descendants("body").FirstOrDefault();
                XElement header = body.Elements("h2").Where(p => p.DescendantNodes().OfType<XText>().Select(r => r.ToString()).StringConcatenate() == title).FirstOrDefault();

                ee.Add(body);

                s.DescendantsAndSelf().Attributes("id").ToList().ForEach(p => p.SetValue(p.Value.ToLower()));
                s.DescendantNodesAndSelf().OfType<XText>().ToList().ForEach(p => p.ReplaceWith(new XText(p.Value.ReplaceTocText())));
                result.Add(s);

            }

            return result;
        }
    }
}
