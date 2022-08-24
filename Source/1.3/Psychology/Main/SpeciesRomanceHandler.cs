using System;
using System.Collections.Generic;
using HugsLib.Settings;

namespace Psychology
{
    public class SpeciesRomanceHandler : SettingHandleConvertible
    {
        public Dictionary<string, SpeciesRomanceSettings> inner = new Dictionary<string, SpeciesRomanceSettings>();
        public Dictionary<string, SpeciesRomanceSettings> InnerList { get { return inner; } set { inner = value; } }

        public override void FromString(string settingValue)
        {
            inner = new Dictionary<string, SpeciesRomanceSettings>();
            if (!settingValue.Equals(string.Empty))
            {
                foreach (string str in settingValue.Split('|'))
                {
                    string[] split = str.Split(',');
                    inner.Add(split[0], new SpeciesRomanceSettings(Convert.ToBoolean(split[1]), Convert.ToBoolean(split[2]), Convert.ToInt32(split[3]), Convert.ToInt32(split[4])));
                }
            }
        }

        public override string ToString()
        {
            List<string> strings = new List<string>();
            foreach (KeyValuePair<string, SpeciesRomanceSettings> item in inner)
            {
                strings.Add(item.Key + "," + item.Value.ToString());
            }
            return inner != null ? string.Join("|", strings.ToArray()) : "";
        }
    }

    public class SpeciesRomanceSettings
    {
        public bool psychologyEnabled;
        public bool caresAboutAgeGap;
        public float minDatingAge;
        public float minLovinAge;
        public SpeciesRomanceSettings(bool psychologyEnabled = true, bool caresAboutAgeGap = true, float minDatingAge = 14f, float minLovinAge = 16f)
        {
            this.psychologyEnabled = psychologyEnabled;
            this.caresAboutAgeGap = caresAboutAgeGap;
            this.minDatingAge = minDatingAge;
            this.minLovinAge = minLovinAge;
        }

        public override string ToString()
        {
            return this.psychologyEnabled + "," + this.caresAboutAgeGap + "," + this.minDatingAge + "," + this.minLovinAge;
        }
    }
}

