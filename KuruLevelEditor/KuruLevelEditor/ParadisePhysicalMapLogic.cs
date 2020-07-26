using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace KuruLevelEditor
{
    class ParadisePhysicalMapLogic
    {
        // ===== CONTROL TYPES AND COLORS =====
        public readonly static int[] HEALING_ZONE_IDS = new int[] { 0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF };
        public readonly static int[] STARTING_ZONE_IDS = new int[] { 0xFB, 0xFC, 0xFD };
        public readonly static int[] ENDING_ZONE_IDS = new int[] { 0xF7, 0xFE, 0xFF };
        public readonly static int[] FIXED_OBJECTS_IDS = new int[] { 0xF2, 0xF3, 0xF4, 0xF5, 0xF8, 0xF9, 0xFA };
        public readonly static int[] NUMBER_TILES = new int[] { 0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9 };
        public readonly static int[] MOVING_OBJECTS_IDS = new int[] { 0xF0, 0xF1, 0xF6 };

        readonly static Color HEALING_ZONE_COLOR = new Color(0xFF, 0x33, 0x33, 0xFF);
        readonly static Color HEALING_ZONE_BASE_COLOR = new Color(0xFF, 0x63, 0x73, 0xFF);
        readonly static Color STARTING_ZONE_BASE_COLOR = new Color(0xA0, 0xA0, 0xFF, 0xFF);
        readonly static Color STARTING_ZONE_CW_COLOR = new Color(0x20, 0x5C, 0xFF, 0xFF);
        readonly static Color STARTING_ZONE_CCW_COLOR = new Color(0x5C, 0x20, 0xFF, 0xFF);
        readonly static Color ENDING_ZONE_COLOR = new Color(0xAA, 0x86, 0x29, 0xFF);
        readonly static Color ENDING_ZONE_BASE_2_COLOR = new Color(0xD6, 0xD6, 0x39, 0xFF);
        readonly static Color ENDING_ZONE_BASE_COLOR = new Color(0xFF, 0xD6, 0x29, 0xFF);

        public readonly static Color UNSUPPORTED_COLOR = new Color(0x00, 0x00, 0x00, 0xFF);

        public static Color HealingZoneColor(int tile_id)
        {
            Color c = tile_id < 0xED ? HEALING_ZONE_BASE_COLOR : HEALING_ZONE_COLOR;
            if (tile_id == 0xEB || tile_id == 0xEE)
            {
                c.B += 0x60;
                c.G -= 0x18;
                c.R -= 0x30;
            }
            else if (tile_id == 0xEC || tile_id == 0xEF)
            {
                c.B += 0x60;
                c.G -= 0x30;
                c.R -= 0x60;
            }

            return c;
        }
        public static Color StartingZoneColor(int tile_id)
        {
            if (tile_id == 0xFB)
                return STARTING_ZONE_CW_COLOR;
            if (tile_id == 0xFC)
                return STARTING_ZONE_CCW_COLOR;

            return STARTING_ZONE_BASE_COLOR;
        }

        public static Color EndingZoneColor(int tile_id)
        {
            if (tile_id == 0xFE)
                return ENDING_ZONE_BASE_COLOR;
            if (tile_id == 0xF7)
                return ENDING_ZONE_BASE_2_COLOR;

            return ENDING_ZONE_COLOR;
        }

        public static Texture2D TextureOfFixedObjects(int tile_id)
        {
            if (tile_id == 0xF2)
                return Load.ConveyorV;
            if (tile_id == 0xF3)
                return Load.ConveyorH;
            if (tile_id == 0xF4)
                return Load.ConveyorDiag;
            if (tile_id == 0xF5)
                return Load.Key;
            if (tile_id == 0xF8)
                return Load.SpringVertical;
            if (tile_id == 0xF9)
                return Load.SpringHorizontal;

            return Load.SpringDiag;
        }

        public static Texture2D TextureOfNumber(int tile)
        {
            return Load.SpecialNumbers[tile - 0xE0];
        }

        public static Texture2D TextureOfMovingObject(int tile_id)
        {
            switch (tile_id)
            {
                case 0xF0:
                    return Load.Lookup;
                case 0xF1:
                    return Load.Info;
                case 0xF6:
                    return Load.RollerCatcher;
            }
            return null;
        }

        public const int VISIBLE_MAX_ID = 0xDF;
        public const int CONTROL_MIN_ID = 0xE0;

        enum PARADISE_OBJECTS
        {
            Array = 0,
            Offset = 1,
            // TODO: 2 = Roller Catcher ?
            Roller = 3,
            Piston = 4,
            Shooter = 5,
            RollerRing = 6,
            Cog = 7,
            ArcOfFire = 8,
            RingOfFire = 9,
            ClockHand = 10,
            Pendulum = 11,
            Ghost = 12,
            Sword = 13,
            MovingWall = 14,
            Gate = 15
        }
        public static string[] ObjectsStr = new string[]
        {
            "Array", "Offset", null, "Roller", "Piston", "Shooter", "RollerRing", "Cog", "ArcOfFire", "RingOfFire",
            "ClockHand", "Pendulum", "Ghost", "Sword", "MovingWall", "Gate"
        };
        static string StrOfObject(PARADISE_OBJECTS obj)
        {
            return ObjectsStr[(int)obj];
        }
        static PARADISE_OBJECTS? ObjectOfStr(string str)
        {
            int i = 0;
            foreach (string elt in ObjectsStr)
            {
                if (elt != null && elt.Equals(str, StringComparison.OrdinalIgnoreCase))
                    return (PARADISE_OBJECTS)i;
                i++;
            }
            return null;
        }

        List<int[]> objects;
        public ParadisePhysicalMapLogic(string[] lines)
        {
            objects = new List<int[]>();
            foreach (string line in lines)
            {
                try
                {
                    string[] elts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    int[] l = new int[6];
                    for (int x = 0; x < 6; x++)
                        l[x] = Convert.ToInt32(elts[x], 16);
                    objects.Add(l);
                }
                catch { }
            }
        }

        public string GetPrettyText()
        {
            StringBuilder res = new StringBuilder();
            int i = 0;
            foreach (int[] obj in objects)
            {
                res.Append(i.ToString().PadLeft(3, ' ') + " ");
                res.Append(StrOfObject((PARADISE_OBJECTS)obj[0]).PadLeft(10, ' ') + " ");
                res.Append(obj[1].ToString().PadLeft(5, ' ') + " ");
                res.Append(obj[2].ToString().PadLeft(5, ' ') + " ");
                res.Append(obj[3].ToString().PadLeft(5, ' ') + " ");
                res.Append(obj[4].ToString().PadLeft(5, ' ') + " ");
                res.Append(obj[5].ToString().PadLeft(5, ' '));
                res.Append(Environment.NewLine);
                i++;
            }
            return res.ToString();
        }

        public void FromPrettyText(string text)
        {
            List<int[]> res = new List<int[]>();
            string[] lines = text.Replace("\r", "").Split("\n", StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                try
                {
                    string[] elts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    int id = Convert.ToInt32(elts[0]);
                    if (id >= 1000) continue;
                    while (res.Count <= id)
                        res.Add(new int[] { 0, 0, 0, 0, 0, 1});
                    int[] cur = new int[] {
                        (int)ObjectOfStr(elts[1]),
                        Convert.ToInt32(elts[2]),
                        Convert.ToInt32(elts[3]),
                        Convert.ToInt32(elts[4]),
                        Convert.ToInt32(elts[5]),
                        Convert.ToInt32(elts[6])
                    };
                    res[id] = cur;
                }
                catch { }
            }
            objects = res;
        }

        public string[] GetLines()
        {
            string[] res = new string[objects.Count];
            int i = 0;
            foreach (int[] obj in objects)
            {
                StringBuilder b = new StringBuilder();
                for (int x = 0; x < 6; x++)
                    b.Append(obj[x].ToString("X").PadLeft(4, ' ') + " ");
                res[i] = b.ToString();
                i++;
            }
            return res;
        }
    }
}
