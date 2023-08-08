using Newtonsoft.Json;
using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// A wall is 2 points and a unique ID.  
    /// A wall must be vertically or horizontally alligned, so the points must either share an X value or share a Y value.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [DataContract(Namespace = "")]
    public class Wall
    {
        //JSON properties
        //Unique ID of the wall
        [JsonProperty (PropertyName = "wall")]
        [DataMember (Name = "ID")]
        public int wall;

        //First point of the wall
        [JsonProperty]
        [DataMember]
        public Vector2D p1;

        //Second point of the wall
        [JsonProperty]
        [DataMember]
        public Vector2D p2;
    }
}
