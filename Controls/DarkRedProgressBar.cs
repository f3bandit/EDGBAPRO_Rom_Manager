using System;
using System.Drawing;
using System.Windows.Forms;

namespace EDGBAPRO_Rom_Manager.Controls
{
    public class DarkRedProgressBar : Control
    {
        private int _value;
        public int Maximum { get; set; } = 100;

        public int Value
        {
            get => _value;
            set
            {
                _value = Math.Max(0, Math.Min(value, Maximum));
                Invalidate();
            }
        }

        public DarkRedProgressBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            BackColor = Color.FromArgb(32, 32, 32);
            ForeColor = Color.FromArgb(190, 35, 35);
            Size = new Size(180, 16);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            using var bg = new SolidBrush(BackColor);
            using var fill = new SolidBrush(ForeColor);
            using var border = new Pen(Color.FromArgb(70, 70, 70));

            g.FillRectangle(bg, ClientRectangle);
            int fillWidth = (int)Math.Round((Width - 2) * (Value / (double)Math.Max(1, Maximum)));
            if (fillWidth > 0)
            {
                g.FillRectangle(fill, 1, 1, fillWidth, Height - 2);
            }
            g.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
        }
    }
}
