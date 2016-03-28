using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Perfor.Lib.Common
{
    /// <summary>
    /// 图片功能类
    /// </summary>
    public class ImageUtility
    {
        /**
         * @ 生成图片的缩略图
         * @ sourcePath 原始图片的路径（包含该文件的文件名）
         * @ width 缩略图的宽度
         * @ height 缩略图的高度
         * @ percentValue 缩略图的百分比值（例如按照 50% 缩放的话，该值就传入 50 即可。）
         * @ mode 缩略图的生成模式
         * */
        public static Bitmap CreateThumbnailImage(string sourcePath, int width, int height, int percentValue, ThumbnailModeEnum mode)
        {
            Bitmap thumbnailBmp = null;
            try
            {
                Image sourceImage = Image.FromFile(sourcePath);


                Bitmap orignalbitmap = new Bitmap(sourceImage);
                Graphics gp = null;
                int sourceWidth, sourceHeight, newWidth = width, newHeight = height;

                sourceWidth = sourceImage.Width;
                sourceHeight = sourceImage.Height;

                bool flag = true;

                switch (mode)
                {
                    case ThumbnailModeEnum.WidthAndHeight:
                        if (newWidth >= sourceWidth && newHeight >= sourceHeight)
                        {
                            flag = false;
                        }
                        else
                        {
                            float widthPer = (float)newWidth / (float)sourceWidth;
                            float heigthPer = (float)newHeight / (float)sourceHeight;

                            if (widthPer <= heigthPer)
                            {
                                newHeight = sourceHeight * newWidth / sourceWidth;
                            }
                            else
                            {
                                newWidth = sourceWidth * newHeight / sourceHeight;
                            }
                        }

                        break;
                    case ThumbnailModeEnum.ByWidth:
                        if (newWidth >= sourceWidth)
                        {
                            flag = false;
                        }
                        newHeight = sourceHeight * newWidth / sourceWidth;
                        break;
                    case ThumbnailModeEnum.ByHeight:
                        if (newHeight >= sourceHeight)
                        {
                            flag = false;
                        }
                        newWidth = sourceWidth * newHeight / sourceHeight;
                        break;
                    case ThumbnailModeEnum.ByPercent:
                        newWidth = sourceWidth * percentValue / 100;
                        newHeight = sourceHeight * percentValue / 100;
                        break;
                }

                thumbnailBmp = new Bitmap(newWidth, newHeight);
                gp = Graphics.FromImage(thumbnailBmp);

                if (flag)
                {
                    gp.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    gp.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    gp.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    gp.Clear(Color.Transparent);

                    gp.DrawImage(orignalbitmap, 0, 0, newWidth, newHeight);
                }
                else
                {
                    thumbnailBmp = orignalbitmap.Clone() as Bitmap;
                }
            }
            catch
            {
            }
            finally
            {
                //if (gp != null)
                //{
                //    gp.Dispose();
                //}

                //if (orignalbitmap != null)
                //{
                //    orignalbitmap.Dispose();
                //}

                //if (sourceImage != null)
                //{
                //    sourceImage.Dispose();
                //}
            }

            return thumbnailBmp;
        }

        /**
         *  回调方法
         * */
        public static bool ThumbnailCallback()
        {
            return false;
        }
    }

    /*
     * @ 缩略图生成样式
     * */
    public enum ThumbnailModeEnum
    {
        /*
         * @ 根据指定的长和宽生成，可能会丢失图片的原始宽高比例
         * */
        WidthAndHeight,
        /**
         * @ 根据指定的宽度生成，高度按照指定的宽度值依据原始宽高比例获得
         * @ 此时忽略指定的高度值。可以保持原始宽高比例
         * */
        ByWidth,
        /***
         * @ 根据指定的高度生成，宽度按照指定的高度值依据原始宽高比例获得
         * @ 此时忽略指定的宽度值。可以保持原始宽高比例
         * */
        ByHeight,
        /**
         * @ 根据指定的百分比来生成，可以保持原始宽高比例
         * */
        ByPercent
    }
}
