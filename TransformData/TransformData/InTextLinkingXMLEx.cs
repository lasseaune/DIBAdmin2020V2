using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace DIB.InTextLinking
{
    public class InTextLinkingXMLEx
    {
        private static string sep = " | ";
        private Regex _Regex = null;
        public InTextLinkingXMLEx(string RegexText)
        {
            _Regex = new Regex (RegexText, RegexOptions.Compiled);

        }
        public class IdRange
        {
            public int start { get; set; }
            public int end { get; set; }
            public XText node { get; set; }
            public List<string> links = new List<string>();

            public IdRange(int start, int end, XText node)
            {
                this.start = start;
                this.end = end;
                this.node = node;
            }
        }
        public class IdElements
        {
            public IEnumerable<IdElement> Elements { get; set; }
            public IdElements(XElement doc, IEnumerable<string> include, IEnumerable<string> exclude)
            {
                Elements =
                    (from el in doc.DescendantsAndSelf()
                     join i in include on el.Name.LocalName equals i
                     select new IdElement(el, exclude))
                     .Where(p => p.Ranges.Count() != 0);
            }
        }
        public class IdElement
        {
            public StringBuilder Text = new StringBuilder();
            public List<IdRange> Ranges = new List<IdRange>();
            private int pos = 0;
            public IdElement(XElement part, IEnumerable<string> exclude)
            {
                IEnumerable<XText> texts =
                    part
                     .Elements()
                     .Where(p => exclude.Where(q => q == p.Name.LocalName).Count() == 0)
                     .SelectMany(q => q.DescendantNodesAndSelf().OfType<XText>());

                if (texts.Count() != 0)
                    IdElementsConcat(texts);

            }
            private void IdElementsConcat(IEnumerable<XText> texts)
            {
                foreach (XText t in texts)
                {
                    string s = t.ToString();
                    int sLength = s.Length;
                    int offseth = 0;
                    if (("//td//entry//").IndexOf("//" + t.Parent.Name.LocalName + "//") != -1)
                    {
                        XNode x = t.Parent.DescendantNodes().OfType<XText>().FirstOrDefault();
                        if (x == t)
                        {
                            s = sep + s;
                            offseth = sep.Length;
                            pos = pos + offseth;
                        }
                    }
                    s = s + " ";
                    Text.Append(s);
                    Ranges.Add(new IdRange(pos, pos + sLength, t));
                    pos = pos + (s.Length - offseth);
                }
            }

        }
    }
}
