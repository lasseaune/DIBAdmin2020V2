using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DIBIndexingCLR
{
    public static class TagExtentions
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
    }
}
