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

- This is all client-host based (so a player must host the server on their machine), for a dedicated server, the switch is really easy and depending on how you write your code, really simple. You can read more about that:
https://docs-multiplayer.unity3d.com/tools/current/porting-to-dgs/ and watch this video to see in practice: https://www.youtube.com/watch?v=C5MphV8b7jw&ab_channel=BobsiTutorials

### **Table of Contents**

- [Introduction](#introduction)
- [How and What to Test](#how-and-what-to-test)
  - [Create and Join a Session](#create-and-join-a-session)
  - [Game Rules](#game-rules)
  - [Controls](#controls)
  - [Additional Information](#additional-information)
- [What I Learned](#what-i-learned)
- [What I Did and Instructions](#what-i-did-and-instructions)
  - [Scripts](#scripts)
  - [Methodology and Understanding of Unity NetCode](#methodology-and-understanding-of-unity-netcode)
  - [Instructions to the Sandbox Creation from Scratch](#instructions-to-the-sandbox-creation-from-scratch)
- [Conclusion](#conclusion)
  - [Future Improvements](#future-improvements)
  - [Sources](#sources)
  - [License](#license)
  - [Visuals](#visuals)

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
+ **NetworkObject** - the fundamental building block in Unity's NetCode. It represents any GameObject that needs to be synchronized across the network. In this case it is the player and the ball, as other things like walls, furniture, nature etc. are static and do not need synchronization (that excludes anything the game might interact with like opening/closing doors, moving chairs, destroying walls etc.).
+ **NetworkBehaviour** - specialized version of ```MonoBehaviour``` that includes networking capabilities. Used in scripts that need to interact with the network. For example Ball.cs, HostUIManager.cs, ThrowBallManager.cs etc.
+ **NetworkManager** - responsible for managing network sessions, handling connections, and coordinating the overall networking process. Luckily, Unity creates this one for you, so all you need to know is that you can access it as a gameObject on the scene and all of the server settings can be managed from there.
+ **IsClient, IsOwner, IsHost, IsServer** - the thing that you need to understand about these is that they are very important to be set correctly. In my case with the ball, I didn't do it the best way possible, making it possible for hackers to manipulate a lot on the server side, but giving the context it was all right for me. Usually you don't want to trigger server side of things through the client. 
1) ```**if(IsServer)**``` - will be true only if the server triggers this (false for client and for host).
2) ```**if(IsHost)**``` - will be true only if the client who hosts the server triggers this (false for server and for host).
3) ```**if(IsClient)**``` - will be true only if a client triggers this (false for server, but for the host this is true).
4) ```**if(IsOwner)**``` - will be true only if the client that triggered this owns the game object that has this scripted attached to (false for server, but can be true for client and host, depending if they own the object, otherwise its false).
5) When you combine ```if (IsClient && IsOwner)``` it will be only true if the one who owns that character executes it, but you are making sure that the server for some reason doesn't execute it (but that's for a bit more advanced stuff, however, it's good to have it in practice).

+ **ServerRpc** - allows clients to send requests or data to the server, those methods require the naming to end with: *ServerRpc. You can see some examples of them in the ThrowBallManager.cs.
+ **ClientRpc** - allows the server to send messages or data to all clients. You usually want to use this template: Client4 does something -> notifies the server through ServerRpc that is in his own IsOwner script -> that ServerRpc notifies the ClientRpc method which updates to all Client1, Client2, Client3, Client4.
+ **NetworkVariable** - allows for the synchronization of variables across the network. Changes made to a NetworkVariable on the server are automatically propagated to all clients: ```NetworkVariable<int> Health = new NetworkVariable<int>(100);```, and to access the value you need to access it like: ```Health.Value```. See examples in the BallThrowManager.cs, PlayerHud.cs, Ball.cs.
+ **Relay** - Relay services facilitate connections between players, especially those behind NATs or firewalls, without the need for manual port forwarding. They act as intermediaries to route traffic between clients and the server. This is not an actual servers, it is just way safer to use it that way and this creates somewhat of lobbies between multiple hosts.
+ **NetworkObjectPool** - manages the pooling of ```NetworkObject``` instances to optimize performance by reusing objects instead of instantiating and destroying them repeatedly, just like a normal object pool but multiplayer.
+ **Network Transform && Client Network Transform** - the **NetworkTransform** component is used to synchronize the position, rotation, and scale of a ```NetworkObject``` across the network. **Client Network Transform** is the same, the only difference is that the **Network Transform** has server authority, while the second one has client authority, usually used on player prefabs or objects that the player directly controls. 
+ **Network Animator** - synchronizes animation states across the network, ensuring that all clients see the same animations for a ```NetworkObject```, requires reference to the actual animator.
+ **Network Rigidbody** - synchronizes the physics state of a Rigidbody across the network, ensuring that all clients experience consistent physics interactions. Requires actual rigidbody.
+ **Network Prefab List** - collection of prefabs that the ```NetworkManager``` can spawn over the network. It ensures that all clients recognize and can instantiate the necessary prefabs. You can reference it there as required.

### **Instructions to the Sandbox Creation from Scratch**
The sandbox that's located at the main branch is really easy to make and here is step-by-step guide on how to recreate it. The ball throw game was created from this sandbox and it was a learning experience for me, so if you want to recreate that, make sure to do it in your own way, so you can actually learn Unity NetCode, or even better - make your own game in your own sandbox!

Instructions:
( coming soon :/ )

# **Conclusion**
### **Future Improvements**
As much as I would like to say that this sandbox is perfect -- it's not. Unity has a better way to integrate the first part of the instructions, which is basically setting up the creation/joining of a server, but I couldn't get a second client to join from the same PC. I also didn't have time to create a better game with actual sounds, animations, and mechanics, but I will definitely do so in the future. You get all this information that took me many days to gather and perfect for free, so make sure to add those game design visuals into your game!

### **Sources**
- https://docs.unity3d.com/6000.0/Documentation/Manual/multiplayer.html - Unity Docs about Multiplayer.
- https://www.youtube.com/watch?v=3jBOTk_qozA&ab_channel=Unity - Distributed Authority for client-hosted multiplayer games | Unite 2024
- https://www.youtube.com/watch?v=fRJlb4t_TXc&ab_channel=Rootbin - Unity Relay
- https://www.youtube.com/watch?v=mgIDwkaVAqw&t=6s&ab_channel=Unity - Unity Relay
- https://www.youtube.com/watch?v=d1FpS5hYlVE&list=PLQMQNmwN3FvyyeI1-bDcBPmZiSaDMbFTi&ab_channel=DilmerValecillos - A series of videos, creating similar to my game and the same as my sandbox. Unity actually refers to him - https://docs-multiplayer.unity3d.com/netcode/current/community-contributions/dilmer/
- https://docs-multiplayer.unity3d.com/tools/current/porting-to-dgs/ - Can read more about how to switch from Client-Based to a Dedicated Server
- https://docs-multiplayer.unity3d.com/netcode/current/learn/bossroom/bossroom/#direct-download - Unity game set with NetCode
- https://www.youtube.com/watch?v=rgJGkYM2_6E&t=415s&ab_channel=ForlornU - This video for the projectile and projectile prediction

### **License**
This project is licensed under the [MIT License](LICENSE).

### **Visuals***
...

### **Conclusion**
In conclusion, I think this has been a really good learning experience for me, and I believe that if you take the time to create a game and apply all of this, then you will certainly understand multiplayer better than you did before that. My game didn't turn out to be great, but my multiplayer environment works, and I found a few mistakes in the code and setup, which is a truly valuable learning experience.
