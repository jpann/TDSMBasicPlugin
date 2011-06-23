using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TDSMBasicPlugin;
using Terraria_Server.Plugin;
using Terraria_Server;
using Terraria_Server.Events;
using Terraria_Server.Commands;

namespace TDSMBasicPlugin
{
    public class TDSMBasicPlugin : Plugin
    {
        public bool isEnabled = false;

        public override void Load()
        {
            Name = "TDSMBasicPlugin";
            Description = "TDSMBasicPlugin.";
            Author = "attak";
            Version = "1";
            ServerProtocol = "1.04";

            isEnabled = true;

        }

        public override void Disable()
        {
            Console.WriteLine(base.Name + " disabled.");
            isEnabled = false;
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
        }

        public override void onPlayerCommand(PlayerCommandEvent Event)
        {
            if (isEnabled == false) { return; }
            string[] commands = Event.getMessage().ToLower().Split(' ');
            if (commands.Length > 0)
            {
                if (commands[0] != null && commands[0].Trim().Length > 0)
                {
                    if (commands[0].Equals("/give"))
                    {
                        // Taken from the TDSM forums
                        Give(Event.getSender(), commands);

                        Event.setCancelled(true);
                    }
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
                    // Probably should intercept "help" so we can add "exportitems" to the list of commands?
                    //...

                    if (commands[0].Equals("exportitems"))
                    {
                        ExportItems(Event.getSender(), commands);

                        Event.setCancelled(true);
                    }
                }
            }
        }

        public override void onPlayerHurt(PlayerHurtEvent Event) 
        { 
        
        }
        public override void onPlayerJoin(LoginEvent Event) 
        { 
        
        }

        public override void onPlayerPreLogin(LoginEvent Event) 
        { 
        
        }

        public override void onPlayerLogout(LogoutEvent Event)
        {

        }

        public override void onPlayerPartyChange(PartyChangeEvent Event) 
        { 

        }

        public override void onPlayerOpenChest(ChestOpenEvent Event) 
        { 
        
        }

        public override void onPlayerStateUpdate(PlayerStateUpdateEvent Event) 
        { 
        
        }

        // This will eventually export all items into an XML file
        public static void ExportItems(Sender sender, string[] commands)
        {
            if (commands.Length > 1 && commands[1] != null && commands[1].Trim().Length > 0)
            {
                string fileName = commands[1].Trim();

                Console.WriteLine("Loading items...");
                Item[] items = new Item[Main.maxItemTypes];
                for (int i = 0; i < Main.maxItemTypes; i++)
                {
                    items[i] = new Item();
                    items[i].SetDefaults(i);

                    Console.WriteLine(string.Format("{0} - {1} [{2}]", i, items[i].name, items[i].toolTip));
                }
                Console.WriteLine(string.Format("{0} items loaded!", items.Count()));
            }
            else
            {
                sender.sendMessage("Command Error: /exportitems <file>");
            }
        }

        // Taken from the TDSM Forums
        public static void Give(Sender sender, string[] commands)
        {
            if (sender is Player)
            {
                Player player = ((Player)sender);
                if (!player.isOp())
                {
                    player.sendMessage("You Cannot Perform That Action.", 255, 238f, 130f, 238f);
                    return;
                }
            }

            // /give <player> <stack> <name> 
            if (commands.Length > 3 && commands[1] != null && commands[2] != null && commands[3] != null &&
                commands[1].Trim().Length > 0 && commands[2].Trim().Length > 0 && commands[3].Trim().Length > 0)
            {
                string playerName = commands[1].Trim();
                string itemName = Program.mergeStrArray(commands);
                itemName = itemName.Remove(0, itemName.IndexOf(" " + commands[3]));

                Player player = Program.server.GetPlayerByName(playerName);
                if (player != null)
                {
                    Item[] items = new Item[Main.maxItemTypes];
                    for (int i = 0; i < Main.maxItemTypes; i++)
                    {
                        items[i] = new Item();
                        items[i].SetDefaults(i);
                    }

                    Item item = null;
                    itemName = itemName.Replace(" ", "").ToLower();
                    for (int i = 0; i < Main.maxItemTypes; i++)
                    {
                        if (items[i].name != null)
                        {
                            string genItemName = items[i].name.Replace(" ", "").Trim().ToLower();
                            if (genItemName == itemName)
                            {
                                item = items[i];
                            }
                        }
                    }

                    int itemType = -1;
                    bool assumed = false;
                    if (item != null)
                    {
                        itemType = item.type;
                    }
                    else
                    {
                        int assumedItem;
                        try
                        {
                            assumedItem = Int32.Parse(itemName);
                        }
                        catch (Exception)
                        {
                            sender.sendMessage("Item '" + itemName + "' not found!");
                            return;
                        }

                        for (int i = 0; i < Main.maxItemTypes; i++)
                        {
                            if (items[i].type == assumedItem)
                            {
                                itemType = items[i].type;
                                assumed = true;
                                break;
                            }
                        }

                        if (!assumed)
                        {
                            sender.sendMessage("Item '" + itemName + "' not found!");
                            return;
                        }
                    }

                    //Clear Data
                    for (int i = 0; i < Main.maxItemTypes; i++)
                    {
                        items[i] = null;
                    }
                    items = null;

                    if (itemType != -1)
                    {

                        int stackSize;
                        try
                        {
                            stackSize = Int32.Parse(commands[2]);
                        }
                        catch (Exception)
                        {
                            stackSize = 1;
                        }

                        Item.NewItem((int)player.position.X, (int)player.position.Y, player.width, player.height, itemType, stackSize, false);

                        Program.server.notifyOps("Giving " + player.name + " some " + itemType.ToString() + " {" + sender.getName() + "}", true);

                        return;
                    }
                }
                else
                {
                    sender.sendMessage("Player '" + playerName + "' not found!");
                    return;
                }
            }
            else
            {
                goto ERROR;
            }

        ERROR:
            sender.sendMessage("Command Error!");
        }
    }
}
