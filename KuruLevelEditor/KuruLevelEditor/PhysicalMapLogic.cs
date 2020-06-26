using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace KuruLevelEditor
{
    class PhysicalMapLogic
    {
        // ===== CONTROL TYPES AND COLORS =====
        public readonly static int[] VISIBLE_CONTROL_TILES = new int[] { 0xF2, 0xF3 };
        public readonly static int[] HEALING_ZONE_IDS = new int[] { 0xEA, 0xEB, /*0xEC,*/ 0xED, 0xEE };
        public readonly static int[] STARTING_ZONE_IDS = new int[] { 0xFB, 0xFC, 0xFD };
        public readonly static int[] ENDING_ZONE_IDS = new int[] { 0xFE, 0xFF };
        public readonly static int[] SPRING_IDS = new int[] { 0xF8, 0xF9 };
        public readonly static int[] NUMBER_TILES = new int[] { 0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9 };
        public readonly static int[] MOVING_OBJECTS_IDS = new int[] { 0xF0, 0xF1, 0xF4, 0xF5, 0xF6, 0xF7 };

        readonly static Color HEALING_ZONE_COLOR = new Color(0xFF, 0x33, 0x33, 0xFF);
        readonly static Color HEALING_ZONE_BASE_COLOR = new Color(0xFF, 0x73, 0x73, 0xFF);
        readonly static Color STARTING_ZONE_BASE_COLOR = new Color(0xA0, 0xA0, 0xFF, 0xFF);
        readonly static Color STARTING_ZONE_CW_COLOR = new Color(0x20, 0x5C, 0xFF, 0xFF);
        readonly static Color STARTING_ZONE_CCW_COLOR = new Color(0x5C, 0x20, 0xFF, 0xFF);
        readonly static Color ENDING_ZONE_COLOR = new Color(0xAA, 0x86, 0x29, 0xFF);
        readonly static Color ENDING_ZONE_BASE_COLOR = new Color(0xFF, 0xD6, 0x29, 0xFF);

        public readonly static Color UNSUPPORTED_COLOR = new Color(0x00, 0x00, 0x00, 0xFF);

        public static Color HealingZoneColor(int tile_id)
        {
            Color c = tile_id < 0xEC ? HEALING_ZONE_BASE_COLOR : HEALING_ZONE_COLOR;
            if (tile_id == 0xEB || tile_id == 0xEE)
            {
                c.B += 0x60;
                c.G -= 0x30;
                c.R -= 0x30;
            }

            return c;
        }
        public static Color StartingZoneColor(int tile_id)
        {
            if (tile_id == 0xFB)
                return STARTING_ZONE_CW_COLOR;
            else if (tile_id == 0xFC)
                return STARTING_ZONE_CCW_COLOR;

            return STARTING_ZONE_BASE_COLOR;
        }

        public static Color EndingZoneColor(int tile_id)
        {
            if (tile_id == 0xFF)
                return ENDING_ZONE_COLOR;

            return ENDING_ZONE_BASE_COLOR;
        }

        public static Texture2D TextureOfSpring(int tile_id)
        {
            if (tile_id == 0xF8)
                return Load.SpringVertical;

            return Load.SpringHorizontal;
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
                case 0xF4:
                    return Load.Shooter;
                case 0xF5:
                    return Load.Piston;
                case 0xF6:
                    return Load.RollerCatcher;
                case 0xF7:
                    return Load.Roller;
            }
            return null;
        }

        public static Texture2D UnderlayOfVisibleControlTile(int tile_id)
        {
            return tile_id == 0xF2 ? Load.StartingDiagonal : Load.EndingDiagonal;
        }

        public const int VISIBLE_MAX_ID = 0xDF;
        public const int CONTROL_MIN_ID = 0xE0;

        // ===== MAP DATA MODIFIER =====
        List<int> map_data;
        int w;
        int capacity;
        public const int NUMBER_RESERVED_ROWS = 4;

        public PhysicalMapLogic(int[,] grid)
        {
            w = grid.GetLength(1);
            map_data = new List<int>();
            capacity = 0;
            for (int y = 0; y < NUMBER_RESERVED_ROWS; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int id = grid[y, x] & 0x3FF;
                    if (id <= VISIBLE_MAX_ID && id != 0)
                    {
                        NormalizeData();
                        return;
                    }
                    map_data.Add(grid[y, x]);
                    capacity++;
                }
            }
            NormalizeData();
        }

        public void OverrideGridData(int[,] grid)
        {
            for (int i = 0; i < capacity; i++)
            {
                int y = i / w;
                int x = i % w;
                if (map_data.Count > i)
                    grid[y, x] = map_data[i];
                else
                    grid[y, x] = 0;
            }
        }

        public struct BonusInfo
        {
            public BonusInfo(int ID, int x, int y)
            {
                this.ID = ID;
                this.x = x;
                this.y = y;
            }
            public int ID;
            public int x;
            public int y;
        }

        int GetMapInfoData(ref int offset)
        {
            if (offset >= map_data.Count)
                return 0;
            int res = 0;
            int tile = map_data[offset];
            while (tile >= 0xE0 && tile <= 0xE9)
            {
                res = res * 10 + tile - 0xE0;
                offset++;
                if (offset >= map_data.Count) break;
                tile = map_data[offset];
            }
            offset++;
            return res;
        }

        List<int> GenerateMapInfoData(int[] vs)
        {
            List<int> res = new List<int>();

            foreach(int v in vs)
            {
                string vStr = v.ToString();
                for (int i = 0; i < vStr.Length; i++)
                    res.Add(Convert.ToInt32(vStr[i].ToString()) + 0xE0);
                res.Add(0);
            }

            return res;
        }

        public BonusInfo? GetBonusInfo()
        {
            int offset = 0;
            int ID = GetMapInfoData(ref offset);
            if (ID != 0)
            {
                BonusInfo bi = new BonusInfo();
                bi.ID = ID;
                bi.x = GetMapInfoData(ref offset)/* * 8 - 4*/;
                bi.y = GetMapInfoData(ref offset)/* * 8 - 4*/;
                return bi;
            }
            return null;
        }

        public void SetBonusInfo(BonusInfo? bonus)
        {
            int offset = 0;
            int ID = GetMapInfoData(ref offset);
            if (bonus.HasValue)
            {
                SetBonusInfo(null);
                map_data.Insert(0, 0);
                map_data.Insert(0, 0);
                map_data.InsertRange(0, GenerateMapInfoData(new int[]{ bonus.Value.ID, bonus.Value.x, bonus.Value.y }));
            }
            else
            {
                if (ID != 0)
                {
                    GetMapInfoData(ref offset);
                    GetMapInfoData(ref offset);
                    for (int i = 0; i < offset; i++) {
                        if (map_data.Count == 0) break;
                        map_data.RemoveAt(0);
                    }
                }
            }
            NormalizeData();
        }

        void NormalizeData()
        {
            // Remove initial zeros
            while (map_data.Count > 0 && map_data[0] == 0)
                map_data.RemoveAt(0);
            // Remove final zeros
            while (map_data.Count > 0 && map_data[map_data.Count -1] == 0)
                map_data.RemoveAt(map_data.Count - 1);
            // TODO: Remove intermediate useless zeros
            // If too large, truncate
            while (map_data.Count > capacity)
                map_data.RemoveAt(map_data.Count - 1);
        }
    }
}
