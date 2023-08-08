//Author: Lucas Jones & Alex Cao

using NetworkUtil;
using Model;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Xml;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Diagnostics;
using ServerController;
using System.IO.Pipes;


namespace SnakeGame
{
    /// <summary>
    /// This class is the server for the snake game.
    /// It handles the handshake with incoming clients, 
    /// also Keep update the state of the world and send it back to all client.
    /// </summary>
    public class SnakeServer
    {
        // A map of clients that are connected, each with an ID
        private Dictionary<long, SocketState> clients;
        private GameSettings settings;
        private ServerWorld world;
        private SnakeController scontrol;
        private int powerupsSpawned;
        private CollisionController cc;
        private int frames = 0;

        /// <summary>
        /// Main thread that run a infinite loop to send update to all the clients
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            SnakeServer server = new();
            server.StartupServer();

            //Add this wall to the server's world for collision handling.
            foreach (Wall wall in server.settings.Walls)
                server.world.AddWall(wall);

            //Begin sending a frame every message
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (true)
            {
                while (watch.ElapsedMilliseconds < server.settings.MSPerFrame)
                { /* Do nothing */; }
                watch.Restart();
                lock (server.world)
                    server.Update();
            }
        }
        /// <summary>
        /// Contructor for the snake server
        /// load the setting.xml file
        /// </summary>
        public SnakeServer()
        {
            clients = new();
            world = new();
            cc = new CollisionController(world);
            scontrol = new();
            try
            {
                settings = CreateFromXML("settings.xml")!;
                Console.WriteLine("SETTINGS FOUND: " + (settings != null));
                Console.WriteLine("Frames Per Shot: " + settings!.FramesPerShot);
                Console.WriteLine("MS Per Frame: " + settings!.MSPerFrame);
                Console.WriteLine("Universe Size: " + settings!.UniverseSize);
                Console.WriteLine("Respawn Rate: " + settings!.RespawnRate);
                Console.WriteLine("Survival Mode On: " + settings!.SurvivalMode);
                Console.WriteLine("Walls In World: " + settings!.Walls.Count());
            }
            catch
            {
                settings = new();
                Console.WriteLine("Failed to create settings.  settings.xml not found");
            }
        }

        /// <summary>
        /// A helper method for reading a xml file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static GameSettings? CreateFromXML(string fileName)
        {
            DataContractSerializer ser = new(typeof(GameSettings));
            XmlReader reader = XmlReader.Create(fileName);
            Object settings = ser.ReadObject(reader)!;
            return (GameSettings)settings;
        }

        /// <summary>
        /// Start the server and start accepting clients
        /// </summary>
        private void StartupServer()
        {
            Networking.StartServer(NewClientConnected, 11000);
            Console.WriteLine("Server now running...");
        }

        /// <summary>
        /// When a client connected, start the handshake, 
        /// send startup info(ID, World size, walls) back to the client.
        /// </summary>
        /// <param name="state"></param>
        private void NewClientConnected(SocketState state)
        {
            if (state.ErrorOccurred)
                return;


            Console.WriteLine("Client Connected.  ID " + state.ID);

            state.OnNetworkAction = ReceiveMessage;

            //Send ID, world size, and all walls in that order!
            lock (clients) 
                clients.Add(state.ID, state);

            Networking.Send(state.TheSocket, state.ID + "\n" + settings.UniverseSize + "\n");

            foreach (Wall wall in settings.Walls)
            {
                Networking.Send(state.TheSocket, JsonConvert.SerializeObject(wall) + "\n");
            }

            Networking.GetData(state);
        }

        /// <summary>
        /// Receive data from clients
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveMessage(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                RemoveClient(state.ID);
                return;
            }

            ProcessMessage(state);
            Networking.GetData(state);
        }
        
        /// <summary>
        /// Remove a client with a given id
        /// </summary>
        /// <param name="id"></param>
        private void RemoveClient(long id)
        {
            lock (clients)
            {
                clients.Remove(id);
            }
        }

        /// <summary>
        /// Receive player name from client, complete the handshake
        /// Create a snake for player with id and name.
        /// Continue to receive moving command from client.
        /// </summary>
        /// <param name="state"></param>
        private void ProcessMessage(SocketState state)
        {
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            foreach (string p in parts)
            {
                if (p.Length == 0)
                    continue;
                if (p[p.Length - 1] != '\n')
                    break;

                //Check moving command to decide snake's change of direction
                if (p == "{\"moving\":\"none\"}\n")
                {
                    /* do nothing */
                }
                else if (p == "{\"moving\":\"up\"}\n")
                {
                    if (world.Snakes.ContainsKey((int)state.ID))
                        scontrol.ChangeDirection(world.Snakes[(int)state.ID], new Vector2D(0, -1));
                }
                else if (p == "{\"moving\":\"down\"}\n")
                {
                    if (world.Snakes.ContainsKey((int)state.ID))
                        scontrol.ChangeDirection(world.Snakes[(int)state.ID], new Vector2D(0, 1));
                }
                else if (p == "{\"moving\":\"left\"}\n")
                {
                    if (world.Snakes.ContainsKey((int)state.ID))
                        scontrol.ChangeDirection(world.Snakes[(int)state.ID], new Vector2D(-1, 0));
                }
                else if (p == "{\"moving\":\"right\"}\n")
                {
                    if (world.Snakes.ContainsKey((int)state.ID))
                        scontrol.ChangeDirection(world.Snakes[(int)state.ID], new Vector2D(1, 0));
                }
                
                //No movement commands. Add the player's snake to this world
                else
                    lock (world)
                        world.SetSnake((int)state.ID, p.Trim(), ValidRandomPosition());

                lock (state)
                    state.RemoveData(0, p.Length);

                //Disconnect clients need to be removed.
                lock (clients)
                {
                    foreach (SocketState client in clients.Values)
                    {
                        if (state.ErrorOccurred)
                        {
                            RemoveClient(state.ID);
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update the state of the world every frame
        /// </summary>
        private void Update()
        {
            frames++;
            frames %= 60;

            //Spawn a powerup!
            if (world.Powerups.Count < 30 && world.Walls.Count > 0 && !settings.SurvivalMode)
            {
                Vector2D position = ValidRandomPosition();
                world.AddPowerup(powerupsSpawned, position);
                powerupsSpawned++;
            }

            //Update everything in the world.  THIS ORDER MATTERS
            //If pwrups come before snakes, they'll be marked then removed before having the "death" be sent to clients.
            foreach (Powerup p in world.Powerups.Values)
            {
                //Check if the powerup has been marked for removal.
                if (p.died)
                    world.Powerups.Remove(p.power);
            }
            foreach (Snake s in world.Snakes.Values)
            {
                if (settings.SurvivalMode && frames == 0)
                    s.score++;

                //Remove snake if dead
                if (s.died)
                {
                    world.Snakes.Remove(s.snake);
                    world.DeadSnakes.Add(s.snake, s);
                    scontrol.SetSnakeRespawnTimer(s, settings.RespawnRate);
                }
                //Update snake, if it dies now it will still be seen for a frame.
                scontrol.Move(s, settings.SurvivalMode);
                scontrol.TestCollision(s, cc, world);
            }
            //Check all dead snakes to see if they respawned.  If they have, return them to the timer.
            foreach (Snake s in world.DeadSnakes.Values)
            {
                if (scontrol.CheckTimer(s))
                {
                    world.DeadSnakes.Remove(s.snake);
                    world.SetSnake(s.snake, s.name, ValidRandomPosition());
                }
            }

            //Send everything to the clients
            lock (clients)
            {
                //Mark snakes as disconnected
                foreach (int ID in world.Snakes.Keys) 
                {
                    if (!clients.ContainsKey(ID))
                        world.Snakes[ID].dc = true;
                }

                //Send all information to all connected clients
                foreach (SocketState ss in clients.Values)
                {
                    foreach (Snake s in world.Snakes.Values)
                    {
                        //Make a copy of s as to not interrupt
                        Snake s2 = new Snake(s);
                        lock (ss)
                            Networking.Send(ss.TheSocket, JsonConvert.SerializeObject(s2) + "\n");
                        if (s.dc)
                        {
                            world.Snakes.Remove(s.snake);
                            Console.WriteLine("Snake " + s.snake + " disconnected.");
                        }
                    }

                    foreach (Powerup p in world.Powerups.Values)
                        Networking.Send(ss.TheSocket, JsonConvert.SerializeObject(p) + "\n");
                }                    
            }
        }

        /// <summary>
        /// Return a random postion that is not colliding with other object in the world
        /// </summary>
        /// <returns></returns>
        private Vector2D ValidRandomPosition()
        {
            Vector2D randpos;
            Random random = new Random();
            do
            {
                float randX = random.Next(settings.UniverseSize) - settings.UniverseSize / 2;
                float randY = random.Next(settings.UniverseSize) - settings.UniverseSize / 2;
                randpos = new Vector2D(randX, randY);
            }
            while (cc.CollidesWith(randpos, 50, -1) != null);

            return randpos;
        }
    }
}