using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace DIBIndexingCLR
{
    public class StemWords
    {
        public string stem { get; set; }
        public List<Words> words { get; set; }
    }
    public class Words
    {
        public string word { get; set; }
        public List<string> id { get; set; }
    }
    public static class IndexProcuctionObject
    {
        public static XElement CreateIndex(this XElement e)
        {
            Regex rxWord = new Regex(@"(?<=(^|\s|\.|\,|\;|\:|\?|\|\(|\)|\[|\]))(?<word>([a-zæøå]{2,}))(?=(\.|\,|\;|\:|\?|\s|$|\$|\/|\\|\£|\)|\(|\[|\]))", RegexOptions.IgnoreCase);

            XElement ws =  new XElement("ws",
                    e.DescendantNodes()
                    .OfType<XText>()
                    .Where(p => p.Ancestors("document").FirstOrDefault() != null)
                    .Select(p => new { mc = rxWord.Matches(p.Value), id = (string)p.Ancestors("section").Attributes("id").FirstOrDefault() })
                    .Select(p => new { id = p.id, l = p.mc.OfType<Match>() })
                    .Select(p => new { id = p.id, ws = p.l.Select(s => s.Groups["word"].Value.ToLower()).ToList() })
                    .SelectMany(p => p.ws.Where(w => w.Length > 1).Select(w => new { word = w, id = p.id }))
                    .GroupBy(p => p.word)
                    .Select(p => new { word = p.Key.ToString(), stem = p.Key.ToString().Stem(), parent = p.GroupBy(s => s.id).Select(s => s.Key).ToList() })
                    .GroupBy(p => p.stem)
                    .OrderBy(p => p.Key)
                    .Select(p => 
                        new XElement("s",
                            new XAttribute("v", p.Key),
                            p.GroupBy(r => r.word)
                                .OrderBy(r => r.Key)
                                .Select(r => 
                                    new XElement("w",
                                        new XAttribute("v", r.Key),
                                        r.SelectMany(s => s.parent).GroupBy(s => s).Select(s => new XElement("e", new XAttribute("id", s.Key)))
                                    )
                                )
                        )
                    )
            );

            return ws;


            //List<string> words = e.DescendantNodes()
            //                    .OfType<XText>()
            //                    .Where(p => p.Ancestors("document").FirstOrDefault() != null)
            //                    .Select(p => rxWord.Matches(p.Value))
            //                    .SelectMany(p => p.OfType<Match>())
            //                    .GroupBy(p => p.Groups["word"].Value.ToLower())
            //                    .Select(p => p.Key)
            //                    .OrderBy(p => p)
            //                    .ToList();

            //List<StemWords> stemwords = e.DescendantNodes()
            //                    .OfType<XText>()
            //                    .Where(p => p.Ancestors("document").FirstOrDefault() != null)
            //                    .Select(p => new { mc = rxWord.Matches(p.Value), id = (string)p.Ancestors("section").Attributes("id").FirstOrDefault() })
            //                    .Select(p => new { id = p.id, l = p.mc.OfType<Match>() })
            //                    .Select(p => new { id = p.id, ws = p.l.Select(s => s.Groups["word"].Value.ToLower()).ToList() })
            //                    .SelectMany(p => p.ws.Where(w => w.Length > 1).Select(w => new { word = w, id = p.id }))
            //                    .GroupBy(p => p.word)
            //                    .Select(p => new { word = p.Key.ToString(), stem = p.Key.ToString().Stem(), parent = p.GroupBy(s => s.id).Select(s => s.Key).ToList() })
            //                    .GroupBy(p => p.stem)
            //                    .OrderBy(p => p.Key)
            //                    .Select(p => new StemWords { stem = p.Key, words = p.GroupBy(r => r.word).OrderBy(r => r.Key).Select(r => new Words { word = r.Key, id = r.SelectMany(s => s.parent).GroupBy(s => s).Select(s => s.Key).ToList() }).ToList() })
            //                    .ToList();

            //foreach (StemWords p in stemwords.Where(p => p.stem.Length == 3))
            //{
            //    Debug.Print(p.stem);
            //    foreach (Words w in p.words)
            //    {
            //        Debug.Print("  " + w.word);
            //        //foreach (string s in w.id)
            //        //{
            //        //    Debug.Print("   " + s);
            //        //}
            //    }
            //}
            //"nyere".Stem();
            //foreach (StemWords w in stemwords.Where(p => p.stem.Length <= 2).Select(p => p))
            //{
            //    string r = w.stem.Reverse();
            //    List<string> rev = stemwords.Where(p => p.stem.Length > r.Length && p.stem.Reverse().Substring(0, r.Length) == r).Select(p => p.stem).ToList();
            //    if (rev.Count() > 0)
            //    {
            //        Debug.Print(w.stem);
            //        foreach (string v in rev)
            //        {
            //            Debug.Print("  " + v);
            //        }
            //    }

            //}

            //foreach (StemWords w in stemwords.Where(p => p.stem.Length > 2).Select(p => p))
            //{
            //    string r = w.stem.Reverse();
            //    List<string> rev = stemwords.Where(p => p.stem.Length > r.Length && p.stem.Reverse().Substring(0, r.Length) == r).Select(p => p.stem).ToList();
            //    if (rev.Count() > 0)
            //    {
            //        Debug.Print(w.stem);
            //        foreach (string v in rev)
            //        {
            //            Debug.Print("  " + v);
            //        }
            //    }

            //}



            ////words.ForEach(p => Debug.Print(p));
        }
    }
}
