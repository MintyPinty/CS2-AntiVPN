using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using System.Text.Json.Serialization;

namespace CS2_AntiVPN;

[MinimumApiVersion(228)]

public class CS2_AntiVPN_Config : BasePluginConfig
{
    [JsonPropertyName("Kick_Delay")] public int KickDelay { get; set; } = 5;
    [JsonPropertyName("Kick_Message")] public string Kick_Message { get; set; } = "{Default}[{Red}AntiVPN{Default}] You have been blocked from the server for using a VPN.";
    [JsonPropertyName("Kick_Message_Delay")] public int Kick_Message_Delay { get; set; } = 1;
    [JsonPropertyName("Webhook_Url")] public string WebhookUrl { get; set; } = "";
}
