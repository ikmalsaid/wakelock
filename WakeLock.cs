using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WakeLock
{
    static class NativeMethods
    {
        // SetThreadExecutionState flags
        internal const uint ES_CONTINUOUS       = 0x80000000;
        internal const uint ES_SYSTEM_REQUIRED  = 0x00000001;
        internal const uint ES_DISPLAY_REQUIRED = 0x00000002;

        [DllImport("kernel32.dll")]
        internal static extern uint SetThreadExecutionState(uint esFlags);
    }

    class WakeLockApp : ApplicationContext
    {
        private NotifyIcon _trayIcon;
        private ToolStripMenuItem _toggleItem;
        private bool _active = false;

        public WakeLockApp()
        {
            _toggleItem = new ToolStripMenuItem("Enable WakeLock", null, OnToggle);
            _toggleItem.Font = new Font(_toggleItem.Font, FontStyle.Bold);

            var exitItem = new ToolStripMenuItem("Exit", null, OnExit);

            var menu = new ContextMenuStrip();
            menu.Items.Add(_toggleItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exitItem);

            _trayIcon = new NotifyIcon()
            {
                Icon = CreateIcon(false),
                Text = "WakeLock – Inactive",
                ContextMenuStrip = menu,
                Visible = true
            };

            _trayIcon.DoubleClick += OnToggle;

            _trayIcon.ShowBalloonTip(2000, "WakeLock",
                "WakeLock is running. Double-click to toggle.",
                ToolTipIcon.Info);
        }

        private void OnToggle(object sender, EventArgs e)
        {
            _active = !_active;

            if (_active)
            {
                NativeMethods.SetThreadExecutionState(
                    NativeMethods.ES_CONTINUOUS |
                    NativeMethods.ES_SYSTEM_REQUIRED |
                    NativeMethods.ES_DISPLAY_REQUIRED);

                _trayIcon.Icon = CreateIcon(true);
                _trayIcon.Text = "WakeLock – Active (sleep prevented)";
                _toggleItem.Text = "Disable WakeLock";
                _trayIcon.ShowBalloonTip(2000, "WakeLock",
                    "Sleep prevention is now ON.",
                    ToolTipIcon.Info);
            }
            else
            {
                NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS);

                _trayIcon.Icon = CreateIcon(false);
                _trayIcon.Text = "WakeLock – Inactive";
                _toggleItem.Text = "Enable WakeLock";
                _trayIcon.ShowBalloonTip(2000, "WakeLock",
                    "Sleep prevention is now OFF.",
                    ToolTipIcon.Info);
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            // Restore normal power state
            NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS);

            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Exit();
        }

        private static Icon CreateIcon(bool active)
        {
            int size = 32;
            using (var bmp = new Bitmap(size, size))
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Background circle
                Color bg = active
                    ? Color.FromArgb(46, 204, 113)   // green
                    : Color.FromArgb(128, 128, 128);  // grey

                using (var brush = new SolidBrush(bg))
                    g.FillEllipse(brush, 1, 1, size - 3, size - 3);

                int cx = size / 2, cy = size / 2;

                if (active)
                {
                    // Sun symbol — centre dot + rays
                    using (var wb = new SolidBrush(Color.White))
                        g.FillEllipse(wb, cx - 4, cy - 4, 8, 8);

                    using (var pen = new Pen(Color.White, 2f))
                    {
                        for (int deg = 0; deg < 360; deg += 45)
                        {
                            double a = deg * Math.PI / 180.0;
                            float x1 = cx + (float)(7 * Math.Cos(a));
                            float y1 = cy + (float)(7 * Math.Sin(a));
                            float x2 = cx + (float)(11 * Math.Cos(a));
                            float y2 = cy + (float)(11 * Math.Sin(a));
                            g.DrawLine(pen, x1, y1, x2, y2);
                        }
                    }
                }
                else
                {
                    // Moon crescent
                    using (var wb = new SolidBrush(Color.White))
                        g.FillEllipse(wb, cx - 7, cy - 7, 14, 14);

                    using (var cb = new SolidBrush(bg))
                        g.FillEllipse(cb, cx - 2, cy - 9, 12, 12);
                }

                return Icon.FromHandle(bmp.GetHicon());
            }
        }

        [STAThread]
        static void Main()
        {
            // Prevent multiple instances
            bool created;
            using (var mutex = new System.Threading.Mutex(true, "WakeLock_SingleInstance", out created))
            {
                if (!created)
                {
                    MessageBox.Show("WakeLock is already running.",
                        "WakeLock", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new WakeLockApp());
            }
        }
    }
}
