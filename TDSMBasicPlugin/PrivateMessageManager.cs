using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria_Server;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TDSMBasicPlugin
{
    public class PrivateMessageManager
    {
        public Server Server { get; set; }

        private string sSettingsFile = "player-privmsg-settings.json";

        private string sMessageIndicator = "";
        private string sFromMessageFormat = "{0}<From: {1}> {2}";   // Parameters: <message indicator> <from> <message>
        private string sToMessageFormat = "{0}<To: {1}> {2}";       // Parameters: <message indicator> <to> <message>
        private int[] nMessageColor = new int[] { 255, 127, 36 };
        private int nExpiration = 30;
        private List<PlayerPrivateMessageSettings> oPlayerSettings = null;

        public PrivateMessageManager()
        {
            this.LoadPlayerSettings();
        }

        ~PrivateMessageManager()
        {
            this.SavePlayerSettings();
        }

        public void Unload()
        {
            this.SavePlayerSettings();
        }

        private void LoadPlayerSettings()
        {
            string sFile = Path.Combine(TDSMBasicPlugin.GetPluginDirectory(), sSettingsFile);

            if (!File.Exists(sFile))
            {
                oPlayerSettings = new List<PlayerPrivateMessageSettings>();
            }
            else
            {
                string json = File.ReadAllText(sFile);

                oPlayerSettings = JsonConvert.DeserializeObject<List<PlayerPrivateMessageSettings>>(json);

                RemoveExpiredSettings();
            }
        }

        private void SavePlayerSettings()
        {
            RemoveExpiredSettings();

            string sFile = Path.Combine(TDSMBasicPlugin.GetPluginDirectory(), sSettingsFile);

            string json = JsonConvert.SerializeObject(oPlayerSettings, Formatting.Indented);

            if (File.Exists(sFile)) File.Delete(sFile);

            using (StreamWriter oOutFile = new StreamWriter(sFile))
            {
                oOutFile.Write(json);
            }
        }

        private void RemoveExpiredSettings()
        {
            for (int i = 0; i < oPlayerSettings.Count(); i++)
            {
                // Don't remove settings for players that are online
                if (Main.player.Contains(oPlayerSettings[i].Player))
                    continue;

                if ((oPlayerSettings[i].LastUpdated - DateTime.Now).Days < nExpiration)
                    oPlayerSettings.Remove(oPlayerSettings[i]);
            }
        }

        public void PrivateMessageEnableDisable(Player Player, bool EnableDisableFlag)
        {
            if (Player == null)
                throw new Exception("Invalid player");

            PlayerPrivateMessageSettings oPlayerSetting = this.GetPlayerSetting(Player);
            if (oPlayerSetting == null)
            {
                oPlayerSetting = this.CreatePlayerSetting(Player);
            }

            oPlayerSetting.PrivateMessageEnabled = EnableDisableFlag;

            if (EnableDisableFlag)
                Player.sendMessage("Private messaging is now enabled");
            else
                Player.sendMessage("Private messaging is now disabled");
        }

        public void SendReply(Player PlayerFrom, string Message)
        {
            if (PlayerFrom == null)
                throw new Exception("Invalid player");

            if (!string.IsNullOrEmpty(Message))
            {
                Player oPlayerTo = this.GetLastPlayerMessageFrom(PlayerFrom);
                if (oPlayerTo != null)
                {
                    this.SendMessage(oPlayerTo, PlayerFrom, Message, true);
                }
                else
                {
                    PlayerFrom.sendMessage("You have to recent players to reply to");
                }
            }
            else
            {
                PlayerFrom.sendMessage("No message provided");
            }
        }

        public void SendMessage(Player PlayerTo, Player PlayerFrom, string Message, bool IsPrivateMessage)
        {
            string sPrivMessageIndicator = "";

            if (PlayerFrom == null || PlayerTo == null)
                throw new Exception("Invalid players");

            if (IsPrivateMessage)
                sPrivMessageIndicator = sMessageIndicator;

            if (!string.IsNullOrEmpty(Message))
            {
                PlayerPrivateMessageSettings oPlayerToSetting = this.GetPlayerSetting(PlayerTo);

                if (oPlayerToSetting != null)
                {
                    if (oPlayerToSetting.PrivateMessageEnabled)
                    {
                        PlayerTo.sendMessage(string.Format(sFromMessageFormat, sPrivMessageIndicator, PlayerFrom.name, Message), 255, nMessageColor[0], nMessageColor[1], nMessageColor[2]);
                        PlayerFrom.sendMessage(string.Format(sToMessageFormat, sPrivMessageIndicator, PlayerTo.name, Message), 255, nMessageColor[0], nMessageColor[1], nMessageColor[2]);
                        this.UpdateLastMessageFrom(PlayerTo, PlayerFrom);
                    }
                    else
                    {
                        PlayerFrom.sendMessage(string.Format("Player {0} has private messaging disabled", PlayerTo.name));
                    }
                }
                else
                {
                    PlayerTo.sendMessage(string.Format(sFromMessageFormat, sPrivMessageIndicator, PlayerFrom.name, Message), 255, nMessageColor[0], nMessageColor[1], nMessageColor[2]);
                    PlayerFrom.sendMessage(string.Format(sToMessageFormat, sPrivMessageIndicator, PlayerTo.name, Message), 255, nMessageColor[0], nMessageColor[1], nMessageColor[2]);
                    this.UpdateLastMessageFrom(PlayerTo, PlayerFrom);
                }
            }
            else
                PlayerFrom.sendMessage("No message provided");
        }

        public PlayerPrivateMessageSettings CreatePlayerSetting(Player Player)
        {
            if (this.PlayerSettingExists(Player))
                return null; // Player setting already exists

            PlayerPrivateMessageSettings oPlayerSetting = new PlayerPrivateMessageSettings();
            oPlayerSetting.Player = Player;
            oPlayerSetting.PrivateMessageEnabled = true;
            oPlayerSetting.PlayerName = Player.name;
            oPlayerSetting.IPAddress = Player.getIPAddress().Split(':')[0];
            oPlayerSetting.LastUpdated = DateTime.Now;
            oPlayerSettings.Add(oPlayerSetting);

            return oPlayerSetting;
        }

        public void UpdateLastMessageFrom(Player PlayerTo, Player PlayerFrom)
        {
            if (PlayerTo == null || PlayerFrom == null)
                throw new Exception("Invalid player");

            PlayerPrivateMessageSettings oPlayerSetting = null;

            if (!this.PlayerSettingExists(PlayerTo))
            {
                oPlayerSetting = this.CreatePlayerSetting(PlayerTo);
            }
            else
            {
                oPlayerSetting = this.GetPlayerSetting(PlayerTo);
            }

            oPlayerSetting.LastMessageFrom = PlayerFrom;
            oPlayerSetting.LastMessageFromName = PlayerFrom.name;
        }

        public PlayerPrivateMessageSettings GetPlayerSetting(Player Player)
        {
            if (Player == null)
                throw new Exception("Invalid player");

            string sIPAddress = Player.getIPAddress().Split(':')[0];

            var oPlayerObject = from player in oPlayerSettings where player.PlayerName == Player.name && player.IPAddress == sIPAddress select player;
            PlayerPrivateMessageSettings oPlayerSetting = (PlayerPrivateMessageSettings)oPlayerObject.FirstOrDefault<PlayerPrivateMessageSettings>();
            if (oPlayerSetting is PlayerPrivateMessageSettings)
            {
                return oPlayerSetting;
            }
            else
                return null;
        }

        public bool PlayerSettingExists(Player Player)
        {
            if (Player == null)
                throw new Exception("Invalid player");

            string sIPAddress = Player.getIPAddress().Split(':')[0];

            var oPlayerObject = from player in oPlayerSettings where player.PlayerName == Player.name && player.IPAddress == sIPAddress select player;
            PlayerPrivateMessageSettings oPlayerSetting = (PlayerPrivateMessageSettings)oPlayerObject.FirstOrDefault<PlayerPrivateMessageSettings>();
            if (oPlayerSetting != null)
                return true;
            else
                return false;
        }

        public Player GetLastPlayerMessageFrom(Player Player)
        {
            if (Player == null)
                throw new Exception("Invalid player");

            string sIPAddress = Player.getIPAddress().Split(':')[0];

            var oPlayerObject = from player in oPlayerSettings where player.PlayerName == Player.name && player.IPAddress == sIPAddress select player;
            PlayerPrivateMessageSettings oPlayerSetting = (PlayerPrivateMessageSettings)oPlayerObject.FirstOrDefault<PlayerPrivateMessageSettings>();

            if (oPlayerSetting != null)
            {
                Player oPlayer = this.Server.GetPlayerByName(oPlayerSetting.LastMessageFromName);
                oPlayerSetting.LastMessageFrom = oPlayer;
                oPlayerSetting.LastUpdated = DateTime.Now;
                if (string.IsNullOrEmpty(oPlayerSetting.IPAddress))
                    oPlayerSetting.IPAddress = oPlayer.getIPAddress().Split(':')[0];
             
                return oPlayerSetting.LastMessageFrom;
            }
            else
                return null;
        }

        #region Properties
        public string GetPrivateMessageIndicator
        {
            get
            {
                return sMessageIndicator;
            }
        }

        public string PlayerSettingsFile
        {
            get
            {
                return sSettingsFile;
            }
            set
            {
                sSettingsFile = value;
            }
        }

        public string ToMessageFormat
        {
            get
            {
                return sToMessageFormat;
            }
            set
            {
                sToMessageFormat = value;
            }
        }

        public string FromMessageFormat
        {
            get
            {
                return sFromMessageFormat;
            }
            set
            {
                sFromMessageFormat = value;
            }
        }

        public int[] MessageColor
        {
            get
            {
                return nMessageColor;
            }
            set
            {
                nMessageColor = value;
            }
        }
        #endregion
    }

    [JsonObject("Player Private Message Settings")]
    public class PlayerPrivateMessageSettings
    {
        [JsonIgnore]
        public Player Player { get; set; }

        [JsonProperty]
        public string PlayerName { get; set; }

        [JsonProperty]
        public string IPAddress { get; set; }

        [JsonIgnore]
        public Player LastMessageFrom { get; set; }

        [JsonProperty]
        public string LastMessageFromName { get; set; }

        [JsonProperty]
        public bool PrivateMessageEnabled { get; set; }

        [JsonProperty]
        public DateTime LastUpdated { get; set; }

        public PlayerPrivateMessageSettings()
        {
            this.PrivateMessageEnabled = true;
        }

        public PlayerPrivateMessageSettings(Player Player, Player PlayerFrom, bool PrivateMessageFlag)
        {
            this.Player = Player;
            this.LastMessageFrom = PlayerFrom;
            this.PrivateMessageEnabled = PrivateMessageFlag;
            this.IPAddress = Player.getIPAddress();
        }
    }
}
