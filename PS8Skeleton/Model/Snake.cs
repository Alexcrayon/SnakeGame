using SnakeGame;
using Newtonsoft.Json;
namespace Model
{

    [JsonObject(MemberSerialization.OptIn)]
    /// <summary>
    /// The snake is a collection of vectors as well as a length.
    /// A new vector is made when it turns, and removed if it longer than the snake.  This is called a joint
    /// </summary>
    public class Snake
    {
        //Various JSON properties

        //ID of the snake, each ID should be unique
        [JsonProperty]
        public int snake;

        //Name of the snake
        [JsonProperty]
        public string name = "";

        //Collection of points that represent the places the snake has turned at.  The first point is the tail and the last point is the head.
        [JsonProperty]
        public List<Vector2D> body = new();

        //Direction of the snake.  Unused as of now.
        [JsonProperty]
        public Vector2D dir = new Vector2D(1,0);

        //Score of the snake
        [JsonProperty]
        public int score;

        //State of the snake. Is it dead?
        [JsonProperty]
        public bool died;

        //State of the snake. Is it alive?
        [JsonProperty]
        public bool alive;

        //Did the snake disconnect?
        [JsonProperty]
        public bool dc;

        //Did the snake join?  Active for only one frame.
        [JsonProperty]
        public bool join;

        public double length;

        public int respawnTimer;

        public Snake() 
        {
            
        }

        public Snake(int ID, string pname, Vector2D position) 
        {
            length = 100;
            snake = ID;
            name = pname;
            score = 0;
            died = false;
            alive = true;
            dc = false;
            join = true;

            Vector2D dir = RandomDirection();

            body.Add(position);
            body.Add(position);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="s"></param>
        public Snake(Snake s) 
        {
            length = s.length;
            snake = s.snake;
            name = s.name;
            score = s.score;
            died = s.died;
            alive = s.alive;
            dc = s.dc;
            join = s.join;
            dir = s.dir;
            body = s.body;
        }

        private Vector2D RandomDirection() 
        {
            Random rand = new Random();
            int randNum = rand.Next(4);

            if (randNum == 0)
                return new Vector2D(0, 1);
            else if (randNum == 1)
                return new Vector2D(1, 0);
            else if (randNum == 2)
                return new Vector2D(-1, 0);
            else
                return new Vector2D(0, -1);

        }

        
    }
}