﻿using System.IO;
using YamlDotNet.Serialization;

namespace LeagueLocaleLauncher
{
    public class LeagueClientSettings
    {
        public static void SetRegion(string region)
        {
            var leagueClientSettingsPath = Config.Loaded.LeagueClientSettingsPath;

            dynamic leagueClientSettings;
            using (TextReader reader = File.OpenText(leagueClientSettingsPath))
            {
                Deserializer deserializer = new Deserializer();
                leagueClientSettings = deserializer.Deserialize<object>(reader);
            }

            leagueClientSettings["install"]["globals"]["region"] = region;

            using (TextWriter writer = File.CreateText(leagueClientSettingsPath))
            {
                var serializer = new Serializer();
                serializer.Serialize(writer, leagueClientSettings);
            }
		}
    }
}
