This is the start of a basic TDSM plugin.

Operator commands:

/give <item> <stack size> <player>

Gives target player the specified item.

Parameters:

item: 	Name of the item.
stack: 	size: How many of the item to give.
player: Name of player to give item to.

/heal <player>

Heals target player.

Parameters:

player: Name of player to heal/mana restore. If this is left out, it will heal you.


User commands:

/msg <player> <message>

Sends a private message to the player.

/r <message>

Sends a reply to the last player that sent you a private message.

/privmsg <on|off>

Enable or disable private messaging. When disabled, you will not be able to receive private messages.


Server commands:

exportitems <type>

Parameters:

type: Right now this only supports a type of 'json'

save-settings

Manually saves the player private message settings to .\Plugins\TDSMBasic\player-privmsg-settings.json

