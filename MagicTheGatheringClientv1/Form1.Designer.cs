namespace MagicTheGatheringClientv1
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.backgroundImg = new System.Windows.Forms.PictureBox();
            this.logoImg = new System.Windows.Forms.PictureBox();
            this.startNGButton = new System.Windows.Forms.Button();
            this.quitButton = new System.Windows.Forms.Button();
            this.zendiButton = new System.Windows.Forms.Button();
            this.eldraziButton = new System.Windows.Forms.Button();
            this.resultsTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.backgroundImg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.logoImg)).BeginInit();
            this.SuspendLayout();
            // 
            // backgroundImg
            // 
            this.backgroundImg.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("backgroundImg.BackgroundImage")));
            this.backgroundImg.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.backgroundImg.Location = new System.Drawing.Point(12, 3);
            this.backgroundImg.Name = "backgroundImg";
            this.backgroundImg.Size = new System.Drawing.Size(799, 385);
            this.backgroundImg.TabIndex = 0;
            this.backgroundImg.TabStop = false;
            // 
            // logoImg
            // 
            this.logoImg.Image = ((System.Drawing.Image)(resources.GetObject("logoImg.Image")));
            this.logoImg.Location = new System.Drawing.Point(134, 46);
            this.logoImg.Name = "logoImg";
            this.logoImg.Size = new System.Drawing.Size(567, 143);
            this.logoImg.TabIndex = 1;
            this.logoImg.TabStop = false;
            // 
            // startNGButton
            // 
            this.startNGButton.Location = new System.Drawing.Point(342, 215);
            this.startNGButton.Name = "startNGButton";
            this.startNGButton.Size = new System.Drawing.Size(146, 46);
            this.startNGButton.TabIndex = 2;
            this.startNGButton.Text = "Start New Game";
            this.startNGButton.UseVisualStyleBackColor = true;
            this.startNGButton.Click += new System.EventHandler(this.startNGButton_Click);
            // 
            // quitButton
            // 
            this.quitButton.Location = new System.Drawing.Point(342, 280);
            this.quitButton.Name = "quitButton";
            this.quitButton.Size = new System.Drawing.Size(146, 46);
            this.quitButton.TabIndex = 4;
            this.quitButton.Text = "Quit";
            this.quitButton.UseVisualStyleBackColor = true;
            this.quitButton.Click += new System.EventHandler(this.quitButton_Click);
            // 
            // zendiButton
            // 
            this.zendiButton.Enabled = false;
            this.zendiButton.Location = new System.Drawing.Point(134, 215);
            this.zendiButton.Name = "zendiButton";
            this.zendiButton.Size = new System.Drawing.Size(146, 46);
            this.zendiButton.TabIndex = 5;
            this.zendiButton.Text = "Zendikar";
            this.zendiButton.UseVisualStyleBackColor = true;
            this.zendiButton.Visible = false;
            this.zendiButton.Click += new System.EventHandler(this.zendiButton_Click);
            // 
            // eldraziButton
            // 
            this.eldraziButton.Enabled = false;
            this.eldraziButton.Location = new System.Drawing.Point(555, 215);
            this.eldraziButton.Name = "eldraziButton";
            this.eldraziButton.Size = new System.Drawing.Size(146, 46);
            this.eldraziButton.TabIndex = 6;
            this.eldraziButton.Text = "Eldrazi";
            this.eldraziButton.UseVisualStyleBackColor = true;
            this.eldraziButton.Visible = false;
            this.eldraziButton.Click += new System.EventHandler(this.eldraziButton_Click);
            // 
            // resultsTextBox
            // 
            this.resultsTextBox.Enabled = false;
            this.resultsTextBox.Location = new System.Drawing.Point(364, 341);
            this.resultsTextBox.Name = "resultsTextBox";
            this.resultsTextBox.ReadOnly = true;
            this.resultsTextBox.Size = new System.Drawing.Size(100, 20);
            this.resultsTextBox.TabIndex = 7;
            this.resultsTextBox.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1247, 721);
            this.Controls.Add(this.resultsTextBox);
            this.Controls.Add(this.eldraziButton);
            this.Controls.Add(this.zendiButton);
            this.Controls.Add(this.quitButton);
            this.Controls.Add(this.startNGButton);
            this.Controls.Add(this.logoImg);
            this.Controls.Add(this.backgroundImg);
            this.DoubleBuffered = true;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Resize += new System.EventHandler(this.Form1_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.backgroundImg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.logoImg)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.PictureBox backgroundImg;
        private System.Windows.Forms.PictureBox logoImg;
        private System.Windows.Forms.Button startNGButton;
        private System.Windows.Forms.Button quitButton;
        private System.Windows.Forms.Button zendiButton;
        private System.Windows.Forms.Button eldraziButton;
        private System.Windows.Forms.TextBox resultsTextBox;
    }
}

