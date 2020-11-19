using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace WordCleanHtml
{
    public class Header
    {
        public int n { get; set; }
        public XElement e { get; set; }

        public Header(XElement element)
        {
            n = Convert.ToInt32(element.Name.LocalName.Replace("h", ""));
            e = element;

        }
    }
        
    class Program
    {
        static void Main(string[] args)
        {
            //string path = @"D:\_books\KOSTRA\2020\html\";
            //string filename = @"KOSTRA hovedveileder 2020.xml";
            string path = @"D:\_books\KOSTRA\2021\last\";
            string filename = @"kostra2021_all.xml";
            XElement e = XElement.Load(path + filename);

            e.DescendantNodes().OfType<XText>().ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"&nbsp\;", " "))));
            e.DescendantNodes().OfType<XText>().ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"&#8209\;", "-"))));
            e.DescendantNodes().OfType<XText>().ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"\s+", " "))));
            e.DescendantNodes().OfType<XText>().ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"\r\n", " "))));
            e.Descendants().OfType<XText>().ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"\s+", " "))));

            e.Descendants("img").Select(p => p.Ancestors("p").FirstOrDefault()).ToList().ForEach(p=>p.ReplaceWith(new XElement("figure", p.Descendants("img").FirstOrDefault())));
            e.Descendants("img").Attributes("src").ToList().ForEach(p => p.SetValue(p.Value.Split('/').LastOrDefault()));

            e.Descendants("p").Where(p => p.GetDescendantText().Trim() == "").ToList().ForEach(p => p.Remove());
            e.Descendants("p").Where(p => Regex.IsMatch(p.GetAttriVal("class").Trim().ToLower(), @"^msotoc\d+$")).ToList().ForEach(p => p.Remove());
            e.Descendants("span").Where(p => "td;th;li;p;b".Split(';').Contains(p.Parent.Name.LocalName) && p.Parent.Nodes().Count() == 1).ToList().ForEach(p => p.Nodes());
            e.Descendants("td").Where(p => p.GetDescendantText().Trim() == "").ToList().ForEach(p => p.ReplaceWith(new XElement("td")));
            e.Descendants("a").Where(p => ((string)p.Attributes("name").FirstOrDefault() ?? "").ToLower().StartsWith("_toc")).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
            e.Descendants("span").Where(p => ((string)p.Attributes("style").FirstOrDefault() ?? "").Trim().StartsWith("font:7.0pt")).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
            e.Descendants("span").Where(p => ((string)p.Attributes("class").FirstOrDefault() ?? "").Trim().StartsWith("halvfet")).ToList().ForEach(p => p.ReplaceWith(new XElement("strong", p.Nodes())));
            e.Descendants("p").Attributes("class").Where(p => p.Value == "MsoNormal").Remove();
            e.Descendants("p").Attributes("style").Where(p => p.Value == "margin-bottom:8.0pt;line-height:107%").Remove();
            e.Descendants("p").Where(p => (string)p.Attributes("class").FirstOrDefault() == "avsnitt-under-undertittel").ToList().ForEach(p => p.ReplaceWith(new XElement("p", new XElement("strong", p.Nodes()))));
            e.Descendants("span").Where(p => p.Attributes("style").Where(a => a.Value == "font-family:Symbol").Count() > 0).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
            e.Descendants("span").Where(p => p.Attributes("style").Where(a => a.Value == "font-size:11.0pt;mso-bidi-font-size:10.0pt;line-height:115%;mso-no-proof: yes").Count() > 0).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
            e.Descendants("span").Where(p => p.Attributes("style").Where(a => a.Value == "font-family:Symbol;mso-fareast-font-family:Symbol;mso-bidi-font-family: Symbol;mso-no-proof:yes").Count() > 0).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
            
            
            e.Descendants("span").Attributes("style").ToList().ForEach(p => p.SetValue(Regex.Replace(p.Value, @"\s+", "")));
            e.Descendants().Attributes("style").Where(p => p.Value.Trim().ToLower().StartsWith("mso-list:l19")).ToList().ForEach(p => p.Remove());
            e.Descendants("tr").Attributes("style").ToList().ForEach(p => p.Remove());
            e.Descendants("span")
                .Where(a => a.Attributes("style")
                    .Where(s => s.Value.Split(';').Where(t => "font-size:8.0pt;line-height:115%".Split(';').Contains(t)).Count()== 2)
                    .FirstOrDefault() != null
                )
                .ToList()
                .ForEach(p => p.ReplaceWith(p.Nodes()));

            e.Descendants("span")
                .Where(a => a.Attributes("style")
                    .Where(s => s.Value.Split(';').Where(t => "font-size:8.0pt;line-height:150%".Split(';').Contains(t)).Count() == 2)
                    .FirstOrDefault() != null
                )
                .ToList()
                .ForEach(p => p.ReplaceWith(p.Nodes()));

            e.Descendants("span")
                .Where(a => a.Attributes("style")
                    .Where(s => s.Value.Split(';').Where(t => "font-size:10.0pt;line-height:115%".Split(';').Contains(t)).Count() == 2)
                    .FirstOrDefault() != null
                )
                .ToList()
                .ForEach(p => p.ReplaceWith(p.Nodes()));
            e.Descendants("span")
                .Where(a => a.Attributes("style")
                    .Where(s => s.Value.Split(';').Where(t => "font-size:8.0pt;mso-no-proof:yes".Split(';').Contains(t)).Count() == 2)
                    .FirstOrDefault() != null
                )
                .ToList()
                .ForEach(p => p.ReplaceWith(p.Nodes()));
            e.Descendants("span")
                .Where(a => a.Attributes("style")
                    .Where(s => s.Value.Split(';').Where(t => "font-size:14.0pt;line-height:115%".Split(';').Contains(t)).Count() == 2)
                    .FirstOrDefault() != null
                )
                .ToList()
                .ForEach(p => p.ReplaceWith(p.Nodes()));
            e.Descendants("span")
                .Where(a => a.Attributes("style")
                    .Where(s => s.Value.Split(';').Where(t => "font-family:\"Times\",serif;mso-bidi-font-family:\"TimesNewRoman\"".Split(';').Contains(t)).Count() == 2)
                    .FirstOrDefault() != null
                )
                .ToList()
                .ForEach(p => p.ReplaceWith(p.Nodes()));
            

            e.Descendants("a").Where(p => ((string)p.Attributes("name").FirstOrDefault() ?? "").Trim().ToLower().StartsWith("_hlk")).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));

            e.Descendants("p").Attributes("style").Where(s => s.Value.Split(';').Where(t => @"line-height:150%;mso-layout-grid-align:none".Split(';').Contains(t)).Count() == 2).ToList().ForEach(p => p.Remove());
            
            e.Descendants("span").Where(p => p.Attributes("class").Where(a => a.Value == "kursiv").Count() > 0).ToList().ForEach(p => p.ReplaceWith(new XElement("em", p.Nodes())));
            

            e.Descendants("span").Where(p => ((string)p.Attributes("style").FirstOrDefault() ?? "").Trim().ToLower().StartsWith("mso-")).Reverse().ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
            e.Descendants().Where(p => p.Name.LocalName.StartsWith("_")).ToList().ForEach(p => p.Remove());

            e.Descendants("td").Where(p => p.DescendantNodes().OfType<XText>().Count() == 1).ToList()
                .ForEach(p => p.ReplaceWith(new XElement(p.Name.LocalName, p.DescendantNodes().OfType<XText>().FirstOrDefault())));
            e.Descendants("span").Where(p => p.DescendantNodes().OfType<XText>().Select(t => t.Value).StringConcatenate() == "").ToList().ForEach(p => p.Remove());


            e.Descendants("strong").Where(p => p.DescendantNodes().OfType<XText>().Count() == 0 && p.Nodes().OfType<XElement>().Where(s => s.Name.LocalName == "br").Count() == 1).ToList().ForEach(p => p.Remove());
            e.Descendants("br").Where(p => p.Parent.Name.LocalName == "strong").Select(p => p.Parent).ToList().ForEach(p => p.Remove());
            e.Descendants("br").Where(p => p.Parent.Name.LocalName == "b").Select(p => p.Parent).ToList().ForEach(p => p.Remove());
            e.Descendants("br").ToList().ForEach(p => p.Remove());
            e.Descendants("span").Where(p => p.Attributes("style").Where(a => a.Value.Trim().ToLower() == "font-size:11.0pt;mso-bidi-font-size:10.0pt;line-height:115%;mso-no-proof:yes".Trim().ToLower()).Count() > 0).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
            e.Descendants("span").Where(p => p.Attributes("style").Where(a => a.Value.Trim().ToLower() == "font-family:Symbol;mso-fareast-font-family:Symbol;mso-bidi-font-family:Symbol;mso-no-proof:yes".Trim().ToLower()).Count() > 0).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
            e.Descendants("span").Attributes("style").Where(p => p.Value.Trim() == "color:red;mso-no-proof:yes").ToList().ForEach(p => p.SetValue("color:red"));


             e.Descendants("a").Where(p => ((string)p.Attributes("style").FirstOrDefault() ?? "").Trim().ToLower().Split(':').FirstOrDefault() == "mso-footnote-id")
                .ToList()
                .ForEach(p => p.ReplaceWith(new XElement("a",
                    new XAttribute("class", "footnote"),
                    new XAttribute("data-id", ((string)p.Attributes("style").FirstOrDefault() ?? "").Trim().ToLower().Split(':').LastOrDefault()),
                    new XText(p.DescendantNodes().OfType<XText>().Select(s=>s.Value).StringConcatenate())
                    )
                ));

            e.Save(path + "temp.xml");
            XElement temp = e.Elements("div").Where(p => (string)p.Attributes("id").FirstOrDefault() == "WordSection1").FirstOrDefault();
            XElement bookmarks = e.Elements("div").Where(p => (string)p.Attributes("style").FirstOrDefault() == "mso-element:footnote-list").FirstOrDefault();

            
            List <Header> header = temp.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"^h\d$")).Select(p => new Header(p)).ToList();
            XElement document = new XElement("document",
                temp.GetChildren(header, header, 1)
            );

            Regex numb = new Regex(@"^(\s+)?(?<numb>(\d+((\.\d+)+)?))(?<rest>([A-ZÆØÅ].+))");
            document = XElement.Parse(document.ToString());

            document.Descendants("span").Where(p => p.Nodes().OfType<XElement>().Count() == 0 && p.DescendantNodesAndSelf().OfType<XText>().Select(s => s.Value).StringConcatenate() == "").ToList().ForEach(p => p.Remove());
            
            document.Descendants()
                .Where(p =>
                    Regex.IsMatch(p.Name.LocalName, @"^h\d$")
                    && numb.IsMatch(p.DescendantNodesAndSelf().OfType<XText>().Select(s => s.Value).FirstOrDefault()??"")
                )
                .Select(p => p.DescendantNodesAndSelf().OfType<XText>().FirstOrDefault())
                .ToList()
                .ForEach(p => p.ReplaceWith(new XText(numb.Match(p.Value.TrimStart()).Groups["numb"].Value + " " + numb.Match(p.Value.TrimStart()).Groups["rest"].Value)));


            List<XElement> friliste = document.Descendants("p")
                .Where(p =>
                     "friliste;msolist".Split(';').Contains(((string)p.Attributes("class").FirstOrDefault() ?? "").Trim().ToLower())
                    && Regex.IsMatch(p.DescendantNodes().OfType<XText>().Select(t => t.Value).StringConcatenate().Trim(), @"^(Funksjon\s+)?\d\d\d((\s)?\-(\s)?\d\d\d)?")
                )
                .ToList();
            foreach (XElement f in friliste)
            {
                string title = f.DescendantNodes().OfType<XText>().Select(t => t.Value).StringConcatenate();
                Regex r = new Regex(@"^(\s+)?(?<pre>((Funksjon(\s+)?)?))(?<code>(\d\d\d))((\s)?\-(\s)?(?<tocode>(\d\d\d)))?(?<rest>(.+))");
                Match m = r.Match(title);
                string pre = m.Groups["pre"].Value;
                string code = m.Groups["code"].Value;
                string tocode = m.Groups["tocode"].Value;
                string rest = m.Groups["rest"].Value.Trim();

                title = (pre.Trim()=="" ? "" : pre.Trim() + " ") + code + (tocode == "" ? " " : "-" + tocode + " ") + rest ;
                List<XNode> n = f.NodesAfterSelf().TakeWhile(p => !friliste.Contains(p)).ToList();
        
                XText ft = f.DescendantNodes().OfType<XText>().FirstOrDefault();
                if (ft.Value.Length > m.Groups["rest"].Index)
                {
                    string s1 = ft.Value.Substring(0, m.Groups["rest"].Index).Trim();
                    string s2 = ft.Value.Substring(m.Groups["rest"].Index).TrimStart();
                    ft.ReplaceWith(new XText(s1 + " " + s2));
                }
                else
                {
                    if (code!="" && rest!="")
                    {
                        XText nx = f.DescendantNodes().OfType<XText>().SkipWhile(p => p != ft).Skip(1).Take(1).FirstOrDefault();
                        if (!nx.Value.StartsWith(" "))
                            nx.ReplaceWith(new XText(" " + nx.Value));
                    }
                }
                
                int h = Convert.ToInt32(f.NodesBeforeSelf().OfType<XElement>().Where(p => Regex.IsMatch(p.Name.LocalName, @"^h\d$")).Select(p => p.Name.LocalName.Replace("h", "")).FirstOrDefault());
                
                
                n.ForEach(p => p.Remove());
                XElement fl = new XElement("section",
                    new XElement("h" + (h + 1).ToString(), f.Nodes()),
                    n
                );
                f.ReplaceWith(fl);

            }

            friliste = document.Descendants("p")
                .Where(p =>
                    ((string)p.Attributes("class").FirstOrDefault() ?? "").Trim().ToLower() == "friliste"
                    && Regex.IsMatch(p.DescendantNodes().OfType<XText>().Select(t => t.Value).StringConcatenate().Trim(), @"^\(\d\)")
                )
                .ToList();
            foreach (XElement f in friliste)
            {
                
                int h = Convert.ToInt32(f.NodesBeforeSelf().OfType<XElement>().Where(p => Regex.IsMatch(p.Name.LocalName, @"^h\d$")).Select(p => p.Name.LocalName.Replace("h", "")).FirstOrDefault());
                List<XNode> n = f.NodesAfterSelf().TakeWhile(p => !friliste.Contains(p)).ToList();

                n.ForEach(p => p.Remove());
                XElement fl = new XElement("section",
                    new XElement("h" + (h + 1).ToString(), f.Nodes()),
                    n
                );
                f.ReplaceWith(fl);

            }

            friliste = document.Descendants("p")
                .Where(p =>
                    ((string)p.Attributes("class").FirstOrDefault() ?? "").Trim().ToLower() == "msolistnumber"
                    && ((string)p.Attributes("style").FirstOrDefault() ?? "").Split(';').Where(s => s.Trim()== "mso-list:none").Count()>0
                    && Regex.IsMatch(p.DescendantNodes().OfType<XText>().Select(t => t.Value).StringConcatenate().Trim(), @"^\(\d\)")
                )
                .ToList();
            foreach (XElement f in friliste)
            {
                
                f.Descendants("span").ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
                int h = Convert.ToInt32(f.NodesBeforeSelf().OfType<XElement>().Where(p => Regex.IsMatch(p.Name.LocalName, @"^h\d$")).Select(p => p.Name.LocalName.Replace("h", "")).FirstOrDefault());
                List<XNode> n = f.NodesAfterSelf().TakeWhile(p => !friliste.Contains(p)).ToList();

                n.ForEach(p => p.Remove());
                XElement fl = new XElement("section",
                    new XElement("h" + (h + 1).ToString(), f.Nodes()),
                    n
                );
                f.ReplaceWith(fl);

            }

            List<string> listName = document
                .Descendants("p")
                .Where(p => ((string)p.Attributes("class").FirstOrDefault() ?? "").Trim().ToLower().Contains("list"))
                .Select(p => 
                    ((string)p.Attributes("class").FirstOrDefault()??"")
                    + @"/" 
                    + (((string)p.Attributes("style").FirstOrDefault() ?? "").Split(' ').Where(s => s.Trim().StartsWith("level")).Select(l =>l).FirstOrDefault()??"")
                )
                .GroupBy(p => p)
                .OrderBy(p => p.Key)
                .Select(p => p.Key)
                .ToList();
            foreach (string s in listName)
            {
                Debug.Print(s);
            }

            XElement first = document
                .Descendants("p")
                .Where(p => ((string)p.Attributes("class").FirstOrDefault() ?? "").Trim().ToLower().Contains("list"))
                .FirstOrDefault();
            while (first != null)
            {
                List<XElement> n = new List<XElement>();
                n.Add(first);
                //string name = (string)first.Attributes("class").FirstOrDefault();
                //string level = ((string)first.Attributes("style").FirstOrDefault() ?? "").Split(' ').Where(s => s.Trim().StartsWith("level")).Select(l => l).FirstOrDefault();
                //XElement list = null;
                //if (name.StartsWith("alfaliste"))
                //{
                //    list = new XElement("ol", new XAttribute("type", "a"));

                //}

                n.AddRange(first.NodesAfterSelf().OfType<XElement>().TakeWhile(p => ((string)p.Attributes("class").FirstOrDefault() ?? "").Trim().ToLower().Contains("list")));
                
                n.ForEach(p=>Debug.Print(p.ToString()));
                XElement container = n.MakeListFromP();
                if (container == null)
                {
                    n.ForEach(p => p.ReplaceWith(new XElement("p", n.Nodes())));
                }
                else
                {
                    container.Descendants("container").ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
                    container.Descendants("p").ToList().ForEach(p => p.ReplaceWith(new XElement("p", p.Nodes())));
                    n.Skip(1).ToList().ForEach(p => p.Remove());
                    first.ReplaceWith(container.Nodes());
                }
                    
                
                first = document
                .Descendants("p")
                .Where(p => ((string)p.Attributes("class").FirstOrDefault() ?? "").Trim().ToLower().Contains("list"))
                .FirstOrDefault();
            }
            //while 

            List<XElement> fn = document.Descendants("a").Where(p => (string)p.Attributes("class").FirstOrDefault() == "footnote").ToList();
            foreach (XElement f in fn )
            {
                string id = ((string)f.Attributes("data-id").FirstOrDefault()??"").Trim();
                XElement pa = f.Ancestors().Where(p => p.Parent.Name.LocalName == "section").FirstOrDefault();
                XElement div = bookmarks.Elements("div").Where(p => ((string)p.Attributes("id").FirstOrDefault()??"").Trim().ToLower() == id.ToLower()).FirstOrDefault();
                XElement a = div.Descendants("a").FirstOrDefault();
                a.Remove();
                div.Attributes("style").Remove();
                div.Descendants("p").ToList().ForEach(p => p.ReplaceWith(new XElement("p", p.Nodes())));
                XElement tab = new XElement("table", new XAttribute("class", "tbNoBorder"), new XElement("tr", new XElement("td", new XElement("sup", a.Nodes()), new XElement("td", div))));
                if (pa.NextNode != null ? (pa.NextNode.NodeType==XmlNodeType.Element ? (string)((XElement)pa.NextNode).Attributes("class").FirstOrDefault()=="tbNoBorder" : false) : false)
                {
                    ((XElement)pa.NextNode).Add(tab.Elements("tr"));
                }
                else
                    pa.AddAfterSelf(tab);

                f.ReplaceWith(new XElement("sup", f.Nodes()));

            }

            document.Descendants("a").Where(p => ((string)p.Attributes("href").FirstOrDefault() ?? "").Trim().ToLower().StartsWith("https://lovdata.no")).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));

            document.Save(path + "document_all.xml");
            
            e.Save(path + "hv2021.xml");


           
        }
    }
}
