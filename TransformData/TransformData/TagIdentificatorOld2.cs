using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;


namespace Dib.Text.IdentifierOld2
{
    public static class Extentions
    {
        public static void ReplaceXtext(this XText t, List<Markup> marks)
        {
            XTextRange xr = t.Annotation<XTextRange>();
            XNode rangeNode = t;
            XElement range = new XElement("range");
            string rangeText = t.ToString();
            int rangeStart = xr.pos;
            int rangeCursor = xr.pos;
            int rangeLength = xr.length;
            string linkText = "";

            foreach (Markup l in marks.OrderBy(p => p.start))
            {

                int linkStart = l.start;
                int linkLength = l.length;

                //Linkens start er foran starten på teksten
                if (linkStart < rangeCursor)
                {
                    //sett Cursor til start
                    linkStart = rangeCursor;
                    linkLength = linkLength - (rangeCursor - linkStart);
                }
                //Hvis linkens star er større enn Cursor legg til text foran link
                if (linkStart > rangeCursor)
                {
                    string s = rangeText.Substring((rangeCursor - rangeStart), ((linkStart - rangeStart) - (rangeCursor - rangeStart)));
                    range.Add(new XText(s));
                    rangeCursor = linkStart;
                }
                //hvis lengden på linken er lengere enn lengden på rangens tekst forkort linken
                if (((rangeCursor - rangeStart) + linkLength) > rangeText.Length)
                {
                    linkLength = rangeText.Length - (rangeCursor - rangeStart);
                }

                linkText = rangeText.Substring((rangeCursor - rangeStart), linkLength);
                rangeCursor = rangeCursor + linkLength;

                if (t.Parent.Name.LocalName == "a"
                    && (t.Parent.Attribute("class") == null ? "" : t.Parent.Attribute("class").Value) == "xref"
                    && t.Parent.Value == linkText
                    )
                {
                    range.Add(new XText(linkText));
                }
                else
                {
                    range.Add(new XElement("diblink",
                            new XAttribute("refid", l.id),
                            xr.tag1 == null ? null : new XAttribute("tag1", xr.tag1),
                            xr.tag1 == null ? null : new XAttribute("regexpname", l.regexpName),
                            new XText(linkText)
                            ));

                }
            }
            if (range.HasElements)
            {
                if ((rangeCursor - rangeStart) < rangeText.Length)
                {
                    string clean = rangeText.Substring((rangeCursor - rangeStart), rangeText.Length - (rangeCursor - rangeStart));
                    range.Add(new XText(clean));

                }
                if (((XElement)rangeNode.Parent).Attributes(XNamespace.Xml + "space").Count() == 0)
                {
                    XAttribute a = new XAttribute(XNamespace.Xml + "space", "preserve");
                    ((XElement)rangeNode.Parent).Add(a);
                }
                rangeNode.ReplaceWith(range.Nodes());
            }

        }
        public static IEnumerable<XElement> GetLinkData(this IdLink markup, IdentyfiedLink identu)
        {
            List<XElement> result = new List<XElement>();
            return result;
        }
        public static string SetStringToNull(this string s)
        {
            if (s == null)
                return null;
            else if (s.Trim() == "")
                return null;
            else
                return s;
        }

        public static string SplitTag3(this string tag3, int n)
        {
            string returnValue = "";
            int count = tag3.Split('/').Count();
            for (int parts = 0; count - n > parts; parts++)
            {
                returnValue = (returnValue != "" ? "/" : "") + tag3.Split('/').ElementAt(parts);
            }
            return returnValue;
        }
        public static string GetIfrsSortValue(this string value)
        {
            Regex r = new Regex(@"(?<p1>(([a-y\.]+)?))(z)?(?<p2>(([\d\.a-z]+)?))");
            Match m = r.Match(value);
            return m.Groups["p1"].Value + m.Groups["p2"].Value;
        }
        public static List<XElement> GetElementsBetween(this List<XElement> l, string From, string To, int taglevel)
        {
            List<XElement> returnValue = null;
            From = From.ToLower();
            To = To.ToLower();
            //if (To == "h/i") Debug.Print("xx");
            //if ((Regex.IsMatch(From, @"bcz\d+") && Regex.IsMatch(To, @"bc\d+"))
            //    || (Regex.IsMatch(To, @"bcz\d+") && Regex.IsMatch(From, @"bc\d+")))
            //    Debug.Print("xx");
            //if (To == "app.e" && From == "app.a") Debug.Print("xx");


            if (From == "" || To == "")
            {
                return returnValue;
            }
            //if (l.Count() == 0) Debug.Print("xx");

            List<string> values = new List<string>();
            values.Add(From);
            values.Add(To);
            if (values.OrderBy(p => p, new NaturalSortComparer<string>()).First() == To)
            {
                string temp = From;
                From = To;
                To = temp;
            }

            bool take = false;

            returnValue = new List<XElement>();
            string sregexp = "^";
            IEnumerable<XElement> ll = null;

            if (taglevel == 2)
            {
                List<string> parts = From.Split('.').ToList();
                Regex rNumber = new Regex(@"(?<n1>([a-z]+))?(?<n2>(\d+))?(?<n3>([a-z]+))?$");
                if (parts.Count() == 1)
                {
                    Match m = rNumber.Match(From);
                    if (m.Groups["n1"].Success)
                    {
                        if (m.Groups["n1"].Value.ToLower().StartsWith("bc"))
                        {
                            sregexp = sregexp + "bc(z)?";
                        }
                        else
                        {
                            for (int j = 0; j < m.Groups["n1"].Value.Length; j++)
                                sregexp = sregexp + "[a-z]";

                        }
                    }
                    if (m.Groups["n2"].Success)
                    {
                        sregexp = sregexp + @"\d+";
                        sregexp = sregexp + @"([a-z]+)?";
                    }

                }
                else
                {
                    for (int i = 0; i < parts.Count(); i++)
                    {
                        if (i + 1 == parts.Count())
                        {
                            Match m = rNumber.Match(parts.ElementAt(i));
                            if (m.Groups["n1"].Success)
                            {
                                if (m.Groups["n1"].Value.ToLower().StartsWith("bc"))
                                {
                                    sregexp = sregexp + "bc(z)?";
                                }
                                else
                                {
                                    for (int j = 0; j < m.Groups["n1"].Value.Length; j++)
                                        sregexp = sregexp + "[a-z]";
                                }
                            }
                            if (m.Groups["n2"].Success)
                            {
                                sregexp = sregexp + @"\d+";
                                sregexp = sregexp + @"([a-z]+)?";
                            }
                        }
                        else
                        {
                            if (Regex.IsMatch(parts.ElementAt(i), @"^bc(z)?\d+$"))
                                sregexp = sregexp + @"bc(z)?\d+" + @"\.";
                            else if (Regex.IsMatch(parts.ElementAt(i), @"^bc$"))
                            {
                                sregexp = sregexp + @"bc" + @"\.";
                            }
                            else
                            {
                                string sPart = parts.ElementAt(i).ToLower();
                                for (int j = 0; j < sPart.Length; j++)
                                {
                                    string sLetter = sPart.ElementAt(j).ToString();
                                    if (Regex.IsMatch(sLetter, @"^[a-z]"))
                                        sregexp = sregexp + sLetter;
                                    else if (Regex.IsMatch(sLetter, @"^[0-9]") && !Regex.IsMatch(sregexp, @"\\d+$"))
                                        sregexp = sregexp + @"\d+";
                                }
                                sregexp = sregexp + @"\.";
                            }
                        }
                    }
                }

                if (sregexp == "^")
                    ll = l
                    .OrderBy(p => p.Value.ToLower().GetIfrsSortValue(), new NaturalSortComparer<string>());
                else
                {
                    sregexp = sregexp + @"$";
                    Regex pattern = new Regex(sregexp);
                    ll = l
                        .Where(p => pattern.IsMatch(p.Value.ToLower()))
                        .OrderBy(p => p.Value.ToLower().GetIfrsSortValue(), new NaturalSortComparer<string>());
                }

            }
            else if (taglevel == 3)
            {
                if (From.Split('/').Count() == To.Split('/').Count())
                {
                    ll = l
                            .Where(p => p.Value.Split('/').Count() == From.Split('/').Count())
                            .OrderBy(p => p.Value.ToLower(), new NaturalSortComparer<string>());
                }
                else if (From.Split('/').Count() > To.Split('/').Count())
                {
                    string rFrom = From;
                    for (int i = 0; i < To.Split('/').Count(); i++)
                    {
                        if (i == 0)
                            From = rFrom.Split('/').ElementAt(i);
                        else
                            From = From + "/" + rFrom.Split('/').ElementAt(i);
                    }
                    ll = l
                            .Where(p => p.Value.Split('/').Count() == From.Split('/').Count());
                    //.OrderBy(p => p.Value.ToLower(), new NaturalSortComparer<string>());
                }
                else if (From.Split('/').Count() < To.Split('/').Count())
                {
                    string rTo = To;
                    for (int i = 0; i < From.Split('/').Count(); i++)
                    {
                        if (i == 0)
                            To = rTo.Split('/').ElementAt(i);
                        else
                            To = To + "/" + rTo.Split('/').ElementAt(i);
                    }
                    ll = l
                            .Where(p => p.Value.Split('/').Count() == From.Split('/').Count());
                    //.OrderBy(p => p.Value.ToLower(), new NaturalSortComparer<string>());
                }
            }
            if (ll == null) return returnValue;

            From = From.Trim().ToLower().GetIfrsSortValue();
            To = To.Trim().ToLower().GetIfrsSortValue();

            foreach (XElement e in ll)
            {
                string sortValue = e.Value.Trim().ToLower().GetIfrsSortValue();
                if (!take)
                {
                    if (sortValue == From)
                    {
                        returnValue.Add(e);
                        take = true;
                    }
                }
                else
                {
                    if (sortValue == To)
                    {
                        returnValue.Add(e);
                        break;
                    }
                    else
                    {
                        List<string> test = new List<string>();
                        test.Add(sortValue);
                        test.Add(To);
                        if (test.First() == To)
                        {
                            break;
                        }
                        returnValue.Add(e);
                    }
                }
            }
            if (returnValue.Count() == 0)
                returnValue = null;

            return returnValue;
        }
        public static string ReverseXor(this string s)
        {
            char[] charArray = s.ToCharArray();
            int len = s.Length - 1;

            for (int i = 0; i < len; i++, len--)
            {
                charArray[i] ^= charArray[len];
                charArray[len] ^= charArray[i];
                charArray[i] ^= charArray[len];
            }

            return new string(charArray);
        }
        public static void AddKey(this Dictionary<string, string> keys, string key, string value)
        {
            if (keys.ContainsKey(key))
            {
                keys[key] = value == null ? "" : value;
            }
            else
            {
                keys.Add(key, value);
            }
        }
        public static bool Between(this int num, int lower, int upper, bool inclusive = false)
        {
            return inclusive ? lower <= num && num <= upper : lower < num && num < upper;
        }
        public static IEnumerable<XElement> GetDescendantsAndSelf(this XElement e, IEnumerable<string> items)
        {
            if (items.FirstOrDefault() == null)
            {
                List<XElement> le = new List<XElement>();
                le.Add(e);
                return le;
            }
            else
                return from q in e.DescendantsAndSelf()
                       join d in items on q.Name.LocalName equals d
                       select q;
        }
        public static string GetAttributeValue(this XElement el, string AttributeName)
        {
            return el.Attribute(AttributeName) == null ? "" : el.Attribute(AttributeName).Value;
        }
    }
    public class TagRegexps : List<TagRegexp>
    {
        public TagRegexps() { }
    }
    public class TagRegexp
    {
        public string tag1 { get; set; }
        public string regexpname { get; set; }
    }
    public class IdLinkUpdate
    {
        public IdLink idlink { get; set; }
        public string tag1 { get; set; }
        public string regexpname { get; set; }
        public void UpdateIdlink(IdLink il, string tag1)
        {
            il.self_tag1 = il.tag1;
            il.tag1 = tag1;
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
    public class MatchTypeValue
    {
        public MatchTypes Type { get; set; }
        public string Value { get; set; }
        public MatchTypeValue(MatchTypes Type, string Value)
        {
            this.Type = Type;
            this.Value = Value;
        }
    }
    public enum IdAreas
    {
        Internal = 0,
        External = 1
    }
    public enum MatchTypes
    {
        Error = -1,
        NumberWord = 0,
        Ledd = 1,
        Punktum = 2,
        Decimal = 3,
        UpperAlfa = 4,
        UpperRoman = 5,
        LowerAlfa = 6,
        LowerRoman = 7,
        IFRSDecimal = 20,
        IFRSLowerAlpha = 21,
        IFRSLowerRoman = 22,
        IFRSBullet = 23,
        IFRSCase = 24,
        IFRSIssue = 25

    }
    public class IdLink
    {
        public string language { get; set; }
        public IdAreas idArea = IdAreas.External;
        public string id { get; set; }
        public string parent { get; set; }
        public int start = 0;
        public int length = 0;
        public string tag1 = "";
        public string self_tag1 = "";
        public string tag2 = "";
        public string tag2_alt = "";
        public string tag3 = "";
        public string totag2 = "";
        public string totag3 = "";
        public string regexpName = "";
        public string prefix = "";
        public string linkprefix = "";
        public bool Identified = false;
        public IdLink(RegexGroupRange rg, int start, int length, string parent = null)
        {
            Dictionary<string, string> Result = rg.Result;
            this.start = start;
            this.length = length;
            rg.parent.parent.nIdLinks++;
            this.id = "id" + rg.parent.parent.nIdLinks;

            this.language = Result.ContainsKey("language") ? Result["language"] : "";
            this.tag1 = (Result.ContainsKey("tag1") ? Result["tag1"] : "").Trim();
            this.tag1 = Regex.Replace(this.tag1, @"((.*?)(lov))(en|a)$", "$1");
            this.tag1 = Regex.Replace(this.tag1, @"((.*?))(\.)$", "$1");
            this.tag1 = Regex.Replace(this.tag1, @"\s+", " ");
            this.tag1 = Regex.Replace(this.tag1, @"slov", "lov");
            this.tag2 = Result.ContainsKey("language") ? Result["language"] : "";
            this.tag2 = Result.ContainsKey("tag2") ? Result["tag2"] : "";
            this.tag2_alt = Result.ContainsKey("tag2_alt") ? Result["tag2_alt"] : "";
            this.tag3 = Result.ContainsKey("tag3") ? Result["tag3"] : "";

            if (Result.ContainsKey("internal"))
            {
                this.idArea = IdAreas.Internal;
            }
            if (Result.ContainsKey("prefix")) this.prefix = Result["prefix"];
            if (Result.ContainsKey("linkprefix")) this.linkprefix = Result["linkprefix"].Trim().ToLower();
            this.regexpName = Result.ContainsKey("name") ? Result["name"] : "";
            this.parent = parent;
        }
    }
    public class IdentyfiedLinks : List<IdentyfiedLink>
    {
        public IdentyfiedLinks() { }

        private class IdentyfiedLinkGroups
        {
            public Markup mLink { get; set; }
            public IEnumerable<IdentyfiedLinkGroup> idGroups { get; set; }
        }
        private class IdentyfiedLinkGroup
        {
            public IdentyfiedLink link { get; set; }
            public IEnumerable<IdentyfiedLink> links { get; set; }

        }
        private IEnumerable<XElement> GetTopicSingle(IdentyfiedLinkGroup idGroup)
        {
            List<XElement> result = new List<XElement>();

            if (idGroup.link.topic_id != null)
            {

                result.Add(new XElement("topic",
                    new XAttribute("topic_id", idGroup.link.topic_id),
                    idGroup.link.segment_id == null
                    ? (
                            idGroup.link.select_id == null
                            ? null
                            : new XElement("select",
                                new XAttribute("select_id", idGroup.link.select_id),
                                idGroup.link.mark_id == null ? null : new XElement("mark", new XAttribute("mark_id", idGroup.link.mark_id))
                            )
                        )

                    : (
                            new XElement("segment",
                                new XAttribute("segment_id", idGroup.link.segment_id),
                                idGroup.link.select_id == null
                                ? null
                                : new XElement("select",
                                    new XAttribute("select_id", idGroup.link.select_id),
                                    idGroup.link.mark_id == null ? null : new XElement("mark", new XAttribute("mark_id", idGroup.link.mark_id))
                                  )

                            )
                        )
                    )
                );
            }

            if (result.Count() != 0) return result;
            else return null;

        }
        public XElement GetIdLinks(IEnumerable<Markup> markuplinks)
        {
            return new XElement("diblinks",
                new XAttribute("id", "diblink"),
                (
                    from markuplink in markuplinks
                    join idlink in this
                     on markuplink.id equals idlink.parent
                    join sublink in this
                     on idlink.id equals sublink.parent into sub
                    from subl in sub.DefaultIfEmpty()
                    select new { m = markuplink, id = idlink, sub = subl }
                 )//idlinks
                .GroupBy(p => p.m)
                .Select(p => new IdentyfiedLinkGroups
                {
                    mLink = p.Key,
                    idGroups = p.GroupBy(s => s.id)
                                .Select(s => new IdentyfiedLinkGroup
                                {
                                    link = s.Key,
                                    links = s.Where(q => q.sub != null).Select(q => q.sub)
                                })
                })
                .Select(l => new XElement("idlinks",
                        new XAttribute("id", l.mLink.id),
                        new XAttribute("version", "1.0"),
                        new XAttribute("text", l.mLink.value),
                            l.idGroups
                            .Select(idGroup =>
                            new XElement("idlink",
                                new XElement("tags",
                                    idGroup.link.tag1 == null ? null : new XAttribute("tag1", idGroup.link.tag1),
                                    idGroup.link.tag2 == null ? null : new XAttribute("tag2", idGroup.link.tag2),
                                    idGroup.link.tag3 == null ? null : new XAttribute("tag3", idGroup.link.tag2),
                                    idGroup.link.totag2 == null ? null : new XAttribute("totag2", idGroup.link.totag2),
                                    idGroup.link.totag3 == null ? null : new XAttribute("totag3", idGroup.link.totag2),
                                    idGroup.link.IDQuality == 0 ? null : new XAttribute("IDQuality", idGroup.link.IDQuality),
                                    idGroup.link.RegexName == null ? null : new XAttribute("RegexName", idGroup.link.RegexName)
                                    ),
                                idGroup.links.Count() == 0
                                ? GetTopicSingle(idGroup)
                                : (
                                    idGroup
                                    .links
                                    .Where(p => p.topic_id != null)
                                    .GroupBy(p => p.topic_id)
                                    .Select(p => new XElement("topic",
                                            new XAttribute("topic_id", p.Key),
                                            p.Select(q => q).ToList()
                                            .GroupBy(q => q.segment_id == null ? "" : q.segment_id)
                                            .Select(q =>
                                                q.Key == ""
                                                ?
                                                new XElement("segment",
                                                    q
                                                    .Where(r => r.select_id != null)
                                                    .GroupBy(r => r.select_id)
                                                    .Select(r =>
                                                        new XElement("select",
                                                            new XAttribute("select_id", r.Key),
                                                            r.Select(m => m.mark_id == null ? null : new XElement("mark", new XAttribute("mark_id", m.mark_id)))
                                                        )
                                                    )
                                                )
                                                :
                                                new XElement("segment",
                                                    new XAttribute("segment_id", q.Key),
                                                    q
                                                    .Where(r => r.select_id != null)
                                                    .GroupBy(r => r.select_id)
                                                    .Select(r => new XElement("select",
                                                            new XAttribute("select_id", r.Key),
                                                                r.Select(m => m.mark_id == null ? null : new XElement("mark", m.mark_id))
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
              );

        }
    }
    public class IdentyfiedLink
    {
        public string id { get; set; }
        public string parent { get; set; }
        //public string ID_topic_id = "";
        public string topic_id { get; set; }
        public string segment_id { get; set; }
        public string select_id { get; set; }
        public string mark_id { get; set; }
        public string title { get; set; }
        public string language { get; set; }
        public string tag1 { get; set; }
        public string tag2 { get; set; }
        public string tag2_alt { get; set; }
        public string tag3 { get; set; }
        public string totag2 { get; set; }
        public string totag3 { get; set; }
        public bool Internal = false;
        public IdAreas idArea { get; set; }
        public string RegexName { get; set; }
        public int pos = 0;
        public int length = 0;
        public bool Deleted = false;
        public string linkprefix = "";
        public int IDQuality = 0;
        public IdentyfiedLink() { }
        public IdentyfiedLink(IdLink idlink)
        {
            this.language = idlink.language;
            this.RegexName = idlink.regexpName;
            this.idArea = idlink.idArea;
            this.id = idlink.id;
            this.parent = idlink.parent;
            this.pos = idlink.start;
            this.tag1 = idlink.tag1.SetStringToNull();
            this.tag2 = idlink.tag2.SetStringToNull();
            this.tag2_alt = idlink.tag2_alt.SetStringToNull();
            this.tag3 = idlink.tag3.SetStringToNull();
            this.totag2 = idlink.totag2.SetStringToNull();
            this.totag3 = idlink.totag3.SetStringToNull();
        }
        public IdentyfiedLink(IdLink il, int IDQuality, XElement e, int i)
        {
            this.language = il.language;
            this.IDQuality = IDQuality;
            this.pos = il.start;
            this.RegexName = il.regexpName;
            this.Internal = (string)e.Parent.AncestorsAndSelf().Attributes("topic_id").LastOrDefault() == "_self" ? true : false;
            this.idArea = il.idArea;
            this.parent = il.parent;
            this.id = il.id + "/" + i.ToString();
            this.tag1 = il.tag1.SetStringToNull();
            this.tag2 = (e.Parent.Name.LocalName == "tag2" ? e.Value : il.tag2).SetStringToNull();
            this.tag3 = (e.Parent.Name.LocalName == "tag3" ? e.Value : null).SetStringToNull();
            this.topic_id = (string)e.Parent.AncestorsAndSelf().Attributes("topic_id").LastOrDefault();
            this.segment_id = (string)e.Parent.AncestorsAndSelf().Attributes("segment_id").FirstOrDefault();
            switch (e.Parent.Name.LocalName)
            {
                case "tag1": this.select_id = (string)e.Parent.Attributes("id").FirstOrDefault(); break;
                case "tag2": this.select_id = (string)e.Parent.Attributes("id").FirstOrDefault(); break;
                case "tag3": this.select_id = (string)e.Ancestors("tag2").Attributes("id").FirstOrDefault(); break;
            }
            this.mark_id = (string)e.Parent.Attributes("id").FirstOrDefault();
            this.title = (string)e.Parent.Attributes("title").FirstOrDefault();
            this.linkprefix = il.linkprefix;
            CheckId();

        }
        public IdentyfiedLink(TagData t)
        {
            this.language = t.idLink.language;
            t.idLink.Identified = true;
            this.IDQuality = t.IDQuality;
            this.pos = t.idLink.start;
            this.Internal = (string)t.tag.Parent.AncestorsAndSelf().Attributes("topic_id").LastOrDefault() == "_self" ? true : false;
            this.idArea = t.idLink.idArea;
            this.RegexName = t.idLink.regexpName;
            this.parent = t.idLink.parent;
            this.id = t.idLink.id;
            this.tag1 = t.idLink.tag1.SetStringToNull();
            this.tag2 = t.idLink.tag2.SetStringToNull();
            this.totag2 = t.idLink.totag2.SetStringToNull();
            this.tag3 = t.idLink.tag3.SetStringToNull();
            this.totag3 = t.idLink.totag3.SetStringToNull();
            this.topic_id = (string)t.tag.Parent.AncestorsAndSelf().Attributes("topic_id").LastOrDefault();
            this.segment_id = (string)t.tag.Parent.AncestorsAndSelf().Attributes("segment_id").FirstOrDefault();
            switch (t.tag.Parent.Name.LocalName)
            {
                case "tag1": this.select_id = (string)t.tag.Parent.Attributes("id").FirstOrDefault(); break;
                case "tag2": this.select_id = (string)t.tag.Parent.Attributes("id").FirstOrDefault(); break;
                case "tag3": this.select_id = (string)t.tag.Ancestors("tag2").Attributes("id").FirstOrDefault(); break;
            }
            this.mark_id = (string)t.tag.Parent.Attributes("id").FirstOrDefault();
            this.title = (string)t.tag.Parent.Attributes("title").FirstOrDefault();
            this.linkprefix = t.idLink.linkprefix;
            CheckId();
        }
        private void CheckId()
        {
            if (this.topic_id != null && this.segment_id != null)
            {
                if (this.topic_id == this.segment_id) this.segment_id = null;
            }
            if (this.segment_id != null && this.select_id != null)
            {
                if (this.segment_id == this.select_id) this.select_id = null;
            }

            if (this.segment_id != null && this.mark_id != null)
            {
                if (this.segment_id == this.mark_id) this.mark_id = null;
            }

            if (this.select_id != null && this.mark_id != null)
            {
                if (this.select_id == this.mark_id) this.mark_id = null;
            }
        }
        //public IdentyfiedLinks(IdLink idlink, XElement tag1, XElement idSpan)
        //{
        //    this.pos = idlink.start;
        //    this.RegexName = idlink.regexpName;
        //    this.idArea = idlink.idArea;
        //    this.id = idlink.id;

        //    this.parent = idlink.parent;
        //    this.ID_topic_id = tag1.GetAttributeValue("topic_id");
        //    this.topic_id = tag1.GetAttributeValue("topic_id");
        //    this.segment_id = idSpan.Ancestors("tag2").First().GetAttributeValue("segment_id");
        //    if (this.segment_id == "")
        //    {
        //        this.segment_id = tag1.GetAttributeValue("segment_id");
        //    }
        //    this.language = tag1.GetAttributeValue("language");
        //    this.pos = idlink.start;
        //    this.tag1 = idlink.tag1;
        //    this.tag2 = idSpan.Attribute("name").Value;
        //    this.totag2 = idlink.totag2;
        //    this.totag3 = idlink.totag3;
        //    //Sjekk tittel for kapittel/del
        //    if (RegexName == "para")
        //        this.title = "§ " + idSpan.Attribute("name").Value;
        //    else
        //    {
        //        this.title = idSpan.GetAttributeValue("title");
        //    }
        //    this.bm = idSpan.GetAttributeValue("id");
        //}
        //public IdentyfiedLinks(IdentyfiedLinks idlink)
        //{
        //    this.RegexName = idlink.RegexName;
        //    this.idArea = idlink.idArea;
        //    this.id = idlink.id;
        //    this.language = idlink.language;
        //    this.parent = idlink.parent;
        //    this.pos = idlink.pos;
        //    this.tag1 = idlink.tag1;
        //    this.tag2 = idlink.tag2;
        //    this.tag2_alt = idlink.tag2_alt;
        //    this.tag3 = idlink.tag3;
        //    this.totag2 = idlink.totag2;
        //    this.totag3 = idlink.totag3;
        //}
        //public IdentyfiedLinks(IdLink idlink, XElement tag1, IdAreas area = IdAreas.External)
        //{
        //    pos = idlink.start;
        //    RegexName = idlink.regexpName;
        //    idArea = area;
        //    id = idlink.id;
        //    parent = idlink.parent;
        //    ID_topic_id = tag1.GetAttributeValue("topic_id");
        //    topic_id = tag1.GetAttributeValue("topic_id");
        //    segment_id = tag1.GetAttributeValue("segment_id");
        //    title = tag1.GetAttributeValue("name");
        //    language = tag1.GetAttributeValue("language");
        //    pos = idlink.start;
        //    this.tag1 = idlink.tag1;
        //    this.tag2 = idlink.tag2;
        //    this.totag2 = idlink.totag2;
        //    this.tag2_alt = idlink.tag2_alt;
        //    this.tag3 = idlink.tag3;
        //    this.totag3 = idlink.totag3;

        //    if (idlink.tag2 != "")
        //    {
        //        XElement xtag2 = null;
        //        if (idlink.tag2_alt != "")
        //        {
        //            xtag2 = tag1
        //                .Elements("tag2")
        //                .Where(p =>
        //                    (p.Attribute("name") == null ? "" : p.Attribute("name").Value.ToLower()) == idlink.tag2_alt.ToLower()
        //                    && p.Attributes("id").FirstOrDefault() != null
        //                ).FirstOrDefault();
        //        }
        //        if (xtag2 == null)
        //        {
        //            xtag2 = tag1
        //                .Elements("tag2")
        //                .Where(p =>
        //                    (p.Attribute("name") == null ? "" : p.Attribute("name").Value.ToLower()) == idlink.tag2.ToLower()
        //                    && p.Attributes("id").FirstOrDefault() != null
        //                ).FirstOrDefault();
        //        }

        //        if (xtag2 != null)
        //        {
        //            this.segment_id = xtag2.GetAttributeValue("segment_id");
        //            this.bm = xtag2.GetAttributeValue("id");
        //            //Sjekk tittel
        //            if (RegexName == "para")
        //                this.title = "§ " + xtag2.GetAttributeValue("name");
        //            else
        //            {
        //                this.title = xtag2.Attribute("title") != null ? xtag2.GetAttributeValue("title") : xtag2.GetAttributeValue("name");
        //            }

        //            if (idlink.tag3 != "")
        //            {
        //                XElement xtag3 = xtag2
        //                    .Elements("tag3")
        //                    .Where(p =>
        //                        (p.Attribute("name") == null ? "" : p.Attribute("name").Value.ToLower()) == idlink.tag3.ToLower()
        //                        && p.Attributes("id").FirstOrDefault() != null
        //                ).FirstOrDefault();

        //                if (xtag3 != null)
        //                {
        //                    segment_id = xtag3.GetAttributeValue("segment_id");
        //                    bm = xtag3.GetAttributeValue("id");

        //                    if (RegexName == "para")
        //                        this.title = "§ " + xtag2.GetAttributeValue("name") + " " + rmActions._NumberWords.GetNumberwordsTitle(idlink.tag3);
        //                    else
        //                    {
        //                        this.title = xtag2.Attribute("title") != null ? xtag2.GetAttributeValue("title") : xtag2.GetAttributeValue("name");
        //                    }
        //                }

        //            }

        //        }

        //    }
        //}
    }
    public class RegexGroupRange
    {
        public RegexGroupRange top;
        public IdentificatorElement parent;
        public Dictionary<string, string> Result = new Dictionary<string, string>();
        public List<RegexGroup> RangeList;

    }
    public class RegexGroup
    {
        public int pos { get; set; }
        public int length { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public RegexGroup() { }
        public RegexGroup(Capture c, string groupname)
        {
            this.pos = c.Index;
            this.length = c.Length;
            this.name = groupname;
            this.value = c.Value;
        }
    }
    public class RegexMatchActions
    {
        public NumberWords _NumberWords;
        protected Regex regex;
        private List<MatchAction> _MatchList;
        private List<string> _GroupNames;
        public RegexMatchActions() { }
        public RegexMatchActions(Regex regex, XElement actions, string listOfWords)
        {
            this.regex = regex;
            _MatchList = GetMatchList(actions, this);
            _GroupNames = GetGroupNames(regex);
            _NumberWords = new NumberWords(listOfWords);
        }

        private List<MatchAction> GetMatchList(XElement actions, RegexMatchActions parent)
        {
            return actions
                    .Elements("action")
                    .Where(p => (string)p.Attributes("name").FirstOrDefault() == "total")
                    .Elements("match")
                    .Where(p => (string)p.Attributes("name").FirstOrDefault() != "")
                    .Select(p => new MatchAction(p, parent)
                    )
                    .ToList();
        }
        private List<string> GetGroupNames(Regex regex)
        {
            return regex
                        .GetGroupNames()
                        .Where(p => !Regex.IsMatch(p, @"^\d+$"))
                        .GroupBy(q => q)
                        .Select(r => r.Key)
                        .ToList();
        }
        private class MatchAction
        {
            public string name { get; set; }
            private RegexAction Actions;
            public MatchAction(XElement e, RegexMatchActions parent)
            {
                this.name = (string)e.Attributes("name").FirstOrDefault();
                this.Actions = new RegexAction(e, parent);
            }
            public void Execute(RegexGroupRange rg)
            {
                this.Actions.Execute(rg);
            }
        }
        private class RegexAction
        {

            private string ENDASH = Convert.ToString((char)0x2013);
            private string type;
            private List<RegexAction> Actions;
            private Dictionary<string, string> Attributes;
            private string name = "";
            private RegexMatchActions Parent;
            public RegexAction(XElement e, RegexMatchActions parent)
            {
                this.Parent = parent;
                try
                {
                    while (e.Name.LocalName == "runaction")
                    {
                        e = e.Ancestors()
                            .Last()
                            .Elements("action")
                            .Where(p => (string)p.Attributes("name").FirstOrDefault() == (string)e.Attributes("name").FirstOrDefault()).FirstOrDefault();
                        if (e == null) return;
                    }
                    this.name = (string)e.Attributes("name").FirstOrDefault();
                    this.type = e.Name.LocalName;
                    foreach (XAttribute a in e.Attributes())
                    {
                        if (Attributes == null) Attributes = new Dictionary<string, string>();
                        Attributes.Add(a.Name.LocalName, a.Value);
                    }
                    this.Actions = e.Elements()
                                        .Select(p => new RegexAction(p, parent))
                                        .ToList();
                }
                catch (SystemException err)
                {
                    throw new Exception("RegexAction - Error: \r\n" + err.Message);
                }
            }
            private class ParaSubReference
            {
                public int no { get; set; }
                public string refid { get; set; }
                public int pos { get; set; }
                public int length { get; set; }
                public string name { get; set; }
                public string lastTag3 { get; set; }
            }
            public void Execute(RegexGroupRange rg)
            {
                try
                {
                    bool stop = false;
                    foreach (RegexAction ra in this.Actions)
                    {
                        switch (ra.type)
                        {
                            case "gettag":
                                KeyValuePair<string, string> tag = ra.Attributes.Where(p => p.Key == "tag").FirstOrDefault();
                                KeyValuePair<string, string> group = ra.Attributes.Where(p => p.Key == "group").FirstOrDefault();
                                RegexGroup taggr = rg.RangeList.Where(p => p.name == group.Value).OrderBy(p => p.pos).FirstOrDefault();
                                string tagVal = (taggr != null ? taggr.value : "");
                                if (rg.Result.ContainsKey(tag.Value))
                                {
                                    rg.Result[tag.Value] = tagVal;
                                }
                                else
                                {
                                    rg.Result.Add(tag.Value, tagVal);
                                }
                                break;
                            case "true":
                            case "false": ra.Execute(rg); break;
                            case "foreach":
                                stop = ExecuteForeach(ra, rg);
                                break;
                            case "match":
                                stop = ExecuteMatch(ra, rg);
                                break;
                            case "get": ExecuteGet(ra, rg); break;
                            case "mark": ExecuteMark(ra, rg); break;
                            case "run": ExecuteRun(ra, rg); break;
                            case "internal": ExecuteInternal(ra, rg); break;
                            default:
                                ra.Execute(rg); break;
                        }
                        if (stop)
                            break;
                    }
                }
                catch (SystemException err)
                {
                    throw new Exception("Error: 'Execute ' \r\n " + err.Message);
                }
            }
            private void ExecuteRun(RegexAction ra, RegexGroupRange rg)
            {
                try
                {
                    if (ra.Attributes == null) return;
                    KeyValuePair<string, string> function = ra.Attributes.Where(p => p.Key == "function").FirstOrDefault();
                    if (function.Key == null) return;
                    switch (function.Value)
                    {
                        case "GetParagrafElements": GetParagrafElements(ra, rg); break;
                    }
                }
                catch (SystemException err)
                {
                    throw new Exception("Error: 'ExecuteRun ' \r\n " + err.Message);
                }
            }

            private List<RegexGroup> GetGroups(RegexGroup currentGroup, List<RegexGroup> groups)
            {
                List<RegexGroup> returnValue;
                returnValue = groups
                        .Where(p =>
                            p.pos >= currentGroup.pos
                            && (p.pos + p.length) <= (currentGroup.pos + currentGroup.length)
                            && p.name != currentGroup.name
                            && p.value != ""
                            )
                    //.GroupBy(p => new { pos = p.pos, length = p.length, name = p.name, value = p.value })
                    //.Select(p => new RegexGroup { pos = p.Key.pos, length = p.Key.length, name = p.Key.name, value = p.Key.value })
                        .OrderBy(p => p.pos).ThenByDescending(p => p.length)
                        .ToList();
                if (returnValue.Count() == 0) returnValue.Add(currentGroup);
                return returnValue;
            }


            private class DateParamenter
            {
                public string name { get; set; }
                public List<string> sub = new List<string>();
            }
            private string GetLovDate(KeyValuePair<string, string> values, RegexGroupRange rg)
            {
                string returnValue = "";
                try
                {
                    Dictionary<string, string> result = new Dictionary<string, string>();
                    List<DateParamenter> dps = (from s in values.Value.ToString().Split('|')
                                                select new DateParamenter
                                                {
                                                    name = s.Split('=').ElementAt(0),
                                                    sub = (from v in s.Split('=').ElementAt(1).Split(',')
                                                           select v).ToList()
                                                }).ToList();
                    foreach (DateParamenter dp in dps)
                    {
                        switch (dp.name)
                        {
                            case "type":
                                {
                                    RegexGroup r = rg.RangeList.Where(p => dp.sub.Where(q => q == p.name).Count() != 0).FirstOrDefault();
                                    if (r != null)
                                        result.Add(dp.name, r.name.Replace("type", ""));

                                }
                                break;
                            default:
                                {
                                    RegexGroup r = rg.RangeList.Where(p => dp.sub.Where(q => q == p.name).Count() != 0).FirstOrDefault();
                                    if (r != null)
                                        result.Add(dp.name, r.value);
                                }
                                break;
                        }
                    }
                    if (result.ContainsKey("date") && result.ContainsKey("number"))
                    {
                        try
                        {

                            DateTime date = DateTime.Parse(result["date"]);
                            returnValue = String.Format("{0:yyyy-MM-dd}", date) + "-" + result["number"];
                        }
                        catch { }
                    }
                    if (returnValue == "")
                    {
                        if (result.ContainsKey("year")
                            && result.ContainsKey("month")
                            && result.ContainsKey("day")
                            && result.ContainsKey("number")
                            )
                        {
                            try
                            {
                                DateTime date = DateTime.Parse(result["year"] + "-" + result["month"] + "-" + result["day"]);
                                returnValue = String.Format("{0:yyyy-MM-dd}", date) + "-" + result["number"];
                            }
                            catch { }
                        }

                    }

                    if (returnValue == "") return returnValue;

                    if (result.ContainsKey("type"))
                    {
                        returnValue = result["type"] + "-" + returnValue;
                    }

                }
                catch (SystemException err)
                {
                    throw new Exception("Error: 'GetLovDate' \r\n " + err.Message);
                }
                return returnValue;
            }

            private void AddToDictionary(Dictionary<string, string> dictionary, string key, string value)
            {
                if (dictionary.ContainsKey(key)) dictionary[key] = value;
                else dictionary.AddKey(key, value);
            }
            private void RemoveFromDictionary(Dictionary<string, string> dictionary, string key)
            {
                if (dictionary.ContainsKey(key)) dictionary.Remove(key);
            }
            private void GetParagrafElements(RegexAction ra, RegexGroupRange rg)
            {
                try
                {
                    KeyValuePair<string, string> groups = ra.Attributes.Where(p => p.Key == "groups").FirstOrDefault();
                    RegexGroup lovprefix = rg.RangeList.Where(p => p.name == "lovprefix").FirstOrDefault();
                    if (lovprefix != null)
                    {
                        AddToDictionary(rg.Result, "linkprefix", lovprefix.value);
                    }
                    RegexGroup paraTotal = rg.RangeList.Where(p => p.name == groups.Value).FirstOrDefault();
                    if (paraTotal == null) return;
                    List<RegexGroup> paraGroup = GetGroups(paraTotal, rg.RangeList);

                    List<RegexGroup> refGroups = paraGroup.Where(p => p.name == "lovrefs").ToList();

                    int min = rg.RangeList.Select(p => p.pos).Min();
                    int max = rg.RangeList.Select(p => p.pos + p.length).Max();
                    string parent_id = "";
                    if (!rg.Result.ContainsKey("internal"))
                    {
                        //if (!rg.Result.ContainsKey("tag1")) AddToDictionary(rg.Result, "tag1", "");
                        parent_id = rg.Result["parentid"];
                        //parent_id = AddIdLink(rg, min, max - min, rg.Result, parent_id);
                    }
                    else
                    {
                        parent_id = rg.Result["parentid"];
                    }

                    string tag2 = "";
                    MatchTypeValue result;
                    string splitPara = "";
                    string para_id = "";
                    for (int i = 0; i < refGroups.Count(); i++)
                    {
                        RegexGroup lovRef = refGroups.ElementAt(i);
                        List<RegexGroup> lovRefGroup =
                                        GetGroups(lovRef, paraGroup);
                        if (lovRefGroup.Count() != 0)
                        {
                            min = lovRefGroup.Select(p => p.pos).Min();
                            max = lovRefGroup.Select(p => p.pos + p.length).Max();

                            //Debug.Print("MATCH2: " + rg.parent.text.ToString().Substring(min, max - min));
                            List<RegexGroup> lovRefGroupSub =
                                            lovRefGroup
                                            .Where(p =>
                                                p.name == "pararef"
                                                    || p.name == "kapnr"
                                                    || p.name == "delnr"
                                                    || p.name == "split_para"
                                                )
                                            .OrderBy(p => p.pos).ThenByDescending(p => p.length)
                                            .ToList();


                            for (int j = 0; j < lovRefGroupSub.Count(); j++)
                            {

                                RemoveFromDictionary(rg.Result, "tag2");
                                RemoveFromDictionary(rg.Result, "tag2_alt");
                                RemoveFromDictionary(rg.Result, "tag3");

                                RegexGroup r = lovRefGroupSub.ElementAt(j);
                                switch (r.name)
                                {
                                    case "pararef":
                                        #region //pararef
                                        List<RegexGroup> pararef = GetGroups(r, lovRefGroup);
                                        RegexGroup prefix = pararef.Where(p => p.name == "paraprefix").FirstOrDefault();
                                        if (prefix != null)
                                        {
                                            AddToDictionary(rg.Result, "linkprefix", prefix.value);
                                        }
                                        RegexGroup paranr = pararef.Where(p => p.name == "paranr").FirstOrDefault();
                                        if (paranr != null)
                                        {
                                            List<ParaSubReference> parasubreference = new List<ParaSubReference>();
                                            tag2 = paranr.value.Trim();
                                            //AddToDictionary(rg.Result, "name", "paranr");
                                            AddToDictionary(rg.Result, "tag2", tag2);
                                            #region //PARA SUB
                                            List<RegexGroup> parasub = pararef.Where(p => p.name != "paranr").ToList();
                                            if (parasub.Count() != 0)
                                            {
                                                //Debug.Print(paranr.name.PadRight(15, ' ') + paranr.pos.ToString() + "->" + paranr.length + " " + paranr.value);
                                                //PARA SUB REFERENCE

                                                for (int n = 0; n < parasub.Count(); n++)
                                                #region //PARA SUB REFERENCE
                                                {
                                                    RegexGroup para = parasub.ElementAt(n);
                                                    string refid = "";
                                                    if (para.name.IndexOf("split") != -1)
                                                    {
                                                        refid = "split|" + para.value;
                                                    }
                                                    else
                                                    {
                                                        result = ra.Parent._NumberWords.GetValue(para.value);
                                                        if (result.Type != MatchTypes.Error)
                                                        {
                                                            switch (para.name)
                                                            {
                                                                case "leddnumber":
                                                                    //refid = ((int)MatchTypes.Ledd).ToString().PadLeft(2, '0') + result.Value.PadLeft(3, '0');
                                                                    refid = "l" + result.Value;
                                                                    break;
                                                                case "punumber":
                                                                    //refid = ((int)MatchTypes.Punktum).ToString().PadLeft(2, '0') + result.Value.PadLeft(3, '0');
                                                                    refid = "p" + result.Value;
                                                                    break;
                                                                case "pktnumber":
                                                                case "letternumber":
                                                                    //refid = ((int)result.Type).ToString().PadLeft(2, '0') + result.Value.PadLeft(3, '0');
                                                                    refid = para.value;
                                                                    break;
                                                                case "pnsingleletter":
                                                                    {
                                                                        //refid = (99).ToString().PadLeft(2, '0') + result.Value.PadLeft(3, '0');
                                                                        refid = para.value;
                                                                        if (tag2.ToLower().EndsWith(para.value.ToLower()))
                                                                        {
                                                                            tag2 = tag2.Substring(0, tag2.Length - para.value.Length);
                                                                            //AddToDictionary(rg.Result, "name", "paranr");
                                                                            AddToDictionary(rg.Result, "tag2", tag2);
                                                                            AddToDictionary(rg.Result, "tag2_alt", tag2 + para.value.Trim());
                                                                        }
                                                                    }
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                        }

                                                    }
                                                    if (refid != "")
                                                    {
                                                        parasubreference.Add(new ParaSubReference
                                                        {
                                                            no = n,
                                                            pos = para.pos,
                                                            length = para.length,
                                                            refid = refid,
                                                            name = para.name
                                                        });
                                                    }
                                                }
                                                #endregion


                                                if (parasubreference.Count() != 0)
                                                {
                                                    if (parasubreference.First().refid.StartsWith("split"))// ? parasubreference.First().refid.Split('|')[0] : ""))
                                                    {
                                                    }
                                                    else
                                                    {
                                                        string split = "";
                                                        string tag3 = "";
                                                        int subPos = 0;
                                                        int sublength = 0;
                                                        for (int m = 0; m < parasubreference.Count(); m++)
                                                        {
                                                            ParaSubReference psr = parasubreference.ElementAt(m);
                                                            if (psr.refid.StartsWith("split"))
                                                            {
                                                                split = psr.refid.Split('|')[1];
                                                                string lastTag3 = "";
                                                                if (tag3 != "")
                                                                {
                                                                    AddToDictionary(rg.Result, "tag3", tag3);
                                                                    para_id = AddIdLink(rg, (subPos == 0 ? paranr.pos : subPos), (sublength == 0 ? paranr.length : sublength), rg.Result, parent_id);
                                                                    RemoveFromDictionary(rg.Result, "tag3");
                                                                    lastTag3 = tag3;
                                                                    tag3 = "";
                                                                }

                                                                if ((m + 1) < parasubreference.Count())
                                                                {
                                                                    ParaSubReference psrNext = parasubreference.ElementAt(m + 1);
                                                                    string refidNext = psrNext.refid;
                                                                    for (int prev = m; prev >= 0; prev--)
                                                                    {
                                                                        ParaSubReference psrPrev = parasubreference.ElementAt(prev);
                                                                        if (psrPrev.name == psrNext.name)
                                                                        {
                                                                            tag3 = psrPrev.lastTag3;
                                                                            break;
                                                                        }

                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                psr.lastTag3 = tag3;
                                                                tag3 = (tag3 != "" ? tag3 + "/" : tag3) + psr.refid;
                                                                AddToDictionary(rg.Result, "tag3", tag3);
                                                                subPos = psr.pos;
                                                                sublength = psr.length;
                                                            }
                                                        }

                                                        if (tag3 != "" && (split.Trim().ToLower() == "til" || split.Trim().ToLower() == ENDASH))
                                                        {
                                                            //if (para_id == "") Debug.Print(para_id);
                                                            AddToDictionary(rg.Result, "split", split);
                                                            AddToDictionary(rg.Result, "to_parentid", para_id);
                                                            AddIdLink(rg, paranr.pos, paranr.length, rg.Result, parent_id);
                                                            RemoveFromDictionary(rg.Result, "split");
                                                            RemoveFromDictionary(rg.Result, "to_parentid");
                                                            split = "";
                                                        }
                                                        else
                                                        {
                                                            split = "";
                                                            if (splitPara == "–") splitPara = "til";
                                                            if (splitPara == "til" && para_id != "")
                                                            {
                                                                AddToDictionary(rg.Result, "split", splitPara);
                                                                AddToDictionary(rg.Result, "to_parentid", para_id);
                                                                AddIdLink(rg, (subPos == 0 ? paranr.pos : subPos), (sublength == 0 ? paranr.length : sublength), rg.Result, parent_id);
                                                                RemoveFromDictionary(rg.Result, "split");
                                                                RemoveFromDictionary(rg.Result, "to_parentid");
                                                                splitPara = "";
                                                            }
                                                            else
                                                            {
                                                                para_id = AddIdLink(rg, (subPos == 0 ? paranr.pos : subPos), (sublength == 0 ? paranr.length : sublength), rg.Result, parent_id);
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (splitPara == "–") splitPara = "til";
                                                    if (splitPara == "til" && para_id != "")
                                                    {
                                                        AddToDictionary(rg.Result, "split", splitPara);
                                                        AddToDictionary(rg.Result, "to_parentid", para_id);
                                                        AddIdLink(rg, paranr.pos, paranr.length, rg.Result, parent_id);
                                                        RemoveFromDictionary(rg.Result, "split");
                                                        RemoveFromDictionary(rg.Result, "to_parentid");
                                                        splitPara = "";
                                                    }
                                                    else
                                                    {
                                                        para_id = AddIdLink(rg, paranr.pos, paranr.length, rg.Result, parent_id);
                                                    }
                                                }
                                            }
                                            #endregion
                                            else
                                            {
                                                if (splitPara == "–") splitPara = "til";
                                                if (splitPara == "til" && para_id != "")
                                                {
                                                    AddToDictionary(rg.Result, "split", splitPara);
                                                    AddToDictionary(rg.Result, "to_parentid", para_id);
                                                    AddIdLink(rg, paranr.pos, paranr.length, rg.Result, parent_id);
                                                    RemoveFromDictionary(rg.Result, "split");
                                                    RemoveFromDictionary(rg.Result, "to_parentid");
                                                    splitPara = "";
                                                }
                                                else
                                                {
                                                    para_id = AddIdLink(rg, paranr.pos, paranr.length, rg.Result, parent_id);
                                                }
                                            }
                                        }
                                        break;
                                        #endregion
                                    case "kapnr":
                                        para_id = "";
                                        #region //kapnr
                                        result = ra.Parent._NumberWords.GetValue(r.value);
                                        tag2 = "kap" + r.value.Trim();// + result.Value.PadLeft(4, '0');
                                        //AddToDictionary(rg.Result, "name", "kapnr");
                                        AddToDictionary(rg.Result, "tag2", tag2);
                                        AddIdLink(rg, r.pos, r.length, rg.Result, parent_id);

                                        //Debug.Print(r.name.PadRight(15, ' ') + r.pos.ToString() + "->" + r.length + " " + r.value + "->" + result.Value);
                                        break;
                                        #endregion
                                    case "delnr":
                                        para_id = "";
                                        #region //delnr
                                        result = ra.Parent._NumberWords.GetValue(r.value);
                                        tag2 = "del" + r.value.Trim();//result.Value.PadLeft(4, '0');
                                        //AddToDictionary(rg.Result, "name", "delnr");
                                        AddToDictionary(rg.Result, "tag2", tag2);
                                        AddIdLink(rg, r.pos, r.length, rg.Result, parent_id);
                                        //Debug.Print(r.name.PadRight(15, ' ') + r.pos.ToString() + "->" + r.length + " " + r.value + "->" + result.Value);
                                        break;
                                        #endregion
                                }
                                //Debug.Print("tag1=" + tag1 + "; tag2=" + tag2 + ";" + (tag3 == "" ? "" :  "tag3=" + tag3));

                            }
                        }
                        if ((i + 1) < refGroups.Count())
                        {
                            RegexGroup next = refGroups.ElementAt(i + 1);

                            RegexGroup split = paraGroup
                                .Where(p =>
                                    p.pos >= lovRef.pos + lovRef.length
                                    && p.pos + p.length <= next.pos
                                    )
                                .FirstOrDefault();
                            if (split != null)
                            {
                                splitPara = split.value.Trim().ToLower();
                                if (splitPara == "–") splitPara = "til";
                                if (splitPara != "til" || para_id == "")
                                {
                                    splitPara = "";
                                }
                                //Debug.Print("SPLIT PARA: " + split.name.PadRight(15, ' ') + split.pos.ToString() + "->" + split.length + " " + split.value);
                                //Debug.Print("\n");
                            }
                        }
                        else
                        {
                            List<RegexGroup> rest = paraGroup
                                .Where(p =>
                                    p.pos >= lovRef.pos + lovRef.length
                                    )
                                    .ToList();
                            foreach (RegexGroup el in rest)
                            {
                                //Debug.Print("\n");
                                //Debug.Print("Slutter med");
                                Debug.Print(el.name.PadRight(15, ' ') + el.pos.ToString() + "->" + el.length + " " + el.value);
                            }
                        }
                    }
                }
                catch (SystemException err)
                {
                    throw new Exception("Error: 'GetParagrafElements' \r\n " + err.Message);
                }
            }
            private string AddIdLink(RegexGroupRange rg, int pos, int length, Dictionary<string, string> result, string parentid)
            {
                //if (pos == 451) Debug.Print("xx");
                IdLink idlink = null;

                if (rg.Result.ContainsKey("split") && rg.Result.ContainsKey("to_parentid"))
                {
                    idlink = rg.parent.idLinks.Where(p => p.id == rg.Result["to_parentid"]).FirstOrDefault();

                    if (idlink != null)
                    {
                        //if (idlink.id == "id1541") Debug.Print("id1541");
                        Markup markup = rg.parent.Markups.Where(p => p.id == rg.Result["parentid"]).FirstOrDefault();
                        int newLength = 0;
                        string pTag2 = idlink.tag2;
                        string totag2 = rg.Result.ContainsKey("tag2") ? rg.Result["tag2"] : "";
                        string first_prepara = rg.Result.ContainsKey("first_prepara") ? rg.Result["first_prepara"] : "";
                        string first_prepara1 = rg.Result.ContainsKey("first_prepara1") ? rg.Result["first_prepara1"] : "";

                        string para_prepara = rg.Result.ContainsKey("para_prepara") ? rg.Result["para_prepara"] : "";
                        string para_prepara1 = rg.Result.ContainsKey("para_prepara1") ? rg.Result["para_prepara1"] : "";
                        if (rg.Result.ContainsKey("name") ? rg.Result["name"].StartsWith("ifrs") : false)
                        {
                            if (!(Regex.IsMatch(pTag2, @"^\d+([A-Z])?$") && Regex.IsMatch(totag2, @"^\d+([A-Z])?$")))
                            {
                                if (first_prepara == para_prepara)
                                {
                                    if (first_prepara1 != para_prepara1)
                                    {
                                        if (
                                            (first_prepara1.StartsWith("BC") && !para_prepara1.StartsWith("BC"))
                                            ||
                                            (first_prepara1.StartsWith("DO") && !para_prepara1.StartsWith("DO"))
                                            ||
                                            (first_prepara1.StartsWith("IN") && !para_prepara1.StartsWith("IN"))
                                            )
                                        {
                                            totag2 = Regex.Replace(totag2, @"^" + para_prepara1, "");
                                            totag2 = Regex.Replace(totag2, @"^0", "");
                                            totag2 = first_prepara1 + totag2;
                                        }
                                        else if (Regex.IsMatch(pTag2, @"^[A-Z]+") && Regex.IsMatch(totag2, @"^\d+"))
                                        {
                                            totag2 = Regex.Match(pTag2, @"^[A-Z]+").Value + totag2;
                                        }
                                        else if (pTag2.Substring(0, 2) == totag2.Substring(0, 2))
                                        {
                                            //do nothing
                                        }
                                        else
                                        {
                                        }
                                    }
                                }
                                else
                                {
                                    if (first_prepara1 == para_prepara1)
                                    {
                                        totag2 = first_prepara + totag2;
                                    }
                                    else
                                    {

                                        pTag2 = Regex.Replace(pTag2, first_prepara + first_prepara1, "");
                                        int cT2split = pTag2.Split('.').Count();
                                        int cToT2split = totag2.Split('.').Count();
                                        if (cT2split == cToT2split)
                                        {
                                            totag2 = first_prepara + first_prepara1 + totag2;
                                        }
                                        else if (cT2split > cToT2split)
                                        {
                                            if (Regex.IsMatch(totag2, @"^\d+$"))
                                            {
                                                totag2 = first_prepara + first_prepara1 + Regex.Replace(pTag2, @"\d+$", totag2);
                                            }
                                            else if (Regex.IsMatch(totag2, @"^\d+\.\d+$"))
                                            {
                                                totag2 = first_prepara + first_prepara1 + Regex.Replace(pTag2, @"\d+\.\d$", totag2);
                                            }
                                            else
                                            {
                                            }
                                        }
                                        else if (cT2split < cToT2split)
                                        {
                                        }
                                    }
                                }
                            }
                        }
                        if (Regex.IsMatch(totag2, @"^ex\d+$"))
                        {
                            Match pre = Regex.Match(idlink.tag2, @"(?<pre>([A-Z]+\.))ex\d+$");
                            if (pre.Groups["pre"].Success)
                                totag2 = pre.Groups["pre"].Value + totag2;
                        }
                        string totag3 = rg.Result.ContainsKey("tag3") ? rg.Result["tag3"] : "";

                        //if (idlink.totag3 != "") Debug.Print("totag3");
                        if (totag2 != "" && idlink.tag2 != totag2)
                        {
                            //if (first_prepara != "" && totag2.IndexOf(first_prepara) ==-1)
                            //{
                            //    totag2 = first_prepara + totag2;
                            //}
                            //if (Regex.IsMatch(totag2,@"^ex\d+$"))
                            //{
                            //    Match pre = Regex.Match(idlink.tag2, @"(?<pre>([A-Z]+\.))ex\d+$");
                            //    if (pre.Groups["pre"].Success)
                            //        totag2 = pre.Groups["pre"].Value + totag2;
                            //}
                            List<string> tag2s = new List<string>();
                            tag2s.Add(idlink.tag2);
                            tag2s.Add(totag2);
                            if (tag2s.OrderBy(p => p, new NaturalSortComparer<string>()).First() == idlink.tag2)
                            {
                                newLength = (pos - idlink.start) + length;
                                idlink.length = idlink.length + newLength;
                                idlink.totag2 = totag2;
                                idlink.totag3 = totag3;


                            }
                            else
                            {
                                RemoveFromDictionary(rg.Result, "split");
                                RemoveFromDictionary(rg.Result, "to_parentid");
                                idlink = new IdLink(rg, pos, length, parentid);
                                rg.parent.idLinks.Add(idlink);
                            }
                        }
                        else
                        {
                            newLength = (pos - idlink.start) + length;
                            idlink.length = idlink.length + newLength;
                            idlink.totag2 = totag2;
                            idlink.totag3 = totag3;
                        }
                    }
                }
                else
                {
                    idlink = new IdLink(rg, pos, length, parentid);
                    Markup markup = rg.parent.Markups.Where(p => p.id == rg.Result["parentid"]).FirstOrDefault();
                    rg.parent.idLinks.Add(idlink);
                }
                return idlink.id;
            }
            private void ExecuteMark(RegexAction ra, RegexGroupRange rg)
            {
                try
                {
                    if (ra.Attributes == null) return;
                    KeyValuePair<string, string> groups = ra.Attributes.Where(p => p.Key == "groups").FirstOrDefault();
                    if (groups.Key != null)
                    {
                        StringBuilder sbText = new StringBuilder();
                        int pos = -1;
                        int length = -1;
                        foreach (string s in groups.Value.Split('|').Where(p => p != ""))
                        #region
                        {
                            if (s.StartsWith("$") && s.EndsWith("$"))
                            {
                                if (length > -1) length = length + s.Substring(1, s.Length - 2).Length;
                                sbText.Append(s.Substring(1, s.Length - 2));
                            }
                            else
                            {
                                RegexGroup r = rg.RangeList.Where(p => p.name == s).FirstOrDefault();
                                if (r == null) return;
                                if (pos == -1)
                                {
                                    pos = r.pos;
                                    length = r.length;
                                }
                                else if (pos < r.pos)
                                {
                                    pos = r.pos;
                                    length = length + r.length;
                                }
                                sbText.Append(r.value);
                            }

                        }
                        #endregion
                        if (pos != -1)
                        {
                            string parentid = rg.Result["parentid"];
                            string linkID = AddIdLink(rg, pos, length, rg.Result, parentid);

                            if (rg.Result.ContainsKey("linkId"))
                                rg.Result["linkId"] = linkID;
                            else
                                rg.Result.Add("linkId", linkID);

                            //Debug.Print("parentId=" + parentid
                            //    + " linkID=" + linkID
                            //  + (rg.Result.ContainsKey("tag1") ? " tag1=" + rg.Result["tag1"] : "")
                            //  + (rg.Result.ContainsKey("tag2") ? " tag2=" + rg.Result["tag2"] : "")
                            //  + (rg.Result.ContainsKey("tag3") ? " tag3=" + rg.Result["tag3"] : ""));


                        }
                    }
                }
                catch (SystemException err)
                {
                    throw new Exception("Error: 'ExecuteMark' \r\n " + err.Message);
                }

            }
            private void ExecuteInternal(RegexAction ra, RegexGroupRange rg)
            {
                if (ra.Attributes != null)
                {
                    KeyValuePair<string, string> prefix = ra.Attributes.Where(p => p.Key == "prefix").FirstOrDefault();
                    if (prefix.Value != null) rg.Result.AddKey("prefix", prefix.Value);
                }
                rg.Result.AddKey("internal", "true");
                if (rg.parent.parent.InternalReference)
                {
                    ra.Execute(rg);
                }
            }
            private void ExecuteGet(RegexAction ra, RegexGroupRange rg)
            {
                try
                {
                    if (ra.Attributes == null) return;
                    KeyValuePair<string, string> tag = ra.Attributes.Where(p => p.Key == "tag").FirstOrDefault();
                    KeyValuePair<string, string> groups = ra.Attributes.Where(p => p.Key == "groups").FirstOrDefault();
                    KeyValuePair<string, string> function = ra.Attributes.Where(p => p.Key == "function").FirstOrDefault();
                    if (tag.Key == null) return;
                    bool found = false;
                    StringBuilder sbTag = new StringBuilder();
                    if (function.Key != null)
                    {
                        KeyValuePair<string, string> values = ra.Attributes.Where(p => p.Key == "values").FirstOrDefault();
                        switch (function.Value)
                        {
                            case "GetLovDate":
                                string date = GetLovDate(values, rg);
                                if (date != "")
                                {
                                    found = true;
                                    sbTag.Append(date);
                                }
                                break;
                        }
                    }
                    else
                    {

                        if (groups.Key == null) return;
                        found = true;
                        foreach (string search in groups.Value.Split('|').Where(p => p != ""))
                        {
                            string s = search;
                            if (s.StartsWith("'") && s.EndsWith("'"))
                            {
                                string tagN = s.Substring(1, s.Length - 2);
                                if (rg.Result.ContainsKey(tagN))
                                {
                                    sbTag.Append(rg.Result[tagN]);
                                }
                                else
                                    return;

                            }
                            else if (s.StartsWith("$") && s.EndsWith("$"))
                            {
                                string strTag = s.Substring(1, s.Length - 2);
                                if (strTag.ToLower() == "_self")
                                    strTag = "this";
                                sbTag.Append(strTag);
                            }
                            else
                            {
                                bool optional = false;
                                if (s.StartsWith("?"))
                                {
                                    optional = true;
                                    s = s.Replace("?", "");
                                }
                                RegexGroup r = rg.RangeList.Where(p => p.name == s).FirstOrDefault();
                                if (r == null && !optional)
                                {
                                    found = false;
                                    break;
                                }
                                else if (r != null)
                                    sbTag.Append(r.value);
                            }
                        }
                    }
                    if (found)
                    {

                        if (rg.Result == null) rg.Result = new Dictionary<string, string>();

                        if (rg.Result.ContainsKey(tag.Value))
                        {
                            rg.Result[tag.Value] = sbTag.ToString();
                        }
                        else
                        {
                            rg.Result.Add(tag.Value, sbTag.ToString());
                        }
                    }

                    RegexAction raFalse = ra.Actions.Where(p => p.type == "false").FirstOrDefault();
                    if (raFalse != null && !found)
                    {
                        ra = raFalse;
                        ra.Execute(rg);
                    }
                    else
                    {
                        ra.Execute(rg);
                    }
                    RemoveFromDictionary(rg.Result, tag.Value);
                }
                catch (SystemException err)
                {
                    throw new Exception("Error: 'ExecuteGet' \r\n " + err.Message);
                }
            }
            private bool ExecuteForeach(RegexAction ra, RegexGroupRange rg)
            {
                bool returnValue = false;
                try
                {
                    if (ra.Attributes == null) return true;
                    KeyValuePair<string, string> queryType = ra.Attributes.Where(p => p.Key == "querytype").FirstOrDefault();
                    if (queryType.Key == null) return returnValue;
                    if (queryType.Value == "group")
                    {
                        KeyValuePair<string, string> group = ra.Attributes.Where(p => p.Key == "groups").FirstOrDefault();
                        string gNames = group.Value;
                        //if (gNames == "ifrs_app_number_outer") Debug.Print("xx");
                        List<RegexGroup> CaptureGroups = (from r in rg.RangeList
                                                          join s in gNames.Split('|').Where(p => p != "")
                                                          on r.name.ToLower() equals s.ToLower()
                                                          orderby r.pos
                                                          select r).ToList();

                        //List<RegexGroup> CaptureGroups = rg.RangeList.Where(p => p.name == group.Value).OrderBy(p => p.pos).ToList();
                        bool boolTo = false;
                        string toParentId = "";
                        for (int i = 0; i < CaptureGroups.Count(); i++)
                        {
                            RegexGroup current = CaptureGroups.ElementAt(i);
                            RegexGroupRange newRg = new RegexGroupRange
                            {
                                top = rg.top == null ? rg : rg.top,
                                parent = rg.parent,
                                RangeList = GetGroups(current, rg.RangeList),
                                Result = rg.Result

                            };

                            if (boolTo)
                            {
                                //newRg.Result["parentid"] = toParentId;
                                if (!newRg.Result.ContainsKey("to_parentid"))
                                    newRg.Result.AddKey("to_parentid", toParentId);
                                else
                                    newRg.Result["to_parentid"] = toParentId;

                                if (!newRg.Result.ContainsKey("split"))
                                    newRg.Result.AddKey("split", "true");
                            }

                            returnValue = true;
                            ra.Execute(newRg);
                            if (rg.Result.ContainsKey("split"))
                            {
                                rg.Result.Remove("split");
                            }

                            toParentId = newRg.Result.ContainsKey("linkId") ? newRg.Result["linkId"] : "";


                            //Sjekk til her
                            if ((i + 1) < CaptureGroups.Count())
                            {
                                RegexGroup next = CaptureGroups.ElementAt(i + 1);
                                List<RegexGroup> splits = rg.RangeList
                                    .Where(p =>
                                        p.pos >= current.pos + current.length
                                        && p.pos + p.length <= next.pos
                                        )
                                    .ToList();
                                if (splits.Count() != 0)
                                {
                                    if (splits.Where(p => p.name.EndsWith("sep_to")).Count() != 0)
                                    {
                                        boolTo = true;
                                    }
                                    else
                                    {
                                        boolTo = false;
                                    }

                                }
                            }
                        }
                    }
                }
                catch (SystemException err)
                {
                    throw new Exception("Error: 'ExecuteForeach' \r\n " + err.Message);
                }
                return returnValue;
            }
            private bool ExecuteMatch(RegexAction ra, RegexGroupRange rg)
            {
                bool returnValue = false;
                try
                {
                    if (ra.Attributes == null) return true;
                    List<string> groups = ra.Attributes.Where(p => p.Key == "name").First().Value.Split('|').Where(p => p != "").Select(p => p).ToList();


                    if (groups.Where(p => rg.RangeList.Where(q => q.name == p).Count() == 0).Count() != 0)
                    {
                        RegexAction raFalse = ra.Actions.Where(p => p.type == "false").FirstOrDefault();
                        if (raFalse != null)
                        {
                            returnValue = true;
                            ra = raFalse;
                            ra.Execute(rg);
                        }
                        else
                            return returnValue;
                    }
                    else
                    {
                        //if (ra.name != "")
                        //AddToDictionary(rg.Result, "name", ra.name);
                        RegexAction raTrue = ra.Actions.Where(p => p.type == "true").FirstOrDefault();
                        if (raTrue != null)
                        {
                            returnValue = true;
                            ra = raTrue;
                            ra.Execute(rg);
                        }
                        else
                        {
                            returnValue = true;
                            ra.Execute(rg);
                        }

                    }
                }
                catch (SystemException err)
                {
                    throw new Exception("Error: 'ExecuteMatch' \r\n " + err.Message);
                }
                return returnValue;
            }
        }
        private class RegexGroups
        {
            public List<RegexGroup> _RegexGroups;
            public RegexGroups(Match m, IdentificatorElement element)
            {
                try
                {
                    GroupCollection groups = m.Groups;
                    foreach (string grpName in element.parent.rmActions._GroupNames)
                    {
                        if (groups[grpName].Success)
                        {
                            foreach (Capture c in groups[grpName].Captures)
                            {
                                if (this._RegexGroups == null) this._RegexGroups = new List<RegexGroup>();
                                this._RegexGroups.Add(new RegexGroup(c, grpName));
                            }
                        }
                    }
                }
                catch (SystemException err)
                {
                    throw new Exception("Error: 'RegexGroups' \r\n " + err.Message);
                }

            }
        }
        private string AddMarkup(RegexGroupRange rg, Match m, string name)
        {
            int pos = m.Index;
            int length = m.Length;

            Markup markup = new Markup(rg, m.Index, m.Length, name, m.Value);
            rg.parent.Markups.Add(markup);
            return markup.id;

        }
        public void Execute(IdentificatorElement element)
        {
            try
            {
                string language = element.parent.Language;
                MatchCollection mc = this.regex.Matches(element.text.ToString());
                foreach (Match m in mc)
                {
                    //Debug.Print(m.Value);
                    //if (m.Groups["ifrs_standard_single"].Success) Debug.Print(m.Value);
                    //if (m.Value.Trim().StartsWith("IFRS")) Debug.Print(m.Value);
                    //if (m.Value.Trim() == "paragraph 73(e)(iv)–(vi)") Debug.Print(m.Value);
                    //if (Regex.IsMatch(m.Value.Trim(), @"IE\.ex6")) Debug.Print(m.Value);
                    RegexGroups regexgroups = new RegexGroups(m, element);

                    var groups = from qrp in regexgroups._RegexGroups
                                 join ml in _MatchList on qrp.name equals ml.name
                                 select new { capture = qrp, action = ml };
                    foreach (var group in groups)
                    {

                        MatchAction ma = group.action;
                        RegexGroupRange rg = new RegexGroupRange
                        {

                            parent = element,
                            RangeList = regexgroups
                                        ._RegexGroups
                                        .Where(p =>
                                                p.pos >= group.capture.pos
                                                && (p.pos + p.length) <= (group.capture.pos + group.capture.length)
                                                )
                                        .GroupBy(p => new { pos = p.pos, length = p.length, name = p.name, value = p.value })
                                        .Select(p => new RegexGroup { pos = p.Key.pos, length = p.Key.length, name = p.Key.name, value = p.Key.value })
                                        .OrderBy(p => p.pos)
                                        .ThenBy(p => p.length)
                                        .ToList()
                        };

                        string parentid = AddMarkup(rg, m, ma.name);
                        if (parentid != null)
                        {
                            //if (parentid == "m21") Debug.Print("xxx");
                            rg.Result.AddKey("parentid", parentid);
                            rg.Result.AddKey("name", ma.name);
                            rg.Result.AddKey("language", language);
                            ma.Execute(rg);

                        }
                        if (rg.parent.idLinks.Where(p => p.parent == parentid).Count() == 0)
                        {

                        }

                        rg = null;
                    }
                }
            }
            catch (SystemException err)
            {
                throw new Exception("Error: 'RegexMatchActions.Execute' \r\n " + err.Message);
            }
        }
    }
    public class NumberWords
    {
        public List<NumberWord> numbers { get; set; }
        public Regex numbersregex { get; set; }
        public Dictionary<string, int> RomanNumbers { get; set; }
        public NumberWords(string numbervalues)
        {

            RomanNumbers = new Dictionary<string, int>();
            RomanNumbers.Add("M", 1000);
            RomanNumbers.Add("CM", 900);
            RomanNumbers.Add("D", 500);
            RomanNumbers.Add("CD", 400);
            RomanNumbers.Add("C", 100);
            RomanNumbers.Add("XC", 90);
            RomanNumbers.Add("L", 50);
            RomanNumbers.Add("XL", 40);
            RomanNumbers.Add("X", 10);
            RomanNumbers.Add("IX", 9);
            RomanNumbers.Add("V", 5);
            RomanNumbers.Add("IV", 4);
            RomanNumbers.Add("I", 1);

            string regex = "";
            foreach (string s in numbervalues.Split(';'))
            {
                if (s.Contains("="))
                {
                    if (this.numbers == null) this.numbers = new List<NumberWord>();
                    NumberWord nw = new NumberWord(s.Split('=').ElementAt(0), s.Split('=').ElementAt(1));
                    this.numbers.Add(nw);
                    if (regex != "") regex = regex + "|" + nw.number;
                    else regex = regex + nw.number;
                }
            }
            numbersregex = new Regex("(" + regex + ")");
        }
        private int GetRomanValue(string strRomanValue)
        {
            int retVal = 0;
            foreach (KeyValuePair<string, int> pair in RomanNumbers)
            {
                while (strRomanValue.IndexOf(pair.Key.ToString()) == 0)
                {
                    retVal += int.Parse(pair.Value.ToString());
                    strRomanValue = strRomanValue.Substring(pair.Key.ToString().Length);
                }
            }
            return retVal;
        }
        public string GetRomanNumber(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
            if (number < 1) return string.Empty;
            if (number >= 1000) return "M" + GetRomanNumber(number - 1000);
            if (number >= 900) return "CM" + GetRomanNumber(number - 900); //EDIT: i've typed 400 instead 900
            if (number >= 500) return "D" + GetRomanNumber(number - 500);
            if (number >= 400) return "CD" + GetRomanNumber(number - 400);
            if (number >= 100) return "C" + GetRomanNumber(number - 100);
            if (number >= 90) return "XC" + GetRomanNumber(number - 90);
            if (number >= 50) return "L" + GetRomanNumber(number - 50);
            if (number >= 40) return "XL" + GetRomanNumber(number - 40);
            if (number >= 10) return "X" + GetRomanNumber(number - 10);
            if (number >= 9) return "IX" + GetRomanNumber(number - 9);
            if (number >= 5) return "V" + GetRomanNumber(number - 5);
            if (number >= 4) return "IV" + GetRomanNumber(number - 4);
            if (number >= 1) return "I" + GetRomanNumber(number - 1);
            throw new ArgumentOutOfRangeException("something bad happened");
        }
        public MatchTypeValue GetValue(string numword)
        {
            try
            {
                numword = numword.Trim();
                if (Regex.IsMatch(numword, @"^\d+$"))
                {
                    return new MatchTypeValue(MatchTypes.Decimal, numword);
                }
                else if (numword.Length == 1)
                {
                    if (numword == "I")
                        return new MatchTypeValue(MatchTypes.UpperRoman, "1");
                    else if (numword == "i")
                        return new MatchTypeValue(MatchTypes.LowerRoman, "1");
                    else if (numword == "X")
                        return new MatchTypeValue(MatchTypes.UpperRoman, "10");
                    else if (numword == "x")
                        return new MatchTypeValue(MatchTypes.LowerRoman, "10");
                    else
                    {
                        int test = (int)numword.ToUpper().ToCharArray().ElementAt(0) - 64;
                        if (Regex.IsMatch(numword, "^ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ$"))
                            return new MatchTypeValue(MatchTypes.UpperAlfa, test.ToString());
                        else
                            return new MatchTypeValue(MatchTypes.LowerAlfa, test.ToString());
                    }
                }
                else if (numword.Length > 1)
                {
                    if (numbersregex.IsMatch(numword))
                    {
                        NumberWord value = numbers.Find(
                             delegate(NumberWord p)
                             {
                                 return p.number == numword.ToLower();
                             });
                        if (value != null)
                        {
                            return new MatchTypeValue(MatchTypes.NumberWord, value.value);
                        }
                        return new MatchTypeValue(MatchTypes.NumberWord, "0"); ;
                    }
                    if (Regex.IsMatch(numword, @"^(X{0,1})(IX|IV|V?I{0,3})$"))
                    {
                        return new MatchTypeValue(MatchTypes.UpperRoman, GetRomanValue(numword).ToString());
                    }

                    if (Regex.IsMatch(numword, @"^(x{0,1})(ix|iv|v?i{0,3})$"))
                    {
                        return new MatchTypeValue(MatchTypes.LowerRoman, GetRomanValue(numword).ToString());
                    }


                }
                return new MatchTypeValue(MatchTypes.Error, "0");
            }
            catch (SystemException err)
            {
                throw new Exception("GetValue - Error: \r\n" + err.Message);
            }
        }
        public string GetAlfaNumber(int iValue)
        {
            return ((char)(iValue + 64)).ToString();
        }
        public string GetNumberWord(int iValue)
        {
            NumberWord n = numbers.Where(p => p.value == iValue.ToString()).FirstOrDefault();
            if (n == null) return "";
            else return n.number;
        }
        public string GetNumberwordsTitle(string wordcodes)
        {
            try
            {
                string returnValue = "";
                if (wordcodes.Length >= 5 && (wordcodes.Length % 5) == 0)
                {
                    int ant = wordcodes.Length / 5;
                    for (int i = 0; i < ant; i++)
                    {
                        string wordcode = wordcodes.Substring((i * 5), 5);
                        MatchTypes iType = (MatchTypes)Convert.ToInt32(wordcode.Substring(0, 2));
                        int iValue = Convert.ToInt32(wordcode.Substring(2, 3));

                        string nv = "";
                        switch (iType)
                        {
                            case MatchTypes.Ledd:
                                nv = GetNumberWord(iValue);
                                if (nv == "")
                                    nv = "(" + iValue.ToString() + ")";
                                nv = nv + " ledd";
                                break;
                            case MatchTypes.Punktum:
                                nv = GetNumberWord(iValue);
                                if (nv == "")
                                    nv = iValue.ToString() + ".";
                                nv = nv + " punktum";
                                break;
                            case MatchTypes.Decimal:
                                nv = "punkt " + iValue.ToString();
                                break;
                            case MatchTypes.LowerAlfa:
                                nv = "punkt " + GetAlfaNumber(iValue).ToLower();
                                break;
                            case MatchTypes.UpperAlfa:
                                nv = "punkt " + GetAlfaNumber(iValue).ToLower();
                                break;
                            case MatchTypes.LowerRoman:
                                nv = "punkt " + GetRomanNumber(iValue).ToLower();
                                break;
                            case MatchTypes.UpperRoman:
                                nv = "punkt " + GetRomanNumber(iValue).ToUpper();
                                break;
                        }
                        if (returnValue != "" && nv != "")
                        {
                            nv = " " + nv;
                        }
                        returnValue = returnValue + nv;

                    }
                }
                return returnValue;
            }
            catch (SystemException err)
            {
                throw new Exception("GetNumberwordsTitle - Error: \r\n" + err.Message);
            }
        }
    }
    public class NumberWord
    {
        public string number { get; set; }
        public string value { get; set; }
        public NumberWord(string number, string value)
        {
            this.number = number;
            this.value = value;
        }
    }
    public class XTextRange
    {
        public int pos { get; set; }
        public int length { get; set; }
        public int id { get; set; }
        public string tag1 { get; set; }
        public List<Markup> mLinks = new List<Markup>();

    }
    public class Markup
    {
        public int start { get; set; }
        public int length { get; set; }
        public string id { get; set; }
        public string regexpName { get; set; }
        public string value { get; set; }
        public string tag1 = "";
        public string tag2 = "";
        public string tag3 = "";
        public string totag2 = "";
        public string totag3 = "";

        public Markup(RegexGroupRange rg, int start, int length, string name, string value)
        {
            this.regexpName = name;
            this.start = start;
            this.length = length;
            this.value = value;
            rg.parent.parent.nMarkups++;
            this.id = "m" + rg.parent.parent.nMarkups;// nrOfInstances.ToString();
        }
    }
    public class IdentificatorElements : List<IdentificatorElement>
    {
        public IEnumerable<IdLink> idLinks { get; set; }
        public IEnumerable<Markup> mLinks { get; set; }
        public IdentificatorElements() { }
        public void SetMarks()
        {
            (
                from ie in this
                from text in ie.element.DescendantNodes()
                            .OfType<XText>().Where(p => p.Annotation<XTextRange>() != null)
                            .Select(p => new { text = p, data = p.Annotation<XTextRange>() })
                select new
                {
                    text = text.text,
                    data = text.data,
                    marks = ie.Markups
                        .Where(m =>
                                (text.data.pos.Between(m.start, m.start + m.length, true))
                            || ((text.data.pos + text.data.length).Between(m.start, m.start + m.length, true))
                            || ((text.data.pos <= m.start) && ((text.data.pos + text.data.length) >= (m.start + m.length)))
                        )
                        .OrderBy(m => m.start)
                        .ToList()
                }
                )
                .Where(p => p.marks.Count() != 0)
                .ToList()
                .ForEach(p => p.text.ReplaceXtext(p.marks));
        }
        public void GetAllIdLinks()
        {
            idLinks = from ie in this
                      from il in ie.idLinks
                      select il;

            mLinks = from ie in this
                     from ml in ie.Markups
                     select ml;
        }
        public void SetAlternativeTag(TagRegexps tags)
        {
            (
                from ie in this
                from t in ie.element.DescendantNodes()
                            .OfType<XText>()
                            .Where(p => p.Annotation<XTextRange>() != null)
                            .Select(p => p.Annotation<XTextRange>())
                            .Where(p =>
                                p.tag1 != null
                                && p.mLinks.Count() != 0
                            )
                select new
                {
                    ilink = (
                        from m in t.mLinks
                        join i in ie.idLinks.Where(q => q.tag1 == "this")
                            on m.id equals i.parent
                        join tag in tags
                            on new
                            {
                                tag1 = t.tag1,
                                rname = m.regexpName
                            }
                        equals
                            new
                            {
                                tag1 = tag.tag1,
                                rname = tag.regexpname
                            }
                        select new IdLinkUpdate
                        {
                            idlink = i,
                            tag1 = t.tag1
                        }
                     )
                     .ToList()
                }
            )
            .ToList()
            .ForEach(p => p.ilink.Select(q => q).ToList().ForEach(q => q.UpdateIdlink(q.idlink, q.tag1)));
        }
    }
    public class IdentificatorElement
    {
        public XElement element { get; set; }
        public Identificator parent { get; set; }
        public StringBuilder text = new StringBuilder();
        public int id { get; set; }
        public List<Markup> Markups = new List<Markup>();
        public List<IdLink> idLinks = new List<IdLink>();
        public IdentificatorElement() { }
    }
    public class TagDataX
    {
        public TagDataString tag { get; set; }
        public List<TagDataString> tags { get; set; }
        public IdLink idLink { get; set; }
    }
    public class TagDataHolder
    {
        public XElement tag { get; set; }
        public IdLink idLink { get; set; }
    }
    public class TagData
    {
        public XElement tag { get; set; }
        public List<XElement> tags { get; set; }
        public IdLink idLink { get; set; }
        public int IDQuality { get; set; }
    }
    public class TagDataElement
    {
        public List<string> regexpnames = new List<string>();
        public XElement t1 { get; set; }
        public XElement t2 { get; set; }
        public XElement t3 { get; set; }

    }
    public class TagDataString
    {
        public string name { get; set; }
        public string tag1 { get; set; }
        public string tag2 { get; set; }
        public string tag3 { get; set; }
        public string topid_id { get; set; }
        public string section_id { get; set; }
        public string bm { get; set; }
    }
    public class Identificator
    {
        //private  List<IdentificatorElement> IdentificatorElements { get; set; }
        private IdentificatorElements identificatorelements { get; set; }
        private TagRegexps tagregexps { get; set; }
        public RegexMatchActions rmActions;
        public int nMarkups = 0;
        public int nIdLinks = 0;
        private string Sep = " | ";
        private string Buffer = " ";
        private int Offseth = 0;
        public bool InternalReference = false;
        public string Include = "";
        public string Exclude = "";
        public string BreakNodes = "";
        public string NoBreakNodes = "";
        public string NoMarkupNodes = "";
        private IEnumerable<string> nonmarkupnodes;
        private XElement Document;
        private string listOfNumbers = "";
        private List<string> breaknodes;
        private List<string> nobreaknodes;
        private IdentyfiedLinks identyfiedlinks { get; set; }
        public XElement ReferencedTopicsTags;
        public XElement InternalTags;
        public XElement ExternalTags;
        public XElement ExternalExtractTags;
        public XElement UnIdentifiedTags;
        public XElement Links = null;
        public bool IdentifyGlobal = true;
        public bool Local = true;
        public string Language = "no";
        public string SQLProcedure = "Abbreviation_Identify_Tags_Ex1";
        public string DatabaseName = "TopicMap.dbo";
        public string _InternalTag = "this";
        public Identificator()
        {
            SetNumberWords();
        }
        public XElement ExecuteLinking(XElement Document,
                        XElement actions,
                        string Regexp = "",
                        string Language = "no"
            )
        {
            try
            {
                Offseth = Sep.Length;
                this.Language = Language;
                this.identyfiedlinks = new IdentyfiedLinks();

                this.Document = Document;
                Regex regex = new Regex(Regexp);
                rmActions = new RegexMatchActions(regex, actions, listOfNumbers);
                List<string> includes = Include.Split(';').Where(p => p != "").ToList();
                List<string> excludes = Exclude.Split(';').Where(p => p != "").ToList();
                breaknodes = BreakNodes.Split(';').Where(p => p != "").ToList();
                nobreaknodes = NoBreakNodes.Split(';').Where(p => p != "").ToList();
                nonmarkupnodes = NoMarkupNodes.Split(';').Where(p => p != "").ToList();

                int id = 0;
                this.identificatorelements = new IdentificatorElements();

                identificatorelements.AddRange(
                        from element in this.Document.DescendantsAndSelf()
                        join include in includes on element.Name.LocalName equals include
                        select GetIdElements(element, id++, excludes)
                );


                //IdentificatorElements = (from element in this.Document.DescendantsAndSelf()
                //                         join include in includes on element.Name.LocalName equals include
                //                         select GetIdElements(element, id++, excludes)
                //                        ).ToList();

                //Sjekker om det finnes alternativ tag1 for interne linker
                identificatorelements.SetMarks();

                identificatorelements.GetAllIdLinks();

                tagregexps = new TagRegexps();
                GetTegRegexp();
                if (tagregexps != null)
                {
                    (
                        from at in Document.Descendants("diblink")
                                .Where(p => p.Attribute("tag1") != null)
                        join tr in tagregexps
                            on
                                new
                                {
                                    tag1 = ((string)at.Attributes("tag1").FirstOrDefault() ?? "").ToLower(),
                                    regexpname = ((string)at.Attributes("regexpname").FirstOrDefault() ?? "").ToLower()
                                }
                        equals
                            new
                            {
                                tag1 = tr.tag1.ToLower(),
                                regexpname = tr.regexpname.ToLower()
                            }
                        join idlink in identificatorelements.idLinks.Where(p => p.tag1 == "this")
                            on (string)at.Attributes("refid").FirstOrDefault() equals idlink.parent
                        select new IdLinkUpdate
                        {
                            idlink = idlink,
                            tag1 = tr.tag1
                        }
                    )
                    .GroupBy(p => p)
                    .Select(p => p.Key)
                    .ToList()
                    .ForEach(p => p.UpdateIdlink(p.idlink, p.tag1));
                }

                //UpdateSubstituteTag1();
                //Oppdaterer interne linker eller refererte dokumenter
                UpdateInternalGlobal();

                //Identifiserer mot global current
                if (this.IdentifyGlobal)
                {
                    this.ExternalExtractTags = GetTags(IdAreas.External);
                    IdentifyExternalTags();
                    UpdateExternalGlobal();
                }

                //Legger til ikke identifiserte linker
                identyfiedlinks.AddRange(from d in GetIdLinks()
                                         select new IdentyfiedLink(d));

                //List<Markup> totMarkup = MarkupLinks();
                this.Links = identyfiedlinks.GetIdLinks(identificatorelements.mLinks);

                (
                    from dl in this.Document.Descendants("diblink")
                    join idl in this.Links.Descendants("idlinks").Where(p => p.Descendants("topic").Count() != 0)
                    on (string)dl.Attributes("refid").FirstOrDefault() equals (string)idl.Attributes("id").FirstOrDefault()
                    select dl
                )
                .ToList()
                .ForEach(p => p.Add(new XAttribute("ided", "1")));

                (
                    from dl in this.Document.Descendants("diblink").Where(p => p.Attribute("ided") == null)
                    select dl
                )
                .ToList()
                .ForEach(p => p.Add(new XAttribute("ided", "0")));
                return this.Document;
            }
            catch (SystemException err)
            {
                throw new Exception("ExecuteLinking - Error: \r\n" + err.Message);
            }


        }
        public XElement IdentifyTags(XElement tags, string language)
        {
            XElement returnValue = null;
            try
            {
                if (tags == null ? false : tags.HasElements)
                {
                    returnValue = IdentifyTags(CodeXMLBase64(tags), language);
                }
                return returnValue;
            }
            catch (SystemException err)
            {
                throw new Exception("IdentifyTags - Error: \r\n" + err.Message);
            }
        }
        private void IdentifyExternalTags()
        {
            try
            {
                if (this.ExternalExtractTags == null ? false : this.ExternalExtractTags.HasElements)
                {
                    this.ExternalTags = IdentifyTags(CodeXMLBase64(this.ExternalExtractTags), this.Language);
                    if (this.ExternalTags != null)
                        this.UnIdentifiedTags = new XElement("tags", this.ExternalTags.Elements("tag1").Where(p => p.Attribute("topic_id") == null));
                }
            }
            catch (SystemException err)
            {
                throw new Exception("IdentifyExternalTags - Error: \r\n" + err.Message);
            }
        }
        private IdentificatorElement GetIdElements(XElement element, int elementId, List<string> excludes)
        {
            IEnumerable<XText> texts = element
                        .DescendantNodesAndSelf().OfType<XText>()
                        .Where(p =>
                            p.Ancestors()
                            .Where(q =>
                                excludes.Where(s => s == q.Name.LocalName).Count() != 0
                                && q != element
                            ).Count() == 0
                        )
                        .Select(p => p);



            //IEnumerable<XText> texts = element
            //                          .DescendantNodesAndSelf().OfType<XText>()
            //                          .Where(p =>
            //                              p.Ancestors()
            //                              .Where(q =>
            //                                    q.Parent == element
            //                                    ||
            //                                    excludes.Where(s => s == q.Name.LocalName).Count() != 0
            //                              )
            //                              .FirstOrDefault() != element
            //                           )
            //                          .Select(p => p);

            IdentificatorElement idEl = new IdentificatorElement
            {
                element = element,
                parent = this,
                id = elementId,
                text = GetIdText(texts, elementId)
            };

            rmActions.Execute(idEl);

            return idEl;
        }
        private StringBuilder GetIdText(IEnumerable<XText> texts, int elementId)
        {
            StringBuilder text = new StringBuilder();
            int pos = 0;
            try
            {
                Regex replace160 = new Regex(@"\u00A0");
                StringBuilder s = new StringBuilder();
                foreach (XText t in texts)
                {
                    s = new StringBuilder();
                    string tag1 = (string)t.Ancestors().Attributes("tag1").FirstOrDefault();
                    string rtag = (string)t.Ancestors().Attributes("replace_tag1").FirstOrDefault();
                    //int rtag1 = t.Ancestors().Where(p => p.Attribute("replace_tag1") != null).Count();
                    //int ntag1 = t.Ancestors().Where(p => p.Attribute("tag1") != null).Count();
                    //if (tag1 != null) Debug.Print(tag1);
                    if (tag1 == null)
                    {
                        Debug.Print((string)t.Ancestors("segment").Attributes("id").FirstOrDefault());
                    }
                    if (rtag != null)
                    {
                        tag1 = rtag;
                    }

                    if (breaknodes.FirstOrDefault() != null)
                    {
                        var an = from a in t.Ancestors()
                                 join bn in breaknodes on a.Name.LocalName equals bn
                                 where a.DescendantNodes().OfType<XText>().FirstOrDefault() == t
                                 select a.DescendantNodes().OfType<XText>().FirstOrDefault();

                        if (an.Count() != 0)
                        {
                            s.Append(Sep + replace160.Replace(t.ToString(), @" ") + Buffer);

                            pos = pos + this.Offseth;
                        }
                        else
                        {
                            s.Append(replace160.Replace(t.ToString(), @" ") + Buffer);

                        }

                        t.AddAnnotation(new XTextRange
                        {
                            id = elementId,
                            pos = pos,
                            length = t.ToString().Length,
                            tag1 = tag1
                        });

                        text.Append(s);
                        pos = text.Length;
                    }
                    else
                    {
                        var an = from a in t.Ancestors()
                                 where a.DescendantNodes().OfType<XText>().FirstOrDefault() == t
                                 select a.DescendantNodes().OfType<XText>().FirstOrDefault();

                        if (an.Count() != 0)
                        {
                            s.Append(Sep + replace160.Replace(t.ToString(), @" ") + Buffer);

                            pos = pos + this.Offseth;
                        }
                        else
                        {
                            s.Append(replace160.Replace(t.ToString(), @" ") + Buffer);

                        }

                        t.AddAnnotation(new XTextRange
                        {
                            id = elementId,
                            pos = pos,
                            length = t.ToString().Length,
                            tag1 = tag1

                        });

                        text.Append(s);
                        pos = text.Length;
                    }



                }
                return text;
            }
            catch (SystemException err)
            {
                throw new Exception("IdElement - Error: \r\n" + err.Message);
            }
        }
        private void SetNumberWords()
        {
            listOfNumbers = listOfNumbers + "første=1;";
            listOfNumbers = listOfNumbers + "fyrste=1;";
            listOfNumbers = listOfNumbers + "andre=2;";
            listOfNumbers = listOfNumbers + "annet=2;";
            listOfNumbers = listOfNumbers + "tredje=3;";
            listOfNumbers = listOfNumbers + "treje=3;";
            listOfNumbers = listOfNumbers + "fjerde=4;";
            listOfNumbers = listOfNumbers + "femte=5;";
            listOfNumbers = listOfNumbers + "sjette=6;";
            listOfNumbers = listOfNumbers + "sjuende=7;";
            listOfNumbers = listOfNumbers + "sjuande=7;";
            listOfNumbers = listOfNumbers + "syvende=7;";
            listOfNumbers = listOfNumbers + "åttende=8;";
            listOfNumbers = listOfNumbers + "åttande=8;";
            listOfNumbers = listOfNumbers + "niende=9;";
            listOfNumbers = listOfNumbers + "niande=9;";
            listOfNumbers = listOfNumbers + "tiende=10;";
            listOfNumbers = listOfNumbers + "tiande=10;";
            listOfNumbers = listOfNumbers + "siste=999;";
        }
        private string CodeXMLBase64(XElement tags)
        {
            string XmlString = tags.ToString();
            XmlString = Regex.Replace(XmlString, @"<\?xml[^<]+?>", "", RegexOptions.Multiline | RegexOptions.Singleline);
            return Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(XmlString));
        }
        public void SaveTextToFile(string filename)
        {
            try
            {
                XElement idelements = new XElement("idelements");
                foreach (IdentificatorElement e in identificatorelements)
                {
                    idelements.Add("idelement", e.text.ToString());
                }
                idelements.Save(filename);
            }
            catch (SystemException err)
            {
                throw new Exception("Save IdElements to file - Error: \r\n" + err.Message);
            }
        }


        private IEnumerable<IdentyfiedLink> GetIdentyfiedLinks(TagData t)
        {
            int i = 0;
            List<IdentyfiedLink> link = new List<IdentyfiedLink>();
            if (t.tags != null)
            {
                //if (t.idLink.id == "id1541") Debug.Print("id1541");
                t.idLink.Identified = true;

                if (t.tag != null)
                {
                    IdentyfiedLink l = new IdentyfiedLink(t);
                    string firstTitle = (string)t.tags.First().Parent.Attributes("title").FirstOrDefault();
                    string lastTitle = (string)t.tags.Last().Parent.Attributes("title").FirstOrDefault();
                    if (firstTitle != null && lastTitle != null)
                        l.title = l.title + firstTitle + "–" + lastTitle;
                    link.Add(l);
                    t.idLink.parent = l.id;

                }

                IEnumerable<IdentyfiedLink> links = from e in t.tags
                                                    select new IdentyfiedLink(t.idLink, t.IDQuality, e, i++);
                return (link)
                        .Union(
                        links
                        );
            }
            else if (t.tag != null)
            {
                t.idLink.Identified = true;
                IdentyfiedLink l = new IdentyfiedLink(t);
                link.Add(l);
                return link;
            }
            else
                return link;


        }

        private IEnumerable<IdLink> GetIdLinks()
        {
            return from l in identificatorelements.idLinks.Where(p => !p.Identified)
                   select l;
        }

        private IEnumerable<TagDataElement> GetTagsTag(XElement tags, int no)
        {
            if (no == 1)
            {
                return from t1 in tags.Descendants("tag1")
                           .Where(p => p.Attribute("id") != null || p.Attribute("topic_id") != null || p.Attribute("segment_id") != null)
                           .Elements("name")
                       select new TagDataElement
                       {
                           regexpnames = t1.Ancestors("tags").Attributes("regexpName").SelectMany(p => p.Value.Split(';')).ToList(),
                           t1 = t1
                       };

            }
            else if (no == 2)
            {
                return from t1 in tags.Descendants("tag1")
                                           .Where(p => p.Attribute("id") != null || p.Attribute("topic_id") != null || p.Attribute("segment_id") != null)
                                           .Elements("name")
                       from t2 in t1.Parent.Elements("tag2")
                           .Where(p => p.Attribute("id") != null || p.Attribute("segment_id") != null)
                           .Elements("name")
                       select new TagDataElement
                       {
                           regexpnames = t1.Ancestors("tags").Attributes("regexpName").SelectMany(p => p.Value.Split(';')).ToList(),
                           t1 = t1,
                           t2 = t2,
                       };
            }
            else if (no == 3)
            {
                return from t1 in tags.Descendants("tag1")
                                           .Where(p => p.Attribute("id") != null || p.Attribute("topic_id") != null || p.Attribute("segment_id") != null)
                                           .Elements("name")
                       from t2 in t1.Parent.Elements("tag2")
                           .Where(p => p.Attribute("id") != null || p.Attribute("segment_id") != null)
                           .Elements("name")
                       from t3 in t2.Parent.Elements("tag3").Where(p => p.Attribute("id") != null).Elements("name")
                       select new TagDataElement
                       {
                           regexpnames = t1.Ancestors("tags").Attributes("regexpName").SelectMany(p => p.Value.Split(';')).ToList(),
                           t1 = t1,
                           t2 = t2,
                           t3 = t3,
                       };
            }
            return null;
        }
        private void UpdateLinksTag3WToTag3(XElement tags)
        {
            //identifiser tag3 med totag3
            IEnumerable<TagDataElement> iTags = GetTagsTag(tags, 3);

            if ((iTags == null ? 0 : iTags.Count()) == 0) return;
            List<TagData> td =
                (from d in GetIdLinks()
                          .Where(p =>
                              p.tag1 != "" &&
                              p.tag2 != "" &&
                              p.totag2 == p.tag2 &&
                              p.tag3 != "" &&
                              p.totag3 != ""

                          )
                 join t3 in iTags
                                 on new
                                 {
                                     ta1 = d.tag1.ToLower(),
                                     ta2 = d.tag2.ToLower(),
                                     ta3 = d.tag3.ToLower()
                                 }
                                 equals
                                 new
                                 {
                                     ta1 = t3.t1.Value.ToLower(),
                                     ta2 = t3.t2.Value.ToLower(),
                                     ta3 = t3.t3.Value.ToLower(),
                                 }
                 //group d by new TagDataHolder { idLink = d,tag = t3.t2.Parent } into g1
                 select new TagData
                 {
                     tag = t3.t2,
                     tags = t3.t2.Parent
                             .Descendants("tag3").Where(p => p.Attribute("id") != null)
                             .Elements("name")
                             .ToList()
                             .GetElementsBetween(d.tag3, d.totag3, 3),
                     idLink = d,
                     IDQuality = 6
                 }).ToList();

            //td.Where(p => p.tag != null || p.tags != null).ToList().ForEach(p => p.idLink.Identified = true);
            identyfiedlinks.AddRange(td.SelectMany(p => GetIdentyfiedLinks(p)));


        }
        private void UpdateLinksTag2WToTag2(XElement tags)
        {
            IEnumerable<TagDataElement> iTags = GetTagsTag(tags, 2);
            if ((iTags == null ? 0 : iTags.Count()) == 0) return;

            //identifiser tag2 med totag2
            IEnumerable<TagData> td =
               from d in GetIdLinks()
                     .Where(p =>
                         p.tag1 != "" &&
                         p.tag2 != "" &&
                         p.totag2 != p.tag2 &&
                         p.totag2 != ""

                     )
               join t2 in iTags
                on new
                {
                    ta1 = d.tag1.ToLower(),
                    ta2 = d.tag2.ToLower(),
                }
                equals
                new
                {
                    ta1 = t2.t1.Value.ToLower(),
                    ta2 = t2.t2.Value.ToLower(),
                }
               //group d by new TagDataHolder { idLink = d, tag = t2.t1.Parent } into g1

               select new TagData
               {
                   tag = t2.t1,
                   tags = t2.t1.Parent
                           .Descendants("tag2")
                           .Where(p => p.Attribute("id") != null || p.Attribute("segment_id") != null)
                           .Elements("name")
                           .ToList().GetElementsBetween(d.tag2, d.totag2, 2),
                   idLink = d,
                   IDQuality = 4

               };
            identyfiedlinks.AddRange(td.SelectMany(p => GetIdentyfiedLinks(p)));

        }
        private void UpdateLinksTag3(XElement tags)
        {
            IEnumerable<TagDataElement> iTags = GetTagsTag(tags, 3);
            if ((iTags == null ? 0 : iTags.Count()) == 0) return;
            IEnumerable<TagData> td =
                from d in GetIdLinks()
                      .Where(p =>
                          p.tag1 != "" &&
                          p.tag2 != "" &&
                          p.tag3 != ""

                      )
                join t3 in iTags
                on new
                {
                    ta1 = d.tag1.ToLower(),
                    ta2 = d.tag2.ToLower(),
                    ta3 = d.tag3.ToLower()
                }
                equals
                new
                {
                    ta1 = t3.t1.Value.ToLower(),
                    ta2 = t3.t2.Value.ToLower(),
                    ta3 = t3.t3.Value.ToLower(),
                }
                select new TagData
                {
                    tag = t3.t3,
                    tags = null,
                    idLink = d,
                    IDQuality = 3
                };

            identyfiedlinks.AddRange(td.SelectMany(p => GetIdentyfiedLinks(p)));

            td =
                from d in GetIdLinks()
                      .Where(p =>
                          p.tag1 != "" &&
                          p.tag2 != "" &&
                          p.tag3 != "" &&
                          p.tag3.Split('/').Count() > 1

                      )
                join t3 in GetTagsTag(tags, 3)
                on new
                {
                    ta1 = d.tag1.ToLower(),
                    ta2 = d.tag2.ToLower(),
                    ta3 = d.tag3.ToLower()
                }
                equals
                new
                {
                    ta1 = t3.t1.Value.ToLower(),
                    ta2 = t3.t2.Value.ToLower(),
                    ta3 = t3.t3.Value.ToLower().SplitTag3(1),
                }
                select new TagData
                {
                    tag = t3.t3,
                    tags = null,
                    idLink = d
                };

            identyfiedlinks.AddRange(td.SelectMany(p => GetIdentyfiedLinks(p)));

        }
        private void UpdateLinksTag2(XElement tags)
        {
            IEnumerable<TagDataElement> iTags = GetTagsTag(tags, 2);
            if ((iTags == null ? 0 : iTags.Count()) == 0) return;
            //identifiser tag2
            IEnumerable<TagData> td =
                from d in GetIdLinks()
                      .Where(p =>
                          p.tag1 != "" &&
                          p.tag2 != ""
                      )
                join t2 in iTags
                 on new
                 {
                     ta1 = d.tag1.ToLower(),
                     ta2 = d.tag2.ToLower(),
                 }
                 equals
                 new
                 {
                     ta1 = t2.t1.Value.ToLower(),
                     ta2 = t2.t2.Value.ToLower(),
                 }
                select new TagData
                {
                    tag = t2.t2,
                    tags = null,
                    idLink = d,
                    IDQuality = 2
                };
            identyfiedlinks.AddRange(td.SelectMany(p => GetIdentyfiedLinks(p)));

        }
        private void ResetLinksTag1(XElement tags, string ResetValue = "")
        {
            (from d in GetIdLinks().Where(p => p.self_tag1 == _InternalTag)
             select d).ToList().ForEach(p => p.tag1 = ResetValue);
        }
        private void UpdateLinksTag1(XElement tags)
        {
            IEnumerable<TagDataElement> iTags = GetTagsTag(tags, 1);
            if ((iTags == null ? 0 : iTags.Count()) == 0) return;
            //identifiser tag1
            IEnumerable<TagData> td =
                from d in GetIdLinks().Where(p => p.tag1 != "" && p.tag2 == "")
                join t1 in iTags
                 on new
                 {
                     ta1 = d.tag1.ToLower(),
                 }
                 equals
                 new
                 {
                     ta1 = t1.t1.Value.ToLower(),
                 }
                select new TagData
                {
                    tag = t1.t1,
                    tags = null,
                    idLink = d,
                    IDQuality = 1
                };
            identyfiedlinks.AddRange(td.SelectMany(p => GetIdentyfiedLinks(p)));

        }
        private void UpdateInternalGlobal()
        {
            try
            {
                if (InternalTags != null)
                    UpdateLinksTag3WToTag3(InternalTags);

                if (ReferencedTopicsTags != null)
                    UpdateLinksTag3WToTag3(ReferencedTopicsTags);

                if (InternalTags != null)
                    UpdateLinksTag2WToTag2(InternalTags);

                if (ReferencedTopicsTags != null)
                    UpdateLinksTag2WToTag2(ReferencedTopicsTags);

                if (InternalTags != null)
                    UpdateLinksTag3(InternalTags);

                if (ReferencedTopicsTags != null)
                    UpdateLinksTag3(ReferencedTopicsTags);

                if (InternalTags != null)
                    UpdateLinksTag2(InternalTags);

                if (ReferencedTopicsTags != null)
                    UpdateLinksTag2(ReferencedTopicsTags);

                if (InternalTags != null)
                {
                    //hvis interne ikke er identifiser på tag2 eller tag2 tilbakestilles tag1 til blank
                    ResetLinksTag1(InternalTags);
                }

                if (InternalTags != null)
                    UpdateLinksTag1(InternalTags);

                if (ReferencedTopicsTags != null)
                    UpdateLinksTag1(ReferencedTopicsTags);

            }
            catch (SystemException err)
            {
                throw new Exception("UpdateInternal - Error: \r\n" + err.Message);
            }
        }
        private void UpdateExternalGlobal()
        {
            if (ExternalTags != null)
            {
                UpdateLinksTag3WToTag3(ExternalTags);
                UpdateLinksTag2WToTag2(ExternalTags);
                UpdateLinksTag3(ExternalTags);
                UpdateLinksTag2(ExternalTags);
                UpdateLinksTag1(ExternalTags);
            }
        }
        //==============================================================================================================================
        //GetTag1() brukes av GetTags()
        //Produserere tagfil for identifisering mot server
        //==============================================================================================================================
        private List<XElement> GetTag1(IEnumerable<IdLink> x, IdAreas idArea)
        {
            List<XElement> returntags = null;

            returntags = x
                        .Where(p => p.idArea == idArea && p.tag1 != null)
                        .GroupBy(p => new { p.tag1 })
                        .Select(p =>
                                new XElement("tag1",
                                    new XAttribute("name", p.Key.tag1),
                                    new XAttribute("language", p.Select(q => q.language).FirstOrDefault()),
                                    GetTag2(x, idArea, p.Key.tag1)
                                    )
                        )
                        .ToList();
            return returntags;
        }
        private List<XElement> GetTag2(IEnumerable<IdLink> x, IdAreas idArea, string tag1)
        {
            List<XElement> returntags = null;
            returntags = x
                        .Where(q => q.idArea == idArea && q.tag1 == tag1 && q.tag2 != null)
                        .GroupBy(q => new { tag2 = q.tag2, totag2 = q.totag2 == null ? "" : q.totag2, altname = q.tag2_alt == null ? "" : q.tag2_alt })
                        .Where(q => q.Key.tag2 != q.Key.totag2)
                        .Select(q =>
                            new XElement("tag2",
                                new XAttribute("name", q.Key.tag2),
                                q.Key.totag2 == "" ? null : new XAttribute("toname", q.Key.totag2),
                                q.Key.altname == "" ? null : new XAttribute("altname", q.Key.altname),
                                GetTag3(x, idArea, tag1, q.Key.tag2)
                                )
                        )
                        .ToList();
            return returntags;
        }
        private List<XElement> GetTag3(IEnumerable<IdLink> x, IdAreas idArea, string tag1, string tag2)
        {
            List<XElement> returntags = null;
            returntags = x
                    .Where(s => s.idArea == idArea && s.tag1 == tag1 && s.tag2 == tag2 && s.tag3 != null)
                    .GroupBy(s => new { tag3 = s.tag3, totag3 = s.totag3 == null ? "" : s.totag3 })
                    .Where(s => s.Key.tag3 != s.Key.totag3)
                    .Select(s =>
                        new XElement("tag3",
                            new XAttribute("name", s.Key.tag3),
                            s.Key.totag3 == "" ? null : new XAttribute("toname", s.Key.totag3)
                            )
                        )
                    .ToList();

            return returntags;
        }
        //==============================================================================================================================
        //GetTags()
        //Produserere tagfil for identifisering mot server
        //==============================================================================================================================
        private XElement GetTags(IdAreas IdArea)
        {

            try
            {
                var x = from e in identificatorelements
                        from l in e.idLinks.Where(p => p.idArea == IdArea && !p.Identified)
                        select l
                        ;
                XElement tags = new XElement("tags",
                    new XAttribute("language", this.Language),
                    GetTag1(x, IdArea)
                    );

                if (IdArea == IdAreas.External)
                {
                    var y = from e in identificatorelements
                            from l in e.idLinks.Where(p =>
                                p.idArea == IdAreas.Internal && !p.Identified
                                && (p.tag1 != "" ? (p.tag1 != _InternalTag) : false)
                                )
                            select l
                        ;
                    tags.Add(
                        y.GroupBy(p => new { p.tag1 })
                        .Select(p =>
                            new XElement("tag1",
                                new XAttribute("name", p.Key.tag1)
                                )));
                }
                if (tags.HasElements)
                {
                    return tags;
                }
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }
        private class XTextObject
        {
            public XText text { get; set; }
            public XTextRange textData { get; set; }
            public List<Markup> markuplinks { get; set; }
            public XTextObject() { }
        }

        private class ProducedLinks
        {
            public string id { get; set; }
            public string ided { get; set; }
        }
        //==============================================================================================================================
        //UpdateSubstituteTag1()
        //Oppdaterere alle internlinker d.v.s tag med tag1="this" og med overliggende tag1 i dokument eller substitute_tag1
        //==============================================================================================================================
        private void GetTegRegexp()
        {
            if (this.InternalTags != null && this.ReferencedTopicsTags != null)
            {

                tagregexps.AddRange(
                    this.InternalTags
                    .Descendants("tag1")
                    .Union(this.ReferencedTopicsTags.Descendants("tag1"))
                    .Elements("name")
                    .Where(p => p.Ancestors().Attributes("regexpName").FirstOrDefault() != null)
                    .Select(p => new
                    {
                        tag1 = p.Value,
                        regexpNames = (string)p.Ancestors().Attributes("regexpName").FirstOrDefault()
                    })
                    .SelectMany(p => p.regexpNames.Split(';').Select(q => new { tag1 = p.tag1, regexpName = q.Trim().ToLower() }))
                    .GroupBy(p => p)
                    .Select(p => new TagRegexp
                    {
                        tag1 = p.Key.tag1,
                        regexpname = p.Key.regexpName
                    })
                );
            }
            else if (this.InternalTags == null && this.ReferencedTopicsTags != null)
            {
                tagregexps.AddRange(
                    this.ReferencedTopicsTags
                    .Descendants("tag1")
                    .Elements("name")
                    .Where(p => p.Ancestors().Attributes("regexpName").FirstOrDefault() != null)
                    .Select(p => new
                    {
                        tag1 = p.Value,
                        regexpNames = (string)p.Ancestors().Attributes("regexpName").FirstOrDefault()
                    })
                    .SelectMany(p => p.regexpNames.Split(';').Select(q => new { tag1 = p.tag1, regexpName = q.Trim().ToLower() }))
                    .GroupBy(p => p)
                    .Select(p => new TagRegexp
                    {
                        tag1 = p.Key.tag1,
                        regexpname = p.Key.regexpName
                    })
                );
            }
            else if (this.InternalTags != null && this.ReferencedTopicsTags == null)
            {

                tagregexps.AddRange(
                        this
                        .InternalTags
                        .Descendants("tag1")
                        .Elements("name")
                        .Where(p => p.Ancestors().Attributes("regexpName").FirstOrDefault() != null)
                        .Select(p => new
                        {
                            tag1 = p.Value,
                            regexpNames = (string)p.Ancestors().Attributes("regexpName").FirstOrDefault()
                        })
                        .SelectMany(p => p.regexpNames.Split(';').Select(q => new { tag1 = p.tag1, regexpName = q.Trim().ToLower() }))
                        .GroupBy(p => p)
                        .Select(p => new TagRegexp
                        {
                            tag1 = p.Key.tag1,
                            regexpname = p.Key.regexpName
                        })
                );

            }
        }
        private void UpdateSubstituteTag1()
        {
            //if ((from IdentificatorElement ie in this.IdentificatorElements
            if ((from ie in identificatorelements
                 where ie
                     .element
                     .DescendantNodesAndSelf()
                     .OfType<XText>()
                     .Where(p => (p.Annotation<XTextRange>() == null ? false : p.Annotation<XTextRange>().tag1 != null))
                     .Count() != 0
                 select ie).ToList().Count() == 0) return;

            XElement tagRegep = null;
            if (this.InternalTags != null && this.ReferencedTopicsTags != null)
            {
                tagRegep = new XElement("tags",
                    this.InternalTags
                    .Descendants("tag1")
                    .Union(this.ReferencedTopicsTags.Descendants("tag1"))
                    .Elements("name")
                    .Where(p => p.Ancestors().Attributes("regexpName").FirstOrDefault() != null)
                    .Select(p => new
                    {
                        tag1 = p.Value,
                        regexpNames = (string)p.Ancestors().Attributes("regexpName").FirstOrDefault()
                    })
                    .SelectMany(p => p.regexpNames.Split(';').Select(q => new { tag1 = p.tag1, regexpName = q.Trim().ToLower() }))
                    .GroupBy(p => p)
                    .Select(p => new XElement("tag1",
                        new XAttribute("name", p.Key.tag1),
                        new XAttribute("regexpname", p.Key.regexpName))
                    ));
            }
            else if (this.InternalTags == null && this.ReferencedTopicsTags != null)
            {
                tagRegep = new XElement("tags",
                    this.ReferencedTopicsTags
                    .Descendants("tag1")
                    .Elements("name")
                    .Where(p => p.Ancestors().Attributes("regexpName").FirstOrDefault() != null)
                    .Select(p => new
                    {
                        tag1 = p.Value,
                        regexpNames = (string)p.Ancestors().Attributes("regexpName").FirstOrDefault()
                    })
                    .SelectMany(p => p.regexpNames.Split(';').Select(q => new { tag1 = p.tag1, regexpName = q.Trim().ToLower() }))
                    .GroupBy(p => p)
                    .Select(p => new XElement("tag1",
                        new XAttribute("name", p.Key.tag1),
                        new XAttribute("regexpname", p.Key.regexpName))
                    ));
            }
            else if (this.InternalTags != null && this.ReferencedTopicsTags == null)
            {
                tagRegep = new XElement("tags",
                    this.InternalTags.Descendants("tag1")
                    .Elements("name")
                    .Where(p => p.Ancestors().Attributes("regexpName").FirstOrDefault() != null)
                    .Select(p => new
                    {
                        tag1 = p.Value,
                        regexpNames = (string)p.Ancestors().Attributes("regexpName").FirstOrDefault()
                    })
                    .SelectMany(p => p.regexpNames.Split(';').Select(q => new { tag1 = p.tag1, regexpName = q.Trim().ToLower() }))
                    .GroupBy(p => p)
                    .Select(p => new XElement("tag1",
                        new XAttribute("name", p.Key.tag1),
                        new XAttribute("regexpname", p.Key.regexpName))
                    ));

            }

            foreach (IdentificatorElement ie in identificatorelements)
            {
                List<XTextObject> textObjects = ie
                            .element
                            .DescendantNodesAndSelf()
                            .OfType<XText>()
                            .Where(p => (p.Annotation<XTextRange>() == null ? false : p.Annotation<XTextRange>().tag1 != null))
                            .Where(p => (p.Annotation<XTextRange>() == null ? -1 : p.Annotation<XTextRange>().id) == ie.id)
                            .Select(p => new XTextObject
                            {
                                text = p,
                                textData = p.Annotation<XTextRange>()

                            })
                             .Select(p => new XTextObject
                             {
                                 text = p.text,
                                 textData = p.textData,
                                 markuplinks = ie.Markups
                                             .Where(q =>
                                                (p.textData.pos.Between(q.start, q.start + q.length, true))
                                             || ((p.textData.pos + p.textData.length).Between(q.start, q.start + q.length, true))
                                             || ((p.textData.pos <= q.start) && ((p.textData.pos + p.textData.length) >= (q.start + q.length)))
                                             )
                                             .ToList()
                             })
                        .Where(p => p.markuplinks.Count() != 0)
                        .OrderBy(p => p.textData.pos)
                        .Select(p => p)
                        .ToList();

                if (textObjects.Count() != 0)
                {
                    (from XTextObject ti in textObjects
                     from ml in ti.markuplinks
                     join idlink in ie.idLinks
                     on ml.id equals idlink.parent
                     join x in tagRegep.Descendants("tag1")
                     on new
                     {
                         t1 = (ti.text.Annotation<XTextRange>().tag1 == null ? "0" : ti.text.Annotation<XTextRange>().tag1).Trim().ToLower(),
                         t2 = idlink.regexpName == null ? "0" : idlink.regexpName.Trim().ToLower()
                     }
                     equals
                      new
                      {
                          t1 = ((string)x.Attributes("name").FirstOrDefault() ?? "-1").Trim().ToLower(),
                          t2 = ((string)x.Attributes("regexpname").FirstOrDefault() ?? "-1").Trim().ToLower()
                      }
                     where idlink.tag1.Trim().ToLower() == _InternalTag.Trim().ToLower()
                     select new IdLinkUpdate
                     {
                         idlink = idlink,
                         tag1 = ti.text.Annotation<XTextRange>().tag1
                     })
                     .ToList()
                     .ForEach(q => q.UpdateIdlink(q.idlink, q.tag1));
                }

            }
        }
        //==============================================================================================================================
        //MarkupLinks()
        //Setter inn linker i xml og produserere diblink segment
        //==============================================================================================================================
        private List<Markup> MarkupLinks()
        {
            List<string> markupIds = new List<string>();
            List<Markup> totMarkup = new List<Markup>();
            try
            {


                foreach (IdentificatorElement ie in identificatorelements)
                {
                    ProducedLinks currentLink = null;
                    List<ProducedLinks> produsedLinks = new List<ProducedLinks>();
                    List<XTextObject> textObjects = ie
                        .element
                        .DescendantNodesAndSelf()
                        .OfType<XText>()
                        .Where(p => (p.Annotation<XTextRange>() == null ? -1 : p.Annotation<XTextRange>().id) == ie.id)
                        .Select(p => new XTextObject
                        {
                            text = p,
                            textData = p.Annotation<XTextRange>()

                        })
                        .Select(p => new XTextObject
                        {
                            text = p.text,
                            textData = p.textData,
                            markuplinks = ie.Markups
                                        .Where(q =>
                                           (p.textData.pos.Between(q.start, q.start + q.length, true))
                                        || ((p.textData.pos + p.textData.length).Between(q.start, q.start + q.length, true))
                                        || ((p.textData.pos <= q.start) && ((p.textData.pos + p.textData.length) >= (q.start + q.length)))
                                        )
                                        .ToList()
                        })
                        .Where(p => p.markuplinks.Count() != 0)
                        .OrderBy(p => p.textData.pos)
                        .Select(p => p)
                        .ToList();

                    #region //Gå gjennom ranger med linker
                    foreach (XTextObject xr in textObjects)
                    {
                        XNode rangeNode = xr.text;
                        XElement range = new XElement("range");
                        string rangeText = xr.text.ToString();
                        int rangeStart = xr.textData.pos;
                        int rangeCursor = xr.textData.pos;
                        int rangeLength = xr.textData.length;
                        string linkText = "";
                        totMarkup.AddRange(xr.markuplinks);
                        foreach (Markup l in xr.markuplinks.OrderBy(p => p.start))
                        {
                            currentLink = null;
                            int linkStart = l.start;
                            int linkLength = l.length;

                            //Linkens start er foran starten på teksten
                            if (linkStart < rangeCursor)
                            {
                                //sett Cursor til start
                                linkStart = rangeCursor;
                                linkLength = linkLength - (rangeCursor - linkStart);
                            }
                            //Hvis linkens star er større enn Cursor legg til text foran link
                            if (linkStart > rangeCursor)
                            {
                                string s = rangeText.Substring((rangeCursor - rangeStart), ((linkStart - rangeStart) - (rangeCursor - rangeStart)));
                                range.Add(new XText(s));
                                rangeCursor = linkStart;
                            }
                            //hvis lengden på linken er lengere enn lengden på rangens tekst forkort linken
                            if (((rangeCursor - rangeStart) + linkLength) > rangeText.Length)
                            {
                                linkLength = rangeText.Length - (rangeCursor - rangeStart);
                            }

                            linkText = rangeText.Substring((rangeCursor - rangeStart), linkLength);
                            rangeCursor = rangeCursor + linkLength;

                            if (xr.text.Parent.Name.LocalName == "a"
                                && (xr.text.Parent.Attribute("class") == null ? "" : xr.text.Parent.Attribute("class").Value) == "xref"
                                && xr.text.Parent.Value == linkText
                                )
                            {
                                range.Add(new XText(linkText));
                            }
                            else
                            {

                                if (produsedLinks.Exists(p => p.id == l.id))
                                {
                                    currentLink = produsedLinks.Where(p => p.id == l.id).First();
                                }

                                if (currentLink != null)
                                {

                                    range.Add(new XElement("diblink",
                                        new XAttribute("refid", currentLink.id),
                                        new XText(linkText)
                                        ));
                                }
                                else
                                {

                                    currentLink = new ProducedLinks { id = l.id, ided = "0" };


                                    var x = from d in xr.text.Ancestors().OfType<XElement>()
                                            join n in nonmarkupnodes on new { name = d.Name.LocalName } equals new { name = n }
                                            select d;


                                    if (x.Count() != 0)
                                    {
                                        range.Add(new XText(linkText));
                                    }
                                    else
                                    {
                                        try
                                        {
                                            //if (linkText == "IFRS 10.DO1–DO12") Debug.Print(l.id);
                                            //IEnumerable<XElement> idlinks = GetIdLinks(l.id, InternalParent);
                                            //if (l.id == "m524") 
                                            //    Debug.Print(l.id);
                                            //XElement links = identyfiedlinks.GetIdLinks(l, linkText);
                                            //IEnumerable<XElement> idlinks = GetIdLinks(l.id);
                                            //List<XElement> le = idlinks.DescendantsAndSelf().ToList();
                                            //if (le.Count() > 5) Debug.Print("xxx");
                                            bool added = false;

                                            if (!added) markupIds.Add(l.id);

                                            range.Add(new XElement("diblink",
                                                new XAttribute("refid", l.id),
                                                new XText(linkText)
                                            ));
                                            added = true;
                                            //////if (links != null)
                                            //////{
                                            //////    if (!added) markupIds.Add(l.id);

                                            //////    if ((string)links.Descendants().Attributes("topic_id").FirstOrDefault() != null)
                                            //////        currentLink.ided = "1";

                                            //////    added = true;
                                            //////    range.Add(new XElement("diblink",
                                            //////        new XAttribute("refid", l.id),
                                            //////        new XAttribute("ided", currentLink.ided),
                                            //////        new XText(linkText)
                                            //////    ));

                                            //////    Links.Add(links);
                                            //////    produsedLinks.Add(currentLink);
                                            //////}
                                            //////else
                                            //////{

                                            //////}
                                            //if (idlinks.FirstOrDefault() != null)
                                            //{
                                            //    if (idlinks.DescendantsAndSelf().Where(p => p.Attribute("topic_id") != null
                                            //        || p.Attribute("segment_id") != null
                                            //        || p.Attribute("bm") != null).Count() == 0)
                                            //    {
                                            //        if (!added) markupIds.Add(l.id);
                                            //        added = true;
                                            //        currentLink.ided = "0";
                                            //        range.Add(new XElement("diblink",
                                            //            new XAttribute("refid", l.id),
                                            //            new XAttribute("ided", currentLink.ided),
                                            //            new XText(linkText)
                                            //        ));
                                            //        Links.Add(
                                            //            new XElement("idlinks",
                                            //            //new XAttribute("version", "2.0"),
                                            //                new XAttribute("id", l.id),
                                            //                new XAttribute("ided", currentLink.ided),
                                            //                new XAttribute("name", l.regexpName),
                                            //                new XAttribute("value", l.value),
                                            //                idlinks
                                            //            )
                                            //        );
                                            //        produsedLinks.Add(currentLink);
                                            //    }
                                            //    else
                                            //    {
                                            //        if (!added) markupIds.Add(l.id);
                                            //        added = true;
                                            //        currentLink.ided = "1";
                                            //        range.Add(new XElement("diblink",
                                            //        new XAttribute("refid", l.id),
                                            //        new XAttribute("ided", currentLink.ided),
                                            //        new XText(linkText)
                                            //        ));

                                            //        Links.Add(
                                            //            new XElement("idlinks",
                                            //                new XAttribute("id", l.id),
                                            //                new XAttribute("ided", currentLink.ided),
                                            //                new XAttribute("name", l.regexpName),
                                            //                new XAttribute("value", l.value),
                                            //                idlinks
                                            //            )
                                            //        );
                                            //        produsedLinks.Add(currentLink);
                                            //    }
                                            //}
                                            if (!added)
                                                range.Add(new XText(linkText));
                                        }
                                        catch
                                        {
                                            range.Add(new XText(linkText));
                                            continue;
                                        }
                                    }

                                }
                            }
                        }
                        if (range.HasElements)
                        {
                            if ((rangeCursor - rangeStart) < rangeText.Length)
                            {
                                string clean = rangeText.Substring((rangeCursor - rangeStart), rangeText.Length - (rangeCursor - rangeStart));
                                range.Add(new XText(clean));

                            }
                            if (((XElement)rangeNode.Parent).Attributes(XNamespace.Xml + "space").Count() == 0)
                            {
                                XAttribute a = new XAttribute(XNamespace.Xml + "space", "preserve");
                                ((XElement)rangeNode.Parent).Add(a);
                            }
                            rangeNode.ReplaceWith(range.Nodes());
                        }

                    }
                    #endregion
                }
                return totMarkup;
            }

            catch (SystemException err)
            {
                return null;
                throw new Exception("MarkupLinks - Error: \r\n" + err.Message);
            }
        }


        private IEnumerable<XElement> GetIdLinks(string parentid)
        {
            try
            {

                return (identyfiedlinks
                        .Where(p => p.parent == parentid
                            && p.id != parentid
                            && !p.Deleted
                            && p.tag1 != null)
                        .OrderBy(p => p.pos)
                        .Select(p => new XElement("link",
                            new XAttribute("id", p.id),
                            p.linkprefix == "" ? null : new XAttribute("prefix", p.linkprefix),
                            new XAttribute("position", p.pos),
                            new XAttribute("lang", p.language),
                            new XAttribute("tag1", p.tag1),
                            new XAttribute("IDQuality", p.IDQuality.ToString()),
                            p.tag2 == null ? null : (p.tag2 == "" ? null : new XAttribute("tag2", p.tag2)),
                            p.tag2_alt == null ? null : (p.tag2_alt == "" ? null : new XAttribute("tag2_alt", p.tag2_alt)),
                            p.tag3 == null ? null : (p.tag3 == "" ? null : new XAttribute("tag3", p.tag3)),
                            p.topic_id == null ? null : (p.topic_id == "" ? null : new XAttribute("topic_id", p.topic_id)),
                            p.segment_id == null
                            ? null
                            : (
                                p.segment_id == ""
                                ? null
                                : (
                                    p.segment_id == (p.topic_id == null ? "" : p.topic_id)
                                    ? null
                                    : new XAttribute("segment_id", p.segment_id)
                                   )
                              )
                            ,
                            p.select_id == null
                            ? null
                            : (
                                p.select_id == ""
                                ? null
                                : (
                                    (
                                           (p.select_id == null ? "" : p.select_id) == (p.topic_id == null ? "" : p.topic_id)
                                        || (p.select_id == null ? "" : p.select_id) == (p.segment_id == null ? "" : p.segment_id)
                                    )
                                    ? null
                                    : new XAttribute("bm", p.select_id)
                                  )
                               ),
                            //p.title == null ? null : (p.title == "" ? null : new XAttribute("title", p.title)),
                            GetIdLinks(p.id)
                        )).ToList()
                    );
            }
            catch (SystemException err)
            {
                throw new Exception("GetIdLinks - Error: \r\n" + err.Message);
            }
        }
        private XElement IdentifyTags(string base64Parameter, string language)
        {
            XElement returnValue = null;
            try
            {

                if (this.Local)
                {
                    XMLCommunicate cx = new XMLCommunicate();
                    string sourceRegexp = cx.Execute(DatabaseName + "." + SQLProcedure + " '" + base64Parameter + "', '" + language + "'");
                    XDocument d = XDocument.Parse(sourceRegexp);
                    returnValue = d.Descendants("tags").FirstOrDefault();
                }
                else
                {
                    using (SqlConnection sqlConnRead = new SqlConnection("context connection=true"))
                    {
                        SqlCommand StoredProcedureCommand = new SqlCommand(SQLProcedure, sqlConnRead);
                        StoredProcedureCommand.CommandType = CommandType.StoredProcedure;
                        SqlParameter myParm1 = StoredProcedureCommand.Parameters.Add("@base64", SqlDbType.VarChar);
                        myParm1.Value = base64Parameter;
                        SqlParameter myParm2 = StoredProcedureCommand.Parameters.Add("@lang", SqlDbType.VarChar, 2);
                        myParm2.Value = language;
                        sqlConnRead.Open();

                        XmlReader r = StoredProcedureCommand.ExecuteXmlReader();
                        XDocument d = XDocument.Load(r);
                        r.Close();
                        returnValue = d.Descendants("result").FirstOrDefault();
                    }
                }
            }
            catch (SystemException err)
            {
                throw new Exception("IdentifyTags - Error: \r\n" + err.Message);
            }
            return returnValue;
        }
        private class XMLCommunicate
        {
            private string url = "http://t-adminservice3admin.dib.no/service.asmx";

            public string Execute(string strCommand)
            {
                try
                {
                    string SQLxml = "";

                    SQLxml = SQLxml + "<?xml version='1.0' encoding='utf-8'?>";
                    SQLxml = SQLxml + "<soap:Envelope xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>";
                    SQLxml = SQLxml + "<soap:Body>";
                    SQLxml = SQLxml + "<GetData2 xmlns='http://tempuri.org/'>";
                    SQLxml = SQLxml + "<xml>";
                    SQLxml = SQLxml + "<XMLString xsi:type=\"xsd:string\">";
                    SQLxml = SQLxml + "<![CDATA[";
                    SQLxml = SQLxml + "<root xmlns:sql='urn:schemas-microsoft-com:xml-sql' >";
                    SQLxml = SQLxml + "<sql:query>";
                    SQLxml = SQLxml + strCommand;
                    SQLxml = SQLxml + "</sql:query>";
                    SQLxml = SQLxml + "</root>";
                    SQLxml = SQLxml + "]]>";
                    SQLxml = SQLxml + "</XMLString>";
                    SQLxml = SQLxml + "</xml>";
                    SQLxml = SQLxml + "</GetData2>";
                    SQLxml = SQLxml + "</soap:Body></soap:Envelope>";




                    HttpWebRequest request = (HttpWebRequest)
                                            HttpWebRequest.Create(url);

                    String xmlString = SQLxml;
                    UTF8Encoding encoding = new UTF8Encoding();

                    byte[] bytesToWrite = encoding.GetBytes(xmlString);
                    request.Timeout = 240000;
                    request.Method = "POST";
                    request.ContentLength = bytesToWrite.Length;
                    request.Headers.Add("SOAPAction: \"http://tempuri.org/GetData2\""); //You need to change this
                    request.ContentType = "text/xml; charset=utf-8";

                    Stream newStream = request.GetRequestStream();
                    newStream.Write(bytesToWrite, 0, bytesToWrite.Length);
                    newStream.Close();

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);

                    string responseFromServer = reader.ReadToEnd();
                    return responseFromServer;
                }
                catch (SystemException err)
                {
                    throw new Exception("XMLCommunication Execute - Error: \r\n" + err.Message);
                }
            }
        }

    }
}
