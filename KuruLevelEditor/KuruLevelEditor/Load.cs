using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace KuruLevelEditor
{
    public static class Load
    {
        // Texture2D
        public static Texture2D[] MinimapColors { get; private set; }
		//public static Texture2D RectTexture { get; private set; }
		public struct WorldAndType
		{
			public WorldAndType(string world, Levels.MapType type)
            {
				this.type = type;
				this.world = world;
            }

            public readonly Levels.MapType type;
			public readonly string world;
        }
		public static Dictionary<WorldAndType, Texture2D[]> Tiles { get; private set; }
		public static Texture2D SpringHorizontal;
		public static Texture2D SpringVertical;

		public static void LoadContent(ContentManager Content, GraphicsDevice graphics)
		{
			// Texture2D
			MinimapColors = new Texture2D[16];
			for (int i = 0; i < MinimapColors.Length; i++)
				MinimapColors[i] = Content.Load<Texture2D>("minimap" + i.ToString("D2"));
			/*Color[] data = new Color[] { Color.White };
			RectTexture = new Texture2D(graphics, 1, 1);
			RectTexture.SetData(data);*/
			Tiles = new Dictionary<WorldAndType, Texture2D[]>();
			foreach (string world in Levels.AllWorlds)
            {
				foreach (Levels.MapType type in new Levels.MapType[] { Levels.MapType.Background, Levels.MapType.Graphical, Levels.MapType.Physical })
                {
					WorldAndType lat = new WorldAndType(world, type);
					Texture2D[] ts = new Texture2D[16];
					for (int i = 0; i < ts.Length; i++)
                    {
						string path = Levels.GetTilePath(world, type, i);
						FileStream fileStream = new FileStream(path, FileMode.Open);
						Texture2D tile = Texture2D.FromStream(graphics, fileStream);
						fileStream.Close();
						ts[i] = tile;
					}
					Tiles.Add(lat, ts);
                }
            }
			SpringHorizontal = Content.Load<Texture2D>("spring_horizontal");
			SpringVertical = Content.Load<Texture2D>("spring_vertical");
		}
	}
}
