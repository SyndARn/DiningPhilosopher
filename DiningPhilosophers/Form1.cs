using SophiaFuture;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiningPhilosophers
{
    public partial class Form1 : Form
    {
        StringToEventCatcherActor catcher = new StringToEventCatcherActor();

        protected void EvHandler(object sender, string i)
        {
            lbResult.Items.Add(i);
            lbResult.SelectedIndex = lbResult.Items.Count - 1;
            catcher.SetEvent(lbResult, new EventHandler<string>(this.EvHandler));
        }

        private Table fTable;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int philosophers;
            int dinners;
            catcher.SetEvent(lbResult, new EventHandler<string>(this.EvHandler));
            if (
                (int.TryParse(txAttendee.Text, out philosophers))
                &&
                (int.TryParse(tbDinner.Text, out dinners))
               )
            {
                fTable = new Table(philosophers, dinners, catcher);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            fTable.Status();
        }
    }
}
