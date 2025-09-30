using Silk.NET.Core.Contexts;
using Silk.NET.Core.Loader;
using Silk.NET.Maths;
using Spout.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static TerraFX.Interop.Windows.Windows;



namespace StreamStartingTimer {

    public static class OpenGLHandler {
        static bool GLInitialised = false;
        static IntPtr Handle = Process.GetCurrentProcess().MainWindowHandle;

        public static void InitGL() {
            if (GLInitialised) return; 
            GLInitialised = true;
            SetOpenglPixelFormat((HWND)Handle);
            var dc = GetDC((HWND)Handle);
            var gctx = wglCreateContext(dc);
            wglMakeCurrent(dc, gctx);
            ReleaseDC((HWND)Handle, dc);
            var gl = new Silk.NET.OpenGL.GL(new WindowsGlNativeContext());
        }
        static unsafe void SetOpenglPixelFormat(HWND window) {
            // Contains desired pixel format characteristics
            PIXELFORMATDESCRIPTOR pfd = new();

            // the size of the struct
            pfd.nSize = (ushort)sizeof(PIXELFORMATDESCRIPTOR);

            // hardcoded version of the struct
            pfd.nVersion = 1;

            // we will draw to the window, we will draw via opengl, and we will use two buffers to swap between them each frame 
            pfd.dwFlags = PFD.PFD_DRAW_TO_WINDOW | PFD.PFD_SUPPORT_OPENGL | PFD.PFD_DOUBLEBUFFER;

            // We expect to use RGBA pixels
            pfd.iPixelType = PFD.PFD_TYPE_RGBA;

            // pixels with 3 * 8 = 24 bits for color 
            pfd.cColorBits = 24;

            // Depth of z-buffer (we don't actually care about that for now)
            pfd.cDepthBits = 32;

            HDC hdc = GetDC(window);
            int iPixelFormat;

            // get the device context's best, available pixel format match  
            iPixelFormat = ChoosePixelFormat(hdc, &pfd);

            // make that match the device context's current pixel format  
            SetPixelFormat(hdc, iPixelFormat, &pfd);

            ReleaseDC(window, hdc);
        }
        public class WindowsGlNativeContext : INativeContext {
            private readonly UnmanagedLibrary _l;

            public WindowsGlNativeContext() {
                _l = new UnmanagedLibrary("opengl32.dll");
            }

            public void Dispose() {
                _l.Dispose();
            }

            public nint GetProcAddress(string proc, int? slot = null) {
                if (TryGetProcAddress(proc, out var address, slot)) {
                    return address;
                }

                throw new InvalidOperationException("No function was found with the name " + proc + ".");
            }

            public unsafe bool TryGetProcAddress(string proc, out nint addr, int? slot = null) {
                if (_l.TryLoadFunction(proc, out addr)) {
                    return true;
                }

                // + 1 for null terminated string
                var asciiName = new byte[proc.Length + 1];

                Encoding.ASCII.GetBytes(proc, asciiName);

                fixed (byte* name = asciiName) {
                    addr = (nint)wglGetProcAddress((sbyte*)name);
                    if (addr != IntPtr.Zero) {
                        return true;
                    }
                }

                return false;
            }
        }
    }

    public class ClockSpout : IDisposable {
        static Bitmap[] ClockFont = new Bitmap[12];
        static int NumberWidth;
        static int NumberHeight;
        static int ColonWidth;
        static int ClockWidth;
        static int ClockArraySize;
        static SpoutSender spoutSender;
        static readonly System.Drawing.Imaging.PixelFormat TexturePixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
        static nint pClockTexture;

        public void GetImageFromTime(int SecondsToGo, nint pClockTexture) {
            //byte[] ClockTexture = new byte[ClockArraySize];
            string CurTime;
            int CurDigit;
            int ColonFix;
            Int32[] TempLine = new Int32[NumberWidth];
            UInt32 TempPixel;
            int y;
            int x;

            CurTime = TimeSpan.FromSeconds(SecondsToGo).ToString(Shared.TimeFormat);

            ColonFix = 0;

            for (int Digit = 0; Digit < 5; Digit++) {
                if (Digit == 2) {
                    ColonFix = ColonWidth - NumberWidth;
                    CurDigit = 10;
                } else { }
                if (CurTime[Digit] == ' ') {
                    CurDigit = 11;
                } else {
                    CurDigit = CurTime[Digit] - 48;
                    if (CurDigit < 0 && CurDigit > 9) { throw new Exception("Digit is not 0-9 or space"); }
                }

                if (CurDigit != 10) {
                    for (y = 0; y < NumberHeight; y++) {
                        for (x = 0; x < NumberWidth; x++) {
                            TempPixel = (UInt32)ClockFont[CurDigit].GetPixel(x, y).ToArgb();
                            TempLine[x] = (Int32)(TempPixel & 0xFF00FF00 | (((TempPixel & 0x00FF0000) >> 16)) | (((TempPixel & 0x000000FF) << 16)));
                        }
                        Marshal.Copy(TempLine, 0, pClockTexture + ((y * ClockWidth * 4) + ((Digit * NumberWidth) + ColonFix) * 4), NumberWidth);
                    }

                }
            }
            //return ClockTexture;
        }

        public ClockSpout(string FontDir) {
            ClockFont[11] = new Bitmap(FontDir + "\\space.png");
            NumberWidth = ClockFont[11].Width;
            NumberHeight = ClockFont[11].Height;
            for (int n = 0; n < 10; n++) {
                ClockFont[n] = new Bitmap(FontDir + "\\" + n.ToString() + ".png");
                if (ClockFont[n].Width != NumberWidth || ClockFont[n].Height != NumberHeight) {
                    Shared.CurSettings.SpoutEnabled = false;
                    System.Windows.Forms.MessageBox.Show(n.ToString() + ".png is not the same size (" + ClockFont[n].Width.ToString() + "x" + ClockFont[n].Height.ToString() + "px vs " + NumberWidth.ToString() + "x" + NumberHeight.ToString() + ")", "Image size mismatch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            ClockFont[10] = new Bitmap(FontDir + "\\colon.png");
            ColonWidth = ClockFont[10].Width;
            if (ClockFont[10].Height != NumberHeight) {
                Shared.CurSettings.SpoutEnabled = false;
                System.Windows.Forms.MessageBox.Show("colon.png is not the same height (" + ClockFont[10].Width.ToString() + "x" + ClockFont[10].Height.ToString() + "px vs " + NumberWidth.ToString() + "x" + NumberHeight.ToString() + ")", "Image size mismatch", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            ClockWidth = NumberWidth * 4 + ColonWidth;
            ClockArraySize = (ClockWidth * NumberHeight) * 4;

            pClockTexture = Marshal.AllocHGlobal(ClockArraySize);

            OpenGLHandler.InitGL();
            spoutSender = new SpoutSender();
            spoutSender.CreateSender(Shared.CurSettings.SpoutName, (uint)ClockWidth, (uint)NumberHeight, 0);
        }
        
        public unsafe async void UpdateTexture() {

            Int32[] TempLine = new Int32[ColonWidth];
            UInt32 TempPixel;

            for (int y = 0; y < NumberHeight; y++) {
                for (int x = 0; x < ColonWidth; x++) {
                    TempPixel = (UInt32)ClockFont[10].GetPixel(x, y).ToArgb();
                    TempPixel = TempPixel & 0xFF00FF00 | (((TempPixel & 0x00FF0000) >> 16)) | (((TempPixel & 0x000000FF) << 16));
                    TempLine[x] = (Int32)TempPixel;
                }
                Marshal.Copy(TempLine, 0, pClockTexture + (y * ClockWidth * 4) + (((2 * NumberWidth)) * 4), ColonWidth);
            }

            int i = 0;
            GetImageFromTime(Shared.frmClock.SecondsToGo, pClockTexture);
            Console.WriteLine($"Sending (i = {i})");
            spoutSender.SendImage((byte*)pClockTexture, (uint)ClockWidth, (uint)NumberHeight, 6408, false, 0);
            spoutSender.SendImage((byte*)pClockTexture, (uint)ClockWidth, (uint)NumberHeight, 6408, false, 0);
            Thread.Sleep(10); // Delay
            if (i < 2) i++;
            else i = 0;
        }

        public void Dispose() {
            if (spoutSender != null) { 
                spoutSender.Dispose();
                Marshal.FreeHGlobal(pClockTexture);
            }
        }
    }
}
