using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace DIB.InTextLinking
{

    public class Expressions : List<Expression>
    {
        public List<Expression> expressions = new List<Expression>();
    }

    public class Expression
    {
        public XElement parent { get; set; }
        public string regexp { get; set; }
        IEnumerable<Part> parts { get; set; }
        public string id { get; set; }
        public Expression( XElement expression, XElement regexps, ref Expressions expressions)
        {
            id = expression.Ancestors("regexp").First().Element("id").Value;
            parts = expression.Parts(regexps, ref expressions);
            regexp = parts.Select(p => p.Regexp.ToString()).StringConcat();
        }
    }
    
    public class Part
    {
        public XElement PartElement { get; set; }
        public string Type { get; set; }
        public string Regexp { get; set; }
        public string NextName { get; set; }
        public bool Result { get; set; }
        public string Location { get; set; }
        IEnumerable<Part> Parts { get; set; }
        public Part(XElement el, XElement regexps, ref Expressions expressions)
        {
            PartElement = el;
            string id = (string)el.Attributes("id").FirstOrDefault();
            if ((id == null ? "" : id) == "la2")
            {
                id = id;
            }
            Type = el.Attribute("type").Value;
            NextName = el.Attribute("name")== null ? "" : el.Attribute("name").Value;
            Parts = el.Parts(regexps, ref expressions);
            //if (Parts.Count() != 0)
            //{
            //    Regexp = Parts.Select(p => p.Regexp.ToString()).StringConcat();
            //}
            //else
            //{
            //    Regexp = (string)el.Elements("text").FirstOrDefault();
            //}
            Regexp = Parts.Select(p => p.Regexp.ToString()).StringConcat();
            Location = el.Attribute("location")== null ? "" : el.Attribute("location").Value;
            switch (Type)
            {
                case "external":
                    InTextLinkingData ild = new InTextLinkingData(true);

                    Regexp = ild.GetExternalExpression(NextName);
                    break;
                case "regexp":
                    XElement next = regexps.Descendants("regexp").Where(p => (p.Element("id") == null ? "" : p.Element("id").Value) == NextName).FirstOrDefault();
                    if (next == null)
                    {
                        Result = false;
                    }
                    Expression expression = next
                                        .Descendants("expression")
                                        .First()
                                        .GetExpression(regexps, ref expressions);
                    Regexp = expression.regexp;
                    break;
                case "single":
                    if (el.Element("text") != null) Regexp = el.Element("text").Value.Trim().ToString();
                    break;
                case "container":
                case "chain":
                    Regexp = "(" + Regexp + ")";
                    break;
                case "optional":
                    if (Parts.Count() != 0)
                    {
                        Regexp = "(" + Regexp + ")?";
                    }
                    else
                    {
                        Regexp = el.Element("text").Value.Trim().ToString();
                        Regexp = "(" + Regexp + ")?";
                    }
                    break;
                case "multiple":
                    Regexp = "(" + Regexp + ")+";
                    break;
                case "positive lookbehind":
                    Regexp = "(?<=" + Regexp + ")";
                    break;
                case "negative lookbehind":
                    Regexp = "(?<!" + Regexp + ")";
                    break;
                case "positive lookahead":
                    Regexp = "(?=" + Regexp + ")";
                    break;
                case "negative lookahead":
                    Regexp = "(?!" + Regexp + ")";
                    break;
                case "or":
                    Regexp = "";
                    foreach (Part p in Parts) Regexp = Regexp + (Regexp == "" ? p.Regexp : "|" + p.Regexp);
                    Regexp = "(" + Regexp + ")";
                    break;
                case "named":
                    Regexp = "(?<" + NextName + ">(" + Regexp + "))";
                    break;

            }
        }
    }

    
    public static class GlobalIL
    {
        public static Regex WordReplace = new Regex("([^\\x00-\\x7E\\xA1-\\xFF\\x152\\x153\\x160\\x161\\x178\\x192])");

        public static string HEXREPL(Match m)
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

        public class rQuery
        {
            public string name { get; set; }
            public Regex query { get; set; }
        }

        public class xSections
        {
            public List<xRange> ranges = new List<xRange>();
        }
        public class xRange
        {
            public int n { get; set; }
            public int start { get; set; }
            public int end { get; set; }
            public XNode node { get; set; }
            public List<xLink> links = new List<xLink>();
            public xRange(int n, int start, int end)
            {
                this.n = n;
                this.start = start;
                this.end = end;
            }
        }

        public  class xLink
        {
            public int start { get; set; }
            public int length { get; set; }
            public string tag1 { get; set; }
            public string tag2 { get; set; }
            public string regexpName { get; set; }
            public string idArea = "external";
            public string prefix = "";
            public string groupname1 { get; set; }
            public string groupname2 { get; set; }
            public xLink(int start, int length)
            {
                this.start = start;
                this.length = length;
            }
        }

        public  class xReference
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

        public class linkInfo
        {
            public string tag1 { get; set; }
            public string tag2 { get; set; }
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
            public string groupname1 { get; set; }
            public string groupname2 { get; set; }
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

        public static bool Between(this int num, int lower, int upper, bool inclusive = false)
        {
            return inclusive ? lower <= num && num <= upper : lower < num && num < upper;
        } 

        public static string StringConcat(this IEnumerable<string> source)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in source)
                sb.Append(s);
            return sb.ToString();
        }

        public static string StringConcat<T>(
                this IEnumerable<T> source,
                Func<T, string> projectionFunc)
        {
            return source.Aggregate(
                new StringBuilder(),
                (s, i) => s.Append(projectionFunc(i)),
                s => s.ToString());
        }



    }

    public static class BuildRegexp
    {

        public static Expression GetExpression(this XElement expression, XElement regexp, ref Expressions expressions)
        {
            string id = expression.Ancestors("regexp").Elements("id").Count() == 0 ? "" : expression.Ancestors("regexp").Elements("id").First().Value;
            if ((id == "" ? 0 : expressions.Where(p => p.id == id).Count()) == 0)
            {
                Expression ex = new Expression(expression, regexp, ref expressions);
                if (ex != null) expressions.Add(ex);
                return ex;
            }
            else
                return expressions.Where(p => p.id == id).First();
        }

        public static IEnumerable<Part> Parts(this XElement element, XElement regexp, ref Expressions expressions)
        {
            Expressions curr = expressions;
            return 
                from p in element.Elements("part")
                select new Part(p, regexp, ref curr);
        }

        public static string RegexBuildFromTag(this XElement el, XElement regexps)
        {
            Expressions expressions = new Expressions();
            if (el.Name.LocalName == "regexp")
            {
                return el
                    .Descendants("expression")
                    .First()
                    .GetExpression(regexps, ref expressions)
                    .regexp;
            }
            else
            {
                Part part = new Part(el,regexps,ref expressions);
                return part.Regexp;
            }
        }
        public static string RegexBuild(this XElement regexps, string id)
        {
            Expressions expressions = new Expressions();
                XElement build = regexps.Elements("regexp").Where(p => (p.Element("id") == null ? "" : p.Element("id").Value) == id).FirstOrDefault();
                return build
                    .Descendants("expression")
                    .First()
                    .GetExpression(regexps, ref expressions)
                    .regexp;
        
        }
    }

    public class ValidateRegexp
    {
        private XElement _RESULT = null;
        private int _BuildOrder = 0;

        public XElement GetBuild(XElement r, XElement regexps)
        {
            try
            {
                if (r.Descendants("part").Where(p => (p.Attribute("type") == null ? "" : p.Attribute("type").Value) == "").Count() != 0)
                {
                    XElement errors = new XElement("errors");
                    foreach (XElement part in r.Descendants("part").Where(p => (p.Attribute("type") == null ? "" : p.Attribute("type").Value) == ""))
                    {
                        XElement error = new XElement("error",
                            new XAttribute("id", r.Element("id").Value),
                            new XAttribute("message", "Mangler type"),
                            part);
                        errors.Add(error);
                    }
                    return errors;
                }
                else
                {
                    if (r.Descendants("part").Where(p => p.Attribute("type").Value == "regexp").Count() == 0)
                    {
                        if (_RESULT == null) _RESULT = new XElement("regexps");
                        if (_RESULT.Elements("regexp").Where(p => p.Element("id").Value == r.Element("id").Value).Count() == 0)
                        {
                            XElement regExp = new XElement(r);
                            _BuildOrder++;
                            if (regExp.Attribute("buildorder") == null)
                                regExp.Add(new XAttribute("buildorder", _BuildOrder.ToString()));
                            else
                                regExp.Attribute("buildorder").Value = _BuildOrder.ToString();

                            _RESULT.Add(regExp);
                        }

                        return _RESULT;
                    }
                    else
                    {
                        foreach (XElement part in r.Descendants("part").Where(p => p.Attribute("type").Value == "regexp"))
                        {
                            if (part.Attribute("name") == null)
                            {
                                XElement errors = new XElement("errors");
                                XElement error = new XElement("error",
                                    new XAttribute("id", r.Element("id").Value),
                                    new XAttribute("message", "Mangler attributt:name"),
                                    part);
                                errors.Add(error);
                                return errors;
                            }

                            XElement next = regexps.Descendants("regexp").Where(p => p.Element("id").Value == part.Attribute("name").Value).FirstOrDefault();
                            if (next == null)
                            {
                                XElement errors = new XElement("errors");
                                XElement error = new XElement("error",
                                    new XAttribute("id", part.Attribute("name").Value),
                                    new XAttribute("message", "Finnes ikke"),
                                    part);
                                errors.Add(error);
                                return errors;
                            }
                            else
                            {
                                XElement result = GetBuild(next, regexps);
                                if (result.Name.LocalName == "errors")
                                {
                                    return result;
                                }
                            }
                        }

                        if (_RESULT == null) _RESULT = new XElement("regexps");
                        if (_RESULT.Elements("regexp").Where(p => p.Element("id").Value == r.Element("id").Value).Count() == 0)
                        {
                            XElement regExp = new XElement(r);
                            _BuildOrder++;
                            if (regExp.Attribute("buildorder") == null)
                                regExp.Add(new XAttribute("buildorder", _BuildOrder.ToString()));
                            else
                                regExp.Attribute("buildorder").Value = _BuildOrder.ToString();

                            _RESULT.Add(regExp);
                        }
                        return _RESULT;
                    }
                    
                }
            }
            catch (SystemException err)
            {
                XElement errors = new XElement("errors");
                XElement error = new XElement("error",
                    new XAttribute("id", r.Element("id").Value),
                    new XAttribute("message", err.Message));
                errors.Add(error);
                return errors;
            }
        }
    }
}
