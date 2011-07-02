using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria_Server;

namespace TDSMBasicPlugin
{
    internal class Utility
    {
        /// <summary>
        /// Gets the player IP address.
        /// </summary>
        /// <param name="PlayerName">Name of the player.</param>
        /// <returns></returns>
        public static string GetPlayerIPAddress(string PlayerName)
        {
            foreach (MyPlayer player in TDSMBasicPlugin.Players)
            {
                if (player != null && player.Active)
                {
                    if (PlayerName.ToLower() == player.Name.ToLower())
                    {
                        return player.IPAddress;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the player.
        /// </summary>
        /// <param name="PlayerName">Name of the player.</param>
        /// <returns></returns>
        public static MyPlayer FindPlayer(string PlayerName)
        {
            PlayerName = PlayerName.ToLower();

            foreach (MyPlayer oPlayer in TDSMBasicPlugin.Players)
            {
                if (oPlayer == null)
                    continue;

                string sName = oPlayer.Name.ToLower();

                if (sName.Equals(PlayerName))
                    return oPlayer;
            }

            return null;
        }

        /// <summary>
        /// Finds the players.
        /// </summary>
        /// <param name="PlayerName">Name of the player.</param>
        /// <returns></returns>
        public static List<MyPlayer> FindPlayers(string PlayerName)
        {
            List<MyPlayer> oPlayers = new List<MyPlayer>();

            PlayerName = PlayerName.ToLower();

            foreach (MyPlayer oPlayer in TDSMBasicPlugin.Players)
            {
                if (oPlayer == null)
                    continue;

                string sName = oPlayer.Name.ToLower();

                if (sName.Equals(PlayerName))
                    return new List<MyPlayer> { oPlayer };

                if (sName.Contains(PlayerName))
                    oPlayers.Add(oPlayer);
            }

            return oPlayers;
        }

        /// <summary>
        /// Gets the item by id.
        /// </summary>
        /// <param name="Id">The id.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the name of the item by.
        /// </summary>
        /// <param name="ItemName">Name of the item.</param>
        /// <returns></returns>
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
    }
}
