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
        BonusInfo? bonus;
        List<object> movingObjects;
        int capacity;
        public const int NUMBER_RESERVED_ROWS = 4;

        public BonusInfo? Bonus
        {
            get { return bonus; }
            set { bonus = value; }
        }

        public List<object> MovingObjects
        {
            get { return movingObjects; }
            set { movingObjects = value; }
        }

        public PhysicalMapLogic(int[,] grid)
        {
            List<int> map_data = new List<int>();
            int w = grid.GetLength(1);
            for (int y = 0; y < NUMBER_RESERVED_ROWS; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int id = grid[y, x] & 0x3FF;
                    if (id <= VISIBLE_MAX_ID && id != 0)
                        goto ForEnd;
                    map_data.Add(grid[y, x]);
                }
            }
        ForEnd:
            capacity = map_data.Count;
            bonus = GetBonusInfo(map_data);
            movingObjects = GetMovingObjects(map_data);
        }

        int GetMapInfoData(List<int> map_data, ref int offset)
        {
            if (offset >= map_data.Count)
                return 0;
            int res = 0;
            int tile = map_data[offset];
            while (tile >= 0xE0 && tile <= 0xE9)
            {
                res = res * 10 + tile - 0xE0;
                offset++;
                if (offset >= map_data.Count) return res;
                tile = map_data[offset];
            }
            offset++;
            return res;
        }

        BonusInfo? GetBonusInfo(List<int> map_data)
        {
            int offset = 0;
            int ID = GetMapInfoData(map_data, ref offset);
            if (ID != 0)
            {
                BonusInfo bi = new BonusInfo();
                bi.ID = ID;
                bi.x = GetMapInfoData(map_data, ref offset)/* * 8 - 4*/;
                bi.y = GetMapInfoData(map_data, ref offset)/* * 8 - 4*/;
                return bi;
            }
            return null;
        }

        List<object> GetMovingObjects(List<int> map_data)
        {
            List<object> res = new List<object>();
            int offset = 0;
            while (offset < map_data.Count)
            {
                if ((map_data[offset] & 0x3FF) == 0xF1)
                {
                    offset++;
                    try
                    {
                        int id = GetMapInfoData(map_data, ref offset);
                        int type = map_data[offset] & 0x3FF;
                        offset++;
                        int p1 = GetMapInfoData(map_data, ref offset);
                        int p2 = GetMapInfoData(map_data, ref offset);
                        int p3 = GetMapInfoData(map_data, ref offset);
                        int p4 = GetMapInfoData(map_data, ref offset);
                        switch (type)
                        {
                            case 0xF4:
                                res.Add(new ShooterInfo(id, p1, p2, p3, p4));
                                break;
                            case 0xF5:
                                res.Add(new PistonInfo(id, p1, p2, p3, p4));
                                break;
                            case 0xF7:
                                res.Add(new RollerInfo(id, p1, p2, p3, p4));
                                break;
                            default:
                                // Should not happen...
                                break;
                        }
                    }
                    catch { }
                }
                else
                    offset++;
            }
            return res;
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

        List<int> GenerateBonusInfo(BonusInfo? bonus)
        {
            List<int> res = new List<int>();
            if (bonus.HasValue)
                res.AddRange(GenerateMapInfoData(new int[]{ bonus.Value.ID, bonus.Value.x, bonus.Value.y }));
            return res;
        }

        List<List<int>> GenerateMovingObjects(List<object> objs)
        {
            List<List<int>> res = new List<List<int>>();
            foreach (object o in objs)
            {
                try
                {
                    int id;
                    int type;
                    int[] param;
                    if (o is ShooterInfo)
                    {
                        ShooterInfo si = (ShooterInfo)o;
                        id = si.ID;
                        type = 0xF4;
                        param = si.Params();
                    }
                    else if (o is PistonInfo)
                    {
                        PistonInfo pi = (PistonInfo)o;
                        id = pi.ID;
                        type = 0xF5;
                        param = pi.Params();
                    }
                    else // if (o is RollerInfo)
                    {
                        RollerInfo ri = (RollerInfo)o;
                        id = ri.ID;
                        type = 0xF7;
                        param = ri.Params();
                    }
                    List<int> elt = new List<int>();
                    elt.Add(0xF1);
                    elt.AddRange(GenerateMapInfoData(new int[] { id }));
                    elt.Add(type);
                    elt.AddRange(GenerateMapInfoData(param));
                    res.Add(elt);
                }
                catch { }
            }
            return res;
        }

        void WriteIfEnoughSpace(int[,] grid, List<int> data, ref int offset)
        {
            int w = grid.GetLength(1);
            int lineRemaining = w - (offset % w);
            if (lineRemaining < data.Count)
            {
                for (int i = 0; i < lineRemaining; i++) data.Insert(0, 0);
            }

            if (offset + data.Count > capacity) return;
            foreach (int d in data)
            {
                grid[offset / w, offset % w] = d;
                offset++;
            }
        }

        public void OverrideGridData(int[,] grid)
        {
            int offset = 0;
            WriteIfEnoughSpace(grid, GenerateBonusInfo(bonus), ref offset);
            foreach (List<int> d in GenerateMovingObjects(movingObjects))
                WriteIfEnoughSpace(grid, d, ref offset);

            int w = grid.GetLength(1);
            while (offset < capacity)
            {
                grid[offset / w, offset % w] = 0;
                offset++;
            }
        }

        public struct ShooterInfo
        {
            public const int DEFAULT_PERIOD = 0x90;
            public ShooterInfo(int ID, int minDir, int maxDir, int startTime, int period = DEFAULT_PERIOD)
            {
                this.ID = ID;
                this.minDir = minDir;
                this.maxDir = maxDir;
                this.startTime = startTime;
                this.period = period;
            }
            public int ID;
            public int minDir;
            public int maxDir;
            public int startTime;
            public int period;
            public int[] Params()
            {
                return new int[] { minDir, maxDir, startTime, period };
            }
        }
        public struct PistonInfo
        {
            public const int DEFAULT_WAIT_PERIOD = 120;
            public const int DEFAULT_MOVE_PERIOD = 240;
            public PistonInfo(int ID, int dir, int startTime, int waitPeriod = DEFAULT_WAIT_PERIOD, int movePeriod = DEFAULT_MOVE_PERIOD)
            {
                this.ID = ID;
                this.dir = dir;
                this.startTime = startTime;
                this.waitPeriod = waitPeriod;
                this.movePeriod = movePeriod;
            }
            public int ID;
            public int dir;
            public int startTime;
            public int waitPeriod;
            public int movePeriod;
            public int[] Params()
            {
                return new int[] { dir, startTime, waitPeriod, movePeriod };
            }
        }
        public struct RollerInfo
        {
            public const int DEFAULT_SPEED = 0xC0;
            public RollerInfo(int ID, int dir, int startTime, int period, int speed = DEFAULT_SPEED)
            {
                this.ID = ID;
                this.dir = dir;
                this.startTime = startTime;
                this.period = period;
                this.speed = speed;
            }
            public int ID;
            public int dir;
            public int startTime;
            public int period;
            public int speed;
            public int[] Params()
            {
                return new int[] { dir, startTime, period, speed };
            }
        }
    }
}
