using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using TransformData.Global;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TransformData
{
    public partial class frmConnectDocuments : Form
    {
        public frmConnectDocuments()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            XElement all = XElement.Load(@"C:\_kobling\data\forarbeider_old5.xml");
            all.Descendants("intrefs").Remove();
            all.Descendants("intref").Remove();
            all.Descendants("refpoint").Remove();
            all.Descendants("extref").Remove();
            all.Descendants("keywords").Remove();

            
            foreach (XElement d in all.Descendants("document"))
            {
                if (d.Element("info") != null)
                {
                    string name = d.Element("info").Value.Trim().ToLower();
                    if (d.Attribute("year") != null) d.Attribute("year").Remove();
                    name.IdentifyMatchForarbeider(d);
                }
            }
            
            all.Save(@"C:\_kobling\data\forarbeider_old2.xml");
            all = XElement.Load(@"C:\_kobling\data\forarbeider_old2.xml");


            XElement tot = new XElement("documents");

            //samle med alle parameter
            XElement container = null;
            foreach (XElement r in all.Descendants("document"))
            {
                container = null;
                if (r.Attribute("selected") == null)
                {
                    int n = all.Descendants("document").Where(p =>
                            (p.Attribute("name1") == null ? "????" : p.Attribute("name1").Value.Trim()) == (r.Attribute("name1") == null ? "!!!!" : r.Attribute("name1").Value.Trim())
                            && (p.Attribute("number") == null ? "????" : p.Attribute("number").Value.Trim()) == (r.Attribute("number") == null ? "!!!!" : r.Attribute("number").Value.Trim())
                            && (p.Attribute("year1") == null ? "????" : p.Attribute("year1").Value.Trim()) == (r.Attribute("year1") == null ? "!!!!" : r.Attribute("year1").Value.Trim())
                            && (p.Attribute("year2") == null ? "????" : p.Attribute("year2").Value.Trim()) == (r.Attribute("year2") == null ? "!!!!" : r.Attribute("year2").Value.Trim())
                            && p.Attribute("selected") == null
                        ).Count();
                    if (n>0)
                    {
                        for (int i = n; i > 0;i--)
                        {
                            XElement d =  all.Descendants("document").Where(p =>
                                    (p.Attribute("name1") == null ? "????" : p.Attribute("name1").Value.Trim()) == (r.Attribute("name1") == null ? "!!!!" : r.Attribute("name1").Value.Trim())
                                    && (p.Attribute("number") == null ? "????" : p.Attribute("number").Value.Trim()) == (r.Attribute("number") == null ? "!!!!" : r.Attribute("number").Value.Trim())
                                    && (p.Attribute("year1") == null ? "????" : p.Attribute("year1").Value.Trim()) == (r.Attribute("year1") == null ? "!!!!" : r.Attribute("year1").Value.Trim())
                                    && (p.Attribute("year2") == null ? "????" : p.Attribute("year2").Value.Trim()) == (r.Attribute("year2") == null ? "!!!!" : r.Attribute("year2").Value.Trim())
                                    && p.Attribute("selected") == null
                                ).ElementAt(i-1);
                            
                            if (container == null)
                            {
                                container = new XElement("container", d.Attributes());
                                tot.Add(container);
                            }
                            container.Add(new XElement(d));
                            
                            d.Add(new XAttribute("selected", "1"));
                        }
                    }
                }
            }

            foreach (XElement r in all.Descendants("document"))
            {
                container = null;
                if (r.Attribute("selected") == null)
                {
                    int n = all.Descendants("document").Where(p =>
                            (p.Attribute("name1") == null ? "????" : p.Attribute("name1").Value.Trim()) == (r.Attribute("name1") == null ? "!!!!" : r.Attribute("name1").Value.Trim())
                            && (p.Attribute("number") == null ? "????" : p.Attribute("number").Value.Trim()) == (r.Attribute("number") == null ? "!!!!" : r.Attribute("number").Value.Trim())
                            && (p.Attribute("type") == null ? "????" : p.Attribute("type").Value.Trim()) == (r.Attribute("type") == null ? "!!!!" : r.Attribute("type").Value.Trim())
                            && p.Attribute("selected") == null
                        ).Count();
                    if (n > 0)
                    {
                        for (int i = n; i > 0; i--)
                        {
                            XElement d = all.Descendants("document").Where(p =>
                                (p.Attribute("name1") == null ? "????" : p.Attribute("name1").Value.Trim()) == (r.Attribute("name1") == null ? "!!!!" : r.Attribute("name1").Value.Trim())
                                && (p.Attribute("number") == null ? "????" : p.Attribute("number").Value.Trim()) == (r.Attribute("number") == null ? "!!!!" : r.Attribute("number").Value.Trim())
                                && (p.Attribute("type") == null ? "????" : p.Attribute("type").Value.Trim()) == (r.Attribute("type") == null ? "!!!!" : r.Attribute("type").Value.Trim())
                                && p.Attribute("selected") == null
                            ).ElementAt(i - 1);

                            if (container == null && tot.Descendants("container").Where(p =>
                                    (p.Attribute("name1") == null ? "????" : p.Attribute("name1").Value.Trim()) == (r.Attribute("name1") == null ? "!!!!" : r.Attribute("name1").Value.Trim())
                                    && (p.Attribute("number") == null ? "????" : p.Attribute("number").Value.Trim()) == (r.Attribute("number") == null ? "!!!!" : r.Attribute("number").Value.Trim())
                                    && (p.Attribute("type") == null ? "????" : p.Attribute("type").Value.Trim()) == (r.Attribute("type") == null ? "!!!!" : r.Attribute("type").Value.Trim())
                                ).Count() != 0)
                            {
                                container = tot.Descendants("container").Where(p =>
                                    (p.Attribute("name1") == null ? "????" : p.Attribute("name1").Value.Trim()) == (r.Attribute("name1") == null ? "!!!!" : r.Attribute("name1").Value.Trim())
                                    && (p.Attribute("number") == null ? "????" : p.Attribute("number").Value.Trim()) == (r.Attribute("number") == null ? "!!!!" : r.Attribute("number").Value.Trim())
                                    && (p.Attribute("type") == null ? "????" : p.Attribute("type").Value.Trim()) == (r.Attribute("type") == null ? "!!!!" : r.Attribute("type").Value.Trim())
                                    ).First();
                            }
                            else if (container == null)
                            {
                                container = new XElement("container", d.Attributes());
                                tot.Add(container);
                            }
                            
                            container.Add(new XElement(d));
                            d.Add(new XAttribute("selected", "1"));
                        }
                    }
                }
            }

            
            foreach (XElement r in all.Descendants("document"))
            {
                container = null;
                if (r.Attribute("selected") == null)
                {
                    int n = all.Descendants("document").Where(p =>
                            (p.Attribute("name1") == null ? "????" : p.Attribute("name1").Value.Trim()) == (r.Attribute("name1") == null ? "!!!!" : r.Attribute("name1").Value.Trim())
                            && (p.Attribute("number") == null ? "????" : p.Attribute("number").Value.Trim()) == (r.Attribute("number") == null ? "!!!!" : r.Attribute("number").Value.Trim())
                            && (p.Attribute("year1") == null ? "????" : p.Attribute("year1").Value.Trim()) == (r.Attribute("year1") == null ? "!!!!" : r.Attribute("year1").Value.Trim())
                            && p.Attribute("selected") == null
                        ).Count();
                    if (n > 0)
                    {
                        for (int i = n; i > 0; i--)
                        {
                            XElement d = all.Descendants("document").Where(p =>
                                    (p.Attribute("name1") == null ? "????" : p.Attribute("name1").Value.Trim()) == (r.Attribute("name1") == null ? "!!!!" : r.Attribute("name1").Value.Trim())
                                    && (p.Attribute("number") == null ? "????" : p.Attribute("number").Value.Trim()) == (r.Attribute("number") == null ? "!!!!" : r.Attribute("number").Value.Trim())
                                    && (p.Attribute("year1") == null ? "????" : p.Attribute("year1").Value.Trim()) == (r.Attribute("year1") == null ? "!!!!" : r.Attribute("year1").Value.Trim())
                                    && p.Attribute("selected") == null
                                ).ElementAt(i - 1);
                            
                            if (container == null && tot.Descendants("container").Where(p =>
                                    (p.Attribute("name1") == null ? "????" : p.Attribute("name1").Value.Trim()) == (r.Attribute("name1") == null ? "!!!!" : r.Attribute("name1").Value.Trim())
                                    && (p.Attribute("number") == null ? "????" : p.Attribute("number").Value.Trim()) == (r.Attribute("number") == null ? "!!!!" : r.Attribute("number").Value.Trim())
                                    && (p.Attribute("year1") == null ? "????" : p.Attribute("year1").Value.Trim()) == (r.Attribute("year1") == null ? "!!!!" : r.Attribute("year1").Value.Trim())
                                ).Count() != 0)
                            {
                                container = tot.Descendants("container").Where(p =>
                                    (p.Attribute("name1") == null ? "????" : p.Attribute("name1").Value.Trim()) == (r.Attribute("name1") == null ? "!!!!" : r.Attribute("name1").Value.Trim())
                                    && (p.Attribute("number") == null ? "????" : p.Attribute("number").Value.Trim()) == (r.Attribute("number") == null ? "!!!!" : r.Attribute("number").Value.Trim())
                                    && (p.Attribute("year1") == null ? "????" : p.Attribute("year1").Value.Trim()) == (r.Attribute("year1") == null ? "!!!!" : r.Attribute("year1").Value.Trim())
                                    ).First();
                            }
                            else if (container == null)
                            {
                                Debug.Print(d.Element("info").Value);
                                container = new XElement("container", d.Attributes());
                                tot.Add(container);
                            }
                            
                            container.Add(new XElement(d));
                            d.Add(new XAttribute("selected", "1"));
                        }
                    }
                }
            }
            int v = all.Descendants("document").Count();
            int x = all.Descendants("document").Where(p => p.Attribute("selected") == null).Count();

            container = null;
            foreach (XElement ve in all.Descendants("document").Where(p => p.Attribute("selected") == null))
            {
                if (container == null)
                {
                    container = new XElement("container", new XAttribute("unconnected", "1"));
                    tot.Add(container);
                }
                container.Add(new XElement(ve));
                //Debug.Print("Navn:" + (ve.Element("name") == null ? "" : ve.Element("name").Value)
                //                   + " Info: " + (ve.Element("info") == null ? "" : ve.Element("info").Value)
                //                   + " År: " + (ve.Element("year") == null ? "" : ve.Element("year").Value));
            }

            foreach (XElement ve in tot.Descendants("container").Where(p => p.Elements("document").Count() > 1 && p.Attribute("unconnected") == null))
            {

                Debug.Print("Container Start: " + ve.Elements("document").Count().ToString() + " Antall bokmerker: " );
                foreach (XElement vee in ve.Elements("document"))
                {
                    Debug.Print(vee.Element("info").Value + " /// " + vee.Element("name").Value);
                }
                Debug.Print("Container Slutt:");
            }
                
            
            int y = tot.Descendants("container").Count();
            int z = tot.Descendants("document").Count();

            tot.Save(@"C:\_kobling\data\tot.xml");
            MessageBox.Show("Koblet");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            XElement tot = XElement.Load(@"C:\_kobling\data\tot.xml");
            string regEx = global.m_REGEXPOUERY.Where(p => p.Key == "forarbeidertot").First().Value;
            Regex q = new Regex(regEx);
            
            string fileName = "";
            for (int i = 0; i < 5; i++)
            {
                switch (i)
                {
                    case 0: fileName = "aksjeloven"; break;
                    case 1: fileName = "allmennaksjeloven"; break;
                    case 2: fileName = "regnskapsloven"; break;
                    case 3: fileName = "Revisorloven"; break;
                    case 4: fileName = "verdipapirhandelloven"; break;
                }
                XElement reference = XElement.Load(@"C:\_kobling\data\" + fileName + ".xml");

                foreach (XElement d in reference.Descendants("document"))
                {
                    string name = d.Attribute("name").Value.Trim().ToLower();
                    if (d.Attribute("year") != null) d.Attribute("year").Remove();
                    name.IdentifyMatchForarbeider(d);
                }
                reference.Save(@"C:\_kobling\data\" + fileName + ".xml");
                XElement references = null;
                
                foreach (XElement r in reference.Descendants("document"))
                {
                    if (r.Attribute("identyfied")==null)
                    {
                        int n = tot.Descendants("container").Where(p =>
                                (p.Attribute("name1") == null ? "????" : p.Attribute("name1").Value.Trim()) == (r.Attribute("name1") == null ? "!!!!" : r.Attribute("name1").Value.Trim())
                                && (p.Attribute("number") == null ? "????" : p.Attribute("number").Value.Trim()) == (r.Attribute("number") == null ? "!!!!" : r.Attribute("number").Value.Trim())
                                && (p.Attribute("year1") == null ? "????" : p.Attribute("year1").Value.Trim()) == (r.Attribute("year1") == null ? "!!!!" : r.Attribute("year1").Value.Trim())
                                && (p.Attribute("year2") == null ? "????" : p.Attribute("year2").Value.Trim()) == (r.Attribute("year2") == null ? "!!!!" : r.Attribute("year2").Value.Trim())
                                && p.Attribute("selected") == null
                            ).Count();
                        if (n == 1)
                        {
                            XElement d = tot.Descendants("container").Where(p =>
                                    (p.Attribute("name1") == null ? "????" : p.Attribute("name1").Value.Trim()) == (r.Attribute("name1") == null ? "!!!!" : r.Attribute("name1").Value.Trim())
                                    && (p.Attribute("number") == null ? "????" : p.Attribute("number").Value.Trim()) == (r.Attribute("number") == null ? "!!!!" : r.Attribute("number").Value.Trim())
                                    && (p.Attribute("year1") == null ? "????" : p.Attribute("year1").Value.Trim()) == (r.Attribute("year1") == null ? "!!!!" : r.Attribute("year1").Value.Trim())
                                    && (p.Attribute("year2") == null ? "????" : p.Attribute("year2").Value.Trim()) == (r.Attribute("year2") == null ? "!!!!" : r.Attribute("year2").Value.Trim())
                                    && p.Attribute("selected") == null
                            ).First();

                                if (d.Element("references") == null)
                                {
                                    references = new XElement("references", d.Attributes());
                                    d.Add(references);
                                }

                                d.Element("references").Add(CleanReferances(r).Nodes());
                                r.Add(new XAttribute("identyfied","1"));
                        }
                        else if (n>1)
                            Debug.Print("error");

                    }
                }
                

                foreach (XElement r in reference.Descendants("document"))
                {
                    if (r.Attribute("identyfied") == null)
                    {
                        int n = tot.Descendants("container").Where(p =>
                                (p.Attribute("name1") == null ? "????" : p.Attribute("name1").Value.Trim()) == (r.Attribute("name1") == null ? "!!!!" : r.Attribute("name1").Value.Trim())
                                && (p.Attribute("number") == null ? "????" : p.Attribute("number").Value.Trim()) == (r.Attribute("number") == null ? "!!!!" : r.Attribute("number").Value.Trim())
                                && (p.Attribute("type") == null ? "????" : p.Attribute("type").Value.Trim()) == (r.Attribute("type") == null ? "!!!!" : r.Attribute("type").Value.Trim())
                            ).Count();
                        if (n == 1)
                        {
                            XElement d = tot.Descendants("container").Where(p =>
                                (p.Attribute("name1") == null ? "????" : p.Attribute("name1").Value.Trim()) == (r.Attribute("name1") == null ? "!!!!" : r.Attribute("name1").Value.Trim())
                                && (p.Attribute("number") == null ? "????" : p.Attribute("number").Value.Trim()) == (r.Attribute("number") == null ? "!!!!" : r.Attribute("number").Value.Trim())
                                && (p.Attribute("type") == null ? "????" : p.Attribute("type").Value.Trim()) == (r.Attribute("type") == null ? "!!!!" : r.Attribute("type").Value.Trim())
                            ).First();

                                if (d.Element("references") == null)
                                {
                                    references = new XElement("references", d.Attributes());
                                    d.Add(references);
                                }
                                d.Element("references").Add(CleanReferances(r).Nodes());
                                r.Add(new XAttribute("identyfied","1"));
                        }
                        else if (n > 1)
                            Debug.Print("error");
                    }
                }
 
                foreach (XElement r in reference.Descendants("document"))
                {
                    if (r.Attribute("identyfied") == null)
                    {
                        int n = tot.Descendants("container").Where(p =>
                                (p.Attribute("name1") == null ? "????" : p.Attribute("name1").Value.Trim()) == (r.Attribute("name1") == null ? "!!!!" : r.Attribute("name1").Value.Trim())
                                && (p.Attribute("number") == null ? "????" : p.Attribute("number").Value.Trim()) == (r.Attribute("number") == null ? "!!!!" : r.Attribute("number").Value.Trim())
                                && (p.Attribute("year1") == null ? "????" : p.Attribute("year1").Value.Trim()) == (r.Attribute("year1") == null ? "!!!!" : r.Attribute("year1").Value.Trim())
                            ).Count();
                        if (n == 1)
                        {
                            XElement d = tot.Descendants("container").Where(p =>
                                (p.Attribute("name1") == null ? "????" : p.Attribute("name1").Value.Trim()) == (r.Attribute("name1") == null ? "!!!!" : r.Attribute("name1").Value.Trim())
                                && (p.Attribute("number") == null ? "????" : p.Attribute("number").Value.Trim()) == (r.Attribute("number") == null ? "!!!!" : r.Attribute("number").Value.Trim())
                                && (p.Attribute("year1") == null ? "????" : p.Attribute("year1").Value.Trim()) == (r.Attribute("year1") == null ? "!!!!" : r.Attribute("year1").Value.Trim())
                            ).First();

                            if (d.Element("references") == null)
                            {
                                references = new XElement("references", d.Attributes());
                                d.Add(references);
                            }
                            d.Element("references").Add(CleanReferances(r).Nodes());
                            r.Add(new XAttribute("identyfied", "1"));
                        }
                        else if (n > 1)
                            Debug.Print("error");
                    }
                }
                foreach (XElement r in reference.Descendants("document").Where(p => p.Attribute("identyfied") == null))
                {
                    Debug.Print(fileName + ": " + r.Attribute("name").Value);
                }
            }

            tot.Save(@"C:\_kobling\data\tot1.xml");
        }

        private XElement CleanReferances(XElement d)
        {
            XElement returnValue = new XElement("root");
            XElement temp = new XElement("temp");
            foreach (XElement r in d.Elements("reference"))
            {
                if (temp
                    .Elements("reference")
                    .Where(p=>
                        p.Attribute("from_document").Value.Trim().ToLower() == r.Attribute("from_document").Value.Trim().ToLower()
                        && p.Attribute("from").Value.Trim().ToLower() == r.Attribute("from").Value.Trim().ToLower()
                        && p.Attribute("to").Value.Trim().ToLower() == r.Attribute("to").Value.Trim().ToLower()
                    ).Count() == 0)
                {
                    XElement newR = new XElement(r);
                    if (newR.Attribute("bm")!= null) newR.Attribute("bm").Remove();
                    temp.Add(newR);
                }
            }
            foreach (XElement r in temp.Elements("reference").OrderBy(p=>p.Attribute("to").Value))
            {
                returnValue.Add(new XElement(r.Name.LocalName
                    , r.Attribute("from_document")
                    , r.Attribute("to")
                    , r.Attribute("from")
                    ));
            }
            return returnValue;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            XElement tot = XElement.Load(@"C:\_kobling\data\tot1.xml");

            Debug.Print("referanser: " +  tot.Descendants("reference").Count().ToString());
            Debug.Print("assosiation: " + tot.Descendants("assosiation").Count().ToString());
            //foreach (XElement container in tot.Descendants("container").Where(p=>p.Attribute("unconnected")==null))
            //{
            //    Debug.Print(container.Element("document").Element("info").Value
            //        + " reference: " + container.Descendants("reference").Count().ToString()
            //        + " assosiation: " + container.Descendants("assosiation").Count().ToString() 
            //        );
            //}

            //ingen koblinger
            Debug.Print("INGEN");
            foreach (XElement container in tot.Descendants("container").Where(p => 
                p.Attribute("unconnected") == null
                && p.Descendants("reference").Count()==0
                && p.Descendants("assosiation").Count() == 0
                ))
            {
                Debug.Print(container.Element("document").Element("info").Value
                    + " dokumenter: " + container.Descendants("document").Count().ToString()
                    + " reference: " + container.Descendants("reference").Count().ToString()
                    + " assosiation: " + container.Descendants("assosiation").Count().ToString()
                    );
            }

            // koblinger
            Debug.Print("KOBLET");
            foreach (XElement container in tot.Descendants("container").Where(p =>
                p.Attribute("unconnected") == null
                && (p.Descendants("reference").Count() != 0
                || p.Descendants("assosiation").Count() != 0)
                ))
            {
                Debug.Print(container.Element("document").Element("info").Value
                    + " dokumenter: " + container.Descendants("document").Count().ToString()
                    + " reference: " + container.Descendants("reference").Count().ToString()
                    + " assosiation: " + container.Descendants("assosiation").Count().ToString()
                    );

                foreach (XElement r in container.Descendants("assosiation").OrderBy(p => p.Attribute("key").Value))
                {
                    string to = r.Attribute("key").Value.Trim().ToLower();
                    int n = container.Descendants("bookmark").Where(p => p.Attribute("key").Value.Trim().ToLower() == to).Count();
                    if (n == 1)
                    {
                        XElement bm = container.Descendants("bookmark").Where(p => p.Attribute("key").Value.Trim().ToLower() == to).ElementAt(0);
                        AddAssosiation(container, r, bm);
                    }
                    else if (n == 0)
                    {
                            XElement bm = null;
                            AddAssosiation(container, r, bm);
                    }
                    else if (n > 1)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            XElement bm = container.Descendants("bookmark").Where(p => p.Attribute("key").Value.Trim().ToLower() == to).ElementAt(i);
                            AddAssosiation(container, r, bm);
                        }
                    }
                }


                foreach (XElement r in container.Descendants("reference").OrderBy(p=>p.Attribute("to").Value))
                {
                    string to = r.Attribute("to").Value.Trim().ToLower();
                    int n = container.Descendants("bookmark").Where(p => p.Attribute("key").Value.Trim().ToLower() == to).Count();
                    if (n == 1)
                    {
                        XElement bm = container.Descendants("bookmark").Where(p => p.Attribute("key").Value.Trim().ToLower() == to).ElementAt(0);
                        AddReferance(container,r, bm);
                    }
                    else if (n == 0)
                    {
                        n = tot.Descendants("bookmark").Where(p => p.Attribute("key").Value.Trim().ToLower() == to).Count();
                        if (n == 0)
                        {
                            XElement bm = null;
                            AddReferance(container, r, bm);
                        }
                        else if (n == 1)
                        {
                            XElement c = tot.Descendants("bookmark").Where(p => p.Attribute("key").Value.Trim().ToLower() == to).ElementAt(0).Ancestors("container").First();
                            XElement bm = tot.Descendants("bookmark").Where(p => p.Attribute("key").Value.Trim().ToLower() == to).ElementAt(0);
                            AddReferance(c, r, bm);
                        }
                        else
                        {
                            Debug.Print("FEIL! FEIL! " + to);
                        }
                    }
                    else if (n > 1)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            XElement bm = container.Descendants("bookmark").Where(p => p.Attribute("key").Value.Trim().ToLower() == to).ElementAt(i);
                            AddReferance(container, r, bm);
                        }
                    }
                }
            }
            foreach (XElement con in tot.Descendants("container").Where(p=>p.Attribute("unconnected")== null))
            {
                con.Add(new XAttribute("topic_id", con.Elements("document").First().Element("topic_id").Value));
            }
            Debug.Print("Qref med id:" + tot.Descendants("qreferance").Where(p=>p.Element("document")!=null).Count());
            Debug.Print("Qref uten id:" + tot.Descendants("qreferance").Where(p => p.Element("document") == null).Count());

            Debug.Print("Qasso med id:" + tot.Descendants("qassosiation").Where(p => p.Element("document") != null).Count());
            Debug.Print("Qasso uten id:" + tot.Descendants("qassosiation").Where(p => p.Element("document") == null).Count());

            
            tot.Save(@"C:\_kobling\data\tot2.xml");
        }

        private void AddAssosiation(XElement container, XElement r, XElement bm)
        {
            if (r.Attribute("member2bm") == null) return;
            XElement referanse = null;
            if (container.Element("qassosiations") == null) container.AddFirst(new XElement("qassosiations"));

            if ((container.Element("qassosiations").Elements("qassosiations").Count() == 0 ?
                0
                :
                container.Element("qassosiations").Elements("qassosiation").Where(p => p.Attribute("to").Value.Trim().ToLower() == r.Attribute("key").Value.Trim().ToLower()).Count()) == 0)
            {
                container.Element("qassosiations").Add(new XElement("qassosiation"
                    , new XAttribute("to", r.Attribute("key").Value.Trim().ToLower())));
            }

            referanse = container.Element("qassosiations").Elements("qassosiation").Where(p => p.Attribute("to").Value.Trim().ToLower() == r.Attribute("key").Value.Trim().ToLower()).First();

            if ((referanse.Elements("from").Count() == 0 ?
                0
                :
                referanse.Elements("from").Where(p =>
                    p.Attribute("topic_id").Value == r.Attribute("member2").Value
                    && p.Attribute("bookmark").Value == r.Attribute("member2bm").Value
                    ).Count()) == 0)
            {
                referanse.Add(new XElement("from"
                    , new XAttribute("topic_id", r.Attribute("member2").Value)
                    , new XAttribute("bookmark", r.Attribute("member2bm").Value)
                    ));
            }



            if (bm != null)
            {

                if ((referanse.Elements("document").Count() == 0 ?
                0
                :
                referanse.Elements("document").Where(p =>
                    p.Attribute("topic_id").Value == bm.Ancestors("document").First().Element("topic_id").Value
                    ).Count()) == 0)
                {
                    referanse.Add(new XElement("document"
                        , new XAttribute("topic_id", bm.Ancestors("document").First().Element("topic_id").Value)
                        ));
                }


                if ((referanse.Elements("title").Count() == 0 ?
                   0
                   :
                   referanse.Elements("title").Where(p =>
                       p.Attribute("text").Value == bm.Attribute("title").Value
                       && bm.Attribute("title").Value.Trim() != ""
                       ).Count()) == 0)
                {
                    referanse.Add(new XElement("title"
                        , new XAttribute("text", bm.Attribute("title").Value.Trim())
                        ));
                }
            }
        }


        private void AddReferance(XElement container, XElement r, XElement bm)
        {
            XElement referanse = null;
            if (container.Element("qreferances") == null) container.AddFirst(new XElement("qreferances"));

            if ((container.Element("qreferances").Elements("qreferance").Count() == 0 ?
                0
                :
                container.Element("qreferances").Elements("qreferance").Where(p => p.Attribute("to").Value.Trim().ToLower() == r.Attribute("to").Value.Trim().ToLower()).Count()) == 0)
            {
                container.Element("qreferances").Add(new XElement("qreferance"
                    , new XAttribute("to",  r.Attribute("to").Value.Trim().ToLower())));
            }

            referanse = container.Element("qreferances").Elements("qreferance").Where(p => p.Attribute("to").Value.Trim().ToLower() == r.Attribute("to").Value.Trim().ToLower()).First();
            
            if ((referanse.Elements("from").Count()==0 ? 
                0 
                : 
                referanse.Elements("from").Where(p=>
                    p.Attribute("name").Value == r.Attribute("from_document").Value
                    && p.Attribute("bookmark").Value == r.Attribute("from").Value
                    ).Count())== 0)
            {
                referanse.Add(new XElement("from"
                    ,  new XAttribute("name", r.Attribute("from_document").Value)
                    ,  new XAttribute("bookmark", r.Attribute("from").Value)
                    ));
            }
            


            if (bm != null)
            {

                if ((referanse.Elements("document").Count() == 0 ?
                0
                :
                referanse.Elements("document").Where(p =>
                    p.Attribute("topic_id").Value == bm.Ancestors("document").First().Element("topic_id").Value
                    ).Count()) == 0)
                {
                    referanse.Add(new XElement("document"
                        , new XAttribute("topic_id", bm.Ancestors("document").First().Element("topic_id").Value)
                        ));
                }

                
                if ((referanse.Elements("title").Count() == 0 ?
                   0
                   :
                   referanse.Elements("title").Where(p =>
                       p.Attribute("text").Value == bm.Attribute("title").Value
                       && bm.Attribute("title").Value.Trim() != ""
                       ).Count()) == 0)
                {
                    referanse.Add(new XElement("title"
                        , new XAttribute("text", bm.Attribute("title").Value.Trim())
                        ));
                }
            }
        }
    }
}
