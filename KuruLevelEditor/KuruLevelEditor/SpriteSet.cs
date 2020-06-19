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
        public int NumberSprites { get; private set; }
        public SpriteSet(Texture2D texture)
        {
            this.texture = texture;
            NumberSprites = texture.Width / WIDTH;
        }
        public void Draw(SpriteBatch sprite_batch, int sprite_number, Rectangle dest)
        {
            sprite_batch.Draw(texture, dest, new Rectangle(sprite_number * WIDTH, 0, WIDTH, HEIGHT), Color.White);
        }
    }
}
