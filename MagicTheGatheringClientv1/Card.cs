using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MagicTheGatheringClientv1
{
    class Card
    {
        private Side[] sides;
        private Counter[] counters;
        private int controller;
        private int owner;
        private string[] names;

        //public Card()
        //{
        //    this.sides = new Side[0];
        //    this.counters = new Counter[0];
        //    this.controller = 0;
        //    this.owner = 0;
        //    this.names = new string[0];
        //}

        //public Card(int owner)
        //{
        //    this.sides = new Side[0];
        //    this.counters = new Counter[0];
        //    this.controller = owner;
        //    this.owner = owner;
        //    this.names = new string[0];
        //}

        public Card(int owner, string name, Newtonsoft.Json.Linq.JObject cards)
        {
            this.counters = new Counter[0];
            this.controller = owner;
            this.owner = owner;
            this.names = ((string)cards[name]["names"] == null) ? new string[1] : new string[2]; //assuming there are no 3 sided monstrosities
            for (int i = 0; i < this.names.Length; i++) { this.names[i] = (this.names.Length == 1) ? (string)cards[name]["name"] : (string)cards[name]["names"][i]; }
            this.sides = new Side[this.names.Length];
            for (int i = 0; i < this.sides.Length; i++)
            {
                int theColorcount = (cards[name]["colors"] == null) ? 0 : cards[name]["colors"].Count();
                int theSuperTypescount = (cards[name]["supertypes"] == null) ? 0 : cards[name]["supertypes"].Count();
                int theTypescount = (cards[name]["types"] == null) ? 0 : cards[name]["types"].Count();
                int theSubtypescount = (cards[name]["subtypes"] == null) ? 0 : cards[name]["subtypes"].Count();
                string[] tmpColors = null, tmpSupers = null, tmpTypes = null, tmpSubtypes = null;
                int tmpcmc = 0;
                string tmpMana = null, tmpPower = null, tmpText = null, tmptoughness = null;

                if (theColorcount != 0)
                {
                    tmpColors = new string[theColorcount];
                    for (int j = 0; j < theColorcount; j++) { tmpColors[j] = (string)cards[name]["colors"][j]; }
                }
                if (theSuperTypescount != 0)
                {
                    tmpSupers = new string[theSuperTypescount];
                    for (int j = 0; j < theSuperTypescount; j++) { tmpSupers[j] = (string)cards[name]["supertypes"][j]; }
                }
                if (theTypescount != 0)
                {
                    tmpTypes = new string[theTypescount];
                    for (int j = 0; j < theTypescount; j++) { tmpTypes[j] = (string)cards[name]["types"][j]; }
                }
                if (theSubtypescount != 0)
                {
                    tmpSubtypes = new string[theSubtypescount];
                    for (int j = 0; j < theSubtypescount; j++) { tmpSubtypes[j] = (string)cards[name]["subtypes"][j]; }
                }
                if (cards[name]["cmc"] != null)
                {
                    tmpcmc = (int)cards[name]["cmc"];
                }
                if (cards[name]["manaCost"] != null)
                {
                    tmpMana = (string)cards[name]["manaCost"];
                }
                if (cards[name]["power"] != null)
                {
                    tmpPower = (string)cards[name]["power"];
                }
                if (cards[name]["text"] != null)
                {
                    tmpText = (string)cards[name]["text"];
                }
                if (cards[name]["toughness"] != null)
                {
                    tmptoughness = (string)cards[name]["toughness"];
                }

                this.sides[i] = new Side
                {
                    cmc = (int)tmpcmc,
                    colors = tmpColors,
                    manaCost = tmpMana,
                    name = this.names[i],
                    power = tmpPower,
                    subtypes = tmpSubtypes,
                    supertypes = tmpSupers,
                    text = tmpText,
                    toughness = tmptoughness,
                    types = tmpTypes
                };
            }
        }
    }

    class Side
    {
        //public string artist { get; set; }
        public int cmc { get; set; }
        //public string[] colorIdentity { get; set; }
        public string[] colors { get; set; }
        //public Foreignname[] foreignNames { get; set; }
        //public string id { get; set; }
        //public string imageName { get; set; }
        //public string layout { get; set; }
        //public Legality[] legalities { get; set; }
        public string manaCost { get; set; }
        //public int multiverseid { get; set; }
        public string name { get; set; }
        //public string[] names { get; set; }
        //public string number { get; set; }
        //public string originalText { get; set; }
        //public string originalType { get; set; }
        public string power { get; set; }
        //public string[] printings { get; set; }
        //public string rarity { get; set; }
        public string[] subtypes { get; set; }
        public string[] supertypes { get; set; }
        public string text { get; set; }
        public string toughness { get; set; }
        //public string type { get; set; }
        public string[] types { get; set; }
    }

    public class Foreignname
    {
        public string language { get; set; }
        public string name { get; set; }
        public int multiverseid { get; set; }
    }

    public class Legality
    {
        public string format { get; set; }
        public string legality { get; set; }
    }

}
