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
        private Color oMessageColor = new Color(255, 127, 36);
        private int nExpiration = 30;
        private List<PlayerPrivateMessageSettings> oPlayerSettings = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateMessageManager"/> class.
        /// </summary>
        public PrivateMessageManager()
        {
            this.LoadPlayerSettings();
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="PrivateMessageManager"/> is reclaimed by garbage collection.
        /// </summary>
        ~PrivateMessageManager()
        {
            this.SavePlayerSettings();
        }

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        public void Unload()
        {
            this.SavePlayerSettings();
        }

        /// <summary>
        /// Loads the player settings.
        /// </summary>
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

        /// <summary>
        /// Saves the player settings.
        /// </summary>
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

        /// <summary>
        /// Removes the expired settings.
        /// </summary>
        private void RemoveExpiredSettings()
        {
            for (int i = 0; i < oPlayerSettings.Count(); i++)
            {
                if (oPlayerSettings[i].Player != null)
                    // Don't remove settings for players that are online
                    if (oPlayerSettings[i].Player.Active)
                        continue;

                if ((oPlayerSettings[i].LastUpdated - DateTime.Now).Days < nExpiration)
                    oPlayerSettings.Remove(oPlayerSettings[i]);
            }
        }

        /// <summary>
        /// Privates the message enable disable.
        /// </summary>
        /// <param name="Player">The player.</param>
        /// <param name="EnableDisableFlag">if set to <c>true</c> [enable disable flag].</param>
        public void PrivateMessageEnableDisable(MyPlayer Player, bool EnableDisableFlag)
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
                Player.SendMessage("Private messaging is now enabled");
            else
                Player.SendMessage("Private messaging is now disabled");
        }

        /// <summary>
        /// Sends the reply.
        /// </summary>
        /// <param name="PlayerFrom">The player from.</param>
        /// <param name="Message">The message.</param>
        public void SendReply(MyPlayer PlayerFrom, string Message)
        {
            if (PlayerFrom == null)
                throw new Exception("Invalid player");

            if (!string.IsNullOrEmpty(Message))
            {
                MyPlayer oPlayerTo = this.GetLastPlayerMessageFrom(PlayerFrom);
                if (oPlayerTo != null)
                {
                    this.SendMessage(oPlayerTo, PlayerFrom, Message, true);
                }
                else
                {
                    PlayerFrom.SendMessage("You have to recent players to reply to");
                }
            }
            else
            {
                PlayerFrom.SendMessage("No message provided");
            }
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="PlayerTo">The player to.</param>
        /// <param name="PlayerFrom">The player from.</param>
        /// <param name="Message">The message.</param>
        /// <param name="IsPrivateMessage">if set to <c>true</c> [is private message].</param>
        public void SendMessage(MyPlayer PlayerTo, MyPlayer PlayerFrom, string Message, bool IsPrivateMessage)
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
                        PlayerTo.SendMessage(string.Format(sFromMessageFormat, sPrivMessageIndicator, PlayerFrom.Name, Message), oMessageColor);
                        PlayerFrom.SendMessage(string.Format(sToMessageFormat, sPrivMessageIndicator, PlayerTo.Name, Message), oMessageColor);

                        this.UpdateLastMessageFrom(PlayerTo, PlayerFrom);
                    }
                    else
                    {
                        PlayerFrom.SendMessage(string.Format("Player {0} has private messaging disabled", PlayerTo.Name));
                    }
                }
                else
                {
                    PlayerTo.SendMessage(string.Format(sFromMessageFormat, sPrivMessageIndicator, PlayerFrom.Name, Message), oMessageColor);
                    PlayerFrom.SendMessage(string.Format(sToMessageFormat, sPrivMessageIndicator, PlayerTo.Name, Message), oMessageColor);

                    this.UpdateLastMessageFrom(PlayerTo, PlayerFrom);
                }
            }
            else
                PlayerFrom.SendMessage("No message provided");
        }

        /// <summary>
        /// Creates the player setting.
        /// </summary>
        /// <param name="Player">The player.</param>
        /// <returns></returns>
        public PlayerPrivateMessageSettings CreatePlayerSetting(MyPlayer Player)
        {
            if (this.PlayerSettingExists(Player))
                return null; // Player setting already exists

            PlayerPrivateMessageSettings oPlayerSetting = new PlayerPrivateMessageSettings();
            oPlayerSetting.Player = Player;
            oPlayerSetting.PrivateMessageEnabled = true;
            oPlayerSetting.PlayerName = Player.Name;
            oPlayerSetting.IPAddress = Player.IPAddress;
            oPlayerSetting.LastUpdated = DateTime.Now;
            oPlayerSettings.Add(oPlayerSetting);

            return oPlayerSetting;
        }

        public void UpdateLastMessageFrom(MyPlayer PlayerTo, MyPlayer PlayerFrom)
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
            oPlayerSetting.LastMessageFromName = PlayerFrom.Name;
        }

        /// <summary>
        /// Gets the player setting.
        /// </summary>
        /// <param name="Player">The player.</param>
        /// <returns></returns>
        public PlayerPrivateMessageSettings GetPlayerSetting(MyPlayer Player)
        {
            if (Player == null)
                throw new Exception("Invalid player");

            string sIPAddress = Player.IPAddress;

            var oPlayerObject = from player in oPlayerSettings where player.PlayerName == Player.Name && player.IPAddress == sIPAddress select player;
            PlayerPrivateMessageSettings oPlayerSetting = (PlayerPrivateMessageSettings)oPlayerObject.FirstOrDefault<PlayerPrivateMessageSettings>();
            if (oPlayerSetting is PlayerPrivateMessageSettings)
            {
                return oPlayerSetting;
            }
            else
                return null;
        }

        /// <summary>
        /// Players the setting exists.
        /// </summary>
        /// <param name="Player">The player.</param>
        /// <returns></returns>
        public bool PlayerSettingExists(MyPlayer Player)
        {
            if (Player == null)
                throw new Exception("Invalid player");

            string sIPAddress = Player.IPAddress;

            var oPlayerObject = from player in oPlayerSettings where player.PlayerName == Player.Name && player.IPAddress == sIPAddress select player;
            PlayerPrivateMessageSettings oPlayerSetting = (PlayerPrivateMessageSettings)oPlayerObject.FirstOrDefault<PlayerPrivateMessageSettings>();
            if (oPlayerSetting != null)
                return true;
            else
                return false;
        }

        //TODO Need to fix this. Replies go to self.
        /// <summary>
        /// Gets the last player message from.
        /// </summary>
        /// <param name="Player">The player.</param>
        /// <returns></returns>
        public MyPlayer GetLastPlayerMessageFrom(MyPlayer Player)
        {
            if (Player == null)
                throw new Exception("Invalid player");

            string sIPAddress = Player.IPAddress;

            var oPlayerObject = from player in oPlayerSettings where player.PlayerName == Player.Name && player.IPAddress == sIPAddress select player;
            PlayerPrivateMessageSettings oPlayerSetting = (PlayerPrivateMessageSettings)oPlayerObject.FirstOrDefault<PlayerPrivateMessageSettings>();

            if (oPlayerSetting != null)
            {
                if (string.IsNullOrEmpty(oPlayerSetting.LastMessageFromName))
                    return null;

                MyPlayer oToPlayer = Utility.FindPlayer(oPlayerSetting.LastMessageFromName);
                if (oToPlayer == null)
                    return null;

                oPlayerSetting.LastMessageFrom = oToPlayer;
                oPlayerSetting.LastUpdated = DateTime.Now;
                oPlayerSetting.LastMessageFromName = oToPlayer.Name;
                if (string.IsNullOrEmpty(oPlayerSetting.IPAddress))
                    oPlayerSetting.IPAddress = Player.IPAddress;
             
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

        public Color MessageColor
        {
            get
            {
                return oMessageColor;
            }
            set
            {
                oMessageColor = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    [JsonObject("Player Private Message Settings")]
    public class PlayerPrivateMessageSettings
    {
        [JsonIgnore]
        public MyPlayer Player { get; set; }

        [JsonProperty]
        public string PlayerName { get; set; }

        [JsonProperty]
        public string IPAddress { get; set; }

        [JsonIgnore]
        public MyPlayer LastMessageFrom { get; set; }

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

        public PlayerPrivateMessageSettings(MyPlayer Player, MyPlayer PlayerFrom, bool PrivateMessageFlag)
        {
            this.Player = Player;
            this.LastMessageFrom = PlayerFrom;
            this.PrivateMessageEnabled = PrivateMessageFlag;
            this.IPAddress = Player.IPAddress;
        }
    }
}
