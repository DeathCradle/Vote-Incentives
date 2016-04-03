using OTA;
using OTA.Command;
using OTA.Commands;
using OTA.Logging;
using OTA.Plugin;
using System.IO;
using TDSM.Core;
using TDSM.Core.Command;
using Terraria;

[assembly: PluginDependency("TDSM.Core")]
[assembly: PluginDependency("OTA.Commands")]
namespace VoteIncentives
{
    [OTAVersion(1, 0)]
    public class VoteIncentivesPlugin : BasePlugin
    {
        public VIConfig Config { get; private set; }

        public string ConfigPath
        {
            get { return Path.Combine(Globals.DataPath, "voteincentives.json"); }
        }

        public LogChannel Logger { get; } = new LogChannel("VoteIncentives", System.ConsoleColor.Magenta, System.Diagnostics.TraceLevel.Info);

        public VoteIncentivesPlugin()
        {
            this.Author = "DeathCradle";
            this.Description = "Aids to get more votes on terraria-servers.com";
            this.Name = "Vote Incentives";
            this.Version = "1";
        }

        protected override void Initialized(object state)
        {
            base.Initialized(state);

            this.AddCommand<TDSMCommandInfo>("vote")
                .WithAccessLevel(AccessLevel.PLAYER)
                .WithDescription("Information on how to vote for this server")
                .WithPermissionNode("voteincentives.vote")
                .Calls((ISender sender, ArgumentList args) =>
                {
                    if (Config.VoteCommandMessage != null && Config.VoteCommandMessage.Length > 0)
                    {
                        foreach (var msg in Config.VoteCommandMessage)
                            sender.Message(msg.Replace("<playersname>", sender.SenderName));
                    }
                });

            this.AddCommand<TDSMCommandInfo>("claim", "reward", "redeem")
                .WithAccessLevel(AccessLevel.PLAYER)
                .WithDescription("Claim a reward(s) if you have voted for this server")
                .WithPermissionNode("voteincentives.claim")
                .Calls((ISender sender, ArgumentList args) =>
                {
                    //there is a 3 minute cache
                    var player = sender as Player;
                    if (player != null)
                    {
                        if (player.IsAuthenticated())
                        {
                            var auth = player.GetAuthenticatedAs();
                            var res = this.HasVoted(auth);
                            switch (res)
                            {
                                case TSVoteResult.NotClaimed:
                                    foreach (var cmd in Config.ConsoleCommands)
                                    {
                                        OTA.Commands.CommandManager.Parser.ParseAndProcess(ConsoleSender.Default, cmd.Replace("<playersname>", player.name));
                                    }
                                    if (Config.ClaimCommandSuccessMessage != null && Config.ClaimCommandSuccessMessage.Length > 0)
                                    {
                                        foreach (var msg in Config.ClaimCommandSuccessMessage)
                                            sender.Message(msg.Replace("<playersname>", player.name));
                                    }
                                    this.NotifyClaimed(player.name);
                                    break;
                                case TSVoteResult.Claimed:
                                    player.SendMessage("This vote has already been claimed.", Microsoft.Xna.Framework.Color.Red);
                                    break;
                                case TSVoteResult.NotFound:
                                    player.SendMessage("You have not voted for this server.", Microsoft.Xna.Framework.Color.Red);
                                    break;
                                case TSVoteResult.Error:
                                default:
                                    player.SendMessage("Failed to check for voting status.", Microsoft.Xna.Framework.Color.Red);
                                    break;
                            }
                        }
                        else sender.Message("You must be authenticated to claim rewards.");
                    }
                    else sender.Message("This is a player command.");
                });

            Config = this.LoadConfig(ConfigPath);
        }

        protected override void Enabled()
        {
            base.Enabled();

            if (Config != null)
            {
                if (!this.IsValidServerKey())
                {
                    Logger.Log("Invalid server key.");
                    this.Disable();
                    return;
                }
            }
            else
            {
                Logger.Log($"Vote Incentives failed to load {ConfigPath}");
                this.Disable();
                return;
            }

            Logger.Log("Enabled.");
        }
    }
}
