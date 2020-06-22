using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace KuruLevelEditor
{
    class PhysicalMapLogic
    {
        public readonly static int[] HEALING_ZONE_IDS = new int[] { 0xEA, 0xEB, /*0xEC,*/ 0xED, 0xEE };
        public readonly static int[] STARTING_ZONE_IDS = new int[] { 0xFB, 0xFC, 0xFD };
        public readonly static int[] ENDING_ZONE_IDS = new int[] { 0xFE, 0xFF };

        readonly static Color HEALING_ZONE_BASE_COLOR = new Color(0xFF, 0x73, 0x73, 0x80);
        readonly static Color STARTING_ZONE_BASE_COLOR = new Color(0x80, 0x8C, 0xFF, 0x80);
        readonly static Color STARTING_ZONE_CW_COLOR = new Color(0x20, 0x5C, 0xFF, 0x80);
        readonly static Color STARTING_ZONE_CCW_COLOR = new Color(0x00, 0x2C, 0xFF, 0x80);
        readonly static Color ENDING_ZONE_COLOR = new Color(0xAA, 0x86, 0x29, 0x80);
        readonly static Color ENDING_ZONE_BASE_COLOR = new Color(0xFF, 0xD6, 0x29, 0x80);

        public readonly static Color UNSUPPORTED_COLOR = new Color(0x00, 0x00, 0x00, 0x80);

        public static Color HealingZoneColor(int tile_id)
        {
            Color c = HEALING_ZONE_BASE_COLOR;
            if (tile_id > 0xEC)
            {
                c.G -= (byte)((tile_id - 0xEC) * 0x30);
                c.B -= (byte)((tile_id - 0xEC) * 0x30);
            }
            else
            {
                c.G -= (byte)((tile_id - 0xEA) * 0x30);
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

        public const int VISIBLE_MAX_ID = 0xDF;
        public const int CONTROL_MIN_ID = 0xE0;
    }
}
