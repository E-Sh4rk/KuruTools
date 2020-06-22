using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;

namespace KuruLevelEditor
{
    class EditorGrid
    {
        const int MIN_LENGTH_UNIT = 0x40;
        int tile_size = 8;
        Rectangle bounds;
        TilesSet sprites;
        int[,] grid;
        Point position;
        int brush_size = 1;
        Levels.MapType type;
        int[,] selectionGrid = null;
        bool inventoryMode = false;
        int[,] inventory;
        Point inventoryPosition = new Point(-8, -8);
        int inventoryTileSize = 16;
        TimeSpan showBrushUntil = TimeSpan.Zero;
        public EditorGrid(Levels.MapType type, Rectangle bounds, TilesSet sprites, int[,] grid, Point position)
        {
            this.bounds = bounds;
            this.sprites = sprites;
            this.grid = grid;
            this.position = position;
            this.type = type;
            // Load Inventory
            if (type != Levels.MapType.Minimap)
            {
                Point nbTiles = sprites.NumberTiles;
                inventory = new int[nbTiles.Y, nbTiles.X];
                int i = 0;
                for (int y = 0; y < nbTiles.Y; y++)
                {
                    for (int x = 0; x < nbTiles.X; x++)
                    {
                        inventory[y, x] = i;
                        i++;
                    }
                }
            }
        }

        int[,] Grid
        {
            get { return inventoryMode ? inventory : grid; }
            set {
                if (!inventoryMode)
                    grid = value;
            }
        }
        Point Position
        {
            get { return inventoryMode ? inventoryPosition : position; }
            set
            {
                if (!inventoryMode)
                    position = value;
                else
                    inventoryPosition = value;

            }
        }
        int TileSize
        {
            get { return inventoryMode ? inventoryTileSize : tile_size; }
            set
            {
                Rectangle view = new Rectangle(Position, new Point(bounds.Size.X, bounds.Size.Y));
                Point center = view.Center;
                Position = new Point(center.X * value / TileSize - bounds.Size.X / 2, center.Y * value / TileSize - bounds.Size.Y / 2);
                if (!inventoryMode)
                    tile_size = value;
                else
                    inventoryTileSize = value;

            }
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
            return new Point(p.X / TileSize, p.Y / TileSize);
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
            get { return grid; }
        }

        public void IncreaseWidth()
        {
            if (type == Levels.MapType.Minimap) return;
            int w = grid.GetLength(1);
            if (w < 0x200)
                w += MIN_LENGTH_UNIT;
            grid = Utils.ResizeArray(grid, grid.GetLength(0), w);
        }
        public void DecreaseWidth()
        {
            if (type == Levels.MapType.Minimap) return;
            int w = grid.GetLength(1);
            if (w > 0x40)
                w -= MIN_LENGTH_UNIT;
            grid = Utils.ResizeArray(grid, grid.GetLength(0), w);
        }
        public void IncreaseHeight()
        {
            if (type == Levels.MapType.Minimap) return;
            int h = grid.GetLength(0);
            if (h < 0x200)
                h += MIN_LENGTH_UNIT;
            grid = Utils.ResizeArray(grid, h, grid.GetLength(1));
        }
        public void DecreaseHeight()
        {
            if (type == Levels.MapType.Minimap) return;
            int h = grid.GetLength(0);
            if (h > 0x40)
                h -= MIN_LENGTH_UNIT;
            grid = Utils.ResizeArray(grid, h, grid.GetLength(1));
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
                    if (TileSize > 1)
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
                        inventoryMode = !inventoryMode;
                    break;
            }
        }

        bool mouse_move_is_selecting = false;
        Point? initial_mouse_move_pos = null;
        Point? initial_mouse_move_map_position = null;
        public void Update(MouseState mouse, KeyboardState keyboard)
        {
            if (initial_mouse_move_pos != null)
            {
                if (mouse.LeftButton == ButtonState.Released)
                {
                    if (mouse_move_is_selecting)
                    {
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
                                    selectionGrid[y, x] = GetTileCode(Grid[pt.Y, pt.X], inventoryMode);
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
            else if (keyboard.IsKeyDown(Keys.LeftAlt) || inventoryMode)
            {
                if (initial_mouse_move_pos == null && mouse.LeftButton == ButtonState.Pressed)
                {
                    initial_mouse_move_pos = mouse.Position;
                    initial_mouse_move_map_position = Position;
                    mouse_move_is_selecting = true;
                }
            }
            else
            {
                if (!inventoryMode && initial_mouse_move_pos == null && bounds.Contains(mouse.Position))
                {
                    if (mouse.LeftButton == ButtonState.Pressed || mouse.RightButton == ButtonState.Pressed)
                    {
                        bool clear = mouse.RightButton == ButtonState.Pressed;
                        Point cpt = ScreenCoordToTileCoord(mouse.Position.X, mouse.Position.Y);
                        Rectangle map_bounds = new Rectangle(0, 0, grid.GetLength(1), grid.GetLength(0));
                        if (selectionGrid == null || (selectionGrid.GetLength(0) == 1 && selectionGrid.GetLength(1) == 1))
                        {
                            int selectedItem = 0;
                            if (!clear && selectionGrid != null)
                                selectedItem = selectionGrid[0, 0];
                            foreach (Point pt in PointsAround(cpt, brush_size, brush_size))
                            {
                                if (map_bounds.Contains(pt))
                                    grid[pt.Y, pt.X] = GetTileCode(selectedItem, !clear);
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
                                    grid[pt.Y, pt.X] = GetTileCode(selectedItem, false);
                            }
                        }
                    }
                }
            }
        }

        int FlipHorizontally(int tile)
        {
            if (type == Levels.MapType.Minimap || tile == 0) return tile;
            return (tile ^ 0x400);
        }
        int FlipVertically(int tile)
        {
            if (type == Levels.MapType.Minimap || tile == 0) return tile;
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
        void DrawTile(SpriteBatch sprite_batch, Rectangle dst, int tile, bool overridePalette)
        {
            if (type == Levels.MapType.Minimap)
                sprites.Draw(sprite_batch, overridePalette ? sprites.SelectedSet : tile, 0, dst);
            else
            {
                int tile_index = tile;
                int tile_id = tile_index & 0x3FF;
                int palette = overridePalette ? sprites.SelectedSet : (tile_index & 0xF000) >> 12;
                SpriteEffects effects =
                    ((tile_index & 0x400) != 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None) |
                    ((tile_index & 0x800) != 0 ? SpriteEffects.FlipVertically : SpriteEffects.None);
                if (type == Levels.MapType.Physical)
                {
                    if (tile_id >= PhysicalMapLogic.CONTROL_MIN_ID)
                    {
                        // Rendering of non-graphic essential elements
                        Color? c;
                        if (PhysicalMapLogic.STARTING_ZONE_IDS.Contains(tile_id))
                            c = PhysicalMapLogic.StartingZoneColor(tile_id);
                        else if (PhysicalMapLogic.HEALING_ZONE_IDS.Contains(tile_id))
                            c = PhysicalMapLogic.HealingZoneColor(tile_id);
                        else if (PhysicalMapLogic.ENDING_ZONE_IDS.Contains(tile_id))
                            c = PhysicalMapLogic.EndingZoneColor(tile_id);
                        else
                            c = PhysicalMapLogic.UNSUPPORTED_COLOR;
                        if (c.HasValue)
                            sprite_batch.FillRectangle(dst, c.Value);
                    }
                    else if (tile_id <= PhysicalMapLogic.VISIBLE_MAX_ID)
                        sprites.Draw(sprite_batch, palette, tile_id, dst, effects);
                }
                else
                    sprites.Draw(sprite_batch, palette, tile_id, dst, effects);
            }
        }
        public void Draw(SpriteBatch sprite_batch, GameTime gt, MouseState mouse, KeyboardState keyboard)
        {
            sprite_batch.FillRectangle(bounds, Color.CornflowerBlue);
            int h = Grid.GetLength(0);
            int w = Grid.GetLength(1);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Rectangle dst = TileCoordToScreenRect(x,y);
                    if (dst.Intersects(bounds))
                        DrawTile(sprite_batch, dst, Grid[y, x], inventoryMode);
                }
            }
            // Draw map bounds
            Rectangle map_bounds = Rectangle.Union(TileCoordToScreenRect(0, 0), TileCoordToScreenRect(w-1, h-1));
            // Issue with DrawRectangle: https://github.com/rds1983/Myra/issues/211
            TilesSet.DrawRectangle(sprite_batch, map_bounds, Color.Red, 2);
            // Draw selected element
            if (!mouse_move_is_selecting)
            {
                if ((!keyboard.IsKeyDown(Keys.LeftControl) && !keyboard.IsKeyDown(Keys.LeftAlt)) || gt.TotalGameTime <= showBrushUntil)
                {
                    Point pt = ScreenCoordToTileCoord(mouse.Position.X, mouse.Position.Y);
                    Rectangle cr = TileCoordToScreenRect(pt.X, pt.Y);//new Rectangle(mouse.Position, new Point(TileSize, TileSize));
                    Rectangle union = cr;
                    if (selectionGrid == null || (selectionGrid.GetLength(0) == 1 && selectionGrid.GetLength(1) == 1))
                    {
                        int selectedItem = 0;
                        if (selectionGrid != null)
                            selectedItem = selectionGrid[0, 0];

                        foreach (Rectangle r in RectanglesAround(cr, brush_size, brush_size))
                        {
                            union = Rectangle.Union(union, r);
                            if (r.Intersects(bounds))
                                DrawTile(sprite_batch, r, selectedItem, true);
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
                            Rectangle r = new Rectangle(cr.Location + new Point(offset.X * cr.Size.X, offset.Y * cr.Size.Y), cr.Size);
                            union = Rectangle.Union(union, r);
                            if (r.Intersects(bounds))
                                DrawTile(sprite_batch, r, selectedItem, false);
                        }
                        TilesSet.DrawRectangle(sprite_batch, union, Color.White, 1);
                    }
                    TilesSet.DrawRectangle(sprite_batch, union, Color.White, 1);
                }
            }
            else // Draw selection rectangle
            {
                Rectangle r = Rectangle.Union(new Rectangle(initial_mouse_move_pos.Value, Point.Zero), new Rectangle(mouse.Position, Point.Zero));
                TilesSet.DrawRectangle(sprite_batch, r, Color.White, 1);
            }
        }
    }
}
