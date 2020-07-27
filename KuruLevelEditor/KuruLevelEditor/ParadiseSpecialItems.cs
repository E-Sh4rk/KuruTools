using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;

namespace KuruLevelEditor
{
    class ParadiseSpecialItems
    {
        private Desktop _desktop;
        private Game1 _game;
        private ParadisePhysicalMapLogic _logic;

        private TextBox movingObjects;
        private ScrollViewer scroll;

        void saveChanges()
        {
            _logic.FromPrettyText(movingObjects.Text == null ? "" : movingObjects.Text);
        }

        string[] MovingObjectsLines()
        {
            if (movingObjects.Text == null)
                return new string[0];
            return movingObjects.Text.Replace("\r", "").Split("\n", StringSplitOptions.RemoveEmptyEntries);
        }
        int lineOfID(string[] lines, int id)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                try
                {
                    string[] elts = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (Convert.ToInt32(elts[0]) == id)
                        return i;
                }
                catch { }
            }
            return -1;
        }
        void addToMovingObjectsBox(string ID, string type, string p1, string p2 = null, string p3 = null, string p4 = null, string p5 = null)
        {
            string[] lines = MovingObjectsLines();
            bool addedAtTheEnd = false;
            try
            {
                int id = -1;
                if (!string.IsNullOrWhiteSpace(ID)) id = Convert.ToInt32(ID);

                if (id < 0)
                {
                    id = 0;
                    while (lineOfID(lines, id) >= 0) id++;
                }

                if (string.IsNullOrWhiteSpace(p1)) p1 = "0";
                if (string.IsNullOrWhiteSpace(p2)) p2 = "0";
                if (string.IsNullOrWhiteSpace(p3)) p3 = "0";
                if (string.IsNullOrWhiteSpace(p4)) p4 = "0";
                if (string.IsNullOrWhiteSpace(p5)) p5 = "0";

                string line = id.ToString().PadLeft(3, ' ') + " " + type.PadLeft(10, ' ') + " " + p1.PadLeft(5, ' ') + " "
                    + p2.PadLeft(5, ' ') + " " + p3.PadLeft(5, ' ') + " " + p4.PadLeft(5, ' ') + " " + p5.PadLeft(5, ' ');
                int i = lineOfID(lines, id);
                if (i < 0)
                {
                    lines[lines.Length - 1] += Environment.NewLine + line;
                    addedAtTheEnd = true;
                }
                else
                    lines[i] = line;
            }
            catch { }
            Point scrollPos = scroll.ScrollPosition;
            StringBuilder res = new StringBuilder();
            foreach (string line in lines)
                res.Append(line + Environment.NewLine);
            movingObjects.Text = res.ToString();
            if (addedAtTheEnd)
                scroll.ScrollPosition = scroll.ScrollMaximum;
            else
                scroll.ScrollPosition = scrollPos;
        }

        public ParadiseSpecialItems(Game1 game, ParadisePhysicalMapLogic logic)
        {
            _game = game;
            _logic = logic;

            Panel panel = new Panel()
            {
                Background = new SolidBrush(Color.Black),
                Padding = new Myra.Graphics2D.Thickness(20,20,20,20)
            };

            var grid = new Grid
            {
                RowSpacing = 10,
                ColumnSpacing = 10
            };

            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.HorizontalAlignment = HorizontalAlignment.Left;
            grid.RowsProportions.Add(new Proportion(ProportionType.Part));
            grid.VerticalAlignment = VerticalAlignment.Top;

            // --- MOVING OBJECTS ---
            scroll = new ScrollViewer()
            {
                GridRow = 0,
                GridColumn = 0,
                GridColumnSpan = 6,
                GridRowSpan = 21,
                ShowHorizontalScrollBar = false,
                ShowVerticalScrollBar = true
            };
            movingObjects = new TextBox()
            {
                Multiline = true,
                Font = Load.Monospace
            };
            scroll.Content = movingObjects;
            grid.Widgets.Add(scroll);

            TextBox id = new TextBox()
            {
                GridRow = 0,
                GridColumn = 6,
                GridColumnSpan = 2,
                HintText = "ID",
            };
            grid.Widgets.Add(id);
            // Array
            TextBox array1 = new TextBox()
            {
                GridRow = 2,
                GridColumn = 6,
                HintText = "Length",
                Width = 100
            };
            grid.Widgets.Add(array1);
            var arrayAdd = new TextButton
            {
                GridColumn = 11,
                GridRow = 2,
                Text = "Add array",
                Width = 100,
            };
            arrayAdd.Click += (s, a) =>
            {
                addToMovingObjectsBox(id.Text, ParadisePhysicalMapLogic.ObjectsStr[0], array1.Text);
            };
            grid.Widgets.Add(arrayAdd);
            // Offset
            TextBox offset1 = new TextBox()
            {
                GridRow = 3,
                GridColumn = 6,
                HintText = "Offset X",
                Width = 100
            };
            grid.Widgets.Add(offset1);
            TextBox offset2 = new TextBox()
            {
                GridRow = 3,
                GridColumn = 7,
                HintText = "Offset Y",
                Width = 100
            };
            grid.Widgets.Add(offset2);
            var offsetAdd = new TextButton
            {
                GridColumn = 11,
                GridRow = 3,
                Text = "Add offset",
                Width = 100,
            };
            offsetAdd.Click += (s, a) =>
            {
                addToMovingObjectsBox(id.Text, ParadisePhysicalMapLogic.ObjectsStr[1], offset1.Text, offset2.Text);
            };
            grid.Widgets.Add(offsetAdd);
            // Roller
            TextBox roller1 = new TextBox()
            {
                GridRow = 5,
                GridColumn = 6,
                HintText = "Direction",
                Width = 100
            };
            grid.Widgets.Add(roller1);
            TextBox roller2 = new TextBox()
            {
                GridRow = 5,
                GridColumn = 7,
                HintText = "Speed",
                Width = 100
            };
            grid.Widgets.Add(roller2);
            TextBox roller3 = new TextBox()
            {
                GridRow = 5,
                GridColumn = 8,
                HintText = "StartTime",
                Width = 100
            };
            grid.Widgets.Add(roller3);
            TextBox roller4 = new TextBox()
            {
                GridRow = 5,
                GridColumn = 9,
                HintText = "Period",
                Width = 100
            };
            grid.Widgets.Add(roller4);
            var rollerAdd = new TextButton
            {
                GridColumn = 11,
                GridRow = 5,
                Text = "Add roller",
                Width = 100,
            };
            rollerAdd.Click += (s, a) =>
            {
                addToMovingObjectsBox(id.Text, ParadisePhysicalMapLogic.ObjectsStr[3], roller1.Text, roller2.Text, roller3.Text, roller4.Text);
            };
            grid.Widgets.Add(rollerAdd);
            // Piston
            TextBox piston1 = new TextBox()
            {
                GridRow = 6,
                GridColumn = 6,
                HintText = "Direction",
                Width = 100
            };
            grid.Widgets.Add(piston1);
            TextBox piston2 = new TextBox()
            {
                GridRow = 6,
                GridColumn = 7,
                HintText = "MovePeriod",
                Width = 100
            };
            grid.Widgets.Add(piston2);
            TextBox piston3 = new TextBox()
            {
                GridRow = 6,
                GridColumn = 8,
                HintText = "StrokeLength",
                Width = 100
            };
            grid.Widgets.Add(piston3);
            TextBox piston4 = new TextBox()
            {
                GridRow = 6,
                GridColumn = 9,
                HintText = "StartTime",
                Width = 100
            };
            grid.Widgets.Add(piston4);
            TextBox piston5 = new TextBox()
            {
                GridRow = 6,
                GridColumn = 10,
                HintText = "WaitPeriod",
                Width = 100
            };
            grid.Widgets.Add(piston5);
            var pistonAdd = new TextButton
            {
                GridColumn = 11,
                GridRow = 6,
                Text = "Add piston",
                Width = 100,
            };
            pistonAdd.Click += (s, a) =>
            {
                addToMovingObjectsBox(id.Text, ParadisePhysicalMapLogic.ObjectsStr[4], piston1.Text, piston2.Text, piston3.Text, piston4.Text, piston5.Text);
            };
            grid.Widgets.Add(pistonAdd);
            // MovingWall
            TextBox mw1 = new TextBox()
            {
                GridRow = 7,
                GridColumn = 6,
                HintText = "W+16H+256SL",
                Width = 100
            };
            grid.Widgets.Add(mw1);
            TextBox mw2 = new TextBox()
            {
                GridRow = 7,
                GridColumn = 7,
                HintText = "MovePeriod",
                Width = 100
            };
            grid.Widgets.Add(mw2);
            TextBox mw3 = new TextBox()
            {
                GridRow = 7,
                GridColumn = 8,
                HintText = "Dir + 4ST",
                Width = 100
            };
            grid.Widgets.Add(mw3);
            TextBox mw4 = new TextBox()
            {
                GridRow = 7,
                GridColumn = 9,
                HintText = "WaitPeriod1",
                Width = 100
            };
            grid.Widgets.Add(mw4);
            TextBox mw5 = new TextBox()
            {
                GridRow = 7,
                GridColumn = 10,
                HintText = "WaitPeriod2",
                Width = 100
            };
            grid.Widgets.Add(mw5);
            var mwAdd = new TextButton
            {
                GridColumn = 11,
                GridRow = 7,
                Text = "Moving Wall",
                Width = 100,
            };
            mwAdd.Click += (s, a) =>
            {
                addToMovingObjectsBox(id.Text, ParadisePhysicalMapLogic.ObjectsStr[14], mw1.Text, mw2.Text, mw3.Text, mw4.Text, mw5.Text);
            };
            grid.Widgets.Add(mwAdd);
            // Shooter
            TextBox shooter1 = new TextBox()
            {
                GridRow = 8,
                GridColumn = 6,
                HintText = "MinDir",
                Width = 100
            };
            grid.Widgets.Add(shooter1);
            TextBox shooter2 = new TextBox()
            {
                GridRow = 8,
                GridColumn = 7,
                HintText = "MaxDir",
                Width = 100
            };
            grid.Widgets.Add(shooter2);
            TextBox shooter3 = new TextBox()
            {
                GridRow = 8,
                GridColumn = 8,
                HintText = "StartTime",
                Width = 100
            };
            grid.Widgets.Add(shooter3);
            TextBox shooter4 = new TextBox()
            {
                GridRow = 8,
                GridColumn = 9,
                HintText = "Period",
                Width = 100
            };
            grid.Widgets.Add(shooter4);
            var shooterAdd = new TextButton
            {
                GridColumn = 11,
                GridRow = 8,
                Text = "Add shooter",
                Width = 100,
            };
            shooterAdd.Click += (s, a) =>
            {
                addToMovingObjectsBox(id.Text, ParadisePhysicalMapLogic.ObjectsStr[5], shooter1.Text, shooter2.Text, shooter3.Text, shooter4.Text);
            };
            grid.Widgets.Add(shooterAdd);
            // RollerRing
            TextBox rr1 = new TextBox()
            {
                GridRow = 10,
                GridColumn = 6,
                HintText = "NumBalls",
                Width = 100
            };
            grid.Widgets.Add(rr1);
            TextBox rr2 = new TextBox()
            {
                GridRow = 10,
                GridColumn = 7,
                HintText = "Radius",
                Width = 100
            };
            grid.Widgets.Add(rr2);
            TextBox rr3 = new TextBox()
            {
                GridRow = 10,
                GridColumn = 8,
                HintText = "MovePeriod",
                Width = 100
            };
            grid.Widgets.Add(rr3);
            TextBox rr4 = new TextBox()
            {
                GridRow = 10,
                GridColumn = 9,
                HintText = "StartAngle",
                Width = 100
            };
            grid.Widgets.Add(rr4);
            var rrAdd = new TextButton
            {
                GridColumn = 11,
                GridRow = 10,
                Text = "Roller ring",
                Width = 100,
            };
            rrAdd.Click += (s, a) =>
            {
                addToMovingObjectsBox(id.Text, ParadisePhysicalMapLogic.ObjectsStr[6], rr1.Text, rr2.Text, rr3.Text, rr4.Text);
            };
            grid.Widgets.Add(rrAdd);
            // Cog
            var cogAdd = new TextButton
            {
                GridColumn = 11,
                GridRow = 11,
                Text = "Add cog",
                Width = 100,
            };
            cogAdd.Click += (s, a) =>
            {
                addToMovingObjectsBox(id.Text, ParadisePhysicalMapLogic.ObjectsStr[7], rr1.Text, rr2.Text, rr3.Text, rr4.Text);
            };
            grid.Widgets.Add(cogAdd);
            // RingOfFire
            var rfAdd = new TextButton
            {
                GridColumn = 11,
                GridRow = 12,
                Text = "Ring of Fire",
                Width = 100,
            };
            rfAdd.Click += (s, a) =>
            {
                addToMovingObjectsBox(id.Text, ParadisePhysicalMapLogic.ObjectsStr[9], rr1.Text, rr2.Text, rr3.Text, rr4.Text);
            };
            grid.Widgets.Add(rfAdd);
            // ArcOfFire
            TextBox af5 = new TextBox()
            {
                GridRow = 13,
                GridColumn = 10,
                HintText = "AngleRange",
                Width = 100
            };
            grid.Widgets.Add(af5);
            var afAdd = new TextButton
            {
                GridColumn = 11,
                GridRow = 13,
                Text = "Arc of Fire",
                Width = 100,
            };
            afAdd.Click += (s, a) =>
            {
                addToMovingObjectsBox(id.Text, ParadisePhysicalMapLogic.ObjectsStr[8], rr1.Text, rr2.Text, rr3.Text, rr4.Text, af5.Text);
            };
            grid.Widgets.Add(afAdd);
            // ClockHand
            TextBox ch1 = new TextBox()
            {
                GridRow = 15,
                GridColumn = 6,
                HintText = "MovePeriod",
                Width = 100
            };
            grid.Widgets.Add(ch1);
            TextBox ch2 = new TextBox()
            {
                GridRow = 15,
                GridColumn = 7,
                HintText = "BaseAngle",
                Width = 100
            };
            grid.Widgets.Add(ch2);
            var chAdd = new TextButton
            {
                GridColumn = 11,
                GridRow = 15,
                Text = "Clock Hand",
                Width = 100,
            };
            chAdd.Click += (s, a) =>
            {
                addToMovingObjectsBox(id.Text, ParadisePhysicalMapLogic.ObjectsStr[10], ch1.Text, ch2.Text);
            };
            grid.Widgets.Add(chAdd);
            // Sword
            var swAdd = new TextButton
            {
                GridColumn = 11,
                GridRow = 16,
                Text = "Add Sword",
                Width = 100,
            };
            swAdd.Click += (s, a) =>
            {
                addToMovingObjectsBox(id.Text, ParadisePhysicalMapLogic.ObjectsStr[13], ch1.Text, ch2.Text);
            };
            grid.Widgets.Add(swAdd);
            // Pendulum
            TextBox pend1 = new TextBox()
            {
                GridRow = 18,
                GridColumn = 6,
                HintText = "MovePeriod",
                Width = 100
            };
            grid.Widgets.Add(pend1);
            TextBox pend2 = new TextBox()
            {
                GridRow = 18,
                GridColumn = 7,
                HintText = "BaseAngle",
                Width = 100
            };
            grid.Widgets.Add(pend2);
            TextBox pend3 = new TextBox()
            {
                GridRow = 18,
                GridColumn = 8,
                HintText = "AngleRange",
                Width = 100
            };
            grid.Widgets.Add(pend3);
            TextBox pend4 = new TextBox()
            {
                GridRow = 18,
                GridColumn = 9,
                HintText = "BaseAnim",
                Width = 100
            };
            grid.Widgets.Add(pend4);
            var pendAdd = new TextButton
            {
                GridColumn = 11,
                GridRow = 18,
                Text = "Pendulum",
                Width = 100,
            };
            pendAdd.Click += (s, a) =>
            {
                addToMovingObjectsBox(id.Text, ParadisePhysicalMapLogic.ObjectsStr[11], pend1.Text, pend2.Text, pend3.Text, pend4.Text);
            };
            grid.Widgets.Add(pendAdd);
            // Ghost
            TextBox gh1 = new TextBox()
            {
                GridRow = 19,
                GridColumn = 6,
                HintText = "Direction",
                Width = 100
            };
            grid.Widgets.Add(gh1);
            TextBox gh2 = new TextBox()
            {
                GridRow = 19,
                GridColumn = 7,
                HintText = "MovePeriod",
                Width = 100
            };
            grid.Widgets.Add(gh2);
            TextBox gh3 = new TextBox()
            {
                GridRow = 19,
                GridColumn = 8,
                HintText = "Distance",
                Width = 100
            };
            grid.Widgets.Add(gh3);
            TextBox gh4 = new TextBox()
            {
                GridRow = 19,
                GridColumn = 9,
                HintText = "StartTime",
                Width = 100
            };
            grid.Widgets.Add(gh4);
            TextBox gh5 = new TextBox()
            {
                GridRow = 19,
                GridColumn = 10,
                HintText = "WaitPeriod",
                Width = 100
            };
            grid.Widgets.Add(gh5);
            var ghAdd = new TextButton
            {
                GridColumn = 11,
                GridRow = 19,
                Text = "Add ghost",
                Width = 100,
            };
            ghAdd.Click += (s, a) =>
            {
                addToMovingObjectsBox(id.Text, ParadisePhysicalMapLogic.ObjectsStr[12], gh1.Text, gh2.Text, gh3.Text, gh4.Text, gh5.Text);
            };
            grid.Widgets.Add(ghAdd);
            // Gate
            TextBox g1 = new TextBox()
            {
                GridRow = 20,
                GridColumn = 6,
                HintText = "KeyID",
                Width = 100
            };
            grid.Widgets.Add(g1);
            TextBox g2 = new TextBox()
            {
                GridRow = 20,
                GridColumn = 7,
                HintText = "Direction",
                Width = 100
            };
            grid.Widgets.Add(g2);
            var gAdd = new TextButton
            {
                GridColumn = 11,
                GridRow = 20,
                Text = "Add gate",
                Width = 100,
            };
            gAdd.Click += (s, a) =>
            {
                addToMovingObjectsBox(id.Text, ParadisePhysicalMapLogic.ObjectsStr[15], g1.Text, g2.Text);
            };
            grid.Widgets.Add(gAdd);

            movingObjects.Text = _logic.GetPrettyText();

            // --- SUBMIT ---

            var buttonQuit = new TextButton
            {
                GridColumn = 1,
                GridRow = 21,
                Text = "Quit",
                Width = 150,
            };
            buttonQuit.Click += (s, a) =>
            {
                saveChanges();
                _game.CloseSpecialItemMenu();
            };
            grid.Widgets.Add(buttonQuit);

            panel.Widgets.Add(grid);
            _desktop = new Desktop();
            _desktop.Root = panel;
        }

        public void Render()
        {
            _desktop.Render();
        }
    }
}
