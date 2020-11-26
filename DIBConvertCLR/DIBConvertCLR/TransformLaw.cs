using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace DIBConvertCLR
{
    public static class LawTransform
    {
        private class MoreinfoItem
        {
            public bool hasRelations { get; set; }
        }
        public static XElement TransformLaw(this XElement dokument)
        {
            dokument.DescendantsAndSelf().Attributes("idx").Remove();
            XElement t = dokument.Elements("tekst").FirstOrDefault();
            XElement m = dokument.Elements("metadata").FirstOrDefault();
            XElement c = new XElement("document",
                t.Tekst(m)
            );
            return c;
        }
        private static IEnumerable<XNode> Transform(this XNode n)
        {
            List<XNode> result = new List<XNode>();
            try
            {
                if (n.NodeType == XmlNodeType.Text) result.Add(n);
                if (n.NodeType == XmlNodeType.Element)
                {
                    XElement e = ((XElement)n);
                    switch (e.Name.LocalName)
                    {

                        case "diblink":
                        case "dibparameter": result.AddRange(e.Diblink()); break;
                        //Do nothing
                        case "tit":
                        case "fref":
                        case "aar":
                        case "omraadekode":
                        case "tittel":
                        case "korttittel":
                        case "metadata":
                        case "thgroup":
                        case "journalnummer":
                        case "hjemmel":
                        case "gjelder":
                        case "etat":
                        case "naeringskode":
                        case "systematiskkode":
                        case "base":
                        case "ktittel":
                        case "kverdi":
                        case "ptittel":
                        case "pverdi":
                        case "lverdi":
                            break;
                        //Apply metadata
                        case "id":
                        case "lastmodified":
                        case "lastupdate":
                            break;
                        case "rettet": result.AddRange(e.CreateMetadataRow("Sist endret", false)); break;
                        case "endrer": result.AddRange(e.CreateMetadataRow("Sist endret", true)); break;
                        case "endret": result.AddRange(e.CreateMetadataRow("Sist endret", false)); break;
                        case "departement": result.AddRange(e.CreateMetadataRow("Departement", false)); break;
                        case "ikraft": result.AddRange(e.CreateMetadataRow("Ikraft", true)); break;
                        case "publisert": result.AddRange(e.CreateMetadataRow("Publisert", false)); break;
                        case "dato": result.AddRange(e.CreateMetadataRow("Dato", true)); break;
                        case "type": result.AddRange(e.CreateMetadataRow("Type", false)); break;
                        //Apply template
                        case "dokument":
                            result.AddRange(e.dokument()); break;
                        //Apply Section
                        case "tekst":
                        case "vedlegg":
                        case "traktat":
                        case "kapittel":
                        case "paragraf":
                        case "artikkel":
                        case "itraktat":
                            //if ((string)e.Attributes("id").FirstOrDefault() =="p2-6")
                            //{

                            //}
                            result.AddRange(e.Section()); break;
                        //Apply transformation
                        case "ledd":
                            result.AddRange(e.Ledd()); break;
                        case "fotnote":
                            result.AddRange(e.Fotnote()); break;
                        case "marg":
                            result.AddRange(e.Marg()); break;
                        case "center":
                            result.AddRange(e.Center()); break;
                        case "lenke":
                            result.AddRange(e.Lenke()); break;
                        case "img":
                            result.AddRange(e.Img()); break;
                        case "tab":
                            result.AddRange(e.Tab()); break;
                        case "b":
                            result.AddRange(e.B()); break;
                        case "i":
                            result.AddRange(e.I()); break;
                        case "liste":
                            {
                                string type = (string)e.Attributes("type").FirstOrDefault();
                                XNode nbefore = e.NodesBeforeSelf().LastOrDefault();
                                XNode nafter = e.NodesAfterSelf().FirstOrDefault();

                                string before = nbefore.ListNodeType();
                                string after = nafter.ListNodeType();

                                if (e.Nodes().Count() == 0)
                                {
                                    result.Add(new XElement("br")); break;
                                }
                                else if ((before != null && after == null) && ((before.Split(';').ElementAt(0) != "liste") || ((before.Split(';').ElementAt(0) == "liste" && before.Split(';').ElementAt(2) == "0"))))
                                {
                                    result.AddRange(e.Liste()); break;
                                }
                                else if ((after != null && before == null) && (after.Split(';').ElementAt(0) == "liste" && after.Split(';').ElementAt(1) == type))
                                {
                                    result.AddRange(e.Liste()); break;
                                }
                                else if (before.Split(';').ElementAt(0) != "liste" && after.Split(';').ElementAt(0) != "liste")
                                {
                                    result.AddRange(e.SingleListe()); break;
                                }
                                else if (
                                    ((before.Split(';').ElementAt(0) != "liste") || ((before.Split(';').ElementAt(0) == "liste" && before.Split(';').ElementAt(2) == "0")))
                                    && (after.Split(';').ElementAt(0) == "liste" && after.Split(';').ElementAt(1) == type)
                                )
                                {
                                    result.AddRange(e.Liste()); break;
                                }
                            }
                            break;
                        case "avsnitt":
                            result.AddRange(e.Avsnitt()); break;
                        case "punktum":
                            result.AddRange(e.Punktum()); break;
                        case "historisk_note":
                            result.AddRange(e.HistoriskNote()); break;
                        //Apply Table transformation
                        case "tbody": result.Add(e.TBody()); break;
                        case "table": result.AddRange(e.Table()); break;
                        case "row": result.AddRange(e.Row()); break;
                        case "entry": result.AddRange(e.Entry()); break;
                        case "tgroup": result.AddRange(e.TGroup()); break;
                        case "ref": result.AddRange(e.Ref()); break;
                        default: //br, sub, sup
                            result.AddRange(e.Default()); break;
                    }
                }
                return result;
            }
            catch (Exception e)
            {

                result.Add(new XElement("p", new XAttribute("type", "error") , "Transformation error:" + e.Message));
                return result;
            }
        }
        #region //ElementStyle
        private static IEnumerable<XNode> dokument(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            XElement tekst = e.Elements("tekst").FirstOrDefault();
            result.AddRange(tekst.Transform());            
            return result;
        }
        private static string ListNodeType(this XNode n)
        {
            if (n == null) return null;
            if (n.NodeType == XmlNodeType.Text)
            {
                return "text();;";
            }
            else if (n.NodeType == XmlNodeType.Element)
            {
                XElement e = (XElement)n;
                return e.Name.LocalName + ";" + ((string)e.Attributes("type").FirstOrDefault() ?? "") + ";" + e.Nodes().Count().ToString();
            }
            else
            {
                return "none;;";
            }
        }
        private static IEnumerable<XNode> Ref(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if ("hjemmel;rettet".Split(';').Contains(e.Parent.Name.LocalName))
                return result;

            if (((string)e.Attributes("id").FirstOrDefault()??"").Trim()!="")
            {
                result.Add(new XElement("span",
                    new XAttribute("class", "lovdataref"),
                    new XAttribute("data-lovdataid", (string)e.Attributes("id").FirstOrDefault()),
                    e.Nodes().SelectMany(p => p.Transform())
                    )
                );

            }
            else
            {
                result.AddRange(e.Nodes().SelectMany(p => p.Transform()));
            }
            return result;
        }
        private static IEnumerable<XNode> HistoriskNote(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("section",
                   e.Attributes("id"),
                   new XAttribute("class", "lovdata-historisk-note"),
                   e.Nodes().SelectMany(p => p.Transform())
                )
            );
            return result;
        }
        private static IEnumerable<XNode> Punktum(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("span",
                e.Attributes("id"),
                new XAttribute("class", "lovdata_punktum"),
                e.Nodes().SelectMany(p => p.Transform())
                )
            );
            return result;
        }
        private static IEnumerable<XNode> Avsnitt(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if ("liste;entry".Split(';').Contains(e.Parent.Name.LocalName) && e.Parent.Elements("avsnitt").Count() == 1)
            {
                result.AddRange(e.Nodes().SelectMany(p => p.Transform()));
            }
            else
            {
                result.Add(new XElement("p",
                    e.Nodes().SelectMany(p => p.Transform())
                    )
                );
            }
            return result;
        }

        private static IEnumerable<XNode> SingleListe(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("table",
                new XAttribute("class", "tbNoBorder"),
                     new XElement("tr",
                        new XElement("td", e.Elements("lverdi").FirstOrDefault().ElementText()),
                        new XElement("td", e.Nodes().SelectMany(p => p.Transform()))
                     )
                )
            );
            return result;
        }
        private static IEnumerable<XNode> Liste(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            try
            {

                string type = (string)e.Attributes("type").FirstOrDefault() ?? "";
                string elementName = "ol";
                string listeType = "1";
                switch (type)
                {
                    case "decimal": listeType = "1"; break;
                    case "upperalfa": listeType = "A"; break;
                    case "lowerralfa": listeType = "a"; break;
                    case "lowerroman": listeType = "i"; break;
                    case "upperroman": listeType = "I"; break;
                    case "strek":
                        listeType = "disc";
                        elementName = "ul";
                        break;
                    case "":
                        listeType = "";
                        elementName = "p";
                        break;
                }
                if (elementName == "ol" || elementName == "ul")
                {
                    result.Add(new XElement(elementName,
                        listeType == "" ? null : new XAttribute("type", listeType),
                        e.ListeItem(),
                        e.NodesAfterSelf()
                            .OfType<XElement>()
                            .TakeWhile(p => 
                                p.Name.LocalName == "liste" 
                                && (string)p.Attributes("type").FirstOrDefault() == type
                            )
                            .Select(p => p.ListeItem())
                        )
                    );
                }
                else
                {
                    result.AddRange(e.SingleListe());
                    result.AddRange(e.NodesAfterSelf().OfType<XElement>().TakeWhile(p => p.Name.LocalName == "liste" && (string)p.Attributes("type").FirstOrDefault() == type).SelectMany(p => p.SingleListe()));
                }
                return result;
            }
            catch (Exception err)
            {
                result.Add(new XElement("p", "Error: " + err.Message));
                return result;
            }


        }
        private static IEnumerable<XNode> ListeItem(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("li",
                    e.Attributes("id"),
                    e.Attributes("value").FirstOrDefault() == null ? null : new XAttribute("value", (string)e.Attributes("value").FirstOrDefault()),
                    new XAttribute("class", "lovlist"),
                   e.Nodes().SelectMany(p => p.Transform())
               )
            );
            return result;
        }
        private static IEnumerable<XNode> I(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (e.Parent.Name.LocalName == "ledd")
            {
                result.Add(new XElement("p",
                    e.Attributes("id"),
                    new XElement("em",
                           e.Nodes().SelectMany(p => p.Transform())
                           )
                    )
                );

            }
            else
            {
                result.Add(new XElement("em",
                        e.Attributes("id"),
                        e.Nodes().SelectMany(p => p.Transform())
                        )
                    );

            }
            return result;
        }
        private static IEnumerable<XNode> B(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (e.Parent.Name.LocalName == "ledd")
            {
                result.Add(new XElement("p",
                    e.Attributes("id"),
                    new XElement("strong",
                       e.Nodes().SelectMany(p => p.Transform())
                       )
                    )
                );
            }
            else
            {

                
                    result.Add(new XElement("strong",
                       e.Attributes("id"),
                       e.Nodes().SelectMany(p => p.Transform())
                       )
                   );
                
            }
            return result;
        }
        private static IEnumerable<XNode> Tab(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (e.Parent.Elements("tab").Count() == 1 && e.Parent.Nodes().LastOrDefault() == e)
            {
                result.Add(new XElement("span",
                    e.Attributes("id"),
                    new XAttribute("class", "lovdata-tab-last"),
                    e.Nodes().SelectMany(p => p.Transform())
                    )
                );
            }
            else
            {
                result.Add(new XElement("span",
                       e.Attributes("id"),
                       new XAttribute("class", "lovdata-tab"),
                       new XAttribute("data-lovdata-tab-indent", (string)e.Attributes("niv").FirstOrDefault() ?? ""),
                       e.Nodes().SelectMany(p => p.Transform())
                       )
                   );
            }
            return result;
        }
        private static IEnumerable<XNode> Img(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("img",
                   e.Attributes("id"),
                   new XAttribute("src", (string)e.Attributes("src").FirstOrDefault()),
                   new XAttribute("alt", (string)e.Attributes("alt").FirstOrDefault()),
                   e.Nodes().SelectMany(p => p.Transform())
                   )
               );
            return result;
        }
        private static IEnumerable<XNode> Lenke(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("div",
                   e.Attributes("id"),
                   new XAttribute("href", (string)e.Attributes("url").FirstOrDefault()),
                   new XAttribute("target", "_blank"),
                   e.Nodes().SelectMany(p => p.Transform())
                   )
               );
            return result;

        }
        private static IEnumerable<XNode> Center(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("span",
                e.Attributes("id"),
                new XAttribute("class", "lovdata-center"),
                new XAttribute("class", "lovdata-center"),
                e.Nodes().SelectMany(p => p.Transform())
                )
            );
            return result;

        }

        private static IEnumerable<XNode> Marg(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (e.Elements("liste").FirstOrDefault() != null)
            {
                result.AddRange(e.Nodes().SelectMany(p => p.Transform()));
            }
            else if (
                (e.Elements().FirstOrDefault() == null
                ? ""
                : (
                    e.Elements().First().Name.LocalName == "b"
                    ? (
                        e.Elements().First().DescendantNodes().OfType<XText>().Select(s => s.Value).StringConcatenate().Trim().EndsWith(":")
                        ? "b"
                        : ""
                      )
                    : ""
                    )
                ) == "b"
                && e.Parent.Name.LocalName=="tekst"
            )
            {
                foreach (XElement b in e.Elements("b"))
                {
                        string title = b.DescendantNodes().OfType<XText>().Select(s => s.Value).StringConcatenate().Trim();
                        result.Add(new XElement("x-box",
                            b.Attributes("id"),
                            new XAttribute("class", "sample"),
                            new XElement("x-box-title", title),
                            b.NodesAfterSelf()
                            .TakeWhile(s => (s.NodeType == XmlNodeType.Element ? ((XElement)s).Name.LocalName : "") != "b")
                            .SelectMany(s => s.Transform())

                            )
                        );
                }
            }
            else
            {
                result.Add(new XElement("p",
                    e.Attributes("id"),
                    (string)e.Attributes("niv").FirstOrDefault() == "2" ? new XAttribute("class", "lovdata-indent") : null,
                    e.Nodes().SelectMany(p => p.Transform())
                    )
                );
            }
            return result;
        }
        private static IEnumerable<XNode> Fotnote(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            //result.Add(new XElement("div",
            result.Add(new XElement("p",
                e.Attributes("id"),
                new XAttribute("class", "lovfotnote"),
                e.Nodes().SelectMany(p => p.Transform())
                )
            );

            return result;
        }
        private static IEnumerable<XNode> Ledd(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            XElement between = e.NodesAfterSelf().OfType<XElement>().TakeWhile(p => !"fotnote;ledd;paragraf;kapittel".Split(';').Contains(p.Name.LocalName)).FirstOrDefault();

            
            while (between != null)
            {
                string bId = (string)between.Attributes("id").FirstOrDefault();
                if (bId == null)
                {
                    e.Add(between);
                    between.Remove();
                }
                else
                {
                    bId = bId.Replace("/" + bId.Split('/').LastOrDefault(), "");
                    XElement parent = e.Descendants().Where(p => (string)p.Attributes("id").FirstOrDefault() == bId).FirstOrDefault();
                    if (parent== null)
                    {
                        parent = e.Descendants().Where(p => p.Nodes().OfType<XText>().Count() > 0).LastOrDefault();
                        if (parent!=null)
                        {
                            parent.Add(between);
                            between.Remove();
                        }
                        else
                        {
                            e.Add(between);
                            between.Remove();
                        }
                    }
                    else
                    {
                        parent.Add(between);
                        between.Remove();
                    }
                }
                between = e.NodesAfterSelf().OfType<XElement>().TakeWhile(p => !"fotnote;ledd;paragraf;kapittel".Split(';').Contains(p.Name.LocalName)).FirstOrDefault();
            }
                

            result.Add(new XElement("section",
                e.Attributes("id"),
                new XAttribute("class", "lovdata-ledd"),
                e.Nodes().SelectMany(p => p.Transform())
                )
            );

            return result;
        }
        private static IEnumerable<XNode> Tekst(this XElement e, XElement metadata)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("section",
                new XAttribute("id", "_top"),
                (e.Elements("tittel").FirstOrDefault() != null 
                ? new XElement("p", 
                        new XElement("strong",
                            e.Elements("tittel").Nodes().SelectMany(p => p.Transform())
                        )
                    ) 
                 : null),
                (e.Elements("korttittel").FirstOrDefault() != null 
                ? new XElement("p",
                    new XElement("em",
                        e.Elements("korttittel").Nodes().SelectMany(p => p.Transform())
                        )
                    ) 
                : null),
                (metadata == null 
                 ? null 
                 : new XElement("dl", 
                        new XAttribute("class", "lovdata-metadata"), 
                        metadata.Nodes().SelectMany(p => p.Transform())
                    )
                ),
                e.Nodes().SelectMany(p => p.Transform())
                )
            );

            return result;
        }
        private static IEnumerable<XNode> CreateMetadataRow(this XElement e, string tittel, bool CheckType)
        {
            List<XNode> result = new List<XNode>();
            string type = (string)e.Attributes("type").FirstOrDefault() ?? "";
            if ((CheckType ? type : "") == "")
            {
                result.Add(new XElement("dt", tittel));
                result.Add(new XElement("dd", e.DescendantNodes().OfType<XText>().Select(s => s.Value).StringConcatenate()));
            }
            return result;
        }

        private static IEnumerable<XNode> Default(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (e.Name.LocalName == "br")
            {
                result.Add(new XElement(e.Name.LocalName.ToString()));
            }
            //else
            //{
            //    if (e.Nodes().Count() == 0) return result;
            //    result.Add(new XElement(e.Name.LocalName.ToString(),
            //        e.Attributes(),
            //        e.Nodes().SelectMany(p => p.Transform())
            //        )
            //    );
            //}
            return result;
        }

        private static IEnumerable<XNode> TelNev(this XElement e)
        {
            List<XNode> result = new List<XNode>();

            result.Add(new XElement("tr",
                new XElement("td",
                    e.Attributes("id"),
                    new XAttribute("class", e.Name.LocalName),
                    e.Nodes().SelectMany(p => p.Transform())
                    )
                )
            );
            return result;
        }
        private static IEnumerable<XNode> Brok(this XElement e)
        {
            List<XNode> result = new List<XNode>();

            result.Add(new XElement("table",
                new XAttribute("border", 0),
                e.Nodes().SelectMany(p => p.Transform())
                )
            );
            return result;
        }
        private static IEnumerable<XNode> Title(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            int n = e.Ancestors()
                .Where(p =>
                    p.Descendants("kapittel").FirstOrDefault() != null
                    && p.Name.LocalName != "forord"
                    && p.Name.LocalName != "glossar"
                )
                .Count();
            if (e.Nodes().Count() == 0) return null;
            result.Add(new XElement("h" + n.ToString(),
                e.Attributes("id"),
                ((string)e.Attributes("autonum").FirstOrDefault() ?? "") == "" ? null : new XText((string)e.Attributes("autonum").FirstOrDefault() + ". "),
                e.Nodes().SelectMany(p => p.Transform())
                )
            );
            return result;
        }
        private static IEnumerable<XNode> Section(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            MoreinfoItem moreinfo = e.Annotations<MoreinfoItem>().FirstOrDefault();
            int n = e.AncestorsAndSelf().TakeWhile(p => p.Name.LocalName != "dokument").Count();
            if (n > 6) n = 6;
            

            result.Add(new XElement("section",
                e.Attributes("id"),
                (e.Name.LocalName == "paragraf" || e.Name.LocalName == "artikkel") ? new XAttribute("class", "lovparagraf") : null,
                moreinfo == null ? null : new XAttribute("data-hasrelations", moreinfo.hasRelations),
                e.Name.LocalName == "vedlegg"
                ? (
                    (
                        (e.Elements().Where(p => "kverdi".Split(';').Contains(p.Name.LocalName)).Select(p => p.ElementText()).FirstOrDefault() ?? "").Trim()
                        + (e.Elements().Where(p => "tit;ktittel".Split(';').Contains(p.Name.LocalName)).Select(p => p.ElementText()).FirstOrDefault() ?? "").Trim()
                    ) == ""
                    ? new XElement("h" + n.ToString(), new XText("Vedlegg"))
                    : new XElement("h" + n.ToString(),
                            new XText(
                                (e.Elements().Where(p => "kverdi".Split(';').Contains(p.Name.LocalName)).Select(p => p.ElementText()).FirstOrDefault() ?? "").Trim()
                                + " "
                                + (e.Elements().Where(p => "tit;ktittel".Split(';').Contains(p.Name.LocalName)).Select(p => p.ElementText()).FirstOrDefault() ?? "").Trim()
                            )
                        )

                )
                : null
                , e.Name.LocalName == "traktat" || e.Name.LocalName == "itraktat"
                ? (
                    (
                        (e.Elements().Where(p => "kverdi".Split(';').Contains(p.Name.LocalName)).Select(p => p.ElementText()).FirstOrDefault() ?? "").Trim()
                        + (e.Elements().Where(p => "tit;ktittel".Split(';').Contains(p.Name.LocalName)).Select(p => p.ElementText()).FirstOrDefault() ?? "").Trim()
                    ) == ""
                    ? new XElement("h" + n.ToString(), new XText("Traktat"))
                    : new XElement("h" + n.ToString(),
                            new XText(
                                (e.Elements().Where(p => "kverdi".Split(';').Contains(p.Name.LocalName)).Select(p => p.ElementText()).FirstOrDefault() ?? "").Trim()
                                + " "
                                + (e.Elements().Where(p => "tit;ktittel".Split(';').Contains(p.Name.LocalName)).Select(p => p.ElementText()).FirstOrDefault() ?? "").Trim()
                            )
                        )
                    
                ) 
                : null,
                e.Name.LocalName == "kapittel" ? new XElement("h" + n.ToString(), (e.Elements("kverdi").FirstOrDefault().ElementText() + " " + e.Elements("ktittel").FirstOrDefault().ElementDescendantText()).Trim()) : null,
                (e.Name.LocalName == "paragraf" || e.Name.LocalName == "artikkel") ?
                    new XElement("h" + n.ToString(), (
                        e.Elements("pverdi").FirstOrDefault().ElementText()
                        + " "
                        + e.Elements("ptittel").FirstOrDefault().ElementDescendantText()).Trim())
                    : null,
                e.Nodes().SelectMany(p => p.Transform())
                )
            );
            return result;
        }
        #endregion
        #region //Table
        private class Span
        {
            public int start { get; set; }
            public int end { get; set; }
            public int iColSpan { get; set; }
            public string colspan { get; set; }
            public Span(int Start, int End)
            {
                start = Start;
                end = End;
                iColSpan = (end - start) + 1;
                colspan = iColSpan.ToString();
            }
        }
        private class Spans
        {
            public List<Span> spans = new List<Span>();
            public Spans(Span span)
            {
                spans.Add(span);
            }
        }
        private static void AddSpan(this XElement e, Span span)
        {
            if (e.Annotation<Spans>() == null)
                e.AddAnnotation(new Spans(span));
            else
            {
                if (!e.Annotation<Spans>().spans.Contains(span))
                {
                    e.Annotation<Spans>().spans.Add(span);
                }
            }
        }
        private static IEnumerable<XNode> TGroup(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (e.Elements("tbody").Count() != 0)
            {
                result.AddRange(e.Elements("tbody").FirstOrDefault().Transform());
            };
            return result;
        }

        private static IEnumerable<XNode> Table(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            string frame = (string)e.Attributes("frame").FirstOrDefault() ?? "";
            if (frame == "all") frame = "1";

            result.Add(new XElement("table",
                frame == "1" ? new XAttribute("border", frame) : null,
                e.Nodes().SelectMany(p => p.Transform())
                )
            );
            return result;
        }
        private static XNode TBody(this XElement e)
        {
            Regex IsInt = new Regex(@"\d+");
            XNode result = null;
            XElement tgroup = e.Ancestors().Where(p => "thgroup;tgroup".Split(';').Contains(p.Name.LocalName) && p.Elements("spanspec").FirstOrDefault() != null).LastOrDefault();
            if (tgroup != null)
            {
                string bodyName = "tbody";
                string entryName = "td";
                switch (tgroup.Name.LocalName)
                {
                    case "thgroup":
                        bodyName = "thead";
                        entryName = "th";
                        break;
                }
                string scols = (string)tgroup.Attributes("cols").LastOrDefault();
                int cols = IsInt.IsMatch(scols.Trim()) ? Convert.ToInt32(scols.Trim()) : tgroup.Elements("colspec").Count();

                XElement tbody = new XElement(bodyName);

                foreach (XElement row in e.Elements("row"))
                {
                    Spans spans_before = row.Annotation<Spans>();

                    XElement tr = new XElement("tr");
                    int rowCol = row.Elements("entry").Where(p => p.DescendantNodes().OfType<XText>().Select(t => t.ToString()).StringConcatenate().Trim() != "").Count();
                    if (rowCol == cols)
                    {

                    }
                    int currentCol = 0;
                    foreach (XElement entry in row.Elements("entry"))
                    {
                        currentCol++;
                        if (spans_before != null)
                        {
                            Span sb = spans_before.spans.Where(p => p.start == currentCol).FirstOrDefault();
                            while (sb != null)
                            {
                                currentCol = sb.end + 1;
                                sb = spans_before.spans.Where(p => p.start == currentCol).FirstOrDefault();
                            }
                        }
                        List<XElement> same_spanname = new List<XElement>();
                        string namest = "";
                        string nameend = "";
                        string spanname = (string)entry.Attributes("spanname").FirstOrDefault();
                        if (spanname != null)
                        {
                            XElement spanspec = tgroup.Elements("spanspec").Where(p => (string)p.Attributes("spanname").FirstOrDefault() == spanname).FirstOrDefault();
                            if (spanspec != null)
                            {
                                namest = (string)spanspec.Attributes("namest").FirstOrDefault();
                                nameend = (string)spanspec.Attributes("nameend").FirstOrDefault();
                            }

                            same_spanname = row.NodesAfterSelf().OfType<XElement>()
                                .TakeWhile(p =>
                                    p.Name.LocalName == "row"
                                    && p.Elements("entry")
                                        .Where(s =>
                                            (string)s.Attributes("spanname").FirstOrDefault() == spanname
                                            && s.DescendantNodes().OfType<XText>().Select(t => t.ToString()).StringConcatenate().Trim() == ""
                                        ).Count() != 0
                                ).ToList();
                        }

                        int iNameSt = IsInt.IsMatch(namest) ? Convert.ToInt32(namest) : currentCol;
                        int iNameEnd = IsInt.IsMatch(namest) ? Convert.ToInt32(nameend) : currentCol;
                        if (iNameEnd != 0 && iNameSt != 0 && iNameSt > iNameEnd)
                        {
                            int start = iNameEnd;
                            iNameEnd = iNameSt;
                            iNameSt = iNameEnd;
                        }

                        else if (iNameSt != 0 && iNameEnd == 0)
                        {
                            iNameEnd = iNameSt;
                        }
                        else if (iNameSt == 0 && iNameEnd == 0)
                        {
                            iNameSt = entry.NodesBeforeSelf().OfType<XElement>().Where(p => p.Name.LocalName == "entry").Count();
                            iNameEnd = iNameSt;
                        }

                        Span span = new Span(iNameSt, iNameEnd);

                        string morerows = (string)entry.Attributes("morerows").FirstOrDefault();
                        if (same_spanname.Count() != 0)
                        {
                            same_spanname.ForEach(p => p.AddSpan(span));
                            same_spanname.ForEach(p => p.Elements("entry").Where(s => (string)s.Attributes("spanname").FirstOrDefault() == spanname).Remove());
                        }
                        if (morerows != null ? IsInt.IsMatch(morerows) : false)
                        {
                            if (row.NodesAfterSelf().OfType<XElement>().Where(p => p.Name.LocalName == "row").Count() >= Convert.ToInt32(morerows))
                            {
                                List<XElement> morerow = row.NodesAfterSelf().OfType<XElement>().Where(p => p.Name.LocalName == "row").Take(Convert.ToInt32(morerows)).ToList();
                                morerow.ForEach(p => p.AddSpan(span));
                            }
                        }
                        int rowspan = row.NodesAfterSelf().TakeWhile(p => p.Annotation<Spans>() == null ? false : p.Annotation<Spans>().spans.Contains(span)).Count() + 1;

                        tr.Add(new XElement(entryName,
                            new XAttribute("rowspan", rowspan.ToString()),
                            new XAttribute("colspan", span.colspan.ToString()),
                            entry.Nodes().SelectMany(p => p.Transform())
                            )
                        );
                    }
                    tbody.Add(tr);
                    result = tbody;

                }
            }
            else
            {
                string elementName = "tbody";
                if (e.Parent.Name == "thgroup")
                {
                    elementName = "thead";
                }
                XElement tbody = new XElement(elementName,
                       e.Nodes().SelectMany(p => p.Transform())
                );
                result = tbody;
            }
            return result;
        }
        private static IEnumerable<XNode> Entry(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            string elementName = "td";

            if (e.Ancestors("thgroup").LastOrDefault() != null)
            {
                elementName = "th";
            }
            result.Add(new XElement(elementName,
                e.Nodes().SelectMany(p => p.Transform())
                )
            );
            return result;
        }
        private static IEnumerable<XNode> Row(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("tr",

                  e.Nodes().SelectMany(p => p.Transform())
                 )
             );
            return result;
        }
        #endregion
    }
    
}
