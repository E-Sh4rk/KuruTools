using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace KuruLevelEditor
{
    class CustomInventories
    {
        Dictionary<Levels.MapType, EditableGrid> grids;

        public CustomInventories(Rectangle bounds)
        {
            grids = new Dictionary<Levels.MapType, EditableGrid>();
            foreach (Levels.MapType t in Enum.GetValues(typeof(Levels.MapType)))
            {
                // TODO: Saving and loading system
                grids[t] = new EditableGrid(bounds, new int[64, 64], new Point(-8, -8), 16);
            }
        }

        public EditableGrid GetInventory(Levels.MapType type)
        {
            return grids[type];
        }

    }
}
