using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace MagicTheGatheringClientv1
{
    public partial class GameBoard : Form
    {
        private Game CurrentGame;
        int help = 0;
        private bool executing = false, firsttime = true, paying;
        private string[] phaseArray = { "Untap", "Upkeep", "Draw", "Main Phase I", "Begin Combat", "Declare Attackers", "Declare Blockers", "Damage", "End Combat", "Main Phase II", "End Step", "Cleanup" };
        private Rectangle PlayerScoreBoard;
        private List<Rectangle> Player1Manas, Player1Perms, Player2Perms, Player1Hand, Player2Grave, Player2Exile, Player1Grave, Player1Exile, Player1Deck, theStack;
        private List<TextBox> Player1ManaText;
        private Rectangle PayingForRec, tmpCardChoiceRec;
        private List<Button> tmpChoices;
        private int tmpActivatedInt, tmpPayingInt, tmpPayingActivatedInt, tmpCardChoiceInt;
        private char tmpCardZone, tmpPayingZone;

        /**/
        /*
        GameBoard::GameBoard() GameBoard::GameBoard()
        NAME
                GameBoard::GameBoard - initiates the game UI
        SYNOPSIS
                public GameBoard::GameBoard( bool zendi );
                    zendi          --> boolean deciding if the player has the zendikar deck
        DESCRIPTION
                creates the game and the UI
        RETURNS
                nothing
        */
        /**/
        public GameBoard(bool zendi)
        {
            InitializeComponent();
            this.Location = new Point(0, 0);
            this.Size = Screen.PrimaryScreen.WorkingArea.Size;

            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            priorityButton.Enabled = false;
            theStackBox.BackgroundImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
            this.tmpChoices = new List<Button>();
            Player1ManaText = new List<TextBox>();
            this.paying = false;
            this.theRecord.Text = "";

            this.CurrentGame = new Game(!zendi);
            phaseDisplay.Text = this.phaseArray[this.CurrentGame.phase];
            priorityButton.Enabled = true;
            Invalidate(true);
        }

        /**/
        /*
        GameBoard::GameBoard_Resize() GameBoard::GameBoard_Resize()
        NAME
                GameBoard::GameBoard_Resize - handles the resize event
        DESCRIPTION
                dynamically resizes all controls based on the new width and height
        RETURNS
                nothing
        */
        /**/
        private void GameBoard_Resize(object sender, System.EventArgs e)
        {
            this.executing = true;
            this.PlayerScoreBoard.Location = new Point((int)0, (int)(.8 * this.Size.Height));
            this.PlayerScoreBoard.Size = new Size((int)(.2 * this.Size.Width), (int)(.2 * this.Size.Height));
            this.Player2Battlefield.Location = new Point((int)(.2 * this.Size.Width), 0);
            this.Player2Battlefield.Size = new Size((int)(.5 * this.Size.Width), (int)(.4 * this.Size.Height));
            this.Player1Battlefield.Location = new Point((int)(.2 * this.Size.Width), (int)(.4 * this.Size.Height));
            this.Player1Battlefield.Size = new Size((int)(.5 * this.Size.Width), (int)(.4 * this.Size.Height));
            this.Player1HandArea.Location = new Point((int)(.2 * this.Size.Width), (int)(.8 * this.Size.Height));
            this.Player1HandArea.Size = new Size((int)(.5 * this.Size.Width), (int)(.2 * this.Size.Height));
            this.Player2Graveyard.Location = new Point((int)(.7 * this.Size.Width), (int)(.2 * this.Size.Height));
            this.Player2Graveyard.Size = new Size((int)(.15 * this.Size.Width), (int)(.4 * this.Size.Height));
            this.Player2ExileZone.Location = new Point((int)(.85 * this.Size.Width), (int)(0 * this.Size.Height));
            this.Player2ExileZone.Size = new Size((int)(.15 * this.Size.Width), (int)(.5 * this.Size.Height));
            this.Player1Graveyard.Location = new Point((int)(.7 * this.Size.Width), (int)(.6 * this.Size.Height));
            this.Player1Graveyard.Size = new Size((int)(.15 * this.Size.Width), (int)(.4 * this.Size.Height));
            this.Player1ExileZone.Location = new Point((int)(.85 * this.Size.Width), (int)(.5 * this.Size.Height));
            this.Player1ExileZone.Size = new Size((int)(.15 * this.Size.Width), (int)(.5 * this.Size.Height));
            this.PayingForRec.Location = new Point((int)(.7 * this.Size.Width), (int)(0 * this.Size.Height));
            this.PayingForRec.Size = new Size((int)(.15 * this.Size.Width), (int)(.2 * this.Size.Height));
            this.lifeText.Location = new Point((int)(0), (int)(.8 * this.Size.Height));
            this.compLifeText.Location = new Point((int)(0), (int)(this.lifeText.Location.Y + 25));
            this.humanLifeText.Location = new Point((int)(0), (int)(this.lifeText.Location.Y + 50));
            this.theRecord.Size = new Size((int)(.2 * this.Size.Width), (int)(.2 * this.Size.Height));
            this.quitButton.Location = new Point((int)(0), 0);
            this.mulliganButton.Location = new Point((int)0, quitButton.Height + 5);
            this.priorityButton.Location = new Point((int)0, mulliganButton.Location.Y + mulliganButton.Height + 5);
            this.passTurnButton.Location = new Point(this.priorityButton.Location.X + this.priorityButton.Width + 5, this.priorityButton.Location.Y);
            this.messageBox.Location = new Point((int)0, priorityButton.Location.Y + priorityButton.Height + 5);
            this.gamestate.Location = new Point((int)0, messageBox.Location.Y + messageBox.Height + 5);
            this.theRecord.Location = new Point((int)0, gamestate.Location.Y + gamestate.Height + 5);
            this.phaseDisplay.Location = new Point((int)0, theRecord.Location.Y + theRecord.Height + 5);
            this.manaPoolId.Location = new Point((int)15, phaseDisplay.Location.Y + phaseDisplay.Height + 5);
            this.Player1ManaPool.Location = new Point((int)15, manaPoolId.Location.Y + manaPoolId.Height + 5);
            this.targetCompButton.Location = new Point((int)this.mulliganButton.Location.X + this.mulliganButton.Width, this.mulliganButton.Location.Y);
            this.Player1ManaPool.Size = new Size((int)(.15 * this.Size.Width), (int)(.1 * this.Size.Height));
            this.SpellCostLeft.Location = new Point(this.Player1ManaPool.Location.X, this.Player1ManaPool.Height + this.Player1ManaPool.Location.Y + 5);
            this.SpellCostLeft.Size = this.Player1ManaPool.Size;
            this.choicesLaidBox.Location = new Point((int)(.2 * this.Size.Width), 0);
            this.choicesLaidBox.Size = new Size((int)(this.Player2Battlefield.Width), (int)(this.Player2Battlefield.Height + this.Player1Battlefield.Height));
            this.theStackBox.Location = new Point((int)(this.Player2ExileZone.Location.X), this.Player2ExileZone.Location.Y);
            this.theStackBox.Size = new Size((int)(this.Player2ExileZone.Width + this.Player1ExileZone.Width), (int)(this.Player2ExileZone.Height + this.Player1ExileZone.Height));
            //this.leftTray.Location = new Point(0, 0);
            //this.leftTray.Size = new Size((int)(this.Size.Width * .2), this.Size.Height);
            this.executing = false;
            Invalidate(true);
        }

        /**/
        /*
        GameBoard::GameBoard_Paint() GameBoard::GameBoard_Paint()
        NAME
                GameBoard::GameBoard_Paint - kicks off DrawGamestate
        DESCRIPTION
                uses a bool to make sure I don't start drawing things while I am already
                drawing things
        RETURNS
                nothing
        */
        /**/
        private void GameBoard_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (!executing)
            {
                if((this.CurrentGame != null) && ((this.CurrentGame.gameOver) || this.CurrentGame.players[0].life < 1 || this.CurrentGame.players[1].life < 1))
                {
                    Form1 over = new Form1(null);
                    if (this.CurrentGame.players[0].deck.Count == 0)
                    {
                        over.Close();
                        over = new Form1("The Player had no cards left to draw. The Comp wins!");
                    }
                    else if (this.CurrentGame.players[1].deck.Count == 0)
                    {
                        over.Close();
                        over = new Form1("The Comp had no cards left to draw. The Player wins!");
                    }
                    else if (this.CurrentGame.players[0].life < 1)
                    {
                        over.Close();
                        over = new Form1("The Player's had life has been depleted. The Comp wins!");
                    }
                    else if (this.CurrentGame.players[1].life < 1)
                    {
                        over.Close();
                        over = new Form1("The Comp's had life has been depleted. The Player wins!");
                    }
                    this.Hide();
                    over.ShowDialog();
                    this.Close();
                }
                executing = true;
                Graphics g = e.Graphics;
                g.Clear(this.BackColor);
                try
                {
                    this.drawGamestate(g);
                    //this.SixtyCardPickup(g);
                }
                catch (FormatException)
                {
                    return;
                }
                executing = false;
            }
        }

        /**/
        /*
        GameBoard::drawGamestate() GameBoard::drawGamestate()
        NAME
                GameBoard::drawGamestate - handles the display of control text
        DESCRIPTION
                Uses facts about the gamestates to enable/disable buttons and
                set text to correctly display what is happening to user
        RETURNS
                nothing
        */
        /**/
        private void drawGamestate(Graphics g)
        {
            if (CurrentGame != null)
            {
                this.theStackBox.Enabled = false;
                this.theStackBox.Visible = false;
                this.Player1ExileZone.Visible = true;
                this.Player2ExileZone.Visible = true;
                phaseDisplay.Text = this.phaseArray[this.CurrentGame.phase];
                if (this.CurrentGame.mulling)
                {
                    this.priorityButton.Text = "Start Playing";
                    this.priorityButton.Enabled = true;
                    this.mulliganButton.Enabled = true;
                    this.messageBox.Text = "Do you want to mulligan down to " + (this.CurrentGame.players[0].hand.Count - 1);
                    messageBox.Size = TextRenderer.MeasureText(messageBox.Text, messageBox.Font);
                    this.passTurnButton.Enabled = false;
                    return;
                }
                else
                {
                    this.mulliganButton.Enabled = false;
                    this.mulliganButton.Visible = false;

                }

                if (this.CurrentGame.scrying || this.CurrentGame.optionalEC || this.CurrentGame.searching || ((this.CurrentGame.resolving) && (this.CurrentGame.toBe[0].whatDo[2].ToLower() == "search") && (this.CurrentGame.targetType == "bl" || this.CurrentGame.targetType == "ms" || this.CurrentGame.targetType == "cc")))
                {
                    this.choicesLaidBox.Enabled = true;
                    this.choicesLaidBox.Visible = true;
                }
                else
                {
                    this.choicesLaidBox.Enabled = false;
                    this.choicesLaidBox.Visible = false;
                }

                this.humanLifeText.Text = "Human: " + this.CurrentGame.players[0].life;
                this.compLifeText.Text = "Compu: " + this.CurrentGame.players[1].life;
                if (this.CurrentGame.skippedAt)
                {
                    this.theRecord.AppendText("There were no legal attackers.\n");
                    this.CurrentGame.skippedAt = false;
                }
                else if (this.CurrentGame.skippedBl)
                {
                    this.theRecord.AppendText("There were no legal blockers.\n");
                    this.CurrentGame.skippedBl = false;
                }
                this.gamestate.Text = "";
                this.gamestate.AppendText("Active Player: " + ((this.CurrentGame.actPlayer == 0) ? "human\n" : "comp\n"));
                this.gamestate.AppendText("Priority: " + ((this.CurrentGame.players[0].priority) ? "human\n" : "comp\n"));
                this.gamestate.AppendText("Comp Hand Size: " + this.CurrentGame.players[1].hand.Count() + "\n");
                this.gamestate.AppendText("Actions to be taken: " + ((this.CurrentGame.canLevel()) ? "anything!\n" : "only instant-speed!\n"));

                if (this.CurrentGame.stack.Count != 0)
                {
                    this.priorityButton.Enabled = true;
                    this.priorityButton.Text = "Pass Priority";
                    this.messageBox.Text = "Pass Priority to Resolve Spell";
                    this.theStackBox.Enabled = true;
                    this.theStackBox.Visible = true;
                    this.Player1ExileZone.Visible = false;
                    this.Player2ExileZone.Visible = false;
                    this.passTurnButton.Enabled = false;
                }
                else if (this.CurrentGame.payAdd)
                {
                    this.priorityButton.Enabled = true;
                    this.priorityButton.Text = "Cancel";
                    this.messageBox.Text = "Please select a " + ((this.CurrentGame.players[0].hand[tmpCardChoiceInt].sides[0].additionalCosts[0].ToLower() == "sacrifice") ? " land to sacrifice." : " a creature card to reveal.");
                }
                else if (this.CurrentGame.resolving && firsttime)
                {
                    if (this.CurrentGame.minTargets > 0)
                    {
                        messageBox.Text = "Please choose " + (this.CurrentGame.maxTargets - this.CurrentGame.targets.Count) + " targets.";
                        if (this.CurrentGame.minTargets == this.CurrentGame.maxTargets) { this.priorityButton.Enabled = false; }
                    }
                    else if (this.CurrentGame.scrying)
                    {
                        this.priorityButton.Text = "Done";
                        this.priorityButton.Enabled = true;
                        messageBox.Text = "Please choose " + (this.CurrentGame.maxTargets - this.CurrentGame.targets.Count) + " cards to go to bottom.";
                    }
                    else
                        messageBox.Text = "Currently Resolving";

                    if ((this.CurrentGame.toBe[0].whatDo[2].ToLower() == "search") && (this.CurrentGame.targetType == "bl" || this.CurrentGame.targetType == "ms" || this.CurrentGame.targetType == "cc"))
                    {
                        this.priorityButton.Enabled = true;
                        this.priorityButton.Text = "Fail to find rest.";
                    }
                    else if (this.CurrentGame.targetType == "cl" && firsttime)
                    {
                        firsttime = false;
                        this.targetCompButton.Enabled = true;
                        this.targetCompButton.Visible = true;
                    }
                }
                else if (paying)
                {
                    this.priorityButton.Enabled = true;
                    this.priorityButton.Text = "Cancel";
                    messageBox.Text = "Pay for it now.";
                    SolidBrush drawBrush = new SolidBrush(Color.Black);
                    Font drawFont = new Font("Arial", 16);
                    g.DrawString("Paying for:", drawFont, drawBrush, PayingForRec.Location);
                    Image theCard = Image.FromFile(@"Images\greenfelt1.png");
                    if (tmpPayingZone == 'H')
                        theCard = Image.FromFile(@"Images\Cards\" + this.CurrentGame.players[0].hand[tmpPayingInt].names[0] + ".jpg");
                    else
                        theCard = Image.FromFile(@"Images\Cards\" + this.CurrentGame.players[0].permanents[tmpPayingInt].names[0] + ".jpg");
                    Rectangle tmpCard = new Rectangle();
                    tmpCard.Height = (int)(PayingForRec.Height - drawFont.Height);
                    tmpCard.Width = (int)(tmpCard.Height * .7);
                    tmpCard.Location = new Point(((int)(this.PayingForRec.Location.X) + 5), ((int)((this.PayingForRec.Location.Y) + drawFont.Height)));
                    g.DrawImage(theCard, tmpCard);
                    theCard.Dispose();
                }
                else if (this.CurrentGame.attacking && this.CurrentGame.actPlayer == 0)
                {
                    this.priorityButton.Enabled = true;
                    messageBox.Text = "Declare Attackers!";
                    this.priorityButton.Text = "Done.";
                }
                else if (this.CurrentGame.blocking && this.CurrentGame.actPlayer == 0)
                {
                    if (this.CurrentGame.attackers.Count != this.CurrentGame.blockers.Count)
                        messageBox.Text = "Select attacker to block";
                    else
                        messageBox.Text = "Declare Blockers!";
                    this.priorityButton.Enabled = true;
                    messageBox.Text = "Declare Blockers!";
                    this.priorityButton.Text = "Done.";
                }
                else if (this.CurrentGame.cleanup)
                {
                    this.priorityButton.Enabled = false;
                    this.priorityButton.Text = "Discard to Continue";
                    messageBox.Text = "Discard down to 7 cards.";
                }
                else if (this.CurrentGame.actPlayer == 0 && this.CurrentGame.players[0].priority)
                {
                    this.passTurnButton.Enabled = true;
                    this.priorityButton.Enabled = true;
                    this.priorityButton.Text = "Pass Priority";
                    messageBox.Text = "Cast Spells or Activate Abilities!";
                }
                else if (this.CurrentGame.actPlayer == 0 && !this.CurrentGame.players[0].priority)
                {
                    this.passTurnButton.Enabled = false;
                    this.priorityButton.Text = "Wait";
                    messageBox.Text = "Your turn. Let the Comp think.";
                }
                else if (this.CurrentGame.actPlayer == 1 && this.CurrentGame.players[0].priority)
                {
                    messageBox.Text = "Comp's Turn, but you got priority";
                    this.passTurnButton.Enabled = false;
                    this.priorityButton.Enabled = true;
                    this.priorityButton.Text = "Pass Priority";
                }
                else if (this.CurrentGame.actPlayer == 1 && !this.CurrentGame.players[0].priority)
                {
                    messageBox.Text = "Comp's Turn, it's thinking";
                    this.passTurnButton.Enabled = false;
                    this.priorityButton.Enabled = false;
                    this.priorityButton.Text = "Pass Priority";
                }
                else
                {
                    messageBox.Text = "How did this happen?";

                }
                messageBox.Size = TextRenderer.MeasureText(messageBox.Text, messageBox.Font);
                this.gamestate.Size = TextRenderer.MeasureText(gamestate.Text, gamestate.Font);
            }
        }

        /**/
        /*
        GameBoard::Player1Battlefield_Paint() GameBoard::Player1Battlefield_Paint()
        NAME
               GameBoard::Player1Battlefield_Paint - handles the drawing of player1 battlefield
        DESCRIPTION
               Iterates through each card in Player1 permanents. Grabs name and pulls image with 
               that name and draws a rectangle on the board. If it's a land, its on the bottom half of
               field, otherwise its on top. If tapped the card is turned 90 degrees
        RETURNS
                nothing
        */
        /**/
        private void Player1Battlefield_Paint(object sender, PaintEventArgs e)
        {
            if (this.CurrentGame != null)
            {
                Graphics g = e.Graphics;
                Player human = this.CurrentGame.players[0];
                int numPerm = human.permanents.Count(), landCount = 0, notLand = 0;
                float num = (float)(1.0 / numPerm);
                bool first = true;
                this.Player1Perms = new List<Rectangle>();
                for (int i = 0; i < numPerm; i++)
                {
                    if (human.permanents[i].sides[0].types[0] == "Land")
                    {
                        Image theCard = Image.FromFile(@"Images\Cards\" + human.permanents[i].names[human.permanents[i].side] + ".jpg");
                        Rectangle tmpCard = new Rectangle();
                        tmpCard.Width = (int)(this.Player1Battlefield.Width * num);
                        tmpCard.Height = (int)(tmpCard.Width / .7);
                        if (tmpCard.Height > ((this.Player1Battlefield.Height / 2) * (float).7))
                        {
                            tmpCard.Height = (int)((this.Player1Battlefield.Height / 2) * (float).7);
                            tmpCard.Width = (int)(tmpCard.Height * .7);
                        }
                        if (first)
                            tmpCard.Location = new Point(((int)(this.Player1Battlefield.Width * ((num > .1) ? .1 : num) * landCount) + 5), ((int)((this.Player1Battlefield.Height / 2)) - 5));
                        else
                            tmpCard.Location = new Point(((int)(this.Player1Battlefield.Width * ((num > .1) ? .1 : num) * landCount) + (int)(10 * (landCount + .5))), ((int)((this.Player1Battlefield.Height / 2)) - 5));
                        first = false;
                        landCount++;
                        if (human.permanents[i].tapped)
                        {
                            theCard.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            int tmpheight = tmpCard.Height;
                            tmpCard.Height = tmpCard.Width;
                            tmpCard.Width = tmpheight;
                        }
                        this.Player1Perms.Push(tmpCard);
                        g.DrawImage(theCard, tmpCard);
                        theCard.Dispose();
                    }
                    else
                    {
                        Image theCard = Image.FromFile(@"Images\Cards\" + human.permanents[i].names[human.permanents[i].side] + ".jpg");
                        Rectangle tmpCard = new Rectangle();
                        tmpCard.Width = (int)(this.Player1Battlefield.Width * num);
                        tmpCard.Height = (int)(tmpCard.Width / .7);
                        if (tmpCard.Height > ((this.Player1Battlefield.Height / 2) * (float).7))
                        {
                            tmpCard.Height = (int)((this.Player1Battlefield.Height / 2) * (float).7);
                            tmpCard.Width = (int)(tmpCard.Height * .7);
                        }

                        if (human.permanents[i].tapped)
                        {
                            theCard.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            int tmpheight = tmpCard.Height;
                            tmpCard.Height = tmpCard.Width;
                            tmpCard.Width = tmpheight;
                        }
                        if (first)
                            tmpCard.Location = new Point(((int)(this.Player1Battlefield.Width * ((num > .1) ? .1 : num) * notLand) + 5), 0);
                        else
                            tmpCard.Location = new Point(((int)(this.Player1Battlefield.Width * ((num > .1) ? .1 : num) * notLand) + (int)(10 * (notLand + .5))), 0);
                        first = false;
                        notLand++;
                        this.Player1Perms.Push(tmpCard);
                        g.DrawImage(theCard, tmpCard);
                        theCard.Dispose();
                    }
                }
            }
        }

        /**/
        /*
        GameBoard::Player1Battlefield_Click() GameBoard::Player1Battlefield_Click()
        NAME
               GameBoard::Player1Battlefield_Click - handles the clicking of the battlefield
        DESCRIPTION
                iterates through all rectangles to see if the Mouseclick is within a drawn card
               calls checkClick to see what must be done if the player clicked a card
        RETURNS
                nothing
        */
        /**/
        private void Player1Battlefield_Click(object sender, EventArgs e)
        {
            if (this.choiceMenu.Items != null || this.choiceMenu.Items.Count != 0)
            {
                for (int j = this.choiceMenu.Items.Count; j > 0; j--)
                    this.choiceMenu.Items.Remove(this.choiceMenu.Items[0]);
                this.choiceMenu.Hide();
            }
            this.choiceMenu.Hide();
            int i = 0, X = MousePosition.X, Y = MousePosition.Y;
            Point position = this.PointToScreen(new Point(X, Y));
            int Ydiff = position.Y - Y, Xdiff = position.X - X;
            Card theCard = null;
            foreach (Rectangle paul in this.Player1Perms)
            {
                if (X > paul.Location.X + Xdiff + this.Player1Battlefield.Location.X && X < Xdiff + paul.Location.X + paul.Width + this.Player1Battlefield.Location.X)
                {
                    if (Y > paul.Location.Y + this.Player1Battlefield.Location.Y + Ydiff && Y < Ydiff + paul.Location.Y + paul.Height + this.Player1Battlefield.Location.Y)
                    {
                        tmpCardChoiceRec = paul;
                        tmpCardChoiceInt = (this.Player1Perms.Count - i - 1);
                        tmpCardZone = 'B';
                        theCard = this.CurrentGame.players[0].permanents[(this.Player1Perms.Count - i - 1)];
                        break;
                    }
                }
                i++;
            }
            if (theCard != null)
            {
                CheckClick(theCard);
                if (this.choiceMenu.Items != null || this.choiceMenu.Items.Count != 0)
                {
                    this.choiceMenu.Show(new Point(this.Player1Perms[i].Location.X + this.Player1Battlefield.Location.X, this.Player1Perms[i].Location.Y + this.Player1Battlefield.Location.Y));
                }
            }
        }

        /**/
        /*
        GameBoard::Player2Battlefield_Paint() GameBoard::Player2Battlefield_Paint()
        NAME
               GameBoard::Player2Battlefield_Paint - handles the drawing of player2 battlefield
        DESCRIPTION
               Iterates through each card in Player2 permanents. Grabs name and pulls image with 
               that name and draws a rectangle on the board. If it's a land, its on the top half of
               field, otherwise its on the bottom. If tapped the card is turned 90 degrees
        RETURNS
                nothing
        */
        /**/
        private void Player2Battlefield_Paint(object sender, PaintEventArgs e)
        {
            if (this.CurrentGame != null)
            {
                Graphics g = e.Graphics;
                Player comp = this.CurrentGame.players[1];
                int numPerm = comp.permanents.Count, landCount = 0, notLand = 0;
                double num = (1.0 / numPerm);
                this.Player2Perms = new List<Rectangle>();
                bool firstLand = true, firstNot = true;
                for (int i = 0; i < numPerm; i++)
                {
                    if (comp.permanents[i].sides[0].types[0] != "Land")
                    {
                        Image theCard = Image.FromFile(@"Images\Cards\" + comp.permanents[i].names[comp.permanents[i].side] + ".jpg");
                        Rectangle tmpCard = new Rectangle();
                        tmpCard.Width = (int)(this.Player1Battlefield.Width * num);
                        tmpCard.Height = (int)(tmpCard.Width / .7);
                        if (tmpCard.Height > ((this.Player1Battlefield.Height / 2) * (float).7))
                        {
                            tmpCard.Height = (int)((this.Player2Battlefield.Height / 2) * (float).7);
                            tmpCard.Width = (int)(tmpCard.Height * .7);
                        }

                        if (firstLand)
                            tmpCard.Location = new Point(((int)(this.Player2Battlefield.Width * ((num > .1) ? .1 : num) * landCount) + 5), ((int)((this.Player2Battlefield.Height / 2) - 5)));
                        else
                            tmpCard.Location = new Point(((int)(this.Player2Battlefield.Width * ((num > .1) ? .1 : num) * landCount) + (int)(10 * (landCount + .5))), ((int)((this.Player2Battlefield.Height / 2) - 5)));
                        firstLand = false;
                        landCount++;
                        if (comp.permanents[i].tapped)
                        {
                            theCard.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            int tmpheight = tmpCard.Height;
                            tmpCard.Height = tmpCard.Width;
                            tmpCard.Width = tmpheight;
                        }
                        this.Player2Perms.Push(tmpCard);
                        g.DrawImage(theCard, tmpCard);
                    }
                    else
                    {
                        Image theCard = Image.FromFile(@"Images\Cards\" + comp.permanents[i].names[comp.permanents[i].side] + ".jpg");
                        Rectangle tmpCard = new Rectangle();
                        tmpCard.Width = (int)(this.Player2Battlefield.Width * num);
                        tmpCard.Height = (int)(tmpCard.Width / .7);
                        if (tmpCard.Height > ((this.Player2Battlefield.Height / 2) * (float).7))
                        {
                            tmpCard.Height = (int)((this.Player2Battlefield.Height / 2) * (float).7);
                            tmpCard.Width = (int)(tmpCard.Height * .7);
                        }

                        if (firstNot)
                            tmpCard.Location = new Point(((int)(this.Player2Battlefield.Width * ((num > .1) ? .1 : num) * notLand) + 5), (this.Player2Battlefield.Location.Y));
                        else
                            tmpCard.Location = new Point(((int)(this.Player2Battlefield.Width * ((num > .1) ? .1 : num) * notLand) + (int)(10 * (notLand + .5))), ((int)(this.Player2Battlefield.Location.Y)));
                        firstNot = false;
                        notLand++;
                        if (comp.permanents[i].tapped)
                        {
                            theCard.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            int tmpheight = tmpCard.Height;
                            tmpCard.Height = tmpCard.Width;
                            tmpCard.Width = tmpheight;
                        }
                        this.Player2Perms.Push(tmpCard);
                        g.DrawImage(theCard, tmpCard);
                    }
                }
            }
        }

        /**/
        /*
        GameBoard::Player2Battlefield_Click() GameBoard::Player2Battlefield_Click()
        NAME
               GameBoard::Player2Battlefield_Click - handles the clicking of the battlefield
        DESCRIPTION
                iterates through all rectangles to see if the Mouseclick is within a drawn card
               calls checkClick to see what must be done if the player clicked a card
        RETURNS
                nothing
        */
        /**/
        private void Player2Battlefield_Click(object sender, EventArgs e)
        {
            if (this.choiceMenu.Items != null || this.choiceMenu.Items.Count != 0)
            {
                for (int j = this.choiceMenu.Items.Count; j > 0; j--)
                    this.choiceMenu.Items.Remove(this.choiceMenu.Items[0]);
                this.choiceMenu.Hide();
            }
            int i = 0, X = MousePosition.X, Y = MousePosition.Y;
            Point position = this.PointToScreen(new Point(X, Y));
            int Ydiff = position.Y - Y, Xdiff = position.X - X;
            Card theCard = null;
            foreach (Rectangle paul in this.Player2Perms)
            {
                if (X > paul.Location.X + Xdiff + this.Player2Battlefield.Location.X && X < paul.Location.X + Xdiff + paul.Width + this.Player2Battlefield.Location.X)
                {
                    if (Y > paul.Location.Y + this.Player2Battlefield.Location.Y + Ydiff && Y < paul.Location.Y + paul.Height + Ydiff + this.Player2Battlefield.Location.Y)
                    {
                        tmpCardChoiceRec = paul;
                        tmpCardChoiceInt = (this.Player2Perms.Count - i - 1);
                        tmpCardZone = 'b';
                        theCard = this.CurrentGame.players[1].permanents[(this.Player2Perms.Count - i - 1)];
                    }
                }
                i++;
            }
            if (theCard != null)
            {
                CheckClick(theCard);
            }
            Invalidate(true);
        }

        /**/
        /*
        GameBoard::Player2Graveyard_Paint() GameBoard::Player2Graveyard_Paint()
        NAME
               GameBoard::Player2Graveyard_Paint - handles the drawing of player2 graveyard
        DESCRIPTION
               Iterates through each card in Player2 grave. Grabs name and pulls image with 
               that name and draws a rectangle in the yard. Goes top to bottom
        RETURNS
                nothing
        */
        /**/
        private void Player2Graveyard_Paint(object sender, PaintEventArgs e)
        {
            if ((this.CurrentGame != null) && (this.CurrentGame.players[1].graveyard.Count != 0))
            {
                Graphics g = e.Graphics;
                Player comp = this.CurrentGame.players[1];
                int numPerm = comp.graveyard.Count;
                double num = (1.0 / numPerm);
                this.Player2Grave = new List<Rectangle>();
                bool firstcard = true;
                for (int i = 0; i < numPerm; i++)
                {
                    Image theCard = Image.FromFile(@"Images\Cards\" + comp.graveyard[i].names[comp.graveyard[i].side] + ".jpg");
                    Rectangle tmpCard = new Rectangle();
                    tmpCard.Height = (int)(this.Player2Graveyard.Height * .2);
                    tmpCard.Width = (int)(tmpCard.Height * .7);
                    if (tmpCard.Height > ((this.Player1Graveyard.Height / 2) * (float).7))
                    {
                        tmpCard.Height = (int)((this.Player1Graveyard.Height / 2) * (float).7);
                        tmpCard.Width = (int)(tmpCard.Height * .7);
                    }

                    if (firstcard)
                        tmpCard.Location = new Point(((int)(this.Player2Graveyard.Width * ((num > .1) ? .1 : num)) + 5), ((int)((this.Player2Graveyard.Location.Y) + 5)));
                    else
                        tmpCard.Location = new Point(((int)(this.Player2Graveyard.Width * ((num > .1) ? .1 : num)) + 5), ((int)((this.Player2Graveyard.Location.Y) + this.Player2Grave[0].Location.Y + 5)));
                    firstcard = false;

                    this.Player2Grave.Push(tmpCard);
                    g.DrawImage(theCard, tmpCard);
                    theCard.Dispose();
                }
            }
        }

        /**/
        /*
        GameBoard::Player2Graveyard_Click() GameBoard::Player2Graveyard_Click()
        NAME
               GameBoard::Player2Graveyard_Click - handles the clicking of the graveyard
        DESCRIPTION
                iterates through all rectangles to see if the Mouseclick is within a drawn card
               calls checkClick to see what must be done if the player clicked a card
        RETURNS
                nothing
        */
        /**/
        private void Player2Graveyard_Click(object sender, EventArgs e)
        {
            if (this.choiceMenu.Items != null || this.choiceMenu.Items.Count != 0)
            {
                for (int j = this.choiceMenu.Items.Count; j > 0; j--)
                    this.choiceMenu.Items.Remove(this.choiceMenu.Items[0]);
                this.choiceMenu.Hide();
            }
            int i = 0, X = MousePosition.X, Y = MousePosition.Y;
            Point position = this.PointToScreen(new Point(X, Y));
            int Ydiff = position.Y - Y, Xdiff = position.X - X;
            Card theCard = null;
            foreach (Rectangle paul in this.Player2Grave)
            {
                if (X > paul.Location.X + this.Player2Graveyard.Location.X + Xdiff && X < paul.Location.X + paul.Width + this.Player2Graveyard.Location.X + Xdiff)
                {
                    if (Y > paul.Location.Y + this.Player2Graveyard.Location.Y + Ydiff && Y < paul.Location.Y + paul.Height + this.Player2Graveyard.Location.Y + Ydiff)
                    {
                        tmpCardChoiceRec = paul;
                        tmpCardChoiceInt = (this.Player2Grave.Count - i - 1);
                        tmpCardZone = 'H';
                        theCard = this.CurrentGame.players[0].hand[(this.Player2Grave.Count - i - 1)];
                    }
                }
                i++;
            }
            if (theCard != null)
            {
                CheckClick(theCard);
            }
            Invalidate(true);
        }

        /**/
        /*
        GameBoard::Player2ExileZone_Paint() GameBoard::Player2ExileZone_Paint()
        NAME
               GameBoard::Player2ExileZone_Paint - handles the drawing of player2 exile
        DESCRIPTION
               Iterates through each card in Player2 exile. Grabs name and pulls image with 
               that name and draws a rectangle in the yard. Goes top to bottom
        RETURNS
                nothing
        */
        /**/
        private void Player2ExileZone_Paint(object sender, PaintEventArgs e)
        {
            if ((this.CurrentGame != null) && (this.CurrentGame.players[1].exile.Count != 0))
            {
                Graphics g = e.Graphics;
                Player comp = this.CurrentGame.players[1];
                int numPerm = comp.exile.Count;
                double num = (1.0 / numPerm);
                this.Player2Exile = new List<Rectangle>();
                bool firstcard = true;
                for (int i = 0; i < numPerm; i++)
                {
                    Image theCard = Image.FromFile(@"Images\Cards\" + comp.graveyard[i].names[comp.graveyard[i].side] + ".jpg");
                    Rectangle tmpCard = new Rectangle();
                    tmpCard.Height = (int)(this.Player2ExileZone.Height * .1);
                    tmpCard.Width = (int)(tmpCard.Height * .7);
                    if (tmpCard.Height > ((this.Player2ExileZone.Height / 2) * (float).7))
                    {
                        tmpCard.Height = (int)((this.Player2ExileZone.Height / 2) * (float).7);
                        tmpCard.Width = (int)(tmpCard.Height * .7);
                    }

                    if (firstcard)
                        tmpCard.Location = new Point(((int)(this.Player2ExileZone.Width * ((num > .1) ? .1 : num)) + 5), ((int)((this.Player2ExileZone.Location.Y) + 5)));
                    else
                        tmpCard.Location = new Point(((int)(this.Player2ExileZone.Width * ((num > .1) ? .1 : num)) + 5), ((int)((this.Player2ExileZone.Location.Y) + this.Player2Exile[0].Location.Y + this.Player2Exile[0].Height + 5)));
                    firstcard = false;

                    this.Player2Exile.Push(tmpCard);
                    g.DrawImage(theCard, tmpCard);
                    theCard.Dispose();
                }
            }
        }

        /**/
        /*
        GameBoard::Player2ExileZone_Click() GameBoard::Player2ExileZone_Click()
        NAME
               GameBoard::Player2ExileZone_Click - handles the clicking of the exilezone
        DESCRIPTION
                iterates through all rectangles to see if the Mouseclick is within a drawn card
               calls checkClick to see what must be done if the player clicked a card
        RETURNS
                nothing
        */
        /**/
        private void Player2ExileZone_Click(object sender, EventArgs e)
        {
            if (this.choiceMenu.Items != null || this.choiceMenu.Items.Count != 0)
            {
                for (int j = this.choiceMenu.Items.Count; j > 0; j--)
                    this.choiceMenu.Items.Remove(this.choiceMenu.Items[0]);
                this.choiceMenu.Hide();
            }
            int i = 0, X = MousePosition.X, Y = MousePosition.Y;
            Point position = this.PointToScreen(new Point(X, Y));
            int Ydiff = position.Y - Y, Xdiff = position.X - X;
            Card theCard = null;
            foreach (Rectangle paul in this.Player2Exile)
            {
                if (X > paul.Location.X + this.Player2ExileZone.Location.X + Xdiff && X < paul.Location.X + paul.Width + this.Player2ExileZone.Location.X + Xdiff)
                {
                    if (Y > paul.Location.Y + this.Player2ExileZone.Location.Y + Ydiff && Y < paul.Location.Y + paul.Height + this.Player2ExileZone.Location.Y + Ydiff)
                    {
                        tmpCardChoiceRec = paul;
                        tmpCardChoiceInt = (this.Player2Exile.Count - i - 1);
                        tmpCardZone = 'H';
                        theCard = this.CurrentGame.players[0].hand[(this.Player2Exile.Count - i - 1)];
                    }
                }
                i++;
            }
            if (theCard != null)
            {
                CheckClick(theCard);
            }
            Invalidate(true);
        }

        /**/
        /*
        GameBoard::Player1HandArea_Paint() GameBoard::Player1HandArea_Paint()
        NAME
               GameBoard::Player1HandArea_Paint - handles the drawing of player1 hand
        DESCRIPTION
               Iterates through each card in Player1 hand. Grabs name and pulls image with 
               that name and draws a rectangle in the handarea. 
        RETURNS
                nothing
        */
        /**/
        private void Player1HandArea_Paint(object sender, PaintEventArgs e)
        {
            if (this.CurrentGame != null)
            {
                Graphics g = e.Graphics;
                Player human = this.CurrentGame.players[0];
                int numHand = human.hand.Count();
                double num = (1.0 / numHand);
                bool first = true;
                this.Player1Hand = new List<Rectangle>();

                for (int i = 0; i < numHand; i++)
                {
                    Image theCard = Image.FromFile(@"Images\Cards\" + human.hand[i].names[human.hand[i].side] + ".jpg");
                    Rectangle tmpCard = new Rectangle();
                    tmpCard.Width = (int)(this.Player1HandArea.Width * num);
                    tmpCard.Height = (int)(tmpCard.Width / .7);
                    if (tmpCard.Height > (this.Player1HandArea.Height * (float).7))
                    {
                        tmpCard.Height = (int)(this.Player1HandArea.Height * (float).7);
                        tmpCard.Width = (int)(tmpCard.Height * .7);
                    }
                    if (first)
                        tmpCard.Location = new Point(((int)(this.Player1HandArea.Width * ((num > .1) ? .1 : num) * i) + 5), ((int)(this.Player1HandArea.Height * (float).1)) - 5);
                    else
                        tmpCard.Location = new Point(((int)(this.Player1HandArea.Width * ((num > .1) ? .1 : num) * i) + 5), ((int)(this.Player1HandArea.Height * (float).1)) - 5);
                    first = false;
                    this.Player1Hand.Push(tmpCard);
                    g.DrawImage(theCard, tmpCard);
                    theCard.Dispose();
                }
            }
        }

        /**/
        /*
        GameBoard::Player1HandArea_Click() GameBoard::Player1HandArea_Click()
        NAME
               GameBoard::Player1HandArea_Click - handles the clicking of the hand
        DESCRIPTION
                iterates through all rectangles to see if the Mouseclick is within a drawn card
               calls checkClick to see what must be done if the player clicked a card
        RETURNS
                nothing
        */
        /**/
        private void Player1HandArea_Click(object sender, EventArgs e)
        {
            if (this.choiceMenu.Items != null || this.choiceMenu.Items.Count != 0)
            {
                for (int j = this.choiceMenu.Items.Count; j > 0; j--)
                    this.choiceMenu.Items.Remove(this.choiceMenu.Items[0]);
                this.choiceMenu.Hide();
            }
            int i = 0, X = MousePosition.X, Y = MousePosition.Y;
            Point position = this.PointToScreen(new Point(X, Y));
            int Ydiff = position.Y - Y, Xdiff = position.X - X;
            Card theCard = null;
            foreach (Rectangle paul in this.Player1Hand)
            {
                if (X > paul.Location.X + this.Player1HandArea.Location.X + Xdiff && X < paul.Location.X + paul.Width + this.Player1HandArea.Location.X + Xdiff)
                {
                    if (Y > paul.Location.Y + this.Player1HandArea.Location.Y + Ydiff && Y < paul.Location.Y + paul.Height + this.Player1HandArea.Location.Y + Ydiff)
                    {

                        tmpCardChoiceRec = paul;
                        tmpCardChoiceInt = (this.Player1Hand.Count - i - 1);
                        tmpCardZone = 'H';
                        theCard = this.CurrentGame.players[0].hand[(this.Player1Hand.Count - i - 1)];
                    }
                }
                i++;
            }
            if (theCard != null)
            {
                CheckClick(theCard);
            }
            Invalidate(true);
        }

        /**/
        /*
        GameBoard::Player1ManaPool_Paint() GameBoard::Player1ManaPool_Paint()
        NAME
               GameBoard::Player1ManaPool_Paint - handles the drawing of player1 manapool
        DESCRIPTION
               Iterates through each mana in Player1 manapool. If there is more than one of that mana, 
               a textbox is drawn depicting a number of how many mana
        RETURNS
                nothing
        */
        /**/
        private void Player1ManaPool_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (CurrentGame != null)
            {
                int[] colorsArray = new int[5] { 0, 0, 0, 0, 0 };
                foreach (Mana man in this.CurrentGame.players[0].manapool)
                {
                    switch (man.color)
                    {
                        case 1:
                            colorsArray[0]++;
                            break;
                        case 2:
                            break;
                        case 3:
                            colorsArray[1]++;
                            break;
                        case 4:
                            colorsArray[2]++;
                            break;
                        case 5:
                            colorsArray[3]++;
                            break;
                        case 6:
                            colorsArray[4]++;
                            break;
                    }
                }
                int num = ((colorsArray[0] == 0) ? 0 : 1) + ((colorsArray[1] == 0) ? 0 : 1) + ((colorsArray[2] == 0) ? 0 : 1) + ((colorsArray[3] == 0) ? 0 : 1) + ((colorsArray[4] == 0) ? 0 : 1);
                Player1Manas = new List<Rectangle>();
                this.Player1ManaPool.Controls.Clear();
                this.Player1ManaText = new List<TextBox>();
                if (num > 0)
                {
                    bool first = true;
                    int manaCount = 0;
                    int i = -1;
                    string[] colors = { "white", "black", "red", "green", "colorless" };
                    foreach (int k in colorsArray)
                    {
                        i++;
                        if (k > 0)
                        {
                            Image symbol = Image.FromFile(@"Images\" + colors[i] + "Mana.png");
                            Rectangle tmpSymbol = new Rectangle();
                            tmpSymbol.Width = (int)(this.Player1ManaPool.Width * .1);
                            tmpSymbol.Height = (int)(tmpSymbol.Width);
                            if (first)
                                tmpSymbol.Location = new Point(((int)(this.Player1ManaPool.Width * (float)(.1) * manaCount) + 5), (int)(5));
                            else
                                tmpSymbol.Location = new Point(((int)(this.Player1ManaPool.Width * (float)(.1) * manaCount) + 5), (int)(5));
                            first = false;
                            this.Player1Manas.Push(tmpSymbol);

                            g.DrawImage(symbol, tmpSymbol);

                            TextBox number = new TextBox();
                            number.Enabled = false;
                            number.ReadOnly = true;
                            number.Text = k.ToString();
                            number.Size = TextRenderer.MeasureText(number.Text, number.Font);
                            number.Location = new Point(((int)(this.Player1ManaPool.Width * (float)(.1) * (manaCount + 1)) + 5), (int)(5));

                            this.Player1ManaText.Push(number);
                            this.Player1ManaPool.Controls.Add(number);
                            manaCount += 2;
                            symbol.Dispose();
                        }

                    }
                }
            }
        }

        /**/
        /*
        GameBoard::Player1ManaPool_Click() GameBoard::Player1ManaPool_Click()
        NAME
               GameBoard::Player1ManaPool_Click - handles the clicking of the manapool
        DESCRIPTION
                iterates through all rectangles to see if the Mouseclick is within a drawn mana symbol
                moves clicked mana from mana pool to mana spending
               call checkPaymentCompletion to see if the player is done paying for their spell/ability
        RETURNS
                nothing
        */
        /**/
        private void Player1ManaPool_Click(object sender, EventArgs e)
        {
            if (paying)
            {
                int i = 0, X = MousePosition.X, Y = MousePosition.Y;
                Point position = this.PointToScreen(new Point(X, Y));
                int Ydiff = position.Y - Y, Xdiff = position.X - X;
                //better find out which mana it was
                foreach (Rectangle paul in this.Player1Manas)
                {
                    if (X > paul.Location.X + this.Player1ManaPool.Location.X + Xdiff && X < paul.Location.X + paul.Width + this.Player1ManaPool.Location.X + Xdiff)
                    {
                        if (Y > paul.Location.Y + this.Player1ManaPool.Location.Y + Ydiff && Y < paul.Location.Y + paul.Height + this.Player1ManaPool.Location.Y + Ydiff)
                        {
                            //order by color
                            List<Mana> tmp = new List<Mana>(this.CurrentGame.players[0].manapool.OrderBy(q => q.color));
                            //this wil hold the mana we'll grab
                            Mana poppin = new Mana(-1, null);
                            int test = 0;
                            if (i == this.Player1Manas.Count() - i)
                                poppin = tmp[0];
                            else if (i == 0)
                                poppin = tmp[tmp.Count() - 1];
                            else
                            {
                                while (test < this.Player1Manas.Count() - 1 && test <= i)
                                {
                                    if (poppin.color != tmp[test].color)
                                        poppin = tmp[test];
                                    test++;
                                }
                            }
                            //they have mana from the eldrazi temple, check to make sure this is an eldrazi spell
                            if (poppin.stipulations == "Eldrazi")
                            {
                                if (!((tmpPayingActivatedInt == -1) ? this.CurrentGame.players[0].hand[tmpPayingInt].sides[0].subtypes.Contains("Eldrazi") : this.CurrentGame.players[0].permanents[tmpPayingInt].sides[0].subtypes.Contains("Eldrazi")))
                                    break;
                            }
                            if ((this.CurrentGame.players[0].manaSpending.Count(x => x.color == poppin.color) < ((tmpPayingActivatedInt == -1) ? this.CurrentGame.players[0].hand[tmpPayingInt].sides[0].manacost.Count(x => x.color == poppin.color) : this.CurrentGame.players[0].permanents[tmpPayingInt].sides[0].effects[tmpPayingActivatedInt].manaCost.Count(x => x.color == poppin.color))) || (this.CurrentGame.players[0].manaSpending.Count(x => x.color == poppin.color) < ((tmpPayingActivatedInt == -1) ? this.CurrentGame.players[0].hand[tmpPayingInt].sides[0].manacost.Count(x => x.color == 0) : this.CurrentGame.players[0].permanents[tmpPayingInt].sides[0].effects[tmpPayingActivatedInt].manaCost.Count(x => x.color == 0))))
                            {
                                this.CurrentGame.players[0].manaSpending.Push(this.CurrentGame.players[0].manapool.PopAt(this.CurrentGame.players[0].manapool.FindIndex(x => x.color == poppin.color)));
                                checkPaymentCompletion();
                            }
                            break;
                        }
                    }
                    i++;
                }
                Invalidate(true);
            }
        }

        /**/
        /*
        GameBoard::SpellCostLeft_Paint() GameBoard::SpellCostLeft_Paint()
        NAME
               GameBoard::SpellCostLeft_Paint - handles the drawing of the mana left to be paid
        DESCRIPTION
               Iterates through each mana in card/ability manacost. If there is more than one of that mana, 
               a textbox is drawn depicting a number of how many mana
        RETURNS
                nothing
        */
        /**/
        private void SpellCostLeft_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (CurrentGame != null)
            {
                if (paying)
                {
                    List<Mana> tmpSpending = new List<Mana>(this.CurrentGame.players[0].manaSpending), tmpTobePayed = new List<Mana>();
                    if (tmpPayingZone == 'H')
                    {
                        tmpTobePayed = new List<Mana>(this.CurrentGame.players[0].hand[tmpPayingInt].sides[0].manacost);
                        //(this.CurrentGame.players[0].hand[tmpPayingInt].sides[0].manacost).Except(this.CurrentGame.players[0].manaSpending).ToList();
                        foreach (Mana m in this.CurrentGame.players[0].hand[tmpPayingInt].sides[0].manacost.FindAll(x => x.color != 0).ToList())
                        {
                            foreach (Mana n in tmpSpending)
                            {
                                if (m.color == n.color)
                                {
                                    tmpTobePayed.Remove(m);
                                    tmpSpending.Remove(n);
                                    break;
                                }
                            }
                        }
                        for (int i = tmpSpending.Count - 1; i > -1; i--)
                        {
                            int spot = tmpTobePayed.FindIndex(x => x.color == 0);
                            if (spot > -1)
                            {
                                tmpTobePayed.RemoveAt(spot);
                                tmpSpending.RemoveAt(0);
                            }
                        }
                    }
                    else
                    {
                        tmpTobePayed = new List<Mana>(this.CurrentGame.players[0].permanents[tmpPayingInt].sides[0].effects[tmpPayingActivatedInt].manaCost);
                        //(this.CurrentGame.players[0].hand[tmpPayingInt].sides[0].manacost).Except(this.CurrentGame.players[0].manaSpending).ToList();
                        foreach (Mana m in this.CurrentGame.players[0].permanents[tmpPayingInt].sides[0].effects[tmpPayingActivatedInt].manaCost.FindAll(x => x.color != 0))
                        {
                            foreach (Mana n in tmpSpending)
                            {
                                if (m.color == n.color)
                                {
                                    tmpTobePayed.Remove(m);
                                    tmpSpending.Remove(n);
                                    break;
                                }
                            }
                        }
                        foreach (Mana m in this.CurrentGame.players[0].permanents[tmpPayingInt].sides[0].effects[tmpPayingActivatedInt].manaCost.FindAll(x => x.color == 0))
                        {
                            if (tmpSpending.Count != 0)
                            {
                                tmpTobePayed.Remove(m);
                                tmpSpending.RemoveAt(0);
                                break;
                            }
                        }
                    }
                    int[] colorsArray = new int[6] { 0, 0, 0, 0, 0, 0 };
                    foreach (Mana man in tmpTobePayed)
                    {
                        switch (man.color)
                        {
                            case 0:
                                colorsArray[4]++;
                                break;
                            case 1:
                                colorsArray[0]++;
                                break;
                            case 2:
                                break;
                            case 3:
                                colorsArray[1]++;
                                break;
                            case 4:
                                colorsArray[2]++;
                                break;
                            case 5:
                                colorsArray[3]++;
                                break;
                            case 6:
                                colorsArray[4]++;
                                break;
                        }
                    }
                    int num = ((colorsArray[5] == 0) ? 0 : 1) + ((colorsArray[0] == 0) ? 0 : 1) + ((colorsArray[1] == 0) ? 0 : 1) + ((colorsArray[2] == 0) ? 0 : 1) + ((colorsArray[3] == 0) ? 0 : 1) + ((colorsArray[4] == 0) ? 0 : 1);
                    this.SpellCostLeft.Controls.Clear();
                    if (num > 0)
                    {
                        bool first = true;
                        int manaCount = 0;
                        int i = -1;
                        string[] colors = { "white", "black", "red", "green", "colorless" };
                        foreach (int k in colorsArray)
                        {
                            i++;
                            if (k > 0)
                            {
                                Image symbol = Image.FromFile(@"Images\" + colors[i] + "Mana.png");
                                Rectangle tmpSymbol = new Rectangle();
                                tmpSymbol.Width = (int)(this.SpellCostLeft.Width * .1);
                                tmpSymbol.Height = (int)(tmpSymbol.Width);
                                if (first)
                                    tmpSymbol.Location = new Point(((int)(this.SpellCostLeft.Width * (float)(.1) * manaCount) + 5), (int)(5));
                                else
                                    tmpSymbol.Location = new Point(((int)(this.SpellCostLeft.Width * (float)(.1) * manaCount) + 5), (int)(5));
                                first = false;

                                g.DrawImage(symbol, tmpSymbol);

                                TextBox number = new TextBox();
                                number.Enabled = false;
                                number.ReadOnly = true;
                                number.Text = k.ToString();
                                number.Size = TextRenderer.MeasureText(number.Text, number.Font);
                                number.Location = new Point(((int)(this.SpellCostLeft.Width * (float)(.1) * (manaCount + 1)) + 5), (int)(5));

                                this.SpellCostLeft.Controls.Add(number);
                                manaCount += 2;
                                symbol.Dispose();
                            }
                        }
                    }
                }
                else
                {
                    if (this.SpellCostLeft.Controls.Count > 0)
                        this.SpellCostLeft.Controls.Clear();
                }
            }
        }
        /**/
        /*
        GameBoard::Player1Graveyard_Paint() GameBoard::Player1Graveyard_Paint()
        NAME
               GameBoard::Player1Graveyard_Paint - handles the drawing of player1 grave
        DESCRIPTION
               Iterates through each card in Player1 grave. Goes top to bottom
        RETURNS
                nothing
        */
        /**/
        private void Player1Graveyard_Paint(object sender, PaintEventArgs e)
        {
            if ((this.CurrentGame != null) && (this.CurrentGame.players[0].graveyard.Count != 0))
            {
                Graphics g = e.Graphics;
                Player comp = this.CurrentGame.players[0];
                int numPerm = comp.graveyard.Count;
                double num = (1.0 / numPerm);
                this.Player1Grave = new List<Rectangle>();
                bool firstcard = true;
                for (int i = 0; i < numPerm; i++)
                {
                    Image theCard = Image.FromFile(@"Images\Cards\" + comp.graveyard[i].names[comp.graveyard[i].side] + ".jpg");
                    Rectangle tmpCard = new Rectangle();
                    tmpCard.Height = (int)(this.Player1Graveyard.Height * .2);
                    tmpCard.Width = (int)(tmpCard.Height * .7);
                    if (tmpCard.Height > ((this.Player1Graveyard.Height / 2) * (float).7))
                    {
                        tmpCard.Height = (int)((this.Player1Graveyard.Height / 2) * (float).7);
                        tmpCard.Width = (int)(tmpCard.Height * .7);
                    }
                    if (firstcard)
                        tmpCard.Location = new Point(((int)(this.Player1Graveyard.Width * ((num > .1) ? .1 : num)) + 5), ((int)(5)));
                    else
                        tmpCard.Location = new Point(((int)(this.Player1Graveyard.Width * ((num > .1) ? .1 : num)) + 5), ((int)(this.Player1Grave[0].Location.Y + this.Player1Grave[0].Height + 5)));
                    firstcard = false;

                    this.Player1Grave.Push(tmpCard);
                    g.DrawImage(theCard, tmpCard);
                    theCard.Dispose();
                }
            }
        }

        /**/
        /*
        GameBoard::Player1Graveyard_Click() GameBoard::Player1Graveyard_Click()
        NAME
               GameBoard::Player1Graveyard_Click - handles the clicking of the grave
        DESCRIPTION
                iterates through all rectangles to see if the Mouseclick is within a drawn card
               calls checkClick to see what must be done if the player clicked a card
        RETURNS
                nothing
        */
        /**/
        private void Player1Graveyard_Click(object sender, EventArgs e)
        {
            if (this.choiceMenu.Items != null || this.choiceMenu.Items.Count != 0)
            {
                for (int j = this.choiceMenu.Items.Count; j > 0; j--)
                    this.choiceMenu.Items.Remove(this.choiceMenu.Items[0]);
                this.choiceMenu.Hide();
            }
            int i = 0, X = MousePosition.X, Y = MousePosition.Y;
            Point position = this.PointToScreen(new Point(X, Y));
            int Ydiff = position.Y - Y, Xdiff = position.X - X;
            Card theCard = null;
            foreach (Rectangle paul in this.Player1Grave)
            {
                if (X > paul.Location.X + this.Player1Graveyard.Location.X + Xdiff && X < paul.Location.X + paul.Width + this.Player1Graveyard.Location.X + Xdiff)
                {
                    if (Y > paul.Location.Y + this.Player1Graveyard.Location.Y + Ydiff && Y < paul.Location.Y + paul.Height + this.Player1Graveyard.Location.Y + Ydiff)
                    {
                        tmpCardChoiceRec = paul;
                        tmpCardChoiceInt = (this.Player1Grave.Count - i - 1);
                        tmpCardZone = 'H';
                        theCard = this.CurrentGame.players[0].hand[(this.Player1Grave.Count - i - 1)];
                    }
                }
                i++;
            }
            if (theCard != null)
            {
                CheckClick(theCard);
            }
            Invalidate(true);
        }

        /**/
        /*
        GameBoard::Player1ExileZone_Paint() GameBoard::Player1ExileZone_Paint()
        NAME
               GameBoard::Player1ExileZone_Paint - handles the drawing of player1 exile
        DESCRIPTION
               Iterates through each card in Player1 exile. Goes top to bottom
        RETURNS
                nothing
        */
        /**/
        private void Player1ExileZone_Paint(object sender, PaintEventArgs e)
        {
            if ((this.CurrentGame != null) && (this.CurrentGame.players[0].exile.Count != 0))
            {
                Graphics g = e.Graphics;
                Player comp = this.CurrentGame.players[0];
                int numPerm = comp.exile.Count;
                double num = (1.0 / numPerm);
                this.Player1Exile = new List<Rectangle>();
                bool firstcard = true;
                for (int i = 0; i < numPerm; i++)
                {
                    Image theCard = Image.FromFile(@"Images\Cards\" + comp.graveyard[i].names[comp.graveyard[i].side] + ".jpg");
                    Rectangle tmpCard = new Rectangle();
                    tmpCard.Height = (int)(this.Player1ExileZone.Height * .1);
                    tmpCard.Width = (int)(tmpCard.Height * .7);
                    if (tmpCard.Height > ((this.Player1ExileZone.Height / 2) * (float).7))
                    {
                        tmpCard.Height = (int)((this.Player1ExileZone.Height / 2) * (float).7);
                        tmpCard.Width = (int)(tmpCard.Height * .7);
                    }

                    if (firstcard)
                        tmpCard.Location = new Point(((int)(this.Player1ExileZone.Width * ((num > .1) ? .1 : num)) + 5), ((int)((this.Player1ExileZone.Location.Y) + 5)));
                    else
                        tmpCard.Location = new Point(((int)(this.Player1ExileZone.Width * ((num > .1) ? .1 : num)) + 5), ((int)((this.Player1ExileZone.Location.Y) + this.Player1Exile[0].Location.Y + this.Player1Exile[0].Height + 5)));
                    firstcard = false;

                    this.Player1Exile.Push(tmpCard);
                    g.DrawImage(theCard, tmpCard);
                    theCard.Dispose();
                }
            }
        }

        /**/
        /*
        GameBoard::Player1ExileZone_Click() GameBoard::Player1ExileZone_Click()
        NAME
               GameBoard::Player1ExileZone_Click - handles the clicking of the exile
        DESCRIPTION
                iterates through all rectangles to see if the Mouseclick is within a drawn card
               calls checkClick to see what must be done if the player clicked a card
        RETURNS
                nothing
        */
        /**/
        private void Player1ExileZone_Click(object sender, EventArgs e)
        {
            if (this.choiceMenu.Items != null || this.choiceMenu.Items.Count != 0)
            {
                for (int j = this.choiceMenu.Items.Count; j > 0; j--)
                    this.choiceMenu.Items.Remove(this.choiceMenu.Items[0]);
                this.choiceMenu.Hide();
            }
            int i = 0, X = MousePosition.X, Y = MousePosition.Y;
            Point position = this.PointToScreen(new Point(X, Y));
            int Ydiff = position.Y - Y, Xdiff = position.X - X;
            Card theCard = null;
            foreach (Rectangle paul in this.Player1Exile)
            {
                if (X > paul.Location.X + this.Player1ExileZone.Location.X + Xdiff && X < paul.Location.X + paul.Width + this.Player1ExileZone.Location.X + Xdiff)
                {
                    if (Y > paul.Location.Y + this.Player1ExileZone.Location.Y + Ydiff && Y < paul.Location.Y + paul.Height + this.Player1ExileZone.Location.Y + Ydiff)
                    {
                        tmpCardChoiceRec = paul;
                        tmpCardChoiceInt = (this.Player1Exile.Count - i - 1);
                        tmpCardZone = 'H';
                        theCard = this.CurrentGame.players[0].hand[(this.Player1Exile.Count - i - 1)];
                    }
                }
                i++;
            }
            if (theCard != null)
            {
                CheckClick(theCard);
            }
            Invalidate(true);
        }

        /**/
        /*
        GameBoard::theStackBox_Paint() GameBoard::theStackBox_Paint()
        NAME
               GameBoard::theStackBox_Paint - handles the drawing of the stack
        DESCRIPTION
                if it isn't empty, Iterates through the stack and draws a rectangle for each card
        RETURNS
                nothing
        */
        /**/
        private void theStackBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (CurrentGame != null)
            {
                int stackSize = this.CurrentGame.stack.Count;
                if (stackSize != 0)
                {
                    bool first = true;
                    this.theStack = new List<Rectangle>();
                    for (int i = 0; i < stackSize; i++)
                    {
                        Image theCard = Image.FromFile(@"Images\Cards\" + this.CurrentGame.stack[i].names[this.CurrentGame.stack[i].side] + ".jpg");
                        Rectangle tmpCard = new Rectangle();
                        tmpCard.Height = (int)(this.theStackBox.Height * .2);
                        tmpCard.Width = (int)(tmpCard.Height * .7);
                        if (tmpCard.Width > (this.theStackBox.Width * (float).8))
                        {
                            tmpCard.Width = (int)(this.theStackBox.Width * (float).8);
                            tmpCard.Height = (int)(tmpCard.Width / (float).7);
                        }

                        if (first)
                            tmpCard.Location = new Point(5, 5);
                        else
                        {
                            tmpCard.Location = new Point((this.theStack[0].Location.X), ((int)(this.theStack[0].Location.Y + (this.theStackBox.Height * .1))));
                        }
                        first = false;
                        this.theStack.Push(tmpCard);
                        g.DrawImage(theCard, tmpCard);
                        theCard.Dispose();
                    }
                }
            }
        }

        /**/
        /*
        GameBoard::quitButton_Click() GameBoard::quitButton_Click()
        NAME
               GameBoard::quitButton_Click - handles the clicking of the quit button
        DESCRIPTION
                quits the game, opens up the first form again
        RETURNS
                nothing
        */
        /**/
        private void quitButton_Click(object sender, EventArgs e)
        {
            Form1 newgame = new Form1("The player quit the game");
            this.Hide();
            newgame.ShowDialog();
            this.Close();
        }

        /**/
        /*
        GameBoard::mulliganButton_Click() GameBoard::mulliganButton_Click()
        NAME
               GameBoard::mulliganButton_Click - handles the mulliganing
        DESCRIPTION
                player gets a new hand, but draws 1 less card if they mulligan
        RETURNS
                nothing
        */
        /**/
        private void mulliganButton_Click(object sender, EventArgs e)
        {
            int count = this.CurrentGame.players[0].hand.Count;
            if (count != 0)
            {
                for (int i = 0; i < count; i++)
                    this.CurrentGame.players[0].deck.Push(this.CurrentGame.players[0].hand.Pop());
                this.CurrentGame.players[0].shuffle();
                this.CurrentGame.players[0].shuffle();
                try
                {
                    this.CurrentGame.players[0].draw(count - 1);
                }
                catch (Exception ie)
                {
                    if (ie.Message == "ArgumentOutOfRangeException")
                    {
                        this.CurrentGame.gameOver = true;
                    }
                }
            }
            Invalidate(true);
        }

        /**/
        /*
        GameBoard::choicesLaidBox_Paint() GameBoard::choicesLaidBox_Paint()
        NAME
               GameBoard::choicesLaidBox_Paint - handles the drawing of the player searching for something
        DESCRIPTION
               if searching the deck, lays out the whole deck for selection
               if scrying, presents top 2 cards
        RETURNS
                nothing
        */
        /**/
        private void choicesLaidBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (choicesLaidBox.Controls.Count != 0 && !this.CurrentGame.optionalEC) { foreach (Control c in this.choicesLaidBox.Controls) { this.choicesLaidBox.Controls.Remove(c); } }
            if (this.CurrentGame.scrying)
            {
                Player human = this.CurrentGame.players[0];
                Image theCard = Image.FromFile(@"Images\Cards\" + human.deck[0].names[human.deck[0].side] + ".jpg");
                Rectangle tmpCard = new Rectangle();
                tmpCard.Width = (int)(this.choicesLaidBox.Width * .2);
                tmpCard.Height = (int)(tmpCard.Width / .7);
                if (tmpCard.Height > (this.choicesLaidBox.Height * (float).4))
                {
                    tmpCard.Height = (int)(this.choicesLaidBox.Height * (float).7);
                    tmpCard.Width = (int)(tmpCard.Height * .7);
                }
                tmpCard.Location = new Point((this.choicesLaidBox.Location.X + 5), ((int)(this.choicesLaidBox.Height * .1 + this.choicesLaidBox.Location.Y)));
                this.Player1Deck.Push(tmpCard);
                g.DrawImage(theCard, tmpCard);


                theCard = Image.FromFile(@"Images\Cards\" + human.deck[1].names[human.deck[1].side] + ".jpg");
                tmpCard = new Rectangle();
                tmpCard.Width = (int)(this.choicesLaidBox.Width * .2);
                tmpCard.Height = (int)(tmpCard.Width / .7);
                if (tmpCard.Height > (this.choicesLaidBox.Height * (float).4))
                {
                    tmpCard.Height = (int)(this.choicesLaidBox.Height * (float).7);
                    tmpCard.Width = (int)(tmpCard.Height * .7);
                }
                tmpCard.Location = new Point((this.choicesLaidBox.Location.X + 5), ((int)(this.choicesLaidBox.Location.Y + this.Player1Deck[0].Location.Y + this.Player1Deck[0].Size.Height + 5)));
                this.Player1Deck.Push(tmpCard);
                g.DrawImage(theCard, tmpCard);
                theCard.Dispose();
            }
            else if (!this.CurrentGame.optionalEC)
            {
                try
                {
                    Player human = this.CurrentGame.players[0];
                    int deckCount = human.deck.Count;
                    double num = (1.0 / Math.Ceiling((double)(deckCount / 10)));
                    bool first = true;
                    this.Player1Deck = new List<Rectangle>();
                    for (int i = 0; i < deckCount; i++)
                    {
                        Image theCard = Image.FromFile(@"Images\Cards\" + human.deck[i].names[human.deck[i].side] + ".jpg");
                        Rectangle tmpCard = new Rectangle();
                        tmpCard.Width = (int)(this.choicesLaidBox.Width * .08);
                        tmpCard.Height = (int)(tmpCard.Width / .7);
                        if (tmpCard.Height > (this.choicesLaidBox.Height * (float).7))
                        {
                            tmpCard.Height = (int)(this.choicesLaidBox.Height * (float).7);
                            tmpCard.Width = (int)(tmpCard.Height * .7);
                        }

                        if (first)
                            tmpCard.Location = new Point(5, 5);
                        else
                        {
                            double test = i / 10.0;
                            if (test % 1 == 0)
                            {
                                tmpCard.Location = new Point((this.Player1Deck[i - 1].Location.X), ((int)(this.choicesLaidBox.Height * .15 * test)));
                            }
                            else
                                tmpCard.Location = new Point((int)(this.Player1Deck[this.Player1Deck.Count - (i)].Location.X + (int)(this.Player1Deck[this.Player1Deck.Count - (i)].Width)), this.Player1Deck[this.Player1Deck.Count - (i)].Location.Y);
                        }

                        first = false;
                        this.Player1Deck.Push(tmpCard);
                        g.DrawImage(theCard, tmpCard);
                        theCard.Dispose();
                    }
                }
                catch (Exception c)
                {
                    Console.WriteLine(c.Message + " " + c.StackTrace);
                }
            }
        }

        /**/
        /*
        GameBoard::choicesLaidBox_Click() GameBoard::choicesLaidBox_Click()
        NAME
               GameBoard::choicesLaidBox_Click - handles the clicking of the choicedLaidBox
        DESCRIPTION
                iterates through all rectangles to see if the Mouseclick is within a drawn card
                calls checkClick to see what must be done if the player clicked a card
        RETURNS
                nothing
        */
        /**/
        private void choicesLaidBox_Click(object sender, EventArgs e)
        {
            if (!this.CurrentGame.optionalEC)
            {
                int i = 0, X = MousePosition.X, Y = MousePosition.Y;
                Point position = this.PointToScreen(new Point(X, Y));
                int Ydiff = position.Y - Y, Xdiff = position.X - X;
                Card theCard = null;
                foreach (Rectangle paul in this.Player1Deck)
                {
                    if (X > paul.Location.X + this.choicesLaidBox.Location.X + Xdiff && X < paul.Location.X + paul.Width + this.choicesLaidBox.Location.X + Xdiff)
                    {
                        if (Y > paul.Location.Y + this.choicesLaidBox.Location.Y + Ydiff && Y < paul.Location.Y + paul.Height + this.choicesLaidBox.Location.Y + Ydiff)
                        {
                            if (!this.CurrentGame.scrying)
                            {
                                tmpCardChoiceRec = paul;
                                tmpCardChoiceInt = (this.Player1Deck.Count - i - 1);
                                tmpCardZone = 'H';
                                theCard = this.CurrentGame.players[0].deck[(this.Player1Deck.Count - i - 1)];
                            }
                            else
                            {
                                tmpCardChoiceRec = paul;
                                tmpCardChoiceInt = (i);
                                tmpCardZone = 'H';
                                theCard = this.CurrentGame.players[0].deck[i];
                            }

                        }
                    }
                    i++;
                }
                if (theCard != null)
                {
                    CheckClick(theCard);
                }
            }
            Invalidate(true);
        }

        /**/
        /*
        GameBoard::choiceMenu_ItemClicked() GameBoard::choiceMenu_ItemClicked()
        NAME
               GameBoard::choiceMenu_ItemClicked - handles the clicking of some item in the menu
        DESCRIPTION
                iterates through each item to see if the Mouseclick which effect/decision was made
                calls appropriete functions depending on what choice
        RETURNS
                nothing
        */
        /**/
        private void choiceMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                this.choicesLaidBox.Enabled = false;
                this.choicesLaidBox.Visible = false;
                this.tmpActivatedInt = int.Parse(((ToolStripItem)e.ClickedItem).Name);
                Card tmp = this.CurrentGame.players[this.CurrentGame.actPlayer].permanents[tmpCardChoiceInt];
                if (!string.IsNullOrEmpty(tmp.sides[tmp.side].effects[0].whatDo[6]) && (tmp.sides[tmp.side].effects[0].modal))
                {
                    if (this.CurrentGame.targets.Count != 0)
                    {
                        this.CurrentGame.modalArray.Push(tmp.sides[0].effects[tmpActivatedInt]);
                        try {
                            this.CurrentGame.resolveEffect(tmp.sides[0].effects[0], tmp.controller);
                        }
                        catch (Exception ie)
                        {
                            if (ie.Message == "ArgumentOutOfRangeException")
                            {
                                this.CurrentGame.gameOver = true;
                            }
                        }
                    }
                    else
                    {
                        this.CurrentGame.modalArray.Push(tmp.sides[0].effects[tmpActivatedInt]);
                        this.choiceMenu.Items.RemoveAt(tmpActivatedInt - 1);
                    }
                }
                if (tmp.sides[tmp.side].effects[tmpActivatedInt].manaCost.Count != 0)
                {
                    this.paying = true;
                    this.tmpPayingInt = this.tmpCardChoiceInt;
                    this.tmpPayingZone = 'B';
                    this.tmpPayingActivatedInt = int.Parse(((ToolStripItem)e.ClickedItem).Name);
                }
                else if (tmp.sides[tmp.side].effects[tmpActivatedInt].whatDo[2].ToLower() == "search")
                {
                    try {
                        this.CurrentGame.resolveEffect(tmp.sides[tmp.side].effects[tmpActivatedInt], tmp.controller);
                    }
                    catch (Exception ie)
                    {
                        if (ie.Message == "ArgumentOutOfRangeException")
                        {
                            this.CurrentGame.gameOver = true;
                        }
                    }
                    if (tmp.sides[tmp.side].effects[tmpActivatedInt].costs[0] == "Sacrifice")
                    {
                        this.CurrentGame.players[0].permanents.Remove(tmp);
                        tmp.changeZones(Zone.Graveyard);
                        this.CurrentGame.players[0].graveyard.Push(tmp);
                    }
                }
                else if (tmp.sides[tmp.side].effects[tmpActivatedInt].manaCost.Count() == 0)
                {
                    this.CurrentGame.resolveActivatedAbility(ref tmp, this.CurrentGame.players[this.CurrentGame.actPlayer].permanents[tmpCardChoiceInt].sides[0].effects[tmpActivatedInt]);
                    this.CurrentGame.players[this.CurrentGame.actPlayer].permanents[tmpCardChoiceInt] = tmp;
                }
                else
                {
                    this.paying = true;
                    this.tmpPayingInt = this.tmpCardChoiceInt;
                    this.tmpPayingActivatedInt = int.Parse(((Button)sender).Name);
                }
                for (int i = this.choiceMenu.Items.Count; i > 0; i--)
                    this.choiceMenu.Items.Remove(this.choiceMenu.Items[0]);
                this.choiceMenu.Hide();
                tmpChoices = new List<Button>();
                Invalidate(true);
                return;
            }
            catch
            {
                string ans = ((ToolStripItem)e.ClickedItem).Name;
                if (ans == "Yes")
                {
                    List<Mana> tmp = new List<Mana>(this.CurrentGame.cpu.getCost(this.CurrentGame.players[0].hand[tmpCardChoiceInt].sides[0].optionalCosts[1]));
                    foreach (Mana m in tmp)
                    {
                        this.CurrentGame.players[0].hand[tmpCardChoiceInt].sides[0].manacost.Push(m);
                    }
                    this.CurrentGame.players[0].hand[tmpCardChoiceInt].sides[0].additionalPaid = true;
                    paying = true;
                    tmpPayingZone = tmpCardZone;
                    tmpPayingInt = tmpCardChoiceInt;
                }
                else
                {
                    paying = true;
                    tmpPayingZone = tmpCardZone;
                    tmpPayingInt = tmpCardChoiceInt;
                    this.CurrentGame.players[0].hand[tmpPayingInt].sides[0].additionalPaid = false;
                }
                this.CurrentGame.optionalEC = false;
                this.CurrentGame.optionalEF = false;
                this.choicesLaidBox.Controls.Clear();
                this.choicesLaidBox.Enabled = false;
                this.choicesLaidBox.Visible = false;
                Invalidate(true);
            }
        }

        /**/
        /*
        GameBoard::priorityButton_Click() GameBoard::priorityButton_Click()
        NAME
               GameBoard::priorityButton_Click - handles the clicking of the priority button
        DESCRIPTION
                All purpose function to start game when done mulling, passing priority to comp,
                etc
        RETURNS
                nothing
        */
        /**/
        private void priorityButton_Click(object sender, EventArgs e)
        {
            //this.CurrentGame.togglePriority();
            if (this.CurrentGame.mulling)
            {
                this.mulliganButton.Enabled = false;
                this.mulliganButton.Visible = false;
                this.CurrentGame.mulling = false;
                return;
            }
            this.priorityButton.Enabled = false;

            this.passTurnButton.Enabled = false;
            this.priorityButton.Invalidate();
            this.passTurnButton.Invalidate();

            this.CurrentGame.payAdd = false;
            this.tmpCardChoiceRec = new Rectangle();

            Invalidate(true);
            if (this.CurrentGame.stack.Count != 0)
            {
                this.CurrentGame.resolveStack();
            }
            else if (!this.CurrentGame.attacking && !this.CurrentGame.blocking)
            {
                if ((this.CurrentGame.resolving) && (this.CurrentGame.minTargets <= this.CurrentGame.targets.Count))
                {
                    this.paying = false;
                    this.tmpPayingInt = -1;
                    this.tmpPayingActivatedInt = -1;
                    this.choicesLaidBox.Enabled = false;
                    this.choicesLaidBox.Visible = false;
                    this.CurrentGame.finishEffect();
                }
                else if (this.paying)
                {
                    this.paying = false;
                    this.CurrentGame.payAdd = false;
                    this.CurrentGame.optionalEC = false;
                    this.CurrentGame.players[0].hand[tmpPayingInt].sides[0].additionalPaid = false;
                    this.CurrentGame.players[0].hand[tmpPayingInt].sides[0].manacost = this.CurrentGame.cpu.getCost(this.CurrentGame.players[0].hand[tmpPayingInt].sides[0].manaCost);
                    this.tmpCardChoiceRec = new Rectangle();
                    this.tmpCardChoiceInt = -1;
                    this.tmpPayingInt = -1;
                    this.tmpPayingZone = ' ';
                    this.tmpPayingActivatedInt = -1;
                    for (int k = this.CurrentGame.players[this.CurrentGame.actPlayer].manaSpending.Count - 1; k > -1; k--) { this.CurrentGame.players[this.CurrentGame.actPlayer].manapool.Push(this.CurrentGame.players[this.CurrentGame.actPlayer].manaSpending.PopAt(k)); }
                    this.CurrentGame.players[0].manaSpending = new List<Mana>();
                }
                else if (this.CurrentGame.stack.Count == 0)
                {
                    this.paying = false;
                    this.tmpPayingInt = -1;
                    this.tmpPayingActivatedInt = -1;
                    for (int k = this.CurrentGame.players[this.CurrentGame.actPlayer].manapool.Count() - 1; k > -1; k--) { this.CurrentGame.players[this.CurrentGame.actPlayer].manapool.PopAt(k); }
                    if (this.CurrentGame.passed)
                    {
                        this.CurrentGame.passed = false;
                        this.CurrentGame.togglePriority();
                        this.CurrentGame.incPhase();
                        this.executeComp();
                    }
                    else
                    {
                        this.CurrentGame.passed = true;
                        this.CurrentGame.togglePriority();
                        this.executeComp();
                    }
                }
                else
                {
                    this.paying = false;
                    this.tmpPayingInt = -1;
                    this.tmpPayingActivatedInt = -1;
                    if (this.CurrentGame.passed)
                    {
                        this.CurrentGame.passed = false;
                        this.CurrentGame.togglePriority();
                        this.CurrentGame.finishEffect();
                        this.executeComp();
                    }
                    this.CurrentGame.passed = true;
                    this.CurrentGame.togglePriority();
                    this.executeComp();
                }
            }
            //Done Assigning
            else if (this.CurrentGame.attacking)
            {
                this.paying = false; this.tmpPayingInt = -1;
                this.tmpPayingActivatedInt = -1;

                this.CurrentGame.attacking = false;
                this.messageBox.AppendText(this.CurrentGame.attackers.Count() + " attackers declared.");
            }
            else if (this.CurrentGame.blocking)
            {
                this.paying = false;
                this.tmpPayingInt = -1;
                this.tmpPayingActivatedInt = -1;
                this.CurrentGame.blocking = false;
                this.messageBox.AppendText(this.CurrentGame.attackers.Count() + " blockers declared.");
            }
            Invalidate(true);
        }

        /**/
        /*
        GameBoard::passTurnButton_Click() GameBoard::passTurnButton_Click()
        NAME
               GameBoard::passTurnButton_Click - handles the clicking of the passTurn button
        DESCRIPTION
                Calls the Game function passTurn()
        RETURNS
                nothing
        */
        /**/
        private void passTurnButton_Click(object sender, EventArgs e)
        {
            this.CurrentGame.passed = false;
            this.CurrentGame.passTurn();
            Invalidate(true);
        }

        /**/
        /*
        GameBoard::targetCompButton_Click() GameBoard::targetCompButton_Click()
        NAME
               GameBoard::targetCompButton_Click - handles the clicking of the targetCompButton 
        DESCRIPTION
                Pushes the Comp's player identifier onto the Game's target list.
                UI displays appropriete message
        RETURNS
                nothing
        */
        /**/
        private void targetCompButton_Click(object sender, EventArgs e)
        {
            this.targetCompButton.Enabled = false;
            this.targetCompButton.Visible = false;
            if (this.CurrentGame.targets.FindIndex(x => x == this.CurrentGame.players[1].identifier) > -1)
            {
                if (this.CurrentGame.toBe[0].whatDo[6] == "Bolt")
                    theRecord.AppendText("Scratch that, two will be dealt instead.\n");
            }
            else
            {
                if (this.CurrentGame.toBe[0].whatDo[6] == "Bolt")
                    theRecord.AppendText("1 Damage will be dealt to comp\n");
                else
                    theRecord.AppendText("You've targeted the comp. What did he ever do to you?\n");
            }
            this.CurrentGame.targets.Push(this.CurrentGame.players[1].identifier);
            CheckClick(null);
            return;
        }

        /**/
        /*
        GameBoard::CheckClick() GameBoard::CheckClick()
        NAME
               GameBoard::CheckClick - is passed a card to see if the clicking of such a card does anything
        DESCRIPTION
                If the game is resolving, the player is targetting and handles that approporeitly.
                Otherwise runs through possbility of combat, casting cards, and activating abilites.
                Decides via what zone a card is in and what effects it has. Executes UI changes as appropriete,
                passes along changes to Game, such as pushing targets into CurrentGame.targets<Guid>
        RETURNS
                nothing
        */
        /**/
        private void CheckClick(Card theCard)
        {
            if (this.CurrentGame.searching && !this.CurrentGame.resolving)
            {
                this.CurrentGame.targets.Push(theCard.instance);
                this.CurrentGame.searchFor(Zone.Hand);
                this.choicesLaidBox.Enabled = false;
                this.choicesLaidBox.Visible = false;
                this.CurrentGame.searching = false;
            }
            //a spell is resolving, player must have been selecting targets
            else if (this.CurrentGame.resolving)
            {
                switch (this.CurrentGame.targetType)
                {
                    //rp == permanent (regular)
                    case "rp":
                        if (tmpCardZone == 'B') { this.CurrentGame.targets.Push(theCard.instance); }
                        break;
                    //pl == player
                    case "pl":
                        break;
                    //cc == creature card
                    case "cc":
                        if (tmpCardZone == 'G' && theCard.sides[theCard.side].types[0].Contains("Creature")) { this.CurrentGame.targets.Push(theCard.instance); }
                        break;
                    //nc == nonblack creature
                    case "nb":
                        if (theCard.sides[theCard.side].types[0].Contains("Creature") && !theCard.sides[theCard.side].colors.Contains("Black")) { this.CurrentGame.targets.Push(theCard.instance); }
                        break;
                    //dc == colorless creature (devoid creature)
                    case "dc":
                        if (theCard.sides[theCard.side].types[0].Contains("Creature") && string.IsNullOrEmpty(theCard.sides[theCard.side].colors[0])) { this.CurrentGame.targets.Push(theCard.instance); }
                        break;
                    //cl == creatures/players
                    case "cl":
                        if ((theCard != null) && (theCard.sides[theCard.side].types.Contains("Creature"))) { this.CurrentGame.targets.Push(theCard.instance); }
                        break;
                    //rc == creature (regular)
                    case "rc":
                        if (theCard.sides[theCard.side].types.Contains("Creature")) { this.CurrentGame.targets.Push(theCard.instance); }
                        break;
                    //np == noncreature permanent
                    case "np":
                        if (!theCard.sides[theCard.side].types.Contains("Creature")) { this.CurrentGame.targets.Push(theCard.instance); }
                        break;
                    //tc == tapped creature
                    case "tc":
                        if (theCard.sides[theCard.side].types.Contains("Creature") && theCard.tapped) { this.CurrentGame.targets.Push(theCard.instance); }
                        break;
                    //sc == creature w/ cmc <= 3 (small creature)
                    case "sc":
                        if (theCard.sides[theCard.side].types.Contains("Creature") && (theCard.sides[theCard.side].manacost.Count < 4)) { this.CurrentGame.targets.Push(theCard.instance); }
                        break;
                    //fc == flying creature
                    case "fc":
                        if (theCard.sides[theCard.side].types.Contains("Creature") && theCard.sides[theCard.side].keywordAbilities.Contains("Flying")) { this.CurrentGame.targets.Push(theCard.instance); }
                        break;
                    //ms == mountain/swamp
                    case "ms":
                        if (theCard.sides[theCard.side].subtypes.Contains("Mountain") || theCard.sides[theCard.side].subtypes.Contains("Swamp")) { this.CurrentGame.targets.Push(theCard.instance); }
                        break;
                    //bl == basic land
                    case "bl":
                        if (theCard.sides[theCard.side].supertypes.Contains("Basic")) { this.CurrentGame.targets.Push(theCard.instance); }
                        break;
                    case "card":
                        this.CurrentGame.targets.Push(theCard.instance);
                        break;
                }
                //the user cannot select more targets, just resolve now
                if (this.CurrentGame.targets.Count() == this.CurrentGame.maxTargets)
                {
                    this.CurrentGame.finishEffect();
                    this.choicesLaidBox.Visible = false;
                    this.choicesLaidBox.Enabled = false;
                    Invalidate(true);
                }
            }
            //the player is declaring attackers
            else if (this.CurrentGame.attacking)
            {
                //if creature is not attacking already and is able, it attacks
                if (theCard.sides[0].types[0] == "Creature" && tmpCardZone == 'B' && this.CurrentGame.actPlayer == 0 && theCard.tapped == false)
                {
                    this.theRecord.AppendText(theCard.sides[theCard.side].name + " is now attacking.\n");
                    this.CurrentGame.players[0].permanents[tmpCardChoiceInt].tapped = true;
                    this.CurrentGame.attackingattackers.Push(theCard.instance);
                }
                //if it was already targetting, assuming the Player changed their mind; creature no longer attacking
                else if (this.CurrentGame.attackingattackers.Contains(theCard.instance))
                {
                    this.CurrentGame.attackingattackers.Remove(theCard.instance);
                    this.CurrentGame.players[0].permanents[tmpCardChoiceInt].tapped = false;
                    this.theRecord.AppendText(theCard.sides[theCard.side].name + " is no longer attcking.");
                    this.theRecord.AppendText(Environment.NewLine);
                }
            }
            //declaring blocks
            else if (this.CurrentGame.blocking)
            {
                //see if its a legal blocker
                if (theCard.sides[0].types[0] == "Creature" && (this.CurrentGame.attackers.Count() == this.CurrentGame.blockers.Count()) && tmpCardZone == 'B' && this.CurrentGame.actPlayer == 1 && theCard.tapped == false)
                {
                    //if creature was already blocking, they must be changing the target, otherwise just continue pushing onto blockers
                    int k = this.CurrentGame.blockers.FindIndex(x => x == theCard.instance);
                    if (k > -1)
                    {
                        this.CurrentGame.blockers.RemoveAt(k);
                        this.CurrentGame.attackers.RemoveAt(k);
                    }
                    this.CurrentGame.blockers.Push(theCard.instance);
                }
                //if clicking opponents creatures, that must be the attacker they're blocking
                else if (theCard.sides[0].types[0] == "Creature" && (this.CurrentGame.attackers.Count() != this.CurrentGame.blockers.Count()) && tmpCardZone == 'b' && this.CurrentGame.actPlayer == 1)
                {
                    this.CurrentGame.attackers.Push(theCard.instance);
                    this.theRecord.AppendText(this.CurrentGame.players[0].permanents[this.CurrentGame.players[0].permanents.FindIndex(x => x.instance == this.CurrentGame.blockers[0])].sides[0].name + " is now blocking " + theCard.sides[theCard.side].name);
                }
            }
            //the player cannot do anything but discard cards
            else if (this.CurrentGame.cleanup)
            {
                if (tmpCardZone == 'H')
                {
                    this.CurrentGame.discard(tmpCardChoiceInt);
                }
            }
            //handles playing lands or casting cards, handling optional effects as well
            else if ((theCard.currentZone == Zone.Hand) && (this.CurrentGame.canPlay(theCard)) && !paying)
            {
                //if its a land, just play it
                if (theCard.sides[theCard.side].types[0] == "Land")
                {
                    this.CurrentGame.Play(theCard);
                    this.theRecord.AppendText("You play " + theCard.names[0] + "\n");
                }
                //if there are optional costs, display questionaire
                else if (!string.IsNullOrEmpty(theCard.sides[theCard.side].optionalCosts[0]))
                {
                    this.CurrentGame.optionalEC = true;
                    TextBox number = new TextBox();
                    number.Enabled = false;
                    number.ReadOnly = true;
                    number.Text = "Do you wish to pay an extra " + theCard.sides[0].manaCost + " for the " + theCard.sides[0].effects[0].whatDo[6] + " cost?";
                    number.Size = TextRenderer.MeasureText(number.Text, number.Font);
                    number.Location = new Point((0), (int)(40));
                    this.choicesLaidBox.Controls.Add(number);

                    this.choiceMenu.Items.Add("Yes");
                    this.choiceMenu.Items[this.choiceMenu.Items.Count - 1].Name = "Yes";
                    this.choiceMenu.Items.Add("No");
                    this.choiceMenu.Items[this.choiceMenu.Items.Count - 1].Name = "No";
                    this.choiceMenu.Show(new Point(number.Location.X + this.choicesLaidBox.Location.X + 10, number.Size.Height + this.choiceMenu.Size.Height + 50));
                }
                //if its modal, they need to select which effects get pushed
                else if (theCard.sides[theCard.side].effects[0].modal)
                {
                    this.CurrentGame.optionalEC = true;
                    TextBox number = new TextBox();
                    number.Enabled = false;
                    number.ReadOnly = true;
                    number.Text = "Please choose two effects:";
                    number.Size = TextRenderer.MeasureText(number.Text, number.Font);
                    number.Location = new Point(0, (int)(40));
                    this.choicesLaidBox.Controls.Add(number);
                    for (int i = 1; i < 5; i++)
                    {
                        this.choiceMenu.Items.Add(theCard.sides[theCard.side].effects[1].whatDo[2] + " " + theCard.sides[theCard.side].effects[1].whatDo[5]);
                        this.choiceMenu.Items[this.choiceMenu.Items.Count - 1].Name = i.ToString();
                    }
                    this.choiceMenu.Show(new Point(number.Location.X + this.choicesLaidBox.Location.X, number.Size.Height + this.choiceMenu.Size.Height + 50));
                }
                //there are additional costs, they need to meet those costs
                else if (!string.IsNullOrEmpty(theCard.sides[theCard.side].additionalCosts[0]))
                {
                    this.CurrentGame.payAdd = true;
                    if (theCard.sides[0].additionalCosts[0] == "Sacrifice")
                    {
                        this.CurrentGame.targetType = "rl";
                        this.CurrentGame.targets = new List<Guid>();
                    }
                    else
                    {
                        this.CurrentGame.targetType = "hc";
                        this.CurrentGame.targets = new List<Guid>();
                    }
                }
                //they're just casting a normal card
                else
                {
                    paying = true;
                    tmpPayingZone = tmpCardZone;
                    tmpPayingInt = tmpCardChoiceInt;
                }
            }
            //if they were in the middle of paying for something they may have changed their mind
            else if ((tmpCardZone == 'H') && paying)
            {
                this.paying = false;
                this.CurrentGame.payAdd = false;
                this.CurrentGame.optionalEC = false;
                this.CurrentGame.players[0].hand[tmpPayingInt].sides[0].additionalPaid = false;
                this.CurrentGame.players[0].hand[tmpPayingInt].sides[0].manacost = this.CurrentGame.cpu.getCost(this.CurrentGame.players[0].hand[tmpPayingInt].sides[0].manaCost);
                this.tmpCardChoiceRec = new Rectangle();
                this.tmpCardChoiceInt = -1;
                this.tmpPayingInt = -1;
                this.tmpPayingZone = ' ';
                this.tmpPayingActivatedInt = -1;
                //give them their mana back
                for (int k = this.CurrentGame.players[this.CurrentGame.actPlayer].manaSpending.Count - 1; k > -1; k--) { this.CurrentGame.players[this.CurrentGame.actPlayer].manapool.Push(this.CurrentGame.players[this.CurrentGame.actPlayer].manaSpending.PopAt(k)); }
            }
            //the card is on the battlefield, must be activating an ability
            //list out the choices
            else if (theCard.currentZone == Zone.Battlefield)
            {
                int i = 0;
                if (theCard.tapped) return;
                foreach (Effect paul in theCard.sides[theCard.side].effects)
                {
                    if (paul.effect == EffectType.ActivatedAbility)
                    {
                        string tmp = "";
                        if (paul.whatDo[2].ToLower() == "add")
                        {
                            if ((paul.whatDo[5].Length == 1) || ((paul.whatDo[5].Length == 2) && (paul.whatDo[5][0] != paul.whatDo[5][1])))
                            {
                                foreach (char c in (paul.whatDo[5]))
                                {
                                    switch (int.Parse(c.ToString()))
                                    {
                                        case 1:
                                            tmp = "Add {W} to mana pool";
                                            break;
                                        case 2:
                                            tmp = "Add {U} to mana pool";
                                            break;
                                        case 3:
                                            tmp = "Add {B} to mana pool";
                                            break;
                                        case 4:
                                            tmp = "Add {R} to mana pool";
                                            break;
                                        case 5:
                                            tmp = "Add {G} to mana pool";
                                            break;
                                        case 6:
                                            tmp = "Add {C} to mana pool";
                                            break;
                                    }
                                    this.choiceMenu.Items.Add(tmp);
                                    this.choiceMenu.Items[this.choiceMenu.Items.Count - 1].Name = i.ToString();
                                    i++;
                                }
                            }
                            else
                            {
                                tmp = "Add {C}{C} to mana pool. This can only be used to cast Eldrazi spells and activate abilities of Eldrazi.";
                                this.choiceMenu.Items.Add(tmp);
                                this.choiceMenu.Items[this.choiceMenu.Items.Count - 1].Name = i.ToString();
                                i++;
                            }
                        }
                        else
                        {
                            if (paul.costs[0] != null && paul.manaCost.Count() != 0)
                            {
                                if (paul.tapCost)
                                    tmp += "{T}, ";
                                foreach (Mana m in paul.manaCost)
                                {
                                    tmp += "{" + m.color.ToString() + "}";
                                }
                                tmp += ", " + paul.costs[0] + " " + paul.costs[1] + ": " + paul.whatDo[2] + " " + paul.whatDo[5];
                            }
                            else if (paul.costs[0] != null)
                            {
                                if (paul.tapCost)
                                    tmp += "{T}, ";
                                tmp += paul.costs[0] + " " + paul.costs[1] + ": " + paul.whatDo[2] + " " + paul.whatDo[5];
                            }
                            else if (paul.manaCost.Count() != 0)
                            {
                                if (paul.tapCost)
                                    tmp += "{T}, ";
                                foreach (Mana m in paul.manaCost)
                                {
                                    tmp += "{" + m.color.ToString() + "}";
                                }
                                tmp += ": " + paul.whatDo[2] + " " + paul.whatDo[5];
                            }
                            this.choiceMenu.Items.Add(tmp);
                            this.choiceMenu.Items[this.choiceMenu.Items.Count - 1].Name = i.ToString();
                            i++;
                        }
                    }
                    else if (paul.whatDo[6] == "Level")
                    {
                        if (this.CurrentGame.canLevel())
                        {
                            string tmp = "";
                            foreach (Mana m in paul.manaCost)
                            {
                                tmp += "{" + m.color.ToString() + "}";
                            }
                            tmp += ": put a Level counter on " + theCard.names[0];
                            this.choiceMenu.Items.Add(tmp);
                            this.choiceMenu.Items[this.choiceMenu.Items.Count - 1].Name = i.ToString();
                        }
                        i++;
                    }
                    else
                        i++;
                }
            }
        }

        /**/
        /*
        GameBoard::checkPaymentCompletion() GameBoard::checkPaymentCompletion()
        NAME
               GameBoard::checkPaymentCompletion - checks to see if payment is complete
        DESCRIPTION
                If the mana in Player1.spendingMana satifies the mana requirements in cost, then
                the card can be played/cast
        RETURNS
                nothing
        */
        /**/
        private void checkPaymentCompletion()
        {
            if (tmpPayingZone == 'B' || tmpPayingZone == 'b')
            {
                if (this.CurrentGame.players[0].manaSpending.Count() == this.CurrentGame.players[this.CurrentGame.actPlayer].permanents[tmpPayingInt].sides[0].effects[this.tmpPayingActivatedInt].manaCost.Count())
                {
                    List<Mana> tmpSpending = new List<Mana>(this.CurrentGame.players[0].manaSpending);
                    List<Mana> tmpPaying = new List<Mana>(this.CurrentGame.players[this.CurrentGame.actPlayer].permanents[tmpPayingInt].sides[0].effects[this.tmpPayingActivatedInt].manaCost);
                    for (int j = this.CurrentGame.players[0].manaSpending.Count() - 1; j > -1; j--)
                    {
                        for (int k = tmpSpending.Count() - 1; k > -1; k--)
                        {
                            if (tmpSpending[k].color == tmpPaying[j].color)
                            {
                                tmpSpending.PopAt(k);
                                tmpPaying.PopAt(j);
                            }
                        }
                    }
                    //if paying only contains generic, and has the same length of spending, we're good
                    if ((tmpSpending.Count() == 0) || ((tmpPaying.FindIndex(x => x.color == 1) < 0) && (tmpPaying.FindIndex(x => x.color == 2) < 0) && (tmpPaying.FindIndex(x => x.color == 3) < 0) && (tmpPaying.FindIndex(x => x.color == 4) < 0) && (tmpPaying.FindIndex(x => x.color == 5) < 0)))
                    {
                        Card tmp = this.CurrentGame.players[this.CurrentGame.actPlayer].permanents[tmpPayingInt];
                        try {
                            this.CurrentGame.resolveEffect(tmp.sides[0].effects[this.tmpPayingActivatedInt], 0);
                        }
                        catch (Exception ie)
                        {
                            if (ie.Message == "ArgumentOutOfRangeException")
                            {
                                this.CurrentGame.gameOver = true;
                            }
                        }
                        if ((!string.IsNullOrEmpty(tmp.sides[0].effects[this.tmpPayingActivatedInt].costs[0])) && (tmp.sides[0].effects[this.tmpPayingActivatedInt].costs[0].ToLower() == "sacrifice") && (tmp.sides[0].effects[this.tmpPayingActivatedInt].costs[1] == "~"))
                        {
                            this.CurrentGame.players[0].permanents.Remove(tmp);
                            tmp.changeZones(Zone.Graveyard);
                            this.CurrentGame.players[0].graveyard.Push(tmp);
                        }
                        else if (tmp.sides[0].effects[tmpPayingActivatedInt].whatDo[6] == "Level")
                        {
                            this.CurrentGame.targets.Push(tmp.instance);
                        }
                        this.paying = false;
                        this.tmpCardChoiceRec = new Rectangle();
                        this.tmpPayingInt = -1;
                        this.tmpPayingActivatedInt = -1;
                        this.tmpPayingZone = ' ';
                        this.CurrentGame.players[0].manaSpending = new List<Mana>();
                    }
                }
            }
            else if (tmpPayingZone == 'H')
            {
                if (this.CurrentGame.players[0].manaSpending.Count() == this.CurrentGame.players[this.CurrentGame.actPlayer].hand[tmpPayingInt].sides[0].manacost.Count())
                {
                    List<Mana> tmpSpending = new List<Mana>(this.CurrentGame.players[0].manaSpending);
                    List<Mana> tmpPaying = new List<Mana>(this.CurrentGame.players[this.CurrentGame.actPlayer].hand[tmpPayingInt].sides[0].manacost);
                    for (int j = tmpPaying.Count() - 1; j > -1; j--)
                    {
                        for (int k = tmpSpending.Count() - 1; k > -1; k--)
                        {
                            if (tmpSpending[k].color == tmpPaying[j].color)
                            {
                                tmpSpending.PopAt(k);
                                tmpPaying.PopAt(j);
                                break;
                            }
                        }
                    }

                    //if paying only contains generic, and has the same length of spending, we're good
                    if ((tmpSpending.Count() == 0) || ((tmpPaying.FindIndex(x => x.color == 1) < 0) && (tmpPaying.FindIndex(x => x.color == 2) < 0) && (tmpPaying.FindIndex(x => x.color == 3) < 0) && (tmpPaying.FindIndex(x => x.color == 4) < 0) && (tmpPaying.FindIndex(x => x.color == 5) < 0) && (tmpPaying.FindIndex(x => x.color == 6) < 0)))
                    {
                        Card tmp = this.CurrentGame.players[this.CurrentGame.actPlayer].hand[tmpPayingInt];
                        this.CurrentGame.Play(tmp);
                        this.theRecord.AppendText("You cast " + tmp.names[0] + "\n");
                        this.paying = false;
                        this.tmpCardChoiceRec = new Rectangle();
                        this.tmpPayingInt = -1;
                        this.tmpPayingZone = ' ';
                        this.tmpPayingActivatedInt = -1;
                        this.CurrentGame.players[0].manaSpending = new List<Mana>();
                    }
                }
            }
        }

        //AI FUNCTIONS
        /**/
        /*
        GameBoard::executeComp() GameBoard::executeComp()
        NAME
               GameBoard::executeComp - executes the computer's turn
        DESCRIPTION
                Meat of functions are in Game, but this handles the display of 
                outcome return values and the order of execution that is triggered
                by the passPriority button
        RETURNS
                nothing
        */
        /**/
        public void executeComp()
        {
            //the comp will not do anything if its not the first main phase on its turn and not declaring attacks/blocks
            if ((this.CurrentGame.phase != 3 && !this.CurrentGame.attacking && !this.CurrentGame.blocking) || this.CurrentGame.actPlayer != 1)
            {
                this.CurrentGame.togglePriority();
                if (this.CurrentGame.passed)
                {
                    this.CurrentGame.passed = false;
                    this.CurrentGame.incPhase();
                    Invalidate(true);
                    return;
                }
                this.CurrentGame.passed = true;
                Invalidate(true);
                return;
            }
            //the player played a spell, better not do anything
            else if (this.CurrentGame.stack.Count != 0)
            {
                this.CurrentGame.togglePriority();
                if (this.CurrentGame.passed)
                {
                    this.CurrentGame.passed = false;
                    this.CurrentGame.finishEffect();
                    Invalidate(true);
                    return;
                }
                this.CurrentGame.passed = true;
                Invalidate(true);
                return;
            }
            //declare attackers
            else if (this.CurrentGame.attacking)
            {
                this.CurrentGame.compAttack();
                this.CurrentGame.attacking = false;
                Refresh();
                this.executeComp();
                return;
            }
            //declare blockers
            else if (this.CurrentGame.blocking)
            {
                this.CurrentGame.compBlock();
                this.CurrentGame.blocking = false;
                Invalidate(true);
                return;
            }
            //execute main phase plays
            else if (this.CurrentGame.phase == 3)
            {
                //Step 1: Play and crack all lands we can
                string outcome;
                this.CurrentGame.compPlayLands(out outcome);
                if (!string.IsNullOrEmpty(outcome))
                {
                    theRecord.AppendText(outcome + "\n");
                    Refresh();
                }
                outcome = "";
                this.CurrentGame.compCrackLands(out outcome);
                if (!string.IsNullOrEmpty(outcome))
                {
                    theRecord.AppendText(outcome + "\n");
                    Refresh();
                }
                //Step 2: Play all creatures we can
                outcome = "";
                this.CurrentGame.compPlayCreatures(out outcome);
                if (!string.IsNullOrEmpty(outcome))
                {
                    theRecord.AppendText(outcome + "\n");
                    Refresh();
                }
                Refresh();
                //Step 3: Play all non-creatures we can
                outcome = "";
                this.CurrentGame.compPlayNon(out outcome);
                if (!string.IsNullOrEmpty(outcome))
                {
                    theRecord.AppendText(outcome + "\n");
                    Refresh();
                }
                Refresh();
                //Step 4: Pass turn
                this.CurrentGame.togglePriority();
                this.CurrentGame.passed = true;
                return;
            }
        }
    }
}
