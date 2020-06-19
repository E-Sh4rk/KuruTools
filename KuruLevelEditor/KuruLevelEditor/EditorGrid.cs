using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using System;
using System.Collections.Generic;
using System.Text;

namespace KuruLevelEditor
{
    class EditorGrid
    {
        int tile_size = 8;
        Rectangle bounds;
        SpriteSet sprites;
        int[,] grid;
        Point position;
        public EditorGrid(Rectangle bounds, SpriteSet sprites, int[,] grid, Point position)
        {
            this.bounds = bounds;
            this.sprites = sprites;
            this.grid = grid;
            this.position = position;
        }

        public Point TileCoordToScreenCoord(int x, int y)
        {
            return new Point(x * tile_size, y * tile_size) - position + bounds.Location;
        }

        public Rectangle TileCoordToScreenRect(int x, int y)
        {
            return new Rectangle(TileCoordToScreenCoord(x, y), new Point(tile_size+1, tile_size));
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
        public Point Position
        {
            get { return position; }
            set { position = value; }
        }

        public void PerformAction(Controller.Action action)
        {
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
            }
        }

        public void DrawRectangle(SpriteBatch sprite_batch, Rectangle rect, Color color)
        {
            sprite_batch.FillRectangle(new Rectangle(rect.Location, new Point(1, rect.Height)), color);
            sprite_batch.FillRectangle(new Rectangle(rect.Location, new Point(rect.Width, 1)), color);
            sprite_batch.FillRectangle(new Rectangle(rect.Location + new Point(rect.Width, 0), new Point(1, rect.Height)), color);
            sprite_batch.FillRectangle(new Rectangle(rect.Location + new Point(0, rect.Height), new Point(rect.Width, 1)), color);
        }
        public void Draw(SpriteBatch sprite_batch)
        {
            sprite_batch.FillRectangle(bounds, Color.White);
            int h = grid.GetLength(0);
            int w = grid.GetLength(1);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Rectangle dst = TileCoordToScreenRect(x,y);
                    if (dst.Intersects(bounds))
                        sprites.Draw(sprite_batch, grid[y, x], dst);
                }
            }
            // Draw map bounds
            Rectangle map_bounds = Rectangle.Union(TileCoordToScreenRect(0, 0), TileCoordToScreenRect(w-1, h-1));
            // Issue with DrawRectangle: https://github.com/rds1983/Myra/issues/211
            DrawRectangle(sprite_batch, map_bounds, Color.Red);
        }
    }
}
