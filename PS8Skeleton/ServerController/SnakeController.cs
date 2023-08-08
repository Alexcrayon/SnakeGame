using Model;
using ServerController;
using System.Reflection;

namespace SnakeGame
{
    public class SnakeController
    {
        /// <summary>
        /// Changes the snake's body position based on direction.  Moves snake at 3 pixels per update usually.
        /// </summary>
        public void Move(Snake s, bool SurvivalMode)
        {
            double moveSpeed = 3;

            double addedLength = moveSpeed;
            s.body[s.body.Count - 1] = s.body[s.body.Count - 1] + (s.dir * moveSpeed);
            //Calculate total length of snake
            double totalLength = 0;
            for (int i = 0; i < s.body.Count - 1; i++) 
            {
                totalLength += Math.Abs((s.body[i] - s.body[i + 1]).Length());
            }

            //Move last segment by three towards the other point.  Any unsused movement must removed from the next segment.  Ignore in survival mode.  Also ignore if the snake hasn't fully expanded yet.
            if (!SurvivalMode && s.length < totalLength)
            {
                double movementLeft = moveSpeed;
                while (movementLeft > 0.0)
                {
                    if (Math.Abs((s.body[0] - s.body[1]).Length()) < movementLeft)
                    {
                        movementLeft -= Math.Abs((s.body[0] - s.body[1]).Length());
                        s.body.RemoveAt(0);
                    }
                    else
                    {
                        //Length of tail is greater than the movement left.  Remove that extra movement from the tail.
                        Vector2D tailDir = s.body[1] - s.body[0];
                        tailDir.Normalize();
                        s.body[0] = s.body[0] + (tailDir * movementLeft);
                        break;
                    }
                }
            }

        }

        public void ChangeDirection(Snake s, Vector2D newDir)
        {
            double SecSegmentLength = 0; 
            if (s.body.Count >= 2)
                //Second segment is current "first" segment as new turn segment hasn't been invented yet.
                SecSegmentLength = Math.Abs((s.body[s.body.Count - 1] - s.body[s.body.Count - 2]).Length());


            //If the second segment is < 10 unit in distance AND if it not a U-Turn (Third and First segment have different direction), DO NOT TURN

            Vector2D segment3Dir = (s.body[s.body.Count - 1] - s.body[s.body.Count - 2]);
            segment3Dir.Normalize();

            if (SecSegmentLength < 10 && !segment3Dir.Equals(newDir))
                return;

            //Check to make sure the turn is a 90 degree turn
            if (Math.Abs(Vector2D.AngleBetweenPoints(s.dir, newDir)) % 90 != 0)
            {
                s.body.Add(new Vector2D(s.body[s.body.Count - 1]));
                s.dir = newDir;
            }
           

        }

        public void TestCollision(Snake s, CollisionController cc, ServerWorld world)
        {
            if (!s.alive)
                return;

            object? collision = cc.CollidesWith(s.body.Last(), 5, s.snake);
            if (collision == null)
                return;

            if (!(collision is Powerup))
            {
                //Hit a wall or snake, die
                s.died = true;
                s.alive = false;
            }
            else
            {
                //Hit powerup.  Remove the powerup, then add score to snake.
                Powerup powerup = (Powerup)collision;
                s.length += 50;
                s.score++;

                powerup.died = true;
            }
        }

        public void SetSnakeRespawnTimer(Snake s, int time)
        {
            s.respawnTimer = time;
        }

        public Boolean CheckTimer(Snake s)
        {
            s.respawnTimer--;

            if (s.respawnTimer <= 0)
                return true;

            return false;
        }
    }
}