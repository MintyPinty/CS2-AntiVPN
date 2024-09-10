# CS2-AntiVPN

## Description
A plugin that kicks players with a VPN enabled.

## Installation
 - Download the newest release from [Releases](https://github.com/MintyPinty/CS2-AntiVPN/releases)
 - Make a folder in /plugins named /CS2-AntiVPN.
 - Put the plugin files in to the new folder.
 - Restart your server.
   
## Requirments
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/) **tested on v265**

## Configuration
```json
{
  "Kick_Delay": 5, // Delay before kicking the player
  "Kick_Message": "{Default}[{Red}AntiVPN{Default}] You have been blocked from the server for using a VPN.", // Message before the kick
  "Kick_Message_Delay": 1, // Delay before outputting the kick message
  "Webhook_Url": "" // Discord Webhook Url
  "ConfigVersion": 1 // Do not change
}
```
