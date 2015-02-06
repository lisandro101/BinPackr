using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BinPackr
{
    public partial class MainForm : Form
    {
        private BinList binList;
        private bool logVisible;

        private const int WINDOW_HEIGHT_LOG_HIDDEN = 518;
        private const int WINDOW_HEIGHT_LOG_SHOWN = 662;
        private const int PANEL_SIZE_MAX = 100000;
        private const int GENERATION_COUNT_MAX = 10000;

        private enum SizeSelection
        {
            SIZE_SELECT_SMALL = 0,
            SIZE_SELECT_MEDIUM,
            SIZE_SELECT_LARGE,
            SizeSelectionCount
        }

        private static readonly int[] SIZE_MIN = { 5, 5, 5 };
        private static readonly int[] SIZE_MAX = { 35, 70, 105 };

        public MainForm(ref BinList newBinList)
        {
            InitializeComponent();
            this.binList = newBinList;
            this.sizeSelect.SelectedIndex = (int) SizeSelection.SIZE_SELECT_MEDIUM;
            SetLogVisible(false);
            this.log.DataSource = Log.Instance;
            Log.Instance.ListChanged += LogListChanged;
        }

        public void LogListChanged(object sender, EventArgs arguments)
        {
            this.log.SelectedIndex = this.log.Items.Count - 1;
        }

        public void RefreshPanel()
        {
            this.panel.Invalidate();
        }

        private void SetLogVisible(bool visible)
        {
            this.Height = (visible ? WINDOW_HEIGHT_LOG_SHOWN : WINDOW_HEIGHT_LOG_HIDDEN);
            this.groupBoxLog.Visible = visible;
            this.buttonShowHideDebug.Text = (visible ? "Hide Log" : "Show Log");
            this.logVisible = visible;
        }

        private void PanelPaint(object sender, PaintEventArgs arguments)
        {
            var graphics = arguments.Graphics;
            foreach(var bin in binList)
            {
                graphics.FillRectangle(bin.Brush, bin.DrawingRectangle);
            }
        }

        private void SetPanelSizeClick(object sender, EventArgs arguments)
        {
            int width;
            int height;

            if(!Int32.TryParse(panelWidthInput.Text, out width)) { width = 0; }
            if(!Int32.TryParse(panelHeightInput.Text, out height)) { height = 0; }

            if(width < 0) { width = 0; }
            else if(width > PANEL_SIZE_MAX) { width = PANEL_SIZE_MAX; }

            if(height < 0) { height = 0; }
            else if(height > PANEL_SIZE_MAX) { height = PANEL_SIZE_MAX; }

            this.panelWidthInput.Text = Convert.ToString(width);
            this.panelHeightInput.Text = Convert.ToString(height);
            this.panel.Width = width;
            this.panel.Height = height;

            Log.Instance.AddLine("Panel size set to " + width + "x" + height + " in pixels");
        }

        private void PopulateClick(object sender, EventArgs arguments)
        {
            int count;
            int width;
            int height;

            if(Int32.TryParse(this.countInput.Text, out count))
            {
                if(count < 0) { count = 0; }
                else if(count > GENERATION_COUNT_MAX) { count = GENERATION_COUNT_MAX; }
            }

            this.countInput.Text = Convert.ToString(count);
            var sizeMin = SIZE_MIN[this.sizeSelect.SelectedIndex];
            var sizeMax = SIZE_MAX[this.sizeSelect.SelectedIndex];
            this.binList.Clear();

            var startTime = DateTime.Now;

            this.binList.Randomize(count, sizeMin, sizeMax, this.panel.Width);

            var endTime = DateTime.Now;

            var time = endTime.Subtract(startTime).Ticks / 10000;
            this.binList.GetBoundingBin(out width, out height);
            var area = width * height;

            Log.Instance.AddLine("Took " + time + "ms to generate " + count + " boxes bounded within " + width + "px by " + height + "px (area: " + area + "px)");

            this.RefreshPanel();
        }

        private void ToggleLogClick(object sender, EventArgs arguments)
        {
            this.SetLogVisible(!this.logVisible);
        }

        private void ScrambleClick(object sender, EventArgs e)
        {
            int width;
            int height;
            var startTime = DateTime.Now;

            BinListSorterRandom.Sort(ref binList, this.panel.Width);

            var endTime = DateTime.Now;
            var time = endTime.Subtract(startTime).Ticks / 10000;
            this.binList.GetBoundingBin(out width, out height);
            var area = width * height;
            Log.Instance.AddLine("Took " + time + "ms to scramble " + binList.Count + " boxes bounded within " + width + "px by " + height + "px (area: " + area + "px)");

            this.RefreshPanel();
        }

        private void SortClick(object sender, EventArgs arguments)
        {
            var panelRectangle = new Rectangle(0, 0, panel.Width, panel.Height);
            int unresolved;
            int width;
            int height;
            var startTime = DateTime.Now;

            BinListSorterTree.Sort(ref binList, panelRectangle, out unresolved);

            var endTime = DateTime.Now;

            this.LogCompletionTime(endTime, startTime, unresolved);

            this.binList.GetBoundingBin(out width, out height);
            var area = width * height;

            Log.Instance.AddLine("The newly sorted boxes are bounded within " + width + "px by " + height + "px (area: " + area + "px)");

            this.RefreshPanel();
        }

        private void LogCompletionTime(DateTime endTime, DateTime startTime, int unresolved)
        {
            var time = endTime.Subtract(startTime).Ticks / 10000;

            if(unresolved > 0)
            {
                Log.Instance.AddLine("Took " + time + "ms to sort " + this.binList.Count + " boxes, failing to place " + unresolved + " boxes");
            }
            else
            {
                Log.Instance.AddLine("Took " + time + "ms to sort " + this.binList.Count + " boxes, placed all boxes successfully");
            }
        }
    }
}
