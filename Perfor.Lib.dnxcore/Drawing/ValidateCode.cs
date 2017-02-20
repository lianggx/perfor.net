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
        // 验证码扭曲
        public static int TWIST = 0;
        // 验证码长度
        private static int CODELEN = 6;
        private static CodeStyle codestyle = CodeStyle.Number;
        // 图片清晰度/宽度/高度
        private static int FINENESS = 100, IMAGE_WIDTH = 100, IMAGE_HEIGHT = 24;
        // 字体大小/样式
        private static int[] fontSize = { 16, 18, 20 };
        private const string EN_CHAR = "ABCDEFGHJKMNPQRSTUVWXYZ";
        private const string EN_CHAR_NUMBER = "23456789ABCDEFGH23456789JKMNPQRS23456789TUVWXYZ";
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
            CODELEN = count;
            IMAGE_WIDTH = width;
            IMAGE_HEIGHT = height;
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
            code = CreateCode();
            byte[] data = CreateBitMap(code);
            return data;
        }

        /// <summary>
        ///  使用默认的设置生成验证码
        /// </summary>
        /// <returns></returns>
        public static byte[] CreateBitMap(string code)
        {
            byte[] data = null;
            IMAGE_WIDTH = code.Length * fontSize[1] + (code.Length * 3);
            using (Bitmap bitmap = new Bitmap(IMAGE_WIDTH, IMAGE_HEIGHT))
            {
                // 绘制验证码图像
                DrawCode(bitmap, code);
                // 给图像设置干扰
                DisturbBitmap(bitmap);
                // 扭曲
                Bitmap b2 = TwistImage(bitmap, true, TWIST, 0);
                MemoryStream ms = new MemoryStream();
                b2.Save(ms, ImageFormat.Jpeg);
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
                for (int i = 0; i < CODELEN; i++)
                {
                    int n = random.Next(EN_CHAR.Length);
                    code += EN_CHAR[n];
                }
            }
            else if (codestyle == CodeStyle.Zh_cn)
            {
                code = CodeRandom.GetRand_CN(4);
            }
            else if (codestyle == CodeStyle.Number)
            {
                code += CodeRandom.GetRandom(0, 9, CODELEN);
            }
            else if (codestyle == (CodeStyle.Char | CodeStyle.Number))
            {
                for (int i = 0; i < CODELEN; i++)
                {
                    int n = random.Next(EN_CHAR_NUMBER.Length);
                    code += EN_CHAR_NUMBER[n];
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
        /// 正弦曲线Wave扭曲图片
        /// </summary>
        /// <param name="srcBmp">源图片</param>
        /// <param name="bXDir">如果扭曲则选择为True</param>
        /// <param name="dMultValue">波形的幅度倍数，越大扭曲的程度越高，一般为3</param>
        /// <param name="dPhase">波形的起始相位，取值区间[0-2*PI)</param>
        /// <returns></returns>
        private static Bitmap TwistImage(Bitmap srcBmp, bool bXDir, double dMultValue, double dPhase)
        {
            Bitmap destBmp = new Bitmap(srcBmp.Width, srcBmp.Height);
            // 将位图背景填充为白色
            System.Drawing.Graphics graph = System.Drawing.Graphics.FromImage(destBmp);
            graph.FillRectangle(new SolidBrush(System.Drawing.Color.White), 0, 0, destBmp.Width, destBmp.Height);
            graph.Dispose();

            double dBaseAxisLen = bXDir ? (double)destBmp.Height : (double)destBmp.Width;

            for (int i = 0; i < destBmp.Width; i++)
            {
                for (int j = 0; j < destBmp.Height; j++)
                {
                    double dx = 0;

                    dx = bXDir ? (Math.PI * (double)j) / dBaseAxisLen : (Math.PI * (double)i) / dBaseAxisLen;
                    dx += dPhase;
                    double dy = Math.Sin(dx);

                    // 取得当前点的颜色
                    int nOldX = 0, nOldY = 0;
                    nOldX = bXDir ? i + (int)(dy * dMultValue) : i;
                    nOldY = bXDir ? j : j + (int)(dy * dMultValue);

                    System.Drawing.Color color = srcBmp.GetPixel(i, j);
                    if (nOldX >= 0 && nOldX < destBmp.Width
                     && nOldY >= 0 && nOldY < destBmp.Height)
                    {
                        destBmp.SetPixel(nOldX, nOldY, color);
                    }
                }
            }

            return destBmp;
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

            Pen line_1 = new Pen(brus[r.Next(brus.Length - 1)], r.Next(3));
            Pen line_2 = new Pen(brus[r.Next(brus.Length - 1)], r.Next(3));
            float x1 = r.Next(IMAGE_WIDTH / 2);
            float x2 = r.Next(IMAGE_WIDTH);
            float y1 = r.Next(IMAGE_HEIGHT / 2);
            float y2 = r.Next(IMAGE_HEIGHT);
            g.DrawLine(line_1, x1, y1, x2, y2);
            x1 = r.Next(IMAGE_WIDTH);
            x2 = r.Next(IMAGE_WIDTH);
            y1 = r.Next(IMAGE_HEIGHT);
            y2 = r.Next(IMAGE_HEIGHT);
            g.DrawLine(line_2, x1, y1, x2, y2);
            // 绘制验证码图像
            int posX = 0;
            int posY = 0;
            for (int i = 0; i < code.Length; i++)
            {
                int fs = fontSize[r.Next(fontSize.Length - 1)];
                if (IMAGE_HEIGHT < 24)
                    posY = 0;
                else
                    posY = r.Next(-5, IMAGE_HEIGHT - fs - 2);
                Font font = new Font(fonts[r.Next(fonts.Length - 1)], fs, fontStyles[r.Next(fontStyles.Length - 1)]);
                string c = code[i].ToString();
                g.DrawString(c, font, brus[r.Next(brus.Length - 1)], posX, posY);
                posX += fs;
            }
        }
    }
}
