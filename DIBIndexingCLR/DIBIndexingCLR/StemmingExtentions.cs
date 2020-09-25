using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace DIBIndexingCLR
{
    public static class StemmingExtentions
    {
        #region //Faste strenger variable
        public static Regex _rx_dec_no = new Regex(@"^(?<=((?<!\d)\s|^))"
                + @"("
                + @"(?<no_dec>(((?<number>((\d{1,3})((\d{3,3})+)?))((\,)(?<dec>((\d+)))))))"
                + @"|(?<no_space_dec>(((?<number>((\d{1,3})(((\s)\d{3,3})+)?))((\,)((?<dec>(\d+))))?)))"
                + @"|(?<no_punkt_dec>(((?<number>((\d{1,3})(((\.)\d{3,3})+)?))((\,)((?<dec>(\d+))))?)))"
                + @"|(?<ge_dec>(((?<number>((\d{1,3})(((\s)\d{3,3})+)?(((\.)\d{3,3}))))((\,)((?<dec>(\d+))))?)))"
                + @"|(?<us_dec>(((?<number>((\d{1,3})(((\,)\d{3,3})+){1,}))((\.)((?<dec>(\d+))))?)))"
                + @")"
                + @"(?=(\s(?!\d)|$))$");
        public static readonly string rxMath = @"(?<MATH>([\(\[]?[-]?((\()?(SUM|neg)(\))?(\s)?)?([0-9]+)[\)\]]??([\(\[]?([\-\+\/\*]([0-9]+))?([\.\,]((?<=[\+\-](\s)?\d+[.,])(\s)??)??[0-9]+)?[\)\]]?)((((\s)?([\=\+\-\*\/]|\.\.|,|\-|\s|[xX])(\s)?)[\(\[]?[-]?([0-9]+)[\)\]]??([\(\[]?([\-\+\/\*]([0-9]+)[\)\]]?)?([\.\,]((?<=[\+\-](\s)?\d+[.,])(\s)??)??[0-9]+)?[\)\]]??))*)?))";
        //public static readonly string rxMath = @"(?<MATH>(((?<=[ ][[]{2}).*?(?=[]]{2}[ .!?])|\d+|\b[a-zA-Z0-9{+-}]*\([a-zA-Z0-9{+-}]*\)|\b[a-zA-Z]\b){3,}))";
        public static readonly string numberX = @"(?<NUMMER>((?<=(^|\s|\())((§+(\s)?)?(\d|\d+)([a-z])?(((\.|\-)\d+([a-z])?)+)?)(?=($|\s|\.|\,|\)))))";
        public static readonly string rxDate = @"(?<DATOER>((((0[1-9]|1[0-9]|2[0-9]|3[0-1]|[1-9])([\\\.\-\/])(0[1-9]|1[0-2]|[1-9])([\\\.\-\/])([0-9]{2,4}))|((0[1-9]|1[0-9]|2[0-9]|3[0-1]|[1-9])\W+)?(((J|j)anuary?)|((F|f)ebruary?)|((M|m)ar(s|ch))|((A|a)pril)|((M|m)a(i|y))|((J|j)un(i|e))|((J|j)ul(i|y))|((A|a)ugust)|((S|s)eptember)|((O|o)(k|c)tober)|((N|n)ovember)|((D|d)e(s|c)ember))\W+[0-9]{2,4})|((0[1-9]|1[0-9]|2[0-9]|3[0-1]|[1-9])(\W+)(((J|j)an)|((F|f)ep)|((M|m)ar)|((A|a)pr)|((M|m)a(i|y))|((J|j)un)|((J|j)ul)|((A|a)ug)|((S|s)ep)|((O|o)(k|c)t)|((N|n)(ov))|((D|d)e(s|c)))\W+[0-9]{2,4})|((^|\s)(0[1-9]|1[0-9]|2[0-9]|3[0-1]|[1-9])(0[1-9]|1[0-2]|[1-9])([0-9]{2,4})(\s|$))|(([0-9]{2,4})([\\\.\-\/])(0[1-9]|1[0-2]|[1-9])([\\\.\-\/])(0[1-9]|1[0-9]|2[0-9]|3[0-1]|[1-9]))))";
        public static readonly string rxName = @"(?<FIRMANAVN_NAVN>((([A-ZÆØÅ][a-zæøå]+|(AS)|(ASA))([\s\-]?)){2,5}))";
        public static readonly string rxLister = @"(?<LISTER>((lov(er)?(?!([a-zæøåA-ZÆØÅ]+))))|(kostra)|(regnskapspost(er)?\s+?(?!([\dx]+)))|(dokumentpakke(r?))|(erklæring(er?))|(kalkulator(er?))|(mal(er?))|(prosessbeskrivelse(r?))|(note(r?))|(revisjonsberetning(er?))|(sjekkliste(r?))|(skjema(er?))|(lovkommentar(er?))|(regler)|(skatteavtal(er?))|(veiledning(er?)))";
        public static readonly string rxDomstoler = @"(?<DOMSTOLER>((((([A-ZÆØÅ][a-zæøå-]+)|(og)(?=([A-ZÆØÅ][a-zæøå-]+)))([\s\-]?)){1,3})(lagmannsrett|tingrett|jordskifterett|sorenskriveri|forliksråd))|(Høyesterett|Riksrett(en)?|Arbeidsrett(en)?|Skjønnsrett(en)?|Namsrett(en)?))";
        public static readonly string rxNumbers = @"(?<NUMMER>((?<=(^|\s|\())(\d+\W?)+(([kKmM][/s/./,/:/;/?]|tusen|million(er|ar|s)?|billion(s)?|milliard(er|ar)?|mill(\.)?)?)(?=(\s|$))))";
        public static readonly string rxNumbersOnly = @"(?<NUMMER>((\s|^)(\d+[\.\,\s]?)+)(\s|$))";
        public static readonly string rxForkNum = @"(?<FORKORTELSER>((([a-zæøåA-ZÆØÅ]+\.\s)+([\d\s\(\)\-]+))|((([a-zæøåA-ZÆØÅ\.]+)([\s$§]+))(?=(([\d-\\.\/\:]+)|(\([a-zæøåA-ZÆØÅ]+\))))((([\d-\\.\/\:]+)|(([\(\)a-zæøåA-ZÆØÅ]+)))))|([a-zæøåA-ZÆØÅ]+\d+)))";
        public static readonly string rxWords = @"(?<WORDS>([^\s]+))";
        public static readonly string rxFrontEx = @"(?<PREFIX>(£|pund|\$|dollar|\€|euro|USD|EUR|kroner|kr|NOK|MNOK|side|punkt|kapittel|avsnitt|kap|nr|nummer|kl|paragraph|m2|(U|u)tv)|år|årene|aar)";
        public static readonly string rxTrailEx = @"(?<POSTFIX>((prosent|pst|percent|\%)(\s+)?reg(elen|len)|\$|dollar|£|pund|\€|\%|USD|EUR|euro|kroner|kr|NOK|mnok|årsverk|aar|år|års|måneders|months|månedene|mnd|måned|uker|dager|days|dag|døgn|timers|timer|minutter|termin|side|punkt|pkt|mrd|kvm|km|kg|stk|dekar|meter|pct|virkedager|kvadratmeter|liter|promille|cm|GWh|gram|fot|procent|kilometer|min|MUSD|MWh|NOx|gir|Grunnbeløp|G))(?=([\)\s+\.\,\:\;]|$))";
        public static readonly string number0 = @"\d+(((\.|\,|\s|\/)\d+)+)?";
        public static readonly string number4 = @"\d\d\d\d\s[A-ZÆØÅ][a-zæøå][a-zæøå][a-zæøå]+";
        #endregion

        #region //Faste strenger prosedyrer


        public static string NormalizeDato(this string d)
        {
            string value = d.Trim();
            try
            {

                DateTime date = DateTime.Parse(value);
                return String.Format("{0:yyyy-MM-dd}", date);
            }
            catch (Exception)
            {
                value = d.Replace(".", " ");
            }

            try
            {
                DateTime date = DateTime.Parse(value, new CultureInfo("nb-NO", false));
                return String.Format("{0:yyyy-MM-dd}", date);
            }
            catch
            {
                value = d;
            }

            try
            {
                DateTime date = DateTime.ParseExact(value, "ddmmyyyy", new CultureInfo("nb-NO", false));
                return String.Format("{0:yyyy-MM-dd}", date);
            }
            catch
            {
                value = d;
            }

            try
            {
                DateTime date = DateTime.ParseExact(value, "ddmmyy", new CultureInfo("nb-NO", false));
                return String.Format("{0:yyyy-MM-dd}", date);
            }
            catch
            {
                return null;
            }

        }
        public static string getDecimalCharcter(this string num)
        {
            string retval = ",";
            try
            {
                int cidx = 0;
                int didx = 0;
                int idx = 0;
                cidx = num.LastIndexOf(",");
                didx = num.LastIndexOf(".");
                idx = cidx == didx ? -1 : cidx > didx ? cidx : didx;
                if (idx >= 0)
                {
                    string dc = num.Substring(idx, 1);
                    string expr = @"(\" + dc + @"\d{3})";
                    retval = Regex.IsMatch(num, expr) ? "#" : dc;
                }
            }
            catch (Exception)
            {
                retval = string.Empty;
            }
            return retval;
        }
        public static string getDecimal(this string num)
        {
            string dec = string.Empty;
            try
            {
                //int idx = num.IndexOf(",");
                int idx = num.IndexOf(num.getDecimalCharcter());
                dec = idx < 0 ? "#" : num.Substring(idx + 1);
            }
            catch (Exception)
            {
                dec = string.Empty;
            }

            return dec;
        }
        public static string removeDecimal(this string num, string decimalCharacter)
        {
            string dec = string.Empty;
            try
            {
                int idx = num.LastIndexOf("." + decimalCharacter) < 0 ? num.LastIndexOf("," + decimalCharacter) : num.LastIndexOf("." + decimalCharacter);
                dec = idx < 0 ? num : num.Substring(0, idx);
            }
            catch (Exception)
            {
                dec = string.Empty;
            }
            return dec;
        }
        public static bool convert2Number(this string num, string desimal, int multiplier, out string numFormated)
        {
            bool retval = true;
            try
            {
                double number = double.MinValue;
                double val = 0.0;
                double ddesimal = 0.0;
                string form = string.Empty;
                if (double.TryParse("0" + CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator + desimal, out val))
                    ddesimal = val;
                else
                    ddesimal = 0.0;

                num = num.removeBigValueFormats();
                if (double.TryParse(num, out val))
                    number = (val + ddesimal) * (double)multiplier;

                if (desimal.ToString().Length < multiplier.ToString().Length && multiplier > 10)
                {
                    desimal = "#";
                    ddesimal = 0.0;
                    num = number.ToString();
                }

                if (retval == true)
                {
                    if (ddesimal != 0)
                    {
                        if (desimal != "#")
                            ddesimal = (double.Parse(num) + ddesimal) * multiplier;

                        numFormated = ddesimal != 0.0 ? number.ToString(string.Format("F{0}", desimal == "#" ? "" : (desimal.Length > 0 ? desimal.Length.ToString() : ""))) : number.ToString("F0");
                    }
                    else
                        numFormated = num;
                }
                else
                    numFormated = num;
            }
            catch (Exception)
            {
                retval = true;
                numFormated = string.Empty;
            }

            return retval;
        }
        public static string removeBigValueFormats(this string num)
        {
            string ret = num;
            Regex rgx = new Regex(@"\W");
            return rgx.Replace(ret, "");
        }
        public static string normalizeNumber(this string snum)
        {
            //System.Globalization.CultureInfo NorwegianCulture = new System.Globalization.CultureInfo("en-us");//"nb-no");
            try
            {
                string normNum = string.Empty;
                string orgNum = snum;
                snum = snum.Trim(new char[] { '.', ' ', ',', ';', ')', '(', '/' });
                int postfixMultiplier = snum.postfixMultiplier();
                bool replaced = false;
                string result = _rx_dec_no.Replace(snum,
                    delegate (Match m)
                    {
                        replaced = true;
                        if (m.Groups["no_dec"].Success)
                        {
                            return Regex.Replace(m.Groups["number"].Value, @"[\s\,\.]", "") + (m.Groups["dec"].Success ? (Regex.IsMatch(m.Groups["dec"].Value, @"^[0]+$") ? "" : "," + m.Groups["dec"].Value) : "");
                        }
                        else if (m.Groups["no_space_dec"].Success)
                        {
                            return Regex.Replace(m.Groups["number"].Value, @"[\s\,\.]", "") + (m.Groups["dec"].Success ? (Regex.IsMatch(m.Groups["dec"].Value, @"^[0]+$") ? "" : "," + m.Groups["dec"].Value) : "");
                        }
                        else if (m.Groups["no_punkt_dec"].Success)
                        {
                            return Regex.Replace(m.Groups["number"].Value, @"[\s\,\.]", "") + (m.Groups["dec"].Success ? (Regex.IsMatch(m.Groups["dec"].Value, @"^[0]+$") ? "" : "," + m.Groups["dec"].Value) : "");
                        }
                        else if (m.Groups["ge_dec"].Success)
                        {
                            return Regex.Replace(m.Groups["number"].Value, @"[\s\,\.]", "") + (m.Groups["dec"].Success ? (Regex.IsMatch(m.Groups["dec"].Value, @"^[0]+$") ? "" : "," + m.Groups["dec"].Value) : "");
                        }
                        else if (m.Groups["us_dec"].Success)
                        {
                            return Regex.Replace(m.Groups["number"].Value, @"[\s\,\.]", "") + (m.Groups["dec"].Success ? (Regex.IsMatch(m.Groups["dec"].Value, @"^[0]+$") ? "" : "," + m.Groups["dec"].Value) : "");
                        }
                        replaced = false;
                        return "";

                    }
                );
                if (replaced && postfixMultiplier == 1) return result;

                if (postfixMultiplier == 1 && Regex.IsMatch(snum, @"^\d+((\W\d+)+)?$"))
                    return snum;
                //("NOS:" + snum).Dump();
                // find multiplier - 25.000,00 k



                snum = snum.removeMultiplier();

                // find decimal 25.000,00
                string desimal = snum.getDecimal();
                snum = snum.removeDecimal(desimal);

                // convert number
                string numFormated = string.Empty;
                if (snum.convert2Number(desimal, postfixMultiplier, out numFormated))
                    snum = numFormated;

                //("---" + orgNum).Dump();
                //return snum.HyperQIt(orgNum);
            }
            catch (Exception)
            {
                snum = string.Empty;
            }
            return snum;
        }
        public static string normalizeBenevning(this string benevning)
        {
            string expr = benevning.ToLower();
            string normalization = string.Empty;
            if (expr == "nok" || expr == "kr" || expr == "kroner") normalization = "NOK";
            else if (expr == "pund" || expr == "£" || expr == "pound" || expr == "gbp") normalization = "GBP";
            else if (expr == "euro" || expr == "€" || expr == "eur") normalization = "EUR";
            else if (expr == "dollar" || expr == "$" || expr == "usd") normalization = "USD";
            else if (expr == "prosent" || expr == "pst" || expr == "%") normalization = "%";
            else if (expr == "paragraf" || expr == "paragraph" || expr == "§") normalization = "§";
            else if (expr == "grunnbeløp" || expr == "g") normalization = "G";
            else if (expr == "år" || expr == "års" || expr == "aar" || expr == "year" || expr == "årene") normalization = "år";
            else if (expr == "mnd" || expr == "måned" || expr == "måneders" || expr == "months") normalization = "mnd";
            else normalization = benevning;

            return normalization;
        }
        public static int postfixMultiplier(this string num)
        {
            //("xx" + num +"xx").Dump();	
            int multiplier = 1;
            if (num.ToLower().Contains("tusen"))
                multiplier = 1000;
            else if (num.ToLower().Contains("milliard")
                    || num.ToLower().Contains("mill")
                    || num.ToLower().Contains("mill."))
                multiplier = 1000000;
            else if (num.ToLower().Contains("milliard"))
                multiplier = 1000000000;
            else if (num.ToLower().Contains("k"))
                multiplier = 1000;
            else if (num.ToLower().Contains("m"))
                multiplier = 1000000;
            //else if (num.ToLower().Contains("g"))
            //    multiplier = 1000000000;
            return multiplier;
        }
        public static string removeMultiplier(this string num)
        {
            string ret = num;
            if (num.ToLower().Contains("tusen") ||
                    num.ToLower().Contains("million") ||
                    num.ToLower().Contains("milliard") ||
                    num.ToLower().Contains("mill.") ||
                    num.ToLower().Contains("mill"))
            {
                Regex rgx = new Regex(@"(tusen|million(er)?|milliard(er)?|mill(\.)?)");
                ret = rgx.Replace(num.ToLower(), "").Trim();
            }
            else if (num.ToLower().Contains("k"))
                ret = num.ToLower().Replace("k", "").Trim();
            else if (num.ToLower().Contains("m"))
                ret = num.ToLower().Replace("m", "").Trim();
            else if (num.ToLower().Contains("g"))
                ret = num.ToLower().Replace("g", "").Trim();

            //		("(" + ret + ")").Dump();

            return ret;
        }

        
        private class ValuePair
        {
            public string key { get; set; }
            public string value { get; set; }
        }
       
       
        public static string removeDiacritics(string str)
        {
            var sb = new StringBuilder();
            char p = (char)96;
            foreach (char c in str.Normalize(NormalizationForm.FormD))
            {
                if (c == 778 && p == 97)
                {
                    sb.Append(c);
                }
                else
                {
                    if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    {
                        sb.Append(c);
                    }
                }
                p = c;
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
        public static string ReplaceNonLetter(this string s)
        {
            s = removeDiacritics(s);
            Regex regex = new Regex(@"[^a-zæøåA-ZÆØÅ0-9]");
            string result = regex.Replace(s,
                delegate (Match m)
                {
                    switch (m.Value)
                    {
                        case " ": return "qspaceq";
                        case "-": return "qhypq";
                        case ".": return "qpunktq";
                        case "/": return "qslachq";
                        case ",": return "qcommaq";
                        case "%": return "qprosq";
                        default: return m.Value;
                    }
                }
            );
            return result;
        }
        #endregion

        #region//Norsk stemming variabler
        public static readonly string s_endings = "bcdfghjlmnoprtvyz";
        public static readonly string vokaler = "aeiouyæåø";
        public static readonly string non_vokaler = "bcdfghjklmnpqrstvwxz";

        public static readonly List<string> end3 = "led;tik;eid;est;ist;met;ent;ill;ell;før;ndl;lei;lag;ang;end;att;ikk;lær;sei;ytt;nst;ald;rsk;tak;mak;nik;ryt;løp;old;riv;tyr;inn;ast;jør;ren;akt;ruk;and;isk;ekk;ian;art;øst;ett;eng;elg;rik;rtn;ank;hav;sik;feb;eat;ukk;stn;ekt;fis;egg;eld;jeg;red;enn;ind;ens;kan;itt;int;bær;opp;ulv;vis;off;ygg;egl;emp;les;tag;alt;yrk;tøv;utt;ilt;tal;ost;ebo;ikt;lsk;egn;lyg;eil;okt;igg;søk;kap;mal;lit;jøp;ass;asj;ser;idl;giv;ans;mik;ort;ogg;ndr;rav;fib;ykk;aml;und;org;nsk;akk;far;erk;ugg;eei;ord;kst;ipp;ekl;nøv;nes;løs;arm;ain;tin;elp;set;lev;app;orm;ope;jær;løv;eep;opt;ran;amp;ørg;tan;røv;lyv;iff;var;rei;gik;ebu;lad;uss;jen;yss;siv;add;ern;rag;udd;øyk;epp;oks;rri;ift;pik;vit;ugl;rod;okk;iat;ask;ysk;lan;ynd;nus;luk;man;rep;pet;urg;ann;bit;iss;øys;lip;ott;rev;emm;lin;uld;ikl;yst;bad;ign;imm;bed;sin;sug;oni;eli;beg;ønn;ust;can;sid;idd;jøl;unk;dei;ani;ean;erm;umm;lst;rap;lid;ter;ypp;kok;ali;øft;vik;mes;enk;lik;evn;kif;aed;øyp;eis;olk;amm;ørr;edd;yan;ksl;nev;tøp;ræd;oil;ein;jag;ynn;pan;øyt;del;rak;lon;rat;ngl;par;obb;ønd;rin;søy;ørk;ail;tet;mer;ild;ølg;ban;jan;mat;erg;ant;nyt;rti;ldr;nag;ekn;uan;økk;vai;pis;adi;oll;rab;res;byd;esi;omm;jet;rid;sku;ømm;ont;iei;nsl;dan;nei;fri;fei;rip;apr;røk;ukt;aff;rsl".Split(';').ToList();
        public static readonly string _sword = "justitiarius|amanuensis|laudabilis|pretensiøs|enetasjes|rangsøles|ambisiøs|disputas|eksersis|enskjærs|instruks|knickers|ortodoks|parentes|religiøs|tilfreds|apropos|bakkels|binders|chassis|enhånds|ensteds|etsteds|fysikus|harpiks|haubits|nikkers|praksis|provins|respons|ressurs|titalls|trenges|virtuos|visitas|adonis|amorøs|ansjos|arktis|blings|forlis|gymnas|justis|kaktus|kjangs|kompis|konsis|kultus|lykkes|nervøs|presis|radius|resurs|seriøs|shorts|sirkus|snacks|spleis|status|taksis|tennis|trives|tyggis|arves|avers|basis|blits|bonus|brems|chips|dings|dosis|dreis|drops|eders|eldes|fjols|frels|fødes|hands|klaus|klips|knaus|korps|kreps|krets|langs|ledes|leies|longs|melis|minus|notis|odiøs|overs|polis|prins|props|ritus|sinus|skyts|slags|slips|sveis|tanks|tiårs|undas|anis|anus|avis|beis|boks|bris|brus|børs|deis|dels|dras|enes|epos|eros|erts|fiks|fjes|fjøs|flis|fres|glas|glis|gods|gras|gris|grus|gufs|hams|haus|heis|hets|hufs|høns|jafs|kaos|kors|kras|kris|krus|krås|kuls|kurs|laps|laus|liks|meis|mers|miks|mons|mops|niks|nips|odds|peis|pels|pers|pils|pors|pris|puls|rams|raps|reis|riks|rips|røys|sams|sats|saus|spis|stas|ters|tids|tips|tufs|veps|vers|vits|voks|aks|als|ans|ars|bas|bis|bus|bås|dis|dus|døs|eks|ens|fis|fus|gas|gys|gås|hus|jus|kas|kis|kos|las|los|lus|lys|løs|lås|mas|mis|mos|mus|nes|nos|nys|nøs|oms|pas|pes|pus|pøs|ras|res|ris|ros|rus|rås|sas|ses|sos|sus|tøs|ufs|vas|vis|vås|øks";
        public static readonly Regex _send = new Regex("(" + _sword + ")$");
        public static readonly string _stop = "kvarhelst|forsøke|hvilken|hvordan|hvorfor|korleis|tilbake|deires|dykkar|eneste|enhver|gjorde|hennar|hennes|hossen|hvilke|kvifor|mellom|riktig|skulle|stille|andre|begge|blitt|bruke|deira|denne|deres|dette|disse|eller|elles|etter|fordi|først|gjøre|hadde|henne|honom|ikkje|ingen|inkje|innen|korso|kunne|mange|medan|meget|mykje|nokon|nokor|nokre|samme|sidan|siden|slutt|somme|start|under|varte|verdi|verte|ville|alle|bare|blei|blir|bort|både|deim|dere|ditt|dykk|eitt|fram|hans|hoss|hvem|hver|hvis|hvor|ikke|ingi|inni|kvar|kven|lage|lang|like|mens|mest|mine|mitt|måte|noen|noka|noko|også|over|rett|selv|sine|sist|sitt|sjøl|skal|slik|somt|sånn|uten|vart|vere|vite|vore|vors|vort|være|vært|ble|bli|bra|båe|deg|dei|dem|den|der|det|din|dit|ein|eit|ene|enn|ett|for|fra|før|han|har|her|hit|hjå|hoe|hun|hva|inn|jeg|kan|kom|kun|kva|kvi|lik|man|med|meg|men|mer|min|mot|mye|ned|nei|noe|når|opp|oss|seg|sia|sin|som|tid|til|upp|var|ved|vil|vår|at|av|da|de|di|du|då|eg|en|er|et|få|gå|ha|ho|ja|me|mi|må|no|nå|og|om|på|si|so|så|um|ut|vi|i|å";
        public static readonly Regex _stopword = new Regex("^(" + _stop + ")$");
        #endregion

        #region //Stemming funksjoner 
        public static string Reverse(this string text)
        {
            if (text == null) return null;

            // this was posted by petebob as well 
            char[] array = text.ToCharArray();
            Array.Reverse(array);
            return new String(array);
        }
        public static string RemoveDoubleKons(this string s)
        {
            Regex rx = new Regex("(bb|dd|ff|gg|kk|ll|nn|mm|pp|rr|ss|tt)(?=$)");
            s = rx.Replace(s,
                delegate (Match m)
                {
                    return m.Value.FirstOrDefault().ToString();
                }
            );
            return s;
        }
        public static string RemoveSpace(this string s)
        {
            Regex split = new Regex(@"\s+");
            s = split.Replace(s, " ");
            return s;
        }
        public static string Stem(this string s)
        {
            //s = removeDiacritics(s);

            s = Regex.Replace(s.ToLower(), @"[^a-zøæå0-9]", " ");
            s = Regex.Replace(s, @"\s+", " ");
            s = Regex.Replace(s, @"regel", "regl");
            s = Regex.Replace(s, @"entrepenør", "entreprenør");

            string legalEnding = @"(?=($|\s|\.|\,|\]|\)|\:|\;))";
            //UT//lov elov slov hetslov lig

            //ede edende edes edt edte
            //addendum addendumet
            //egium
            //ensede enset ensete ensetere ensetest enseteste
            //igede igetere igetest igeteste
            //een eer eene

            //itet iteten itetene iteter
            //ista iste
            //olkede olketere olketest olketeste
            //eged egede eget egete egetere egetest egeteste
            //edede edet edete edetere edetest edeteste
            //ammetere ammetest ammeteste ammede
            //åpen åpene åpenere åpenest åpeneste åpenet åpent åpna åpne åpnene åpnere åpnest åpneste
            //eierer eierere eiereren eiererene eiererer eiererne
            //abelt ablele ablelene ableler ablelere ablelest ableleste
            //umra umre umrene umrer umrere umrest umreste
            //økede øket økete økte økt


            //string _endelser = "økede øket økete økte økt umra umre umrene umrer umrere umrest umreste abelt abele abelene abeler abelere abelest abeleste era ete eierer eierere eiereren eiererene eiererer eiererne ammetere ammetest ammeteste ammede åpen åpene åpenere åpenest åpeneste åpenet åpent åpna åpne åpnene åpnere åpnest åpneste edede edet edete edetere edetest edeteste eged egede eget egete egetere egetest egeteste olkede olketere olketest olketeste olkede aa ista iste itet iteten itetene iteter een eer eene igede igetere igetest igeteste ensede enset ensete ensetere ensetest enseteste egium art endum endumet ede edende edes edt edte eres erende istra istre istrene istret etre etrene etrer endre endrene endrer engre alt erten ertene erter ete else elsene elser elsen ern erte ert eret erer erne eren ere erene aen aer aene aet est st este esten estene ester esta ste heta leg eleg ig eig elig els vt dt erte ert s a  e  ede  ande  ende  ane  ene  hetene  en  heten  ar  er  heter  as  es  edes  endes  enes  hetenes  ens  hetens  ers  ets  et  het  ast";
            string _endelser = "ernes erar erande ingens ggjarar ggjande stridigt ningens igete økede øket økete økte økt umra umre umrene umrer umrere umrest umreste abelt abele abelene abeler abelere abelest abeleste era ete eierer eierere eiereren eiererene eiererer eiererne ammetere ammetest ammeteste ammede åpen åpene åpenere åpenest åpeneste åpenet åpent åpna åpne åpnene åpnere åpnest åpneste edede edet edete edetere edetest edeteste eged egede eget egete egetere egetest egeteste olkede olketere olketest olketeste olkede aa ista iste itet iteten itetene iteter een eer eene igede igetere igetest igeteste ensede enset ensete ensetere ensetest enseteste egium art endum endumet ede edende edes edt edte eres erende istra istre istrene istret etre etrene etrer endre endrene endrer engre alt erten ertene erter ete else elsene elser elsen ern erte ert eret erer erne eren ere erene aen aer aene aet est st este esten estene ester esta ste heta leg eleg ig eig elig els vt dt erte ert s a  e  ede  ande  ende  ane  ene  hetene  en  heten  ar  er  heter  as  es  edes  endes  enes  hetenes  ens  hetens  ers  ets  et  het  ast";
            string step0 =
                @"(?<step0>" +
                @"(" +
                "ens" +
                "))";



            //string sStep1a = @"(?<front>((?<r1>(([a-zæøå]+?([" + vokaler + "][" + non_vokaler + "])?)))+?))"
            //string sStep1a = @"(?<front>([a-zæøå]+?([" + vokaler + "][" + non_vokaler + "]+)))";
            //string sStep1a = @"(?<front>([a-zæøå]+?([" + vokaler + "][" + non_vokaler + "]+?)))"
            string sStep0 = @"(?<front>([a-zæøå][a-zæøå]+?))"
            + step0
            + legalEnding;



            Regex rStep0 = new Regex(sStep0);

            s = rStep0.Replace(s,
                delegate (Match m)
                {
                    if ((m.Groups["front"].Success ? m.Groups["front"].Value.Length : 0) >= 2)
                    {
                        if (m.Groups["step0"].Success && m.Groups["front"].Value.Length >= 2)
                        {
                            switch (m.Groups["step0"].Value)
                            {
                                case "ens":
                                    return m.Groups["front"].Value + "en";
                            }
                        }
                    }
                    return m.Value;
                }
            );

            string step1_a =
                @"(?<step1_a>" +
                @"(" +
                    _endelser
                    .Split(' ')
                    .Where(p => p.Trim() != "")
                    .GroupBy(p => p)
                    .Select(p => p.Key)
                    .OrderByDescending(p => p.Length)
                    .Select(p => p.StartsWith("est") ? @"(?<![fm])" + p : p)
                    .Select(p => p == "s" ? @"(?<=([" + s_endings + "]|[" + non_vokaler + "]k))" + p : p)
                    .Select(p => p == "ar" ? @"(?<![pf])" + p : p)
                    .Select(p => p == "ete" ? @"(?<=((bb|dd|ff|gg|kk|ll|nn|mm|pp|rr|ss|tt)|(l)))" + p : p)
                    .Select(p => p == "era" ? @"(?<=(bb|dd|ff|gg|kk|ll|nn|mm|pp|rr|ss|tt))" + p : p)
                    //.Select(p => p == "ens" ? @"(?<!(lat|int|fa|akt|ati|dem|eag|edi|eni|ent|erg|ert|ick|iem|ike|ing|isi|lsk|man|min|mot|nit|nyd|olk|udi|uff|ulg|vid|v|u|aft|s|ang|ud|nd|sc|b|lig|nons|kad|dags|sed|sid|sist|tend|tin|pot|verg|sekv|frekv|lis|r|gr|l|p|gre))" + p : p)
                    .Select(p => p == "ens" ? @"(?<!(lat|int|fa|akt|ati|dem|eag|edi|eni|ent|erg|ert|ick|iem|ike|ing|isi|lsk|man|min|mot|nit|nyd|olk|udi|uff|ulg|vid|v|u|aft|s|ang|ud|nd|sc|b|lig|nons|kad|dags|sed|sid|sist|tend|tin|pot|verg|sekv|frekv|lis|r|gr|gre))" + p : p)
                    .StringConcatenate("|") +
                "))";


            string sStep1a = @"(?<front>([a-zæøå][a-zæøå]+?))"
            + step1_a
            + legalEnding;


            Regex rStep1a = new Regex(sStep1a);
            s = rStep1a.Replace(s,
                delegate (Match m)
                {

                    switch (m.Value)
                    {
                        case "esta":
                        case "efta":
                        case "edta":
                            return m.Value;
                    }

                    if (_stopword.IsMatch(m.Value))
                    {
                        return m.Value;
                    }

                    if ((m.Groups["front"].Success ? m.Groups["front"].Value.Length : 0) <= 3)
                    {

                        if (m.Groups["step1_a"].Success && m.Groups["front"].Value.Length <= 3)
                        {
                            switch (m.Groups["step1_a"].Value)
                            {
                                case "ernes":
                                    switch (m.Groups["front"].Value)
                                    {
                                        case "fj":
                                            return m.Groups["front"].Value + "ern";
                                    }
                                    return m.Groups["front"].Value.RemoveDoubleKons();
                                case "erande":
                                    return m.Groups["front"].Value.RemoveDoubleKons();
                                case "ingens":
                                    return m.Groups["front"].Value + "ing";
                                case "ggjarar":
                                case "ggjande":
                                    return m.Groups["front"].Value + "g";
                                case "stridigt":
                                    return m.Groups["front"].Value + "strid";
                                case "ningens":
                                    return m.Groups["front"].Value + "ning";
                                case "økede":
                                case "øket":
                                case "økete":
                                case "økte":
                                case "økt":
                                    return m.Groups["front"].Value + "øk";
                                case "umra":
                                case "umre":
                                case "umrene":
                                case "umrer":
                                case "umrere":
                                case "umrest":
                                case "umreste":
                                    //return m.Groups["front"].Value + "umm";
                                    return m.Groups["front"].Value + "um";
                                case "abelt":
                                case "abele":
                                case "abelene":
                                case "abeler":
                                case "abelere":
                                case "abelest":
                                case "abeleste":
                                    return m.Groups["front"].Value + "abel";
                                case "eierer":
                                case "eierere":
                                case "eiereren":
                                case "eiererene":
                                case "eiererer":
                                case "eiererne":
                                    return m.Groups["front"].Value + "eier";
                                case "ammetere":
                                case "ammetest":
                                case "ammeteste":
                                case "ammede":
                                    //return m.Groups["front"].Value + "amm";
                                    return m.Groups["front"].Value + "am";
                                case "åpen":
                                case "åpene":
                                case "åpenere":
                                case "åpenest":
                                case "åpeneste":
                                case "åpenet":
                                case "åpent":
                                case "åpna":
                                case "åpne":
                                case "åpnene":
                                case "åpnere":
                                case "åpnest":
                                case "åpneste":
                                    return m.Groups["front"].Value + "åpen";
                                case "edede":
                                case "edet":
                                case "edete":
                                case "edetere":
                                case "edetest":
                                case "edeteste":
                                    return m.Groups["front"].Value + "ed";
                                case "eged":
                                case "egede":
                                case "eget":
                                case "egete":
                                case "egetere":
                                case "egetest":
                                case "egeteste":
                                    return m.Groups["front"].Value + "eg";
                                case "olkede":
                                case "olketere":
                                case "olketest":
                                case "olketeste":
                                    return m.Groups["front"].Value + "olk";
                                case "ista":
                                case "iste":
                                    return m.Groups["front"].Value + "ist";
                                case "itet":
                                case "iteten":
                                case "itetene":
                                case "iteter":
                                    return m.Groups["front"].Value + "itet";
                                case "een":
                                case "eene":
                                case "eer":
                                    return m.Groups["front"].Value;
                                case "igede":
                                case "igete":
                                case "igetere":
                                case "igetest":
                                case "igeteste":
                                    return m.Groups["front"].Value + "ig";
                                case "ensede":
                                case "enset":
                                case "ensete":
                                case "ensetere":
                                case "ensetest":
                                case "enseteste":
                                    return m.Groups["front"].Value + "ens";
                                case "egium":
                                    return m.Groups["front"].Value + "egi";
                                case "art":
                                    return m.Groups["front"].Value + "ar";
                                case "endum":
                                case "endumet":
                                    return m.Groups["front"].Value + "end";
                                case "era":
                                    if (Regex.IsMatch(m.Groups["front"].Value, @"(bb|dd|ff|gg|kk|ll|nn|mm|pp|rr|ss|tt)$"))
                                    {
                                        return m.Groups["front"].Value.RemoveDoubleKons();
                                    }
                                    return m.Groups["front"].Value + "er";
                                case "ete":
                                    if (Regex.IsMatch(m.Groups["front"].Value, @"(bb|dd|ff|gg|kk|ll|nn|mm|pp|rr|ss|tt)$"))
                                    {
                                        return m.Groups["front"].Value.RemoveDoubleKons();
                                    }
                                    else if ((m.Groups["front"].Value.Length > 1 && m.Groups["front"].Value.EndsWith("l")))
                                    {
                                        return m.Groups["front"].Value;
                                    }
                                    return m.Groups["front"].Value + "et";
                                case "ede":
                                    if (m.Groups["front"].Value.EndsWith("sl"))
                                    {
                                        return m.Groups["front"].Value + "ed";
                                    }
                                    else if (Regex.IsMatch(m.Groups["front"].Value, @"(bb|dd|ff|gg|kk|ll|nn|mm|pp|rr|ss|tt)$"))
                                    {
                                        return m.Groups["front"].Value.RemoveDoubleKons();
                                    }
                                    else if ((m.Groups["front"].Value.Length > 1 && m.Groups["front"].Value.EndsWith("l")))
                                    {
                                        return m.Groups["front"].Value;
                                    }
                                    return m.Groups["front"].Value + "ed";
                                case "edende":
                                case "edes":
                                case "edt":
                                case "edte":
                                    return m.Groups["front"].Value + "ed";
                                case "istra":
                                case "istre":
                                case "istrene":
                                case "istret":
                                    return m.Groups["front"].Value + "ist";
                                case "etre":
                                case "etrene":
                                case "etrer":
                                    return m.Groups["front"].Value + "et";
                                case "endre":
                                case "endrene":
                                case "endrer":
                                    return m.Groups["front"].Value + "end";
                                case "engre":
                                    return m.Groups["front"].Value + "eng";
                                case "alt":
                                    return m.Groups["front"].Value + "al";
                                //case "este":
                                //case "esten":
                                //case "estene":
                                //case "ester":
                                //    return m.Groups["front"].Value + "est";
                                case "er":
                                case "ere":
                                case "eren":
                                case "erene":
                                case "erer":
                                case "eret":
                                case "ern":
                                case "erne":
                                case "ers":
                                case "ert":
                                case "erte":
                                case "erten":
                                case "ertene":
                                case "erter":
                                case "eres":
                                case "erende":
                                    switch (m.Groups["front"].Value)
                                    {
                                        case "ell":
                                        case "all":
                                            return m.Groups["front"].Value + "er";
                                        case "fj":
                                        case "gj":
                                        case "hj":
                                            return m.Groups["front"].Value + "ern";
                                        case "uk":
                                        case "øk":
                                        case "yt":
                                        case "tr":
                                            return m.Groups["front"].Value + "e";
                                        case "ei":
                                        case "ab":
                                        case "li":
                                        case "iv":
                                        case "ot":
                                        case "pe":
                                        case "se":
                                        case "hv":
                                        case "fl":
                                            return m.Groups["front"].Value + "er";
                                        case "bo":
                                        case "ny":
                                        case "si":
                                            return m.Groups["front"].Value;
                                    }
                                    return m.Groups["front"].Value.RemoveDoubleKons();
                                case "dt":
                                    return m.Groups["front"].Value + "d";
                                case "vt":
                                    return m.Groups["front"].Value + "v";
                                case "s":
                                    if (_send.IsMatch(m.Value))
                                    {
                                        return m.Value;
                                    }
                                    break;
                            }
                        }

                        switch (m.Groups["front"].Value)
                        {
                            case "øk":
                                return m.Groups["front"].Value + "e";
                            case "søk":
                            case "møt":
                            case "vei":
                            case "gav":
                            case "not":
                            case "ank":
                            case "add":
                            case "adl":
                            case "age":
                            case "agn":
                                return m.Groups["front"].Value;
                            default:

                                switch (m.Groups["front"].Value.Length)
                                {
                                    case 1:
                                    case 2:
                                    case 3:
                                        if (m.Groups["step1_a"].Value.StartsWith("en"))
                                        {
                                            return m.Groups["front"].Value + "en";
                                        }

                                        break;
                                }
                                return m.Value;
                        }

                    }

                    if (m.Groups["step1_a"].Success)
                    {

                        switch (m.Groups["step1_a"].Value)
                        {
                            case "økede":
                            case "øket":
                            case "økete":
                            case "økte":
                            case "økt":
                                return m.Groups["front"].Value + "øk";
                            case "umra":
                            case "umre":
                            case "umrene":
                            case "umrer":
                            case "umrere":
                            case "umrest":
                            case "umreste":
                                //return m.Groups["front"].Value + "umm";
                                return m.Groups["front"].Value + "um";
                            case "abelt":
                            case "abele":
                            case "abelene":
                            case "abeler":
                            case "abelere":
                            case "abelest":
                            case "abeleste":
                                return m.Groups["front"].Value + "abel";
                            case "eierer":
                            case "eierere":
                            case "eiereren":
                            case "eiererene":
                            case "eiererer":
                            case "eiererne":
                                return m.Groups["front"].Value + "eier";
                            case "ammetere":
                            case "ammetest":
                            case "ammeteste":
                            case "ammede":
                                //return m.Groups["front"].Value + "amm";
                                return m.Groups["front"].Value + "am";
                            case "åpen":
                            case "åpene":
                            case "åpenere":
                            case "åpenest":
                            case "åpeneste":
                            case "åpenet":
                            case "åpent":
                            case "åpna":
                            case "åpne":
                            case "åpnene":
                            case "åpnere":
                            case "åpnest":
                            case "åpneste":
                                return m.Groups["front"].Value + "åpen";
                            case "edede":
                            case "edet":
                            case "edete":
                            case "edetere":
                            case "edetest":
                            case "edeteste":
                                return m.Groups["front"].Value + "ed";
                            case "eged":
                            case "egede":
                            case "eget":
                            case "egete":
                            case "egetere":
                            case "egetest":
                            case "egeteste":
                                return m.Groups["front"].Value + "eg";
                            case "olkede":
                            case "olketere":
                            case "olketest":
                            case "olketeste":
                                return m.Groups["front"].Value + "olk";
                            case "ista":
                            case "iste":
                                return m.Groups["front"].Value + "ist";
                            case "itet":
                            case "iteten":
                            case "itetene":
                            case "iteter":
                                return m.Groups["front"].Value + "itet";
                            case "een":
                            case "eene":
                            case "eer":
                                return m.Groups["front"].Value;
                            case "igede":
                            case "igete":
                            case "igetere":
                            case "igetest":
                            case "igeteste":
                                return m.Groups["front"].Value + "ig";
                            case "ensede":
                            case "enset":
                            case "ensete":
                            case "ensetere":
                            case "ensetest":
                            case "enseteste":
                                return m.Groups["front"].Value + "ens";
                            case "egium":
                                return m.Groups["front"].Value + "egi";
                            case "art":
                                return m.Groups["front"].Value + "ar";
                            case "endum":
                            case "endumet":
                                return m.Groups["front"].Value + "end";
                            case "era":
                                if (Regex.IsMatch(m.Groups["front"].Value, @"(bb|dd|ff|gg|kk|ll|nn|mm|pp|rr|ss|tt)$"))
                                {
                                    return m.Groups["front"].Value.RemoveDoubleKons();
                                }
                                return m.Groups["front"].Value + "er";
                            case "ete":
                                if (Regex.IsMatch(m.Groups["front"].Value, @"(bb|dd|ff|gg|kk|ll|nn|mm|pp|rr|ss|tt)$"))
                                {
                                    return m.Groups["front"].Value.RemoveDoubleKons();
                                }
                                else if (m.Groups["front"].Value.Length > 1 && m.Groups["front"].Value.EndsWith("l"))
                                {
                                    return m.Groups["front"].Value;
                                }
                                return m.Groups["front"].Value + "et";
                            case "ede":
                                if (m.Groups["front"].Value.EndsWith("sl"))
                                {
                                    return m.Groups["front"].Value + "ed";
                                }
                                else if (Regex.IsMatch(m.Groups["front"].Value, @"(bb|dd|ff|gg|kk|ll|nn|mm|pp|rr|ss|tt)$"))
                                {
                                    return m.Groups["front"].Value.RemoveDoubleKons();
                                }
                                else if (m.Groups["front"].Value.Length > 1 && m.Groups["front"].Value.EndsWith("l"))
                                {
                                    return m.Groups["front"].Value;
                                }
                                return m.Groups["front"].Value + "ed";
                            case "edende":
                            case "edes":
                            case "edt":
                            case "edte":
                                return m.Groups["front"].Value + "ed";
                            case "istra":
                            case "istre":
                            case "istrene":
                            case "istret":
                                return m.Groups["front"].Value + "ist";
                            case "etre":
                            case "etrene":
                            case "etrer":
                                return m.Groups["front"].Value + "et";
                            case "endre":
                            case "endrene":
                            case "endrer":
                                return m.Groups["front"].Value + "end";
                            case "engre":
                                return m.Groups["front"].Value + "eng";
                            case "alt":
                                return m.Groups["front"].Value + "al";
                            //case "este":
                            //case "esten":
                            //case "estene":
                            //case "ester":
                            //    return m.Groups["front"].Value + "est";
                            case "er":
                            case "ere":
                            case "eren":
                            case "erene":
                            case "erer":
                            case "eret":
                            case "ern":
                            case "erne":
                            case "ers":
                            case "ert":
                            case "erte":
                            case "erten":
                            case "ertene":
                            case "erter":
                            case "eres":
                            case "erende":
                                switch (m.Groups["front"].Value)
                                {
                                    case "ab":
                                    case "li":
                                        return m.Groups["front"].Value + "er";
                                }
                                return m.Groups["front"].Value.RemoveDoubleKons(); ;
                            case "ggjarar":
                            case "ggjande":
                                return m.Groups["front"].Value + "g";
                            case "dt":
                                return m.Groups["front"].Value + "d";
                            case "vt":
                                return m.Groups["front"].Value + "v";
                            case "s":
                                if (_send.IsMatch(m.Value))
                                {
                                    return m.Value;
                                }
                                return Regex.Replace(m.Value, m.Groups["step1_a"].Value + "$", "").RemoveDoubleKons();
                            case "ste":
                                return m.Groups["front"].Value + "st";
                            case "st":
                                return m.Value;
                            default:
                                return Regex.Replace(m.Value, m.Groups["step1_a"].Value + "$", "").RemoveDoubleKons();
                        }
                    }
                    return m.Value.RemoveDoubleKons();
                }
            );

            Regex stepLast = new Regex(@"(^|\s)[a-zæøå]{2,}(bb|dd|ff|gg|kk|ll|nn|mm|pp|rr|ss|tt)" + legalEnding);
            s = stepLast.Replace(s,
                delegate (Match m)
                {
                    return m.Value.Substring(0, m.Value.Length - 1);
                }
            );

            return s;
        }

        #endregion

    }
}
