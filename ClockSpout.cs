//using Silk.NET.Core.Contexts;
//using Silk.NET.Core.Loader;
//using TerraFX.Interop.Windows;
//using static TerraFX.Interop.Windows.Windows;
using Spout.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
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
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;




namespace StreamStartingTimer {

    public class ClockSpout : IDisposable {
        static List<Byte[]> ClockFont;
        static int NumberWidth;
        static int NumberHeight;
        static int ColonWidth;
        static int ClockWidth;
        static int ClockArraySize;
        static SpoutSender spoutSender;
        static readonly System.Drawing.Imaging.PixelFormat TexturePixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
        static nint pClockTexture;
        static string PrevTime = "xx:xx";

        public void GetImageFromTime(uint SecondsToGo, nint pClockTexture) {
            string CurTime;
            int CurDigit;
            int ColonFix;
            int y;

            CurTime = TimeSpan.FromSeconds(SecondsToGo).ToString(Shared.TimeFormat);

            ColonFix = 0;

            for (int Digit = 0; Digit < 5; Digit++) {
                if (Digit == 2) {
                    ColonFix = ColonWidth - NumberWidth;
                } else if (CurTime[Digit] != PrevTime[Digit]) {
                    if (!Shared.CurSettings.ShowLeadingZero && (Digit == 0) && (CurTime[Digit] == '0')) {
                        CurDigit = 10;
                    } else {
                        CurDigit = CurTime[Digit] - 48;
                        if (CurDigit < 0 && CurDigit > 9) { throw new Exception("Digit is not 0-9 or space"); }
                    }
                    for (y = 0; y < NumberHeight; y++) {
                        if (Shared.CurSettings.Debug) {
                            SendImage();
                            Thread.Sleep(5);
                            Marshal.Copy(ClockFont[10], y * NumberWidth * 4, pClockTexture + ((y * ClockWidth * 4) + ((Digit * NumberWidth) + ColonFix) * 4), NumberWidth * 4);
                            SendImage();
                            Thread.Sleep(5);
                        }
                        Marshal.Copy(ClockFont[CurDigit], y * NumberWidth * 4, pClockTexture + ((y * ClockWidth * 4) + ((Digit * NumberWidth) + ColonFix) * 4), NumberWidth * 4);
                    }
                }
            }
            PrevTime = CurTime;
        }

        private byte[] ConvertImage(Bitmap TempClockFont, string CharName, bool CheckWidth) {
            if ((CheckWidth && TempClockFont.Width != NumberWidth) || TempClockFont.Height != NumberHeight) {
                Shared.CurSettings.SpoutEnabled = false;
                System.Windows.Forms.MessageBox.Show(CharName + ".png is not the same size (" + TempClockFont.Width.ToString() + "x" + TempClockFont.Height.ToString() + "px vs " + NumberWidth.ToString() + "x" + NumberHeight.ToString() + ")", "Image size mismatch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            } else {
                byte[] ReturnImage = new byte[TempClockFont.Width * TempClockFont.Height * 4];
                int n = 0;
                Color TempPixel;
                for (int y = 0; y < TempClockFont.Height; y++) {
                    for (int x = 0; x < TempClockFont.Width; x++) {
                        TempPixel = TempClockFont.GetPixel(x, y);
                        ReturnImage[n] = TempPixel.R;
                        ReturnImage[n + 1] = TempPixel.G;
                        ReturnImage[n + 2] = TempPixel.B;
                        ReturnImage[n + 3] = TempPixel.A;
                        n += 4;
                    }
                }
                return ReturnImage;
            }
        }

        public unsafe ClockSpout(string FontDir) {
            int n;
            Bitmap TempClockFont;
            string MissingFiles = "";
            for (n = 0; n < 10; n++) {
                if (!File.Exists(FontDir + "\\" + n.ToString() + ".png")) {
                    MissingFiles += n.ToString() + ".png, ";
                }
            }
            if (!File.Exists(FontDir + "\\colon.png")) {
                MissingFiles += "colon.png, ";
            }
            if (MissingFiles != "") {
                MessageBox.Show("Some or all of the clock font images are missing\n" + MissingFiles + "\nDisabling Spout2 output", "Files Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Shared.CurSettings.SpoutEnabled = false;
                return;
            }
            TempClockFont = new Bitmap(FontDir + "\\0.png");
            NumberWidth = TempClockFont.Width;
            NumberHeight = TempClockFont.Height;
            ClockFont = new List<Byte[]>();
            ClockFont.Add(ConvertImage(TempClockFont, "0", false));
            for (n = 1; n < 10; n++) {
                TempClockFont = new Bitmap(FontDir + "\\" + n.ToString() + ".png");
                ClockFont.Add(ConvertImage(TempClockFont, n.ToString(), true));
            }
            if (File.Exists(FontDir + "\\" + "space.png")) { 
                TempClockFont = new Bitmap(FontDir + "\\" + "space.png");
                ClockFont.Add(ConvertImage(TempClockFont, "space", true));
            } else {
                ClockFont.Add(new byte[NumberWidth * NumberHeight * 4]);
            }
            TempClockFont = new Bitmap(FontDir + "\\colon.png");
            ColonWidth = TempClockFont.Width;
            ClockWidth = NumberWidth * 4 + ColonWidth;
            ClockArraySize = (ClockWidth * NumberHeight) * 4;

            pClockTexture = Marshal.AllocHGlobal(ClockArraySize);
            Byte[] Colon = new byte[ClockArraySize];
            Colon = ConvertImage(TempClockFont, "colon", false);

            spoutSender = new SpoutSender();
            spoutSender.CreateSender(Shared.CurSettings.SpoutName, (uint)ClockWidth, (uint)NumberHeight, 0);

            // Add the : to the clock texture now. It never needs copying again
            for (int y = 0; y < NumberHeight; y++) {
                spoutSender.SendImage((byte*)pClockTexture, (uint)ClockWidth, (uint)NumberHeight, 6408, false, 0);
                Marshal.Copy(Colon, y*ColonWidth*4, pClockTexture + (y * ClockWidth * 4) + (((2 * NumberWidth)) * 4), ColonWidth*4);
            }

        }
        
        public void UpdateTexture() {

            int i = 0;
            GetImageFromTime(Shared.SecondsToGo, pClockTexture);
            Console.WriteLine($"Sending (i = {i})");
            
            SendImage();
            if (i < 2) i++;
            else i = 0;
        }

        private unsafe void SendImage() {
            spoutSender.SendImage((byte*)pClockTexture, (uint)ClockWidth, (uint)NumberHeight, 6408, false, 0);
            if (Shared.CurSettings.DoubleFrames) { spoutSender.SendImage((byte*)pClockTexture, (uint)ClockWidth, (uint)NumberHeight, 6408, false, 0); }
        }

        public void Dispose() {
            if (spoutSender != null) {
                spoutSender.ReleaseSender();
                spoutSender.Dispose();
                spoutSender = null;
                Marshal.FreeHGlobal(pClockTexture);
                PrevTime = "xx:xx";
            }
        }
    }
}
