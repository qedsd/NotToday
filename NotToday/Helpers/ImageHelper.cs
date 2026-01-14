using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;

namespace NotToday.Helpers
{
    public static class ImageHelper
    {
        public static BitmapImage ImageConvertToBitmapImage(Image img)
        {
            var bitmapImage = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Png);
                stream.Position = 0;

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }
        public static Bitmap ImageToBitmap(Image source, Rectangle destinationRect)
        {
            var destinationImage = new Bitmap(destinationRect.Width, destinationRect.Height);
            destinationImage.SetResolution(source.HorizontalResolution, source.VerticalResolution);
            using (var graphics = Graphics.FromImage(destinationImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.DrawImage(source, new Rectangle(0, 0, destinationImage.Width, destinationImage.Height), destinationRect, GraphicsUnit.Pixel);
            }
            return destinationImage;
        }
        public static Bitmap ImageToBitmap(Image source)
        {
            Rectangle destinationRect = new Rectangle(0,0,source.Width, source.Height);
            return ImageToBitmap(source, destinationRect);
        }
        public static WriteableBitmap BitmapConvertToWriteableBitmap(Bitmap img)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                img.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();

                return new WriteableBitmap(bitmapImage);
            }
        }
        public static WriteableBitmap BitmapConvertToWriteableBitmap(Image img)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                img.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();

                return new WriteableBitmap(bitmapImage);
            }
        }
        public static WriteableBitmap ConvertBitmapToWriteableBitmapDirect(Bitmap bitmap)
        {
            if (bitmap == null) return null;

            int width = bitmap.Width;
            int height = bitmap.Height;

            // 创建WriteableBitmap，使用Bgra32格式（与32位ARGB Bitmap兼容）
            WriteableBitmap writeableBitmap = new WriteableBitmap(
                width,
                height,
                96,  // DPI X
                96,  // DPI Y
                PixelFormats.Bgra32,  // 对应Bitmap的Format32bppArgb
                null);

            // 锁定Bitmap获取数据
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                // 锁定WriteableBitmap进行写入
                writeableBitmap.Lock();

                // 计算数据大小
                int bufferSize = bitmapData.Height * bitmapData.Stride;

                // 使用CopyMemory进行快速内存复制
                CopyMemory(
                    writeableBitmap.BackBuffer,
                    bitmapData.Scan0,
                    (uint)bufferSize);

                // 指定更新区域（整个图像）
                writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            }
            finally
            {
                // 重要：必须解锁
                writeableBitmap.Unlock();
                bitmap.UnlockBits(bitmapData);
            }

            return writeableBitmap;
        }
        // 内存复制辅助方法
        [DllImport("kernel32.dll", EntryPoint = "RtlCopyMemory")]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint length);
    }
}
