﻿using Microsoft.Xna.Framework;
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
		public static Texture2D SpringDiag;
		public static Texture2D[] SpecialNumbers { get; private set; }
		public static Texture2D StartingDiagonal;
		public static Texture2D EndingDiagonal;

		public static Texture2D Lookup;
		public static Texture2D Info;
		public static Texture2D Shooter;
		public static Texture2D Piston;
		public static Texture2D Roller;
		public static Texture2D RollerCatcher;

		public static Texture2D Ice;
		public static Texture2D Key;
		public static Texture2D ConveyorH;
		public static Texture2D ConveyorV;
		public static Texture2D ConveyorDiag;

		public static Texture2D OverworldMap;
		public static Texture2D ConnectorDot;
		public static Texture2D BlueDot;
		public static Texture2D OrangeDot;
		public static Texture2D Star;
		public static Texture2D MagicHatUnbeaten;
		public static Texture2D MagicHatBeaten;

		public static Texture2D SelectedMarker;


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
				{
					if (Settings.Paradise)
						MinimapColors[i] = Content.Load<Texture2D>("minimap" + i.ToString("D2") + "_paradise");
					else
						MinimapColors[i] = Content.Load<Texture2D>("minimap" + i.ToString("D2"));
				}
				SpringHorizontal = Content.Load<Texture2D>("spring_horizontal");
				SpringVertical = Content.Load<Texture2D>("spring_vertical");
				SpringDiag = Content.Load<Texture2D>("spring_diag");
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
				Ice = Content.Load<Texture2D>("ice");
				Key = Content.Load<Texture2D>("key");
				ConveyorV = Content.Load<Texture2D>("conveyor_v");
				ConveyorH = Content.Load<Texture2D>("conveyor_h");
				ConveyorDiag = Content.Load<Texture2D>("conveyor_diag");
				SelectedMarker = Content.Load<Texture2D>("selected_marker");
				Monospace = Content.Load<SpriteFont>("monospace");
				return true;
			}
			catch { }
			return false;
        }

		static Levels.MapType[] tilesType;
		// Must be called after Levels.init
		public static bool LoadSpriteContent(GraphicsDevice graphics)
		{
			tilesType = Settings.Paradise ?
				new Levels.MapType[] { Levels.MapType.Background, Levels.MapType.Graphical2, Levels.MapType.Graphical, Levels.MapType.Physical }
				: new Levels.MapType[] { Levels.MapType.Background, Levels.MapType.Graphical, Levels.MapType.Physical };
			try
			{
				Tiles = new Dictionary<WorldAndType, Texture2D[]>();
				foreach (string world in Levels.AllWorlds)
				{
					foreach (Levels.MapType type in tilesType)
					{
						/*WorldAndType lat = new WorldAndType(world, type);
						Texture2D[] ts = new Texture2D[16];*/
						for (int i = 0; i < /*ts.Length*/16; i++)
						{
							string path = Levels.GetTilePath(world, type, i);
							/*FileStream fileStream = new FileStream(path, FileMode.Open);
							Texture2D tile = Texture2D.FromStream(graphics, fileStream);
							fileStream.Close();
							ts[i] = tile;*/
							if (!File.Exists(path)) return false; // Since partial loading has been introduced, we just check here that the file exists...
						}
						//Tiles.Add(lat, ts);
					}
				}
				return true;
			}
			catch { }
			return false;
		}
		public static void LoadWorldTiles(GraphicsDevice graphics, string world)
        {
			foreach (Levels.MapType type in tilesType)
			{
				WorldAndType lat = new WorldAndType(world, type);
				if (Tiles.ContainsKey(lat)) continue;
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
		public static Texture2D GetImage(GraphicsDevice graphics, string path)
        {
			FileStream fileStream = new FileStream(path, FileMode.Open);
			Texture2D map = Texture2D.FromStream(graphics, fileStream);
			fileStream.Close();
			return map;
		}
		public static void LoadOverworldMap(GraphicsDevice graphics)
        {
			string path = Levels.GetOverworldPath();
			Texture2D map = GetImage(graphics, path);
			OverworldMap = map;
        }

		public static void LoadOverworldObjects(GraphicsDevice graphics)
        {
			Dictionary<string, string> dict = Levels.GetOverworldObjectsPaths();
			ConnectorDot = GetImage(graphics, dict["connectorDot"]);
			BlueDot = GetImage(graphics, dict["blueDot"]);
			OrangeDot = GetImage(graphics, dict["orangeDot"]);
			Star = GetImage(graphics, dict["star"]);
			MagicHatUnbeaten = GetImage(graphics, dict["magicHatUnbeaten"]);
			MagicHatBeaten = GetImage(graphics, dict["magicHatBeaten"]);
        }
	}
}
