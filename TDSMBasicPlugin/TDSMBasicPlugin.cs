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
            if (commands.Length > 0)
            {
                try
                {
                    string sPlayerCmdPrivMsg = @"/privmsg\s+(?<flag>[on|off]+)?";
                    string sPlayerCmdHeal = @"/heal\s+(?<player>.+)?";
                    string sPlayerCmdReply = @"/[r|reply]\s+(?<msg>.+)?";
                    string sPlayerCmdMessage = @"/[m|msg]\s+(?<player>[A-Z-a-z0-9\ ]+)\s+(?<msg>.+)?";

                    Player oPlayer = ((Player)Event.getSender());
                    Match commandMatch;

                    if (CheckForOp(oPlayer))
                    {
                        // Op commands
                        commandMatch = Regex.Match(sCommand, sPlayerCmdHeal, RegexOptions.IgnoreCase);
                        if (commandMatch.Success)
                        {
                            // Heal
                            Heal(Event.getSender(), commandMatch);
                            Event.setCancelled(true);
                            return;
                        }
                    }

                    commandMatch = Regex.Match(sCommand, sPlayerCmdPrivMsg, RegexOptions.IgnoreCase);
                    if (commandMatch.Success)
                    {
                        // PrivMsg On/Off
                        PrivateMessage(Event.getSender(), commandMatch);
                        Event.setCancelled(true);
                        return;
                    }

                    commandMatch = Regex.Match(sCommand, sPlayerCmdReply, RegexOptions.IgnoreCase);
                    if (commandMatch.Success)
                    {
                        // Reply
                        ReplyMessage(Event.getSender(), commandMatch);
                        Event.setCancelled(true);
                        return;
                    }

                    commandMatch = Regex.Match(sCommand, sPlayerCmdMessage, RegexOptions.IgnoreCase);
                    if (commandMatch.Success)
                    {
                        // Message
                        PrivateMessage(Event.getSender(), commandMatch);
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
            Event.setCancelled(false);
        }

        public override void onPlayerPreLogin(PlayerLoginEvent Event) 
        {
            Event.setCancelled(false);
        }

        public override void onPlayerLogout(PlayerLogoutEvent Event)
        {
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
        public static void PrivateMessageEnableDisable(Sender sender, Match match)
        {
            if (!(sender is Player))
            {
                return;
            }

            if (match.Success)
            {
                string flag = match.Groups["flag"].Value;

                if (flag.Equals("on"))
                {
                    oPrivateMsgManager.PrivateMessageEnableDisable((Player)sender, true);
                }
                else if (flag.Equals("off"))
                {
                    oPrivateMsgManager.PrivateMessageEnableDisable((Player)sender, false);
                }
            }
            else
            {
                sender.sendMessage("Command Error: /privmsg <on|off>");
            }
        }

        // Taken from the TDSM Forums but slightly modified
        //public static void Give(Sender sender, string[] commands)
        //{
        //    string sCommand = Program.mergeStrArray(commands);

        //    if (!(sender is Player))
        //    {
        //        return;
        //    }

        //    //TODO Need to redo this so it matches Heal()

        //    // /give <item> <stack> <player>
        //    Match match = Regex.Match(sCommand, @"/give\s+(?<item>[A-Z-a-z0-9\ ]+)\s+(?<stack>[0-9]+)\s+(?<player>.+)?", RegexOptions.IgnoreCase);
        //    if (match.Success)
        //    {
        //        string itemName = match.Groups["item"].Value;
        //        string stack = match.Groups["stack"].Value;
        //        string playerName = match.Groups["player"].Value;

        //        Player player = Program.server.GetPlayerByName(playerName);
        //        if (player != null)
        //        {
        //            Item[] items = new Item[Main.maxItemTypes];
        //            for (int i = 0; i < Main.maxItemTypes; i++)
        //            {
        //                items[i] = new Item();
        //                items[i].SetDefaults(i);
        //            }

        //            Item item = null;
        //            itemName = itemName.Replace(" ", "").ToLower();
        //            for (int i = 0; i < Main.maxItemTypes; i++)
        //            {
        //                if (items[i].name != null)
        //                {
        //                    string genItemName = items[i].name.Replace(" ", "").Trim().ToLower();
        //                    if (genItemName == itemName)
        //                    {
        //                        item = items[i];
        //                    }
        //                }
        //            }

        //            int itemType = -1;
        //            bool assumed = false;
        //            if (item != null)
        //            {
        //                itemType = item.type;
        //            }
        //            else
        //            {
        //                int assumedItem;
        //                try
        //                {
        //                    assumedItem = Int32.Parse(itemName);
        //                }
        //                catch (Exception)
        //                {
        //                    sender.sendMessage("Item '" + itemName + "' not found!");
        //                    return;
        //                }

        //                for (int i = 0; i < Main.maxItemTypes; i++)
        //                {
        //                    if (items[i].type == assumedItem)
        //                    {
        //                        itemType = items[i].type;
        //                        assumed = true;
        //                        break;
        //                    }
        //                }

        //                if (!assumed)
        //                {
        //                    sender.sendMessage("Item '" + itemName + "' not found!");
        //                    return;
        //                }
        //            }

        //            //Clear Data
        //            for (int i = 0; i < Main.maxItemTypes; i++)
        //            {
        //                items[i] = null;
        //            }
        //            items = null;

        //            if (itemType != -1)
        //            {

        //                int stackSize;
        //                try
        //                {
        //                    stackSize = Int32.Parse(stack);
        //                }
        //                catch (Exception)
        //                {
        //                    stackSize = 1;
        //                }

        //                Item.NewItem((int)player.position.X, (int)player.position.Y, player.width, player.height, itemType, stackSize, false);

        //                Program.server.notifyOps("Giving " + player.name + " some " + itemType.ToString() + " {" + sender.getName() + "}", true);

        //                return;
        //            }
        //        }
        //        else
        //        {
        //            sender.sendMessage("Player '" + playerName + "' not found!");
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        sender.sendMessage("Command Error: /give <item> <stack> <player>");
        //    }
        //}

        // Heal player

        public static void Heal(Sender sender, Match match)
        {
            int stack = 20;
            Player player = null;

            if (!(sender is Player))
            {
                return;
            }

            try
            {
                if (match.Success)
                {
                    string playerName = match.Groups["player"].Value;

                    player = Program.server.GetPlayerByName(playerName);
                    if (player == null)
                    {
                        sender.sendMessage("Player '" + playerName + "' not found!");
                        return;
                    }
                }
                else
                {
                    // Heal self
                    if (!(sender is Player))
                    {
                        return;
                    }

                    player = ((Player)sender);
                }

                if (player != null)
                {
                    Item heart = GetItemById(58);
                    Item mana = GetItemByName("star");

                    if (heart == null || mana == null) // Heart or Mana Crystal not found!
                    {
                        sender.sendMessage("Heart and Mana Crystal items not found!");
                        return;
                    }

                    for (int i = 0; i < stack; i++)
                    {
                        RestorePlayerHealth(sender, player);
                        RestorePlayerMana(sender, player);
                    }

                    if ((Player)sender != player)
                    {
                        Program.server.notifyOps(string.Format("{0} is healing {1}", sender.getName(), player.name));
                        player.sendMessage(string.Format("You were healed by {0}", sender.getName()));
                    }

                    return;
                }

            }
            catch (Exception er)
            {
                sender.sendMessage(string.Format("Command Exception: {0}", er.Message));
                Program.tConsole.WriteLine(string.Format("Exception executing command from {0}: {1}", sender.getName(), er.Message));
                Program.tConsole.WriteLine(string.Format("Exception Stack Trace:\n\r{0}", er.StackTrace));
            }
        }

        // Reply to last received message
        public static void ReplyMessage(Sender sender, Match match)
        {
            if (!(sender is Player))
            {
                return;
            }

            if (match.Success)
            {
                string message = match.Groups["msg"].Value;

                try
                {
                    oPrivateMsgManager.SendReply((Player)sender, message);
                }
                catch (Exception er)
                {
                    sender.sendMessage(string.Format("Reply Error: {0}", er.Message));
                    Console.WriteLine(string.Format("Reply Error: {0}", er.Message));
                }
            }
            else
            {
                sender.sendMessage("Command Error: /reply <message>");
            }
        }

        // Private message a player
        public static void PrivateMessage(Sender sender, Match match)
        {
            if (!(sender is Player))
            {
                return;
            }

            if (match.Success)
            {
                string playerName = match.Groups["player"].Value;
                string message = match.Groups["msg"].Value;

                Player player = Program.server.GetPlayerByName(playerName);
                if (player != null)
                {
                    try
                    {
                        PrivateMessagePlayer((Player)sender, player, message);
                    }
                    catch (Exception er)
                    {
                        sender.sendMessage(string.Format("Private Message Error: {0}", er.Message));
                        Console.WriteLine(string.Format("Private Message Error: {0}", er.Message));
                    }
                }
                else
                {
                    sender.sendMessage("Player not found!");
                }
            }
            else
            {
                sender.sendMessage("Command Error: /msg <player> <message>");
            }
        }
        #endregion

        #region Methods
        public static void PrivateMessagePlayer(Player PlayerFrom, Player PlayerTo, string Message)
        {
            if (PlayerFrom == null || PlayerTo == null)
                return;

            oPrivateMsgManager.SendMessage(PlayerTo, PlayerFrom, Message, true); //test
        }

        public static void RestorePlayerHealth(Sender sender, Player player)
        {
            Item heart = GetItemById(58);

            if (heart == null)
            {
                sender.sendMessage("Unable to heal: Unable to find heart item.");
                return;
            }

            Item.NewItem((int)player.position.X, (int)player.position.Y, player.width, player.height, heart.type, 20, false);
        }
        
        public static void RestorePlayerMana(Sender sender, Player player)
        {
            Item star = GetItemByName("star");

            if (star == null)
            {
                sender.sendMessage("Unable to restore mana: Unable to find star item.");
                return;
            }

            Item.NewItem((int)player.position.X, (int)player.position.Y, player.width, player.height, star.type, 20, false);
        }

        public static bool CheckForOp(Player player)
        {
            if (player.isOp())
                return true;
            else
                return false;
        }

        public static Item GetItemById(int Id)
        {
            Item[] items = new Item[Main.maxItemTypes];
            for (int i = 0; i < Main.maxItemTypes; i++)
            {
                items[i] = new Item();
                items[i].SetDefaults(i);
            }

            Item item = null;
            for (int i = 0; i < Main.maxItemTypes; i++)
            {
                if (items[i].name != null)
                {
                    if (i == Id)
                    {
                        item = items[i];
                    }
                }
            }

            for (int i = 0; i < Main.maxItemTypes; i++)
            {
                items[i] = null;
            }
            items = null;

            return item;
        }

        public static Item GetItemByName(string ItemName)
        {
            Item[] items = new Item[Main.maxItemTypes];
            for (int i = 0; i < Main.maxItemTypes; i++)
            {
                items[i] = new Item();
                items[i].SetDefaults(i);
            }

            Item item = null;
            ItemName = ItemName.Replace(" ", "").ToLower();
            for (int i = 0; i < Main.maxItemTypes; i++)
            {
                if (items[i].name != null)
                {
                    string genItemName = items[i].name.Replace(" ", "").Trim().ToLower();
                    if (genItemName == ItemName)
                    {
                        item = items[i];
                    }
                }
            }

            for (int i = 0; i < Main.maxItemTypes; i++)
            {
                items[i] = null;
            }
            items = null;

            return item;
        }

        public static string GetPluginDirectory()
        {
            return sPluginDir;
        }
        #endregion
    }
}
