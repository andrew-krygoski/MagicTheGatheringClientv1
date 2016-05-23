using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MagicTheGatheringClientv1
{
    enum Zone { Hand, Graveyard, Battlefield, Library, Exile, Stack };

    class Card
    {
        public Guid instance;
        public Zone currentZone;
        public int side;
        public Side[] sides;
        public List<Counter> counters;
        public int controller;
        public int owner;
        public string[] names;
        public bool tapped;
        public int damageMarked;
        public bool sick;

        /**/
        /*
        Card::Card() Card::Card()
        NAME
                Card::Card - creates a new card
        SYNOPSIS
                public (int owner, string name, Newtonsoft.Json.Linq.JObject cards);
                    owner                --> owner of the card represented by 0 or 1
                    name       --> name of the card
                    cards      --> json object containing all the card text
        DESCRIPTION
               queries the json object to pull out all information about the card.
               Feeds all pertinent information into a Side class. Initilizes all lists and arrays
        RETURNS
                Nothing
        */
        /**/
        public Card(int owner, string name, Newtonsoft.Json.Linq.JObject cards)
        {
            this.instance = Guid.NewGuid();
            this.side = 0;
            this.counters = new List<Counter>();
            this.controller = owner;
            this.owner = owner;
            this.damageMarked = 0;
            this.sick = false;
            this.tapped = false;
            this.names = ((string)cards[name]["names"] == null) ? new string[1] : new string[2]; //assuming there are no 3 sided monstrosities
            for (int i = 0; i < this.names.Length; i++) { this.names[i] = (this.names.Length == 1) ? (string)cards[name]["name"] : (string)cards[name]["names"][i]; }
            this.sides = new Side[this.names.Length];
            for (int i = 0; i < this.sides.Length; i++)
            {
                int theColorcount = (cards[name]["colors"] == null) ? 0 : cards[name]["colors"].Count();
                int theSuperTypescount = (cards[name]["supertypes"] == null) ? 0 : cards[name]["supertypes"].Count();
                int theTypescount = (cards[name]["types"] == null) ? 0 : cards[name]["types"].Count();
                int theSubtypescount = (cards[name]["subtypes"] == null) ? 0 : cards[name]["subtypes"].Count();
                string[] tmpColors = new string[1], tmpSupers = new string[1], tmpTypes = new string[1], tmpSubtypes = new string[1];
                int tmpcmc = 0;
                string tmpMana = null, tmpPower = null, tmpText = null, tmptoughness = null, tmpid = null;

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
                if (cards[name]["id"] != null)
                {
                    tmpid = (string)cards[name]["id"];
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
                    types = tmpTypes,
                    id = tmpid,
                    //optionalPaid = false,
                    optionalCosts = new string[2],
                    additionalCosts = new string[2],
                    keywordAbilities = new List<string>(),
                    effects = new List<Effect>(),
                    manacost = new List<Mana>()
                };
            }
        }

        /**/
        /*
        Card::Card() Card::Card()
        NAME
                Card::Card - creates a new card
        SYNOPSIS
                public (int owner, string name, string power, string toughness, Effect e);
                    owner                --> owner of the card represented by 0 or 1
                    name       --> name of the card
                    power      --> string containing the cards' power
                    toughness  --> string containing the cards' toughness
                    e          --> effect containing any abilities the card has
        DESCRIPTION
               Used only to create tokens, this creates tokens. Same as original otherwise
        RETURNS
                Nothing
        */
        /**/
        public Card(int owner, string name, string power, string toughness, Effect e)
        {
            this.instance = Guid.NewGuid();
            this.side = 0;
            this.counters = new List<Counter>();
            this.controller = owner;
            this.owner = owner;
            this.damageMarked = 0;
            this.sick = false;
            this.tapped = false;
            this.names = new string[1]; 
            this.names[0] = name;
            string[] tmptypes = new string[0];
            string tmptext = "";
            string[] colors = new string[0];
            if(name == "Eldrazi Spawn")
            {
                tmptext = "Sacrifice this creature: Add {1} to your mana pool.";
                tmptypes = new string[2] { "Eldrazi", "Spawn" };
            }
            else if(name == "Plant")
            {
                tmptext = "";
                tmptypes = new string[1] { "Plant" };
                colors = new string[1] { "Green" };
            }
            else
            {
                tmptypes = new string[1] { "Hellion" };
                colors = new string[1] { "Red" };
            }
            this.sides = new Side[2];
            this.sides[0] = new Side
            {
                cmc = (int)0,
                colors = colors,
                manaCost = null,
                name = this.names[0],
                power = power,
                subtypes = tmptypes,
                supertypes = new string[1] { "Token" },
                text = tmptext,
                toughness = toughness,
                types = new string[1] { "Creature" },
                id = null,
                optionalCosts = new string[2],
                additionalCosts = new string[2],
                keywordAbilities = new List<string>(),
                effects = new List<Effect>(),
                manacost = new List<Mana>()
            };
            this.sides[0].effects.Push(e);
        }

        /**/
        /*
        Card::changeZones() Card::changeZones()
        NAME
                Card::changeZones - moves a card into a new zone
        SYNOPSIS
               void changeZones(Zone newZone);
                    newZone   --> the zone the card is moving to
        DESCRIPTION
               Everytime a card changes zones, it becomes a new instance of itself
               thus we need to assign a new guid everytime we move a card
        RETURNS
                Nothing
        */
        /**/
        public void changeZones(Zone newZone)
        {
            this.instance = Guid.NewGuid();
            this.currentZone = newZone;
        }
    }

    class Side
    {
        public string id { get; set; }
        public int cmc { get; set; }
        public string[] colors { get; set; }
        public string manaCost { get; set; }
        public List<Mana> manacost { get; set; }
        public string name { get; set; }
        public string power { get; set; }
        public string[] subtypes { get; set; }
        public string[] supertypes { get; set; }
        public string text { get; set; }
        public string toughness { get; set; }
        public string[] types { get; set; }
        public List<string> keywordAbilities { get; set; }
        public List<Effect> effects { get; set; }
        public string[] additionalCosts { get; set; }
        public bool additionalPaid { get; set; }
        public string[] optionalCosts { get; set; }
        public string[] levelUp { get; set; }
    }

    enum EffectType { EventTrigger, PhaseTrigger, ActivatedAbility, Continous, Replacement, OneShot, LevelUp };
    class Effect
    {
        public EffectType effect { get; set; }
        /*
        [0] == number of targets
        [1] == stipulations
        */
        public string[] targets { get; set; } 
        public bool tapCost { get; set; }
        /*
       [0] == who (me or oppenent) is the controller of the effect (one moving cards)
       [1] == optional? (you may)
       [2] == actionWord (put, search, sacrifice, etc)
       [3] == effected (targets, permanents, creatures, etc)
       [4] == effected stipulation (oppenent controls, flying, etc)
       [5] == effect (buffs till eot, etc)
       [6] == ability name (landfall, annihilator)
       */
        public string[] whatDo { get; set; }
        /*
        [0] == cast, etb, beg, end etc
        [1] == what is cast, etb, etc
        */
        public string[] when { get; set; }
        public bool modal { get; set; }
        public List<Mana> manaCost { get; set; }
        public string[] costs { get; set; }
       
        /**/
        /*
        Effect::Effect() Effect::Effect()
        NAME
                Effect::Effect - initilizes an effect
        DESCRIPTION
               Initilizes all arrays and lists and booleans
        RETURNS
                Nothing
        */
        /**/
        public Effect()
        {
            this.manaCost = new List<Mana>();
            this.when = new string[2];
            this.targets = new string[2];
            this.costs = new string[2];
            this.modal = false;
            this.whatDo = new string[7];
            this.tapCost = false;
        }

        /**/
        /*
        Effect::Effect() Effect::Effect()
        NAME
                Effect::Effect - initilizes an effect
        SYNOPSIS
               void Effect(Effect e);
                    e   --> the effect to be copies
        DESCRIPTION
               Copies all qualities of e into a new effect
        RETURNS
                Nothing
        */
        /**/
        public Effect(Effect E)
        {
            this.effect = E.effect;
            this.modal = (E.modal) ? true : false;
            this.manaCost = new List<Mana>(E.manaCost);
            this.when = (string[])E.when.Clone();
            this.targets = (string[])E.targets.Clone();
            this.costs = (string[])E.costs.Clone();
            this.whatDo = (string[])(E.whatDo.Clone());
            this.tapCost = (E.tapCost) ? true : false;
        }

        /**/
        /*
        Effect::spawnify() Effect::spawnify()
        NAME
                Effect::spawnify - creates eldrazi spawn effect
        SYNOPSIS
               void spawnify(int controller);
                    e   --> the effect to be copies
        DESCRIPTION
               Creates a new effect for eldrazi spawn only
        RETURNS
                Nothing
        */
        /**/
        public void spawnify(int controller)
        {
            this.effect = EffectType.ActivatedAbility;
            this.costs[0] = "sacrifice";
            this.costs[1] = "~";
            this.modal = false;
            this.whatDo = new string[7];
            this.whatDo[0] = (controller == 0) ? "human" : "opponent";
            this.whatDo[1] = "no";
            this.whatDo[2] = "add";
            this.whatDo[3] = (controller == 0) ? "human" : "opponent";
            this.whatDo[5] = "6";
            this.tapCost = false;
        }

    }
}
