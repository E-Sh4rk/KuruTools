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
        const int TILE_SIZE = 8;
        Rectangle bounds;
        SpriteSet sprites;
        int[,] grid;
        Point position;
        public EditorGrid(Rectangle bounds, SpriteSet sprites, int[,] grid)
        {
            this.bounds = bounds;
            this.sprites = sprites;
            this.grid = grid;
            position = Point.Zero;
        }

        public Point TileCoordToScreenCoord(int x, int y)
        {
            return new Point(x* TILE_SIZE, y* TILE_SIZE) - position + bounds.Location;
        }

        public Rectangle TileCoordToScreenRect(int x, int y)
        {
            return new Rectangle(TileCoordToScreenCoord(x,y), new Point(TILE_SIZE, TILE_SIZE));
        }

        public void Draw(SpriteBatch sprite_batch)
        {
            sprite_batch.FillRectangle(bounds, Color.White);
            for (int y = 0; y < grid.GetLength(0); y++)
            {
                for (int x = 0; x < grid.GetLength(1); x++)
                {
                    Rectangle dst = TileCoordToScreenRect(x,y);
                    if (dst.Intersects(bounds))
                        sprites.Draw(sprite_batch, grid[y, x], dst);
                }
            }
        }
    }
}
