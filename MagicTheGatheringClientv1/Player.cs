using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicTheGatheringClientv1
{
    class Player
    {
        public Card[] deck;
        public Card[] graveyard;
        public Card[] exile;
        public Card[] hand;
        public Card[] permanents;
        public Mana[] manapool;
        public int life;
        public bool priority;
        public bool active;
        public Player(bool whichDeck)
        {
            //load deck here
        }


    }
}
