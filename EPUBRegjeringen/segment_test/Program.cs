using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
namespace segment_test
{
    public class NavPointData
    {
        public string segmentname { get; set; }
        public string id { get; set; }
        public string src { get; set; }
        public NavPointData (string filename) 
        {
            src = filename;
            segmentname = src.Split('.').FirstOrDefault();
            id = src.Split('#').Count()<2 ? "" : src.Split('#').LastOrDefault();
        }
    }
    static class Extention
    {
        public static IEnumerable<XElement> GetItem(this XElement e)
        {
            List<XElement> result = new List<XElement>();
            string search = "";
            //if ((string)e.Attributes("id").FirstOrDefault() == "720d158a-cb2e-4405-b39c-43875ff41b49") search = search; 
            switch (e.Name.LocalName)
            {
                case "li":
                    search = e.Nodes()
                        .TakeWhile(p => 
                            p.NodeType == System.Xml.XmlNodeType.Text 
                            || !"ol;ul;p".Split(';').Contains((p.NodeType == System.Xml.XmlNodeType.Element ? ((XElement)p).Name.LocalName : "")) 
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
                        e.Elements().SelectMany(p=>p.GetItem())
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
                .Where(p =>  p.IsPart("TIT"))
                .Select(p => 
                    new XElement("item",
                        p.Parent.Attributes("id"),
                        new XAttribute("title", p.DescendantNodes().OfType<XText>().Select(s=>s.ToString()).StringConcatenate()),
                        new XAttribute("search", p.DescendantNodes().OfType<XText>().Select(s=>s.ToString()).StringConcatenate()),
                        new XAttribute("text_type", "1"),
                        p.NodesAfterSelf()
                        .OfType<XElement>()
                        .TakeWhile(s=>!s.IsPart("DEL"))
                        .SelectMany(s=>s.GetItem()),
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
                return e.Elements().SelectMany(p=>p.GetParts());
            }
            //div[substring-before(substring-after(@class, '-'),'-') = 'DEL']
            //*[substring-before(substring-after(@class, '-'),'-') = 'TIT']">

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

                //return e.Elements("div").Where(p => p.IsPart("DEL")).SelectMany(p => p.GetPart());
            }
            
        }
        public static IEnumerable<XElement> GetBookcontentItems(this XElement bookcontent)
        {
            return  bookcontent.Descendants("body").SelectMany(p=>p.GetParts());
                
                    
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
                        new XAttribute("title", p.Elements("navLabel").Elements("text").DescendantNodes().OfType<XText>().Select(s=>s.ToString()).StringConcatenate().ReplaceTocText().Trim()),
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
    class Program
    {
        private static void CreateStylesheet(XElement result, string path, string name)
        {
            XNamespace xsl = XNamespace.Get("http://www.w3.org/1999/XSL/Transform");
            XNamespace msxsl = XNamespace.Get("urn:schemas-microsoft-com:xslt");


            XDocument stylesheet = new XDocument(
                new XDeclaration("1.0", "utf-8", "no"),
                new XElement(xsl + "stylesheet",
                    new XAttribute("version", "1.0"),
                    new XAttribute(XNamespace.Xmlns + "xsl", xsl),
                    new XAttribute(XNamespace.Xmlns + "msxsl", msxsl),
                    new XAttribute("exclude-result-prefixes", "msxsl")
                    )
            );

            stylesheet.Root.Add(new XElement(xsl + "output",
                new XAttribute("method", "xml"),
                new XAttribute("indent", "yes")
                ));


            stylesheet.Root.Add(
                result.Descendants()
                .Where(p => p.Ancestors("settings").Count() == 0)
                .Where(p => !"segment;segments".Split(';').Contains(p.Name.LocalName))
                .Select(p => new
                {
                    name = p.Name.LocalName,
                    classname = ((string)p.Attributes("class").FirstOrDefault() ?? "").Trim() != "" && ((string)p.Attributes("class").FirstOrDefault() ?? "").Trim().Split(' ').Count() == 1 && ((string)p.Attributes("class").FirstOrDefault() ?? "").Trim().Split('-').Count() > 2
                    ? ((string)p.Attributes("class").FirstOrDefault() ?? "").Trim()
                    : "",
                    attr = p.Attributes().Where(r => r.Parent.Name.LocalName != "navpoint" && !"alt;src;ID;href;id;class".Split(';').Contains(r.Name.LocalName)).Select(r => new { name = r.Name.LocalName, value = r.Value }).GroupBy(r => r).ToList()
                })

                .GroupBy(p => new { name = p.name, classname = p.classname.Trim() })
                .OrderBy(p => p.Key.classname.Split('-').Count() > 1 ? p.Key.classname.Split('-').ElementAt(1) : "")
                .Select(p =>
                    new XElement(xsl + "template",
                        new XAttribute("match", p.Key.name + ((p.Key.classname != "" && p.Key.classname.Split(' ').Count() == 1) ? "[@class='" + p.Key.classname + "']" : "")),
                        p.SelectMany(r => r.attr.Select(v => new XComment(v.Key.name + @"->" + v.Key.value))),
                         new XElement(xsl + "element",
                            new XAttribute("name", "{local-name()}"),
                            new XElement(xsl + "for-each",
                                new XAttribute("select", "@*"),
                                    new XElement(xsl + "if",
                                        new XAttribute("test", "local-name()!='ID'"),
                                        new XElement(xsl + "copy-of",
                                                new XAttribute("select", ".")
                                        )
                                    )
                            ),
                            new XElement(xsl + "apply-templates")
                        )
                    )
                )
            );

            stylesheet.Save(path + name + "_main.xsl");

            stylesheet = new XDocument(
                            new XDeclaration("1.0", "utf-8", "no"),
                            new XElement(xsl + "stylesheet",
                                new XAttribute("version", "1.0"),
                                new XAttribute(XNamespace.Xmlns + "xsl", xsl),
                                new XAttribute(XNamespace.Xmlns + "msxsl", msxsl),
                                new XAttribute("exclude-result-prefixes", "msxsl")
                                )
                        );

            stylesheet.Root.Add(new XElement(xsl + "output",
                new XAttribute("method", "xml"),
                new XAttribute("indent", "yes")
                ));


            stylesheet.Root.Add(
                result.Descendants()
                 .Where(p => p.Ancestors("settings").Count() == 0)
                .Where(p => !"segment;segments".Split(';').Contains(p.Name.LocalName))
                .Select(p => new { name = p.Name.LocalName, classname = ((string)p.Attributes("class").Select(s => s.Value).FirstOrDefault() ?? "").Trim() })
                .GroupBy(p => p)
                .Where(p =>
                    p.Key.classname.Split('-').Count() > 1 ? ("TIT;DEL").Split(';').Contains(p.Key.classname.Split('-').ElementAt(1)) : false)
                .OrderBy(p => p.Key.classname.Split('-').Count() > 1 ? p.Key.classname.Split('-').ElementAt(1) : "")
                    .Select(p =>
                        new XElement(xsl + "template",
                           new XAttribute("match", p.Key.name + ((p.Key.classname == "" || p.Key.classname.Split(' ').Count() > 1) ? "" : "[@class='" + p.Key.classname + "']")),

                           new XElement(xsl + "apply-templates")
                        )
                    )
            );

            stylesheet.Save(path + name + "_idx.xsl");
        }
        static void Main0(string[] args)
        {
            XElement tag1 = new XElement("tags");
            string id = "lov-1967-02-10";// (string)d.Root.Attributes("id").FirstOrDefault();
            Regex mid1 = new Regex(@"(?<name>([a-zæøå]+))\-(?<year>(\d\d\d\d))\-(?<mm>(\d\d))\-(?<day>(\d\d))\-(?<nr>(\d+))");
            //for-20090520-0556
            Regex mid2 = new Regex(@"(?<name>([a-zæøå]+))\-(?<year>(\d\d\d\d))(?<mm>(\d\d))(?<day>(\d\d))\-(?<nr>(\d+))");
            Regex mid3 = new Regex(@"(?<name>([a-zæøå]+))\-(?<year>(\d\d\d\d))\-(?<mm>(\d\d))\-(?<day>(\d\d))$");
            Regex mid4 = new Regex(@"(?<name>([a-zæøå]+))\-(?<year>(\d\d\d\d))(?<mm>(\d\d))(?<day>(\d\d))$");
            if (id != null)
            {
                string t4 = "";
                string t5 = "";

                if (mid1.IsMatch(id))
                {
                    Match m1 = mid1.Match(id);
                    t4 = m1.Groups["name"].Value + "-" + m1.Groups["year"].Value + "-" + m1.Groups["mm"].Value + "-" + m1.Groups["day"].Value + "-" + Convert.ToInt32(m1.Groups["nr"].Value).ToString();
                    t5 = m1.Groups["year"].Value + "-" + m1.Groups["mm"].Value + "-" + m1.Groups["day"].Value + "-" + Convert.ToInt32(m1.Groups["nr"].Value).ToString();
                    tag1.Add(new XElement("name", t4));
                    tag1.Add(new XElement("name", t5));
                    id = t4;
                }
                else if (mid2.IsMatch(id))
                {
                    Match m2 = mid2.Match(id);
                    t4 = m2.Groups["name"].Value + "-" + m2.Groups["year"].Value + "-" + m2.Groups["mm"].Value + "-" + m2.Groups["day"].Value + "-" + Convert.ToInt32(m2.Groups["nr"].Value).ToString();
                    t5 = m2.Groups["year"].Value + "-" + m2.Groups["mm"].Value + "-" + m2.Groups["day"].Value + "-" + Convert.ToInt32(m2.Groups["nr"].Value).ToString();
                    tag1.Add(new XElement("name", t4));
                    tag1.Add(new XElement("name", t5));
                    id = t4;
                }
                else if (mid3.IsMatch(id))
                {
                    Match m3 = mid3.Match(id);
                    t4 = m3.Groups["name"].Value + "-" + m3.Groups["year"].Value + "-" + m3.Groups["mm"].Value + "-" + m3.Groups["day"].Value + "-0";
                    t5 = m3.Groups["year"].Value + "-" + m3.Groups["mm"].Value + "-" + m3.Groups["day"].Value + "-0";
                    tag1.Add(new XElement("name", t4));
                    tag1.Add(new XElement("name", t5));
                    id = t4;
                }
                else if (mid4.IsMatch(id))
                {
                    Match m4 = mid4.Match(id);
                    t4 = m4.Groups["name"].Value + "-" + m4.Groups["year"].Value + "-" + m4.Groups["mm"].Value + "-" + m4.Groups["day"].Value + "-0";
                    t5 = m4.Groups["year"].Value + "-" + m4.Groups["mm"].Value + "-" + m4.Groups["day"].Value + "-0";
                    tag1.Add(new XElement("name", t4));
                    tag1.Add(new XElement("name", t5));
                    id = t4;
                }
                else
                {
                    //return new SqlXml(new XElement("error", "Feil id format: " + id).CreateReader());
                }
                //string t1 = id.Split('-')[0];
                //string t2 = id.Split('-')[1];
                //string t3 = id.Split('-')[2];
                //string t4 = t1 + "-" + t2.Substring(0, 4) + "-" + t2.Substring(4, 2) + "-" + t2.Substring(6, 2) + "-" + Convert.ToInt32(t3).ToString();
                //string t5 = t2.Substring(0, 4) + "-" + t2.Substring(4, 2) + "-" + t2.Substring(6, 2) + "-" + Convert.ToInt32(t3).ToString();

            }
        }
        static void Main(string[] args)
        {
            //string path = @"D:\_data\Prop 1 LS 2014-2015\2015_2016\OEBPS\";

            //string path = @"D:\_data\Prop 1 S\doc\OEBPS\";
            //string path = @"D:\_data\nou2015\10\OEBPS\";
            //string path = @"D:\_data\Regjeringen\prop\prp201520160014000\OEBPS\";
            //string path = @"D:\_data\Regjeringen\prop\prp201520160038000\OEBPS\";
            //string path = @"D:\_data\Regjeringen\Prop_120L_2015_2016\OEBPS\";
            //string path = @"D:\_data\Regjeringen\prop121_2016\OEBPS\";
            //string path = @"D:\_data\Regjeringen\nou2016_5\OEBPS\";
            //string path = @"D:\_data\Regjeringen\nou2016_11\OEBPS\";
            //string path = @"D:\_data\Regjeringen\prop_153_s_2015_2016\OEBPS\";
            //string path = @"D:\_data\Regjeringen\prop_1LS_2016_2017\OEBPS\";
            //string path = @"D:\_data\Regjeringen\prop_112l_2016_2017\OEBPS\";
            //string path = @"D:\_data\Regjeringen\nou2017_15\OEBPS\";
            //string path = @"D:\_data\Regjeringen\prop_100_l_2017_2018\prop_100_l_2017_2018\xml\OEBPS\";
            //string path = @"D:\_data\_regjeringen\98\OEBPS\";
            string path = @"D:\_data\_regjeringen\PROP\2020-2021\1LS\OEBPS\";
            string outPath = @"D:\_data\_regjeringen\PROP\2020-2021\1LS\";

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
            
            nav.Save(path + @"\xml\toc.xml");
            
            
            nav.Descendants("navpoint").ToList().ForEach(p=>p.AddAnnotation(new NavPointData((string)p.Attributes("src").FirstOrDefault())));

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
                XElement s = new XElement("segment",new XElement("bookcontent", segments.Nodes()));
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
                sd.Root.DescendantsAndSelf().Where(p => p.Attributes("id").FirstOrDefault() == null).ToList().ForEach(p => p.Add(new XAttribute("id", Guid.NewGuid().ToString())));

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
                

                s.DescendantsAndSelf().Attributes("id").ToList().ForEach(p=>p.SetValue(p.Value.ToLower()));
                s.DescendantNodesAndSelf().OfType<XText>().ToList().ForEach(p=>p.ReplaceWith(new XText(p.Value.ReplaceTocText())));
                result.Add(s);

 
            }


            result.Add( new XElement("segment",
                    new XAttribute("id", "_setup"),
                    new XElement("settings",
                        new XElement("startup",
                            new XAttribute("startsegment", "_top")
                        ),
                        new XElement("viewstyle",
                            new XElement("main", "regjeringen" + "_main.xsl"),
                            new XElement("zoom", "regjeringen" + "_main.xsl"),
                            new XElement("index", "regjeringen_idx.xsl")
                        ),
                        new XElement("indextags",
                            new XElement("indextag", new XAttribute("elementname", "navpoint"), new XAttribute("titletype", "attribute"), new XAttribute("titlename", "title")),
                            new XElement("indextag", new XAttribute("elementname", "div"), new XAttribute("titletype", "element"), new XAttribute("titlename", "h2")),
                            new XElement("indextag", new XAttribute("elementname", "div"), new XAttribute("titletype", "element"), new XAttribute("titlename", "h3")),
                            new XElement("indextag", new XAttribute("elementname", "p"), new XAttribute("titletype", "element"), new XAttribute("titlename", "a"))
                            )

                    )
                ));


            
            
            CreateStylesheet(result, path, "regjeringen");

            XElement sitem = new XElement("items");

            XElement top = result.Elements("segment").Where(p => (string)p.Attributes("id").FirstOrDefault() == "_top").FirstOrDefault();
            if (top != null)
            {
                sitem = new XElement("items",
                    top
                    .Descendants("navpoint")
                    .Select(p => new XElement("item",
                        p.Attributes("id"),
                        p.Attributes("title"),
                        new XAttribute("text_type", "16"),
                        new XAttribute("segment", "true"),
                        (
                            (result
                            .Elements("segment")
                            .Where(s => (string)s.Attributes("id").FirstOrDefault() == (string)p.Attributes("id").FirstOrDefault())
                            .Descendants("bookcontent")
                            .Count() == 0)
                            ? null
                            : (result
                                .Elements("segment")
                                .Where(s => (string)s.Attributes("id").FirstOrDefault() == (string)p.Attributes("id").FirstOrDefault())
                                .Descendants("bookcontent")
                                .Select(s=>s.GetBookcontentItems()))
                            )
                        )
                    )
                );
            }

            
            //XElement sitem = new XElement("items",
            //    result.Descendants("segment")
            //    .Where(p => (string)p.Attributes("id").FirstOrDefault() == "_top")
            //    .Descendants("navpoint")
            //    .Select(p => new XElement("item",
            //        p.Attributes("id"),
            //        p.Attributes("title"),
            //        new XAttribute("segment", "true")
            //        )
            //    )
            //);
            result.Descendants("img").Attributes("src").ToList().ForEach(p => p.SetValue(p.Value.Split('/').LastOrDefault()));

            
            

            XElement search = new XElement("items",
                sitem.Descendants("item").Where(p => ((string)p.Attributes("search").FirstOrDefault() ?? "") != "")
                .Select(p => new XElement("search",
                    new XAttribute("parent_id", (string)p.Ancestors().Where(s => (string)s.Attributes("segment").FirstOrDefault() == "true").Attributes("id").LastOrDefault()),
                    new XAttribute("object_id", (string)p.Attributes("id").FirstOrDefault()),
                    p.Attributes("text_type"),
                    (string)p.Attributes("search").FirstOrDefault()
                    ))
                );

            sitem.DescendantsAndSelf().Attributes("search").Remove();
            sitem.Save(outPath + "sitems.xml");
            
            search.Save(outPath + "search.xml");
            
            result.Save(outPath + "segments.xml");

        
        }
    }
}
