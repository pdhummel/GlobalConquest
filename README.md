# Global Conquest 2025

Global Conquest 2025 is a redo of the multi-player DOS game released in 1992.  
* https://en.wikipedia.org/wiki/Global_Conquest  
* https://archive.org/details/globalconquestmanual/GlobalConquest-Manual/

The original was a follow-up to the game Command HQ, https://en.wikipedia.org/wiki/Command_HQ, and also has its roots in the computer game Empire, https://en.wikipedia.org/wiki/Empire_(1977_video_game).

Global Conquest is essentially a 4x game - "explore, expand, exploit, and exterminate".  
The original Global Conquest was unique in its game play, as it provided a modified real-time experience. 
With turn based games, one player makes a move and then resolves their actions. Then the next player, in turn, does the same. Game state is only modified one player at a time. Without any timer constraints, players can take as much time as they like to make their best possible move. I.e., consider chess as an example.
With real time games, players take actions simultaneously and those actions are resolved in real time. There are rewards for thinking fast and having quick and efficient interactions with the user interface. This potentially means some players can get more done than their opponents if they are more dexterious.
The Global Conquest hybrid approach allows for players to simultaneously plan their actions, but then action resolution happens separately without further player interaction during an execution phase. Different execution trigger configurations allow the game to more closely resemble a turn-based game or even approach a real-time game experience.
The planning and execution phase system make it similar to tactical combat games like Baulder's Gate 3 or XCom, but abstracted to a strategic level.

## Project Goals and Designed Deviations
To recreate the hybrid, modified real-time experience of Global Conquest, so that is playable on modern computers over the internet.

The game is being designed with known differences from the original. 
* The use of a hex-based map instead of a square-based grid.
* Unknown areas appear differently from sea tiles. The original conflated sea tiles and the unknown.
* Will be playable on a modern operating system over the internet.

## Roadmap
### Milestone 1
* Combat with a limited number of unit types and a fixed number of units per side.
* King of the Hill or Elimination victory condition.
* Simple execution trigger.

### Milestone 2
* Add Burbs to map.
* Ability to purchase and produce units.
* Burb management screen.
* Add support for all land and sea units.
  * Transports with load and unload.
  * Infantry dig-in.
  * Submarine visibility handling.
  * Spies.
* Ship bombardment.

### Milestone 3
* Advanced movement -- waypoints.
* Destinations screen.
* Add unit repair logic.
* Add unit attrition logic.
* Unit context menu - blitz, sneak, pursue, etc.

### Milestone 4
* Airplanes


### Milestone 5
* AI opponents
* Add natives
* Add more victory conditions.
  * Add number of turns game setting.
  * Calculate victory points.
* Add support for all execution triggers. (timers, etc.)

### Milestone 6
* Host Game and Join Game setting validations.
* Save and load game.
* Resign.
* Playback.

### Milestone 7
* Improved Economics.
* Unit production by city.
* Unit supported by city.
* Add oil and mineral resources.

### Milestone 8
* Treaties

### Milestone 9
* Events

### Milestone 10
* UI improvements.
* Network robustness.
* Steam integration
* Multi-platform support



## Technical Notes
The game is being developed on the DotNet framework and leverages the game library, MonoGame, https://monogame.net/. Furthermore Myra, https://github.com/rds1983/Myra, is used to create a Windows Forms like experience.

### Map Generation
Much of the Hex Map generation code was borrowed from blackfalconsoftware:
* https://www.codeproject.com/articles/Hexagonal-grid-for-games-and-other-projects-Part-1
* https://blackfalconsoftware.com/
* https://blackfalconsoftware.wordpress.com/
* https://blackfalconsoftware.wordpress.com/2017/12/12/hexagonal-maps-part-v-designing-contiguous-hexagons/
* https://blackfalconsoftware.wordpress.com/2025/03/24/the-military-simulation-workbench-msworkbench/
* https://blackfalconsoftware.wordpress.com/2016/08/22/part-i-creating-a-digital-hexagonal-tile-map/
* https://blackfalconsoftware.wordpress.com/2017/05/10/part-ii-using-the-mouse-to-scroll-a-hexagonal-tile-map/
* https://blackfalconsoftware.wordpress.com/2017/06/27/hexagonal-maps-part-iii-selecting-a-tilehexagon/
* https://blackfalconsoftware.wordpress.com/2017/07/05/hexagonal-maps-part-iv-highlighting-a-selected-a-tilehexagon/

In addition, the idea to use a noise algorithm to create differing terrains, which create cohesive land masses:
https://www.redblobgames.com/maps/terrain-from-noise/.

### Build and Execute
* dotnet build
* dotnet run  

Or maybe more easily, download the binary zip package to a 64 bit Windows machine and run `GlobalConquest.exe`.

### Personal Notes
I am mostly a java enterprise developer by trade -- distributed programming, REST APIs, cloud tools. Python is also one of my favorite productive ways to get stuff done too.

So DotNet and CSharp are not my current strengths, and so you may find that my CSharp code smells like java. 
