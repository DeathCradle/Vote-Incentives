using System;
using System.Net;

namespace VoteIncentives
{
    public static class TS
    {
        public static TSVoteResult HasVoted(this VoteIncentivesPlugin instance, string username)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    var resp = wc.DownloadString($"http://terraria-servers.com/api/?object=votes&element=claim&key={instance.Config.ServerKey}&username={username}");

                    int res;
                    if (Int32.TryParse(resp, out res))
                    {
                        return (TSVoteResult)res;
                    }
                }
            }
            catch (Exception e)
            {
                instance.Logger.Log(e, "HasVoted exception");
            }

            return TSVoteResult.Error;
        }

        public static bool IsValidServerKey(this VoteIncentivesPlugin instance)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    var json = wc.DownloadString($"http://terraria-servers.com/api/?object=servers&element=detail&key={instance.Config.ServerKey}");

                    try
                    {
                        var srv = Newtonsoft.Json.JsonConvert.DeserializeObject<TSServerInfo>(json);
                        int tmp;
                        return srv != null && Int32.TryParse(srv.Id, out tmp) && tmp > 0;
                    }
                    catch { }
                }
            }
            catch (Exception e)
            {
                instance.Logger.Log(e, "IsValidServerKey exception");
            }
            return false;
        }

        public static bool NotifyClaimed(this VoteIncentivesPlugin instance, string username)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    return wc.DownloadString($"http://terraria-servers.com/api/?action=post&object=votes&element=claim&key={instance.Config.ServerKey}&username={username}") == "1";
                }
            }
            catch (Exception e)
            {
                instance.Logger.Log(e, "NotifyClaimed exception");
            }
            return false;
        }
    }

    public enum TSVoteResult
    {
        Error = -1,

        NotFound = 0,
        NotClaimed = 1,
        Claimed = 2
    }

    public class TSServerInfo
    {
        [Newtonsoft.Json.JsonProperty("id")]
        public string Id { get; set; }
    }
}
