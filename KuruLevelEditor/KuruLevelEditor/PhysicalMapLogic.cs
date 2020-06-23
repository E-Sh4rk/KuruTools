using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public readonly static int[] SPRING_IDS = new int[] { 0xF8, 0xF9 };

        readonly static Color HEALING_ZONE_COLOR = new Color(0xFF, 0x33, 0x33, 0xFF);
        readonly static Color HEALING_ZONE_BASE_COLOR = new Color(0xFF, 0x73, 0x73, 0xFF);
        readonly static Color STARTING_ZONE_BASE_COLOR = new Color(0xA0, 0xA0, 0xFF, 0xFF);
        readonly static Color STARTING_ZONE_CW_COLOR = new Color(0x20, 0x5C, 0xFF, 0xFF);
        readonly static Color STARTING_ZONE_CCW_COLOR = new Color(0x5C, 0x20, 0xFF, 0xFF);
        readonly static Color ENDING_ZONE_COLOR = new Color(0xAA, 0x86, 0x29, 0xFF);
        readonly static Color ENDING_ZONE_BASE_COLOR = new Color(0xFF, 0xD6, 0x29, 0xFF);

        public readonly static Color UNSUPPORTED_COLOR = new Color(0x00, 0x00, 0x00, 0xA0);

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

        public const int VISIBLE_MAX_ID = 0xDF;
        public const int CONTROL_MIN_ID = 0xE0;
    }
}
