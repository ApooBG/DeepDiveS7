# Introduction to Deep Dive S7
I dived deep into **Multiplayer with NetCode for GameObjects in Unity**. In this GitHub repository, you will find three branches, including this one:

+ **main** - A multiplayer sandbox that allows you to create or join a session. It synchronizes players' movement, rotation, and animation of the playerPrefab.
+ **BallThrowGame** - Building upon the main branch, I extended it into creating a ball-throwing game, which is also my full deep dive.
+ **build** - Use this branch to get the build of the BallThrower game.

# How and What to Test:
### Create and Join a Session:

In order to see the results of my Deep Dive project, you have to pull from branch build. Since it is a multiplayer game, best case scenario is to get someone to do that as well on their laptop, otherwise you can just open two times the ```BallChaserDeepDive.exe``` file. Choose one of the opened games to be the host and add enter your nickname, then click on "Start Host". On the bottom left you will see two buttons - Start Game and Console and on the middle-top of the screen you will find the code for your lobby. Anyone who wants to connect to your game must enter their nickname and that join code and click on Start Client. When everybody has joined, you can click on Start Game.

### Controls & Game Rules:
After the game has started, a **chaser** is randomly chosen, while everybody else is a **runner**. Each second somebody is a runner, they get -1 point deduction, the leaderboard can be seen on the right. To become a runner, the chaser has to hit somebody with a ball, after which they switch places. Chasers cannot run and cannot walk while aiming. Each game is 60 seconds, in the end you can click on Start Game to restart it. The player with most points wins.

WASD / Arrows - movement
Holding Left Shift - running
Holding Left Mouse Button - aiming
Wheel up and down - changing the force of the throw
Right Mouse Button - cancelling a throw while aiming
Releasing Left Mouse Button - throwing a ball

### Additional Information:
This entire game was created solely to apply the multiplayer concepts I have learned. Therefore, game design choices can be overlooked, as the main goal was to create a functional multiplayer environment. You might not like the player prefab, the 60-second game duration, or the throwing mechanic, but my focus was on implementing multiplayer functionality rather than developing a polished game.

# What I Learned
1. Unity Multiplayer Center (Unity 6):
I explored the Unity Multiplayer Center to understand how to integrate networking services directly from Unity’s ecosystem. This included setting up services, understanding the Relay system for NAT traversal, and configuring the project so that it could run smoothly over the network.

2. Netcode for GameObjects (Unity’s Networking Layer):
I learned how to utilize Unity’s Netcode for GameObjects (NGO) to manage networked GameObjects.

IsClient, IsServer, IsOwner: I understood the importance of these checks when writing code that behaves differently based on context. For instance:
IsServer: Running authoritative logic, spawning objects, and making game-wide decisions.
IsClient: Handling local input, animations, and UI updates.
IsOwner: Applying logic that should only affect the player who owns a particular object (e.g., player movement or inventory changes).

3. Object Pooling in Multiplayer:
I learned how to pool objects so they could be efficiently reused. Ensuring pooled objects are synchronized correctly between the server and all clients was crucial. This reduced network overhead and improved performance.

4. Physics in Multiplayer:
I discovered how to handle physics operations across the network.

6. Relay Setup:
I learned how to configure and use Relay services, allowing players behind NAT or firewalls to connect without complicated port forwarding. This ensured a seamless multiplayer experience regardless of the players’ network environments.

7. Multiplayer Animation Setup:
Synchronizing animations across the network was a key challenge. I learned:

How to trigger animation states on the server and have those states replicated to all clients.
How to properly handle transitions (e.g., aiming, walking, running) so that every player sees a consistent visual state for all characters.


# What I Did and Instructions
Initial UI Creation:
I started with creating the UI elements necessary for player login, role selection, and basic network operations (buttons for starting as server/host/client). Once the UI was in place, I linked it to the networking logic so players could join matches through a simple, intuitive interface.

Real-Time Synchronization of Animation, Position, and Rotation:
After implementing the basic multiplayer connection logic, I focused on syncing player positions, rotations, and animations in real-time. This ensured that when one player moved or changed animation states, all other players saw those changes smoothly and consistently.

#Future Improvements
Further refine interpolation and prediction to reduce latency-induced jitter.
Implement more complex game logic on the server, such as scoring and power-ups.
Integrate a more robust relay and lobby system for easier matchmaking.
This README outlines what I learned during this multiplayer project and the steps I took to implement these systems. As the project evolves, I will continue to update this documentation.

# Conclusion & Sources
