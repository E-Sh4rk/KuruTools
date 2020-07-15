using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace KuruLevelEditor
{
    // TODO: Refactor: implement EditableGrid class, and CustomInventory will just be a dictionnary from MapType to EditableGrid
    // Also use this EditableGrid in EditorGrid to replace the standard grid
    class CustomInventory
    {
        const int UNDO_CAPACITY = 100;
        Dictionary<Levels.MapType, int[,]> grids;
        Dictionary<Levels.MapType, Point> positions;
        Dictionary<Levels.MapType, int> tileSizes;

        Dictionary<Levels.MapType, OverflowingStack<int[,]>> undoHistories;
        Dictionary<Levels.MapType, Stack<int[,]>> redoHistories;

        public CustomInventory()
        {
            grids = new Dictionary<Levels.MapType, int[,]>();
            positions = new Dictionary<Levels.MapType, Point>();
            tileSizes = new Dictionary<Levels.MapType, int>();
            undoHistories = new Dictionary<Levels.MapType, OverflowingStack<int[,]>>();
            redoHistories = new Dictionary<Levels.MapType, Stack<int[,]>>();
            foreach (Levels.MapType t in Enum.GetValues(typeof(Levels.MapType)))
            {
                grids[t] = new int[64, 64]; // TODO: Saving and loading system
                positions[t] = new Point(-8, -8);
                tileSizes[t] = 16;
                undoHistories[t] = new OverflowingStack<int[,]>(UNDO_CAPACITY);
                redoHistories[t] = new Stack<int[,]>();
            }
        }

        public int[,] Grid(Levels.MapType type)
        {
            return grids[type];
        }
        public void SetGrid(Levels.MapType type, int[,] grid)
        {
            grids[type] = grid;
        }
        public Point Position(Levels.MapType type)
        {
            return positions[type];
        }
        public void SetPosition(Levels.MapType type, Point position)
        {
            positions[type] = position;
        }
        public int TileSize(Levels.MapType type)
        {
            return tileSizes[type];
        }
        public void SetTileSize(Levels.MapType type, int size)
        {
            tileSizes[type] = size;
        }
        public void AddToUndoHistory(Levels.MapType type, int[,] g = null)
        {
            if (g == null) g = Utils.CopyArray(Grid(type));
            undoHistories[type].Push(g);
            redoHistories[type].Clear();
        }
        public void Undo(Levels.MapType type)
        {
            if (undoHistories[type].Count > 0)
            {
                redoHistories[type].Push(Grid(type));
                grids[type] = undoHistories[type].Pop();
            }
        }
        public void Redo(Levels.MapType type)
        {
            if (redoHistories[type].Count > 0)
            {
                undoHistories[type].Push(Grid(type));
                grids[type] = redoHistories[type].Pop();
            }
        }
    }
}
