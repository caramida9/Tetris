This is my Tetris project made in Unity.

The project was built from a 2D starting point and it has 3 scenes: Game Menu, Level(where the actual game is player) and Game Over.

The project contains all the basic tetris shapes and one custom shape in the form of a cross. 
The shapes are called tetraminos and they are built from smaller individual minos that a square shape with 1 unit length.
Each tetramino is saved as a prefab in a resources folder. 
They have an audio source that will be used to play the different sound effects and a Tetromino Script that is used to control all of it's behaviour.
Another game object is the grid which is also made of minos and it has a size of 10 width and 20 height.

The Game Menu and Game Over scenes only contain a simply canvas with some text and UI buttons, along with game object that contains the menu script.
The Game Menu contains a slider that can be used to select the starting level, from 0 to 9 and a play button.
The Game Over contains a retry button and a return to menu button.

The Level Scene also contains a Canvas that has information about the current score, level and total number of lines cleared.
It contains the grid and a game object, that controls the general game script.

The game script starts by resetting the score, setting the level at the selected level and spawning the first tetrominos.
The tetrominos are selected randomly from the Resource Folder. When the game starts the first Tetromino is spawned at a fixed position above the grid.
Then the preview tetromino is also spawned to the left side and disabled. When the next spawn function will be called, the next tetromino will take the
preview game object in order to spawn.
The game script also controls the UI elements, the difficulty through the fall speed, and the process through which the lines are cleared.
Whenever a tetromino lands, the game script will update the grid matrix to contain the new minos. Then it will check if the current row is full.
It will delete that row and move all the rows above it down by one unit. Those functions are called from the tetromino class.

The tetromino scripts control the way each tetromino behaves. It has controls for moving left, right, down and for rotation, using the arrow keys.
For each movement it checks if that movement is allowed by checking if each mino inside the tetromino is still inside the grid, otherwise it will cancel the movement.
For rotation additional conditions are used, as certain pieces don't require rotation.
And finally for falling down, apart from using the player input, the game checks how much time has passed and automatically moves the piece down by one unit
at a specific interval(which starts at 1 second and gets shorter with each level).
When a piece can no longer move down, it means it has reached the bottom of the grid, at which points checks are made to see if we need to clear row.
If the piece landed above the grid that means the game is over, if not, then the piece is disabled and the next one is spawned.

For player comfort we added adition functionality if the player made a simple tap(in which case it will move one unit) or if the player is holding down the key, 
in which case the piece will start moving faster in the desired direction after a small delay.
For touch screen input we check where the player has initially tapped the screen and then depending on the direction in which he dragged we will move the piece.
If it was a simple tap then we will just rotate.
All the graphics and sound resources are taken from online sources.