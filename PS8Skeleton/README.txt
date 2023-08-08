
--------------------------------------------------------------------------------------------------------
To play the game:
	1. Run Server.exe in PS8Skeleton\Server\bin\Debug\net6.0
	2. Build the SnakeGame solution in the Visual Studio and run the Snake Client
	3. Click connect, wasd to control the snake

--------------------------------------------------------------------------------------------------------
PS7 - NetworkController
--------------------------------------------------------------------------------------------------------
PS8 - Snake Game Client part1
--------------------------------------------------------------------------------------------------------

MVC Model
	View is found in the SnakeClient project.
	Controller is found in the GameController project.
	Model is found in the Model project.
	
	MainPage in the view has a reference to the GameController, and passes that controller to the WorldPanel.
	Controller is responsible for holding the current world model, player ID, player name, and server-side functionality.
	Controller sends updates, such as errors occuring or a new world needing to be created, to the MainPage through events.

	The World is the Model and holds all information about the model, including a list of all current objects.  Each object holds it's own values such as alive or dead, but the world is responsible for accessing these values.
	The Controller updates the world as needed, and also sends messages to the server if the snake moves.
	The View uses the information in the world and uses it to draw.

	Only the Controller has direct access to the world. The view uses a GetWorld() function to get the world from the Controller.


Additional features

	Explosion
	----------------------------
	An explosion animation will be played upon snake's death.
	This is handled and drawn in the WorldPanel of SnakeClient.
	Each frame of the animation is loaded as IImage and will be drawn one frame at a time.
	ExplosionDrawer check snake's alive field to decide when to draw animation and it will only draw once after snake's death.
	
	Credit: Explosion pixel art by Sogomn
	https://opengameart.org/content/explosion-3 

	At first the explosion was an add-on to the snake's drawer, but that created issues when two or more snakes would die at the same time.
	To fix this, the controller now adds an explosion object to the world when a snake has "died" as true.  This makes it so each explosion is
	an individual object with it's own location and frame count.

--------------------------------------------------------------------------------------------------------
PS9 - Snake Game Client part2
--------------------------------------------------------------------------------------------------------
Create a GameSettings class to hold information from the settings.XML file.
ServerWorld added to the Model Class.
World renamed to ClientWorld to avoid confusion with the new ServerWorld in model.  They both do similar things, but the client should not be able to access the server's logic, so a seperation is required.
Origionally we had all the snake logic in the snake part of Model, but that makes no MVC sense so we moved all the snake controll logic to a new project called ServerController.  So now SnakeServer will update, and communicate to 
the ServerController to update the model.
Moved GameSettings to ServerController.
Added a CollisionController class that handles the collision math.
SnakerServer.cs is being used to keep one uniform of controllers and worlds.

For the snake, we origionally had the length visible right at the start, but we think it's cooler to see it "grow out" at the beggining.

Added a "speed boost" powerup, which is red instead (I think this is our only change to client).  Instead of adding points, they make you double speed for whatever time was put in the settings.  Iff this time is 0, these powerups won't spawn.

ISSUE WITH ADDING A NEW POWERUP
The new "type" value for a powerup must be shared using a JSON so the View knows to draw it differently with access to the server.
Unfortunately, adding this new "type" seems to make the given client not work.  At all.  We tried assigning it a value, we tried making an empty default constructor.  Nothing worked.
Couldn't think of another way to implement the Speed Boost powerup without violating MVC.  Clients will just have to add "type" to their model to work.

Additional Feature
	
	Speed Boost
	In the xml, there should be a setting called SpeedBoostTimer.  This is an int that determines how long the speed boost lasts in frames.  Set this to 0 or less to turn off speed boost powerups completely.

Current Bugs
	
	There is this strange bug in which the snake has a section of it's body turn into a square.  We know it does this because one of the points in it's body vanishes, causing the drawer to draw a square ellipse.
	However, we don't know WHY this point is vanishing.  This happens EXTREMELY rarely.  We've seen it 4 times only throughout playtesting the entire week, and can't recreate it.

*REMOVING THE SPEED BOOST

Added Survival Mode
	It was a cool idea, couldn't make it work. Instead, the server will have a survival mode in which snakes have an infinite length, and no food spawns.
	For this feature, set SurvivalMode to true in the XML.  The score is how many seconds you stayed alive without crashing.

Reduced Respawn Time
	Losing all progress is punishment enough.  Halved so you can see how you died, but accelerated so you aren't just sitting there.

After clients disconnected, snakes will still a little longer on screen before they were removed.
