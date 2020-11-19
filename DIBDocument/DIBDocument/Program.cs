using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DIBDocument
{
    class Program
    {

        static void Main00(string[] args)
        {
            XElement diblink = XElement.Load(@"D:\_diblink\diblink_test.xml");

            
            XElement result = diblink.xlinks();

        }
        static void MainZZ(string[] args)
        {
            //string path = @"D:\_tema\";
            //string fileName = "temadoc.xml";
            //string fileNameOut = "temadoc_convert.xml";

            string path = @"D:\_tema\";
            string fileName = "7.xml";
            //string fileName = "tema.xml";
            string fileNameOut = "convert.xml";


            XElement document = XElement.Load(path + fileName);//.Descendants("document").Where(p=>(string)p.Ancestors("topic").Attributes("id").FirstOrDefault() == "{4758A60C-7487-4720-871A-EFFFB1766CFA}").FirstOrDefault();
            if (document != null)
            {
                XElement d1 = document.ConvertDG();

                XElement xo = d1.Descendants("xobjects").FirstOrDefault();
                if (xo!=null)
                {
                    xo.Save(path + "xobjects.xml");
                }
            }
            return;

            List<XElement> h5 = document.Descendants("h5").ToList();

            //XElement documents = new XElement("documents");
            //documents.Add(document.Descendants("document").Select(p => new XElement("document", p.Attributes().Where(a => a.Name.LocalName != "topic_id"), new XAttribute("topic_id", (string)p.Parent.Parent.Attributes("id").FirstOrDefault()), p.Nodes())));

            //fileName = "doc.xml";
            //document = XElement.Load(path + fileName);
            //documents.Add(document.Descendants("document").Select(p => new XElement("document", p.Attributes().Where(a => a.Name.LocalName != "topic_id"), new XAttribute("topic_id", (string)p.Parent.Parent.Attributes("id").FirstOrDefault()), p.Nodes())));

            //fileName = "tema.xml";
            //document = XElement.Load(path + fileName);
            //documents.Add(document.Descendants("document").Select(p => new XElement("document", p.Attributes().Where(a => a.Name.LocalName != "topic_id"), new XAttribute("topic_id", (string)p.Parent.Parent.Attributes("id").FirstOrDefault()), p.Nodes())));

            

            //string path = @"D:\_dibdoc\";
            //string fileName = "forarbeider.xml";
            XElement documents = XElement.Load(path + fileName);

            //List<string> dsize = documents.Elements("document").Select(p => new { id = (string)p.Attributes("topic_id").FirstOrDefault(), length = p.DescendantNodes().OfType<XText>().Select(t => t.Value).StringConcatenate().Length }).OrderByDescending(p=>p.length).Select(p=>p.id + "=" + p.length.ToString()).ToList();
            //Debug.Print("-------" + "dok størrelse" + "--------");
            //dsize.ForEach(p => Debug.Print(p));




            //documents.AddStyles();

            //List<string> k_type = documents.Descendants().Attributes("type").Select(p => p.Parent.Name.LocalName + "=" + p.Value).GroupBy(p => p).Select(p => p.Key).OrderBy(p=>p).ToList();
            //Debug.Print("-------" + "type attributt" + "--------");
            //k_type.ForEach(p => Debug.Print(p));

            //List<string> styles = null;
            //string elementName = "p";
            //styles = documents.ElementStyles(elementName);
            //Debug.Print("-------" + elementName + "--------");
            //styles.ForEach(p => Debug.Print(p));

            //styles = documents.ElementStylesValue(elementName, "text-align;font-weight");
            //Debug.Print("-------" + elementName + "--------");
            //styles.ForEach(p => Debug.Print(p));

            //elementName = "span";
            //styles = documents.ElementStyles(elementName);
            //Debug.Print("-------" + elementName + "--------");
            //styles.ForEach(p => Debug.Print(p));

            //styles = documents.ElementStylesValue(elementName, "text-align;font-weight;text-decoration");
            //Debug.Print("-------" + elementName + "--------");
            //styles.ForEach(p => Debug.Print(p));

            //elementName = "table";
            //styles = documents.ElementStyles(elementName);
            //Debug.Print("-------" + elementName + "--------");
            //styles.ForEach(p => Debug.Print(p));


            //styles = documents.ElementStylesValue(elementName, "border-collapse;border-style");
            //Debug.Print("-------" + elementName + "--------");
            //styles.ForEach(p => Debug.Print(p));

            //elementName = "th";
            //styles = documents.ElementStyles(elementName);
            //Debug.Print("-------" + elementName + "--------");
            //styles.ForEach(p => Debug.Print(p));

            //styles = documents.ElementStylesValue(elementName, "text-align");
            //Debug.Print("-------" + elementName + "--------");
            //styles.ForEach(p => Debug.Print(p));

            //elementName = "td";
            //styles = documents.ElementStyles(elementName);
            //Debug.Print("-------" + elementName + "--------");
            //styles.ForEach(p => Debug.Print(p));

            //styles = documents.ElementStylesValue(elementName, "text-align;vertical-align");
            //Debug.Print("-------" + elementName + "--------");
            //styles.ForEach(p => Debug.Print(p));

            //XElement xsd = documents.GetXSD();
            //xsd.Save(path + "forarbeider_xsd.xml");

            

            
            ////List<string> pNameElemntes = document.Descendants("p").Descendants().Select(p => p.Name.LocalName).GroupBy(p => p).Select(p => p.Key + " " + p.Count()).ToList();
            ////List<string> elementName = document.Descendants("img").Select(p => p.Parent.Name.LocalName).GroupBy(p => p).Select(p => p.Key + " " + p.Count()).ToList();

            //List<XElement> ParaAsListe = documents.Descendants("p").Where(p => (p.Nodes().OfType<XComment>().Select(s => s.Value).FirstOrDefault() ?? "").Trim() == "[if !supportLists]").ToList();
            //List<XElement> ParaAsNormal = documents.Descendants("p").Where(p => ((string)p.Attributes("class").FirstOrDefault()??"").Trim().ToLower() == "msonormal").ToList();

            //List<string> comments = documents.DescendantNodes().OfType<XComment>().Select(s => s.Value).ToList();

            //XElement result = new XElement("documents");

            //foreach (XElement d in documents.Elements("document"))
            //{
            //    int n = d.Descendants("img").Count();
            //    XElement convert = d.ConvertDG();
            //    //convert.Add(new XAttribute("topic_id", (string)d.Parent.Parent.Attributes("id").FirstOrDefault()));
            //    int nn = convert.Descendants("img").Count();
            //    if (n > 0 && nn != n)
            //    {

            //    }
            //    result.Add(convert);
            //}
            //result.Save(path + fileNameOut);
        }
        static void Main0(string[] args)
        {
            string path = @"D:\_test\dibDocument\";
            XElement map = XElement.Load(path + "index.xml");
            XElement search = XElement.Load(path + "search.xml"); ;

            XElement result = map.IndexSearch_Ex(search);

            
        }

        private static XElement my_f_document_external_parts_query(XElement document)
        {
            Regex rxParts = new Regex(@"(?<extpart>(\/\#(?<value>([a-zæaøåA-ZÆØÅ0-9]+))\#\/))");
            List<XElement> dParts = document.DescendantNodes().OfType<XText>().Where(p => rxParts.IsMatch(p.Value)).Select(p => p.Parent).ToList();
            List<string> expressions = new List<string>();
            if (dParts.Count() > 0)
            {
                foreach (XElement dp in dParts)
                {
                    string test = dp.DescendantNodes().OfType<XText>().Select(s => s.Value).StringConcatenate();
                    string partname = rxParts.Match(test).Groups["value"].Value.Trim().ToLower();
                    if (partname != "")
                    {
                        dp.SetAttributeValueEx("externalpartid", partname);
                        expressions.Add(partname);
                    }
                }
            }
            if (expressions.Count() > 0)
            {
                XElement externalparts = new XElement("externalparts",
                    expressions.Select(p => new XElement("externalpart", new XAttribute("name", p.Trim())))
                );
                XElement d = document.DescendantsAndSelf("document").FirstOrDefault();
                if (d != null)
                {
                    d.Add(externalparts);
                }
                return document;
            }
            return document;
        }
        static void Mainvar(string[] args)
        {
            //string test = @"  En variabel ";
            //string test = @"En annen variabel || Veiledningstekst";
            //string test = @" Variabel nr tre :: Standardverdi ";
            //string test = @"Fire || Veiledning::Verdi";
            //string test = @"Alt1 // han // hun";
            //string test = @"Alt2 // Mann == han // Kvinne == hun";
            string test  = @" Alt4 :: hun // Mann == han // Kvinne == hun ";
            //string test = @" Alt3::hun // han // hun";

            string svar_splitter = @"(\|\||\:\:|\/\/|\=\=|$)";
            string svar_name = @"^((?<name>(.*?))(?=" + svar_splitter + "))";
            string svar_comment = @"((\|\|)(?<comment>(.*?))(?=" + svar_splitter + "))";
            string svar_standardverdi = @"((\:\:)(?<standard>(.*?))(?=" + svar_splitter + "))";
            string svar_optionname = @"((\/\/)(?<optionname>(.*?))(?=" + svar_splitter + "))";
            string svar_optionvalue = @"((\=\=)(?<optionvalue>(.*?))(?=" + svar_splitter + "))";
            
            string svar_option = @"(?<option>(" + svar_optionname +  @"(" + svar_optionvalue + ")?))";

            string svar_objects = svar_name
                + "(("
                + @"(\s+)?"
                + svar_standardverdi
                + "|" + svar_comment
                + "|" + svar_option
                + ")+)?";
            Regex rxsvar_objects = new Regex(svar_objects);

            

            List<string> lvar_groupname = new List<string>();
            lvar_groupname.Add("name");
            lvar_groupname.Add("standard");
            lvar_groupname.Add("comment");
            lvar_groupname.Add("option");

            List<string> lvar_subgroupname = new List<string>();
            lvar_subgroupname.Add("optionname");
            lvar_subgroupname.Add("optionvalue");

            string vname = "";
            string vcomment = "";
            string vstandard = "";
            XElement options = null;

            MatchCollection mc = rxsvar_objects.Matches(test);

            XElement variable = new XElement("variable");

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
                                    vname = c.Value.Trim();
                                    break;
                                case "standard":
                                    vstandard = c.Value.Trim();
                                    break;
                                case "comment":
                                    vcomment = c.Value.Trim();
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
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
            
            Debug.Print("name = " + vname);
            Debug.Print("comment = " + vcomment);
            Debug.Print("standard = " + vstandard);
            Debug.Print(options == null ? "" : options.ToString());
        }
        //Byggekloss
        static void Main(string[] args)
        {
            string path = @"D:\_DIBDocument\Byggeklosser\";

            XElement document = XElement.Load(path + "document.xml");

            document = my_f_document_external_parts_query(document);
            XElement externalparts = XElement.Load(path + "externalparts.xml");

            
            XElement result = document.ConvertDG(externalparts);

            result.Save(path +  @"result.xml");

            

        }

        static void Main1(string[] args)
        {
            string top = "_top";

            XElement map = XElement.Load(@"D:\_dibdocument\sitem.xml");
            string segment_id = "paragrafd1e31648119605573";
            string id = "ea91d41f-0e29-4754-bcd2-e7f7e08c72e2";

            XElement indextop = map.Descendants().Where(p => ((string)p.Attributes().AttributeOfName("key;id") ?? "").Trim().ToLower() == top.Trim().ToLower()).FirstOrDefault();
            if (indextop == null)
            {
                indextop = map.Descendants().Where(p => "true;1".Split(';').Contains((string)p.Attributes().AttributeOfName("segment;s"))).FirstOrDefault();
                if (indextop == null)
                {
                    //return null;
                }
                indextop = indextop.Parent;
            }
            indextop = new XElement(indextop);

            XElement part = null;
            XElement index = null;
            if ((id == null ? "" : id) != "")
            {
                segment_id = "";

                part = indextop
                    .Descendants()
                    .Where(p => ((string)p.Attributes().AttributeOfName("id;key") ?? "").Trim().ToLower() == id.Trim().ToLower())
                    .AncestorsAndSelf()
                    .Where(p => ((string)p.Attributes().AttributeOfName("title;text") ?? "") != "")
                    .FirstOrDefault();

                if (part == null) return;

                XElement sg = part.AncestorsAndSelf().Where(p => "true;1".Split(';').Contains((string)p.Attributes().AttributeOfName("segment;s"))).FirstOrDefault();

                string sid = (string)sg.Attributes().AttributeOfName("id;key");
                sg.AddAnnotation(new CurrentSegment { segment_id = sid });
                segment_id = (string)sg.Attributes().AttributeOfName("key;id");
                if (part != null && segment_id != id)
                {
                    XElement parents = new XElement("parents",
                            part
                            .Ancestors()
                            .Where(p => ((string)p.Attributes().AttributeOfName("title;text") ?? "_top") != "_top")
                            .Reverse()
                            .Select(p => new XElement("parent",
                                new XAttribute("id", (string)p.Attributes().AttributeOfName("id;key")),
                                new XAttribute("name", (string)p.Attributes().AttributeOfName("title;text"))
                            )
                        )
                    );
                    index = new XElement("index",
                        part.GetIndexItems(segment_id)
                    );

                    //return new SqlXml(new XElement("navdata",
                    //    new XAttribute("segment_id", segment_id),
                    //    new XAttribute("select_id", (string)part.Attributes().AttributeOfName("key;id")),
                    //    index).CreateReader()
                    //);
                }
            }

            if ((segment_id == null ? "" : segment_id) != "")
            {
                XElement sg = indextop
                    .Descendants()
                    .Where(p => ((string)p.Attributes().AttributeOfName("id;key") ?? "").Trim().ToLower() == segment_id.Trim().ToLower())
                    .AncestorsAndSelf()
                    .Where(p => "true;1".Split(';').Contains((string)p.Attributes().AttributeOfName("segment;s")))
                    .FirstOrDefault();

                
                string sid = (string)sg.Attributes().AttributeOfName("id;key");
                sg.AddAnnotation(new CurrentSegment { segment_id = sid });



                index = new XElement("index",
                    indextop.Elements().SelectMany(p => p.GetIndexItems(segment_id))
                );
                //return new SqlXml(new XElement("navdata",
                //    new XAttribute("segment_id", segment_id),
                //    index).CreateReader()
                //);
            }
            else
            {
                XElement sg = indextop
                    .Descendants()
                    .Where(p => ((string)p.Attributes().AttributeOfName("segment_id") ?? "").Trim().ToLower() == top.Trim().ToLower())
                    .FirstOrDefault();
                if (sg == null)
                {
                    sg = indextop
                    .Descendants()
                    .Where(p => "true;1".Split(';').Contains(((string)p.Attributes().AttributeOfName("segment;s") ?? "")))
                    .FirstOrDefault();
                    //if (sg == null) return null;
                    string sid = (string)sg.Attributes().AttributeOfName("id;key");
                    sg.AddAnnotation(new CurrentSegment { segment_id = sid });
                    index = new XElement("index",
                        indextop.Elements().SelectMany(p => p.GetIndexItems(top))
                    );
                    //return new SqlXml(new XElement("navdata",
                    //    new XAttribute("segment_id", sid),
                    //    index).CreateReader()
                    //);
                }
                else
                {
                    //if (sg == null) return null;
                    string sid = (string)sg.Attributes().AttributeOfName("id;key");
                    sg.AddAnnotation(new CurrentSegment { segment_id = sid });
                    index = new XElement("index",
                        indextop.Elements().SelectMany(p => p.GetIndexItems(top))
                    );
                    //return new SqlXml(new XElement("navdata",
                    //    new XAttribute("segment_id", top),
                    //    index).CreateReader()
                    //);
                }
            }
        }
    }
}
