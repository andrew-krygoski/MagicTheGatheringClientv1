using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace MagicTheGatheringClientv1
{
    class Game
    {
        //0 == untap
        //1 == upkeep
        //2 == draw
        //3 == main
        //4 == start
        //5 == declare attack
        //6 == declare block
        //7 == damage
        //8 == end
        //9 == main
        //10 == end
        //11 == cleanup
        public int phase { get; set; }
        public Player[] players { get; set; }
        public List<Card> stack { get; set; }
        private Random rand = new Random();
        public int actPlayer { get; set; }
        public Interpreter cpu;
        public int toBeC, maxTargets, minTargets;
        /* rp == permanent (regular)
        //pl == player
        //cc == creature card
        //nc == nonblack creature
        //dc == colorless creature (devoid creature)
        //cl == creatures/players
        //rc == creature (regular)
        //np == noncreature permanent
        //tc == tapped creature
        //sc == creature w/ cmc <= 3 (small creature)
        //fc == flying creature
        */
        public string targetType;
        public bool gameOver, mulling, payAdd, resolving, attacking, blocking, cleanup, skippedAt, skippedBl, optionalEC, optionalEF, passed, landplayed, searching, scrying, firstTurn;
        public List<Guid> attackers, blockers, targets, attackingattackers;
        public List<Effect> modalArray;
        public List<Effect> toBe;

        /**/
        /*
        Game::Game() Game::Game()
        NAME
                Game::Game - creates a Game object.
        SYNOPSIS
                public Game::Game( bool Eldrazi );
                    Eldrazi          --> whether or not the user is playing the Eldrazi deck.
        DESCRIPTION
                This function just creates a new instance of the game and initializes all appropriete values
        RETURNS
                Nothing
        */
        /**/
        public Game(bool Eldrazi)
        {
            //make players, assign decks
            this.players = new Player[2];
            this.players[0] = new Player(Eldrazi, 0);
            this.players[1] = new Player(!Eldrazi, 1);
            //make Interpreter
            this.cpu = new Interpreter();
            //interpret all the players cards
            foreach (Player p in this.players)
            {
                foreach (Card c in p.deck)
                {
                    this.cpu.parseSide(c);
                }
            }
            //shuffle the decks and make them draw seven cards
            this.players[0].shuffle();
            this.players[1].shuffle();
            this.players[0].draw(7);
            this.players[1].draw(7);
            this.stack = new List<Card>();
            this.mulling = true;
            this.phase = 1;
            //first player is random
            this.actPlayer = rand.Next(0, 2);
            firstTurn = true;
            this.players[0].priority = true;
            this.passed = (this.actPlayer == 0) ? false : true;
            this.attacking = false;
            this.blocking = false;
            this.skippedAt = false;
            this.skippedBl = false;
            this.resolving = false;
            this.payAdd = false;
            this.landplayed = false;
            this.optionalEC = false;
            this.optionalEF = false;
            this.scrying = false;
            this.searching = false;
            this.attackingattackers = new List<Guid>();
            this.attackers = new List<Guid>();
            this.blockers = new List<Guid>();
            this.targets = new List<Guid>();
            this.toBe = new List<Effect>();
            this.modalArray = new List<Effect>();
            this.toBeC = -1;
        }

        /**/
        /*
        Game::incPhase() Game::incPhase()
        NAME
                Game::inPhase - moves the phases along
        DESCRIPTION
                If the phase is not 12, the game simply increases the phase by one
                Otherwise it sets the phase back to zero. Calls handlePhase() after incing
        RETURNS
                Nothing
        */
        /**/
        public void incPhase()
        {
            this.phase++;
            if (this.phase == 12)
                this.phase = 0;
            this.handlePhase();
        }

        /**/
        /*
        Game::handlePhase() Game::handlePhase()
        NAME
                Game::handlePhase - handles decisions of what to do for each phase
        DESCRIPTION
                Called from the incPhase(), this function decides for the game what
                actions need to be taken for this phase. I.E call untapPhase() if it
                is the Untap, or make the active player draw if it is the draw step
        RETURNS
                Nothing
        */
        /**/
        private void handlePhase()
        {
            //untap everything
            if (this.phase == 0) { this.untapPhase(); this.incPhase(); }
            //check for the only phase trigger in the game
            else if (this.phase == 1) { this.checkPhaseTriggers(); if (this.actPlayer == 1) this.passed = true; }
            //make the active player draw if it is not the first turn
            else if (this.phase == 2) { if (!firstTurn) { if (this.actPlayer == 0) { this.players[0].draw(1); } else { this.players[1].draw(1); } } else { firstTurn = false; } this.checkPhaseTriggers(); }
            else if (this.phase == 3) { this.checkPhaseTriggers(); }
            else if (this.phase == 4) { this.checkPhaseTriggers(); }
            //handle combat phases
            else if (this.phase == 5) { this.attackPhase(); this.checkPhaseTriggers(); }
            else if (this.phase == 6) { this.defendPhase(); this.checkPhaseTriggers(); }
            else if (this.phase == 7) { this.resolveCombat(); this.checkPhaseTriggers(); }
            else if (this.phase == 8) { this.checkPhaseTriggers(); }
            else if (this.phase == 9) { this.checkPhaseTriggers(); }
            else if (this.phase == 10) { this.checkPhaseTriggers(); }
            //execute cleanup phase to make players cleanup
            else if (this.phase == 11) { this.cleanupPhase(); if (!this.cleanup) { this.landplayed = false; this.actPlayer = (this.actPlayer == 1) ? 0 : 1; this.incPhase(); } }
            else { Console.WriteLine("How did I get here?"); }
        }

        /**/
        /*
        Game::passTurn() Game::passTurn()
        NAME
                Game::passTurn - allows the human to pass the turn
        DESCRIPTION
                Allows the user to pass their turn without having to 
                hit the priority button a bunch. Sets phase to 10 and calls incPhase()
        RETURNS
                Nothing
        */
        /**/
        public void passTurn()
        {
            this.phase = 10;
            incPhase();
        }

        /**/
        /*
        Game::checkPhaseTriggers() Game::checkPhaseTriggers()
        NAME
                Game::passTurn - sees if there are any triggers triggering
        DESCRIPTION
                Looks at all permanents to see if there are any phase triggers
                with the appropriete trigger. Calls resolve effect if so
        RETURNS
                nothing
        */
        /**/
        private void checkPhaseTriggers()
        {
            //triggers
            foreach (Player kyle in this.players)
            {
                foreach (Card theCard in kyle.permanents)
                {
                    foreach (Effect e in theCard.sides[theCard.side].effects)
                    {
                        if (e.effect == EffectType.PhaseTrigger)
                        {
                            if (e.when[1] == "upkeep" && this.phase == 1)
                                this.resolveEffect(e, theCard.controller);
                        }
                    }
                }
            }
        }

        /**/
        /*
        Game::checkEtbTriggers() Game::checkEtbTriggers()
        NAME
                Game::passTurn - same as check phase triggers
        SYNOPSIS
                void Game::checkEtbTriggers(Card entered);
                    entered          --> the card object that is causing this function to fire
        DESCRIPTION
                Goes through each permanent on the battlefield to see if they have a trigger
                that fires when the card that entered enters. Mostly to catch Allys, lands, 
                and ~. If a trigger is found, resolveEffect() is called on it
        RETURNS
                Nothing
        */
        /**/
        private void checkEtbTriggers(Card entered)
        {
            //go through all the cards, grab all the etb triggers
            List<Card> tmpList = new List<Card>();
            foreach (Player kyle in this.players)
            {
                foreach (Card theCard in kyle.permanents)
                {
                    foreach (Effect e in theCard.sides[theCard.side].effects)
                    {
                        if ((!string.IsNullOrEmpty(e.when[0])) && (e.when[0] == "etb"))
                        {
                            tmpList.Push(theCard);
                        }
                    }
                }
            }
            //for each card with a trigger, see if its being triggered by theCard
            foreach (Card c in tmpList)
            {
                foreach (Effect e in c.sides[0].effects)
                {
                    if (e.when[0] == "etb")
                    {
                        //it triggers when it enters
                        if (e.when[1] == "~" && (entered.instance == c.instance))
                        {
                            if ((e.whatDo[6] == "Kicker" && c.sides[0].additionalPaid) || e.whatDo[6] != "Kicker")
                                this.resolveEffect(e, c.controller);
                        }
                        else if (e.when[1].Contains(','))
                        {
                            string[] paul = e.when[1].Split(',');
                            //allyfall
                            if (paul[1] == "ally")
                            {
                                if (c.sides[0].subtypes[c.sides[0].subtypes.Length] == "Ally")
                                    this.resolveEffect(e, c.controller);
                            }
                            else if (paul[1] == "another")
                            {
                                if (c.sides[0].types[0] == "Creature" && c.controller == entered.controller)
                                    this.resolveEffect(e, c.controller);
                            }
                        }
                        else if (e.whatDo[6] == "Landfall" && entered.sides[entered.side].types[0] == "Land")
                            this.resolveEffect(e, c.controller);
                    }
                }
            }
            //enforce effects (fucking basilisk)
        }

        /**/
        /*
        Game::canPlay()  Game::canPlay()
        NAME
                Game::canPlay - checks to see if it is legal to play the card
        SYNOPSIS
                bool canPlay(Card theCard);
                    theCard          --> the card that we're checking the legality of
        DESCRIPTION
                Looks at the card type, followed by the phase it is, checks to see if 
                there are targets. If there are targets, make sure that there are 
                legal targets. 
        RETURNS
                True if the card can be cast
                False otherwise
        */
        /**/
        public bool canPlay(Card theCard)
        {
            bool ans = false;

            //you can always cast an instant
            if (theCard.sides[theCard.side].types[0] == "Instant")
                ans = true;
            //otherwise everything else is cast at sorcery speed
            else
                ans = (getSorcery() && (this.players[theCard.controller].priority == true) && (theCard.controller == this.actPlayer));

            //you can only play one land per turn
            if (this.landplayed && theCard.sides[theCard.side].types[0] == "Land")
                ans = false;

            //check for valid targets on spells
            if (theCard.sides[theCard.side].types[0] == "Instant" || theCard.sides[theCard.side].types[0] == "Sorcery")
            {
                if (!string.IsNullOrEmpty(theCard.sides[0].effects[0].targets[0]) && theCard.sides[0].effects[0].whatDo[6] != "Bolt")
                {
                    foreach (Player p in this.players)
                    {
                        foreach (Card c in p.permanents)
                        {
                            foreach (string t in c.sides[0].types)
                            {
                                //if there are legal targets, return true if it is
                                if (t.ToLower() == theCard.sides[0].effects[0].targets[1])
                                    return ans;
                            }
                        }
                    }
                    //its false otherwise, since there were no legal targets
                    ans = false;
                }
            }
            return ans;
        }

        /**/
        /*
        Game::canLevel() Game::canLevel()
        NAME
                Game::canLevel - checks to see if it is legal to activate a level
        DESCRIPTION
                Calls getSorcery, can't use the CanPlay
        RETURNS
                True if you can level
                False otherwise
        */
        /**/
        public bool canLevel() { return this.getSorcery(); }

        /**/
        /*
        Game::getSorcery() Game::getSorcery()
        NAME
                Game::getSorcery - allows the human to pass the turn
        DESCRIPTION
                Sorcery speed is only on the main with an empty stack
        RETURNS
                True if sorceries can be cast
                False otherwise
        */
        /**/
        private bool getSorcery() { return ((phase == 3 || phase == 9) && stack.Count() == 0); }

        /**/
        /*
        Game::Play() Game::Play()
        NAME
                Game::Play - moves the card to the correct location
        SYNOPSIS
                void Play(Card theCard);
                    theCard          --> theCard that needs to be played
        DESCRIPTION
                If the card is a land, play it and check for etb triggers
                Otherwise it goes on the stack
        RETURNS
                Nothing
        */
        /**/
        public void Play(Card theCard)
        {
            if (theCard.sides[theCard.side].types[0] == "Land")
            {
                this.players[theCard.controller].hand.Remove(theCard);
                theCard.currentZone = Zone.Battlefield;
                this.players[theCard.controller].permanents.Push(theCard);
                if (theCard.sides[theCard.side].types[0] == "Land")
                    this.landplayed = true;
                if (theCard.sides[0].types[0] != "Enchantment")
                    checkEtbTriggers(theCard);
            }
            else
            {
                this.players[theCard.controller].hand.Remove(theCard);
                theCard.currentZone = Zone.Stack;
                this.stack.Push(theCard);
            }
        }

        /**/
        /*
        Game::resolveStack() Game::resolveStack()
        NAME
                Game::resolveStack - moves things from the stack to where they belong
        DESCRIPTION
                Takes the card on the top of the stack and moves it to where it belongs
                If its an activated ability effect, it will just resolve that effect instead
        RETURNS
                Nothing
        */
        /**/
        public void resolveStack()
        {
            Card theCard = this.stack.Pop();
            //its an effect, just resolve it
            if (toBe.Count != 0)
            {
                Effect ex = toBe.Pop();
                int thg = toBeC;
                toBeC = -5;
                this.resolveEffect(ex, thg);
                return;
            }
            //it's a permanent, put it on the battlefield
            else if (theCard.sides[theCard.side].types[0] != "Instant" && theCard.sides[theCard.side].types[0] != "Sorcery")
            {
                theCard.currentZone = Zone.Battlefield;
                this.players[theCard.controller].permanents.Push(theCard);
                if (theCard.sides[theCard.side].types[0] == "Land")
                    this.landplayed = true;
                if (theCard.sides[0].types[0] != "Enchantment")
                    checkEtbTriggers(theCard);
            }
            //its not a permanent, resolve it
            else
            {
                foreach (Effect e in theCard.sides[theCard.side].effects)
                {
                    this.toBe.Add(e);
                }
                this.resolveEffect(theCard.sides[theCard.side].effects[0], theCard.controller);
            }
        }

        /**/
        /*
        Game::resolveActivatedAbility() Game::resolveActivatedAbility()
        NAME
                Game::resolveActivatedAbility - resolves an activated ability on lands
        SYNOPSIS
                void resolveActivatedAbility(ref Card theCard, Effect theEffect)
                    theCard             --> the card that contains theEffect
                    theEffect        --> the effect to be resolved
        DESCRIPTION
                Parses out the color of mana to add to the manapool
        RETURNS
                nothing
        */
        /**/
        public void resolveActivatedAbility(ref Card theCard, Effect theEffect)
        {
            if (theEffect.tapCost == true)
            {
                theCard.tapped = true;
            }
            foreach (Char c in theEffect.whatDo[5])
            {
                if (theEffect.whatDo[5].Length == 2)
                    this.players[theCard.controller].manapool.Push(new Mana(int.Parse(c.ToString()), "Eldrazi"));
                else
                    this.players[theCard.controller].manapool.Push(new Mana(int.Parse(c.ToString()), ""));
            }
        }

        private List<string> wordNums = new List<string>() { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight" };

        /**/
        /*
        Game::resolveEffect() Game::resolveEffect()
        NAME
                Game::resolveEffect - resolves the effect
        SYNOPSIS
                public void resolveEffect(Effect e, int controller);
                    e                --> the effect to be resolved
                    controller       --> the controller of the effect, 0 or 1
        DESCRIPTION
                This function handles the first part of resolving a spell, the targeting.
                If there are targets (cards for the player to choose, this gets the number 
                of targets, sets them, and returns for the human to choose. If there are
                no targets, just passes along to finishEffect(); Calls CompChooseTargets if it
                is the Comp's spell.
        RETURNS
                Nothing
        */
        /**/
        /* rp == permanent (regular)
        //pl == player
        //cc == creature card
        //nc == nonblack creature
        //dc == colorless creature (devoid creature)
        //cl == creatures/players
        //rc == creature (regular)
        //np == noncreature permanent
        //tc == tapped creature
        //sc == creature w/ cmc <= 3 (small creature)
        //fc == flying creature
        //ms == mountain/swamp
        //bl == basic land
        //card == a card (scrying)
        //rl == regular land
        //hc == hand creature (creature card from hand)
        */
        public void resolveEffect(Effect e, int controller)
        {
            //its an activated ability and not a card, put it onto the stack before resolving
            if (e.effect == EffectType.ActivatedAbility && toBeC != -5)
            {
                foreach (Card c in this.players[0].permanents)
                {
                    foreach (Effect ef in c.sides[0].effects)
                    {
                        if (ef == e)
                        {
                            this.stack.Push(c);
                            this.toBe.Push(e);
                        }
                    }

                }
                return;
            }
            else
                toBeC = -1;

            //if the card is modal, resolve the two effects they chose
            if (e.modal)
            {
                resolveEffect(this.modalArray[0], controller);
                resolveEffect(this.modalArray[1], controller);
            }
            //its annihilator
            else if (e.whatDo[6] == "Annihilator")
            {
                this.resolving = true;
                if (this.toBe.Count == 0)
                    this.toBe.Push(e);
                this.toBeC = controller;
                this.maxTargets = 2;
                this.minTargets = 2;
                this.targetType = "rp";
                if (controller == 0)
                    compChooseTargets();
            }
            //we're searching for a land
            else if (e.whatDo[2].ToLower() == "search")
            {
                if (controller == 0)
                {
                    this.resolving = true;
                }
                if (this.toBe.Count == 0)
                    this.toBe.Push(e);
                this.toBeC = controller;
                string[] tmp = e.whatDo[5].Split(' ');
                //looking for two lands?
                if ((tmp[0] == "two") || (tmp[5] == "two"))
                    this.maxTargets = 2;
                else
                    this.maxTargets = 1;
                //always min 0, cause you can fail
                this.minTargets = 0;

                //looking for mountain/swamp
                if ((tmp[0] != "two") && (tmp[4] != "basic"))
                    this.targetType = "ms";
                //resolving Command, creature card
                else if (tmp[0] == "your")
                    this.targetType = "cc";
                //everything else gets basic land
                else
                    this.targetType = "bl";
                //execute comp if comp
                if (controller == 1)
                {
                    compChooseTargets();
                }
            }
            //its the Forked bolt
            else if (e.whatDo[6] == "Bolt")
            {
                this.resolving = true;
                if (this.toBe.Count == 0)
                    this.toBe.Push(e);
                this.toBeC = controller;
                this.maxTargets = 2;
                this.minTargets = 1;
                this.targetType = "cl";
                if (controller == 1)
                    compChooseTargets();
            }
            //we're scrying, need to choose cards to go to bottom
            else if (e.whatDo[2].ToLower() == "scry")
            {
                this.scrying = true;
                this.maxTargets = 2;
                this.minTargets = 0;
                this.resolving = true;
                if (this.toBe.Count == 0)
                    this.toBe.Push(e);
                this.toBeC = controller;
                this.targetType = "card";
            }
            //its a generic targeting card
            else if (!string.IsNullOrEmpty(e.targets[0]))
            {
                this.resolving = true;
                if (this.toBe.Count == 0)
                    this.toBe.Push(e);
                this.toBeC = controller;
                this.maxTargets = int.Parse(e.targets[0]);

                //we can choose no targets
                if (e.targets[0] == ">")
                    this.minTargets = 0;
                else
                    this.minTargets = int.Parse(e.targets[0]);

                //parse what its targeting
                if (e.targets[1] == "player")
                    this.targetType = "pl";
                else if (e.targets[1] == "creature|player")
                    this.targetType = "cl";
                else if (e.targets[1] == "creature")
                    this.targetType = "rc";
                else
                {
                    Console.WriteLine("Targets " + e.targets[1]);
                }
                if (controller == 1)
                    compChooseTargets();
            }
            //there are no targets, just go to finish effect
            else if (string.IsNullOrEmpty(e.targets[0]))
            {
                if (toBe.Count == 0)
                    this.toBe.Push(e);
                this.toBeC = controller;
                this.finishEffect();
            }
            else
            {
                Console.WriteLine("Effect = " + e);
            }

        }

        /**/
        /*
        Game::finishEffect() Game::finishEffect()
        NAME
                Game::finishEffect - finishes resolving an effect after targets are chosen
        DESCRIPTION
                Looks at the effect stored in toBe and resolves the spell using the targets
                List. Applies statebasedActions() at the end
        RETURNS
                Nothing
        */
        /**/
        public void finishEffect()
        {
            foreach (Effect e in this.toBe)
            {
                //its a kicker effect, and if they targeted players then its marsh casualties, give creatures -1 or -2
                if (e.whatDo[6] == "Kicker" && (this.players[0].identifier == targets[0] || this.players[1].identifier == targets[0]))
                {
                    string[] five = e.whatDo[5].Split(' ');
                    string count = five[0];
                    if (this.stack[0].sides[0].additionalPaid)
                    {
                        count = five[1];
                    }
                    int num = (this.players[0].identifier == targets[0]) ? 0 : 1;
                    foreach (Card c in this.players[num].permanents)
                    {
                        c.counters.Add(new Counter(count, true));
                    }
                }
                //sacrifice all the targets
                else if (e.whatDo[2].ToLower() == "sacrifice")
                {
                    this.sacrifice();
                }
                //we searched and chose our targets
                else if (e.whatDo[2].ToLower() == "search")
                {
                    string[] tmp = e.whatDo[5].Split(' ');
                    foreach (Guid t in this.targets)
                    {

                        Card c = this.players[this.toBeC].deck.Find(x => x.instance == t);
                        this.players[this.toBeC].deck.Remove(c);
                        //if its your, we got a creature card
                        if (tmp[0] != "your")
                        {
                            //see if it comes in untapped (Harrow or Tar Pit)
                            if (!(((tmp[0] == "two") || (tmp[5] == "two")) || ((tmp[0] != "two") && (tmp[4] != "basic")))) { c.tapped = true; }
                            c.changeZones(Zone.Battlefield);
                            this.players[this.toBeC].permanents.Push(c);
                        }
                        else
                        {
                            c.changeZones(Zone.Hand);
                            this.players[this.toBeC].hand.Push(c);
                        }
                    }
                }
                //it deals damage
                else if (e.whatDo[2].ToLower() == "deal" || e.whatDo[2].ToLower() == "deals")
                {
                    //get the number of damage
                    int num;
                    string[] tmp = e.whatDo[5].Split(' ');
                    try
                    {
                        num = int.Parse(tmp[0]);
                    }
                    catch (Exception)
                    {
                        num = wordNums.FindIndex(x => x == tmp[0].ToLower());
                        if (num == -1)
                        {

                            Console.WriteLine("What IS THIS?? " + e.whatDo);
                        }
                        num = 0;
                    }
                    if (e.whatDo[6] == "Bolt")
                    {
                        if (this.targets.Count == 1)
                            num = 2;
                        else
                            num = 1;
                    }
                    foreach (Guid g in this.targets)
                    {
                        //find the targets, assign the damage as appropriete
                        if (g == this.players[0].identifier) { this.players[0].life -= num; continue; }
                        else if (g == this.players[1].identifier) { this.players[1].life -= num; continue; }
                        bool human = true;
                        int found = this.players[0].permanents.FindIndex(x => x.instance == g);
                        if (found == -1) { human = false; found = this.players[1].permanents.FindIndex(x => x.instance == g); }
                        if (found == -1) { Console.WriteLine("IT DISSAPEARED, NO??"); return; }
                        this.players[((human) ? 0 : 1)].permanents[found].damageMarked += num;
                    }
                }
                //we're destroying a creature, which is conviently the same thing as sacrificing with this deck
                else if (e.whatDo[2].ToLower() == "destroy")
                {
                    //corpsehatch
                    if (e.whatDo[4] == "nonblack")
                    {
                        foreach (Guid g in this.targets)
                        {
                            bool human = true;
                            int found = this.players[0].permanents.FindIndex(x => x.instance == g);
                            if (found == -1) { human = false; found = this.players[1].permanents.FindIndex(x => x.instance == g); }
                            if (found == -1) { Console.WriteLine("IT DISSAPEARED, NO??"); return; }
                            if (!this.players[(human) ? 0 : 1].permanents[found].sides[0].colors.Contains("Black"))
                                this.sacrifice();
                        }
                    }
                    else
                    {
                        this.sacrifice();
                    }
                }
                //we're putting counters or tokens onto the field
                else if (e.whatDo[2].ToLower() == "put")
                {
                    string[] tmp = e.whatDo[5].Split(' ');
                    int num;
                    if (tmp[0] == "a")
                    {
                        //Putting plant tokens on the battlefield
                        if (tmp[3] == "Plant")
                        {
                            int count = this.players[this.toBeC].permanents.Count(x => x.sides[0].types[0] == "Land");
                            for (int i = 0; i < count; i++)
                            {
                                this.players[this.toBeC].permanents.Push(new Card(this.toBeC, "Plant", "0", "1", null));
                            }
                        }
                        //Putting counters on all plants on the battlefield
                        else if (tmp[5] == "Plant")
                        {
                            foreach (Card c in this.players[this.toBeC].permanents.FindAll(x => x.sides[0].subtypes.Contains("Plant")))
                            {
                                c.counters.Push(new Counter("plus1", false));
                            }
                        }
                        //Putting a counter on Greypelt hunter
                        else if (tmp[tmp.Length - 1] == "Hunter.")
                        {
                            this.players[this.toBeC].permanents[this.players[this.toBeC].permanents.FindIndex(x => x.names[0] == "Graypelt Hunter")].counters.Push(new Counter("plus1", false));
                        }
                        //putting an expedition counter on an enchantment
                        else if (tmp[tmp.Length - 1] == "Expedition.")
                        {
                            this.players[this.toBeC].permanents[this.players[this.toBeC].permanents.FindIndex(x => x.names[0] == "Khalni Heart Expedition")].counters.Push(new Counter("Expedition", false));
                        }
                        //Eldrazi spawn tokens
                        else if (tmp[tmp.Length - 1] == "pool.\"")
                        {
                            Effect eC = new Effect();
                            eC.spawnify(this.toBeC);
                            this.players[this.toBeC].permanents.Push(new Card(this.toBeC, "Eldrazi Spawn", "0", "1", eC));
                        }
                        //Retreat
                        else
                        {
                            Console.WriteLine("Didnt do that yet");
                        }
                    }
                    //its a leveler
                    else if (tmp[0] == "level")
                    {
                        //if it is a level where it gains something, give it to the creature, otherwise just return because effects are already being applied
                        Card theCard = this.players[0].permanents[this.players[0].permanents.FindIndex(x => x.instance == targets[0])];
                        theCard.counters.Push(new Counter("level", false));
                        if (theCard.sides[0].levelUp[1].Split(' ')[0] == "LEVEL")
                        {
                            int start = -1;
                            if (theCard.counters.Count(x => x.type == "level") <= int.Parse(theCard.sides[0].levelUp[1][theCard.sides[0].levelUp[1].Length - 1].ToString()))
                            {
                                start = 2;
                            }
                            else
                            {
                                for (int i = 2; i < theCard.sides[0].levelUp.Length; i++)
                                    if (theCard.sides[0].levelUp[i].Split(' ')[0] == "LEVEL") { start = i + 1; }
                            }
                            if ((theCard.counters.Count(x => x.type == "level") == int.Parse(theCard.sides[0].levelUp[start - 1][theCard.sides[0].levelUp[start - 1].Length - 3].ToString())))
                            {
                                for (int i = start; i < theCard.sides[0].levelUp.Length; i++)
                                {
                                    if (theCard.sides[0].levelUp[i][1] == '/')
                                    {
                                        theCard.sides[theCard.side].power = theCard.sides[0].levelUp[i][0].ToString();
                                        theCard.sides[theCard.side].toughness = theCard.sides[0].levelUp[i][2].ToString();
                                    }
                                    else if ((theCard.sides[0].levelUp[i].Split(' ')[0] == "First") || (theCard.sides[0].levelUp[i].Split(' ')[0] == "Flying") || (theCard.sides[0].levelUp[i].Split(' ')[0] == "Trample"))
                                    {
                                        this.cpu.parseKeyword(theCard.sides[0].levelUp[i].Split(','), ref theCard);
                                    }
                                    else if (theCard.sides[0].levelUp[i].Split(' ')[0] == "Other")
                                    {
                                        if (start == 2)
                                        {
                                            this.players[theCard.controller].contEffects.Add(new Counter("plus1", false));
                                        }
                                        else
                                        {
                                            this.players[theCard.controller].contEffects.RemoveAt(this.players[theCard.controller].contEffects.FindIndex(x => x.type == "plus1"));
                                            this.players[theCard.controller].contEffects.Add(new Counter("plus2", false));
                                        }
                                    }
                                    else if ((theCard.sides[0].levelUp[i].Split(' ')[0] == "LEVEL"))
                                        break;
                                    else
                                        Console.WriteLine("Unforseen");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("What am I doing?");

                        }
                    }
                    //putting other eldrazi spawn onto the field
                    else if ((num = wordNums.FindIndex(x => x == tmp[0])) > -1)
                    {
                        Effect eC = new Effect();
                        eC.spawnify(this.toBeC);
                        Card spawn = new Card(this.toBeC, "Eldrazi Spawn", "1", "1", eC);
                        for (int i = 0; i < num; i++)
                        {
                            this.players[this.toBeC].permanents.Push(spawn);
                        }
                    }
                    //Hellion tokens
                    else if (tmp[0] == "that")
                    {
                        foreach (Card c in this.players[this.toBeC].permanents.FindAll(x => x.sides[0].types.Contains("Creature")))
                        {
                            this.players[this.toBeC].permanents.Remove(c);
                            c.changeZones(Zone.Graveyard);
                            this.players[this.toBeC].graveyard.Push(c);
                            this.players[this.toBeC].permanents.Push(new Card(this.toBeC, "Hellion", "4", "4", null));
                        }
                    }
                    //Resolving oblivion sower's land-stealing ability
                    else if (tmp[0] == "any" && tmp[1] == "number")
                    {
                        foreach (Guid g in this.targets)
                        {
                            Card c = this.players[this.toBeC].exile[this.players[this.toBeC].exile.FindIndex(x => x.instance == g)];
                            this.players[this.toBeC].exile.Remove(c);
                            c.changeZones(Zone.Battlefield);
                            this.players[this.toBeC].permanents.Push(c);
                        }
                    }
                    //putting a creature on top of the owners library
                    else if (tmp[tmp.Length - 1] == "top.")
                    {
                        bool human = true;
                        int found = this.players[0].permanents.FindIndex(x => x.instance == this.targets[0]);
                        if (found == -1) { human = false; found = this.players[1].permanents.FindIndex(x => x.instance == this.targets[0]); }
                        if (found == -1) { Console.WriteLine("IT DISSAPEARED, NO??"); return; }
                        Card c = this.players[(human) ? 0 : 1].permanents[found];
                        this.players[(human) ? 0 : 1].permanents.Remove(c);
                        c.changeZones(Zone.Library);
                        this.players[(human) ? 0 : 1].deck.Insert(2, c);
                    }
                    else
                    {
                        Console.WriteLine("What's huh " + e.whatDo);
                    }
                }
                //put the 0/1/2 cards onto the bottom
                else if (e.whatDo[2].ToLower() == "scry")
                {
                    if (this.targets.Count == 1)
                    {
                        Card bottom = (this.players[this.toBeC].deck[0].instance == targets[0]) ? this.players[this.toBeC].deck[0] : this.players[this.toBeC].deck[1];
                        this.players[this.toBeC].deck.Remove(bottom);
                        this.players[this.toBeC].deck.Add(bottom);
                    }
                    else if (this.targets.Count == 2)
                    {
                        this.players[this.toBeC].deck.Add(this.players[this.toBeC].deck.Pop());
                        this.players[this.toBeC].deck.Add(this.players[this.toBeC].deck.Pop());
                    }
                }
                //giving buffs, or gaining life
                else if (e.whatDo[2].ToLower() == "gain" || e.whatDo[2].ToLower() == "gains")
                {
                    if (targetType != "rc" && targetType != "dc")
                    {
                        string[] tmp = e.whatDo[5].Split(' ');
                        int num;
                        try
                        {
                            num = int.Parse(tmp[0]);
                            this.players[(e.whatDo[3] == "human") ? 0 : 1].life += num;
                        }
                        catch (Exception)
                        {
                            num = wordNums.FindIndex(x => x == tmp[0].ToLower());
                            this.players[(e.whatDo[3] == "human") ? 0 : 1].life += num;
                        }
                    }
                    else if (targetType != "dc")
                    {
                        bool human = true;
                        int found = this.players[0].permanents.FindIndex(x => x.instance == this.targets[0]);
                        if (found == -1) { human = false; found = this.players[1].permanents.FindIndex(x => x.instance == this.targets[0]); }
                        if (found == -1) { Console.WriteLine("IT DISSAPEARED, NO??"); return; }
                        Card c = this.players[(human) ? 0 : 1].permanents[found];
                        c.sides[0].keywordAbilities.Add("Haste");
                    }
                }
                //loses life, just players
                else if (e.whatDo[2].ToLower() == "loses" || e.whatDo[2].ToLower() == "lose")
                {
                    string[] tmp = e.whatDo[5].Split(' ');
                    int num;
                    try
                    {
                        num = int.Parse(tmp[0]);
                        this.players[(e.whatDo[3] == "human") ? 0 : 1].life -= num;
                    }
                    catch (Exception)
                    {
                        num = wordNums.FindIndex(x => x == tmp[0].ToLower());
                        this.players[(e.whatDo[3] == "human") ? 0 : 1].life -= num;
                    }
                }
                //only players draw cards
                else if (e.whatDo[2].ToLower() == "draw" || e.whatDo[2].ToLower() == "draws")
                {
                    string[] tmp = e.whatDo[5].Split(' ');
                    int num;
                    try
                    {
                        num = int.Parse(tmp[0]);
                        this.players[(e.whatDo[3] == "human") ? 0 : 1].draw(num);
                    }
                    catch (Exception)
                    {
                        if (tmp[0] == "a")
                            num = 1;
                        else
                            num = wordNums.FindIndex(x => x == tmp[0].ToLower());
                        this.players[(e.whatDo[3] == "human") ? 0 : 1].draw(num);
                    }
                }
                //empty my targeting array
                this.targetType = "";
                this.targets = new List<Guid>();
            }
            //reset all values
            this.toBe = new List<Effect>();
            this.resolving = false;
            this.toBeC = -1;
            this.scrying = false;
            this.maxTargets = -1;
            this.minTargets = -1;
            this.optionalEC = false;
            this.optionalEF = false;
            this.searching = false;
            this.statebasedActions();
        }

        /**/
        /*
        Game::untapPhase() Game::untapPhase()
        NAME
                Game::untapPhase - untaps all creatures
        DESCRIPTION
                Goes through all permanents of active player and sets tapped = false;
                also removes summoning sickness
        RETURNS
                Nothing
        */
        /**/
        private void untapPhase() { foreach (Card theCard in this.players[actPlayer].permanents) { theCard.tapped = false; theCard.sick = false; } }

        /**/
        /*
        Game::attackPhase() Game::attackPhase()
        NAME
                Game::attackPhase - decides if there is a
        DESCRIPTION
                Goes through all permanents of active player and sees if they
                can even attack with anything. If not, they don't get to attack
        RETURNS
                Nothing
        */
        /**/
        private void attackPhase()
        {
            if (this.canDeclareA(this.actPlayer))
            {
                attacking = true;
            }
            else
            {
                skippedAt = true;
                attacking = false;
            }
        }

        /**/
        /*
        Game::attackPhase() Game::attackPhase()
        NAME
                Game::attackPhase - sees if the player can defend themselves
        DESCRIPTION
                Goes through all permanents of active player and sees if they
                can even attack with anything. If not, they don't get to defend against nothing
        RETURNS
                Nothing
        */
        /**/
        private void defendPhase()
        {
            int player = (actPlayer == 0) ? 1 : 0;
            if (canDeclareB(player) && this.attackingattackers.Count != 0)
            {
                this.blocking = true;
            }
            else
            {
                skippedBl = true;
                this.blocking = false;
            }
        }

        /**/
        /*
        Game::resolveCombat() Game::resolveCombat()
        NAME
                Game::resolveCombat - apply affects and resolve damage
        DESCRIPTION
                Apply effects to creatures
                Takes the attacker from attackingattackers and sees if it is in attackers
                if so then the card is being blocked and looks for the blocker in blockers
                if the blocker is still there, they deal damage to eachother
                if not blocked, player loses life
                deapply effects to creatures
                Applies statebasedactions at end
        RETURNS
                Nothing
        */
        /**/
        private void resolveCombat()
        {
            int active = actPlayer, non = (actPlayer == 0) ? 1 : 0;
            bool aallies = false, ballies = false;
            if (this.attackingattackers.Count() == 0)
                return;
            //go through all attackers
            for (int i = 0; i < this.attackingattackers.Count(); i++)
            {
                //make sure the attacker is still there
                int thing = this.players[active].permanents.FindIndex(x => x.instance == attackingattackers[i]);
                if (thing != -1)
                {
                    Card attackingattacker = this.players[active].permanents[thing];
                    if (attackingattacker.sides[0].power == "*")
                    {
                        aallies = true;
                        attackingattacker.sides[0].power = this.players[active].permanents.Count(x => x.sides[0].subtypes.Contains("Ally")).ToString();
                        attackingattacker.sides[0].toughness = this.players[active].permanents.Count(x => x.sides[0].subtypes.Contains("Ally")).ToString();
                    }
                    //save the power toughness before applying effects
                    string tmpatPower = attackingattacker.sides[0].power;
                    string tmpatTough = attackingattacker.sides[0].toughness;
                    //apply counter effects
                    if (attackingattacker.counters.Count != 0)
                    {
                        foreach (Counter c in attackingattacker.counters)
                        {
                            if (c.type[0] == 'p')
                            {
                                attackingattacker.sides[0].power = (int.Parse(attackingattacker.sides[0].power) + int.Parse(c.type[4].ToString())).ToString();
                                attackingattacker.sides[0].toughness = (int.Parse(attackingattacker.sides[0].toughness) + int.Parse(c.type[4].ToString())).ToString();
                            }
                            else if (c.type[0] == 'm')
                            {
                                attackingattacker.sides[0].power = (int.Parse(attackingattacker.sides[0].power) - int.Parse(c.type[4].ToString())).ToString();
                                attackingattacker.sides[0].toughness = (int.Parse(attackingattacker.sides[0].toughness) - int.Parse(c.type[4].ToString())).ToString();
                            }
                        }
                    }
                    if (this.players[attackingattacker.controller].contEffects.Count != 0)
                    {
                        foreach (Counter c in this.players[attackingattacker.controller].contEffects)
                        {
                            if (c.type[0] == 'p')
                            {
                                attackingattacker.sides[0].power = (int.Parse(attackingattacker.sides[0].power) + int.Parse(c.type[4].ToString())).ToString();
                                attackingattacker.sides[0].toughness = (int.Parse(attackingattacker.sides[0].toughness) + int.Parse(c.type[4].ToString())).ToString();
                            }
                            else if (c.type[0] == 'm')
                            {
                                attackingattacker.sides[0].power = (int.Parse(attackingattacker.sides[0].power) - int.Parse(c.type[4].ToString())).ToString();
                                attackingattacker.sides[0].toughness = (int.Parse(attackingattacker.sides[0].toughness) - int.Parse(c.type[4].ToString())).ToString();
                            }
                        }
                    }
                    //it's not blocked, deals damage to player
                    if (!this.attackers.Contains(attackingattackers[i]))
                    {
                        this.players[non].life -= int.Parse(attackingattacker.sides[0].power);
                        attackingattacker.sides[0].power = tmpatPower;
                        attackingattacker.sides[0].toughness = tmpatTough;
                        if (attackingattacker.names[0].Split(' ')[0] == "Dominator")
                        {
                            Card tmp = (this.players[non]).deck.PopAt(0);
                            tmp.changeZones(Zone.Exile);
                            (this.players[non]).exile.Push(tmp);
                        }
                    }
                    //it is blocked
                    else
                    {
                        //make sure the blocker is still there
                        int blockerNum = this.attackers.FindIndex(x => x == attackingattackers[i]);
                        if (blockerNum > -1)
                        {
                            Card blocker = this.players[non].permanents[blockerNum];
                            if (blocker.sides[0].power == "*")
                            {
                                ballies = true;
                                blocker.sides[0].power = this.players[non].permanents.Count(x => x.sides[0].subtypes.Contains("Ally")).ToString();
                                blocker.sides[0].toughness = this.players[non].permanents.Count(x => x.sides[0].subtypes.Contains("Ally")).ToString();
                            }
                            string tmpblockpower = blocker.sides[0].power, tmpblocktough = blocker.sides[0].toughness;
                            //apply counter effects
                            if (blocker.counters.Count != 0)
                            {
                                foreach (Counter c in blocker.counters)
                                {
                                    if (c.type[0] == 'p')
                                    {
                                        blocker.sides[0].power = (int.Parse(blocker.sides[0].power) + int.Parse(c.type[4].ToString())).ToString();
                                        blocker.sides[0].toughness = (int.Parse(blocker.sides[0].toughness) + int.Parse(c.type[4].ToString())).ToString();
                                    }
                                    else if (c.type[0] == 'm')
                                    {
                                        blocker.sides[0].power = (int.Parse(blocker.sides[0].power) - int.Parse(c.type[4].ToString())).ToString();
                                        blocker.sides[0].toughness = (int.Parse(blocker.sides[0].toughness) - int.Parse(c.type[4].ToString())).ToString();
                                    }
                                }
                            }
                            if (this.players[blocker.controller].contEffects.Count != 0)
                            {
                                foreach (Counter c in this.players[blocker.controller].contEffects)
                                {
                                    if (c.type[0] == 'p')
                                    {
                                        blocker.sides[0].power = (int.Parse(blocker.sides[0].power) + int.Parse(c.type[4].ToString())).ToString();
                                        blocker.sides[0].toughness = (int.Parse(blocker.sides[0].toughness) + int.Parse(c.type[4].ToString())).ToString();
                                    }
                                    else if (c.type[0] == 'm')
                                    {
                                        blocker.sides[0].power = (int.Parse(blocker.sides[0].power) - int.Parse(c.type[4].ToString())).ToString();
                                        blocker.sides[0].toughness = (int.Parse(blocker.sides[0].toughness) - int.Parse(c.type[4].ToString())).ToString();
                                    }
                                }
                            }
                            //apply damage, damage back
                            blocker.damageMarked += int.Parse(attackingattacker.sides[0].power);
                            attackingattacker.damageMarked += int.Parse(blocker.sides[0].power);
                            if ((attackingattacker.sides[0].keywordAbilities.Contains("Trample")) && (int.Parse(attackingattacker.sides[0].power) > int.Parse(blocker.sides[0].toughness)))
                            {
                                this.players[non].life -= int.Parse(attackingattacker.sides[0].power) - int.Parse(attackingattacker.sides[0].toughness);
                                if (attackingattacker.names[0].Split(' ')[0] == "Dominator")
                                {
                                    Card tmp = (this.players[non]).deck.PopAt(0);
                                    tmp.changeZones(Zone.Exile);
                                    (this.players[non]).exile.Push(tmp);
                                }
                            }
                            //put power back
                            attackingattacker.sides[0].power = tmpatPower;
                            attackingattacker.sides[0].toughness = tmpatTough;
                            blocker.sides[0].power = tmpblockpower;
                            blocker.sides[0].power = tmpblocktough;
                            if (ballies)
                            {
                                blocker.sides[0].power = "*";
                                attackingattacker.sides[0].toughness = "*";
                            }
                        }
                        //defender is gone somehow (happens), only trample goes through
                        else
                        {
                            if (attackingattacker.sides[0].keywordAbilities.Contains("Trample"))
                            {
                                this.players[non].life -= int.Parse(attackingattacker.sides[0].power);
                                attackingattacker.sides[0].power = tmpatPower;
                                attackingattacker.sides[0].toughness = tmpatTough;
                                if (attackingattacker.names[0].Split(' ')[0] == "Dominator")
                                {
                                    Card tmp = (this.players[non]).deck.PopAt(0);
                                    tmp.changeZones(Zone.Exile);
                                    (this.players[non]).exile.Push(tmp);
                                }
                            }
                        }
                    }
                    if (aallies)
                    {
                        attackingattacker.sides[0].power = "*";
                        attackingattacker.sides[0].toughness = "*";
                    }
                }
            }
            //execute statebased actions
            this.statebasedActions();
        }

        /**/
        /*
        Game::discard() Game::discard()
        NAME
                Game::discard - apply affects and resolve damage
        SYNOPSIS
                public Game::Game( int card );
                    card          --> integer of the card that we're discarding
        DESCRIPTION
                takes the card out of the human's hand and puts it into the graveyard
                if it is the cleanup phase, turn off cleanup when handsize is 7
        RETURNS
                Nothing
        */
        /**/
        public void discard(int card)
        {
            this.players[0].hand[card].changeZones(Zone.Graveyard);
            this.players[0].graveyard.Push(this.players[0].hand[card]);
            this.players[0].hand.Remove(this.players[0].hand[card]);
            if (this.cleanup && this.players[0].hand.Count() < 8)
            {
                this.cleanup = false;
                this.landplayed = false;
                this.actPlayer = (this.actPlayer == 1) ? 0 : 1;
                this.incPhase();
            }
        }

        /**/
        /*
        Game::sacrifice() Game::sacrifice()
        NAME
                Game::sacrifice - sacrifices all creatures in targets
        DESCRIPTION
                iterates through synopsis and sends to the graveyard all permanents 
                with their instances in targets
        RETURNS
                Nothing
        */
        /**/
        public void sacrifice()
        {
            int human = 0;
            foreach (Guid t in this.targets)
            {
                Card tmp = this.players[0].permanents.Find(x => x.instance == t);
                if (tmp == null)
                {
                    tmp = this.players[1].permanents.Find(x => x.instance == t);
                    human = 1;
                }

                this.players[human].permanents.Remove(tmp);
                foreach (Counter s in tmp.counters)
                    tmp.counters.Remove(s);
                tmp.changeZones(Zone.Graveyard);
                this.players[human].graveyard.Push(tmp);
            }
            this.targets = new List<Guid>();
        }

        /**/
        /*
        Game::searchFor() Game::searchFor()
        NAME
                Game::searchFor - 
        SYNOPSIS
                public Game::searchFor( Zone z );
                    z          --> the zone we're sending the card
        DESCRIPTION
               takes the card(s) specified in targets out of the deck
               and delivers it to the zone specified in z
        RETURNS
                Nothing
        */
        /**/
        public void searchFor(Zone z)
        {
            foreach (Guid t in this.targets)
            {
                bool human = true;
                int theCard = this.players[0].deck.FindIndex(x => x.instance == t);
                if (theCard == -1) { human = false; this.players[0].deck.FindIndex(x => x.instance == t); }
                if (theCard == -1) { Console.WriteLine("Something's wrong..."); }
                Card tmp = this.players[((human) ? 0 : 1)].deck[theCard];
                this.players[((human) ? 0 : 1)].deck.Remove(tmp);
                tmp.changeZones(z);
                switch (z)
                {
                    case Zone.Battlefield:
                        this.players[((human) ? 0 : 1)].permanents.Push(tmp);
                        break;
                    case Zone.Hand:
                        this.players[((human) ? 0 : 1)].hand.Push(tmp);
                        break;
                }
                this.players[((human) ? 0 : 1)].shuffle();
            }
            this.targets = new List<Guid>();
        }

        /**/
        /*
        Game::statebasedActions() Game::statebasedActions()
        NAME
                Game::statebasedActions - apply affects and see if anything died
        DESCRIPTION
               for each permanent on the field, apply counter effects, then see
               if damage marked > toughness. If so it goes to the grave
        RETURNS
                Nothing
        */
        /**/
        private void statebasedActions()
        {
            foreach (Player p in this.players)
            {
                for (int i = p.permanents.Count - 1; i > -1; i--)
                {
                    Card c = p.permanents[i];
                    if (c.sides[0].toughness != null)
                    {
                        bool allies = false;
                        if (c.sides[0].power == "*")
                        {
                            allies = true;
                            c.sides[0].power = this.players[c.controller].permanents.Count(x => x.sides[0].subtypes.Contains("Ally")).ToString();
                            c.sides[0].toughness = this.players[c.controller].permanents.Count(x => x.sides[0].subtypes.Contains("Ally")).ToString();
                        }
                        string tmppower = c.sides[0].power, tmptough = c.sides[0].toughness;
                        if (c.counters.Count != 0)
                        {
                            foreach (Counter co in c.counters)
                            {
                                if (co.type[0] == 'p')
                                {
                                    c.sides[0].power = (int.Parse(c.sides[0].power) + int.Parse(co.type[4].ToString())).ToString();
                                    c.sides[0].toughness = (int.Parse(c.sides[0].toughness) + int.Parse(co.type[4].ToString())).ToString();
                                }
                                else if (co.type[0] == 'm')
                                {
                                    c.sides[0].power = (int.Parse(c.sides[0].power) - int.Parse(co.type[4].ToString())).ToString();
                                    c.sides[0].toughness = (int.Parse(c.sides[0].toughness) - int.Parse(co.type[4].ToString())).ToString();
                                }
                            }
                        }
                        if (this.players[c.controller].contEffects.Count != 0)
                        {
                            foreach (Counter s in this.players[c.controller].contEffects)
                            {
                                if (s.type[0] == 'p')
                                {
                                    c.sides[0].power = (int.Parse(c.sides[0].power) + int.Parse(s.type[4].ToString())).ToString();
                                    c.sides[0].toughness = (int.Parse(c.sides[0].toughness) + int.Parse(s.type[4].ToString())).ToString();
                                }
                                else if (s.type[0] == 'm')
                                {
                                    c.sides[0].power = (int.Parse(c.sides[0].power) - int.Parse(s.type[4].ToString())).ToString();
                                    c.sides[0].toughness = (int.Parse(c.sides[0].toughness) - int.Parse(s.type[4].ToString())).ToString();
                                }
                            }
                        }
                        if (c.damageMarked >= int.Parse(c.sides[0].toughness))
                        {
                            c.sides[0].power = tmppower;
                            c.sides[0].toughness = tmptough;
                            if (allies)
                            {
                                c.sides[0].power = "*";
                                c.sides[0].toughness = "*";
                            }
                            p.permanents.Remove(c);
                            foreach (Counter s in c.counters)
                                c.counters.Remove(s);
                            c.changeZones(Zone.Graveyard);
                            p.graveyard.Push(c);
                        }
                        else
                        {
                            c.sides[0].power = tmppower;
                            c.sides[0].toughness = tmptough;
                        }
                    }
                }
            }
        }

        /**/
        /*
        Game::canDeclareA() Game::canDeclareA()
        NAME
                Game::canDeclareA - see if the player can even go into combat
        SYNOPSIS
                public Game::Game( int player );
                    player          --> integer of the player, 0 or 1
        DESCRIPTION
                looks through each permanent on the players battlefield and if there is a 
                single creature able to attack, it returns true to allow for choosing of attackers
        RETURNS
                true if the player will have a combat phase, false otherwise
        */
        /**/
        private bool canDeclareA(int player)
        {
            foreach (Card c in this.players[player].permanents)
            {
                if ((c.sides[c.side].types[0] == "Creature") && (!c.tapped) && ((!c.sick) || (c.sides[0].keywordAbilities.Contains("Haste"))))
                {
                    return true;
                }
            }
            return false;
        }

        /**/
        /*
        Game::canDeclareB() Game::canDeclareB()
        NAME
                Game::canDeclareB - see if the player can even go into combat
        SYNOPSIS
                public Game::canDeclareB( int player );
                    player          --> integer of the player, 0 or 1
        DESCRIPTION
                looks through each permanent on the players battlefield and if there is a 
                single creature able to block, it returns true to allow for choosing of blockers
        RETURNS
                true if the player will have a defend phase, false otherwise
        */
        /**/
        private bool canDeclareB(int player)
        {
            foreach (Card c in this.players[player].permanents)
            {
                if ((c.sides[c.side].types[0] == "Creature") && (!c.tapped))
                {
                    return true;
                }
            }
            return false;
        }

        /**/
        /*
        Game::cleanupPhase() Game::cleanupPhase()
        NAME
                Game::cleanupPhase - toggles cleanup phase option
        DESCRIPTION
                makes players discard if hansize too large
                removes temporary counters from all permanents
        RETURNS
                Nothing
        */
        /**/
        private void cleanupPhase()
        {
            if (actPlayer == 0)
            {
                if (this.players[0].hand.Count() > 7)
                {
                    this.cleanup = true;
                }
            }
            foreach (Player p in this.players)
            {
                foreach (Card c in p.permanents)
                {
                    foreach (Counter q in c.counters)
                    {
                        if (q.temporary)
                            c.counters.Remove(q);
                    }
                }
            }
        }

        /**/
        /*
        Game::togglePriority() Game::togglePriority()
        NAME
                Game::togglePriority - toggles priority
        DESCRIPTION
                toggles priority
        RETURNS
                Nothing
        */
        /**/
        public void togglePriority()
        {
            if (players[1].priority == players[0].priority)
                players[1].priority = !players[1].priority;
            players[0].priority = players[1].priority;
            players[1].priority = !(players[1].priority);
        }

        //AI FUNCTIONS
        /**/
        /*
        Game::compAttack() Game::compAttack()
        NAME
                Game::compAttack - computer chooses attackers
        DESCRIPTION
                computer goes through all the creatures it has and
                if the creature is able to attack, they attack
        RETURNS
                Nothing
        */
        /**/
        public void compAttack()
        {
            foreach (Card c in this.players[1].permanents)
            {
                if (c.sides[0].types.Contains("Creature") && !c.tapped && !c.sick)
                {
                    c.tapped = true;
                    this.attackingattackers.Push(c.instance);
                }
            }
        }

        /**/
        /*
        Game::compBlock() Game::compBlock()
        NAME
                Game::compBlock - same as attack
        DESCRIPTION
                if there are attackers, and the comp can block, it will assign the 
                first blocker to the first attacker and go from there
        RETURNS
                Nothing
        */
        /**/
        public void compBlock()
        {
            int i = 0;
            foreach (Card c in this.players[1].permanents)
            {
                if ((i < this.attackingattackers.Count) && c.sides[0].types.Contains("Creature") && !c.tapped)
                {
                    this.blockers.Push(c.instance);
                    this.attackers.Push(this.attackers.Push(this.attackingattackers[i]));
                    i++;
                }
            }
        }

        /**/
        /*
        Game::compPlayLands() Game::compPlayLands()
        NAME
                Game::discard - apply affects and resolve damage
        SYNOPSIS
               void compPlayLands(out string outcome)
                    outcome          --> string describing what the computer did
        DESCRIPTION
                plays the first land it sees in its hand. Sets outcome to describe
                what it did for display purposes
        RETURNS
                Nothing
        */
        /**/
        public void compPlayLands(out string outcome)
        {
            int i = 0;
            outcome = "";
            foreach (Card c in this.players[1].hand)
            {
                if (c.sides[0].types.Contains("Land"))
                {
                    this.Play(c);
                    outcome = "Comp plays " + c.names[0];
                    break;
                }
                i++;
            }
            if (outcome == "")
                outcome = "Computer has no lands in hand to play.";
        }

        /**/
        /*
        Game::compCrackLands() Game::compCrackLands()
        NAME
                Game::compCrackLands - apply affects and resolve damage
        SYNOPSIS
                void compCrackLands(out string outcome);
                    outcome          --> string describing what the computer did
        DESCRIPTION
                if the land has the name Evolving Wilds or Tar Pit, will activate the ability
                and resolve it
        RETURNS
                Nothing
        */
        /**/
        public void compCrackLands(out string outcome)
        {
            outcome = "";
            for (int i = 0; i < this.players[1].permanents.Count; i++)
            {
                if ((this.players[1].permanents[i].names[0] == "Evolving Wilds" || this.players[1].permanents[i].names[0] == "Rocky Tar Pit") && !this.players[1].permanents[i].tapped)
                {
                    Effect tmp = this.players[1].permanents[i].sides[this.players[1].permanents[i].side].effects[0];
                    this.targets.Push(this.players[1].permanents[i].instance);
                    outcome = "Comp cracks " + this.players[1].permanents[i].names[0];
                    this.sacrifice();
                    this.resolveEffect(tmp, 1);
                }
                i++;
            }
        }

        /**/
        /*
        Game::compPlayCreatures() Game::compPlayCreatures()
        NAME
                Game::compPlayCreatures - apply affects and resolve damage
        SYNOPSIS
                void compPlayCreatures(out string outcome);
                    outcome          --> string describing what the computer did
        DESCRIPTION
                Plays the first creature it sees and possibly can
        RETURNS
                Nothing
        */
        /**/
        public void compPlayCreatures(out string outcome)
        {
            int i = 0;
            outcome = "";
            foreach (Card c in this.players[1].hand)
            {
                List<int> lands;
                if (c.sides[0].types.Contains("Creature") && this.canPlay(c) && this.gotEnough(c.sides[0].manacost, out lands))
                {
                    foreach (int land in lands) { this.players[1].permanents[land].tapped = true; }
                    this.Play(c);
                    outcome = "Comp cast " + c.names[0];
                    break;
                }
                i++;
            }
        }

        /**/
        /*
        Game::compPlayNon() Game::compPlayNon()
        NAME
                Game::compPlayNon - apply affects and resolve damage
        SYNOPSIS
                void compPlayNon(out string outcome);
                    outcome          --> string describing what the computer did
        DESCRIPTION
                Plays the first noncreature it sees and possibly can
        RETURNS
                Nothing
        */
        /**/
        public void compPlayNon(out string outcome)
        {
            int i = 0;
            outcome = "";
            foreach (Card c in this.players[1].hand)
            {
                List<int> lands;
                if (!c.sides[0].types.Contains("Creature") && !c.sides[0].types.Contains("Land") && string.IsNullOrEmpty(c.sides[0].additionalCosts[0]) && this.canPlay(c) && this.gotEnough(c.sides[0].manacost, out lands))
                {
                    foreach (int land in lands) { this.players[1].permanents[land].tapped = true; }
                    this.Play(c);
                    outcome = "Comp cast " + c.names[0];
                    break;
                }
                i++;
            }
        }

        /**/
        /*
        Game::gotEnough() Game::gotEnough()
        NAME
                Game::gotEnough - sees if the comp has the mana required for a spell
        SYNOPSIS
                void Game::gotEnough(List<Mana> theCost, out List<int> lands);
                    theCost          --> the cost of the spell it wants to cast
                    lands           --> list of land indentifiers to be tapped for mana
        DESCRIPTION
               looks at all the mana it can produce from lands. it then sees if that 
               can satisfy the mana requirements. if so, lands is set to a list of 
               integers that match the location of lands to be tapped
        RETURNS
                Nothing
        */
        /**/
        public bool gotEnough(List<Mana> theCost, out List<int> lands)
        {
            List<int> tmp = new List<int>(), tracker = new List<int>();
            int i = 0;
            foreach (Card c in this.players[1].permanents)
            {
                if ((c.sides[0].types.Contains("Land")) && (!c.tapped))
                {
                    for (int k = 0; k < c.sides[c.side].effects.Count; k++)
                    {
                        if (c.sides[c.side].effects[k].effect == EffectType.ActivatedAbility && c.sides[c.side].effects[k].whatDo[2].ToLower() == "add")
                        {
                            tmp.Push(int.Parse(c.sides[c.side].effects[k].whatDo[5]));
                            tracker.Push(i);
                        }
                    }
                }
                i++;
            }
            i = 0;
            lands = new List<int>();
            if (theCost.Count <= tmp.Count)
            {
                foreach (int num in tmp)
                {
                    int index = theCost.FindIndex(x => x.color == num);
                    if (index > -1)
                    {
                        theCost.PopAt(index);
                        lands.Push(tracker.PopAt(i));
                    }
                    i++;
                }
                //if paying only contains generic, and has the same length of spending, we're good
                if (theCost.Count == 0)
                {
                    return true;
                }
                else if (!(theCost.Contains(new Mana(1, null))) && !(theCost.Contains(new Mana(2, null))) && !(theCost.Contains(new Mana(3, null))) && !(theCost.Contains(new Mana(4, null))) && !(theCost.Contains(new Mana(5, null))) && !(theCost.Contains(new Mana(6, null))) && tmp.Count == theCost.Count)
                {
                    int k = 0;
                    while (k < theCost.Count)
                    {
                        lands.Push(tracker.Pop());
                        k++;
                    }
                }
            }
            return false;
        }

        /**/
        /*
        Game::compChooseTargets() Game::compChooseTargets()
        NAME
                Game::compChooseTargets - assigns targets for the computer
        DESCRIPTION
               using targetType to know what to target, will push the first
               possible targets into the target list
        RETURNS
                Nothing
        */
        /**/
        private void compChooseTargets()
        {
            int i = 0;
            switch (this.targetType)
            {
                case "rp":
                    while (i < maxTargets && this.players[1].permanents.Count > i)
                    {
                        this.targets.Push(this.players[1].permanents[i].instance);
                        i++;
                    }
                    break;
                case "pl":
                    this.targets.Push(this.players[0].identifier);
                    break;
                case "cc":
                    this.targets.Push(this.players[1].graveyard.Find(x => x.sides[0].types.Contains("Creature")).instance);
                    break;
                case "nc":
                    while (i < maxTargets && this.players[1].permanents.Count > i)
                    {
                        if (this.players[1].permanents[i].sides[0].types.Contains("Creature") && this.players[1].permanents[i].sides[0].colors[0] != "Black")
                            this.targets.Push(this.players[1].permanents[i].instance);
                        i++;
                    }
                    break;
                case "dc":

                    Console.WriteLine("Comp choosing a devoid. Skynet is sentient");
                    break;
                case "cl":
                    while (i < maxTargets && this.players[0].permanents.Count > i)
                    {
                        if (this.players[1].permanents[i].sides[0].types.Contains("Creature") && this.players[1].permanents[i].sides[0].toughness == "1")
                            this.targets.Push(this.players[1].permanents[i].instance);
                        i++;
                    }
                    if (i == this.players[0].permanents.Count && i != maxTargets)
                        this.targets.Push(this.players[1].identifier);
                    break;
                case "rc":
                    while (i < maxTargets && this.players[1].permanents.Count > i)
                    {
                        if (this.players[1].permanents[i].sides[0].types.Contains("Creature"))
                            this.targets.Push(this.players[1].permanents[i].instance);
                        i++;
                    }
                    break;
                case "np":

                    Console.WriteLine("Comp choosing a noncreature. Skynet is sentient");
                    break;
                case "tc":
                    while (i < maxTargets && this.players[1].permanents.Count > i)
                    {
                        if (this.players[1].permanents[i].sides[0].types.Contains("Creature") && this.players[1].permanents[i].tapped)
                            this.targets.Push(this.players[1].permanents[i].instance);
                        i++;
                    }
                    break;
                case "sc":
                    while (i < maxTargets && this.players[1].permanents.Count > i)
                    {
                        if (this.players[1].permanents[i].sides[0].types.Contains("Creature") && this.players[1].permanents[i].sides[0].cmc <= 3)
                            this.targets.Push(this.players[1].permanents[i].instance);
                        i++;
                    }
                    break;
                case "fc":
                    while (i < maxTargets && this.players[1].permanents.Count > i)
                    {
                        if (this.players[1].permanents[i].sides[0].types.Contains("Creature") && this.players[1].permanents[i].sides[0].keywordAbilities.Contains("Flying"))
                            this.targets.Push(this.players[1].permanents[i].instance);
                        i++;
                    }
                    break;
                case "bl":
                    while (i < maxTargets && this.players[1].permanents.Count > i)
                    {
                        if (this.players[1].deck[i].sides[0].supertypes.Contains("Basic"))
                            this.targets.Push(this.players[1].deck[i].instance);
                        i++;
                    }
                    break;
                case "ms":
                    while (i < maxTargets && this.players[1].permanents.Count > i)
                    {
                        if (this.players[1].deck[i].sides[0].subtypes.Contains("Mountain") || this.players[1].deck[i].sides[0].subtypes.Contains("Swamp"))
                            this.targets.Push(this.players[1].deck[i].instance);
                        i++;
                    }
                    break;
            }
            finishEffect();
        }
    }

    class Interpreter
    {
        //rule 701
        List<string> keywordActions = new List<string>() { "activate", "add", "draw", "draws", "attach", "block", "cast", "destroy", "deals", "deal", "discard", "exchange", "exile", "fight", "play", "regenerate", "reveal", "sacrifice", "sacrifices", "search", "searches", "shuffles", "tap", "untap", "taps", "untaps", "scry", "return", "put", "puts", "loses", "gain", "gains", "becomes", "gets", "get" };
        List<string> keywordActionsSpells = new List<string>() { "activate", "add", "draw", "draws", "attach", "block", "counter", "destroy", "deals", "deal", "discard", "exchange", "exile", "fight", "play", "regenerate", "reveal", "sacrifice", "sacrifices", "search", "searches", "shuffles", "tap", "untap", "taps", "untaps", "scry", "return", "put", "puts", "loses", "gain", "gains", "becomes", "gets", "get" };

        //rule 702
        private List<string> keywordAbilities = new List<string>() { "Deathtouch", "Double Strike", "First Strike", "Flash", "Flying", "Haste", "Lifelink", "Reach", "Trample", "Vigilance", "Kicker" };

        /**/
        /*
        Game::parseSide() Game::parseSide()
        NAME
                Interpreter::parseSide - entry point for parsing the card text
        SYNOPSIS
                void parseSide(Card theCard);
                    theCard          --> the object we're parsing that holds the text
        DESCRIPTION
                If the card is a basic land, immedietly parses that since they're all the same

        RETURNS
                Nothing
        */
        /**/
        public void parseSide(Card theCard)
        {
            //parse cmc
            if (!string.IsNullOrEmpty(theCard.sides[0].manaCost))
                parseCMC(ref theCard);
            //it's a one shota and not a permanent
            if (theCard.sides[theCard.side].types[0] == "Instant" || theCard.sides[theCard.side].types[0] == "Sorcery")
            {
                parseOneshot(theCard.sides[theCard.side].text, ref theCard);
                return;
            }
            //all basic lands are the same except for the color it taps for
            if (!string.IsNullOrEmpty(theCard.sides[theCard.side].supertypes[0]) && theCard.sides[theCard.side].supertypes[0] == "Basic")
            {
                int effectNum = theCard.sides[theCard.side].effects.Count();
                theCard.sides[theCard.side].effects.Insert(effectNum, new Effect());
                theCard.sides[theCard.side].effects[effectNum].effect = EffectType.ActivatedAbility;
                theCard.sides[theCard.side].effects[effectNum].costs[0] = "Tap";
                theCard.sides[theCard.side].effects[effectNum].costs[1] = "~";
                theCard.sides[theCard.side].effects[effectNum].tapCost = true;
                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "opponent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "add";
                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "";
                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "";
                switch (theCard.sides[theCard.side].name)
                {
                    case "Plains":
                        theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "1";
                        break;
                    case "Island":
                        theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "2";
                        break;
                    case "Swamp":
                        theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "3";
                        break;
                    case "Mountain":
                        theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "4";
                        break;
                    case "Forest":
                        theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "5";
                        break;
                }
                return;
            }
            //its vanilla and nothing to parse
            if (string.IsNullOrEmpty(theCard.sides[theCard.side].text))
            {
                return;
            }
            //seperate lines
            string[] theText = (theCard.sides[theCard.side].text).Split('\n');

            //level up cards are like activated abilities, but with extra things attached to it
            if (theText[0].Split(' ')[0] == "Level")
            {
                parseLeveler(theText, ref theCard);
            }
            else
            {
                //parse Keyword Abilities
                parseKeyword(theText, ref theCard);

                //see if it enters tapped
                parseEnterTapped(theText, ref theCard);

                //Find triggers
                parseTriggered(theText, ref theCard);

                //find activated abilities
                parseActivated(theText, ref theCard);
            }
            //set summoning sickness to true
            if (theCard.sides[0].types.Contains("Creature"))
                theCard.sick = true;
        }

        /**/
        /*
        Game::parseEnterTapped() Game::parseEnterTapped()
        NAME
                Interpreter::parseEnterTapped - sees if the card enters tapped
        SYNOPSIS
                void parseEnterTapped(string[] text, ref Card theCard);
                    text          --> array of lines of textbox
                    theCard       --> the actual card to pasrse
        DESCRIPTION
                If there is "~ enters the battlefield tapped", sets tapped to true
                making it enter tapped when played later on
        RETURNS
                Nothing
        */
        /**/
        private void parseEnterTapped(string[] text, ref Card theCard)
        {
            //every single one is worded as such
            if (text[0] == theCard.sides[theCard.side].name + " enters the battlefield tapped.")
            {
                theCard.tapped = true;
            }
        }

        /**/
        /*
        Game::parseKeyword() Game::parseKeyword()
        NAME
                Interpreter::parseKeyword - sees if there are any keword abilities
        SYNOPSIS
                void parseKeyword(string[] text, ref Card theCard);
                    text          --> array of lines of textbox
                    theCard       --> the actual card to pasrse
        DESCRIPTION
                If there is a line that matches anything in the above array, puts that
                into the Side's keywordAbilities 
        RETURNS
                Nothing
        */
        /**/
        public void parseKeyword(string[] text, ref Card theCard)
        {
            Regex keywordsReg = new Regex(@"\b(" + string.Join("|", keywordAbilities.Select(Regex.Escape).ToArray()) + @"\b)");
            foreach (string line in text)
            {
                foreach (Match m in keywordsReg.Matches(line))
                {
                    if (m.Value[0] == 'K')
                    {
                        theCard.sides[theCard.side].optionalCosts[0] = "Kicker";
                        theCard.sides[theCard.side].optionalCosts[1] = parseManas(line.Split(' ')[1]);
                    }
                    else
                    {
                        theCard.sides[theCard.side].keywordAbilities.Push(m.Value);
                    }
                }
            }
        }


        /**/
        /*
        Game::parseLeveler() Game::parseLeveler()
        NAME
                Interpreter::parseLeveler - parses out leveling abilities
        SYNOPSIS
                void parseKeyword(string[] text, ref Card theCard);
                    text          --> array of lines of textbox
                    theCard       --> the actual card to pasrse
        DESCRIPTION
                It's an activated ability, but special; thus it gets its 
                own function
        RETURNS
                Nothing
        */
        /**/
        private void parseLeveler(string[] text, ref Card theCard)
        {
            int effectNum = 0;
            theCard.sides[theCard.side].effects.Insert(effectNum, new Effect());
            theCard.sides[theCard.side].effects[effectNum].effect = EffectType.LevelUp;
            parseCost(text[0].Split(' ')[2], ref theCard, effectNum);
            theCard.sides[theCard.side].effects[effectNum].when[0] = "sorcery";
            theCard.sides[theCard.side].effects[effectNum].when[1] = "";
            theCard.sides[theCard.side].levelUp = text;
            theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
            theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
            theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "put";
            theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "a";
            theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "";
            theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "level";
            theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "Level";
        }

        /**/
        /*
        Game::parseOneshot() Game::parseOneshot()
        NAME
                Interpreter::parseOneshot - parses the oneshot effects that aren't on permanents
        SYNOPSIS
                void parseKeyword(string[] text, ref Card theCard);
                    text          --> array of lines of textbox
                    theCard       --> the actual card to pasrse
        DESCRIPTION
                uses the regex to parse out abilities
        RETURNS
                Nothing
        */
        /**/
        private void parseOneshot(string text, ref Card theCard)
        {
            //create the effect
            int effectNum = theCard.sides[theCard.side].effects.Count();
            theCard.sides[theCard.side].effects.Insert(effectNum, new Effect());
            theCard.sides[theCard.side].effects[effectNum].effect = EffectType.OneShot;

            //This sees that the first words are which decides what kind of effect it is 
            Regex effectReg = new Regex(@"((Target|Each)\s(.+))?\s?\b(" + string.Join("|", keywordActionsSpells.Select(Regex.Escape).ToArray()) + @")\b\s(.+)", RegexOptions.IgnoreCase);

            //finds out what is targeted
            Regex identify = new Regex(@"(.+)?(creature|creatures|lands|land|permanent|permanents|" + theCard.sides[theCard.side].name + @"|opponent|oppenents)(.+)?", RegexOptions.IgnoreCase);

            //if its a modal spell, go parse that seperately
            if (text.Contains('•')) { parseModal(text, ref theCard, effectNum); Console.WriteLine("Parsin modal"); }
            //there's only one noncreature kicker card
            else if (text.Contains("Kicker"))
            {
                theCard.sides[theCard.side].optionalCosts[0] = "Kicker";
                theCard.sides[theCard.side].optionalCosts[1] = parseManas(text.Split(' ')[1]);
                //theCard.sides[theCard.side].optionalPaid = false;

                theCard.sides[theCard.side].effects[effectNum].targets[0] = "1";
                theCard.sides[theCard.side].effects[effectNum].targets[1] = "player";

                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "get";
                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "creatures";
                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "player";
                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "minus1 minus2";
                theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "Kicker";
            }
            //there's only on noncreature landfall card
            else if (text.Contains("Landfall"))
            {
                theCard.sides[theCard.side].optionalCosts[0] = "Kicker";
                theCard.sides[theCard.side].optionalCosts[1] = parseManas(text.Split(' ')[1]);
                //theCard.sides[theCard.side].optionalPaid = false;

                theCard.sides[theCard.side].effects[effectNum].targets[0] = "1";
                theCard.sides[theCard.side].effects[effectNum].targets[1] = "creature";

                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "get";
                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "creature";
                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "";
                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "plus2 plus4";
                theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "Landfall";
            }
            //parses out the additional cost
            else if (text.Contains("As an additional cost"))
            {
                Match carl = effectReg.Match(text.Split('\n')[0]);
                theCard.sides[theCard.side].additionalCosts[0] = carl.Groups[4].Value;
                theCard.sides[theCard.side].additionalCosts[1] = carl.Groups[5].Value;

                if (text.Split('\n')[1].Split(' ')[0] != "Target")
                {
                    theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                    theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                    theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "search";
                    theCard.sides[theCard.side].effects[effectNum].whatDo[3] = (theCard.controller == 0) ? "human" : "oppenent";
                    theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "untapped";
                    theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "two basic lands";
                }
                else
                {
                    theCard.sides[theCard.side].effects[effectNum].targets[0] = "1";
                    theCard.sides[theCard.side].effects[effectNum].targets[1] = "creature";

                    theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                    theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                    theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "gets";
                    theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "creature";
                    theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "";
                    theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "minusX";
                    theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "Revealed";
                }
            }
            //there's only one awaken card
            else if (text.Contains("Awaken"))
            {
                Match carl = effectReg.Match(text.Split('\n')[0]);
                theCard.sides[theCard.side].optionalCosts[0] = "Awaken";
                theCard.sides[theCard.side].optionalCosts[1] = parseManas(text.Split('\n')[1].Split('(')[0]);
                // theCard.sides[theCard.side].optionalPaid = false;

                theCard.sides[theCard.side].effects[effectNum].targets[0] = "1";
                theCard.sides[theCard.side].effects[effectNum].targets[1] = "creature";

                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "destroy";
                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "creature";
                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "tapped";
                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "destroy";
                theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "";

                effectNum++;
                theCard.sides[theCard.side].effects.Insert(effectNum, new Effect());
                theCard.sides[theCard.side].effects[effectNum].effect = EffectType.OneShot;

                theCard.sides[theCard.side].effects[effectNum].targets[0] = "1";
                theCard.sides[theCard.side].effects[effectNum].targets[1] = "land";

                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "put";
                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "land";
                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "";
                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "3 plus1 counters";
                theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "Awaken";
            }
            //it's forked bolt
            else if (text.Contains("among"))
            {
                theCard.sides[theCard.side].effects[effectNum].targets[0] = "~";
                theCard.sides[theCard.side].effects[effectNum].targets[1] = "1 2";

                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "deal";
                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "creature";
                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "player";
                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "2 divided";
                theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "Bolt";
            }
            //the card destroys
            else if (text.Split(' ')[0] == "Destroy")
            {
                if (text.Split(' ')[1] == "each")
                {
                    theCard.sides[theCard.side].effects[effectNum].whatDo[0] = "both";
                    theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                    theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "destroy";
                    theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "creatures";
                    theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "<3cmc";
                    theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "each";
                    theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "";
                }
                else
                {
                    if (text.Split(' ')[2] == "nonblack")
                    {
                        theCard.sides[theCard.side].effects[effectNum].targets[0] = "1";
                        theCard.sides[theCard.side].effects[effectNum].targets[1] = "nonblack";

                        theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "destroy";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "creature";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "nonblack";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "";

                        effectNum++;
                        theCard.sides[theCard.side].effects.Insert(effectNum, new Effect());
                        theCard.sides[theCard.side].effects[effectNum].effect = EffectType.OneShot;


                        theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "put";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[3] = (theCard.controller == 0) ? "human" : "oppenent";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "two 0/1";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "";
                    }
                    else
                    {
                        theCard.sides[theCard.side].effects[effectNum].targets[0] = "1";
                        theCard.sides[theCard.side].effects[effectNum].targets[1] = "<3cmc";

                        theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "destroy";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "creature";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "<3cmc";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "";
                    }
                }
            }
            //Read the Bones
            else if (text.Split(' ')[0] == "Scry")
            {
                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "scry";
                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "";
                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "2";
                theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "";

                effectNum++;
                theCard.sides[theCard.side].effects.Insert(effectNum, new Effect());
                theCard.sides[theCard.side].effects[effectNum].effect = EffectType.OneShot;

                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "draw";
                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "";
                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "2";
                theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "";

                effectNum++;
                theCard.sides[theCard.side].effects.Insert(effectNum, new Effect());
                theCard.sides[theCard.side].effects[effectNum].effect = EffectType.OneShot;

                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "lose";
                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "";
                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "2 life";
                theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "";
            }
            //only one card with tapping
            else if (text.Split(' ')[0] == "Tap")
            {
                theCard.sides[theCard.side].effects[effectNum].targets[0] = ">";
                theCard.sides[theCard.side].effects[effectNum].targets[1] = "2";

                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "tap";
                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "creatures";
                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "";
                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "";
                theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "";

                effectNum++;
                theCard.sides[theCard.side].effects.Insert(effectNum, new Effect());
                theCard.sides[theCard.side].effects[effectNum].effect = EffectType.OneShot;

                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "draw";
                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "";
                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "1";
                theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "";
            }
            //oust
            else if (text.Split(' ')[0] == "Put")
            {
                theCard.sides[theCard.side].effects[effectNum].targets[0] = "1";
                theCard.sides[theCard.side].effects[effectNum].targets[1] = "creature";

                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "put";
                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "creatures";
                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "";
                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "owners library 2nd from top.";
                theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "";

                effectNum++;
                theCard.sides[theCard.side].effects.Insert(effectNum, new Effect());
                theCard.sides[theCard.side].effects[effectNum].effect = EffectType.OneShot;

                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "gain";
                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "controller";
                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "";
                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "3 life";
                theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "";
            }
            //hellion eruption
            else if (text.Split(' ')[0] == "Sacrifice")
            {
                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "put";
                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "";
                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "";
                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "that many hellions";
                theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "";
            }
            else
            {
                Console.WriteLine(theCard.sides[theCard.side].name + " aint gettin' implmented");
            }
        }

        /**/
        /*
        Game::parseModal() Game::parseModal()
        NAME
                Interpreter::parseModal - parses modal abilities
        SYNOPSIS
                void parseModal(string[] text, ref Card theCard);
                    text          --> array of lines of textbox
                    theCard       --> the actual card to pasrse
                    effectNum     --> the number of the effect we're adding
        DESCRIPTION
                Seperates text by • and iterates through all of them, 
                giving each option its own effect that will later be resolved
        RETURNS
                Nothing
        */
        /**/
        private void parseModal(string text, ref Card theCard, int effectNum)
        {
            string[] splitUp = text.Split('•');

            theCard.sides[theCard.side].effects.Add(new Effect());
            theCard.sides[theCard.side].effects[0].modal = true;

            theCard.sides[theCard.side].effects[0].whatDo[0] = (theCard.controller == 0) ? "human" : "opponent";
            theCard.sides[theCard.side].effects[0].whatDo[1] = "no";
            theCard.sides[theCard.side].effects[0].whatDo[2] = "modal";
            theCard.sides[theCard.side].effects[0].whatDo[6] = "";

            //skip the first element since that is the choosing number
            for (int i = 1; i < splitUp.Length; i++)
            {
                string[] tmp = splitUp[i].Split(' ');
                theCard.sides[theCard.side].effects[0].whatDo[6] += splitUp[i] + "|";
                theCard.sides[theCard.side].effects.Add(new Effect());
                theCard.sides[theCard.side].effects[i].modal = false;
                theCard.sides[theCard.side].effects[i].whatDo[0] = (theCard.controller == 0) ? "human" : "opponent";
                theCard.sides[theCard.side].effects[i].whatDo[1] = "no";
                if (i == 3)
                {
                    theCard.sides[theCard.side].effects[i].whatDo[2] = "shuffle";
                    theCard.sides[theCard.side].effects[i].whatDo[5] = "graveyard";

                }
                else
                {

                    theCard.sides[theCard.side].effects[i].whatDo[2] = tmp[1];
                    theCard.sides[theCard.side].effects[i].whatDo[5] = "";
                    for (int k = 2; k < tmp.Length; k++)
                        theCard.sides[theCard.side].effects[i].whatDo[5] += tmp[k] + " ";
                }
            }
            theCard.sides[theCard.side].effects.RemoveAt(theCard.sides[theCard.side].effects.Count - 1);
        }

        /**/
        /*
        Game::parseModal() Game::parseModal()
        NAME
                Interpreter::parseActivated - parses modal abilities
        SYNOPSIS
                void parseActivated(string[] text, ref Card theCard);
                    text          --> array of lines of textbox
                    theCard       --> the actual card to pasrse
        DESCRIPTION
                Uses the activated ability regex to identify whether or not a creature has an activated ability
        RETURNS
                Nothing
        */
        /**/
        /*Rule 602.1: They can be expressed as “[cost(s)]: [conditional], [effect].”
        Regex results in:
            Group 0: Whole thing
            Group 1: Cost
            Group 2: Effect
        */
        private const string ACTIVATEDABILITY = @"(.+)\s?:\s?(.+)\.";
        private void parseActivated(string[] text, ref Card theCard)
        {
            foreach (string line in text)
            {
                //defend against false positives
                if (theCard.sides[theCard.side].name == "Emrakul's Hatcher" || theCard.sides[theCard.side].name == "Pawn of Ulamog")
                    return;
                //apply regex
                var testRegex = new Regex(ACTIVATEDABILITY, RegexOptions.IgnoreCase);
                MatchCollection AllMatches = testRegex.Matches(line);
                int effectNum = theCard.sides[theCard.side].effects.Count();
                //iterate through all the matches and give an effect to all found
                foreach (Match tom in AllMatches)
                {
                    theCard.sides[theCard.side].effects.Insert(effectNum, new Effect());
                    theCard.sides[theCard.side].effects[effectNum].effect = EffectType.ActivatedAbility;
                    parseCost(tom.Groups[1].Value, ref theCard, effectNum);
                    parsePermEffect(tom.Groups[2].Value, ref theCard, effectNum);
                    effectNum++;
                }
            }
        }

        /**/
        /*
        Game::parseTriggered() Game::parseTriggered()
        NAME
                Interpreter::parseTriggered - identifies trigger abilities
        SYNOPSIS
                void parseTriggered(string[] text, ref Card theCard);
                    text          --> array of lines of textbox
                    theCard       --> the actual card to pasrse
        DESCRIPTION
                Uses the trigger regex to identify whether or not a creature has an activated ability
        RETURNS
                Nothing
        */
        /**/
        /*They can also be expressed as  “[When/Whenever/At] [trigger event], [conditional], [effect].”
        Regex results in:
            Group 0: Whole thing
            Group 1: Trigger when
            Group 2: When, At, Whenever
            Group 3: Effect
        */
        private const string TRIGGEREDABILITY = @"((When|Whenever|At)[^,]*),\s?(.+)\.";
        private void parseTriggered(string[] text, ref Card theCard)
        {
            bool oneshot = false;
            foreach (string line in text)
            {
                //ingest is special
                if (line.Split(' ')[0] == "Ingest")
                {
                    int Num = theCard.sides[theCard.side].effects.Count();
                    theCard.sides[theCard.side].effects.Insert(Num, new Effect());
                    theCard.sides[theCard.side].effects[Num].when[0] = "damages";
                    theCard.sides[theCard.side].effects[Num].when[1] = "player";

                    theCard.sides[theCard.side].effects[Num].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                    theCard.sides[theCard.side].effects[Num].whatDo[1] = "no";
                    theCard.sides[theCard.side].effects[Num].whatDo[2] = "exile";
                    theCard.sides[theCard.side].effects[Num].whatDo[3] = (theCard.controller == 0) ? "human" : "oppenent";
                    theCard.sides[theCard.side].effects[Num].whatDo[4] = "";
                    theCard.sides[theCard.side].effects[Num].whatDo[5] = "topcard";
                    theCard.sides[theCard.side].effects[Num].whatDo[6] = "Ingest";
                    return;
                }
                //603.1
                //triggers always start with When/Whenever/At
                var testRegex = new Regex(TRIGGEREDABILITY);
                int effectNum = theCard.sides[theCard.side].effects.Count();
                foreach (Match currentMatch in testRegex.Matches(line))
                {
                    theCard.sides[theCard.side].effects.Insert(effectNum, new Effect());
                    if (theCard.sides[theCard.side].subtypes[0] == "Equipment")
                    {
                        //Console.WriteLine("equipment!");
                        theCard.sides[theCard.side].effects[effectNum].when[0] = "attacks";
                        theCard.sides[theCard.side].effects[effectNum].when[1] = "equipped";

                        theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "look";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[3] = (theCard.controller == 0) ? "human" : "oppenent";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "land";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "putonbattlefieldtapped";

                        oneshot = true;
                    }
                    else if (currentMatch.Groups[1].Value.Split(' ')[0] == "At")
                    {
                        //Console.WriteLine("DAT SCUTE");
                        theCard.sides[theCard.side].effects[effectNum].effect = EffectType.PhaseTrigger;
                        theCard.sides[theCard.side].effects[effectNum].when[0] = "beginning";
                        theCard.sides[theCard.side].effects[effectNum].when[1] = "1";

                        theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "put";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "~";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "control5lands";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "4 plus1";
                        oneshot = true;
                    }
                    else
                    {
                        theCard.sides[theCard.side].effects[effectNum].effect = EffectType.EventTrigger;

                        //deals with card changing zones (etb/dies/etc)
                        var zoneChangeRegex = new Regex(@"Whe[never]{1,5}\s" + theCard.names[theCard.side] + @"\s(.+).");
                        if (zoneChangeRegex.IsMatch(line))
                        {
                            foreach (Match thing in zoneChangeRegex.Matches(line))
                            {
                                if (line.Split(' ')[0] == "Whenever")
                                {
                                    //grab that last word before comma
                                    var orRegex = new Regex(@"or\sanother\s(.+)\s(.+),\s(.+)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                                    foreach (Match thing1 in orRegex.Matches(thing.Groups[1].Value))
                                    {
                                        //Butcher or pawn
                                        if (thing1.Groups[2].Value == "dies")
                                        {
                                            // Console.WriteLine("BUTCHER OR PAWN");
                                            if (theCard.names[theCard.side].Split(' ')[0] == "Butcher")
                                            {
                                                theCard.sides[theCard.side].effects[effectNum].when[0] = "dies";
                                                theCard.sides[theCard.side].effects[effectNum].when[1] = "~,creature";
                                            }
                                            else
                                            {
                                                theCard.sides[theCard.side].effects[effectNum].when[0] = "dies";
                                                theCard.sides[theCard.side].effects[effectNum].when[1] = "~,nontokencreature";
                                            }
                                        }
                                        else
                                        {
                                            // Console.WriteLine("One of those Ally guys");
                                            theCard.sides[theCard.side].effects[effectNum].when[0] = "etb";
                                            theCard.sides[theCard.side].effects[effectNum].when[1] = "~,ally";
                                        }
                                    }
                                }
                                else
                                {
                                    var deathVSetbRegex = new Regex(@"enters\sthe\sbattlefield,\s(.+)");
                                    if (deathVSetbRegex.IsMatch(thing.Groups[1].Value))
                                    {
                                        //Console.WriteLine("Its got a sick etb");
                                        theCard.sides[theCard.side].effects[effectNum].when[0] = "etb";
                                        theCard.sides[theCard.side].effects[effectNum].when[1] = "~";
                                    }
                                    else
                                    {
                                        theCard.sides[theCard.side].effects[effectNum].when[0] = "dies";
                                        theCard.sides[theCard.side].effects[effectNum].when[1] = "~,nontokencreature";
                                        //Console.WriteLine("Clicked on the Ruined Serviator thing");
                                    }
                                }
                            }
                        }
                        //first word is when, second word is you? Gotta be on cast
                        else if (line.Split(' ')[1] == "you")
                        {
                            if (line.Split(' ')[line.Split(' ').Count() - 1] == "battlefield")
                            {
                                theCard.sides[theCard.side].effects[effectNum].when[0] = "cast";
                                theCard.sides[theCard.side].effects[effectNum].when[1] = "~";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "return";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = (theCard.controller == 0) ? "human" : "oppenent";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "card";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "cc";
                            }
                            else
                            {
                                //Console.WriteLine("Big Daddy");
                                theCard.sides[theCard.side].effects[effectNum].when[0] = "cast";
                                theCard.sides[theCard.side].effects[effectNum].when[1] = "~";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "exile";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = (theCard.controller == 0) ? "oppenent" : "human";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "top four cards of his or her library, then you may put any number of land cards that player owns from exile onto the battlefield under your control.";
                            }
                            oneshot = true;
                        }
                        //landfall
                        else if (line.Split(' ')[0] == "Landfall")
                        {
                            // Console.WriteLine("LANDFALL HO!");
                            theCard.sides[theCard.side].effects[effectNum].when[0] = "etb";
                            theCard.sides[theCard.side].effects[effectNum].when[1] = "land";
                        }
                        //since Annihilator is the only effect in the deck that doesn't change other than the numPerm, just fill it all out here
                        else if (line.Split(' ')[0] == "Annihilator")
                        {
                            // Console.WriteLine("LANDFALL HO!");
                            theCard.sides[theCard.side].effects[effectNum].when[0] = "attacks";
                            theCard.sides[theCard.side].effects[effectNum].when[1] = "~";

                            theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "sacrifice";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "permanent";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[4] = (theCard.controller == 0) ? "human" : "oppenent";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[5] = line.Split(' ')[1];
                            theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "Annihilator";
                            oneshot = true;
                        }
                        else
                        {
                            // Console.WriteLine("IT BETREAYS US!");
                            theCard.sides[theCard.side].effects[effectNum].when[0] = "sacrifices";
                            theCard.sides[theCard.side].effects[effectNum].when[1] = "oppenentnontokenperm";
                        }
                    }
                    if (!oneshot)
                        parsePermEffect(currentMatch.Groups[3].Value, ref theCard, effectNum);
                    //Console.WriteLine("Trigger Group " + j + ": " + tom.Groups[1]);
                    effectNum++;
                    //Console.WriteLine("-------------------------------------------------");
                }
            }
            if (text[0].Split(' ')[text[0].Split(' ').Length - 1] == "—")
            {
                int effectNum = theCard.sides[theCard.side].effects.Count();
                theCard.sides[theCard.side].effects.Insert(effectNum, new Effect());
                string[] choices = theCard.sides[theCard.side].text.Split('\n');
                theCard.sides[theCard.side].effects[effectNum].when[0] = "etb";
                theCard.sides[theCard.side].effects[effectNum].when[1] = "land";
                theCard.sides[theCard.side].effects[effectNum].effect = EffectType.EventTrigger;
                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "choose";
                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "one";
                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "";
                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = choices[1] + "\n" + choices[2];
                theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "Landfall";
            }
        }

        /**/
        /*
        Game::parsePermEffect() Game::parsePermEffect()
        NAME
                Interpreter::parsePermEffect - parses effects from creatures and other permanents
        SYNOPSIS
                void parsePermEffect(string text, ref Card theCard, int effectNum)
                    text          --> array of lines of textbox
                    theCard       --> the actual card to pasrse
                    effectNum     --> the number of the effect we're parsing
        DESCRIPTION
                Parses out all the abilities that aren't oneshots
        RETURNS
                Nothing
        */
        /**/
        //Rule 609-611 "Sacrifice a creature: Bloodthrone Vampire gets +2/+2 until end of turn.";
        private const string CONDITIONALEFFECT = @"if\s([^,]*),\s?(.+)";
        private void parsePermEffect(string text, ref Card theCard, int effectNum)
        {
            var testRegex = new Regex(CONDITIONALEFFECT, RegexOptions.IgnoreCase);
            //if the effect has a conditional, i.e "if..."
            if (testRegex.IsMatch(text))
            {
                foreach (Match tom in testRegex.Matches(text))
                {
                    string[] tmp = tom.Groups[1].Value.Split(' ');
                    if (tmp[tmp.Length - 1] == "creature")
                    {
                        theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "loses";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[3] = (theCard.controller == 0) ? "human" : "oppenent";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "controlothercolorless";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "2 life";
                    }
                    else if (tmp[tmp.Length - 1] == "kicked")
                    {
                        if (tom.Groups[2].Value.Split(' ')[0] == "destroy")
                        {
                            theCard.sides[theCard.side].effects[effectNum].targets[0] = "1";
                            theCard.sides[theCard.side].effects[effectNum].targets[1] = "creature";

                            theCard.sides[theCard.side].optionalCosts[0] = "Kicker";
                            theCard.sides[theCard.side].optionalCosts[1] = "{2}{B}";

                            theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "destroy";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "creature";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "kicked";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "destroy";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "Kicker";
                        }
                        else
                        {
                            theCard.sides[theCard.side].effects[effectNum].targets[0] = "1";
                            theCard.sides[theCard.side].effects[effectNum].targets[1] = "creature";

                            theCard.sides[theCard.side].optionalCosts[0] = "Kicker";
                            theCard.sides[theCard.side].optionalCosts[1] = "{1}{R}";

                            theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "deals";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "creature";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "kicked";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "2";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "Kicker";
                        }
                    }
                    else if (tmp[0] == "you")
                    {
                        theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "draw";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[3] = (theCard.controller == 0) ? "human" : "oppenent";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "pay2";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "1";
                        theCard.sides[theCard.side].effects[effectNum].whatDo[6] = "Landfall";
                    }
                }
            }
            //it's not a conditional, its straightforward
            //parses like oneshot
            else
            {
                Regex effectReg = new Regex(@"((Target|Each|" + theCard.sides[theCard.side].name + @"|you|it)\s(.+)?)?\s?\b(" + string.Join("|", keywordActions.Select(Regex.Escape).ToArray()) + @")\b\s(.+)", RegexOptions.IgnoreCase);
                Regex identify = new Regex(@"(.+)?(creature|creatures|lands|land|permanent|permanents|" + theCard.sides[theCard.side].name + @"|opponent|oppenents)(.+)?", RegexOptions.IgnoreCase);
                if (effectReg.IsMatch(text))
                {
                    foreach (Match simon in effectReg.Matches(text))
                    {
                        if (simon.Groups[2].Value == theCard.sides[theCard.side].name || simon.Groups[2].Value == "it")
                        {
                            theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "~";//[3]
                            if (simon.Groups[4].Value.ToLower() == "deals")
                            {
                                //its magmaw
                                theCard.sides[theCard.side].effects[effectNum].targets[0] = "1";
                                theCard.sides[theCard.side].effects[effectNum].targets[1] = "creature|player";

                                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = simon.Groups[4].Value;
                                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "creature";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "1";
                            }
                            else if (simon.Groups[4].Value.ToLower() == "gains")
                            {
                                //its veteran warleader
                                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = simon.Groups[4].Value;
                                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "~";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "first strike|vigilance|trample";
                            }
                            else if (simon.Groups[4].Value.ToLower() == "gets")
                            {
                                //both bloodthrone and baloth, both get +2/+2 till end of turn
                                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = simon.Groups[4].Value;
                                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "~";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "plus2 end of turn.";
                            }
                            else if (simon.Groups[4].Value.ToLower() == "becomes")
                            {
                                //stirring wildwood
                                theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = simon.Groups[4].Value;
                                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "~";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = simon.Groups[5].Value;
                            }
                            else
                            {

                                Console.WriteLine(theCard.sides[theCard.side].name + " did not work well in name or it");
                            }
                        }
                        else if (simon.Groups[2].Value.ToLower() == "you")
                        {
                            theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                            string[] tmp = simon.Groups[3].Value.Split(' ');
                            int placeCounter = 0;
                            if (tmp[placeCounter] == "may")//[1]
                            {
                                placeCounter++;
                                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "yes";
                            }
                            else
                                theCard.sides[theCard.side].effects[effectNum].whatDo[1] = "no";

                            if (tmp[placeCounter] == "have")
                            {
                                placeCounter++;
                                if (tmp[placeCounter] == "target")
                                {
                                    placeCounter += 2;
                                    theCard.sides[theCard.side].effects[effectNum].targets[0] = "1";
                                    theCard.sides[theCard.side].effects[effectNum].targets[1] = "creature";
                                    theCard.sides[theCard.side].effects[effectNum].whatDo[2] = simon.Groups[4].Value;
                                    theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "creature";
                                    theCard.sides[theCard.side].effects[effectNum].whatDo[5] = simon.Groups[5].Value;
                                    //its a basilisk, target creature
                                }
                                else if (tmp[placeCounter] == "Tajuru")
                                {
                                    placeCounter += 2;
                                    theCard.sides[theCard.side].effects[effectNum].targets[0] = "1";
                                    theCard.sides[theCard.side].effects[effectNum].targets[1] = "creature";
                                    theCard.sides[theCard.side].effects[effectNum].whatDo[2] = simon.Groups[4].Value;
                                    theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "creature";
                                    theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "flying";
                                    theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "allies";
                                    //its tajuru archer
                                }
                                else
                                {
                                    placeCounter += 4;
                                    theCard.sides[theCard.side].effects[effectNum].whatDo[2] = simon.Groups[4].Value;
                                    theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "allies";
                                    theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "you control";
                                    theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "vigilence";
                                    //its joraga bard, ally creatures you control
                                }
                            }
                            else
                            {
                                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = simon.Groups[4].Value;
                                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = (theCard.controller == 0) ? "human" : "oppenent";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = simon.Groups[5].Value;
                                //find out who falls into here  
                            }
                        }
                        else if (simon.Groups[2].Value.ToLower() == "target" || simon.Groups[2].Value == "each")
                        {
                            if (simon.Groups[5].Value == "3 life")
                            {
                                //bloodrite invoker
                                theCard.sides[theCard.side].effects[effectNum].targets[0] = "1";
                                theCard.sides[theCard.side].effects[effectNum].targets[1] = "player";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "loses";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = "player";
                                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "3life;othergain3life";
                            }
                            else
                            {
                                Match kyle = identify.Match(simon.Groups[3].Value);
                                if (kyle.Groups[1].Value != " " && kyle.Groups[1].Value != "")
                                {
                                    theCard.sides[theCard.side].effects[effectNum].whatDo[4] = kyle.Groups[1].Value;
                                }
                                theCard.sides[theCard.side].effects[effectNum].whatDo[3] = kyle.Groups[2].Value;
                                if (kyle.Groups[3].Value != " " && kyle.Groups[3].Value != "")
                                {
                                    theCard.sides[theCard.side].effects[effectNum].whatDo[4] += "|" + kyle.Groups[3].Value;//[4]
                                    if (kyle.Groups[1].Value != " " && kyle.Groups[1].Value != "")
                                    {

                                        Console.WriteLine(theCard.sides[theCard.side].name + " is wierd");
                                    }
                                }
                                theCard.sides[theCard.side].effects[effectNum].whatDo[2] = simon.Groups[4].Value;//[2]
                                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = simon.Groups[5].Value;//[5]
                            }
                        }
                        else if (simon.Groups[4].Value.ToLower() == "add")
                        {
                            theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "add";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[3] = (theCard.controller == 0) ? "human" : "oppenent";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[5] = parseManas(simon.Groups[5].Value);
                            if (theCard.sides[theCard.side].effects[effectNum].whatDo[5].Length == 2 && theCard.sides[theCard.side].effects[effectNum].whatDo[5][0] != '6')
                            {
                                Effect tmp = theCard.sides[theCard.side].effects[effectNum];
                                int tmpNum = effectNum + 1;
                                theCard.sides[theCard.side].effects.Insert(tmpNum, new Effect(tmp));
                                theCard.sides[theCard.side].effects[tmpNum].whatDo[5] = theCard.sides[theCard.side].effects[effectNum].whatDo[5][1].ToString();
                                theCard.sides[theCard.side].effects[effectNum].whatDo[5] = theCard.sides[theCard.side].effects[effectNum].whatDo[5][0].ToString();
                            }
                            else if (theCard.sides[theCard.side].effects[effectNum].whatDo[5].Length == 2)
                            {
                                theCard.sides[theCard.side].effects[effectNum].whatDo[4] = "Eldrazi";
                            }
                        }
                        else if (simon.Groups[4].Value == "Draw")
                        {
                            theCard.sides[theCard.side].effects[effectNum].whatDo[0] = (theCard.controller == 0) ? "human" : "oppenent";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "no";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[2] = "draw";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[3] = (theCard.controller == 0) ? "human" : "oppenent";
                            theCard.sides[theCard.side].effects[effectNum].whatDo[5] = "1";
                        }
                        else if (simon.Groups[2].Value == "")
                        {
                            theCard.sides[theCard.side].effects[effectNum].whatDo[5] = simon.Groups[5].Value;//[5]
                            theCard.sides[theCard.side].effects[effectNum].whatDo[2] = simon.Groups[4].Value;//[2]
                        }
                        else
                        {

                            Console.WriteLine(theCard.sides[theCard.side].name + " didn't fall into anything");
                        }
                    }
                }
                else
                {

                    Console.WriteLine(theCard.sides[theCard.side].name + " aint gettin' implmented");
                }
            }
        }

        /**/
        /*
        Game::parseCost() Game::parseCost()
        NAME
                Interpreter::parseCost - parses out the mana cost
        SYNOPSIS
                void parsePermEffect(string text, ref Card theCard, int effectNum)
                    text          --> array of lines of textbox
                    theCard       --> the actual card to pasrse
                    effectNum     --> the number of the effect we're parsing
        DESCRIPTION
                Uses the mana costs to parse out costs,
                if its not a mana cost, then its a special cost like Magmaw
                stores in theCard
        RETURNS
                Nothing
        */
        /**/
        private const string MANACOSTS = @"(\{.+\})+";
        private const string AWORD = @"(\w+)";
        private void parseCost(string text, ref Card theCard, int effectNum)
        {
            string[] tmp = text.Split(' ');
            if (tmp[0] == "Equip") { theCard.sides[theCard.side].effects[effectNum].manaCost.Add(new Mana(0, "1")); return; }
            string[] heywhats = text.Split(',');
            var testRegex = new Regex(MANACOSTS, RegexOptions.IgnoreCase);
            foreach (string tom in heywhats)
            {
                if ((testRegex.Match(tom)).Success) { parseManaCost(tom, ref theCard, effectNum); }
                else
                {
                    string tmpTom = tom;
                    var aword = new Regex(AWORD);
                    if (tmpTom[0] == ' ')
                        tmpTom = tmpTom.Substring(1, tmpTom.Length - 1);
                    string[] costsSplit = tmpTom.Split(' ');
                    string withoutFirst = "";
                    for (int i = 1; i < costsSplit.Length; i++)
                    {
                        withoutFirst += costsSplit[i] + " ";
                    }
                    withoutFirst = withoutFirst.Substring(0, withoutFirst.Length - 1);
                    theCard.sides[theCard.side].effects[effectNum].costs[0] = costsSplit[0];
                    if (withoutFirst == theCard.names[0])
                        withoutFirst = "~";
                    theCard.sides[theCard.side].effects[effectNum].costs[1] = withoutFirst;
                }
            }
        }

        /**/
        /*
        Game::parseManaCost() Game::parseManaCost()
        NAME
                Interpreter::parseManaCost - parses out the actual manas 
        SYNOPSIS
                void parseManaCost(string text, ref Card theCard, int effectNum);
                    text          --> array of lines of textbox
                    theCard       --> the actual card to pasrse
                    effectNum     --> the number of the effect we're parsing
        DESCRIPTION
               Parses out the mana cost and tapcosts
        RETURNS
                Nothing
        */
        /**/
        private const string MANACOLORS = @"\{(\d\d?|\w)\}";
        private void parseManaCost(string text, ref Card theCard, int effectNum)
        {
            var testRegex = new Regex(MANACOLORS, RegexOptions.IgnoreCase);
            foreach (Match tom in testRegex.Matches(text))
            {
                foreach (Group g in tom.Groups)
                {
                    if (g.Value.Length == 1)
                    {
                        int attempt;
                        if (g.Value == "T")
                        {
                            theCard.sides[theCard.side].effects[effectNum].tapCost = true;
                        }
                        else if (int.TryParse(g.Value, out attempt))
                        {
                            for (int i = 0; i < int.Parse(g.Value); i++)
                                theCard.sides[theCard.side].effects[effectNum].manaCost.Add(new Mana(0, ""));
                        }
                        else
                        {
                            switch (g.Value[0])
                            {
                                case 'W':
                                    theCard.sides[theCard.side].effects[effectNum].manaCost.Add(new Mana(1, null));
                                    break;
                                case 'U':
                                    theCard.sides[theCard.side].effects[effectNum].manaCost.Add(new Mana(2, null));
                                    break;
                                case 'B':
                                    theCard.sides[theCard.side].effects[effectNum].manaCost.Add(new Mana(3, null));
                                    break;
                                case 'R':
                                    theCard.sides[theCard.side].effects[effectNum].manaCost.Add(new Mana(4, null));
                                    break;
                                case 'G':
                                    theCard.sides[theCard.side].effects[effectNum].manaCost.Add(new Mana(5, null));
                                    break;
                            }
                        }
                    }
                }
            }
        }

        /**/
        /*
        Game::parseManaCost() Game::parseManaCost()
        NAME
                Interpreter::parseManaCost - parses out the actual manas 
        SYNOPSIS
                void parseManaCost(string text, ref Card theCard, int effectNum);
                    text          --> string of mana, i.e "{B}{4}"
        DESCRIPTION
               Creates a string of numbers to represent mana
        RETURNS
                returns a string to represent the mana, i.e "{3}{0}{0}{0}{0}"
        */
        /**/
        private string parseManas(string text)
        {
            var testRegex = new Regex(MANACOLORS, RegexOptions.IgnoreCase);
            string total = "";
            foreach (Match tom in testRegex.Matches(text))
            {
                foreach (Group g in tom.Groups)
                {
                    if (g.Value.Length == 1)
                    {
                        int attempt;
                        if (int.TryParse(g.Value, out attempt))
                        {
                            total += "0";
                        }
                        switch (g.Value[0])
                        {
                            case 'C':
                                total += "6";
                                break;
                            case 'W':
                                total += "1";
                                break;
                            case 'U':
                                total += "2";
                                break;
                            case 'B':
                                total += "3";
                                break;
                            case 'R':
                                total += "4";
                                break;
                            case 'G':
                                total += "5";
                                break;
                        }
                    }
                }
            }
            return total;
        }

        /**/
        /*
        Game::getCost() Game::getCost()
        NAME
                Interpreter::getCost - parses out the manas 
        SYNOPSIS
                void getCost(string text)
                    text          --> string of mana, i.e "{3}{4}"
        DESCRIPTION
               Creates a list of manas from the string
        RETURNS
                returns a list of actual mana objects
        */
        /**/
        public List<Mana> getCost(string text)
        {
            var testRegex = new Regex(MANACOLORS, RegexOptions.IgnoreCase);
            List<Mana> theReturn = new List<Mana>();
            foreach (Match tom in testRegex.Matches(text))
            {
                foreach (Group g in tom.Groups)
                {
                    if (!g.Value.Contains("{"))
                    {
                        int attempt;
                        if (int.TryParse(g.Value, out attempt))
                        {
                            for (int i = 0; i < int.Parse(g.Value); i++)
                                theReturn.Add(new Mana(0, ""));
                        }
                        else
                        {
                            switch (g.Value[0])
                            {
                                case 'W':
                                    theReturn.Add(new Mana(1, null));
                                    break;
                                case 'U':
                                    theReturn.Add(new Mana(2, null));
                                    break;
                                case 'B':
                                    theReturn.Add(new Mana(3, null));
                                    break;
                                case 'R':
                                    theReturn.Add(new Mana(4, null));
                                    break;
                                case 'G':
                                    theReturn.Add(new Mana(5, null));
                                    break;
                            }
                        }
                    }
                }
            }
            return theReturn;
        }

        /**/
        /*
        Game::parseCMC() Game::parseCMC()
        NAME
                Interpreter::parseCMC - parses out the cmc
        SYNOPSIS
                void parseCMC(ref Card theCard);
                    theCard       --> the actual card to pasrse
        DESCRIPTION
                Parses out the cmc and stores it in the card
        RETURNS
                Nothing
        */
        /**/
        private void parseCMC(ref Card theCard)
        {
            var testRegex = new Regex(MANACOLORS, RegexOptions.IgnoreCase);
            foreach (Match tom in testRegex.Matches(theCard.sides[0].manaCost))
            {
                foreach (Group g in tom.Groups)
                {
                    if (!g.Value.Contains("{"))
                    {
                        int attempt;
                        if (int.TryParse(g.Value, out attempt))
                        {
                            for (int i = 0; i < int.Parse(g.Value); i++)
                                theCard.sides[theCard.side].manacost.Add(new Mana(0, ""));
                        }
                        else
                        {
                            switch (g.Value[0])
                            {
                                case 'W':
                                    theCard.sides[theCard.side].manacost.Add(new Mana(1, null));
                                    break;
                                case 'U':
                                    theCard.sides[theCard.side].manacost.Add(new Mana(2, null));
                                    break;
                                case 'B':
                                    theCard.sides[theCard.side].manacost.Add(new Mana(3, null));
                                    break;
                                case 'R':
                                    theCard.sides[theCard.side].manacost.Add(new Mana(4, null));
                                    break;
                                case 'G':
                                    theCard.sides[theCard.side].manacost.Add(new Mana(5, null));
                                    break;
                            }
                        }
                    }
                }
            }
        }

    }
}
