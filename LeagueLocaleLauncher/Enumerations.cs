using System.Collections.Generic;

namespace LeagueLocaleLauncher
{
    public static class Enumerations
    {
        public static Dictionary<Language, string> Languages = new Dictionary<Language, string>
        {
            { Language.CHINESE_SIMPLIFIED,      "zh_CN"},
            { Language.CZECH_CZECH_REPUBLIC,    "cs_CZ"},
            { Language.GERMAN_GERMANY,          "de_DE"},
            { Language.GREEK_GREECE,            "el_GR"},
            { Language.ENGLISH_AUSTRALIA,       "en_AU"},
            { Language.ENGLISH_UNITED_KINGDOM,  "en_GB"},
            { Language.ENGLISH_UNITED_STATES,   "en_US"},
            { Language.SPANISH_SPAIN,           "es_ES"},
            { Language.SPANISH_MEXICO,          "es_MX"},
            { Language.FRENCH_FRANCE,           "fr_FR"},
            { Language.HUNGARIAN_HUNGARY,       "hu_HU"},
            { Language.ITALIAN_ITALY,           "it_IT"},
            { Language.JAPANESE_JAPAN,          "ja_JP"},
            { Language.KOREAN_KOREA,            "ko_KR"},
            { Language.POLISH_POLAND,           "pl_PL"},
            { Language.PORTUGUESE_BRAZIL,       "pt_BR"},
            { Language.ROMANIAN_ROMANIA,        "ro_RO"},
            { Language.RUSSIAN_RUSSIA,          "ru_RU"},
            { Language.TURKISH_TURKEY,          "tr_TR"}
        };
    }
}
