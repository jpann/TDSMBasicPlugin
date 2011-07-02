This is the start of a basic TDSM plugin.

OPERATOR COMMANDS

/heal <player>

Heals target player.

Parameters:

player: Name of player to heal/mana restore. If this is left out, it will heal you.


USER COMMANDS

/msg <player> <message>

Sends a private message to the player.

/r <message>

Sends a reply to the last player that sent you a private message.

/privmsg <on|off>

Enable or disable private messaging. When disabled, you will not be able to receive private messages.


SERVER COMMANDS

exportitems <type>

Parameters:

type: Right now this only supports a type of 'json'

save-settings

Manually saves the player private message settings to .\Plugins\TDSMBasic\player-privmsg-settings.json

