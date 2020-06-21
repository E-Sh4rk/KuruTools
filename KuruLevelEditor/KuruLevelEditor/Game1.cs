using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace KuruLevelEditor
{
    public class Game1 : Game
    {
        const int LATERAL_PANEL_WIDTH = 200;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Desktop _mainMenuDesktop;
        private Desktop _lateralMenuDesktop;

        enum Mode
        {
            Menu,
            Minimap
        }
        private Mode mode = Mode.Menu;
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
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            MyraEnvironment.Game = this;

            Levels.Init();
            Load.LoadContent(Content, GraphicsDevice);

            // ===== MAIN MENU =====
            var grid = new Grid
            {
                RowSpacing = 8,
                ColumnSpacing = 8
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
            };
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
            };
            foreach (string name in Levels.AllLevels)
                comboMap.Items.Add(new ListItem(name, Color.White));
            grid.Widgets.Add(comboMap);

            var buttonEdit = new TextButton
            {
                GridColumn = 0,
                GridColumnSpan = 2,
                GridRow = 2,
                Text = "Edit",
                Width = 50,
                Height = 50
            };

            buttonEdit.Click += (s, a) =>
            {
                if (comboMap.SelectedIndex == null || comboType.SelectedIndex == null)
                {
                    var messageBox = Dialog.CreateMessageBox("Error", "Please select an element and a map.");
                    messageBox.ShowModal(_mainMenuDesktop);
                }
                else
                {
                    mode = Mode.Minimap;
                    map = comboMap.SelectedItem.Text;
                    LoadGrid();
                }
            };

            grid.Widgets.Add(buttonEdit);

            _mainMenuDesktop = new Desktop();
            _mainMenuDesktop.Root = grid;

            // ===== LATERAL MENU =====
            Panel panel = new Panel()
            {
                Left = 0,
                Width = LATERAL_PANEL_WIDTH,
                Background = new SolidBrush(Color.Black),
            };

            var lateral = new Grid
            {
                RowSpacing = 8,
                ColumnSpacing = 8
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
                Width = 50,
                Height = 50
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
                Text = "Save and quit",
                Width = 75,
                Height = 50
            };
            buttonSaveQuit.Click += (s, a) =>
            {
                SaveGrid();
                mode = Mode.Menu;
            };
            lateral.Widgets.Add(buttonSaveQuit);

            var buttonQuit = new TextButton
            {
                GridColumn = 2,
                GridRow = 0,
                Text = "Quit",
                Width = 50,
                Height = 50
            };
            buttonQuit.Click += (s, a) =>
            {
                mode = Mode.Menu;
            };
            lateral.Widgets.Add(buttonQuit);

            panel.Widgets.Add(lateral);
            _lateralMenuDesktop = new Desktop();
            _lateralMenuDesktop.Root = panel;
            // TODO: Display shortcuts
        }

        void SaveGrid()
        {
            string[] lines = Levels.GetLinesFromGrid(editor.Grid, 1);
            File.WriteAllLines(Levels.GetLevelPath(map, Levels.MapType.Minimap), lines);
        }

        Levels.MapType MapType()
        {
            if (mode == Mode.Minimap)
                return Levels.MapType.Minimap;
            throw new System.Exception();
        }

        EditorGrid editor;
        TilesSet sset;
        void LoadGrid()
        {
            int[,] grid = Levels.GetGridFromLines(File.ReadAllLines(Levels.GetLevelPath(map, Levels.MapType.Minimap)), 64, 64);
            sset = new TilesSet(Load.MinimapColors, false, new Rectangle(0, 200, LATERAL_PANEL_WIDTH, GraphicsDevice.Viewport.Height - 200), 32);
            editor = new EditorGrid(MapType(),
                new Rectangle(GraphicsDevice.Viewport.X + LATERAL_PANEL_WIDTH,
                GraphicsDevice.Viewport.Y, GraphicsDevice.Viewport.Width - LATERAL_PANEL_WIDTH, GraphicsDevice.Viewport.Height),
                sset, grid, new Point(-8, -8));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (mode != Mode.Menu)
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
                            editor.PerformAction(action);
                            break;
                    }
                    
                }
                editor.Update(ms, ks);
                sset.Update(ms);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (mode == Mode.Menu)
                _mainMenuDesktop.Render();
            else
            {
                _spriteBatch.Begin();
                editor.Draw(_spriteBatch, Mouse.GetState());
                _spriteBatch.End();
                _lateralMenuDesktop.Render();
                _spriteBatch.Begin();
                sset.DrawSets(_spriteBatch);
                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
