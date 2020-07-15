using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace KuruLevelEditor
{
    class EditableGrid
    {
        const int UNDO_CAPACITY = 100;

        Rectangle bounds;
        public int[,] Grid { get; set; }
        public Point Position { get; set; }
        int tileSize;
        public int TileSize
        {
            get { return tileSize; }
            set {
                Rectangle view = new Rectangle(Position, new Point(bounds.Width, bounds.Height));
                Point center = view.Center;
                Position = new Point(center.X * value / TileSize - bounds.Width / 2, center.Y * value / TileSize - bounds.Height / 2);
                tileSize = value;
            }
        }

        OverflowingStack<int[,]> undoHistory;
        Stack<int[,]> redoHistory;

        public EditableGrid(Rectangle bounds, int[,] grid, Point position, int tileSize)
        {
            this.bounds = bounds;
            Grid = grid;
            Position = position;
            this.tileSize = tileSize;
            undoHistory = new OverflowingStack<int[,]>(UNDO_CAPACITY);
            redoHistory = new Stack<int[,]>();
        }

        public void AddToUndoHistory(int[,] g = null)
        {
            if (g == null) g = Utils.CopyArray(Grid);
            undoHistory.Push(g);
            redoHistory.Clear();
        }
        public void Undo()
        {
            if (undoHistory.Count > 0)
            {
                redoHistory.Push(Grid);
                Grid = undoHistory.Pop();
            }
        }
        public void Redo()
        {
            if (redoHistory.Count > 0)
            {
                undoHistory.Push(Grid);
                Grid = redoHistory.Pop();
            }
        }
    }
}
