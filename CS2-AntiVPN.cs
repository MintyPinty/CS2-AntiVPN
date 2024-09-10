using System.Linq;
using System.Threading;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text.RegularExpressions;

namespace CS2_AntiVPN;

public class CS2_AntiVPN : BasePlugin, IPluginConfig<CS2_AntiVPN_Config>
{

    public required CS2_AntiVPN_Config Config { get; set; }

    public override string ModuleName => "CS2-AntiVPN";
    public override string ModuleVersion => "1.3";
    public override string ModuleAuthor => "mintyx";
    public override string ModuleDescription => "Kicks Players with a VPN enabled.";

    private string? _serverName;
    private string? ipAddress;

    public override void Load(bool hotReload)
    {
        Logger.LogInformation("CS2-AntiVPN Loaded! -> mintyx");
    }
    public void OnConfigParsed(CS2_AntiVPN_Config config)
    {
        Config = config;
    }
    private string ReplaceMessageColors(string input)
    {
        string[] ColorAlphabet = { "{GREEN}", "{BLUE}", "{RED}", "{SILVER}", "{MAGENTA}", "{GOLD}", "{DEFAULT}", "{LIGHTBLUE}", "{LIGHTPURPLE}", "{LIGHTRED}", "{LIGHTYELLOW}", "{YELLOW}", "{GREY}", "{LIME}", "{OLIVE}", "{ORANGE}", "{DARKRED}", "{DARKBLUE}", "{BLUEGREY}", "{PURPLE}" };
        string[] ColorChar = { $"{ChatColors.Green}", $"{ChatColors.Blue}", $"{ChatColors.Red}", $"{ChatColors.Silver}", $"{ChatColors.Magenta}", $"{ChatColors.Gold}", $"{ChatColors.Default}", $"{ChatColors.LightBlue}", $"{ChatColors.LightPurple}", $"{ChatColors.LightRed}", $"{ChatColors.LightYellow}", $"{ChatColors.Yellow}", $"{ChatColors.Grey}", $"{ChatColors.Lime}", $"{ChatColors.Olive}", $"{ChatColors.Orange}", $"{ChatColors.DarkRed}", $"{ChatColors.DarkBlue}", $"{ChatColors.BlueGrey}", $"{ChatColors.Purple}" };

        for (int z = 0; z < ColorAlphabet.Length; z++)
        {
            input = Regex.Replace(input, Regex.Escape(ColorAlphabet[z]), ColorChar[z], RegexOptions.IgnoreCase);
        }
        return input;
    }
    private static int PlayersConnected()
    {
        return Utilities.GetPlayers()
            .Where(player => player.IsValid && !player.IsHLTV && !player.IsBot)
            .Count();
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {

        if (@event.Userid == null)
        {
            return HookResult.Continue;
        }

        var player = @event.Userid;

        string ipAddress = player.IpAddress?.Split(":")[0] ?? string.Empty;
        string playerName = player.PlayerName ?? "Unknown Player";

        Logger.LogInformation($"{playerName} Joined from IP: {ipAddress}");

        Task.Run(async () =>
        {
            var isVpn = await CheckVpn(ipAddress, player);
            if (isVpn)
            {
                await VpnAction(player);
            }
        });

        return HookResult.Continue;
    }

    private async Task<bool> CheckVpn(string ipAddress, CCSPlayerController player)
    {
        using var client = new HttpClient();
        var url = $"http://ip-api.com/json/{ipAddress}?fields=status,mobile,proxy,hosting,query";

        try
        {
            var response = await client.GetAsync(url);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var jsonResult = JsonDocument.Parse(responseBody).RootElement;

                var isUsingVpn = jsonResult.GetProperty("proxy").GetBoolean();
                var isUsingHost = jsonResult.GetProperty("hosting").GetBoolean();

                if (isUsingVpn || isUsingHost)
                {
                    Logger.LogInformation($"VPN or Hosting Service Detected! Attempting to kick the player...");
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Unable to fetch IP `{ipAddress}` info!");
        }

        return false;
    }
    private Task VpnAction(CCSPlayerController player)
    {
        Server.NextFrame(() =>
        {
            player.ChangeTeam(CounterStrikeSharp.API.Modules.Utils.CsTeam.Spectator);
        });

        Thread.Sleep(Config.Kick_Message_Delay * 1000);
        string Kick_Message = ReplaceMessageColors(Config.Kick_Message);

        Server.NextFrame(() =>
        {
            player.PrintToChat($"{Kick_Message}");
        });
        Server.NextFrame(() =>
        {
            player.PrintToCenterAlert($"{Kick_Message}");
        });

        Thread.Sleep((Config.KickDelay - Config.Kick_Message_Delay) * 1000);

        Server.NextFrame(() =>
        {
            Server.ExecuteCommand($"kickid {player.UserId} VPN usage is not allowed on this server");
            Logger.LogInformation($"Kicked {player.PlayerName} for using VPN!");
        });

        return Task.CompletedTask;
    }


}
