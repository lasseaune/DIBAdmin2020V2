using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Data.SqlTypes;
namespace DIBIndexingCLR
{
    public class IdTags
    {
        public XAttribute l { get; set; }
        public string x { get; set; }
    }
    public class TagName
    {
        public string xid { get; set; }
        public string Value { get; set; }
        public string Front { get; set; }
        public string Rest { get; set; }

        public TagName(XElement e)
        {
            xid = (string)e.Parent.Attributes("xid").FirstOrDefault();
            Value = e.Value.Trim().TrimEnd('.').FirstCharToUpper();
            Match m = Regex.Match(Value, @"^(?<front>((Lov om|Forskrift om|Forskrift til|Vedtak om |[A-ZÆØÅ]([a-zæøåA-ZÆØÅ0-9]|(?<=[a-zæøåA-ZÆØÅ])\-)+(\s([a-zæøåA-ZÆØÅ0-9]|(?<=[a-zæøåA-ZÆØÅ])\-)+){0,1})\s))(?<rest>(.+))");
            Front = m.Groups["front"].Value.ToLower();
            Rest = m.Groups["rest"].Value;
        }
    }

    public static class Extentions
    {
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
        public static void IdetifyLinksTag1(this XElement diblinks, XElement iddoc, string language)
        {
            (
             from t in diblinks.Descendants("idlinks").Where(p => p.Attributes("topic_id").FirstOrDefault() == null).Attributes("tag1").Where(p => p.Value != "" && Regex.IsMatch(p.Value, @"^x\d+$"))
             join xid in iddoc.Descendants("t").Where(p => ((string)p.Attributes("l").FirstOrDefault() ?? "") == language).Attributes("xid")
             on t.Value equals "x" + xid.Value
             select new IdTags { l = t, x = (string)xid.Parent.Attributes("id").FirstOrDefault() }
            ).ToList().ForEach(p => p.l.Parent.AddAttributte("topic_id", p.x));

            (
             from t in diblinks.Descendants("idlinks").Where(p => p.Attributes("topic_id").FirstOrDefault() == null).Attributes("tag1").Where(p => p.Value != "" && !Regex.IsMatch(p.Value, @"^x\d+$"))
             join xid in iddoc.Descendants("t").Where(p => ((string)p.Attributes("l").FirstOrDefault() ?? "") == language).Elements("w")
             on t.Value.ToLower() equals Regex.Replace(xid.Value.ToLower(), @"(s)?(?<=[a-zæøåA-ZÆØÅ])(lov(en|a)?)(?=(\s|$))", "lov")
             select new IdTags { l = t, x = (string)xid.Parent.Attributes("id").FirstOrDefault() }
            ).ToList()
            .ForEach(p => p.l.Parent.AddAttributte("topic_id", p.x));
        }
        public static XElement GetRootElement(this SqlXml xml)
        {
            if (xml.IsNull) return null;
            try
            {
                XmlDocument xmldoc = new XmlDocument();
                XmlReader r = xml.CreateReader();
                r.MoveToContent();
                xmldoc.Load(r);
                return XElement.Load(xmldoc.CreateNavigator().ReadSubtree());
            }
            catch
            {
                return null;
            }
        }
        public static string IsNull(this string s)
        {
            //if ((s == null ? null : (s.Trim() == "" ? null : s)) == "2.3.3")
            //{
            //}
            return (s == null ? null : (s.Trim() == "" ? null : s));
        }
        public static void AddAttributte(this XElement e, string name, string value)
        {
            if (e.Attributes(name).FirstOrDefault() == null)
            {
                e.Add(new XAttribute(name, value));
            }
        }
        public static bool Between(this int num, int lower, int upper, bool inclusive = false)
        {
            return inclusive ? lower <= num && num <= upper : lower < num && num < upper;
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
    }
}
