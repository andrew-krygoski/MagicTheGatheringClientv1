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
        public Guid identifier;
        public List<Card> deck;
        public List<Card> graveyard;
        public List<Card> exile;
        public List<Card> hand;
        public List<Card> permanents;
        public List<Mana> manapool;
        public List<Mana> manaSpending;
        public List<Counter> contEffects;
        public int life;
        public bool priority;
        public int human;
        public Player(bool whichDeck, int human)
        {
            this.life = 20;
            this.contEffects = new List<Counter>();
            this.human = human;
            this.deck = new List<Card>();
            this.hand = new List<Card>();
            this.permanents = new List<Card>();
            this.exile = new List<Card>();
            this.graveyard = new List<Card>();
            this.manapool = new List<Mana>();
            this.manaSpending = new List<Mana>();
            this.priority = false;
            this.identifier = Guid.NewGuid();
            if (whichDeck)
                makeDeck("duel-decks-zendikar-vs-eldrazi-eldrazi-1.txt");
            else
                makeDeck("duel-decks-zendikar-vs-eldrazi-zendikar-1.txt");
        }

        /**/
        /*
        Player::shuffle() Game::Player()
        NAME
                Player::shuffle - shuffles the deck
        DESCRIPTION
                Pops a card out of a random spot in the deck and 
                pushes it to the front, 1000 times. A la hindu shuffle kinda
        RETURNS
                true if the player will have a defend phase, false otherwise
        */
        /**/
        public void shuffle()
        {
            Random rand = new Random();
            //pushes and pops a random spot 1000 times
            for (int i = 0; i < 1000; i++)
            {
                int num = rand.Next(0, this.deck.Count);
                this.deck.Push(this.deck.PopAt(num));
            }
        }

        /**/
        /*
        Player::draw() Player::draw()
        NAME
                Player::draw - draws a card
        SYNOPSIS
                public Player::draw( int num );
                    num          --> number of cards to draw
        DESCRIPTION
                looks through each permanent on the players battlefield and if there is a 
                single creature able to block, it returns true to allow for choosing of blockers
        RETURNS
                true if the player will have a defend phase, false otherwise
        */
        /**/
        public void draw(int num)
        {
            for (int i = 0; i < num; i++)
            {

                Card tmp = this.deck.Pop();
                tmp.changeZones(Zone.Hand);
                this.hand.Push(tmp);

            }
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
                    line = decklist.ReadLine();
                    while (!string.IsNullOrEmpty(line))
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
                        for (int l = 0; l < numCards; l++)
                        {
                            Card tmp = new Card(human, name, cards);
                            tmp.currentZone = Zone.Library;
                            this.deck.Push(tmp);
                        }
                        i++;
                        //dont know why I need to do below, but it works, so don't touch it.
                        line = decklist.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            line = decklist.ReadLine();
                    }
                }
            }
        }
    }

    static class ListExtension
    {
        public static T PopAt<T>(this List<T> list, int i)
        {
            T r = list[i];
            list.RemoveAt(i);
            return r;
        }

        public static T Pop<T>(this List<T> list)
        {
            T r = list[0];
            list.RemoveAt(0);
            return r;
        }

        public static T Push<T>(this List<T> list, T theCard)
        {
            list.Insert(0, theCard);
            return theCard;
        }
    }
}
