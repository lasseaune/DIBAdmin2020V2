using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexEditor
{
    public static class Extentions
    {
        public static string StringConcatenate(this IEnumerable<string> source, string split)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in source)
                if (sb.Length == 0)
                    sb.Append(s);
                else
                    sb.Append(split + s);
            return sb.ToString();
        }
        public static string StringConcatenate(this IEnumerable<string> source)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in source)
                sb.Append(s);
            return sb.ToString();
        }
        public static string StringConcatenate<T>(
            this IEnumerable<T> source,
            Func<T, string> projectionFunc)
        {
            return source.Aggregate(
                new StringBuilder(),
                (s, i) => s.Append(projectionFunc(i)),
                s => s.ToString());
        }
    }
}
