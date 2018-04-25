﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using ZXing;
using ZXing.QrCode;

namespace V2RayGCon.Lib.QRCode
{
    // 这段代码由 ssr-csharp 魔改而成
    public class QRCode
    {
        static QrCodeEncodingOptions options = new QrCodeEncodingOptions
        {
            DisableECI = true,
            CharacterSet = "UTF-8"
        };

        public static Bitmap GenQRCode(string content, int size = 512)
        {
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            options.Width = size;
            options.Height = size;

            IBarcodeWriter writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = options,
            };

            return new Bitmap(writer.Write(content));
        }

        static Func<Rectangle, Rectangle, int> GenNearCenterCompareFunc(int centerX, int centerY)
        {
            return (a, b) =>
            {
                int distAX = a.X + a.Width / 2 - centerX;
                int distAY = a.Y + a.Height / 2 - centerY;
                int distA = distAX * distAX + distAY * distAY;

                int distBX = b.X + b.Width / 2 - centerX;
                int distBY = b.Y + b.Height / 2 - centerY;
                int distB = distBX * distBX + distBY * distBY;

                return Math.Sign(distA - distB);
            };
        }

        // 默认以屏幕高宽 约（划重点）1/5为移动单位，生成一系列边长为3/5的正方形扫描窗口
        public static List<List<Rectangle>> GenSquareScanWinList(Point srcSize, int parts = 5, int scanSize = 3)
        {
            // center x/y adjustment x/y
            int unit, cx, cy, ax, ay;

            // 注意屏幕可能是竖着的
            unit = Math.Min(srcSize.X, srcSize.Y) / parts;
            ax = (srcSize.X % unit) / (srcSize.X / unit - scanSize); //这就是为什么上面说约1/5
            ay = (srcSize.Y % unit) / (srcSize.Y / unit - scanSize);
            cx = srcSize.X / 2;
            cy = srcSize.Y / 2;

            // 注意最后一个窗口并非和屏幕对齐,但影响不大
            int sx, sy; // start pose x/y
            var winList = new List<Rectangle>();
            for (var row = 0; row <= srcSize.Y / unit - scanSize; row++)
            {
                sy = row * (unit + ay);
                for (var file = 0; file <= srcSize.X / unit - scanSize; file++)
                {
                    sx = file * (unit + ax);
                    winList.Add(new Rectangle(sx, sy, scanSize * unit, scanSize * unit));
                }
            }

            var NearCenterComparer = GenNearCenterCompareFunc(cx, cy);

            winList.Sort((a, b) => NearCenterComparer(a, b));

            // gen <winRect, screeRect>, ...

            var scanList = new List<List<Rectangle>>();
            foreach (var rect in winList)
            {
                scanList.Add(new List<Rectangle> {
                    new Rectangle(0,0,rect.Width,rect.Height),
                    rect
                });
            }
            return scanList;
        }

        public static List<List<Rectangle>> GenZoomScanWinList(Point srcSize, int factor = 5)
        {
            List<List<Rectangle>> scanList = new List<List<Rectangle>>();

            for (var i = 1; i < Math.Max(factor, 3); i++)
            {
                var shrink = 2.8 - Math.Pow(1.0 + 1.0 / i, i);
                scanList.Add(
                    new List<Rectangle> {
                        new Rectangle(0,0,(int)(srcSize.X*shrink),(int)(srcSize.Y*shrink)),
                        new Rectangle(0,0,srcSize.X,srcSize.Y)
                    });
            }
            return scanList;
        }

        static bool ScanScreen(Screen screen, List<List<Rectangle>> scanRectList, Action<string> success)
        {
            using (Bitmap screenshot = new Bitmap(screen.Bounds.Width, screen.Bounds.Height))
            {
                // take a screenshot
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(
                        screen.Bounds.X,
                        screen.Bounds.Y,
                        0,
                        0,
                        screenshot.Size,
                        CopyPixelOperation.SourceCopy);
                }

                for (int i = 0; i < scanRectList.Count; i++)
                {
                    var result = ScanWindow(screenshot, scanRectList[i][0], scanRectList[i][1]);

                    if (result == null)
                    {
                        continue;
                    }

                    var link = result.Text;

                    Debug.WriteLine("Source window {0}: {1}", i, scanRectList[i][1]);
                    Debug.WriteLine("Target window {0}: {1}", i, scanRectList[i][0]);
                    Debug.WriteLine("Read: " + Lib.Utils.CutStr(link, 32));

                    var qrcodeRect = GetQRCodeRect(
                        result,
                        scanRectList[i][0],
                        scanRectList[i][1]);

                    ShowSplashForm(screen, qrcodeRect, () => success(link));
                    return true;
                }
            }

            return false;
        }

        public static void ScanQRCode(Action<string> success, Action fail)
        {
            Thread.Sleep(100);

            foreach (var screen in Screen.AllScreens)
            {
                var scanRectList = GenSquareScanWinList(new Point(
                    screen.Bounds.Width,
                    screen.Bounds.Height));

                scanRectList.AddRange(GenZoomScanWinList(new Point(
                    screen.Bounds.Width,
                    screen.Bounds.Height)));

                if (ScanScreen(screen, scanRectList, success))
                {
                    return;
                }
            }
            fail();
        }

        static Rectangle GetQRCodeRect(Result result, Rectangle winRect, Rectangle screenRect)
        {
            // get qrcode rect
            Point start = new Point(winRect.Width, winRect.Height);
            Point end = new Point(0, 0);

            foreach (var point in result.ResultPoints)
            {
                start.X = Math.Min(start.X, (int)point.X);
                start.Y = Math.Min(start.Y, (int)point.Y);
                end.X = Math.Max(end.X, (int)point.X);
                end.Y = Math.Max(end.Y, (int)point.Y);
            }

            double factor = 1.0 * screenRect.Width / winRect.Width;

            Rectangle qrRect = new Rectangle(
                (int)(start.X * factor + screenRect.X),
                (int)(start.Y * factor + screenRect.Y),
                (int)(factor * (end.X * 1.0 - start.X)),
                (int)(factor * (end.Y * 1.0 - start.Y)));

            Debug.WriteLine("factor: {0}", factor);
            Debug.WriteLine("qrCode: {0}", qrRect);

            return qrRect;
        }

        static void ShowSplashForm(Screen screen, Rectangle rect, Action closed)
        {
            var qrSplash = new QRCodeSplashForm();
            qrSplash.FormClosed += (s, e) => closed();

            qrSplash.Location = new Point(screen.Bounds.X, screen.Bounds.Y);
            // qrSplash.Size = new Size(screen_size.X, screen_size.Y);
            qrSplash.TargetRect = rect;
            qrSplash.Size = screen.Bounds.Size;
            Debug.WriteLine("target: " + qrSplash.TargetRect);
            qrSplash.Show();
        }

        static Result ScanWindow(Bitmap screenshot, Rectangle winRect, Rectangle screenRect)
        {
            using (Bitmap window = new Bitmap(winRect.Width, winRect.Height))
            {
                using (Graphics g = Graphics.FromImage(window))
                {
                    g.DrawImage(screenshot, winRect, screenRect, GraphicsUnit.Pixel);
                }

                var binBMP = new BinaryBitmap(
                    new HybridBinarizer(
                        new BitmapLuminanceSource(window)));

                QRCodeReader reader = new QRCodeReader();

                return reader.decode(binBMP);
            }
        }
    }
}
