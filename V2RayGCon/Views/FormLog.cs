﻿using System.Linq;
using System.Windows.Forms;

namespace V2RayGCon.Views
{
    public partial class FormLog : Form
    {
        #region Sigleton
        static FormLog _instant;
        public static FormLog GetForm()
        {
            if (_instant == null || _instant.IsDisposed)
            {
                _instant = new FormLog();
            }
            return _instant;
        }
        #endregion

        Service.Setting setting;

        int maxNumberLines;
        delegate void PushLogDelegate(string content);

        FormLog()
        {
            setting = Service.Setting.Instance;
            maxNumberLines = setting.maxLogLines;

            InitializeComponent();

            this.FormClosed += (s, e) => setting.OnLog -= LogReceiver;

            Lib.UI.SetFormLocation<FormLog>(this, Model.Data.Enum.FormLocations.BottomLeft);

            this.Show();
            setting.OnLog += LogReceiver;
        }

        void LogReceiver(object sender, Model.Data.DataEvent e)
        {
            PushLogDelegate pushLog = new PushLogDelegate(PushLog);
            try
            {
                rtBoxLogger?.Invoke(pushLog, e.Data);
            }
            catch { }
        }

        public void PushLog(string content)
        {
            if (rtBoxLogger.Lines.Length >= maxNumberLines - 1)
            {
                rtBoxLogger.Lines = rtBoxLogger.Lines.Skip(rtBoxLogger.Lines.Length - maxNumberLines).ToArray();
            }
            rtBoxLogger.AppendText(content + "\r\n");
        }

        private void rtBoxLogger_TextChanged(object sender, System.EventArgs e)
        {
            // set the current caret position to the end
            rtBoxLogger.SelectionStart = rtBoxLogger.Text.Length;
            // scroll it automatically
            rtBoxLogger.ScrollToCaret();
        }
    }
}
