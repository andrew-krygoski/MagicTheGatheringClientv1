namespace MagicTheGatheringClientv1
{
    partial class GameBoard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameBoard));
            this.priorityButton = new System.Windows.Forms.Button();
            this.phaseDisplay = new System.Windows.Forms.TextBox();
            this.messageBox = new System.Windows.Forms.TextBox();
            this.lifeText = new System.Windows.Forms.TextBox();
            this.compLifeText = new System.Windows.Forms.TextBox();
            this.humanLifeText = new System.Windows.Forms.TextBox();
            this.theRecord = new System.Windows.Forms.TextBox();
            this.passTurnButton = new System.Windows.Forms.Button();
            this.gamestate = new System.Windows.Forms.TextBox();
            this.Player2Battlefield = new System.Windows.Forms.PictureBox();
            this.Player1Battlefield = new System.Windows.Forms.PictureBox();
            this.Player1HandArea = new System.Windows.Forms.PictureBox();
            this.Player1Graveyard = new System.Windows.Forms.PictureBox();
            this.Player2Graveyard = new System.Windows.Forms.PictureBox();
            this.Player2ExileZone = new System.Windows.Forms.PictureBox();
            this.Player1ExileZone = new System.Windows.Forms.PictureBox();
            this.Player1ManaPool = new System.Windows.Forms.PictureBox();
            this.manaPoolId = new System.Windows.Forms.TextBox();
            this.choiceMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.targetCompButton = new System.Windows.Forms.Button();
            this.choicesLaidBox = new System.Windows.Forms.PictureBox();
            this.theStackBox = new System.Windows.Forms.PictureBox();
            this.SpellCostLeft = new System.Windows.Forms.PictureBox();
            this.mulliganButton = new System.Windows.Forms.Button();
            this.quitButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.Player2Battlefield)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Player1Battlefield)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Player1HandArea)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Player1Graveyard)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Player2Graveyard)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Player2ExileZone)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Player1ExileZone)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Player1ManaPool)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.choicesLaidBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.theStackBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SpellCostLeft)).BeginInit();
            this.SuspendLayout();
            // 
            // priorityButton
            // 
            this.priorityButton.Location = new System.Drawing.Point(13, 73);
            this.priorityButton.Name = "priorityButton";
            this.priorityButton.Size = new System.Drawing.Size(75, 23);
            this.priorityButton.TabIndex = 2;
            this.priorityButton.Text = "Pass Priority";
            this.priorityButton.UseVisualStyleBackColor = true;
            this.priorityButton.Click += new System.EventHandler(this.priorityButton_Click);
            // 
            // phaseDisplay
            // 
            this.phaseDisplay.Location = new System.Drawing.Point(12, 275);
            this.phaseDisplay.Name = "phaseDisplay";
            this.phaseDisplay.ReadOnly = true;
            this.phaseDisplay.Size = new System.Drawing.Size(124, 20);
            this.phaseDisplay.TabIndex = 3;
            this.phaseDisplay.Text = "Phase Here";
            this.phaseDisplay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // messageBox
            // 
            this.messageBox.Location = new System.Drawing.Point(13, 102);
            this.messageBox.Name = "messageBox";
            this.messageBox.ReadOnly = true;
            this.messageBox.Size = new System.Drawing.Size(124, 20);
            this.messageBox.TabIndex = 6;
            this.messageBox.Text = "Message Here";
            this.messageBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lifeText
            // 
            this.lifeText.Location = new System.Drawing.Point(12, 637);
            this.lifeText.Name = "lifeText";
            this.lifeText.ReadOnly = true;
            this.lifeText.Size = new System.Drawing.Size(28, 20);
            this.lifeText.TabIndex = 7;
            this.lifeText.Text = "Life:";
            // 
            // compLifeText
            // 
            this.compLifeText.Location = new System.Drawing.Point(13, 664);
            this.compLifeText.Name = "compLifeText";
            this.compLifeText.ReadOnly = true;
            this.compLifeText.Size = new System.Drawing.Size(100, 20);
            this.compLifeText.TabIndex = 8;
            this.compLifeText.Text = "Computer:";
            // 
            // humanLifeText
            // 
            this.humanLifeText.Location = new System.Drawing.Point(12, 690);
            this.humanLifeText.Name = "humanLifeText";
            this.humanLifeText.ReadOnly = true;
            this.humanLifeText.Size = new System.Drawing.Size(100, 20);
            this.humanLifeText.TabIndex = 9;
            this.humanLifeText.Text = "Human:";
            // 
            // theRecord
            // 
            this.theRecord.Location = new System.Drawing.Point(13, 197);
            this.theRecord.Multiline = true;
            this.theRecord.Name = "theRecord";
            this.theRecord.ReadOnly = true;
            this.theRecord.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.theRecord.Size = new System.Drawing.Size(143, 72);
            this.theRecord.TabIndex = 10;
            this.theRecord.Text = " ";
            // 
            // passTurnButton
            // 
            this.passTurnButton.Location = new System.Drawing.Point(94, 73);
            this.passTurnButton.Name = "passTurnButton";
            this.passTurnButton.Size = new System.Drawing.Size(75, 23);
            this.passTurnButton.TabIndex = 11;
            this.passTurnButton.Text = "Pass Turn";
            this.passTurnButton.UseVisualStyleBackColor = true;
            this.passTurnButton.Click += new System.EventHandler(this.passTurnButton_Click);
            // 
            // gamestate
            // 
            this.gamestate.Location = new System.Drawing.Point(13, 129);
            this.gamestate.Multiline = true;
            this.gamestate.Name = "gamestate";
            this.gamestate.ReadOnly = true;
            this.gamestate.Size = new System.Drawing.Size(143, 62);
            this.gamestate.TabIndex = 12;
            // 
            // Player2Battlefield
            // 
            this.Player2Battlefield.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("Player2Battlefield.BackgroundImage")));
            this.Player2Battlefield.Location = new System.Drawing.Point(422, 82);
            this.Player2Battlefield.Name = "Player2Battlefield";
            this.Player2Battlefield.Size = new System.Drawing.Size(100, 50);
            this.Player2Battlefield.TabIndex = 13;
            this.Player2Battlefield.TabStop = false;
            this.Player2Battlefield.Click += new System.EventHandler(this.Player2Battlefield_Click);
            this.Player2Battlefield.Paint += new System.Windows.Forms.PaintEventHandler(this.Player2Battlefield_Paint);
            // 
            // Player1Battlefield
            // 
            this.Player1Battlefield.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("Player1Battlefield.BackgroundImage")));
            this.Player1Battlefield.Location = new System.Drawing.Point(422, 152);
            this.Player1Battlefield.Name = "Player1Battlefield";
            this.Player1Battlefield.Size = new System.Drawing.Size(100, 50);
            this.Player1Battlefield.TabIndex = 14;
            this.Player1Battlefield.TabStop = false;
            this.Player1Battlefield.Click += new System.EventHandler(this.Player1Battlefield_Click);
            this.Player1Battlefield.Paint += new System.Windows.Forms.PaintEventHandler(this.Player1Battlefield_Paint);
            // 
            // Player1HandArea
            // 
            this.Player1HandArea.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("Player1HandArea.BackgroundImage")));
            this.Player1HandArea.Location = new System.Drawing.Point(422, 218);
            this.Player1HandArea.Name = "Player1HandArea";
            this.Player1HandArea.Size = new System.Drawing.Size(100, 50);
            this.Player1HandArea.TabIndex = 15;
            this.Player1HandArea.TabStop = false;
            this.Player1HandArea.Click += new System.EventHandler(this.Player1HandArea_Click);
            this.Player1HandArea.Paint += new System.Windows.Forms.PaintEventHandler(this.Player1HandArea_Paint);
            // 
            // Player1Graveyard
            // 
            this.Player1Graveyard.Image = ((System.Drawing.Image)(resources.GetObject("Player1Graveyard.Image")));
            this.Player1Graveyard.Location = new System.Drawing.Point(945, 439);
            this.Player1Graveyard.Name = "Player1Graveyard";
            this.Player1Graveyard.Size = new System.Drawing.Size(100, 50);
            this.Player1Graveyard.TabIndex = 16;
            this.Player1Graveyard.TabStop = false;
            this.Player1Graveyard.Click += new System.EventHandler(this.Player1Graveyard_Click);
            this.Player1Graveyard.Paint += new System.Windows.Forms.PaintEventHandler(this.Player1Graveyard_Paint);
            // 
            // Player2Graveyard
            // 
            this.Player2Graveyard.Image = ((System.Drawing.Image)(resources.GetObject("Player2Graveyard.Image")));
            this.Player2Graveyard.Location = new System.Drawing.Point(945, 82);
            this.Player2Graveyard.Name = "Player2Graveyard";
            this.Player2Graveyard.Size = new System.Drawing.Size(100, 50);
            this.Player2Graveyard.TabIndex = 17;
            this.Player2Graveyard.TabStop = false;
            this.Player2Graveyard.Click += new System.EventHandler(this.Player2Graveyard_Click);
            this.Player2Graveyard.Paint += new System.Windows.Forms.PaintEventHandler(this.Player2Graveyard_Paint);
            // 
            // Player2ExileZone
            // 
            this.Player2ExileZone.Enabled = false;
            this.Player2ExileZone.Image = ((System.Drawing.Image)(resources.GetObject("Player2ExileZone.Image")));
            this.Player2ExileZone.Location = new System.Drawing.Point(1099, 82);
            this.Player2ExileZone.Name = "Player2ExileZone";
            this.Player2ExileZone.Size = new System.Drawing.Size(100, 50);
            this.Player2ExileZone.TabIndex = 18;
            this.Player2ExileZone.TabStop = false;
            this.Player2ExileZone.Visible = false;
            this.Player2ExileZone.Click += new System.EventHandler(this.Player2ExileZone_Click);
            this.Player2ExileZone.Paint += new System.Windows.Forms.PaintEventHandler(this.Player2ExileZone_Paint);
            // 
            // Player1ExileZone
            // 
            this.Player1ExileZone.Enabled = false;
            this.Player1ExileZone.Image = ((System.Drawing.Image)(resources.GetObject("Player1ExileZone.Image")));
            this.Player1ExileZone.Location = new System.Drawing.Point(1099, 439);
            this.Player1ExileZone.Name = "Player1ExileZone";
            this.Player1ExileZone.Size = new System.Drawing.Size(100, 50);
            this.Player1ExileZone.TabIndex = 19;
            this.Player1ExileZone.TabStop = false;
            this.Player1ExileZone.Visible = false;
            this.Player1ExileZone.Click += new System.EventHandler(this.Player1ExileZone_Click);
            this.Player1ExileZone.Paint += new System.Windows.Forms.PaintEventHandler(this.Player1ExileZone_Paint);
            // 
            // Player1ManaPool
            // 
            this.Player1ManaPool.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("Player1ManaPool.BackgroundImage")));
            this.Player1ManaPool.Location = new System.Drawing.Point(12, 401);
            this.Player1ManaPool.Name = "Player1ManaPool";
            this.Player1ManaPool.Size = new System.Drawing.Size(100, 50);
            this.Player1ManaPool.TabIndex = 20;
            this.Player1ManaPool.TabStop = false;
            this.Player1ManaPool.Click += new System.EventHandler(this.Player1ManaPool_Click);
            this.Player1ManaPool.Paint += new System.Windows.Forms.PaintEventHandler(this.Player1ManaPool_Paint);
            // 
            // manaPoolId
            // 
            this.manaPoolId.Location = new System.Drawing.Point(12, 375);
            this.manaPoolId.Name = "manaPoolId";
            this.manaPoolId.ReadOnly = true;
            this.manaPoolId.Size = new System.Drawing.Size(100, 20);
            this.manaPoolId.TabIndex = 21;
            this.manaPoolId.Text = "Mana Pool:";
            // 
            // choiceMenu
            // 
            this.choiceMenu.Name = "contextMenuStrip1";
            this.choiceMenu.Size = new System.Drawing.Size(61, 4);
            this.choiceMenu.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.choiceMenu_ItemClicked);
            // 
            // targetCompButton
            // 
            this.targetCompButton.Enabled = false;
            this.targetCompButton.Location = new System.Drawing.Point(94, 43);
            this.targetCompButton.Name = "targetCompButton";
            this.targetCompButton.Size = new System.Drawing.Size(75, 23);
            this.targetCompButton.TabIndex = 22;
            this.targetCompButton.Text = "Target Comp";
            this.targetCompButton.UseVisualStyleBackColor = true;
            this.targetCompButton.Visible = false;
            this.targetCompButton.Click += new System.EventHandler(this.targetCompButton_Click);
            // 
            // choicesLaidBox
            // 
            this.choicesLaidBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("choicesLaidBox.BackgroundImage")));
            this.choicesLaidBox.Enabled = false;
            this.choicesLaidBox.Location = new System.Drawing.Point(576, 152);
            this.choicesLaidBox.Name = "choicesLaidBox";
            this.choicesLaidBox.Size = new System.Drawing.Size(100, 50);
            this.choicesLaidBox.TabIndex = 23;
            this.choicesLaidBox.TabStop = false;
            this.choicesLaidBox.Visible = false;
            this.choicesLaidBox.Click += new System.EventHandler(this.choicesLaidBox_Click);
            this.choicesLaidBox.Paint += new System.Windows.Forms.PaintEventHandler(this.choicesLaidBox_Paint);
            // 
            // theStackBox
            // 
            this.theStackBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("theStackBox.BackgroundImage")));
            this.theStackBox.Location = new System.Drawing.Point(1005, 151);
            this.theStackBox.Name = "theStackBox";
            this.theStackBox.Size = new System.Drawing.Size(100, 50);
            this.theStackBox.TabIndex = 25;
            this.theStackBox.TabStop = false;
            this.theStackBox.Paint += new System.Windows.Forms.PaintEventHandler(this.theStackBox_Paint);
            // 
            // SpellCostLeft
            // 
            this.SpellCostLeft.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SpellCostLeft.BackgroundImage")));
            this.SpellCostLeft.Location = new System.Drawing.Point(12, 457);
            this.SpellCostLeft.Name = "SpellCostLeft";
            this.SpellCostLeft.Size = new System.Drawing.Size(100, 50);
            this.SpellCostLeft.TabIndex = 26;
            this.SpellCostLeft.TabStop = false;
            this.SpellCostLeft.Paint += new System.Windows.Forms.PaintEventHandler(this.SpellCostLeft_Paint);
            // 
            // mulliganButton
            // 
            this.mulliganButton.Location = new System.Drawing.Point(12, 44);
            this.mulliganButton.Name = "mulliganButton";
            this.mulliganButton.Size = new System.Drawing.Size(75, 23);
            this.mulliganButton.TabIndex = 27;
            this.mulliganButton.Text = "Mulligan";
            this.mulliganButton.UseVisualStyleBackColor = true;
            this.mulliganButton.Click += new System.EventHandler(this.mulliganButton_Click);
            // 
            // quitButton
            // 
            this.quitButton.Location = new System.Drawing.Point(12, 13);
            this.quitButton.Name = "quitButton";
            this.quitButton.Size = new System.Drawing.Size(75, 23);
            this.quitButton.TabIndex = 28;
            this.quitButton.Text = "Quit";
            this.quitButton.UseVisualStyleBackColor = true;
            this.quitButton.Click += new System.EventHandler(this.quitButton_Click);
            // 
            // GameBoard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1247, 721);
            this.Controls.Add(this.quitButton);
            this.Controls.Add(this.mulliganButton);
            this.Controls.Add(this.SpellCostLeft);
            this.Controls.Add(this.theStackBox);
            this.Controls.Add(this.choicesLaidBox);
            this.Controls.Add(this.targetCompButton);
            this.Controls.Add(this.manaPoolId);
            this.Controls.Add(this.Player1ManaPool);
            this.Controls.Add(this.Player1ExileZone);
            this.Controls.Add(this.Player2ExileZone);
            this.Controls.Add(this.Player2Graveyard);
            this.Controls.Add(this.Player1Graveyard);
            this.Controls.Add(this.Player1HandArea);
            this.Controls.Add(this.Player1Battlefield);
            this.Controls.Add(this.Player2Battlefield);
            this.Controls.Add(this.gamestate);
            this.Controls.Add(this.passTurnButton);
            this.Controls.Add(this.theRecord);
            this.Controls.Add(this.humanLifeText);
            this.Controls.Add(this.compLifeText);
            this.Controls.Add(this.lifeText);
            this.Controls.Add(this.messageBox);
            this.Controls.Add(this.phaseDisplay);
            this.Controls.Add(this.priorityButton);
            this.DoubleBuffered = true;
            this.Name = "GameBoard";
            this.Text = "Magic the Gathering: Zendikar Vs. Eldrazi";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.GameBoard_Paint);
            this.Resize += new System.EventHandler(this.GameBoard_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.Player2Battlefield)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Player1Battlefield)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Player1HandArea)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Player1Graveyard)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Player2Graveyard)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Player2ExileZone)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Player1ExileZone)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Player1ManaPool)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.choicesLaidBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.theStackBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SpellCostLeft)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button priorityButton;
        private System.Windows.Forms.TextBox phaseDisplay;
        private System.Windows.Forms.TextBox messageBox;
        private System.Windows.Forms.TextBox lifeText;
        private System.Windows.Forms.TextBox compLifeText;
        private System.Windows.Forms.TextBox humanLifeText;
        private System.Windows.Forms.TextBox theRecord;
        private System.Windows.Forms.Button passTurnButton;
        private System.Windows.Forms.TextBox gamestate;
        private System.Windows.Forms.PictureBox Player2Battlefield;
        private System.Windows.Forms.PictureBox Player1Battlefield;
        private System.Windows.Forms.PictureBox Player1HandArea;
        private System.Windows.Forms.PictureBox Player1Graveyard;
        private System.Windows.Forms.PictureBox Player2Graveyard;
        private System.Windows.Forms.PictureBox Player2ExileZone;
        private System.Windows.Forms.PictureBox Player1ExileZone;
        private System.Windows.Forms.PictureBox Player1ManaPool;
        private System.Windows.Forms.TextBox manaPoolId;
        private System.Windows.Forms.ContextMenuStrip choiceMenu;
        private System.Windows.Forms.Button targetCompButton;
        private System.Windows.Forms.PictureBox choicesLaidBox;
        private System.Windows.Forms.PictureBox theStackBox;
        private System.Windows.Forms.PictureBox SpellCostLeft;
        private System.Windows.Forms.Button mulliganButton;
        private System.Windows.Forms.Button quitButton;
    }
}