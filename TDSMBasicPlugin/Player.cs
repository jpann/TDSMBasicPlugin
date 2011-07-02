using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria_Server;

namespace TDSMBasicPlugin
{
    /// <summary>
    /// 
    /// </summary>
    public class MyPlayer
    {
        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>The index.</value>
        public int Index { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyPlayer"/> class.
        /// </summary>
        /// <param name="player">The player.</param>
        public MyPlayer(int player)
        {
            Index = player;
        }

        /// <summary>
        /// Gets the player.
        /// </summary>
        /// <value>The player.</value>
        public Player Player
        {
            get
            {
                return Main.player[Index];
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get 
            { 
                return this.Player.name; 
            }
        }

        /// <summary>
        /// Gets the IP address.
        /// </summary>
        /// <value>The IP address.</value>
        public string IPAddress
        {
            get
            {
                return Netplay.serverSock[Index].tcpClient.Client.RemoteEndPoint.ToString().Split(':')[0];
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="MyPlayer"/> is active.
        /// </summary>
        /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
        public bool Active
        {
            get { return Player != null && Player.active; }
        }

        /// <summary>
        /// Heals the life.
        /// </summary>
        /// <param name="Amount">The amount.</param>
        public void HealLife(int Amount)
        {
            Item heart = Utility.GetItemById(58);

            if (heart == null)
            {
                throw new Exception("Unable to heal: Unable to find heart item.");
            }

            for (int i = 0; i < Amount; i++)
                Item.NewItem((int)Player.position.X, (int)Player.position.Y, Player.width, Player.height, heart.type, 20, false);
        }

        /// <summary>
        /// Heals the mana.
        /// </summary>
        /// <param name="Amount">The amount.</param>
        public void HealMana(int Amount)
        {
            Item star = Utility.GetItemByName("star");

            if (star == null)
            {
                throw new Exception("Unable to restore mana: Unable to find star item.");
            }

            for (int i = 0; i < Amount; i++)
                Item.NewItem((int)Player.position.X, (int)Player.position.Y, Player.width, Player.height, star.type, 20, false);
        }

        /// <summary>
        /// Determines whether this instance is op.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is op; otherwise, <c>false</c>.
        /// </returns>
        public bool IsOp()
        {
            if (Player.isOp())
                return true;
            else
                return false;
        }

        /// <summary>
        /// Spawns this instance.
        /// </summary>
        public void Spawn()
        {
            SendData(PacketTypes.PlayerSpawn, "", Index, 0.0f, 0.0f, 0.0f);
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void SendMessage(string msg)
        {
            SendMessage(msg, 0, 255, 0);
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="color">The color.</param>
        public void SendMessage(string msg, Color color)
        {
            SendMessage(msg, color.R, color.G, color.B);
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="red">The red.</param>
        /// <param name="green">The green.</param>
        /// <param name="blue">The blue.</param>
        public void SendMessage(string msg, int red, int green, int blue)
        {
            SendData(PacketTypes.ChatText, msg, 255, red, green, blue);
        }

        /// <summary>
        /// Damages the player.
        /// </summary>
        /// <param name="Damage">The damage.</param>
        public void DamagePlayer(int Damage)
        {
            NetMessage.SendData((int)PacketTypes.PlayerDamage, -1, -1, "", Index, ((new Random()).Next(-1, 1)), Damage, (float)0);
        }

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="msgType">Type of the MSG.</param>
        /// <param name="text">The text.</param>
        /// <param name="number">The number.</param>
        /// <param name="number2">The number2.</param>
        /// <param name="number3">The number3.</param>
        /// <param name="number4">The number4.</param>
        /// <param name="number5">The number5.</param>
        public void SendData(PacketTypes msgType, string text = "", int number = 0, float number2 = 0f, float number3 = 0f, float number4 = 0f, int number5 = 0)
        {
            NetMessage.SendData((int)msgType, Index, -1, text, number, number2, number3, number4, number5);
        }
    }
}
