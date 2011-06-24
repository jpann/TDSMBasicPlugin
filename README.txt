This is the start of a basic TDSM plugin.

Operator commands:

/give <item> <stack size> <player>
	
- Item: Name of the item.
- stack size: How many of the item to give.
- player: Name of player to give item to.

/heal [player]

- player: Name of player to heal/mana restore. If this is left out, it will heal you.


Server commands:

exportitems <type>

- type: Right now this only supports a type of 'json'
