using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;


namespace RegexEditor
{
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
    public static class InTextExtentions
    {
        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: return "";
                case "": return "";
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }
        private static List<string> GetGroupNames(Regex regex)
        {
            return regex
                        .GetGroupNames()
                        .Where(p => !Regex.IsMatch(p, @"^\d+$"))
                        .GroupBy(q => q)
                        .Select(r => r.Key)
                        .ToList();
        }

        public static string TagReplaceMatches(string text, MatchCollection mc, List<string> groupName)
        {
            int rangeStart = 0;
            int rangeCursor = 0;
            int rangeLength = text.Length;
            string result = "";
            List<Match> ml = new List<Match>();
            foreach (Match mark in mc.OfType<Match>())
            {
                string name = "";
                foreach (string s in groupName)
                {
                    if (mark.Groups[s].Success)
                    {
                        name = s;
                        break;
                    }
                }
                if (name != "")
                {
                    int markStart = mark.Index;
                    int markLength = mark.Length;

                    //Hvis linkens star er større enn Cursor legg tsubMark text foran link
                    if (markStart > rangeCursor)
                    {
                        string s = text.Substring((rangeCursor - rangeStart), ((markStart - rangeStart) - (rangeCursor - rangeStart)));
                        result = result + s;
                        rangeCursor = markStart;
                    }
                    //hvis lengden på linken er lengere enn lengden på rangens tekst forkort linken
                    if (((rangeCursor - rangeStart) + markLength) > text.Length)
                    {
                        markLength = text.Length - (rangeCursor - rangeStart);
                    }


                    if ((markLength - (rangeCursor - markStart)) > 0)
                    {
                        switch (name)
                        {
                            case "YEAR":
                                result = result + mark.Value.Replace("(", @"\(").Replace(")", @"\)");
                                break;
                            case "LOV":
                                result = result + @"(s)?(?<=[a-zæøåA-ZÆØÅ])(lov(en|a)?)(?=(\s|$))";
                                break;
                            case "MM":
                                result = result + @"(?<=\s)(m(\.)?(m|v)(\.)?)(?=(\s|$|\,|\.))";
                                break;
                            case "SQUARE":
                            //result = result + @"(\[[a-zæøåA-ZÆØÅ0-9\.\-\s]+\])?";
                            //break;
                            case "BRAC":
                                //result = result + @"(\([a-zæøåA-ZÆØÅ0-9\.\-\s]+\))?";
                                break;
                            case "CAPT":
                                result = result + "(" + mark.Value + "|" + mark.Value.ToLower() + ")";
                                break;
                            case "SIGN":
                                result = result + @"(\" + mark.Value + ")?";
                                break;
                            case "ELLER":
                                result = result + @"(\s)?\/\-(\s)?";
                                break;
                            case "HYPEN":
                                result = result + @"(\s)?\-(\s)?";
                                break;
                            default:
                                result = result + text.Substring((rangeCursor - rangeStart), markLength - (rangeCursor - markStart));
                                break;
                        }


                    }
                    rangeCursor = rangeCursor + (markLength - (rangeCursor - markStart));
                }

            }

            if ((rangeCursor - rangeStart) < text.Length)
            {
                string clean = text.Substring((rangeCursor - rangeStart), text.Length - (rangeCursor - rangeStart));
                result = result + clean;

            }

            Regex test = new Regex(result);
            Match m = test.Match(text);
            if (m.Success)
            {
                //Debug.Print(m.Value);
            }
            else
            {

            }
            return result;
        }
        public static List<string> SetFrontTags(this IGrouping<string, TagName> value)
        {
            List<string> result = new List<string>();
            string front = "";
            switch (value.Key.ToLower())
            {
                case "lov om ": front = @"(L|l)ov\som\s"; break;
                case "forskrift om ": front = @"(F|f)orskrift\som\s"; break;
                case "forskrift til ": front = @"(F|f)orskrift\stil\s"; break;
                case "vedtak om ": front = @"(F|f)orskrift\stil\s"; break;

            }
            if (front != "")
            {
                List<string> rest = value.GroupBy(p => p.Rest).Where(p => p.GroupBy(r => r.xid).Count() == 1).Select(p => p.SetTagsRest()).ToList();
                string or = rest.OrderByDescending(p => p.Length).Select(p => p).StringConcatenate("|");
                result.Add(front + "(" + or + ")");
            }
            else if (value.GroupBy(p => p.Value).Select(p => p.Key).Count() > 1)
            {
                //Debug.Print(value.Key);
            }
            else if (value.Select(p => p).Count() == 1)
            {
                //Debug.Print(value.Key);
            }

            return result;

        }
        public static string SetTagsRest(this IGrouping<string, TagName> rest)
        {
            string result = "";
            string sregex = "("
                    + @"(?<MM>((?<=\s)(m(\.)?(m|v)(\.)?)(?=(\s|$))))"
                    + @"|(?<YEAR>(\(\d\d\d\d\)))$"
                    + @"|(?<SQUARE>(\[[a-zæøåA-ZÆØÅ0-9\.\-\s\/]+\]))$"
                    + @"|(?<BRAC>(\([a-zæøåA-ZÆØÅ0-9\.\-\s\/]+\)))$"
                    + @"|(?<LOV>((s)?(?<=[a-zæøåA-ZÆØÅ])(lov(en|a)?)(?=(\s|$))))"
                    + @"|(?<ELLER>((\s)?\/\-(\s)?))"
                    + @"|(?<HYPEN>((\s)?\-(\s)?))"
                    + @"|(?<CAPT>([A-ZÆØÅ]))"
                    + @"|(?<SIGN>((?!<\\)[\.\,\-\&\/\(\)\[\]]))"
                    + @")";

            Regex rx = new Regex(sregex);
            List<string> gr = GetGroupNames(rx);
            MatchCollection mc = rx.Matches(rest.Key);
            if (mc.Count > 0)
            {
                result = TagReplaceMatches(rest.Key, mc, gr);
            }
            else
            {
                result = rest.Key;
            }
            if (rest.GroupBy(p => p.xid).Count() == 1)
            {
                return "(?<x" + rest.GroupBy(p => p.xid).Select(p => p.Key).First() + ">(" + result + "))";
            }
            return result;

        }
        public static string SetTags(this IGrouping<string, TagName> value)
        {


            string result = "";
            string sregex = "("
                    + @"(?<MM>((?<=\s)(m(\.)?(m|v)(\.)?)(?=(\s|$))))"
                    + @"|(?<YEAR>(\(\d\d\d\d\)))$"
                    + @"|(?<SQUARE>(\[[a-zæøåA-ZÆØÅ0-9\.\-\s\/]+\]))$"
                    + @"|(?<BRAC>(\([a-zæøåA-ZÆØÅ0-9\.\-\s\/]+\)))$"
                    + @"|(?<LOV>((s)?(?<=[a-zæøåA-ZÆØÅ])(lov(en|a)?)(?=(\s|$))))"
                    + @"|(?<ELLER>((\s)?\/\-(\s)?))"
                    + @"|(?<HYPEN>((\s)?\-(\s)?))"
                    + @"|(?<CAPT>([A-ZÆØÅ]))"
                    + @"|(?<SIGN>((?!<\\)[\.\,\-\&\/\(\)\[\]]))"
                    + @")";

            Regex rx = new Regex(sregex);
            List<string> gr = GetGroupNames(rx);
            MatchCollection mc = rx.Matches(value.Key);
            if (mc.Count > 0)
            {
                result = TagReplaceMatches(value.Key, mc, gr);
            }
            else
            {
                result = value.Key;
            }
            if (value.GroupBy(p => p.xid).Count() == 1)
            {
                return "(?<" + "x" + value.GroupBy(p => p.xid).Select(p => p.Key).First() + ">(" + result + "))";
            }
            return result;

        }
        public static string SetTags(this string value)
        {
            if (value.IndexOf("overg.regler") > -1)
            { }
            string result = "";
            string sregex = "("
                    + @"(?<MM>((?<=\s)(m(\.)?(m|v)(\.)?)(?=(\s|$))))"
                    + @"|(?<YEAR>(\(\d\d\d\d\)))$"
                    + @"|(?<SQUARE>(\[[a-zæøåA-ZÆØÅ0-9\.\-\s\/]+\]))$"
                    + @"|(?<BRAC>(\([a-zæøåA-ZÆØÅ0-9\.\-\s\/]+\)))$"
                    + @"|(?<LOV>((s)?(?<=[a-zæøåA-ZÆØÅ])(lov(en|a)?)(?=(\s|$))))"
                    + @"|(?<ELLER>((\s)?\/\-(\s)?))"
                    + @"|(?<HYPEN>((\s)?\-(\s)?))"
                    + @"|(?<CAPT>([A-ZÆØÅ]))"
                    + @"|(?<SIGN>((?!<\\)[\.\,\-\&\/\(\)\[\]]))"
                    + @")";

            Regex rx = new Regex(sregex);
            List<string> gr = GetGroupNames(rx);
            MatchCollection mc = rx.Matches(value);
            if (mc.Count > 0)
            {
                result = TagReplaceMatches(value, mc, gr);
            }
            else
            {
                result = value;
            }
            return result;
        }
        public static string BuildRegexFromIdDoc(this XElement idDoc)
        {
            if (idDoc==null ? true : idDoc.Name.LocalName!="tags") return null;

            string front = "forskr om //forskrift om //forskrifter om |lov om |forskrift til |vedtak om |stortingsvedtak om |retningslinjer for |særavtale for |opptaksregler for |årsregnskap for |veiledende retningslinjer |stortingets skattevedtak ";
            XElement e = idDoc;
            if (e == null) return null;
            List<TagName> tagnames = e.Descendants("t").Elements("w")
                .Where(p => !p.Value.StartsWith("_") && ((string)p.Parent.Attributes("p").FirstOrDefault() ?? "0") == "1")
                .Where(p =>
                    (!Regex.IsMatch(p.Value.Trim(), @"^([a-zæøåA-ZÆØÅ0-9]|(?<=[a-zæøåA-ZÆØÅ])\-)+$"))
                    ||
                    (Regex.IsMatch(p.Value.Trim(), @"^([a-zæøåA-ZÆØÅ0-9]|(?<=[a-zæøåA-ZÆØÅ])\-)+$") && !Regex.IsMatch(p.Value.Trim(), @"(forskrift(en)?|lov(en|a)?)$")))
                .Where(p => "short;short_name;abbrev;dib_name;public".Split(';').Contains((string)p.Attributes("t").FirstOrDefault() ?? ""))
                .Select(p => new TagName(p))
                .ToList();
            List<string> sourcenames = new List<string>();


            string av = @"(\sav)?\s(?<mlovdate>((?<lovdate>((((?<year>(\d{4,4}))([\s\.\-]+)+(?<month>(((([0-1])([0-9])?)|(jan(uar)?|feb(ruar)?|mar(s)?|apr(il)?|mai|jun(i)?|jul(i)?|aug(ust)?|sep(tember)?|okt(ober)?|nov(ember)?))))([\s\.\-]+)+(?<day>((([0-3])?([0-9])))))|((?<day>((([0-3])?([0-9]))))([\s\.\-]+)+(?<month>((((J|j)anuar|(J|j)an|(F|f)ebruar|(F|f)eb|(M|m)ars|(M|m)ar|(A|a)pril|(A|a)pr|(M|m)ai|(J|j)uni|(J|j)un|(J|j)uli|(J|j)ul|(A|a)ugust|(A|a)ug|(S|s)eptember|(S|s)ep|(O|o)ktober|(O|o)kt|(N|n)ovember|(N|n)ov|(D|d)esember|(D|d)es)|(([0-1])([0-9])?))))([\s\.\-]+)+(?<year>((\d{2,2}|\d{4,4})))))))(([\s\.\-]+)+(N|n)r)?([\s\.\-]+)+(?<number>(\d{1,4}))))";


            foreach (string s in front.Split('|'))
            {
                string sFront = "";
                switch (s)
                {
                    case "forskr om //forskrift om //forskrifter om ": sFront = @"(F|f)orskr(\.)?(ift(er)?)?(" + av + @")?\som\s"; break;
                    case "lov om ": sFront = @"(L|l)ov(" + av + @")?\som\s"; break;
                    case "forskrift til ": sFront = @"(F|f)orskr(\.)?(ift)?\stil\s"; break;
                    case "vedtak om ": sFront = @"(V|v)edtak\som\s"; break;
                    case "stortingsvedtak om ": sFront = @"(S|s)tortingsvedtak\som\s"; break;
                    case "retningslinjer for ": sFront = @"(R|r)etningslinjer\sfor\s"; break;
                    case "særavtale for ": sFront = @"(S|s)æravtale\sfor\s"; break;
                    case "opptaksregler for ": sFront = @"(O|o)pptaksregler\sfor\s"; break;
                    case "årsregnskap for ": sFront = @"(Å|å)rsregnskap\sfor\s"; break;
                    case "veiledende retningslinjer ": sFront = @"(V|v)eiledende\sretningslinjer\s"; break;
                    case "stortingets skattevedtak ": sFront = @"(S|s)tortingets\sskattevedtak\s"; break;
                }
                if (sFront == "") return null;
                string rest = tagnames
                     .Where(p => Regex.Split(s.ToLower(), @"\/\/").Contains(p.Front.ToLower()))
                     .GroupBy(p => p.Rest)
                     .OrderByDescending(p => p.Key)
                     .Select(p => p.SetTagsRest())
                     .Select(p => p)
                     .StringConcatenate("|");

                string regexp = "(" + sFront + "(" + rest + "))";
                Regex test = new Regex(regexp);
                int testvalue = tagnames
                     .Where(p => Regex.Split(s.ToLower(), @"\/\/").Contains(p.Front.ToLower()))
                     .GroupBy(p => p.Rest)
                     .OrderByDescending(p => p.Key)
                     .SelectMany(p => p.Select(r => test.Match(r.Value)))
                     .Where(p => !p.Success)
                     .Count();
                if (testvalue > 0)
                {
                    return null;
                }
                sourcenames.Add(regexp);
            }
            sourcenames.AddRange(
                tagnames
                     .Where(p => !front.Split('|').SelectMany(r => Regex.Split(r.ToLower(), @"\/\/")).ToList().Contains((p.Front == null ? "" : p.Front.ToLower()))
                     && !Regex.IsMatch(p.Value, @"\s")
                     && !Regex.IsMatch(p.Value, @"(?<!(loven\sog\s|\/)[A-ZØÆÅa-zæøå]+)(lov(a|en)?|selskap|forskrift(en|a)?|ABC|veileder)$")
                     )
                     .GroupBy(p => p.Value)
                     .OrderByDescending(p => p.Key.Length)
                     .Select(p => p.SetTags())
                     .Select(p => p)
            );

            string result = "(" + sourcenames.Select(p => p).StringConcatenate("|") + @")(\.)?(?=\s§) ";

            return result;
        }
    }
    public class InTextLinkingAbbrevations
    {
        public string Tag1()
        {
            //New tag1
            XElement e = XElement.Load(@"D:\_RegexEditor\idpart_new.xml");
            string s = e.Value;
            //XElement iddoc = XElement.Load(@"D:\_RegexEditor\iddoc.xml");
            //string s = iddoc.BuildRegexFromIdDoc();
            //string s = File.ReadAllText(@"D:\DIBProduction\test.txt");

            return "(" + s + ")";
        }
        public string GetAbbrevations(List<string> abbrevations)
        {
            string returnValue = string.Empty;
            foreach (string abbrevation in abbrevations)
            {
                
                string expression = ReplaceCharWithRegexp(abbrevation);
                if (expression!= string.Empty)
                    returnValue += (returnValue == string.Empty ? expression : "|" + expression);
                
            }
            if (returnValue != string.Empty)
            {
                //returnValue = returnValue + @"|([A-ZÆØÅ])+|([a-zæøåA-ZÆØÅ\-])+(l|L|F|f)(\.|ov(a|en(e)?)?|orskrift(en))?";
                returnValue = "(" + returnValue + ")";
            }
            return returnValue;
        }
        
        private string ReplaceCharWithRegexp(string abbrevation)
        {
            string returnValue = string.Empty;
            try
            {
                if (Regex.IsMatch(abbrevation, "^[a-zæøå][a-zæøå]"))
                    {
                        abbrevation = "(" + abbrevation.Substring(0, 1).ToUpper() + "|" + abbrevation.Substring(0, 1).ToLower() + ")" + abbrevation.Substring(1);
                    }
                    abbrevation = Regex.Replace(abbrevation, @"\s+", @"\s+");
                    abbrevation = Regex.Replace(abbrevation, @"\/", @"(\s+)?\/(\s+)?");
                    abbrevation = abbrevation.Replace(@".", @"(\.)?");
                    abbrevation = abbrevation.Replace(@"(2015)", @"\(2015\)");
                    returnValue = abbrevation;
            }
            catch (SystemException e)
            {
                returnValue = string.Empty;
            }
            return returnValue;
        }

    }
}
