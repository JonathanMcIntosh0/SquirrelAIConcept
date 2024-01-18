# Release
To play current build just download zip file from [latest release](https://github.com/JonathanMcIntosh0/SquirrelAIConcept/releases/latest) 
(no need for source code). Unzip and run SquirrelAIConcept.exe. 

# Controls

- WASD or ARROW KEYS to move
- SHIFT to toggle sprint
- SPACE to toggle "ghosting" (Squirrels unable to see you)
- ESC (or ALT + F4) to quit

# Description

This is a game concept with Squirrels using Goal Oriented Action Planning (GOAP) based AI can be observed and interacted with by the player.

## World
The world mainly consists of trees, garbage cans and nuts.
- Each squirrel gets assigned a unique Home Tree (where they are spawned).
- Each tree continuously spawns nuts in intervals until it reaches a maximum capacity.
- Each garbage can is either empty (grey) or full (black) and flips between states after a delay.

## Actions
Squirrels can do the following actions:
- Pick up nut
- Investigate garbage can: If can is full then add garbage to inventory. If can is empty then squirrel gets trapped for a while.
- Store inventory: If at home tree, it can empty its inventory.
- Hide: Climb up tree and wait

Exceptions:
- Squirrels have a max inventory capacity for garbage and nuts.
- Also they can only carry one type of food at a time (either nuts or garbage).
- Only one Squirrel can climb up a tree or garbage can at a time. 

## Vision and memory

Each squirrel keeps track of its own world state and possible targets which is then used for planning.
Targets can either be in memory or currently in sight.
When a target moves out of sight, it gets stored in memory.
A squirrel, by default, can store in memory the location of 1 tree, 2 garbage cans and 5 nuts.

**Note**: Squirrels can only see if a tree or garbage can is occupied while in vision. Additionally, we can keep track of locations of nuts in memory that may have been picked up by another squirrel while not in vision.

## Goals
Squirrels have the following goals/behaviours:
- **Explore**: Move a certain distance away from current location.
- **Wander**: Move a certain total distance.
- **Stock Food**: Gather nuts or garbage and bring back to home tree to store.
- **Flee**: When player gets near, flee to a tree and wait for player to leave.

