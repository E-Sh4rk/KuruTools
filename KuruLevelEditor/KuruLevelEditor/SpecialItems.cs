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
    class SpecialItems
    {
        private Desktop _desktop;
        private Game1 _game;
        private PhysicalMapLogic _logic;

        private TextBox bonusId;
        private TextBox bonusX;
        private TextBox bonusY;

        private TextBox movingObjects;

        void saveChanges()
        {
            // Bonus
            PhysicalMapLogic.BonusInfo? bonus = null;
            if (!string.IsNullOrWhiteSpace(bonusId.Text))
            {
                try
                {
                    bonus = new PhysicalMapLogic.BonusInfo(
                        Convert.ToInt32(bonusId.Text), Convert.ToInt32(bonusX.Text), Convert.ToInt32(bonusY.Text));
                }
                catch { bonus = null; }
            }
            _logic.Bonus = bonus;
            // Moving objects
            List<object> moving = new List<object>();
            if (movingObjects.Text != null)
            {
                string[] lines = movingObjects.Text.Replace("\r", "").Split("\n", StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    try
                    {
                        string[] elts = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        int id = Convert.ToInt32(elts[0]);
                        string type = elts[1];
                        int p1 = Convert.ToInt32(elts[2]);
                        int p2 = 0, p3 = 0, p4 = 0;
                        if (type != "C")
                        {
                            p2 = Convert.ToInt32(elts[3]);
                            p3 = Convert.ToInt32(elts[4]);
                            p4 = Convert.ToInt32(elts[5]);
                        }
                        switch (type)
                        {
                            case "S":
                                moving.Add(new PhysicalMapLogic.ShooterInfo(id, p1, p2, p3, p4));
                                break;
                            case "P":
                                moving.Add(new PhysicalMapLogic.PistonInfo(id, p1, p2, p3, p4));
                                break;
                            case "R":
                                moving.Add(new PhysicalMapLogic.RollerInfo(id, p1, p2, p3, p4));
                                break;
                            case "C":
                                moving.Add(new PhysicalMapLogic.CatcherInfo(id, p1));
                                break;
                            default:
                                // Unrecognized object
                                break;
                        }
                    }
                    catch { }
                }
            }
            _logic.MovingObjects = moving;
        }

        void add4pToMovingObjectsBox(string ID, string type, string p1, string p2, string p3, string p4,
            int p1d, int p2d, int p3d, int p4d)
        {
            if (string.IsNullOrWhiteSpace(ID)) return;

            if (string.IsNullOrWhiteSpace(movingObjects.Text))
                movingObjects.Text = "";
            else if (!movingObjects.Text.EndsWith('\n'))
                movingObjects.Text += Environment.NewLine;

            if (string.IsNullOrWhiteSpace(p1))
                p1 = p1d.ToString();
            if (string.IsNullOrWhiteSpace(p2))
                p2 = p2d.ToString();
            if (string.IsNullOrWhiteSpace(p3))
                p3 = p3d.ToString();
            if (string.IsNullOrWhiteSpace(p4))
                p4 = p4d.ToString();

            movingObjects.Text += ID.PadLeft(5, ' ') + " " + type + " " + p1.PadLeft(5, ' ') + " "
                + p2.PadLeft(5, ' ') + " " + p3.PadLeft(5, ' ') + " " + p4.PadLeft(5, ' ') + Environment.NewLine;
        }

        void add1pToMovingObjectsBox(string ID, string type, string p1, int p1d)
        {
            if (string.IsNullOrWhiteSpace(ID)) return;

            if (string.IsNullOrWhiteSpace(movingObjects.Text))
                movingObjects.Text = "";
            else if (!movingObjects.Text.EndsWith('\n'))
                movingObjects.Text += Environment.NewLine;

            if (string.IsNullOrWhiteSpace(p1))
                p1 = p1d.ToString();

            movingObjects.Text += ID.PadLeft(5, ' ') + " " + type + " " + p1.PadLeft(5, ' ') + Environment.NewLine;
        }

        public SpecialItems(Game1 game, PhysicalMapLogic logic)
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

            // --- BONUS ---

            PhysicalMapLogic.BonusInfo? bonus = _logic.Bonus;
            Label labelBonus = new Label()
            {
                Id = "labelBonus",
                Text = "Bonus (ID/X/Y)",
                GridColumn = 0,
                GridRow = 0
            };
            grid.Widgets.Add(labelBonus);
            bonusId = new TextBox()
            {
                GridRow = 0,
                GridColumn = 1,
                HintText = "ID",
                Width = 150,
                Text = bonus.HasValue ? bonus.Value.ID.ToString() : null
            };
            grid.Widgets.Add(bonusId);
            bonusX = new TextBox()
            {
                GridRow = 0,
                GridColumn = 2,
                HintText = "X",
                Width = 150,
                Text = bonus.HasValue ? bonus.Value.x.ToString() : null
            };
            grid.Widgets.Add(bonusX);
            bonusY = new TextBox()
            {
                GridRow = 0,
                GridColumn = 3,
                HintText = "Y",
                Width = 150,
                Text = bonus.HasValue ? bonus.Value.y.ToString() : null
            };
            grid.Widgets.Add(bonusY);

            var bonusRemove = new TextButton
            {
                GridColumn = 4,
                GridRow = 0,
                Text = "Remove",
                Width = 150,
            };
            bonusRemove.Click += (s, a) =>
            {
                bonusId.Text = "";
                bonusX.Text = "";
                bonusY.Text = "";
            };
            grid.Widgets.Add(bonusRemove);

            var bonusShow = new TextButton
            {
                GridColumn = 5,
                GridRow = 0,
                Text = "Show/Set on map",
                Width = 150,
            };
            bonusShow.Click += (s, a) =>
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(bonusId.Text))
                    {
                        int id = Convert.ToInt32(bonusId.Text);
                        Point? loc = null;
                        if (!string.IsNullOrWhiteSpace(bonusX.Text) && !string.IsNullOrWhiteSpace(bonusY.Text))
                            loc = new Point(Convert.ToInt32(bonusX.Text), Convert.ToInt32(bonusY.Text));

                        saveChanges();
                        _game.SetBonusLocation(id, loc);
                    }
                }
                catch { }
            };
            grid.Widgets.Add(bonusShow);

            // --- MOVING OBJECTS ---
            var scroll = new ScrollViewer()
            {
                GridRow = 2,
                GridColumn = 0,
                GridColumnSpan = 6,
                GridRowSpan = 14,
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

            Label labelShooter = new Label()
            {
                Id = "labelShooter",
                Text = "Shooter",
                GridColumn = 0,
                GridRow = 16
            };
            grid.Widgets.Add(labelShooter);
            TextBox shooterId = new TextBox()
            {
                GridRow = 16,
                GridColumn = 1,
                HintText = "ID",
                Width = 150
            };
            grid.Widgets.Add(shooterId);
            TextBox shooterMinDir = new TextBox()
            {
                GridRow = 16,
                GridColumn = 2,
                HintText = "MinDir",
                Width = 150
            };
            grid.Widgets.Add(shooterMinDir);
            TextBox shooterMaxDir = new TextBox()
            {
                GridRow = 16,
                GridColumn = 3,
                HintText = "MaxDir",
                Width = 150
            };
            grid.Widgets.Add(shooterMaxDir);
            TextBox shooterStartTime = new TextBox()
            {
                GridRow = 16,
                GridColumn = 4,
                HintText = "StartTime",
                Width = 150
            };
            grid.Widgets.Add(shooterStartTime);
            TextBox shooterPeriod = new TextBox()
            {
                GridRow = 16,
                GridColumn = 5,
                HintText = "Period",
                Width = 150
            };
            grid.Widgets.Add(shooterPeriod);
            var shooterAdd = new TextButton
            {
                GridColumn = 6,
                GridRow = 16,
                Text = "Add shooter",
                Width = 150,
            };
            shooterAdd.Click += (s, a) =>
            {
                add4pToMovingObjectsBox(shooterId.Text, "S", shooterMinDir.Text, shooterMaxDir.Text, shooterStartTime.Text, shooterPeriod.Text,
                    0, 0, 0, PhysicalMapLogic.ShooterInfo.DEFAULT_PERIOD);
            };
            grid.Widgets.Add(shooterAdd);

            Label labelPiston = new Label()
            {
                Id = "labelPiston",
                Text = "Piston",
                GridColumn = 0,
                GridRow = 17
            };
            grid.Widgets.Add(labelPiston);
            TextBox pistonId = new TextBox()
            {
                GridRow = 17,
                GridColumn = 1,
                HintText = "ID",
                Width = 150
            };
            grid.Widgets.Add(pistonId);
            TextBox pistonDir = new TextBox()
            {
                GridRow = 17,
                GridColumn = 2,
                HintText = "Dir",
                Width = 150
            };
            grid.Widgets.Add(pistonDir);
            TextBox pistonStartTime = new TextBox()
            {
                GridRow = 17,
                GridColumn = 3,
                HintText = "StartTime",
                Width = 150
            };
            grid.Widgets.Add(pistonStartTime);
            TextBox pistonWaitPeriod = new TextBox()
            {
                GridRow = 17,
                GridColumn = 4,
                HintText = "WaitPeriod",
                Width = 150
            };
            grid.Widgets.Add(pistonWaitPeriod);
            TextBox pistonMovePeriod = new TextBox()
            {
                GridRow = 17,
                GridColumn = 5,
                HintText = "MovePeriod",
                Width = 150
            };
            grid.Widgets.Add(pistonMovePeriod);
            var pistonAdd = new TextButton
            {
                GridColumn = 6,
                GridRow = 17,
                Text = "Add piston",
                Width = 150,
            };
            pistonAdd.Click += (s, a) =>
            {
                add4pToMovingObjectsBox(pistonId.Text, "P", pistonDir.Text, pistonStartTime.Text, pistonWaitPeriod.Text, pistonMovePeriod.Text,
                    0, 0, PhysicalMapLogic.PistonInfo.DEFAULT_WAIT_PERIOD, PhysicalMapLogic.PistonInfo.DEFAULT_MOVE_PERIOD);
            };
            grid.Widgets.Add(pistonAdd);

            Label labelRoller = new Label()
            {
                Id = "labelRoller",
                Text = "Roller",
                GridColumn = 0,
                GridRow = 18
            };
            grid.Widgets.Add(labelRoller);
            TextBox rollerId = new TextBox()
            {
                GridRow = 18,
                GridColumn = 1,
                HintText = "ID",
                Width = 150
            };
            grid.Widgets.Add(rollerId);
            TextBox rollerDir = new TextBox()
            {
                GridRow = 18,
                GridColumn = 2,
                HintText = "Dir",
                Width = 150
            };
            grid.Widgets.Add(rollerDir);
            TextBox rollerStartTime = new TextBox()
            {
                GridRow = 18,
                GridColumn = 3,
                HintText = "StartTime",
                Width = 150
            };
            grid.Widgets.Add(rollerStartTime);
            TextBox rollerPeriod = new TextBox()
            {
                GridRow = 18,
                GridColumn = 4,
                HintText = "Period",
                Width = 150
            };
            grid.Widgets.Add(rollerPeriod);
            TextBox rollerSpeed = new TextBox()
            {
                GridRow = 18,
                GridColumn = 5,
                HintText = "Speed",
                Width = 150
            };
            grid.Widgets.Add(rollerSpeed);
            var rollerAdd = new TextButton
            {
                GridColumn = 6,
                GridRow = 18,
                Text = "Add roller",
                Width = 150,
            };
            rollerAdd.Click += (s, a) =>
            {
                add4pToMovingObjectsBox(rollerId.Text, "R", rollerDir.Text, rollerStartTime.Text, rollerPeriod.Text, rollerSpeed.Text,
                    0, 0, 120, PhysicalMapLogic.RollerInfo.DEFAULT_SPEED);
            };
            grid.Widgets.Add(rollerAdd);

            Label labelCatcher = new Label()
            {
                Id = "labelCatcher",
                Text = "Roller Catcher",
                GridColumn = 0,
                GridRow = 19
            };
            grid.Widgets.Add(labelCatcher);
            TextBox catcherId = new TextBox()
            {
                GridRow = 19,
                GridColumn = 1,
                HintText = "ID",
                Width = 150
            };
            grid.Widgets.Add(catcherId);
            TextBox catcherDir = new TextBox()
            {
                GridRow = 19,
                GridColumn = 2,
                HintText = "Dir",
                Width = 150
            };
            grid.Widgets.Add(catcherDir);
            var catcherAdd = new TextButton
            {
                GridColumn = 6,
                GridRow = 19,
                Text = "Add catcher",
                Width = 150,
            };
            catcherAdd.Click += (s, a) =>
            {
                add1pToMovingObjectsBox(catcherId.Text, "C", catcherDir.Text, 0);
            };
            grid.Widgets.Add(catcherAdd);

            foreach (object o in _logic.MovingObjects)
            {
                int id;
                string type;
                int[] param;
                if (o is PhysicalMapLogic.CatcherInfo)
                {
                    PhysicalMapLogic.CatcherInfo ci = (PhysicalMapLogic.CatcherInfo)o;
                    id = ci.ID;
                    type = "C";
                    param = ci.Params();
                    add1pToMovingObjectsBox(id.ToString(), type, param[0].ToString(), 0);
                }
                else
                {
                    if (o is PhysicalMapLogic.ShooterInfo)
                    {
                        PhysicalMapLogic.ShooterInfo si = (PhysicalMapLogic.ShooterInfo)o;
                        id = si.ID;
                        type = "S";
                        param = si.Params();
                    }
                    else if (o is PhysicalMapLogic.PistonInfo)
                    {
                        PhysicalMapLogic.PistonInfo pi = (PhysicalMapLogic.PistonInfo)o;
                        id = pi.ID;
                        type = "P";
                        param = pi.Params();
                    }
                    else // if (o is PhysicalMapLogic.RollerInfo)
                    {
                        PhysicalMapLogic.RollerInfo ri = (PhysicalMapLogic.RollerInfo)o;
                        id = ri.ID;
                        type = "R";
                        param = ri.Params();
                    }
                    add4pToMovingObjectsBox(id.ToString(), type, param[0].ToString(), param[1].ToString(), param[2].ToString(),
                        param[3].ToString(), 0, 0, 0, 0);
                }
            }

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
