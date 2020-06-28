using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KuruLevelEditor
{
    public class Game1 : Game
    {
        const int LATERAL_PANEL_WIDTH = 200;
        const int PALETTE_SELECTOR_Y = 250;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Desktop _mainMenuDesktop;
        private Desktop _lateralMenuDesktop;
        private SpecialItems _specialItemInterface = null;
        private PhysicalMapLogic _physicalMapLogic = null;

        enum Mode
        {
            Loading,
            Menu,
            Physical,
            Graphical,
            Background,
            Minimap
        }
        private Mode mode = Mode.Loading;
        private string map;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            MyraEnvironment.Game = this;

            if (!Settings.LoadSettings() || !Load.LoadFixedContent(Content))
            {
                Exit();
                return;
            }

            if (Levels.Init() && Load.LoadSpriteContent(GraphicsDevice))
                LoadInterface();
            else
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Settings.RunExtractor("--extract-tiles tiles");
                        reloadAtNextFrame = true;
                    }
                    catch { Exit(); }
                });
            }

            // TODO: Test ROM extractor and Emulator commands (what if in an other directory??)
            // TODO: Remember selection between grids so that it is possible to copy parts of other levels
        }

        void PleaseSelectMapMsg()
        {
            var messageBox = Dialog.CreateMessageBox("Error", "Please select an element and a map.");
            messageBox.ShowModal(_mainMenuDesktop);
        }
        Mode IndexToSelectedMode(int? index)
        {
            if (index == 0)
                return Mode.Physical;
            else if (index == 1)
                return Mode.Graphical;
            else if (index == 2)
                return Mode.Background;
            else if (index == 3)
                return Mode.Minimap;
            return Mode.Menu;
        }

        void LoadInterface()
        {
            // ===== MAIN MENU =====
            var grid = new Grid
            {
                RowSpacing = 10,
                ColumnSpacing = 20
            };

            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.HorizontalAlignment = HorizontalAlignment.Center;
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            grid.VerticalAlignment = VerticalAlignment.Center;

            var labelType = new Label
            {
                Id = "labelType",
                Text = "Choose the element to modify:",
                GridColumn = 0,
                GridRow = 0,
            };
            grid.Widgets.Add(labelType);

            var comboType = new ComboBox
            {
                GridColumn = 1,
                GridRow = 0,
                Width = 150
            };
            comboType.Items.Add(new ListItem("Walls", Color.White));
            comboType.Items.Add(new ListItem("Ground", Color.White));
            comboType.Items.Add(new ListItem("Background", Color.White));
            comboType.Items.Add(new ListItem("MiniMap", Color.White));
            grid.Widgets.Add(comboType);

            var labelMap = new Label
            {
                Id = "labelMap",
                Text = "Choose the map to modify:",
                GridColumn = 0,
                GridRow = 1,
            };
            grid.Widgets.Add(labelMap);

            var comboMap = new ComboBox
            {
                GridColumn = 1,
                GridRow = 1,
                Width = 150
            };
            foreach (string name in Levels.AllLevels)
                comboMap.Items.Add(new ListItem(name, Color.White));
            grid.Widgets.Add(comboMap);

            var buttonEdit = new TextButton
            {
                GridColumn = 2,
                GridRow = 0,
                GridRowSpan = 2,
                Text = "Edit",
                Width = 150,
                Height = 60
            };

            buttonEdit.Click += (s, a) =>
            {
                if (comboMap.SelectedIndex == null || comboType.SelectedIndex == null)
                {
                    PleaseSelectMapMsg();
                }
                else
                {
                    mode = IndexToSelectedMode(comboType.SelectedIndex);
                    map = comboMap.SelectedItem.Text;
                    LoadGrid();
                }
            };

            grid.Widgets.Add(buttonEdit);

            var buttonReset = new TextButton
            {
                GridColumn = 3,
                GridRow = 0,
                Text = "Reset this map",
                Width = 150,
                Height = 25
            };
            buttonReset.Click += (s, a) =>
            {
                if (comboMap.SelectedIndex == null || comboType.SelectedIndex == null)
                {
                    PleaseSelectMapMsg();
                }
                else
                {
                    var messageBox = Dialog.CreateMessageBox("Confirmation", "Are you sure ? This level will be reset.");
                    messageBox.Closed += (s, a) =>
                    {
                        if (messageBox.Result)
                        {
                            Task.Factory.StartNew(() => {
                                try
                                {
                                    mode = Mode.Loading;
                                    File.Delete(Levels.GetLevelPath(comboMap.SelectedItem.Text,
                                        MapType(IndexToSelectedMode(comboType.SelectedIndex))));
                                    Settings.RunExtractor("");
                                    mode = Mode.Menu;
                                }
                                catch { Exit(); }
                            });
                        }
                    };
                    messageBox.ShowModal(_mainMenuDesktop);
                }
            };
            grid.Widgets.Add(buttonReset);

            var buttonResetAll = new TextButton
            {
                GridColumn = 3,
                GridRow = 1,
                Text = "Reset ALL maps",
                Width = 150,
                Height = 25
            };
            buttonResetAll.Click += (s, a) =>
            {
                var messageBox = Dialog.CreateMessageBox("Confirmation", "Are you sure ? ALL levels will be reset.");
                messageBox.Closed += (s, a) =>
                {
                    if (messageBox.Result)
                    {
                        Task.Factory.StartNew(() => {
                            try
                            {
                                mode = Mode.Loading;
                                Levels.DeleteAllLevels();
                                Settings.RunExtractor("");
                                reloadAtNextFrame = true;
                            }
                            catch { Exit(); }
                        });
                    }
                };
                messageBox.ShowModal(_mainMenuDesktop);
            };
            grid.Widgets.Add(buttonResetAll);

            var buttonBuild = new TextButton
            {
                GridColumn = 2,
                GridRow = 2,
                GridRowSpan = 2,
                Text = "Build",
                Width = 150,
                Height = 60
            };
            buttonBuild.Click += (s, a) =>
            {
                Task.Factory.StartNew(() => {
                    try
                    {
                        mode = Mode.Loading;
                        Settings.RunExtractor("");
                        mode = Mode.Menu;
                    }
                    catch { Exit(); }
                });
            };
            grid.Widgets.Add(buttonBuild);

            var buttonBuildAndRun = new TextButton
            {
                GridColumn = 3,
                GridRow = 2,
                GridRowSpan = 2,
                Text = "Build and run",
                Width = 150,
                Height = 60
            };
            buttonBuildAndRun.Click += (s, a) =>
            {
                Task.Factory.StartNew(() => {
                    try
                    {
                        mode = Mode.Loading;
                        Settings.RunExtractor("");
                        mode = Mode.Menu;
                        Settings.RunEmulator();
                    }
                    catch { Exit(); }
                });
            };
            grid.Widgets.Add(buttonBuildAndRun);

            _mainMenuDesktop = new Desktop();
            _mainMenuDesktop.Root = grid;

            // ===== LATERAL MENU =====
            Panel panel = new Panel()
            {
                Left = 0,
                Padding = new Thickness(10, 10, 10, 10),
                Width = LATERAL_PANEL_WIDTH,
                Background = new SolidBrush(Color.Transparent),
            };

            var lateral = new Grid
            {
                RowSpacing = 10,
                ColumnSpacing = 10
            };

            lateral.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            lateral.HorizontalAlignment = HorizontalAlignment.Left;
            lateral.RowsProportions.Add(new Proportion(ProportionType.Part));
            lateral.VerticalAlignment = VerticalAlignment.Top;

            var buttonSave = new TextButton
            {
                GridColumn = 0,
                GridRow = 0,
                Text = "Save",
                Width = 40,
                Height = 30
            };
            buttonSave.Click += (s, a) =>
            {
                SaveGrid();
            };
            lateral.Widgets.Add(buttonSave);

            var buttonSaveQuit = new TextButton
            {
                GridColumn = 1,
                GridRow = 0,
                Text = "Save + quit",
                Width = 90,
                Height = 30,
                GridColumnSpan = 2
            };
            buttonSaveQuit.Click += (s, a) =>
            {
                SaveGrid();
                mode = Mode.Menu;
            };
            lateral.Widgets.Add(buttonSaveQuit);

            var buttonQuit = new TextButton
            {
                GridColumn = 3,
                GridRow = 0,
                Text = "Quit",
                Width = 40,
                Height = 30
            };
            buttonQuit.Click += (s, a) =>
            {
                mode = Mode.Menu;
            };
            lateral.Widgets.Add(buttonQuit);

            var buttonWP = new TextButton
            {
                GridColumn = 0,
                GridRow = 1,
                Text = "W+",
                Width = 40,
                Height = 30
            };
            buttonWP.Click += (s, a) =>
            {
                editor.IncreaseWidth();
            };
            lateral.Widgets.Add(buttonWP);
            var buttonWM = new TextButton
            {
                GridColumn = 1,
                GridRow = 1,
                Text = "W-",
                Width = 40,
                Height = 30
            };
            buttonWM.Click += (s, a) =>
            {
                editor.DecreaseWidth();
            };
            lateral.Widgets.Add(buttonWM);
            var buttonHP = new TextButton
            {
                GridColumn = 2,
                GridRow = 1,
                Text = "H+",
                Width = 40,
                Height = 30
            };
            buttonHP.Click += (s, a) =>
            {
                editor.IncreaseHeight();
            };
            lateral.Widgets.Add(buttonHP);
            var buttonHM = new TextButton
            {
                GridColumn = 3,
                GridRow = 1,
                Text = "H-",
                Width = 40,
                Height = 30
            };
            buttonHM.Click += (s, a) =>
            {
                editor.DecreaseHeight();
            };
            lateral.Widgets.Add(buttonHM);

            var buttonOP = new TextButton
            {
                GridColumn = 0,
                GridRow = 2,
                Text = "Overlay +",
                Width = 90,
                Height = 30,
                GridColumnSpan = 2
            };
            buttonOP.Click += (s, a) =>
            {
                List<EditorGrid.OverlayGrid> overlays = new List<EditorGrid.OverlayGrid>();
                overlays.AddRange(editor.Underlays);
                overlays.AddRange(editor.Overlays);
                overlays.Reverse();
                foreach (EditorGrid.OverlayGrid overlay in overlays)
                {
                    if (!overlay.enabled)
                    {
                        overlay.enabled = true;
                        break;
                    }
                }
            };
            lateral.Widgets.Add(buttonOP);
            var buttonOM = new TextButton
            {
                GridColumn = 2,
                GridRow = 2,
                Text = "Overlay -",
                Width = 90,
                Height = 30,
                GridColumnSpan = 2
            };
            buttonOM.Click += (s, a) =>
            {
                List<EditorGrid.OverlayGrid> overlays = new List<EditorGrid.OverlayGrid>();
                overlays.AddRange(editor.Underlays);
                overlays.AddRange(editor.Overlays);
                foreach (EditorGrid.OverlayGrid overlay in overlays)
                {
                    if (overlay.enabled)
                    {
                        overlay.enabled = false;
                        break;
                    }
                }
            };
            lateral.Widgets.Add(buttonOM);

            var buttonGOn = new TextButton
            {
                GridColumn = 0,
                GridRow = 3,
                Text = "Grid On",
                Width = 90,
                Height = 30,
                GridColumnSpan = 2
            };
            buttonGOn.Click += (s, a) =>
            {
                editor.GridEnabled = true;
            };
            lateral.Widgets.Add(buttonGOn);
            var buttonGOff = new TextButton
            {
                GridColumn = 2,
                GridRow = 3,
                Text = "Grid Off",
                Width = 90,
                Height = 30,
                GridColumnSpan = 2
            };
            buttonGOff.Click += (s, a) =>
            {
                editor.GridEnabled = false;
            };
            lateral.Widgets.Add(buttonGOff);

            var buttonSpecial = new TextButton
            {
                GridColumn = 0,
                GridRow = 4,
                Text = "Special Objects",
                Width = 180,
                Height = 30,
                GridColumnSpan = 4
            };
            buttonSpecial.Click += (s, a) =>
            {
                if (mode == Mode.Physical)
                {
                    _physicalMapLogic = new PhysicalMapLogic(editor.MapGrid);
                    _specialItemInterface = new SpecialItems(this, _physicalMapLogic);
                }
                else
                {
                    var messageBox = Dialog.CreateMessageBox("Error", "Special items can only be edited in Wall edition mode.");
                    messageBox.ShowModal(_lateralMenuDesktop);
                }
            };
            lateral.Widgets.Add(buttonSpecial);

            var buttonHelp = new TextButton
            {
                GridColumn = 0,
                GridRow = 5,
                Text = "Help",
                Width = 180,
                Height = 30,
                GridColumnSpan = 4
            };
            buttonHelp.Click += (s, a) =>
            {
                var messageBox = Dialog.CreateMessageBox("Commands",
                    "Move: CTRL+LeftClick or Arrows\n" +
                    "Zoom: CTRL+Wheel or CTRL+[+/-]\n" +
                    "Next / Previous palette: Wheel\n" +
                    "Make a selection: ALT+LeftClick\n" +
                    "Brush size: ALT+Wheel or ALT+[+/-]\n" +
                    "Flip selection: SHIFT+Wheel or SHIFT+Arrows\n" +
                    "Open/Close inventory: Space\n" +
                    "Undo / Redo: CTRL+Z / CTRL+Y"
                    );
                messageBox.ShowModal(_lateralMenuDesktop);
            };
            lateral.Widgets.Add(buttonHelp);

            var buttonSBR = new TextButton
            {
                GridColumn = 0,
                GridRow = 17,
                Text = "Save, build and run",
                Width = 180,
                Height = 30,
                GridColumnSpan = 4
            };
            buttonSBR.Click += (s, a) =>
            {
                SaveGrid();
                Task.Factory.StartNew(() => {
                    try
                    {
                        Mode mode_bkp = mode;
                        mode = Mode.Loading;
                        Settings.RunExtractor("");
                        mode = mode_bkp;
                        Settings.RunEmulator();
                    }
                    catch { Exit(); }
                });
            };
            lateral.Widgets.Add(buttonSBR);

            panel.Widgets.Add(lateral);
            _lateralMenuDesktop = new Desktop();
            _lateralMenuDesktop.Root = panel;

            mode = Mode.Menu;
        }

        public void CloseSpecialItemMenu()
        {
            editor.AddToUndoHistory();
            _physicalMapLogic.OverrideGridData(editor.MapGrid);
            _physicalMapLogic = null;
            _specialItemInterface = null;
        }

        int _lastBonusId = -1;
        public void SetBonusLocation(int id, Point? currentLocation)
        {
            CloseSpecialItemMenu();
            _lastBonusId = id;
            editor.SpecialItemMode(Color.Red, currentLocation);
        }

        public void ChangeSpecialItemLocation(Point? newLocation)
        {
            // Note: Can only be a bonus
            _physicalMapLogic = new PhysicalMapLogic(editor.MapGrid);
            if (newLocation.HasValue)
                _physicalMapLogic.Bonus = new PhysicalMapLogic.BonusInfo(_lastBonusId, newLocation.Value.X, newLocation.Value.Y);
            else
                _physicalMapLogic.Bonus = null;

            editor.AddToUndoHistory();
            _physicalMapLogic.OverrideGridData(editor.MapGrid);
            _physicalMapLogic = null;
        }

        void SaveGrid()
        {
            string[] lines = Levels.GetLinesFromGrid(editor.MapGrid, mode != Mode.Minimap ? 4 : 1, mode != Mode.Minimap,
                Levels.TilesOffset(Levels.GetWorldOfLevel(map), MapType()));
            File.WriteAllLines(Levels.GetLevelPath(map, MapType()), lines);
        }

        Levels.MapType MapType(Mode? mode = null)
        {
            if (!mode.HasValue)
                mode = this.mode;
            if (mode == Mode.Physical)
                return Levels.MapType.Physical;
            if (mode == Mode.Graphical)
                return Levels.MapType.Graphical;
            if (mode == Mode.Background)
                return Levels.MapType.Background;
            if (mode == Mode.Minimap)
                return Levels.MapType.Minimap;
            throw new System.Exception();
        }

        EditorGrid editor;
        TilesSet sset;
        void LoadGrid()
        {
            int[,] grid;
            List<EditorGrid.OverlayGrid> underlays = new List<EditorGrid.OverlayGrid>();
            List<EditorGrid.OverlayGrid> overlays = new List<EditorGrid.OverlayGrid>();
            string world = Levels.GetWorldOfLevel(map);
            if (mode == Mode.Minimap)
            {
                grid = Levels.GetGridFromLines(File.ReadAllLines(Levels.GetLevelPath(map, Levels.MapType.Minimap)), 64, 64,
                    Levels.TilesOffset(world, Levels.MapType.Minimap));
                sset = new TilesSet(Load.MinimapColors, false,
                    new Rectangle(0, PALETTE_SELECTOR_Y, LATERAL_PANEL_WIDTH, GraphicsDevice.Viewport.Height - PALETTE_SELECTOR_Y), 64);
            }
            else
            {
                grid = Levels.GetGridFromLines(File.ReadAllLines(Levels.GetLevelPath(map, MapType())), Levels.TilesOffset(world, MapType()));
                sset = new TilesSet(Load.Tiles[new Load.WorldAndType(world, MapType())],
                    true, new Rectangle(0, PALETTE_SELECTOR_Y, LATERAL_PANEL_WIDTH, GraphicsDevice.Viewport.Height - PALETTE_SELECTOR_Y), 64);

                int[,] ogrid;
                TilesSet osset;
                if (mode != Mode.Background)
                {
                    ogrid = Levels.GetGridFromLines(File.ReadAllLines(Levels.GetLevelPath(map, Levels.MapType.Background)),
                        Levels.TilesOffset(world, Levels.MapType.Background));
                    osset = new TilesSet(Load.Tiles[new Load.WorldAndType(world, Levels.MapType.Background)],
                        true, Rectangle.Empty, 0);
                    underlays.Add(new EditorGrid.OverlayGrid(osset, ogrid, false));
                }
                if (mode != Mode.Graphical)
                {
                    ogrid = Levels.GetGridFromLines(File.ReadAllLines(Levels.GetLevelPath(map, Levels.MapType.Graphical)),
                        Levels.TilesOffset(world, Levels.MapType.Graphical));
                    osset = new TilesSet(Load.Tiles[new Load.WorldAndType(world, Levels.MapType.Graphical)],
                        true, Rectangle.Empty, 0);
                    if (mode == Mode.Physical)
                        underlays.Add(new EditorGrid.OverlayGrid(osset, ogrid, false));
                    else
                        overlays.Add(new EditorGrid.OverlayGrid(osset, ogrid, false));
                }
                if (mode != Mode.Physical)
                {
                    ogrid = Levels.GetGridFromLines(File.ReadAllLines(Levels.GetLevelPath(map, Levels.MapType.Physical)),
                        Levels.TilesOffset(world, Levels.MapType.Physical));
                    osset = new TilesSet(Load.Tiles[new Load.WorldAndType(world, Levels.MapType.Physical)],
                        true, Rectangle.Empty, 0);
                    overlays.Add(new EditorGrid.OverlayGrid(osset, ogrid, true));
                }
            }

            editor = new EditorGrid(this, MapType(),
                new Rectangle(LATERAL_PANEL_WIDTH, 0, GraphicsDevice.Viewport.Width - LATERAL_PANEL_WIDTH, GraphicsDevice.Viewport.Height),
                sset, grid, new Point(-8, -8), overlays.ToArray(), underlays.ToArray());
        }

        bool reloadAtNextFrame = false;
        protected override void Update(GameTime gameTime)
        {
            /*if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();*/
            if (reloadAtNextFrame)
            {
                reloadAtNextFrame = false;
                Levels.Init();
                Load.LoadSpriteContent(GraphicsDevice);
                LoadInterface();
            }
            else if (mode != Mode.Loading && mode != Mode.Menu && !_lateralMenuDesktop.HasModalWidget && _specialItemInterface == null)
            {
                MouseState ms = Mouse.GetState();
                KeyboardState ks = Keyboard.GetState();
                List<Controller.Action> actions = Controller.GetActionsGrid(ks, ms, gameTime);
                foreach (Controller.Action action in actions)
                {
                    switch (action)
                    {
                        case Controller.Action.SELECT_PREVIOUS:
                            sset.SelectPrevious();
                            break;
                        case Controller.Action.SELECT_NEXT:
                            sset.SelectNext();
                            break;
                        default:
                            editor.PerformAction(gameTime, action);
                            break;
                    }
                    
                }
                editor.Update(gameTime, ms, ks);
                sset.Update(ms);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (mode == Mode.Loading) {
                Point center = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                Point location = new Point(center.X - Load.LoadingScreen.Width/2, center.Y - Load.LoadingScreen.Height / 2);
                _spriteBatch.Begin();
                _spriteBatch.Draw(Load.LoadingScreen, new Rectangle(location, Load.LoadingScreen.Bounds.Size), Color.White);
                _spriteBatch.End();
            }
            else if (mode == Mode.Menu)
                _mainMenuDesktop.Render();
            else if (_specialItemInterface != null)
                _specialItemInterface.Render();
            else
            {
                _spriteBatch.Begin();
                editor.Draw(_spriteBatch, gameTime, Mouse.GetState(), Keyboard.GetState());
                _spriteBatch.FillRectangle(new Rectangle(0, 0, LATERAL_PANEL_WIDTH, GraphicsDevice.Viewport.Height), Color.Black);
                sset.DrawSets(_spriteBatch);
                _spriteBatch.End();
                _lateralMenuDesktop.Render();
            }

            base.Draw(gameTime);
        }
    }
}
