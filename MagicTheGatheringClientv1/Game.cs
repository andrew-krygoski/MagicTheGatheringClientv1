using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicTheGatheringClientv1
{
    class Game
    {
        private int phase;
        private Player[] players;
        private Card[] stack;
        private string[,] triggers;
        private Random rand = new Random();
        private int aPlayer;
        private int nPlayer;

        public Game(bool Zendikar)
        {
            aPlayer = rand.Next(0, 2);
            nPlayer = ((aPlayer == 1) ? (0) : (1));
            //players[0] = new Player(Zendikar);
            //players[1] = new Player(!Zendikar);
            phase = 0;
            players[aPlayer].active = true;
            players[nPlayer].active = false;
        }

        private void executeTurn()
        {
            untapPhase();
            upkeepPhase();
            drawPhase();
            mainPhase();
            beginComPhase();
            declareBlPhase();
            damagePhase();
            endComPhase();
            endPhase();
            cleanupPhase();
        }

        private void untapPhase()
        { }

        private void upkeepPhase()
        { }

        private void drawPhase()
        { }

        private void mainPhase()
        { }

        private void beginComPhase()
        { }

        private void declareBlPhase()
        { }

        private void damagePhase()
        { }

        private void endComPhase()
        { }

        private void endPhase()
        { }

        private void cleanupPhase()
        { }

        private void togglePriority() { players[0].priority = !(players[0].priority); players[1].priority = !(players[1].priority); }


    }
}
