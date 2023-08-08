using Model;
using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ServerController
{
    /// <summary>
    /// Handle collisions detection between objects for server
    /// </summary>
    public class CollisionController
    {
        private ServerWorld world;

        public CollisionController(ServerWorld world)
        {
            this.world = world;
        }

        /// <summary>
        /// Check a snake's position against everthing else in the world
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="radius"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public object? CollidesWith(Vector2D pos, int radius, int ID)
        {
            foreach (Wall wall in world.Walls.Values)
                if (CollidesWall(pos, wall, radius))
                    return wall;

            foreach (Powerup pwup in world.Powerups.Values)
                if (CollidesPowerup(pos, pwup, radius))
                    return pwup;

            foreach (Snake snake in world.Snakes.Values)
                if (CollidesSnake(pos, snake, ID, radius))
                    return snake;

            return null;
        }

        /// <summary>
        /// Return true if given position collides with a wall, else return false
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="w"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private bool CollidesWall(Vector2D pos, Wall w, int radius)
        {
            if (WithinBoundsOfLine(pos, w.p1, w.p2, 25 + radius))
                return true;

            return false;
        }

        /// <summary>
        /// Return true if given position collides with a powerup, else return false
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="p"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private bool CollidesPowerup(Vector2D pos, Powerup p, int radius)
        {
            if ((pos - p.loc).Length() <= 10 + radius)
                return true;

            return false;
        }

        /// <summary>
        /// Return true if given position collides with a snake(self or other snake), else return false
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="s"></param>
        /// <param name="ID"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private bool CollidesSnake(Vector2D pos, Snake s, int ID, int radius)
        {
            //check self-collision
            if (s.body.Count > 3 && s.snake == ID)
            {
                int segmentIndex = s.body.Count - 3;

                //search backward and check collision for every body segment until tail
                for (int i = segmentIndex - 1; i > 0; i--)
                {
                    if (WithinBoundsOfLine(pos, s.body[i], s.body[i - 1], 10))
                    {
                        return true;
                    }
                }
            }

            //check collision with other snake
            else if (s.snake != ID)
            {
                for (int i = 0; i < s.body.Count - 1; i++)
                {
                    if (WithinBoundsOfLine(pos, s.body[i], s.body[i + 1], 5 + radius))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Helper method for checking collision with snake's body segments
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private bool WithinBoundsOfLine(Vector2D pos, Vector2D p1, Vector2D p2, int radius)
        {
            bool withinX = (p1.X - radius < pos.X && pos.X < p2.X + radius) || (p1.X + radius > pos.X && pos.X > p2.X - radius);
            bool withinY = (p1.Y - radius < pos.Y && pos.Y < p2.Y + radius) || (p1.Y + radius > pos.Y && pos.Y > p2.Y - radius);
            return withinX && withinY;
        }


    }
}
