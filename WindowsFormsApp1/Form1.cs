using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsMacro.Core;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Hotkey.RegHotKey(this.Handle, Hotkey.KeyModifiers.None, Keys.F1,new MethodInvoker(a));
            Hotkey.RegHotKey(this.Handle, Hotkey.KeyModifiers.None, Keys.F2, new MethodInvoker(b));
        }

        public void a()
        {
            MessageBox.Show("asd");
        }
        public void b()
        {
            MessageBox.Show("asdf");
        }
        protected override void WndProc(ref Message message)
        {
            Hotkey.ProcessHotkey(ref message);
            base.WndProc(ref message);
        }
    }
}
