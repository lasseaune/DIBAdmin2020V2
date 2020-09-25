using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DIB.Data;


namespace DIB.InTextLinking
{
    public class InTextLinkingXML
    {

        private List<GlobalIL.rQuery> _ListOfRegexQuery = null;
        private Regex _MainRegex = null;
        private XElement _MainAction = null;
        private XElement _Actions = null;
        private string _LANGUAGE = "no";
        private bool _Local = false;
        public XElement InternalTags = null;
        public XElement ExternalTags = null;
        public XElement UnidentyfiedTags = null;
        public XElement _DOCUMENTFRAME = null;
        public bool Ready = false;

        public InTextLinkingXML(string RegexQuery
                                , XElement MainAction
                                , XElement AllActions
                                , bool local
        )
        {
            try
            {
                _MainRegex = new Regex(RegexQuery);
                _MainAction = MainAction;
                _Actions = AllActions;
                _Local = local;
                Ready = true;
            }
            catch
            {

            }
        }


        public XElement FindUnidentyfiedTags(XElement XmlDoc, string language)
        {
            XElement result = null;
            _LANGUAGE = language;
            result = GetUnidentyfiedTags(XmlDoc);
            return result;
        }
        public XElement InsertLinksInDibDocument(XElement XmlDoc, string language)
        {
            XElement result = null;
            _LANGUAGE = language;
            result = GetLinksInDibDocument(XmlDoc);
            return result;
        }

        public XElement GetLinksInText(string text, string language)
        {
            XElement result = null;
            _LANGUAGE = language;
            XElement XmlDoc = XElement.Parse("<content>" + text + "</content>");
            List<GlobalIL.xRange> documentRanges = GetRangesFromDibDocument(XmlDoc);

            XElement tags = GetExternalTagsFromRanges(documentRanges);
            if (tags != null)
            {
                result = IdentifyTagsXML(tags);
                ExternalTags = result.Descendants("external").FirstOrDefault();
                UnidentyfiedTags = result.Descendants("unidentyfied").FirstOrDefault();
                return ExternalTags;
            }
            return null;
        }

        private XElement GetUnidentyfiedTags(XElement XmlDoc)
        {
            //_DOCUMENTFRAME =  CreateDocumentFrame(XmlDoc);

            List<GlobalIL.xRange> documentRanges = GetRangesFromDibDocument(XmlDoc);

            XElement tags = GetExternalTagsFromRanges(documentRanges);
            if (tags != null)
            {
                XElement result = IdentifyTagsXML(tags);
                if (result == null) return XmlDoc;

                ExternalTags = result.Descendants("external").FirstOrDefault();
                UnidentyfiedTags = result.Descendants("unidentyfied").FirstOrDefault();
            }
            return UnidentyfiedTags;
        }

        private XElement GetLinksInDibDocument(XElement XmlDoc)
        {
            //_DOCUMENTFRAME =  CreateDocumentFrame(XmlDoc);

            List<GlobalIL.xRange> documentRanges = GetRangesFromDibDocument(XmlDoc);

            XElement tags = GetExternalTagsFromRanges(documentRanges);
            if (tags != null)
            {
                XElement result = IdentifyTagsXML(tags);
                if (result == null) return XmlDoc;

                ExternalTags = result.Descendants("external").FirstOrDefault();
                UnidentyfiedTags = result.Descendants("unidentyfied").FirstOrDefault();
            }

            if (tags == null && !InternalTagsExists(documentRanges)) return XmlDoc;

            LocateLinksInRanges(documentRanges, ExternalTags, InternalTags);

            return XmlDoc;
        }

        private XElement CreateDocumentFrame(XElement d)
        {
            XElement result = new XElement("refparents");
            int n = d
                    .DescendantsAndSelf()
                    .Where(p =>
                            p.Name.LocalName == "section"
                        || p.Name.LocalName == "document"
                        || p.Name.LocalName == "level"
                        || p.Name.LocalName == "documents"
                        )
                    .Count();

            for (int i = 0; i < n; i++)
            {
                XElement e = d
                    .DescendantsAndSelf()
                    .Where(p =>
                        p.Name.LocalName == "section"
                        || p.Name.LocalName == "document"
                        || p.Name.LocalName == "level"
                        || p.Name.LocalName == "documents"
                    )
                    .ElementAt(i);
                if (e.Attribute("id") != null)
                {
                    XElement re = new XElement("refparent"
                        , new XAttribute("id", e.Attribute("id").Value)
                        );
                    if (e.Parent.Attribute("id") != null)
                    {
                        XElement parent = result.Descendants("refparent").Where(p => p.Attribute("id").Value == e.Parent.Attribute("id").Value).FirstOrDefault();
                        if (parent != null)
                        {
                            parent.Add(re);
                        }
                    }
                    else
                    {
                        result.Add(re);
                    }
                }
            }

            return result;
        }
        private void AddNodeRange(ref List<GlobalIL.xRange> NodeRanges, ref string NodeText, ref int n, ref int pos, ref int ant, XNode node)
        {
            string s = node.ToString();
            s = GlobalIL.WordReplace.Replace(s, new MatchEvaluator(GlobalIL.HEXREPL), -1);
            //s = (n==0 ? "" : " ") + s;
            //s = s + " ";
            //Endret 8/11-2012
            int sLength = s.Length;
            int offseth = 0;
            string TDReplace = " | ";

            if (("//td//entry//").IndexOf("//" + node.Parent.Name.LocalName + "//") != -1)
            {
                XNode x = node.Parent.DescendantNodes().OfType<XText>().FirstOrDefault();
                if (x == node)
                {
                    s = TDReplace + s;
                    offseth = TDReplace.Length;
                    pos = pos + offseth;
                }
            }
            s = s + " ";
            NodeText = NodeText + s;
            GlobalIL.xRange r = new GlobalIL.xRange(n, pos, pos + sLength);
            r.node = node;
            NodeRanges.Add(r);
            pos = pos + (s.Length-offseth);
            ant++;
        }

        private List<GlobalIL.xRange> GetRangesFromContent(XElement document)
        {
            List<GlobalIL.xRange> documentRanges = new List<GlobalIL.xRange>();
            try
            {

                int n = document
                        .Nodes()
                        .Count();

                string NodeText = "";
                List<GlobalIL.xRange> NodeRanges = new List<GlobalIL.xRange>();
                int ant = 0;
                int pos = 0;

                for (int j = 0; j < n; j++)
                {
                    XNode node = document
                        .Nodes()
                        .ElementAt(j);
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        int m = ((XElement)node)
                            .DescendantNodes()
                            .OfType<XText>()
                            .Count();

                        for (int i = 0; i < m; i++)
                        {
                            XNode t = ((XElement)node)
                                        .DescendantNodes()
                                        .OfType<XText>()
                                        .ElementAt(i);
                            AddNodeRange(ref NodeRanges, ref NodeText, ref n, ref pos, ref ant, t);

                        }
                    }
                    else if (node.NodeType == XmlNodeType.Text)
                    {
                        AddNodeRange(ref NodeRanges, ref NodeText, ref n, ref pos, ref ant, node);
                    }

                }
                if (NodeText.Trim() != "")
                {
                    SearchMatches(NodeText, NodeRanges);
                    documentRanges.AddRange(NodeRanges.Where(p => p.links.Count() != 0).ToList());
                }
            }
            catch
            {

            }
            return documentRanges;
        }
        private List<GlobalIL.xRange> GetRangesFromDibDocument(XElement document)
        {
            List<GlobalIL.xRange> documentRanges = new List<GlobalIL.xRange>();
            try
            {

                int n = document
                        .DescendantNodesAndSelf()
                        .OfType<XElement>()
                        .Where(p =>
                               ((XElement)p).Name.LocalName == "section"
                            || ((XElement)p).Name.LocalName == "document"
                            || ((XElement)p).Name.LocalName == "level"
                            )
                        .Count();
                if (n == 0)
                {
                    return GetRangesFromContent(document);
                }

                for (int i = 0; i < n; i++)
                {
                    XNode section = document
                        .DescendantNodesAndSelf()
                        .OfType<XElement>()
                        .Where(p =>
                               ((XElement)p).Name.LocalName == "section"
                            || ((XElement)p).Name.LocalName == "document"
                            || ((XElement)p).Name.LocalName == "level"
                            )
                        .ElementAt(i);

                    int m = ((XElement)section)
                            .Nodes()
                            .OfType<XElement>()
                            .Where(p =>
                                        ((XElement)p).Name.LocalName != "section"
                                    && ((XElement)p).Name.LocalName != "document"
                                    && ((XElement)p).Name.LocalName != "level"
                                    && ((XElement)p).Name.LocalName != "title"
                                    && ((XElement)p).Name.LocalName != "kws"
                                )
                            .DescendantNodes()
                            .OfType<XText>()
                            .Count();

                    string NodeText = "";
                    List<GlobalIL.xRange> NodeRanges = new List<GlobalIL.xRange>();
                    int ant = 0;
                    int pos = 0;

                    for (int j = 0; j < m; j++)
                    {
                        XNode t = ((XElement)section)
                            .Nodes()
                            .OfType<XElement>()
                            .Where(p =>
                                        ((XElement)p).Name.LocalName != "section"
                                    && ((XElement)p).Name.LocalName != "document"
                                    && ((XElement)p).Name.LocalName != "level"
                                    && ((XElement)p).Name.LocalName != "title"
                                    && ((XElement)p).Name.LocalName != "kws"
                                )
                            .DescendantNodes()
                            .OfType<XText>()
                            .ElementAt(j);
                        
                        AddNodeRange(ref NodeRanges, ref NodeText, ref n, ref pos, ref ant, t);

                    }
                    if (NodeText.Trim() != "")
                    {
                        SearchMatches(NodeText, NodeRanges);
                        documentRanges.AddRange(NodeRanges.Where(p => p.links.Count() != 0).ToList());
                    }
                }
            }
            catch
            {

            }
            return documentRanges;
        }


        private void LocateLinksInRanges(List<GlobalIL.xRange> xs, XElement externalTags, XElement internalTags)
        {
            foreach (GlobalIL.xRange xr in xs.Where(p => p.links.Count() != 0))
            {
                XNode rangeNode = xr.node;
                string rangeText = xr.node.ToString();
                int rangeStart = xr.start;
                int rangeCursor = xr.start;
                int rangeLength = xr.end - xr.start;

                string linkText = "";

                XElement range = new XElement("range");
                int nr = xr.links.Count();
                for (int ir = 0; ir < nr; ir++)
                {
                    GlobalIL.xLink l = xr.links.OrderBy(p => p.start).ElementAt(ir);

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
                        string s = rangeText.Substring((rangeCursor - rangeStart), ((linkStart-rangeStart) - (rangeCursor - rangeStart)));
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
                        //range.Add(XElement.Parse("<text>" + linkText + "</text>").Nodes());

                    }
                    else
                    {
                        //Debug.Print(linkText + "///" + l.regexpName);
                        if (l.idArea == "internal" && internalTags != null)
                        {
                            CreateLinkInternal(range, internalTags, l.prefix + l.tag1 + l.tag2, linkText);
                        }
                        else if (externalTags !=null)
                        {
                            CreateLink(range, externalTags, l.tag1, l.tag2, linkText);
                        }
                    }

                }

                if (range.HasElements)
                {
                    if ((rangeCursor-rangeStart) < rangeText.Length)
                    {
                        string clean = rangeText.Substring((rangeCursor - rangeStart), rangeText.Length - (rangeCursor-rangeStart));
                        range.Add(new XText(clean));
                        //range.Add(new XText(rangeText.Substring(rangeCursor, rangeText.Length - rangeCursor)));
                    }

                    if (((XElement)rangeNode.Parent).Attributes(XNamespace.Xml + "space").Count() == 0)
                    {
                        XAttribute a = new XAttribute(XNamespace.Xml + "space", "preserve");
                        ((XElement)rangeNode.Parent).Add(a);
                    }
                    rangeNode.ReplaceWith(range.Nodes());
                }
            }

        }


        private bool InternalTagsExists(List<GlobalIL.xRange> xs)
        {
            bool returnValue = false;
            try
            {
                var x = from p in xs
                        from q in p.links
                        where q.idArea == "internal"
                        select q;
                if (x.Count() != 0) returnValue = true;
                return returnValue;
            }
            catch
            {
                return returnValue;
            }
        }
        private XElement GetExternalTagsFromRanges(List<GlobalIL.xRange> xs)
        {

            try
            {
                var x = from p in xs
                        from q in p.links
                        where q.idArea == "external"
                        select q;

                var y = x.GroupBy(p => new { p.tag1, p.tag2, p.regexpName });

                XElement tags = new XElement("tags");

                foreach (var w in y.OrderBy(p => p.Key.tag1).OrderBy(p => p.Key.tag2))
                {
                    if ((w.Key.tag2 == null ? "" : w.Key.tag2) != "")
                    {
                        tags.Add(new XElement("tag"
                                , new XAttribute("t1", w.Key.tag1)
                                , new XAttribute("t2", w.Key.tag2)
                                , new XAttribute("regexpName", w.Key.regexpName)
                                ));

                    }
                    else
                        tags.Add(new XElement("tag"
                        , new XAttribute("t1", w.Key.tag1)
                        , new XAttribute("regexpName", w.Key.regexpName)
                        ));
                }
                if (tags.HasElements)
                    return tags;
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        private XElement IdentifyTagsXML(XElement tags)
        {

            try
            {
                string XmlString = tags.ToString();
                XmlString = Regex.Replace(XmlString, @"<\?xml[^<]+?>", "", RegexOptions.Multiline | RegexOptions.Singleline);
                XmlString = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(XmlString));
                InTextLinkingData _data = new InTextLinkingData(_Local);

                return _data.IdentifyTags(XmlString, _LANGUAGE);

            }
            catch
            {
                return null;
            }
        }

        private void CreateLinkInternal(XElement container, XElement _INT, string tag1, string linkTekst)
        {
            int n = 0;
            n = _INT
                .Elements("tag")
                .Where(p => p.Attribute("t1").Value == tag1)
                .Count();
            if (n == 0)
            {
                container.Add(new XText(linkTekst));
                //container.Add(XElement.Parse("<text>" + linkTekst + "</text>").Nodes());
            }
            else if (n == 1)
            {
                XElement tag = _INT
                    .Elements("tag")
                    .Where(p => p.Attribute("t1").Value == tag1)
                    .ElementAt(0);

                container.Add(new XElement("a"
                            , linkTekst
                            , new XAttribute("class", "xref")
                            , new XAttribute("href", "#")
                    //, new XAttribute("onclick", "return false;")
                            , new XAttribute("data-bm", tag.Attribute("bm").Value)
                            ));
            }
        }

        private void CreateLink(XElement container, XElement _EX, string tag1, string tag2, string linkTekst)
        {
            int n = 0;
            n = _EX
                .Elements("tag")
                .Where(p => p.Attribute("t1").Value == tag1
                    && (p.Attribute("t2") == null ? "" : p.Attribute("t2").Value) == tag2)
                .Count();
            if (n == 0)
            {
                container.Add(new XText(linkTekst));
                //container.Add(XElement.Parse("<text>" + linkTekst + "</text>").Nodes());
            }
            else if (n == 1)
            {
                XElement tag = _EX
                    .Elements("tag")
                    .Where(p => p.Attribute("t1").Value == tag1
                        && (p.Attribute("t2") == null ? "" : p.Attribute("t2").Value) == tag2)
                    .ElementAt(0);

                container.Add(new XElement("a"
                            , linkTekst
                            , new XAttribute("class", "xref")
                            , new XAttribute("data-tid", tag.Attribute("topic_id").Value)
                            , (tag.Attribute("bm") == null ? null : new XAttribute("data-bm", tag.Attribute("bm").Value))
                            ));
            }
            else
            {
                string id = System.Guid.NewGuid().ToString();
                XElement link = new XElement("a"
                    , linkTekst
                    , new XAttribute("class", "xref")
                    , new XAttribute("data-xreflist", id));
                container.Add(link);

                XElement span = new XElement("span"
                            , new XAttribute("class", "hn xreflist")
                            , new XAttribute("id", id));
                for (int i = 0; i < n; i++)
                {
                    XElement tag = _EX
                        .Elements("tag")
                        .Where(p => p.Attribute("t1").Value == tag1
                            && (p.Attribute("t2") == null ? "" : p.Attribute("t2").Value) == tag2)
                        .ElementAt(i);

                    span.Add(new XElement(
                                  "a"
                                , tag.Attribute("name").Value
                                , new XAttribute("class", "xref")
                                , new XAttribute("data-tid-list", tag.Attribute("topic_id").Value)
                                , (tag.Attribute("bm") == null ? null : new XAttribute("data-bm", tag.Attribute("bm").Value)
                                )
                            )
                    );
                }
                container.Add(span);
            }
        }

        private void SearchMatches(string NodeText, List<GlobalIL.xRange> NodeRanges)
        {
            MatchCollection mc = _MainRegex.Matches(NodeText);
            int start = 0;
            int count = mc.Count;
            if (count != 0)
            {
                for (int i = 0; i < count; i++)
                {
                    Match m = mc[i];
                    GlobalIL.actionObject ao = new GlobalIL.actionObject(NodeText, m, start);
                    ExecuteActions(_MainAction, ref ao, ref NodeRanges);
                    start = ao.start;
                }
            }
        }


        private List<GlobalIL.xRange> GetRanges(XNode n, ref string textNode)
        {
            List<GlobalIL.xRange> rN = null;
            if (n.NodeType == XmlNodeType.Element)
            {
                rN = GetXTextInRange((XElement)n, ref textNode);
            }
            return rN;
        }

        private List<GlobalIL.xRange> GetXTextInRange(XElement e, ref string textNode)
        {
            List<GlobalIL.xRange> returnValue = new List<GlobalIL.xRange>();
            int n = 0;
            int pos = 0;
            foreach (XText t in e.DescendantNodes().OfType<XText>())
            {
                string s = t.ToString();
                s = GlobalIL.WordReplace.Replace(s, new MatchEvaluator(GlobalIL.HEXREPL), -1);
                //s = (n==0 ? "" : " ") + s;
                s = s + " ";
                textNode = textNode + s;
                GlobalIL.xRange r = new GlobalIL.xRange(n, pos, pos + s.Length);
                r.node = t;
                returnValue.Add(r);
                pos = pos + s.Length;
                n++;
            }
            return returnValue;
        }


        private void ExecuteActions(XElement action, ref GlobalIL.actionObject ao, ref List<GlobalIL.xRange> container)
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
                        case "internal":
                            ActionInternal(a, ref ao, ref container);
                            return;
                        case "runaction":
                            if (_Actions.Elements("action").Where(p => p.Attribute("name").Value == a.Attribute("name").Value).Count() != 0)
                            {
                                a = _Actions.Elements("action").Where(p => p.Attribute("name").Value == a.Attribute("name").Value).First();
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


        private void ActionInternal(XElement a, ref GlobalIL.actionObject ao, ref List<GlobalIL.xRange> container)
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

                ActionIdentifyInternal(a, ref ao, ref container, a.Attribute("tag").Value.ToString(), result);
            }
            catch
            {
            }
        }


        private void ActionIf(XElement a, ref GlobalIL.actionObject ao, ref List<GlobalIL.xRange> container)
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

        private bool ActionMatch(XElement a, ref GlobalIL.actionObject ao, ref List<GlobalIL.xRange> container)
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

        private void ActionGet(XElement a, ref GlobalIL.actionObject ao, ref List<GlobalIL.xRange> container)
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

        private void ActionMark(XElement a, ref GlobalIL.actionObject ao, ref List<GlobalIL.xRange> container)
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
                        List<GlobalIL.xRange> rl = container.Where(p =>
                            (start.Between(p.start, p.end, true))
                            ||
                            (end.Between(p.start, p.end, true))
                            ).ToList();
                        GlobalIL.xRange range = null;
                        if (rl.Count() == 1)
                        {
                            range = rl.ElementAt(0);
                            GlobalIL.xLink link = new GlobalIL.xLink(start, end - start);
                            link.tag1 = ao.li.First().tag1;
                            link.tag2 = ao.li.First().tag2 == null ? "" : ao.li.First().tag2;
                            if (a.Ancestors("internal").Where(p => p.Attribute("prefix") != null).Count() != 0)
                            {
                                link.idArea = "internal";
                                link.prefix = a.Ancestors("internal").Where(p => p.Attribute("prefix") != null).First().Attribute("prefix").Value;
                            }
                            link.regexpName = ao.name;
                            range.links.Add(link);
                        }
                        else if (rl.Count() > 1)
                        {
                            foreach (GlobalIL.xRange r in rl)
                            {
                                GlobalIL.xLink link = null;
                                if (start.Between(r.start, r.end))
                                {
                                    link = new GlobalIL.xLink(start, (r.end) - start);
                                }
                                else if (end.Between(r.start, r.end))
                                {
                                    link = new GlobalIL.xLink(r.start, end - (r.start));
                                }
                                else
                                    link = new GlobalIL.xLink(r.start, (r.end) - (r.start));

                                if (r.node.ToString().Trim() != ".")
                                {
                                    link.tag1 = ao.li.First().tag1;
                                    link.tag2 = ao.li.First().tag2 == null ? "" : ao.li.First().tag2;
                                    if (a.Ancestors("internal").Where(p => p.Attribute("prefix") != null).Count() != 0)
                                    {
                                        link.idArea = "internal";
                                        link.prefix = a.Ancestors("internal").Where(p => p.Attribute("prefix") != null).First().Attribute("prefix").Value;
                                    }
                                    link.regexpName = ao.name;
                                    r.links.Add(link);
                                }
                            }
                        }
                    }
                    ao.start = end;
                }
            }
            catch (SystemException e)
            {
            }
        }

        private void ActionForeach(XElement a, ref GlobalIL.actionObject ao, ref List<GlobalIL.xRange> container)
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
                            GlobalIL.actionObject su = new GlobalIL.actionObject(ao, c, a.Attribute("groups").Value);
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
                    Regex qu = _ListOfRegexQuery.Where(p => p.name == a.Attribute("name").Value).First().query;
                    string searchText = ao.m.Groups[a.Attribute("groups").Value].Value;
                    MatchCollection mc = qu.Matches(searchText);
                    int n = mc.Count;
                    for (int i = 0; i < n; i++)
                    {
                        Match m = mc[i];
                        GlobalIL.actionObject su = new GlobalIL.actionObject(ao, m);
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

        private bool ActionIdentifyInternal(XElement a, ref GlobalIL.actionObject ao, ref List<GlobalIL.xRange> container, string tag, string value)
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

        private bool ActionIdentify(XElement a, ref GlobalIL.actionObject ao, ref List<GlobalIL.xRange> container, string tag, string value)
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

        private string BuildTag(GlobalIL.actionObject ao, string tags, string trim)
        {
            string returnValue = "";
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

        private bool SetTag(ref GlobalIL.actionObject ao, string tag, string value)
        {
            bool returnValue = false;
            if (value == "") return returnValue;
            switch (tag)
            {
                case "tag1":
                    ao.tag1 = value;
                    if (IdentifyTag1Ex(ref ao))
                    {
                        return true;
                    }
                    break;
                case "tag2":
                    ao.tag2 = value;
                    if (IdentifyTag2Ex(ref ao))
                    {
                        return true;
                    }
                    break;
                default: break;
            }

            return returnValue;
        }

        private bool IdentifyTag1Ex(ref GlobalIL.actionObject ao)
        {
            bool returnValue = false;
            try
            {
                string tag1 = ao.tag1;
                List<GlobalIL.linkInfo> _newli = new List<GlobalIL.linkInfo>();
                GlobalIL.linkInfo li = new GlobalIL.linkInfo();
                li.tag1 = tag1;
                _newli.Add(li);
                ao.li = new List<GlobalIL.linkInfo>(_newli);
                returnValue = true;


            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("IdentifyTag1Ex Error: {0}", e.Message.ToString()));
            }
            return returnValue;
        }

        private bool IdentifyTag2Ex(ref GlobalIL.actionObject ao)
        {
            bool returnValue = false;
            try
            {
                string tag1 = ao.tag1;
                string tag2 = ao.tag2;
                List<GlobalIL.linkInfo> _newli = new List<GlobalIL.linkInfo>();
                GlobalIL.linkInfo li = new GlobalIL.linkInfo();
                li.tag1 = tag1;
                li.tag2 = tag2;
                _newli.Add(li);
                ao.li = new List<GlobalIL.linkInfo>(_newli);
                returnValue = true;

            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("IdentifyTag2 Error: {0}", e.Message.ToString()));
            }
            return returnValue;
        }


        private string GetLovDate(ref GlobalIL.actionObject ao, string values)
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
    }
}
