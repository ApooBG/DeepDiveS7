# Introduction to Deep Dive S7
### **Introduction**
I dived deep into **Multiplayer with NetCode for GameObjects in Unity**. In this GitHub repository, you will find three branches, including this one:

+ **main** - A multiplayer sandbox that allows you to create or join a session. It synchronizes players' movement, rotation, and animation of the `playerPrefab`.
+ **BallThrowGame** - Building upon the main branch, I extended it into creating a ball-throwing game, which is also my full deep dive.
+ **build** - Use this branch to get the build of the BallThrower game.

In this README file, you will find:

- How to use the build to test the results.
- What I have learned during my Deep Dive.
- A step-by-step guide with explanations on how to implement the playground from scratch located in the **main branch**.
- Information about each script and its functionality.
- A conclusion summarizing the process, along with tips, tricks, and my learning sources.

### **Table of Contents**
...

### **Prerequisites** 
...

### **Installation**
...

# **How and What to Test**
### **Create and Join a Session**

To see the results of my Deep Dive project, you have to pull from the **build** branch. Since it is a multiplayer game, the best scenario is to have someone else run it on their laptop. Otherwise, you can open two instances of the `BallChaserDeepDive.exe` file. Choose one of the opened games to be the host, enter your nickname, and then click on "Start Host". 

On the bottom left, you will see two buttons - **Start Game** and **Console**, and at the middle-top of the screen, you will find the code for your lobby. Anyone who wants to connect to your game must enter their nickname and the join code, then click on **Start Client**. Once everyone has joined, you can click on **Start Game**.

### **Game Rules**
After the game starts, a **chaser** is randomly chosen, while everyone else becomes a **runner**. Each second a player remains a runner, they receive a -1 point deduction. The leaderboard is visible on the right. To become a runner, the chaser must hit someone with a ball, after which they switch places. Chasers cannot run and cannot walk while aiming. Each game lasts 60 seconds. At the end, you can click on **Start Game** to restart. The player with the most points wins.

### **Controls**
- **WASD / Arrows** - Movement
- **Hold Left Shift** - Running
- **Hold Left Mouse Button** - Aiming
- **Scroll Wheel Up/Down** - Change throw force
- **Right Mouse Button** - Cancel throw while aiming
- **Release Left Mouse Button** - Throw a ball

### **Additional Information:**
This entire game was created solely to apply the multiplayer concepts I have learned. Therefore, game design choices can be overlooked, as the main goal was to create a functional multiplayer environment. You might not like the player prefab, the 60-second game duration, or the throwing mechanic, but my focus was on implementing multiplayer functionality rather than developing a polished game.

# **What I Learned**
1. **Unity Multiplayer Center (Unity 6)**:
   I explored the Unity Multiplayer Center to understand how to integrate networking services directly from Unity’s ecosystem. This included setting up services, understanding the Relay system for NAT traversal, and configuring the project so that it could run smoothly over the network.

2. **Netcode for GameObjects (Unity’s Networking Layer)**:
   I learned how to utilize Unity’s Netcode for GameObjects (NGO) to manage networked GameObjects.

   - **IsServer**: Running authoritative logic, spawning objects, and making game-wide decisions.
   - **IsClient**: Handling local input, animations, and UI updates.
   - **IsOwner**: Applying logic that should only affect the player who owns a particular object (e.g., player movement or inventory changes).

3. **Object Pooling in Multiplayer:**
   I learned how to pool objects so they could be efficiently reused. Ensuring pooled objects are synchronized correctly between the server and all clients was crucial. This reduced network overhead and improved performance.

4. **Physics in Multiplayer:**
   I discovered how to handle physics operations across the network.

5. **Relay Setup:**
   I learned how to configure and use Relay services, allowing players behind NAT or firewalls to connect without complicated port forwarding. This ensured a seamless multiplayer experience regardless of the players’ network environments.

6. **Multiplayer Animation Setup:**
   Synchronizing animations across the network was a key challenge. I learned:
   
   - How to trigger animation states on the server and have those states replicated to all clients.
   - How to properly handle transitions (e.g., aiming, walking, running) so that every player sees a consistent visual state for all characters.

7. **UI & Cinemachine:**
   I learned how to manage client UI, player UI, and host UI, and how to influence each one of them. I also learned how to set up a Cinemachine camera for the player that follows each client.

# **What I Did and Instructions**

### **Scripts**
+ **CanvasFaceCamera.cs** - Attached to the player canvas, this script finds the `PlayerCameraFollow` script and makes the canvas always face the camera.
+ **Logger.cs** - Attached to the logger GameObject. This script logs game states, such as when someone joins the session. The Logger's instance is called in almost every script.
+ **PlayerCameraFollow.cs** - Attached to the camera component, this script uses Cinemachine to follow the player.
+ **PlayerControl.cs** - Attached to the player prefab GameObject. This script captures the player's information and syncs it with the server, including position, rotation, animation, and other necessary data.
+ **PlayerHud.cs** - Attached to the UI component of the player. This script stores the username chosen by the player and updates the textbox across all clients.
+ **PlayersManager.cs** - Attached to a GameObject called `PlayersManager`. This script manages the number of players and provides access to the player prefab based on their ID.
+ **UIManager.cs** - Attached to the UI Canvas for all players. This script allows starting a host or joining a game with a set username and relay code.
+ **RelayManager.cs** - Attached to a GameObject called `RelayManager`. This script is sourced from the Unity documentation about Relay.
+ **SpawnerControl.cs** - Attached to a scene GameObject called `NetworkObjectPool`. This script allows the server to initialize a pool of the object prefab and spawn it when needed.
+ **NetworkString.cs** - Sourced from Unity Docs. This script allows storing and retrieving a string in a network variable.
+ **NetworkObjectPool.cs** - Attached to a GameObject called `NetworkObjectPool` and sourced from an open-source project. This script enables object pooling from the server side.
+ **Ball.cs** - Attached to the ball prefab, this script checks with a boolean if the ball has hit a player.
+ **ClientUIManager.cs** - Attached to the client UI. This script references the client UI components.
+ **HostUIManager.cs** - Attached to the host UI. This script references the host UI components.
+ **ProjectileProperties.cs** - A class used to store properties. Not attached to any GameObject.
+ **ProjectileThrow.cs** - Attached to the player's child object used to spawn balls on throw. This script handles ball prediction, changing throw force, and spawning new balls.
+ **ThrowBallManager.cs** - Attached to a GameObject called `ThrowBallManager`. This script manages the entire game logic of the ThrowBall game.
+ **TrajectoryPredictor.cs** - Attached alongside `ProjectileThrow.cs`. This script performs a raycast for ball throw prediction.

### **Methodology and Understanding of Unity NetCode**
...

### **Instructions to the Sandbox Creation from Scratch**
...

# **Conclusion**
### **Future Improvements**
...

### **Sources**
...

### **License**
...

### **Visuals***
...

### **Conclusion**
...
