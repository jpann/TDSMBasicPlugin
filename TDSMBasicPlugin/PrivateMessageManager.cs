using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria_Server;
using System.IO;
using Newtonsoft.Json;

namespace TDSMBasicPlugin
{
    public class PrivateMessageManager
    {
        private string sSettingsFile = "playersprivmsg.json";

        private string sMessageIndicator = "*P ";
        private string sMessageFormat = "{0}<{1}> {2}"; // <message indicator> <from> <message>
        private int[] nMessageColor = new int[] { 255, 127, 36 };
        private List<PlayerPrivateMessageSettings> oPlayerSettings = null;

        public PrivateMessageManager()
        {
            this.LoadPlayerSettings();
        }

        ~PrivateMessageManager()
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
            }
        }

        private void SavePlayerSettings()
        {
            string sFile = Path.Combine(TDSMBasicPlugin.GetPluginDirectory(), sSettingsFile);

            string json = JsonConvert.SerializeObject(oPlayerSettings, Formatting.Indented);

            if (File.Exists(sFile)) File.Delete(sFile);

            using (StreamWriter oOutFile = new StreamWriter(sFile))
            {
                oOutFile.Write(json);
            }
        }

        public void SendReply(Player PlayerFrom, string Message)
        {
            if (PlayerFrom == null)
                throw new Exception("Invalid players");

            if (!string.IsNullOrEmpty(Message))
            {
                Player oPlayerTo = this.GetLastPlayerSentTo(PlayerFrom);
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
                PlayerPrivateMessageSettings oPlayerSetting = this.GetPlayerSetting(PlayerTo);
                if (oPlayerSetting != null)
                {
                    if (oPlayerSetting.PrivateMessageEnabled)
                    {
                        PlayerTo.sendMessage(string.Format(sMessageFormat, sPrivMessageIndicator, PlayerFrom.name, Message), 255, nMessageColor[0], nMessageColor[1], nMessageColor[2]);
                    }
                    else
                    {
                        PlayerFrom.sendMessage(string.Format("Player {0} has private messaging disabled", PlayerTo.name));
                    }
                }
                else
                {
                    PlayerTo.sendMessage(string.Format(sMessageFormat, sPrivMessageIndicator, PlayerFrom.name, Message), 255, nMessageColor[0], nMessageColor[1], nMessageColor[2]);
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
            oPlayerSetting.PrivateMessageEnabled = false;
            oPlayerSettings.Add(oPlayerSetting);

            return oPlayerSetting;
        }

        public void UpdatePlayerSetting(Player Player, Player PlayerFrom)
        {
            if (Player == null || PlayerFrom == null)
                throw new Exception("Invalid player");

            PlayerPrivateMessageSettings oPlayerSetting = null;

            if (!this.PlayerSettingExists(Player))
            {
                oPlayerSetting = this.CreatePlayerSetting(Player);
            }
            else
            {
                oPlayerSetting = this.GetPlayerSetting(Player);
            }

            oPlayerSetting.LastMessageFrom = PlayerFrom;
        }

        public PlayerPrivateMessageSettings GetPlayerSetting(Player Player)
        {
            if (Player == null)
                throw new Exception("Invalid player");

            var oPlayer = from player in oPlayerSettings where player.Player == Player select player;
            if (oPlayer is PlayerPrivateMessageSettings)
            {
                PlayerPrivateMessageSettings oPlayerSetting = (PlayerPrivateMessageSettings)oPlayer;

                return oPlayerSetting;
            }
            else
                return null;
        }

        public bool PlayerSettingExists(Player Player)
        {
            if (Player == null)
                throw new Exception("Invalid player");

            var oPlayer = from player in oPlayerSettings where player.Player == Player select player;
            if (oPlayer is PlayerPrivateMessageSettings)
                return true;
            else
                return false;
        }

        public Player GetLastPlayerSentTo(Player Player)
        {
            if (Player == null)
                throw new Exception("Invalid player");

            var oPlayer = from player in oPlayerSettings where player.Player == Player select player;
            if (oPlayer is PlayerPrivateMessageSettings)
            {
                PlayerPrivateMessageSettings oPlayerSetting = (PlayerPrivateMessageSettings)oPlayer;

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

        public string MessageFormat
        {
            get
            {
                return sMessageFormat;
            }
            set
            {
                sMessageFormat = value;
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

    public class PlayerPrivateMessageSettings
    {
        public Player Player { get; set; }
        public Player LastMessageFrom { get; set; }
        public bool PrivateMessageEnabled { get; set; }
    }
}
