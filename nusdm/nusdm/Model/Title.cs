using Newtonsoft.Json;
using nusdm.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace nusdm
{
    public class Title
    {
        public string TitleId { get; set; }
        public string TitleIdHexified { get; set; }

        public string TitleKey { get; set; }
        public string TitleKeyHexified { get; set; }

        public string Name { get; set; }
        public string NameSanitized { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        public string TitleType { get; set; }

        [JsonConstructor]
        public Title(string titleId, string titleKey, string name, string region)
        {
            if (!String.IsNullOrEmpty(titleId))
            {
                TitleId = SanitizeString(titleId.ToUpper());
                TitleIdHexified = FormatHexString(TitleId);
            }

            if (!String.IsNullOrEmpty(titleKey))
            {
                TitleKey = titleKey.ToUpper();
                TitleKeyHexified = FormatHexString(TitleKey);
            }
            else
            {
                TitleKey = Keygen.GenerateKey(TitleId);
                TitleKeyHexified = FormatHexString(TitleKey);
            }

            if (!String.IsNullOrEmpty(name))
            {
                Name = name;
                NameSanitized = SanitizeString(Name);
            }

            if (String.IsNullOrEmpty(region))
            {
                Region = "UKN";
            }
            else
            {
                Region = region.Trim();
            }

            if (TitleId.StartsWith("00050000", StringComparison.OrdinalIgnoreCase))
            {
                TitleType = "GAME";
            }
            else if (TitleId.StartsWith("00050002", StringComparison.OrdinalIgnoreCase))
            {
                TitleType = "DEMO";
            }
            else if (TitleId.StartsWith("0005000C", StringComparison.OrdinalIgnoreCase))
            {
                TitleType = "DLC";
            }
            else if (TitleId.StartsWith("0005000E", StringComparison.OrdinalIgnoreCase))
            {
                TitleType = "UPDATE";
            }
            else if (TitleId.StartsWith("00050010", StringComparison.OrdinalIgnoreCase))
            {
                TitleType = "SYSAPP";
            }
            else if (TitleId.StartsWith("0005001B", StringComparison.OrdinalIgnoreCase))
            {
                TitleType = "SYSDAT";
            }
            else if (TitleId.StartsWith("00050030", StringComparison.OrdinalIgnoreCase))
            {
                TitleType = "APPLET";
            }
            else
            {
                TitleType = "UNKNOWN";
            }
        }

        private string SanitizeString(string s)
        {
            return String.Join("", s.Split(Path.GetInvalidFileNameChars())).Trim();
        }

        public override string ToString()
        {
            return $"{TitleId} {Region} {Name} {TitleType} {TitleKey}";
        }


        private string FormatHexString(string s)
        {
            if (s.Contains(" "))
            {
                return s;
            }
            for (int i = 2; i < s.Length; i += 3)
            {
                s = s.Insert(i, " ");
            }
            return s;
        }
    }
}
