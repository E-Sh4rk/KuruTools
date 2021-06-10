using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Resources;
using System.Text;
using System.Linq;

namespace KuruLevelEditor
{
    class OverworldEditor
    {
        private Desktop _desktop;
        private Game1 _game;
        private RenderTarget2D _renderTarget;

        public const int SCALE = 2;
        public const int NUM_MARKERS = 54;
        public const int FIRST_MAGIC_HAT = 43;

        enum Connection { East, West, North, South}
        enum Exit { Normal, Secret}
        enum Coords { X, Y}
        enum DoorKey { Neither, Door, Key}
        enum MarkerType { NormalLevel, MagicHat}

        private TextBox[] connections = new TextBox[4];
        private Label labelConnections;
        private TextBox[] exits = new TextBox[2];
        private TextBox[] coords = new TextBox[2];
        private TextBox doorKey;
        private ComboBox doorKeyCombo;

        private class OverworldObject
        {
            public byte East, West, North, South;
            public byte NormalExit, SecretExit;
            public ushort X, Y;
            public byte DoorKey;
            public MarkerType type;

            public OverworldObject(uint[] data, MarkerType type)
            {
                East = (byte)data[0];
                West = (byte)data[1];
                North = (byte)data[2];
                South = (byte)data[3];
                NormalExit = (byte)data[4];
                SecretExit = (byte)data[5];
                X = (ushort)data[6];
                Y = (ushort)data[7];
                DoorKey = (byte)data[8];
                this.type = type;
            }

            public Rectangle rect()
            {
                int xOffset = (type == MarkerType.NormalLevel) ? -8 : -16; // Internal offsets in-game for displaying markers
                int yOffset = (type == MarkerType.NormalLevel) ? -8 : -14;
                int size = (type == MarkerType.NormalLevel) ? 16 : 32;
                return new Rectangle((X + xOffset) * SCALE ,(Y + yOffset) * SCALE, size * SCALE, size * SCALE);
            }

            public Texture2D img()
            {
                return (type == MarkerType.NormalLevel) ? Load.Star : Load.MagicHatUnbeaten;
            }

            public void Draw(SpriteBatch spriteBatch)
            {
                spriteBatch.Draw(img(), rect(), Color.White);
            }
        }

        private OverworldObject[] markers;

        void saveChanges()
        {

        }

        void loadData()
        {
            Load.LoadOverworldMap(_game.GraphicsDevice);
            Load.LoadOverworldObjects(_game.GraphicsDevice);

            string[] lines = Utils.SplitNonEmptyLines(File.ReadAllText(Levels.GetOverworldMarkerPath()));
            uint[,] table = Utils.LinesToUintTable(lines, lines.Length, 9, false);
            markers = new OverworldObject[NUM_MARKERS];

            for (int i = 0; i < NUM_MARKERS; i++)
            {
                uint[] row = Enumerable.Range(0, table.GetLength(1))
                                .Select(x => table[i, x])
                                .ToArray();
                MarkerType type = (i + 1 < FIRST_MAGIC_HAT) ? MarkerType.NormalLevel : MarkerType.MagicHat;
                markers[i] = new OverworldObject(row, type);
            }
        }

        public OverworldEditor(Game1 game)
        {
            _game = game;

            loadData();

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

            // Door/Key
            doorKey = new TextBox()
            {
                GridColumn = 8,
                GridRow = 2,
                Width = 90,
                Readonly = true
            };
            grid.Widgets.Add(doorKey);

            doorKeyCombo = new ComboBox
            {
                GridColumn = 8,
                GridRow = 1,
                Width = 90,
                SelectedIndex = (int)DoorKey.Neither
            };
            for (int i = 0; i < 3; i++)
            {
                doorKeyCombo.Items.Add(new ListItem(Enum.GetNames(typeof(DoorKey))[i], Color.White));
            }
            doorKeyCombo.SelectedIndexChanged += (s, a) =>
            {
                doorKey.Readonly = doorKeyCombo.SelectedIndex == (int)DoorKey.Neither;
            };
            grid.Widgets.Add(doorKeyCombo);

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

            _renderTarget = new RenderTarget2D(_game.GraphicsDevice, Load.OverworldMap.Width*SCALE, Load.OverworldMap.Height*SCALE);

            Image img = new Image()
            {
                Renderable = new TextureRegion(_renderTarget)
            };
            ScrollViewer mapScroll = new ScrollViewer()
            {
                GridColumn = 0,
                GridRow = 3,
                GridColumnSpan = 10,
                Padding = new Myra.Graphics2D.Thickness(0, 200, 0, 0),
                ShowHorizontalScrollBar = true,
                ShowVerticalScrollBar = false
            };
            mapScroll.Content = img;

            panel.Widgets.Add(mapScroll);

            panel.Widgets.Add(grid);
            _desktop = new Desktop();
            _desktop.Root = panel;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _game.GraphicsDevice.SetRenderTarget(_renderTarget);
            _game.GraphicsDevice.Clear(Color.Transparent);
            Texture2D map = Load.OverworldMap;
            spriteBatch.Begin();
            spriteBatch.Draw(map, new Rectangle(0, 0, map.Width*SCALE, map.Height*SCALE), Color.White);
            //test
            foreach (OverworldObject marker in markers)
            {
                /*
                if (marker.East >= 0 && marker.East < NUM_MARKERS)
                {
                    DrawConnectorDots(spriteBatch, marker.X, markers[marker.East].X, marker.Y, markers[marker.East].Y);
                }*/
                // A quirk of all the level markers in Paradise - they all have 1 westward connection!
                if (marker.West >= 0 && marker.West < NUM_MARKERS)
                {
                    DrawConnectorDots(spriteBatch, marker.X, markers[marker.West].X, marker.Y, markers[marker.West].Y);
                }
                /*
                if (marker.North >= 0 && marker.North < NUM_MARKERS)
                {
                    DrawConnectorDots(spriteBatch, marker.X, markers[marker.North].X, marker.Y, markers[marker.North].Y);
                }
                if (marker.South >= 0 && marker.South < NUM_MARKERS)
                {
                    DrawConnectorDots(spriteBatch, marker.X, markers[marker.South].X, marker.Y, markers[marker.South].Y);
                }*/
            }
            //Connector dots go behind level/hat markers
            foreach (OverworldObject marker in markers)
            {
                marker.Draw(spriteBatch);
            }

            spriteBatch.End();
            _game.GraphicsDevice.SetRenderTarget(null);
        }

        public void DrawConnectorDots(SpriteBatch spriteBatch, int x1, int x2, int y1, int y2)
        {

            int xDiff = x1 - x2;
            int yDiff = y1 - y2;
            if (xDiff < 0) xDiff += 3;
            xDiff >>= 2;
            if (yDiff < 0) yDiff += 3;
            yDiff >>= 2;

            for (int i = 1; i < 4; i++)
            {
                spriteBatch.Draw(Load.ConnectorDot,
                    new Rectangle((x2 - 3 + xDiff * i) * SCALE,
                        (y2 - 3 + yDiff * i) * SCALE,
                        8 * SCALE, 8 * SCALE), 
                    Color.White);
            }
        }

        public void Render()
        {
            _desktop.Render();
        }
    }
}
