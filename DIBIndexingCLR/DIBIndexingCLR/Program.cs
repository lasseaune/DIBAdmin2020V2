using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;

namespace DIBIndexingCLR
{
    class Program
    {
        static void Main(string[] args)
        {


            //XElement document = XElement.Load(@"D:\_DIBIndexingCLR\segments.xml");
            XElement document = XElement.Load(@"D:\_DIBIndexingCLR\document5.xml");
            document.Descendants("searchitems").ToList().ForEach(p => p.Remove());
            document.Descendants("index").ToList().ForEach(p => p.Remove());
            document.Descendants("diblink").ToList().ForEach(p => p.Remove());
            document.Descendants().Where(p => p.Attributes("id").FirstOrDefault() == null).ToList().ForEach(p => p.Add(new XAttribute("id", Guid.NewGuid().ToString())));
            XElement regexps = XElement.Load(@"D:\_DIBIndexingCLR\100_total_no_20200930_new.xml");
            string regex = regexps.DescendantsAndSelf("root").Select(p => p.Value).FirstOrDefault();
            Regex rx = null;
            rx = new Regex(regex);
            

            //<a class="diblink" data-refid="m148" data-replacetext="(#link;{E9C8D41B-183C-437C-B533-72049F2DF7B6}; 7c147780-2f64-41be-93e0-9fc8372e3150; Frivillig yrkesskadetrygd for selvstendig oppdragstakere;#)" href="#m148">Frivillig yrkesskadetrygd for selvstendig oppdragstakere</a>
            while (document.Descendants().Where(p => "diblink;dibparameter".Split(';').Contains(p.Name.LocalName)).Count() != 0)
            {
                document.Descendants().Where(p => "diblink;dibparameter".Split(';').Contains(p.Name.LocalName)).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
            }

            while (document.Descendants("a").Where(p => ((string)p.Attributes("class").FirstOrDefault() ?? "").Trim().ToLower() == "diblink" && ((string)p.Attributes("data-replacetext").FirstOrDefault() ?? "").Trim() != "").Count() != 0)
            {
                document.Descendants("a").Where(p => ((string)p.Attributes("class").FirstOrDefault() ?? "").Trim().ToLower() == "diblink" && ((string)p.Attributes("data-replacetext").FirstOrDefault() ?? "").Trim() != "")
                    .ToList()
                    .ForEach(p => p.ReplaceWith(new XText((string)p.Attributes("data-replacetext").FirstOrDefault())));
            }

            while (document.Descendants("a").Where(p => ((string)p.Attributes("class").FirstOrDefault() ?? "").Trim().ToLower() == "diblink").Count() != 0)
            {
                document.Descendants("a").Where(p => ((string)p.Attributes("class").FirstOrDefault() ?? "").Trim().ToLower() == "diblink").ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
            }

            document.Descendants("span").Where(p => Regex.IsMatch(p.DescendantNodes().OfType<XText>().Select(s => s.Value).StringConcatenate(), @"^\s+$")).ToList().ForEach(p => p.ReplaceWith(new XText(" ")));
            document.Descendants("span").Where(p => p.DescendantNodes().Count() == 0).ToList().ForEach(p => p.ReplaceWith(new XText(" ")));


            Stream stream = new MemoryStream();
            document.Save(stream);
            // Rewind the stream ready to read from it elsewhere
            stream.Position = 0;
            document = XElement.Load(stream);


            XElement actions = XElement.Load(@"D:\_DIBIndexingCLR\InTextLinksActionsNOR.xml");
            if (actions == null)
            {
                return;
            }

            XElement iddoc = XElement.Load(@"D:\_DIBIndexingCLR\iddoc_20200930.xml");
            if (iddoc == null)
            {
                return;
            }

            string language = "no";

            TextProcuctionObject tpo = new TextProcuctionObject();
            tpo.TextSize = 5000;
            tpo.SetActions(rx, actions);
            tpo.ExecuteTextProcuction(document, language);

            XElement diblinks = tpo.Links;

            diblinks.IdetifyLinksTag1(iddoc, language);

            if (language == "no")
            {
                language = "en";
                diblinks.IdetifyLinksTag1(iddoc, language);
            }


            XElement result = new XElement("package",
                document,
                diblinks,
                tpo.XIndex
            );
            document.Save(@"D:\_DIBIndexingCLR\document.xml");
            tpo.XIndex.Save(@"D:\_DIBIndexingCLR\XIndex.xml");
        }
        static void Main0(string[] args)
        {
            string front = "forskr om //forskrift om //forskrifter om |lov om |forskrift til |vedtak om |stortingsvedtak om |retningslinjer for |særavtale for |opptaksregler for |årsregnskap for |veiledende retningslinjer |stortingets skattevedtak ";
            string filename = @"D:\DIBProduction\iddoc.xml";
            XElement e = XElement.Load(filename);

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
                if (sFront == "") return;
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
                    return;
                }
                sourcenames.Add(regexp);
            }
            sourcenames.AddRange(
                tagnames
                     .Where(p => !front.Split('|').SelectMany(r => Regex.Split(r.ToLower(), @"\/\/")).ToList().Contains((p.Front==null ? "" : p.Front.ToLower()))
                     && !Regex.IsMatch(p.Value, @"\s")
                     && !Regex.IsMatch(p.Value, @"(?<!(loven\sog\s|\/)[A-ZØÆÅa-zæøå]+)(lov(a|en)?|selskap|forskrift(en|a)?|ABC|veileder)$")
                     )
                     .GroupBy(p => p.Value)
                     .OrderByDescending(p => p.Key.Length)
                     .Select(p => p.SetTags())
                     .Select(p => p)
            );

            string result = "(" + sourcenames.Select(p => p).StringConcatenate("|") + @")(\.)?(?=\s§) ";
            File.WriteAllText(@"D:\DIBProduction\test.txt", result);
        }
        static void Main1(string[] args)
        {
            string filename = @"D:\DIBProduction\iddoc.xml";
            XElement e = XElement.Load(filename);
            //e.Descendants().Attributes("xid").ToList().ForEach(p => p.Remove());
            //int x = 1;
            //foreach (IGrouping<string, XElement> t in  e.Descendants("t").GroupBy(p=>((string)p.Attributes("id").FirstOrDefault()).ToLower()))
            //{
            //    XAttribute a = new XAttribute("xid", "x" + (x++).ToString());
            //    t.Select(p => p).ToList().ForEach(p => p.Add(a));
            //}
            //e.Save(filename);
            //return;

            List<TagName> tagnames = e.Descendants("t").Elements("w")
                .Where(p => !p.Value.StartsWith("_") && ((string)p.Parent.Attributes("p").FirstOrDefault() ?? "0") == "1")
                .Where(p =>
                    (!Regex.IsMatch(p.Value.Trim(), @"^([a-zæøåA-ZÆØÅ0-9]|(?<=[a-zæøåA-ZÆØÅ])\-)+$"))
                    ||
                    (Regex.IsMatch(p.Value.Trim(), @"^([a-zæøåA-ZÆØÅ0-9]|(?<=[a-zæøåA-ZÆØÅ])\-)+$") && !Regex.IsMatch(p.Value.Trim(), @"(forskrift(en)?|lov(en|a)?)$")))
                .Where(p => "short;short_name;abbrev;dib_name;public".Split(';').Contains((string)p.Attributes("t").FirstOrDefault() ?? ""))
                .Select(p => new TagName(p))
                .ToList();

            List<string> front = tagnames
                .Where(p=>(p.Front == null ? "" : p.Front).Trim()!="")
                .GroupBy(p => p.Front)
                .OrderByDescending(p=>p.Count())
                .Select(p => p.Key + " / "+ p.Count().ToString() +  "/" + p.Select(r=>r.Rest).FirstOrDefault())
                .ToList();

            string sfront = tagnames
                .Where(p => (p.Front == null ? "" : p.Front).Trim() != "")
                .GroupBy(p => p.Front)
                .OrderByDescending(p => p.Count())
                .Select(p => p.Key)
                .Take(12)
                .StringConcatenate("|");
            List<string> tagvalue = tagnames.Select(r => r.Value.ToLower().TrimEnd('.')).ToList();




            List<string> tag1 = e.Descendants("t").Elements("w").Where(p => !tagvalue.Contains(p.Value.ToLower().TrimEnd('.'))).Select(p => p.Value).OrderBy(p=>p).ToList();


            List<string> name = e.Descendants("t").Elements("w")
                .Where(p => !p.Value.StartsWith("_"))
                .Where(p => (!Regex.IsMatch(p.Value.Trim(), @"^([a-zæøåA-ZÆØÅ0-9]|(?<=[a-zæøåA-ZÆØÅ])\-)+$")) || (Regex.IsMatch(p.Value.Trim(), @"^([a-zæøåA-ZÆØÅ0-9]|(?<=[a-zæøåA-ZÆØÅ])\-)+$") && !Regex.IsMatch(p.Value.Trim(), @"(forskrift(en)?|lov(en|a)?)$")))
                .Where(p => "short;short_name;abbrev;dib_name;public".Split(';').Contains((string)p.Attributes("t").FirstOrDefault() ?? ""))
                .Select(p => new TagName(p))
                .GroupBy(p=> p.Front)
                .SelectMany(p => p.SetFrontTags())
                .ToList();

            string s =  name.OrderByDescending(p=>p.Length).ToList().Select(p => "(" + p + ")").StringConcatenate("|");
            Regex test = new Regex("(" + s + ")");

            name = e.Descendants("t").Elements("w")
                .Where(p => !p.Value.StartsWith("_"))
                .Where(p => (!Regex.IsMatch(p.Value.Trim(), @"^[a-zæøåA-ZÆØÅ0-9\-]+$")) || (Regex.IsMatch(p.Value.Trim(), @"^[a-zæøåA-ZÆØÅ0-9\-]+$") && !Regex.IsMatch(p.Value.Trim(), @"(forskrift(en)?|lov(en|a)?)$")))
                .Where(p => "short;short_name;abbrev;dib_name;public".Split(';').Contains((string)p.Attributes("t").FirstOrDefault() ?? ""))
                .GroupBy(p => p.Value.Trim().TrimEnd('.').FirstCharToUpper())
                .Select(p =>p.Key)
                .OrderBy(p => p.Length)
                .ThenBy(p => p)
                .ToList();
            foreach (string text in name)
            {
                Match m = test.Match(text);
                if (m.Success)
                {
                    Debug.Print(m.Value);
                }
                else
                {

                }
            }

            File.WriteAllText(@"D:\DIBProduction\test.txt", s);
        }
        static void Main2(string[] args)
        {
            DateTime start = DateTime.Now;
            string path = @"D:\DIBProduction\";
            //string path = @"D:\_books\2019\Skatte-ABC\3\";
            //string path = @"D:\_books\2019\mva\xml\";
            string filename = path + "document19.xml";
            XElement e = XElement.Load(filename);

            //e = e.Elements("segment").Where(p => (string)p.Attributes("segment_id").FirstOrDefault() == "pack1").Elements("document").FirstOrDefault();
            //e = new XElement(e);
            e.Descendants("a").Where(p => (string)p.Attributes("class").FirstOrDefault() == "diblink").ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
            e.Descendants("span").Where(p => Regex.IsMatch(p.DescendantNodes().OfType<XText>().Select(s=>s.Value).StringConcatenate(), @"^\s+$")).ToList().ForEach(p => p.ReplaceWith(new XText(" ")));
            foreach (XElement c in e.Descendants("class").Reverse().ToList())
            {
                string name = c.Value;
                c.Parent.Add(new XAttribute("class", name));
                c.Remove();

            }
            Stream stream = new MemoryStream();
            e.Save(stream);
            // Rewind the stream ready to read from it elsewhere
            stream.Position = 0;

            //e.Save(path +  @"segments0.xml");
            e = XElement.Load(stream);

            //e = XElement.Parse(e.ToString(), LoadOptions.PreserveWhitespace);

            XElement regexps = XElement.Load(@"D:\RegExp\NOR\2020\100_total_no.xml");
            string regex = regexps.DescendantsAndSelf("root").Select(p => p.Value).FirstOrDefault();
            Regex rx = new Regex(regex);
            XElement actions = XElement.Load(@"d:\regexp\NOR\2020\InTextLinksActionsNOR.xml");
            string language = "no";

            filename = @"D:\DIBProduction\iddoc.xml";
            XElement iddoc = XElement.Load(filename);

            XElement segments = new XElement("segments");
            XElement diblinks = new XElement("diblinks");
            TextProcuctionObject tpo = new TextProcuctionObject();
            tpo.TextSize = 5000;
            tpo.SetActions(rx, actions);

            XElement ws = e.CreateIndex();
            if (e.DescendantsAndSelf().Where(p=>p.Name.LocalName=="segment").Count()>0)
            {
                foreach (XElement segment in e.Elements("segment").Where(p => p.Elements("document").Count() > 0))
                {
                    XElement document = new XElement(segment.Elements("document").FirstOrDefault());
                    tpo.ExecuteTextProcuction(document, language);
                    XElement newsegment = new XElement(segment.Name.LocalName, segment.Attributes(), segments.Elements("index").FirstOrDefault(), document);
                    segments.Add(newsegment);
                    diblinks.Add(tpo.Links.Elements("idlinks"));
                }


                (
                    from t in diblinks.Descendants("idlinks").Attributes("tag1").Where(p => p.Value != "" && Regex.IsMatch(p.Value, @"^x\d+$"))
                    join xid in iddoc.Descendants("t").Attributes("xid")
                    on t.Value equals "x" + xid.Value
                    select new IdTags { l = t, x = (string)xid.Parent.Attributes("id").FirstOrDefault() }
                )
                .ToList()
                .ForEach(p => p.l.Parent.AddAttributte("topic_id", p.x));

                (
                     from t in diblinks.Descendants("idlinks").Attributes("tag1").Where(p => p.Value != "" && !Regex.IsMatch(p.Value, @"^x\d+$"))
                     join xid in iddoc.Descendants("t").Elements("w")
                     on t.Value.ToLower() equals Regex.Replace(xid.Value.ToLower(), @"(s)?(?<=[a-zæøåA-ZÆØÅ])(lov(en|a)?)(?=(\s|$))", "lov")
                     select new IdTags { l = t, x = (string)xid.Parent.Attributes("id").FirstOrDefault() }
                )
                .ToList()
                .ForEach(p => p.l.Parent.AddAttributte("topic_id", p.x));



                segments.Add(new XElement("segment", new XAttribute("segment_id", "diblink"), diblinks));


                List<string> tag1 = diblinks.Elements("idlinks").Attributes("text").Select(p => p.Value).ToList();

                tag1.GroupBy(p => p.ToLower()).Select(p => p.Key).OrderBy(p => p).ToList().ForEach(p => Debug.Print(p));

                segments.Descendants().Where(p => p.Name.LocalName == "x-index").ToList().ForEach(p => Debug.Print(p.Value));
                segments.Save(path + @"segments1.xml");
            }
            else
            {
                XElement document = new XElement(e.DescendantsAndSelf().Where(p=>p.Name.LocalName=="document").FirstOrDefault());
                tpo.ExecuteTextProcuction(document, language);

                diblinks.Add(tpo.Links.Elements("idlinks"));

                (
                    from t in diblinks.Descendants("idlinks").Attributes("tag1").Where(p => p.Value != "" && Regex.IsMatch(p.Value, @"^x\d+$"))
                    join xid in iddoc.Descendants("t").Attributes("xid")
                    on t.Value equals "x" + xid.Value
                    select new IdTags { l = t, x = (string)xid.Parent.Attributes("id").FirstOrDefault() }
                )
                .ToList()
                .ForEach(p => p.l.Parent.AddAttributte("topic_id", p.x));

                (
                     from t in diblinks.Descendants("idlinks").Attributes("tag1").Where(p => p.Value != "" && !Regex.IsMatch(p.Value, @"^x\d+$"))
                     join xid in iddoc.Descendants("t").Elements("w")
                     on t.Value.ToLower() equals Regex.Replace(xid.Value.ToLower(), @"(s)?(?<=[a-zæøåA-ZÆØÅ])(lov(en|a)?)(?=(\s|$))", "lov")
                     select new IdTags { l = t, x = (string)xid.Parent.Attributes("id").FirstOrDefault() }
                )
                .ToList()
                .ForEach(p => p.l.Parent.AddAttributte("topic_id", p.x));



                XElement xindexes = new XElement("x-indexs",
                    document
                    .Descendants("x-index")
                    .Select(p => new
                    {
                        id = (string)p.Attributes("id").FirstOrDefault(),
                        rid = (string)p.Attributes("rid").FirstOrDefault(),
                        text = p.DescendantNodes().OfType<XText>().Select(s=>s.Value).StringConcatenate(),
                        matches = (string)p.Attributes("matches").FirstOrDefault(),
                    })
                    .GroupBy(p=>p.rid)
                    .Select(p => new {
                        id = p.Select(i => i.id).FirstOrDefault(),
                        matches = p.Select(i => i.matches).FirstOrDefault(),
                        text = p.Select(i => i.text).StringConcatenate(),
                        n = p.Count()
                        }
                    )
                    .GroupBy(p=>p.text)
                    .Select(p=>new XElement("x-item",
                                new XAttribute("text", p.Key),
                                p.Select(i => new XElement("x-match", 
                                    new XAttribute("matches", i.matches),
                                    new XAttribute("id", i.id)
                                    )
                                )
                        )
                    )
                );


                XElement result = new XElement("package",
                    document,
                    diblinks,
                    xindexes
                    );
                result.Save(path + @"package1.xml");
            }
           

            DateTime stop = DateTime.Now;
            TimeSpan time = stop - start;

            //tpo.Links.Save(@"D:\_books\2019\Skatte-ABC\3\links.xml");
            Debug.Print(time.ToString(@"hh\:mm\:ss"));

           
           




            
        }
    }
}
