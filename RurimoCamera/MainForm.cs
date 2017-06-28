
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing.Imaging;

namespace RurimoCamera
{
    public partial class MainForm : Form
    {
        public static string FolderPath = "";
        public static MainForm mainform;

        // Keyboard Hook
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;   

        public MainForm()
        {
            InitializeComponent();
        }

        // MainForm Load
        private void MainForm_Load(object sender, EventArgs e)
        {
          // Form Transparent
          this.TransparencyKey = Color.Turquoise;
          this.BackColor = Color.Turquoise;

          mainform = this;
          FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
          sizelabel.Text = "width : " + (this.Width - 16) + " height : " + (this.Height - 64);

           // Global Keyboard hooking
          _hookID = SetHook(_proc);
        }

        // ScreenShot Menu Button
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            // screenShot
            ScreenShot();
        }


        /**
         * @ScreenShot
         * geting form size and saving a image as ".png" file with random naming
         */
        private void ScreenShot()
        {
            this.WindowState = FormWindowState.Normal;
            Bitmap cropped = null;

            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                               Screen.PrimaryScreen.Bounds.Height,
                               PixelFormat.Format32bppArgb);

            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                        Screen.PrimaryScreen.Bounds.Y,
                                        0,
                                        0,
                                        Screen.PrimaryScreen.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);

            // do screenShot
            string random = ((new Random()).Next()).ToString(); // set random file name
            if (Directory.Exists(FolderPath))
            {
                try
                {
                    Rectangle rect = new Rectangle(this.Location.X + 8, this.Location.Y + 56, this.Width - 16, this.Height - 64);
                    cropped = (Bitmap)bmpScreenshot.Clone(rect, bmpScreenshot.PixelFormat);
                    cropped.Save(FolderPath + "\\" + nammingbox.Text + random + ".png", ImageFormat.Png);
                }
                catch
                {
                    try
                    {
                        // Determine the size of the "virtual screen", which includes all monitors.
                        int screenLeft = SystemInformation.VirtualScreen.Left;
                        int screenTop = SystemInformation.VirtualScreen.Top;
                        int screenWidth = SystemInformation.VirtualScreen.Width;
                        int screenHeight = SystemInformation.VirtualScreen.Height;

                        // Create a bitmap of the appropriate size to receive the screenshot.
                        using (Bitmap bmp = new Bitmap(screenWidth, screenHeight))
                        {
                            // Draw the screenshot into our bitmap.
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                g.CopyFromScreen(screenLeft, screenTop, 0, 0, bmp.Size);
                            }

                            Rectangle rect = new Rectangle(this.Location.X + 8, this.Location.Y + 56, this.Width - 16, this.Height - 64);
                            cropped = (Bitmap)bmp.Clone(rect, bmp.PixelFormat);

                            // Do something with the Bitmap here, like save it to a file:
                            cropped.Save(FolderPath + "\\" + nammingbox.Text + random + ".png", ImageFormat.Png);
                        }
                    }
                    catch (Exception e)
                    {
                        // Show Window Message
                        IntPtr handle = IntPtr.Zero;
                        handle = GetForegroundWindow();
                        MessageBox.Show(new WindowWrapper(handle), "Check camera's position", "Rurimo Camera");
                    }
                }
            }
            else
            {
                // Show Window Message
                IntPtr handle = IntPtr.Zero;
                handle = GetForegroundWindow();
                MessageBox.Show(new WindowWrapper(handle), "The specified folder does not exist", "Rurimo Camera");
            }
        }


        /**
         * @ScreenClipboard
         * geting form size and saving a image at Clipboard
         */
        private Bitmap ScreenClipboard()
        {
            Bitmap cropped = null;

            // Create Random Directory
            string random = ((new Random()).Next()).ToString();

            if (Directory.Exists(FolderPath))
            {
                try
                {
                    // Determine the size of the "virtual screen", which includes all monitors.
                    int screenLeft = SystemInformation.VirtualScreen.Left;
                    int screenTop = SystemInformation.VirtualScreen.Top;
                    int screenWidth = SystemInformation.VirtualScreen.Width;
                    int screenHeight = SystemInformation.VirtualScreen.Height;

                    // Create a bitmap of the appropriate size to receive the screenshot.
                    using (Bitmap bmp = new Bitmap(screenWidth, screenHeight))
                    {
                        // Draw the screenshot into our bitmap.
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.CopyFromScreen(screenLeft, screenTop, 0, 0, bmp.Size);
                        }

                        Rectangle rect = new Rectangle(this.Location.X + 8, this.Location.Y + 56, this.Width - 16, this.Height - 64);
                        cropped = (Bitmap)bmp.Clone(rect, bmp.PixelFormat);

                    }
                }
                catch (Exception e)
                {
                    // Show Window Message
                    IntPtr handle = IntPtr.Zero;
                    handle = GetForegroundWindow();
                    MessageBox.Show(new WindowWrapper(handle), "Check camera's position", "Rurimo Camera");
                }
            }
            else
            {
                // Show Window Message
                IntPtr handle = IntPtr.Zero;
                handle = GetForegroundWindow();
                MessageBox.Show(new WindowWrapper(handle), "The specified folder does not exist", "Rurimo Camera");
            }

            return cropped;
        }

        #region Global Keyboard Hoook

        // Set Hook
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                // F2 : Screen Shot

                if ((((Keys)vkCode).ToString()).Equals("F2"))
                {
                    mainform.ScreenShot();
                }

                else if ((((Keys)vkCode).ToString()).Equals("F3"))
                {
                    Clipboard.SetImage(mainform.ScreenClipboard());
                }

            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn,
                            IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
                            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion


        // Select Folder
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() != DialogResult.Cancel)
                FolderPath = folderBrowserDialog1.SelectedPath;
        }

        private void FormResize(object sender, EventArgs e)
        {
            sizelabel.Text = "width : " + (this.Width - 16) + " height : " + (this.Height - 64);
        }

        [DllImport("user32")]
        private static extern IntPtr GetForegroundWindow();

        // Win32 Api
        public class WindowWrapper : System.Windows.Forms.IWin32Window
        {
            private IntPtr _hwnd;
            public WindowWrapper(IntPtr handle)
            {
                _hwnd = handle;
            }
            public IntPtr Handle
            {
                get { return _hwnd; }
            }
        }

    }
}
