using System;
using System.Linq;
using System.Xml.Linq;

namespace DIBConvertCLR
{
    class Program
    {
        static void Main(string[] args)
        {
            string err = "";
            XElement document = XElement.Load(@"d:\_DibDownloadClr\document.xml");
            if (document == null) return;
            XElement convert = document.TransformLaw();
            err = "Transformert";
            XElement top = new XElement(convert.Elements("section").Where(p => (string)p.Attributes("id").FirstOrDefault() == "_top").FirstOrDefault());
            top = new XElement("document", top.Attributes(), top.Nodes());
            err = "Hent map";
            XElement map = new XElement("items", top.MapSection());


            err = "Legg til map";
            XElement index = map.AddIndex();
            err = "hent searchitems";
            top.GetSearchItems();
            XElement searchitems = new XElement("searchitems",
                top.Descendants()
                    .Where(p => (string)p.Attributes("id").FirstOrDefault() != null && (p.Annotation<SearchItem>() == null ? null : p.Annotation<SearchItem>().text) != null)
                    .Select(p => new XElement("search",
                        new XAttribute("object_id", p.Annotation<SearchItem>().id != null ? p.Annotation<SearchItem>().id : (string)p.Attributes("id").FirstOrDefault() ?? "none"),
                        new XAttribute("text_type", p.Annotation<SearchItem>().type.ToString()),
                        new XText(p.Annotation<SearchItem>().text)
                    )
                )
            );

            XElement result = new XElement("html5");
            result.Add(top);
            result.Add(index);
            result.Add(map);
            result.Add(searchitems);
            result.Save(@"d:\_DibDownloadClr\result.xml");
        }
    }
}
