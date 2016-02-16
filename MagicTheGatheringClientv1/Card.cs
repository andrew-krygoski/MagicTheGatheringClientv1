using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicTheGatheringClientv1
{
    class Card
    {
        public Side[] sides;

    }

    class Side
    {
        public string name;
        public Mana[] manacost;
        public char[] supertypes;
        public char[] types;
        public string text;
        public int power;
        public int toughness;
        public int controller;
        public int owner;
        public char[] color;
        public Counter[] counters;
    }

}
