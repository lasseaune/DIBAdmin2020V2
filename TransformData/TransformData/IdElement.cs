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


namespace Dib.Text.Id
{
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
    public class IdentyfiedLinks
    {
        public string id { get; set; }
        public string parent { get; set; }
        public string ID_topic_id = "";
        public string topic_id = "";
        public string segment_id = "";
        public string bm = "";
        public string title = "";
        public string language = "no";
        public string tag1 = "";
        public string tag2 = "";
        public string tag2_alt = "";
        public string tag3 = "";
        public string totag2 = "";
        public string totag3 = "";
        public bool Internal = false;
        public IdAreas idArea { get; set; }
        public string RegexName { get; set; }
        public int pos = 0;
        public bool Deleted = false;
        public IdentyfiedLinks() { }
        public IdentyfiedLinks(IdLink idlink, XElement tag1, XElement idSpan)
        {
            this.pos = idlink.start;
            this.RegexName = idlink.regexpName;
            this.idArea = idlink.idArea;
            this.id = idlink.id;

            this.parent = idlink.parent;
            this.ID_topic_id = tag1.GetAttributeValue("topic_id");
            this.topic_id = tag1.GetAttributeValue("topic_id");
            this.segment_id = idSpan.Ancestors("tag2").First().GetAttributeValue("segment_id");
            if (this.segment_id == "")
            {
                this.segment_id = tag1.GetAttributeValue("segment_id");
            }
            this.language = tag1.GetAttributeValue("language");
            this.pos = idlink.start;
            this.tag1 = idlink.tag1;
            this.tag2 = idSpan.Attribute("name").Value;
            this.totag2 = idlink.totag2;
            this.totag3 = idlink.totag3;
            //Sjekk tittel for kapittel/del
            if (RegexName == "para")
                this.title = "§ " + idSpan.Attribute("name").Value;
            else
            {
                this.title = idSpan.GetAttributeValue("title");
            }
            this.bm = idSpan.GetAttributeValue("id");
        }
        public IdentyfiedLinks(IdentyfiedLinks idlink)
        {
            this.RegexName = idlink.RegexName;
            this.idArea = idlink.idArea;
            this.id = idlink.id;
            this.language = idlink.language;
            this.parent = idlink.parent;
            this.pos = idlink.pos;
            this.tag1 = idlink.tag1;
            this.tag2 = idlink.tag2;
            this.tag2_alt = idlink.tag2_alt;
            this.tag3 = idlink.tag3;
            this.totag2 = idlink.totag2;
            this.totag3 = idlink.totag3;
        }
        public IdentyfiedLinks(IdLink idlink)
        {
            this.RegexName = idlink.regexpName;
            this.idArea = idlink.idArea;
            this.id = idlink.id;
            if (idlink.parent == "m241")
                Debug.Print("xxxx");
            this.parent = idlink.parent;
            this.pos = idlink.start;
            this.tag1 = idlink.tag1;
            this.tag2 = idlink.tag2;
            this.tag2_alt = idlink.tag2_alt;
            this.tag3 = idlink.tag3;
            this.totag2 = idlink.totag2;
            this.totag3 = idlink.totag3;
            if (idlink.tag1.ToLower().IndexOf("tvang") != -1)
                Debug.Print("xxxx");
        }
        public IdentyfiedLinks(IdLink idlink, XElement tag1)
        {
            pos = idlink.start;
            RegexName = idlink.regexpName;
            idArea = idlink.idArea;
            id = idlink.id;
            parent = idlink.parent;
            ID_topic_id = tag1.GetAttributeValue("topic_id");
            topic_id = tag1.GetAttributeValue("topic_id");
            segment_id = tag1.GetAttributeValue("segment_id");
            title = tag1.GetAttributeValue("name");
            language = tag1.GetAttributeValue("language");
            pos = idlink.start;
            this.tag1 = idlink.tag1;
            this.tag2 = idlink.tag2;
            this.totag2 = idlink.totag2;
            this.tag2_alt = idlink.tag2_alt;
            this.tag3 = idlink.tag3;
            this.totag3 = idlink.totag3;

            if (idlink.idArea == IdAreas.External)
            {
                if (idlink.tag2 != "")
                {
                    XElement xtag2 = null;
                    if (idlink.tag2_alt != "")
                    {
                        xtag2 = tag1
                            .Elements("tag2")
                            .Where(p =>
                                (p.Attribute("name") == null ? "" : p.Attribute("name").Value.ToLower()) == idlink.tag2_alt.ToLower()
                                && p.Attributes("id").FirstOrDefault() != null
                            ).FirstOrDefault();
                    }
                    if (xtag2 == null)
                    {
                        xtag2 = tag1
                            .Elements("tag2")
                            .Where(p =>
                                (p.Attribute("name") == null ? "" : p.Attribute("name").Value.ToLower()) == idlink.tag2.ToLower()
                                && p.Attributes("id").FirstOrDefault() != null
                            ).FirstOrDefault();
                    }

                    if (xtag2 != null)
                    {
                        this.segment_id = xtag2.GetAttributeValue("segment_id");
                        this.bm = xtag2.GetAttributeValue("id");
                        //Sjekk tittel
                        if (RegexName == "para")
                            this.title = "§ " + xtag2.GetAttributeValue("name");
                        else
                        {
                            this.title = xtag2.GetAttributeValue("name");
                        }

                        if (idlink.tag3 != "")
                        {
                            XElement xtag3 = xtag2
                                .Elements("tag3")
                                .Where(p =>
                                    (p.Attribute("name") == null ? "" : p.Attribute("name").Value.ToLower()) == idlink.tag3.ToLower()
                                    && p.Attributes("id").FirstOrDefault() != null
                            ).FirstOrDefault();

                            if (xtag3 != null)
                            {
                                segment_id = xtag3.GetAttributeValue("segment_id");
                                bm = xtag3.GetAttributeValue("id");

                                if (RegexName == "para")
                                    this.title = "§ " + xtag2.GetAttributeValue("name") + " " + IdElements.rmActions._NumberWords.GetNumberwordsTitle(idlink.tag3);
                                else
                                {
                                    this.title = xtag2.GetAttributeValue("title");
                                }
                            }

                        }

                    }

                }
            }
        }
    }
    public class InternalItem
    {
        public string name { get; set; }
        public string id { get; set; }
        public string topic_id { get; set; }
        public string regextype { get; set; }
        public string title { get; set; }
        public string substitute_id { get; set; }
        public string area { get; set; }

        public InternalItem(string name, string id, string topic_id, string regextype, string title, string substitute_id, string area)
        {
            this.name = name;
            this.id = id;
            this.topic_id = topic_id;
            this.regextype = regextype;
            this.title = title;
            this.substitute_id = substitute_id;
            this.area = area;
        }
    }
    public class RegexGroupRange
    {
        public RegexGroupRange top;
        public IdElement parent;
        public Dictionary<string, string> Result = new Dictionary<string, string>();
        public List<RegexGroup> RangeList;
    }
    public class RegexGroup
    {
        public int pos { get; set; }
        public int length { get; set; }
        public string name { get; set; }
        public string value { get; set; }
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
        protected static List<string> _GroupNames;
        public RegexMatchActions() { }
        public RegexMatchActions(Regex regex, XElement actions, string listOfWords)
        {
            this.regex = regex;
            _MatchList = GetMatchList(actions);
            _GroupNames = GetGroupNames(regex);
            _NumberWords = new NumberWords(listOfWords);
        }

        private List<MatchAction> GetMatchList(XElement actions)
        {
            return actions
                    .Elements("action")
                    .Where(p => (string)p.Attributes("name").FirstOrDefault() == "total")
                    .Elements("match")
                    .Where(p => (string)p.Attributes("name").FirstOrDefault() != "")
                    .Select(p => new MatchAction(p)
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
            public MatchAction(XElement e)
            {
                this.name = (string)e.Attributes("name").FirstOrDefault();
                this.Actions = new RegexAction(e);
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
            public RegexAction(XElement e)
            {
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
                                        .Select(p => new RegexAction(p))
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
                            case "true":
                            case "false": ra.Execute(rg); break;
                            case "foreach":
                                ExecuteForeach(ra, rg);
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

                    if (function.Value == "GetParagrafElements")
                    {
                        GetParagrafElements(ra, rg);
                    }
                }
                catch (SystemException err)
                {
                    throw new Exception("Error: 'ExecuteRun ' \r\n " + err.Message);
                }
            }

            private List<RegexGroup> GetGroups(RegexGroup currentGroup, List<RegexGroup> groups)
            {
                return groups
                        .Where(p =>
                            p.pos >= currentGroup.pos
                            && (p.pos + p.length) <= (currentGroup.pos + currentGroup.length)
                            && p.name != currentGroup.name
                            )
                        .OrderBy(p => p.pos).ThenByDescending(p => p.length)
                        .ToList();
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
                    RegexGroup paraTotal = rg.RangeList.Where(p => p.name == groups.Value).FirstOrDefault();
                    if (paraTotal == null) return;
                    List<RegexGroup> paraGroup = GetGroups(paraTotal, rg.RangeList);

                    List<RegexGroup> refGroups = paraGroup.Where(p => p.name == "lovrefs").ToList();

                    int min = rg.RangeList.Select(p => p.pos).Min();
                    int max = rg.RangeList.Select(p => p.pos + p.length).Max();
                    string parent_id = "";
                    if (!rg.Result.ContainsKey("internal"))
                    {
                        if (!rg.Result.ContainsKey("tag1")) AddToDictionary(rg.Result, "tag1", "");
                        parent_id = rg.Result["parentid"];
                        parent_id = AddIdLink(rg, min, max - min, rg.Result, parent_id);
                    }
                    else
                    {
                        parent_id = rg.Result["parentid"];
                    }
                    //else
                    //{
                    //    string debugtext = rg.parent.text.ToString().Substring(min, max - min);
                    //    Debug.Print(debugtext);
                    //}
                    //string debugtext = rg.parent.text.ToString().Substring(min, max - min);
                    //Debug.Print("MAX: " + max.ToString());
                    //if (max == 5357) Debug.Print("MAX: " + max.ToString());


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
                                                        result = IdElements.rmActions._NumberWords.GetValue(para.value);
                                                        if (result.Type != MatchTypes.Error)
                                                        {
                                                            switch (para.name)
                                                            {
                                                                case "leddnumber":
                                                                    refid = ((int)MatchTypes.Ledd).ToString().PadLeft(2, '0') + result.Value.PadLeft(3, '0');
                                                                    break;
                                                                case "punumber":
                                                                    refid = ((int)MatchTypes.Punktum).ToString().PadLeft(2, '0') + result.Value.PadLeft(3, '0');
                                                                    break;
                                                                case "pktnumber":
                                                                case "letternumber":
                                                                    refid = ((int)result.Type).ToString().PadLeft(2, '0') + result.Value.PadLeft(3, '0');
                                                                    break;
                                                                case "pnsingleletter":
                                                                    {
                                                                        refid = (99).ToString().PadLeft(2, '0') + result.Value.PadLeft(3, '0');
                                                                        //sjekk denne!!
                                                                        if (tag2.ReverseXor().StartsWith(para.value.ReverseXor()))
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
                                                            refid = refid
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
                                                                    bool found = false;
                                                                    int mm = 0;
                                                                    lastTag3 = lastTag3.ReverseXor();
                                                                    while (!found)
                                                                    {
                                                                        if (lastTag3.Length > mm + 5)
                                                                        {
                                                                            string lastRef = lastTag3.Substring(mm, 5).ReverseXor();
                                                                            if (lastRef.Substring(0, 2) == refidNext.Substring(0, 2))
                                                                            {
                                                                                if (lastTag3.Length > (mm + 5))
                                                                                {
                                                                                    tag3 = lastTag3.Substring(mm + 5).ReverseXor();
                                                                                    found = true;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            mm = mm + 5;
                                                                        }
                                                                        else
                                                                        {
                                                                            found = true;
                                                                            break;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                tag3 = tag3 + psr.refid;
                                                                AddToDictionary(rg.Result, "tag3", tag3);
                                                                subPos = psr.pos;
                                                                sublength = psr.length;
                                                                //if (tag3.Length > 15)
                                                                //    Debug.Print(debugtext);

                                                            }
                                                        }

                                                        if (tag3 != ""
                                                            && split.Trim().ToLower() != "til"
                                                            && split.Trim().ToLower() != ENDASH
                                                            )
                                                        {
                                                            split = "";
                                                            if (splitPara == "–") splitPara = "til";
                                                            if (splitPara == "til" && para_id != "")
                                                            {
                                                                AddToDictionary(rg.Result, "split", splitPara);
                                                                AddIdLink(rg, (subPos == 0 ? paranr.pos : subPos), (sublength == 0 ? paranr.length : sublength), rg.Result, para_id);
                                                                RemoveFromDictionary(rg.Result, "split");
                                                                splitPara = "";
                                                            }
                                                            else
                                                            {
                                                                para_id = AddIdLink(rg, (subPos == 0 ? paranr.pos : subPos), (sublength == 0 ? paranr.length : sublength), rg.Result, parent_id);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            AddToDictionary(rg.Result, "split", split);
                                                            AddIdLink(rg, paranr.pos, paranr.length, rg.Result, para_id);
                                                            RemoveFromDictionary(rg.Result, "split");
                                                            split = "";
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (splitPara == "–") splitPara = "til";
                                                    if (splitPara == "til" && para_id != "")
                                                    {
                                                        AddToDictionary(rg.Result, "split", splitPara);
                                                        AddIdLink(rg, paranr.pos, paranr.length, rg.Result, para_id);
                                                        RemoveFromDictionary(rg.Result, "split");
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
                                                    AddIdLink(rg, paranr.pos, paranr.length, rg.Result, para_id);
                                                    RemoveFromDictionary(rg.Result, "split");
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
                                        result = IdElements.rmActions._NumberWords.GetValue(r.value);
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
                                        result = IdElements.rmActions._NumberWords.GetValue(r.value);
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
                IdLink idlink = null;
                if (rg.Result.ContainsKey("split"))
                {
                    idlink = rg.parent.idLinks.Where(p => p.id == parentid).FirstOrDefault();
                    if (idlink != null)
                    {
                        idlink.totag2 = rg.Result.ContainsKey("tag2") ? rg.Result["tag2"] : "";
                        idlink.totag3 = rg.Result.ContainsKey("tag3") ? rg.Result["tag3"] : "";
                    }
                }
                else
                {
                    //if ((rg.Result.ContainsKey("tag2") ? rg.Result["tag2"] : "") == "1-4"
                    //    && (rg.Result.ContainsKey("tag3") ? rg.Result["tag3"] : "") == "1.3") 
                    //{
                    //    Debug.Print("xxx");
                    //}

                    idlink = new IdLink(rg, pos, length, parentid);
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
                            AddIdLink(rg, pos, length, rg.Result, parentid);
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
                            if (s.StartsWith("$") && s.EndsWith("$"))
                            {
                                sbTag.Append(s.Substring(1, s.Length - 2));
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
                        foreach (RegexGroup gr in rg.RangeList.Where(p => p.name == group.Value))
                        {
                            RegexGroupRange newRg = new RegexGroupRange
                            {
                                top = rg.top == null ? rg : rg.top,
                                parent = rg.parent,
                                RangeList = rg.RangeList
                                                .Where(p =>
                                                    p.pos >= gr.pos
                                                    && (p.pos + p.length) <= (gr.pos + gr.length)
                                                    )
                                                    .ToList(),
                                Result = rg.Result

                            };
                            returnValue = true;
                            ra.Execute(newRg);
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
                        return returnValue;
                    }
                    else
                    {
                        //if (!rg.Result.ContainsKey("parentid"))
                        //{
                        //    int min = rg.RangeList.Where(q => groups.Where(p => p == q.name).Count() != 0).Min(p => p.pos);
                        //    int max = rg.RangeList.Where(q => groups.Where(p => p == q.name).Count() != 0).Max(p => p.pos + p.length);

                        //    string parentid = AddMarkupLink(rg, min, max - min, ra.name);
                        //    rg.Result.AddKey("parentid", parentid);
                        //    rg.Result.AddKey("name", ra.name);
                        //}

                        if (ra.name != "")
                            AddToDictionary(rg.Result, "name", ra.name);
                        RegexAction raFalse = ra.Actions.Where(p => p.type == "false").FirstOrDefault();
                        if (raFalse != null)
                        {
                            returnValue = true;
                            ra = raFalse;
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
            public RegexGroups(Match m)
            {
                try
                {
                    GroupCollection groups = m.Groups;
                    foreach (string grpName in _GroupNames)
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

        private static string AddMarkupLink(RegexGroupRange rg, int pos, int length, string name)
        {
            string parentid = null;

            if (rg.parent.ranges.Count() != 0)
            #region
            {
                List<Range> ranges = rg.parent.ranges.Where(p =>
                       (pos.Between(p.pos, p.pos + p.length, true))
                    || ((pos + length).Between(p.pos, p.pos + p.length, true))
                    || ((pos <= p.pos) && ((pos + length) >= (p.pos + p.length)))
                    ).ToList();
                Range range = null;
                if (ranges.Count() == 1)
                {
                    range = ranges.ElementAt(0);
                    if (rg.Result == null)
                    {
                        parentid = null;
                    }
                    else
                    {
                        MarkupLink markuplink = new MarkupLink(rg, range, pos, length, name);
                        parentid = markuplink.id;
                        range.MarkupLinks.Add(markuplink);
                    }
                }
                else if (ranges.Count() > 1)
                {
                    MarkupLink parent = null;
                    for (int i = 0; i < ranges.Count(); i++)
                    //foreach (Range r in ranges)
                    {
                        Range r = ranges.OrderBy(p => p.pos).ElementAt(i);
                        MarkupLink markuplink = null;
                        if (pos.Between(r.pos, r.pos + r.length)
                            && (pos + length).Between(r.pos, r.pos + r.length))
                        {
                            markuplink = new MarkupLink(rg, r, pos, length, name, parent);
                        }
                        else if (pos.Between(r.pos, r.pos + r.length))
                        {
                            markuplink = new MarkupLink(rg, r, pos, length, name, parent);
                        }
                        else if ((pos + length).Between(r.pos, r.pos + r.length))
                        {
                            markuplink = new MarkupLink(rg, r, r.pos, (pos + length) - (r.pos), name, parent);
                        }
                        else
                            markuplink = new MarkupLink(rg, r, r.pos, r.length, name, parent);

                        if (r.node.ToString().Trim() != "." && markuplink != null)
                        {
                            if (i == 0)
                            {
                                parent = markuplink;
                            }
                            r.MarkupLinks.Add(markuplink);
                        }
                    }
                }
            }
            #endregion
            return parentid;

        }

        private string AddMarkupLink(RegexGroupRange rg, Match m, string name)
        {
            string parentid = null;
            int pos = m.Index;
            int length = m.Length;


            if (rg.parent.ranges.Count() != 0)
            #region
            {
                List<Range> ranges = rg.parent.ranges.Where(p =>
                       (pos.Between(p.pos, p.pos + p.length, true))
                    || ((pos + length).Between(p.pos, p.pos + p.length, true))
                    || ((pos <= p.pos) && ((pos + length) >= (p.pos + p.length)))
                    ).ToList();
                Range range = null;
                if (ranges.Count() == 1)
                {
                    range = ranges.ElementAt(0);
                    if (rg.Result == null)
                    {
                        parentid = null;
                    }
                    else
                    {
                        MarkupLink markuplink = new MarkupLink(rg, range, pos, length, name);
                        parentid = markuplink.id;
                        range.MarkupLinks.Add(markuplink);
                    }
                }
                else if (ranges.Count() > 1)
                {
                    MarkupLink parent = null;
                    for (int i = 0; i < ranges.Count(); i++)
                    //foreach (Range r in ranges)
                    {
                        Range r = ranges.OrderBy(p => p.pos).ElementAt(i);
                        MarkupLink markuplink = null;
                        if (pos.Between(r.pos, r.pos + r.length)
                            && (pos + length).Between(r.pos, r.pos + r.length))
                        {
                            markuplink = new MarkupLink(rg, r, pos, length, name, parent);
                        }
                        else if (pos.Between(r.pos, r.pos + r.length))
                        {
                            markuplink = new MarkupLink(rg, r, pos, length, name, parent);
                        }
                        else if ((pos + length).Between(r.pos, r.pos + r.length))
                        {
                            markuplink = new MarkupLink(rg, r, r.pos, (pos + length) - (r.pos), name, parent);
                        }
                        else
                            markuplink = new MarkupLink(rg, r, r.pos, r.length, name, parent);

                        if (r.node.ToString().Trim() != "." && markuplink != null)
                        {
                            if (i == 0)
                            {
                                parent = markuplink;
                            }
                            r.MarkupLinks.Add(markuplink);
                        }
                    }
                }
            }
            #endregion
            return parentid;

        }
        public void Execute(IdElement element)
        {
            try
            {
                MatchCollection mc = this.regex.Matches(element.text.ToString());
                foreach (Match m in mc)
                //for (int i = 0; i < mc.Count;i++)
                {
                    //Match m = mc[i];
                    //if (Regex.IsMatch(m.Value.ToLower(), @"§ 10-4 med note 1.3", RegexOptions.IgnoreCase)) Debug.Print(m.Value);
                    RegexGroups regexgroups = new RegexGroups(m);

                    var groups = from qrp in regexgroups._RegexGroups
                                 join ml in _MatchList on qrp.name equals ml.name
                                 select new { capture = qrp, action = ml };
                    foreach (var group in groups)
                    {

                        MatchAction ma = group.action;
                        RegexGroupRange rg = new RegexGroupRange
                        {

                            parent = element,
                            RangeList = regexgroups._RegexGroups
                                            .Where(p =>
                                                p.pos >= group.capture.pos
                                                && (p.pos + p.length) <= (group.capture.pos + group.capture.length)
                                                )
                                                .ToList()
                        };
                        string parentid = AddMarkupLink(rg, m, ma.name);
                        if (parentid != null)
                        {
                            rg.Result.AddKey("parentid", parentid);
                            rg.Result.AddKey("name", ma.name);
                            ma.Execute(rg);
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
    public class Range
    {
        public XText node { get; set; }
        public int pos { get; set; }
        public int length { get; set; }
        public List<MarkupLink> MarkupLinks = new List<MarkupLink>();
        public int id { get; set; }
        public int pid { get; set; }
    }

    public class IdLink
    {
        static int nrOfInstances = 0;
        public IdAreas idArea = IdAreas.External;
        public string id { get; set; }
        public string parent { get; set; }
        public int start = 0;
        public int length = 0;
        public string tag1 = "";
        public string tag2 = "";
        public string tag2_alt = "";
        public string tag3 = "";
        public string totag2 = "";
        public string totag3 = "";
        public string regexpName = "";
        public string prefix = "";

        public IdLink(RegexGroupRange rg, int start, int length, string parent = null)
        {
            Dictionary<string, string> Result = rg.Result;
            this.start = start;
            this.length = length;
            this.id = "id" + IdLink.nrOfInstances;
            nrOfInstances++;

            this.tag1 = (Result.ContainsKey("tag1") ? Result["tag1"] : "").Trim();
            this.tag1 = Regex.Replace(this.tag1, @"((.*?)(lov))(en|a)$", "$1");
            this.tag1 = Regex.Replace(this.tag1, @"((.*?))(\.)$", "$1");
            this.tag1 = Regex.Replace(this.tag1, @"\s+", " ");
            this.tag1 = Regex.Replace(this.tag1, @"slov", "lov");

            this.tag2 = Result.ContainsKey("tag2") ? Result["tag2"] : "";
            this.tag2_alt = Result.ContainsKey("tag2_alt") ? Result["tag2_alt"] : "";
            this.tag3 = Result.ContainsKey("tag3") ? Result["tag3"] : "";

            if (Result.ContainsKey("internal"))
            {
                this.idArea = IdAreas.Internal;
            }
            if (Result.ContainsKey("prefix")) this.prefix = Result["prefix"];
            this.regexpName = Result.ContainsKey("name") ? Result["name"] : "";
            this.parent = parent;
        }
    }

    public class MarkupLink
    {
        static int nrOfInstances = 0;
        public IdAreas idArea = IdAreas.External;
        public int start { get; set; }
        public int length { get; set; }
        public string id { get; set; }
        public int refid { get; set; }
        public Range ParentRange { get; set; }
        public string tagAncestor { get; set; }
        public MarkupLink ParentMarkupLink { get; set; }
        public string regexpName { get; set; }
        public MarkupLink(RegexGroupRange rg, Range r, int start, int length, string name, MarkupLink parent = null)
        {
            this.regexpName = name;
            this.ParentMarkupLink = parent;
            this.ParentRange = r;
            this.start = start;
            this.length = length;
            this.id = "m" + MarkupLink.nrOfInstances.ToString();

            if (rg.Result.ContainsKey("internal"))
            {
                this.idArea = IdAreas.Internal;
                if ((rg.parent.parent.InternalItems == null ? 0 : rg.parent.parent.InternalItems.Count()) != 0)
                {
                    var ans = rg
                        .parent
                        .parent
                        .InternalItems
                        .Where(q =>
                                r.node.Ancestors()
                                .Where(p =>
                                    q.id == (string)p.Attributes("id").FirstOrDefault()
                                )
                                .Count() != 0
                        );

                    if (ans.Count() != 0)
                    {
                        this.tagAncestor = ans.First().id;
                    }
                }
            }

            nrOfInstances++;
        }
    }


    public class IdElements
    {
        public static RegexMatchActions rmActions;
        public static string sep = " | ";
        public static string buffer = " ";
        public static int offseth = sep.Length;
        public bool InternalReference = false;
        public List<IdElement> elements { get; set; }
        public XElement externalTags;
        public XElement externalExtractTags;
        public XElement internalTags;
        public XElement unidentifiedTags;
        public XElement documentTags;
        private XElement Document;
        public IEnumerable<InternalItem> InternalItems;
        private IEnumerable<string> include;
        private IEnumerable<string> exclude;
        public IEnumerable<string> BreakNodes;
        public IEnumerable<string> nonmarkupnodes;
        private List<IdentyfiedLinks> identyfiedlinks { get; set; }
        private bool Local = true;
        private string Language = "no";
        private int LinkCounter = 0;
        public string SQLProcedure = "Abbreviation_Identify_Tags_Ex1";
        public string DatabaseName = "TopicMap.dbo";

        public IdElements(XElement document,
                        XElement actions,
                        string Regexp = "",
                        string Include = "",
                        string Exclude = "",
                        string Breaknodes = "",
                        string NonMarkupNodes = "",
                        bool InternalReference = false,
                        string language = "no",
                        bool Local = true)
        {
            try
            {
                this.Document = document;
                this.Language = language;
                Regex regex = new Regex(Regexp);
                this.Local = Local;
                this.include = Include.Split(';').Where(p => p != "");
                this.exclude = Exclude.Split(';').Where(p => p != "");
                this.BreakNodes = Breaknodes.Split(';').Where(p => p != "");
                this.nonmarkupnodes = NonMarkupNodes.Split(';').Where(p => p != "");

                string listOfNumbers = "";
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


                if (InternalReference)
                {
                    this.InternalReference = true;
                }
                rmActions = new RegexMatchActions(regex, actions, listOfNumbers);
                var sections = Document.GetDescendantsAndSelf(include);
                if (sections.Count() == 0)
                {
                    return;
                }
                this.elements = sections.GetIdElements(this, exclude).ToList();
            }
            catch (SystemException err)
            {
                throw new Exception("IdElements - Error: \r\n" + err.Message);
            }
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
                foreach (IdElement e in this.elements)
                {
                    idelements.Add("idelement", e.text);
                }
                idelements.Save(filename);
            }
            catch (SystemException err)
            {
                throw new Exception("Save IdElements to file - Error: \r\n" + err.Message);
            }
        }
        private void UpdateIdInternal()
        {
            try
            {

                var InternalParent = from ip in InternalItems
                                     where ip.area == "global"
                                     select ip;

                foreach (InternalItem iItem in InternalParent)
                {
                    var ilinks = from l in this.elements
                                 from d in l.idLinks.Where(p => p.tag1 != "" && iItem.regextype.ToString().Split(';').Where(rt => rt != "" && rt.ToLower() == p.regexpName.ToLower()).Count() != 0)
                                 join t in documentTags.Descendants("tag1").Where(p => (string)p.Attributes("segment_id").FirstOrDefault() == iItem.id)
                                     on
                                 new
                                 {
                                     t1 = d.tag1.ToString()
                                 }
                                 equals
                                  new
                                  {
                                      t1 = (string)t.Attributes("name").FirstOrDefault()
                                  }
                                 select new { id = d, el = t };
                    foreach (var l in ilinks)
                    {
                        IdentyfiedLinks newIl = new IdentyfiedLinks();
                        newIl.idArea = l.id.idArea;
                        newIl.pos = l.id.start;
                        newIl.RegexName = l.id.regexpName;
                        newIl.Internal = true;
                        newIl.parent = l.id.parent;
                        newIl.id = l.id.id;
                        newIl.tag1 = l.id.tag1;
                        newIl.topic_id = (string)l.el.Attributes("topic_id").FirstOrDefault();
                        newIl.ID_topic_id = newIl.topic_id;
                        newIl.segment_id = (string)l.el.Attributes("segment_id").FirstOrDefault();
                        newIl.bm = (string)l.el.Attributes("id").FirstOrDefault();
                        newIl.title = (string)l.el.Attributes("title").FirstOrDefault();
                        identyfiedlinks.Add(newIl);
                    }
                }



                var links = from l in identyfiedlinks.Where(p => p.tag2 != "" && p.totag2 == p.tag2 && p.tag3 != "" && p.totag3 != "" && p.Internal == false)
                            join t in documentTags.Descendants("tag2") on
                                 new
                                 {
                                     t1 = l.topic_id.ToLower(),
                                     t2 = l.tag2.ToLower(),
                                 }
                             equals
                                 new
                                 {
                                     t1 = t.Ancestors("tag1").First().Attribute("substitute_id").Value.ToLower(),
                                     t2 = t.Attribute("name").Value.ToLower(),
                                 }
                            //select l;
                            select new { id = l, el = t };
                List<IdentyfiedLinks> newLinks = new List<IdentyfiedLinks>();
                foreach (var l in links)
                {
                    if (l.id.tag2 == l.id.totag2 && l.id.tag3.Length == l.id.totag3.Length && l.id.tag3.Substring(0, 2) == l.id.totag3.Substring(0, 2))
                    {
                        bool found = false;


                        foreach (tag3data e in GetParaListTag3(
                            l.el.Ancestors("tag1").First().Attribute("id").Value,
                            l.el.Attribute("name").Value.ToLower(),
                            l.el.Attribute("name").Value.ToLower(),
                            l.id.tag3,
                            l.id.totag3
                            )
                        )
                        {
                            if (!found)
                            {
                                found = true;
                            }
                            IdentyfiedLinks idEl = new IdentyfiedLinks(l.id);
                            idEl.topic_id = e.topic_id;
                            idEl.ID_topic_id = l.id.ID_topic_id;
                            idEl.segment_id = e.segment_id;

                            idEl.bm = e.id;
                            idEl.title = e.title;
                            idEl.Internal = true;
                            newLinks.Add(idEl);
                        }
                        if (found)
                        {
                            l.id.Deleted = true;
                            IdentyfiedLinks idLi = identyfiedlinks
                                                    .Where(p => p.id == l.id.parent)
                                                    .FirstOrDefault();
                            if (idLi != null)
                            {
                                idLi.segment_id = (string)l.el.Ancestors("tag1").Attributes("segment_id").FirstOrDefault(); ;
                                idLi.topic_id = (string)l.el.Ancestors("tag1").Attributes("topic_id").FirstOrDefault();

                                idLi.bm = "";
                                idLi.title = (string)l.el.Ancestors("tag1").Attributes("title").FirstOrDefault();
                                idLi.Internal = true;

                            }


                        }
                    }
                }

                if (newLinks.Count() != 0)
                {
                    identyfiedlinks.AddRange(newLinks);
                }

                var idl = from l in identyfiedlinks.Where(p => p.tag2 != "" && p.tag3 != "" && p.Internal == false && p.Deleted == false)
                          join t in documentTags.Descendants("tag3") on
                              new
                              {
                                  t1 = l.topic_id.ToLower(),
                                  t2 = l.tag2.ToLower(),
                                  t3 = l.tag3.ToLower()
                              }
                          equals
                              new
                              {
                                  t1 = t.Ancestors("tag1").First().Attribute("substitute_id").Value.ToLower(),
                                  t2 = t.Ancestors("tag2").First().Attribute("name").Value.ToLower(),
                                  t3 = t.Attribute("name").Value.ToLower()
                              }
                          //select l;
                          select new { id = l, el = t };

                int i = 0;
                foreach (var l in idl)
                {
                    i++;
                    l.id.Internal = true;
                    l.id.language = Language;
                    l.id.topic_id = l.el.Ancestors("tag1").Attributes("topic_id").Count() == 0 ? "" : l.el.Ancestors("tag1").First().GetAttributeValue("topic_id");
                    l.id.segment_id = l.el.GetAttributeValue("segment_id");
                    l.id.bm = l.el.GetAttributeValue("id");
                    if (l.id.RegexName == "para" && l.id.tag3 != "")
                    {
                        l.id.title = "§ " + l.el.Ancestors("tag2").First().GetAttributeValue("name") + " " + IdElements.rmActions._NumberWords.GetNumberwordsTitle(l.id.tag3);
                    }
                    else
                    {
                        l.id.title = l.el.GetAttributeValue("title");
                    }
                }

                idl = from l in identyfiedlinks.Where(p => p.Internal == false && p.Deleted == false)
                      join t in documentTags.Descendants("tag1").Where(p => p.Attribute("substitute_id") != null) on
                          new
                          {
                              t1 = l.topic_id.ToLower()
                          }
                      equals
                          new
                          {
                              t1 = t.Attribute("substitute_id").Value.ToLower()
                          }
                      where l.Internal == false
                      select new { id = l, el = t };

                i = 0;
                foreach (var l in idl)
                {
                    i++;
                    l.id.Internal = true;
                    l.id.language = Language;
                    l.id.topic_id = l.el.GetAttributeValue("topic_id");
                    l.id.segment_id = l.el.GetAttributeValue("segment_id");
                    l.id.bm = l.el.GetAttributeValue("id");
                    string tagId = l.id.bm;
                    if (l.id.bm == l.id.segment_id)
                    {
                        l.id.bm = "";
                    }
                    l.id.title = l.el.GetAttributeValue("title");

                    XElement tag = null;
                    if (l.id.tag2 != "" || l.id.tag3 != "")
                    {
                        tag = EvalInternalTags(tagId, l.id.tag2, l.id.tag3, l.id.RegexName);
                        if (tag != null)
                        {
                            l.id.Internal = true;
                            l.id.language = Language;
                            l.id.topic_id = l.el.GetAttributeValue("topic_id");
                            l.id.segment_id = (string)tag.Attributes("segment_id").FirstOrDefault();
                            l.id.bm = (string)tag.Attributes("id").FirstOrDefault();
                            l.id.title = (string)tag.Attributes("title").FirstOrDefault();
                        }
                    }

                }

                var xx = from l in identyfiedlinks.Where(p => p.Internal == false && p.idArea == IdAreas.Internal)
                         select l;
            }
            catch (SystemException err)
            {
                throw new Exception("UpdateIdInternal - Error' \r\n " + err.Message);
            }
        }
        private void UpdateIdLinks()
        {
            try
            {

                if (externalTags != null)
                {
                    var x = from l in this.elements
                            from d in l.idLinks.Where(p => p.tag1 != "" && p.totag2 != "" && p.tag2 == p.totag2 && p.tag3 != "")
                            select d;

                    identyfiedlinks = (from l in this.elements
                                       from d in l.idLinks.Where(p => p.tag1 != "" && (p.totag2 == "" || (p.totag2 != "" && p.tag2 == p.totag2)))
                                       join t in externalTags
                                        .Descendants("tag1")
                                        .Where(p => p.Attribute("topic_id") != null)
                                        .Elements("name") on
                                            new { t1 = d.tag1.ToLower() }
                                       equals
                                            new { t1 = t.Attribute("text").Value.ToLower() }
                                       select new IdentyfiedLinks(d, t.Parent)).ToList();


                    identyfiedlinks.AddRange(
                                    from l in this.elements
                                    from d in l.idLinks.Where(p => p.tag1 != "" && p.totag2 != "" && p.tag2 != p.totag2)
                                    join tag1 in externalTags
                                        .Descendants("tag1")
                                        .Where(p => p.Attribute("topic_id") != null)
                                        .Elements("name") on
                                            new { t0 = d.tag1.ToLower() }
                                        equals
                                            new { t0 = tag1.Attribute("text").Value.ToLower() }
                                    join t in externalTags
                                        .Descendants("tag1")
                                        .Where(p => p.Attribute("topic_id") != null)
                                        .Elements("tag2")
                                        .Elements("idspan")
                                        .OrderBy(p => p.Attribute("name").Value, new NaturalSortComparer<string>())
                                       on
                                            new { t1 = tag1.GetAttributeValue("topic_id"), t2 = d.tag2 }
                                       equals
                                            new { t1 = t.GetAttributeValue("topic_id"), t2 = t.Ancestors("tag2").First().Attribute("name").Value }

                                    select new IdentyfiedLinks(d, t.Ancestors("tag1").First(), t));

                }

                if (InternalItems != null)
                {
                    identyfiedlinks.AddRange(from l in this.elements
                                             from d in l.idLinks.Where(p =>
                                                 p.idArea == IdAreas.Internal
                                                 && p.tag1 == ""
                                                 )
                                             select new IdentyfiedLinks(d));
                    UpdateIdInternal();
                }



                identyfiedlinks.AddRange(
                    (from idelement in this.elements
                     from Elink in idelement.idLinks
                     join Ilink in identyfiedlinks
                          on Elink.id equals Ilink.id into JoinedElinkILink
                     from Ilink in JoinedElinkILink.DefaultIfEmpty()
                     select new
                     {
                         Idlink = Ilink,
                         Link = Elink
                     })
                    .Where(p => p.Idlink == null)
                    .Select(p => new IdentyfiedLinks(p.Link))
                );


            }
            catch (SystemException err)
            {
                throw new Exception("Error: 'IdElements - UpdateIdLinks' \r\n " + err.Message);
            }
        }
        public XElement Execute()
        {
            try
            {
                this.externalExtractTags = GetTags(IdAreas.External);
                this.externalTags = externalExtractTags;
                this.internalTags = GetTags(IdAreas.Internal);
                if (this.externalTags == null ? false : this.externalTags.HasElements)
                {
                    this.externalTags = IdentifyTags(CodeXMLBase64(this.externalTags), this.Language);
                    if (this.externalTags != null)
                        this.unidentifiedTags = new XElement("tags", this.externalTags.Elements("tag1").Where(p => p.Attribute("topic_id") == null));
                }
                if (
                    (this.externalTags == null ? false : this.externalTags.HasElements)
                    ||
                        (
                        (this.internalTags == null ? false : this.internalTags.HasElements)
                        && (this.InternalItems == null ? 0 : this.InternalItems.Count()) != 0
                        )
                    )
                {
                    UpdateIdLinks();
                    LinkCounter = 0;
                    MarkupLinks();
                }
            }
            catch (SystemException err)
            {
                throw new Exception("Error: 'IdElements - Execute' \r\n " + err.Message);
            }
            return this.Document;
        }
        private static List<XElement> GetTag1(IEnumerable<IdLink> x, IdAreas idArea)
        {
            List<XElement> returntags = null;

            returntags = x
                        .Where(p => p.idArea == idArea)
                        .GroupBy(p => new { p.tag1 })
                        .Select(p =>
                                new XElement("tag1",
                                    new XAttribute("name", p.Key.tag1),
                                    GetTag2(x, idArea, p.Key.tag1)
                                    )
                        )
                        .ToList();
            return returntags;
        }
        private static List<XElement> GetTag2(IEnumerable<IdLink> x, IdAreas idArea, string tag1)
        {
            List<XElement> returntags = null;
            returntags = x
                        .Where(q => q.idArea == idArea && q.tag1 == tag1 && q.tag2 != "")
                        .GroupBy(q => new { tag2 = q.tag2, totag2 = q.totag2, altname = q.tag2_alt })
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
        private static List<XElement> GetTag3(IEnumerable<IdLink> x, IdAreas idArea, string tag1, string tag2)
        {
            List<XElement> returntags = null;
            returntags = x
                    .Where(s => s.idArea == idArea && s.tag1 == tag1 && s.tag2 == tag2 && s.tag3 != "")
                    .GroupBy(s => new { tag3 = s.tag3, totag3 = s.totag3 })
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
        private XElement GetTags(IdAreas IdArea)
        {

            try
            {
                var x = from e in this.elements
                        from l in e.idLinks.Where(p => p.idArea == IdArea)
                        select l
                        ;
                XElement tags = new XElement("tags",
                    GetTag1(x, IdArea)
                    );
                //Debug.Print("xxxx");
                //if (IdArea == IdAreas.External)
                //{
                //   tags.Add(
                //            x.GroupBy(p => new { p.tag1 })
                //            .Select(p =>
                //                new XElement("tag1",
                //                    new XAttribute("name", p.Key.tag1),
                //                        x
                //                        .Where(q => q.idArea == IdArea && q.tag1 == p.Key.tag1 && q.tag2 != "")
                //                        .GroupBy(q => new { q.tag2 })
                //                        .Select(q =>
                //                            new XElement("tag2",
                //                                new XAttribute("name", q.Key.tag2),
                //                                x
                //                                .Where(s => s.idArea == IdArea && s.tag1 == p.Key.tag1 && s.tag2 == q.Key.tag2 && s.tag3 != "")
                //                                .GroupBy(s => new { s.tag3 })
                //                                .Select(s =>
                //                                    new XElement("tag3",
                //                                        new XAttribute("name", s.Key.tag3)
                //                                        )
                //                                    )
                //                                )
                //                            )
                //                        )
                //                    )
                //                );
                //}
                //else if (IdArea == IdAreas.Internal)
                //{
                //    tags.Add(
                //            x.GroupBy(p => new { p.tag1 })
                //            .Select(p =>
                //                new XElement("tag1",
                //                    new XAttribute("name", p.Key.tag1),
                //                    x
                //                    .Where(q => q.idArea == IdArea && q.tag1 == p.Key.tag1 && q.tag2 != "")
                //                    .GroupBy(q => new { q.tag2 })
                //                        .Select(q =>
                //                            new XElement("tag2",
                //                                new XAttribute("name", q.Key.tag2),
                //                                x
                //                                .Where(s => s.idArea == IdArea && s.tag1 == p.Key.tag1 && s.tag2 == q.Key.tag2 && s.tag3 != "")
                //                                .GroupBy(s => new { s.tag3 })
                //                                .Select(s =>
                //                                    new XElement("tag3",
                //                                        new XAttribute("name", s.Key.tag3)
                //                                        )
                //                                    )
                //                                )
                //                            )
                //                        )
                //                    )
                //                );
                //}

                if (IdArea == IdAreas.External)
                {
                    var y = from e in this.elements
                            from l in e.idLinks.Where(p => p.idArea == IdAreas.Internal && p.tag1 != "")
                            select l
                        ;
                    tags.Add(
                        y.GroupBy(p => new { p.tag1 })
                        .Select(p =>
                            new XElement("tag1",
                                new XAttribute("name", p.Key.tag1))));
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
        private void MarkupLinks()
        {
            List<string> markupIds = new List<string>();

            try
            {
                IEnumerable<Range> ranges = from e in this.elements
                                            from r in e.ranges
                                            where r.MarkupLinks.Count() != 0
                                            select r;

                foreach (Range xr in ranges)
                #region //Gå gjennom ranger med linker
                {

                    XNode rangeNode = xr.node;
                    string rangeText = xr.node.ToString();
                    int rangeStart = xr.pos;
                    int rangeCursor = xr.pos;
                    int rangeLength = xr.length;

                    string linkText = "";

                    XElement range = new XElement("range");
                    //int nr = xr.MarkupLinks.Count();
                    //for (int ir = 0; ir < nr; ir++)
                    foreach (MarkupLink l in xr.MarkupLinks.OrderBy(p => p.start))
                    {

                        //MarkupLink l = xr.MarkupLinks.OrderBy(p => p.start).ElementAt(ir);


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

                        if (xr.node.Parent.Name.LocalName == "a"
                            && (xr.node.Parent.Attribute("class") == null ? "" : xr.node.Parent.Attribute("class").Value) == "xref"
                            && xr.node.Parent.Value == linkText
                            )
                        {
                            range.Add(new XText(linkText));
                        }
                        else
                        {
                            if (l.ParentMarkupLink != null)
                            {

                                if (markupIds.Where(p => p == l.ParentMarkupLink.id).Count() != 0)
                                {
                                    range.Add(new XElement("diblink",
                                        new XAttribute("refid", l.ParentMarkupLink.id),
                                        new XText(linkText)
                                        ));
                                }
                                else
                                {
                                    range.Add(new XText(linkText));
                                }
                            }
                            else
                            {

                                //if (LinkCounter == 217) Debug.Print(LinkCounter.ToString());

                                //Debug.Print("main: " + l.id + " " + linkText + "///" + l.regexpName);

                                var x = from d in xr.node.Ancestors().OfType<XElement>()
                                        join n in nonmarkupnodes on new { name = d.Name.LocalName } equals new { name = n }
                                        select d;



                                if (x.Count() != 0)
                                {
                                    range.Add(new XText(linkText));
                                }
                                else
                                {
                                    InternalItem InternalParent = null;
                                    if (InternalItems != null)
                                    {
                                        IEnumerable<string> parentIds = xr.node
                                                                    .Ancestors()
                                                                    .OfType<XElement>()
                                                                    .Where(p =>
                                                                        p.Attribute("id") != null
                                                                    )
                                                                    .Select(p => p.Attribute("id").Value)
                                                                    .GroupBy(p => p)
                                                                    .Select(p => p.Key);

                                        if (parentIds.FirstOrDefault() != null)
                                        {
                                            InternalParent = (from ip in InternalItems.Where(p => p.area == "segment")
                                                              join p in parentIds on
                                                              new { id = ip.id.ToString() }
                                                              equals
                                                              new
                                                              {
                                                                  id = p
                                                              }
                                                              select ip).ToList().FirstOrDefault();
                                        }
                                    }
                                    if (linkText.IndexOf("tvang") != -1) Debug.Print("xxxx");
                                    //if (l.id == "m463") Debug.Print("xxxx");
                                    IEnumerable<XElement> idlinks = GetIdLinks(l.id, InternalParent);
                                    //List<XElement> le = idlinks.DescendantsAndSelf().ToList();
                                    //if (le.Count() > 5) Debug.Print("xxx");
                                    bool added = false;

                                    if (idlinks.FirstOrDefault() != null)
                                    {
                                        if (idlinks.Where(p => p.Attribute("topic_id") == null).Count() != 0)
                                        {
                                            if (!added) markupIds.Add(l.id);
                                            added = true;
                                            range.Add(new XElement("diblink",
                                                new XAttribute("refid", l.id),
                                                new XAttribute("ided", 0),
                                                new XElement("idlinks",
                                                    new XAttribute("idlinkid", l.id)
                                                    , idlinks),
                                                new XText(linkText)
                                            ));
                                        }
                                        else
                                        {
                                            idlinks = idlinks.Where(p => p != null).OrderByDescending(p => p.Attribute("language").Value).ThenBy(p => p.Attribute("title").Value);
                                            if (!added) markupIds.Add(l.id);
                                            added = true;

                                            range.Add(new XElement("diblink",
                                            new XAttribute("refid", l.id),
                                            new XAttribute("ided", 1),
                                            new XElement("idlinks",
                                                new XAttribute("idlinkid", l.id)
                                                , idlinks),
                                            new XText(linkText)
                                            ));
                                        }
                                    }
                                    if (!added)
                                        range.Add(new XText(linkText));
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
                            //range.Add(new XText(rangeText.Substring(rangeCursor, rangeText.Length - rangeCursor)));
                        }

                        //if (((XElement)rangeNode.Parent).Attributes(XNamespace.Xml + "space").Count() == 0)
                        //{
                        //    XAttribute a = new XAttribute(XNamespace.Xml + "space", "preserve");
                        //    ((XElement)rangeNode.Parent).Add(a);
                        //}
                        rangeNode.ReplaceWith(range.Nodes());
                    }
                }
                #endregion
            }
            catch (SystemException err)
            {
                throw new Exception("MarkupLinks - Error: \r\n" + err.Message);
            }
        }
        private IEnumerable<XElement> GetIdLinks(string parentid, string ID_topic_id, InternalItem internalItem)
        {
            try
            {
                return identyfiedlinks
                        .Where(p => p.parent == parentid && p.ID_topic_id == ID_topic_id && !p.Deleted)
                        .OrderBy(p => p.pos)
                        .SelectMany(p => GetIdLinksData(p, internalItem)
                    );
            }
            catch (SystemException err)
            {
                throw new Exception("GetIdLinks - Error: \r\n" + err.Message);
            }

        }
        private IEnumerable<XElement> GetIdLinks(string parentid, InternalItem internalItem)
        {
            try
            {

                return identyfiedlinks
                        .Where(p => p.parent == parentid && !p.Deleted)
                        .OrderBy(p => p.pos)
                        .SelectMany(p => GetIdLinksData(p, internalItem)
                    );
            }
            catch (SystemException err)
            {
                throw new Exception("GetIdLinks - Error: \r\n" + err.Message);
            }

        }
        private XElement GetLinkElement(IdentyfiedLinks p,
                        InternalItem internalItem,
                        string topic_id,
                        string segment_id,
                        string bm,
                        string title,
                        string language,
                        int start
            )
        {
            try
            {
                return new XElement("idlink",
                            topic_id == null || topic_id == "" ? null : new XAttribute("topic_id", topic_id),
                            segment_id == null || segment_id == "" ? null : new XAttribute("segment_id", segment_id),
                            bm == null ? null : (bm == "" ? null : new XAttribute("bm", bm)),
                            title == null || title == "" ? null : new XAttribute("title", title),
                            language == null || language == "" ? null : new XAttribute("language", language),
                            new XAttribute("n", start.ToString()),
                            GetIdLinks(p.id, p.ID_topic_id, internalItem));
            }
            catch (SystemException err)
            {
                throw new Exception("GetLinkElement - Error: \r\n" + err.Message);
            }
        }
        private XElement EvalInternalTags(string tag1, string tag2, string tag3, string RegexpName)
        {
            try
            {
                XElement returnTag = null;
                XElement XTag2 = null;
                if (tag2 != "")
                {
                    //if (tag2 == "14-12") Debug.Print("xxx");
                    bool w = true;
                    while (w)
                    {
                        XTag2 = documentTags.Descendants("tag2").Where(q =>
                                        (q.Attribute("name") == null ? "" : q.Attribute("name").Value.ToLower()) == tag2.ToLower()
                                        && (
                                            q.Ancestors("tag1").First().Attribute("id") == null ? "" : q.Ancestors("tag1").First().Attribute("id").Value.ToLower()) == tag1.ToLower()
                                        ).FirstOrDefault();
                        if (XTag2 != null) break;

                        if (tag2.EndsWith(" i") || tag2.EndsWith(" å"))
                        {
                            tag2 = tag2.Substring(0, (tag2.Length - 2));
                        }
                        else
                            break;
                    }
                }

                if (XTag2 != null && tag3 != "")
                {
                    returnTag = XTag2.Descendants("tag3").Where(q =>
                         (q.Attribute("name") == null ? "" : q.Attribute("name").Value.ToLower()) == tag3.ToLower()
                        ).FirstOrDefault();

                    ;
                    if (returnTag == null && RegexpName == "para")
                    {
                        if (tag3.Length > 5 && (tag3.Length % 5) == 0)
                        {
                            int ant = tag3.Length / 5;
                            for (int i = ant - 1; i > 0; i--)
                            {
                                tag3 = tag3.Substring(0, (i * 5));
                                returnTag = XTag2.Descendants("tag3").Where(q =>
                                    (string)q.Attributes("name").FirstOrDefault() == tag3).FirstOrDefault();
                                if (returnTag != null)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    if (returnTag != null)
                    {
                        if (RegexpName == "para")
                            return new XElement("tag",
                                new XAttribute("topic_id", (string)returnTag.Ancestors("tag1").Attributes("topic_id").FirstOrDefault()),
                                new XAttribute("segment_id", (string)returnTag.Attributes("segment_id").FirstOrDefault()),
                                new XAttribute("id", (string)returnTag.Attributes("id").FirstOrDefault()),
                                new XAttribute("title", "§ " + (string)returnTag.Ancestors("tag2").Attributes("name").FirstOrDefault() + " " + rmActions._NumberWords.GetNumberwordsTitle(tag3))
                            );
                        else
                            return new XElement("tag",
                                new XAttribute("topic_id", (string)returnTag.Ancestors("tag1").Attributes("topic_id").FirstOrDefault()),
                                new XAttribute("segment_id", (string)returnTag.Attributes("segment_id").FirstOrDefault()),
                                new XAttribute("id", (string)returnTag.Attributes("id").FirstOrDefault()),
                                new XAttribute("title", (string)returnTag.Attributes("title").FirstOrDefault())
                            );

                    }
                }
                if (XTag2 != null)
                {
                    if (RegexpName == "para")
                    {
                        string tag2_title = (string)XTag2.Attributes("name").FirstOrDefault();
                        if (tag2_title.ToLower().StartsWith("kap") || tag2_title.ToLower().StartsWith("del"))
                            tag2_title = (string)XTag2.Attributes("title").FirstOrDefault();
                        else
                            tag2_title = "§ " + (string)XTag2.Attributes("name").FirstOrDefault();


                        return new XElement("tag",
                            new XAttribute("topic_id", (string)XTag2.Ancestors("tag1").Attributes("topic_id").FirstOrDefault()),
                            new XAttribute("segment_id", (string)XTag2.Attributes("segment_id").FirstOrDefault()),
                            new XAttribute("id", XTag2.GetAttributeValue("id")),
                            new XAttribute("title", tag2_title)
                        );
                    }
                    else
                        return new XElement("tag",
                            new XAttribute("topic_id", (string)XTag2.Ancestors("tag1").Attributes("topic_id").FirstOrDefault()),
                            new XAttribute("segment_id", XTag2.GetAttributeValue("segment_id")),
                            new XAttribute("id", XTag2.GetAttributeValue("id")),
                            new XAttribute("title", (string)XTag2.Attributes("title").FirstOrDefault())
                        );
                }

            }
            catch (SystemException err)
            {
                throw new Exception("EvalInternalTags - Error: \r\n" + err.Message);
            }
            return null;
        }
        private class tag3data
        {
            public string topic_id = "";
            public string segment_id = "";
            public string id = "";
            public string title = "";
            public string tag3 = "";
        }
        private XAttribute GetAttribute(string name, string value)
        {
            return value == null ? null : (value == "" ? null : new XAttribute(name, value));
        }
        private IEnumerable<XElement> GetIdLinksData(IdentyfiedLinks p, InternalItem internalItem)
        {
            try
            {
                Debug.Print(LinkCounter.ToString());
                //if (LinkCounter == 28) Debug.Print("xxxx");
                List<XElement> links = new List<XElement>();
                if (internalItem == null && p.topic_id == "")
                {
                    LinkCounter++;
                    links.Add(new XElement("idlink",
                         GetAttribute("tag1", p.tag1),
                         GetAttribute("tag2", p.tag2),
                         GetAttribute("tag2alt", p.tag2_alt),
                         GetAttribute("tag3", p.tag3),
                         GetAttribute("area", p.idArea == IdAreas.External ? "0" : "1"),
                         GetAttribute("language", p.language),
                         GetAttribute("n", p.pos.ToString()),
                         GetIdLinks(p.id, internalItem)
                     ));
                    return links.Select(m => m);
                }
                if (p.idArea == IdAreas.Internal && internalItem != null)
                {
                    string topic_id = "";
                    if (p.topic_id != "" && p.Internal)
                    {
                        LinkCounter++;
                        links.Add(GetLinkElement(p, internalItem, p.topic_id, p.segment_id, p.bm, p.title, p.language, p.pos));
                        return links.Select(m => m);
                    }
                    else if (p.tag1 == "")
                    {
                        topic_id = internalItem.topic_id;
                        string internal_segment_id = internalItem.id;
                        string language = this.Language;
                        if (p.tag2 != "" || p.tag3 != "")
                        {
                            //TILTAG
                            XElement tag = null;
                            if (p.totag2 != "" && p.totag3 == "")
                            {

                                foreach (string e in GetParaList(internal_segment_id, p.tag2, p.totag2).OrderBy(m => m, new NaturalSortComparer<string>()))
                                {

                                    tag = EvalInternalTags(internal_segment_id, e, p.tag3, p.RegexName);
                                    if (tag != null)
                                    {
                                        LinkCounter++;
                                        links.Add(GetLinkElement(p, internalItem, topic_id, (string)tag.Attributes("segment_id").FirstOrDefault(), (string)tag.Attributes("id").FirstOrDefault(), (string)tag.Attributes("title").FirstOrDefault(), language, p.pos));
                                    }

                                }
                                return links.Select(m => m);

                            }

                            if (p.tag2 != "" && p.tag3 != "" && p.totag3 != "")
                            {
                                if (p.tag2 == p.totag2 && p.tag3.Length == p.totag3.Length && p.tag3.Substring(0, 2) == p.totag3.Substring(0, 2))
                                {
                                    foreach (tag3data e in GetParaListTag3(internal_segment_id, p.tag2, p.tag2_alt, p.tag3, p.totag3).OrderBy(m => m.tag3, new NaturalSortComparer<string>()))
                                    {

                                        LinkCounter++;
                                        links.Add(
                                            new XElement("idlink",
                                                topic_id == null || topic_id == "" ? null : new XAttribute("topic_id", topic_id),
                                                e.segment_id == null || e.segment_id == "" ? null : new XAttribute("segment_id", e.segment_id),
                                                e.id == null ? null : (e.id == "" ? null : new XAttribute("bm", e.id)),
                                                e.title == null || e.title == "" ? null : new XAttribute("title", e.title),
                                                language == null || language == "" ? null : new XAttribute("language", language),
                                                new XAttribute("n", p.pos.ToString())
                                            )
                                        );

                                    }
                                }
                                if ((links == null ? 0 : links.Count()) != 0)
                                    return links.Select(m => m);

                            }

                            tag = EvalInternalTags(internal_segment_id, p.tag2, p.tag3, p.RegexName);
                            if (tag != null)
                            {
                                LinkCounter++;
                                links.Add(GetLinkElement(p, internalItem, topic_id, (string)tag.Attributes("segment_id").FirstOrDefault(), (string)tag.Attributes("id").FirstOrDefault(), (string)tag.Attributes("title").FirstOrDefault(), language, p.pos));
                                return links.Select(m => m);
                            }
                            else
                                //return null;
                                return links.Select(m => m);
                        }
                        else
                            //return null;
                            return links.Select(m => m);
                    }
                    else
                        //return null;
                        return links.Select(m => m);
                }
                else
                {
                    LinkCounter++;

                    links.Add(GetLinkElement(p, internalItem, p.topic_id, p.segment_id, p.bm, p.title, p.language, p.pos));
                    return links.Select(m => m);
                }

            }
            catch (SystemException err)
            {
                throw new Exception("GetIdLinksData - Error: \r\n" + err.Message);
            }

        }
        private List<tag3data> GetParaListTag3(string id, string tag2, string tag2_alt, string from, string to)
        {
            bool take = false;
            bool found = false;
            List<tag3data> result = new List<tag3data>();
            foreach (tag3data e in documentTags
                                 .Descendants("tag3")
                                 .Where(q =>
                                    (q.Ancestors("tag1").First().Attribute("id") == null ? "" : q.Ancestors("tag1").First().Attribute("id").Value.ToLower()) == id.ToLower()
                                    && q.Ancestors("tag2").First().Attribute("name").Value == tag2
                                 )
                                 .Select(q => new tag3data
                                 {
                                     title = "§ " + q.Ancestors("tag2").First().Attribute("name").Value + " " + rmActions._NumberWords.GetNumberwordsTitle(q.Attribute("name").Value.ToString()),
                                     segment_id = q.Attribute("segment_id").Value.ToString(),
                                     id = q.Attribute("id").Value.ToString(),
                                     tag3 = q.Attribute("name").Value.ToString(),
                                     topic_id = q.Ancestors("tag1").First().Attribute("topic_id").Value.ToString()
                                 })
                                 .Where(q => q.tag3.Length == from.Length)
                                 .OrderBy(q => q.tag3, new NaturalSortComparer<string>()))
            {
                if (e.tag3 == from && !take) take = true;
                if (take)
                {
                    result.Add(e);
                    if (e.tag3 == to)
                    {
                        found = true;
                        break;
                    }
                }
            }
            if (!found)
            {
                return new List<tag3data>();
            }
            return result;
        }
        private List<string> GetParaList(string id, string from, string to)
        {
            bool take = false;
            bool found = false;
            List<string> result = new List<string>();
            foreach (string e in documentTags
                                 .Descendants("tag2")
                                 .Where(q =>
                                    (q.Ancestors("tag1").First().Attribute("id") == null ? "" : q.Ancestors("tag1").First().Attribute("id").Value.ToLower()) == id.ToLower()
                                 )
                                 .Select(q => q.Attribute("name").Value.ToString())
                                 .OrderBy(q => q, new NaturalSortComparer<string>()))
            {
                if (e == from && !take) take = true;
                if (take)
                {
                    result.Add(e);
                    if (e == to)
                    {
                        found = true;
                        break;
                    }
                }
            }
            if (!found)
            {
                result = new List<string>();
                result.Add(from);
                return result;
            }
            return result;
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

    public class IdElement
    {
        public IdElements parent;
        public List<Range> ranges { get; set; }
        public List<IdLink> idLinks = new List<IdLink>();
        public StringBuilder text = new StringBuilder();
        public string text1 = "";
        public int pos = 0;
        public IEnumerable<Range> GetRanges()
        {
            return this.ranges;
        }
        private class RangeType
        {
            public XText t { get; set; }
            public int n { get; set; }
        }



        public IdElement(XElement element, IdElements parent, IEnumerable<string> excludeItems)
        {
            try
            {
                Regex replace160 = new Regex(@"\u00A0");
                this.parent = parent;

                StringBuilder s = new StringBuilder();

                foreach (XText t in element.DescendantNodesAndSelf().OfType<XText>())
                {
                    if (this.ranges == null) this.ranges = new List<Range>();
                    s = s.Clear();
                    if (parent.BreakNodes.FirstOrDefault() != null)
                    {
                        var an = from a in t.Ancestors()
                                 join bn in parent.BreakNodes on a.Name.LocalName equals bn
                                 where a.DescendantNodes().OfType<XText>().FirstOrDefault() == t
                                 select a.DescendantNodes().OfType<XText>().FirstOrDefault();

                        var ex = t.Ancestors().Where(p => (excludeItems.FirstOrDefault() == null ? 1 : excludeItems.Where(q => q == p.Name.LocalName).Count()) != 0).Count();
                        if (ex != 0)
                        {
                            if (an.Count() != 0)
                            {
                                s.Append(IdElements.sep + replace160.Replace(t.ToString(), @" ") + IdElements.buffer);

                                this.pos = this.pos + IdElements.offseth;
                            }
                            else
                            {
                                s.Append(replace160.Replace(t.ToString(), @" ") + IdElements.buffer);

                            }

                            this.ranges.Add(new Range
                            {
                                node = t,
                                pos = this.pos,
                                length = t.ToString().Length
                            });

                            this.text.Append(s);
                            this.pos = text.Length;
                        }
                    }
                    else
                    {
                        var an = from a in t.Ancestors()
                                 where a.DescendantNodes().OfType<XText>().FirstOrDefault() == t
                                 select a.DescendantNodes().OfType<XText>().FirstOrDefault();

                        var ex = t.Ancestors().Where(p => (excludeItems.FirstOrDefault() == null ? 1 : excludeItems.Where(q => q == p.Name.LocalName).Count()) != 0).Count();
                        if (ex != 0)
                        {
                            if (an.Count() != 0)
                            {
                                s.Append(IdElements.sep + replace160.Replace(t.ToString(), @" ") + IdElements.buffer);

                                this.pos = this.pos + IdElements.offseth;
                            }
                            else
                            {
                                s.Append(replace160.Replace(t.ToString(), @" ") + IdElements.buffer);

                            }

                            this.ranges.Add(new Range
                            {
                                node = t,
                                pos = this.pos,
                                length = t.ToString().Length
                            });
                            this.text.Append(s);
                            this.pos = text.Length;
                        }
                    }



                }

                if (this.text.Length != 0)
                {
                    IdElements.rmActions.Execute(this);
                }
            }
            catch (SystemException err)
            {
                throw new Exception("IdElement - Error: \r\n" + err.Message);
            }
        }


    }

    public static class Extentions
    {
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
        public static List<XElement> GetElementsBetween(this List<XElement> l, string From, string To)
        {
            List<XElement> returnValue = null;
            if (From == "" || To == "")
            {
                return returnValue;
            }
            
            
            if (l.Exists(p => p.Value.Trim().ToLower() == From.Trim().ToLower()) && l.Exists(p => p.Value.Trim().ToLower() == To.Trim().ToLower()))
            {

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
                foreach (XElement e in l.OrderBy(p => p.Value, new NaturalSortComparer<string>()))
                {
                    if (!take)
                    {
                        if (e.Value.Trim().ToLower() == From.Trim().ToLower())
                        {
                            take = true;
                        }
                    }
                    else
                    {
                        returnValue.Add(e);
                        if (e.Value.Trim().ToLower() == To.Trim().ToLower())
                        {
                            break;
                        }
                    }
                }
                if (returnValue.Count() == 0)
                    returnValue = null;
            }
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

        public static IEnumerable<IdElement> GetIdElements(this IEnumerable<XElement> elements, IdElements parent, IEnumerable<string> excludeItems)
        {
            return from XElement element in elements
                   where element.DescendantNodes().OfType<XText>().Count() != 0
                   select new IdElement(element, parent, excludeItems);

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
}
