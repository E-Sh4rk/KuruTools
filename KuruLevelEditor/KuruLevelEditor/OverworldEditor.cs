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
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace KuruLevelEditor
{
    class OverworldEditor
    {
        private Desktop _desktop;
        private Game1 _game;
        private RenderTarget2D _renderTarget;

        public const int PADDING = 20;

        public const int SCALE = 2;
        public const int NUM_MARKERS = 54;
        public const int FIRST_MAGIC_HAT = 43;
        public const int VERTICAL_PANEL_HEIGHT = 200;

        enum Connection { East, West, South, North}
        enum Exit { Normal, Secret}
        enum Coords { X, Y}
        enum DoorKey { Neither, Door, Key}
        enum MarkerType { NormalLevel, MagicHat}

        private TextButton buttonSaveMarker;

        private TextBox[] connections = new TextBox[4];
        private Label labelConnections;
        private TextBox[] exits = new TextBox[2];
        private TextBox[] coords = new TextBox[2];
        private TextBox doorKey;
        private ComboBox doorKeyCombo;
        private ScrollViewer mapScroll;

        private Rectangle clickableBounds;
        private int selectedMarker = -1;

        private class OverworldObject
        {
            public byte East, West, South, North;
            public byte NormalExit, SecretExit;
            public ushort X, Y;
            public byte DoorKey;
            public MarkerType type;

            public Rectangle rect;
            public Texture2D img;

            public int xOffset;
            public int yOffset;
            public int size;

            public OverworldObject(uint[] data, MarkerType type)
            {
                this.type = type;
                SetData(data);

                //Image
                img = (type == MarkerType.NormalLevel) ? Load.Star : Load.MagicHatUnbeaten;
            }

            public void Draw(SpriteBatch spriteBatch)
            {
                spriteBatch.Draw(img, rect, Color.White);
            }

            public uint[] GetData()
            {
                return new uint[] { East, West, South, North, NormalExit, SecretExit, X, Y, DoorKey };
            }

            public void SetData(uint[] data)
            {
                East = (byte)data[0];
                West = (byte)data[1];
                South = (byte)data[2];
                North = (byte)data[3];
                NormalExit = (byte)data[4];
                SecretExit = (byte)data[5];
                X = (ushort)data[6];
                Y = (ushort)data[7];
                DoorKey = (byte)data[8];

                //Rect detecting mouse click position
                xOffset = (type == MarkerType.NormalLevel) ? -8 : -16; // Internal offsets in-game for displaying markers
                yOffset = (type == MarkerType.NormalLevel) ? -8 : -14;
                size = (type == MarkerType.NormalLevel) ? 16 : 32;
                rect = new Rectangle((X + xOffset) * SCALE, (Y + yOffset) * SCALE, size * SCALE, size * SCALE);
            }
        }

        private OverworldObject[] markers;

        void saveChanges()
        {
            uint[,] table = new uint[NUM_MARKERS, 9];
            for (int j = 0; j < table.GetLength(0); j++)
            {
                uint[] dat = markers[j].GetData();
                for (int i = 0; i < table.GetLength(1); i++)
                {
                    table[j, i] = dat[i];
                }
            }
            string result = Utils.UintTableToString(table, false);
            File.WriteAllText(Levels.GetOverworldMarkerPath(), result);
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
                Padding = new Myra.Graphics2D.Thickness(PADDING, PADDING, PADDING, PADDING)
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
                    case (int)Connection.South:
                        connectionBox.GridColumn = 2;
                        connectionBox.GridRow = 3;
                        break;
                    case (int)Connection.North:
                        connectionBox.GridColumn = 2;
                        connectionBox.GridRow = 1;
                        break;
                }

                connections[i] = connectionBox;
                grid.Widgets.Add(connectionBox);
            }

            labelConnections = new Label()
            {
                Id = "labelConnections",
                Text = "Level Connections",
                GridColumn = 2,
                GridRow = 2
            };
            grid.Widgets.Add(labelConnections);

            // Exits

            var labelNormalExit = new Label()
            {
                Id = "labelNormalExit",
                Text = "Normal Exit Level:",
                GridColumn = 4,
                GridRow = 1
            };
            grid.Widgets.Add(labelNormalExit);
            var labelSecretExit = new Label()
            {
                Id = "labelSecretExit",
                Text = "Secret Exit Level:",
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
                Width = 150,
                Readonly = true
            };
            grid.Widgets.Add(doorKey);

            doorKeyCombo = new ComboBox
            {
                GridColumn = 8,
                GridRow = 1,
                Width = 150,
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

            var buttonHelp = new TextButton
            {
                GridColumn = 8,
                GridRow = 3,
                Text = "Help",
                Width = 150,
            };
            buttonHelp.Click += (s, a) =>
            {
                var messageBox = Dialog.CreateMessageBox("Help",
                    "Only the westward connection is used\n" +
                    "to render small blue dots between markers.\n" +
                    "\n" +
                    "255 for connection or exit level = None.\n" +
                    "\n" +
                    "Door/Key values are affect Kururin's overworld\n" +
                    "sprite and are *not* associated with the actual\n" +
                    "door/key inside the level.\n" +
                    "\n" +
                    "Door/Key values have a maximum value of 15.\n" +
                    "(Higher values aren't saved.)\n"
                    );
                messageBox.ShowModal(_desktop);
            };
            grid.Widgets.Add(buttonHelp);

            // --- SUBMIT ---

            buttonSaveMarker = new TextButton
            {
                GridColumn = 9,
                GridRow = 1,
                Text = "Save this marker",
                Width = 150,
                Enabled = false
            };
            buttonSaveMarker.Click += (s, a) =>
            {
                markers[selectedMarker].SetData(GetDataFromGUI());
                UpdateGUI(markers[selectedMarker]); //case where user enters invalid data - reverts to original data
            };
            grid.Widgets.Add(buttonSaveMarker);

            var buttonSaveAllAndQuit = new TextButton
            {
                GridColumn = 9,
                GridRow = 2,
                Text = "Save All & Quit",
                Width = 150,
            };
            buttonSaveAllAndQuit.Click += (s, a) =>
            {
                saveChanges();
                _game.CloseOverworldEditor();
            };
            grid.Widgets.Add(buttonSaveAllAndQuit);

            var buttonCancel = new TextButton
            {
                GridColumn = 9,
                GridRow = 3,
                Text = "Cancel",
                Width = 150,
            };
            buttonCancel.Click += (s, a) =>
            {
                _game.CloseOverworldEditor();
            };
            grid.Widgets.Add(buttonCancel);

            clickableBounds = new Rectangle(PADDING, VERTICAL_PANEL_HEIGHT + PADDING, 
                _game.GraphicsDevice.Viewport.Width - 2 * PADDING,
                Load.OverworldMap.Height * SCALE);

            _renderTarget = new RenderTarget2D(_game.GraphicsDevice, Load.OverworldMap.Width*SCALE, Load.OverworldMap.Height*SCALE);

            Image img = new Image()
            {
                Renderable = new TextureRegion(_renderTarget)
            };
            mapScroll = new ScrollViewer()
            {
                GridColumn = 0,
                GridRow = 4,
                GridColumnSpan = 10,
                Padding = new Myra.Graphics2D.Thickness(0, VERTICAL_PANEL_HEIGHT, 0, 0),
                ShowHorizontalScrollBar = true,
                ShowVerticalScrollBar = false
            };
            mapScroll.Content = img;

            panel.Widgets.Add(mapScroll);

            panel.Widgets.Add(grid);
            _desktop = new Desktop();
            _desktop.Root = panel;
        }

        public uint[] GetDataFromGUI()
        {
            uint doorKeyVal = 0;
            if (doorKeyCombo.SelectedIndex == (int)DoorKey.Door)
                doorKeyVal = uint.Parse(doorKey.Text);
            else if (doorKeyCombo.SelectedIndex == (int)DoorKey.Key)
                doorKeyVal = 256 - uint.Parse(doorKey.Text);
            try
            {
                return new uint[] { uint.Parse(connections[0].Text), uint.Parse(connections[1].Text), uint.Parse(connections[2].Text), uint.Parse(connections[3].Text),
                uint.Parse(exits[0].Text), uint.Parse(exits[1].Text), uint.Parse(coords[0].Text), uint.Parse(coords[1].Text), doorKeyVal};
            } catch (System.FormatException)
            {
                return markers[selectedMarker].GetData();
            }
        }

        bool mouse_move_is_selecting = false;
        public void Update(MouseState mouse)
        {
            if (mouse.LeftButton == ButtonState.Released)
            {
                if (mouse_move_is_selecting)
                {
                    //Translate mouse position to coords within the scroll viewer (+ scale it)
                    Point p = new Point(mouse.X, mouse.Y) + mapScroll.ScrollPosition - clickableBounds.Location;
                    if (clickableBounds.Contains(new Point(mouse.X, mouse.Y)))
                    {
                        //Pick a marker to select, if any
                        //Debug.WriteLine(p.ToString());
                        //bool found = false;
                        for (int i = 0; i < markers.Length; i++)
                        {
                            if (markers[i].rect.Contains(p))
                            {
                                //Debug.WriteLine("found");
                                //Debug.WriteLine("[{0}]", string.Join(", ", markers[i].GetData()));
                                selectedMarker = i;
                                //found = true;

                                UpdateGUI(markers[i]);
                                UpdateGUIReadOnly(false);
                                break;
                            }
                        }
                        //if (!found)
                        //{
                        //    selectedMarker = -1;
                        //}
                    }
                    mouse_move_is_selecting = false;
                }
            }
            else if (mouse.LeftButton == ButtonState.Pressed)
            {
                mouse_move_is_selecting = true;
            }
        }

        private void UpdateGUI(OverworldObject marker)
        {
            labelConnections.Text = String.Format("Level {0} Connections", selectedMarker);

            connections[(int)Connection.East].Text = marker.East.ToString();
            connections[(int)Connection.West].Text = marker.West.ToString();
            connections[(int)Connection.North].Text = marker.North.ToString();
            connections[(int)Connection.South].Text = marker.South.ToString();

            exits[(int)Exit.Normal].Text = marker.NormalExit.ToString();
            exits[(int)Exit.Secret].Text = marker.SecretExit.ToString();

            coords[(int)Coords.X].Text = marker.X.ToString();
            coords[(int)Coords.Y].Text = marker.Y.ToString();

            if (marker.DoorKey == 0)
            {
                doorKeyCombo.SelectedIndex = -1;
                doorKey.Text = "";
            }
            else if ((marker.DoorKey & 0x80) == 0)
            {
                doorKeyCombo.SelectedIndex = (int)DoorKey.Door;
                doorKey.Text = marker.DoorKey.ToString();
            }
            else
            {
                doorKeyCombo.SelectedIndex = (int)DoorKey.Key;
                doorKey.Text = (256 - marker.DoorKey).ToString();
            }
        }

        public void UpdateGUIReadOnly(bool readOnly)
        {
            foreach(TextBox b in connections) { b.Readonly = readOnly; }
            foreach(TextBox b in exits) { b.Readonly = readOnly; }
            foreach(TextBox b in coords) { b.Readonly = readOnly; }
            doorKey.Readonly = readOnly;
            //ComboBox has no readonly field
            buttonSaveMarker.Enabled = true;
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
                // A quirk of all the level markers in Paradise - they all have 1 westward connection to the previous level that unlocks it (except the first level)!
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

            if (selectedMarker != -1)
            {
                OverworldObject m = markers[selectedMarker];
                Texture2D sm = Load.SelectedMarker;
                spriteBatch.Draw(sm, new Rectangle((m.X - 2 + m.xOffset) * SCALE, (m.Y - 2 + m.yOffset) * SCALE, (m.size + 4) * SCALE, (m.size + 4) * SCALE), Color.White);
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
