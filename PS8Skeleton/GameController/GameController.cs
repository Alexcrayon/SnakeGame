using System.Text.RegularExpressions;
using NetworkUtil;
using Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace SnakeGame
{
    /// <summary>
    /// Manages logic when the server sends information
    /// </summary>
    public class GameController
    {
        //Delegate used various for events
        public delegate void GameHandler();
        //Event for when an Update Arrives
        public event GameHandler? UpdateArrived;
        //Event for when the world gets created.
        public event GameHandler? CreateWorld;
        //Event for handling connection errors.
        public delegate void ErrorHandler(string? err);
        public event ErrorHandler? Error;

        //The client's player name
        private string playerName = "";
        //A world containing needed information.  Stays null until a world is generated.
        private ClientWorld? theWorld;
        //The state of the server.  Remains null until the state is set.
        SocketState? theServer;
        //Unique ID of the player.
        private int playerID;
        //Determine if a world exists or not.
        private bool worldMade = false;

        /// <summary>
        /// Returns the current world the controller is using.  Returns null if no world has been created yet.
        /// </summary>
        /// <returns></returns>
        public ClientWorld? GetWorld()
        {
            return theWorld;
        }

        /// <summary>
        /// Returns true iff a world has been created, false otherwise.
        /// </summary>
        /// <returns></returns>
        public bool HasWorld() 
        {
            return worldMade;
        }

        /// <summary>
        /// Closes the current world. To be used in the event of a communication error.
        /// </summary>
        public void CloseWorld() 
        {
            worldMade = false;
        }

        /// <summary>
        /// Returns player ID that the controller is using.
        /// </summary>
        /// <returns></returns>
        public int GetPlayerID()
        {
            return playerID;
        }

        /// <summary>
        /// Handles connection to a server
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="player"></param>
        public void Connect(string addr, string player)
        {
            Networking.ConnectToServer(OnConnect, addr, 11000);
            playerName = player;
        }

        /// <summary>
        /// Used when connecting.
        /// </summary>
        /// <param name="state"></param>
        private void OnConnect(SocketState state)
        {
            //Show user error occurred if one occurs
            if (state.ErrorOccurred)
            {
                //At this point, error should not be null, as the main page has subscribed to all events.
                Error?.Invoke(state.ErrorMessage);
                return;
                
            }

            theServer = state;

            //Send player's name to server
            Networking.Send(state.TheSocket, playerName + "\n");
            
            //Start event loop to receive Update from server
            state.OnNetworkAction = ReceiveMessage;
            Networking.GetData(state);
        }

        private void ReceiveMessage(SocketState state)
        {
            //Show user error occurred if one occurs
            if (state.ErrorOccurred)
            {
                //At this point, error should not be null, as the main page has subscribed to all events.
                Error?.Invoke(state.ErrorMessage);
                return;
            }

            ProcessMessage(state);
            Networking.GetData(state);
        }

        /// <summary>
        /// Modifies the world based on recieved information from the server.
        /// </summary>
        /// <param name="state"></param>
        private void ProcessMessage(SocketState state)
        {
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            //If no world has been created yet, that means this is recieving for the first time, and a world must be created from the first two lines.
            if (!worldMade)
            {
                //Create a new world based on the given size if a world does not exist yet.
                int.TryParse(parts[0], out int id);
                int.TryParse(parts[1], out int size);
                theWorld = new ClientWorld(size);
                playerID = id;

                CreateWorld?.Invoke();
                worldMade = true;

                //Remove the first 2 parts, ID and Size of World from consideration of the next part.
                state.RemoveData(0, parts[0].Length);
                state.RemoveData(0, parts[1].Length);
                parts[0] = "";
                parts[1] = "";
            }

            //Add neccisairy components to the world.
            foreach (string part in parts)
            {
                if (part.Length == 0)
                    continue;
                if (part[part.Length - 1] != '\n')
                    break;

                //ADD PART TO WORLD
                JObject obj = JObject.Parse(part);
                JToken ?token = obj["wall"];
                
                //Lock used to prevent adding to the world and drawing from the world from happening at the same time.
                lock (theWorld!)
                {

                    if (token != null)
                    {
                        Wall? wall = JsonConvert.DeserializeObject<Wall>(part);
                        theWorld.SetWall(wall!);
                    }

                    token = obj["snake"];
                    if (token != null)
                    {
                        Snake ?snake = JsonConvert.DeserializeObject<Snake>(part);
                        theWorld.SetSnake(snake!);
                        //Create an explosion if the snake has died.
                        if (snake!.died) 
                        {
                            theWorld.SetExplosion(new Explosion(snake!));
                        }
                    }

                    token = obj["power"];
                    if (token != null)
                    {
                        Powerup ?powerup = JsonConvert.DeserializeObject<Powerup>(part);
                        theWorld.SetPowerup(powerup!);
                    }


                    state.RemoveData(0, part.Length);
                }
            }

            UpdateArrived?.Invoke();
        }

        /// <summary>
        /// Sends the specified messege to the server.  Used for telling the server to move the snake up/down/left/right.
        /// </summary>
        /// <param name="message"></param>
        public void inputEntered(string message)
        {
            if(theServer is not null)
                Networking.Send(theServer.TheSocket, message);
        }

    }
}