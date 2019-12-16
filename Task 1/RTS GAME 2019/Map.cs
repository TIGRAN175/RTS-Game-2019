using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTS_GAME_2019
{

    class Map
    {
        DataGridView myGrid;
        public struct Position
        {
            public int rowIndex, colIndex;
            public Position(int p1, int p2)
            {
                rowIndex = p1;
                colIndex = p2;
            }
        }

        public readonly int MAP_ROWS = 20;
       public readonly int MAP_COLS = 20;

        public Unit[,] unitMap;
        public Map(int numUnitsToCreate)
        {
            unitMap = new Unit[MAP_ROWS, MAP_COLS];

            if (numUnitsToCreate > (MAP_ROWS * MAP_COLS))
            {
                numUnitsToCreate = MAP_ROWS * MAP_COLS;
            }

            Random rand = new Random();
            
            int numUnitsTeamOne = numUnitsToCreate / 2;
            int numUnitsTeamTwo = numUnitsToCreate - numUnitsTeamOne;
            while(numUnitsToCreate > 0)
            {
                int rowIndex, colIndex;
                do
                {
                     rowIndex = rand.Next(0, MAP_ROWS);
                     colIndex = rand.Next(0, MAP_COLS);
                } while (unitMap[rowIndex, colIndex] != null);
                //at this point we have an empty spot

                int teamNumber = 0; //default team zero
                if(numUnitsToCreate > numUnitsTeamOne)
                {
                    teamNumber = 1;
                }

                int unitType = rand.Next(0, 2);
                if(unitType == 0)
                {
                    //create meelee
                    unitMap[rowIndex, colIndex] = new MeeleeUnit(rowIndex, colIndex, teamNumber);
                }else if (unitType == 1)
                {
                    //create ranged
                    unitMap[rowIndex, colIndex] = new RangedUnit(rowIndex, colIndex, teamNumber);
                }
                numUnitsToCreate--;
            }

        }

        public void GenerateMap(DataGridView grid)
        {
            myGrid = grid;
            grid.ColumnCount = MAP_COLS;
            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.Width = 20;
            }

            for (int i = 0; i < MAP_ROWS; i++)
            {
                string[] symbolArray = new string[MAP_COLS];
                int[] teamArray = new int[MAP_COLS];
                for (int j = 0; j < MAP_COLS; j++)
                {
                    teamArray[j] = -1;
                    if (unitMap[i, j] != null)
                    {
                        symbolArray[j] = "" + unitMap[i, j].Symbol;
                        teamArray[j] = unitMap[i, j].Team;
                        Debug.WriteLine("team selected: " + teamArray[j]);
                    }
                    else
                    {
                        symbolArray[j] = "";
                    }
                }
                grid.Rows.Add(symbolArray);
                for(int k=0; k < MAP_COLS; k++)
                {
                    //only two teams for now... add wizards
                    if(teamArray[k] == 0)
                    {
                        grid.Rows[i].Cells[k].Style.BackColor = Color.Aqua;

                    }else if (teamArray[k] == 1)
                    {
                        grid.Rows[i].Cells[k].Style.BackColor = Color.Red;
                    }
                }

            }
        }

        public bool IsWithinRange(Unit attacker, Unit enemy)
        {
            int attackerX = attacker.XPos;
            int attackerY = attacker.YPos;
            int enemyX = enemy.XPos;
            int enemyY = enemy.YPos;

            double pythagDistance = CalculatePythagDistance(attackerX, attackerY, enemyX, enemyY);
            int onlyIntegerPart = (int) pythagDistance;
            if(attacker.AttackRange >= onlyIntegerPart)
            {
                // we can reach the unit
                return true;
            }
            else
            {
                //cant reach the unit
                return false;
            }
        }

        public List<Position> GetValidMoves(Unit unitToMove)
        {
            int rowIndex = unitToMove.XPos;
            int colIndex = unitToMove.YPos;
            List<Position> tempMoves = new List<Position>();
            tempMoves.Add(new Position(rowIndex - 1, colIndex - 1)); //topLeft
            tempMoves.Add(new Position(rowIndex - 1, colIndex));//toptop
            tempMoves.Add(new Position(rowIndex - 1, colIndex + 1)); //topRight
            tempMoves.Add(new Position(rowIndex, colIndex + 1)); // rightRight
            tempMoves.Add(new Position(rowIndex + 1, colIndex + 1));//bottomRight
            tempMoves.Add(new Position(rowIndex + 1, colIndex)); // bottom bottom
            tempMoves.Add(new Position(rowIndex + 1, colIndex - 1)); // bottom left
            tempMoves.Add(new Position(rowIndex, colIndex -1)); // left left
            List<Position> actualMoves = new List<Position>();
            foreach(Position pos in tempMoves)
            {
                if (CanMoveTo(pos))
                {
                    actualMoves.Add(pos);
                }
            }
            return actualMoves;
        }

        public bool CanMoveTo(Position pos)
        {
            if(IsInMap(pos.rowIndex, pos.colIndex))
            {
                //check if any units conflict or buildings
                if(unitMap[pos.rowIndex, pos.colIndex] == null)
                {
                    //ADD BUILDING CHECK HERE
                    return true;
                }
                
            }
            return false;
        }

        public void MoveUnitRandomly(Unit unitToMove)
        {
            List<Position> possibleMoves = GetValidMoves(unitToMove);
            Random rand = new Random();
            if(possibleMoves.Count == 0)
            {
                Debug.WriteLine("Cant move unit, no possible moves: " + unitToMove.ToString());
                return;
            }

            int randomPosIndex = rand.Next(0, possibleMoves.Count);
            Position randomPositionChosen = possibleMoves.ElementAt(randomPosIndex);
            MoveUnit(unitToMove, randomPositionChosen.rowIndex, randomPositionChosen.colIndex);
        }

        public double CalculatePythagDistance(int x1, int y1, int x2, int y2)
        {
            int xVal = Math.Abs(x1 - x2);
            int yVal = Math.Abs(y1 - y2);
            return Math.Sqrt((xVal * xVal) + (yVal * yVal));
        }

        public void DestroyUnit(Unit unitToDestroy)
        {
            unitMap[unitToDestroy.XPos, unitToDestroy.YPos] = null;
            myGrid[unitToDestroy.YPos, unitToDestroy.XPos].Style.BackColor = Color.White;
            myGrid[unitToDestroy.YPos, unitToDestroy.XPos].Value = "";
        }

        public void MoveUnit(Unit unit, int rowIndexToMove, int colIndexToMove)
        {
            if(!IsInMap(rowIndexToMove, colIndexToMove))
            {
                return;
            }
     
                int currX = unit.XPos;
                int currY = unit.YPos;
                unit.MoveToPosition(rowIndexToMove, colIndexToMove);
                unitMap[currX, currY] = null;
                unitMap[rowIndexToMove, colIndexToMove] = unit;
                myGrid[colIndexToMove, rowIndexToMove].Value = "" + unit.Symbol;
            myGrid[currY, currX].Style.BackColor = Color.White;

            Color colorToSet = Color.White;
            if(unit.Team == 0)
            {
                colorToSet = Color.Aqua;
            }else if (unit.Team == 1)
            {
                colorToSet = Color.Red;
            }
            myGrid[colIndexToMove, rowIndexToMove].Style.BackColor = colorToSet;
                myGrid[currY, currX].Value = "";
                

        }

        public List<Unit> GetUnitList()
        {
            List<Unit> unitList = new List<Unit>();
            for (int i =0; i < MAP_ROWS; i++)
            {
                for(int j =0; j < MAP_COLS; j++)
                {
                    if(unitMap[i,j] != null)
                    {
                        unitList.Add(unitMap[i, j]);
                    }
                }
            }
            return unitList;
        }

        public void MoveTowardsEnemy(Unit unitToMove, Unit enemy)
        {
            List<Position> validPositions = GetValidMoves(unitToMove);
            if (validPositions.Count == 0)
            {
                Debug.WriteLine("Cant Move unit is stuck: " + unitToMove.ToString());
                return;
            }

            //we know the unit can move if we reach here

            List<double> pythagDistances = new List<double>();
            foreach(Position pos in validPositions){
                double currPythagDistance = CalculatePythagDistance(pos.rowIndex, pos.colIndex, enemy.XPos, enemy.YPos);
                pythagDistances.Add(currPythagDistance);
            }

            //at this point we have the corresponding pythags for each coordinate
            int smallestIndex = 0;
            double smallestPythag = pythagDistances.ElementAt(0);

            for(int i =1; i < pythagDistances.Count; i++)
            {
                if( pythagDistances.ElementAt(i) < smallestPythag )
                {
                    smallestPythag = pythagDistances.ElementAt(i);
                    smallestIndex = i;
                }
            }

            //now we have the correct coordinate at index smallestIndex

            Position finalPositionToMove = validPositions.ElementAt(smallestIndex);
            MoveUnit(unitToMove, finalPositionToMove.rowIndex, finalPositionToMove.colIndex);
        }


        public bool IsInMap(int rowIndex, int colIndex)
        {
            if(rowIndex >= MAP_ROWS || rowIndex < 0)
            {
                return false;
            }

            if(colIndex >= MAP_COLS || colIndex < 0)
            {
                return false;
            }

            //Debug.WriteLine("(" + rowIndex + "," + colIndex + ") is in map");

            return true;
        }


        public String PrintMap()
        {
            String o = "";
            for(int i=0; i < MAP_ROWS; i++)
            {
                for(int j =0; j < MAP_COLS; j++)
                {
                    if(unitMap[i, j] != null)
                    {
                   // Console.Write(unitMap[i,j].ToString() + "|");
                        o += unitMap[i, j].Symbol + "|";
                    }else
                    {
                        o += "  |";
                    }

                }
                //Console.WriteLine();
                o += "\n";
            }
            return o;
        }
    }
}
