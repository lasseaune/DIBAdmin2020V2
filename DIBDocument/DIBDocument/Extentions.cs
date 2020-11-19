using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using System.Globalization;


namespace DIBDocument
{
    public class TagRef
    {
        public string topic_id { get; set; }
        public string id { get; set; }
        public string cid { get; set; }
    }
    public class Tags
    {
        public XElement tag1 { get; set; }
        public XElement tag2 { get; set; }
        public XElement tag3 { get; set; }
        public Tags() { }
    }
    public class IdLinkElement
    {
        public string id { get; set; }
        public string topic_id { get; set; }
        public string language { get; set; }

        public IdLinkElement(string Id, string TopicId, string Language)
        {
            id = Id;
            topic_id = TopicId;
            language = Language;
        }
    }
    public class IdLinksElement
    {
        public IdLinkElement linkelement { get; set; }

        public IdLinksElement(XElement idlinks, XElement topics, string language)
        {
            List<IdLinkElement> idlinkelement = (
                from i in idlinks.Elements("idlink").Elements("topic")
                join t in topics.Elements("topic")
                on (string)i.Attributes("topic_id").FirstOrDefault() equals (string)t.Attributes("topic_id").FirstOrDefault()
                select new IdLinkElement(
                    (string)i.Ancestors("idlinks").Attributes("id").FirstOrDefault(),
                    (string)t.Attributes("topic_id").FirstOrDefault(),
                    (string)t.Attributes("language").FirstOrDefault())
            ).ToList();

            if (idlinkelement.Count() == 1)
                linkelement = idlinkelement.FirstOrDefault();
            else if (idlinkelement.Where(p => p.language == language).Count() == 1)
                linkelement = idlinkelement.Where(p => p.language == language).FirstOrDefault();

        }
    }

    public class ElementResult
    {
        public List<XElement> elements { get; set; }
        public string value { get; set; }
        public string Error { get; set; }
        public ElementResult() { }
    }
    public class ElementSets
    {
        //public List<ElementSet> elementsets = new List<ElementSet>();
        public List<string> elementsetvalue = new List<string>();
        public string Error { get; set; }
        public ElementSets(Match m, XElement companydata)
        {
            int captures = m.Groups["elementset"].Captures.Count;
            if (captures == 1)
            {
                Capture c = m.Groups["elementset"].Captures[0];
                ElementSet elementset = new ElementSet(c, m, companydata, null);
                //elementsets.Add(new ElementSet(c, m, companydata, null));
                if (elementset.elementresult != null)
                {
                    if (elementset.elementresult.Error != null)
                    {
                        Error = elementset.elementresult.Error;
                        return;
                    }

                    if (elementset.elementresult.value != null)
                        elementsetvalue.Add(elementset.elementresult.value);
                    else if (elementset.elementresult.elements.Count() != 0)
                        elementsetvalue.AddRange(elementset.elementresult.elements.Select(p => p.Value).ToList());
                    else if (elementset.elementresult.Error != null)
                        Error = elementset.elementresult.Error;
                }
            }
            else if (captures > 1)
            {
                foreach (Capture ct in m.Groups["elementset"].Captures)
                {
                    if (ct.Value == "hvor")
                    {
                        Error = "Feil i spørring";
                        return;
                    }
                }
                XElement parent = null;
                Capture c = m.Groups["elementset"].Captures[0];
                ElementSet es = new ElementSet(c, m, companydata, null);
                if (es.elementresult != null)
                {
                    if (es.elementresult.Error != null)
                    {
                        Error = es.elementresult.Error;
                        return;
                    }
                    foreach (XElement e in es.elementresult.elements)
                    {
                        parent = e;
                        string value = e.Value;
                        for (int i = 1; i < captures; i++)
                        {
                            c = m.Groups["elementset"].Captures[i];
                            ElementSet sub = new ElementSet(c, m, companydata, e.Parent);
                            value += sub.prefix + sub.elementresult.elements.Select(p => p.Value).FirstOrDefault();
                        }
                        elementsetvalue.Add(value);
                    }
                }
            }
        }
    }
    public class ElementSet
    {
        public Capture prefix { get; set; }
        Capture ename { get; set; }
        Capture where { get; set; }
        Capture pname { get; set; }
        Capture pvalue { get; set; }
        public ElementResult elementresult { get; set; }
        public ElementSet(Capture c, Match m, XElement companydata, XElement parent)
        {
            prefix = m.CaptureGet(c, "prefix");
            ename = m.CaptureGet(c, "ename");
            where = m.CaptureGet(c, "where");
            if (where != null)
            {
                pname = m.CaptureGet(where, "pname");
                pvalue = m.CaptureGet(where, "pval");
                elementresult = ename.Value.ElementResultGet(companydata, parent, pname.Value, pvalue.Value);
            }
            else
            {
                elementresult = ename.Value.ElementResultGet(companydata, parent);
            }
        }
    }
    public class XVarName
    {
        public string name { get; set; }
    }
    public class TextRange
    {
        public int pos { get; set; }
        public int length { get; set; }
    }
    public class Heading
    {
        public XElement Element { get; set; }
        public int Level { get; set; }
        public Heading(XElement h)
        {
            Element = h;
            Level = Convert.ToInt32(Regex.Match(h.Name.LocalName, @"h(?<n>(\d+))").Groups["n"].Value);
        }
    }
    public class ItemSearch
    {
        public string score { get; set; }
        public ItemSearch() { }
    }
    public class UniqueAtt
    {
        public string key { get; set; }
        public List<XAttribute> l = new List<XAttribute>();
    }
    public class CurrentSegment
    {
        public string segment_id { get; set; }
    }
    public class TopSegment
    {
        public string segment_id { get; set; }
        public string id { get; set; }
    }
    public class SegmentPackData
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string SegmentId { get; set; }

    }
    public static class Extentions
    {
        public static List<string> GetExpressionsInDocument(this string s, Regex r)
        {
            List<string> result = new List<string>();
            s = Regex.Replace(s, @"\s+", "");
            MatchCollection mc = r.Matches(s);
            foreach (Match m in mc)
            {
                result.Add(m.Groups["value"].Value);
            }
            return result;
        }
        public static IEnumerable<XNode> SplitImgPara(this XElement e)
        {
            List<XNode> n = new List<XNode>();
            string elementName = e.Name.LocalName;
            if (e.Nodes().Count() == 1 && (e.Nodes().OfType<XElement>().Count() == 1 ? ((XElement)e.Nodes().OfType<XElement>().FirstOrDefault()).Name.LocalName : "") == "img")
            {
                n.Add(e.Nodes().FirstOrDefault());
            }
            else
            {
                while (e.Nodes().Count() != 0)
                {
                    List<XNode> pn = new List<XNode>();
                    pn.AddRange(
                        e.Nodes()
                        .TakeWhile(p =>
                            p.NodeType == XmlNodeType.Element
                            ? ((XElement)p).DescendantNodesAndSelf().OfType<XElement>().Where(s => s.Name.LocalName == "img").Count() == 0
                            : true
                        )
                    );
                    XNode img = e
                        .Nodes()
                        .SkipWhile(p =>
                            p.NodeType == XmlNodeType.Element
                            ? ((XElement)p).DescendantNodesAndSelf().OfType<XElement>().Where(s => s.Name.LocalName == "img").Count() == 0
                            : true
                        )
                        .Take(1).FirstOrDefault();
                    if (img == null)
                    {
                        if (pn.Count() != 0)
                        {
                            n.Add(new XElement(elementName, pn));
                            pn.ForEach(p => p.Remove());
                        }
                    }
                    else if ((img.NodeType == XmlNodeType.Element ? ((XElement)img).Name.LocalName : "") == "img")
                    {
                        if (pn.Count() != 0)
                        {
                            n.Add(new XElement(elementName, pn));
                            pn.ForEach(p => p.Remove());
                        }
                        n.Add(img);
                        img.Remove();
                    }
                    else
                    {
                        XElement e1 = ((XElement)img);
                        List<XNode> n1 = e1.SplitImgPara().ToList();
                        XElement current = null;
                        foreach (XNode nn in n1)
                        {
                            if (nn.NodeType == XmlNodeType.Element)
                            {
                                XElement e2 = (XElement)nn;
                                if (e2.Name.LocalName == "img")
                                {
                                    if (current != null)
                                    {
                                        n.Add(new XElement(elementName, current));
                                        current = null;
                                    }
                                    n.Add(nn);
                                }
                                else
                                {
                                    if (current == null)
                                    {
                                        current = new XElement(e1.Name.LocalName);
                                    }
                                    current.Add(nn);
                                }
                            }
                            else if (nn.NodeType == XmlNodeType.Text)
                            {
                                if (current == null)
                                {
                                    current = new XElement(e1.Name.LocalName);
                                }
                                current.Add(nn);
                            }
                        }
                        if (current != null) n.Add(new XElement(elementName, current));
                        img.Remove();
                    }
                }
            }
            return n;
        }
        public static SegmentPackData GetSegmentPackData(this XElement e)
        {
            XElement segment = e.AncestorsAndSelf().Where(p => (string)p.Attributes("segment").FirstOrDefault() == "true").FirstOrDefault();
            if (segment == null)
                return new SegmentPackData { Name = "", SegmentId = "", Id = "" };

            string segment_id = (string)segment.Attributes("id").FirstOrDefault();
            string id = "";
            string name = "";
            if (Regex.IsMatch(segment_id, @"^pack\d+$"))
            {
                XElement element = segment.Elements().Where(p => p.Attributes("title").FirstOrDefault() != null).FirstOrDefault();
                if (element != null)
                {
                    name = (string)segment.Attributes("title").FirstOrDefault() + " - " + (string)element.Attributes("title").FirstOrDefault();
                    id = (string)element.Attributes("id").FirstOrDefault();
                }
            }
            else if (segment.Parent != null)
            {
                if ((string)segment.Parent.Attributes("segment").FirstOrDefault() == "true" && Regex.IsMatch(((string)segment.Parent.Attributes("id").FirstOrDefault() ?? ""), @"^pack\d+$"))
                {
                    name = ((string)segment.Parent.Attributes("title").FirstOrDefault() ?? "") + " - " + (string)segment.Attributes("title").FirstOrDefault();
                }
                else
                {
                    name = (string)segment.Attributes("title").FirstOrDefault();
                }

                id = (string)segment.Attributes("id").FirstOrDefault();
            }
            else
            {
                name = (string)segment.Attributes("title").FirstOrDefault();
                id = (string)segment.Attributes("id").FirstOrDefault();
            }

            return new SegmentPackData { Name = name, SegmentId = segment_id, Id = id };
        }
        public static XAttribute AttributeOfName(this IEnumerable<XAttribute> a, string names)
        {
            return a.Select(p => p).Where(p => p.AttributeNameEqval(names)).FirstOrDefault();
        }
        public static bool AttributeNameEqval(this XAttribute a, string names)
        {
            return names.Split(';').Contains(a.Name.LocalName);
        }

        public static IEnumerable<XNode> GetIndexItems(this XElement e, string segment_id)
        {
            List<XNode> result = new List<XNode>();
            bool segment = "true;1".Split(';').Contains(
                ((string)e.Attributes().AttributeOfName("segment;s") ?? "").Trim().ToLower()
            );


            string sid = (string)e.Attributes().AttributeOfName("segment_id");
            string id = (string)e.Attributes().AttributeOfName("id;key");
            if (e.NodesBeforeSelf().OfType<XElement>().Where(p => (string)p.Attributes().AttributeOfName("key;id") == id).Count() != 0) return result;
            string title = ((string)e.Attributes().AttributeOfName("text;title") ?? "").Trim();
            bool cs = e.Annotation<CurrentSegment>() == null ? false : e.Annotation<CurrentSegment>().segment_id == id;
            TopSegment ts = e.Annotation<TopSegment>() == null ? null : e.Annotation<TopSegment>();
            if (cs)
            {
                result.Add(new XElement("item",
                    ts == null ? new XAttribute("id", id) : new XAttribute("id", ts.segment_id),
                    new XAttribute("title", title),
                    ts == null ? new XAttribute("s", "1") : null,
                    e.Elements().SelectMany(p => p.GetIndexItems(sid))
                    )
                );
                return result;
            }
            else if (ts != null)
            {
                result.Add(new XElement("item",
                    new XAttribute("id", ts.segment_id),
                    new XAttribute("title", title),
                    new XAttribute("s", "1")
                    )
                );
                return result;

            }
            else if (segment &&
                (
                    e.Descendants().Where(p => (string)p.Attributes().AttributeOfName("key;id") == segment_id).Count() != 0
                || e.Ancestors().Where(a => (a.Annotation<CurrentSegment>() == null ? "" : a.Annotation<CurrentSegment>().segment_id) != "").Count() != 0
                )
            )
            {
                result.Add(new XElement("item",
                    new XAttribute("id", id),
                    new XAttribute("title", title),
                    new XAttribute("s", "1"),
                    e.Elements().SelectMany(p => p.GetIndexItems(segment_id))
                    )
                );
                return result;
            }
            else if (segment && segment_id.Trim().ToLower() == sid && id == sid)
            {
                result.Add(new XElement("item",
                    new XAttribute("id", id),
                    new XAttribute("title", title),
                    new XAttribute("s", "1"),
                    e.Elements().SelectMany(p => p.GetIndexItems(segment_id))
                    )
                );
                return result;
            }
            else if (segment && segment_id.Trim().ToLower() != sid)
            {
                result.Add(new XElement("item",
                    new XAttribute("id", id),
                    new XAttribute("title", title),
                    new XAttribute("s", "1")
                    )
                );
                return result;

            }
            else if (!segment && title != "" && sid == segment_id)
            {
                result.Add(new XElement("item",
                    new XAttribute("id", id),
                    new XAttribute("title", title),
                    e.Elements().SelectMany(p => p.GetIndexItems(segment_id))
                    )
                );
                return result;
            }
            return result;
        }
        public static IEnumerable<XNode> LevelItem(this XNode n)
        {
            List<XNode> result = new List<XNode>();
            if (n.NodeType == XmlNodeType.Text)
                result.Add(n);
            else if (n.NodeType == XmlNodeType.Element)
            {
                XElement e = (XElement)n;
                if (e.Name.LocalName == "level" && (string)e.Attributes("segment").FirstOrDefault() == "true")
                {
                    result.Add(new XElement(e.Name.LocalName, e.Attributes(), e.Elements("title")));
                }
                else
                {
                    result.Add(new XElement(e.Name.LocalName, e.Attributes(), e.LevelItems()));
                }
            }
            return result;
        }
        public static IEnumerable<XNode> LevelItems(this XElement e)
        {
            return e.Nodes().SelectMany(p => p.LevelItem());

        }
        public static XElement xlinks(this XElement diblink)
        {
            XElement xlinks = new XElement("x-links",
                diblink
                .Elements("idlinks")
                .Where(p => (string)p.Elements("idlink").Elements("tags").Attributes("RegexName").FirstOrDefault() != "link")
                .Where(p =>
                        p.Elements("idlink").Count() == 1
                    && ((string)p.Elements("idlink").Elements("topic").Attributes("topic_id").FirstOrDefault() ?? "") != ""
                    && (
                        p.Elements("idlink").Elements("topic").Elements("select").Count() == 0
                        ||
                        (string)p.Elements("idlink").Elements("topic").Elements("select").Attributes("select_id").FirstOrDefault() == (string)p.Elements("idlink").Elements("topic").Attributes("topic_id").FirstOrDefault()
                        )
                    && (
                        p.Elements("idlink").Elements("topic").Elements("segment").Elements("select").Count() == 0
                        ||
                        (string)p.Elements("idlink").Elements("topic").Elements("segment").Attributes("segment_id").FirstOrDefault() == (string)p.Elements("idlink").Elements("topic").Elements("segment").Elements("select").Attributes("select_id").FirstOrDefault())
                )
                .Elements("idlink")
                .Union(

                    diblink
                    .Elements("idlinks")
                    .Where(p =>
                        (string)p.Elements("idlink").Elements("tags").Attributes("RegexName").FirstOrDefault() == "link"
                        && ((string)p.Elements("idlink").Elements("topic").Attributes("topic_id").FirstOrDefault() ?? "") != ""
                        && ((string)p.Elements("idlink").Elements("tags").Attributes("tag1").FirstOrDefault() ?? "").Split(';').Count() > 2
                    )
                    .Elements("idlink")
                )
                .GroupBy(p => (string)p.Elements("topic").Attributes("topic_id").FirstOrDefault())
                .Select(p => new XElement("x-link",
                        new XAttribute("topic_id", p.Key),
                        p.Select(s => new XElement("ref",
                                new XAttribute("id", (string)s.Ancestors("idlinks").Attributes("id").FirstOrDefault()),
                                new XAttribute("type", (string)s.Elements("tags").Attributes("RegexName").FirstOrDefault() ?? ""),
                                //new XAttribute("n", ((string)s.Elements("tags").Attributes("tag1").FirstOrDefault() ?? "").Split(';').Count().ToString()),
                                (
                                    ((string)s.Elements("tags").Attributes("RegexName").FirstOrDefault() ?? "") == "link"
                                    &&
                                    ((string)s.Elements("tags").Attributes("tag1").FirstOrDefault() ?? ";;;").Split(';').ElementAt(2).ToString() != ""
                                )
                                ? new XAttribute("bm", ((string)s.Elements("tags").Attributes("tag1").FirstOrDefault() ?? ";;;").Split(';').ElementAt(2).ToString())
                                : null
                            )
                        )
                    )
                )
            );
            return xlinks;

        }
        public static XElement MapHtmlElement(this XElement e)
        {
            if (e == null) return null;
            return new XElement("item",
                e.Attributes("id"),
                e.Attributes("index"),
                ((string)e.Attributes("segment").FirstOrDefault() ?? "false") == "true" ? new XAttribute("s", "1") : null,
                e.Name.LocalName == "section" && e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).Count() != 0
                ? (
                    ((string)e.Attributes("content_title").FirstOrDefault() ?? "") == ""
                    ? new XAttribute("title", e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).Select(p => p.DescendantNodes().TakeWhile(s => (s.NodeType == XmlNodeType.Element ? ((XElement)s).Name.LocalName : "") != "br").OfType<XText>().Where(s => s.Parent.AncestorsAndSelf("sup").FirstOrDefault() == null).Select(s => s.Value).StringConcatenate()).FirstOrDefault() ?? "")
                    : new XAttribute("title", (string)e.Attributes("content_title").FirstOrDefault())
                  )
                : null,
                e.Elements().Select(p => p.MapHtmlElement())
            );
        }
        public static IEnumerable<XElement> LevelMapItem(this XElement e)
        {
            List<XElement> result = new List<XElement>();
            switch (e.Name.LocalName)
            {

                case "title":
                    return result;
                case "level":
                    result.Add(new XElement("item",
                        e.Attributes("id"),
                        e.Attributes("segment"),
                        new XAttribute("title", e.Elements("title").Nodes().OfType<XText>().Select(s => s.Value).StringConcatenate()),
                        e.LevelMap()
                        )
                    );
                    break;
                default:
                    result.Add(new XElement("item",
                        e.Attributes("id"),
                        e.LevelMap()
                        )
                    );
                    break;
            }
            return result;
        }
        public static IEnumerable<XElement> LevelMap(this XElement e)
        {
            if (e == null) return null;
            return e.Elements().SelectMany(p => p.LevelMapItem());
        }
        public static void SetAttributeValueEx(this XElement e, string name, string value)
        {
            XAttribute att = e.Attributes(name).FirstOrDefault();
            if (att != null)
            {
                att.SetValue(value);
            }
            else
            {
                e.Add(new XAttribute(name, value));
            }
        }
        public static ElementResult ElementResultGet(this string name, XElement companydata, XElement parent = null, string pname = null, string pvalue = null)
        {
            ElementResult result = new ElementResult();
            try
            {
                if (name == null) return null;
                bool count = (name.Split('.').LastOrDefault() ?? "").ToLower() == "count";
                bool sum = (name.Split('.').LastOrDefault() ?? "").ToLower() == "sum";
                List<string> names = name.Split('.').Where(p => !"sum;count".Split(';').Contains(p.ToLower())).ToList();
                List<XElement> l = new List<XElement>();
                l.Add(companydata);
                IEnumerable<XElement> cd = l;
                string ename = "";
                foreach (string s in names)
                {
                    if (cd.Count() == 0)
                        break;
                    cd = cd.Elements().Where(p => p.Name.LocalName.ToLower() == s.Trim().ToLower());
                    ename = s;
                }
                if (cd.Count() == 0)
                {
                    result.Error = "Ingen elementer funnet";
                    return result;
                }
                if (cd.Where(p => p.Name.LocalName.ToLower() == ename).Count() != cd.Count())
                {
                    result.Error = "Feil antall elementer med name=" + ename;
                    return result;
                }

                if (parent != null)
                {
                    cd = cd.Where(p => p.Parent == parent);
                    if (cd.Count() == 0)
                    {
                        result.Error = "Ingen elementer funnet med gjeldende forelder";
                        return result;
                    }
                    if (cd.Where(p => p.Name.LocalName.ToLower() == ename).Count() != cd.Count())
                    {
                        result.Error = "Feil antall elementer funnet med gjeldende forelder";
                        return result;
                    }
                }



                if ((pname == null ? "" : pname) != "" && (pvalue == null ? "" : pvalue) != "")
                {
                    pname = pname.Trim().ToLower();
                    List<string> svalue = pvalue.Split('|').Select(r => r.Trim().ToLower()).ToList();
                    cd = cd
                        .Where(p =>
                            p.Elements().Where(s => s.Name.LocalName.Trim().ToLower() == pname && svalue.Contains(s.Value.Trim().ToLower())).Count() != 0
                            || p.Parent.Elements().Where(s => s.Name.LocalName.Trim().ToLower() == pname && svalue.Contains(s.Value.Trim().ToLower())).Count() != 0
                    );
                    if (cd.Count() == 0)
                    {
                        result.Error = "Ingen elementer funnet med hvor " + pname + "=" + pvalue;
                        return result;
                    }
                    if (cd.Where(p => p.Name.LocalName.ToLower() == ename).Count() != cd.Count())
                    {
                        result.Error = "Feil antall elementer funnet med hvor " + pname + "=" + pvalue;
                        return result;
                    }

                }


                if (count)
                {
                    result.value = cd.Count().ToString();
                    return result;
                }
                if (sum)
                {
                    double vsum = 0;
                    foreach (string s in cd.Select(p => p.Value))
                    {
                        try
                        {
                            vsum += Convert.ToDouble(s, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            result.value = "No sum";
                            return result;
                        }
                    }
                    result.value = vsum.ToString();
                    return result;
                }
                result.elements = cd.ToList();
                return result;
            }
            catch (SystemException err)
            {
                result.Error = err.Message;
                return result;
            }
        }

        public static Capture CaptureGet(this Match m, Capture c, string name)
        {
            foreach (Capture e in m.Groups[name].Captures)
            {
                if (e.Index >= c.Index && (e.Index + e.Length) <= (c.Index + c.Length))
                {
                    return e;
                }
            }
            return null;
        }
        public static XElement GetSettingsByValue(this string value)
        {
            if (value.Split(':').Count() == 1)
                return new XElement("setting", value);
            else if (value.Split(':').Count() == 2)
                return new XElement(Regex.Replace((value.Split(':').FirstOrDefault() ?? "none").Trim().ToLower(), @"[^a-z]", "_"), (value.Split(':').LastOrDefault() ?? "none").Trim().ToLower());
            else
                return new XElement("setting", value);
        }
        public static void AttributesUnique(this UniqueAtt atts)
        {
            atts.l.Skip(1).ToList().ForEach(p => p.SetValue(Guid.NewGuid().ToString()));
        }
        public static XElement IndexSearch(this XElement map, XElement searchitems)
        {
            string search = (string)searchitems.Attributes("query").FirstOrDefault();
            (
                from m in map.Descendants()
                join s in searchitems.Elements()
                on ((string)m.Attributes().Where(p => "id;key".Split(';').Contains(p.Name.LocalName)).FirstOrDefault() ?? "1-").ToLower() equals ((string)s.Attributes("id").FirstOrDefault() ?? "-1").ToLower()
                select new { me = m, se = s }
            ).ToList().ForEach(p => p.me.AddAnnotation(new ItemSearch { score = (string)p.se.Attributes("score").FirstOrDefault() }));

            XElement searchresult = new XElement("searchresult",
                search == null ? null : new XAttribute("query", search),
                searchitems.Descendants("sw").FirstOrDefault(),
                map.Descendants()
                .Where(p =>
                       p.Attributes().Where(s => "text;title".Split(';').Contains(s.Name.LocalName)).FirstOrDefault() != null
                    && p.Attributes().Where(s => "id;key".Split(';').Contains(s.Name.LocalName)).FirstOrDefault() != null
                    && p.DescendantsAndSelf().Where(s => s.Annotation<ItemSearch>() != null).Count() != 0)
                .Select(p => new XElement("item",
                    new XAttribute("id", (string)p.Attributes().Where(s => "id;key".Split(';').Contains(s.Name.LocalName)).FirstOrDefault()),
                    new XAttribute("n", p.DescendantsAndSelf().Where(s => s.Annotation<ItemSearch>() != null).Count().ToString()),
                    new XAttribute("max", p.DescendantsAndSelf().Where(s => s.Annotation<ItemSearch>() != null).Select(s => s.Annotation<ItemSearch>().score).Max(s => Convert.ToUInt32(s)).ToString()),
                    //new XAttribute("score", p.Annotation<ItemSearch>() == null ? "-1": p.Annotation<ItemSearch>().score)
                    new XAttribute("score",
                            p.DescendantsAndSelf().Where(s => s.Annotation<ItemSearch>() != null).Count() == 0
                            ? "-1"
                            : p.DescendantsAndSelf().Where(s => s.Annotation<ItemSearch>() != null).Select(s => Convert.ToInt32(s.Annotation<ItemSearch>().score)).Max(s => s).ToString()
                        )
                    )

                )
            );
            return searchresult;
        }
        public static XElement IndexSearch_Ex(this XElement map, XElement searchitems)
        {
            string search = (string)searchitems.Attributes("query").FirstOrDefault();
            (
                from m in map.Descendants()
                join s in searchitems.Elements()
                on ((string)m.Attributes().Where(p => "id;key".Split(';').Contains(p.Name.LocalName)).FirstOrDefault() ?? "1-").ToLower() equals ((string)s.Attributes("id").FirstOrDefault() ?? "-1").ToLower()
                select new { me = m, se = s }
            ).ToList().ForEach(p => p.me.AddAnnotation(new ItemSearch { score = (string)p.se.Attributes("score").FirstOrDefault() }));

            XElement searchresult = new XElement("searchresult",
                search == null ? null : new XAttribute("query", search),
                searchitems.Descendants("sw").FirstOrDefault(),
                map.Descendants()
                .Where(p =>
                       p.Attributes().Where(s => "text;title".Split(';').Contains(s.Name.LocalName)).FirstOrDefault() != null
                    && p.Attributes().Where(s => "id;key".Split(';').Contains(s.Name.LocalName)).FirstOrDefault() != null
                    && ((string)p.Attributes("index").FirstOrDefault() ?? "true") == "true"
                    && p.DescendantsAndSelf().Where(s => s.Annotation<ItemSearch>() != null).Count() != 0)
                .Select(p => new XElement("item",
                    new XAttribute("id", (string)p.Attributes().Where(s => "id;key".Split(';').Contains(s.Name.LocalName)).FirstOrDefault()),
                    new XAttribute("n", p.DescendantsAndSelf().Where(s => s.Annotation<ItemSearch>() != null).Count().ToString()),
                    new XAttribute("max", p.DescendantsAndSelf().Where(s => s.Annotation<ItemSearch>() != null).Select(s => s.Annotation<ItemSearch>().score).Max(s => Convert.ToUInt32(s)).ToString()),
                    new XAttribute("score",
                            p.Annotation<ItemSearch>() == null
                            ? "0"
                            : Convert.ToInt32(p.Annotation<ItemSearch>().score).ToString()
                    ),
                    p
                    .Descendants()
                    .Where(s =>
                        (
                        s.Attributes().Where(a => "text;title".Split(';').Contains(a.Name.LocalName)).FirstOrDefault() == null
                        || ((string)s.Attributes("index").FirstOrDefault() ?? "true") == "false"
                        )
                        && s.Attributes().Where(t => "id;key".Split(';').Contains(t.Name.LocalName)).FirstOrDefault() != null
                        && s.Annotation<ItemSearch>() != null
                        && s.Ancestors().Where(r =>
                            r.Attributes().Where(t => "text;title".Split(';').Contains(t.Name.LocalName)).FirstOrDefault() != null
                            && r.Attributes().Where(t => "id;key".Split(';').Contains(t.Name.LocalName)).FirstOrDefault() != null
                            && ((string)r.Attributes("index").FirstOrDefault() ?? "true") == "true"
                        ).FirstOrDefault() == p
                    ).Select(s => new XElement("item",
                            new XAttribute("id", (string)s.Attributes().Where(t => "id;key".Split(';').Contains(t.Name.LocalName)).FirstOrDefault()),
                            new XAttribute("score", s.Annotation<ItemSearch>().score)
                            )
                        )
                    )

                )
            );
            return searchresult;
        }
        public static string GetNodesWithXVar(this IEnumerable<XNode> n)
        {
            string s = "";
            foreach (XNode p in n)
            {
                if (p.NodeType == XmlNodeType.Text)
                {
                    s = s + ((XText)p).Value;
                }
                else if (p.NodeType == XmlNodeType.Element)
                {
                    XElement e = (XElement)p;


                    if (e.Name.LocalName == "x-var")
                    {
                        XVarName name = e.Annotation<XVarName>();
                        if (name != null)
                        {
                            s = s + " [" + name.name + "] ";
                        }
                        else
                        {
                            s = s + " [" + ((string)e.Attributes("id").FirstOrDefault() ?? "") + "] ";
                        }
                    }
                }

            }
            return Regex.Replace(s, @"\s+", " ");
        }
        public static string AttributesGetValue(this XElement e, string names)
        {
            string value = (string)e.Attributes().Where(p => names.Split(';').Contains(p.Name.LocalName)).FirstOrDefault();
            value = value == null ? "" : value;
            return value;
        }
        public static XElement GetBookmarkData(this XElement bookmarks, XElement map)
        {
            (
                from b in bookmarks.Elements("bookmark").Attributes("id")
                join i in map.Descendants().Attributes().Where(p => "id;key".Split(';').Contains(p.Name.LocalName))
                on (string)b.Value.ToLower() equals i.Value.ToLower()
                select new { bookmark = b, id = i }
            )
            .ToList()
            .ForEach(p => p.bookmark.Parent.Add(new XAttribute("title", (string)p.id.Parent.AncestorsAndSelf().Attributes().Where(s => "text;title".Split(';').Contains(s.Name.LocalName)).LastOrDefault())));
            return bookmarks;
        }
        public static IEnumerable<XElement> GetXObject(this XElement e)
        {
            return
                e
                .Elements()
                .SelectMany(p => p.GetXObjects());
        }
        public static IEnumerable<XElement> GetXObjects(this XElement e)
        {
            List<XElement> result = new List<XElement>();
            if (e == null) return result;
            if (e.Name.LocalName == "x-var")
            {
                result.Add(new XElement(e));
            }
            else
            {
                if (e.Name.LocalName == "level" && ((string)e.Attributes("data-optional").FirstOrDefault() ?? "") == "true")
                {
                    result.Add(
                        new XElement("x-section-optional",
                        ((string)e.Attributes("data-var-id").FirstOrDefault() ?? "") == "" ? null : new XAttribute("id", (string)e.Attributes("data-var-id").FirstOrDefault()),
                        e.GetXObject()
                        )
                    );
                }
                else if (e.Name.LocalName.StartsWith("x-"))
                {
                    result.Add(
                        new XElement(e.Name.LocalName,
                        e.Attributes(),
                        e.GetXObject()
                        )
                    );
                }
                else
                {
                    result.AddRange(e.GetXObject());
                }

            }
            return result;
        }
        public static IEnumerable<XElement> GetFirstHeadings(
          this IEnumerable<Heading> headingList,
          int Level
          )
        {
            headingList.Where(p => (string)p.Element.Attributes("id").FirstOrDefault() == null).ToList().ForEach(p => p.Element.Add(new XAttribute("id", Guid.NewGuid().ToString())));

            return
                headingList
                    .Where(h => h.Level <= Level)
                    //.TakeWhile(h => h.Level <= Level)
                    .Select(h =>
                        new XElement("level",
                            new XAttribute("id", (string)h.Element.Attributes("id").FirstOrDefault()),
                            //new XElement("title", Regex.Replace(h.Element.DescendantNodesAndSelf().OfType<XText>().Select(s => s.ToString()).StringConcatenate(" ").Trim(), @"\s+", " ")),
                            new XElement("title", h.Element.Nodes()),
                           headingList.GetChildrenHeadings(h)
                        )
                    );

        }
        public static IEnumerable<XElement> GetChildrenHeadings(
           this IEnumerable<Heading> headingList,
           Heading parent
           )
        {
            int level = parent.Level + 1;
            for (int i = level; i < 7; i++)
            {
                int n = headingList
                        .SkipWhile(h => h.Element != parent.Element)
                        .Skip(1)
                        .TakeWhile(h => h.Level >= i)
                        .Where(h => h.Level == i)
                        .Count();

                if (n != 0)
                {
                    level = i;
                    return
                        headingList
                            .SkipWhile(h => h.Element != parent.Element)
                            .Skip(1)
                            .TakeWhile(h => h.Level >= level)
                            .Where(h => h.Level == level)
                            .Select(h =>
                                new XElement("level",
                                    new XAttribute("id", (string)h.Element.Attributes("id").FirstOrDefault()),
                                    //new XElement("title", Regex.Replace(h.Element.DescendantNodesAndSelf().OfType<XText>().Select(s => s.ToString()).StringConcatenate(" ").Trim(), @"\s+", " ")),
                                    new XElement("title", h.Element.Nodes()),
                                    GetChildrenHeadings(headingList, h)
                                )
                            );

                }
            }
            return null;
        }
        public static void SetXboxTitle(this XElement xbox)
        {
            XElement first = xbox.Elements().Where(p => p.DescendantNodes().OfType<XText>().Select(s => s.ToString()).StringConcatenate().Trim() != "").FirstOrDefault();
            if (first == null) return;
            if (first.Name.LocalName == "level")
            {
                string hx = first.Elements("title").Select(p => p.Value).FirstOrDefault();
                if (hx.Length < 100)
                {
                    xbox.Add(new XAttribute("title", hx));
                }
                else
                {
                    xbox.Add(new XAttribute("title", hx.Substring(0, 97) + "..."));
                }
            }
            else if (first.Name.LocalName == "p")
            {
                string hx = first.DescendantNodes().TakeWhile(p => (p.NextNode == null ? "" : (p.NextNode.NodeType == XmlNodeType.Element ? ((XElement)p.NextNode).Name.LocalName : "")) != "br").OfType<XText>().Select(s => s.ToString()).StringConcatenate();
                if (hx.Length < 100)
                {
                    xbox.Add(new XAttribute("title", hx));
                }
                else
                {
                    xbox.Add(new XAttribute("title", hx.Substring(0, 97) + "..."));
                }
            }


        }
        public static void FlatToHierarcy(this XElement text, bool InXObject = false)
        {
            IEnumerable<Heading> headingList = null;
            if (InXObject)
            {
                XElement first = text.Elements().Where(p => p.DescendantNodes().OfType<XText>().Select(s => s.ToString()).StringConcatenate().Trim() != "").FirstOrDefault();
                if ((first == null ? false : Regex.IsMatch(first.Name.LocalName, @"h\d+")) && text.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d+")).Count() == 1)
                {
                    string hx = text.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d+")).DescendantNodes().OfType<XText>().Select(s => s.ToString()).StringConcatenate();
                    if (text.Attributes("title").FirstOrDefault() == null)
                    {
                        text.Add(new XAttribute("title", hx));
                        text.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d+")).First().Remove();
                    }
                    else
                    {
                        text.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d+")).First().ReplaceWith(new XElement("p", new XElement("strong", hx)));
                    }
                    return;
                }
                else if (first.Name.LocalName == "p" && text.Attributes("title").FirstOrDefault() == null)
                {
                    string hx = first.DescendantNodes().TakeWhile(p => (p.NextNode == null ? "" : (p.NextNode.NodeType == XmlNodeType.Element ? ((XElement)p.NextNode).Name.LocalName : "")) != "br").OfType<XText>().Select(s => s.ToString()).StringConcatenate();
                    if (hx.Length < 100)
                    {
                        text.Add(new XAttribute("title", hx));
                    }
                    else
                    {
                        text.Add(new XAttribute("title", hx.Substring(0, 97) + "..."));
                    }
                }


                headingList = text.Descendants().Where(p =>
                           Regex.IsMatch(p.Name.LocalName, @"h\d+")
                    )
                    .Select(p => new Heading(p)
                );

            }
            else
            {
                if (text.Name.LocalName == "x-alternative")
                {
                    headingList = text.Descendants().Where(p =>
                            Regex.IsMatch(p.Name.LocalName, @"h\d+")
                     )
                     .Select(p => new Heading(p)
                 );
                }
                else
                {
                    headingList = text.Descendants().Where(p =>
                               p.Ancestors().Where(s => s.Name.LocalName.ToString().StartsWith("x-")).Count() == 0
                            && Regex.IsMatch(p.Name.LocalName, @"h\d+")
                        )
                        .Select(p => new Heading(p)
                    );
                }
            }
            if (headingList.Count() == 0) return;

            int Level = Convert.ToInt32(Regex.Match(headingList.First().Element.Name.LocalName, @"h(?<n>(\d+))").Groups["n"].Value);
            XElement headings = new XElement("headings", GetFirstHeadings(headingList, Level));

            List<XElement> h = headings.DescendantsAndSelf("level").ToList();
            if (h.Count() != 0)
            {

                List<XNode> nodelist_before = new List<XNode>();
                XElement first = h.FirstOrDefault();
                if (first != null)
                {
                    nodelist_before.AddRange(
                        text
                        .Nodes()
                        .OfType<XElement>()
                        .TakeWhile(p => ((string)p.Attributes("id").FirstOrDefault() ?? "") != (string)first.Attributes("id").FirstOrDefault())
                    );
                    nodelist_before.ForEach(p => p.Remove());
                }

                foreach (XElement current in h)
                {
                    List<XNode> nodelist = new List<XNode>();
                    XElement next = h.SkipWhile(p => p != current).Skip(1).Take(1).FirstOrDefault();
                    nodelist = new List<XNode>();
                    if (next != null)
                    {
                        nodelist.AddRange(
                            text
                            .Nodes()
                            .OfType<XElement>()
                            .SkipWhile(p => ((string)p.Attributes("id").FirstOrDefault() ?? "") != (string)current.Attributes("id").FirstOrDefault())
                            .TakeWhile(p => ((string)p.Attributes("id").FirstOrDefault() ?? "") != (string)next.Attributes("id").FirstOrDefault())
                        );
                        nodelist.ForEach(p => p.Remove());
                        current.Elements("title").FirstOrDefault().AddAfterSelf(new XElement("text", nodelist.Skip(1)));
                    }
                    else
                    {
                        nodelist.AddRange(
                            text
                            .Nodes()
                            .OfType<XElement>()
                            .SkipWhile(p => ((string)p.Attributes("id").FirstOrDefault() ?? "") != (string)current.Attributes("id").FirstOrDefault())

                        );
                        nodelist.ForEach(p => p.Remove());
                        current.Elements("title").FirstOrDefault().AddAfterSelf(new XElement("text", nodelist.Skip(1)));
                    }
                }

                if (nodelist_before.Count() != 0)
                {
                    headings.AddFirst(nodelist_before);
                }

            }
            if (InXObject)
            {
                text.ReplaceWith(new XElement(text.Name.LocalName, text.Attributes(), headings.Nodes()));

            }
            else
            {
                if (text.Name.LocalName == "x-alternative")
                {
                    text.ReplaceWith(new XElement(text.Name.LocalName, text.Attributes(), headings.Nodes()));
                }
                else
                {
                    text.AddAfterSelf(headings.Nodes());
                }
            }


        }
        public static bool Between(this int num, int lower, int upper, bool inclusive = false)
        {
            return inclusive ? lower <= num && num <= upper : lower < num && num < upper;
        }
        public static StringBuilder DocumentText(this IEnumerable<XText> texts)
        {
            StringBuilder returnvalue = new StringBuilder();
            int p = 0;
            int l = 0;
            foreach (XText t in texts)
            {
                t.Value = Regex.Replace(t.Value, @"(\s+|\n|\r|\t)", " ");
                t.RemoveAnnotations<TextRange>();
                returnvalue.Append(t.Value);
                l = t.Value.Length;
                t.AddAnnotation(new TextRange { pos = p, length = l });
                p = p + l;
            }
            return returnvalue;
        }
        public static void ConcatXText(this XElement d)
        {
            XText n = d.DescendantNodesAndSelf().OfType<XText>().Where(p => p.NextNode != null).Where(p => p.NextNode.NodeType == XmlNodeType.Text).FirstOrDefault();
            while (n != null)
            {
                n.Value = n.Value + ((XText)n.NextNode).Value;
                n.NextNode.Remove();
                n = d.DescendantNodesAndSelf().OfType<XText>().Where(p => p.NextNode != null).Where(p => p.NextNode.NodeType == XmlNodeType.Text).FirstOrDefault();
            }

        }
        public static StringBuilder ElementText(this IEnumerable<XText> texts)
        {
            StringBuilder returnvalue = new StringBuilder();
            int p = 0;
            int l = 0;
            foreach (XText t in texts)
            {
                t.RemoveAnnotations<TextRange>();
                returnvalue.Append(t.Value);
                l = t.Value.Length;
                t.AddAnnotation(new TextRange { pos = p, length = l });
                p = p + l;
            }
            return returnvalue;
        }

        public static string DecodeXText(this XText t)
        {
            string returnValue = t.Value.ToString();
            //if (Regex.IsMatch(returnValue, "&amp;")) Debug.Print("xxx");
            returnValue = Regex.Replace(returnValue, "&amp;", ((char)38).ToString());
            returnValue = Regex.Replace(returnValue, "&quot;", ((char)34).ToString());
            returnValue = Regex.Replace(returnValue, "&apos;", ((char)39).ToString());
            returnValue = Regex.Replace(returnValue, "&lt;", ((char)60).ToString());
            returnValue = Regex.Replace(returnValue, "&gt;", ((char)60).ToString());
            return returnValue;
        }
        public static XNode BufferFirstXTextOfParentElementWith(this XNode s, string ReplaceString = " ")
        {
            return s.Parent == null
                    ? s
                    : (
                        s.Parent.DescendantNodes().OfType<XText>().First() == s
                        ? new XText(ReplaceString + ((XText)s).DecodeXText())
                        : s
                    );
        }
        public static XNode ReplaceNodesOfNameWith(this XNode s, string ElementNames, string ReplaceString = " ")
        {
            if (ElementNames != "")
            {
                if (s.NodeType == XmlNodeType.Element)
                {
                    if (ElementNames.Split(';').Where(p => p.ToString() == ((XElement)s).Name.LocalName.ToString()).Count() == 1)
                    {
                        return new XText(ReplaceString);
                    }
                    else
                    {
                        ((XElement)s).Nodes().Select(r => r.ReplaceNodesOfNameWith(ElementNames, ReplaceString));
                    }

                }
                if (s.NodeType == XmlNodeType.Text)
                {
                    if (ElementNames.Split(';').Where(p => p.ToString() == s.Parent.Name.LocalName.ToString()).Count() == 1)
                    {
                        return new XText(ReplaceString);
                    }
                }
            }

            return s;

        }
        public static string GetElementTextComplete(this XElement e, string ReplaceNodesNames, string RegexCharToReplace = @"\s+", string ReplaceString = " ", string Separate = " ")
        {
            return e
            .DescendantNodes()
            .Select(s => s.ReplaceNodesOfNameWith(ReplaceNodesNames, ReplaceString))
            .OfType<XText>()
            .Select(s => s.BufferFirstXTextOfParentElementWith())
            .Select(s => ((XText)s).DecodeXText())
            .StringConcatenate()
            .Trim();
        }
        public static XElement GetRootElement(this SqlXml xml)
        {
            if (xml.IsNull) return null;
            try
            {
                XmlDocument xmldoc = new XmlDocument();
                XmlReader r = xml.CreateReader();
                r.MoveToContent();
                xmldoc.Load(r);
                return XElement.Load(xmldoc.CreateNavigator().ReadSubtree());
            }
            catch
            {
                return null;
            }
        }
        public static string StringConcatenate(this IEnumerable<string> source, string split)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in source)
                if (sb.Length == 0)
                    sb.Append(s);
                else
                    sb.Append(split + s);
            return sb.ToString();
        }
        public static string StringConcatenate(this IEnumerable<string> source)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in source)
                sb.Append(s);
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
}
