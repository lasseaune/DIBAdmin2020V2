using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;


namespace DIB.RegExp.ExternalStaticLinks
{

    public class ExternalStaticLinksEx1
    {
        public int LinkCounter = 0;
        public int MatchCounter = 0;
        private int _SERVER = 0;
        private XElement _RETURNTYPE = null;
        Regex _regit = new Regex("([^\\x00-\\x7E\\xA1-\\xFF\\x152\\x153\\x160\\x161\\x178\\x192])");
        
        public ExternalStaticLinksEx1(int server)
        {
            _SERVER = server;
        }
        public string _lang = "no";
        private XElement _actions = null;
        private ExternalStaticLinksData _data = new ExternalStaticLinksData();
        public class iObject
        {
            public string tag = "";
            public string lang = "";
            public string name = "";
            public string id = "";
            public string text = "";
            public List<iObject> n = new List<iObject>();
        }

        public List<iObject> _lObjects = new List<iObject>();

        public List<rQuery> _list_rQuery = new List<rQuery>();

        public class rQuery
        {
            public string name { get; set; }
            public Regex query { get; set; }
        }



        public class linkInfo
        {
            public string topic_id { get; set; }
            public string topic_name { get; set; }
            public string bm { get; set; }
            public string bm_name { get; set; }
        }

        public class actionObject
        {
            public string text { get; set; }
            public string name { get; set; }
            public int type { get; set; }
            public int start { get; set; }
            public int offset { get; set; }
            public string tag1 { get; set; }
            public string tag2 { get; set; }
            public Match mTop { get; set; }
            public Match m { get; set; }
            public List<linkInfo> li { get; set; }
            public Capture c { get; set; }
            public XElement topics { get; set; }
            public actionObject(string text, Match m, int start)
            {
                this.text = text;
                this.mTop = m;
                this.m = m;
                this.type = 0;
                this.start = start;
                this.offset = 0;

            }
            public actionObject(actionObject ao, Match m)
            {
                this.text = ao.text;
                this.tag1 = ao.tag1;
                this.tag2 = ao.tag2;
                this.mTop = ao.mTop;
                this.li = ao.li;
                if (ao.m != null)
                    this.offset = ao.offset + ao.m.Index;
                else
                    this.offset = ao.offset + c.Index;
                this.start = 0;
                this.m = m;
                this.type = 0;
                this.topics = ao.topics;
            }
            public actionObject(actionObject ao, Capture c, string name)
            {
                this.text = ao.text;
                this.tag1 = ao.tag1;
                this.tag2 = ao.tag2;
                this.mTop = ao.mTop;
                this.offset = ao.offset;
                this.start = ao.start;
                this.c = c;
                this.type = 1;
                this.name = name;
                this.li = ao.li;
                this.topics = ao.topics;
            }

        }

        public class iObjectEx
        {
            private string topic_id { get; set; }
            private List<iAbbrev> abbrevs = new List<iAbbrev>();
            private List<iAbbrev> bookmarks = new List<iAbbrev>();
        }

        public class iAbbrev
        {
            public string abbrev { get; set; }
        }

        public class iBookmarks
        {
            public string tag2 = "";
            public string bm = "";
        }

        private List<iObjectEx> _iObjectEx = null;
        private XElement _iObjectXML = null;

        private string BuildTag(actionObject ao, string tags, string trim)
        {
            string returnValue = "";
            //foreach (string s in tags.Split('|'))
            int n = tags.Split('|').Count();
            for (int i = 0; i < n; i++)
            {
                string s = tags.Split('|').ElementAt(i);
                if (s.StartsWith("$") && s.EndsWith("$"))
                {
                    returnValue = returnValue + Regex.Replace(s, @"\$", "");
                }
                else
                {
                    if (ao.m.Groups[s].Success)
                    {
                        if (trim == "1")
                        {
                            returnValue = returnValue + ao.m.Groups[s].Value.Trim().ToLower().Replace(" ", "");
                        }
                        else
                        {
                            returnValue = returnValue + ao.m.Groups[s].Value.Trim().ToLower().TrimEnd().TrimStart();
                        }
                    }
                }
            }
            return returnValue;
        }


        private void ExecuteActions(XElement action, ref actionObject ao, ref List<xRange> container)
        {
            try
            {
                //foreach (XElement a in action.Elements())
                int n = action.Elements().Count();
                for (int i = 0; i < n; i++)
                {
                    XElement a = action.Elements().ElementAt(i);
                    switch (a.Name.LocalName)
                    {
                        case "action":
                        case "querys":
                            break;
                        case "runaction":
                            if (action.Ancestors("actions").Last().Elements("action").Where(p => p.Attribute("name").Value == a.Attribute("name").Value).Count() != 0)
                            {
                                a = action.Ancestors("actions").Last().Elements("action").Where(p => p.Attribute("name").Value == a.Attribute("name").Value).First();
                                ExecuteActions(a, ref ao, ref container);
                            }
                            break;
                        case "match":
                            if (ActionMatch(a, ref ao, ref container))
                                return;
                            else
                                break;
                        case "get":
                            ActionGet(a, ref ao, ref container);
                            return;
                        case "mark":
                            ActionMark(a, ref ao, ref container);
                            return;
                        case "foreach":
                            ActionForeach(a, ref ao, ref container);
                            break;
                        case "if":
                            ActionIf(a, ref ao, ref container);
                            return;
                        default: return;
                    }
                }
            }
            catch
            {
            }
        }

        private void ActionIf(XElement a, ref actionObject ao, ref List<xRange> container)
        {
            try
            {
                if (a.Attribute("tag") == null) return;
                if (a.Attribute("function") == null) return;
                if (a.Attribute("function").Value == "replace")
                {
                    if (a.Attribute("valuein") == null || a.Attribute("valueout") == null) return;
                    if (a.Attribute("tag") == null) return;
                    string result = "";
                    switch (a.Attribute("tag").Value)
                    {
                        case "tag1":
                            result = Regex.Replace(ao.tag1, a.Attribute("valuein").Value.ToString(), a.Attribute("valueout").Value.ToString(), RegexOptions.IgnoreCase);
                            ActionIdentify(a, ref ao, ref container, "tag1", result.ToLower());
                            break;
                        case "tag2":
                            result = Regex.Replace(ao.tag2, a.Attribute("valuein").Value.ToString(), a.Attribute("valueout").Value.ToString(), RegexOptions.IgnoreCase);
                            ActionIdentify(a, ref ao, ref container, "tag2", result.ToLower());
                            break;
                    }
                }
            }
            catch
            {
            }
        }

        private bool ActionMatch(XElement a, ref actionObject ao, ref List<xRange> container)
        {
            bool returnValue = false;
            try
            {
                if (ao.type == 0)
                {
                    if (a.Attribute("name") == null) return true;
                    bool found = true;
                    int n = a.Attribute("name").Value.Split('|').Count();

                    for (int i = 0; i < n; i++)
                    {
                        string s = a.Attribute("name").Value.Split('|').ElementAt(i);
                        if (!ao.m.Groups[s].Success)
                        {
                            found = false;
                        }
                    }
                    if (found)
                    {
                        returnValue = true;
                        ao.name = a.Attribute("name").Value;
                        ExecuteActions(a, ref ao, ref container);
                    }
                }
                else if (ao.type == 1)
                {
                }
            }
            catch
            {
            }
            return returnValue;
        }

        private string GetLovDate(ref actionObject ao, string values)
        {
            //values="type=lov,for,res|date=lovdate|day=day|month=month|year=year|number=number"
            string returnValue = "";
            string type = "";
            string day = "";
            string month = "";
            string year = "";
            string number = "";
            DateTime? date = null;
            int n = values.Split('|').Count();
            //foreach (string s in values.Split('|'))
            for (int i = 0; i < n; i++)
            {
                string s = values.Split('|').ElementAt(i);
                string att = s.Split('=').ElementAt(0);
                string value = s.Split('=').ElementAt(1);
                switch (att)
                {
                    case "type":
                        foreach (string t in value.Split(','))
                        {
                            if (ao.m.Groups[t].Success)
                            {
                                switch (t)
                                {
                                    case "typelov": type = "lov"; break;
                                    case "typefor": type = "for"; break;
                                    case "typeres": type = "for"; break;
                                }
                                if (type != "")
                                    break;
                            }
                        }
                        break;
                    case "date":
                        try
                        {
                            if (ao.m.Groups[value].Success)
                                date = DateTime.Parse(ao.m.Groups[value].Value.ToString());
                        }
                        catch
                        {

                        }
                        break;
                    case "day":
                        if (ao.m.Groups[value].Success)
                            day = ao.m.Groups[value].Value.ToString();
                        break;
                    case "month":
                        if (ao.m.Groups[value].Success)
                            month = ao.m.Groups[value].Value.ToString();
                        break;
                    case "year":
                        if (ao.m.Groups[value].Success)
                            year = ao.m.Groups[value].Value.ToString();
                        break;
                    case "number":
                        if (ao.m.Groups[value].Success)
                            number = ao.m.Groups[value].Value.ToString();
                        break;
                }

            }

            if (date.HasValue)
            {
                if (type != "")
                {
                    returnValue = type + "-" + String.Format("{0:yyyy-MM-dd}", date) + "-" + number;
                }
                else
                {
                    returnValue = String.Format("{0:yyyy-MM-dd}", date) + "-" + number;
                }
            }
            else
            {
                return returnValue;
            }

            return returnValue;
        }

        private bool SetTag(ref actionObject ao, string tag, string value)
        {
            bool returnValue = false;
            if (value == "") return returnValue;
            switch (tag)
            {
                case "tag1":
                    ao.tag1 = value;
                    if (IdentifyTag1(ref ao))
                    //if (IdentifyTag1Ex(ref ao))
                    {
                        return true;
                    }
                    break;
                case "tag2":
                    ao.tag2 = value;
                    if (IdentifyTag2(ref ao))
                    //if (IdentifyTag2Ex(ref ao))
                    {
                        return true;
                    }
                    break;
                default: break;
            }

            return returnValue;
        }
        private void ActionGet(XElement a, ref actionObject ao, ref List<xRange> container)
        {
            try
            {
                string trim = a.Attribute("trim") == null ? "" : a.Attribute("trim").Value;
                if (a.Attribute("tag") == null || a.Attribute("groups") == null) return;
                string aoName = ao.name;

                string result = "";
                if (ao.type == 0)
                {
                    if (a.Attribute("function") != null)
                    {
                        if (a.Attribute("values") == null) return;
                        switch (a.Attribute("function").Value)
                        {
                            case "GetLovDate":
                                result = GetLovDate(ref ao, a.Attribute("values").Value); break;
                            default: break;
                        }
                    }
                    else
                    {
                        result = BuildTag(ao, a.Attribute("groups").Value, trim);
                    }
                }
                else if (ao.type == 1 && a.Attribute("groups").Value.Split('|').Where(p => p == aoName.ToString()).Count() != 0)
                {
                    foreach (string t in a.Attribute("groups").Value.Split('|'))
                    {
                        if (t == aoName)
                        {
                            if (trim == "1")
                            {
                                result = result + ao.c.Value.Trim().ToLower().ToString().Replace(" ", "");
                            }
                            else
                            {
                                result = result + ao.c.Value.Trim().ToLower().ToString().TrimEnd().TrimStart();
                            }
                        }
                        else if (t.StartsWith("$") && t.EndsWith("$"))
                        {
                            result = result + t.Replace("$", "");
                        }
                    }
                    result = result.Trim().ToLower().TrimEnd().TrimStart();
                }


                ActionIdentify(a, ref ao, ref container, a.Attribute("tag").Value.ToString(), result);
            }
            catch
            {
            }
        }

        private void ActionMark(XElement a, ref actionObject ao, ref List<xRange> container)
        {
            try
            {
                if ((ao.li != null ? ao.li.Count() : 0) != 0)
                {

                    int start = 0;
                    int end = 0;
                    if (ao.type == 0)
                    {
                        start = ao.offset + ao.m.Groups[a.Attribute("groups").Value.Split('|').First()].Index;
                        end = ao.offset + ao.m.Groups[a.Attribute("groups").Value.Split('|').Last()].Index + ao.m.Groups[a.Attribute("groups").Value.Split('|').Last()].Length;
                    }
                    else if (ao.type == 1)
                    {
                        start = ao.offset + ao.c.Index;
                        end = ao.offset + ao.c.Index + ao.c.Length;
                    }

                    if (container.Count() != 0)
                    {
                        List<xRange> rl = container.Where(p => p.start <= start
                                && p.end >= end).ToList();
                        xRange range = null;
                        if (rl.Count() == 1)
                        {
                            range = rl.ElementAt(0);
                        }
                        else if (rl.Count() > 1)
                        {
                            string rangeId = rl.Min(p => Convert.ToInt32(p.n).ToString()
                                    + "-" + rl.Min(q => Convert.ToInt32(q.n).ToString()));
                            if (container.Where(p => p.n == rangeId).Count() != 0)
                            {
                                range = container.Where(p => p.n == rangeId).First();
                            }
                            else
                            {
                                range = new xRange(rangeId, start, end);
                                container.Add(range);
                            }

                        }
                        if (range != null)
                        {
                            xLink link = new xLink(start, end - start);
                            int n = ao.li.Count();
                            for (int i = 0; i < n; i++)
                            {
                                linkInfo li = ao.li[i];
                                if (li.topic_id != "" && (li.bm == null ? "" : li.bm) != "")
                                {
                                    link.references.Add(new xReference(li.topic_id, li.bm, li.topic_name));
                                }
                                else if (li.topic_id != "" && (li.bm == null ? "" : li.bm) == "")
                                {
                                    link.references.Add(new xReference(li.topic_id, li.topic_name));
                                }

                            }
                            if (link.references.Count != 0)
                                range.links.Add(link);
                        }

                    }
                    ao.start = end;
                }
            }
            catch (SystemException e)
            {
            }
        }

        private void ActionForeach(XElement a, ref actionObject ao, ref List<xRange> container)
        {
            try
            {
                if (a.Attribute("querytype").Value == "group")
                {
                    int n = ao.m.Groups[a.Attribute("groups").Value].Captures.Count;
                    List<Capture> cl = new List<Capture>();
                    for (int i = 0; i < n; i++)
                    {

                        Capture c = ao.m.Groups[a.Attribute("groups").Value].Captures[i];
                        if (!cl.Exists(p => p.Index == c.Index && p.Length == c.Length))
                        {
                            actionObject su = new actionObject(ao, c, a.Attribute("groups").Value);
                            su.offset = ao.offset;
                            su.start = ao.start;
                            su.m = ao.m;
                            ExecuteActions(a, ref su, ref container);
                            ao.start = su.start;
                        }
                        cl.Add(c);
                    }
                }
                else if (a.Attribute("querytype").Value == "subquery")
                {
                    Regex qu = _list_rQuery.Where(p => p.name == a.Attribute("name").Value).First().query;
                    string searchText = ao.m.Groups[a.Attribute("groups").Value].Value;
                    MatchCollection mc = qu.Matches(searchText);
                    int n = mc.Count;
                    for (int i = 0; i < n; i++)
                    {
                        Match m = mc[i];
                        actionObject su = new actionObject(ao, m);
                        su.offset = ao.offset + ao.m.Groups[a.Attribute("groups").Value].Index;
                        su.start = ao.start;
                        ExecuteActions(a, ref su, ref container);
                        ao.start = su.start;
                    }
                }
            }
            catch
            {
            }
        }


        private static string HEXREPL(Match m)
        {
            string returnValue = "";
            try
            {
                int x = Convert.ToChar(m.ToString());
                if (x > 32767)
                {
                    x = (x - 65536);
                    x = x & 0xFF;
                    char a = (char)x;
                    returnValue = a.ToString();
                }

                switch (x)
                {
                    case 160:
                    case 8201:
                        returnValue = " ";
                        break;
                    case 8208:
                    case 8209:
                    case 8210:
                    case 8211:
                    case 8212:
                    case 8213:
                        returnValue = "-";
                        break;
                    case 171:
                    case 187:
                    case 8216:
                    case 8217:
                    case 8220:
                    case 8221:
                        {
                            char a = (char)34;
                            returnValue = a.ToString();
                        }
                        break;
                    default:
                        returnValue = m.ToString();
                        break;
                }
            }
            catch (SystemException e)
            {
                returnValue = m.ToString();
            }
            return returnValue;
        }

        private class xReference
        {
            public string topic_id { get; set; }
            public string bm { get; set; }
            public string name { get; set; }
            public xReference(string topic_id, string bm, string name)
            {
                this.topic_id = topic_id;
                this.bm = bm;
                this.name = name;
            }
            public xReference(string topic_id, string name)
            {
                this.topic_id = topic_id;
                this.bm = null;
                this.name = name;
            }

        }
        private class xLink
        {
            public int start { get; set; }
            public int length { get; set; }
            public List<xReference> references = new List<xReference>();
            public xLink(int start, int length)
            {
                this.start = start;
                this.length = length;
            }
        }

        private class xRange
        {
            public string n { get; set; }
            public int start { get; set; }
            public int end { get; set; }
            public XNode node { get; set; }
            public List<xLink> links = new List<xLink>();
            public xRange(string n, int start, int end)
            {
                this.n = n;
                this.start = start;
                this.end = end;
            }
        }

        private List<xRange> GetXTextInRange(XElement e)
        {
            List<xRange> returnValue = new List<xRange>();
            int n = 0;
            int pos = 0;
            foreach (XText t in e.DescendantNodes().OfType<XText>())
            {
                xRange r = new xRange(n.ToString(), pos, pos + t.ToString().Length);
                r.node = t;
                returnValue.Add(r);
                pos = pos + t.ToString().Length;
                n++;
            }
            return returnValue;
        }

        private List<xRange> GetRanges(XNode n, ref string textNode)
        {
            List<xRange> rN = null;
            if (n.NodeType == XmlNodeType.Element)
            {
                textNode = ((XElement)n).DescendantNodes().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate();
                textNode = _regit.Replace(textNode, new MatchEvaluator(HEXREPL), -1);
                rN = GetXTextInRange((XElement)n);
            }
            else if (n.NodeType == XmlNodeType.Text)
            {
                textNode = n.ToString();
                rN = new List<xRange>();
                xRange nRange = new xRange("0", 0, textNode.Length);
                nRange.node = n;
                rN.Add(nRange);
            }
            return rN;
        }

        private XElement GetMatches(List<xRange> rN)
        {
            XElement returnValue = null;
            List<xRange> Ranges = rN
                            .Where(p => p.links.Count() != 0 && p.n.IndexOf("-") == -1)
                            .OrderBy(p => p.start).ToList();

            List<xRange> splitRanges = rN
                .Where(p => p.links.Count() != 0 && p.n.IndexOf("-") != -1)
                .OrderBy(p => p.start).ToList();

            for (int iR = 0; iR < Ranges.Count(); iR++)
            {
                xRange range = Ranges.ElementAt(iR);
                bool found = false;
                for (int iSR = 0; iSR < splitRanges.Count(); iSR++)
                {
                    xRange splitRange = splitRanges.ElementAt(iSR);
                    string nRange = splitRange.n;
                    if (nRange.Split('-').Where(p => p == range.n).Count() == 0)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    XNode nF = rN.Where(p => p.n == range.n).First().node;
                    string nFText = nF.ToString();
                    int nFStart = 0;
                    int rangeStart = range.start;
                    XElement nContainer = new XElement("ncontainer");

                    foreach (xLink link in range.links.OrderBy(p => p.start))
                    {
                        int linkStart = link.start;
                        int linkLength = link.length;
                        if ((linkStart - rangeStart) < nFText.Length)
                        {
                            nContainer.Add(new XText(nFText.Substring(nFStart, (linkStart - rangeStart) - nFStart)));
                        }
                        string linkText = nFText.Substring(linkStart - rangeStart, linkLength);
                        //Debug.Print("link: " + linkText);
                        CreateLinks(nContainer, link, linkText);
                        //nContainer.Add(link.Nodes());
                        nFStart = (linkStart - rangeStart) + linkLength;
                    }
                    if (nContainer.HasElements)
                    {
                        if (nFStart < nFText.Length)
                            nContainer.Add(new XText(nFText.Substring(nFStart, nFText.Length - nFStart)));

                        if (((XElement)nF.Parent).Attributes(XNamespace.Xml + "space").Count() == 0)
                        {
                            XAttribute a = new XAttribute(XNamespace.Xml + "space", "preserve");
                            ((XElement)nF.Parent).Add(a);
                        }

                        nF.ReplaceWith(nContainer.Nodes());
                        returnValue = new XElement("x", nContainer.Nodes());
                    }

                }
            }
            return returnValue;
        }

        private XElement GetReferanceString(XNode n)
        {
            XElement returnValue = new XElement("x", n);
            if (_actions.Attribute("query") == null) return returnValue;

            try
            {
                if (!(n.NodeType == XmlNodeType.Element || n.NodeType == XmlNodeType.Text)) return returnValue;
                string textNode = "";
                List<xRange> rN = GetRanges(n, ref textNode);
                if ((rN == null ? 0 : rN.Count()) == 0) return returnValue;

                MatchCollection mc = _list_rQuery.Where(p => p.name == _actions.Attribute("query").Value).First().query.Matches(textNode);
                int start = 0;
                int count = mc.Count;
                if (count != 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        Match m = mc[i];
                        //Debug.Print("Match: " + m.Value);
                        MatchCounter++;
                        actionObject ao = new actionObject(textNode, m, start);
                        ExecuteActions(_actions, ref ao, ref rN);
                        start = ao.start;

                    }
                    if (rN
                        .Where(p => p.links.Count() != 0).Count() != 0)
                    {
                        XElement matches = GetMatches(rN);
                        if (matches == null)
                            returnValue = matches;
                    }
                }
            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("GetReferanceString Error: {0}", e.Message.ToString()));
            }
            return returnValue;
        }

        private void CreateLinks(XElement container, xLink link, string text)
        {
            try
            {
                if (_RETURNTYPE == null)
                {
                    int n = link.references.Count();
                    if (n == 1)
                    {
                        xReference r = link.references.ElementAt(0);
                        XElement a = new XElement("a"
                            , text
                            , new XAttribute("class", "xref")
                            , new XAttribute("data-tid", r.topic_id));

                        if (r.bm != null)
                            a.Add(new XAttribute("data-bm", r.bm));

                        container.Add(a);
                        LinkCounter++;

                    }
                    else if (n > 1)
                    {

                        string id = System.Guid.NewGuid().ToString();
                        XElement a = new XElement("a"
                            , text
                            , new XAttribute("class", "xref")
                            , new XAttribute("data-xreflist", id));

                        container.Add(a);
                        LinkCounter++;

                        XElement span = new XElement("span"
                            , new XAttribute("class", "hn xreflist")
                            , new XAttribute("id", id));

                        for (int i = 0; i < n; i++)
                        {
                            xReference r = link.references.ElementAt(i);
                            if (r.topic_id != "")
                            {
                                XElement slink = new XElement("a", r.name
                                        , new XAttribute("class", "xref")
                                        , new XAttribute("data-tid-list", r.topic_id));
                                if (r.bm != null)
                                    slink.Add(new XAttribute("data-bm", r.bm));
                                span.Add(slink);

                            }
                        }
                        container.Add(span);
                    }

                }
                else
                {
                    int n = link.references.Count();
                    for (int i = 0; i < n; i++)
                    {
                        xReference r = link.references.ElementAt(i);
                        if (r.topic_id != "")
                        {
                            XElement rlink = new XElement("link"
                                    , new XAttribute("topic_id", r.topic_id)
                                    , new XAttribute("title", r.name));
                            if (r.bm != null)
                                rlink.Add(new XAttribute("bm", r.bm));
                            if (_RETURNTYPE.Elements("link").Where(p => p == rlink).Count() == 0)
                                _RETURNTYPE.Add(rlink);
                        }
                    }
                }
            }
            catch
            {

            }
        }


        //private void GetNodes(XElement element, XElement copy, ref string text, ref int start)
        //{
        //    try
        //    {
        //        int m = element.Nodes().Count();
        //        for (int i = 0; i < m; i++)
        //        {
        //            XNode n = element.Nodes().ElementAt(i);
        //            if (n.NodeType == XmlNodeType.Element)
        //            {
        //                XElement sElement = (XElement)n;
        //                XElement elCopy = new XElement(sElement.Name.ToString(), sElement.Attributes());
        //                copy.Add(elCopy);
        //                GetNodes(sElement, elCopy, ref text, ref start);
        //            }
        //            else if (n.NodeType == XmlNodeType.Text)
        //            {
        //                text = text + n.ToString();
        //                copy.Add(new XElement("text",
        //                    new XAttribute("start", start),
        //                    new XAttribute("end", (start + n.ToString().Length).ToString())));
        //                start = start + n.ToString().Length;
        //            }
        //        }

        //    }
        //    catch (SystemException err)
        //    {
        //    }
        //}


        private void GetReferances(XElement e)
        {
            try
            {
                int m = e.Nodes().Count();

                //foreach (XNode n in e.Nodes())
                for (int i = 0; i < m; i++)
                {
                    XNode n = e.Nodes().ElementAt(i);
                    if (n.NodeType == XmlNodeType.Element)
                    {
                        XElement test = (XElement)n;
                        if (!test.Name.ToString().EndsWith("ref")
                            && !((test.Name.ToString() == "title") && (test.Attributes("type").Count() != 0 ? test.Attribute("type").Value : "") != "enkeltsaker-tittel")
                            && !(test.Name.ToString() == "a"
                                && (test.Attributes("type").Count() != 0 ?
                                    test.Attribute("type").Value : "") == "xref")
                            && !(test.Name.ToString() == "a"
                                && test.Attributes("href").Count() != 0))
                        {
                            if (test.DescendantNodes().OfType<XText>().Count() == 1)
                            {
                                XNode n1 = test.DescendantNodes().OfType<XText>().ElementAt(0);
                                GetReferanceString(n1);
                            }
                            else if (test.Name.ToString() == "p")
                            {
                                GetReferanceString(n);
                            }
                            else if (test.Name.ToString() == "a"
                                && (test.Attributes("type").Count() != 0 ?
                                    test.Attribute("type").Value : "") != "xref")
                            {
                                GetReferanceString(n);
                            }
                            else
                                GetReferances((XElement)n);
                        }
                        else
                        {
                        }
                    }
                    else if (n.NodeType == XmlNodeType.Text)
                    {

                        XElement test = GetReferanceString(n);
                        if (test.Nodes().Count() != 1)
                        {
                            m = m + test.Nodes().Count() - 1;
                            i = i + test.Nodes().Count() - 1;
                        }
                    }
                }
            }
            catch (SystemException err)
            {
                throw new Exception(string.Format("GetReferances Error: {0}", err.Message.ToString()));
            }
        }


        //Startprosedyren - normalt
        public XElement GetExternalStaticLinks(XElement document, string language)
        {
            try
            {
                document = GetDocumentParts(document, language);
            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("GetExternalStaticLinks Error: {0}", e.Message.ToString()));
            }
            return document;
        }

        //Startprosedyren - test henter ut missing

        public XElement GetExternalStaticLinks(XElement document, string language, ref XElement missing)
        {
            try
            {
                document = GetDocumentParts(document, language);

                missing = UpdateMissing();
            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("GetExternalStaticLinks Error: {0}", e.Message.ToString()));
            }

            return document;
        }

        public void GetStaticLinks(XElement document, string language, ref XElement ReturnType)
        {
            try
            {
                _RETURNTYPE = ReturnType;
                GetDocumentParts(document, language);
            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("GetStaticLinks Error: {0}", e.Message.ToString()));
            }
        }

        public void GetLinksInText(XElement document, string language, ref XElement ReturnType)
        {
            try
            {
                _RETURNTYPE = ReturnType;
                GetDocumentParts(document, language);
            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("GetStaticLinks Error: {0}", e.Message.ToString()));
            }
        }


        public bool SetActionsList(XElement actions)
        {
            bool returnValue = false;
            try
            {
                XElement gXML = actions;
                if (gXML.DescendantsAndSelf("actions").Count() != 0)
                {
                    _actions = gXML.DescendantsAndSelf("actions").First();
                    returnValue = true;

                }
            }
            catch (SystemException e)
            {

            }
            return returnValue;
        }

        public bool SetRegexpList(Dictionary<string, string> regexps)
        {
            bool returnValue = false;
            try
            {
                List<rQuery> rl = _data.LoadTokenRegExp(regexps);
                _list_rQuery = rl;
                returnValue = true;
            }
            catch (SystemException e)
            {

            }
            return returnValue;
        }

        private XElement GetDocumentParts(XElement document, string language)
        {
            _iObjectEx = new List<iObjectEx>();
            _iObjectXML = new XElement("topics");
            try
            {
                _lang = language;

                if (_actions == null)
                {
                    XElement gXML = _data.LoadGlobalXML("_regexp_actions", _SERVER);
                    if (gXML.Descendants("actions").Count() != 0)
                    {
                        _actions = gXML.Descendants("actions").First();
                    }
                }

                if (_list_rQuery.Count() == 0)
                {
                    if (_actions.Elements("querys").Count() != 0)
                    {
                        _list_rQuery = _data.LoadTokenRegExp(_SERVER, _actions.Elements("querys").First());
                    }
                    else
                    {
                        _list_rQuery = _data.LoadTokenRegExp(_SERVER);
                    }
                }

                if (_actions != null && _list_rQuery.Count() != 0)
                {
                    if (document.DescendantsAndSelf("level").Count() != 0)
                    //finn intextlinking(dibzoom) for 5,6,7 typene
                    {
                        int n = document.DescendantsAndSelf("level").Count();
                        //foreach (XElement e in document.DescendantsAndSelf("level"))
                        for (int i = 0; i < n; i++)
                        {
                            XElement e = document.DescendantsAndSelf("level").ElementAt(i);
                            GetReferances(e);
                        }
                    }
                    else if (document.DescendantsAndSelf("content").Count() != 0)
                    //finn intextlinking(dibzoom) for 2 typene
                    {
                        int n = document.DescendantsAndSelf("content").Count();

                        //foreach (XElement e in document.DescendantsAndSelf("content"))
                        for (int i = 0; i < n; i++)
                        {
                            XElement e = document.DescendantsAndSelf("content").ElementAt(i);
                            GetReferances(e);
                        }
                    }
                    //foreach (XElement e in document.Descendants("document"))
                    else if (document.DescendantsAndSelf("document").Count() != 0)
                    {
                        int n = document.DescendantsAndSelf("document").Count();
                        //foreach (XElement e in document.DescendantsAndSelf("level"))
                        for (int i = 0; i < n; i++)
                        {
                            XElement e = document.DescendantsAndSelf("document").ElementAt(i);
                            GetReferances(e);
                        }
                    }

                }
            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("GetDocumentParts Error: {0}", e.Message.ToString()));
            }
            return document;

        }

        private bool ActionIdentify(XElement a, ref actionObject ao, ref List<xRange> container, string tag, string value)
        {
            bool returnValue = true;
            try
            {
                if (SetTag(ref ao, tag, value))
                {
                    if (a.Elements("true").Count() != 0)
                    {
                        a = a.Elements("true").First();
                        ExecuteActions(a, ref ao, ref container);
                    }
                    else
                    {
                        ExecuteActions(a, ref ao, ref container);
                    }
                }
                else
                {
                    if (a.Elements("false").Count() != 0)
                    {
                        a = a.Elements("false").First();
                        ExecuteActions(a, ref ao, ref container);
                    }
                }
            }
            catch (SystemException e)
            {
                returnValue = false;
            }
            return returnValue;
        }

        private bool IdentifyTag1(ref actionObject ao)
        {
            bool returnValue = false;
            try
            {
                string tag1 = ao.tag1;
                List<linkInfo> _newli = new List<linkInfo>();
                bool found = false;
                int n = _lObjects.Where(p => p.tag == tag1 && p.lang == _lang).Count();
                for (int i = 0; i < n; i++)
                //foreach (iObject io in _lObjects.Where(p => p.tag == tag1 && p.lang == _lang ))
                {
                    iObject io = _lObjects.Where(p => p.tag == tag1 && p.lang == _lang).ElementAt(i);
                    if (io.id != "")
                    {
                        found = true;
                        linkInfo li = new linkInfo();
                        li.topic_id = io.id;
                        li.topic_name = io.name;
                        _newli.Add(li);
                    }
                }

                if (!found)
                {
                    //List<iObject> ioNew = GetParent(ao, tag1);
                    List<iObject> ioNew = _data.GetParentEx(ao, tag1, _lang, _SERVER);
                    n = ioNew.Count();
                    if (n != 0)
                    {
                        _lObjects.AddRange(ioNew);

                        for (int i = 0; i < n; i++)
                        {
                            iObject io = ioNew.ElementAt(i);
                            if (io.id != "")
                            {
                                found = true;
                                linkInfo li = new linkInfo();
                                li.topic_id = io.id;
                                li.topic_name = io.name;
                                _newli.Add(li);
                            }
                        }
                    }
                }

                if (_newli.Count() != 0)
                {
                    ao.li = new List<linkInfo>(_newli);
                    returnValue = true;
                }

            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("IdentifyTag1 Error: {0}", e.Message.ToString()));
            }
            return returnValue;
        }

        private bool IdentifyTag2(ref actionObject ao)
        {
            bool returnValue = false;
            try
            {
                string tag1 = ao.tag1;
                string tag2 = ao.tag2;
                List<linkInfo> _li = ao.li;
                List<linkInfo> _newli = new List<linkInfo>();
                int n = _lObjects.Where(p => p.tag == tag1 && p.lang == _lang && p.id != "").Count();
                for (int i = 0; i < n; i++)
                {
                    iObject io = _lObjects.Where(p => p.tag == tag1 && p.lang == _lang && p.id != "").ElementAt(i);
                    bool found = false;
                    int nn = io.n.Where(p => p.tag == tag2).Count();
                    for (int ii = 0; ii < nn; ii++)
                    {
                        iObject ioBm = io.n.Where(p => p.tag == tag2).ElementAt(ii);
                        found = true;
                        if (ioBm.id != "")
                        {
                            linkInfo li = new linkInfo();
                            li.topic_id = io.id;
                            li.topic_name = io.name;
                            li.bm = ioBm.name;
                            _newli.Add(li);
                        }
                    }

                    if (!found)
                    {
                        List<iObject> ioNew = _data.GetChildEx(ao, io, tag2, _SERVER);
                        io.n.AddRange(ioNew);
                        nn = ioNew.Count();
                        for (int ii = 0; ii < nn; ii++)
                        {
                            iObject ioN = ioNew.ElementAt(ii);
                            if (ioN.id != "") //Lagt 4/10-11 til for å ikke generere blank bm
                            {
                                found = true;
                                linkInfo li = new linkInfo();
                                li.topic_id = io.id;
                                li.topic_name = io.name;
                                li.bm = ioN.id;
                                _newli.Add(li);
                            }

                        }
                    }
                }

                if (_newli.Count() != 0)
                {
                    returnValue = true;
                    ao.li = new List<linkInfo>(_newli);
                }
            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("IdentifyTag2 Error: {0}", e.Message.ToString()));
            }
            return returnValue;
        }



        private XElement UpdateMissing()
        {
            XElement returnValue = new XElement("root"); ;
            try
            {
                if (_lObjects.Where(p => p.id == "" || p.n.Where(q => q.id == "").Count() != 0).Count() != 0)
                {
                    XElement root = new XElement("root");

                    foreach (iObject io in _lObjects.Where(p => p.id == "" || p.n.Where(q => q.id == "").Count() != 0))
                    {
                        XElement row = null;
                        if ((root.Elements("row").Count() != 0 ? root.Elements("row").Where(p => p.Element("tag1").Value == io.tag).Count() : 0) != 0)
                        {
                            row = root.Elements("row").Where(p => p.Element("tag1").Value == io.tag).First();
                            row.Element("n").Value = (Convert.ToInt32(row.Element("n").Value) + 1).ToString();
                            row.Elements("regexps").First().Add(new XElement("regexp", io.text));
                        }
                        else
                        {
                            row = new XElement("row"
                                , new XElement("tag1", io.tag)
                                , new XElement("lang", _lang)
                                , new XElement("regexps"
                                    , new XElement("regexp", io.text))
                                , new XElement("n", "1")
                            );
                            root.Add(row);
                        }

                        if (io.n.Where(p => p.id == "").Count() != 0)
                        {
                            foreach (iObject ioBm in io.n.Where(p => p.id == ""))
                            {
                                XElement bm = null;
                                if ((row.Elements("bm").Count() != 0 ? row.Elements("bm").Where(p => p.Element("tag2").Value == ioBm.tag).Count() : 0) != 0)
                                {
                                    bm = row.Elements("bm").Where(p => p.Element("tag2").Value == ioBm.tag).First();
                                    bm.Element("n").Value = (Convert.ToInt32(bm.Element("n").Value) + 1).ToString();
                                    bm.Elements("regexps").First().Add(new XElement("regexp", ioBm.text));
                                }
                                else
                                {
                                    bm = new XElement("bm"
                                    , new XElement("tag2", ioBm.tag)
                                    , new XElement("regexps"
                                        , new XElement("regexp", ioBm.text))
                                    , new XElement("n", "1"));
                                    row.Add(bm);
                                }
                            }
                        }

                    }
                    List<XElement> r = root.Elements("row").OrderBy(p => p.Element("tag1").Value).ToList();
                    returnValue = new XElement("root");
                    foreach (XElement e in r.OrderBy(p => p.Element("tag1").Value))
                    {
                        returnValue.Add(e);
                    }

                }
            }
            catch (SystemException e)
            {

            }

            return returnValue;

        }

    }
}
