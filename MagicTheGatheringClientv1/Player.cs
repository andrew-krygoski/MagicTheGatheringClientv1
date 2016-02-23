using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
        int human;

        public Player(bool whichDeck, int human)
        {
            this.human = human;
            this.life = 20;
            this.deck = new Card[60];
            if (whichDeck)
                makeDeck("duel-decks-zendikar-vs-eldrazi-eldrazi-1.txt");
            else
                makeDeck("duel-decks-zendikar-vs-eldrazi-zendikar-1.txt");

        }

        private void makeDeck(string deckName)
        {
            using (StreamReader reader = new StreamReader("AllCards.json"))
            {
                Newtonsoft.Json.Linq.JObject cards = Newtonsoft.Json.Linq.JObject.Parse(reader.ReadLine());
                using (StreamReader decklist = new StreamReader(deckName))
                {
                    string line;
                    int i = 0;
                    while ((line = decklist.ReadLine()) != null)
                    {
                        string[] thing = line.Split(' ');
                        int numCards = int.Parse(thing[0]);
                        string name = "";
                        for (int j = 1; j < thing.Length; j++)
                        {
                            if (j != 1)
                                name += " ";
                            name += thing[j];
                        }
                        for (int l = 0; l < numCards; l++) { this.deck[i] = new Card(human, name, cards); }
                        i++;
                    }
                }
            }
        }


    }
}
