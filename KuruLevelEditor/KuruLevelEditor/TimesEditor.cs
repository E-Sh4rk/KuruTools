using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Resources;
using System.Text;

namespace KuruLevelEditor
{
    class TimesEditor
    {
        private Desktop _desktop;
        private Game1 _game;

        private TextBox normalLevels;
        private ScrollViewer scrollNormal;
        private TextBox easyLevels;
        private ScrollViewer scrollEasy;

        private bool inSeconds;

        const int NUMBER_TIMES_PER_LEVEL = 3;

        void saveChanges()
        {
            string[] levels = Levels.AllLevels;
            int nbLevels = levels.Length;
            string[] normalLines = normalLevels.Text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            ushort[,] normal = Utils.LinesToUint16Table(normalLines, nbLevels, NUMBER_TIMES_PER_LEVEL);
            string[] easyLines = easyLevels.Text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            ushort[,] easy = Utils.LinesToUint16Table(easyLines, easyLines.Length, NUMBER_TIMES_PER_LEVEL);
            ushort[,] table = new ushort[nbLevels + easyLines.Length, NUMBER_TIMES_PER_LEVEL];
            for (int j = 0; j < table.GetLength(0); j++)
            {
                for (int i = 0; i < table.GetLength(1); i++)
                    table[j, i] = j >= nbLevels ? easy[j-nbLevels, i] : normal[j, i];
            }
            // TODO: isSeconds
            string result = Utils.Uint16TableToString(table);
            File.WriteAllText(Levels.GetTimesPath(), result);
        }

        void loadData()
        {
            // TODO: inSeconds
            normalLevels.Text = "";
            easyLevels.Text = "";
            try
            {
                string[] lines = File.ReadAllLines(Levels.GetTimesPath());
                int i = 0;
                string[] levels = Levels.AllLevels;
                foreach (string level in levels)
                {
                    if (i >= lines.Length) break;
                    normalLevels.Text += lines[i] + "     # " + level + Environment.NewLine;
                    i++;
                }
                foreach (string level in levels)
                {
                    if (i >= lines.Length) break;
                    easyLevels.Text += lines[i] + "     # " + level + Environment.NewLine;
                    i++;
                }
            }
            catch { }
        }

        public TimesEditor(Game1 game, bool inSeconds)
        {
            _game = game;
            this.inSeconds = inSeconds;

            Panel panel = new Panel()
            {
                Background = new SolidBrush(Color.Black),
                Padding = new Myra.Graphics2D.Thickness(20,20,20,20)
            };

            var grid = new Grid
            {
                RowSpacing = 10,
                ColumnSpacing = 10
            };

            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.HorizontalAlignment = HorizontalAlignment.Left;
            grid.RowsProportions.Add(new Proportion(ProportionType.Part));
            grid.VerticalAlignment = VerticalAlignment.Top;

            Label labelExplanation = new Label()
            {
                Id = "labelExplanation",
                Text = "Please enter times for the ranks Trainee, Professor and Master.",
                GridColumn = 0,
                GridRow = 0,
                GridColumnSpan = 6
            };
            grid.Widgets.Add(labelExplanation);

            // Normal mode
            Label labelNormal = new Label()
            {
                Id = "labelNormal",
                Text = "Normal mode:",
                GridColumn = 0,
                GridRow = 1,
                GridColumnSpan = 2
            };
            grid.Widgets.Add(labelNormal);

            scrollNormal = new ScrollViewer()
            {
                GridRow = 2,
                GridColumn = 0,
                GridColumnSpan = 6,
                GridRowSpan = 9,
                ShowHorizontalScrollBar = false,
                ShowVerticalScrollBar = true
            };
            normalLevels = new TextBox()
            {
                Multiline = true,
                Font = Load.Monospace
            };
            scrollNormal.Content = normalLevels;
            grid.Widgets.Add(scrollNormal);

            // Easy mode
            Label labelEasy = new Label()
            {
                Id = "labelEasy",
                Text = "Easy mode:",
                GridColumn = 0,
                GridRow = 11,
                GridColumnSpan = 2
            };
            grid.Widgets.Add(labelEasy);

            scrollEasy = new ScrollViewer()
            {
                GridRow = 12,
                GridColumn = 0,
                GridColumnSpan = 6,
                GridRowSpan = 9,
                ShowHorizontalScrollBar = false,
                ShowVerticalScrollBar = true
            };
            easyLevels = new TextBox()
            {
                Multiline = true,
                Font = Load.Monospace
            };
            scrollEasy.Content = easyLevels;
            grid.Widgets.Add(scrollEasy);

            loadData();

            // --- SUBMIT ---

            var buttonSaveQuit = new TextButton
            {
                GridColumn = 1,
                GridRow = 21,
                Text = "Save and Quit",
                Width = 150,
            };
            buttonSaveQuit.Click += (s, a) =>
            {
                saveChanges();
                _game.CloseTimesEditor();
            };
            grid.Widgets.Add(buttonSaveQuit);

            var buttonCancel = new TextButton
            {
                GridColumn = 3,
                GridRow = 21,
                Text = "Cancel",
                Width = 150,
            };
            buttonCancel.Click += (s, a) =>
            {
                _game.CloseTimesEditor();
            };
            grid.Widgets.Add(buttonCancel);

            panel.Widgets.Add(grid);
            _desktop = new Desktop();
            _desktop.Root = panel;
        }

        public void Render()
        {
            _desktop.Render();
        }
    }
}
