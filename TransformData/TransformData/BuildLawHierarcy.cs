using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TransformData
{
    class BuildLawHierarcy
    {
        public BuildLawHierarcy()
        {
            List<string> l = new List<string>();
            l.Add(@"^(?<id>(del\s(\s+)?(\d+)))(\s+)?(\.|$)");
            l.Add(@"^(?<id>(del\s(\s+)?([ivx]+)))(\s+)?(\.|$)");
            l.Add(@"^(?<id>(del\s(\s+)?([a-z]+)))(\s+)?(\.|$)");
            l.Add(@"(?<id>(([^\.]+)\sdel))(\s+)?(\.|$)");
            l.Add(@"^(?<id>(kap(\.)?(it(t)?el)?(\s)?(\s+)?(\d)(\d+)?(\s+)?([a-z])?))(\:|\.|\s|$)");
            l.Add(@"^(?<id>(kap(\.)?(it(t)?el)?(\s)?(\s+)?[ivx]+))(\:|\.|\s|$)");
            l.Add(@"^(?<id>(\d+(\s)?(\.)?(\s)?kapit(t)?(el)?(let)?))(\.|\s|$)");
            l.Add(@"^(?<id>((fyrste|første|andre|annet|tredje|fjerde|femte|sjette|sjuande|sjuende|åttende|åttande|niende|niande|tiende|tiande)(\s)?(\.)?(\s)?kapit(t)?el))(\.|\s|$)");
            l.Add(@"^(?<id>([ivx]+))(\.|\s|$)");
            l.Add(@"^(?<id>([a-h]))(\s+)?(\.|$)");
            l.Add(@"^(?<id>(avdeling(\s)(\s+)?(\d+)))(\s)?(\s+)?(\.|$)");
            l.Add(@"^(?<id>(avdeling(\s)(\s+)?([ivx]+)))(\s)?(\s+)?(\.|$)");
            l.Add(@"^(?<id>(avsnitt(\s)(\s+)?(\d+)))(\s)?(\s+)?(\.|$)");
            l.Add(@"^(?<id>(avsnitt(\s)(\s+)?([ivx]+)))(\s)?(\s+)?(\.|$)");
            l.Add(@"^(Ny|Til((\sny)?|\sopphevelse(n)?\sav)|Endret)(\s+)(§+(\s)?)?(?<id>(\d+(\s)?([a-z])?((\-\d+(\s)?([a-z])?)+)?))(\s|\.|$)");
            l.Add(@"(^|^[^\.]+)((?<!\si\s)§+)(\s)?(?<id>(\d+(\s)?([a-z])?((\s+)?(til|" + (char)0x2013 + @")(\s+)?\d+(\s)?([a-z])?)?))(\s)?(\s+)?(\.|$)");
            //para = 1;
            l.Add(@"^(?<id>(art(\s)?(\s+)?(\d+)))(\s)?(\s+)?(\.|$)");
            //para = 1;
            l.Add(@"^(?<id>(art(\s)?(\s+)?([ivx]+)))(\s)?(\s+)?(\.|$)");
            //para = 1;
            l.Add(@"(^|^[^\.]+)((?<!\si\s)§+)(\s)?(?<id>(\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?((\s+)?(til|" + (char)0x2013 + @")(\s+)?\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?)?))(\s)?(\s+)?(\.|$|\s)");
            //para = 2;
            l.Add(@"(^|^[^\.]+)((?<!\si\s)§+)(\s)?(?<id>(\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?))(\s)?(\s+)?(\.|$|\s)");
            //para = 3;
            l.Add(@"(^|^[^\.]+)((?<!\si\s)§+)(\s)?(?<id>(\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?))(\s)?(\s+)?(\.|$|\s)");
            //para = 4;
        }
    }
}
