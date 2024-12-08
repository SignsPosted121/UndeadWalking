# About This Project
This was a 2D project I worked on in my earlier years of programming, probably between 2019-2021. The game was a top down strategy fighting game against zombies.

# Highlights
* Tilemap: This was the first time I used tilemap, and I made several different isometric angles of textures for the map.
* Inventory: There was a simple grid-based inventory system that stored the player's items, and the gun they used would use bullets from their inventory.
* Crafting/Dismantling: While the crafting was never implemented into the UI, objects around the map could be distmantled for parts to gather.
* Stealth: All of enemies used a sight and sound based stealth system. They would wander aimlessly, until they would hear a noise. They would then move towards the noise to investigate. However, if the player moved into their eyesight, they would beeline for the player.
* AI Navigation Map: The navigation map style I chose for this project is called A star. Essentially it assigns nodes (in this case, the points on the tilemap) a cost, and measures multiple paths by how much closer they are to the destination node, their cost of movement, and the final cost (cumulative of all nodes required to move through) to arrive at the destination node. Inside the project is my old code, but I ended up using some foreign algorithm for testing purposes.

# Insight On Drawbacks and Issues
* AI Navigation: The code for the A star pathfinding I wrote worked in most cases, but had a serious issue when needing to calculate a path that required the AI to move away from the target to move closer to them. I just never found a way to calculate the cost of a path requiring movement away from the target while balancing the pruning I did along paths to save on calculation time.
* Graphics: The graphics were not the best I've made, I had some practice at this point but I took a bold choice trying to design 2d characters from a front persepective, which requires hours if not days of work just to find a nice design, let alone all of the animations I added on top of this work.

# Final Comments
I learned a lot of different aspects of game design here, but overall I wish I had spent less time focusing on the graphics of my game and instead more on the stealth system and AI navigation. Had I done so I would probably have a better idea of how to approach stealth in my current project better. Overall this was very fun, and the information I gained from the inventory system was instrumental to future projects.