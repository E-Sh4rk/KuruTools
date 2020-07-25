using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

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
    }
}
