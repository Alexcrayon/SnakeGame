using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class ServerWorld
    {
        /// <summary>
        /// Holds all snakes currently alive in the game
        /// </summary>
        public Dictionary<int, Snake> Snakes = new();

        /// <summary>
        /// Holds all snakes currently dead in the game, for respawning.
        /// </summary>
        public Dictionary<int, Snake> DeadSnakes = new();

        /// <summary>
        /// Holds all Powerups in the game
        /// </summary>
        public Dictionary<int, Powerup> Powerups = new();

        /// <summary>
        /// Holds all Walls in the game
        /// </summary>
        public Dictionary<int, Wall> Walls = new();

        /// <summary>
        /// Adds a snake to the Snakes dictionary
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="name"></param>
        /// <param name="pos"></param>
        public void SetSnake(int ID, string name, Vector2D pos) 
        {
            Snakes[ID] = new Snake(ID, name, pos);
        }

        /// <summary>
        /// Adds a wall to the Walls dictionary
        /// </summary>
        /// <param name="wall"></param>
        public void AddWall(Wall wall) 
        {
            Walls[wall.wall] = wall;
        }

        /// <summary>
        /// Adds a powerup to the Powerups dictionary
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="pos"></param>
        public void AddPowerup(int ID, Vector2D pos) 
        {
            Powerup p = new Powerup(ID, pos);
            Powerups[p.power] = p;
        }
    }
}
