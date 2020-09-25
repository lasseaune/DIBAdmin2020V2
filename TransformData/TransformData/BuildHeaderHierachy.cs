using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DIB.BuildHeaderHierachy
{
    class BuildHeaderHierachy
    {
        private int _minPara = 100;
        private int _maxPara = 0;
        private bool _FirstLevelText = false;
        private int _vedlCounter = 0;

        public BuildHeaderHierachy(int min, int max, bool FirstLevelText)
        {
            _minPara = min;
            _maxPara = max;
            _FirstLevelText = FirstLevelText;
        }
        private void _v0_SetParaValue(XElement e, string testId)
        {
            string fromPara = "";
            string toPara = "";
            if (testId.IndexOf("til") != -1)
            {
                fromPara = testId.Split("til".ToCharArray()).ElementAt(0).Trim();
                toPara = testId.Split("til".ToCharArray()).ElementAt(1).Trim();
            }
            else if (testId.IndexOf((char)0x2013) != -1)
            {
                fromPara = testId.Split((char)0x2013).ElementAt(0).Trim();
                toPara = testId.Split((char)0x2013).ElementAt(1).Trim();
            }
            else if (testId.IndexOf((char)0x2014) != -1)
            {
                fromPara = testId.Split((char)0x2014).ElementAt(0).Trim();
                toPara = testId.Split((char)0x2014).ElementAt(1).Trim();
            }

            testId = testId.Replace(" ", "_").ToUpper();
            if (!testId.StartsWith("ART"))
            {
                testId = "P" + testId;
            }
            XAttribute hid = new XAttribute("hid", testId);
            e.Add(hid);
            if (fromPara != "")
            {
                XAttribute fa = new XAttribute("frompara", fromPara);
                e.Add(fa);
                fa = new XAttribute("topara", toPara);
                e.Add(fa);
            }
        }




        private void _v0_GetFirstLevel(XElement sections
                                , List<XElement> h
                                , int min
                                , int max
                                , int level)
        {
            XElement e1 = null;
            int inMin = min;
            while (min < max)
            {
                for (; min < max; min++)
                {
                    e1 = h.ElementAt(min);
                    if (_v0_GetType(e1) == 200)
                    {
                        _v0_ExtractHeader(sections, h, ref min, ref max, _minPara, level + 1);
                        break;
                        //if (min >= max) return;
                        //if (_v0_GetType(h.ElementAt(min)) == 200) break;
                    }
                    if (_v0_GetType(e1) == 100 && _FirstLevelText == true)
                    {
                        _v0_ExtractHeader(sections, h, ref min, ref max, _minPara, level + 1);
                        break;
                        //if (min >= max) return;
                        //if (_v0_GetType(h.ElementAt(min)) == 200) break;
                    }
                    else if (_v0_GetType(e1) == 100 && _FirstLevelText == false)
                    {
                        int newMin = min + 1;
                        _v0_ExtractHeader(sections, h, ref min, ref newMin, _minPara, level + 1);
                        break;
                        //if (min >= max) return;
                        //if (_v0_GetType(h.ElementAt(min)) == 200) break;
                    }
                    else if (_v0_GetPara(e1) >= _minPara)
                    {
                        int paraLevel = _v0_GetPara(e1);
                        _v0_ExtractPara(sections, h, ref min, ref max, paraLevel, level + 1);
                        break;
                        //if (min >= max) return;
                        //if (_v0_GetType(h.ElementAt(min)) == 200) break;

                    }
                    else
                    {
                        _v0_ExtractHeader(sections, h, ref min, ref max, _minPara, level + 1);
                        break;
                        //if (min >= max) return;
                        //if (_v0_GetType(h.ElementAt(min)) == 200) break;
                    }

                }
            }

        }

        private void _v0_ExtractHeader(XElement section
                        , List<XElement> h
                        , ref int min
                        , ref int max
                        , int paraLevel
                        , int level)
        {
            XElement e1 = null;
            XElement last = null;
            int type = -1;
            while (min < max)
            {
                int inMin = min;
                for (; min < max; min++)
                {
                    e1 = h.ElementAt(min);

                    if (type == -1) type = _v0_GetType(e1);
                    if (_v0_GetType(e1) == type)
                    {
                        if (last != null && type == 100 && h.Count() > 1 && (min + 1) < max)
                        {

                            int ln = Convert.ToInt32(last.Attribute("n").Value);
                            int para00 = _v0_GetPara(h.ElementAt(ln - 1));
                            int type00 = _v0_GetType(h.ElementAt(ln - 1));
                            int para01 = _v0_GetPara(h.ElementAt(min + 1));
                            int type01 = _v0_GetType(h.ElementAt(min + 1));
                            //if (para00 > 0 && para01 > 0 && para00 > para01)
                            if (para00 > 0 && para00 == para01 && type00 == type01)
                            {
                                para00 = _v0_GetPara(h.ElementAt(ln + 1));
                                if (para00 > 0 && para00 != para01)
                                    return;
                            }

                        }
                        last = new XElement("section"
                                , e1.Attributes()
                                , new XAttribute("text", _v0_GetText(e1))
                                , new XAttribute("n", min.ToString())
                                , new XAttribute("level", level));
                        section.Add(last);
                    }
                    else
                    {
                        break;
                    }
                }
                if (min >= max) return;
                int para0 = _v0_GetPara(h.ElementAt(min));
                int type0 = _v0_GetType(h.ElementAt(min));

                if (para0 == paraLevel)
                {
                    _v0_ExtractPara(section.Elements("section").Last(), h, ref min, ref max, paraLevel, level + 1);
                    //if (_v0_GetType(h.ElementAt(min)) != type0) return;
                    if (min >= max) return;
                    //if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                    if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last) && level != 3 && _v0_GetType(h.ElementAt(min)) != 100) return;
                }
                else if (para0 == paraLevel + 1)
                {
                    _v0_ExtractPara(section.Elements("section").Last(), h, ref min, ref max, paraLevel + 1, level + 1);
                    if (min >= max) return;
                    //if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                    if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last) && level != 3 && _v0_GetType(h.ElementAt(min)) != 100) return;
                }

                else if (para0 == 0 && type0 != 100 && section.Elements("section").Last().Ancestors().Where(p => (p.Attribute("htype") == null ? -1 : Convert.ToInt32(p.Attribute("htype").Value)) == type0).Count() != 0)
                {
                    return;
                }
                else if (para0 == 0
                    && type0 == 100
                    && section.Elements("section").Last().Ancestors().Where(p => (p.Attribute("htype") == null ? -1 : Convert.ToInt32(p.Attribute("htype").Value)) == type0).Count() != 0)
                {
                    if ((min + 1) < max)
                    {
                        int para1 = _v0_GetPara(h.ElementAt(min + 1));
                        int type1 = _v0_GetType(h.ElementAt(min + 1));
                        if (para1 != 0 && para1 <= paraLevel)
                        {
                            return;
                        }
                    }
                }
                else if (type0 == 200) return;

                else
                {
                    if ((min + 2) < max)
                    {
                        int para1 = _v0_GetPara(h.ElementAt(min + 1));
                        int type1 = _v0_GetType(h.ElementAt(min + 1));
                        int para2 = _v0_GetPara(h.ElementAt(min + 2));
                        int type2 = _v0_GetType(h.ElementAt(min + 2));

                        if (level == 3 && type0 == 100)
                        {
                            _v0_ExtractHeader(section.Elements("section").Last(), h, ref min, ref max, paraLevel, level + 1);
                            if (min >= max) return;
                            //if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                            if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last) && level != 3 && _v0_GetType(h.ElementAt(min)) != 100) return;
                        }
                        else if (para0 == 0 && para1 == paraLevel)
                        {
                            _v0_ExtractHeader(section.Elements("section").Last(), h, ref min, ref max, paraLevel, level + 1);
                            if (min >= max) return;
                            //if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                            if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last) && level != 3 && _v0_GetType(h.ElementAt(min)) != 100) return;
                        }
                        else if (para0 == 0 && para1 == 0)
                        {
                            _v0_ExtractHeader(section.Elements("section").Last(), h, ref min, ref max, paraLevel, level + 1);
                            if (min >= max) return;
                            //if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                            if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last) && level != 3 && _v0_GetType(h.ElementAt(min)) != 100) return;
                        }
                        else if (para0 == 0 && para1 > paraLevel)
                        {
                            _v0_ExtractHeader(section.Elements("section").Last(), h, ref min, ref max, paraLevel + 1, level + 1);
                            if (min >= max) return;
                            if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last) && level != 3 && _v0_GetType(h.ElementAt(min)) != 100) return;
                        }
                        else
                            return;
                    }
                    else
                    {
                        if ((min + 1) < max)
                        {
                            int para1 = _v0_GetPara(h.ElementAt(min + 1));
                            int type1 = _v0_GetType(h.ElementAt(min + 1));
                            if (para1 > paraLevel)
                            {
                                _v0_ExtractPara(last, h, ref min, ref max, paraLevel + 1, level + 1);
                                if (min >= max) return;
                                //if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                                if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last) && level != 3 && _v0_GetType(h.ElementAt(min)) != 100) return;
                            }
                            else
                                return;
                        }
                    }

                }
                if (inMin == min)
                    min++;
            }

        }

        private void _v0_ExtractPara(XElement section
                                , List<XElement> h
                                , ref int min
                                , ref int max
                                , int paraLevel
                                , int level)
        {

            XElement e1 = null;
            XElement last = null;
            while (min < max)
            {

                for (; min < max; min++)
                {
                    e1 = h.ElementAt(min);
                    if (_v0_GetPara(e1) == paraLevel)
                    {
                        last = new XElement("section"
                                , e1.Attributes()
                                , new XAttribute("text", _v0_GetText(e1))
                                , new XAttribute("n", min.ToString())
                                , new XAttribute("level", level));
                        section.Add(last);
                    }
                    else
                        break;
                }
                if (min >= max) return;
                int para0 = _v0_GetPara(h.ElementAt(min));
                int type0 = _v0_GetType(h.ElementAt(min));

                if (type0 == 200) return;
                else if (para0 > 0 && para0 < paraLevel) return;
                else if (para0 > paraLevel)
                {
                    _v0_ExtractPara(section.Elements("section").Last(), h, ref min, ref max, paraLevel + 1, level + 1);
                    if (min >= max) return;
                    if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                }
                else if (para0 == 0 && type0 != 100 && section.Elements("section").Last().Ancestors().Where(p => (p.Attribute("htype") == null ? -1 : Convert.ToInt32(p.Attribute("htype").Value)) == type0).Count() != 0)
                {
                    return;
                }
                else if ((min + 2) < max)
                {
                    int para1 = _v0_GetPara(h.ElementAt(min + 1));
                    int type1 = _v0_GetType(h.ElementAt(min + 1));
                    int para2 = _v0_GetPara(h.ElementAt(min + 2));
                    int type2 = _v0_GetType(h.ElementAt(min + 2));


                    if (para0 > paraLevel)
                    {
                        _v0_ExtractPara(section.Elements("section").Last(), h, ref min, ref max, paraLevel + 1, level + 1);
                        if (min >= max) return;
                        if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                    }
                    else if (para0 == 0 && (para1 > paraLevel || para2 > paraLevel))
                    {
                        _v0_ExtractHeader(section.Elements("section").Last(), h, ref min, ref max, paraLevel + 1, level + 1);
                        if (min >= max) return;
                        if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                    }
                    else if (_v0_NextParaLevel(h, min, max, level) == paraLevel
                        && section.Elements("section").Last().Ancestors().Where(p => (p.Attribute("htype") == null ? -1 : Convert.ToInt32(p.Attribute("htype").Value)) == type0).Count() == 0)
                    {
                        _v0_ExtractHeader(section.Elements("section").Last(), h, ref min, ref max, paraLevel, level + 1);
                        if (min >= max) return;
                        if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;

                    }
                    else
                        return;

                }
                else
                {
                    if ((min + 1) < max)
                    {
                        int para1 = _v0_GetPara(h.ElementAt(min + 1));
                        int type1 = _v0_GetType(h.ElementAt(min + 1));
                        if (para0 > paraLevel)
                        {
                            _v0_ExtractPara(section.Elements("section").Last(), h, ref min, ref max, paraLevel + 1, level + 1);
                            if (min >= max) return;
                            if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                        }
                        else
                            return;
                    }
                    else
                        return;
                }
            }
        }

        private int _v0_NextParaLevel(List<XElement> h, int min, int max, int paraLevel)
        {
            int returnValue = 0;
            for (; min < max; min++)
            {
                XElement e = h.ElementAt(min);
                int pLevel = _v0_GetPara(e);
                if (pLevel > 0)
                {
                    returnValue = pLevel;
                    break;
                }
            }
            return returnValue;
        }

        private string _v0_GetText(XElement e)
        {
            return e.Value.Trim();
        }


        private string _v0_GetId(XElement e)
        {
            return e.Attribute("hid") == null ? "" : e.Attribute("hid").Value;
        }

        private int _v0_GetPara(XElement e)
        {
            return e.Attribute("hpara") == null ? 0 : Convert.ToInt32(e.Attribute("hpara").Value);
        }

        private int _v0_GetType(XElement e)
        {
            return e.Attribute("htype") == null ? 0 : Convert.ToInt32(e.Attribute("htype").Value);
        }

        public void _v0_GetHeaderType(List<XElement> h)
        {
            bool found = false;
            foreach (XElement e in h)
            {
                found = false;
                string testValue = e.Value.Trim();
                for (int i = 0; i < 21; i++)
                {
                    string testRegexp = "";
                    int para = 0;
                    switch (i)
                    {
                        case 0:
                            testRegexp = @"^(?<id>(del\s(\s+)?(\d+)))(\s+)?(\.|$)";
                            break;
                        case 1:
                            testRegexp = @"^(?<id>(del\s(\s+)?([ivx]+)))(\s+)?(\.|$)";
                            break;
                        case 2:
                            testRegexp = @"^(?<id>(del\s(\s+)?([a-z]+)))(\s+)?(\.|$)";
                            break;
                        case 3:
                            testRegexp = @"(?<id>(([^\.]+)\sdel))(\s+)?(\.|$)";
                            break;
                        case 4:
                            testRegexp = @"^(?<id>(kap(\.)?(it(t)?el)?(\s)?(\s+)?(\d)(\d+)?(\s+)?([a-z])?))(\:|\.|\s|$)";
                            break;
                        case 5:
                            testRegexp = @"^(?<id>(kap(\.)?(it(t)?el)?(\s)?(\s+)?[ivx]+))(\:|\.|\s|$)";
                            break;
                        case 6:
                            testRegexp = @"^(?<id>(\d+(\s)?(\.)?(\s)?kapit(t)?(el)?(let)?))(\.|\s|$)";
                            break;
                        case 7:
                            testRegexp = @"^(?<id>((fyrste|første|andre|annet|tredje|fjerde|femte|sjette|sjuande|sjuende|åttende|åttande|niende|niande|tiende|tiande)(\s)?(\.)?(\s)?kapit(t)?el))(\.|\s|$)";
                            break;
                        case 8:
                            testRegexp = @"^(?<id>([ivx]+))(\.|\s|$)";
                            break;
                        case 9:
                            testRegexp = @"^(?<id>([a-h]))(\s+)?(\.|$)";
                            break;
                        case 10:
                            testRegexp = @"^(?<id>(avdeling(\s)(\s+)?(\d+)))(\s)?(\s+)?(\.|$)";
                            break;
                        case 11:
                            testRegexp = @"^(?<id>(avdeling(\s)(\s+)?([ivx]+)))(\s)?(\s+)?(\.|$)";
                            break;
                        case 12:
                            testRegexp = @"^(?<id>(avsnitt(\s)(\s+)?(\d+)))(\s)?(\s+)?(\.|$)";
                            break;
                        case 13:
                            testRegexp = @"^(?<id>(avsnitt(\s)(\s+)?([ivx]+)))(\s)?(\s+)?(\.|$)";
                            break;
                        case 14:
                            testRegexp = @"^(Ny|Til((\sny)?|\sopphevelse(n)?\sav)|Endret)(\s+)(§+(\s)?)?(?<id>(\d+(\s)?([a-z])?((\-\d+(\s)?([a-z])?)+)?))(\s|\.|$)";
                            break;
                        case 15:
                            testRegexp = @"(^|^[^\.]+)((?<!\si\s)§+)(\s)?(?<id>(\d+(\s)?([a-z])?((\s+)?(til|" + (char)0x2013 + @")(\s+)?\d+(\s)?([a-z])?)?))(\s)?(\s+)?(\.|$)";
                            para = 1;
                            break;
                        case 16:
                            testRegexp = @"^(?<id>(art(\s)?(\s+)?(\d+)))(\s)?(\s+)?(\.|$)";
                            para = 1;
                            break;
                        case 17:
                            testRegexp = @"^(?<id>(art(\s)?(\s+)?([ivx]+)))(\s)?(\s+)?(\.|$)";
                            para = 1;
                            break;
                        case 18:
                            testRegexp = @"(^|^[^\.]+)((?<!\si\s)§+)(\s)?(?<id>(\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?((\s+)?(til|" + (char)0x2013 + @")(\s+)?\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?)?))(\s)?(\s+)?(\.|$|\s)";
                            para = 2;
                            break;
                        case 19:
                            testRegexp = @"(^|^[^\.]+)((?<!\si\s)§+)(\s)?(?<id>(\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?))(\s)?(\s+)?(\.|$|\s)";
                            para = 3;
                            break;
                        case 20:
                            testRegexp = @"(^|^[^\.]+)((?<!\si\s)§+)(\s)?(?<id>(\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?))(\s)?(\s+)?(\.|$|\s)";
                            para = 4;
                            break;

                    }

                    string testId = "";
                    Match m = Regex.Match(testValue, testRegexp, RegexOptions.IgnoreCase);
                    if (m.Groups["id"].Value != "")
                    {
                        testId = m.Groups["id"].Value;
                        testId = testId.Replace(".", "");
                        testId = testId.Replace(" ", "_").ToUpper();
                        if (i == 14)
                        {
                            int cc = 1;
                            foreach (Capture c in m.Groups["para"].Captures)
                            {
                                cc++;
                            }
                            para = cc;
                        }
                        XAttribute htype = new XAttribute("htype", i);
                        e.Add(htype);
                        if (para > 0)
                        {
                            _v0_SetParaValue(e, testId);
                        }
                        else
                        {
                            XAttribute hid = new XAttribute("hid", testId);
                            e.Add(hid);
                        }
                        XAttribute hpara = new XAttribute("hpara", para);
                        e.Add(hpara);
                        if (para > 0)
                        {
                            if (_minPara > para)
                            {
                                _minPara = para;
                            }
                            if (_maxPara < para)
                            {
                                _maxPara = para;
                            }
                        }
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    string testRegexp = @"^(til\skapittel|merknader|vedlegg(\s|$)|[^\.]+\s(vedlegg|\(ef\))\s)";
                    Match m = Regex.Match(testValue, testRegexp, RegexOptions.IgnoreCase);
                    if (m.Value != "")
                    {

                        XAttribute htype = new XAttribute("htype", 200);
                        e.Add(htype);
                        _vedlCounter++;
                        XAttribute hid = new XAttribute("hid", "vedlegg_" + _vedlCounter.ToString());
                        e.Add(hid);
                        XAttribute hpara = new XAttribute("hpara", 0);
                        e.Add(hpara);

                    }
                    else
                    {
                        XAttribute htype = new XAttribute("htype", 100);
                        e.Add(htype);
                        XAttribute hid = new XAttribute("hid", System.Guid.NewGuid().ToString().GetHashCode().ToString());
                        e.Add(hid);
                        XAttribute hpara = new XAttribute("hpara", 0);
                        e.Add(hpara);

                    }
                }
            }
        }


        private string GetTestRegExpPara(int i)
        {
            string testRegexp = "";
            switch (i)
            {
                case 0:
                    testRegexp = @"(^|[^\.]+)§+(\s)?(?<id>(\d+(\s)?([a-z])?((\s+)?(til|" + (char)0x2013 + @")(\s+)?\d+(\s)?([a-z])?)?))(\s)?(\s+)?(\.|$)";
                    break;
                case 1:
                    testRegexp = @"(^|[^\.]+)(?<id>(art(\s)?(\s+)?(\d+)))(\s)?(\s+)?(\.|$)";
                    break;
                case 2:
                    testRegexp = @"(^|[^\.]+)(?<id>(art(\s)?(\s+)?([ivx]+)))(\s)?(\s+)?(\.|$)";
                    break;
                case 3:
                    testRegexp = @"(^|[^\.]+)§+(\s)?(?<id>(\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?((\s+)?(til|" + (char)0x2013 + @")(\s+)?\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?)?))(\s)?(\s+)?(\.|$)";
                    break;
                case 4:
                    testRegexp = @"(^|[^\.]+)§(\s)?(?<id>(\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?))(\s)?(\s+)?(\.|$)";
                    break;
                case 5:
                    testRegexp = @"(^|[^\.]+)§(\s)?(?<id>(\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?))(\s)?(\s+)?(\.|$)";
                    break;

            }

            return testRegexp;
        }
        private string GetTestRegExpHeader(int i)
        {
            string testRegexp = "";
            switch (i)
            {
                case 0:
                    testRegexp = @"^(?<id>(del\s(\s+)?(\d+)))(\s+)?(\.|$)";
                    break;
                case 1:
                    testRegexp = @"^(?<id>(del\s(\s+)?([ivx]+)))(\s+)?(\.|$)";
                    break;
                case 2:
                    testRegexp = @"^(?<id>(del\s(\s+)?([a-z]+)))(\s+)?(\.|$)";
                    break;
                case 3:
                    testRegexp = @"(?<id>(([^\.]+)\sdel))(\s+)?(\.|$)";
                    break;
                case 4:
                    testRegexp = @"^(?<id>(kap(\.)?(it(t)?el)?(\s)?(\s+)?(\d)(\d+)?(\s+)?([a-z])?))(\:|\.|\s|$)";
                    break;
                case 5:
                    testRegexp = @"^(?<id>(kap(\.)?(it(t)?el)?(\s)?(\s+)?[ivx]+))(\:|\.|\s|$)";
                    break;
                case 6:
                    testRegexp = @"^(?<id>(\d+((\s)?(ste|net|dje|de|te)?)?(\s)?(\.)?(\s)?(K|k)apit(t)?(el)?(let)?))(\.|\s|$)";
                    break;
                case 7:
                    testRegexp = @"^(?<id>((fyrste|første|andre|annet|tredje|fjerde|femte|sjette|sjuande|sjuende|åttende|åttande|niende|niande|tiende|tiande)(\s)?(\.)?(\s)?kapit(t)?el))(\.|\s|$)";
                    break;
                case 8:
                    testRegexp = @"^(?<id>([ivx]+))(\.|\s|$)";
                    break;
                case 9:
                    testRegexp = @"^(?<id>([a-h]))(\s+)?(\.)";
                    break;
                case 10:
                    testRegexp = @"^(?<id>(avdeling(\s)(\s+)?(\d+)))(\s)?(\s+)?(\.)";
                    break;
                case 11:
                    testRegexp = @"^(?<id>(avdeling(\s)(\s+)?([ivx]+)))(\s)?(\s+)?(\.)";
                    break;
                case 12:
                    testRegexp = @"^(?<id>(avsnitt(\s)(\s+)?(\d+)))(\s)?(\s+)?(\.)";
                    break;
                case 13:
                    testRegexp = @"^(?<id>(avsnitt(\s)(\s+)?([ivx]+)))(\s)?(\s+)?(\.)";
                    break;
                case 14:
                    testRegexp = @"^(vedlegg\s|[^\.]+\s(vedlegg|\(ef\))\s)";
                    break;
                case 15:
                    testRegexp = @"^[a-zæøåÆØÅ]+(((\s)?(\,)?\s+[a-zæøåÆØÅ]+)+)?";
                    break;
            }

            return testRegexp;
        }

        private bool TryConvertToInt(string s)
        {

            int j;
            bool returnVal = Int32.TryParse(s, out j);

            return returnVal;
        }

        public XElement GetIndexBranch(List<XElement> h, bool AsIs, XElement holder)
        {
            bool found = false;

            int min = 0;
            int level = 1;
            XElement returnValue = holder;
            
            if (returnValue != null)
            {

                XElement sections = new XElement("sections");
                _v0_GetFirstLevel(sections, h, min, h.Count(), level);
                if (sections.HasElements)
                {
                    returnValue.Add(sections.Elements());
                    sections = returnValue;
                    //AddPara(sections, h);

                    foreach (XAttribute sa in sections.Descendants().Where(p => (p.Attribute("tid") == null ? "" : p.Attribute("tid").Value) != "").Attributes("tid"))
                    {
                        sa.Value = sa.Value.Replace("Ø", "OE");
                        sa.Value = sa.Value.Replace("Æ", "AE");
                        sa.Value = sa.Value.Replace("Å", "AA");
                    }
                    List<string> unik = new List<string>();
                    List<string> NotUnik = new List<string>();
                    found = true;
                    int ansCounter = 1;
                    while (found)
                    {
                        found = false;
                        foreach (XElement s in sections.Descendants().Where(p => p.Ancestors().Count() == ansCounter))
                        {
                            found = true;
                            string tid = s.Attribute("hid").Value;// == null ? s.Attribute("id").Value : s.Attribute("tid").Value;
                            if (!(tid.StartsWith("KAP") || tid.StartsWith("P")) && ansCounter > 1)
                            {
                                tid = s.Ancestors("section").First().Attribute("hid").Value + "_" + tid;
                                s.Attribute("hid").Value = tid;
                            }

                            string result = unik.Find(
                               delegate(string p)
                               {
                                   return p == tid;
                               });
                            if (result == null)
                            {
                                unik.Add(tid);
                            }
                            else
                            {
                                string resultNot = NotUnik.Find(
                                delegate(string p)
                                {
                                    return p == tid;
                                });
                                if (resultNot == null)
                                {
                                    NotUnik.Add(tid);
                                }
                            }
                        }
                        ansCounter++;
                    }
                    foreach (string u in NotUnik)
                    {
                        unik.Remove(u);
                    }
                    ansCounter = 1;
                    found = true;
                    while (found)
                    {
                        found = false;
                        foreach (XElement s in sections.Descendants().Where(p => p.Ancestors().Count() == ansCounter))
                        {
                            found = true;
                            string tid = s.Attribute("hid").Value;// == null ? s.Attribute("id").Value : s.Attribute("tid").Value;
                            string result = NotUnik.Find(
                               delegate(string p)
                               {
                                   return p == tid;
                               });
                            if (result == null)
                            {
                                unik.Add(tid);
                                if (s.Attribute("id") == null)
                                {
                                    XAttribute id = new XAttribute("id", tid);
                                    s.Add(id);
                                }
                                else
                                {
                                    s.Attribute("id").Value = tid;
                                }
                            }
                            else
                            {

                                //string parentId = s.Ancestors("section").Where(p => !TryConvertToInt(p.Attribute("hid").Value)).First().Attribute("hid").Value;//s.Parent.Attribute("hid").Value;// == null ? s.Parent.Attribute("tid").Value : s.Parent.Attribute("id").Value;
                                string parentId = s.AncestorsAndSelf("section").Where(p => !TryConvertToInt(p.Attribute("hid").Value)).First().Attribute("hid").Value;//s.Parent.Attribute("hid").Value;// == null ? s.Parent.Attribute("tid").Value : s.Parent.Attribute("id").Value;
                                string newId = parentId + "_" + tid;
                                string resultNot = unik.Find(
                                delegate(string p)
                                {
                                    return p == newId;
                                });
                                if (resultNot == null)
                                {
                                    unik.Add(newId);
                                    if (s.Attribute("id") == null)
                                    {
                                        XAttribute id = new XAttribute("id", newId);
                                        s.Add(id);
                                    }
                                    else
                                    {
                                        s.Attribute("id").Value = newId;
                                    }
                                }
                                else
                                {
                                    //debug.print("Ikke unik ID!");
                                }
                            }
                        }
                        ansCounter++;
                    }


                    returnValue = sections;

                }

            }


            return returnValue;

        }


    }
}
