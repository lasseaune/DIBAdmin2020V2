using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using TransformData.Global;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace TransformData
{
    public partial class frmReadUtgivelseXML : Form
    {
        private static class XI
        {
            public static XNamespace xi = "http://www.w3.org/2001/XInclude";
            public static XName inc = xi + "include";
        }

        private XElement _BOOK = null;
        
        public frmReadUtgivelseXML()
        {
            InitializeComponent();
        }

        private void btnFileName_Click(object sender, EventArgs e)
        {
            OpenFileDialog oF = new OpenFileDialog();
            oF.Filter = "XML filer (*.xml)| *.xml";
            oF.FilterIndex = 0;
            if (oF.ShowDialog() == DialogResult.OK)
            {

                string fileName = oF.FileName;
                textBox1.Text = fileName;
                FileInfo fi = new FileInfo(fileName);
                string path = fi.DirectoryName;
                try
                {
                    _BOOK = new XElement("bok");
                    XElement book = XElement.Load(fileName);

                    foreach (XElement include in book.Descendants("include"))
                    {
                        string href = (string)include.Attributes("href").FirstOrDefault();
                        if (href != null)
                        {
                            string name = (string)include.Attributes("name").FirstOrDefault();
                            XElement current = null;
                            if (name != null)
                            {
                                current = new XElement("Kapittel",
                                    new XElement("Tittel", name)
                                    );
                                _BOOK.Add(current);

                                XElement part = XElement.Load(path + @"\" + href);

                                foreach (XElement inc in part.Elements(XI.inc))
                                {
                                    string phref = (string)inc.Attributes("href").FirstOrDefault();
                                    if (phref != null)
                                    {
                                        try
                                        {
                                            XElement chapter = XElement.Load(path + @"\" + phref);
                                            current.Add(chapter);
                                        }
                                        catch (SystemException err)
                                        {
                                            MessageBox.Show("Feil ved åpning av '" + path + @"\" + phref + "'");
                                            continue;
                                        }
                                    }
                                }

                            }
                            else
                            {
                                try
                                {
                                    XElement part = XElement.Load(path + @"\" + href);
                                    _BOOK.Add(part);
                                }
                                catch (SystemException err)
                                {
                                    MessageBox.Show("Feil ved åpning av '" + path + @"\" + href + "'");
                                    continue;
                                }
                            }


                        }
            
                    }
                }
                catch (SystemException err)
                {
                }

            }
        }

        private void btnSaveXML_Click(object sender, EventArgs e)
        {
            if (_BOOK != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "XML filer (*.xml)| *.xml";
                sfd.FilterIndex = 0;

                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    _BOOK.Save(sfd.FileName);
            }
        }

        private void TransformBok()
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((_BOOK != null ? _BOOK.Name.LocalName : "") == "bok")
            {
                XElement newBook = (XElement)_BOOK.GetElement();
                if (newBook != null)
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "XML filer (*.xml)| *.xml";
                    sfd.FilterIndex = 0;

                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        
                        List<XElement> sections = newBook.Descendants("level").ToList();
                        foreach (XElement section in sections)
                        {
                            if (section.Nodes().Count() == 0)
                                section.Remove();
                            else
                            {
                                XElement text = new XElement("text",
                                    section.Elements().Where(p =>
                                        p.Name != "title"
                                        && p.Name != "level")

                                    );
                                section.Elements().Where(p =>
                                        p.Name != "title"
                                        && p.Name != "level").Remove();
                                section.Element("title").AddAfterSelf(text);
                            }
                        }
                        newBook.Save(sfd.FileName);
                    }
                
                }

            }
        }

        private void btnKommentarutg_Click(object sender, EventArgs e)
        {

            OpenFileDialog oF = new OpenFileDialog();
            oF.Filter = "XML filer (*.xml)| *.xml";
            oF.FilterIndex = 0;
            if (oF.ShowDialog() == DialogResult.OK)
            {
                XElement root = XElement.Load(oF.FileName);

                XElement komm = new XElement("root"
                    , new XElement("document",
                        new XAttribute("doctypeid", "7")
                        )
                    );

                XElement kommAsl = null;
                //XElement kommAsl = new XElement("root"
                //    , new XElement("document",
                //        new XAttribute("doctypeid", "7")
                //        )
                //    );
                XElement kommAsal = null;
                //XElement kommAsal = new XElement("root"
                //, new XElement("document",
                //    new XAttribute("doctypeid", "7")
                //    )
                //);


                bokExtentions.ExportSettings settings = new bokExtentions.ExportSettings();
                settings.teaser = 70;
                settings.LeftW = "w40";
                settings.RightW = "w60";
                settings.sectionName = "level";
                

                foreach (XElement level in root.Elements("level"))
                {
                    if (level.Element("title").Value == "Aksjeloven")
                    {
                        GetLevel(level, ref komm, ref kommAsl, settings);
                    }
                    else if (level.Element("title").Value == "Allmennaksjeloven")
                    {
                        GetLevel(level, ref komm, ref kommAsal, settings);
                    }
                    else
                        komm.Add(level);
                    
                }

                komm.Add(kommAsl);
                komm.Add(kommAsal);

                if (komm != null)
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "XML filer (*.xml)| *.xml";
                    sfd.FilterIndex = 0;

                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        komm.Save(sfd.FileName);
                    }
                }
            }

        }
        private void GetLevel(XElement level, ref XElement komm, ref XElement kommdoc,bokExtentions.ExportSettings settings)
        {
            string section = "level";
            komm.Add(new XElement(section,
                level.Element("title"),
                level.Element("text"),
                level.GetLevels(settings)
                ));
            
            kommdoc = new XElement(section,
                new XElement("title", "Kommentarer til " +  level.Element("title").Value),
                new XElement("text"),
                level.GetKommentars(settings)
                );
            
        }
    }
    static class bokExtentions
    {
        
        public class ExportSettings
        {
            public string referance_topic_id { get; set; }
            public int teaser;
            public string sectionName = "level";
            public string LeftW = "w50";
            public string RightW = "w50";
            public string TableClass = "tbCommentTable tbNoBorder";
        }

        public static object GetNoterKomm(this XElement noter, string leddno, ExportSettings settings)
        {
            if (noter == null) return null;
            string section = "level";
            XElement container = new XElement("container");
            foreach (XElement note in noter
                            .Descendants(section)
                            .Where(p =>
                                Regex.Match((p.Element("title") == null ? "" : p.Element("title").Value).Trim(), @"^(?<leddno>(\d+))((\.\d+)+)?(\.)?(\s|$)").Groups["leddno"].Value==leddno))
            {
                string noteText = note.Element("title").Value + " " + note.Element("text").GetElementText();
                if (noteText.Length > settings.teaser)
                {
                    noteText = noteText.Substring(0, settings.teaser) + "...";
                }

                XElement div = new XElement("div",
                    new XAttribute("data-zbm", note.Attribute("id").Value),
                    settings.referance_topic_id == null ? null : new XAttribute("data-ztid", settings.referance_topic_id),
                    new XAttribute("class", "zref"),
                    new XText(noteText)
                    );
                container.Add(div);
                //<div data-zbm="100222" data-ztid="{111222333}" class="zref">1.0 Kommentar....</div>
            }

            return container.Nodes();
        }

        public static object GetParagrafText(this XElement level, XElement data, ExportSettings settings)
        {
            string section = "level";
            
            XElement paraContent = new XElement("table",
                                    new XAttribute("class", settings.TableClass),
                                    new XElement("tbody"));
            XElement ledd = new XElement("ledd",
                new XAttribute("leddno","0")
                ); 
            string leddno = "";
            foreach( XElement el in level.Element("text").Elements())
            {
                if ((el.Attribute("class") == null ? "" : el.Attribute("class").Value)=="ledd")
                {
                    
                    if (ledd != null) 
                    {
                        //legg til ledd
                        leddno = ledd.Attribute("leddno").Value;
                        paraContent.Element("tbody").Add(new XElement("tr",
                                            new XElement("td",
                                                new XAttribute("class", settings.LeftW),
                                                   ledd.Nodes()
                                            ),
                                            new XElement("td",
                                                new XAttribute("class", settings.RightW),
                                                    data.GetNoterKomm(leddno,settings)
                                            )
                                        ));
                        
                    }
                    leddno = el.Attribute("leddno").Value;
                    ledd = new XElement("ledd",
                        new XAttribute("leddno",leddno.ToString()),
                        el
                        ); 
                }
                else 
                    ledd.Add(el);
            }

            //legg til ledd
            leddno = ledd.Attribute("leddno").Value;
            paraContent.Element("tbody").Add(new XElement("tr",
                                new XElement("td",
                                    new XAttribute("class", settings.LeftW),
                                        ledd.Nodes()
                                ),
                                new XElement("td",
                                    new XAttribute("class", settings.RightW),
                                        data.GetNoterKomm(leddno,settings)
                                )
                            ));

            XElement rettskilder = (XElement)level.Elements(section).Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "rettskilder").FirstOrDefault();
            XElement rk = new XElement("container");
            if (rettskilder != null)
            {
                rk.Add(new XElement("p",
                    new XElement("strong",
                        rettskilder.Element("title").Nodes()
                        )));
                rk.Add(rettskilder.Element("text").Nodes());
            }
            XElement text = new XElement("text",
                                paraContent,
                                rk.Nodes()
                            );

            XElement newLevel = new XElement(section,
                level.Attribute("id"),
                level.Element("title"),
                text,
                level.GetLevels(settings)
                );
            return newLevel;
        }
        public static object GetNoterInnl(this XElement noter, ExportSettings settings)
        {
            if (noter == null) return null;
            string section = "level";
            XElement container = new XElement("container");
            foreach (XElement note in noter
                            .Descendants(section)
                            .Where(p =>
                                Regex.IsMatch((p.Element("title") == null ? "" : p.Element("title").Value).Trim(), @"^\d+((\.\d+)+)?(\.)?(\s|$)")))
            {
                string noteText = note.Element("title").Value + " " + note.Element("text").GetElementText();
                if (noteText.Length > settings.teaser)
                {
                    noteText = noteText.Substring(0, settings.teaser) + "...";
                }

                XElement div = new XElement("div",
                    new XAttribute("data-zbm", note.Attribute("id").Value),
                    settings.referance_topic_id == null ? null : new XAttribute("data-ztid", settings.referance_topic_id),
                    new XAttribute("class", "zref"),
                    new XText(noteText)
                    );
                container.Add(div);
                //<div data-zbm="100222" data-ztid="{111222333}" class="zref">1.0 Kommentar....</div>
            }

            return container.Nodes();
        }

        public static object GetUnderkapittelText(this XElement level, XElement data, ExportSettings settings)
        {
            string section = "level";
            XElement text = new XElement("text",
                                new XElement("table",
                                    new XAttribute("class", settings.TableClass),
                                    new XElement("tbody",
                                        new XElement("tr",
                                            new XElement("td",
                                                new XAttribute("class",settings.LeftW),
                                                   level.Element("text").Nodes()
                                            ),
                                            new XElement("td",
                                                new XAttribute("class",settings.RightW),
                                              data.GetNoterInnl(settings)
                                            )
                                        )
                                    )
                                )
                            );
            if (text.Descendants("td").Where(p => p.Elements().Count() != 0).Count() == 0)
                text = new XElement("text");
            XElement newLevel = new XElement(section,
                level.Attribute("id"),
                level.Element("title"),
                text,
                level.GetLevels(settings)
                );
            return newLevel;
        }

        public static object GetKommentars(this XElement e, ExportSettings settings)
        {
            string section = settings.sectionName;
            XElement container = new XElement("container");
            foreach (XElement level in e.Elements(section))
            {
                string className = (string)level.Attributes("class").FirstOrDefault();
                if (className == null) continue;
                switch (className)
                {
                    case "Kapittel":
                    case "Underkapittel":
                        {
                            container.Add(new XElement(section,
                                        level.Element("title"),
                                        new XElement("text"),
                                        level.Elements(section).Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "innledning").FirstOrDefault(),
                                        level.GetKommentars(settings)
                                    )
                                );
                            
                            
                        }
                        break;
                    case "Paragraf":
                        {
                            container.Add(new XElement(section,
                                        level.Element("title"),
                                        new XElement("text"),
                                        level.Elements(section).Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "kommentarer").FirstOrDefault()
                                    )
                                );
                            
                        }
                        break;
                    default: continue;

                }
            }
            return container.Nodes();
        }

        public static object GetLevels(this XElement e, ExportSettings settings)
        {
            string section = "level";
            XElement container = new XElement("container");
            foreach (XElement level in e.Elements(section))
            {
                string className =  (string)level.Attributes("class").FirstOrDefault();
                if (className == null) continue;
                switch (className)
                {
                    case "Kapittel":
                    case "Underkapittel":
                        {
                           XElement innl = (XElement)level.Elements(section).Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "innledning").FirstOrDefault();
                           container.Add(level.GetUnderkapittelText(innl,settings)); 
                        }
                        break;
                    case "Paragraf":
                        {
                            XElement komm = (XElement)level.Elements(section).Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "kommentarer").FirstOrDefault();
                            container.Add(level.GetParagrafText(komm, settings));
                        }
                        break;
                    default: continue;

                }
            }
            return container.Nodes();
        }

        public static string GetAtt(this XElement e, string name)
        {
            return (string)e.Attributes(name).FirstOrDefault();
        }

        public static object GetNodes(this XElement e)
        {
            if (e == null) return null;
            XElement holder = new XElement("holder");
            foreach (XNode n in e.Nodes())
            {
                if (n.NodeType == XmlNodeType.Text)
                {
                    holder.Add(n);
                }
                else if (n.NodeType == XmlNodeType.Element)
                    holder.Add(((XElement)n).GetElement());
            }
            return holder.Nodes();
        }

        public static object GetNodesKommentar(this XElement e)
        {
            string section = "level";
            if (e == null) return null;
            XElement holder = new XElement("holder");
            XNode avsnitt = (XNode)e.Elements("Avsnitt").FirstOrDefault();
            
            XElement lastsection = null;
            while (avsnitt != null)
            {
                if (avsnitt.NodeType == XmlNodeType.Element)
                {
                    XElement test = (XElement)avsnitt;
                    XElement temp = new XElement("temp");
                    temp.Add(test.GetElement());
                    if (temp.Elements().First().Name.LocalName == section)
                    {
                        temp.Elements().First().Add(new XAttribute("pid", e.Ancestors("Paragraf").First().Attribute("Id").Value));
                        temp.Elements().First().Add(new XAttribute("pnr", e.Ancestors("Paragraf").First().Attribute("pNr").Value));
                        temp.Elements().First().Add(new XAttribute("id", "note_" 
                            + e.Ancestors("Kapittel").Last().Element("Tittel").Value.ToLower()
                            + "_" + e.Ancestors("Paragraf").First().Attribute("pNr").Value
                            + "_" + temp.Elements().First().Element("title").Value 
                            ));
                        holder.Add(temp.Nodes());
                        lastsection = holder.Elements(section).Last();
                    }
                    else if (lastsection != null)
                        lastsection.Add(temp.Nodes());
                    else
                        holder.Add(temp.Nodes());
                }
                else
                    Debug.Print("xxx");
                avsnitt = avsnitt.NextNode;
            }
            return holder.Nodes();
        }

        private static object GetFirstNumber(this XElement e)
        {
            string section = "level";
            XNode nfirst = e.DescendantNodes().OfType<XText>().Where(p => p.ToString() != "").FirstOrDefault();
            string first = nfirst == null ? "" : nfirst.ToString();
            string regexpA = "";
            switch (e.Name.LocalName)
            {
                case "Avsnitt": regexpA = @"^\d+((\.\d+)+)?(\.)?\s+"; break;
                case "Ledd": regexpA = @"^\(\d+\)\s+"; break;
                default:
                    Debug.Print("GetFirstNumber - Name: " + e.Name.LocalName);
                    return null;
            }


            if (nfirst != null && Regex.IsMatch(first.Trim(), regexpA) && regexpA != "" & e.Ancestors("UnummerertListe").Count() == 0 && e.Ancestors("NummerertListe").Count()==0)
            {
                string second = Regex.Replace(first, regexpA, "");
                first = Regex.Match(first, regexpA).Value;
                nfirst.ReplaceWith(new XText(second.TrimStart()));
                XElement holder = new XElement("holder");
                if (e.Parent.Name == "Kommentarer")
                {
                    holder = new XElement("holder",
                        new XElement(section,
                            new XElement("title", first.Trim()),
                            new XElement("p", e.GetNodes())
                            )
                    );

                }
                else
                {

                    holder = new XElement("holder",
                        new XElement("p", first.Trim()),
                        new XElement("p", e.GetNodes())
                    );
                }
                return holder.Nodes();
            }
            else
                if ((e.Attribute("Sitat") == null ? "" : e.Attribute("Sitat").Value) == "Ja")
                {
                    return new XElement("blockquote", e.GetNodes());
                }
                else
                    return new XElement("p", e.GetNodes());

        }

        private static object GetUth(this XElement e)
        {
            switch ((string)e.Attributes("Type").FirstOrDefault())
            {
                case "Kursiv": return new XElement("i", e.GetNodes());
                default:
                    Debug.Print("GetUth - Name: " + e.Name.LocalName);
                    return null;
            }
        }

        private static string GetKommentarLedd(this string s)
        {
            switch (s.Trim().ToLower())
            {
                case "allment": return "0";
                case "innledning": return "0";
                case "første ledd": return "1";
                case "annet ledd": return "2";
                case "andre ledd": return "2";
                case "tredje ledd": return "3";
                case "fjerde ledd": return "4";
                case "femte ledd": return "5";
                case "sjette ledd": return "6";
                case "sjuende ledd": return "7";
                case "syvende ledd": return "7";
                case "åttende ledd": return "8";
                case "niende ledd": return "9";
                default:
                    {
                        Debug.Print(s);
                        return "0";
                    }

            }
        }
        private static void GetKommentar(this XElement e, XElement container)
        {
            string section = "level";
            XElement holder = new XElement("holder");

            holder.Add(new XElement(section,
                new XAttribute("lid", e.Element("Tittel").Value.GetKommentarLedd()),
                new XAttribute("pid", e.Ancestors("Paragraf").First().Attribute("Id").Value),
                new XAttribute("pnr", e.Ancestors("Paragraf").First().Attribute("pNr").Value),
                new XElement("title", e.Element("Tittel").Value),
                e.GetNodesKommentar()));
            container.Add(holder.Nodes());
        }
        private static object GetKommentarer(this XElement e)
        {
            
            XElement holder = new XElement("holder");
            e.GetKommentar(holder);
            XNode next = e.NextNode;
            while (next != null)
            {
                if (next.NodeType == XmlNodeType.Element)
                {
                    if (((XElement)next).Name.LocalName == "Kommentarer")
                    {
                        XElement el = (XElement)next;
                        el.GetKommentar(holder);
                        next = next.NextNode;
                    }
                    else
                    {
                        next = null;
                    }
                }
            }
            return holder.Nodes();
            
        }

        public static object GetSectionElements(this XElement e)
        {
            string section = "level";
            XElement kap = new XElement(section,
                    e.Attribute("Id") == null ? null : new XAttribute("id", e.Attribute("Id").Value),
                    new XElement("title", e.Element("Tittel").GetElementText())
                    );
            
            XElement lastSection = null;

            
            foreach (XElement el in e.Elements().Where(p=>
               p.Name.LocalName != "Tittel"
                ))
            {
                if (el.Name.LocalName == "Avsnitt")
                {
                    XElement avsnitt = new XElement("avsnitt");
                    avsnitt.Add(el.GetElement());
                    if (avsnitt.Elements().Count() == 2)
                    {
                        int niva = 0;
                        if (Regex.IsMatch(avsnitt.Elements().First().GetElementText().Trim(), @"^\d+\.\d+$"))
                            niva = 2;
                        else if (Regex.IsMatch(avsnitt.Elements().First().GetElementText().Trim(), @"^\d+\.\d+\.\d+$"))
                            niva = 3;

                        XElement newSection = new XElement(section,
                            new XAttribute("id", "note_" + el.Ancestors("Kapittel").Last().Element("Tittel").Value.ToLower() + "_" + avsnitt.Elements().First().GetElementText() ),
                            new XAttribute("niva", niva.ToString()),
                            new XElement("title", avsnitt.Elements().First().GetElementText()),
                            avsnitt.Elements().Last()
                            );

                        if (niva == 2)
                            kap.Add(newSection);
                        else if (niva == 3)
                            kap.Descendants(section).Where(p => (p.Attribute("niva") == null ? "" : p.Attribute("niva").Value) == "2").Last().Add(newSection);

                        lastSection = newSection;
                    }
                    else
                    {
                        if (lastSection == null)
                            kap.Add(avsnitt.Nodes());
                        else
                            lastSection.Add(avsnitt.Nodes());
                    }
                }
                else
                {
                    if (lastSection == null)
                        kap.Add(el.GetElement());
                    else
                        lastSection.Add(el.GetElement());
                }
                
            }

            return kap;
        }
        
        public static object GetKapittelElements(this XElement e)
        {
            string section = "level";
            XElement kap =  new XElement(section,
                    new XAttribute("class",e.Name.LocalName),
                    e.Attribute("Id") == null ? null : new XAttribute("id", e.Attribute("Id").Value),
                    new XElement("title", e.Element("Tittel").GetElementText())
                    );
            
            XElement lastSection = null;
            XElement innledning = null;
            XElement ntitle = null;
            foreach (XElement el in e.Elements().Where(p=>
                p.Name.LocalName != "Underkapittel"
                && p.Name.LocalName != "Paragraf"
                && p.Name.LocalName != "Rettskilder"
                && p.Name.LocalName != "Kommentarer"
                && p.Name.LocalName != "Tittel"
                ))
            {
                if (el.Name.LocalName == "Mtit")
                {
                    if (el.Value.Trim().ToLower() == "innledning")
                    {
                        lastSection = new XElement(section,
                            new XAttribute("pid", el.Ancestors().Where(p =>
                                p.Name.LocalName == "Underkapittel"
                                || p.Name.LocalName == "Kapittel").First().Attribute("Id").Value),
                            new XAttribute("class", "innledning"),
                            new XElement("title", el.GetElementText()));
                        innledning = lastSection;
                        kap.Add(lastSection);
                    }
                    else if (Regex.IsMatch(el.Value.TrimStart(), @"^\d+((\.\d+)+)?\s"))
                    {
                        XElement newSection = new XElement(section,
                            new XElement("title", el.GetElementText()));

                        if (innledning != null)
                            innledning.Add(newSection);
                        else
                            kap.Add(newSection);

                        ntitle = newSection;
                        lastSection = newSection;
                    }
                    else if (lastSection == null)
                        kap.Add(el.GetElement());
                    else
                        lastSection.Add(el.GetElement());
                }
                else if (el.Name.LocalName == "Avsnitt")
                {
                    XElement avsnitt = new XElement("avsnitt");
                    avsnitt.Add(el.GetElement());
                    if (avsnitt.Elements().Count() == 2)
                    {
                        XElement newSection = new XElement(section,
                            new XElement("title", avsnitt.Elements().First().GetElementText()),
                            avsnitt.Elements().Last()
                            );
                        if (ntitle == null && innledning == null)
                            kap.Add(newSection);
                        else if (ntitle != null)
                            ntitle.Add(newSection);
                        else if (innledning != null)
                            innledning.Add(newSection);

                        lastSection = newSection;
                    }
                    else
                    {
                        if (lastSection == null)
                            kap.Add(avsnitt.Nodes());
                        else
                            lastSection.Add(avsnitt.Nodes());
                    }
                }
                else
                {
                    if (lastSection == null)
                        kap.Add(el.GetElement());
                    else
                        lastSection.Add(el.GetElement());
                }

            }

            foreach (XElement el in e.Elements().Where(p =>
                p.Name.LocalName == "Paragraf"
                ))
            {
                kap.Add(el.GetElement());
            }

            foreach (XElement el in e.Elements().Where(p =>
                            p.Name.LocalName == "Underkapittel"
                            ))
            {
                kap.Add(el.GetElement());
            }

            return kap;
        }
        public static object GetPkts(this XElement e)
        {
            XElement list = new XElement("liste");
            XElement lastLi = null;
            foreach (XElement pkt in e.Elements("Pkt"))
            {
                XElement li = (XElement)pkt.GetElement();
                if (lastLi != null)
                {
                    if (Regex.IsMatch(li.GetElementText().TrimStart(), @"^\-\d+")
                        && !Regex.IsMatch(lastLi.GetElementText().TrimEnd(), @"\;$"))
                    {
                        lastLi.Elements("p").Last().Add(li.GetElementText());
                    }
                    else
                    {
                        list.Add(li);
                        lastLi = li;
                    }

                }
                else
                {
                    list.Add(li);
                    lastLi = li;
                }   
            }
            return list.Nodes();
        }

        public static object GetElement(this XElement e)
        {
            string section = "level";
            switch(e.Name.LocalName)
            {
                case "bok": return new XElement("root",
                    new XElement("document",
                        new XAttribute("doctypeid","7")),
                    e.GetNodes());
                case "Uth": return e.GetUth();
                case "Mtit": return new XElement("p",e.GetNodes());
                case "Avsnitt": return e.GetFirstNumber();

                case "Seksjon":
                    if (e.Element("Tittel") == null)
                        return e.GetNodes();
                    else
                        return e.GetSectionElements();
                        //return new XElement(section,
                        //        e.GetNodes()
                        //    );
                case "Underkapittel":
                case "Kapittel":
                    return e.GetKapittelElements();
                case "Tittel": return new XElement("title", e.GetNodes());
                case "Paragraf": return new XElement(section,
                    new XAttribute("class", e.Name.LocalName),
                    new XAttribute("pnr", e.Attribute("pNr").Value),
                    new XAttribute("id", e.Attribute("Id").Value),
                     e.GetNodes()
                     );
                case "Ledd":
                    {
                        string leddno = Regex.Match(e.GetElementText().Trim(), @"^\((?<leddno>(\d+))\)").Groups["leddno"].Value;
                        return new XElement("div",
                         leddno == "" ? null : new XAttribute("leddno", leddno),
                        new XAttribute("id", e.Attribute("Id").Value),
                        leddno == "" ? null : new XAttribute("class", "ledd"),
                        e.GetFirstNumber());
                    }
                case "Litra": return new XElement("div",
                    new XAttribute("class", "litra"),
                    new XElement("li", e.GetNodes()));
                case "Rettskilder": return new XElement(section,
                        new XElement("title", "Rettskilder"),
                        new XAttribute("class", "rettskilder"),
                        e.GetNodes()
                    );
                case "Kommentarer":
                    if (e == e.Parent.Elements("Kommentarer").First())
                    {
                        return new XElement(section,
                            new XElement("title", "Kommentarer"),
                            new XAttribute("class", "kommentarer"),
                            e.GetKommentarer());
                    }
                    return null;
                case "Forkortelser": return new XElement(section,
                            new XElement("title", e.Element("Tittel").GetElement()),
                            new XElement("table", 
                                new XElement("tr",
                                    new XElement("th", "Term"),
                                    new XElement("th", "Definisjon")
                                    ), 
                                e.GetElements("Forkortelse")
                            )
                        );
                case "Forkortelse": return new XElement("tr", e.GetNodes());
                case "Term":
                case "Definisjon": return new XElement("td", e.GetNodes());
                case "Forarbeider": return e.GetNodes();
                case "Lovendring": return e.GetNodes();
                case "UnummerertListe": return new XElement("ul",
                    new XAttribute("style", "list-style:none;"),
                    e.GetPkts());
                case "Pkt": return new XElement("li", e.GetNodes());
                case "NummerertListe": return new XElement("ol",
                        new XAttribute("style", "list-style:none;"),
                        e.GetPkts());
                default:
                    Debug.Print("GetElement - Name: " + e.Name.LocalName);
                    return null;
            }

        }
        public static object GetElements(this XElement element, string name)
        {
            if (element == null) return null;
            XElement returnValue = new XElement("temp");
            foreach (XElement e in element.Elements(name))
            {
                returnValue.Add(e.GetElement());
            }
            return returnValue.Nodes();
        }

    }

}
