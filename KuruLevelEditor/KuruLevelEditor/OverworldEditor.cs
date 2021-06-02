using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Resources;
using System.Text;

namespace KuruLevelEditor
{
    class OverworldEditor
    {
        private Desktop _desktop;
        private Game1 _game;

        enum Connection { East, West, North, South}
        enum Exit { Normal, Secret}
        enum Coords { X, Y}

        private TextBox[] connections = new TextBox[4];
        private Label labelConnections;
        private TextBox[] exits = new TextBox[2];
        private TextBox[] coords = new TextBox[2];
        private TextBox[] doorKey;

        void saveChanges()
        {

        }

        void loadData()
        {

        }

        public OverworldEditor(Game1 game)
        {
            _game = game;

            Panel panel = new Panel()
            {
                Background = new SolidBrush(Color.Black),
                Padding = new Myra.Graphics2D.Thickness(20, 20, 20, 20)
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

            Label labelExplanation = new Label()
            {
                Id = "labelExplanationOverworld",
                Text = "Select an overworld marker below to edit its properties.",
                GridColumn = 0,
                GridRow = 0,
                GridColumnSpan = 9
            };
            grid.Widgets.Add(labelExplanation);

            //Connections
            for (int i = 0; i < 4; i++)
            {
                TextBox connectionBox = new TextBox()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Readonly = true,
                    Width = 30
                };
                
                switch (i)
                {
                    case (int) Connection.East:
                        connectionBox.GridColumn = 3;
                        connectionBox.GridRow = 2;
                        break;
                    case (int) Connection.West:
                        connectionBox.GridColumn = 1;
                        connectionBox.GridRow = 2;
                        break;
                    case (int)Connection.North:
                        connectionBox.GridColumn = 2;
                        connectionBox.GridRow = 1;
                        break;
                    case (int)Connection.South:
                        connectionBox.GridColumn = 2;
                        connectionBox.GridRow = 3;
                        break;
                }

                connections[i] = connectionBox;
                grid.Widgets.Add(connectionBox);
            }

            labelConnections = new Label()
            {
                Id = "labelConnections",
                Text = "Map Connections",
                GridColumn = 2,
                GridRow = 2
            };
            grid.Widgets.Add(labelConnections);

            // Exits

            var labelNormalExit = new Label()
            {
                Id = "labelNormalExit",
                Text = "Normal Exit Map:",
                GridColumn = 4,
                GridRow = 1
            };
            grid.Widgets.Add(labelNormalExit);
            var labelSecretExit = new Label()
            {
                Id = "labelSecretExit",
                Text = "Secret Exit Map:",
                GridColumn = 4,
                GridRow = 2
            };
            grid.Widgets.Add(labelSecretExit);

            for (int i = 0; i < 2; i++)
            {
                TextBox exitBox = new TextBox()
                {
                    Readonly = true,
                    Width = 30
                };

                switch (i)
                {
                    case ((int) Exit.Normal):
                        exitBox.GridColumn = 5;
                        exitBox.GridRow = 1;
                        break;
                    case ((int)Exit.Secret):
                        exitBox.GridColumn = 5;
                        exitBox.GridRow = 2;
                        break;
                }

                exits[i] = exitBox;
                grid.Widgets.Add(exitBox);
            }

            // Coordinates

            var labelXCoord = new Label()
            {
                Id = "labelXCoord",
                Text = "X Coordinate:",
                GridColumn = 6,
                GridRow = 1
            };
            grid.Widgets.Add(labelXCoord);
            var labelYCoord = new Label()
            {
                Id = "labelYCoord",
                Text = "Y Coordinate:",
                GridColumn = 6,
                GridRow = 2
            };
            grid.Widgets.Add(labelYCoord);

            for (int i = 0; i < 2; i++)
            {
                TextBox coordBox = new TextBox()
                {
                    Readonly = true,
                    Width = 60
                };

                switch (i)
                {
                    case ((int)Coords.X):
                        coordBox.GridColumn = 7;
                        coordBox.GridRow = 1;
                        break;
                    case ((int)Coords.Y):
                        coordBox.GridColumn = 7;
                        coordBox.GridRow = 2;
                        break;
                }

                coords[i] = coordBox;
                grid.Widgets.Add(coordBox);
            }

            // --- SUBMIT ---

            var buttonSaveQuit = new TextButton
            {
                GridColumn = 9,
                GridRow = 1,
                Text = "Save and Quit",
                Width = 150,
            };
            buttonSaveQuit.Click += (s, a) =>
            {
                saveChanges();
                _game.CloseOverworldEditor();
            };
            grid.Widgets.Add(buttonSaveQuit);

            var buttonCancel = new TextButton
            {
                GridColumn = 9,
                GridRow = 2,
                Text = "Cancel",
                Width = 150,
            };
            buttonCancel.Click += (s, a) =>
            {
                _game.CloseOverworldEditor();
            };
            grid.Widgets.Add(buttonCancel);

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
