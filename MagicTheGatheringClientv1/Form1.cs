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

namespace MagicTheGatheringClientv1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            extractData();
        }

        void extractData()
        {
            Player player1 = new Player(true, 1);
            Console.WriteLine("ITS A CHRISTMAS MIRICLE");            
        }
    }
}
