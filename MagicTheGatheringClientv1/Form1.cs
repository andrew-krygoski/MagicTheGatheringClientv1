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
    public partial class Form1 : Form
    {
        /**/
        /*
        Form1::Form1() Form1::Form1()
        NAME
                Form1::Form1 - initiates the starting page
        SYNOPSIS
                public Form1::Form1( string results );
                    results          --> the player must have either quit the game or it ended. 
        DESCRIPTION
                displays start screen, displays results if someone wins
        RETURNS
                nothing
        */
        /**/
        public Form1(string results)
        {
            InitializeComponent();
            this.Location = new Point(0, 0);
            this.Size = Screen.PrimaryScreen.WorkingArea.Size;

            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);

            if(!string.IsNullOrEmpty(results))
            {
                this.resultsTextBox.Text = results;
                this.resultsTextBox.Enabled = true;
                this.resultsTextBox.Visible = true;
                this.resultsTextBox.Size = TextRenderer.MeasureText(this.resultsTextBox.Text, this.resultsTextBox.Font);
                this.resultsTextBox.Location = new Point(this.quitButton.Location.X - (this.resultsTextBox.Width / 2), this.quitButton.Location.Y + this.quitButton.Size.Height + 10);
            }
        }

        /**/
        /*
        Form1::Form1_Resize() Form1::Form1_Resize()
        NAME
                Form1::Form1_Resize - handles the resizing of the form
        DESCRIPTION
                resets the positions of controls based off of the new dimensions
        RETURNS
                nothing
        */
        /**/
        private void Form1_Resize(object sender, EventArgs e)
        {
            this.backgroundImg.Size = this.Size;
            this.backgroundImg.Location = new Point(0, 0);
            if (this.Size.Width * .6 > 550)
                this.logoImg.Size = new Size(550, 136);
            else
                this.logoImg.Size = new Size((int)(this.Size.Width * .6), (int)((this.Size.Width * .6 )/ 4));
            this.startNGButton.Location = new Point(this.Width / 2 - this.startNGButton.Width / 2, this.Height / 2 - this.startNGButton.Height);
            this.logoImg.Location = new Point((int)((this.Size.Width * .5) - (this.logoImg.Size.Width / 2)), this.startNGButton.Location.Y - this.logoImg.Height - 10);
            this.zendiButton.Location = new Point(this.startNGButton.Location.X - (this.zendiButton.Width) + 10, this.startNGButton.Location.Y);
            this.eldraziButton.Location = new Point(this.startNGButton.Location.X + (this.eldraziButton.Width ) + 10, this.startNGButton.Location.Y);
            this.quitButton.Location = new Point(this.startNGButton.Location.X, this.startNGButton.Location.Y + this.quitButton.Size.Height + 10);
            this.resultsTextBox.Size = TextRenderer.MeasureText(this.resultsTextBox.Text, this.resultsTextBox.Font);
            this.resultsTextBox.Location = new Point(this.quitButton.Location.X, this.quitButton.Location.Y + this.quitButton.Size.Height + 10);
        }

        /**/
        /*
        Form1::Form1_Resize() Form1::Form1_Resize()
        NAME
                Form1::Form1_Resize - handles the resizing of the form
        DESCRIPTION
                resets the positions of controls based off of the new dimensions
        RETURNS
                nothing
        */
        /**/
        private void startNGButton_Click(object sender, EventArgs e)
        {
            this.eldraziButton.Enabled = true;
            this.eldraziButton.Visible = true;
            this.zendiButton.Enabled = true;
            this.zendiButton.Visible = true;
            this.startNGButton.Visible = false;
        }

        /**/
        /*
        Form1::eldraziButton_Click() Form1::eldraziButton_Click()
        NAME
                Form1::eldraziButton_Click - handles the clicking of the Eldrazi choice button
        DESCRIPTION
                creates a new instance of gameboard with the player playing the eldrazi deck
        RETURNS
                nothing
        */
        /**/
        private void eldraziButton_Click(object sender, EventArgs e)
        {
            GameBoard newgame = new GameBoard(false);
            this.Hide();
            newgame.ShowDialog();
            this.Close();
        }

        /**/
        /*
        Form1::zendiButton_Click() Form1::zendiButton_Click()
        NAME
                Form1::zendiButton_Click - handles the clicking of the Zendikar choice button
        DESCRIPTION
                creates a new instance of gameboard with the player playing the zendikar deck
        RETURNS
                nothing
        */
        /**/
        private void zendiButton_Click(object sender, EventArgs e)
        {
            GameBoard newgame = new GameBoard(true);
            this.Hide();
            newgame.ShowDialog();
            this.Close();
        }

        /**/
        /*
        Form1::quitButton_Click() Form1::quitButton_Click()
        NAME
                Form1::quitButton_Click - handles the clicking of the quit choice button
        DESCRIPTION
                closes out of the program
        RETURNS
                nothing
        */
        /**/
        private void quitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}