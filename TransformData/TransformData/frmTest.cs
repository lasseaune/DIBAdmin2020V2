using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.IO;
using DIB.RegExp.ExternalStaticLinks;
using TransformData.Global;
using System.Diagnostics;
using Dib.Text.Identifier;
using System.Text.RegularExpressions;
using DIB.InTextLinking;
using Dib.Transform;
namespace TransformData
{
    public partial class frmTest : Form
    {
        XElement exTag = new XElement("external");
        XElement intTag = new XElement("internal");
        XElement unIdTag = new XElement("unid");
        
        private class XmlBookmarkResult
        {
            public string elemId { get; set; }
            public string elemSourceID { get; set; }
            public string elemTitle { get; set; }
            public string elemSourceName { get; set; }
            public string elemContextPath { get; set; }
            public string elemName { get; set; }
            public string elemType { get; set; }
            public string text { get; set; }
            public XmlBookmarkResult()
            {
                elemId = string.Empty;
                elemSourceID = string.Empty;
                elemTitle = string.Empty;
                elemSourceName = string.Empty;
                elemContextPath = string.Empty;
                elemName = string.Empty;
                elemType = string.Empty;
                text = string.Empty;

            }
            public XmlBookmarkResult(string Id, string SID, string Title, string SourceName, string ContextPath, string Name, string Type, string Text)
            {
                elemId = Id;
                elemSourceID = SID;
                elemTitle = Title;
                elemSourceName = SourceName;
                elemContextPath = ContextPath;
                elemName = Name;
                elemType = Type;
                text = Text;
            }
        }

        
        public frmTest()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string fileName = @"D:\_data\_dibadm\start.xml";
            XmlTextReader xmlreader = new XmlTextReader(fileName);
            XsltArgumentList xslArg = new XsltArgumentList();

            //string path = Path.GetDirectoryName(Application.ExecutablePath);
            
            string xsltPath = @"C:\Users\lau\Documents\Visual Studio 2010\Projects\TransformData\TransformData\xsl\xml2json.xslt";

            

            if (!File.Exists(xsltPath))
            {
                MessageBox.Show("Finner ikke filen '" + xsltPath + "'");
                return;
            }


            string outString = transformDocument.TransformXmlWithXslToString(xmlreader
                , xsltPath, xslArg);
            
            xmlreader.Close();

            File.WriteAllText(@"D:\_data\_dibadm\start.txt", outString);
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string s = 
                "Foretaksregisteret sendte 12 oktober 1998 brev til den kjærende part med pålegg om å melde ny revisor da tidligere revisor hadde meldt egenfratreden.  Pålegget ga selskapet frist på 2 måneder for å melde ny revisor i motsatt fall ville saken bli oversendt skifteretten med anmodning om tiltak etter aksjeloven § 13-2 første ledd nr 4 (aksjeloven av 1976)."
                +"18 mars 1999 sendte den kjærende part brev til Foretaksregisteret med villighets-erklæring fra ny revisor. 14 april 1999 meddelte Foretaksregisteret at meldingen av 18 mars ikke kunne godtas da den ikke var sendt på godkjent blankett, ikke hadde bekreftet utskrift av protokoll for generalforsamling og også ellers hadde formelle mangler. 20 mai 1999 avholdt den kjærende part ekstraordinær generalforsamling hvor den nye revisor formelt ble valgt.  Melding om dette ble imidlertid ikke før 29 september 1999 sendt til Foretaksregisteret, som 27 august hadde sendt melding til Indre Follo skifterett med henvisning til § 16-17 i aksjeloven av 1997."
                +"Skifteretten avsa 22 september 1999 kjennelse med slik slutning:"
                + "    Bygg og Tømmermester Tangen AS, adr.  Østrengvn. 8, 1405"
                + " Langhus, oppløses i medhold av aksjeloven § 16-15 første ledd nr 4.";

            string r = string.Empty; //@"\(|PARAQL;\)|PARAQR";
            r =
              @"(?<g1>((([1-9]|0[1-9]|1[0-9]|2[0-9]|3[01])\W+(((J|j)anuary?)|((F|f)ebruary?)|((M|m)ar(s|ch))|((A|a)pril)|((M|m)a(i|y))|((J|j)un(i|e))|((J|j)ul(i|y))|((A|a)ugust)|((S|s)eptember)|((O|o)(k|c)tober)|((N|n)ovember)|((D|d)e(s|c)ember))\W+[0-9]{2,4})))|||QDATE$1QDATE;;;"
            + @"((([\d\w]\s*?)))(?<g1>(\-))((\s*?[\d\w]))|||HYPHENQ;;;"
            + @"((\s+)?)(?<g1>(-))((\s+)?)|||HYPHENQ;;;"
            + @"(?<=[\w])(?<g1>(\.))(?=[\s])|||;;;"
            + @"(([\d\w])(?<g1>(\.))([\d\w]))|||PUNCTQ;;;"
            + @"(?<g1>([\)]))||| RPARQ ;;;"
            + @"(?<g1>([\(]))||| LPARQ ;;;"
            + @"(?<![\/])(?<g1>([\/]))(?![\/])||| SLASHQ ;;;"
            + @"(?<g1>(§))|||PARAQ;;;";

            string result = s.ReplaceTextWith(r);
        }

        private void ExecuteInTextLinking(
            XElement Document,
            XElement Actions,
            string Regexp,
            string Include,
            string Exclude,
            string Breaknodes,
            string NonMarkupNodes
                    )
        {
            
            //IdElements idEls = new IdElements(Document, Actions, Regexp, Include, Exclude, Breaknodes, NonMarkupNodes );
            ////idEls.SaveTextToFile(@"C:\_Work\2009\ABC\2013_1\Lignings-ABC 2013\idelements.xml");
            //XElement newD = idEls.Execute();
            ////newD.Save(@"C:\_Work\2009\ABC\2013_1\Lignings-ABC 2013\newABC.xml");
            //if (idEls.externalTags == null ? false : idEls.externalTags.HasElements) exTag = new XElement(idEls.externalTags);
            //if (idEls.internalTags == null ? false : idEls.internalTags.HasElements) intTag = new XElement(idEls.internalTags);
            //if (idEls.unidentifiedTags == null ? false : idEls.unidentifiedTags.HasElements) unIdTag = new XElement(idEls.unidentifiedTags);
            //MessageBox.Show("finished");
        }
        


        private void button3_Click(object sender, EventArgs e)
        {
            XElement d = XElement.Load(@"d:\_data\document.xml");


            string include = "section;document;level";
			string exclude = "section;document;level;title;kws";
            string breaknodes = "td;entry;dd;dt";
            string nonmarkupnodes = "title";

            XElement regexps = XElement.Load(@"D:\RegExp\InTextLinksRegexp_20130214.xml");
            string regex = regexps.RegexBuild("100_total");
            XElement actions = XElement.Load(@"d:\regexp\InTextLinksActions.xml");
            ExecuteInTextLinking(d, actions, regex, include, exclude, breaknodes, nonmarkupnodes);            
        }

        private IEnumerable<XElement> GetParagraf(XElement p)
        {
            List<XElement> tag2 = new List<XElement>();
            tag2.Add( new XElement("tag2", 
                            
                            new XAttribute("segment_id",p.Attributes("id").First().Value.Trim().ToLower()),
                            (p.Element("ParagrafText") == null ? null : new XAttribute("id", p.Element("ParagrafText").Attributes("id").First().Value.Trim().ToLower())),
                            new XAttribute("title", "§ " + p.Attributes("pNr").First().Value.Trim()),
                            new XElement("name", p.Attributes("pNr").First().Value.Trim().ToLower()),
                            GetLedd(p),
                            GetLitra(p),
                            GetLitraLitra(p)
                        )
            );
            tag2.AddRange(GetKommentar(p));
            return tag2; 
        }
        private IEnumerable<XElement> GetLitraLitra(XElement paragraf)
        {
            
            string leddId = "l";
            
            IEnumerable<XElement> LitraLitra = paragraf
                        .Descendants("Litra")
                        .Where(p => p.Ancestors("Litra").Count() != 0)
                        .Where(p => p.Attributes("Value").FirstOrDefault() != null)
                        .Select(p => new XElement("tag3",
                                new XAttribute("segment_id", (string)paragraf.Attributes("id").FirstOrDefault()),
                                new XAttribute("id", (string)p.Attributes("id").FirstOrDefault()),
                                new XAttribute("title", "§ " + paragraf.Attributes("pNr").First().Value.Trim()),
                                new XElement("name", p.Ancestors("Ledd").Count() != 0 
                                            ? leddId + (string)p.Ancestors("Ledd").Attributes("nr").FirstOrDefault() + "/" + (string)p.Attributes("Value").FirstOrDefault()
                                            :"")
                            )
                        );
            if (LitraLitra.Count() != 0)
            {

            }
            return null;
        }

        private IEnumerable<XElement> GetLitra(XElement paragraf)
        {
            
            string leddId = "l";
            return paragraf.Descendants("Litra")
                    .Where(p=>p.Ancestors("Litra").Count() == 0)
                    .Where(p=>p.Attributes("Value").FirstOrDefault()!=null)
                    .Select(p => new XElement("tag3",
                            
                            new XAttribute("segment_id", (string)paragraf.Attributes("id").FirstOrDefault()),
                            new XAttribute("id", (string)p.Attributes("id").FirstOrDefault()),
                            new XAttribute("title", "§ " + paragraf.Attributes("pNr").First().Value.Trim()
                                + (p.Ancestors("Ledd").Count() != 0
                                ? "(" + (string)p.Ancestors("Ledd").First().Attributes("nr").FirstOrDefault() + ") pkt. nr. "
                                : " pkt nr. ") 
                                + (string)p.Attributes("Value").FirstOrDefault()),
                            new XElement("name", 
                                p.Ancestors("Ledd").Count() != 0
                                ? leddId + (string)p.Ancestors("Ledd").First().Attributes("nr").FirstOrDefault() + "/" + (string)p.Attributes("Value").FirstOrDefault() 
                                : (string)p.Attributes("Value").FirstOrDefault())
                        )
                    );
        }

        private IEnumerable<XElement> GetKommentar(XElement paragraf)
        {
            return paragraf.Descendants("Kommentar")
                    .Select(p => new XElement("tag2",
                            new XAttribute("segment_id", (string)paragraf.Attributes("id").FirstOrDefault()),
                            new XAttribute("id", (string)p.Attributes("id").FirstOrDefault()),
                            new XAttribute("title", "Note " + p.Element("Tittel").Value + " til § " + paragraf.Attributes("pNr").First().Value.Trim()),
                            new XElement("name", paragraf.Attributes("pNr").First().Value.Trim().ToLower() + "/" + p.Element("Tittel").Value)
                        )
                    );
        }

        private IEnumerable<XElement> GetLedd(XElement paragraf)
        {
            int blokk = 3;
            string leddId = "l";
            return paragraf.Descendants("Ledd")
                    .Select(p=>new XElement("tag3",
                            new XAttribute("segment_id", (string)paragraf.Attributes("id").FirstOrDefault()),
                            new XAttribute("id", (string)p.Attributes("id").FirstOrDefault()),
                            new XAttribute("title", "§" + paragraf.Attributes("pNr").First().Value.Trim() + " (" + (string)p.Attributes("nr").FirstOrDefault() + ")"),
                            new XElement("name", leddId + ((string)p.Attributes("nr").FirstOrDefault()))
                        )
                    );
        }

        private XAttribute FindSubstituteId(string ancestorname, XElement internalparent)
        {
            XAttribute returnValue = null;
            string substitute_id = (string)internalparent
                            .Elements("item")
                            .Where(p => (string)p.Attributes("name").FirstOrDefault() == ancestorname.Trim().ToLower())
                            .Attributes("topic_id")
                            .FirstOrDefault();
            if (substitute_id != null)
            {
                return new XAttribute("substitute_id", substitute_id);
            }
            else
                return returnValue;
                    
        }
        private void button4_Click(object sender, EventArgs e)
        {
            //string path = @"D:\_books\_arbakke2\segment\"; 
            //XElement total = XElement.Load(path + @"segments.xml");

            string path = @"D:\_books\2017\KommentarerASL\"; 
            XElement total = XElement.Load(path + @"utgivelseX.xml");
            total.DescendantNodes().OfType<XText>().ToList().ForEach(p=>p.ReplaceWith(new XText(Regex.Replace(p.Value, @"\s+", " "))));

            //List<XElement> sg = total.Descendants("segment").Where(p => (string)p.Attributes("id").FirstOrDefault()==null).ToList();
            //return;

            Identificator id = new Identificator();
            XElement tags = new XElement("tags",
                new XElement("tag1",
                    new XAttribute("name", "aksjelov")),
                            new XElement("tag1",
                new XAttribute("name", "allmennaksjelov"))
            );
            XElement result = id.IdentifyTags(tags, "no");



            XElement intParent = new XElement("internalnodes",
                
                new XElement("item",
                    new XAttribute("id", "02368ba8-8c75-4c11-afa3-3af95bd310a1"),
                    new XAttribute("segment_id", "02368ba8-8c75-4c11-afa3-3af95bd310a1"),
                    new XAttribute("title", "Kommentarutgave - Aksjeloven"),
                    new XAttribute("regexpName", "source_para;note_ref_para;note_ref"),
                    new XAttribute("area", "segment"),
                    new XAttribute("name", "aksjelov"),
                    result.Descendants("tag1").Where(p=>p.Elements("name").Where(q=>q.Value.ToLower() =="aksjelov" ).Count()!=0).Elements("name")//new XElement("name", "Aksjelov")
                    ),
                  new XElement("item",
                    new XAttribute("id", "7f30b93a-825a-4476-9fa7-703f58520cbc"),
                    new XAttribute("segment_id", "7f30b93a-825a-4476-9fa7-703f58520cbc"),
                    new XAttribute("title", "Kommentarutgave - Allmennaksjeloven"),
                    new XAttribute("regexpName", "source_para;note_ref_para;note_ref;"),
                    new XAttribute("area", "segment"),
                    new XAttribute("name", "allmennaksjelov"),
                    result.Descendants("tag1").Where(p => p.Elements("name").Where(q => q.Value.ToLower() == "allmennaksjelov").Count() != 0).Elements("name")//new XElement("name", "allmennaksjelov")
                    ),
                  new XElement("item",
                    new XAttribute("id", "b823b838-aada-4e2e-a7ae-f9a8bffe101a"),
                    new XAttribute("segment_id", "b823b838-aada-4e2e-a7ae-f9a8bffe101a"),
                    new XAttribute("title", "Kommentarutgave - Innledning"),
                    new XAttribute("regexpName", "idavsnitt;"),
                    new XAttribute("area", "global")
                      
                      )

               );

            total.Descendants("Hoveddel").Where(p => (string)p.Attributes("id").FirstOrDefault() == "02368ba8-8c75-4c11-afa3-3af95bd310a1").ToList().ForEach(p => p.Add(new XAttribute("tag1", "aksjelov")));
            total.Descendants("Hoveddel").Where(p => (string)p.Attributes("id").FirstOrDefault() == "7f30b93a-825a-4476-9fa7-703f58520cbc").ToList().ForEach(p => p.Add(new XAttribute("tag1", "allmennaksjelov")));

            total.Descendants().Where(p => "Hoveddel;Underdel:Kapittel".Split(';').Contains(p.Name.LocalName)).ToList().ForEach(p => p.Add(new XAttribute("segment", "true")));

            //(
            //        from el in total.Descendants()
            //        join i in intParent.Descendants("item").Where(p => p.Attribute("name") != null)
            //        on (string)el.Attributes("id").FirstOrDefault() equals (string)i.Attributes("id").FirstOrDefault()
            //        select new
            //        {
            //            el = el,
            //            item = i
            //        }
            //)
            //.ToList().ForEach(p => p.el.Add(new XAttribute("tag1", (string)p.item.Attribute("name").Value)));



            
            XElement paralist = new XElement("tags");

            paralist.Add(
                total
                .DescendantsAndSelf("Seksjon")
                .Where(r =>
                        ((string)r.Ancestors("Hoveddel").Attributes("id").FirstOrDefault()).ToLower() == ((string)intParent
                                                                                                            .Elements("item")
                                                                                                            .Where(p => p.Attribute("regexpName").Value == "idavsnitt;")
                                                                                                            .Attributes("segment_id")
                                                                                                            .FirstOrDefault()).ToLower()
                        && r.Element("Tittel") != null
                )
                .Where(s => s.Attribute("segment") == null
                    &&
                    Regex.IsMatch(s.Elements("Tittel").First().Value.Trim(), @"^(?<nr>(\d+((\.\d+)+)?))$"))
                .Select(s =>
                    new XElement("tag1",
                        new XAttribute("topic_id", "_self"),
                        new XAttribute("segment_id", (string)s.Ancestors().Where(p=>(string)p.Attributes("segment").FirstOrDefault() == "true").Attributes("id").FirstOrDefault()),
                        new XAttribute("id", (string)s.Attributes("id").FirstOrDefault()),
                        s.Attribute("regexpName"),
                        new XAttribute("title", Regex.Match(s.Elements("Tittel").First().Value.Trim(), @"(?<nr>(\d+((\.\d+)+)?))$").Groups["nr"].Value),
                        new XElement("name", "avsnitt" + Regex.Match(s.Elements("Tittel").First().Value.Trim(), @"(?<nr>(\d+((\.\d+)+)?))$").Groups["nr"].Value)
                        )
                    )
                );


            paralist.Add(
                intParent
                .Elements("item")
                .Select(q=> new XElement("tag1",
                    new XAttribute("topic_id", "_self"),
                    q.Attributes(),
                    q.Elements(),
                    total.Descendants("Kapittel")
                    .Where(p => (string)p.Attributes("id").Select(s => s.Value.ToLower()).FirstOrDefault() == (string)p.Ancestors("segment").Attributes("id").Select(s => s.Value.ToLower()).FirstOrDefault()
                        && (string)p.Ancestors("Hoveddel").Attributes("id").Select(s=>s.Value.ToLower()).FirstOrDefault()
                        == (string)q.Attributes("segment_id").Select(s=>s.Value.ToLower()).FirstOrDefault()
                    )
                    .Select(p=> new XElement("tag2", 
                            new XAttribute("segment_id",p.Attributes("id").First().Value.Trim().ToLower()),
                            new XAttribute("title", p.Element("Tittel").Value.Trim()),
                            new XElement("name","kap" + Regex.Match(p.Element("Tittel").Value.Trim().ToLower(),@"(kap\.\s+)(\d+)").Groups[2].Value)
                            )
                    ),
                    total.Descendants("Underkapittel")
                    .Where(p => (string)p.Attributes("id").Select(s => s.Value.ToLower()).FirstOrDefault() == (string)p.Ancestors("segment").Attributes("id").Select(s => s.Value.ToLower()).FirstOrDefault()
                        && (string)p.Ancestors("Hoveddel").Attributes("id").Select(s => s.Value.ToLower()).FirstOrDefault()
                        == (string)q.Attributes("segment_id").Select(s => s.Value.ToLower()).FirstOrDefault()
                    )
                    .Select(p => new XElement("tag2",
                            new XAttribute("segment_id", p.Attributes("id").First().Value.Trim().ToLower()),
                            new XAttribute("title", p.Element("Tittel").Value.Trim()),
                            new XElement("name", "kap" + Regex.Match(p.Parent.Element("Tittel").Value.Trim().ToLower(), @"(kap\.\s+)(\d+)").Groups[2].Value + "?" + Regex.Match(p.Element("Tittel").Value.Trim().ToLower(), @"^(?<value>([ivx]+))\.").Groups["value"].Value)
                            )
                    ),
                    total.Descendants("Paragraf")
                    .Where(p => (string)p.Ancestors("Hoveddel").Attributes("id").Select(s => s.Value.ToLower()).FirstOrDefault()
                        == (string)q.Attributes("id").Select(s => s.Value.ToLower()).FirstOrDefault()
                    )
                    .SelectMany(p=> GetParagraf(p))
                                
                        
                    
                    
                    )
                )
            );



            paralist.Save(path + @"paralist.xml");


            paralist = new XElement("internaltags", paralist);

            
            id.Include = "Hoveddel";
            id.Exclude = "Tittel;Mtit";
            id.BreakNodes = "Utgivelse;Hoveddel;Forkortelser;Forkortelse;Kapittel;Seksjon;Underkapittel;Paragraf;Avsnitt;Litra;Term;Definisjon;Tittel;Ledd;Mtit;Uth";
            id.NoMarkupNodes = "Tittel;Mtit";
            id.Local = true;
            //id.externalTags = ext;
            id.InternalReference = true;
            id.InternalTags = paralist;

            XElement regexps = XElement.Load(@"D:\RegExp\nor\InTextLinksRegexp_nor.xml");
            string regex = regexps.RegexBuild("100_total_no");
            XElement actions = XElement.Load(@"d:\regexp\nor\InTextLinksActionsNor.xml");

            DateTime start = DateTime.Now;
            XElement newTotal = id.ExecuteLinking(total, actions, regex, "no");
            if (id.Links.HasElements)
            {
                Debug.Print(id.Links.Descendants("link").Count().ToString());
                newTotal.Add(new XElement("segment",
                        new XAttribute("id", id.Links.Name.LocalName),
                            id.Links
                    )
                );
            }
            newTotal.Add(new XElement("segment",
                        new XAttribute("id", "_internaltags"),
                            paralist
                    )
                );
            newTotal.Save(path + @"newSegment.xml");
            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            Debug.Print("Tid=" + duration.ToString());



            







            //XElement segments = XElement.Load(@"D:\_books\_arbakke2\segment\segments.xml");
            //exTag = new XElement("external");
            //intTag = new XElement("internal");
            //unIdTag = new XElement("unid");


            //string include = "Bok";
            //string exclude = "Bok";
            //string breaknodes = "Bok;Hoveddel;Forkortelser;Forkortelse;Kapittel;Seksjon;Underkapittel;Paragraf;Avsnitt;Litra;Term;Definisjon;Tittel;Ledd;Mtit";
            //string nonmarkupnodes = "Tittel;Mtit";

            //XElement regexps = XElement.Load(@"D:\RegExp\InTextLinksRegexp_20130214.xml");
            //string regex = regexps.RegexBuild("100_total");
            //XElement actions = XElement.Load(@"d:\regexp\InTextLinksActions.xml");




            //IdElements idEls = new IdElements(segments, actions, regex, include, exclude, breaknodes, nonmarkupnodes, true);

            //idEls.InternalItems = intParent.Elements("item")
            //                            .Select(p => new InternalItem(
            //                                (string)p.Attributes("name").FirstOrDefault(),
            //                                (string)p.Attributes("id").FirstOrDefault(),
            //                                (string)p.Attributes("topic_id").FirstOrDefault(),
            //                                (string)p.Attributes("type").FirstOrDefault(),
            //                                (string)p.Attributes("title").FirstOrDefault(),
            //                                (string)p.Attributes("substitute_id").FirstOrDefault(),
            //                                (string)p.Attributes("area").FirstOrDefault()
            //                                ));
            //idEls.documentTags = paralist;
            //idEls.InternalReference = true;
            ////idEls.SaveTextToFile(@"C:\_Work\2009\ABC\2013_1\Lignings-ABC 2013\idelements.xml");
            //XElement newD = idEls.Execute();
            //newD.Save(@"D:\_books\_arbakke2\segment\newsegments.xml");
            //if (idEls.externalTags == null ? false : idEls.externalTags.HasElements) exTag = new XElement(idEls.externalTags);
            //if (idEls.internalTags == null ? false : idEls.internalTags.HasElements) intTag = new XElement(idEls.internalTags);
            //if (idEls.unidentifiedTags == null ? false : idEls.unidentifiedTags.HasElements) unIdTag = new XElement(idEls.unidentifiedTags);
            //MessageBox.Show("finished");
            //exTag.Save(@"D:\_books\_arbakke2\external.xml");
            //intTag.Save(@"D:\_books\_arbakke2\internal.xml");
            //unIdTag.Save(@"D:\_books\_arbakke2\uid.xml");

        }

        private void button5_Click(object sender, EventArgs e)
        {

            
            XElement total = new XElement("root",
                new XElement("content",
                        new XElement("p", "SA 3802-1 pkt. 112-117")
                )
            );
            Identificator id = new Identificator();
            id.Include = "content";
            id.Exclude = "";
            id.BreakNodes = "p";
            id.NoMarkupNodes = "";
            id.Local = true;
            id.InternalReference = false;
            id.InternalTags = null;
            id.ReferencedTopicsTags = null;


            XElement regexps = XElement.Load(@"D:\RegExp\NOR\InTextLinksRegexp_NOR.xml");
            string regex = regexps.RegexBuild("100_total_no");
            XElement actions = XElement.Load(@"d:\regexp\NOR\InTextLinksActionsNOR.xml");

            DateTime start = DateTime.Now;
            id.Local = true;
            XElement newTotal = id.ExecuteLinking(total, actions, regex, "no");
            if (id.Links.HasElements)
            {

            }
            
        }

        private void btnLABC_Click(object sender, EventArgs e)
        {

            //string folder = @"C:\_Work\2009\ABC\2013_2014\intext\";
            //string file = @"Lignings-ABC 2013-14.xml";
            //string newFile = @"ABC2014_new.xml";
            //XElement total = XElement.Load(folder + file);

            //total.Descendants("NBHY").ToList().ForEach(p => p.ReplaceWith(new XText("-")));
            //total.Descendants("NBSP").ToList().ForEach(p => p.ReplaceWith(new XText(" ")));

            //XmlReader r = total.CreateReader();
            //r.MoveToContent();
            //total = XElement.Load(r.ReadSubtree());

            ////total = new XElement("ABC",
            ////    new XText("etter asl/asal § 8-1 første ledd ")
            ////    );

            //exTag = new XElement("external");
            //intTag = new XElement("internal");
            //unIdTag = new XElement("unidentyfied");


            //string include = "ABC";
            //string exclude = "ABC";
            //string breaknodes = "A;A1;TIT;MTIT;STIKKT;HENV;PKT;EKSEMPELOVERSKRIFT;EKSEMPELTEKST;SEK;MS;MS1;MS2;MS3;MS4;MS5;ENTRY";
            //string nonmarkupnodes = "STIKKT;TIT;MS;MS1;MS2;MS3;MS4;MS5";



            //XElement regexps = XElement.Load(@"D:\RegExp\InTextLinksRegexp_20130214.xml");
            //string regex = regexps.RegexBuild("100_total");
            //XElement actions = XElement.Load(@"d:\regexp\InTextLinksActions.xml");
            //IdElements idEls = new IdElements(total, actions, regex, include, exclude, breaknodes, nonmarkupnodes);
            //XElement newD = idEls.Execute();
            //newD.Save(folder + newFile);
            //if (idEls.externalTags == null ? false : idEls.externalTags.HasElements) exTag = new XElement(idEls.externalTags);
            //if (idEls.internalTags == null ? false : idEls.internalTags.HasElements) intTag = new XElement(idEls.internalTags);
            //if (idEls.unidentifiedTags == null ? false : idEls.unidentifiedTags.HasElements) unIdTag = new XElement(idEls.unidentifiedTags);
            //MessageBox.Show("finished");

            //exTag.Save( folder + "@external.xml");
            //intTag.Save(folder + @"internal.xml");
            //unIdTag.Save(folder + @"uid.xml");

        }

        private void frmTest_Load(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            string s = "test";//"Aksjeloven";
            string regexp = @"((.*?)(lov))(en|a)$";
            string group = "lovend=;";
            string y = Regex.Replace(s, regexp, "$1");

            string x = ReplaceRegexpExtended(s, regexp, group); 

            
        }

        private string ReplaceRegexpExtended(string s, string expression, string groups)
        {
            List<KeyValuePair<string, string>> kpvs = new List<KeyValuePair<string,string>>(); 
            
            kpvs = groups
                .Split(';')
                .ToList()
                .Where(p=>p.IndexOf("=")!=-1)
                .Select(p=> new KeyValuePair<string,string>( p.Split('=')[0] ,   p.Split('=').Count() == 2 ? p.Split('=').ElementAt(1) : ""))
                .ToList();
            Regex r = new Regex(expression);             
            List<string> groupNames = r.GetGroupNames().Where(p=>p!="0").ToList();       
            s = Regex.Replace(s, expression
                        ,
                        delegate(Match m)
                        {
                            string replace = "";
                            foreach(string g in groupNames)
                            {
                                KeyValuePair<string, string> kvp = kpvs.Where(p=>p.Key == g).FirstOrDefault();
                                if (kvp.Key != null)
                                {
                                    replace = replace + kvp.Value;
                                }
                                else
                                {
                                    replace = replace + m.Groups[g].Value;
                                }
                            }
                            return replace;
                        }
                        , RegexOptions.Multiline | RegexOptions.Singleline);
            return s;
        }

        private void btnAnsSel_Click(object sender, EventArgs e)
        {
            //XElement total = XElement.Load(@"D:\_books\Ansvarlige selskaper og indre selskaper\segment\segments.xml");


            //XmlReader r = total.CreateReader();
            //r.MoveToContent();
            //total = XElement.Load(r.ReadSubtree());

            ////total = new XElement("ABC",
            ////    new XText("etter asl/asal § 8-1 første ledd ")
            ////    );

            //exTag = new XElement("external");
            //intTag = new XElement("internal");
            //unIdTag = new XElement("unidentyfied");


            //string include = "levels";
            //string exclude = "levels";
            //string breaknodes = "title;p;tr;td;footnote";
            //string nonmarkupnodes = "title";



            //XElement regexps = XElement.Load(@"D:\RegExp\InTextLinksRegexp_20130214.xml");
            //string regex = regexps.RegexBuild("100_total");
            //XElement actions = XElement.Load(@"d:\regexp\InTextLinksActions.xml");
            //IdElements idEls = new IdElements(total, actions, regex, include, exclude, breaknodes, nonmarkupnodes);
            //XElement newD = idEls.Execute();
            //newD.Save(@"D:\_books\Ansvarlige selskaper og indre selskaper\segment\newSegments.xml");
            //if (idEls.externalTags == null ? false : idEls.externalTags.HasElements) exTag = new XElement(idEls.externalTags);
            //if (idEls.internalTags == null ? false : idEls.internalTags.HasElements) intTag = new XElement(idEls.internalTags);
            //if (idEls.unidentifiedTags == null ? false : idEls.unidentifiedTags.HasElements) unIdTag = new XElement(idEls.unidentifiedTags);
            //MessageBox.Show("finished");

            //exTag.Save(@"D:\_books\Ansvarlige selskaper og indre selskaper\external.xml");
            //intTag.Save(@"D:\_books\Ansvarlige selskaper og indre selskaper\internal.xml");
            //unIdTag.Save(@"D:\_books\Ansvarlige selskaper og indre selskaper\uid.xml");

        }

        private void button7_Click(object sender, EventArgs e)
        {
            //XElement total = XElement.Load(@"C:\_Work\Kodeoversikt\segment\segments.xml");



            //XElement intParent = new XElement("internalnodes",
                
            //    new XElement("item",
            //        new XAttribute("id", "fc869ab0-201a-4309-904e-2cb062a3cc53"),
            //        new XAttribute("segment_id", "fc869ab0-201a-4309-904e-2cb062a3cc53"),
            //        new XAttribute("topic_id", "_self"),
            //        new XAttribute("substitute_id", "{44812A21-CC70-43FD-AF3A-965ED6CF837A}"),
            //        new XAttribute("name", "kode"),
            //        new XAttribute("title", "Kodeoversikten 2013 (15. utgave)"),
            //        new XAttribute("type", "tag_lonnkode"),
            //        new XAttribute("area", "global")
            //        )
            //   );

            
            
            //XElement paralist = new XElement("tags");


            //paralist.Add(intParent
            //    .Elements("item")
            //    .Select(q=> new XElement("tag1",
            //        q.Attributes(),
            //        total.Descendants("level")
            //        .Where(p => 
            //            p.Attribute("segment") == null
            //            && ((string)p.Attributes("id").FirstOrDefault() ?? "").StartsWith("K")
            //        )
            //        .Select(p=> new XElement("tag2", 
            //                new XAttribute("name",p.Attributes("id").First().Value.Trim().ToLower().Replace("k","")),
            //                new XAttribute("segment_id", p.Attributes("id").First().Value.Trim().ToLower()),
            //                new XAttribute("id", (string)p.Attributes("id").FirstOrDefault()),
            //                new XAttribute("title", p.Element("title").Value.Trim())
            //                )
            //        )
            //        )
            //    )
            //);



            //paralist.Save(@"C:\_Work\Kodeoversikt\segment\paralist.xml");               

            //XElement segments = XElement.Load(@"C:\_Work\Kodeoversikt\segment\segments.xml");
            //exTag = new XElement("external");
            //intTag = new XElement("internal");
            //unIdTag = new XElement("unid");


            //string include = "segment";
            //string exclude = "segment";
            //string breaknodes = "level;p;title;table;br;td;li";
            //string nonmarkupnodes = "title";

            //XElement regexps = XElement.Load(@"D:\RegExp\InTextLinksRegexp_20130214.xml");
            //string regex = regexps.RegexBuild("100_total");
            //XElement actions = XElement.Load(@"d:\regexp\InTextLinksActions.xml");

            //IdElements idEls = new IdElements(segments, actions, regex, include, exclude, breaknodes, nonmarkupnodes, true);

            //idEls.InternalItems = intParent.Elements("item")
            //                            .Select(p => new InternalItem(
            //                                (string)p.Attributes("name").FirstOrDefault(),
            //                                (string)p.Attributes("id").FirstOrDefault(),
            //                                (string)p.Attributes("topic_id").FirstOrDefault(),
            //                                (string)p.Attributes("type").FirstOrDefault(),
            //                                (string)p.Attributes("title").FirstOrDefault(),
            //                                (string)p.Attributes("substitute_id").FirstOrDefault(),
            //                                (string)p.Attributes("area").FirstOrDefault()
            //                                ));
            //idEls.documentTags = paralist;
            //idEls.InternalReference = true;
            ////idEls.SaveTextToFile(@"C:\_Work\2009\ABC\2013_1\Lignings-ABC 2013\idelements.xml");
            //XElement newD = idEls.Execute();
            //newD.Save(@"C:\_Work\Kodeoversikt\segment\newsegments.xml");
            //if (idEls.externalTags == null ? false : idEls.externalTags.HasElements) exTag = new XElement(idEls.externalTags);
            //if (idEls.internalTags == null ? false : idEls.internalTags.HasElements) intTag = new XElement(idEls.internalTags);
            //if (idEls.unidentifiedTags == null ? false : idEls.unidentifiedTags.HasElements) unIdTag = new XElement(idEls.unidentifiedTags);
            //MessageBox.Show("finished");
            //exTag.Save(@"C:\_Work\Kodeoversikt\external.xml");
            //intTag.Save(@"C:\_Work\Kodeoversikt\internal.xml");
            //unIdTag.Save(@"C:\_Work\Kodeoversikt\uid.xml");

        }

        private void button8_Click(object sender, EventArgs e)
        {
            XElement total = XElement.Load(@"C:\_Work\Kodeoversikt\segment\segments.xml");



            XElement intParent = new XElement("internalnodes",
                new XElement("item",
                    new XAttribute("id", "a7cd35e0-8b2b-4224-a20d-9aeaeb3493aa"),
                    new XAttribute("segment_id", "a7cd35e0-8b2b-4224-a20d-9aeaeb3493aa"),
                    new XAttribute("topic_id", "_self"),
                    new XAttribute("substitute_id", "{44812A21-CC70-43FD-AF3A-965ED6CF837A}"),
                    new XAttribute("name", "Kodeoversikten 2013 (15. utgave)"),
                    new XAttribute("regexpName", "kodenr"),
                    new XAttribute("area", "global"),
                    new XAttribute("language", "no"),
                    new XElement("name", new XAttribute("text", "kode"))
                )
            );



            XElement paralist = new XElement("tags");


            paralist.Add(intParent
                .Elements("item")
                .Select(q => new XElement("tag1",
                    q.Attributes(),
                    q.Elements(),
                    total.Descendants("level")
                    .Where(p =>
                        ((string)p.Attributes("id").FirstOrDefault() ?? "").StartsWith("K")
                    )
                    .Select(p => new XElement("tag2",
                            new XAttribute("name", p.Attributes("id").First().Value.Trim().ToLower().Replace("k", "")),
                            new XAttribute("segment_id", p.Ancestors("segment").Attributes("id").First().Value.Trim()),
                            new XAttribute("id", (string)p.Attributes("id").FirstOrDefault()),
                            new XAttribute("title", p.Element("title").Value.Trim())
                            )
                    )
                    )
                )
            );

            paralist.Save(@"C:\_Work\Kodeoversikt\segment\paralist.xml");  

            Identificator id = new Identificator();
            id.Include = "segment";
            id.Exclude = "segment;title;br";
            id.BreakNodes = "p;table;td;tr;th";
            id.Local = true;

            //id.InternalReference = true;
            id.InternalTags = paralist;

            XElement regexps = XElement.Load(@"D:\RegExp\InTextLinksRegexp_20130214.xml");
            string regex = regexps.RegexBuild("100_total");
            XElement actions = XElement.Load(@"d:\regexp\InTextLinksActions.xml");
            
            XElement newTotal = id.ExecuteLinking(total, actions, regex);
            if (id.Links.HasElements)
            {
                newTotal.Add(new XElement("segment",
                        new XAttribute("id", id.Links.Name.LocalName),
                            id.Links
                    )
                );
            }
            newTotal.Save(@"C:\_Work\Kodeoversikt\segment\newSegment.xml");
        }

        private void btnIFRSdoc_Click(object sender, EventArgs e)
        {
            string path = @"D:\_data\_ifrs\2018\BlueBook\index\";
            //string path = @"D:\_data\_ifrs\2017\GreenBook\Final XML\index\";
            //string path = @"D:\_data\_ifrs\redbook\2017\Final XML\index\";
            //string path = @"C:\_Work\2009\IFRS\2016_2\index\";
            //string path = @"C:\_Work\2009\IFRS\2016\index\";
            //string path = @"C:\_Work\2009\IFRS\2015_2\index\";
            //string path = @"C:\_Work\2009\IFRS\2015\Red\index\";
            //string path = @"C:\_Work\2009\IFRS\2014\IFRSRed2014\index\";
            //string path = @"C:\_Work\2009\IFRS\2014\IFRSGreen2014\index\";
            //string path = @"C:\_Work\2009\IFRS\2013\index\";
            //string path = @"C:\_Work\IFRS\HedgeMacro\index\";
            //string path = @"C:\_Work\IFRS\6310_Amendments_to_IAS_27_AUG_2014_XML\index\";
            //string path = @"C:\_Work\IFRS\64697_ED_Recognition_of_Deferred_Tax_Proposed_amendments_to_IAS_12_XML\index\";
            //string path = @"C:\_Work\IFRS\48351_IFRS_9_juli_2014\index\";
            XElement total = XElement.Load(path + @"segments.xml");

            XElement ext = null;
            //////string path_external = @"C:\_Work\2009\IFRS\2015_2\index\";
            ////////string path_external = @"C:\_Work\2009\IFRS\2014\IFRSGreen2014\index\";
            ////////string path_external = @"C:\_Work\2009\IFRS\2013\index\";
            ////////string path_external = @"C:\_Work\2009\IFRS\2012\index\";
            //////ext = XElement.Load(path_external + "tags.xml");

            //////ext = new XElement("externaltags",
            //////    new XElement("tags",
            //////        new XAttribute("topic_id", "{E6963F65-0A41-4B37-BEDE-F792AE584976}"),//"{AE8BA9A8-69B7-4598-A0D5-58573622E923}"),
            //////        //new XAttribute("topic_id", "{0A74B856-379D-4F72-8DE3-BDB968A7A1B4}"),
            //////        new XAttribute("regexpName", "ifrs_paras;ifrs_source"),
            //////        new XAttribute("language", "en"),
            //////        ext.Descendants("tag1")
            //////        //    select new XElement("tag1",
            //////        //        new XAttribute("topic_id", "{AE8BA9A8-69B7-4598-A0D5-58573622E923}"),
            //////        //        //new XAttribute("topic_id", "{0A74B856-379D-4F72-8DE3-BDB968A7A1B4}"),
            //////        //        new XAttribute("regexpName", "ifrs_paras;ifrs_source"),
            //////        //        new XAttribute("segment_id", (string)tag1.Attributes("segment_id").FirstOrDefault()),
            //////        //        tag1.Elements()
            //////        //    )
            //////        //)
            //////    )
            //////);


            XElement paralist = XElement.Load(path + "tags.xml");
            XElement InternalTags = new XElement("internaltags",
                new XElement("tags",
                    new XAttribute("topic_id", "_self"),
                    new XAttribute("regexpName", "ifrs_paras;ifrs_source"),
                    new XAttribute("area", "global"),
                    new XAttribute("language", "en"),
                    paralist.Elements("tag1")
                )
            );

           
            //(
            //    from t1 in paralist.Elements("tag1")
            //    select t1
            //)
            //.ToList()
            //.ForEach(p => p.Add(
            //        new XAttribute("topic_id", "_self"),
            //        new XAttribute("regexpName", "ifrs_paras;ifrs_source"),
            //        new XAttribute("area", "global"),
            //        new XAttribute("language", "en")
            //    //new XElement("name", new XAttribute("text", (string)p.Attributes("name").FirstOrDefault())),
            //    //new XElement("name", new XAttribute("text", "IFRS9"))
            //    )
            //);
            
            //XElement tagRegep = new XElement("tags",
            //        paralist
            //        .Descendants("tag1")
            //        .Union(ext.Descendants("tag1"))
            //        .Elements("name")
            //        .Where(p=>p.Parent.Attribute("regexpName")!=null)
            //        .Select(p=> new {
            //            tag1 = p.Value,
            //            regexpNames = (string)p.Parent.Attributes("regexpName").FirstOrDefault()
            //        })
            //        .SelectMany(p=>p.regexpNames.Split(';').Select(q=>new {tag1=p.tag1,regexpName=q}))
            //        .GroupBy(p=>p)
            //        .Select(p=> new XElement("tag1",
            //            new XAttribute("name", p.Key.tag1),
            //            new XAttribute("regexpName", p.Key.regexpName))
            //        ));



            InternalTags.Save(path + @"paralist.xml");

            Identificator id = new Identificator();
            id.Include = "segment";
            id.Exclude = "title;linebreak;tags;settings";
            id.BreakNodes = "section;subsection;subsubsection;subsubsubsection;level5;level6;td;th;tr;deflistitem;term;def;gref;para;npara;item;edu_para;edu_insert;entry_name;entry_reference;primary_entry;secondary_entry;tertiary_entry;quaternary_entry";
            id.NoMarkupNodes = "title";
            id.Local = true;
            id.ReferencedTopicsTags = ext;
            id.IdentifyGlobal = false;
            id.InternalReference = true;
            id.InternalTags = InternalTags;

            XElement regexps = XElement.Load(@"D:\RegExp\IFRS\InTextLinksRegexp_IFRS.xml");
            string regex = regexps.RegexBuild("ifrs_eng");
            XElement actions = XElement.Load(@"d:\regexp\IFRS\InTextLinksActionsIFRS.xml");


            //total = new XElement("segments",
            //        new XAttribute("id","1"),
            //        new XElement("segment",
            //            new XAttribute("id", "2"),
            //            new XElement("npara",
            //                new XAttribute("id", "3"),
            //                new XText("dette er IFRS 1 paragraph C4(g)(ii) på at det")
            //                    )
            //            )
            //        );
            
            DateTime start = DateTime.Now;
            Application.DoEvents();
            XElement newTotal = id.ExecuteLinking(total, actions, regex,"en");
            if (id.Links.HasElements)
            {
                //Debug.Print(id.Links.Descendants("link").Count().ToString());
                newTotal.Add(new XElement("segment",
                        new XAttribute("id", "diblink"),
                            id.Links
                    )
                );
            }

            newTotal.Save(path + @"newSegment.xml");
            
            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            Debug.Print("Tid=" +  duration.ToString());
        }

        private void btnSjekkliste_Click(object sender, EventArgs e)
        {
            string path = @"C:\_Work\Sjekkliste\";
            //string path_external = @"C:\_Work\2009\IFRS\2013\index\";
            XElement total = XElement.Load(path + @"IFRS sjekkliste før intextlinking.xml");


            Identificator id = new Identificator();
            id.Include = "document";
            id.Exclude = "document;ititle";
            id.BreakNodes = "item;law;ingress;description;p;table;td;tr;th";
            id.Local = true;
            //id.externalTags = ext;
            id.InternalReference = false;
            //id.documentTags = paralist;

            XElement regexps = XElement.Load(@"D:\RegExp\InTextLinksRegexp_20130214.xml");
            string regex = regexps.RegexBuild("100_total");
            XElement actions = XElement.Load(@"d:\regexp\InTextLinksActions.xml");

            Debug.Print("Sjekkliste");
            DateTime start = DateTime.Now;
            XElement newTotal = id.ExecuteLinking(total, actions, regex);
            if (id.Links.HasElements)
            {
                Debug.Print("Antall: " + id.Links.Descendants("link").Count().ToString());
                newTotal.Add(new XElement("segment",
                        new XAttribute("id", id.Links.Name.LocalName),
                            id.Links
                    )
                );
            }
            newTotal.Save(path + @"newSegment.xml");
            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            Debug.Print("Tid=" + duration.ToString());
        }

        private void btnSPH_Click(object sender, EventArgs e)
        {
            string path = @"D:\_data\_lovdata\lover.xml";
            XElement externaltags = XElement.Load(path);
            string language = "no";

    
            //string path = @"C:\_Work\SPH\";
            //string path = @"C:\_Work\SPH\sph-2015-2\";
            //path = @"C:\_Work\SPH\sph-2016-2\";
            path = @"D:\_SPH\2018\";
            XElement total = XElement.Load(path + @"segments.xml");


            XElement tags = total.Descendants("segment").Where(p=>(string)p.Attributes("id").FirstOrDefault() == "_internaltags").Descendants("tags").FirstOrDefault();
            tags.Add(new XAttribute("topic_id", "_self"));
            tags.Add(new XAttribute("regexpName", "_self"));
            tags.Add(new XAttribute("area", "global"));
            tags.Add(new XAttribute("language", language));
            XElement InternalTags = new XElement("internaltags",
                    tags
            );


            Identificator id = new Identificator();
            id.Include = "segment";
            id.Exclude = "ktittel;ptittel";
            id.BreakNodes = "avsnitt";
            id.NoMarkupNodes = "ktittle;ptittel";
            id.Local = true;
            id.InternalReference = true;
            id.ExternalTags = externaltags;
            id.InternalTags = InternalTags;

            XElement regexps = XElement.Load(@"D:\RegExp\NOR\InTextLinksRegexp_NOR.xml");
            string regex = regexps.RegexBuild("100_total_no");
            XElement actions = XElement.Load(@"d:\regexp\NOR\InTextLinksActionsNOR.xml");

            DateTime start = DateTime.Now;
            XElement newTotal = id.ExecuteLinking(total, actions, regex, language);
            if (id.Links.HasElements)
            {
                Debug.Print(id.Links.Descendants("link").Count().ToString());
                newTotal.Add(new XElement("segment",
                        new XAttribute("id", id.Links.Name.LocalName),
                            id.Links
                    )
                );
            }

            newTotal.Save(path + @"newSegment.xml");

            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            Debug.Print("Tid=" + duration.ToString());
        }

        private void btnIFRSN_Click(object sender, EventArgs e)
        {
            string language = "no";
            string path = @"C:\_Work\IFRS Norge\IFRSN\HTML\old\";
            //XElement total = XElement.Load(path + @"segments.xml");
            XElement total = new XElement("segment",
                new XAttribute("id","1"),
                new XElement("title",
                    new XAttribute("id","2"),
                    new XText("test")
                    ),
                new XElement("p",
                    new XAttribute("id","3"),
                    //new XText("dette er en test på at det {DA3DB617-F9A3-4668-93E6-BBB2E37B928F} virker for alle tema")
                    new XText("dette er en test på {DA3DB617-F9A3-4668-93E6-BBB2E37B928F}. Virker for alle tema")
                    )
                );

            Identificator id = new Identificator();
            id.Include = "segment";
            id.Exclude = "segment";
            id.BreakNodes = "wilist;witem;p;td;text";
            id.NoMarkupNodes = "";
            id.Local = true;
            id.InternalReference = false;
            id.InternalTags = null;

            //XElement regexps = XElement.Load(@"D:\RegExp\InTextLinksRegexp_20130214.xml");
            //string regex = regexps.RegexBuild("100_total");
            //XElement actions = XElement.Load(@"d:\regexp\InTextLinksActions.xml");

            XElement regexps = XElement.Load(@"D:\RegExp\NOR\InTextLinksRegexp_NOR.xml");
            string regex = regexps.RegexBuild("100_total");
            XElement actions = XElement.Load(@"d:\regexp\NOR\InTextLinksActionsNOR.xml");

            DateTime start = DateTime.Now;
            XElement newTotal = id.ExecuteLinking(total, actions, regex, language);
            if (id.Links.HasElements)
            {
                Debug.Print(id.Links.Descendants("link").Count().ToString());
                newTotal.Add(new XElement("segment",
                        new XAttribute("id", id.Links.Name.LocalName),
                            id.Links
                    )
                );
            }

            newTotal.Save(path + @"newSegment.xml");

            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            Debug.Print("Tid=" + duration.ToString());
        }

        private void root7_Click(object sender, EventArgs e)
        {
            string language = "no";
            string path = @"C:\_test\itl\";
            XElement total = XElement.Load(path + @"test.xml");
            total.Elements("content").Remove();

            Identificator id = new Identificator();
            id.Include = "level";
            id.Exclude = "level;title";
            id.BreakNodes = "text;p;table;tr;th;td;div;li";
            id.NoMarkupNodes = "title;a";
            id.Local = true;
            id.InternalReference = false;
            id.InternalTags = null;

            //XElement regexps = XElement.Load(@"D:\RegExp\InTextLinksRegexp_20130214.xml");
            //string regex = regexps.RegexBuild("100_total");
            //XElement actions = XElement.Load(@"d:\regexp\InTextLinksActions.xml");

            XElement regexps = XElement.Load(@"D:\RegExp\NOR\InTextLinksRegexp_NOR.xml");
            string regex = regexps.RegexBuild("100_total");
            XElement actions = XElement.Load(@"d:\regexp\NOR\InTextLinksActionsNOR.xml");

            DateTime start = DateTime.Now;
            XElement newTotal = id.ExecuteLinking(total, actions, regex, language);
            if (id.Links.HasElements)
            {
                Debug.Print(id.Links.Descendants("link").Count().ToString());
                //newTotal.Add(new XElement("segment",
                //        new XAttribute("id", id.Links.Name.LocalName),
                //            id.Links
                //    )
                //);
                id.Links.Save(path + @"diblinks.xml");
            }

            newTotal.Save(path + @"newtest.xml");

            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            Debug.Print("Tid=" + duration.ToString());
        }

        private void btnChecklist_Click(object sender, EventArgs e)
        {
            string language = "no";
            string path = @"C:\_test\sjekkliste\";
            XElement total = XElement.Load(path + @"xml8.xml");
            total.DescendantsAndSelf().Nodes().OfType<XText>().ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(Regex.Replace(p.Value, @"\r\n|\n", ""), @"\s+|\t", " "))));
            //total.DescendantsAndSelf().Nodes().OfType<XText>().ToList().ForEach(p=>Debug.Print(p.Value));

            Identificator id = new Identificator();
            id.Include = "item";
            id.Exclude = "item";
            id.BreakNodes = "ititle;law;ingress;description;text;p;table;tr;th;td;div;li";
            id.NoMarkupNodes = "a";
            id.Local = true;
            id.InternalReference = false;
            id.InternalTags = null;

            //XElement regexps = XElement.Load(@"D:\RegExp\InTextLinksRegexp_20130214.xml");
            //string regex = regexps.RegexBuild("100_total");
            //XElement actions = XElement.Load(@"d:\regexp\InTextLinksActions.xml");

            XElement regexps = XElement.Load(@"D:\RegExp\NOR\InTextLinksRegexp_NOR.xml");
            string regex = regexps.RegexBuild("100_total_no");
            XElement actions = XElement.Load(@"d:\regexp\NOR\InTextLinksActionsNOR.xml");

            DateTime start = DateTime.Now;
            XElement newTotal = id.ExecuteLinking(total, actions, regex, language);
            if (id.Links.HasElements)
            {
                Debug.Print(id.Links.Descendants("link").Count().ToString());
                //newTotal.Add(new XElement("segment",
                //        new XAttribute("id", id.Links.Name.LocalName),
                //            id.Links
                //    )
                //);
                id.Links.Save(path + @"diblinks.xml");
            }

            newTotal.Save(path + @"newtest.xml");

            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            Debug.Print("Tid=" + duration.ToString());

        }

        private void btmAA2014_Click(object sender, EventArgs e)
        {
            string language = "no";
            //string path = @"C:\_Work\Årsavslutning\";
            //string path = @"C:\_Work\IFRS Norge\Redigerbar\20150119\";
            string path = @"D:\_books\Ansvarlige selskaper og indre selskaper\index\";
            //XElement total = XElement.Load(path + @"segments.xml");
            //total.DescendantsAndSelf().Nodes().OfType<XText>().ToList().ForEach(p=>Debug.Print(p.Value));
            XElement total = new XElement("segment",
                            new XAttribute("id", "1"),
                            new XElement("title",
                                new XAttribute("id", "2"),
                                new XText("test")
                                ),
                            new XElement("p",
                                new XAttribute("id", "3"),
                                new XText("Forskrift om beregning av ansvarlig kapital § 17 første ledd bokstav g til i, andre ledd og tredje ledd.")
                                )
                            );

            path = @"C:\_Test\lovdata_container_linked\lover.xml";
            XElement externaltags = XElement.Load(path);
            

            Identificator id = new Identificator();
            id.Include = "segment";
            id.Exclude = "title";
            id.BreakNodes = "level;text;p;table;tr;th;td;div;li";
            id.NoMarkupNodes = "a;title";
            id.Local = true;
            id.ReferencedTopicsTags = externaltags;
            id.InternalReference = true;
            id.InternalTags = null;

            //XElement regexps = XElement.Load(@"D:\RegExp\InTextLinksRegexp_20130214.xml");
            //string regex = regexps.RegexBuild("100_total");
            //XElement actions = XElement.Load(@"d:\regexp\InTextLinksActions.xml");

            XElement regexps = XElement.Load(@"D:\RegExp\NOR\InTextLinksRegexp_NOR.xml");
            string regex = regexps.RegexBuild("100_total_no");
            XElement actions = XElement.Load(@"d:\regexp\NOR\InTextLinksActionsNOR.xml");

            DateTime start = DateTime.Now;
            XElement newTotal = id.ExecuteLinking(total, actions, regex, language);
            if (id.Links.HasElements)
            {
                Debug.Print(id.Links.Descendants("link").Count().ToString());
                newTotal.Add(new XElement("segment",
                        new XAttribute("id", id.Links.Name.LocalName),
                            id.Links
                    )
                );
            }
            if (id.ExternalExtractTags!= null)
                id.ExternalExtractTags.Save(path + @"extExtratags.xml");
            if (id.ExternalTags != null)
                id.ExternalTags.Save(path + @"exttags.xml"); 
            newTotal.Save(path + @"newsegments.xml");

            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            Debug.Print("Tid=" + duration.ToString());

        }

        private void btnLABC2015_Click(object sender, EventArgs e)
        {
            XElement externaltags = XElement.Load(@"D:\_data\_lovdata\lover.xml");

            //string path = @"D:\_data\_skatte_abc\20170623\index\";
            string path = @"D:\_books\Skattebetalings\2017\index\";
            //string path = @"D:\_data\_skatte_forv_håndbok\20170913\index\";
            string language = "no";

            XDocument d = XDocument.Load(path + @"segments.xml");
            XElement total = d.Root;
            Identificator id = new Identificator();
            id.Include = "segment";
            id.Exclude = "tags:tittel;stikkt;tit;ms;ms1;ms2;ms3;ms4;ms5";
            id.BreakNodes = "forord;innldel;lover;vedlegg;del;delkapittel;kapittel;sek;subsek1;subsek2;subsek3;subsek4;subsek5;a;a1;kom;tbl;table;entry;row;tbody;tgroup;list;pkt;";
            id.NoMarkupNodes = "xref;zref";
            id.Local = true;
            id.InternalReference = false;
            id.InternalTags = null;
            id.ReferencedTopicsTags = externaltags;

            //XElement regexps = XElement.Load(@"D:\RegExp\InTextLinksRegexp_20130214.xml");
            //string regex = regexps.RegexBuild("100_total");
            //XElement actions = XElement.Load(@"d:\regexp\InTextLinksActions.xml");

            XElement regexps = XElement.Load(@"D:\RegExp\NOR\InTextLinksRegexp_NOR.xml");
            string regex = regexps.RegexBuild("100_total_no");
            XElement actions = XElement.Load(@"d:\regexp\NOR\InTextLinksActionsNOR.xml");

            DateTime start = DateTime.Now;
            id.Local = true;
            XElement newTotal = id.ExecuteLinking(total, actions, regex, language);
            if (id.Links.HasElements)
            {
                Debug.Print(id.Links.Descendants("link").Count().ToString());
                newTotal.Add(new XElement("segment",
                        new XAttribute("id", id.Links.Name.LocalName),
                            id.Links
                    )
                );
            }
            id.ExternalExtractTags.Save(path + @"extExtratags.xml");
            if (id.ExternalTags!= null) id.ExternalTags.Save(path + @"exttags.xml");
            newTotal.Save(path + @"newABC.xml");

            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            Debug.Print("Tid=" + duration.ToString());
            MessageBox.Show("Ferdig");

        }

        private void button9_Click(object sender, EventArgs e)
        {

            string language = "no";
            string imgPath = @"c:\_test\lovdata\images\";
            string topicsPath = @"C:\_Test\lovdata\topics\documents.xml";
            string lDocsPath = @"C:\_Test\lovdata\topics\lovdatadocuments.xml";
            string dibTopicsPath = @"C:\_Test\lovdata\topics\dibdocuments.xml";
            XDocument topics = XDocument.Load(topicsPath);
            topics.Descendants("document").Attributes("status").Remove();
            topics.Descendants("document").Elements("filename").Remove();
            string path = @"C:\_Test\lovdata_container\";

            if (File.Exists(@"C:\_Test\lovdata_container_linked\lover.xml"))
                File.Delete(@"C:\_Test\lovdata_container_linked\lover.xml");

            List<string> fileEntries = Directory.GetFiles(path).ToList();

            


           

            XElement regexps = XElement.Load(@"D:\RegExp\NOR\InTextLinksRegexp_NOR.xml");
            string regex = regexps.RegexBuild("100_total_no");
            XElement actions = XElement.Load(@"d:\regexp\NOR\InTextLinksActionsNOR.xml");

            XElement lDocs = new XElement("documents");
            foreach (string fileName in fileEntries)
            {
                
                XElement lDoc = new XElement("document");
                FileInfo fi = new FileInfo(fileName);
                XDocument container = XDocument.Load(fileName);
                if (fi.Name == "for-2016-08-12-974.xml")
                {

                }
                lDoc.Add(new XElement("filename", fi.Name));
                lDoc.Add(new XElement("id", (string)container.Root.Attributes("id").FirstOrDefault()));
                lDoc.Add(new XElement("navn", (string)container.Descendants("tekst").Elements("tittel").Select(p => p.Value).FirstOrDefault()));
                //lDoc.Add(new XElement("navn", (string)container.Root.Attributes("id").FirstOrDefault()));

                string id = (string)container.Root.Attributes("id").FirstOrDefault();
                if (id != null)
                {
                    XElement topic = topics.Descendants("document")
                        .Where(p=>p.Attributes("status").FirstOrDefault()==null)
                        .Where(p => ((string)p.Elements("id").FirstOrDefault() ?? "").ToLower() == id.ToLower())
                        .FirstOrDefault();
                    if (topic==null)
                    {
                        topic = topics
                            .Descendants("document")
                            .Elements()
                            .Where(p => p.Attributes("status").FirstOrDefault() == null)
                            .Where(p => p.Name.LocalName.ToLower().StartsWith("name_ext"))
                            .Where(p => p.Value != "")
                            .Select(p => new { m = Regex.Match(p.Value.ToLower().Trim(), @"((niffor|nifl|for|lov)(\-))?(?<year>(\d\d\d\d))(\-)(?<mon>(\d+))(\-)(?<day>(\d+))(\-|\snr(\.)\s)(?<number>(\d+))", RegexOptions.IgnoreCase), e = p })
                            .Where(p => p.m.Success)
                            .Select(p => new { id = p.m.Groups["year"].Value + "-" + Convert.ToInt32(p.m.Groups["mon"].Value).ToString().PadLeft(2, '0') + "-" + Convert.ToInt32(p.m.Groups["day"].Value).ToString().PadLeft(2, '0') + "-" + Convert.ToInt32(p.m.Groups["number"].Value).ToString(), e = p.e })
                            .Where(p => p.id == id.Substring(4))
                            .Select(p => p.e.Parent)
                            .FirstOrDefault();
                        if (topic != null)
                        {

                            if (topic.Elements("id").FirstOrDefault() == null)
                                topic.Add(new XElement("id", id));
                            else
                                topic.Elements("id").FirstOrDefault().Value = id;
                        }
                        
                        
                    }
                    string topic_id = (topic == null ? null : topic.Elements("topic_id").Select(p => p.Value).FirstOrDefault());
                    if (topic_id != null)
                    {
                        lDoc.Add(new XAttribute("status", "1"));
                        lDoc.Add(new XElement("topic_id", topic_id));
                        if (topic.Attributes("status").FirstOrDefault() != null)
                        {
                            topic.Attributes("status").FirstOrDefault().SetValue("1");
                        }
                        else
                            topic.Add(new XAttribute("status", "1"));

                        if (topic.Elements("filename").FirstOrDefault() == null)
                            topic.Add(new XElement("filename", fi.Name));
                        else
                            topic.Elements("filename").FirstOrDefault().Value = fi.Name;

                        if (container.Root.Attributes("topic_id").FirstOrDefault() == null)
                        {
                            container.Root.Add(new XAttribute("topic_id", topic_id));
                        }
                        else
                        {
                            container.Root.Attributes("topic_id").FirstOrDefault().SetValue(topic_id);
                        }
                        if (container.Descendants("img").Attributes("src").Count() != 0)
                        {
                            if (Directory.Exists(imgPath +  topic_id  +  @"\"))
                                Directory.GetFiles(imgPath +  topic_id  +  @"\").ToList().ForEach(p=>File.Delete(p));
                            else
                                Directory.CreateDirectory(imgPath +  topic_id  +  @"\");

                            List<string> src = container.Descendants("img").Attributes("src").Select(p=>p.Value).ToList();
                                foreach (string s in src)
                                {
                                    if (!File.Exists(imgPath +  topic_id + @"\" + s))
                                        File.Copy(imgPath + s, imgPath  + topic_id + @"\" + s);
                                }
                        }
                        
                        XElement tag1 = container.Root.Elements("tags").Elements("tag1").FirstOrDefault();

                        if (
                            ((string)tag1.Attributes("title").FirstOrDefault()).Trim().ToLower()
                            != 
                            ((string)topic.Elements("name").Select(p => p.Value).FirstOrDefault()).Trim().ToLower()
                            )
                        {
                            tag1.Attribute("title").SetValue(((string)topic.Elements("name").Select(p => p.Value).FirstOrDefault()).Trim());
                        }
                        List<string> names = tag1
                                        .Elements("name")
                                        .Where(p=>p.Value.Trim().ToLower()!="inkl.")
                                        .Select(p => p.Value.Trim().TrimEnd('.').ToLower())
                                        .Union(
                                            topic
                                            .Elements("names")
                                            .Elements("name")
                                            .Where(p => p.Value.Trim().ToLower() != "inkl.")
                                            .Select(p => p.Value.Trim().TrimEnd('.').ToLower())
                                        )
                                        .GroupBy(p => p)
                                        .Select(p => p.Key)
                                        .ToList();
                        if (names.Count() > 4) Debug.Print("xx");
                        tag1.Elements("name").Remove();
                        names.ForEach(p => tag1.AddFirst(new XElement("name", p)));

                        container.Save(fileName);
                        topics.Save(topicsPath);
                        topics = XDocument.Load(topicsPath);
                    }
                    else
                    {
                        lDoc.Add(new XAttribute("status", "0"));
                        Debug.Print(id +  " " + (string)container.Root.Descendants("korttittel").Select(p=>p.Value).FirstOrDefault() ?? "");
                        
                        
                        container.Root.Attributes("topic_id").Remove();
                    }

                }
                
                lDocs.Add(lDoc);
            }
            int noId = topics.Descendants("document")
                        .Where(p => p.Attributes("status").FirstOrDefault() == null).Count();
            int yesId = topics.Descendants("document")
                        .Where(p => (string)p.Attributes("status").FirstOrDefault() =="1").Count();

            

            MessageBox.Show(yesId.ToString() + " identifisert, " + noId.ToString() + " uidentifiserte");

            string uIdes =  topics.Descendants("document")
                       .Where(p => p.Attributes("status").FirstOrDefault() == null)
                       .Select(p =>p.Elements("topic_id").Select(s=>s.Value).FirstOrDefault() + ' ' +  p.Elements("id").Select(s=>s.Value).FirstOrDefault() + ' ' + p.Elements("name").Select(s=>s.Value).FirstOrDefault() + "\r\n")
                       .StringConcat();
            MessageBox.Show(uIdes);

            lDocs.Save(lDocsPath);

            lDocs.Save(@"c:\_test\lovdata_container_linked\documents.xml");

            XElement LovdataTags = new XElement("externaltags",
                      fileEntries
                       .Select(p => XDocument.Load(p))
                       .Where(p => p.Root.Attributes("topic_id").FirstOrDefault() != null)
                       .Select(p => new XElement("tags",
                           p.Root.Attributes("topic_id"),
                           new XAttribute("regexpName", "tag_lovname;source_para"),
                           new XAttribute("area", "global"),
                           new XAttribute("language", "no"),
                           new XElement("tag1",
                               new XAttribute("id", (string)p.Root.Attributes("topic_id").FirstOrDefault()),
                               p.Root.Elements("tags").Elements("tag1").Attributes("title"),
                                p.Root.Elements("tags").Elements("tag1").Elements("name"),
                                p.Root.Elements("tags").Elements("tag1").Elements("tag2")
                               )
                          ))
                  );

            LovdataTags.Save(@"c:\_test\lovdata_container_linked\lover.xml");


            XElement dibTopics = new XElement(topics.Root);
            dibTopics.Descendants("names").ToList().ForEach(p=>p.Remove());
            dibTopics.Save(dibTopicsPath);
            topics.Save(topicsPath);

            //foreach (string fileName in fileEntries.Where(p=>p.IndexOf("lov-2007-06-29-74.xml")!=-1))
            XElement eexternaltags = new XElement("externaltags",
                fileEntries
                 .Select(p => XDocument.Load(p))
                 .Where(p => p.Root.Attributes("topic_id") != null)
                 .Select(p => new XElement("tags",
                     p.Root.Attributes("topic_id"),
                     new XAttribute("regexpName", "tag_lovname;source_para"),
                     new XAttribute("area", "global"),
                     new XAttribute("language", language),
                     p.Root.Elements("tags").Elements("tag1")))
            );

            int x = 0;
            
            foreach (string fileName in fileEntries)
            {
                x++;
                button9.Text = x.ToString();
                button9.Refresh();
                if (fileName == @"C:\_Test\lovdata_container\for-2009-05-20-556.xml")
                {
                }
                
                XDocument container = XDocument.Load(fileName);
                string id = (string)container.Root.Attributes("id").FirstOrDefault();
                string topic_id = (string)container.Root.Attributes("topic_id").FirstOrDefault();
                if (topic_id != null)
                {
                    XElement externaltags = eexternaltags;
                    externaltags.Descendants("tags").Where(p=>(string)p.Attributes("topic_id").FirstOrDefault()==topic_id).Remove();


                   //XElement externaltags = new XElement("externaltags",
                   //    fileEntries.Where(p => p != fileName)
                   //     .Select(p => XDocument.Load(p))
                   //     .Where(p=>p.Root.Attributes("topic_id")!=null)
                   //     .Select(p => new XElement("tags",
                   //         p.Root.Attributes("topic_id"),
                   //         new XAttribute("regexpName", "tag_lovname;source_para"),
                   //         new XAttribute("area", "global"),
                   //         new XAttribute("language", language),
                   //         p.Root.Elements("tags").Elements("tag1")))
                   //);

                    
                    
                    XElement document = container.Root.Elements("dokument").FirstOrDefault();
                //    new XElement("internaltags",
                //new XElement("tags",
                //    new XAttribute("topic_id", "_self"),
                //    new XAttribute("regexpName", "ifrs_paras;ifrs_source"),
                //    new XAttribute("area", "global"),
                //    new XAttribute("language", "en"),
                    XElement tags = new XElement(container.Root.Elements("tags").FirstOrDefault());
                    tags.Add(new XAttribute("topic_id", "_self"));
                    tags.Add(new XAttribute("regexpName", "source_para"));
                    tags.Add(new XAttribute("area", "global"));
                    tags.Add(new XAttribute("language", language));

                    XElement internaltags =  new XElement("internaltags",tags);
                        
                    
                    Identificator idf = new Identificator();
                    if (id != null) idf.DefaultTag1 = id;
                    idf.Include = "tekst";
                    idf.Exclude = "korttittel;tittel;tit;kverdi;ktittel;pverdi;ptittel;ref;sup;historisk_note;fotnote;lverdi;fref;omraadekode;aar;sub";
                    idf.BreakNodes = "ledd;avsnitt;liste;marg;entry;paragraf;center;";
                    idf.NoMarkupNodes = "ref;";
                    idf.Local = true;
                     
                    idf.InternalReference = true;
                    idf.InternalTags = internaltags;
                    idf.ReferencedTopicsTags = externaltags;

                    //document = new XElement("tekst", 
                    //        new XElement("ledd",
                    //             new XText("dette er en test på at asl. § 2-2. aksjeloven § 2-2. Dete er derfor")
                    //    ));
                    
                    XElement newTotal = idf.ExecuteLinking(document, actions, regex, language);
                    if (idf.Links.HasElements)
                    {
                        XElement d = new XElement("container",
                            container.Root.Attributes("id"),
                            container.Root.Attributes("topic_id"),
                            document,
                            internaltags,
                            container.Root.Elements("items").FirstOrDefault(),
                            idf.Links,
                            container.Root.Elements("searchitems").FirstOrDefault()
                        );
                        d.Save(@"c:\_test\lovdata_container_linked\" + id + ".xml");
                    }
                    else
                    {
                        XElement d = new XElement("container",
                            container.Root.Attributes("id"),
                            container.Root.Attributes("topic_id"),
                            document,
                            internaltags,
                            container.Root.Elements("items").FirstOrDefault(),
                            new XElement("diblink"),
                            container.Root.Elements("searchitems").FirstOrDefault()
                        );
                        d.Save(@"c:\_test\lovdata_container_linked\" + id + ".xml");

                    }

            
                }
            }

            MessageBox.Show("Ferdig!");
        }

        private void btnMVA_Click(object sender, EventArgs e)
        {

            string language = "no";
            string path = @"D:\_data\_mva_abc\index\";

            XElement externaltags = XElement.Load(@"D:\_data\_lovdata\lover.xml");

            XElement total = XElement.Load(path + @"segments.xml");
            XElement internaltags = XElement.Load(path  + "internaltags.xml");

            Identificator id = new Identificator();
            id.Include = "segment";
            id.Exclude = "tags;tittel;tit;ms;ms1;ms2;ms3;ms4;ms5";
            id.BreakNodes = "kapittel;delkapittel;forord;glossar;seksjon;sign:dato;sek;subsek1;subsek2;subsek3;subsek4;subsek5;a;a1;enkeltsaker-1;enkeltsaker-2;enkeltsaker-tittel;tbl;table;entry;row;tbody;tgroup;list;pkt;term;def";
            id.NoMarkupNodes = "xref;zref";
            id.Local = false;
            id.InternalReference = true;
            id.InternalTags = internaltags;
            id.ReferencedTopicsTags = externaltags;

            //XElement regexps = XElement.Load(@"D:\RegExp\InTextLinksRegexp_20130214.xml");
            //string regex = regexps.RegexBuild("100_total");
            //XElement actions = XElement.Load(@"d:\regexp\InTextLinksActions.xml");

            XElement regexps = XElement.Load(@"D:\RegExp\NOR\InTextLinksRegexp_NOR.xml");
            string regex = regexps.RegexBuild("100_total_no");
            XElement actions = XElement.Load(@"d:\regexp\NOR\InTextLinksActionsNOR.xml");

            DateTime start = DateTime.Now;
            id.Local = true;
            XElement newTotal = id.ExecuteLinking(total, actions, regex, language);
            if (id.Links.HasElements)
            {
                Debug.Print(id.Links.Descendants("link").Count().ToString());
                newTotal.Add(new XElement("segment",
                        new XAttribute("id", id.Links.Name.LocalName),
                            id.Links
                    )
                );
            }
            id.ExternalExtractTags.Save(path + @"extExtratags.xml");
            if (id.ExternalTags != null) id.ExternalTags.Save(path + @"exttags.xml");
            newTotal.Save(path + @"newABC.xml");

            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            Debug.Print("Tid=" + duration.ToString());
            MessageBox.Show("Ferdig");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            string language = "no";
            //XElement total = new XElement("searchitems",
            //    new XElement("search",
            //        new XText("Dersom lagmannsretten kommer til at ligningen er materielt uriktig, må ligningen oppheves, og det angis i domsslutningen hvordan ligningen skal utføres, jf. ligningsloven § 11-1 nr. 5 første punktum. Ved saksbehandlingsfeil oppheves ligningen uten angivelse av hvordan ny ligning skal foretas, jf. ligningsloven § 11-1 nr. 5 andre punktum. Under prosedyren har Lani også gitt uttrykk for sitt syn på hva dom kan gå ut på hvis retten finner at ligningen er materielt riktig, men basert på andre saksforhold eller rettslige vilkår enn Skatteklagenemnda har lagt til grunn.")
            //));

            XElement total = XElement.Load(@"d:/test/search/search.xml");
                

            Identificator id = new Identificator();
            id.Include = "searchitems";
            id.Exclude = "";
            id.BreakNodes = "search";
            id.NoMarkupNodes = "";
            id.Local = false;
            id.InternalReference = true;
            id.InternalTags = null;
            id.ReferencedTopicsTags = null;


            XElement regexps = XElement.Load(@"D:\RegExp\NOR\InTextLinksRegexp_NOR.xml");
            string regex = regexps.RegexBuild("100_total_no");
            XElement actions = XElement.Load(@"d:\regexp\NOR\InTextLinksActionsNOR.xml");

            DateTime start = DateTime.Now;
            id.Local = true;
            //XElement newTotal = id.ExecuteMark(total, actions, regex, language);

           // newTotal.Save(@"d:/test/search/newsearch.xml");
            MessageBox.Show("Ferdig");
        }

        private void btnLinks_Click(object sender, EventArgs e)
        {
            string path = @"C:\_Test\lovdata_container_linked\lover.xml";
            XElement externaltags = XElement.Load(path);
            string language = "no";

            path = @"C:\_Work\bøker\Lærebok i skatterett\Lærebok i skatterett\";
            XDocument d = XDocument.Load(path + @"links.xml");
            XElement total = d.Root;
            Identificator id = new Identificator();
            id.Include = "links";
            id.Exclude = "id";
            id.BreakNodes = "text;";
            id.NoMarkupNodes = "tag1;tag2";
            id.Local = true;
            id.InternalReference = false;
            id.InternalTags = null;
            id.ReferencedTopicsTags = externaltags;

            //XElement regexps = XElement.Load(@"D:\RegExp\InTextLinksRegexp_20130214.xml");
            //string regex = regexps.RegexBuild("100_total");
            //XElement actions = XElement.Load(@"d:\regexp\InTextLinksActions.xml");

            XElement regexps = XElement.Load(@"D:\RegExp\NOR\InTextLinksRegexp_NOR.xml");
            string regex = regexps.RegexBuild("100_total_no");
            XElement actions = XElement.Load(@"d:\regexp\NOR\InTextLinksActionsNOR.xml");

            DateTime start = DateTime.Now;
            id.Local = true;
            XElement newTotal = id.ExecuteLinking(total, actions, regex, language);
            if (id.Links.HasElements)
            {
                Debug.Print(id.Links.Descendants("link").Count().ToString());
                newTotal.Add(new XElement("segment",
                        new XAttribute("id", id.Links.Name.LocalName),
                            id.Links
                    )
                );
            }
            id.ExternalExtractTags.Save(path + @"extExtratags.xml");
            if (id.ExternalTags != null) id.ExternalTags.Save(path + @"exttags.xml");
            newTotal.Save(path + @"newLinks.xml");

            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            Debug.Print("Tid=" + duration.ToString());
            MessageBox.Show("Ferdig");

        }

        private void btnSkattBook_Click(object sender, EventArgs e)
        {
            string path = @"C:\_Work\bøker\Bedrift, selskap og skatt 6. utg\";
            XElement total = XElement.Load(path + @"segments.xml");

           



            XElement intParent = new XElement("internalnodes",
                new XElement("item",
                    new XAttribute("id", "3fc5501e-5c1d-4ffe-95a7-e4dc07ab941a"),
                    new XAttribute("segment_id", "3fc5501e-5c1d-4ffe-95a7-e4dc07ab941a"),
                    new XAttribute("title", "Kommentarutgave - Innledning"),
                    new XAttribute("regexpName", "idavsnitt;"),
                    new XAttribute("area", "global")

                      )

               );


            XElement paralist = new XElement("tags");

            paralist.Add(
                total
                .DescendantsAndSelf("Seksjon")
                .Where(r =>
                        ((string)r.Ancestors("Hoveddel").Attributes("id").FirstOrDefault()).ToLower() == ((string)intParent
                                                                                                            .Elements("item")
                                                                                                            .Where(p => p.Attribute("regexpName").Value == "idavsnitt;")
                                                                                                            .Attributes("segment_id")
                                                                                                            .FirstOrDefault()).ToLower()
                        && r.Element("Tittel") != null
                )
                .Where(s => s.Attribute("segment") == null
                    &&
                    Regex.IsMatch(s.Elements("Tittel").First().Value.Trim(), @"^(?<nr>(\d+((\.\d+)+)?))$"))
                .Select(s =>
                    new XElement("tag1",
                        new XAttribute("topic_id", "_self"),
                        new XAttribute("segment_id", (string)s.Ancestors("segment").Attributes("id").FirstOrDefault()),
                        new XAttribute("id", (string)s.Attributes("id").FirstOrDefault()),
                        s.Attribute("regexpName"),
                        new XAttribute("title", Regex.Match(s.Elements("Tittel").First().Value.Trim(), @"(?<nr>(\d+((\.\d+)+)?))$").Groups["nr"].Value),
                        new XElement("name", "avsnitt" + Regex.Match(s.Elements("Tittel").First().Value.Trim(), @"(?<nr>(\d+((\.\d+)+)?))$").Groups["nr"].Value)
                        )
                    )
                );


            paralist.Add(intParent
                .Elements("item")
                .Select(q => new XElement("tag1",
                    new XAttribute("topic_id", "_self"),
                    q.Attributes(),
                    q.Elements(),
                    total.Descendants("Kapittel")
                    .Where(p => (string)p.Attributes("id").Select(s => s.Value.ToLower()).FirstOrDefault() == (string)p.Ancestors("segment").Attributes("id").Select(s => s.Value.ToLower()).FirstOrDefault()
                        && (string)p.Ancestors("Hoveddel").Attributes("id").Select(s => s.Value.ToLower()).FirstOrDefault()
                        == (string)q.Attributes("segment_id").Select(s => s.Value.ToLower()).FirstOrDefault()
                    )
                    .Select(p => new XElement("tag2",
                            new XAttribute("segment_id", p.Attributes("id").First().Value.Trim().ToLower()),
                            new XAttribute("title", p.Element("Tittel").Value.Trim()),
                            new XElement("name", "kap" + Regex.Match(p.Element("Tittel").Value.Trim().ToLower(), @"(kap\.\s+)(\d+)").Groups[2].Value)
                            )
                    ),
                    total.Descendants("Underkapittel")
                    .Where(p => (string)p.Attributes("id").Select(s => s.Value.ToLower()).FirstOrDefault() == (string)p.Ancestors("segment").Attributes("id").Select(s => s.Value.ToLower()).FirstOrDefault()
                        && (string)p.Ancestors("Hoveddel").Attributes("id").Select(s => s.Value.ToLower()).FirstOrDefault()
                        == (string)q.Attributes("segment_id").Select(s => s.Value.ToLower()).FirstOrDefault()
                    )
                    //.Where(p =>
                    //    p.Attributes("segment").FirstOrDefault() == null
                    //    && ((string)p.Ancestors("Hoveddel").Attributes("id").FirstOrDefault()).ToLower() == ((string)q.Attributes("segment_id").FirstOrDefault()).ToLower()
                    //)
                    .Select(p => new XElement("tag2",
                            new XAttribute("segment_id", p.Attributes("id").First().Value.Trim().ToLower()),
                            new XAttribute("title", p.Element("Tittel").Value.Trim()),
                            new XElement("name", "kap" + Regex.Match(p.Parent.Element("Tittel").Value.Trim().ToLower(), @"(kap\.\s+)(\d+)").Groups[2].Value + "?" + Regex.Match(p.Element("Tittel").Value.Trim().ToLower(), @"^(?<value>([ivx]+))\.").Groups["value"].Value)
                            )
                    ),
                    total.Descendants("Paragraf")
                    .Where(p => (string)p.Attributes("id").Select(s => s.Value.ToLower()).FirstOrDefault() == (string)p.Ancestors("segment").Attributes("id").Select(s => s.Value.ToLower()).FirstOrDefault()
                        && (string)p.Ancestors("Hoveddel").Attributes("id").Select(s => s.Value.ToLower()).FirstOrDefault()
                        == (string)q.Attributes("segment_id").Select(s => s.Value.ToLower()).FirstOrDefault()
                    )
                    .SelectMany(p => GetParagraf(p))




                    )
                )
            );



            paralist.Save(@"D:\_books\_arbakke2\paralist.xml");


            paralist = new XElement("internaltags", paralist);


            //id.Include = "segment";
            //id.Exclude = "Tittel;Mtit";
            //id.BreakNodes = "bookcontent;Hoveddel;Forkortelser;Forkortelse;Kapittel;Seksjon;Underkapittel;Paragraf;Avsnitt;Litra;Term;Definisjon;Tittel;Ledd;Mtit;Uth";
            //id.NoMarkupNodes = "Tittel;Mtit";
            //id.Local = true;
            ////id.externalTags = ext;
            //id.InternalReference = true;
            //id.InternalTags = paralist;

            //XElement regexps = XElement.Load(@"D:\RegExp\nor\InTextLinksRegexp_nor.xml");
            //string regex = regexps.RegexBuild("100_total_no");
            //XElement actions = XElement.Load(@"d:\regexp\nor\InTextLinksActionsNor.xml");

            //DateTime start = DateTime.Now;
            //XElement newTotal = id.ExecuteLinking(total, actions, regex, "no");
            //if (id.Links.HasElements)
            //{
            //    Debug.Print(id.Links.Descendants("link").Count().ToString());
            //    newTotal.Add(new XElement("segment",
            //            new XAttribute("id", id.Links.Name.LocalName),
            //                id.Links
            //        )
            //    );
            //}
            //newTotal.Add(new XElement("segment",
            //            new XAttribute("id", "_internaltags"),
            //                paralist
            //        )
            //    );
            //newTotal.Save(path + @"newSegment.xml");
            //DateTime end = DateTime.Now;
            //TimeSpan duration = end - start;
            //Debug.Print("Tid=" + duration.ToString());


        }

        private void btnLevel7_Click(object sender, EventArgs e)
        {
            string language = "no";
            string path = @"D:\_intext\";
            XElement total = XElement.Load(path + @"doc.xml");
            total.DescendantsAndSelf().Nodes().OfType<XText>().ToList().ForEach(p => p.ReplaceWith(new XText(Regex.Replace(Regex.Replace(p.Value, @"\r\n|\n", ""), @"\s+|\t", " "))));
            //total.DescendantsAndSelf().Nodes().OfType<XText>().ToList().ForEach(p=>Debug.Print(p.Value));

            Identificator id = new Identificator();
            id.Include = "level";
            id.Exclude = "title;xref;a;zref";
            id.BreakNodes = "level;text;p;table;tr;th;td;div;li";
            id.NoMarkupNodes = "title;xref;a;zref";
            id.Local = true;
            id.InternalReference = false;
            id.InternalTags = null;

            //XElement regexps = XElement.Load(@"D:\RegExp\InTextLinksRegexp_20130214.xml");
            //string regex = regexps.RegexBuild("100_total");
            //XElement actions = XElement.Load(@"d:\regexp\InTextLinksActions.xml");

            XElement regexps = XElement.Load(@"D:\RegExp\NOR\InTextLinksRegexp_NOR.xml");
            string regex = regexps.RegexBuild("100_total_no");
            XElement actions = XElement.Load(@"d:\regexp\NOR\InTextLinksActionsNOR.xml");

            DateTime start = DateTime.Now;
            XElement newTotal = id.ExecuteLinking(total, actions, regex, language);
            if (id.Links.HasElements)
            {
                Debug.Print(id.Links.Descendants("link").Count().ToString());
                //newTotal.Add(new XElement("segment",
                //        new XAttribute("id", id.Links.Name.LocalName),
                //            id.Links
                //    )
                //);
                id.Links.Save(path + @"diblinks.xml");
            }

            newTotal.Save(path + @"newtest.xml");

            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            Debug.Print("Tid=" + duration.ToString());
        }



    }
}
