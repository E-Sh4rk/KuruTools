using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Myra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;

namespace KuruLevelEditor
{
    class GridEditor
    {
        enum InventoryMode
        {
            No, Base, Custom
        }
        public class OverlayGrid
        {
            public OverlayGrid(TilesSet ts, int[,] grid, bool enabled)
            {
                this.ts = ts;
                this.grid = grid;
                this.enabled = enabled;
            }
            public readonly TilesSet ts;
            public readonly int[,] grid;
            public bool enabled;
        }

        const int MIN_LENGTH_UNIT = 0x40;
        readonly Color BACKGROUND_COLOR = Color.CornflowerBlue;

        Game1 game;

        Rectangle bounds;
        TilesSet sprites;
        EditableGrid map;
        int brush_size = 1;
        Levels.MapType type;
        int[,] selectionGrid;
        InventoryMode inventoryMode = InventoryMode.No;
        EditableGrid inventory;
        TimeSpan showBrushUntil = TimeSpan.Zero;
        EditableGrid customInventory;

        Color? specialItemMode = null;
        Point? specialItemLocation = null;

        public OverlayGrid[] Overlays { get; private set; }
        public OverlayGrid[] Underlays { get; private set; }
        public bool GridEnabled { get; set; }
        public GridEditor(Game1 game, Levels.MapType type, Rectangle bounds, TilesSet sprites, int[,] grid,
            Point position, OverlayGrid[] overlays, OverlayGrid[] underlays, int[,] selectionGrid, EditableGrid customInventory)
        {
            this.game = game;
            this.bounds = bounds;
            this.sprites = sprites;
            map = new EditableGrid(bounds, grid, position, 8);
            this.type = type;
            this.selectionGrid = selectionGrid;
            this.customInventory = customInventory;
            GridEnabled = true;
            // Load Inventory
            if (type != Levels.MapType.Minimap)
            {
                Point nbTiles = sprites.NumberTiles;
                int[,] inventoryGrid = new int[nbTiles.Y, nbTiles.X];
                int i = 0;
                for (int y = 0; y < nbTiles.Y; y++)
                {
                    for (int x = 0; x < nbTiles.X; x++)
                    {
                        inventoryGrid[y, x] = i;
                        i++;
                    }
                }
                inventory = new EditableGrid(bounds, inventoryGrid, new Point(-8, -8), 16);
            }
            else
                inventory = null;
            Overlays = overlays;
            Underlays = underlays;
        }

        EditableGrid CurrentEditableGrid
        {
            get
            {
                switch (inventoryMode)
                {
                    case InventoryMode.No: return map;
                    case InventoryMode.Base: return inventory;
                    case InventoryMode.Custom: return customInventory;
                }
                return null;
            }
        }
        int[,] Grid
        {
            get { return CurrentEditableGrid.Grid; }
            set { CurrentEditableGrid.Grid = value; }
        }
        Point Position
        {
            get { return CurrentEditableGrid.Position; }
            set { CurrentEditableGrid.Position = value; }
        }
        int TileSize
        {
            get { return CurrentEditableGrid.TileSize; }
            set { CurrentEditableGrid.TileSize = value;  }
        }

        public void SpecialItemMode(Color c, Point? initialLocation)
        {
            specialItemMode = c;
            specialItemLocation = initialLocation;
        }
        void QuitSpecialItemMode() {
            specialItemMode = null;
            specialItemLocation = null;
        }

        public Point TileCoordToScreenCoord(int x, int y)
        {
            return new Point(x * TileSize, y * TileSize) - Position + bounds.Location;
        }

        public Rectangle TileCoordToScreenRect(int x, int y)
        {
            return new Rectangle(TileCoordToScreenCoord(x, y), new Point(TileSize, TileSize));
        }

        public Point ScreenCoordToTileCoord(int x, int y)
        {
            Point p = new Point(x, y) + Position - bounds.Location;
            int pxo = p.X < 0 ? -TileSize + 1 : 0;
            int pyo = p.Y < 0 ? -TileSize + 1 : 0;
            return new Point((p.X + pxo) / TileSize, (p.Y + pyo) / TileSize);
        }

        public Rectangle ScreenCoordToTileRect(int x, int y)
        {
            return new Rectangle(ScreenCoordToTileCoord(x, y), new Point(TileSize, TileSize));
        }

        public int BrushSize
        {
            get { return brush_size; }
            set { brush_size = value; }
        }
        public int[,] MapGrid
        {
            get { return map.Grid; }
        }
        public int[,] SelectionGrid
        {
            get { return selectionGrid; }
        }

        public void AddToUndoHistory(int[,] g = null)
        {
            if (inventoryMode == InventoryMode.Base) return;
            CurrentEditableGrid.AddToUndoHistory(g);
        }
        public void IncreaseWidth()
        {
            if (type == Levels.MapType.Minimap && inventoryMode != InventoryMode.Custom) return;
            if (inventoryMode == InventoryMode.Base) return;
            int w = Grid.GetLength(1);
            if (w < 0x200)
            {
                AddToUndoHistory();
                w += MIN_LENGTH_UNIT;
                Grid = Utils.ResizeArray(Grid, Grid.GetLength(0), w);
            }
        }
        public void DecreaseWidth()
        {
            if (type == Levels.MapType.Minimap && inventoryMode != InventoryMode.Custom) return;
            if (inventoryMode == InventoryMode.Base) return;
            int w = Grid.GetLength(1);
            if (w > 0x40)
            {
                AddToUndoHistory();
                w -= MIN_LENGTH_UNIT;
                Grid = Utils.ResizeArray(Grid, Grid.GetLength(0), w);
            }
        }
        public void IncreaseHeight()
        {
            if (type == Levels.MapType.Minimap && inventoryMode != InventoryMode.Custom) return;
            if (inventoryMode == InventoryMode.Base) return;
            int h = Grid.GetLength(0);
            if (h < 0x200)
            {
                AddToUndoHistory();
                h += MIN_LENGTH_UNIT;
                Grid = Utils.ResizeArray(Grid, h, Grid.GetLength(1));
            }
        }
        public void DecreaseHeight()
        {
            if (type == Levels.MapType.Minimap && inventoryMode != InventoryMode.Custom) return;
            if (inventoryMode == InventoryMode.Base) return;
            int h = Grid.GetLength(0);
            if (h > 0x40)
            {
                AddToUndoHistory();
                h -= MIN_LENGTH_UNIT;
                Grid = Utils.ResizeArray(Grid, h, Grid.GetLength(1));
            }
        }

        List<Rectangle> RectanglesAround(Rectangle r, int sizeX, int sizeY)
        {
            List<Rectangle> res = new List<Rectangle>();
            sizeX--;
            sizeY--;
            for (int i = -sizeX; i <= sizeX; i++)
            {
                for (int j = -sizeY; j <= sizeY; j++)
                    res.Add(new Rectangle(r.Location + new Point(i*r.Width,j*r.Height), r.Size));
            }
            return res;
        }
        List<Point> PointsAround(Point pt, int sizeX, int sizeY)
        {
            List<Point> res = new List<Point>();
            sizeX--;
            sizeY--;
            for (int i = -sizeX; i <= sizeX; i++)
            {
                for (int j = -sizeY; j <= sizeY; j++)
                    res.Add(pt + new Point(i,j));
            }
            return res;
        }

        void ShowBrush(GameTime gt)
        {
            showBrushUntil = gt.TotalGameTime.Add(new TimeSpan(0, 0, 1));
        }

        public void PerformAction(GameTime gt, Controller.Action action)
        {
            if (mouse_move_is_selecting)
                return;
            int amount = 16;
            switch (action)
            {
                case Controller.Action.BOTTOM:
                    Position += new Point(0, amount);
                    break;
                case Controller.Action.TOP:
                    Position += new Point(0, -amount);
                    break;
                case Controller.Action.RIGHT:
                    Position += new Point(amount, 0);
                    break;
                case Controller.Action.LEFT:
                    Position += new Point(-amount, 0);
                    break;
                case Controller.Action.ZOOM_IN:
                    if (TileSize < 32)
                        TileSize++;
                    break;
                case Controller.Action.ZOOM_OUT:
                    if (TileSize > 3)
                        TileSize--;
                    break;
                case Controller.Action.BRUSH_PLUS:
                    ShowBrush(gt);
                    if (brush_size < 0x20)
                        BrushSize++;
                    break;
                case Controller.Action.BRUSH_MINUS:
                    ShowBrush(gt);
                    if (brush_size > 1)
                        BrushSize--;
                    break;
                case Controller.Action.FLIP_VERTICAL:
                case Controller.Action.FLIP_HORIZONTAL:
                    ShowBrush(gt);
                    bool vertical = action == Controller.Action.FLIP_VERTICAL;
                    if (selectionGrid != null)
                    {
                        for (int i = 0; i < selectionGrid.GetLength(0); i++)
                        {
                            for (int j = 0; j < selectionGrid.GetLength(1); j++)
                            {
                                if (vertical)
                                    selectionGrid[i, j] = FlipVertically(selectionGrid[i, j]);
                                else
                                    selectionGrid[i, j] = FlipHorizontally(selectionGrid[i, j]);
                            }
                        }
                        if (vertical)
                            selectionGrid = Utils.FlipVertically(selectionGrid);
                        else
                            selectionGrid = Utils.FlipHorizontally(selectionGrid);
                    }
                    break;
                case Controller.Action.TOGGLE_INVENTORY:
                    if (type != Levels.MapType.Minimap)
                        inventoryMode = inventoryMode == InventoryMode.Base ? InventoryMode.No : InventoryMode.Base;
                    break;
                case Controller.Action.TOGGLE_CUSTOM_INVENTORY:
                    inventoryMode = inventoryMode == InventoryMode.Custom ? InventoryMode.No : InventoryMode.Custom;
                    break;
                case Controller.Action.UNDO:
                    if (inventoryMode != InventoryMode.Base)
                        CurrentEditableGrid.Undo();
                    break;
                case Controller.Action.REDO:
                    if (inventoryMode != InventoryMode.Base)
                        CurrentEditableGrid.Redo();
                    break;
                case Controller.Action.FORCE_PALETTE:
                    if (selectionGrid != null)
                    {
                        for (int i = 0; i < selectionGrid.GetLength(0); i++)
                        {
                            for (int j = 0; j < selectionGrid.GetLength(1); j++)
                                selectionGrid[i, j] = ChangePalette(selectionGrid[i, j], sprites.SelectedSet);
                        }
                    }
                    break;
            }
        }

        bool mouse_move_is_selecting = false;
        Point? initial_mouse_move_pos = null;
        Point? initial_mouse_move_map_position = null;
        public void Update(GameTime gt, MouseState mouse, KeyboardState keyboard)
        {
            bool disableConstruction = inventoryMode == InventoryMode.Base;
            if (initial_mouse_move_pos != null)
            {
                if (mouse.LeftButton == ButtonState.Released)
                {
                    if (mouse_move_is_selecting)
                    {
                        QuitSpecialItemMode();
                        ShowBrush(gt);
                        Rectangle map_bounds = new Rectangle(0, 0, Grid.GetLength(1), Grid.GetLength(0));
                        Rectangle r = Rectangle.Union(new Rectangle(initial_mouse_move_pos.Value, Point.Zero), new Rectangle(mouse.Position, Point.Zero));
                        Point coord1 = ScreenCoordToTileCoord(r.X, r.Y);
                        Point coord2 = ScreenCoordToTileCoord(r.X + r.Width, r.Y + r.Height);
                        Point size = coord2 - coord1 + new Point(1, 1);
                        selectionGrid = new int[size.Y, size.X];
                        for (int y = 0; y < size.Y; y++)
                        {
                            for (int x = 0; x < size.X; x++)
                            {
                                Point pt = new Point(x + coord1.X, y + coord1.Y);
                                if (map_bounds.Contains(pt))
                                    selectionGrid[y, x] = GetTileCode(Grid[pt.Y, pt.X], inventoryMode == InventoryMode.Base);
                            }
                        }
                    }
                    initial_mouse_move_pos = null;
                    initial_mouse_move_map_position = null;
                    mouse_move_is_selecting = false;
                }
                else if (!mouse_move_is_selecting)
                {
                    Point offset = initial_mouse_move_pos.Value - mouse.Position;
                    Position = initial_mouse_move_map_position.Value + offset;
                }
            }

            if (keyboard.IsKeyDown(Keys.LeftControl))
            {
                if (initial_mouse_move_pos == null && mouse.LeftButton == ButtonState.Pressed)
                {
                    initial_mouse_move_pos = mouse.Position;
                    initial_mouse_move_map_position = Position;
                    mouse_move_is_selecting = false;
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftAlt) || disableConstruction)
            {
                if (initial_mouse_move_pos == null && mouse.LeftButton == ButtonState.Pressed)
                {
                    QuitSpecialItemMode();
                    initial_mouse_move_pos = mouse.Position;
                    initial_mouse_move_map_position = Position;
                    mouse_move_is_selecting = true;
                }
            }
            else
            {
                if (!disableConstruction && initial_mouse_move_pos == null && bounds.Contains(mouse.Position))
                {
                    if (mouse.LeftButton == ButtonState.Pressed || mouse.RightButton == ButtonState.Pressed)
                    {
                        int[,] grid_bkp = null;
                        bool clear = mouse.RightButton == ButtonState.Pressed;
                        Point cpt = ScreenCoordToTileCoord(mouse.Position.X, mouse.Position.Y);
                        Rectangle map_bounds = new Rectangle(0, 0, Grid.GetLength(1), Grid.GetLength(0));
                        if (specialItemMode.HasValue)
                        {
                            if (map_bounds.Contains(cpt))
                            {
                                if (clear && specialItemLocation.HasValue && specialItemLocation.Value.Equals(cpt))
                                {
                                    specialItemLocation = null;
                                    game.ChangeSpecialItemLocation(null);
                                }
                                else if (!clear && (!specialItemLocation.HasValue || !specialItemLocation.Value.Equals(cpt)))
                                {
                                    specialItemLocation = cpt;
                                    game.ChangeSpecialItemLocation(cpt);
                                }
                            }
                        }
                        else if (selectionGrid == null || (selectionGrid.GetLength(0) == 1 && selectionGrid.GetLength(1) == 1))
                        {
                            int selectedItem = 0;
                            if (!clear && selectionGrid != null)
                                selectedItem = selectionGrid[0, 0];
                            foreach (Point pt in PointsAround(cpt, brush_size, brush_size))
                            {
                                if (map_bounds.Contains(pt))
                                {
                                    int nv = GetTileCode(selectedItem, !clear);
                                    if (grid_bkp == null && Grid[pt.Y, pt.X] != nv)
                                        grid_bkp = Utils.CopyArray(Grid);
                                    Grid[pt.Y, pt.X] = nv;
                                }
                            }
                        }
                        else
                        {
                            Point selection_size = new Point(selectionGrid.GetLength(1), selectionGrid.GetLength(0));
                            Point half_size = new Point(selection_size.X / 2, selection_size.Y / 2);
                            foreach (Point offset in PointsAround(Point.Zero, half_size.X + 1, half_size.Y + 1))
                            {
                                Point selection_offset = offset + half_size;
                                if (selection_offset.X >= selection_size.X || selection_offset.Y >= selection_size.Y)
                                    continue;
                                int selectedItem = clear ? 0 : selectionGrid[selection_offset.Y, selection_offset.X];
                                Point pt = cpt + offset;
                                if (map_bounds.Contains(pt))
                                {
                                    int nv = GetTileCode(selectedItem, false);
                                    if (grid_bkp == null && Grid[pt.Y, pt.X] != nv)
                                        grid_bkp = Utils.CopyArray(Grid);
                                    Grid[pt.Y, pt.X] = nv;
                                }
                            }
                        }
                        if (grid_bkp != null)
                            AddToUndoHistory(grid_bkp);
                    }
                }
            }
        }

        int FlipHorizontally(int tile)
        {
            if (type == Levels.MapType.Minimap || tile == 0 /* Avoid generating many flipped zeros */)
                return tile;
            return (tile ^ 0x400);
        }
        int FlipVertically(int tile)
        {
            if (type == Levels.MapType.Minimap || tile == 0 /* Avoid generating many flipped zeros */)
                return tile;
            return (tile ^ 0x800);
        }

        int GetTileCode(int tile, bool overridePalette)
        {
            if (type == Levels.MapType.Minimap)
                return overridePalette ? sprites.SelectedSet : tile;
            else
            {
                int tile_index = tile;
                int tile_id = tile_index & 0x3FF;
                int palette = overridePalette ? sprites.SelectedSet << 12 : tile_index & 0xF000;
                int flips = tile_index & 0xC00;
                return tile_id + flips + palette;
            }
        }
        int ChangePalette(int tile, int palette)
        {
            if (type == Levels.MapType.Minimap)
                return palette;
            return (tile & 0x0FFF) + (palette << 12);
        }
        void DrawTile(SpriteBatch sprite_batch, TilesSet sprites, Rectangle dst, int tile, bool overridePalette, bool showSpecial)
        {
            if (type == Levels.MapType.Minimap)
                sprites.Draw(sprite_batch, overridePalette ? sprites.SelectedSet : tile, 0, dst);
            else
            {
                tile = overridePalette ? ChangePalette(tile, sprites.SelectedSet) : tile;
                int tile_id = tile & 0x3FF;
                int palette = (tile & 0xF000) >> 12;
                SpriteEffects effects =
                    ((tile & 0x400) != 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None) |
                    ((tile & 0x800) != 0 ? SpriteEffects.FlipVertically : SpriteEffects.None);
                if (showSpecial)
                {
                    if (Settings.Paradise)
                    {
                        if (tile_id >= PhysicalMapLogic.CONTROL_MIN_ID)
                        {
                            // Rendering of non-graphic special elements
                            Color? c = null;
                            if (ParadisePhysicalMapLogic.STARTING_ZONE_IDS.Contains(tile_id))
                                c = ParadisePhysicalMapLogic.StartingZoneColor(tile_id);
                            else if (ParadisePhysicalMapLogic.HEALING_ZONE_IDS.Contains(tile_id))
                                c = ParadisePhysicalMapLogic.HealingZoneColor(tile_id);
                            else if (ParadisePhysicalMapLogic.ENDING_ZONE_IDS.Contains(tile_id))
                                c = ParadisePhysicalMapLogic.EndingZoneColor(tile_id);
                            else if (ParadisePhysicalMapLogic.FIXED_OBJECTS_IDS.Contains(tile_id))
                                sprite_batch.Draw(ParadisePhysicalMapLogic.TextureOfFixedObjects(tile_id), dst, null, Color.White, 0, Vector2.Zero, effects, 0F);
                            else if (ParadisePhysicalMapLogic.MOVING_OBJECTS_IDS.Contains(tile_id)) // Flips have no effect on moving objects tiles
                                sprite_batch.Draw(ParadisePhysicalMapLogic.TextureOfMovingObject(tile_id), dst, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0F);
                            else if (ParadisePhysicalMapLogic.NUMBER_TILES.Contains(tile)) // Palette and flips matters for numbers
                                sprite_batch.Draw(ParadisePhysicalMapLogic.TextureOfNumber(tile), dst, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0F);
                            else
                                c = ParadisePhysicalMapLogic.UNSUPPORTED_COLOR;
                            if (c.HasValue)
                                sprite_batch.FillRectangle(dst, c.Value);
                        }
                        if (tile_id <= PhysicalMapLogic.VISIBLE_MAX_ID)
                            sprites.Draw(sprite_batch, palette, tile_id, dst, effects);
                    }
                    else
                    {
                        if (tile_id >= PhysicalMapLogic.CONTROL_MIN_ID)
                        {
                            // Rendering of non-graphic special elements
                            Color? c = null;
                            if (PhysicalMapLogic.STARTING_ZONE_IDS.Contains(tile_id))
                                c = PhysicalMapLogic.StartingZoneColor(tile_id);
                            else if (PhysicalMapLogic.HEALING_ZONE_IDS.Contains(tile_id))
                                c = PhysicalMapLogic.HealingZoneColor(tile_id);
                            else if (PhysicalMapLogic.ENDING_ZONE_IDS.Contains(tile_id))
                                c = PhysicalMapLogic.EndingZoneColor(tile_id);
                            else if (PhysicalMapLogic.SPRING_IDS.Contains(tile_id))
                                sprite_batch.Draw(PhysicalMapLogic.TextureOfSpring(tile_id), dst, null, Color.White, 0, Vector2.Zero, effects, 0F);
                            else if (PhysicalMapLogic.VISIBLE_CONTROL_TILES.Contains(tile_id))
                                sprite_batch.Draw(PhysicalMapLogic.UnderlayOfVisibleControlTile(tile_id), dst, null, Color.White, 0, Vector2.Zero, effects, 0F);
                            else if (PhysicalMapLogic.MOVING_OBJECTS_IDS.Contains(tile_id)) // Flips have no effect on moving objects tiles
                                sprite_batch.Draw(PhysicalMapLogic.TextureOfMovingObject(tile_id), dst, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0F);
                            else if (PhysicalMapLogic.NUMBER_TILES.Contains(tile)) // Palette and flips matters for numbers
                                sprite_batch.Draw(PhysicalMapLogic.TextureOfNumber(tile), dst, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0F);
                            else
                                c = PhysicalMapLogic.UNSUPPORTED_COLOR;
                            if (c.HasValue)
                                sprite_batch.FillRectangle(dst, c.Value);
                        }
                        if (tile_id <= PhysicalMapLogic.VISIBLE_MAX_ID || PhysicalMapLogic.VISIBLE_CONTROL_TILES.Contains(tile_id))
                            sprites.Draw(sprite_batch, palette, tile_id, dst, effects);
                    }
                }
                else
                    sprites.Draw(sprite_batch, palette, tile_id, dst, effects);
            }
        }
        void DrawGrid(SpriteBatch sprite_batch, int[,] grid, TilesSet sprites, bool overridePalette, bool showSpecial, Rectangle? ignoreArea = null)
        {
            Rectangle notIn = ignoreArea.HasValue ? ignoreArea.Value : Rectangle.Empty;
            int h = grid.GetLength(0);
            int w = grid.GetLength(1);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Rectangle dst = TileCoordToScreenRect(x, y);
                    if (dst.Intersects(bounds) && !dst.Intersects(notIn))
                        DrawTile(sprite_batch, sprites, dst, grid[y, x], overridePalette, showSpecial);
                }
            }
        }
        struct DelayedSpriteDrawing
        {
            public DelayedSpriteDrawing(Rectangle r, int item, bool overridePalette)
            {
                this.r = r;
                this.item = item;
                this.overridePalette = overridePalette;
                color = null;
            }
            public DelayedSpriteDrawing(Rectangle r, Color color)
            {
                this.r = r;
                this.color = color;
                item = null;
                overridePalette = false;
            }
            public Rectangle r;
            public int? item;
            public bool overridePalette;
            public Color? color;
        }
        public void Draw(SpriteBatch sprite_batch, GameTime gt, MouseState mouse, KeyboardState keyboard)
        {
            bool disableConstruction = inventoryMode == InventoryMode.Base;
            bool inventoryOpened = inventoryMode != InventoryMode.No;
            bool showSpecial = type == Levels.MapType.Physical;
            sprite_batch.FillRectangle(bounds, BACKGROUND_COLOR);
            // Compute selected elements sprites
            Rectangle? selectedElementsBounds = null;
            List<DelayedSpriteDrawing> toDraw = new List<DelayedSpriteDrawing>();
            if (!mouse_move_is_selecting)
            {
                if ((!keyboard.IsKeyDown(Keys.LeftControl) && !keyboard.IsKeyDown(Keys.LeftAlt) && !disableConstruction) || gt.TotalGameTime <= showBrushUntil)
                {
                    Point pt = ScreenCoordToTileCoord(mouse.Position.X, mouse.Position.Y);
                    Rectangle cr = TileCoordToScreenRect(pt.X, pt.Y);
                    selectedElementsBounds = cr;
                    if (specialItemMode.HasValue)
                    {
                        if (cr.Intersects(bounds))
                            toDraw.Add(new DelayedSpriteDrawing(cr, specialItemMode.Value));
                        if (specialItemLocation.HasValue)
                        {
                            Rectangle sil = TileCoordToScreenRect(specialItemLocation.Value.X, specialItemLocation.Value.Y);
                            if (sil.Intersects(bounds))
                                toDraw.Add(new DelayedSpriteDrawing(sil, specialItemMode.Value));
                        }
                    }
                    else if (selectionGrid == null || (selectionGrid.GetLength(0) == 1 && selectionGrid.GetLength(1) == 1))
                    {
                        int selectedItem = 0;
                        if (selectionGrid != null)
                            selectedItem = selectionGrid[0, 0];

                        foreach (Rectangle r in RectanglesAround(cr, brush_size, brush_size))
                        {
                            selectedElementsBounds = Rectangle.Union(selectedElementsBounds.Value, r);
                            if (r.Intersects(bounds))
                                toDraw.Add(new DelayedSpriteDrawing(r, selectedItem, true));
                        }
                    }
                    else
                    {
                        Point selection_size = new Point(selectionGrid.GetLength(1), selectionGrid.GetLength(0));
                        Point half_size = new Point(selection_size.X / 2, selection_size.Y / 2);
                        foreach (Point offset in PointsAround(Point.Zero, half_size.X + 1, half_size.Y + 1))
                        {
                            Point selection_offset = offset + half_size;
                            if (selection_offset.X >= selection_size.X || selection_offset.Y >= selection_size.Y)
                                continue;
                            int selectedItem = selectionGrid[selection_offset.Y, selection_offset.X];
                            Rectangle r = new Rectangle(cr.Location + new Point(offset.X * cr.Width, offset.Y * cr.Height), cr.Size);
                            selectedElementsBounds = Rectangle.Union(selectedElementsBounds.Value, r);
                            if (r.Intersects(bounds))
                                toDraw.Add(new DelayedSpriteDrawing(r, selectedItem, false));
                        }
                    }
                }
            }
            // Draw map, selected elements and overlays
            if (!inventoryOpened)
            {
                foreach (OverlayGrid underlay in Underlays)
                {
                    if (underlay.enabled)
                        DrawGrid(sprite_batch, underlay.grid, underlay.ts, false, false);
                }
            }
            DrawGrid(sprite_batch, Grid, sprites, inventoryMode == InventoryMode.Base, showSpecial, selectedElementsBounds);
            if (selectedElementsBounds.HasValue)
            {
                foreach (DelayedSpriteDrawing d in toDraw)
                {
                    if (d.color.HasValue)
                        sprite_batch.FillRectangle(d.r, d.color.Value);
                    if (d.item.HasValue)
                        DrawTile(sprite_batch, sprites, d.r, d.item.Value, d.overridePalette, showSpecial);
                }
            }
            if (!inventoryOpened)
            {
                foreach (OverlayGrid overlay in Overlays)
                {
                    if (overlay.enabled)
                        DrawGrid(sprite_batch, overlay.grid, overlay.ts, false, false);
                }
            }
            // Draw map bounds and grid
            Point tlCoord = TileCoordToScreenCoord(0, 0);
            Rectangle map_bounds = new Rectangle(tlCoord, TileCoordToScreenCoord(Grid.GetLength(1), Grid.GetLength(0)) - tlCoord);
            if (TileSize > 8 && GridEnabled)
            {
                for (int x = map_bounds.X + TileSize; x < map_bounds.X + map_bounds.Width; x += TileSize)
                    sprite_batch.FillRectangle(new Rectangle(x, map_bounds.Y, 1, map_bounds.Height), Color.Gray);
                for (int y = map_bounds.Y + TileSize; y < map_bounds.Y + map_bounds.Height; y += TileSize)
                    sprite_batch.FillRectangle(new Rectangle(map_bounds.X, y, map_bounds.Width, 1), Color.Gray);
            }
            if (showSpecial && !inventoryOpened && !Settings.Paradise)
                sprite_batch.FillRectangle(new Rectangle(map_bounds.X, map_bounds.Y + TileSize * PhysicalMapLogic.NUMBER_RESERVED_ROWS, map_bounds.Width, 1), Color.Orange);
            TilesSet.DrawRectangle(sprite_batch, map_bounds, Color.Red, 2); // Issue with DrawRectangle: https://github.com/rds1983/Myra/issues/211
            // Draw selection rectangle
            if (mouse_move_is_selecting)
            {
                Rectangle r = Rectangle.Union(new Rectangle(initial_mouse_move_pos.Value, Point.Zero), new Rectangle(mouse.Position, Point.Zero));
                TilesSet.DrawRectangle(sprite_batch, r, Color.White, 1);
            }
            else if (selectedElementsBounds.HasValue)
                TilesSet.DrawRectangle(sprite_batch, selectedElementsBounds.Value, Color.White, 1);
        }
    }
}
