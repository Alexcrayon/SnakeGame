using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SnakeGame
{
    /// <summary>
    /// Class represents a container for various game settings that can be created with an XML file.
    /// </summary>
    [DataContract (Namespace="")]
    public class GameSettings
    {
        //Frames in a shot
        [DataMember]
        public readonly int FramesPerShot;
        //Milliseconds per frame
        [DataMember]
        public readonly int MSPerFrame;
        //Respawn rate, in frames
        [DataMember]
        public readonly int RespawnRate;
        //Determines if suvival mode is on or off.
        [DataMember]
        public readonly bool SurvivalMode;
        //Size of the world.  It is a square.
        [DataMember]
        public readonly int UniverseSize;
        //A list of the walls for the world to contain.
        [DataMember]
        public readonly List<Wall> Walls = new();
    }
}
