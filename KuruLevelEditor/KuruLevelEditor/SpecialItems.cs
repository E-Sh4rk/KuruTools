using Microsoft.Xna.Framework;
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

        void saveChanges()
        {
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
            _logic.SetBonusInfo(bonus);
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

            PhysicalMapLogic.BonusInfo? bonus = _logic.GetBonusInfo();
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
                Text = bonus.HasValue ? bonus.Value.ID.ToString() : ""
            };
            grid.Widgets.Add(bonusId);
            bonusX = new TextBox()
            {
                GridRow = 0,
                GridColumn = 2,
                HintText = "X",
                Width = 150,
                Text = bonus.HasValue ? bonus.Value.x.ToString() : ""
            };
            grid.Widgets.Add(bonusX);
            bonusY = new TextBox()
            {
                GridRow = 0,
                GridColumn = 3,
                HintText = "Y",
                Width = 150,
                Text = bonus.HasValue ? bonus.Value.y.ToString() : ""
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
                        game.SetBonusLocation(id, loc);
                    }
                }
                catch { }
            };
            grid.Widgets.Add(bonusShow);

            // --- SUBMIT ---

            var buttonQuit = new TextButton
            {
                GridColumn = 1,
                GridRow = 2,
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
