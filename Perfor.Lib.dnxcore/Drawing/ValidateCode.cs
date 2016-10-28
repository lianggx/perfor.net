using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Perfor.Lib.Drawing
{
    /// <summary>
    ///  输出验证图片
    /// </summary>
    public partial class ValidateCode
    {
        #region Identity
        // 验证码长度
        private static int codeLen = 4;
        private static CodeStyle codestyle = CodeStyle.Number;
        // 图片清晰度/宽度/高度
        private static int fineness = 100, imageWidth = 100, imageHeight = 24;
        // 字体大小/样式
        private static int[] fontSize = { 18, 20, 22 };
        private const string ENChar = "ABCDEFGHJKMNPQRSTUVWXYZ";
        private const string ENCharNumber = "23456789ABCDEFGH23456789JKMNPQRS23456789TUVWXYZ";
        private static Brush[] brus = { Brushes.Blue, Brushes.Black, Brushes.DarkRed, Brushes.OrangeRed };
        private static string[] fonts = { "BiauKai", "STHeiti", "SimSun", "STCaiyun" };
        private static FontStyle[] fontStyles = { FontStyle.Bold, FontStyle.Italic, FontStyle.Regular, FontStyle.Strikeout, FontStyle.Underline, FontStyle.Bold | FontStyle.Italic };
        #endregion

        /// <summary>
        ///  生成验证图片
        /// </summary>
        /// <param name="count">验证码长度4-16</param>
        /// <param name="width">图片宽度16-360</param>
        /// <param name="height">图片高度16-320</param>
        /// <param name="style">验证码样式</param>
        /// <param name="code">返回的验证码</param>
        /// <returns></returns>
        public static byte[] Create(int count, int width, int height, CodeStyle style, out string code)
        {
            codeLen = count;
            imageWidth = width;
            imageHeight = height;
            codestyle = style;
            code = string.Empty;
            return Create(out code);
        }

        /// <summary>
        ///  指定要生成的验证码类型，然后使用默认的设置生成验证码
        /// </summary>
        /// <param name="codeStyle">验证码类型，0英文字母，1中文字符，3数字</param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static byte[] Create(CodeStyle codeStyle, out string code)
        {
            codestyle = codeStyle;
            code = string.Empty;
            return Create(out code);
        }

        /// <summary>
        ///  使用默认的设置生成验证码
        /// </summary>
        /// <returns></returns>
        public static byte[] Create(out string code)
        {
            byte[] data = null;
            code = CreateCode();
            using (Bitmap bitmap = new Bitmap(imageWidth, imageHeight))
            {

                // 绘制验证码图像
                DrawCode(bitmap, code);
                // 给图像设置干扰
                DisturbBitmap(bitmap);
                MemoryStream ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Jpeg);
                data = ms.ToArray();
            }
            return data;
        }

        /// <summary>
        ///  开始生成
        /// </summary>
        /// <returns></returns>
        private static string CreateCode()
        {
            string code = string.Empty;
            Random random = new Random();

            if (codestyle == CodeStyle.Char)
            {
                for (int i = 0; i < codeLen; i++)
                {
                    int n = random.Next(ENChar.Length);
                    code += ENChar[n];
                }
            }
            else if (codestyle == CodeStyle.Zh_cn)
            {
                code = CodeRandom.GetRand_CN(4);
            }
            else if (codestyle == CodeStyle.Number)
            {
                code += CodeRandom.GetRandom(0, 9, codeLen);
            }
            else if (codestyle == (CodeStyle.Char | CodeStyle.Number))
            {
                for (int i = 0; i < codeLen; i++)
                {
                    int n = random.Next(ENCharNumber.Length);
                    code += ENCharNumber[n];
                }
            }

            return code;
        }

        /// <summary>
        ///  为图片设置干扰点
        /// </summary>
        /// <param name="bitmap"></param>
        private static void DisturbBitmap(Bitmap bitmap)
        {
            Random random = new Random();
            for (int i = 0; i < bitmap.Width;)
            {
                for (int j = 0; j < bitmap.Height;)
                {
                    bitmap.SetPixel(i, j, Color.FromArgb(120, random.Next(128), random.Next(128), random.Next(128)));
                    j += random.Next(20);
                }
                i += random.Next(8);
            }
        }

        /// <summary>
        ///  绘制验证码图片
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="code"></param>
        private static void DrawCode(Bitmap bitmap, string code)
        {
            //随机画两条线
            Random r = new Random();
            Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);

            Pen line_1 = new Pen(brus[r.Next(brus.Length)], r.Next(3));
            Pen line_2 = new Pen(brus[r.Next(brus.Length)], r.Next(3));
            float x1 = r.Next(imageWidth / 2);
            float x2 = r.Next(imageWidth);
            float y1 = r.Next(imageHeight / 2);
            float y2 = r.Next(imageHeight);
            g.DrawLine(line_1, x1, y1, x2, y2);
            x1 = r.Next(imageWidth);
            x2 = r.Next(imageWidth);
            y1 = r.Next(imageHeight);
            y2 = r.Next(imageHeight);
            g.DrawLine(line_2, x1, y1, x2, y2);
            // 绘制验证码图像
            int posX = 0;
            int posY = 0;
            for (int i = 0; i < code.Length; i++)
            {
                int fs = fontSize[r.Next(fontSize.Length)];
                posY = r.Next(imageHeight - fs - 5);
                Font font = new Font(fonts[r.Next(fonts.Length)], fs, fontStyles[r.Next(fontStyles.Length)]);
                string c = code[i].ToString();
                g.DrawString(c, font, brus[r.Next(brus.Length)], posX, posY);
                posX += fs;
            }
        }
    }
}
