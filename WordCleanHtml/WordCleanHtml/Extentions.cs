using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace WordCleanHtml
{
    public static class Extentions
    {
        public static XElement StripNumber(this XElement e, Regex rxNumber)
        {
            XText first = e.DescendantNodes().OfType<XText>().FirstOrDefault();
            string test = first.Value.Trim();
            test = rxNumber.Replace(test, "");
            first.ReplaceWith(new XText(test));
            return new XElement(e.Name.LocalName, e.Nodes());
        }
        public static string ListValue(this string value, string listType)
        {
            string returnValue = "";
            if (Regex.IsMatch(value.Trim(), @"^[a-z]$") && listType == "a")
            {
                int alfa = (char)'a';
                int n = (value.ToCharArray().First() - alfa) + 1;
                returnValue = n.ToString();
            }
            else if (Regex.IsMatch(value.Trim(), @"^\d+$") && listType == "1")
            {
                returnValue = value;
            }
            else if (Regex.IsMatch(value.Trim(), @"^[ivx]+$") && listType == "i")
            {
                switch (value)
                {
                    case "i": returnValue = "1"; break;
                    case "ii": returnValue = "2"; break;
                    case "iii": returnValue = "3"; break;
                    case "iv": returnValue = "4"; break;
                    case "v": returnValue = "5"; break;
                    case "vi": returnValue = "6"; break;
                    case "vii": returnValue = "7"; break;
                    case "viii": returnValue = "8"; break;
                    case "ix": returnValue = "9"; break;
                    case "x": returnValue = "10"; break;
                    case "xi": returnValue = "11"; break;
                    case "xii": returnValue = "12"; break;
                    case "xiii": returnValue = "13"; break;
                    case "xiv": returnValue = "14"; break;
                    case "xv": returnValue = "15"; break;
                    case "xvi": returnValue = "16"; break;
                    case "xvii": returnValue = "17"; break;
                    case "xviii": returnValue = "18"; break;
                    case "xix": returnValue = "19"; break;
                    case "xx": returnValue = "20"; break;
                    case "xxi": returnValue = "21"; break;
                    case "xxii": returnValue = "22"; break;
                    case "xxiii": returnValue = "23"; break;
                    case "xxiv": returnValue = "24"; break;
                    case "xxv": returnValue = "25"; break;
                    case "xxvi": returnValue = "26"; break;
                    case "xxvii": returnValue = "27"; break;
                    case "xxviii": returnValue = "28"; break;
                    case "xxix": returnValue = "29"; break;
                    case "xxx": returnValue = "30"; break;
                    default: returnValue = "0"; break;
                }
            }
            else if (value.Trim() == "—")
            {
                returnValue = "-";
            }
            else if (value.Trim() == "")
                returnValue = value.Trim();
            else
                returnValue = value.Trim();
            return returnValue;
        }
        public static XElement MakeListFromP(this List<XElement> l)
        {
            XElement returnObject = new XElement("container");
            XElement returnValue = null;
            string className = "";
            string value = "";
            XElement elValue = null;
            while (returnValue == null)
            {
                XElement first = l.FirstOrDefault();
                if (first == null)
                {
                    if (returnObject.HasElements)
                        return returnObject;
                    else
                        return null;
                }
                    
                className = (string)first.Attributes("class").FirstOrDefault();
                elValue = null;
                value = "";
                if (first.DescendantNodes().OfType<XElement>().Where(p => p.Name.LocalName == "span" && p.Nodes().OfType<XText>().Count() > 0).FirstOrDefault() == first.DescendantNodes().FirstOrDefault())
                {
                    elValue = first.Elements("span").FirstOrDefault();
                    value = Regex.Replace(elValue.Value, @"[\.\)]", "").Trim();
                }
                else if (first.DescendantNodes().OfType<XElement>().Where(p => p.Name.LocalName == "b" && p.Nodes().OfType<XText>().Count()>0).FirstOrDefault() == first.DescendantNodes().FirstOrDefault())
                {
                    elValue = first.Elements("b").FirstOrDefault();
                    value = Regex.Replace(elValue.Value, @"[\.\)]", "").Trim();
                }
                else if (first.DescendantNodes().OfType<XElement>().Where(p => p.Name.LocalName == "s" && p.Nodes().OfType<XText>().Count() > 0).FirstOrDefault() == first.DescendantNodes().FirstOrDefault())
                {
                    elValue = first.Elements("s").FirstOrDefault();
                    value = Regex.Replace(elValue.Value, @"[\.\)]", "").Trim();
                }
                else
                {
                    elValue = first.DescendantNodesAndSelf().OfType<XElement>().Where(p => p.Nodes().OfType<XText>().Count() > 0).FirstOrDefault();
                    value = Regex.Replace(Regex.Split(elValue.Value,@" ").FirstOrDefault(), @"[\.\)]", "").Trim();
                }

                returnValue = null;

                if (Regex.IsMatch(value.Trim(), @"^[a-h]$"))
                {
                    value = "a";
                    returnValue = new XElement("ol", new XAttribute("type", "a"));
                }
                else if (Regex.IsMatch(value, @"^\d+$"))
                {
                    returnValue = new XElement("ol", new XAttribute("type", "1"));
                }
                else if (Regex.IsMatch(value, @"^[ivx]+$"))
                {
                    returnValue = new XElement("ol", new XAttribute("type", "i"));
                }
                else if ("-;—;Kontoklasse".Split(';').Contains(value.Trim()))
                {
                    returnValue = new XElement("ul");
                }
                else
                {
                    returnObject.Add(first);
                    l = l.Skip(1).ToList();
                }
            }
            List<XElement> l1 = l.Where(p => (string)p.Attributes("class").FirstOrDefault() == className).ToList();

            string listType = value;
            XElement last = null;
            foreach (XElement e in l1)
            {
                XElement t = new XElement(e);
                className = (string)t.Attributes("class").FirstOrDefault();


                if (t.DescendantNodes().OfType<XElement>().Where(p => p.Name.LocalName == "span" && p.Nodes().OfType<XText>().Count() > 0).FirstOrDefault() == t.DescendantNodes().FirstOrDefault())
                {
                    elValue = t.Elements("span").FirstOrDefault();
                    t.Elements("span").FirstOrDefault().Remove();
                    value = Regex.Replace(elValue.Value, @"[\.\)]", "").Trim();
                    value = value.ListValue(listType);
                }
                else if (t.DescendantNodes().OfType<XElement>().Where(p => p.Name.LocalName == "b" && p.Nodes().OfType<XText>().Count()>0).FirstOrDefault() == t.DescendantNodes().FirstOrDefault())
                {
                    elValue = t.Elements("b").FirstOrDefault();
                    t.Elements("b").FirstOrDefault().Remove();
                    value = Regex.Replace(elValue.Value, @"[\.\)]", "").Trim();
                    value = value.ListValue(listType);
                }
                else if (t.DescendantNodes().OfType<XElement>().Where(p => p.Name.LocalName == "s" && p.Nodes().OfType<XText>().Count() > 0).FirstOrDefault() == t.DescendantNodes().FirstOrDefault())
                {
                    elValue = t.Elements("s").FirstOrDefault();
                    t.Elements("s").FirstOrDefault().Remove();
                    value = Regex.Replace(elValue.Value, @"[\.\)]", "").Trim();
                    value = value.ListValue(listType);

                }
                else
                {
                    elValue = t.DescendantNodesAndSelf().OfType<XElement>().Where(p => p.Nodes().OfType<XText>().Count() > 0).FirstOrDefault();
                    XText tx = t.DescendantNodesAndSelf().OfType<XText>().FirstOrDefault();
                    value = Regex.Split(tx.Value, @" ").FirstOrDefault();
                    if (value != "Kontoklasse")
                    {
                        tx.ReplaceWith(new XText(Regex.Replace(tx.Value.TrimStart(), @"^" + value, "")));
                        value = Regex.Replace(value, @"[\.\)]", "").Trim();
                        value = value.ListValue(listType);
                    }
                }



                XElement li = null;
                if (value.Trim() == "")
                {
                    List<XElement> n = e.NodesAfterSelf().OfType<XElement>().TakeWhile(p => !l1.Contains(p) && l.Contains(p)).ToList();
                    last.Add(e);
                }
                else if (("-;—;Kontoklasse".Split(';').Contains(value.Trim())))
                {
                    li = new XElement("li", t.Nodes());
                    last = li;
                    List<XElement> n = e.NodesAfterSelf().OfType<XElement>().TakeWhile(p => !l1.Contains(p) && l.Contains(p)).ToList();
                    li.Add(n.MakeListFromP());
                    returnValue.Add(li);
                }
                else if (listType == "1" || listType == "a" || listType == "i")
                {
                    value = value.ListValue(listType);
                    li = new XElement("li", new XAttribute("value", value), t.Nodes());
                    last = li;
                    List<XElement> n = e.NodesAfterSelf().OfType<XElement>().TakeWhile(p => !l1.Contains(p) && l.Contains(p)).ToList();
                    li.Add(n.MakeListFromP());
                    returnValue.Add(li);
                }
                else
                {
                    value = value.ListValue("");
                    li = new XElement("li", new XText(value), new XElement("br"), t.Nodes());
                }


            }
            returnObject.Add(returnValue);
            return returnObject;



        }
        public static IEnumerable<XElement> GetChildren(this XElement document, List<Header> total, List<Header> headers, int n)
        {
            if (headers.Count() == 0)
            { 
                return new List<XElement>();
            }
            return headers.Where(p => p.n == n)
                .Select(p => new XElement("section",
                    p.e,
                    document.Elements().SkipWhile(d => d != p.e).Skip(1).TakeWhile(d => !total.Select(h => h.e).Contains(d)),
                    document.GetChildren(total, total.SkipWhile(h=>h.e != p.e).Skip(1).TakeWhile(h=>h.n>n).ToList(),n+1)
                )
            );
        }
        public static string GetDescendantText(this XElement e)
        {
            return e.DescendantNodes().OfType<XText>().Select(p => p.Value).StringConcatenate().Trim();
        }
        public static XElement ReloadXElement(this XElement e)
        {

            Stream stream = new MemoryStream();
            e.Save(stream);
            // Rewind the stream ready to read from it elsewhere
            stream.Position = 0;
            return XElement.Load(stream);
        }
        public static string GetAttriVal(this XElement e, string name)
        {
            string result = ((string)e.Attributes(name).FirstOrDefault() ?? "").Trim().ToLower();
            return result;
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
    }
}
