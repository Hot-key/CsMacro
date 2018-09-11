using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CsMacro.Core.Hotkey;

namespace CsMacro.Core
{
    internal static class WinAPI
    {
        [DllImport("user32.dll")]
        internal static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, Hotkey.KeyModifiers fsModifiers, Keys vk);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern void SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }

    public static class Mouse
    {
        public enum MouseEvent : uint
        {
            MOUSEMOVE = 0x0001,
            ABSOLUTEMOVE = 0x8000,
            LBUTTONDOWN = 0x0002,
            LBUTTONUP = 0x0004,
            RBUTTONDOWN = 0x0008,
            RBUTTONUP = 0x00010
        }
        /// <summary>
        /// 마우스를 지정한 좌표로 이동합니다.
        /// </summary>
        /// <param name="vx">x 좌표</param>
        /// <param name="vy">y 좌표</param>
        /// <param name="type">마우스 이동 형식입니다.</param>
        public static void Move(int vx, int vy,int type = 1)
        {
            if(type == 1)
            {
                Cursor.Position = new Point(vx, vy);
            }
            else
            {
                var screenBounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                var outputX = (uint)(vx * 65536 / screenBounds.Width) + 1;
                var outputY = (uint)(vy * 65536 / screenBounds.Height) + 1;
                WinAPI.mouse_event((uint)MouseEvent.ABSOLUTEMOVE | (uint)MouseEvent.MOUSEMOVE, outputX, outputY, 0, 0);
            }
        }
        /// <summary>
        /// 마우스를 지정한 좌표로 이동후 왼쪽 클릭 합니다.
        /// </summary>
        /// <param name="vx">x 좌표</param>
        /// <param name="vy">y 좌표</param>
        /// <param name="Count">마우스 클릭 횟수 입니다.</param>
        /// <param name="Delay">마우스를 누르고 때는 시간 입니다.</param>
        public static void ClickL(int vx, int vy, int Count = 1,int Delay = 20)
        {
            Cursor.Position = new Point(vx, vy);

            for (int i = 0; i < Count; i++)
            {
                WinAPI.mouse_event((uint)MouseEvent.LBUTTONDOWN, 0, 0, 0, 0);
                Etc.Delay(Delay);
                WinAPI.mouse_event((uint)MouseEvent.LBUTTONUP, 0, 0, 0, 0);
            }
        }
        /// <summary>
        /// 마우스를 지정한 좌표로 이동후 오른쪽 클릭 합니다.
        /// </summary>
        /// <param name="vx">x 좌표</param>
        /// <param name="vy">y 좌표</param>
        /// <param name="Count">마우스 클릭 횟수 입니다.</param>
        /// <param name="Delay">마우스를 누르고 때는 시간 입니다.</param>
        public static void ClickR(int vx, int vy, int Count = 1, int Delay = 20)
        {
            Cursor.Position = new Point(vx, vy);

            for(int i = 0; i < Count; i++)
            {
                WinAPI.mouse_event((uint)MouseEvent.RBUTTONDOWN, 0, 0, 0, 0);
                Etc.Delay(Delay);
                WinAPI.mouse_event((uint)MouseEvent.RBUTTONUP, 0, 0, 0, 0);
            }
        }
    }

    public static class Keyboard
    {
        /// <summary>
        /// Windows.Forms의 Send를 이용하여 문자를 입력합니다.
        /// </summary>
        /// <param name="text">입력할 문자열 입니다.</param>
        public static void Send(string text)
        {
            SendKeys.Send(text);
        }
    }

    public static class Hotkey
    {
        private static int HotkeyID = 31197;
        private static List<HotkeySet> Hotkeys = new List<HotkeySet>();
        public enum KeyModifiers
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Windows = 8
        }

        public static int RegHotKey(IntPtr Handle, KeyModifiers key, Keys vk, MethodInvoker method)
        {
            HotkeyID++;
            WinAPI.RegisterHotKey(Handle, HotkeyID, key, vk);
            Hotkeys.Add(new HotkeySet(HotkeyID, key, vk, method));
            return HotkeyID;
        }

        public static void UnRegHotKey()
        {

        }

        public static void ProcessHotkey(ref Message message)
        {
            switch (message.Msg)
            {
                case 0x0312:
                    Keys key = (Keys)(((int)message.LParam >> 16) & 0xFFFF);
                    KeyModifiers modifier = (KeyModifiers)((int)message.LParam & 0xFFFF);
                    var keySet = Hotkeys.GetEnumerator();
                    while (keySet.MoveNext())
                    {
                        HotkeySet tmpKey = keySet.Current;
                        if ((tmpKey.key) == modifier && tmpKey.vk == key)
                        {
                            tmpKey.method.Invoke();
                        }
                    }
                    break;
            }
        }
    }

    public class HotkeySet
    {
        public readonly int HotkeyID;
        public readonly KeyModifiers key;
        public readonly Keys vk;
        public readonly MethodInvoker method;

        public HotkeySet(int HotkeyID, KeyModifiers key, Keys vk, MethodInvoker method)
        {
            this.HotkeyID = HotkeyID;
            this.key = key;
            this.vk = vk;
            this.method = method;
        }
    }
    internal static class Etc
    {
        internal static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }

            return DateTime.Now;
        }
    }
}

namespace CsMacro.Mouse
{
    
}

namespace CsMacro.Keyboard
{

}