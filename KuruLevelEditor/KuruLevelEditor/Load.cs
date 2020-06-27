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
		//public static Texture2D RectTexture { get; private set; }
		public static Texture2D LoadingScreen;
        public static Texture2D[] MinimapColors { get; private set; }
		
		public static Texture2D SpringHorizontal;
		public static Texture2D SpringVertical;
		public static Texture2D[] SpecialNumbers { get; private set; }
		public static Texture2D StartingDiagonal;
		public static Texture2D EndingDiagonal;

		public static Texture2D Lookup;
		public static Texture2D Info;
		public static Texture2D Shooter;
		public static Texture2D Piston;
		public static Texture2D Roller;
		public static Texture2D RollerCatcher;

		public static SpriteFont Monospace;

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


		public static bool LoadFixedContent(ContentManager Content)
        {
			try
			{
				/*Color[] data = new Color[] { Color.White };
				RectTexture = new Texture2D(graphics, 1, 1);
				RectTexture.SetData(data);*/
				LoadingScreen = Content.Load<Texture2D>("loading");
				MinimapColors = new Texture2D[16];
				for (int i = 0; i < MinimapColors.Length; i++)
					MinimapColors[i] = Content.Load<Texture2D>("minimap" + i.ToString("D2"));
				SpringHorizontal = Content.Load<Texture2D>("spring_horizontal");
				SpringVertical = Content.Load<Texture2D>("spring_vertical");
				SpecialNumbers = new Texture2D[10];
				for (int i = 0; i < SpecialNumbers.Length; i++)
					SpecialNumbers[i] = Content.Load<Texture2D>("special" + i.ToString());
				StartingDiagonal = Content.Load<Texture2D>("starting_diagonal");
				EndingDiagonal = Content.Load<Texture2D>("ending_diagonal");
				Lookup = Content.Load<Texture2D>("lookup");
				Info = Content.Load<Texture2D>("info");
				Shooter = Content.Load<Texture2D>("shooter");
				Piston = Content.Load<Texture2D>("piston");
				Roller = Content.Load<Texture2D>("roller");
				RollerCatcher = Content.Load<Texture2D>("roller_catcher");
				Monospace = Content.Load<SpriteFont>("monospace");
				return true;
			}
			catch { }
			return false;
        }

		// Must be called after Levels.init
		public static bool LoadSpriteContent(GraphicsDevice graphics)
		{
			try
			{
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
				return true;
			}
			catch { }
			return false;
		}
	}
}
