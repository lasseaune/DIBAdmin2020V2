using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Data.SqlTypes;

namespace DIBConvertCLR
{
    public class DGVariableEval
    {
        private string name { get; set; }
        private string id { get; set; }
        public bool collection { get; set; }
        private Regex GetN = new Regex(@"\*N\*", RegexOptions.IgnoreCase);
        private Regex NumberFromName { get; set; }
        private Regex NumberFromId { get; set; }
        public DGVariableEval(string Name, string Id)
        {
            name = Name;
            id = Id;
            collection = Regex.IsMatch(name, @"\*N\*", RegexOptions.IgnoreCase);
            if (collection)
            {
                NumberFromName = GetRegexNr(name);
                NumberFromId = GetRegexNr(id);
            }
        }
        private Regex GetRegexNr(string s)
        {
            string r = ReplaceNToGroup(s);
            return new Regex(r, RegexOptions.IgnoreCase);
        }
        private string ReplaceNToGroup(string s)
        {
            return GetN.Replace(s, @"(?<nr>(\d+))");
        }
        public string GetNumberInName(string name)
        {
            if (!collection) return null;
            string result = NumberFromName.Match(name).Groups["nr"].Value;
            if (Regex.IsMatch(result, @"^\d+$"))
                return result;
            return null;
        }
        public string GetNumberInId(string id)
        {
            if (!collection) return null;
            string result = NumberFromName.Match(id).Groups["nr"].Value;
            if (Regex.IsMatch(result, @"^\d+$"))
                return result;
            return null;
        }

        public string SetNumberInId(string nr)
        {
            if (!collection) return null;
            return GetN.Replace(id, nr);
        }
    }
    public class NaturalSortComparer<T> : IComparer<string>, IDisposable
    {
        #region IComparer<string> Members

        public int Compare(string x, string y)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComparer<string> Members

        int IComparer<string>.Compare(string x, string y)
        {
            if (x == y)
                return 0;

            string[] x1, y1;

            if (!table.TryGetValue(x, out x1))
            {
                x1 = Regex.Split(x.Replace(" ", ""), "([0-9]+)");
                table.Add(x, x1);
            }

            if (!table.TryGetValue(y, out y1))
            {
                y1 = Regex.Split(y.Replace(" ", ""), "([0-9]+)");
                table.Add(y, y1);
            }

            for (int i = 0; i < x1.Length && i < y1.Length; i++)
            {
                if (x1[i] != y1[i])
                    return PartCompare(x1[i], y1[i]);
            }

            if (y1.Length > x1.Length)
                return 1;
            else if (x1.Length > y1.Length)
                return -1;
            else
                return 0;
        }

        private static int PartCompare(string left, string right)
        {
            int x, y;
            if (!int.TryParse(left, out x))
                return left.CompareTo(right);

            if (!int.TryParse(right, out y))
                return left.CompareTo(right);

            return x.CompareTo(y);
        }

        #endregion

        private Dictionary<string, string[]> table = new Dictionary<string, string[]>();

        public void Dispose()
        {
            table.Clear();
            table = null;
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
        public TopicInfo(XElement topic)
        {
            id = (string)topic.Attributes("topic_id").FirstOrDefault();
            topictype = (string)topic.Attributes("tid").FirstOrDefault();
            title = (string)topic.Attributes("name").FirstOrDefault();
            segment_id = (string)topic.Attributes("segment_id").FirstOrDefault();
            paragraph_id = (string)topic.Attributes("paragraf_id").FirstOrDefault();
            view = (string)topic.Attributes("view").FirstOrDefault();
            language = (string)topic.Attributes("lang").FirstOrDefault();
        }
    }
    public class TextRange
    {
        public int pos { get; set; }
        public int length { get; set; }
    }
    public class TopicInfoGrouping
    {
        public string topic_id { get; set; }
        public string bm { get; set; }
        public TopicInfoGrouping(XAttribute t)
        {
            topic_id = t.Value;
            bm = ((string)t.Parent.Attributes("bm").FirstOrDefault() ?? "") == "undefined" ? "" : ((string)t.Parent.Attributes("bm").FirstOrDefault() ?? "");
        }
    }

    public class ElementData
    {
        public bool IncludeHeader = false;
        public bool TopElement = false;
        public bool IsSegment = false;
        public bool Current = false;
        public bool Generated = false;
        public bool HasContent = false;
        public string segmentName { get; set; }
        public string segment_id { get; set; }
        public XElement segment { get; set; }
        public XElement element { get; set; }
        public XElement item { get; set; }
        public XElement map { get; set; }
        public XElement index { get; set; }
        public ElementData(ElementData parent, XElement e)
        {
            Generated = true;
            if (parent.Current == true)
            {
                element = new XElement(e.Name.LocalName, e.Attributes().Where(p => p.Name.LocalName != "segment"), e.Nodes().Select(p => ElementNode(p)));
                map = new XElement("map", element.Map());
            }
        }
        public IEnumerable<XNode> ElementNode(XNode n)
        {
            List<XNode> nodelist = new List<XNode>();
            if (n.NodeType == XmlNodeType.Text)
                nodelist.Add(n);
            else if (n.NodeType == XmlNodeType.Element)
            {
                XElement e = (XElement)n;
                if ((string)e.Attributes("segment").FirstOrDefault() == "true")
                {
                    nodelist.Add(new XElement(e.Name.LocalName, e.Attributes(), e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d"))));
                }
                else
                {
                    nodelist.Add(new XElement(e.Name.LocalName, e.Attributes(), e.Nodes().Select(p => ElementNode(p))));
                }
            }
            return nodelist;

        }
        public ElementData(XElement e)
        {
            string err = "Start";
            try
            {

                IncludeHeader = (string)e.Attributes("includeheader").FirstOrDefault() == "true" ? true : false;
                IsSegment = (string)e.Attributes("segment").FirstOrDefault() == "true" ? true : false;
                HasContent = e.Elements().Where(p =>
                        !(Regex.IsMatch(p.Name.LocalName, @"h\d")
                        ||
                        (string)p.Attributes("segment").FirstOrDefault() == "true")
                    ).Count() == 0 ? false : true;
                TopElement = e.AncestorsAndSelf().LastOrDefault() == e;
                Current = true;
                if (IncludeHeader && TopElement && IsSegment && HasContent)
                {
                    err = "IncludeHeader";
                    XElement first = e.Elements().Where(p => ((string)p.Attributes("segment").FirstOrDefault() ?? "false") == "true").FirstOrDefault();
                    if (first != null)
                    { 
                        segmentName = first.GetHeaderText();
                        first.Attributes("segment").Remove();

                        segment = new XElement("document",
                            new XElement(e.Name.LocalName,
                            e.Attributes(),
                            e.Elements().TakeWhile(p => ((string)p.Attributes("segment").FirstOrDefault() ?? "false") != "true")
                            )
                        );
                        segment = new XElement("segment",
                            new XAttribute("searchname", segmentName),
                            new XAttribute("segment_id", (string)first.Attributes("id").FirstOrDefault()),
                            new XAttribute("default", "true"),
                            segment
                        );
                    }
                    else
                    {
                        segment = new XElement("document",
                            new XElement(e.Name.LocalName,
                            e.Attributes(),
                            e.Elements().TakeWhile(p => ((string)p.Attributes("segment").FirstOrDefault() ?? "false") != "true")
                            )
                        );
                        segmentName = e.GetHeaderText();
                        segment = new XElement("segment",
                            new XAttribute("searchname", segmentName),
                            new XAttribute("segment_id", (string)e.Attributes("id").FirstOrDefault()),
                            new XAttribute("default", "true"),
                            segment
                        );
                    }


                }
                else if (TopElement && IsSegment && HasContent)
                {
                    err = "Topsegment";
                    segmentName = e.GetHeaderText();
                    segment = new XElement("document",
                        new XElement(e.Name.LocalName,
                        e.Attributes(),
                        e.Elements().TakeWhile(p => ((string)p.Attributes("segment").FirstOrDefault() ?? "false") == "true")
                        )
                    );
                    segment = new XElement("segment",
                        new XAttribute("searchname", segmentName),
                        new XAttribute("segment_id", (string)e.Attributes("id").FirstOrDefault()),
                        segment
                    );

                }
                else if (!TopElement && IsSegment && HasContent && Current)
                {
                    err = "Current med content";
                    segmentName = e.GetHeaderText();
                    segment = new XElement("document",
                        new XElement(e.Name.LocalName,
                        e.Attributes(),
                        e.Elements().TakeWhile(p => ((string)p.Attributes("segment").FirstOrDefault() ?? "false") != "true")
                        )
                    );
                    segment = new XElement("segment",
                        new XAttribute("searchname", segmentName),
                        new XAttribute("segment_id", (string)e.Attributes("id").FirstOrDefault()),
                        segment
                    );
                }
                else if (IsSegment && !HasContent && Current)
                {
                    err = "SegmentNoContent: id=" + (string)e.Attributes("id").FirstOrDefault()??"none" ;
                    XElement first = e.Elements().Where(p => ((string)p.Attributes("segment").FirstOrDefault() ?? "false") == "true").FirstOrDefault();
                    if (first != null)
                    {
                        segmentName = first.GetHeaderText();
                        first.Attributes("segment").Remove();
                        ElementData ed = new ElementData(this, first);
                        segment = new XElement("document",
                            new XElement(e.Name.LocalName,
                            e.Attributes(),
                            e.Elements().TakeWhile(p => ((string)p.Attributes("segment").FirstOrDefault() ?? "false") != "true"),
                            ed.element

                            )
                        );
                    }
                    else
                    {
                        segmentName = e.GetHeaderText();
                        segment = new XElement("document",
                            new XElement(e.Name.LocalName,
                            e.Attributes(),
                            e.Elements().TakeWhile(p => ((string)p.Attributes("segment").FirstOrDefault() ?? "false") != "true")
                            )
                        );
                    }

                    segment = new XElement("segment",
                        new XAttribute("searchname", segmentName),
                        new XAttribute("segment_id", (string)e.Attributes("id").FirstOrDefault()),
                        segment
                    );
                }
            }
            catch
            {
                throw new Exception("Elementdata: " + err);
            }
        }
        

    }

    public class SearchItem
    {
        public string text { get; set; }
        public int type { get; set; }
        public string id { get; set; }
        public SearchItem() { }
        public SearchItem(XElement e)
        {
            id = (string)e.Ancestors("x-box").Attributes("id").FirstOrDefault() == null ? null : (string)e.Ancestors("x-box").Attributes("id").FirstOrDefault();
            switch (e.Name.LocalName)
            {
                case "section":
                    if ((string)e.Attributes("class").FirstOrDefault() == "lovdata-ledd")
                    {
                        text = Regex.Replace(e.DescendantNodes()
                                .OfType<XText>()
                                .Where(p => p.Ancestors("a").FirstOrDefault() == null)
                                .Select(p => p.Value.ToString()).StringConcatenate(" "), @"\s+", " ").Trim();
                        type = 0;
                    }
                    else 
                    {
                        text = Regex.Replace(
                                e.Elements()
                                .Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d+"))
                                .DescendantNodes()
                                .OfType<XText>()
                                .Where(p => p.Ancestors("a").Where(a=>a.Attributes("data-replacetext").FirstOrDefault()==null).FirstOrDefault() == null)
                                .Select(p => p.Value.ToString())
                                .StringConcatenate(" "),
                                @"\s+", " ").Trim();
                        type = 1;
                        
                    }
                    break;
                default:
                    {
                        text = Regex.Replace(e.DescendantNodes()
                                .OfType<XText>()
                                .Where(p => p.Ancestors("a").Where(a => a.Attributes("data-replacetext").FirstOrDefault() == null).FirstOrDefault() == null)
                                .Select(p => p.Value.ToString()).StringConcatenate(" "), @"\s+", " ").Trim();
                        type = 0;
                    }
                    break;
            }
        }
    }

    public class StyleElement
    {
        public string name { get; set; }
        public List<string> values { get; set; }
        public StyleElement(string style)
        {
            name = (style.Split(':').FirstOrDefault() ?? "").Trim().ToLower();
            values = (style.Split(':').LastOrDefault() ?? "").Trim().Split(' ').Where(p => p.Trim() != "").Select(p => p.Trim()).ToList();
        }

    }
    public class StyleElements
    {
        public List<StyleElement> styles { get; set; }
        public StyleElements(XAttribute style)
        {
            if (style == null) return;
            string Styles = style.Value;
            styles = Styles.Split(';').Where(p => p.Trim() != "").Select(p => new StyleElement(p.Trim())).ToList();
        }
    }
    public static class Extentions
    {
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
        public static XElement GetInternalTagsMVA(this XElement total)
        {
            string letterNumber = "første|annet|tredje|fjerde|femte|sjette|sjuende|åttende|niende|tiende";
            int n = 1;
            List<string> ln = letterNumber.Split('|').Select(s => (n++).ToString() + ";" + s).ToList();
            Regex rx = new Regex(@"(?<ledd>((" + letterNumber + @")))\sledd(\sbokstav\s(?<bokstav>(([a-z]))))?");
            Regex rxPara = new Regex(@"^§\s+(?<name>(\d+-\d+([a-z])?))");
            Regex rxLedd = new Regex(@"^\((?<number>(\d+))\)");
            XElement internaltags = new XElement("segment",
                new XAttribute("segment_id", "_internaltags"),
                new XElement("tags",
                    new XAttribute("area", "global"),
                    new XAttribute("language", "no"),
                    new XAttribute("regexpName", "para;source_para"),
                    new XAttribute("topic_id", "_self"),
                    new XElement("tag1",
                        new XAttribute("id", "_self"),
                        new XAttribute("title", "Merverdiavgiftshåndboken 2015"),
                        new XElement("name", "mval"),
                        new XElement("name", "mval."),
                        new XElement("name", "merverdiavgiftsloven"),
                        new XElement("name", "merverdiavgiftlov"),
                        total.Descendants("paragraf")
                            .Where(p => (string)p.NodesBeforeSelf().OfType<XElement>().Attributes("autonum").FirstOrDefault() != "23")
                            .Select(p => new XElement("tag2",
                                new XAttribute("id", (string)p.Parent.Attributes("id").FirstOrDefault()),
                                new XAttribute("title", p.NodesBeforeSelf().OfType<XElement>().Select(r => r.Nodes().OfType<XText>().Select(s => s.Value).StringConcatenate()).LastOrDefault()),
                                new XElement("name",
                                    Regex.Replace(rxPara.Match(p
                                    .NodesBeforeSelf().OfType<XElement>()
                                    .Where(s => s.Attributes("autonum").FirstOrDefault() != null)
                                    .Select(r => r.Nodes().OfType<XText>().Select(s => s.Value).StringConcatenate()).FirstOrDefault()
                                    ).Groups["name"].Value, "?", "-").Trim()),
                                new XElement("name", p.NodesBeforeSelf().OfType<XElement>().Select(r => r.Nodes().OfType<XText>().Select(s => s.Value).StringConcatenate()).LastOrDefault()),
                                p.NodesBeforeSelf().OfType<XElement>().LastOrDefault().Attributes("autonum").FirstOrDefault() == null
                                ? null
                                : Regex.Replace(rxPara.Match(p
                                    .NodesBeforeSelf().OfType<XElement>()
                                    .Where(s => s.Attributes("autonum").FirstOrDefault() != null)
                                    .Select(r => r.Nodes().OfType<XText>().Select(s => s.Value).StringConcatenate()).FirstOrDefault()
                                    ).Groups["name"].Value, "?", "-").Trim()
                                    == Regex.Replace(((string)p.NodesBeforeSelf().OfType<XElement>().LastOrDefault().Attributes("autonum").FirstOrDefault() ?? ""), "?", "-").Trim()
                                    ? null
                                    : new XElement("name", (string)p.NodesBeforeSelf().OfType<XElement>().LastOrDefault().Attributes("autonum").FirstOrDefault())
                                ,
                                p.NodesAfterSelf().OfType<XElement>().Descendants()
                                .Where(s => s.Name.LocalName.StartsWith("ms"))
                                .Select(s => new { e = s, text = s.Nodes().OfType<XText>().Select(ss => ss.Value).StringConcatenate(), r = rx.Match(s.Nodes().OfType<XText>().Select(ss => ss.Value).StringConcatenate()) })
                                .Where(s => s.text.Trim().IndexOf("ledd") != -1 && s.r.Groups["ledd"].Success)
                                .Select(s =>
                                    new XElement("tag3",
                                        new XAttribute("id", (string)s.e.Parent.Attributes("id").FirstOrDefault()),
                                        new XAttribute("title", s.text),
                                        new XElement("name", "l" + ln.Where(v => v.Split(';').Last() == s.r.Groups["ledd"].Value).First().Split(';').First()),
                                        s.r.Groups["bokstav"].Success ? new XElement("name", "l" + ln.Where(v => v.Split(';').Last() == s.r.Groups["ledd"].Value).First().Split(';').First() + "/" + s.r.Groups["bokstav"].Value) : null
                                    )
                                )
                            )
                        )
                    )
                )
            );

            return internaltags;
        }
        public static string CapitalizeFirstLetter(this string s)
        {
            if (String.IsNullOrEmpty(s)) return s;
            if (s.Length == 1) return s.ToUpper();
            return s.Remove(1).ToUpper() + s.Substring(1);
        }
        public static void RenameAttribute(this XElement e, string name, string newName)
        {
            if (e.Attributes(newName).FirstOrDefault() == null)
            {
                e.Add(new XAttribute(newName, e.Attribute(name).Value.ToLower()));
                e.Attribute(name).Remove();
            }
        }

        public static XElement MakeDl(this XElement p)
        {
            List<XNode> before = new List<XNode>();
            before.AddRange(p.Nodes().TakeWhile(r => (r.NodeType == XmlNodeType.Text ? ((XText)r).Value.Contains(":") : false) == false));
            XText t = (XText)p.Nodes().SkipWhile(r => (r.NodeType == XmlNodeType.Text ? ((XText)r).Value.Contains(":") : false) == false).Take(1).FirstOrDefault();
            if (t.Value.Split(':').FirstOrDefault() != null)
            {
                before.Add(new XText(t.Value.Split(':').FirstOrDefault() ?? ""));
            }

            List<XNode> after = new List<XNode>();
            if (t.Value.Split(':').LastOrDefault() != null)
            {
                after.Add(new XText(t.Value.Split(':').LastOrDefault() ?? ""));
            }
            after.AddRange(p.Nodes().SkipWhile(r => (r.NodeType == XmlNodeType.Text ? ((XText)r).Value.Contains(":") : false) == false).Skip(1));

            XElement dl = new XElement("dl",
                    new XElement("dt", before),
                    new XElement("dd", after)
            );
            dl.Descendants("dt").DescendantNodes().OfType<XElement>().Where(i => i.Name.LocalName == "i").ToList().ForEach(i => i.ReplaceWith(i.Nodes()));
            dl.Descendants("dt").DescendantNodes().OfType<XText>().ToList().ForEach(tx => tx.Value = Regex.Replace(tx.Value, @"(\«|\»)", ""));

            return dl;
        }
        public static string GetHeaderText(this XElement e)
        {
            if (e == null) return "";
            string text = e.Elements()
                    .Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d+"))
                    .DescendantNodes()
                    .OfType<XText>()
                    .Where(s => s.Ancestors().Where(t => "a;sup".Split(';').Contains(t.Name.LocalName)).Count() == 0)
                    .Select(s => s.Value)
                    .StringConcatenate();
            text = Regex.Replace(text.Trim(), @"^(\d+|Del\s\d+)(\.)?(\s+)?", "");
            return text;
        }
        public static void FormatHeader(this XElement p)
        {
            if (p.DescendantNodes().OfType<XText>().Count() == 0) return;
            p.DescendantNodes().OfType<XText>().ToList().ForEach(t => t.Value = Regex.Replace(t.Value, @"\s+", " "));
            //p.ReplaceWith(new XElement(p.Name.LocalName, Regex.Replace(p.DescendantNodes().OfType<XText>().Select(s => s.ToString()).StringConcatenate().Trim(), @"\s+", " ")));
            p.DescendantNodes().OfType<XText>().LastOrDefault().Value.TrimEnd();
            p.DescendantNodes().OfType<XText>().FirstOrDefault().Value.TrimStart();
        }
        public static IEnumerable<XElement> MapIndex(this XElement i)
        {
            if (i == null) return null;
            return i.Elements().Where(p => p.Attributes("title").FirstOrDefault() != null).Select(p => new XElement("item", p.Attributes(), p.MapIndex()));
        }
        public static void GetSearchItems(this XElement e)
        {
            switch (e.Name.LocalName)
            {
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                case "h7":
                    break;
                case "p":
                case "td":
                case "li":
                    {
                        e.AddAnnotation(new SearchItem(e));
                    }
                    break;

                case "section":
                    if (e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d+")).FirstOrDefault() != null)
                    {
                        e.AddAnnotation(new SearchItem(e));
                        e.Elements().ToList().ForEach(p => p.GetSearchItems());
                        break;

                    }
                    else if ((string)e.Attributes("class").FirstOrDefault() == "lovdata-ledd")
                    {
                        e.AddAnnotation(new SearchItem(e));
                        break;
                    }
                    else
                    {
                        e.Elements().ToList().ForEach(p => p.GetSearchItems());
                        break;
                    }
                    
                default:
                    e.Elements().ToList().ForEach(p => p.GetSearchItems()); break;
            }

        }
        public static void AddNavigation(this XElement segment, XElement map)
        {
            string segment_id = (string)segment.Attributes("segment_id").FirstOrDefault();
            XElement document = segment.Elements("document").FirstOrDefault();
            XElement next =  map.Descendants().SkipWhile(p => (string)p.Attributes("id").FirstOrDefault() != segment_id).Skip(1).SkipWhile(p => (string)p.Attributes("s").FirstOrDefault() != "1").Take(1).FirstOrDefault();
            XElement previouse = map.Descendants().Reverse().SkipWhile(p => (string)p.Attributes("id").FirstOrDefault() != segment_id).Skip(1).SkipWhile(p => (string)p.Attributes("s").FirstOrDefault() != "1").Take(1).FirstOrDefault();
            if (next != null)
                document.Add(new XElement("section",
                    new XElement("a",
                        new XAttribute("class", "iref"),
                        new XAttribute("data-segment", (string)next.Attributes("id").FirstOrDefault()),
                        new XAttribute("data-id", (string)next.Attributes("id").FirstOrDefault()),
                        new XText("Neste >")
                        )
                    )
                );
            if (previouse != null)
                document.AddFirst(new XElement("section",
                    new XElement("a", 
                        new XAttribute("class", "iref"), 
                        new XAttribute("data-segment", (string)previouse.Attributes("id").FirstOrDefault()),
                        new XAttribute("data-id", (string)previouse.Attributes("id").FirstOrDefault()),
                        new XText("< Forrige")
                        )
                    )
                );
        }
        public static void AddSearch(this XElement s)
        {
            string segment_id = (string)s.Attributes("segment_id").FirstOrDefault();
            string searchName = (string)s.Attributes("searchname").FirstOrDefault();
            if (searchName != null)
            {
                if (Regex.IsMatch(searchName.Trim(), @"^Kapittel\s+\d+(\.)?"))
                    searchName = Regex.Replace(searchName.Trim(), @"^Kapittel\s+\d+(\.)?", "");
            }
            //if (s.Elements("document").Elements("section").Elements("h1").Where(p => Regex.IsMatch(p.Value, @"^Del\s\d+")).FirstOrDefault() != null)
            //    searchName = s.Elements("document").Elements("section").Elements("section").Elements("h2").Select(p => p.Value).FirstOrDefault();
            //else
            //    searchName = s.Elements("document").Elements("section").Elements().Where(p=>Regex.IsMatch(p.Name.LocalName,@"h\d")).Select(p => p.Value).FirstOrDefault();

            ////string searchName = s.Descendants("section").Where(p => Regex.IsMatch(((string)p.Attributes("id").FirstOrDefault() ?? ""), @"^\d+$")).Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).Nodes().OfType<XText>().Select(p => p.Value.ToString()).StringConcatenate();
            ////if (searchName == "")
            ////{
            ////    XElement fh = s.Descendants("section").Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).FirstOrDefault();
            ////    if (fh != null)
            ////    {
            ////        searchName = fh.Nodes().OfType<XText>().Select(p => p.Value.ToString()).StringConcatenate();
            ////    }
            ////}
            ////else
            ////{
            //searchName = Regex.Replace(searchName.Trim(), @"^\d+\s", "");
            ////}
            if (searchName == null) return;
            if (searchName != null)
            {
                s.Elements().ToList().ForEach(p => p.GetSearchItems());
                XElement searchitems = new XElement("searchitems",
                    new XAttribute("parent_id", segment_id),
                    new XAttribute("searchname", searchName),
                    s.Descendants()
                        .Where(p => (string)p.Attributes("id").FirstOrDefault() != null && (p.Annotation<SearchItem>() == null ? null : p.Annotation<SearchItem>().text) != null)
                        .Select(p => new XElement("search",
                                   new XAttribute("object_id", (string)p.Attributes("id").FirstOrDefault()),
                                   new XAttribute("text_type", p.Annotation<SearchItem>().type.ToString()),
                                   new XAttribute("parent_id", segment_id),
                                   new XText(p.Annotation<SearchItem>().text)
                               )
                    )
                );
                s.Add(searchitems);
            }
        }
        public static string GetLetters(this string text)
        {
            string test = text.Trim().ToLower();
            test = Regex.Replace(test, "[^a-zæøå0-9]", " ");
            test = Regex.Replace(test, @"\s+", " ");
            return test.Trim();

        }
        public static XElement AddIndex(this XElement map)
        {
            XElement mapitem = map;
            mapitem = mapitem.Descendants("item").Where(p => (string)p.Attributes("title").FirstOrDefault() != null && (string)p.Attributes("id").FirstOrDefault() != null).FirstOrDefault();

            XElement index = new XElement("index");
            if (mapitem == null) return index;

            foreach (XElement p in mapitem.Parent.Elements().Where(p=>(string)p.Attributes("title").FirstOrDefault()!=null && (string)p.Attributes("id").FirstOrDefault() != null))
            {
                index.Add(new XElement("item", p.Attributes(),p.MapIndex()));
            }
            return index;
        }
        public static void AddIndex(this XElement s, XElement map)
        {

            XElement mapitem = map.Descendants().Where(p => (string)p.Attributes("id").FirstOrDefault() == (string)s.Attributes("segment_id").FirstOrDefault()).FirstOrDefault();
            if (mapitem == null) return;
            XElement index = new XElement("index");
            XElement current = index;
            XElement last = null;
            foreach (XElement p in mapitem.AncestorsAndSelf().Where(p => p.Attributes("title").FirstOrDefault() != null).Reverse())
            {
                current.Add(p.NodesBeforeSelf().OfType<XElement>().Where(i => i.Attributes("title").FirstOrDefault() != null).Select(i => new XElement("item", i.Attributes())));
                XElement item = new XElement("item", p.Attributes());
                current.Add(item);
                current.Add(p.NodesAfterSelf().OfType<XElement>().Where(i => i.Attributes("title").FirstOrDefault() != null).Select(i => new XElement("item", i.Attributes())));
                current = item;
                last = p;
            }
            current.Add(last.MapIndex());
            s.Add(index);
        }
        public static XElement MapSectionItem(this XElement e)
        {
            ElementData ed = null;
            if (e.Annotation<ElementData>() == null)
            {
                ed = new ElementData(e);
                e.AddAnnotation(ed);
            }
            else
            {
                ed = e.Annotation<ElementData>();
            }

            return new XElement("item",
                e.Attributes("id"),
                ed.IsSegment && ed.Generated == false ? new XAttribute("s", "1") : null,
                e.Name.LocalName == "section" && e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).Count() != 0
                ? (
                    ((string)e.Attributes("content_title").FirstOrDefault() ?? "") == ""
                    ? new XAttribute("title", e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).Select(p => p.DescendantNodes().OfType<XText>().Select(s => s.Value).StringConcatenate()).FirstOrDefault() ?? "")
                    : new XAttribute("title", (string)e.Attributes("content_title").FirstOrDefault())
                  )
                : null,
                e.MapSection()
            );
        }

        public static IEnumerable<XElement> MapSection(this XElement e)
        {
            if (e.Annotation<ElementData>() == null)
            {
                ElementData ed = new ElementData(e);
                e.AddAnnotation(ed);
            }
            return e.Elements().Where(p=>p.DescendantsAndSelf().Attributes("id").FirstOrDefault()!=null).Select(p => p.MapSectionItem());
        }

        public static XElement MapItem(this XElement e)
        {
            if (e.Attributes("x-nav").FirstOrDefault() != null) return null;
            return new XElement("item",
                e.Attributes("id"),
                (string)e.Attributes("segment").FirstOrDefault() == "true" ? new XAttribute("s", "1") : null,
                "section;Del;Forord;Kapittel;Seksjon;Subsek1;Subsek2;Subsek3;Subsek4;Subsek5;Subsek6".Contains(e.Name.LocalName) ? new XAttribute("title", e.Elements().Where(p => "Tittel".Split(';').Contains(p.Name.LocalName)).Select(p => p.DescendantNodes().OfType<XText>().Select(s => s.Value).StringConcatenate()).FirstOrDefault() ?? "") : null,
                e.Map()
            );
        }
        public static IEnumerable<XElement> Map(this XElement e)
        {
            return e.Elements().Select(p => p.MapItem());
        }
        public static void InternalLinks(this XText t, string type)
        {
            string value = t.Value;
            XElement Container = new XElement("container");
            int index = 0;
            MatchCollection mc = null;
            if (type == "avsnitt")
            {
                mc = Regex.Matches(value, @"((S|s)e(\sogså)?\skap\.|(A|a)vsnitt)\s(?<nr>(\d+((\.\d+)+)?))(\s(og|eller)\s(avsnitt\s)?(?<nr>(\d+((\.\d+)+)?)))?");
            }
            if (mc == null) return;
            if (mc.Count != 0)
            {
                foreach (Match m in mc)
                {
                    foreach (Capture c in m.Groups["nr"].Captures)
                    {
                        if (c.Index > index)
                        {
                            if ((c.Index - index) > 0) Container.Add(new XText(value.Substring(index, c.Index - index)));
                            Container.Add(new XElement("Iref", c.Value));
                            index = c.Index + c.Length;
                        }

                    }
                }
                if ((value.Length) - index > 0)
                    Container.Add(new XText(value.Substring(index, (value.Length) - index)));
                t.ReplaceWith(Container.Nodes());
            }
        }
        public static void SetLevelIdNotNumbered(this XElement e)
        {
            string id = (string)e.Attributes("id").FirstOrDefault();
            int n = 1;
            e.Descendants().ToList().ForEach(p => p.SetAttributeValueEx("id", id + "-" + (n++).ToString()));

        }
        public static void SetLevelIdNumbered(this XElement e)
        {
            Regex nr = new Regex(@"^(?<nr>(\d+\.\d+((\.\d+)+)?))((\.)?\s)");
            string id = nr.Match((e.Elements("title").Select(s => s.Value).FirstOrDefault() ?? "").Trim().ToLower()).Groups["nr"].Value;
            int i = 0;
            e.SetAttributeValueEx("id", id);
            List<XElement> el = e.Descendants()
                .TakeWhile(p => p.Name.LocalName != "level")
                .ToList();
            foreach (XElement n in el)
            {
                if (n.Name.LocalName != "text")
                {
                    i++;
                    n.SetAttributeValueEx("id", id + "-" + i.ToString());
                }
            }
        }
        public static void IdentyfyDescendants(this XElement el)
        {
            int n = 1;
            string currId = (string)el.Attributes("id").FirstOrDefault();
            List<XElement> d = el.Descendants().Where(p =>
                   ((string)p.Attributes("id").FirstOrDefault() ?? "") == ""
                && (string)p.Ancestors().Where(a => ((string)a.Attributes("id").FirstOrDefault() ?? "") != "").Attributes("id").FirstOrDefault() == currId
                ).ToList();

            foreach (XElement e in d)
            {
                e.SetAttributeValueEx("id", currId + "-" + (n++).ToString());
            }
        }
        public static void UpdateSubID(this List<XElement> el)
        {

            foreach (XElement ee in el)
            {
                int n = 1;
                foreach (XElement e in ee.Descendants().TakeWhile(p => !("Del;Kapittel;Seksjon;Subsek1;Subsek2;Subsek3;Subsek4;Subsek5;Subsek6".Split(';').Contains(p.Name.LocalName))))
                {
                    XElement parent = e.Ancestors().Where(p => "Del;Kapittel;Seksjon;Subsek1;Subsek2;Subsek3;Subsek4;Subsek5;Subsek6".Split(';').Contains(p.Name.LocalName)).FirstOrDefault();
                    XAttribute pid = parent.Attributes("id").FirstOrDefault();
                    XAttribute id = e.Attributes("id").FirstOrDefault();
                    if (pid != null)
                    {
                        if (Regex.IsMatch(pid.Value, @"^([IVX]+|(\d+((\.\d+)+)?))(\-\d+)?$"))
                        {
                            if (id != null)
                                id.SetValue(pid.Value + "-" + n.ToString());
                            else
                                e.Add(new XAttribute("id", pid.Value + "-" + n.ToString()));
                            n++;
                        }
                    }
                }
            }


        }
        public static void UpdateID(this XElement e)
        {
            string tittel = e.Elements("Tittel").DescendantNodes().OfType<XText>().Select(p => Regex.Match(p.Value, @"^(\d+((\.\d+)+)?|[IVX]+)(?=\s)").Value).Where(p => p.Trim() != "").FirstOrDefault();
            if (tittel != null)
            {
                XAttribute id = e.Attributes("id").FirstOrDefault();
                if (id != null)
                {
                    id.SetValue(tittel);
                }
            }
        }
        public static void ReplaceSpace(this XText t)
        {
            string text = t.Value;
            text = Regex.Replace(text, @"\r\n", " ");
            text = Regex.Replace(text, @"\s+", " ");
            if (Regex.IsMatch(text, @"(?<=[A-ZÆØÅa-zæøå])?(?=[A-ZÆØÅa-zæøå])"))
            {
                text = Regex.Replace(text, @"(?<=[A-ZÆØÅa-zæøå])?(?=[A-ZÆØÅa-zæøå])", "");
            }

            if ("Tittel;Avsnitt;Petit;p;td;li;title".Split(';').Contains(t.Parent.Name.LocalName))
            {
                if (t == t.Parent.DescendantNodes().OfType<XText>().FirstOrDefault())
                {
                    text = text.TrimStart();
                }
                if (t == t.Parent.DescendantNodes().OfType<XText>().LastOrDefault())
                {
                    text = text.TrimEnd();
                }
            }
            if ("span;strong;em".Split(';').Contains(t.Parent.Name.LocalName) && t == t.Parent.Parent.DescendantNodes().OfType<XText>().First() && t.Parent.Parent.Name.LocalName == "p")
            {
                text = text.TrimStart();
            }
            if ("span;strong;em".Split(';').Contains(t.Parent.Name.LocalName) && t == t.Parent.Parent.DescendantNodes().OfType<XText>().Last() && t.Parent.Parent.Name.LocalName == "p")
            {
                text = text.TrimEnd();
            }


            t.ReplaceWith(new XText(text));
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
        public static void VariableUpdate(this XElement variable, XElement triggervariable)
        {
            string name = variable.Elements("name").Select(p => p.Value).FirstOrDefault();
            string id = variable.Elements("id").Select(p => p.Value).FirstOrDefault();
            DGVariableEval dgv = new DGVariableEval(name, id);
            if (dgv.collection)
            {
                if (variable.Elements("variable").Count() == 0 && triggervariable.Elements("variable").Count() != 0)
                {
                    foreach (XElement v in triggervariable.Elements("variable"))
                    {
                        string vname = (string)v.Attributes("name").FirstOrDefault();
                        string nr = dgv.GetNumberInName(vname);
                        XElement newvar = new XElement("variable",
                            new XElement("id", dgv.SetNumberInId(nr)),
                            new XElement("value", v.Value)
                            );
                        variable.Add(newvar);
                    }
                }
                else if (variable.Elements("variable").Count() != 0 && triggervariable.Elements("variable").Count() != 0)
                {
                    List<XElement> ex_vars = variable.Elements("variable").ToList();
                    variable.Elements("variable").Remove();
                    string nr = "";
                    foreach (XElement v in triggervariable.Elements("variable"))
                    {
                        string vname = (string)v.Attributes("name").FirstOrDefault();
                        nr = dgv.GetNumberInName(vname);
                        if (nr != null)
                        {
                            XElement ex_var = ex_vars.Where(p => dgv.GetNumberInId((p.Elements("id").Select(s => s.Value).FirstOrDefault() ?? "")) == nr).FirstOrDefault();
                            if (ex_var != null)
                            {
                                variable.Add(new XElement(ex_var));
                                ex_vars.Where(p => dgv.GetNumberInId((p.Elements("id").Select(s => s.Value).FirstOrDefault() ?? "")) == nr).Remove();
                            }
                            else
                            {
                                XElement newvar = new XElement("variable",
                                    new XElement("id", dgv.SetNumberInId(nr)),
                                    new XElement("value", v.Value)
                                    );
                                variable.Add(newvar);
                            }
                        }
                    }
                    if (ex_vars.Count() != 0)
                    {
                        variable.Add(ex_vars.Select(p => new XElement(p)));
                    }
                }
            }
            else
            {
                XElement value = variable.Elements("value").FirstOrDefault();

                if (triggervariable.Value != "")
                {
                    if (value == null)
                        variable.Add(new XElement("value", triggervariable.Value));
                    else
                    {
                        if (value.Value.Trim() == "")
                            value.SetValue(triggervariable.Value);
                    }
                }
            }

        }
        public static XElement InsertProffData(this XElement variables, XElement triggerdata)
        {
            (
                from v in variables.Elements("variable").Where(p => p.Elements("name").FirstOrDefault() != null)
                join tv in triggerdata.Elements("variable")
                on
                    (v.Elements("name").Select(p => p.Value).FirstOrDefault() ?? "-1").Trim().ToLower()
                    equals
                    ((string)tv.Attributes("name").FirstOrDefault() ?? "-0").Trim().ToLower()
                select new { var = v, trig = tv }
            ).ToList()
            .ForEach(p => p.var.VariableUpdate(p.trig));
            return variables;
        }
        public static void AddLinkInfo(this XElement document, XElement xlinkgroup)
        {
            (
                from dl in document.Descendants("a").Where(s => (string)s.Attributes("class").FirstOrDefault() == "diblink")
                join rf in xlinkgroup.Descendants("ref")
                on ((string)dl.Attributes("href").FirstOrDefault() ?? "") equals "#" + ((string)rf.Attributes("id").FirstOrDefault() ?? "")
                join to in xlinkgroup.Descendants("topic")
                on (string)rf.Parent.Attributes("topic_id").FirstOrDefault() equals (string)to.Attributes("topic_id").FirstOrDefault()
                select new { link = dl, topicinfo = to }
                )
                .Union(
                from dl in document.Descendants().Where(p => "diblink;dibparameter".Split(';').Contains(p.Name.LocalName))
                join rf in xlinkgroup.Descendants("ref")
                on (string)dl.Attributes().Where(p => "refid;rid".Split(';').Contains(p.Name.LocalName)).FirstOrDefault() equals (string)rf.Attributes("id").FirstOrDefault()
                join to in xlinkgroup.Descendants("topic")
                on (string)rf.Parent.Attributes("topic_id").FirstOrDefault() equals (string)to.Attributes("topic_id").FirstOrDefault()
                select new { link = dl, topicinfo = to }
                ).ToList()
                .ForEach(p => p.link.AddAnnotation(new TopicInfo(p.topicinfo)));
        }
        public static IEnumerable<XNode> DibLinkRaw(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if ((string)e.Attributes("ided").FirstOrDefault() == "0")
            {
                result.Add(new XText(e.Nodes().OfType<XText>().Select(t => t.ToString()).StringConcatenate()));
            }
            else
            {
                result.Add(new XElement("a",
                        e.Attributes("id"),
                        new XAttribute("href", "#" + (string)e.Attributes("refid").FirstOrDefault()),
                        new XAttribute("class", "diblink"),
                        new XAttribute("data-refid", (string)e.Attributes("refid").FirstOrDefault()),
                        (string)e.Attributes("replaceText").FirstOrDefault() == null ? null : new XAttribute("data-replacetext", (string)e.Attributes("replaceText").FirstOrDefault()),
                        new XText(e.Nodes().OfType<XText>().Select(t => t.ToString()).StringConcatenate())
                    )
                );
            }
            return result;
        }
        public static IEnumerable<XNode> Diblink(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            TopicInfo ti = e.Annotation<TopicInfo>();
            if (ti != null)
            {
                result.Add(new XElement("a",
                        new XAttribute("class", "diblink topic"),
                        new XAttribute("href", "#" + (string)e.Attributes("refid").FirstOrDefault()),
                        new XAttribute("data-topictitle", ti.title),
                        new XAttribute("data-topictype", ti.topictype),
                        new XAttribute("data-topicid", ti.id),
                        new XAttribute("data-view", ti.view),
                        new XAttribute("data-language", ti.language),
                        (
                            (string)e.Attributes("replaceText").FirstOrDefault() ?? "") == ""
                            ? e.Nodes().OfType<XText>().Select(t => t.ToString()).StringConcatenate()
                            : ti.title
                        )
                    );

            }
            else if ((string)e.Attributes("ided").FirstOrDefault() == "0")
            {
                result.Add(new XText(e.Nodes().OfType<XText>().Select(t => t.ToString()).StringConcatenate()));
            }
            else
            {
                if (e.Name.LocalName == "dibparameter")
                {
                    result.Add(new XElement("a",
                        new XAttribute("class", "diblink"),
                        new XAttribute("href", "#" + (string)e.Attributes("refid").FirstOrDefault()),
                        new XText((string)e.Attributes("replaceText").FirstOrDefault())
                        )
                    );
                }
                else
                {
                    result.Add(new XElement("a",
                        new XAttribute("class", "diblink"),
                        new XAttribute("href", "#" + (string)e.Attributes("refid").FirstOrDefault()),
                         e.Nodes().OfType<XText>().Select(t => t.ToString()).StringConcatenate()
                        )
                    );
                }
            }
            return result;
        }
        public static XAttribute GetAttributeToLower(this XElement e, string name)
        {
            string value = (string)e.Attributes().Where(p => p.Name.LocalName.ToLower() == name.ToLower()).FirstOrDefault();
            return value == null ? null : new XAttribute(name.ToLower(), value.ToLower());
        }
        public static XElement FormatTopicInfo(this IGrouping<string, TopicInfoGrouping> p, XElement topics)
        {
            XElement topic = topics.Elements("topic").Where(t => (string)t.Attributes("topic_id").FirstOrDefault() == p.Key).FirstOrDefault();
            if (topic == null) return null;
            if (p.Where(s => s.bm != "").Count() == 0)
            {
                return new XElement("li",
                     new XElement("a",
                                new XAttribute("class", "diblink"),
                                new XAttribute("href", "#tid=" + p.Key),
                                new XText((string)topic.Attributes("name").FirstOrDefault())
                            )
                        );
            }
            else
            {
                return new XElement("li",
                     new XText((string)topic.Attributes("name").FirstOrDefault()),
                     new XElement("ul",
                        new XAttribute("class", "topicinfoitems"),
                        p.Where(s => s.bm != "")
                        .Select(s => new XElement("li",
                                new XElement("a",
                                    new XAttribute("class", "diblink"),
                                    new XAttribute("href", "#tid=" + p.Key + "//id=" + s.bm),
                                    new XText((string)topic.Descendants("bookmark").Where(t => ((string)t.Attributes("id").FirstOrDefault() ?? "").ToLower() == s.bm.ToLower()).Attributes("title").FirstOrDefault() ?? "undefined")
                                )
                            )
                        )
                    )
                );
            }


        }
        public static void ReplaceReferences(this XElement r, XElement topics)
        {
            r.ReplaceWith(new XElement("ul",
                new XAttribute("class", "topicinfo"),
                r.Element("topicinfo")
                .Attributes("id")
                .Where(p => p.Value != "")
                .Select(p => new TopicInfoGrouping(p))
                .GroupBy(p => p.topic_id)
                .Select(p => p.FormatTopicInfo(topics))
                )
            );
        }
        public static string ElementDescendantText(this XElement e)
        {
            if (e == null) return "";
            return e.DescendantNodes().OfType<XText>().Select(t => t.ToString()).StringConcatenate();
        }
        public static string ElementText(this XElement e)
        {
            if (e == null) return "";
            return e.Nodes().OfType<XText>().Select(t => t.ToString()).StringConcatenate();
        }
        public static void EvalNVariable(this XElement v)
        {
            if (v == null) return;

            string id = v.Elements("id").Select(p => p.Value).FirstOrDefault();
            if (id != null)
            {
                Regex number = new Regex(Regex.Replace(id, @"\*N\*", @"(?<nr>(\d+))"));
                v.Add(new XAttribute("counter", "true"));
                List<int> nos = new List<int>();
                foreach (XElement e in v.Elements("variable"))
                {
                    string vid = e.Elements("id").Select(p => p.Value).FirstOrDefault();
                    string no = number.Match(vid).Groups["nr"].Value;
                    e.Add(new XElement("nr", no));
                    nos.Add(Convert.ToInt32(no));
                }
                if (nos.Count() != 0)
                {
                    v.Add(new XElement("numbers", nos.OrderBy(n => n).Select(p => p.ToString()).StringConcatenate(";")));
                }
            }
        }
        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.Default.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        public static string DecodeVariableId(this string text)
        {
            return text
                .Replace("-S-", " ")
                .Replace("-D-", ".")
                .Replace("-C-", ",")
                .Replace("-P-", "%")
                .Replace("-SL-", "/")
                .Replace("-SQ-", "'")
                .Replace("-Q-", "\"")
                .Replace("-QN-", "?")
                .Replace("-I-", "!")
                .Replace("-PL-", "+")
                .Replace("-LT-", "&lt;")
                .Replace("-GT-", "&gt;")
                .Replace("-BL-", "(")
                .Replace("-BR-", ")")
                .Replace("-A-", "&amp;");
        }
        public static IEnumerable<XElement> SortResultSetByTopicType(this IGrouping<string, XElement> set, string sort = "0")
        {
            List<XElement> result = new List<XElement>();

            switch (sort)
            {
                case "1":
                    result.AddRange(
                        set
                        //.OrderBy(item => item.TNum, new NaturalSortComparer<string>())
                        .OrderByDescending(p => ((string)p.Attributes("odate").FirstOrDefault() ?? "xxx"))
                        .Select(p => new XElement("result",
                                    p.Attributes(),
                                    p.Elements("items")
                            )
                        )
                    );
                    break;

                default:
                    result.AddRange(
                        set
                        .OrderBy(p => (((string)p.Attributes("short").FirstOrDefault() ?? "") + " " + ((string)p.Attributes("name").FirstOrDefault() ?? "")).ReplaceNOLetters(), new NaturalSortComparer<string>())
                        .Select(p => new XElement("result",
                                    p.Attributes(),
                                    p.Elements("items")
                            )
                        )
                    );
                    break;
            }
            return result;
        }
        public static string ReplaceNOLetters(this string s)
        {
            s = s.Trim().ToLower();

            //s = Regex.Replace(s, "å", "xxx");
            //s = Regex.Replace(s, "ø", "xxy");
            //s = Regex.Replace(s, "æ", "xxz");

            return s;
        }

        public static IEnumerable<XElement> SortResultSetByTopicTypeOrder(this IGrouping<int, XElement> set, XElement sort)
        {
            if (sort == null)
            {
                return set
                    .OrderBy(p => (((string)p.Attributes("short").FirstOrDefault() ?? "") + " " + ((string)p.Attributes("name").FirstOrDefault() ?? "")).ReplaceNOLetters(), new NaturalSortComparer<string>())
                    .Select(p => new XElement("result",
                                    p.Attributes(),
                                    p.Elements("items")
                            )
                        );
            }
            else
            {
                return (
                     from o in set.GroupBy(s => (string)s.Attributes("tid").FirstOrDefault())
                     join s in sort.Elements("order")
                     on o.Key equals (string)s.Attributes("id").FirstOrDefault() into so
                     from p in so.DefaultIfEmpty()
                     orderby Convert.ToInt32((p == null ? "0" : ((string)so.Attributes("no").FirstOrDefault() ?? "0")))
                     select new { e = o, s = p == null ? "0" : ((string)p.Attributes("na").FirstOrDefault() ?? "0") }
                ).ToList()
                .SelectMany(p => p.e.SortResultSetByTopicType(p.s))
                .ToList();
            }
        }

        public static XElement SetRelations(this XElement document, XElement relations)
        {
            if (relations != null)
            {
                (
                    from ai in relations.Elements("item")
                    join i in document.DescendantsAndSelf()
                    on (string)ai.Attributes("id").FirstOrDefault() equals (string)i.Attributes("id").FirstOrDefault()
                    select new { acc = ai, item = i }
                )
                .ToList()
                .ForEach(p => p.item.Add(new XAttribute("x-relations", "true")));
            }
            return document;
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
