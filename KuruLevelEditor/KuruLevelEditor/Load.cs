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
        public static Texture2D Colors16 { get; private set; }

		public static void LoadContent(ContentManager Content)
		{
			// Texture2D
			Colors16 = Content.Load<Texture2D>("16colors");
		}
	}
}
