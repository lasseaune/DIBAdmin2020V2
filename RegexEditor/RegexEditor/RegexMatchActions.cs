using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Diagnostics;

namespace RegexEditor
{
    public class IdElement
    {
        public string text; 
    }
    public class RegexMatchActions
    {
        
        protected Regex regex;
        private List<MatchAction> _MatchList;
        protected static List<string> _GroupNames;
        public RegexMatchActions() { }
        public RegexMatchActions(Regex regex, XElement actions)
        {
            this.regex = regex;
            _MatchList = GetMatchList(actions);
            _GroupNames = GetGroupNames(regex);
            
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
            private RegexAction _RegexAction;
            public MatchAction(XElement e)
            {
                this.name = (string)e.Attributes("name").FirstOrDefault();
                this._RegexAction = new RegexAction(e);
            }
            public void Execute(RegexGroupRange rg)
            {
                _RegexAction.Execute(rg);
            }
        }
        private class RegexAction
        {
            private string type;
            private List<RegexAction> _RegexAction;
            private Dictionary<string, string> Attributes;
            public RegexAction(XElement e)
            {
                while (e.Name.LocalName == "runaction")
                {
                    e = e.Ancestors()
                        .Last()
                        .Elements("action")
                        .Where(p => (string)p.Attributes("name").FirstOrDefault() == (string)e.Attributes("name").FirstOrDefault()).FirstOrDefault();
                    if (e == null) return;
                }
                this.type = e.Name.LocalName;
                foreach (XAttribute a in e.Attributes())
                {
                    if (Attributes == null) Attributes = new Dictionary<string, string>();
                    Attributes.Add(a.Name.LocalName, a.Value);
                }
                this._RegexAction = e.Elements()
                                    .Select(p => new RegexAction(p))
                                    .ToList();
            }
            public void Execute(RegexGroupRange rg)
            {
                try
                {
                    bool stop = false;
                    foreach (RegexAction ra in _RegexAction)
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
                            default:
                                ra.Execute(rg); break;
                        }
                        if (stop)
                            break;
                    }
                }
                catch(SystemException err)
                {
                    throw new Exception("Error: 'Execute ' \r\n " + err.Message);
                }
            }

            private void ExecuteRun(RegexAction ra, RegexGroupRange rg)
            {
                try
                {
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

            private void GetParagrafElements(RegexAction ra, RegexGroupRange rg)
            {
                KeyValuePair<string, string> groups = ra.Attributes.Where(p => p.Key == "groups").FirstOrDefault();
                RegexGroup paraTotal = rg.RangeList.Where(p => p.name == groups.Value).FirstOrDefault();
                if (paraTotal == null) return;
                List<RegexGroup> paraGroup = GetGroups(paraTotal, rg.RangeList);

                List<RegexGroup> refGroups = paraGroup.Where(p => p.name == "lovrefs").ToList();
                string ID = "";
                for(int i =0; i < refGroups.Count();i++)
                {
                    ID = "";
                    RegexGroup lovRef = refGroups.ElementAt(i);

                    List<RegexGroup> lovRefGroup =
                                    GetGroups(lovRef, paraGroup);

                    List<RegexGroup> lovRefGroupSub = 
                                    lovRefGroup
                                    .Where(p =>
                                        p.name == "pararef"
                                            || p.name == "kapnr"
                                            || p.name == "delnr"
                                        )
                                    .OrderBy(p => p.pos).ThenByDescending(p => p.length)
                                    .ToList();

                        
                    for (int j = 0; j < lovRefGroupSub.Count();j++)
                    {
                        RegexGroup r = lovRefGroupSub.ElementAt(j);
                        switch (r.name)
                        {
                            case "pararef":

                                List<RegexGroup> pararef = GetGroups(r, lovRefGroup);
                                RegexGroup paranr = pararef.Where(p => p.name == "paranr").FirstOrDefault();
                                if (paranr != null)
                                {
                                    ID = ID + "P" + paranr.value;
                                    List<RegexGroup> parasub = pararef.Where(p=>p.name != "paranr").ToList();
                                    for (int n = 0; n < parasub.Count(); n++)
                                    {
                                        RegexGroup para = parasub.ElementAt(n);
                                        if (para.name.IndexOf("split") != -1)
                                        {
                                            Debug.Print("\r\n");
                                            Debug.Print(para.name.PadRight(15, ' ') + para.pos.ToString() + "->" + para.length + " " + para.value);
                                            Debug.Print("\r\n");
                                        }
                                        else
                                        {
                                            Debug.Print(para.name.PadRight(15, ' ') + para.pos.ToString() + "->" + para.length + " " + para.value);
                                        }
                                    }
                                }
                                break;
                            case "kapnr":
                                Debug.Print(r.name.PadRight(15, ' ') + r.pos.ToString() + "->" + r.length + " " + r.value);
                                break;
                            case "delnr":
                                Debug.Print(r.name.PadRight(15, ' ') + r.pos.ToString() + "->" + r.length + " " + r.value);
                                break;
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
                            Debug.Print("\r\n");
                            Debug.Print(split.name.PadRight(15, ' ') + split.pos.ToString() + "->" + split.length + " " + split.value);
                            Debug.Print("\r\n");
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
                            Debug.Print("\r\n");
                            Debug.Print("Slutter med");
                            Debug.Print(el.name.PadRight(15, ' ') + el.pos.ToString() + "->" + el.length + " " + el.value);
                        }
                    }
                }
            }
            private void ExecuteMark(RegexAction ra, RegexGroupRange rg)
            {
                try
                {
                    KeyValuePair<string, string> groups = ra.Attributes.Where(p => p.Key == "groups").FirstOrDefault();
                    if (groups.Key != null)
                    {
                        StringBuilder sbText = new StringBuilder();
                        int pos = -1;
                        int length = -1;
                        foreach (string s in groups.Value.Split('|').Where(p => p != ""))
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
                        if (pos != -1)
                        {
                            Debug.Print(pos.ToString() + "->" + length.ToString() + " : " + sbText.ToString() );
                        }
                    }
                }
                catch (SystemException err)
                {
                    throw new Exception("Error: 'ExecuteMark' \r\n " + err.Message);
                }
                
            }

            private void ExecuteGet(RegexAction ra, RegexGroupRange rg)
            {
                try
                {
                    KeyValuePair<string, string> tag = ra.Attributes.Where(p => p.Key == "tag").FirstOrDefault();
                    KeyValuePair<string, string> groups = ra.Attributes.Where(p => p.Key == "groups").FirstOrDefault();
                    KeyValuePair<string, string> function = ra.Attributes.Where(p => p.Key == "function").FirstOrDefault();
                    if (tag.Key == null) return;
                    if (function.Key != null)
                    {
                        KeyValuePair<string, string> values = ra.Attributes.Where(p => p.Key == "values").FirstOrDefault();
                    }
                    else
                    {
                        bool found = true;
                        if (groups.Key == null) return;
                        StringBuilder sbTag = new StringBuilder();
                        foreach (string s in groups.Value.Split('|').Where(p => p != ""))
                        {
                            if (s.StartsWith("$") && s.EndsWith("$"))
                            {
                                sbTag.Append(s.Substring(1, s.Length - 2));
                            }
                            else
                            {
                                RegexGroup r = rg.RangeList.Where(p => p.name == s).FirstOrDefault();
                                if (r == null)
                                {
                                    found = false;
                                    break;
                                }
                                else
                                    sbTag.Append(r.value);
                            }
                        }
                        if (found)
                        {
                            if (rg.Result == null) rg.Result = new Dictionary<string, string>();
                            rg.Result.Add(tag.Value, sbTag.ToString());
                        }

                        RegexAction raFalse = ra._RegexAction.Where(p => p.type == "false").FirstOrDefault();
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
                    KeyValuePair<string, string> queryType = ra.Attributes.Where(p => p.Key == "querytype").FirstOrDefault();
                    if (queryType.Key == null) return returnValue;
                    if (queryType.Value == "group")
                    {
                        KeyValuePair<string, string> group = ra.Attributes.Where(p => p.Key == "groups").FirstOrDefault();
                        foreach (RegexGroup gr in rg.RangeList.Where(p => p.name == group.Value))
                        {
                           RegexGroupRange newRg = new RegexGroupRange
                           {
                               top = rg.top == null ? rg : rg.top ,
                               parent = rg,
                               RangeList = rg.RangeList
                                               .Where(p =>
                                                   p.pos >= gr.pos
                                                   && (p.pos + p.length) <= (gr.pos + gr.length)
                                                   )
                                                   .ToList()
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
                    List<string> groups = ra.Attributes.Where(p => p.Key == "name").First().Value.Split('|').Where(p => p != "").Select(p => p).ToList();
                    if (groups.Where(p => rg.RangeList.Where(q => q.name == p).Count() == 0).Count() == 0)
                    {
                        returnValue = true;
                        RegexAction raTrue = ra._RegexAction.Where(p => p.type != "false").FirstOrDefault();
                        if (raTrue != null)
                        {
                            ra = raTrue;
                            ra.Execute(rg);
                        }
                    }
                    else
                    {
                        RegexAction raTrue = ra._RegexAction.Where(p => p.type == "true").FirstOrDefault();
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
            public RegexGroups(Match m)
            {
                GroupCollection groups = m.Groups;
                foreach (string grpName in _GroupNames)
                {
                    if (groups[grpName].Success)
                    {
                        foreach (Capture c in groups[grpName].Captures)
                        {
                            if (this._RegexGroups == null) this._RegexGroups = new List<RegexGroup>();
                            this._RegexGroups.Add(new RegexGroup(c,grpName));
                        }
                    }
                }

            }
        }
        private class RegexGroupRange
        {
            public RegexGroupRange top;
            public RegexGroupRange parent;
            public Dictionary<string, string> Result;
            public List<RegexGroup> RangeList;
        }
        private class RegexGroup
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

        public void Execute(IdElement element)
        {
            foreach (Match m in this.regex.Matches(element.text))
            {
                RegexGroups regexgroups = new RegexGroups(m);

                if (regexgroups._RegexGroups == null)
                {

                }
                else
                {
                    var groups = from qrp in regexgroups._RegexGroups
                                 join ml in _MatchList on qrp.name equals ml.name
                                 select new { capture = qrp, action = ml };
                    foreach (var group in groups)
                    {
                        MatchAction ma = group.action;
                        RegexGroupRange rg = new RegexGroupRange
                        {
                            Result = null,
                            RangeList = regexgroups._RegexGroups
                                            .Where(p =>
                                                p.pos >= group.capture.pos
                                                && (p.pos + p.length) <= (group.capture.pos + group.capture.length)
                                                )
                                                .ToList()
                        };
                        ma.Execute(rg);
                    }
                }
            }
        }
    }
}
