using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace DIBDocument
{
    public static class DocumentGenerator
    {
        public class ElementCounter
        {
            public int n { get; set; }
        }
        public class TitleEval
        {
            public XText f { get; set; }
            public string value { get; set; }
            public bool optional = false;
            public bool Default = false;
            public int number = -1;
            public int subnumber = -1;
            public string newValue { get; set; }
            public TitleEval(XElement t)
            {
                f = t.DescendantNodes().OfType<XText>().FirstOrDefault();
                if (f != null)
                {
                    value = f.Value;
                    Match m = rxOptionalTitle.Match(value);

                    optional = m.Groups["optional"].Success ? true : false;
                    Default = m.Groups["default"].Success ? false : true;
                    number = m.Groups["decimal"].Success ? Convert.ToInt32(m.Groups["decimal"].Value) : -1;
                    subnumber = m.Groups["subdecimal"].Success ? Convert.ToInt32(m.Groups["subdecimal"].Value) : -1;
                    if (subnumber != -1) subnumber = subnumber;
                    newValue = rxOptionalTitle.Replace(value, "").TrimStart();
                    if (newValue.TrimStart().StartsWith("."))
                    {
                        newValue = Regex.Replace(newValue.TrimStart(), @"^\.", "").TrimStart();
                    }
                    //t.Parent.Elements("level").Elements("title").ToList().ForEach(p => p.AddAnnotation(new TitleEval(p)));
                }
            }
        }

        public class DGText
        {
            public string text { get; set; }
        }
        public class DGMatchResult
        {
            public int index { get; set; }
            public int length { get; set; }

            public string value { get; set; }

            public List<XText> parent = new List<XText>();

            public DGMatchResult(Group g, XElement d)
            {

                List<string> t = d.DescendantNodesAndSelf().OfType<XText>()
                   .Where(p => p.Annotations<TextRange>().FirstOrDefault() != null)
                   .Select(p => new { an = p.Annotations<TextRange>().FirstOrDefault(), t = p })
                   .Select(p => p.an.pos.ToString() + "/" + p.an.length.ToString() + p.t.Value)
                   .ToList();

                index = g.Index;
                length = g.Length;
                value = g.Value;
                parent = d.DescendantNodesAndSelf().OfType<XText>().Where(p => p.Annotations<TextRange>().FirstOrDefault() != null)
                        .Where(p =>
                                index.Between(p.Annotation<TextRange>().pos, p.Annotation<TextRange>().pos + p.Annotation<TextRange>().length, true)
                            || (index + length).Between(p.Annotation<TextRange>().pos, p.Annotation<TextRange>().pos + p.Annotation<TextRange>().length, true)
                            || (
                                   index < p.Annotation<TextRange>().pos
                                && (index + length) > (p.Annotation<TextRange>().pos + p.Annotation<TextRange>().length)
                            )
                        )
                        .OrderBy(p => p.Annotation<TextRange>().pos)
                        .ToList();
                //.Where(p =>
                //       p.Annotation<TextRange>().pos <= index
                //       && (index + length) <= p.Annotation<TextRange>().pos + p.Annotation<TextRange>().length
                //    )
                //    .ToList();
            }

            public DGMatchResult(XText t)
            {
                TextRange tr = t.Annotations<TextRange>().FirstOrDefault();
                index = tr.pos;
                length = tr.length;
                value = t.Value;
                parent.Add(t);

            }
        }
        private enum DGPosition
        {
            Start = 0,
            End = 1
        }
        public enum DGVartype
        {
            global = 0,
            local = 1
        }
        public class DGVarData
        {
            private Regex NameSplitter = new Regex(@"\|\|");
            private Regex NameStandardSplitter = new Regex(@"\:\:");
            private Regex NameOptionsSplitter = new Regex(@"\/\/");
            public int index { get; set; }
            public int length { get; set; }
            public DGVartype vartype { get; set; }
            public string value { get; set; }
            public XText node { get; set; }

            public string name { get; set; }
            public string varname { get; set; }
            public string comment { get; set; }
            public string standard { get; set; }
            public int counter { get; set; }
            public bool IsCounter { get; set; }
            public bool IsDynCounter { get; set; }

            public XElement options { get; set; }
            private List<string> lvar_groupname = "name;standard;comment;option".Split(';').ToList();
            private List<string> lvar_subgroupname = "optionname;optionvalue".Split(';').ToList();
            public DGVarData(int Index, int Length, string Value, DGVartype VarType)
            {
                index = Index;
                length = Length;
                value = Value.Trim();
                vartype = VarType;
                if (value.Trim() != "") DecompVariableEx();
            }
            private void DecompVariableName(string tempName)
            {

                name = rxCounter.Replace(tempName,
                    delegate (Match m)
                    {
                        counter = m.Groups["counter"].Success ? Convert.ToInt32(m.Groups["counter"].Value) : -1;
                        return "";
                    }
                );

                name = rxDataCounter.Replace(name,
                    delegate (Match m)
                    {
                        IsCounter = m.Groups["datacounter"].Success ? true : false;
                        return "";
                    }
                );

                name = rxDynCounter.Replace(name,
                    delegate (Match m)
                    {
                        IsDynCounter = m.Groups["dyncounter"].Success ? true : false;
                        return m.Value;
                    }
                );

                varname = name.CreateVariableId();

            }
            private XElement DecompOption(string value)
            {
                string label = null;
                string text = null;
                if (NameStandardSplitter.Split(value).Count() == 2)
                {
                    label = (NameStandardSplitter.Split(value).ElementAtOrDefault(0) ?? "").Trim();
                    text = (NameStandardSplitter.Split(value).ElementAtOrDefault(1) ?? "").Trim();
                }
                else
                {
                    label = value.Trim();
                }
                if ((label == null ? "" : label).Trim() == "") return null;
                return new XElement("x-option-value",
                    new XAttribute("label", label),
                    text == null ? null : new XAttribute("value", text)
                );
            }
            private void DecompVariableEx()
            {
                MatchCollection mc = rxsvar_objects.Matches(value);
                foreach (Match m in mc)
                {
                    GroupCollection groups = m.Groups;
                    foreach (string grpName in lvar_groupname)
                    {
                        if (groups[grpName].Success)
                        {
                            foreach (Capture c in groups[grpName].Captures)
                            {
                                switch (grpName)
                                {
                                    case "name":
                                        {
                                            string tempName = c.Value.Trim();
                                            if (tempName != "")
                                                DecompVariableName(tempName);
                                        }
                                        break;
                                    case "standard":
                                        standard = c.Value.Trim();
                                        break;
                                    case "comment":
                                        comment = c.Value.Trim();
                                        break;
                                    case "option":
                                        {
                                            XElement voption = null;
                                            foreach (string subgrpName in lvar_subgroupname)
                                            {
                                                foreach (Capture cc in groups[subgrpName].Captures)
                                                {
                                                    if (cc.Index >= c.Index && (cc.Index + cc.Length) <= (c.Index + c.Length))
                                                    {
                                                        switch (subgrpName)
                                                        {
                                                            case "optionname":
                                                                {
                                                                    if (cc.Value.Trim() != "")
                                                                    {
                                                                        voption = new XElement("x-option-value", new XAttribute("label", cc.Value.Trim()));
                                                                    }

                                                                    break;
                                                                }
                                                            case "optionvalue":
                                                                {
                                                                    if (voption != null && cc.Value.Trim() != "")
                                                                        voption.Add(new XAttribute("value", cc.Value.Trim()));
                                                                }
                                                                break;
                                                        }
                                                    }
                                                }
                                            }
                                            if (voption != null && options == null) options = new XElement("x-option-values");
                                            if (voption != null) options.Add(voption);
                                            if (options!=null)
                                            { 
                                                XElement first = options.Elements("x-option-value").FirstOrDefault();
                                                if (first != null)
                                                {
                                                    standard = ((string)first.Attributes("value").FirstOrDefault() ?? "").Trim() == ""
                                                        ? ((string)first.Attributes("label").FirstOrDefault() ?? "")
                                                        : (string)first.Attributes("value").FirstOrDefault() ?? "";
                                                }
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                    }
                }


            }
            private void DecompVariable()
            {

                if (NameSplitter.IsMatch(value)) //||
                {
                    string tempName = NameSplitter.Split(value).FirstOrDefault();

                    DecompVariableName(tempName);

                    value = NameSplitter.Split(value).LastOrDefault();
                    if (NameStandardSplitter.IsMatch(value))
                    {
                        comment = NameStandardSplitter.Split(value).FirstOrDefault();
                        standard = NameStandardSplitter.Split(value).LastOrDefault();

                    }
                    else if (NameOptionsSplitter.IsMatch(value) && NameOptionsSplitter.Split(value).Count() > 0)
                    {
                        List<string> loptions = NameOptionsSplitter.Split(value).ToList();
                        options = new XElement("x-option-values",
                            loptions.Select(p => DecompOption(p))
                        );
                        standard = (string)options.Elements().Attributes("value").FirstOrDefault() ?? "";
                    }
                    else
                    {
                        comment = value;
                    }

                }
                else if (NameStandardSplitter.IsMatch(value)) //::
                {
                    string tempName = NameStandardSplitter.Split(value).FirstOrDefault();
                    DecompVariableName(tempName);
                    standard = NameStandardSplitter.Split(value).LastOrDefault();

                }
                else
                {
                    DecompVariableName(value);
                }
            }

        }
        public class DGListAnalyse
        {
            public string startName { get; set; }
            public int startDepth { get; set; }
            public string endName { get; set; }
            public int endDepth { get; set; }
            public string AncestorNane { get; set; }
            public XElement Ancestor { get; set; }
            public XElement Parent { get; set; }
            public XElement FirstSibling { get; set; }

            public XNode LastNode { get; set; }

            public IEnumerable<XNode> NodesBefore { get; set; }

            public int AncestorDepth = -1;

            public int Status { get; set; }
            public DGListAnalyse() { }
            public DGListAnalyse(XElement start, XElement end)
            {
                startName = start.Parent.Name.LocalName;
                startDepth = start.Ancestors().Count();
                endName = end.Parent.Name.LocalName;
                endDepth = end.Ancestors().Count();
                XElement ancestor =
                (
                    from a in start.AncestorsAndSelf()
                    join b in end.AncestorsAndSelf()
                    on a equals b
                    orderby a.Ancestors().Count() descending
                    select a
                ).FirstOrDefault();
                if (ancestor != null)
                {
                    Ancestor = ancestor;
                    AncestorNane = ancestor.Name.LocalName;
                    AncestorDepth = start.AncestorsAndSelf().OrderByDescending(p => p.AncestorsAndSelf().Count()).TakeWhile(p => p != ancestor).Count();


                    if (AncestorDepth != -1)
                    {
                        Parent = start.AncestorsAndSelf().OrderByDescending(p => p.AncestorsAndSelf().Count()).ElementAt(AncestorDepth);

                        NodesBefore = Parent.Nodes().TakeWhile(p => p != start);

                        LastNode = Parent.DescendantNodes().LastOrDefault();

                        FirstSibling = (
                            from a in Parent.Nodes()
                            join b in end.AncestorsAndSelf()
                            on a equals b
                            orderby b.Ancestors().Count() descending
                            select b
                        ).FirstOrDefault();


                    }
                }
            }

        }
        public class DGVarReplace
        {
            public XText t { get; set; }
            public TextRange range { get; set; }
            public List<DGVarData> var { get; set; }

            public DGVarReplace() { }
        }
        private static string _x_var = "x-var";
        private static Regex rxCounter = new Regex(@"(?<countergroup>((\*(?<counter>(\d+))\*)))");
        private static Regex rxDataCounter = new Regex(@"(?<datecountergroup>((\|(?<datacounter>(data\-counter)))))");
        private static Regex rxDynCounter = new Regex(@"(?<dyncountergroup>((\*(?<dyncounter>(N))\*)))");
        private static Regex rxListe = new Regex(@"(?<start>(\((\*)(?!\))))(.*?)(?<end>(\*\)))");
        private static string sLocal = @"(?<local>((?<start>(\[\[))(?<value>(.*?))(?<end>(\]\]))))";
        private static string sGlobal = @"(?<global>((?<start>(\{\{))(?<value>(.*?))(?<end>(\}\}))))";
        private static Regex rxLocalorGlobal = new Regex(@"((" + sLocal + @")|(" + sGlobal + @"))");
        //private static Regex rxSettings = new Regex(@"\(SETTINGS(.*?)SETTINGS\)");
        private static Regex rxBox = new Regex(@"(?<start>(\[BOX(\=)?))(?<value>([\-\(\)\?\,\.\sa-zæøåA-ZÆØÅ0-9\=]*?\:\:))?(.*?)(?<end>(BOX\]))");
        private static string sBrevhode = @"(?<start>(\(BREVHODE))(?<value>((.*?)))(?<end>(BREVHODE\)))";
        private static string sLetterhead = @"(?<start>(\(LETTERHEAD))(?<value>((.*?)))(?<end>(LETTERHEAD\)))";
        private static Regex rxLetterhead = new Regex(@"(?<letterhead>((" + sBrevhode + @"|" + sLetterhead + @")))");
        private static Regex rxSettings = new Regex(@"(?<settings>((?<start>(\(S(ETTINGS|ettings)))(?<value>((.*?)))(?<end>(S(ETTINGS|ettings)\)))))");
        private static Regex rxComments = new Regex(@"(?<comment>((?<start>(\(\!))(?<value>((((.)(?!(\(\!)))*?)))(?<end>(\!\)))))");
        private static Regex rxAlternatives = new Regex(@"(?<alternatives>((?<start>(\(\%))(?<value>((((.)(?!(\(\%)))*?)))(?<end>(\%\)))))");
        private static Regex rxOptional = new Regex(@"(?<start>(\(\(\#\)))(?<value>([\sa-zæøåA-ZÆØÅ0-9\-]*?(\|\||\:\:)))?(.*?)(?<end>(\#\)))");
        private static Regex rxFN = new Regex(@"(?<start>(\(FN))(?<value>(.*?))(?<end>(FN\)))");
        private static Regex rxOptionalTitle = new Regex(@"^(\s)?(\((?<optional>([#]))?(?<default>([#]))?\))?(\s)?((?<decimal>(\d+))\.((?<subdecimal>(\d+)))?)?");

        private static Regex rxCommentSplit = new Regex(@"\:\:");

        private static string svar_splitter = @"(\|\||\:\:|\/\/|\=\=|$)";
        private static string svar_name = @"^((?<name>(.*?))(?=" + svar_splitter + "))";
        private static string svar_comment = @"((\|\|)(?<comment>(.*?))(?=" + svar_splitter + "))";
        private static string svar_standardverdi = @"((\:\:)(?<standard>(.*?))(?=" + svar_splitter + "))";
        private static string svar_optionname = @"((\/\/)(?<optionname>(.*?))(?=" + svar_splitter + "))";
        private static string svar_optionvalue = @"((\=\=)(?<optionvalue>(.*?))(?=" + svar_splitter + "))";

        private static string svar_option = @"(?<option>(" + svar_optionname + @"(" + svar_optionvalue + ")?))";

        private static string svar_objects = svar_name
            + "(("
            + @"(\s+)?"
            + svar_standardverdi
            + "|" + svar_comment
            + "|" + svar_option
            + ")+)?";
        private static Regex rxsvar_objects = new Regex(svar_objects);




        #region //BOX
        //public class Box
        //{
        //    public string  konto { get; set; }
        //    public string kontotype { get; set; }
        //    public string kontobm  { get; set; }
        //    public string partbm  { get; set; }
        //    public string boxstartid { get; set; }
        //    public string heading { get; set; }
        //    public string boxendid { get; set; }
        //    public List<XElement> boxlines = new List<XElement>();
        //    public XElement xboxformat { get; set; }
        //}
        //public static XElement TransformBoxToXBox(this XElement origin)
        //{
        //    XElement returnxml = null;
        //    var boxdata = origin.
        //                  Descendants("text").
        //                  Where(p => p.Value.ToLower().Contains(string.Format("[Box").ToLower())).
        //                  Descendants().
        //                  Select(p => {
        //                      if (!p.hasElementsWithBox() && p.Value.ToLower().Contains(string.Format("[Box").ToLower()))
        //                          return new Box
        //                          {
        //                              konto = "",
        //                              kontotype = "",
        //                              kontobm = "",
        //                              partbm = p.Ancestors("text").FirstOrDefault().Attribute("id").Value,
        //                              boxstartid = p.Attribute("id").Value,
        //                              heading = Regex.Matches(p.Value, @"(?<=(\[BOX=))([^:<]+)", RegexOptions.IgnoreCase).Count > 0 ?
        //                                        Regex.Matches(p.Value, @"(?<=(\[BOX=))([^:<]+)", RegexOptions.IgnoreCase)[0].Value.Trim() : string.Empty,
        //                          };
        //                      else
        //                          return null;
        //                  }).
        //                  Where(p => p != null);

        //    List<Box> lbox = boxdata.ToList();

        //    try
        //    {
        //        for (int i = 0; i < lbox.Count(); i++)
        //        {
        //            List<XElement> lines = new List<XElement>();
        //            try
        //            {
        //                XElement el = null;
        //                try
        //                {
        //                    el = origin.
        //                        Descendants("text").
        //                        Where(p => p.Attributes("id").Any() ? p.Attribute("id").Value == lbox[i].partbm : false).
        //                        Descendants().
        //                        Where(p => p.Attributes("id").Any() ? p.Attribute("id").Value == lbox[i].boxstartid : false).
        //                        First();

        //                    el = el != null && el.Value.ToLower().Contains(string.Format("Box]").ToLower()) ? el : null;

        //                    if (el == null)
        //                    {
        //                        el = origin.
        //                            Descendants("text").
        //                            Where(p => p.Attributes("id").Any() ? p.Attribute("id").Value == lbox[i].partbm : false).
        //                            Descendants().
        //                            Where(p => p.Attributes("id").Any() ? p.Attribute("id").Value == lbox[i].boxstartid : false).
        //                            First().
        //                            ElementsAfterSelf().
        //                            Where(p => p.Value.ToLower().Contains(string.Format("Box]").ToLower())).
        //                            First();
        //                    }
        //                }
        //                catch (Exception)
        //                {
        //                }
        //                lbox[i].boxendid = el != null ? el.Attributes("id").Any() ? el.Attribute("id").Value : string.Empty : lbox[i].boxstartid;
        //            }
        //            catch (Exception)
        //            {
        //            }
        //        }

        //        foreach (Box b in lbox)
        //        {
        //            try
        //            {
        //                var Firstline = origin.
        //                                    Descendants("text").
        //                                    Where(p => p.Attributes("id").Any() ? p.Attribute("id").Value == b.partbm : false).
        //                                    Descendants().
        //                                    Where(p => p.Attributes("id").Any() ? p.Attribute("id").Value == b.boxstartid : false).
        //                                    SkipWhile(x => x.Attribute("id").Value != b.boxstartid).
        //                                    First();
        //                b.boxlines.Add(Firstline.RemoveBoxCode(b.heading));

        //                if (b.boxstartid != b.boxendid)
        //                {
        //                    var Restoflines = origin.
        //                        Descendants("text").
        //                        Where(p => p.Attributes("id").Any() ? p.Attribute("id").Value == b.partbm : false).
        //                        Descendants().
        //                        Where(p => p.Attributes("id").Any() ? p.Attribute("id").Value == b.boxstartid : false).
        //                        SkipWhile(x => x.Attribute("id").Value != b.boxstartid).
        //                        First().
        //                        ElementsAfterSelf().
        //                        TakeUntilInclusive(x => x.Attribute("id").Value == b.boxendid);

        //                    foreach (XElement xe in Restoflines)
        //                        b.boxlines.Add(xe.RemoveBoxCode(b.heading));
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //            }
        //        }
        //        lbox.ConvertToXBox();
        //        returnxml = lbox.ReplaceBOXwithXBOX(origin);
        //    }
        //    catch (Exception)
        //    {
        //        returnxml = origin;
        //    }
        //    return returnxml;
        //}






        //public static void ConvertToXBox(this List<Box> listbox)
        //{
        //    foreach (Box b in listbox)
        //    {
        //        b.xboxformat = new XElement("x-box", new XAttribute("id", Guid.NewGuid().ToString()), b.heading == string.Empty ? null : new XAttribute("title", b.heading));

        //        foreach (XElement el in b.boxlines)
        //            b.xboxformat.Add(el);
        //    }
        //}

        //public static XElement ReplaceBOXwithXBOX(this List<Box> listbox, XElement origin)
        //{
        //    XElement returnxml = null;


        //    try
        //    {
        //        XElement editedxml = new XElement(origin);
        //        foreach (Box b in listbox)
        //        {
        //            //# Add xbox xml before firstline
        //            var first = editedxml.
        //                        Descendants("part").
        //                        Where(p => p.Attributes("bm").Any() ? p.Attribute("bm").Value == b.partbm : false).
        //                        Descendants().
        //                        Where(p => p.Attributes("id").Any() ? p.Attribute("id").Value == b.boxstartid : false).
        //                        First();

        //            if (first != null)
        //            {
        //                first.AddBeforeSelf(b.xboxformat);
        //            }

        //            //# Remove Box's original first, last and between lines
        //            List<string> removeElementList = new List<string>();
        //            removeElementList.Add(b.boxstartid);
        //            if (b.boxstartid != b.boxendid)
        //                removeElementList.Add(b.boxendid);

        //            foreach (XElement e in b.boxlines)
        //            {
        //                string id = e.Attributes("id").Any() ? e.Attribute("id").Value : string.Empty;
        //                if (!removeElementList.Exists(p => p == id))
        //                    removeElementList.Add(e.Attributes("id").Any() ? e.Attribute("id").Value : string.Empty);
        //            }

        //            foreach (string id in removeElementList)
        //            {
        //                if (id != string.Empty)
        //                {
        //                    var line = editedxml.
        //                                Descendants("part").
        //                                Where(p => p.Attributes("bm").Any() ? p.Attribute("bm").Value == b.partbm : false).
        //                                Descendants().
        //                                Where(p => p.Attributes("id").Any() ? p.Attribute("id").Value == id : false).
        //                                Last();

        //                    if (line != null)
        //                        line.Remove();
        //                }
        //            }
        //        }
        //        returnxml = editedxml;
        //    }
        //    catch (Exception e)
        //    {
        //        returnxml = origin;
        //    }

        //    return returnxml;
        //}

        //public static bool hasElementsWithBox(this XElement element)
        //{
        //    bool retval = false;
        //    foreach (XElement e in element.Descendants())
        //    {
        //        if (e.Value.ToLower().Contains(string.Format("[Box").ToLower()))
        //        {
        //            retval = true;
        //            break;
        //        }
        //    }

        //    return retval;
        //}

        //public static IEnumerable<T> TakeUntilInclusive<T>(this IEnumerable<T> data, Func<T, bool> predicate)
        //{
        //    int index = 0;
        //    var enumerator = data.GetEnumerator();
        //    while (enumerator.MoveNext())
        //    {
        //        index++;
        //        if (predicate(enumerator.Current)) { break; }
        //    }
        //    enumerator.Dispose();
        //    return data.Take(index);
        //}

        //public static XElement RemoveBoxCode(this XElement element, string heading)
        //{
        //    string innerxml = element.ToString();
        //    innerxml = Regex.Replace(innerxml, @"box\]", "", RegexOptions.IgnoreCase);
        //    innerxml = Regex.Replace(innerxml, @"\[box=", "", RegexOptions.IgnoreCase);
        //    innerxml = Regex.Replace(innerxml, @"\[box", "", RegexOptions.IgnoreCase);
        //    innerxml = Regex.Replace(innerxml, Regex.Escape(heading) + @"::", "", RegexOptions.IgnoreCase);
        //    innerxml = Regex.Replace(innerxml, Regex.Escape(heading) + @" ::", "", RegexOptions.IgnoreCase);
        //    innerxml = Regex.Replace(innerxml, @">\s+", ">", RegexOptions.IgnoreCase);
        //    return XElement.Parse(innerxml);
        //}
        #endregion
        public static void DGDibLink(this XElement d)
        {
            List<XElement> e = d.Descendants().Where(p => "diblink;dibparameter".Split(';').Contains(p.Name.LocalName)).ToList();
            foreach (XElement l in e)
            {
                if ((string)l.Attributes("ided").FirstOrDefault() == "0")
                {
                    string replaceText = (string)l.Attributes("replaceText").FirstOrDefault();
                    if (replaceText == null)
                    {
                        l.ReplaceWith(l.Nodes());
                    }
                    else
                    {
                        l.ReplaceWith(replaceText);
                    }
                }
                else
                {
                    if (l.Name.LocalName == "dibparameter")
                    {
                        string replaceText = ((string)l.Attributes("replaceText").FirstOrDefault() ?? "").Trim();
                        if (replaceText == "")
                        {
                            l.ReplaceWith(new XText(" "));
                        }
                        else
                        {
                            l.ReplaceWith(new XElement("a",
                                new XAttribute("class", "diblink"),
                                (string)l.Attributes("refid").FirstOrDefault() == null ? null : new XAttribute("data-refid", (string)l.Attributes("refid").FirstOrDefault()),
                                new XAttribute("data-replacetext", l.Value),
                                new XAttribute("href", "#" + (string)l.Attributes("refid").FirstOrDefault()),
                                new XText(replaceText)
                                )
                            );
                        }
                    }
                    else
                    {
                        l.ReplaceWith(new XElement("a",
                            new XAttribute("class", "diblink"),
                            (string)l.Attributes("refid").FirstOrDefault() == null ? null : new XAttribute("data-refid", (string)l.Attributes("refid").FirstOrDefault()),
                            new XAttribute("href", "#" + (string)l.Attributes("refid").FirstOrDefault()),
                            l.Nodes())
                        );
                    }
                }
            }

        }
        public static void SetDGText(this XElement d)
        {
            d.ConcatXText();
            StringBuilder sb = d.DescendantNodesAndSelf().OfType<XText>().DocumentText();
            d.RemoveAnnotations<DGText>();
            d.AddAnnotation(new DGText { text = sb.ToString() });
        }
        public static void DGReplaceVars(this XElement d, List<DGVarData> varData)
        {
            (
               from text in d.DescendantNodesAndSelf()
                            .OfType<XText>().Where(p => p.Annotation<TextRange>() != null)
                            .Select(p => new { text = p, data = p.Annotation<TextRange>() })
                            .Select(p => new DGVarReplace
                            {
                                t = p.text,
                                range = p.data,
                                var = varData.Where(m =>
                                    m.index >= p.data.pos
                                    && (m.index + m.length) <= (p.data.pos + p.data.length)
                                )
                                .OrderBy(m => m.index)
                                .ToList()
                            })
               where text.var.Count() != 0
               select text
            )
            .OrderBy(p => p.range.pos)
            .ToList()
            .ForEach(p => p.DGReplaceVar());

        }
        public static void DGReplaceVar(this DGVarReplace t)
        {
            TextRange xr = t.range;
            XNode rangeNode = t.t;
            XElement range = new XElement("range");
            string rangeText = t.t.Value;
            int rangeStart = xr.pos;
            int rangeCursor = xr.pos;
            int rangeLength = xr.length;
            string linkText = "";
            foreach (DGVarData vd in t.var.OrderBy(p => p.index))
            {

                int linkStart = vd.index;
                int linkLength = vd.length;

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

                XElement var = new XElement(_x_var,
                    new XAttribute("id", vd.varname),
                    new XAttribute("type", vd.vartype)

                );
                var.AddAnnotation(vd);
                range.Add(var);

            }
            if (range.HasElements)
            {
                if ((rangeCursor - rangeStart) < rangeText.Length)
                {
                    string clean = rangeText.Substring((rangeCursor - rangeStart), rangeText.Length - (rangeCursor - rangeStart));
                    range.Add(new XText(clean));

                }
                if (rangeNode.Parent != null)
                {
                    rangeNode.ReplaceWith(range.Nodes());
                }
            }
        }
        public static string CreateVariableId(this string text)
        {
            return text
               .Trim()
               .Replace(" ", "-S-")
               .Replace(".", "-D-")
               .Replace(",", "-C-")
               .Replace("%", "-P-")
               .Replace("/", "-SL-")
               .Replace("\"", "-Q-")
               .Replace("'", "-SQ-")
               .Replace("&lt;", "-LT-")
               .Replace("<", "-LT-")
               .Replace("&gt;", "-GT-")
               .Replace(">", "-GT-")
               .Replace("&amp;", "-A-")
               .Replace("&", "-A-")
               .Replace("?", "-QN-")
               .Replace("!", "-I-")
               .Replace("+", "-PL-")
               .Replace("(", "-BL-")
               .Replace(")", "-BR-")
               .Replace("{", "")
               .Replace("}", "")
               .Replace("[", "")
               .Replace("]", "");
        }
        private static void DGLocateLocalorGlobalVariableClean(this XElement d)
        {
            string text = (d.Annotation<DGText>() == null ? "" : (d.Annotation<DGText>().text == null ? "" : d.Annotation<DGText>().text));
            if (text == "") return;


            foreach (Match m in rxLocalorGlobal.Matches(text))
            {
                DGVartype vartype = 0;
                DGMatchResult var = null;
                if (m.Groups[DGVartype.global.ToString()].Success)
                {
                    vartype = DGVartype.global;
                    var = new DGMatchResult(m.Groups[DGVartype.global.ToString()], d);
                }
                else if (m.Groups[DGVartype.local.ToString()].Success)
                {
                    vartype = DGVartype.local;
                    var = new DGMatchResult(m.Groups[DGVartype.local.ToString()], d);
                }

                foreach (XText t in var.parent)
                {
                    if (("diblink;dibparameter;a").Split(';').Contains(t.Parent.Name.LocalName))
                    {
                        t.Parent.ReplaceWith(t.Parent.Nodes());
                    }
                }

            }
        }
        private static List<DGVarData> DGLocateLocalorGlobalVariable(this XElement d)
        {
            string text = (d.Annotation<DGText>() == null ? "" : (d.Annotation<DGText>().text == null ? "" : d.Annotation<DGText>().text));
            if (text == "") return null;

            List<DGVarData> varData = new List<DGVarData>();

            foreach (Match m in rxLocalorGlobal.Matches(text))
            {
                if (!@"{{}};[[]]".Split(';').Contains(m.Value.Trim().Replace(" ", "")))
                {
                    DGVartype vartype = 0;
                    DGMatchResult var = null;
                    if (m.Groups[DGVartype.global.ToString()].Success)
                    {
                        vartype = DGVartype.global;
                        var = new DGMatchResult(m.Groups[DGVartype.global.ToString()], d);
                    }
                    else if (m.Groups[DGVartype.local.ToString()].Success)
                    {
                        vartype = DGVartype.local;
                        var = new DGMatchResult(m.Groups[DGVartype.local.ToString()], d);
                    }

                    if (var != null)
                    {
                        DGMatchResult varStart = new DGMatchResult(m.Groups["start"], d);
                        DGMatchResult varEnd = new DGMatchResult(m.Groups["end"], d);
                        DGMatchResult varValue = new DGMatchResult(m.Groups["value"], d);



                        varData.Add(new DGVarData(var.index, var.length, varValue.value, vartype));
                    }
                }
            }

            return varData;
        }
        private static void ReplaceNode(DGMatchResult mr, string typeName, string oid, DGPosition pos, List<DGVarData> vardata)
        {
            List<XNode> nodelist = new List<XNode>();
            XElement tn = new XElement(typeName,
                    new XAttribute("oid", oid),
                    new XAttribute("position", pos.ToString())
            );

            int mStart = mr.index;
            int Cursor = mr.index;
            int mLength = mr.length;
            int mEnd = mStart + mLength;

            string tValue = "";
            foreach (XText t in mr.parent)
            {

                TextRange tr = t.Annotation<TextRange>();
                int tStart = tr.pos;
                int tLength = tr.length;
                int tEnd = tStart + tLength;
                //Hvis teksten har tekst foran

                bool range = false;

                if (!((tEnd <= mStart) || (mEnd <= tStart)))
                {
                    if (tStart <= mStart && mStart < tEnd && tEnd <= mEnd)
                    {
                        int start = mStart - tStart; //OK
                        int length = tLength >= start + mLength ? mLength : tLength - start;//mLength  -(mEnd -tEnd);
                        t.Replace(tn, tStart, tLength, start, length);
                        range = true;
                    }
                    else if (tStart < mStart && tEnd > mEnd)
                    {
                        int start = mStart - tStart; //OK
                        int length = tLength >= start + mLength ? mLength : tLength - start;
                        t.Replace(tn, tStart, tLength, start, length);
                        range = true;
                    }
                    else if (mStart <= tStart && tEnd >= mEnd)
                    {
                        int start = 0; //OK
                        int length = tLength >= start + mLength ? mLength : tLength - start;
                        t.Replace(tn, tStart, tLength, start, length);
                        range = true;
                    }
                    else if (mStart <= tStart && tEnd <= mEnd)
                    {
                        int start = 0;
                        int length = tLength;
                        t.Replace(tn, tStart, tLength, start, length);
                        range = true;
                    }
                    else
                    {
                        string test = tValue;
                    }


                    if (range)
                    {
                        t.RemoveAnnotations<TextRange>();
                        t.Remove();
                    }
                }

            }
        }

        private static void Replace(this XText n, XElement tn, int tStart, int tLength, int start, int length)
        {
            string tValue = n.Value;
            string s1 = tValue.Substring(0, start);
            if (s1 != "")
            {
                XText bT = new XText(s1);
                bT.AddAnnotation(new TextRange { pos = tStart, length = s1.Length });
                n.AddBeforeSelf(bT);
            }
            string s2 = tValue.Substring(start, length);
            if (s2 != "")
            {
                XCData xcd = tn.Nodes().OfType<XCData>().FirstOrDefault();
                if (xcd != null)
                {
                    xcd.Value = xcd.Value + s2;
                    tn.Add(xcd);
                }
                else
                {
                    xcd = new XCData(s2);
                    tn.Add(xcd);
                    n.AddBeforeSelf(tn);
                }
            }
            string s3 = tValue.Substring(start + length, tLength - (start + length));
            if (s3 != "")
            {
                XText bT = new XText(s3);
                bT.AddAnnotation(new TextRange { pos = tStart + start + length, length = s3.Length });
                n.AddBeforeSelf(bT);
            }

        }
        private static void ConvertText(this IEnumerable<XElement> texts, List<DGVarData> vardata)
        {
            foreach (XElement t in texts)
            {
                XElement part = new XElement(t);
                int n;
                part.SetDGText();
                n = part.DGMarkupPrepare(rxSettings, "x-settings", vardata);
                if (n > 0)
                {
                    part.DGCommentExtrakt("x-settings", n);
                }

                part.SetDGText();
                n = part.DGListsPrepare(vardata);
                if (n > 0)
                {
                    part.DGListsExtract(n, vardata);
                }
                part.SetDGText();

                n = part.DGMarkupPrepare(rxBox, "x-box", vardata);
                if (n > 0)
                {
                    part.DGBoxExtract("x-box", n);
                }

                if (part.Descendants("x-box").Where(p => p.Elements().Where(s => Regex.IsMatch(s.Name.LocalName, @"h\d")).Count() != 0).Count() != 0)
                {
                    List<XElement> xb = part.Descendants("x-box").Where(p => p.Elements().Where(s => Regex.IsMatch(s.Name.LocalName, @"h\d")).Count() != 0).ToList();

                    xb.ForEach(p => p.FlatToHierarcy(true));
                }
                if (part.Descendants("x-box").Where(p => p.Attributes("title").FirstOrDefault() == null).Count() != 0)
                {
                    List<XElement> xb = part.Descendants("x-box").Where(p => p.Attributes("title").FirstOrDefault() == null).ToList();
                    xb.ForEach(p => p.SetXboxTitle());
                }

                part.SetDGText();

                n = part.DGMarkupPrepare(rxFN, "x-fn", vardata);
                if (n > 0)
                {
                    part.DGFootnote("x-fn", n);
                }

                part.SetDGText();
                n = part.DGMarkupPrepare(rxComments, "x-comment", vardata);
                if (n > 0)
                {
                    part.DGCommentExtrakt("x-comment", n);
                }

                part.SetDGText();
                n = part.DGMarkupPrepare(rxOptional, "x-optional", vardata);
                if (n > 0)
                {
                    part.DGOptionalExtrakt("x-optional", n);
                }

                part.SetDGText();
                n = part.DGMarkupPrepare(rxAlternatives, "x-alternatives", vardata);
                if (n > 0)
                {
                    part.DGAlternativesExtract("x-alternatives", n);
                    List<XElement> xb = part.Descendants("x-alternative").Where(p => p.Elements().Where(s => Regex.IsMatch(s.Name.LocalName, @"h\d")).Count() != 0).ToList();
                    xb.ForEach(p => p.FlatToHierarcy(false));
                }

                part.SetDGText();
                n = part.DGMarkupPrepare(rxLetterhead, "x-letterhead", vardata);
                if (n > 0)
                {
                    part.DGLetterheadExtrakt("x-letterhead", n);
                }


                part.SetDGText();
                part.DGNewPage(vardata);


                part.SetDGText();

                part.DGAM(vardata);
                part.DGNB(vardata);

                part.Descendants("p").Where(p => p.Nodes().Count() == 0).ToList().ForEach(p => p.Remove());
                part.Descendants("p").Where(p => p.DescendantNodes().OfType<XText>().Select(s => s.ToString()).StringConcatenate().Trim() == "" && p.Elements("img").Count() == 1).ToList().ForEach(p => p.ReplaceWith(p.Elements("img")));
                part.Descendants("div").Where(p => p.Nodes().Count() == 0).ToList().ForEach(p => p.Remove());
                part.Descendants("ol").Where(p => p.Descendants().Where(s => s.Name.LocalName.StartsWith("x-")).Count() == 0 && p.DescendantNodes().OfType<XText>().Select(s => s.ToString()).StringConcatenate().Trim() == "").ToList().ForEach(p => p.Remove());
                part.Descendants("ul").Where(p => p.Descendants().Where(s => s.Name.LocalName.StartsWith("x-")).Count() == 0 && p.DescendantNodes().OfType<XText>().Select(s => s.ToString()).StringConcatenate().Trim() == "").ToList().ForEach(p => p.Remove());

                List<XElement> divs = part.Elements("div").Where(p => p.Elements().Where(s => Regex.IsMatch(s.Name.LocalName, @"h\d")).Count() != 0).ToList();
                if (divs.Count() != 0)
                {
                    foreach (XElement div in divs)
                    {
                        if (div.Elements().Where(s => Regex.IsMatch(s.Name.LocalName, @"h\d")).Count() == 1 && div.Elements().FirstOrDefault() == div.Elements().Where(s => Regex.IsMatch(s.Name.LocalName, @"h\d")).FirstOrDefault())
                        {
                            XElement header = div.Elements().Where(s => Regex.IsMatch(s.Name.LocalName, @"h\d")).FirstOrDefault();
                            header.Remove();
                            div.AddBeforeSelf(header);
                            if ((string)div.Attributes("class").FirstOrDefault() == null)
                            {
                                div.ReplaceWith(div.Nodes());
                            }
                            else
                            {

                            }
                        }
                        else if (div.Elements().Where(s => Regex.IsMatch(s.Name.LocalName, @"h\d")).Count() > 1)
                        {
                            div.ReplaceWith(div.Nodes());
                        }
                        else
                        {
                            if ((string)div.Attributes("class").FirstOrDefault() == null)
                            {
                                div.ReplaceWith(div.Nodes());
                            }
                            else
                            {

                            }
                        }
                    }
                }

                part.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d") && "td;th".Split(';').Contains(p.Parent.Name.LocalName)).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));

                t.ReplaceWith(part);
                if (vardata.Count() == 0)
                {

                    if (part.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).Count() != 0)
                    {
                        part.FlatToHierarcy();
                    }

                }
                else
                {
                    if (part.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).Count() != 0)
                    {
                        part.FlatToHierarcy();
                    }
                }
            }
        }
        private static void DGFootnote(this XElement d, string name, int n)
        {
            List<XCData> cdata = d.DescendantNodes().OfType<XCData>().ToList();
            cdata.ForEach(p => p.Remove());
            for (int i = 1; i <= n; i++)
            {
                XElement markupStart = d.Descendants(name).Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString() && (string)p.Attributes("position").FirstOrDefault() == DGPosition.Start.ToString()).FirstOrDefault();
                XElement markupEnd = d.Descendants(name).Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString() && (string)p.Attributes("position").FirstOrDefault() == DGPosition.End.ToString()).FirstOrDefault();

                if (markupStart != null && markupEnd != null)
                {
                    List<XNode> nodelist = new List<XNode>();
                    DGListAnalyse la = new DGListAnalyse(markupStart, markupEnd);
                    markupStart.AddAnnotation(la);

                    if (markupStart.NodesAfterSelf().Contains(markupEnd))
                    {
                        nodelist.AddRange(markupStart.NodesAfterSelf().TakeWhile(p => p != markupEnd));
                        if (nodelist.Where(p => p.NodeType == XmlNodeType.Element).Count() != 0)
                        {
                            foreach (XNode e in nodelist.Where(p => p.NodeType == XmlNodeType.Element).Select(p => p))
                            {
                                if (e.NodeType == XmlNodeType.Element)
                                {
                                    XElement temp = new XElement((XElement)e);
                                    switch (temp.Name.LocalName.ToLower())
                                    {
                                        case "a":
                                        case "em":
                                            {
                                                e.AddBeforeSelf(temp.Nodes());
                                                e.Remove();
                                            }
                                            break;
                                        default:
                                            {
                                                e.AddBeforeSelf(temp.Nodes());
                                                e.Remove();
                                            }
                                            break;
                                    }
                                }
                            }
                            nodelist = new List<XNode>();
                            nodelist.AddRange(markupStart.NodesAfterSelf().TakeWhile(p => p != markupEnd));
                        }
                        nodelist.ForEach(p => p.Remove());
                        markupStart.Add(nodelist);
                        markupEnd.Remove();
                    }
                    else
                    {

                    }

                    markupStart.Attributes("oid").Remove();
                    markupStart.Attributes("position").Remove();
                }

            }
        }

        private static void DGNB(this XElement d, List<DGVarData> vardata)
        {
            string text = (d.Annotation<DGText>() == null ? "" : (d.Annotation<DGText>().text == null ? "" : d.Annotation<DGText>().text));
            if (text == "") return;
            Regex r = new Regex(@"\(NB\)");
            string name = "x-nb";
            int n = 0;
            foreach (Match m in r.Matches(text))
            {
                n++;

                DGMatchResult markupStart = new DGMatchResult(m.Groups["start"], d);
                ReplaceNode(markupStart, name, n.ToString(), DGPosition.Start, vardata);
            }
        }

        private static void DGAM(this XElement d, List<DGVarData> vardata)
        {
            string text = (d.Annotation<DGText>() == null ? "" : (d.Annotation<DGText>().text == null ? "" : d.Annotation<DGText>().text));
            if (text == "") return;
            Regex r = new Regex(@"\(AM\)");
            string name = "x-am";
            int n = 0;
            foreach (Match m in r.Matches(text))
            {
                n++;

                DGMatchResult markupStart = new DGMatchResult(m.Groups["start"], d);
                ReplaceNode(markupStart, name, n.ToString(), DGPosition.Start, vardata);

            }
        }
        private static void DGNewPage(this XElement d, List<DGVarData> vardata)
        {
            string text = (d.Annotation<DGText>() == null ? "" : (d.Annotation<DGText>().text == null ? "" : d.Annotation<DGText>().text));
            if (text == "") return;
            Regex r = new Regex(@"\((Ny side|New Page)\)");
            string name = "x-pagebreak";
            int n = 0;
            foreach (Match m in r.Matches(text))
            {
                n++;

                DGMatchResult markupStart = new DGMatchResult(m.Groups["start"], d);
                ReplaceNode(markupStart, name, n.ToString(), DGPosition.Start, vardata);

            }

            for (int i = 1; i <= n; i++)
            {
                XElement newPage = d.Descendants(name).Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString() && (string)p.Attributes("position").FirstOrDefault() == DGPosition.Start.ToString()).FirstOrDefault();
                XElement first = d.DiveNodes(newPage);
            }
        }

        private static void DGOptionalExtrakt(this XElement d, string name, int n)
        {
            for (int i = 1; i <= n; i++)
            {
                XElement markupStart = d.Descendants(name).Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString() && (string)p.Attributes("position").FirstOrDefault() == DGPosition.Start.ToString()).FirstOrDefault();
                XElement markupEnd = d.Descendants(name).Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString() && (string)p.Attributes("position").FirstOrDefault() == DGPosition.End.ToString()).FirstOrDefault();

                if (markupStart != null && markupEnd != null)
                {

                    List<XNode> nodelist = new List<XNode>();
                    DGListAnalyse la = new DGListAnalyse(markupStart, markupEnd);
                    markupStart.AddAnnotation(la);

                    XElement first = la.Parent.DiveNodes(markupStart);
                    XElement last = la.Parent.DiveNodes(markupEnd);


                    nodelist.AddRange(first.NodesAfterSelf().TakeWhile(p => p != last));
                    nodelist.ForEach(p => p.Remove());
                    markupStart.Add(nodelist);
                    last.Remove();

                    XElement keyword = markupStart.Descendants(name + "-value").FirstOrDefault();
                    if (keyword != null)
                    {
                        XCData xcd = keyword.Nodes().OfType<XCData>().FirstOrDefault();
                        if (xcd != null)
                        {
                            string sKeyword = xcd.Value.TrimEnd('|').TrimEnd(':').Trim();
                            markupStart.Add(new XAttribute("keyword", sKeyword));
                            markupStart.Add(new XAttribute("id", sKeyword.CreateVariableId() + "_FreeElement"));
                        }
                        else
                        {
                            string id = (string)markupStart.Ancestors().Attributes("id").LastOrDefault();
                            markupStart.Add(new XAttribute("id", id + "_FreeElement"));
                        }
                        keyword.Remove();
                    }
                    else
                    {
                        string id = (string)markupStart.Ancestors().Attributes("id").LastOrDefault();
                        markupStart.Add(new XAttribute("id", id + "_FreeElement"));

                    }

                    List<XCData> cdata = markupStart.DescendantNodes().OfType<XCData>().ToList();
                    cdata.ForEach(p => p.Remove());
                    markupStart
                          .Descendants()
                          .Where(p =>
                                 "p;strong;span;em".Split(';').Contains(p.Name.LocalName)
                              && p.DescendantNodes().OfType<XElement>().Where(s => ("iframe;img;" + _x_var).Split(';').Contains(s.Name.LocalName)).Count() == 0
                              && p.DescendantNodesAndSelf().OfType<XText>().Select(s => s.ToString()).StringConcatenate().Trim() == "")
                          .Remove();

                    markupStart.RemoveFirstBr();
                    markupStart.RemoveLastBr();

                    markupStart.Attributes("oid").Remove();
                    markupStart.Attributes("position").Remove();

                    if ((markupStart.Elements("p").Count() != 0 || markupStart.Elements("table").Count() != 0)
                        && first.Name.LocalName == "p")
                    {
                        first.ReplaceWith(markupStart);
                    }

                    else if (markupStart.Parent.Name.LocalName == "p" && markupStart.Parent.Nodes().Count() == 1)
                    {
                        markupStart.Parent.ReplaceWith(markupStart);
                    }
                    else if (markupStart.Parent.Name.LocalName == "p" && markupStart.Parent.Nodes().Where(p => (p.NodeType == XmlNodeType.Element ? "br;x-optional".Split(';').Contains(((XElement)p).Name.LocalName) : false) == false).Count() == 0)
                    {
                        markupStart.Parent.ReplaceWith(markupStart);
                    }
                    else if (markupStart.Parent.Name.LocalName == "div" && markupStart.Parent.Elements().Count() == 1)
                    {
                        markupStart.Parent.ReplaceWith(markupStart);
                    }
                    else
                    {

                    }

                }

            }
        }
        private static void DGCommentExtrakt(this XElement d, string name, int n)
        {
            List<XCData> cdata = d.DescendantNodes().OfType<XCData>().ToList();
            cdata.ForEach(p => p.Remove());
            for (int i = 1; i <= n; i++)
            {
                XElement markupStart = d.Descendants(name).Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString() && (string)p.Attributes("position").FirstOrDefault() == DGPosition.Start.ToString()).FirstOrDefault();
                XElement markupEnd = d.Descendants(name).Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString() && (string)p.Attributes("position").FirstOrDefault() == DGPosition.End.ToString()).FirstOrDefault();

                if (markupStart != null && markupEnd != null)
                {
                    List<XNode> nodelist = new List<XNode>();
                    DGListAnalyse la = new DGListAnalyse(markupStart, markupEnd);
                    markupStart.AddAnnotation(la);

                    XElement first = la.Parent.DiveNodes(markupStart);
                    XElement last = la.Parent.DiveNodes(markupEnd);

                    nodelist.AddRange(first.NodesAfterSelf().TakeWhile(p => p != last));
                    nodelist.ForEach(p => p.Remove());
                    markupStart.Add(nodelist);
                    last.Remove();


                    XElement split = markupStart.DescendantNodes().OfType<XElement>().Where(p => p.Name.LocalName == "x-com-split" && (string)p.Attributes("oid").FirstOrDefault() == i.ToString()).FirstOrDefault();
                    if (split != null)
                    {
                        split = markupStart.DiveNodes(split);
                        nodelist = new List<XNode>();
                        nodelist.AddRange(markupStart.Nodes().TakeWhile(p => p != split));
                        nodelist.ForEach(p => p.Remove());
                        split.Remove();
                        string keyword = "";
                        foreach (XNode k in nodelist)
                        {
                            if (k.NodeType == XmlNodeType.Text)
                                keyword = keyword + ((XText)k).Value;
                            else if (k.NodeType == XmlNodeType.Element)
                                keyword = keyword + ((XElement)k).DescendantNodes().OfType<XText>().Select(s => s.Value.ToString()).StringConcatenate();
                        }
                        if (keyword.Trim() != "")
                            markupStart.Add(new XAttribute("title", keyword.Trim()));
                    }

                    markupStart
                          .Descendants()
                          .Where(p =>
                                 "p;strong;span;em".Split(';').Contains(p.Name.LocalName)
                              && p.DescendantNodes().OfType<XElement>().Where(s => ("iframe;img;" + _x_var).Split(';').Contains(s.Name.LocalName)).Count() == 0
                              && p.DescendantNodesAndSelf().OfType<XText>().Select(s => s.ToString()).StringConcatenate().Trim() == "")
                          .Remove();

                    markupStart.RemoveFirstBr();
                    markupStart.RemoveLastBr();

                    markupStart.Attributes("oid").Remove();
                    markupStart.Attributes("position").Remove();

                    if ((markupStart.Elements("p").Count() != 0 || markupStart.Elements("table").Count() != 0)
                        && first.Name.LocalName == "p")
                    {
                        first.ReplaceWith(markupStart);
                    }
                    else if (markupStart.Parent.Name.LocalName == "p" && markupStart.Parent.Nodes().Count() == 1)
                    {
                        markupStart.Parent.ReplaceWith(markupStart);
                    }


                }

            }
        }
        private static void DGLetterheadExtrakt(this XElement d, string name, int n)
        {
            List<XCData> cdata = d.DescendantNodes().OfType<XCData>().ToList();
            cdata.ForEach(p => p.Remove());
            for (int i = 1; i <= n; i++)
            {
                XElement markupStart = d.Descendants(name).Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString() && (string)p.Attributes("position").FirstOrDefault() == DGPosition.Start.ToString()).FirstOrDefault();
                XElement markupEnd = d.Descendants(name).Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString() && (string)p.Attributes("position").FirstOrDefault() == DGPosition.End.ToString()).FirstOrDefault();

                if (markupStart != null && markupEnd != null)
                {
                    List<XNode> nodelist = new List<XNode>();
                    DGListAnalyse la = new DGListAnalyse(markupStart, markupEnd);
                    markupStart.AddAnnotation(la);

                    XElement first = la.Parent.DiveNodes(markupStart);
                    XElement last = la.Parent.DiveNodes(markupEnd);

                    nodelist.AddRange(first.NodesAfterSelf().TakeWhile(p => p != last));
                    nodelist.ForEach(p => p.Remove());
                    markupStart.Add(nodelist);
                    last.Remove();

                    markupStart
                          .Descendants()
                          .Where(p =>
                                 "p;strong;span;em".Split(';').Contains(p.Name.LocalName)
                              && p.DescendantNodes().OfType<XElement>().Where(s => ("iframe;img;" + _x_var).Split(';').Contains(s.Name.LocalName)).Count() == 0
                              && p.DescendantNodesAndSelf().OfType<XText>().Select(s => s.ToString()).StringConcatenate().Trim() == "")
                          .Remove();

                    markupStart.RemoveFirstBr();
                    markupStart.RemoveLastBr();

                    markupStart.Attributes("oid").Remove();
                    markupStart.Attributes("position").Remove();

                    //if ((markupStart.Elements("p").Count() != 0 || markupStart.Elements("table").Count() != 0)
                    //    && first.Name.LocalName == "p")
                    //    first.ReplaceWith(markupStart);

                    if ((markupStart.Elements("p").Count() != 0 || markupStart.Elements("table").Count() != 0)
                        && first.Name.LocalName == "p")
                    {
                        first.ReplaceWith(markupStart);
                    }
                    else if (markupStart.Parent.Name.LocalName == "p" && markupStart.Parent.Elements().Count() == 1)
                    {
                        markupStart.Parent.ReplaceWith(markupStart);

                        //List<XNode> test =  markupStart.Nodes().Where(p => !(p.NodeType == XmlNodeType.Text || (p.NodeType == XmlNodeType.Element ? ((XElement)p).Name.LocalName : "") == _x_var)).ToList();

                        //if (test.Count()==0)                        {
                        //    List<XNode> temp = markupStart.Nodes().ToList();
                        //    markupStart.Nodes().ToList().ForEach(p => p.Remove());
                        //    markupStart.Add(new XElement("p", temp));
                        //}
                    }
                    else if (markupStart.Parent.Name.LocalName == "div" && markupStart.Parent.Elements().Count() == 1)
                    {
                        markupStart.Parent.ReplaceWith(markupStart);
                    }
                    else
                    {

                    }
                }

            }
        }
        private static void DGAlternativesExtract(this XElement d, string name, int n)
        {
            List<XCData> cdata = d.DescendantNodes().OfType<XCData>().ToList();
            cdata.ForEach(p => p.Remove());

            for (int i = 1; i <= n; i++)
            {
                XElement markupStart = d.Descendants(name).Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString() && (string)p.Attributes("position").FirstOrDefault() == DGPosition.Start.ToString()).FirstOrDefault();
                XElement markupEnd = d.Descendants(name).Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString() && (string)p.Attributes("position").FirstOrDefault() == DGPosition.End.ToString()).FirstOrDefault();

                if (markupStart != null && markupEnd != null)
                {
                    List<XNode> nodelist = new List<XNode>();
                    DGListAnalyse la = new DGListAnalyse(markupStart, markupEnd);
                    markupStart.AddAnnotation(la);

                    XElement first = la.Parent.DiveNodes(markupStart);
                    XElement last = la.Parent.DiveNodes(markupEnd);

                    nodelist.AddRange(first.NodesAfterSelf().TakeWhile(p => p != last));
                    nodelist.ForEach(p => p.Remove());
                    markupStart.Add(nodelist);
                    last.Remove();

                    XElement split = markupStart.DescendantNodes().OfType<XElement>().Where(p => p.Name.LocalName == "x-alt-split" && (string)p.Attributes("oid").FirstOrDefault() == i.ToString()).FirstOrDefault();
                    if (split != null)
                    {
                        split = markupStart.DiveNodes(split);
                        //markupStart = d.Descendants(name).Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString() && (string)p.Attributes("position").FirstOrDefault() == DGPosition.Start.ToString()).FirstOrDefault();
                        List<XNode> alt = new List<XNode>();

                        while (split != null)
                        {
                            nodelist = new List<XNode>();
                            nodelist.AddRange(markupStart.Nodes().TakeWhile(p => p != split));
                            nodelist.ForEach(p => p.Remove());
                            split.Remove();
                            alt.Add(new XElement("x-alternative", nodelist));
                            split = markupStart.DescendantNodes().OfType<XElement>().Where(p => p.Name.LocalName == "x-alt-split" && (string)p.Attributes("oid").FirstOrDefault() == i.ToString()).FirstOrDefault();
                            if (split != null)
                            {
                                split = markupStart.DiveNodes(split);
                            }
                        }
                        nodelist = new List<XNode>();
                        nodelist.AddRange(markupStart.Nodes());
                        nodelist.ForEach(p => p.Remove());
                        alt.Add(new XElement("x-alternative", nodelist));
                        int o = 1;
                        foreach (XElement a in alt)
                        {
                            XNode var = a.Descendants("x-alt-val-end").Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString()).FirstOrDefault();
                            if (var != null)
                            {
                                string title = var.NodesBeforeSelf().Where(p => p.NodeType != XmlNodeType.CDATA).OfType<XText>().Select(p => p.ToString()).StringConcatenate();
                                if (title != null)
                                {
                                    a.Add(new XAttribute("title", title.Trim()));
                                }
                                var.NodesBeforeSelf().Where(p => p.Parent.NodeType != XmlNodeType.CDATA).OfType<XText>().Remove();
                                var.Remove();
                            }
                            a.Add(new XAttribute("n", o.ToString()));

                            a
                           .Descendants()
                           .Where(p =>
                                  "p;strong;span;em".Split(';').Contains(p.Name.LocalName)
                               && p.DescendantNodes().OfType<XElement>().Where(s => ("iframe;img;" + _x_var).Split(';').Contains(s.Name.LocalName)).Count() == 0
                               && p.DescendantNodesAndSelf().OfType<XText>().Select(s => s.ToString()).StringConcatenate().Trim() == "")
                           .Remove();

                            a.RemoveFirstBr();
                            a.RemoveLastBr();

                            o++;
                        }
                        markupStart.Nodes().Remove();
                        markupStart.Add(alt);


                    }
                    if (markupStart.Parent.Nodes().Count() == 1 && markupStart.Parent.Name.LocalName == "p")
                    {
                        markupStart.Parent.ReplaceWith(markupStart);
                    }
                }
            }
        }
        private static void RemoveFirstBr(this XElement e)
        {

            XNode a1 = e.Nodes().FirstOrDefault();
            if ((a1 != null ? a1.NodeType == XmlNodeType.Text : false))
            {
                if (((XText)a1).Value.Trim() == "")
                {
                    a1.Remove();
                    a1 = e.Nodes().FirstOrDefault();
                }
            }
            if (a1 != null)
            {
                string a1Name = a1.NodeType == XmlNodeType.Element ? ((XElement)a1).Name.LocalName : "";

                if (a1Name != "")
                {
                    XNode a2 = a1.NextNode;
                    if (a2 != null)
                    {
                        string a2Name = a2.NodeType == XmlNodeType.Element ? ((XElement)a2).Name.LocalName : "";
                        if (a1Name == "br" && a2Name != "br")
                        {
                            a1.Remove();
                        }
                        if (a1Name == "p")
                        {
                            a2 = ((XElement)a1).FirstNode;
                            a2Name = a2.NodeType == XmlNodeType.Element ? ((XElement)a2).Name.LocalName : "";
                            if (a2Name == "br")
                            {
                                a2.Remove();
                            }
                        }
                    }
                }
            }
            e.Descendants("p").ToList().ForEach(p => p.RemoveFirstBr());
            e.Descendants("p").ToList().ForEach(p => p.RemoveLastBr());
        }
        private static void RemoveLastBr(this XElement e)
        {
            XNode a1 = e.Nodes().LastOrDefault();
            if (a1 != null)
            {
                string a1Name = a1.NodeType == XmlNodeType.Element ? ((XElement)a1).Name.LocalName : "";

                if (a1Name != "")
                {
                    XNode a2 = a1.PreviousNode;
                    if (a2 != null)
                    {
                        string a2Name = a2.NodeType == XmlNodeType.Element ? ((XElement)a2).Name.LocalName : "";
                        if (a1Name == "br" && a2Name != "br")
                        {
                            a1.Remove();
                        }
                    }
                }
            }
        }
        private static int DGMarkupPrepare(this XElement d, Regex r, string elementName, List<DGVarData> vardata)
        {
            string text = (d.Annotation<DGText>() == null ? "" : (d.Annotation<DGText>().text == null ? "" : d.Annotation<DGText>().text));
            if (text == "") return 0;
            int n = 0;
            foreach (Match m in r.Matches(text))
            {
                n++;

                DGMatchResult markupStart = new DGMatchResult(m.Groups["start"], d);
                ReplaceNode(markupStart, elementName, n.ToString(), DGPosition.Start, vardata);

                if (markupStart.value.Trim().ToLower() == "[box=")
                {
                    d.Descendants("x-box")
                        .Where(p =>
                               (string)p.Attributes("oid").FirstOrDefault() == n.ToString()
                            && ((string)p.Attributes("position").FirstOrDefault() ?? "").ToLower() == "start"
                        )
                        .ToList()
                        .ForEach(p => p.Add(new XAttribute("close", "true")));
                }


                DGMatchResult markupEnd = new DGMatchResult(m.Groups["end"], d);
                ReplaceNode(markupEnd, elementName, n.ToString(), DGPosition.End, vardata);

                DGMatchResult markupValue = new DGMatchResult(m.Groups["value"], d);
                if (markupValue.value != "" && elementName == "x-optional")
                {
                    ReplaceNode(markupValue, elementName + "-value", n.ToString(), DGPosition.End, vardata);
                }
                else if (markupValue.value != "" && elementName == "x-box")
                {
                    //markupValue = new DGMatchResult(m.Groups["value"], d);
                    ReplaceNode(markupValue, elementName + "-value", n.ToString(), DGPosition.Start, vardata);
                }
                else if (markupValue.value != "" && elementName == "x-comment")
                {
                    bool bSplit = false;
                    markupValue = new DGMatchResult(m.Groups["value"], d);
                    foreach (XText t in markupValue.parent)
                    {
                        Match mm = Regex.Match(t.Value, @"\:\:");
                        if (mm.Success)
                        {
                            bSplit = true;
                            if (mm.Length == t.Value.Length)
                                t.ReplaceWith(new XElement("x-com-split", new XAttribute("oid", n.ToString())));
                            else
                            {
                                TextRange tr = t.Annotations<TextRange>().FirstOrDefault();
                                string f = t.Value.Substring(0, mm.Index);
                                if (f != "")
                                {
                                    XText tf = new XText(f);
                                    tf.AddAnnotation(new TextRange { pos = tr.pos, length = f.Length });
                                    t.AddBeforeSelf(tf);
                                }
                                t.AddBeforeSelf(new XElement("x-com-split", new XAttribute("oid", n.ToString())));
                                string a = t.Value.Substring(mm.Index + mm.Length);
                                if (a != "")
                                {
                                    XText ta = new XText(a);
                                    ta.AddAnnotation(new TextRange { pos = tr.pos + mm.Index + mm.Length, length = a.Length });
                                    t.AddBeforeSelf(ta);
                                }
                                t.Remove();
                            }
                        }

                    }
                }
                else if (markupValue.value != "" && elementName == "x-alternatives")
                {
                    bool bSplit = false;
                    foreach (XText t in markupValue.parent)
                    {
                        Match mm = Regex.Match(t.Value, @"\|\|");
                        if (mm.Success)
                        {
                            bSplit = true;
                            if (mm.Length == t.Value.Length)
                                t.ReplaceWith(new XElement("x-alt-split", new XAttribute("oid", n.ToString())));
                            else
                            {
                                TextRange tr = t.Annotations<TextRange>().FirstOrDefault();
                                string f = t.Value.Substring(0, mm.Index);
                                if (f != "")
                                {
                                    XText tf = new XText(f);
                                    tf.AddAnnotation(new TextRange { pos = tr.pos, length = f.Length });
                                    t.AddBeforeSelf(tf);
                                }
                                t.AddBeforeSelf(new XElement("x-alt-split", new XAttribute("oid", n.ToString())));
                                string a = t.Value.Substring(mm.Index + mm.Length);
                                if (a != "")
                                {
                                    XText ta = new XText(a);
                                    ta.AddAnnotation(new TextRange { pos = tr.pos + mm.Index + mm.Length, length = a.Length });
                                    t.AddBeforeSelf(ta);
                                }
                                t.Remove();
                            }
                        }
                    }
                    markupValue = new DGMatchResult(m.Groups["value"], d);

                    foreach (XText t in markupValue.parent)
                    {
                        Match mm = Regex.Match(t.Value, @"\:\:");
                        if (mm.Success)
                        {
                            bSplit = true;
                            if (mm.Length == t.Value.Length)
                                t.ReplaceWith(new XElement("x-alt-val-end", new XAttribute("oid", n.ToString())));
                            else
                            {
                                TextRange tr = t.Annotations<TextRange>().FirstOrDefault();
                                string f = t.Value.Substring(0, mm.Index);
                                if (f != "")
                                {
                                    XText tf = new XText(f);
                                    tf.AddAnnotation(new TextRange { pos = tr.pos, length = f.Length });
                                    t.AddBeforeSelf(tf);
                                }
                                t.AddBeforeSelf(new XElement("x-alt-val-end", new XAttribute("oid", n.ToString())));
                                string a = t.Value.Substring(mm.Index + mm.Length);
                                if (a != "")
                                {
                                    XText ta = new XText(a);
                                    ta.AddAnnotation(new TextRange { pos = tr.pos + mm.Index + mm.Length, length = a.Length });
                                    t.AddBeforeSelf(ta);
                                }
                                t.Remove();

                            }
                        }

                    }

                }
            }
            return n;
        }
        public class SplitNodes
        {
            public List<XNode> before = new List<XNode>();

            public List<XNode> after = new List<XNode>();
            public XElement target { get; set; }
            public bool result { get; set; }
            public SplitNodes(XElement Parent, XElement Target)
            {
                if (Parent.Elements().Where(p => p == Target).FirstOrDefault() != null)
                {
                    before = Parent.Nodes().TakeWhile(p => p != Target).ToList();
                    after = Parent.Nodes().SkipWhile(p => p != Target).Skip(1).ToList();
                    target = new XElement(Target);
                    result = true;
                }
                else
                {
                    XElement top = Parent.Elements().Where(p => p.DescendantsAndSelf().Contains(Target)).FirstOrDefault();
                    SplitNodes sn = new SplitNodes(top, Target);
                    if (sn.result == true)
                    {
                        List<XNode> Before = Parent.Nodes().TakeWhile(p => p != top).ToList();
                        List<XNode> After = Parent.Nodes().SkipWhile(p => p != top).Skip(1).ToList();
                        Before.ForEach(p => p.Remove());
                        before.AddRange(Before);
                        before.Add(new XElement(top.Name.LocalName, sn.before));

                        After.ForEach(p => p.Remove());
                        after.Add(new XElement(top.Name.LocalName, sn.after));
                        after.AddRange(After);
                        result = true;
                    }
                }

            }
        }
        private static XElement DiveNodes(this XElement e, XElement target)
        {
            XElement parent = e.Elements().Where(s => s.DescendantNodesAndSelf().Contains(target)).FirstOrDefault();
            if (parent == target)
            {
                return parent;
            }
            else
            {

                SplitNodes sn = new SplitNodes(parent, target);
                if (sn.result == true)
                {
                    //sn.before.ForEach(p => p.Remove());
                    parent.AddBeforeSelf(new XElement(parent.Name.LocalName, sn.before));
                    //sn.after.ForEach(p => p.Remove());
                    parent.AddAfterSelf(new XElement(parent.Name.LocalName, sn.after));
                    parent.Nodes().Remove();
                    parent.Add(target);
                }
                return parent;
            }

        }
        private static void DGBoxExtract(this XElement d, string name, int n)
        {
            for (int i = 1; i <= n; i++)
            {
                XElement markupStart = d.Descendants(name).Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString() && (string)p.Attributes("position").FirstOrDefault() == DGPosition.Start.ToString()).FirstOrDefault();
                XElement markupEnd = d.Descendants(name).Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString() && (string)p.Attributes("position").FirstOrDefault() == DGPosition.End.ToString()).FirstOrDefault();
                if (markupStart != null && markupEnd != null)
                {
                    List<XNode> nodelist = new List<XNode>();
                    DGListAnalyse la = new DGListAnalyse(markupStart, markupEnd);

                    markupStart.AddAnnotation(la);

                    XElement first = la.Parent.DiveNodes(markupStart);
                    XElement last = la.Parent.DiveNodes(markupEnd);

                    nodelist.AddRange(first.NodesAfterSelf().TakeWhile(p => p != last));
                    nodelist.ForEach(p => p.Remove());
                    markupStart.Add(nodelist);
                    last.Remove();



                    XElement boxValue = markupStart.Descendants(name + "-value").Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString()).FirstOrDefault();
                    string title = "";
                    if (boxValue != null)
                    {
                        XCData c = boxValue.DescendantNodes().OfType<XCData>().FirstOrDefault();
                        title = Regex.Replace(c.Value, @"\=|\:\:", "").Trim();
                        if (title.IndexOf("(AM)") != -1)
                        {
                            title = title.Replace("(AM)", "").Trim();
                            markupStart.Add(new XAttribute("class", "am"));
                        }
                        if (title.IndexOf("(NB)") != -1)
                        {
                            title = title.Replace("(NB)", "").Trim();
                            markupStart.Add(new XAttribute("class", "nb"));
                        }
                        else if (title.Trim().ToLower().StartsWith("eksempel") || title.Trim().ToLower().StartsWith("eksempler"))
                        {
                            markupStart.Add(new XAttribute("class", "xsample"));
                        }
                        if (title.Trim() != "")
                        {
                            markupStart.Add(new XAttribute("title", title));
                        }

                        boxValue.Remove();
                    }
                    if (title == "")
                    {
                        markupStart.Descendants("p").Where(p => p.Nodes().Count() == 0).Remove();
                        XElement fp = markupStart.DescendantNodes().OfType<XElement>().Where(p => p.Name.LocalName == "p").FirstOrDefault();
                        if (fp != null)
                        {
                            string fpValue = fp.DescendantNodes().OfType<XText>().Select(p => p.ToString()).StringConcatenate();
                            if (fpValue.Trim().ToLower().StartsWith("eksempel"))
                            {
                                title = Regex.Replace(fpValue, @"\=|\:\:", "").Trim();
                                markupStart.Add(new XAttribute("title", title));
                                markupStart.Add(new XAttribute("class", "xsample"));
                                fp.Remove();
                            }
                            else if (fpValue.Trim().ToLower().EndsWith("::"))
                            {
                                title = Regex.Replace(fpValue, @"\=|\:\:", "").Trim();
                                if (title.IndexOf("(AM)") != -1)
                                {
                                    markupStart.Add(new XAttribute("class", "am"));
                                }
                                else if (title.ToLower().IndexOf("a-meldling") != -1)
                                {
                                    markupStart.Add(new XAttribute("class", "am"));
                                }
                                else if (title.IndexOf("(NB)") != -1)
                                {
                                    title = title.Replace("(NB)", "").Trim();
                                    markupStart.Add(new XAttribute("class", "nb"));
                                }
                                else if (title.Trim().ToLower().StartsWith("eksempel") || title.Trim().ToLower().StartsWith("eksempler"))
                                {
                                    markupStart.Add(new XAttribute("title", title));
                                    markupStart.Add(new XAttribute("class", "xsample"));
                                }

                                fp.Remove();
                            }
                            else if (fp.Value.Trim() == "")
                            {
                                fp.Remove();
                            }

                        }

                    }
                    markupStart.RemoveFirstBr();
                    markupStart.RemoveLastBr();


                    markupStart
                        .Descendants()
                        .Where(p =>
                            "p".Split(';').Contains(p.Name.LocalName)
                        && p.DescendantNodes().OfType<XElement>().Where(s => "img".Split(';').Contains(s.Name.LocalName)).Count() != 0
                    )
                    .ToList().ForEach(p => p.ReplaceWith(p.SplitImgPara()));


                    markupStart
                    .Descendants()
                    .Where(p =>
                            "p;strong;span;em".Split(';').Contains(p.Name.LocalName)
                        && p.DescendantNodes().OfType<XElement>().Where(s => ("iframe;img;" + _x_var).Split(';').Contains(s.Name.LocalName)).Count() == 0
                        && p.DescendantNodesAndSelf().OfType<XText>().Select(s => s.ToString()).StringConcatenate().Trim() == "")
                    .Remove();

                    markupStart.Attributes("oid").Remove();
                    markupStart.Attributes("position").Remove();

                    if ((markupStart.Elements("p").Count() != 0 || markupStart.Elements("table").Count() != 0)
                        && first.Name.LocalName == "p")
                    {
                        first.ReplaceWith(markupStart);
                    }
                    else if (markupStart.Parent.Name.LocalName == "p" && markupStart.Parent.Nodes().Count() == 1)
                    {
                        markupStart.Parent.ReplaceWith(markupStart);
                    }

                }
            }
            List<XCData> cdata = d.DescendantNodes().OfType<XCData>().ToList();
            cdata.ForEach(p => p.Remove());

        }
        private static int DGListsPrepare(this XElement d, List<DGVarData> vardata)
        {
            string text = (d.Annotation<DGText>() == null ? "" : (d.Annotation<DGText>().text == null ? "" : d.Annotation<DGText>().text));
            if (text == "") return 0;
            int n = 0;
            foreach (Match m in rxListe.Matches(text))
            {
                if (!Regex.IsMatch(m.Value.Replace(" ", "").Trim(), @"\([\*]+\)"))
                {
                    n++;
                    DGMatchResult listStart = new DGMatchResult(m.Groups["start"], d);
                    ReplaceNode(listStart, "x-list", n.ToString(), DGPosition.Start, vardata);
                    DGMatchResult listEnd = new DGMatchResult(m.Groups["end"], d);
                    ReplaceNode(listEnd, "x-list", n.ToString(), DGPosition.End, vardata);
                }
            }
            return n;
        }
        private static void DGListsExtract(this XElement d, int n, List<DGVarData> vardata)
        {
            for (int i = 1; i <= n; i++)
            {
                XElement listStart = d.Descendants("x-list").Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString() && (string)p.Attributes("position").FirstOrDefault() == DGPosition.Start.ToString()).FirstOrDefault();
                XElement listEnd = d.Descendants("x-list").Where(p => (string)p.Attributes("oid").FirstOrDefault() == i.ToString() && (string)p.Attributes("position").FirstOrDefault() == DGPosition.End.ToString()).FirstOrDefault();
                if (listStart != null && listEnd != null)
                {

                    DGListAnalyse la = new DGListAnalyse(listStart, listEnd);
                    listStart.AddAnnotation(la);

                    XNode fn = listStart.NodesAfterSelf().ElementAt(0);
                    XNode sn = listStart.NodesAfterSelf().ElementAt(1);
                    if ((sn.NodeType == XmlNodeType.Element ? ((XElement)sn).Name.LocalName : "") == _x_var
                        && (fn.NodeType == XmlNodeType.Text))
                    {
                        listStart.Add(new XAttribute("header", ((XText)fn).Value.Trim()));
                        fn.Remove();
                        XElement var = (XElement)sn;
                        string id = (string)var.Attributes("id").FirstOrDefault();
                        DGVarData vd = vardata.Where(p => p.varname == id).FirstOrDefault();
                        listStart.Add(new XAttribute("varname", id));
                        listStart.Add(new XAttribute("defaultcounter", vd.counter));
                        sn.Remove();
                    }


                    List<XNode> nodelist = new List<XNode>();
                    XElement first = la.Parent.DiveNodes(listStart);
                    XElement last = la.Parent.DiveNodes(listEnd);
                    if (first != null && last != null)
                    {

                        nodelist.AddRange(first.NodesAfterSelf().TakeWhile(p => p != last));
                        nodelist.ForEach(p => p.Remove());
                        listStart.Add(nodelist);
                        last.Remove();

                        listStart
                            .Descendants()
                            .Where(p =>
                                   "p;strong;span;em".Split(';').Contains(p.Name.LocalName)
                                && p.DescendantNodes().OfType<XElement>().Where(s => ("iframe;img;" + _x_var).Split(';').Contains(s.Name.LocalName)).Count() == 0
                                && p.DescendantNodesAndSelf().OfType<XText>().Select(s => s.ToString()).StringConcatenate().Trim() == "")
                            .Remove();


                        List<XCData> cdata = listStart.DescendantNodes().OfType<XCData>().ToList();
                        cdata.ForEach(p => p.Remove());

                        listStart.RemoveFirstBr();
                        listStart.RemoveLastBr();


                        if (first.Name.LocalName == "p")
                        {
                            if (listStart.Elements().Where(p => p.Descendants(_x_var).Count() != 0).Count() > 1)
                            {
                                listStart.Add(new XAttribute("oftype", "pblock"));
                                first.ReplaceWith(listStart);
                            }
                            else if (listStart.Elements().Count() == 1 && listStart.Elements("table").Count() == 1 && listStart.Elements("table").Descendants(_x_var).Count() != 0)
                            {
                                listStart.Add(new XAttribute("oftype", "tablerow"));
                                first.ReplaceWith(listStart);
                            }
                            else if (listStart.Elements("p").Descendants(_x_var).Count() != 0)
                            {
                                listStart.Add(new XAttribute("oftype", "pblock"));
                                first.ReplaceWith(listStart);

                            }
                            else if (
                                listStart.Elements().Select(p => p.Name.LocalName).FirstOrDefault() == "ul"
                                && listStart.Descendants(_x_var).Select(p => p.Parent.Name.LocalName).FirstOrDefault() == "li"
                                )

                            {
                                listStart.Add(new XAttribute("oftype", "ul"));
                                first.ReplaceWith(listStart);
                            }
                            else
                            {

                            }
                        }
                        else if (first.Parent.Name.LocalName == "p" && first.Name.LocalName == "x-list" && first == listStart)
                        {
                            //listStart.Add(new XAttribute("oftype", "p"));
                            //XElement eholder = new XElement("p", listStart.Nodes());
                            //listStart.Nodes().Remove();
                            //listStart.Add(eholder);
                            //first.Parent.ReplaceWith(first);
                            List<XNode> nlist = new List<XNode>();
                            if (first.Parent.Nodes().TakeWhile(p => p != first).Count() != 0)
                                nlist.Add(new XElement("p", first.Parent.Attributes("id"), first.Parent.Nodes().TakeWhile(p => p != first)));

                            listStart.Add(new XAttribute("oftype", "p"));
                            XElement eholder = new XElement("p", (nlist.Count() == 0 ? null : first.Parent.Attributes("id")), listStart.Nodes());
                            nlist.Add(new XElement(listStart.Name.LocalName, listStart.Attributes(), eholder));

                            if (first.Parent.Nodes().SkipWhile(p => p != first).Skip(1).Count() != 0)
                                nlist.Add(new XElement("p", first.Parent.Nodes().SkipWhile(p => p != first).Skip(1)));

                            first.Parent.ReplaceWith(nlist);
                        }
                        else if (@"li;td".Split(';').Contains(first.Parent.Name.LocalName) && first.Name.LocalName == "x-list" && first == listStart)
                        {
                            if (listStart.Elements().Where(p => p.Descendants(_x_var).Count() != 0).Count() > 1)
                            {
                                listStart.Add(new XAttribute("oftype", "pblock"));
                                first.ReplaceWith(listStart);
                            }
                            else if (listStart.Elements().Count() == 1 && listStart.Elements("table").Count() == 1 && listStart.Elements("table").Descendants(_x_var).Count() != 0)
                            {
                                listStart.Add(new XAttribute("oftype", "tablerow"));
                            }
                            else if (listStart.Elements("p").Descendants(_x_var).Count() != 0)
                            {
                                listStart.Add(new XAttribute("oftype", "pblock"));

                            }
                            else if (
                                   listStart.Descendants(_x_var).Count() != 0
                                && (
                                        listStart.Descendants().Where(p => "x-var;br;strong;span;".Split(';').Contains(p.Name.LocalName)).Count() != 0
                                        || listStart.DescendantNodes().OfType<XText>().Count() != 0
                                    )
                                )
                            {
                                listStart.Add(new XAttribute("oftype", "cell-line"));
                            }
                            else
                            {


                            }
                        }
                        else
                        {

                        }
                    }
                }
            }

        }

        public static XElement ConvertDG(this XElement document, XElement extparts = null)
        {

            if (document.Elements().Count() == 0) return document;

            document = new XElement(document);
            document.Descendants()
                           .Where(p => p.Name.LocalName != "level")
                           .Attributes("id")
                           .Select(p => new { val = p.Value, att = p })
                           .GroupBy(p => p.val)
                           .Where(p => p.Count() > 1)
                           .Select(p => new UniqueAtt { key = p.Key, l = p.Select(s => s.att).ToList() })
                           .ToList()
                           .ForEach(p => p.AttributesUnique());
            document.DGDibLink();
            document.Descendants().Attributes(XNamespace.Xml + "space").ToList().ForEach(p => p.Remove());
            if (extparts == null ? false : extparts.HasElements)
            {
                extparts.Descendants().Where(p => "diblink;dibparameter".Split(';').Contains(p.Name.LocalName)).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
                extparts.Descendants().Attributes("id").ToList().ForEach(p => p.Remove());
                List<XElement> dParts = document.Descendants().Where(p => ((string)p.Attributes("externalpartid").FirstOrDefault() ?? "") != "").Reverse().ToList();
                if (dParts.Count() > 0)
                {
                    foreach (XElement dp in dParts)
                    {
                        string partname = ((string)dp.Attributes("externalpartid").FirstOrDefault() ?? "").Trim().ToLower();
                        if (partname != "")
                        {
                            XElement part = extparts.Elements("externalpart").Where(p => ((string)p.Attributes("name").FirstOrDefault() ?? "").Trim().ToLower() == partname).Elements("text").FirstOrDefault();
                            if (part != null)
                            {
                                switch (dp.Name.LocalName)
                                {
                                    case "p": dp.ReplaceWith(part.Nodes()); break;
                                    case "td":
                                    case "li": dp.ReplaceWith(new XElement(dp.Name.LocalName, dp.Attributes("id")), part.Nodes()); break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            document.ConcatXText();
            document.SetDGText();
            document.DGLocateLocalorGlobalVariableClean();
            document.ConcatXText();
            document = new XElement(document);
            document.SetDGText();
            List<DGVarData> varData = document.DGLocateLocalorGlobalVariable();
            if (varData.Count() != 0)
            {
                //document.Descendants("a").Where(p => (string)p.Attributes("class").FirstOrDefault() == "diblink").ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
                document.Descendants().Where(p => p.Name.LocalName == "br").ToList().ForEach(p => p.ReplaceWith(new XElement("br")));
                document.ConcatXText();
                document = new XElement(document);
                document.SetDGText();
                varData = document.DGLocateLocalorGlobalVariable();
                document.DGReplaceVars(varData);
            }
            document.Descendants("p").Where(p => "th;td;li".Split(';').Contains(p.Parent.Name.LocalName) && p.Parent.Elements().Count() == 1).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
            List<XElement> dobbleTexts = document.Descendants("text").Where(p => p.Elements().Count() == 1 && p.Elements().Select(s => s.Name.LocalName).FirstOrDefault() == "text").ToList();
            dobbleTexts.ForEach(p => p.ReplaceWith(p.Nodes()));
            XElement variables = null;
            variables = new XElement("variables",
                varData
                .GroupBy(p => new { type = p.vartype, varname = p.varname })
                .Select(p =>
                    new XElement("variable",
                        new XElement("type", p.Key.type.ToString()),
                        new XElement("id", p.Key.varname.ToString()),
                        new XElement("name", p.Select(s => s.name).OrderByDescending(s => s.Length).FirstOrDefault()),
                        p.Where(s => s.standard != null).Count() == 0
                        ? null
                        : (
                            p.Where(s => s.standard != null).Select(s => s.standard).OrderByDescending(s => s.Length).FirstOrDefault() == ""
                            ? null
                            : new XElement("standard", p.Where(s => s.standard != null).Select(s => s.standard).OrderByDescending(s => s.Length).FirstOrDefault())
                        ),
                        p.Where(s => s.comment != null).Count() == 0 ? null : (p.Where(s => s.comment != null).Select(s => s.comment).OrderByDescending(s => s.Length).FirstOrDefault() == "" ? null : new XElement("comment", p.Where(s => s.comment != null).Select(s => s.comment).OrderByDescending(s => s.Length).FirstOrDefault())),
                        p.Where(s => s.options != null).Count() == 0
                        ? null
                        : new XElement("options",
                            p.Where(s => s.options != null)
                            .SelectMany(s =>
                                s.options
                                .Elements("x-option-value")
                                .Select(o => new {
                                    label = ((string)o.Attributes("label").FirstOrDefault() ?? "").Trim(),
                                    value = ((string)o.Attributes("value").FirstOrDefault() ?? "").Trim()
                                }
                                )
                            )
                            .GroupBy(s => s)
                            .Select(s =>
                                new XElement("option",
                                    s.Key.label == "" ? null : new XAttribute("label", s.Key.label),
                                    s.Key.value == "" ? null : new XAttribute("value", s.Key.value)
                                )
                            )
                        )
                    )
                )
            );

            document.SetDGText();
            List<XElement> texts = document.Descendants("text").ToList();
            texts.ConvertText(varData);

            int alts = 1;
            document.Descendants("x-alternatives").ToList().ForEach(p => p.Add(new XAttribute("id", (alts++).ToString() + "_Alternative")));

            document.Descendants().Where(p => "x-box;level".Split(';').Contains(p.Name.LocalName) && (string)p.Attributes("id").FirstOrDefault() == null).ToList().ForEach(p => p.Add(new XAttribute("id", Guid.NewGuid().ToString())));
            document.Descendants().Where(p => p.Name.LocalName.StartsWith("x-")).Attributes().Where(p => "position;oid".Split(';').Contains(p.Name.LocalName)).ToList().ForEach(p => p.Remove());
            XElement xobjects = null;
            if ((variables == null ? 0 : variables.Elements("variable").Count()) != 0)
            {
                //rxOptionalTitle
                document.CheckLevelOptions();
                document.Descendants().Where(p => "x-alternatives;x-optional".Split(';').Contains(p.Name.LocalName))
                    .Select(p => new { id = (string)p.Attributes("id").FirstOrDefault() })
                    .GroupBy(p => p.id)
                    .Select(p => p.Key)
                    .ToList()
                    .ForEach(p => variables.Add(new XElement("variable",
                            new XElement("type", "local"),
                            new XElement("id", p)
                        )
                    )
                );
                document.Descendants("level").Attributes("data-var-id")
                    .ToList()
                    .ForEach(p => variables.Add(new XElement("variable",
                            new XElement("type", "local"),
                            new XElement("id", p.Value)
                        )
                    )
                );

                int xn = 1;
                document.Descendants().ToList().ForEach(p => p.AddAnnotation(new ElementCounter { n = xn++ }));
                document.Descendants().Where(p => p.Name.LocalName.StartsWith("x-") && p.Attributes("id").FirstOrDefault() == null).ToList().ForEach(p => p.Add(new XAttribute("id", "x" + p.Annotation<ElementCounter>().n.ToString())));

                List<XElement> xvar =
                    document
                    .Descendants("x-var")
                    .Where(p => p
                        .Ancestors()
                        .Where(a =>
                             a.Name.LocalName.StartsWith("x-")
                             || (
                                 a.Name.LocalName == "level"
                                 && ((string)a.Attributes("data-optional").FirstOrDefault() ?? "") == "true"
                                 )
                        ).Count() == 0
                    )
                    .GroupBy(p => new { id = (string)p.Attributes("id").FirstOrDefault() })
                    .Select(p => p.Select(s => s).FirstOrDefault())
                    .ToList();

                List<XElement> xobj = document.Descendants()
                    .Where(p =>
                        p.Name.LocalName != "x-var"
                        && (
                            p.Name.LocalName.StartsWith("x-")
                            ||
                                (
                                    p.Name.LocalName == "level"
                                    && ((string)p.Attributes("data-optional").FirstOrDefault() ?? "") == "true"
                                )
                            )
                        && p.Ancestors().Where(a =>
                            a.Name.LocalName.StartsWith("x-")
                            || (
                                a.Name.LocalName == "level"
                                && ((string)a.Attributes("data-optional").FirstOrDefault() ?? "") == "true"
                                )
                            ).Count() == 0)
                    .Select(p => p).ToList();

                xobjects = new XElement("xobjects",
                    xvar
                    .Where(p =>
                        p
                        .Ancestors().Where(a =>
                            a.Name.LocalName.StartsWith("x-")
                            || (
                                a.Name.LocalName == "level"
                                && ((string)a.Attributes("data-optional").FirstOrDefault() ?? "") == "true"
                                )
                            ).Count() == 0
                    )
                    .Select(p => p)
                    .Union(xobj.Select(p => p))
                    .OrderBy(p => p.Annotation<ElementCounter>().n)
                    .Select(p => p.GetXObjects())
                );


                //xobjects = new XElement("xobjects",
                //    xvar
                //    .Where(p => 
                //        p
                //        .Ancestors()
                //        .Where(a => 
                //            a.Name.LocalName.StartsWith("x-") 
                //            || (
                //                a.Name.LocalName=="level" 
                //                && ((string)a.Attributes("data-optional").FirstOrDefault()??"") == "true")
                //        )
                //        .Count() == 0
                //    )
                //    .Select(p => p)
                //    .Union(xobj.Select(p => p))
                //    .OrderBy(p => p.Annotation<ElementCounter>().n)
                //    .Select(p => p.GetXObjects(xvar))
                //);

                //document.AddFirst(variables);
            }

            (
                from xv in document.Descendants("title").Elements("x-var")
                join v in variables.Elements("variable")
                on (string)xv.Attributes("id").FirstOrDefault() equals v.Elements("id").Select(p => p.Value).FirstOrDefault()
                select new { xvar = xv, name = v.Elements("name").Select(p => p.Value).FirstOrDefault() }
            ).ToList()
            .ForEach(p => p.xvar.AddAnnotation(new XVarName { name = p.name }));

            List<XElement> settings = document.Descendants("x-settings").ToList();
            settings.ForEach(p => p.Remove());
            XElement xsettings = new XElement("x-settings",
               settings
               .Select(p => p.Value.Trim().ToLower())
               .Where(p => p != "")
               .Select(p => p.GetSettingsByValue())
           );
            document.AddFirst(xsettings);

            XElement xp = document.Descendants("externalparts").FirstOrDefault();
            if (xp != null)
                xp.Remove();

            XElement index = new XElement("index", document.Index6_7());
            return new XElement("convert67",
                index,
                document,
                variables,
                xobjects,
                xp
            );
        }
        public static void CheckLevelOptions(this XElement e)
        {
            e
            .Descendants("level")
            .Elements("title")
            .ToList()
            .ForEach(p => p.AddAnnotation(new TitleEval(p)));

            e.Elements("level").EvalTitle();
        }
        public static void SetTitle(this XElement title, bool number, bool optional, bool autocount)
        {

            TitleEval te = title.Annotation<TitleEval>();
            if (autocount && te != null)
            {
                if (te.number != -1)
                {
                    //if (te.optional) title.Parent.Add(new XAttribute("data-optional", "true"));
                    //if (te.Default) title.Parent.Add(new XAttribute("data-default", te.Default));
                    if (te.optional)
                    {
                        title.Parent.Add(new XAttribute("data-optional", "true"));
                        title.Parent.Add(new XAttribute("data-default", te.Default));
                    }
                    if (te.subnumber != -1) title.Parent.Add(new XAttribute("data-subnumber", te.Default));
                    title.Parent.Add(new XAttribute("data-var-id", (string)title.Parent.Attributes("id").FirstOrDefault() + "_SelectableItem"));
                    title.Parent.Add(new XAttribute("data-autocount", "true"));
                    title.Parent.Add(new XAttribute("data-type", "decimal"));

                    te.f.Value = title.Annotation<TitleEval>().newValue;
                    //title.Value = title.Annotation<TitleEval>().newValue;
                }
                else if (te.optional)
                {
                    title.Parent.Add(new XAttribute("data-var-id", (string)title.Parent.Attributes("id").FirstOrDefault() + "_SelectableItem"));
                    title.Parent.Add(new XAttribute("data-optional", "true"));
                    title.Parent.Add(new XAttribute("data-default", te.Default));
                    te.f.Value = title.Annotation<TitleEval>().newValue;
                    //title.Value = title.Annotation<TitleEval>().newValue;
                }
            }
            else if (number && te != null)
            {
                if (te.number != -1)
                {
                    //if (te.optional) title.Parent.Add(new XAttribute("data-optional", "true"));
                    //if (te.Default) title.Parent.Add(new XAttribute("data-default", te.Default));
                    if (te.optional)
                    {
                        title.Parent.Add(new XAttribute("data-optional", "true"));
                        title.Parent.Add(new XAttribute("data-default", te.Default));
                    }
                    title.Parent.Add(new XAttribute("data-var-id", (string)title.Parent.Attributes("id").FirstOrDefault() + "_SelectableItem"));
                    title.Parent.Add(new XAttribute("data-autocount", "true"));
                    title.Parent.Add(new XAttribute("data-type", "decimal"));

                    te.f.Value = title.Annotation<TitleEval>().newValue;
                    //title.Value = title.Annotation<TitleEval>().newValue;
                }
            }
            else if (optional && te != null)
            {
                if (te.optional)
                {
                    title.Parent.Add(new XAttribute("data-var-id", (string)title.Parent.Attributes("id").FirstOrDefault() + "_SelectableItem"));
                    title.Parent.Add(new XAttribute("data-optional", "true"));
                    title.Parent.Add(new XAttribute("data-default", te.Default));
                    te.f.Value = title.Annotation<TitleEval>().newValue;
                    //title.Value = title.Annotation<TitleEval>().newValue;
                }
            }
            //title.Parent.Elements("level").EvalTitle();
            title.Parent.Elements("level").EvalTitle();
        }
        public static void EvalTitle(this IEnumerable<XElement> levels)
        {
            if (levels.Count() == 0) return;
            bool number = levels.Elements("title").Where(p => p.Annotation<TitleEval>() == null ? false : p.Annotation<TitleEval>().optional && p.Annotation<TitleEval>().number != -1).Count() != 0 ? true : false;
            bool optional = levels.Elements("title").Where(p => p.Annotation<TitleEval>() == null ? false : p.Annotation<TitleEval>().optional).Count() != 0 ? true : false;
            bool pautocount = (string)levels.FirstOrDefault().Parent.Attributes("data-autocount").FirstOrDefault() == "true" ? true : false;

            levels
                .Elements("title")
                .ToList()
                .ForEach(p => p.SetTitle(number, optional, pautocount));


        }
    }


}
