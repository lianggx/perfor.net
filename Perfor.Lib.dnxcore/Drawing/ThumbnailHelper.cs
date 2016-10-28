using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Perfor.Lib.Drawing
{
    /// <summary>
    /// 图片功能类
    /// </summary>
    public class ThumbnailHelper
    {
        /// <summary>
        ///  生成图片的缩略图
        /// </summary>
        /// <param name="sourcePath">原始图片的路径（包含该文件的文件名）</param>
        /// <param name="mode">缩略图的生成模式</param>
        /// <param name="width">缩略图的宽度</param>
        /// <param name="height">缩略图的高度</param>
        /// <param name="percentValue">缩略图的百分比值（例如按照 50% 缩放的话，该值就传入 50 即可。）</param>
        /// <returns></returns>
        public static Bitmap Create(string sourcePath, ThumbnailModeEnum mode, int width, int height, int percentValue)
        {
            bool flag = true;
            Bitmap thumbnailBmp = null;
            Graphics grap = null;
            Bitmap orignalbitmap = null;
            Image sourceImage = null;
            try
            {
                sourceImage = Image.FromFile(sourcePath);
                orignalbitmap = new Bitmap(sourceImage);
                int sourceWidth, sourceHeight, newWidth = width, newHeight = height;
                sourceWidth = sourceImage.Width;
                sourceHeight = sourceImage.Height;

                switch (mode)
                {
                    case ThumbnailModeEnum.WidthAndHeight:
                        if (newWidth >= sourceWidth && newHeight >= sourceHeight)
                            flag = false;
                        else
                        {
                            float widthPer = (float)newWidth / (float)sourceWidth;
                            float heigthPer = (float)newHeight / (float)sourceHeight;

                            if (widthPer <= heigthPer)
                                newHeight = sourceHeight * newWidth / sourceWidth;
                            else
                                newWidth = sourceWidth * newHeight / sourceHeight;
                        }
                        break;
                    case ThumbnailModeEnum.ByWidth:
                        if (newWidth >= sourceWidth)
                            flag = false;
                        newHeight = sourceHeight * newWidth / sourceWidth;
                        break;
                    case ThumbnailModeEnum.ByHeight:
                        if (newHeight >= sourceHeight)
                            flag = false;
                        newWidth = sourceWidth * newHeight / sourceHeight;
                        break;
                    case ThumbnailModeEnum.ByPercent:
                        newWidth = sourceWidth * percentValue / 100;
                        newHeight = sourceHeight * percentValue / 100;
                        break;
                }

                thumbnailBmp = new Bitmap(newWidth, newHeight);
                grap = Graphics.FromImage(thumbnailBmp);

                if (flag)
                {
                    grap.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    grap.SmoothingMode = SmoothingMode.HighQuality;
                    grap.CompositingQuality = CompositingQuality.HighQuality;
                    grap.Clear(Color.Transparent);

                    grap.DrawImage(orignalbitmap, 0, 0, newWidth, newHeight);
                }
                else
                {
                    thumbnailBmp = orignalbitmap.Clone() as Bitmap;
                }
            }
            catch { }
            finally
            {
                if (grap != null) grap.Dispose();
                if (orignalbitmap != null) orignalbitmap.Dispose();
                if (sourceImage != null) sourceImage.Dispose();
            }

            return thumbnailBmp;
        }

        /// <summary>
        ///  图像转换为base64字符串
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static string BitmapToBase64(Image image)
        {
            byte[] data = null;
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, image.RawFormat);
                data = ms.ToArray();
                ms.Flush();
            }
            string base64 = Convert.ToBase64String(data);
            return base64;
        }

        /// <summary>
        ///  转换base64字符串为图像
        /// </summary>
        /// <param name="base64Data"></param>
        /// <returns></returns>
        public static Bitmap BitmapFromBase64(string base64Data)
        {
            Bitmap image = null;
            byte[] data = Convert.FromBase64String(base64Data);
            using (MemoryStream ms = new MemoryStream(data))
            {
                image = (Bitmap)Bitmap.FromStream(ms);
                ms.Flush();
            }

            return image;
        }
    }
}
