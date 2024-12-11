# Unity Multiplayer Project
This project explores implementing a multiplayer experience in Unity using Unity’s Multiplayer Center and Netcode for GameObjects. The goal was to create a game environment where multiple players can join, interact, and see real-time updates to each other’s positions, states, and animations.

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


# What I Did
Initial UI Creation:
I started with creating the UI elements necessary for player login, role selection, and basic network operations (buttons for starting as server/host/client). Once the UI was in place, I linked it to the networking logic so players could join matches through a simple, intuitive interface.

Real-Time Synchronization of Animation, Position, and Rotation:
After implementing the basic multiplayer connection logic, I focused on syncing player positions, rotations, and animations in real-time. This ensured that when one player moved or changed animation states, all other players saw those changes smoothly and consistently.

#Future Improvements
Further refine interpolation and prediction to reduce latency-induced jitter.
Implement more complex game logic on the server, such as scoring and power-ups.
Integrate a more robust relay and lobby system for easier matchmaking.
This README outlines what I learned during this multiplayer project and the steps I took to implement these systems. As the project evolves, I will continue to update this documentation.
