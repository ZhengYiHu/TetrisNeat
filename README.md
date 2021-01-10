# Tetris Neat

Tetris Neat is the implementation of NEAT in a Tetris Game using Unity.

## Run Executable
To run the executable file, open the folder **Tetris Neat Executable** and run **Tetris Neat.exe**.

Executable file is for windows only.

If other operative systems are used, export the executable from the project files.
Instructions below:

## Build executable from Unity
The project is made in Unity, so to access the project file is necessary to install Unity first.


## Installation
The software is available on the [main website](https://unity3d.com/get-unity/download) of Unity: 

Once installed, open the project folder file named **Tetris Neat**.

## Build
To build an executable file of the project:
1. **File > Build Settings**
2. Configure the settings according to the desired path and OS


Note: The executable file build from the project will not contain trained data to load.
Trained data must be saved first in the [persistent data path](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html) locally using the "save generation" button on the UI.

To test externally trained data (for example import **GameData.json** in the project), it's necessary to drag **GameData.json** inside the persistent data path folder, but the executable file submitted is already loaded with that to have a quick comparison between a not trained AI vs a trained one.

## Usage
The program will automatically run from the first generation, no user input is needed to start the simulation.

On the left, 200 Tetris AIs are displayed playing the game, while on the right a render of the best genome from the previous generation is showed.
The black and white Tetris board represents the binary matrix that is seen by the AI, while the nodes and connections represent the current state of the brain of the fittest genome as a neural network.
### Save/Load generation

Two buttons are available for the users to save and to load the generation. 
**Save Generation** will save the current state of the training session into a .json file saved in the persistent data path, while **Load Generation** will load the currently saved data. Saving data will always overwrite existing data from the folder since only one save slot is available.

