using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{   
    /// <summary>
    /// This class is used to animate explosions for snakes. Since this is entirely client side, there is no reason to make this a JSON object
    /// It contains a position and a frame count.
    /// </summary>
    public class Explosion
    {
        //Position of the explosion
        public Vector2D pos;
        //Frame of the explosion's animation
        public int frame;
        //ID is set on creation.  It shares an ID with the snake that died to create it.
        public readonly int explosion;

        /// <summary>
        /// Creates an explosion at frame 0 at the given position.
        /// </summary>
        /// <param name="_pos"></param>
        public Explosion(Snake snake)
        {
            pos = snake.body[snake.body.Count-1];
            explosion = snake.snake;
            frame = 0;
        }

        /// <summary>
        /// Used so that the frame count can only be incremented by 1. 
        /// Returns the current frame of the animation.
        /// <summary>
        public int NextFrame() 
        {
            frame++;
            return frame;
        }

    }
}
