using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public SpriteSet(Texture2D texture, bool zero_selectable)
        {
            this.texture = texture;
            NumberSprites = texture.Width / WIDTH;
            index_min = zero_selectable ? 0 : 1;
            Selected = index_min;
        }
        public void Draw(SpriteBatch sprite_batch, int sprite_number, Rectangle dest)
        {
            sprite_batch.Draw(texture, dest, new Rectangle(sprite_number * WIDTH, 0, WIDTH, HEIGHT), Color.White);
        }
        public void DrawSelected(SpriteBatch sprite_batch, Rectangle dest)
        {
            Draw(sprite_batch, Selected, dest);
        }
    }
}
