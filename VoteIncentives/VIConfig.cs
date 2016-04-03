using System;
using System.IO;

namespace VoteIncentives
{
    public class VIConfig
    {
        public string ServerKey { get; set; } = " ";

        public string[] ConsoleCommands { get; set; } = new[]
        {
            "/give 50 Gold\\ Coin <playersname>",
            "/heal <playername>"
        };

        public string[] VoteCommandMessage { get; set; } = new[]
        {
            "[c/FF6900:Go to http://terraria-servers.com/ and vote for this server to receive rewards!]",
            "[c/FF6900:Use /claim once you have voted to redeem your reward]"
        };

        public string[] ClaimCommandSuccessMessage { get; set; } = new[]
        {
            "[c/FF6900:Awesome, thanks for your vote!]",
            "[c/FF6900:As promised you now have your reward.]"
        };
    }

    public static class VIConfigExtensions
    {
        public static VIConfig LoadConfig(this VoteIncentivesPlugin instance, string from)
        {
            VIConfig config = null;
            if (File.Exists(from))
            {
                try
                {
                    var contents = File.ReadAllText(from);
                    config = Newtonsoft.Json.JsonConvert.DeserializeObject<VIConfig>(contents);
                }
                catch (Exception e)
                {
                    instance.Logger.Log(e, $"Error parsing {from}");
                    return null;
                }
            }
            else
            {
                config = new VIConfig();
                try
                {
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(from, json);
                }
                catch (Exception e)
                {
                    instance.Logger.Log(e, $"Error saving to {from}");
                    return null;
                }
            }

            if (config != null && (config.ConsoleCommands == null || config.ConsoleCommands.Length == 0))
            {
                instance.Logger.Log($"Config contains no ConsoleCommands. No rewards can be issued.");
                return null;
            }
            if (config != null && (config.ClaimCommandSuccessMessage == null || config.ClaimCommandSuccessMessage.Length == 0))
            {
                instance.Logger.Log($"Config contains no ClaimCommandSuccessMessage. No rewards can be issued.");
                return null;
            }
            if (config != null && (config.VoteCommandMessage == null || config.VoteCommandMessage.Length == 0))
            {
                instance.Logger.Log($"Config contains no VoteCommandMessage. No rewards can be issued.");
                return null;
            }

            return config;
        }
    }
}
