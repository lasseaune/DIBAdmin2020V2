using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using static DIBAdminAPI.Models.Result;
using System.IO.Compression;
using System.Collections;
using DIBAdminAPI.Data.Entities;

namespace DIBAdminAPI.Helpers.Extentions
{
    public class ElementEnumerator
    {
        public int idx { get; set; }
        public ElementEnumerator(XElement e, int n)
        {
            idx = n;
        }

    }
    public class DGVariableEval
    {
        private string name { get; set; }
        private string id { get; set; }
        private DateTime changedate { get; set; }
        public bool collection { get; set; }
        private Regex GetN = new Regex(@"\*N\*", RegexOptions.IgnoreCase);
        private Regex NumberFromName { get; set; }
        private Regex NumberFromId { get; set; }
        public DGVariableEval(string Name, string Id, DateTime ChangeDate)
        {
            name = Name;
            id = Id;
            changedate = ChangeDate;
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
            //string result = NumberFromName.Match(id).Groups["nr"].Value;
            string result = NumberFromId.Match(id).Groups["nr"].Value;
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
        public string bm { get; set; }
        public string type { get; set; }
        public string authorized { get; set; }
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
            authorized = ((string)topic.Attributes("authorized").FirstOrDefault() ?? "") == "" ? "1" : (string)topic.Attributes("authorized").FirstOrDefault();
        }
        public TopicInfo(XElement topic, string segmentId, string Bm, string Type)
        {
            //default
            if (segmentId != "") { }
            id = (string)topic.Attributes("topic_id").FirstOrDefault();
            topictype = (string)topic.Attributes("tid").FirstOrDefault();
            title = (((string)topic.Attributes("short").FirstOrDefault() ?? "") + " " + ((string)topic.Attributes("name").FirstOrDefault() ?? "")).Trim();
            segment_id = segmentId == "" ? ((string)topic.Attributes("segment_id").FirstOrDefault() ?? "") : segmentId;
            paragraph_id = (string)topic.Attributes("paragraf_id").FirstOrDefault();
            view = (string)topic.Attributes("view").FirstOrDefault();
            language = (string)topic.Attributes("lang").FirstOrDefault();
            bm = Bm == null ? null : (Bm.Trim() == "" ? null : Bm.Trim());
            type = Type == null ? null : (Type.Trim() == "" ? null : Type.Trim().ToLower());
            authorized = ((string)topic.Attributes("authorized").FirstOrDefault() ?? "") == "" ? "1" : (string)topic.Attributes("authorized").FirstOrDefault();
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
    public static class CompressionHelper
    {
        public static byte[] Compress(byte[] data)
        {
            byte[] compressArray = null;
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                    {
                        deflateStream.Write(data, 0, data.Length);
                    }
                    compressArray = memoryStream.ToArray();
                }
            }
            catch (Exception exception)
            {
                // do something !
            }
            return compressArray;
        }

        public static byte[] Decompress(byte[] data)
        {
            byte[] decompressedArray = null;
            try
            {
                using (MemoryStream decompressedStream = new MemoryStream())
                {
                    using (MemoryStream compressStream = new MemoryStream(data))
                    {
                        using (DeflateStream deflateStream = new DeflateStream(compressStream, CompressionMode.Decompress))
                        {
                            deflateStream.CopyTo(decompressedStream);
                        }
                    }
                    decompressedArray = decompressedStream.ToArray();
                }
            }
            catch (Exception exception)
            {
                // do something !
            }

            return decompressedArray;
        }
    }
    public static class Extentions
    {
        public static int StringIsMatchValue(this string text, string search)
        {
            int rank = 0;
            foreach (string s in search.Split(' ').Where(p => p != ""))
            {
                int hit = text.ToLower().IndexOf(s.ToLower());
                if (hit < 0)
                {
                    return hit;
                }
                else
                {
                    rank = rank + hit;
                }
            }
            return rank;
        }
        public static bool StringIsMatch(this string text, string search )
        {
            foreach (string s in search.Split(' ').Where(p=>p!=""))
            {
                
                if (text.IndexOf(s.ToLower())<0)
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsGuid(this string value)
        {
            Guid x;
            return Guid.TryParse(value, out x);
        }
        public static XElement CreateXMLDocument(this int resourcetypeId)
        {
            XElement document = null;
            string id = null;
            switch (resourcetypeId)
            {
                case 20:
                    id = Guid.NewGuid().ToString();

                    document =
                        new XElement("document",
                            new ElementConstructor().CreateCheckItem()
                        );
                    return document;
                case 19:
                    {
                        id = Guid.NewGuid().ToString();

                        document =
                            new XElement("document",
                                new XElement("section",
                                    new XAttribute("id", id),
                                    new XElement("h1",
                                        new XAttribute("id", Guid.NewGuid().ToString()),
                                        new XText("Overskrift 1")
                                    ),
                                    new XElement("p",
                                        new XAttribute("id", Guid.NewGuid().ToString()),
                                        new XText("Første setning....")
                                    )
                                )
                            );
                        return document;
                    }
                default:
                    return null;
            }

        }
        //GetElementsPlacement
        //Funksjonen henter ut alle plasseringsdata for elementer i document 
        public static XElement GetElementsPlacement(this XElement document, int segmentIndex)
        {
            int n = 0;
            document.Descendants().ToList().ForEach(p => p.AddAnnotation(new ElementEnumerator(p, n++)));

            return new XElement("items",
                    new XAttribute("segmentOrder", segmentIndex.ToString()),
                    document
                    .Descendants()
                    .Attributes("id")
                    .Select(p => new XElement("item",
                                p.GetElementPlacement()
                            )
                    )
                );
        }
        //GetElementPlacement
        //Funksjonen henter ut alle plasseringsdata for element
        public static IEnumerable<XAttribute> GetElementPlacement(this XAttribute id)
        {
            List<XAttribute> result = new List<XAttribute>();
            result.Add(new XAttribute("Id", id.Value));
            string parentId = (string)id.Parent
                        .Ancestors("section")
                        .Where(a =>
                            a.Elements()
                            .Where(n => Regex.IsMatch(n.Name.LocalName, @"^h\d")).Count() > 0
                        ).Attributes("id").LastOrDefault();
            if (parentId != null)
            {
                result.Add(new XAttribute("parentId", parentId));
            }
            string title = id
                        .Parent
                        .Elements()
                        .Where(n => Regex.IsMatch(n.Name.LocalName, @"^h\d"))
                        .Select(n => n
                                .DescendantNodes()
                                .OfType<XText>()
                                .Where(s => s.Ancestors("sup").Count() == 0)
                                .Select(s => s.Value)
                                .StringConcatenate()
                        ).FirstOrDefault();
            if (title != null)
            {
                result.Add(new XAttribute("title", title));
            }

            result.Add(new XAttribute("idx", id.Parent.Annotations<ElementEnumerator>().Select(p => p.idx).FirstOrDefault().ToString()));
            string tag1 = (string)id.Parent.AncestorsAndSelf().Attributes("tag1").FirstOrDefault();
            if (tag1 != null)
                result.Add(new XAttribute("tag1", tag1));
            string tag2 = (string)id.Parent.AncestorsAndSelf().Attributes("tag2").FirstOrDefault();
            if (tag2 != null)
                result.Add(new XAttribute("tag2", tag2));
            string tag3 = (string)id.Parent.AncestorsAndSelf().Attributes("tag3").FirstOrDefault();
            if (tag3 != null)
                result.Add(new XAttribute("tag3", tag3));
            return result;

        }
        public static int? SetValueToNull(this bool truefalse, int value)
        {
            if (truefalse)
                return null;
            else
                return value;
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
        public static void RenameAttribute(this XElement e, string name, string newName)
        {
            if (e.Attributes(newName).FirstOrDefault() == null)
            {
                e.Add(new XAttribute(newName, e.Attribute(name).Value.ToLower()));
                e.Attribute(name).Remove();
            }
        }
        public static int IsNumeric(this string test)
        {
            if (Regex.IsMatch(test, @"[0-9]+"))
            {
                return Convert.ToInt32(test);
            }
            return 1;
        }

       public static bool SearchText(this string text, Regex rx, int n)
       {
            List<int> j = new List<int>();
            rx.Replace(text.ToLower(),
                delegate(Match m)
                {
                    for (int i = 1; i<=n; i++)
                    {
                        if (m.Groups["i" + i.ToString()].Success)
                        {
                            if (!j.Contains(i))
                            {
                                j.Add(i);
                            }
                        }
                    }
                    return m.Value;
                }
            );
            if (j.Count() == n)
            {
                return true;
            }
            return false;
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
        public static void VariableUpdate(this XElement variable, XElement triggervariable, DateTime ProffDate)
        {
            string name = variable.Elements("name").Select(p => p.Value).FirstOrDefault();
            string id = variable.Elements("id").Select(p => p.Value).FirstOrDefault();
            DateTime sistOppdatert = DateTime.MinValue;

            if (variable.Attributes("changedate").FirstOrDefault() != null)
            {
                if (!DateTime.TryParseExact(variable.Attributes("changedate").FirstOrDefault().Value, "o", CultureInfo.InvariantCulture, DateTimeStyles.None, out sistOppdatert))
                    sistOppdatert = DateTime.MinValue;
            }

            DGVariableEval dgv = new DGVariableEval(name, id, ProffDate);
            if (dgv.collection)
            {
                if (variable.Elements("variable").Count() == 0 && triggervariable.Elements("variable").Count() != 0)
                {
                    foreach (XElement v in triggervariable.Elements("variable"))
                    {
                        string vname = (string)v.Attributes("name").FirstOrDefault();

                        string nr = dgv.GetNumberInName(vname);
                        if (nr != null)
                        {
                            XElement newvar = new XElement("variable",
                                new XElement("id", dgv.SetNumberInId(nr)),
                                new XElement("value", v.Value)
                                );
                            variable.Add(newvar);
                        }
                    }
                }
                else if (variable.Elements("variable").Count() != 0 && triggervariable.Elements("variable").Count() != 0)
                {
                    if (ProffDate >= sistOppdatert)
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
                                    ex_vars.Where(p => dgv.GetNumberInId((p.Elements("id").Select(s => s.Value).FirstOrDefault() ?? "")) == nr).ToList().ForEach(
                                        p => ex_vars.Remove(p)
                                    );
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
            }
            else
            {
                XElement value = variable.Elements("value").FirstOrDefault();

                if (triggervariable.Value != "" || sistOppdatert < ProffDate)
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
            var varLastChanged = DateTime.MinValue;

            if (triggerdata != null && triggerdata.HasAttributes)
            {
                if (!DateTime.TryParseExact(triggerdata.Attribute("changedate").Value, "o", CultureInfo.InvariantCulture, DateTimeStyles.None, out varLastChanged))
                    varLastChanged = DateTime.MinValue;
            }

            (
                from v in variables.Elements("variable").Where(p => p.Elements("name").FirstOrDefault() != null)
                join tv in triggerdata.Elements("variable")
                on
                    (v.Elements("name").Select(p => p.Value).FirstOrDefault() ?? "-1").Trim().ToLower()
                    equals
                    ((string)tv.Attributes("name").FirstOrDefault() ?? "-0").Trim().ToLower()
                select new { var = v, trig = tv }
            ).ToList()
            .ForEach(p => p.var.VariableUpdate(p.trig, varLastChanged));
            return variables;
        }
        public static void AddLinkInfo(this XElement document, XElement xlinkgroup)
        {
            (
                from dl in document.Descendants("a").Where(s => (string)s.Attributes("class").FirstOrDefault() == "diblink")
                join rf in xlinkgroup.Descendants("ref")
                on ((string)dl.Attributes("href").FirstOrDefault() ?? "") equals "#" + ((string)rf.Attributes("id").FirstOrDefault() ?? "")
                join to in xlinkgroup.Descendants("topic")
                on (string)rf.Ancestors("x-link").Attributes("topic_id").FirstOrDefault() equals (string)to.Attributes("topic_id").FirstOrDefault()
                select new { link = dl, topicinfo = to, r = rf }
                )
                .Union(
                from dl in document.Descendants().Where(p => "diblink;dibparameter".Split(';').Contains(p.Name.LocalName))
                join rf in xlinkgroup.Descendants("ref")
                on (string)dl.Attributes().Where(p => "refid;rid".Split(';').Contains(p.Name.LocalName)).FirstOrDefault() equals (string)rf.Attributes("id").FirstOrDefault()
                join to in xlinkgroup.Descendants("topic")
                on (string)rf.Ancestors("x-link").Attributes("topic_id").FirstOrDefault() equals (string)to.Attributes("topic_id").FirstOrDefault()
                select new { link = dl, topicinfo = to, r = rf }
                ).ToList()
                .ForEach(p => p.link.AddAnnotation(
                        new TopicInfo(
                            p.topicinfo,
                            (string)p.r.Parent.Attributes("segment_id").FirstOrDefault() ?? "",
                            (string)p.r.Attributes("bm").FirstOrDefault() ?? "",
                            (string)p.r.Attributes("type").FirstOrDefault() ?? ""
                        )
                    )
                );
        }
        public static IEnumerable<XNode> Diblink(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            TopicInfo ti = e.Annotation<TopicInfo>();
            if (ti != null)
            {
                if ((ti.type == null ? "" : ti.type) == "link")
                {
                    if ((ti.bm == null ? "" : ti.bm) == "")
                    {
                        result.Add(new XElement("a",
                                new XAttribute("class", "diblink topic"),
                                //e.Attributes("href"),
                                new XAttribute("href", "#" + (string)e.Attributes("refid").FirstOrDefault()),
                                new XAttribute("data-topictitle", ti.title),
                                new XAttribute("data-topictype", ti.topictype),
                                new XAttribute("data-topicid", ti.id),
                                new XAttribute("data-view", ti.view),
                                new XAttribute("data-language", ti.language),
                                new XAttribute("data-authorized", (ti.authorized == null ? "1" : ti.authorized)),
                                ti.bm == null ? null : new XAttribute("data-id", ti.bm),
                                ((string)e.Attributes("replaceText").FirstOrDefault() ?? "") == "" ? e.Value : (string)e.Attributes("replaceText").FirstOrDefault()
                                )
                            );
                    }
                    else
                    {
                        result.Add(new XElement("a",
                            new XAttribute("data-authorized", (ti.authorized == null ? "1" : ti.authorized)),
                            new XAttribute("class", "diblink"),
                            new XAttribute("href", "#" + (string)e.Attributes("refid").FirstOrDefault()),
                            ((string)e.Attributes("replaceText").FirstOrDefault() ?? "") == "" ? e.Value : (string)e.Attributes("replaceText").FirstOrDefault()
                            )
                        );
                    }

                }
                else
                {
                    result.Add(new XElement("a",
                            new XAttribute("class", "diblink topic"),
                            new XAttribute("href", "#" + (string)e.Attributes("refid").FirstOrDefault()),
                            new XAttribute("data-topictitle", ti.title),
                            new XAttribute("data-topictype", ti.topictype),
                            new XAttribute("data-topicid", ti.id),
                            new XAttribute("data-view", ti.view),
                            new XAttribute("data-language", ti.language),
                            new XAttribute("data-authorized", (ti.authorized == null ? "1" : ti.authorized)),
                            (
                                (string)e.Attributes("replaceText").FirstOrDefault() ?? "") == ""
                                ? e.Nodes().OfType<XText>().Select(t => t.ToString()).StringConcatenate()
                                : e.Nodes().OfType<XText>().Select(t => t.ToString()).StringConcatenate()
                            )
                        );

                }

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
                        new XAttribute("data-authorized", "1"),
                        new XAttribute("class", "diblink"),
                        new XAttribute("href", "#" + (string)e.Attributes("refid").FirstOrDefault()),
                        new XText((string)e.Attributes("replaceText").FirstOrDefault())
                        )
                    );
                }
                else
                {
                    result.Add(new XElement("a",
                        new XAttribute("data-authorized", "1"),
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
        public static ResultSetContainer GetResultSet(this XElement referances)
        {
            XElement sort = referances.Elements("sort").FirstOrDefault();
            XElement set = new XElement("results",
                               new XAttribute("count", referances.Elements("result").Count().ToString()),
                               referances.Elements("result")
                               //.GroupBy(p=>0)
                               .GroupBy(p => ("2;3").Split(';').Contains((string)p.Attributes("cid").FirstOrDefault())
                                       ? 0 :
                                       (
                                           (string)p.Attributes("cid").FirstOrDefault() == "4"
                                           ? 1
                                           : 2)
                                       )
                               .Select(p => new XElement("set",
                               new XAttribute("column", p.Key),
                               //new XAttribute("column", "0"),
                               new XAttribute("offset", "0"),
                               new XAttribute("count", p.Select(s => s).Count()),
                                    p.SortResultSetByTopicTypeOrder(sort)
                                )
                           )
                       );

            return new ResultSetContainer(set);
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

        public static void RemoveRedundantSpaces(this XElement d)
        {
            XText n = d.DescendantNodesAndSelf().OfType<XText>().Where(
                p => (
                    p.Value.Contains("  ")
                    || p.Value.Contains(" ,")
                    || p.Value.Contains(" .")
                    || p.Value.Contains(" :")
                //|| p.Value.StartsWith(" ")
                )
            ).FirstOrDefault();

            while (n != null)
            {
                n.Value = n.Value  //.TrimStart()
                                 .Replace("  ", " ")
                                 .Replace(" ,", ", ")
                                 .Replace(" .", ". ")
                                 .Replace(" :", ": ");

                n = d.DescendantNodesAndSelf().OfType<XText>().Where(
                    p => (
                        p.Value.Contains("  ")
                        || p.Value.Contains(" ,")
                        || p.Value.Contains(" .")
                        || p.Value.Contains(" :")
                    //|| p.Value.StartsWith(" ")
                    )
                ).FirstOrDefault();
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

        public static IEnumerable Append(this IEnumerable first, params object[] second)
        {
            return first.OfType<object>().Concat(second);
        }
        public static IEnumerable<T> Append<T>(this IEnumerable<T> first, params T[] second)
        {
            return first.Concat(second);
        }
        public static IEnumerable Prepend(this IEnumerable first, params object[] second)
        {
            return second.Concat(first.OfType<object>());
        }
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> first, params T[] second)
        {
            return second.Concat(first);
        }
    }

}
