using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace RTS_GAME_2019
{
    class GameEngine
    {
        Map map;
        int remainingRounds;
        int roundCount;
        System.Timers.Timer myTimer;
        RichTextBox textBox;
        Label winLabel;
        public GameEngine(Map map, int roundLimit, System.Timers.Timer timer, RichTextBox textBox, Label winLabel)
        {
            this.winLabel = winLabel;
            this.textBox = textBox;
            roundCount = 0;
            myTimer = timer;
            this.map = map;
            remainingRounds = roundLimit;

        }

        public void StartTimer()
        {
            myTimer.Enabled = true;
        }

        public void StopTimer()
        {
            myTimer.Enabled = false;
        }

        public bool isTimerRunning()
        {
            return myTimer.Enabled;
        }

        public int PerformRound()
        {
            if(remainingRounds <= 0)
            {
                //done!!!
                return roundCount;
            }

            //move, attack or run away
            //foreach(Unit u in unitList)
            List<Unit> unitList = map.GetUnitList();

            for (int i =0; i < unitList.Count; i++)
            {
                Unit u = unitList.ElementAt(i);
                //Case 1: below health so run away
                if((u.Health / (double) u.MaxHealth) * 100.0 <= 25)
                {
                    map.MoveUnitRandomly(u);
                    continue;
                }

                //Case 2: Finding enemy and decide on attacking

                Debug.WriteLine("Unit searching -- " + u.ToString());
                Unit closestEnemy = u.FindClosestUnit(map);

                if (closestEnemy != null)
                {
                    Debug.WriteLine("closest enemy -- " + closestEnemy.ToString());
                    if(map.IsWithinRange(u, closestEnemy))
                    {
                        // we can attack
                        bool didDie = u.AttackUnit(closestEnemy, map);
                        if (didDie)
                        {
                            unitList.RemoveAt(i);
                            i--;
                        }
                    }
                    else
                    {
                        //we cant attack so move towards
                        map.MoveTowardsEnemy(u, closestEnemy);
                    }
                    textBox.Invoke(new Action(() => textBox.AppendText(u.ToString() + "\n")));
                    textBox.Invoke(new Action(() => textBox.ScrollToCaret()));

                }
                else
                {
                    Debug.WriteLine("Team " + u.Team + " WINS!!! -- no enemies left");
                    winLabel.Invoke(new Action(() => winLabel.Text = "Team " + u.Team + " has won the game!"));
                    StopTimer();
                    return roundCount;
                }
            }
                //now round is done
                roundCount++;
                remainingRounds--;
                return roundCount;
        }
    }
}
