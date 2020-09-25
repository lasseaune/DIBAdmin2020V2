using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace DIBIndexingCLR
{
    public class MatchObjects
    {
        public List<MatchObject> matches { get; set; }

        public MatchObjects(MatchCollection mc)
        {
            matches = mc.OfType<Match>().Select(p => new MatchObject(p)).ToList(); ;
        }
    }
    public class MatchObject
    {
        public string id = Guid.NewGuid().ToString();
        public bool processed = false;
        public string name { get; set; }
        public bool replace = false;
        public Match match { get; set; }
        public string matches { get; set; }
        public MatchObject(Match m)
        {
            match = m;

        }

    }
    public enum IdAreas
    {
        Internal = 0,
        External = 1
    }
    public class Markup
    {
        public string id { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public string regexpName { get; set; }
        public string value { get; set; }
        public bool replace = false;
        public string tag1 { get; set; }
        public string tag2 { get; set; }
        public string matches { get; set; }
        public string parentId { get; set; }
        public Markup(string parent_id, int start, int length, string name, string value, bool replace = false)
        {
            this.regexpName = name;
            this.start = start;
            this.length = length;
            this.value = value;
            this.replace = replace;
            this.id = Guid.NewGuid().ToString();
            this.parentId = parent_id;
        }
        public Markup(int start, int length, string name, string value, bool replace = false)
        {
            this.regexpName = name;
            this.start = start;
            this.length = length;
            this.value = value;
            this.replace = replace;
            this.id = Guid.NewGuid().ToString();
        }
    }
    public enum IdLinkType
    {
        Link = 0,
        Index = 1
    }
    public enum IndexType
    {
        Math = 0,
        Date = 1,
        Court = 2,
        Name = 3
    }
    public class IdLink
    {
        public IdLinkType type { get; set; }
        public string language { get; set; }
        public IdAreas idArea = IdAreas.External;
        public int start { get; set; }
        public int length { get; set; }
        public string id { get; set; }
        public string regexpName { get; set; }
        public string value { get; set; }
        public string indexValue { get; set; }

        public bool replace = false;
        public string parent { get; set; }

        public string tag1 = "";
        public string self_tag1 = "";
        public string tag2 = "";
        public string tag2_alt = "";
        public string tag3 = "";
        public string totag2 = "";
        public string totag2_alt = "";
        public string totag3 = "";
        public string prefix = "";
        public string linkprefix = "";
        public bool Identified = false;
        public string replaceText = "";
        public string replacePattern = "";
        public IdLink(RegexGroupRange rg, int start, int length, string value, IndexType IndexType, string parent)
        {
            this.id = Guid.NewGuid().ToString();
            type = IdLinkType.Index;
            this.start = start;
            this.length = length;
            this.value = value;
            this.parent = parent;
        }
        public IdLink(RegexGroupRange rg, int start, int length, string parent)
        {
            this.id = Guid.NewGuid().ToString();
            this.parent = parent;
            type = IdLinkType.Link;
            Dictionary<string, string> Result = rg.Result;
            this.start = start;
            this.length = length;
            this.language = Result.ContainsKey("language") ? Result["language"] : "";
            this.tag1 = (Result.ContainsKey("tag1") ? Result["tag1"] : "").Trim();
            this.tag1 = Regex.Replace(this.tag1, @"((.*?)(lov))(en|a)$", "$1");
            this.tag1 = Regex.Replace(this.tag1, @"((.*?))(\.)$", "$1");
            this.tag1 = Regex.Replace(this.tag1, @"\s+", " ");
            this.tag1 = Regex.Replace(this.tag1, @"slov", "lov");
            this.tag1 = Regex.Replace(this.tag1, @"[–\?]", "-");
            this.language = Result.ContainsKey("language") ? Result["language"] : "";
            this.tag2 = Result.ContainsKey("tag2") ? Result["tag2"].Trim() : "";
            this.tag2 = Regex.Replace(this.tag2, @"[–\?]", "-");
            this.tag2_alt = Result.ContainsKey("tag2_alt") ? Result["tag2_alt"] : "";
            this.tag3 = Result.ContainsKey("tag3") ? Result["tag3"].Trim() : "";

            if (Result.ContainsKey("internal"))
            {
                this.idArea = IdAreas.Internal;
            }
            if (Result.ContainsKey("prefix")) this.prefix = Result["prefix"];
            if (Result.ContainsKey("linkprefix")) this.linkprefix = Result["linkprefix"].Trim().ToLower();
            this.regexpName = Result.ContainsKey("name") ? Result["name"] : "";

        }
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
                             delegate (NumberWord p)
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
    public class RegexGroupRange
    {
        public RegexGroupRange top;
        public XTextRange parent;
        public Dictionary<string, string> Result = new Dictionary<string, string>();
        public List<RegexGroup> RangeList;
        public Markup mark { get; set; }
        public List<string> MatchGroups = new List<string>();
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
        public NumberWords _NumberWords { get; set; }
        public List<MatchAction> _MatchList { get; set; }
        public List<string> _GroupNames { get; set; }
        public RegexMatchActions() { }
        public RegexMatchActions(Regex regex, XElement actions, string listOfWords)
        {
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
        public class MatchAction
        {
            public string name { get; set; }
            public bool replace { get; set; }
            public string lang { get; set; }
            private RegexAction Actions;
            public RegexMatchActions Parent { get; set; }
            public MatchAction(XElement e, RegexMatchActions parent)
            {
                this.name = (string)e.Attributes("name").FirstOrDefault();
                this.replace = (string)e.Attributes("replace").FirstOrDefault() == "true" ? true : false;
                this.lang = (string)e.Attributes("lang").FirstOrDefault() == null ? null : (string)e.Attributes("lang").FirstOrDefault();
                this.Actions = new RegexAction(e, parent);
            }
            public void Execute(RegexGroupRange rg)
            {
                this.Actions.Execute(rg);
            }
        }
        private class RegexAction
        {
            private Regex rxPunktumNo = new Regex(@"(\/)?u\d+$");
            private string ENDASH = Convert.ToString((char)0x2013);
            private string type;
            private List<RegexAction> Actions;
            private Dictionary<string, string> Attributes;
            private string name = "";
            private RegexMatchActions Parent;
            public RegexAction(XElement e, RegexMatchActions parent)
            {
                Parent = parent;
                try
                {
                    XElement action = null;
                    while (e.Name.LocalName == "runaction")
                    {
                        action = e.Ancestors()
                            .Last()
                            .Elements("action")
                            .Where(p => (string)p.Attributes("name").FirstOrDefault() == (string)e.Attributes("name").FirstOrDefault()).FirstOrDefault();
                        if (action == null) return;
                        e = action;
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
                            case "note": ExecuteNote(ra, rg); break;
                            case "index": ExecuteIndex(ra, rg); break;
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
                            return returnValue;
                        }
                        catch
                        {

                        }
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
                                return returnValue;
                            }
                            catch
                            {
                            }
                        }

                    }

                    if (returnValue == "")
                    {
                        if (result.ContainsKey("year")
                            && result.ContainsKey("month")
                            && result.ContainsKey("day")
                            && result.ContainsKey("name")
                            )
                        {
                            try
                            {
                                DateTime date = DateTime.Parse(result["year"] + "-" + result["month"] + "-" + result["day"]);
                                returnValue = result["name"] + String.Format("{0:yyyyMMdd}", date);
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
                else dictionary.Add(key, value);
            }
            private void RemoveFromDictionary(Dictionary<string, string> dictionary, string key)
            {
                if (dictionary.ContainsKey(key)) dictionary.Remove(key);
            }
            private void GetParagrafElements(RegexAction ra, RegexGroupRange rg)
            {

                int line = 0;

                try
                {
                    KeyValuePair<string, string> groups = ra.Attributes.Where(p => p.Key == "groups").FirstOrDefault();
                    RegexGroup lovprefix = rg.RangeList.Where(p => p.name == "lovprefix").FirstOrDefault();
                    if (lovprefix != null)
                    {
                        AddToDictionary(rg.Result, "linkprefix", lovprefix.value);
                    }
                    line = 1;
                    RegexGroup paraTotal = rg.RangeList.Where(p => p.name == groups.Value).FirstOrDefault();
                    if (paraTotal == null) return;
                    List<RegexGroup> paraGroup = GetGroups(paraTotal, rg.RangeList);

                    List<RegexGroup> refGroups = paraGroup.Where(p => p.name == "lovrefs").ToList();

                    int min = rg.RangeList.Select(p => p.pos).Min();
                    int max = rg.RangeList.Select(p => p.pos + p.length).Max();
                    string parent_id = "";
                    line = 2;
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
                    line = 3;
                    string tag2 = "";
                    MatchTypeValue result;
                    string splitPara = "";
                    string para_id = "";
                    for (int i = 0; i < refGroups.Count(); i++)
                    {
                        line = 4;
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
                                                    || p.name == "vedlnr"
                                                    || p.name == "kapnr"
                                                    || p.name == "delnr"
                                                    || p.name == "split_para"
                                                )
                                            .OrderBy(p => p.pos).ThenByDescending(p => p.length)
                                            .ToList();

                            line = 5;
                            for (int j = 0; j < lovRefGroupSub.Count(); j++)
                            {

                                RemoveFromDictionary(rg.Result, "tag2");
                                RemoveFromDictionary(rg.Result, "tag2_alt");
                                RemoveFromDictionary(rg.Result, "tag3");
                                line = 6;
                                RegexGroup r = lovRefGroupSub.ElementAt(j);
                                switch (r.name)
                                {
                                    case "pararef":
                                        #region //pararef
                                        line = 7;
                                        List<RegexGroup> pararef = GetGroups(r, lovRefGroup).Where(p => p.name != "source_para").ToList();
                                        RegexGroup prefix = pararef.Where(p => p.name == "paraprefix").FirstOrDefault();
                                        if (prefix != null)
                                        {
                                            AddToDictionary(rg.Result, "linkprefix", prefix.value);
                                        }
                                        RegexGroup paranr = pararef.Where(p => p.name == "paranr").FirstOrDefault();
                                        if (paranr != null)
                                        {
                                            line = 8;
                                            List<ParaSubReference> parasubreference = new List<ParaSubReference>();
                                            tag2 = paranr.value.Trim();
                                            //AddToDictionary(rg.Result, "name", "paranr");
                                            AddToDictionary(rg.Result, "tag2", tag2);
                                            #region //PARA SUB
                                            List<RegexGroup> parasub = pararef.Where(p =>
                                                p.name != "paranr"
                                                && p.name != "source_para"
                                            ).ToList();

                                            if (parasub.Count() != 0)
                                            {
                                                //Debug.Print(paranr.name.PadRight(15, ' ') + paranr.pos.ToString() + "->" + paranr.length + " " + paranr.value);
                                                //PARA SUB REFERENCE
                                                line = 9;
                                                string pSingleLetter = "";
                                                for (int n = 0; n < parasub.Count(); n++)
                                                #region //PARA SUB REFERENCE
                                                {
                                                    line = 10;
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
                                                            line = 11;
                                                            switch (para.name)
                                                            {
                                                                case "leddnumber":
                                                                    //refid = ((int)MatchTypes.Ledd).ToString().PadLeft(2, '0') + result.Value.PadLeft(3, '0');
                                                                    refid = "l" + result.Value;
                                                                    break;
                                                                case "punumber":
                                                                    //refid = ((int)MatchTypes.Punktum).ToString().PadLeft(2, '0') + result.Value.PadLeft(3, '0');
                                                                    refid = "u" + result.Value;
                                                                    break;
                                                                case "pktnumber":
                                                                case "letternumber":
                                                                    //refid = ((int)result.Type).ToString().PadLeft(2, '0') + result.Value.PadLeft(3, '0');
                                                                    refid = para.value;
                                                                    if (parasub.Count() == 1 && para.name == "letternumber")
                                                                    {
                                                                        if (tag2.ToLower().EndsWith(para.value.ToLower()))
                                                                        {
                                                                            tag2 = tag2.Substring(0, tag2.Length - para.value.Length);
                                                                            //AddToDictionary(rg.Result, "name", "paranr");
                                                                        }
                                                                        AddToDictionary(rg.Result, "tag2", tag2);
                                                                        AddToDictionary(rg.Result, "tag2_alt", tag2 + para.value.Trim());
                                                                        pSingleLetter = para.value.Trim();
                                                                    }
                                                                    break;
                                                                case "pnsingleletter":
                                                                    {
                                                                        //refid = (99).ToString().PadLeft(2, '0') + result.Value.PadLeft(3, '0');
                                                                        refid = para.value;
                                                                        if (tag2.ToLower().EndsWith(para.value.ToLower()))
                                                                        {
                                                                            tag2 = tag2.Substring(0, tag2.Length - para.value.Length);
                                                                            //AddToDictionary(rg.Result, "name", "paranr");
                                                                        }
                                                                        AddToDictionary(rg.Result, "tag2", tag2);
                                                                        AddToDictionary(rg.Result, "tag2_alt", tag2 + para.value.Trim());
                                                                        pSingleLetter = para.value.Trim();

                                                                    }
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                        }

                                                    }
                                                    if (refid != "")
                                                    {
                                                        line = 12;
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

                                                bool bSingleLetter = false;
                                                if (parasubreference.Count() != 0)
                                                {
                                                    line = 13;
                                                    if (parasubreference.First().refid.StartsWith("split") && parasubreference.Count() == 1)// ? parasubreference.First().refid.Split('|')[0] : ""))
                                                    {
                                                        line = 14;
                                                    }
                                                    else
                                                    {
                                                        line = 15;
                                                        string split = "";
                                                        string tag3 = "";
                                                        int subPos = 0;
                                                        int sublength = 0;
                                                        string lastTag3 = "";
                                                        for (int m = 0; m < parasubreference.Count(); m++)
                                                        {
                                                            line = 16;
                                                            ParaSubReference psr = parasubreference.ElementAt(m);
                                                            if (m == 0 && psr.name == "pnsingleletter")
                                                            {
                                                                bSingleLetter = true;
                                                            }
                                                            else if (psr.refid.StartsWith("split"))
                                                            {
                                                                line = 17;
                                                                split = psr.refid.Split('|')[1];

                                                                if (tag3 != "")
                                                                {
                                                                    if (bSingleLetter)
                                                                    {
                                                                        string ntag2 = rg.Result["tag2_alt"];
                                                                        RemoveFromDictionary(rg.Result, "tag2");
                                                                        AddToDictionary(rg.Result, "tag2", ntag2);
                                                                        RemoveFromDictionary(rg.Result, "tag2_alt");
                                                                        bSingleLetter = false;
                                                                    }
                                                                    AddToDictionary(rg.Result, "tag3", tag3);
                                                                    para_id = AddIdLink(rg, (subPos == 0 ? paranr.pos : subPos), (sublength == 0 ? paranr.length : sublength), rg.Result, parent_id);
                                                                    RemoveFromDictionary(rg.Result, "tag3");
                                                                    lastTag3 = tag3;
                                                                    tag3 = "";
                                                                }

                                                                if ((m + 1) < parasubreference.Count())
                                                                {
                                                                    line = 18;
                                                                    ParaSubReference psrNext = parasubreference.ElementAt(m + 1);
                                                                    string refidNext = psrNext.refid;
                                                                    for (int prev = m; prev >= 0; prev--)
                                                                    {
                                                                        line = 19;
                                                                        ParaSubReference psrPrev = parasubreference.ElementAt(prev);

                                                                        if (psrPrev.name == psrNext.name)
                                                                        {
                                                                            line = 20;
                                                                            tag3 = psrPrev.lastTag3;
                                                                            break;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                line = 21;


                                                                if (rxPunktumNo.IsMatch(tag3))
                                                                {
                                                                    tag3 = rxPunktumNo.Replace(tag3, "");
                                                                }

                                                                psr.lastTag3 = tag3;
                                                                if (split == "til" && para_id != "")
                                                                {
                                                                    IdLink idlink = rg.parent.parent.idLinks.Where(p => p.id == para_id).FirstOrDefault();
                                                                    if ((idlink.tag3 == null ? "" : idlink.tag3) != "")
                                                                    {
                                                                        tag3 = (tag3 != "" ? tag3 + "/" : tag3) + psr.refid;
                                                                        AddToDictionary(rg.Result, "tag3", tag3);
                                                                        idlink.totag3 = tag3;
                                                                        lastTag3 = tag3;
                                                                        tag3 = "";
                                                                        subPos = psr.pos;
                                                                        sublength = psr.length;
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    tag3 = (tag3 != "" ? tag3 + "/" : tag3) + psr.refid;
                                                                    AddToDictionary(rg.Result, "tag3", tag3);
                                                                    subPos = psr.pos;
                                                                    sublength = psr.length;
                                                                }
                                                            }
                                                        }

                                                        if (tag3 != "" && (split.Trim().ToLower() == "til" || split.Trim().ToLower() == ENDASH))
                                                        {
                                                            line = 22;
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
                                                            line = 23;
                                                            split = "";
                                                            if (splitPara == "–") splitPara = "til";
                                                            if (splitPara == "til" && para_id != "")
                                                            {
                                                                line = 24;
                                                                AddToDictionary(rg.Result, "split", splitPara);
                                                                AddToDictionary(rg.Result, "to_parentid", para_id);
                                                                AddIdLink(rg, (subPos == 0 ? paranr.pos : subPos), (sublength == 0 ? paranr.length : sublength), rg.Result, parent_id);
                                                                RemoveFromDictionary(rg.Result, "split");
                                                                RemoveFromDictionary(rg.Result, "to_parentid");
                                                                splitPara = "";
                                                            }
                                                            else
                                                            {
                                                                line = 25;
                                                                para_id = AddIdLink(rg, (subPos == 0 ? paranr.pos : subPos), (sublength == 0 ? paranr.length : sublength), rg.Result, parent_id);
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    line = 26;
                                                    if (splitPara == "–") splitPara = "til";
                                                    if (splitPara == "til" && para_id != "")
                                                    {
                                                        line = 27;
                                                        AddToDictionary(rg.Result, "split", splitPara);
                                                        AddToDictionary(rg.Result, "to_parentid", para_id);
                                                        AddIdLink(rg, paranr.pos, paranr.length, rg.Result, parent_id);
                                                        RemoveFromDictionary(rg.Result, "split");
                                                        RemoveFromDictionary(rg.Result, "to_parentid");
                                                        splitPara = "";
                                                    }
                                                    else
                                                    {
                                                        line = 28;
                                                        para_id = AddIdLink(rg, paranr.pos, paranr.length, rg.Result, parent_id);
                                                    }
                                                }
                                            }
                                            #endregion
                                            else
                                            {
                                                line = 29;
                                                if (splitPara == "–") splitPara = "til";
                                                if (splitPara == "til" && para_id != "")
                                                {
                                                    line = 30;
                                                    AddToDictionary(rg.Result, "split", splitPara);
                                                    AddToDictionary(rg.Result, "to_parentid", para_id);
                                                    AddIdLink(rg, paranr.pos, paranr.length, rg.Result, parent_id);
                                                    RemoveFromDictionary(rg.Result, "split");
                                                    RemoveFromDictionary(rg.Result, "to_parentid");
                                                    splitPara = "";
                                                }
                                                else
                                                {
                                                    line = 31;
                                                    para_id = AddIdLink(rg, paranr.pos, paranr.length, rg.Result, parent_id);
                                                }
                                            }
                                        }
                                        break;
                                    #endregion
                                    case "vedlnr":
                                        line = 32;
                                        para_id = "";
                                        #region //kapnr
                                        result = ra.Parent._NumberWords.GetValue(r.value);
                                        tag2 = "v" + r.value.Trim();// + result.Value.PadLeft(4, '0');
                                        //AddToDictionary(rg.Result, "name", "kapnr");
                                        AddToDictionary(rg.Result, "tag2", tag2);
                                        AddIdLink(rg, r.pos, r.length, rg.Result, parent_id);
                                        break;
                                    #endregion
                                    case "kapnr":
                                        line = 32;
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
                                        line = 33;
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
                            line = 34;
                            RegexGroup next = refGroups.ElementAt(i + 1);

                            RegexGroup split = paraGroup
                                .Where(p =>
                                    p.pos >= lovRef.pos + lovRef.length
                                    && p.pos + p.length <= next.pos
                                    )
                                .FirstOrDefault();
                            if (split != null)
                            {
                                line = 35;
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
                            line = 36;
                            List<RegexGroup> rest = paraGroup
                                .Where(p =>
                                    p.pos >= lovRef.pos + lovRef.length
                                    )
                                    .ToList();
                            foreach (RegexGroup el in rest)
                            {
                                //Debug.Print("\n");
                                //Debug.Print("Slutter med");
                                //Debug.Print(el.name.PadRight(15, ' ') + el.pos.ToString() + "->" + el.length + " " + el.value);
                            }
                        }
                    }
                }
                catch (SystemException err)
                {

                    //throw new Exception("Error: 'GetParagrafElements' \r\n " + err.Message + " " + line.ToString());
                }
            }
            private string AddIdLink(RegexGroupRange rg, int pos, int length, Dictionary<string, string> result, string parentid)
            {
                IdLink idlink = null;
                string stage = "start AddIdLink";
                try
                {

                    if (rg.Result.ContainsKey("split") && rg.Result.ContainsKey("to_parentid"))
                    {
                        stage = "contain split";
                        idlink = rg.parent.parent.idLinks.Where(p => p.id == rg.Result["to_parentid"]).FirstOrDefault();

                        if (idlink != null)
                        {
                            int newLength = 0;
                            string pTag2 = idlink.tag2;
                            string totag2 = rg.Result.ContainsKey("tag2") ? rg.Result["tag2"] : "";
                            string totag2_alt = rg.Result.ContainsKey("tag2_alt") ? rg.Result["tag2_alt"] : "";
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

                            if (totag2 != "" && idlink.tag2 != totag2)
                            {
                                List<string> tag2s = new List<string>();
                                tag2s.Add(idlink.tag2);
                                tag2s.Add(totag2);
                                if (tag2s.OrderBy(p => p, new NaturalSortComparer<string>()).First() == idlink.tag2)
                                {
                                    newLength = (pos - idlink.start) + length;
                                    idlink.length = newLength; //idlink.length + newLength; //rettet fordi sublink markerte for langt
                                    idlink.totag2 = totag2;
                                    idlink.totag2_alt = totag2_alt;
                                    idlink.totag3 = totag3;
                                }
                                else
                                {
                                    RemoveFromDictionary(rg.Result, "split");
                                    RemoveFromDictionary(rg.Result, "to_parentid");
                                    idlink = new IdLink(rg, pos, length, parentid);
                                    if (rg.Result.ContainsKey("replace"))
                                    {
                                        idlink.replace = rg.Result["replace"] == "true" ? true : false;
                                    }
                                    if (rg.Result.ContainsKey("replacePattern"))
                                    {
                                        idlink.replacePattern = rg.Result["replacePattern"];
                                    }
                                    rg.parent.parent.idLinks.Add(idlink);
                                }
                            }
                            else
                            {
                                newLength = (pos - idlink.start) + length;
                                idlink.length = newLength; //idlink.length + newLength; //rettet fordi sublink markerte for langt
                                idlink.totag2 = totag2;
                                idlink.totag3 = totag3;
                            }
                        }
                    }
                    else
                    {
                        stage = "Add idlink";
                        idlink = new IdLink(rg, pos, length, parentid);
                        if (rg.Result.ContainsKey("replace"))
                        {
                            idlink.replace = rg.Result["replace"] == "true" ? true : false;
                        }
                        if (rg.Result.ContainsKey("replacePattern"))
                        {
                            idlink.replacePattern = rg.Result["replacePattern"];
                        }
                        rg.parent.parent.idLinks.Add(idlink);
                    }
                    return idlink.id;
                }
                catch (SystemException err)
                {
                    throw new Exception("Error: 'AddIdlink' \r\n " + "Stage: " + stage + "\r\n" + err.Message);
                }
            }
            private void ExecuteIndex(RegexAction ra, RegexGroupRange rg)
            {
                try
                {
                    if (ra.Attributes == null) return;
                    KeyValuePair<string, string> groups = ra.Attributes.Where(p => p.Key == "groups").FirstOrDefault();
                    KeyValuePair<string, string> replacePattern = ra.Attributes.Where(p => p.Key == "replace").FirstOrDefault();

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

                            IdLink idl = new IdLink(rg, pos, length, "", IndexType.Math, parentid);
                            rg.parent.parent.idLinks.Add(idl);
                            //string linkID = AddIdLink(rg, pos, length, rg.Result, parentid);


                        }
                    }
                }
                catch (SystemException err)
                {
                    throw new Exception("Error: 'ExecuteIndex' \r\n " + err.Message);
                }

            }
            private void ExecuteMark(RegexAction ra, RegexGroupRange rg)
            {
                string stage = "start";
                if (ra.Attributes == null) return;
                KeyValuePair<string, string> groups = ra.Attributes.Where(p => p.Key == "groups").FirstOrDefault();
                KeyValuePair<string, string> replacePattern = ra.Attributes.Where(p => p.Key == "replace").FirstOrDefault();
                try
                {
                    stage = "try";
                    //KeyValuePair<string, string> groups = ra.Attributes.Where(p => p.Key == "groups").FirstOrDefault();
                    //KeyValuePair<string, string> replacePattern = ra.Attributes.Where(p => p.Key == "replace").FirstOrDefault();

                    if (groups.Key != null)
                    {
                        StringBuilder sbText = new StringBuilder();
                        int pos = -1;
                        int length = -1;
                        foreach (string s in groups.Value.Split('|').Where(p => p != ""))
                        #region
                        {
                            stage = "foreach value=" + s;
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
                            stage = "pos";
                            string parentid = rg.Result["parentid"];
                            if (replacePattern.Key != null)
                            {
                                if (rg.Result.ContainsKey("replacePattern"))
                                    rg.Result["replacePattern"] = replacePattern.Value;
                                else
                                    rg.Result.Add("replacePattern", replacePattern.Value);
                            }

                            stage = "add link:" + sbText.ToString();
                            string linkID = AddIdLink(rg, pos, length, rg.Result, parentid);
                            stage = "link added";

                            if (rg.Result.ContainsKey("linkId"))
                                rg.Result["linkId"] = linkID;
                            else
                                rg.Result.Add("linkId", linkID);
                        }
                    }
                }
                catch (SystemException err)
                {
                    throw new Exception("Error: 'ExecuteMark' \r\n " + "Stage: " + stage + "\r\n" + "Groups: " + groups + "\r\n" + err.Message);
                }

            }
            private void ExecuteNote(RegexAction ra, RegexGroupRange rg)
            {
                string parentid = rg.Result["parentid"];
                if (parentid == null) return;
                try
                {
                    if (ra.Attributes == null) return;
                    KeyValuePair<string, string> tag = ra.Attributes.Where(p => p.Key == "tag").FirstOrDefault();
                    KeyValuePair<string, string> groups = ra.Attributes.Where(p => p.Key == "groups").FirstOrDefault();
                    bool found = false;
                    StringBuilder sbTag = new StringBuilder();
                    if (groups.Key == null) return;
                    found = true;
                    int pos = -1;
                    int length = -1;
                    foreach (string search in groups.Value.Split('|').Where(p => p != ""))
                    {
                        string s = search;
                        if (s.StartsWith("$") && s.EndsWith("$"))
                        {
                            string strTag = s.Substring(1, s.Length - 2);
                            if (strTag.ToLower() == "_self")
                                strTag = "this";
                            sbTag.Append(strTag);
                        }
                        else
                        {
                            RegexGroup r = rg.RangeList.Where(p => p.name == s).FirstOrDefault();
                            if (r == null)
                            {
                                found = false;
                                break;
                            }
                            else if (r != null)
                            {
                                sbTag.Append(r.value);
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
                    if (found)
                    {
                        AddIdLink(rg, pos, length, rg.Result, parentid);
                        RemoveFromDictionary(rg.Result, tag.Value);
                    }

                }
                catch (SystemException err)
                {
                    throw new Exception("Error: 'ExecuteNote' \r\n " + err.Message);
                }
            }
            private void ExecuteInternal(RegexAction ra, RegexGroupRange rg)
            {
                if (ra.Attributes != null)
                {
                    KeyValuePair<string, string> prefix = ra.Attributes.Where(p => p.Key == "prefix").FirstOrDefault();
                    if (prefix.Value != null) rg.Result.Add("prefix", prefix.Value);
                }
                rg.Result.Add("internal", "true");
                ra.Execute(rg);
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
                            if (s.StartsWith("rx="))
                            {
                                Regex rx = new Regex(Regex.Split(s, "rx=").LastOrDefault());
                                RegexGroup r = rg.RangeList.Where(p => rx.IsMatch(p.name)).FirstOrDefault();
                                if (r != null)
                                {
                                    sbTag.Append(r.name);
                                }
                                else
                                {
                                    found = false;
                                }
                            }
                            else if (s.StartsWith("'") && s.EndsWith("'"))
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

                    //RegexAction raFalse = ra.Actions.Where(p => p.type == "false").FirstOrDefault();
                    if (found && ra.Actions.Where(p => p.type == "true").FirstOrDefault() != null)
                    {
                        ra = ra.Actions.Where(p => p.type == "true").FirstOrDefault();
                        ra.Execute(rg);
                    }
                    else if (!found && ra.Actions.Where(p => p.type == "false").FirstOrDefault() != null)
                    {
                        ra = ra.Actions.Where(p => p.type == "false").FirstOrDefault();
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
                                    newRg.Result.Add("to_parentid", toParentId);
                                else
                                    newRg.Result["to_parentid"] = toParentId;

                                if (!newRg.Result.ContainsKey("split"))
                                    newRg.Result.Add("split", "true");
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
                        groups.Where(p => !rg.MatchGroups.Contains(p)).ToList().ForEach(p => rg.MatchGroups.Add(p));
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
            public RegexGroups(Match m, RegexMatchActions actions)
            {
                try
                {
                    foreach (string grpName in actions._GroupNames)
                    {
                        if (m.Groups[grpName].Success)
                        {
                            foreach (Capture c in m.Groups[grpName].Captures)
                            {
                                if (this._RegexGroups == null) this._RegexGroups = new List<RegexGroup>();
                                if (c.Length > 0)
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
        public Markup ExecuteMatchActions(MatchObject m, string language, XTextRange range)
        {
            Markup mark = null;
            try
            {
                if (m.processed)
                {
                    string parentId = m.id;
                    mark = new Markup(parentId, m.match.Index, m.match.Length, m.name, m.match.Value, m.replace);
                    mark.matches = m.matches;
                    return mark;
                }
                else
                {
                    RegexGroups regexgroups = new RegexGroups(m.match, this);
                    var groups = from qrp in regexgroups._RegexGroups
                                 join ml in _MatchList on qrp.name equals ml.name
                                 select new { capture = qrp, action = ml };
                    foreach (var group in groups)
                    {
                        MatchAction ma = group.action;
                        RegexGroupRange rg = new RegexGroupRange
                        {
                            parent = range,
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

                        string parentId = m.id;
                        m.name = ma.name;
                        m.replace = ma.replace;
                        mark = new Markup(parentId, m.match.Index, m.match.Length, m.name, m.match.Value, m.replace);
                        rg.MatchGroups.Add(ma.name);
                        rg.mark = mark;
                        //string parentid = mark.id; // AddMarkup(rg, m, ma);

                        if (parentId != null)
                        {
                            //if (parentid == "m21") Debug.Print("xxx");
                            rg.Result.Add("parentid", parentId);
                            rg.Result.Add("name", ma.name);
                            rg.Result.Add("language", ma.lang == null ? language : ma.lang);
                            rg.Result.Add("replace", ma.replace == true ? "true" : "false");
                            ma.Execute(rg);
                        }
                        mark.matches = rg.MatchGroups.Select(p => p).StringConcatenate(";");
                        rg = null;
                        ma = null;
                    }
                    regexgroups = null;
                    groups = null;
                    return mark;
                }
            }
            catch (SystemException err)
            {
                throw new Exception("Error: 'RegexMatchActions.Execute' \r\n " + err.Message);
            }
        }
        public Markup ExecuteMatchActions(Match m, string language, XTextRange range)
        {
            Markup mark = null;
            try
            {
                RegexGroups regexgroups = new RegexGroups(m, this);
                var groups = from qrp in regexgroups._RegexGroups
                             join ml in _MatchList on qrp.name equals ml.name
                             select new { capture = qrp, action = ml };
                foreach (var group in groups)
                {
                    MatchAction ma = group.action;
                    RegexGroupRange rg = new RegexGroupRange
                    {
                        parent = range,
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



                    mark = new Markup(m.Index, m.Length, ma.name, m.Value, ma.replace);
                    rg.MatchGroups.Add(ma.name);
                    rg.mark = mark;
                    string parentid = mark.id; // AddMarkup(rg, m, ma);

                    if (parentid != null)
                    {
                        //if (parentid == "m21") Debug.Print("xxx");
                        rg.Result.Add("parentid", parentid);
                        rg.Result.Add("name", ma.name);
                        rg.Result.Add("language", ma.lang == null ? language : ma.lang);
                        rg.Result.Add("replace", ma.replace == true ? "true" : "false");
                        ma.Execute(rg);
                    }
                    mark.matches = rg.MatchGroups.Select(p => p).StringConcatenate(";");
                    rg = null;
                    ma = null;
                }
                regexgroups = null;
                groups = null;
                return mark;
            }
            catch (SystemException err)
            {
                throw new Exception("Error: 'RegexMatchActions.Execute' \r\n " + err.Message);
            }
        }
    }
    public class XTextRange
    {
        public XText Text { get; set; }
        //public StringBuilder IndexText = new StringBuilder();
        public int Offseth { get; set; }
        public int Length { get; set; }
        public XTextRanges parent { get; set; }
        public string Tag1 { get; set; }
        public string Tag2 { get; set; }

        public List<Markup> marks = new List<Markup>();

        public XTextRange(XText t, int offseth, XTextRanges Parent)
        {
            Text = t;
            parent = Parent;
            Offseth = offseth;
            Length = t.Value.Length;
            XAttribute aTag1 = t.Ancestors().Attributes().Where(p => "tag1".Split(';').Contains(p.Name.LocalName)).FirstOrDefault();
            if (aTag1 != null)
            {
                Tag1 = (string)aTag1;
                Tag2 = (string)t.Ancestors().TakeWhile(p => p != aTag1.Parent).Attributes("tag2").FirstOrDefault();
            }

        }
    }

    public class XTextRanges
    {
        public StringBuilder Text = new StringBuilder();
        public List<XTextRange> xrange = new List<XTextRange>();
        public int Offseth = 0;
        public TextProcuctionObject parent { get; set; }
        public List<IdLink> idLinks = new List<IdLink>();
        public XTextRanges(TextProcuctionObject textprocuctionobject)
        {
            parent = textprocuctionobject;
        }
    }
    public class TextProcuctionObject
    {
        public XElement Links { get; set; }
        public XElement XIndex { get; set; }
        public List<XTextRanges> Ranges { get; set; }
        public RegexMatchActions Actions { get; set; }
        public string Language { get; set; }
        public bool InternalReference { get; set; }
        public int TextSize = 1000;
        public Regex TextElements { get; set; }
        public TextProcuctionObject() { }

        private Regex RX { get; set; }
        public void SetActions(Regex rx, XElement actions)
        {
            RX = rx;
            Actions = new RegexMatchActions(rx, actions, GetNumberWords());
            TextElements = new Regex(@"^(footnote|footnotes|section|p|x\-box|x\-box\-title|li|td|tr|th|div|sup|h1|h2|h3|h4|h5|h6|h7|table|ol|ul|blockquote|sup|sub)$");
        }
        public void ExecuteTextProcuction(XElement e, string language, bool internalreference = false)
        {
            InternalReference = internalreference;
            Language = language;
            Ranges = new List<XTextRanges>();
            Regex replace160 = new Regex(@"\u00A0");
            e.DescendantNodes().OfType<XText>().ToList().ForEach(p => p.ReplaceWith(new XText(replace160.Replace(p.Value, @" "))));
            e.DescendantNodes().OfType<XText>().ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(p.Value, @"\s+", @" "))));
            List<XElement> br = e.Descendants().Where(p => "br;hr".Split(';').Contains(p.Name.LocalName)).ToList();
            //List<XElement> br = e.Descendants().Where(p => p.Ancestors("document").FirstOrDefault() != null && "br;hr".Split(';').Contains(p.Name.LocalName)).ToList();
            List<XText> firstAfterBr = new List<XText>();
            foreach (XElement b in br)
            {
                firstAfterBr.Add(e.DescendantNodes().SkipWhile(p => p != b).Skip(1).OfType<XText>().FirstOrDefault());
            }

            //EvaluateXTexts(e.DescendantNodes().Where(p => p.Ancestors("document").FirstOrDefault() != null).OfType<XText>(), firstAfterBr);
            EvaluateXTexts(e.DescendantNodes().OfType<XText>(), firstAfterBr);

            foreach (XTextRanges r in Ranges)
            {
                MatchCollection mc = RX.Matches(r.Text.ToString());
                EvaluateMatches(r, mc);
            }
            Links = GetLinks();
            XIndex = new XElement("x-indexs",
                    e
                    .Descendants("x-index")
                    .Select(p => new
                    {
                        id = (string)p.Attributes("id").FirstOrDefault(),
                        rid = (string)p.Attributes("rid").FirstOrDefault(),
                        text = p.DescendantNodes().OfType<XText>().Select(s => s.Value).StringConcatenate(),
                        matches = (string)p.Attributes("matches").FirstOrDefault(),
                        att = p.Attributes().Where(a=>!"id;rid;matches".Split(';').Contains(a.Name.LocalName))
                    })
                    .GroupBy(p => p.rid)
                    .Select(p => new {
                        id = p.Select(i => i.id).FirstOrDefault(),
                        matches = p.Select(i => i.matches).FirstOrDefault(),
                        text = p.Select(i => i.text).StringConcatenate(),
                        att = p.Select(i=>i.att).GroupBy(i=>i).Select(i=>i.Key),
                        n = p.Count()
                    }
                    )
                    .GroupBy(p => p.text)
                    .Select(p => new XElement("x-item",
                                new XAttribute("text", p.Key),
                                p.Select(i => new XElement("x-match",
                                    new XAttribute("matches", i.matches),
                                    new XAttribute("id", i.id),
                                    i.att.Select(a=>a)
                                    )
                                )
                        )
                    )
                );
        }

        private class IdentyfiedLinkGroups
        {
            public Markup mLink { get; set; }
            public IEnumerable<IdentyfiedLinkGroup> idGroups { get; set; }
        }
        private class IdentyfiedLinkGroup
        {
            public IdLink link { get; set; }
            public IEnumerable<IdLink> links { get; set; }

        }

        public XElement GetLinks()
        {
            XElement result = new XElement("diblink",
                new XAttribute("version", "2.0"),
                new XAttribute("id", "diblink"),
                (
                    from mlink in Ranges.SelectMany(p => p.xrange.SelectMany(m => m.marks))
                    join ilink in Ranges.SelectMany(p => p.idLinks).Where(p => p.type == IdLinkType.Link) on mlink.parentId equals ilink.parent
                    select new { m = mlink, id = ilink }
                 )
                .GroupBy(p => p.m.parentId)
                .Select(p => new IdentyfiedLinkGroups
                {
                    //Sjekk opp
                    mLink = p.Select(m => m.m).FirstOrDefault(),
                    idGroups = p.GroupBy(s => s.id)
                                .Select(s => new IdentyfiedLinkGroup
                                {
                                    link = s.Key
                                })
                })
                .Select(l =>
                    new XElement("idlinks",
                        new XAttribute("id", l.mLink.id),
                        new XAttribute("text", l.mLink.value),
                        new XAttribute("version", "2.0"),
                        l.mLink.tag1.IsNull() == null ? null : new XAttribute("self_tag1", l.mLink.tag1),
                        l.mLink.tag2.IsNull() == null ? null : new XAttribute("self_tag2", l.mLink.tag2),
                        l.idGroups.Select(p => p.link.tag1).FirstOrDefault().IsNull() == null ? null : new XAttribute("tag1", l.idGroups.Select(p => p.link.tag1).FirstOrDefault()),
                        l.idGroups.GroupBy(p => p.link.replaceText).OrderByDescending(p => p.Key).Select(p => p.Key).FirstOrDefault().IsNull() == null ? null : new XAttribute("replaceText", l.idGroups.GroupBy(p => p.link.replaceText).OrderByDescending(p => p.Key).Select(p => p.Key)),
                        l.mLink.regexpName.IsNull() == null ? null : new XAttribute("RegexName", l.mLink.regexpName),
                        l.mLink.matches.IsNull() == null ? null : new XAttribute("matches", l.mLink.matches),
                        l.idGroups
                        .Select(idGroup =>
                            new XElement("idlink",
                                idGroup.link.tag2.IsNull() == null ? null : new XAttribute("tag2", idGroup.link.tag2),
                                idGroup.link.tag3.IsNull() == null ? null : new XAttribute("tag3", idGroup.link.tag3),
                                idGroup.link.totag2.IsNull() == null ? null : new XAttribute("totag2", idGroup.link.totag2),
                                idGroup.link.totag3.IsNull() == null ? null : new XAttribute("totag3", idGroup.link.totag3)
                            )
                        ).Where(p => p.Attributes().Count() > 0)
                    )
                )
            );
            return result;
        }
        private void EvaluateMatches(XTextRanges Range, MatchCollection mc)
        {
            MatchObjects mo = new MatchObjects(mc);
            (
                from xr in Range.xrange
                select new
                {
                    r = xr,
                    marks = mo.matches
                                .Where(m =>
                                        (xr.Offseth.Between(m.match.Index, m.match.Index + m.match.Length, true))
                                    || ((xr.Offseth + xr.Length).Between(m.match.Index, m.match.Index + m.match.Length, true))
                                    || ((xr.Offseth <= m.match.Index) && ((xr.Offseth + xr.Length) >= (m.match.Index + m.match.Length)))
                                )
                                .ToList()
                }
            )
            .Where(p => p.marks.Count() != 0)
            .ToList()
            .ForEach(p => ReplaceXtext(p.r, p.marks));//, nonmarkupnodes, p.mark_elemets));
        }

        public void ReplaceXtext(XTextRange xr, List<MatchObject> matches)//, IEnumerable<string> nonmarkupnodes, List<IdLink> idls)
        {
            XElement range = new XElement("range");
            int rangeStart = xr.Offseth;
            int rangeCursor = xr.Offseth;
            int rangeLength = xr.Length;
            string linkText = "";
            string rangeText = xr.Text.Value;
            foreach (MatchObject m in matches.OrderBy(p => p.match.Index))
            {
                //Debug.Print(m.Value);
                Markup mark = Actions.ExecuteMatchActions(m, Language, xr);
                if (mark != null)
                {
                    if (xr.Tag1 != null)
                    {
                        mark.tag1 = xr.Tag1;
                        mark.tag2 = xr.Tag2;
                    }
                    xr.marks.Add(mark);
                    //if (mark.id == "m1025") Debug.Print(mark.value);
                    //if (mark.value.Trim() == "§§ 9-3 til 9-8") Debug.Print(mark.value); 
                    int markStart = mark.start;
                    int markLength = mark.length;
                    if (
                            (markStart >= xr.Offseth && markStart < xr.Offseth + xr.Length)
                        || (markStart + markLength > xr.Offseth && markStart + markLength <= xr.Offseth + xr.Length))
                    {
                        //Linkens start er foran starten på teksten
                        if (markStart < rangeCursor)
                        {
                            //sett Cursor tsubMark start
                            markLength = markLength - (rangeCursor - markStart);
                            markStart = rangeCursor;
                                
                        }
                        //Hvis linkens star er større enn Cursor legg tsubMark text foran link
                        if (markStart > rangeCursor)
                        {
                            string s = rangeText.Substring((rangeCursor - rangeStart), ((markStart - rangeStart) - (rangeCursor - rangeStart)));
                            if (s.Trim() == "")
                            {
                                range.Add(new XElement("span", new XText(s)));
                            }
                            else
                            {
                                range.Add(new XText(s));
                            }
                            rangeCursor = markStart;
                        }
                        //hvis lengden på linken er lengere enn lengden på rangens tekst forkort linken
                        if (((rangeCursor - rangeStart) + markLength) > rangeText.Length)
                        {
                            markLength = rangeText.Length - (rangeCursor - rangeStart);
                        }


                        if ((markLength - (rangeCursor - markStart)) > 0)
                        {
                            linkText = rangeText.Substring((rangeCursor - rangeStart), markLength - (rangeCursor - markStart));

                            rangeCursor = rangeCursor + (markLength - (rangeCursor - markStart));
                        }

                        List<IdLink> il = xr.parent.idLinks.Where(p => p.parent == mark.parentId).ToList();
                        if (il.Count() == 0 && linkText.Trim().Length > 0)
                        {
                            range.Add(
                                    new XElement("x-index",
                                        new XAttribute("id", mark.id),
                                        new XAttribute("rid", mark.parentId),
                                        new XAttribute("matches", mark.matches),
                                        mark.matches
                                        .Split(';').Skip(1)
                                        .Select(p => m.match.Groups[p].Success ? new XAttribute(p, m.match.Groups[p].Value) : null),
                                        linkText
                                    )
                            );
                        }
                        else
                        {
                            IdLinkType itype = il.GroupBy(p => p.type).Select(p => p.Key).FirstOrDefault();
                            switch (itype)
                            {
                                case IdLinkType.Index:
                                    {
                                        range.Add(
                                            new XElement("x-index",
                                                new XAttribute("id", mark.id),
                                                new XAttribute("rid", mark.parentId),
                                                new XAttribute("matches", mark.matches),
                                                mark.matches.Split(';').Skip(1)
                                                .Select(p=> m.match.Groups[p].Success ? new XAttribute(p, m.match.Groups[p].Value) : null),
                                                
                                                linkText
                                            )
                                        );
                                    }
                                    break;
                                case IdLinkType.Link:
                                    {
                                        range.Add(
                                            new XElement("x-link-to",
                                                new XAttribute("id", mark.id),
                                                new XAttribute("rid", mark.parentId),
                                                linkText
                                            )
                                        );
                                    }
                                    break;
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
                if (xr.Text.Parent != null)
                {
                    xr.Text.ReplaceWith(range.Nodes());
                }
            }
        }
        private void EvaluateXTexts(IEnumerable<XText> XTexts, List<XText> brtext)
        {
            XTextRanges xtextranges = null;
            XTextRange xtextrange = null;
            string splitt = " | ";
            bool bSplitt = false;

            foreach (XText t in XTexts)
            {
                if (xtextranges == null)
                {
                    xtextranges = new XTextRanges(this);
                }

                if (brtext.Contains(t) || ((t.Parent.AncestorsAndSelf().Where(p => TextElements.IsMatch(p.Name.LocalName) && p.DescendantNodes().OfType<XText>().FirstOrDefault() == t).FirstOrDefault() != null)))
                {
                    xtextranges.Text.Append(splitt);
                    xtextranges.Offseth = xtextranges.Offseth + splitt.Length;
                    if (xtextranges.Offseth > TextSize)
                    {
                        Ranges.Add(xtextranges);
                        xtextranges = new XTextRanges(this);
                    }
                }

                xtextrange = new XTextRange(t, xtextranges.Offseth, xtextranges);

                xtextranges.xrange.Add(xtextrange);
                xtextranges.Text.Append(t.Value);
                xtextranges.Offseth = xtextranges.Offseth + t.Value.Length;
            }
            if (xtextranges.Text.Length > 0)
                Ranges.Add(xtextranges);
        }

        private string GetNumberWords()
        {
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
            listOfNumbers = listOfNumbers + "ellevte=11;";
            listOfNumbers = listOfNumbers + "tolvte=12;";
            listOfNumbers = listOfNumbers + "trettende=13;";
            listOfNumbers = listOfNumbers + "fjortende=14;";
            listOfNumbers = listOfNumbers + "femtende=15;";
            listOfNumbers = listOfNumbers + "siste=999;";
            return listOfNumbers;
        }
    }

    public static class ProcuctionExtentions
    {

    }
}
