using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using System;
using System.Collections.Generic;
using System.Text;

namespace KuruLevelEditor
{
    class TilesSet
    {
        const int WIDTH = 8;
        const int HEIGHT = 8;
        const int TILES_PER_ROW = 256 / WIDTH;
        Texture2D[] textures;
        int index_min = 0;
        Rectangle display_area;
        int display_size;
        int nb_per_row;
        public int NumberSets { get; private set; }
        public int SelectedSet { get; private set; }
        public void SelectNext()
        {
            SelectedSet++;
            if (SelectedSet >= NumberSets)
                SelectedSet = index_min;
        }
        public void SelectPrevious()
        {
            SelectedSet--;
            if (SelectedSet < index_min)
                SelectedSet = NumberSets - 1;
        }
        public TilesSet(Texture2D[] textures, bool zero_selectable, Rectangle display_area, int display_size)
        {
            this.textures = textures;
            NumberSets = textures.Length;
            index_min = zero_selectable ? 0 : 1;
            SelectedSet = index_min;
            this.display_area = display_area;
            this.display_size = display_size;
            nb_per_row = (display_area.Width + 1) / (display_size + 1);
        }
        public static void DrawRectangle(SpriteBatch sprite_batch, Rectangle rect, Color color, int thickness = 1)
        {
            int half = thickness / 2;
            sprite_batch.FillRectangle(new Rectangle(rect.Location + new Point(-half, 0), new Point(thickness, rect.Height)), color);
            sprite_batch.FillRectangle(new Rectangle(rect.Location + new Point(0, -half), new Point(rect.Width, thickness)), color);
            sprite_batch.FillRectangle(new Rectangle(rect.Location + new Point(rect.Width - half, 0), new Point(thickness, rect.Height)), color);
            sprite_batch.FillRectangle(new Rectangle(rect.Location + new Point(0, rect.Height - half), new Point(rect.Width, thickness)), color);
        }
        public void Draw(SpriteBatch sprite_batch, int sprite_set, int sprite_number, Rectangle dest, SpriteEffects effects = SpriteEffects.None)
        {
            Texture2D texture = textures[sprite_set];
            if (sprite_number >= TILES_PER_ROW * texture.Height / HEIGHT)
                return;
            int x = sprite_number % TILES_PER_ROW;
            int y = sprite_number / TILES_PER_ROW;
            sprite_batch.Draw(texture, dest, new Rectangle(x * WIDTH, y * HEIGHT, WIDTH, HEIGHT), Color.White, 0, Vector2.Zero, effects, 0);
        }
        public void DrawSelected(SpriteBatch sprite_batch, int sprite_number, Rectangle dest, SpriteEffects effects = SpriteEffects.None)
        {
            Draw(sprite_batch, SelectedSet, sprite_number, dest, effects);
        }
        public void DrawSets(SpriteBatch sprite_batch)
        {
            for (int i = index_min; i < NumberSets; i++)
            {
                int x = (i-index_min) % nb_per_row;
                int y = (i-index_min) / nb_per_row;
                Rectangle dst =
                    new Rectangle(display_area.X + x * (display_size + 1), display_area.Y + y * (display_size + 1), display_size, display_size);
                sprite_batch.Draw(textures[i], dst, null, Color.White);
                if (SelectedSet == i)
                    DrawRectangle(sprite_batch, dst, Color.White, 2);
            }
        }

        public void Update(MouseState mouse)
        {
            Point p = mouse.Position;
            if (display_area.Contains(p) && mouse.LeftButton == ButtonState.Pressed)
            {
                int x = (p.X - display_area.X) / (display_size + 1);
                if (x < nb_per_row)
                {
                    int y = (p.Y - display_area.Y) / (display_size + 1);
                    int i = y * nb_per_row + x + index_min;
                    if (i >= index_min && i < NumberSets)
                        SelectedSet = i;
                }
            }
        }

    }
}
