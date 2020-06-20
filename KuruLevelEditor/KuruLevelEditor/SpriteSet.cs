using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using System;
using System.Collections.Generic;
using System.Text;

namespace KuruLevelEditor
{
    class SpriteSet
    {
        const int WIDTH = 8;
        const int HEIGHT = 8;
        Texture2D texture;
        int index_min = 0;
        Rectangle display_area;
        int display_size;
        int nb_per_row;
        public int NumberSprites { get; private set; }
        public int Selected { get; private set; }
        public void SelectNext()
        {
            Selected++;
            if (Selected >= NumberSprites)
                Selected = index_min;
        }
        public void SelectPrevious()
        {
            Selected--;
            if (Selected < index_min)
                Selected = NumberSprites - 1;
        }
        public SpriteSet(Texture2D texture, bool zero_selectable, Rectangle display_area, int display_size)
        {
            this.texture = texture;
            NumberSprites = texture.Width / WIDTH;
            index_min = zero_selectable ? 0 : 1;
            Selected = index_min;
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
        public void Draw(SpriteBatch sprite_batch, int sprite_number, Rectangle dest)
        {
            sprite_batch.Draw(texture, dest, new Rectangle(sprite_number * WIDTH, 0, WIDTH, HEIGHT), Color.White);
        }
        public void DrawSelected(SpriteBatch sprite_batch, Rectangle dest)
        {
            Draw(sprite_batch, Selected, dest);
        }
        public void DrawSet(SpriteBatch sprite_batch)
        {
            for (int i = index_min; i < NumberSprites; i++)
            {
                int x = (i-index_min) % nb_per_row;
                int y = (i-index_min) / nb_per_row;
                Rectangle dst =
                    new Rectangle(display_area.X + x * (display_size + 1), display_area.Y + y * (display_size + 1), display_size, display_size);
                sprite_batch.Draw(texture, dst, new Rectangle(i * WIDTH, 0, WIDTH, HEIGHT), Color.White);
                if (Selected == i)
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
                    if (i >= index_min && i < NumberSprites)
                        Selected = i;
                }
            }
        }

    }
}
