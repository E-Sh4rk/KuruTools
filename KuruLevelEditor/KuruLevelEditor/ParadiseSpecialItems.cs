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
                    + p2.PadLeft(5, ' ') + " " + p3.PadLeft(5, ' ') + " " + p4.PadLeft(5, ' ') + p5.PadLeft(5, ' ');
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
                GridRowSpan = 20,
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
            // Piston
            // Shooter
            // RollerRing
            // Cog
            // ArcOfFire
            // RingOfFire
            // ClockHand
            // Pendulum
            // Ghost
            // Sword
            // MovingWall
            // Gate

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
