using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTS_GAME_2019
{
    public partial class Form1 : Form
    {
        GameEngine gameEngine;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DataGridView grid = (DataGridView)this.Controls["grid"];
            lblRound.Text = "Round 1";
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 5000;
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;

            Map map = new Map(10);
            map.GenerateMap(grid);
            gameEngine = new GameEngine(map, 20, timer, textBox, lblWinner);
            gameEngine.StartTimer();
            //gameEngine.PerformRound();

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            int roundToUpdate = gameEngine.PerformRound();
            lblRound.Invoke(new Action(() => lblRound.Text  = "Round " + roundToUpdate));


        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!gameEngine.isTimerRunning())
            {
            gameEngine.StartTimer();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            gameEngine.StopTimer();
        }
    }
}
