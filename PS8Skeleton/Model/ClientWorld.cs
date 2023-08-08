using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// Represents the main world of the snake game, and holds all walls, snakes, and powerups.
    /// The controller recieves instructions from the server on how to modify the world.
    /// The client uses the information in the world to draw.
    /// </summary>
    public class ClientWorld
    {
        /// <summary>
        /// Holds all snakes currently in the game
        /// </summary>
        public Dictionary<int, Snake> Snakes = new();

        /// <summary>
        /// Holds all walls currently in the game
        /// </summary>
        public Dictionary<int, Wall> Walls = new();

        /// <summary>
        /// Holds all Powerups in the game
        /// </summary>
        public Dictionary<int, Powerup> Powerups = new();

        /// <summary>
        /// Holds all Explosions in the game
        /// </summary>
        public Dictionary<int, Explosion> Explosions = new();


        /// <summary>
        /// Size of the world.  The world is a size by size square.
        /// </summary>
        public int Size
        { get; private set; }


        /// <summary>
        /// Basic cunstructor that sets size of the world and makes all Dictionaries empty.
        /// </summary>
        /// <param name="_size"></param>
        public ClientWorld(int _size)
        {
            Snakes = new Dictionary<int, Snake>();
            Powerups = new Dictionary<int, Powerup>();
            Walls = new Dictionary<int, Wall>();
            Explosions = new Dictionary<int, Explosion>();

            Size = _size;
        }

        /// <summary>
        /// Adds a wall to the Dictionary if it isn't null.
        /// </summary>
        /// <param name="wall"></param>
        public void SetWall(Wall wall) 
        {

            Walls[wall.wall] = wall;

        }

        /// <summary>
        /// Adds a snake if it isn't null, and removes it if it disconnected.
        /// </summary>
        /// <param name="snake"></param>
        public void SetSnake(Snake snake)
        {

            if (snake.dc)
            {
                if (Snakes.ContainsKey(snake.snake))
                    Snakes.Remove(snake.snake);
            }
            else
                Snakes[snake.snake] = snake;
            
        }

        /// <summary>
        /// Adds or removes a powerup from the game based on if it's dead or not.
        /// </summary>
        /// <param name="powerup"></param>
        public void SetPowerup(Powerup powerup)
        {

            if (powerup.died)
            {
                if (Powerups.ContainsKey(powerup.power))
                    Powerups.Remove(powerup.power);
            }
            else
                Powerups[powerup.power] = powerup;

        }

        /// <summary>
        /// Adds or removes an Explosion from the game based on its frame count.
        /// </summary>
        /// <param name="powerup"></param>
        public void SetExplosion(Explosion explosion)
        {
            if (explosion.frame != 0)
            {
                if (Powerups.ContainsKey(explosion.explosion))
                    Powerups.Remove(explosion.explosion);
            }
            else
                Explosions[explosion.explosion] = explosion;
        }

    }
    

   
}
