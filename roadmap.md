We are starting with an empty godot 4 project using c#. We are creating a 2D turn based tactics game.
for assets, generate placeholder sprites for me to replace layer. For now just create sprites of full color. each color represents one thing.

Features will include:

A series of missions played in order.
A science fiction theme.
Two factions: Cowboys and Ninjas.
The missions occur in space, at the start of each mission an animation will play showing the ninja attack space ship flying up next to the cowboy spaceship.
At the start of the round, the player will select locations on the cowbow ship for the ninjas to board. An animation will play showing the ninjas jumping onto the cowboy ship.
When the ninjas pass through the wall of the cowboy ship, an explosion animation plays.
The graphics are sprite based. Create suitable sprites for Ninjas, Cowboys, spaceship interior ground tiles, spaceship interior walls. The background is a starfield.
We will run the game in 1080p resolution, but our sprites will be 64x64. We will scale our graphics to a suitable size.
We will create 1 sample level to start with, ensuring the system is modular so we can easily add more levels.
There will be 4 ninjas on the ninja ship.
The cowboy ship will be a 30x15 grid with some interior walls creating rooms. Also create door sprites.
Randomly add 10 cowboys in the cowboy ship.
We will need sprites to show dead cowboys and ninjas lying on the ground with blood.
Our objective is to steal the vaccine that is positioned randomly inside a room on the cowboy ship. Create a sprite to represent the vaccine, it should be a scifi container of green glowing fluid.

Tactics battle mechanics as follows:

Ensure units are modular so we can easily make variations in future. Ensure attributes are exported so they can easily be changed in the editor.

UI elements during a tactical battle:
Turn number to keep track of how long we have been playing.
Stats next to each unit.
End turn button (create a dialogue asking if you are sure in the case that any ninjas have remaining AP, or have not selected Ambush or Conceal).


NINJA/PLAYER TURN

Ninjas have the following attributes:
12 HP
10 AP
3 Shuriken

We are starting with an empty godot 4 project. We are creating a 2D turn based tactics game.

Features will include:

A series of missions played in order.
A science fiction theme.
Two factions: Cowboys and Ninjas.
The missions occur in space, at the start of each mission an animation will play showing the ninja attack space ship flying up next to the cowboy spaceship.
The graphics are sprite based. Create suitable sprites for Ninjas, Cowboys, spaceship interior ground tiles, spaceship interior walls. The background is a starfield.
We will run the game in 1080p resolution, but our sprites will be 64x64. We will scale our graphics to a suitable size.
We will create 1 sample level to start with, ensuring the system is modular so we can easily add more levels.
There will be 4 ninjas on the ninja ship.
The cowboy ship will be a 30x15 grid with some interior walls creating rooms. Also create door sprites.
Randomly add 10 cowboys in the cowboy ship.
We will need sprites to show dead cowboys and ninjas lying on the ground with blood.
Our objective is to steal the vaccine that is positioned randomly inside a room on the cowboy ship. Create a sprite to represent the vaccine, it should be a scifi container of green glowing fluid.

Tactics battle mechanics as follows:

Ensure units are modular so we can easily make variations in future. Ensure attributes are exported so they can easily be changed in the editor.

UI elements during a tactical battle:
Turn number to keep track of how long we have been playing.
Stats next to each unit.
End turn button (create a dialogue asking if you are sure in the case that any ninjas have remaining AP, or have not selected Ambush or Conceal).

GAME START:

At the start of the round, the player will select locations on the cowbow ship for the ninjas to board. An animation will play showing the ninjas jumping onto the cowboy ship.
When the ninjas pass through the wall of the cowboy ship, an explosion animation plays. These original tiles will be marked, as these are the tiles the vaccine needs to be brought to for the player to win.

NINJA/PLAYER TURN

Ninjas have the following attributes:
12 HP
10 AP
3 Shuriken

A ninja is selected by clicking on them. When a ninja is selected, the cursor changes to a flashing square and we draw a dotted line between the ninja and the square our cursor is hovering over.  When we click on a new square, the following happens:

Empty space: Move to the location (smoothly animate the ninja pathing to the location, use a suitable pathfinding algorithm (A* or better). We can move diagonally. Walls block movement.  We can move through Cowboys or Ninjas, but we cannot end our movement on them. Each square moved costs 1 AP. When moving our cursor around to choose what to do, change the flashing square to red to indicate we can't reach that location, and don't allow us to make that move if we don't have enough AP.

Cowboy: If the cowboy is within 3 squares, open a small dialogue box asking if we want to SHURIKEN or KATANA. If we select shuriken, show an animation of a of a shuriken being thrown at the cowboy. Chance to hit is 80%, -5% per square or distance. Throwing a shuriken costs 3 AP. If we select KATANA, move the ninja next to the cowboy, then attack with the KATANA. Attacking with KATAN costs 2AP + 1AP per square the ninjas was moved. Chance to hit is 80%.  KATANA deals 3-5 damage, with even random chance. SHURIKEN deals 2-4 damage with even random chance. If the cowboy is further than 3 squares away and we have line of sight (not blocked by wall or another cowboy or ninja), then we throw a SHURIKEN and cannot attack with KATANA. Each time we throw a Shuriken we use one from the ninjas attributes and cannot use them anymore once we have 0.

If a ninja moves on top of the vaccine, the ninja will pick it up, and the vaccine icon will be shown next to the ninja. If the ninja carrying the vaccine moves back to any of the boarding squares where the ninjas entered the cowboy spaceship a victory screen is displayed. If the ninja dies the vaccine drops on the floor in the same tile as the dead ninja.  The vaccine sprite must be displayed on top of the dead ninja.

The player may also click on the ninja again to open a new dialogue with the following options:
Ambush
Conceal

Ambush requires 5AP.
Conceal requires 3AP.

These actions do things on the cowboys turn. Once a ninja has selected one of these actions, it cannot do any further actions that turn.  All mini dialogues should have a cancel option. Ninjas HP, AP and SHURIKEN count should all be displayed in small text next to the ninja. They are given as both current and total, eg 5/10HP. You can use a SHURIKEN graphic to represent shurikens.


Cowboys have the following attributes:
12 HP
10 AP

If a cowboy can see a ninja they will shoot at it. Shooting uses 4AP. Shooting has a 80% chance to hit, -10% per square of distance. If the ninja is adjacent to the Cowboy the hit chance is only 50%. If a cowboy cannot see a ninja they will pick a square to walk to, using 1 AP per square moved. If they see a Ninja they will stop and shoot if they have enough AP remaining, if not they will keep talking.

If a Ninja is in Ambush and a Cowboy moves within 4 Squares of the Ninja and in line of site, the Cowboy stops and the ninja gets to attack. The ninjas is moved next to the cowboy and attacks with KATANA, hit chance 80% dealing 3-5 damage. After this is resolved the cowboy can resume their turn, attacking or continueing their move if they are still alive and have sufficient AP.

If a Ninja is in Conceal, and a Cowboy moves within line of sight, there is an 85% chance the cowboy does not see the ninja.  This is checked every square the cowboy moves while the ninja is in line of sight. If the check is failed and the cowboy sees the ninja, the conceal is cancelled and the cowboy will attack if it has sufficient remaining AP. If one cowboy reveals the ninja, the conceal is cancelled and any subsequent cowboys moving into line of sight will see the ninja as normal.

If all the ninjas are killed on the cowboy turn a game over screen is shown, with the option to restart the test level.

We will implement new weapons and abilities for ninjas and cowboys in future so ensure they are modular.

When the game is launched we will show the menu screen.  This will have a start game button to launch our test level.  Please create a cool background picture to use at a suitable resolution of a science fiction ninja with a katana fighting a science fiction cowboy shooting a handgun on board a spaceship. There is a container with a BIOHAZARD symbol in the background. There is a window showing space in the background. The ninja is wearing goggles, a face mask, scarf and typical black ninja costume. The cowboy has cybernet eyes, a cowboy hat, vest, and other typical cowboy costume features.

The game will be called SPACE NINJA: TACTICS.

Keep your architecture modular
Ask for clarification if required
Create suitable animations and effects for the attacks and actions.