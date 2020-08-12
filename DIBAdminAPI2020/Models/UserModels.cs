using DIBAdminAPI.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DIBAdminAPI.Models
{
    public class UserSession
    {
        [JsonProperty("sessionid")]
        public string session_id { get; set; }
        public int KundeId { get; set; }
        public string KundeNavn { get; set; }
        public int KontoId { get; set; }
        public string KontoNavn { get; set; }
        [JsonProperty("user")]
        public string Epost { get; set; }
        [JsonProperty("username")]
        public string Navn { get; set; }
        public int RolleId { get; set; }
        [JsonProperty("rolle")]
        public string Rolle { get; set; }
        public int Rettighet { get; set; }
        [JsonProperty("userkey")]
        public string BrukerKey { get; set; }
        public int SpraakId { get; set; }
        public string Kundegruppe { get; set; }
        public string Produktpakke { get; set; }
        public DateTime LogonDatoTidspunkt { get; set; }
        [JsonConverter(typeof(BoolConverter))]
        public bool Aktiv { get; set; }
        public DateTime Opprettet { get; set; }
        public DateTime? LisensTil { get; set; }
        [JsonConverter(typeof(BoolConverter))]
        public bool Demokonto { get; set; }
        public int ApplicationId { get; set; }

        public bool KrevSSO { get; set; }
        public string EpostSuffiks { get; set; }
        public string Initials
        {
            get
            {
                try
                {
                    if (!string.IsNullOrEmpty(Navn) && !string.IsNullOrWhiteSpace(Navn))
                    {
                        Navn = Regex.Replace(Navn, @"\s+", " ");
                        var names = Navn.Split(new char[] { ' ', '-' });
                        var initials = "";

                        if (names.Any())
                        {
                            for (int i = 0; i < names.Length; i++)
                            {
                                int startIdx = 0;
                                if (names[i] != "")
                                {
                                    while (startIdx < names[i].Length && !char.IsLetter(names[i], startIdx))
                                        startIdx++;

                                    if (startIdx < names[i].Length)
                                        initials += names[i].Substring(startIdx, 1);
                                }
                            }
                        }
                        return initials.ToUpper();
                    }
                    return string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
    }
}
