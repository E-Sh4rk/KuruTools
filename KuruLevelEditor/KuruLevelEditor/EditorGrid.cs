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
        public EditorGrid(Levels.MapType type, Rectangle bounds, TilesSet sprites, int[,] grid, Point position)
        {
            this.bounds = bounds;
            this.sprites = sprites;
            this.grid = grid;
            this.position = position;
            this.type = type;
        }

        public Point TileCoordToScreenCoord(int x, int y)
        {
            return new Point(x * tile_size, y * tile_size) - position + bounds.Location;
        }

        public Rectangle TileCoordToScreenRect(int x, int y)
        {
            return new Rectangle(TileCoordToScreenCoord(x, y), new Point(tile_size, tile_size));
        }

        public Point ScreenCoordToTileCoord(int x, int y)
        {
            Point p = new Point(x, y) + position - bounds.Location;
            return new Point(p.X / tile_size, p.Y / tile_size);
        }

        public Rectangle ScreenCoordToTileRect(int x, int y)
        {
            return new Rectangle(ScreenCoordToTileCoord(x, y), new Point(tile_size, tile_size));
        }

        public int TileSize {
            get { return tile_size; }
            set {
                Rectangle view = new Rectangle(position, new Point (bounds.Size.X, bounds.Size.Y));
                Point center = view.Center;
                position = new Point(center.X * value / tile_size - bounds.Size.X/2, center.Y * value / tile_size - bounds.Size.Y/2);
                tile_size = value;
            }
        }
        public int BrushSize
        {
            get { return brush_size; }
            set { brush_size = value; }
        }
        public Point Position
        {
            get { return position; }
            set { position = value; }
        }
        public int[,] Grid
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

        public void PerformAction(Controller.Action action)
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
                    if (tile_size < 32)
                        TileSize++;
                    break;
                case Controller.Action.ZOOM_OUT:
                    if (tile_size > 1)
                        TileSize--;
                    break;
                case Controller.Action.BRUSH_PLUS:
                    if (brush_size < 0x20)
                        BrushSize++;
                    break;
                case Controller.Action.BRUSH_MINUS:
                    if (brush_size > 1)
                        BrushSize--;
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
                        Rectangle r = Rectangle.Union(new Rectangle(initial_mouse_move_pos.Value, Point.Zero), new Rectangle(mouse.Position, Point.Zero));
                        Point coord1 = ScreenCoordToTileCoord(r.X, r.Y);
                        Point coord2 = ScreenCoordToTileCoord(r.X + r.Width, r.Y + r.Height);
                        Point size = coord2 - coord1 + new Point(1, 1);
                        selectionGrid = new int[size.Y, size.X];
                        for (int y = 0; y < size.Y; y++)
                        {
                            for (int x = 0; x < size.X; x++)
                                selectionGrid[y, x] = grid[y + coord1.Y, x + coord1.X];
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
                    initial_mouse_move_map_position = position;
                    mouse_move_is_selecting = false;
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftAlt))
            {
                if (initial_mouse_move_pos == null && mouse.LeftButton == ButtonState.Pressed)
                {
                    initial_mouse_move_pos = mouse.Position;
                    initial_mouse_move_map_position = position;
                    mouse_move_is_selecting = true;
                }
            }
            else
            {
                if (initial_mouse_move_pos == null)
                {
                    if (selectionGrid == null || (selectionGrid.GetLength(0) == 1 && selectionGrid.GetLength(1) == 1))
                    {
                        int selectedItem = -1;
                        if (mouse.LeftButton == ButtonState.Pressed)
                        {
                            selectedItem = 0;
                            if (selectionGrid != null)
                                selectedItem = selectionGrid[0, 0];
                        }
                        if (mouse.RightButton == ButtonState.Pressed)
                            selectedItem = 0;
                        if (selectedItem >= 0 && bounds.Contains(mouse.Position))
                        {
                            Point cpt = ScreenCoordToTileCoord(mouse.Position.X, mouse.Position.Y);
                            Rectangle map_bounds = new Rectangle(0, 0, grid.GetLength(1), grid.GetLength(0));
                            foreach (Point pt in PointsAround(cpt, brush_size, brush_size))
                            {
                                if (map_bounds.Contains(pt))
                                    grid[pt.Y, pt.X] = GetTileCode(selectedItem, true);
                            }
                        }
                    }
                    else
                    {
                        // TODO
                    }
                }
            }
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
                    if (tile_id >= PhysicalMapLogic.SPECIAL_MIN_ID && tile_id <= PhysicalMapLogic.SPECIAL_MAX_ID)
                    {
                        // Rendering of non-graphic essential elements
                        Color? c = null;
                        if (PhysicalMapLogic.STARTING_ZONE_IDS.Contains(tile_id))
                            c = PhysicalMapLogic.StartingZoneColor(tile_id);
                        else if (PhysicalMapLogic.HEALING_ZONE_IDS.Contains(tile_id))
                            c = PhysicalMapLogic.HealingZoneColor(tile_id);
                        else if (PhysicalMapLogic.ENDING_ZONE_IDS.Contains(tile_id))
                            c = PhysicalMapLogic.EndingZoneColor(tile_id);
                        if (c.HasValue)
                            sprite_batch.FillRectangle(dst, c.Value);
                    }
                    else if (tile_id >= PhysicalMapLogic.CONTROL_MIN_ID)
                        sprite_batch.FillRectangle(dst, PhysicalMapLogic.UNSUPPORTED_COLOR);
                    else if (tile_id <= PhysicalMapLogic.VISIBLE_MAX_ID)
                        sprites.Draw(sprite_batch, palette, tile_id, dst, effects);
                }

            }
        }
        public void Draw(SpriteBatch sprite_batch, MouseState mouse)
        {
            sprite_batch.FillRectangle(bounds, Color.CornflowerBlue);
            int h = grid.GetLength(0);
            int w = grid.GetLength(1);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Rectangle dst = TileCoordToScreenRect(x,y);
                    if (dst.Intersects(bounds))
                        DrawTile(sprite_batch, dst, grid[y, x], false);
                }
            }
            // Draw map bounds
            Rectangle map_bounds = Rectangle.Union(TileCoordToScreenRect(0, 0), TileCoordToScreenRect(w-1, h-1));
            // Issue with DrawRectangle: https://github.com/rds1983/Myra/issues/211
            TilesSet.DrawRectangle(sprite_batch, map_bounds, Color.Red, 2);
            // Draw selected element
            if (!mouse_move_is_selecting)
            {
                Point pt = ScreenCoordToTileCoord(mouse.Position.X, mouse.Position.Y);
                Rectangle cr = TileCoordToScreenRect(pt.X, pt.Y);//new Rectangle(mouse.Position, new Point(tile_size, tile_size));
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
                    foreach (Point offset in PointsAround(Point.Zero, half_size.X+1, half_size.Y+1))
                    {
                        Point selection_offset = offset + half_size;
                        if (selection_offset.X >= selection_size.X || selection_offset.Y >= selection_size.Y)
                            continue;
                        int selectedItem = selectionGrid[selection_offset.Y, selection_offset.X];
                        Rectangle r = new Rectangle(cr.Location + new Point(offset.X*cr.Size.X, offset.Y*cr.Size.Y), cr.Size);
                        union = Rectangle.Union(union, r);
                        if (r.Intersects(bounds))
                            DrawTile(sprite_batch, r, selectedItem, false);
                    }
                    TilesSet.DrawRectangle(sprite_batch, union, Color.White, 1);
                }
                TilesSet.DrawRectangle(sprite_batch, union, Color.White, 1);
            }
            else // Draw selection rectangle
            {
                Rectangle r = Rectangle.Union(new Rectangle(initial_mouse_move_pos.Value, Point.Zero), new Rectangle(mouse.Position, Point.Zero));
                TilesSet.DrawRectangle(sprite_batch, r, Color.White, 1);
            }
        }
    }
}
