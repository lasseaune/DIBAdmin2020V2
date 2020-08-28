using DIBAdminAPI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;


namespace DIBAdminAPI.Source
{
    public class ElementEnumerator
    {
        public int idx { get; set; }
        public ElementEnumerator(XElement e, int n)
        {
            idx = n;
        }

    }
    public static class Extensions
    {
        //GetElementsPlacement
        //Funksjonen henter ut alle plasseringsdata for elementer i document 
        public static XElement GetElementsPlacement(this XElement document, int segmentIndex)
        {
            int n = 0;
            document.Descendants().ToList().ForEach(p => p.AddAnnotation(new ElementEnumerator(p, n++)));

            return new XElement("items",
                    new XAttribute("segmentOrder", segmentIndex.ToString()),
                    document
                    .Descendants()
                    .Attributes("id")
                    .Select(p => new XElement("item",
                                p.GetElementPlacement()
                            )
                    )
                );
        }
        //GetElementPlacement
        //Funksjonen henter ut alle plasseringsdata for element
        public static IEnumerable<XAttribute> GetElementPlacement(this XAttribute id)
        {
            List<XAttribute> result = new List<XAttribute>();
            result.Add(new XAttribute("Id", id.Value));
            string parentId = (string)id.Parent
                        .Ancestors("section")
                        .Where(a =>
                            a.Elements()
                            .Where(n => Regex.IsMatch(n.Name.LocalName, @"^h\d")).Count() > 0
                        ).Attributes("id").LastOrDefault();
            if (parentId != null)
            {
                result.Add(new XAttribute("parentId", parentId));
            }
            string title = id
                        .Parent
                        .Elements()
                        .Where(n => Regex.IsMatch(n.Name.LocalName, @"^h\d"))
                        .Select(n => n
                                .DescendantNodes()
                                .OfType<XText>()
                                .Where(s => s.Ancestors("sup").Count() == 0)
                                .Select(s => s.Value)
                                .StringConcatenate()
                        ).FirstOrDefault();
            if (title != null)
            {
                result.Add(new XAttribute("title", title));
            }

            result.Add(new XAttribute("idx", id.Parent.Annotations<ElementEnumerator>().Select(p => p.idx).FirstOrDefault().ToString()));
            string tag1 = (string)id.Parent.AncestorsAndSelf().Attributes("tag1").FirstOrDefault();
            if (tag1 != null)
                result.Add(new XAttribute("tag1", tag1));
            string tag2 = (string)id.Parent.AncestorsAndSelf().Attributes("tag2").FirstOrDefault();
            if (tag2 != null)
                result.Add(new XAttribute("tag2", tag2));
            string tag3 = (string)id.Parent.AncestorsAndSelf().Attributes("tag3").FirstOrDefault();
            if (tag3 != null)
                result.Add(new XAttribute("tag3", tag3));
            return result;

        }
        public static string Md5Hash(this string input)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string GetCallerName(this string s)
        {
            var method = new StackTrace().GetFrame(1).GetMethod();
            return string.Format("{1}:{0} -> ", method.Name, method.DeclaringType);
        }

        public static string GetProgramName(this string s)
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        }
    }
}
