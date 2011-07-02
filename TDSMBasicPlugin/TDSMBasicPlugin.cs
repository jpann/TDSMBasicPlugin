using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TDSMBasicPlugin;
using Terraria_Server.Plugin;
using Terraria_Server;
using Terraria_Server.Events;
using Terraria_Server.Commands;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace TDSMBasicPlugin
{
    public class TDSMBasicPlugin : Plugin
    {
        public bool isEnabled = false;
        private static string sPluginDir = null;
        private static string sItemExportFile = "items";
        private static PrivateMessageManager oPrivateMsgManager = null;
        public static MyPlayer[] Players = new MyPlayer[Main.maxPlayers];

        public override void Load()
        {
            Name = "TDSMBasicPlugin";
            Description = "TDSMBasicPlugin.";
            Author = "attak";
            Version = "1";
            TDSMBuild = 12;
            ServerProtocol = "1.05";

            isEnabled = true;

            sPluginDir = Statics.getPluginPath + Statics.systemSeperator + "TDSMBasic";
            sItemExportFile = Path.Combine(sPluginDir, sItemExportFile);

            if (!Program.createDirectory(sPluginDir, true))
            {
                Console.WriteLine("Failed to create crucial Folder");
                return;
            }

            oPrivateMsgManager = new PrivateMessageManager();
            oPrivateMsgManager.Server = this.Server;
        }

        public override void Disable()
        {
            Console.WriteLine(base.Name + " disabled.");
            isEnabled = false;

            oPrivateMsgManager.Unload();
        }

        public override void Enable()
        {
            Console.WriteLine(base.Name + " enabled.");

            this.registerHook(Hooks.TILE_BREAK);
            this.registerHook(Hooks.PLAYER_COMMAND);
            this.registerHook(Hooks.PLAYER_CHAT);
            this.registerHook(Hooks.PLAYER_CHEST);
            this.registerHook(Hooks.PLAYER_HURT);
            this.registerHook(Hooks.PLAYER_LOGIN);
            this.registerHook(Hooks.PLAYER_LOGOUT);
            this.registerHook(Hooks.PLAYER_PARTYCHANGE);
            this.registerHook(Hooks.PLAYER_PRELOGIN);
            this.registerHook(Hooks.PLAYER_STATEUPDATE);
            this.registerHook(Hooks.CONSOLE_COMMAND);
            this.registerHook(Hooks.PLAYER_LOGIN);
            this.registerHook(Hooks.PLAYER_DEATH);
        }

        #region Hooks
        public override void onPlayerDeath(PlayerDeathEvent Event)
        {

        }

        public override void onPlayerCommand(PlayerCommandEvent Event)
        {
            if (isEnabled == false) { return; }
            string[] commands = Event.getMessage().ToLower().Split(' ');
            string sCommand = Event.getMessage().ToLower();

            string sSenderName = ((Player)Event.getSender()).getName();

            if (commands.Length > 0)
            {
                try
                {
                    string sPlayerCmdPrivMsg = @"(/pm|/privmsg)\s+(?<flag>[on|off]+)?";
                    string sPlayerCmdHeal = @"/heal(?:\s+(?<player>.*))?";
                    string sPlayerCmdReply = @"[/reply|/r]\s(?<msg>.+)?";
                    string sPlayerCmdMessage = @"[/m|/msg]\s(?<player>[A-Z-a-z0-9]+)\s(?<msg>.+)?";

                    MyPlayer oPlayer = Utility.FindPlayer(((Player)Event.getSender()).getName());
                    Match commandMatch;

                    if (oPlayer.IsOp())
                    {
                        // Op commands
                        commandMatch = Regex.Match(sCommand, sPlayerCmdHeal, RegexOptions.IgnoreCase);
                        if (commandMatch.Success)
                        {
                            // Heal
                            string targetPlayerName = commandMatch.Groups["player"].Value;

                            Heal(oPlayer, targetPlayerName);
                            Event.setCancelled(true);
                            return;
                        }
                    }

                    commandMatch = Regex.Match(sCommand, sPlayerCmdPrivMsg, RegexOptions.IgnoreCase);
                    if (commandMatch.Success)
                    {
                        // PrivMsg On/Off
                        string flag = commandMatch.Groups["flag"].Value;

                        PrivateMessageEnableDisable(oPlayer, flag);
                        Event.setCancelled(true);
                        return;
                    }

                    commandMatch = Regex.Match(sCommand, sPlayerCmdReply, RegexOptions.IgnoreCase);
                    if (commandMatch.Success)
                    {
                        // Reply
                        string message = commandMatch.Groups["msg"].Value;

                        ReplyMessage(oPlayer, message);
                        Event.setCancelled(true);
                        return;
                    }

                    commandMatch = Regex.Match(sCommand, sPlayerCmdMessage, RegexOptions.IgnoreCase);
                    if (commandMatch.Success)
                    {
                        // Message
                        string playerName = commandMatch.Groups["player"].Value;
                        string message = commandMatch.Groups["msg"].Value;

                        PrivateMessage(oPlayer, playerName, message);

                        Event.setCancelled(true);
                        return;
                    }
                }
                catch (Exception er)
                {
                    Event.getSender().sendMessage(string.Format("Error: {0}", er.Message));
                    Console.WriteLine(string.Format("Error processing command '{0}': {1}", commands[0], er.Message));
                }
            }
        }

        public override void onTileBreak(TileBreakEvent Event)
        {
            
        }

        public override void onPlayerChat(PlayerChatEvent Event) 
        { 

        }

        public override void onPlayerCommandProcess(ConsoleCommandEvent Event)
        {
            if (isEnabled == false) { return; }

            string[] commands = Event.getMessage().ToLower().Split(' '); 
            if (commands.Length > 0)
            {
                if (commands[0] != null && commands[0].Trim().Length > 0) 
                {
                    if (commands[0].Equals("exportitems"))
                    {
                        ExportItems(Event.getSender(), commands);

                        Event.setCancelled(true);
                    }
                }

                if (commands[0] != null && commands[0].Trim().Length > 0)
                {
                    if (commands[0].Equals("save-settings"))
                    {
                        oPrivateMsgManager.Unload();

                        Event.setCancelled(true);
                    }
                }
            }
        }

        public override void onPlayerHurt(PlayerHurtEvent Event)
        {
            Event.setCancelled(false);
        }

        public override void onPlayerJoin(PlayerLoginEvent Event) 
        {
            int nPlayerIndex = Event.getPlayer().whoAmi;

            MyPlayer oPlayer = new MyPlayer(nPlayerIndex);

            Players[nPlayerIndex] = oPlayer;

            Event.setCancelled(false);
        }

        public override void onPlayerPreLogin(PlayerLoginEvent Event) 
        {
            Event.setCancelled(false);
        }

        public override void onPlayerLogout(PlayerLogoutEvent Event)
        {
            int nPlayerIndex = Event.getPlayer().whoAmi;

            MyPlayer oPlayer = Players[nPlayerIndex];

            Players[nPlayerIndex] = null;

            Event.setCancelled(false);
        }

        public override void onPlayerPartyChange(PartyChangeEvent Event) 
        {
            Event.setCancelled(false);
        }

        public override void onPlayerOpenChest(ChestOpenEvent Event) 
        {
            Event.setCancelled(false);
        }

        public override void onPlayerStateUpdate(PlayerStateUpdateEvent Event) 
        {
            Event.setCancelled(false);
        }
        #endregion

        #region Server Command Methods
        public static void ExportItems(Sender sender, string[] commands)
        {
            string sFile = null;

            if (commands.Length > 1 && commands[1] != null && commands[1].Trim().Length > 0)
            {
                string sOutType = commands[1].Trim();

                Item[] items = new Item[Main.maxItemTypes];

                for (int i = 0; i < Main.maxItemTypes; i++)
                {
                    items[i] = new Item();
                    items[i].SetDefaults(i);
                }

                try
                {
                    if (sOutType == "json")
                        sFile = ItemSerializer.ObjectToJson(items, string.Format("{0}.{1}", sItemExportFile, sOutType));

                    if (string.IsNullOrEmpty(sFile))
                        throw new Exception("Output type not selected.");

                    sender.sendMessage(string.Format("Items exported to {0}", sFile));
                }
                catch (Exception er)
                {
                    sender.sendMessage(string.Format("Error exporting items: {0}", er.Message));
                }
            }
            else
            {
                sender.sendMessage("Command Error: /exportitems <output type>");
            }
        }
        #endregion

        #region Command Methods
        /// <summary>
        /// Privates the message enable disable.
        /// </summary>
        /// <param name="Player">The player.</param>
        /// <param name="Flag">The flag.</param>
        public static void PrivateMessageEnableDisable(MyPlayer Player, string Flag)
        {
            if (Player == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(Flag))
            {
                if (Flag.Equals("on"))
                {
                    oPrivateMsgManager.PrivateMessageEnableDisable(Player, true);
                }
                else if (Flag.Equals("off"))
                {
                    oPrivateMsgManager.PrivateMessageEnableDisable(Player, false);
                }
            }
            else
            {
                Player.SendMessage("Command Error: /privmsg <on|off>");
            }
        }

        /// <summary>
        /// Heals the specified player.
        /// </summary>
        /// <param name="Player">The player.</param>
        /// <param name="TargetPlayerName">Name of the target player.</param>
        public static void Heal(MyPlayer Player, string TargetPlayerName)
        {
            int stack = 20;
            MyPlayer oTargetPlayer = null;

            if (Player == null)
            {
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(TargetPlayerName))
                {
                    oTargetPlayer = Utility.FindPlayer(TargetPlayerName);

                    if (oTargetPlayer == null)
                    {
                        Player.SendMessage("Player '" + TargetPlayerName + "' not found!");
                        return;
                    }
                }
                else
                {
                    oTargetPlayer = Player;
                }

                if (oTargetPlayer != null)
                {
                    oTargetPlayer.HealLife(stack);
                    oTargetPlayer.HealMana(stack);

                    //Server.notifyOps(string.Format("{0} is healing {1}", Player.Name, oTargetPlayer.Name), false);
                    Player.SendMessage(string.Format("You healed {0}", oTargetPlayer.Name));
                    oTargetPlayer.SendMessage(string.Format("You were healed by {0}", Player.Name));

                    return;
                }

            }
            catch (Exception er)
            {
                Player.SendMessage(string.Format("Command Error: {0}", er.Message));
                Program.tConsole.WriteLine(string.Format("Exception executing command from {0}: {1}", Player.Name, er.Message));
                Program.tConsole.WriteLine(string.Format("Exception Stack Trace:\n\r{0}", er.StackTrace));
            }
        }

        /// <summary>
        /// Replies the message.
        /// </summary>
        /// <param name="Player">The player.</param>
        /// <param name="Message">The message.</param>
        public static void ReplyMessage(MyPlayer Player, string Message)
        {
            if (Player == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(Message))
            {
                try
                {
                    oPrivateMsgManager.SendReply(Player, Message);
                }
                catch (Exception er)
                {
                    Player.SendMessage(string.Format("Reply Error: {0}", er.Message));
                    Console.WriteLine(string.Format("Reply Error: {0}", er.Message));
                }
            }
            else
            {
                Player.SendMessage("Command Error: /reply <message>");
            }
        }

        /// <summary>
        /// Privates the message.
        /// </summary>
        /// <param name="Player">The player.</param>
        /// <param name="PlayerNameTo">The player name to.</param>
        /// <param name="Message">The message.</param>
        public static void PrivateMessage(MyPlayer Player, string PlayerNameTo, string Message)
        {
            if (Player == null)
            {
                return;
            }

            try
            {
                MyPlayer oTargetPlayer = Utility.FindPlayer(PlayerNameTo);
                if (oTargetPlayer != null)
                {
                    try
                    {
                        oPrivateMsgManager.SendMessage(oTargetPlayer, Player, Message, true);
                    }
                    catch (Exception er)
                    {
                        Player.SendMessage(string.Format("Private Message Error: {0}", er.Message));
                        Console.WriteLine(string.Format("Private Message Error: {0}", er.Message));
                    }
                }
                else
                {
                    Player.SendMessage("Player not found!");
                }
            }
            catch (Exception er)
            {
                Player.SendMessage(string.Format("Command Error: {0}", er.Message));
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the plugin directory.
        /// </summary>
        /// <returns></returns>
        public static string GetPluginDirectory()
        {
            return sPluginDir;
        }
        #endregion
    }
}
