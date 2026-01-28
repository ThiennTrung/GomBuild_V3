using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GomBuild_V3.Common
{
    public static class LogHelper
    {
        private static RichTextBox _logBox;

        /// Khởi tạo RichTextBox cho logger (gọi 1 lần ở FormMain)
        public static void Init(RichTextBox richTextBox)
        {
            _logBox = richTextBox;
        }
        public static void Log(string message)
        {
            if (_logBox == null) return;

            string log = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";

            if (_logBox.InvokeRequired)
            {
                _logBox.Invoke(new Action(() =>
                {
                    _logBox.AppendText(log);
                    _logBox.ScrollToCaret();
                }));
            }
            else
            {
                _logBox.AppendText(log);
                _logBox.ScrollToCaret();
            }
        }

        public static void Clear()
        {
            if (_logBox == null) return;

            if (_logBox.InvokeRequired)
                _logBox.Invoke(new Action(() => _logBox.Clear()));
            else
                _logBox.Clear();
        }
    }
}
