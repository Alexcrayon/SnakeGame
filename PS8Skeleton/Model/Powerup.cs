using Newtonsoft.Json;
using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// A powerup is a point in space with a unique ID that can either be alive or dead.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Powerup
    {
        //Various JSON properties.

        //Specific ID given to the powerup, each powerup should have a unique ID.
        [JsonProperty]
        public int power;

        //Location of Powerup.
        [JsonProperty]
        public Vector2D loc;

        //State of the Powerup.
        [JsonProperty]
        public bool died;

        public Powerup(int ID, Vector2D _loc) 
        {
            loc = _loc;
            power = ID;
        }
    }
}
