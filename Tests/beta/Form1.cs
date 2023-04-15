using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace beta
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.MouseLeave += Form1_MouseLeave;

            var st = new SideTrigger();
            st.OnTriggered += St_OnTriggered;
        }
        bool show = true;
        private void Form1_MouseLeave(object? sender, EventArgs e)
        {
            if (!show)
                return;
            show = false;
            this.Invoke(() =>
            {
                for (int i = 0; i < 8; i++)
                {
                    this.Location = new Point((int)( - Math.Sin(Math.PI * i / 16) * 150 -10), this.Location.Y);
                    Task.Delay(1).Wait();
                }
                this.Hide();
            });
        }

        private void St_OnTriggered()
        {
            if (show)
                return;
            show = true;
            this.Invoke(() =>
            {
                this.Show();
                for (int i = 0; i <8; i++)
                {
                    this.Location = new Point((int)(1-Math.Sin(Math.PI * i / 16) *-150-160), this.Location.Y);
                    Task.Delay(1).Wait();
                }
            });
        }
    }
}
