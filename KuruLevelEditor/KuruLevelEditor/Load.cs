using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace KuruLevelEditor
{
    public static class Load
    {
        // Texture2D
        public static Texture2D MinimapColors { get; private set; }
		//public static Texture2D RectTexture { get; private set; }

		public static void LoadContent(ContentManager Content, GraphicsDevice graphics)
		{
			// Texture2D
			MinimapColors = Content.Load<Texture2D>("minimap_colors");
			/*Color[] data = new Color[] { Color.White };
			RectTexture = new Texture2D(graphics, 1, 1);
			RectTexture.SetData(data);*/
		}
	}
}
