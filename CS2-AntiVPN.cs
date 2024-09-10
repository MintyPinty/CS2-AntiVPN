using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using CounterStrikeSharp.API.Modules.Entities;

namespace CS2_AntiVPN;

public class CS2_AntiVPN : BasePlugin
{
    public override string ModuleName => "CS2-AntiVPN";

    public override string ModuleVersion => "1.0";
    public override string ModuleAuthor => "mintyx";
    public override string ModuleDescription => "Kicks Players with a VPN enabled.";

    private string? _serverName;
    private string? ipAddress;

    public override void Load(bool hotReload)
    {

    }
    private static int PlayersConnected()
    {
        return Utilities.GetPlayers().Where(player => player.IsValid && !player.IsHLTV && !player.IsBot).Count();
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        string ipAddress = player!.IpAddress!.Split(":")[0];
        string playerName = player!.PlayerName;

        Logger.LogInformation($"{playerName} Joined {ipAddress}");
        Task.Run(async () =>
        {
            bool isvpn = await CheckVpn(ipAddress, player);
            if (isvpn)
            {
                await VpnAction(player);
            }

        }).GetAwaiter().GetResult();



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
                    Logger.LogInformation($"VPN Detected! Attempting to kick the player...");
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

            Server.ExecuteCommand($"kickid {player.UserId} VPN usage is not allowed on this server");
            Logger.LogInformation($"Kicked {player.PlayerName} for using VPN!");
            
        });

        return Task.CompletedTask;
    }
}
