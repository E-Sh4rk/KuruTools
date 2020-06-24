using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace KuruLevelEditor
{
    class SpecialItems
    {
        private Desktop _desktop;
        private Game1 _game;

        public SpecialItems(Game1 game)
        {
            _game = game;

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

            Label labelBonus = new Label()
            {
                Id = "labelBonus",
                Text = "Bonus",
                GridColumn = 0,
                GridRow = 0
            };
            grid.Widgets.Add(labelBonus);
            TextBox bonusId = new TextBox()
            {
                GridRow = 0,
                GridColumn = 1,
                HintText = "ID",
                Width = 150
            };
            grid.Widgets.Add(bonusId);
            TextBox bonusX = new TextBox()
            {
                GridRow = 0,
                GridColumn = 2,
                HintText = "X",
                Width = 150
            };
            grid.Widgets.Add(bonusX);
            TextBox bonusY = new TextBox()
            {
                GridRow = 0,
                GridColumn = 3,
                HintText = "Y",
                Width = 150
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
                // TODO
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
                // TODO
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
