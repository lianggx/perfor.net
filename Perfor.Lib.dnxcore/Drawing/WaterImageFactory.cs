using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;

namespace dywebsdk.Drawing
{
    /// <summary>
    ///  水印图片的操作管理
    /// </summary>
    public class WaterImageFactory
    {
        /// <summary>
        /// 添加图片水印
        /// </summary>
        /// <param name="sourcePicture">源图片文件名</param>
        /// <param name="waterPicture">水印图片文件名</param>
        /// <param name="alpha">透明度(0.1-1.0数值越小透明度越高)</param>
        /// <param name="position">位置</param>
        /// <param name="PicturePath" >图片的路径</param>
        /// <returns>返回生成于指定文件夹下的水印文件名</returns>
        public static Bitmap DrawImage(string sourcePicture, string waterPicture, float alpha, ImagePosition position)
        {
            if (!File.Exists(sourcePicture))
            {
                throw new FileNotFoundException(sourcePicture);
            }
            if (!File.Exists(waterPicture))
            {
                throw new FileNotFoundException(waterPicture);
            }

            //
            // 源图片，水印图片全路径
            //
            string sourcePictureName = sourcePicture;
            string waterPictureName = waterPicture;
            string fileSourceExtension = System.IO.Path.GetExtension(sourcePictureName).ToLower();
            string fileWaterExtension = System.IO.Path.GetExtension(waterPictureName).ToLower();
            //
            // 判断文件是否存在,以及类型是否正确
            //
            if ((fileSourceExtension != ".gif" &&
                fileSourceExtension != ".jpg" &&
                fileSourceExtension != ".jpeg" &&
                fileSourceExtension != ".png" &&
                fileWaterExtension != ".bmp") ||
                (fileWaterExtension != ".gif" &&
                fileWaterExtension != ".jpg" &&
                fileWaterExtension != ".jpeg" &&
                fileWaterExtension != ".png" &&
                fileWaterExtension != ".bmp"))
            {
                throw new ArgumentException("图片的扩展名仅支持.gif,.jpg,.jpeg,.png,.bmp");
            }

            //
            // 将需要加上水印的图片装载到Image对象中
            //
            Image imgPhoto = Image.FromFile(sourcePicture);
            //
            // 确定其长宽
            //
            int phWidth = imgPhoto.Width;
            int phHeight = imgPhoto.Height;

            //
            // 封装 GDI+ 位图，此位图由图形图像及其属性的像素数据组成。
            //
            Bitmap bmPhoto = new Bitmap(phWidth, phHeight, PixelFormat.Format24bppRgb);

            //
            // 设定分辨率
            // 
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            //
            // 定义一个绘图画面用来装载位图
            //
            Graphics grPhoto = Graphics.FromImage(bmPhoto);

            //
            //同样，由于水印是图片，我们也需要定义一个Image来装载它
            //
            Image imgWatermark = new Bitmap(waterPictureName);

            //
            // 获取水印图片的高度和宽度
            //
            int wmWidth = imgWatermark.Width;
            int wmHeight = imgWatermark.Height;

            //SmoothingMode：指定是否将平滑处理（消除锯齿）应用于直线、曲线和已填充区域的边缘。
            // 成员名称   说明 
            // AntiAlias      指定消除锯齿的呈现。  
            // Default        指定不消除锯齿。  
            // HighQuality  指定高质量、低速度呈现。  
            // HighSpeed   指定高速度、低质量呈现。  
            // Invalid        指定一个无效模式。  
            // None          指定不消除锯齿。 
            grPhoto.SmoothingMode = SmoothingMode.AntiAlias;

            //
            // 第一次描绘，将我们的底图描绘在绘图画面上
            //
            grPhoto.DrawImage(imgPhoto, new Rectangle(0, 0, phWidth, phHeight), 0, 0, phWidth, phHeight, GraphicsUnit.Pixel);

            //
            // 与底图一样，我们需要一个位图来装载水印图片。并设定其分辨率
            //
            Bitmap bmWatermark = new Bitmap(bmPhoto);
            bmWatermark.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            //
            // 继续，将水印图片装载到一个绘图画面grWatermark
            //
            Graphics grWatermark = Graphics.FromImage(bmWatermark);

            //
            //ImageAttributes 对象包含有关在呈现时如何操作位图和图元文件颜色的信息。
            //       
            ImageAttributes imageAttributes = new ImageAttributes();

            //
            //Colormap: 定义转换颜色的映射
            //
            ColorMap colorMap = new ColorMap();

            //
            //我的水印图被定义成拥有绿色背景色的图片被替换成透明
            //
            colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
            colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);

            ColorMap[] remapTable = { colorMap };

            imageAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

            float[][] colorMatrixElements = {
           new float[] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f}, // red红色
           new float[] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f}, //green绿色
           new float[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f}, //blue蓝色       
           new float[] {0.0f,  0.0f,  0.0f,  alpha, 0.0f}, //透明度     
           new float[] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}};//

            //  ColorMatrix:定义包含 RGBA 空间坐标的 5 x 5 矩阵。
            //  ImageAttributes 类的若干方法通过使用颜色矩阵调整图像颜色。
            ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);


            imageAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default,
             ColorAdjustType.Bitmap);

            //
            //上面设置完颜色，下面开始设置位置
            //
            int xPosOfWm;
            int yPosOfWm;

            switch (position)
            {
                case ImagePosition.BottomMiddle:
                    xPosOfWm = (phWidth - wmWidth) / 2;
                    yPosOfWm = phHeight - wmHeight - 10;
                    break;
                case ImagePosition.Center:
                    xPosOfWm = (phWidth - wmWidth) / 2;
                    yPosOfWm = (phHeight - wmHeight) / 2;
                    break;
                case ImagePosition.LeftBottom:
                    xPosOfWm = 10;
                    yPosOfWm = phHeight - wmHeight - 10;
                    break;
                case ImagePosition.LeftTop:
                    xPosOfWm = 10;
                    yPosOfWm = 10;
                    break;
                case ImagePosition.RightTop:
                    xPosOfWm = phWidth - wmWidth - 10;
                    yPosOfWm = 10;
                    break;
                case ImagePosition.RigthBottom:
                    xPosOfWm = phWidth - wmWidth - 10;
                    yPosOfWm = phHeight - wmHeight - 10;
                    break;
                case ImagePosition.TopMiddle:
                    xPosOfWm = (phWidth - wmWidth) / 2;
                    yPosOfWm = 10;
                    break;
                default:
                    xPosOfWm = 10;
                    yPosOfWm = phHeight - wmHeight - 10;
                    break;
            }

            //
            // 第二次绘图，把水印印上去
            //
            grWatermark.DrawImage(imgWatermark,
             new Rectangle(xPosOfWm, yPosOfWm, wmWidth, wmHeight), 0, 0, wmWidth, wmHeight, GraphicsUnit.Pixel, imageAttributes);

            imgPhoto = bmWatermark;
            grPhoto.Dispose();
            grWatermark.Dispose();

            //
            // 保存文件到服务器的文件夹里面
            //
            Bitmap bitmap = (Bitmap)imgPhoto;
            imgWatermark.Dispose();

            return bitmap;
        }

        /// <summary>
        /// 在图片上添加水印文字
        /// </summary>
        /// <param name="sourcePicture">源图片文件</param>
        /// <param name="waterWords">需要添加到图片上的文字</param>
        /// <param name="alpha">透明度</param>
        /// <param name="position">位置</param>
        /// <param name="PicturePath">文件路径</param>
        /// <returns></returns>
        public static Bitmap DrawWords(string sourcePicture, string waterWords, float alpha, ImagePosition position)
        {
            //
            // 判断参数是否有效
            //
            if (sourcePicture == string.Empty || waterWords == string.Empty || alpha == 0.0)
            {
                throw new ArgumentNullException(string.Format($"{sourcePicture },{ waterWords},{ alpha}"));
            }

            //
            // 源图片全路径
            //
            string sourcePictureName = sourcePicture;
            string fileExtension = System.IO.Path.GetExtension(sourcePictureName).ToLower();

            //
            // 判断文件是否存在,以及文件名是否正确
            //
            if (System.IO.File.Exists(sourcePictureName) == false || (
                fileExtension != ".gif" &&
                 fileExtension != ".jpeg" &&
                fileExtension != ".jpg" &&
                fileExtension != ".png" &&
                 fileExtension != ".bmp"))
            {
                throw new ArgumentException("图片的扩展名仅支持.gif,.jpg,.jpeg,.png,.bmp");
            }

            //创建一个图片对象用来装载要被添加水印的图片
            Image imgPhoto = Image.FromFile(sourcePictureName);

            //获取图片的宽和高
            int phWidth = imgPhoto.Width;
            int phHeight = imgPhoto.Height;

            //
            //建立一个bitmap，和我们需要加水印的图片一样大小
            Bitmap bmPhoto = new Bitmap(phWidth, phHeight, PixelFormat.Format24bppRgb);

            //SetResolution：设置此 Bitmap 的分辨率
            //这里直接将我们需要添加水印的图片的分辨率赋给了bitmap
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            //Graphics：封装一个 GDI+ 绘图图面。
            Graphics grPhoto = Graphics.FromImage(bmPhoto);

            //设置图形的品质
            grPhoto.SmoothingMode = SmoothingMode.AntiAlias;

            //将我们要添加水印的图片按照原始大小描绘（复制）到图形中
            grPhoto.DrawImage(
             imgPhoto,                                           //   要添加水印的图片
             new Rectangle(0, 0, phWidth, phHeight), //  根据要添加的水印图片的宽和高
             0,                                                     //  X方向从0点开始描绘
             0,                                                     // Y方向 
             phWidth,                                            //  X方向描绘长度
             phHeight,                                           //  Y方向描绘长度
             GraphicsUnit.Pixel);                              // 描绘的单位，这里用的是像素

            //根据图片的大小我们来确定添加上去的文字的大小
            //在这里我们定义一个数组来确定
            int[] sizes = new int[] { 16, 14, 12, 10, 8, 6, 4 };

            //字体
            Font crFont = null;
            //矩形的宽度和高度，SizeF有三个属性，分别为Height高，width宽，IsEmpty是否为空
            SizeF crSize = new SizeF();

            //利用一个循环语句来选择我们要添加文字的型号
            //直到它的长度比图片的宽度小
            for (int i = 0; i < 7; i++)
            {
                crFont = new Font("arial", sizes[i], FontStyle.Bold);

                //测量用指定的 Font 对象绘制并用指定的 StringFormat 对象格式化的指定字符串。
                crSize = grPhoto.MeasureString(waterWords, crFont);

                // ushort 关键字表示一种整数数据类型
                if ((ushort)crSize.Width < (ushort)phWidth)
                    break;
            }

            //截边5%的距离，定义文字显示(由于不同的图片显示的高和宽不同，所以按百分比截取)
            int yPixlesFromBottom = (int)(phHeight * .05);

            //定义在图片上文字的位置
            float wmHeight = crSize.Height;
            float wmWidth = crSize.Width;

            float xPosOfWm;
            float yPosOfWm;

            switch (position)
            {
                case ImagePosition.BottomMiddle:
                    xPosOfWm = phWidth / 2;
                    yPosOfWm = phHeight - wmHeight - 10;
                    break;
                case ImagePosition.Center:
                    xPosOfWm = phWidth / 2;
                    yPosOfWm = phHeight / 2;
                    break;
                case ImagePosition.LeftBottom:
                    xPosOfWm = wmWidth;
                    yPosOfWm = phHeight - wmHeight - 10;
                    break;
                case ImagePosition.LeftTop:
                    xPosOfWm = wmWidth / 2;
                    yPosOfWm = wmHeight / 2;
                    break;
                case ImagePosition.RightTop:
                    xPosOfWm = phWidth - wmWidth - 10;
                    yPosOfWm = wmHeight;
                    break;
                case ImagePosition.RigthBottom:
                    xPosOfWm = phWidth - wmWidth - 10;
                    yPosOfWm = phHeight - wmHeight - 10;
                    break;
                case ImagePosition.TopMiddle:
                    xPosOfWm = phWidth / 2;
                    yPosOfWm = wmWidth;
                    break;
                default:
                    xPosOfWm = wmWidth;
                    yPosOfWm = phHeight - wmHeight - 10;
                    break;
            }

            //封装文本布局信息（如对齐、文字方向和 Tab 停靠位），显示操作（如省略号插入和国家标准 (National) 数字替换）和 OpenType 功能。
            StringFormat StrFormat = new StringFormat();

            //定义需要印的文字居中对齐
            StrFormat.Alignment = StringAlignment.Center;

            //SolidBrush:定义单色画笔。画笔用于填充图形形状，如矩形、椭圆、扇形、多边形和封闭路径。
            //这个画笔为描绘阴影的画笔，呈灰色
            int m_alpha = Convert.ToInt32(256 * alpha);
            SolidBrush semiTransBrush2 = new SolidBrush(Color.FromArgb(m_alpha, 0, 0, 0));

            //描绘文字信息，这个图层向右和向下偏移一个像素，表示阴影效果
            //DrawString 在指定矩形并且用指定的 Brush 和 Font 对象绘制指定的文本字符串。
            grPhoto.DrawString(waterWords,                                    //string of text
                                       crFont,                                         //font
                                       semiTransBrush2,                            //Brush
                                       new PointF(xPosOfWm + 1, yPosOfWm + 1),  //Position
                                       StrFormat);

            //从四个 ARGB 分量（alpha、红色、绿色和蓝色）值创建 Color 结构，这里设置透明度为153
            //这个画笔为描绘正式文字的笔刷，呈白色
            SolidBrush semiTransBrush = new SolidBrush(Color.FromArgb(153, 255, 255, 255));

            //第二次绘制这个图形，建立在第一次描绘的基础上
            grPhoto.DrawString(waterWords,                 //string of text
                                       crFont,                                   //font
                                       semiTransBrush,                           //Brush
                                       new PointF(xPosOfWm, yPosOfWm),  //Position
                                       StrFormat);

            //imgPhoto是我们建立的用来装载最终图形的Image对象
            //bmPhoto是我们用来制作图形的容器，为Bitmap对象
            imgPhoto = bmPhoto;
            //释放资源，将定义的Graphics实例grPhoto释放，grPhoto功德圆满
            grPhoto.Dispose();

            //
            // 保存文件到服务器的文件夹里面
            //
            Bitmap bitmap = (Bitmap)imgPhoto;
            return bitmap;
        }
    }
}