using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.IO;
using System.Threading;

namespace Perfor.Lib.Common
{
    /*
     * @ 输出验证图片
     * */
    public partial class CheckCode
    {
        #region Identity
        // 验证码长度
        private int codeLen = 4;
        private CodeStyleType codestyle = CodeStyleType.NUMBER;
        // 图片清晰度/宽度/高度
        private int fineness = 100, imgWidth = 100, imgHeight = 24;
        // 字体家族名称
        private string fontFamily = "Arial Black";
        // 字体大小/样式
        private int fontSize = 15;
        FontStyleType fontStyle = FontStyleType.NORMAL;
        // 绘制起始坐标 X，Y
        private int posX = 0, posY = 0;
        #endregion

        /**
         * @ 生成验证图片
         * @ codeLength 验证码长度4-16
         * @ fineNess 图片清晰度0-100
         * @ imgWidth 图片宽度16-360
         * @ imgHeight 图片高度16-320
         * @ fontFamily 字体家族名称"宋体"
         * @ fontSize 字体大小8-72
         * @ fontStyle 字体样式 
         * @ pointX 绘制起始 X 坐标点
         * @ pointY 绘制起始 Y 坐标点
         * @ sessionKey 保存验证码值到 Session 的 key
         * @ codeStyle 验证码类型
         * */
        public byte[] WriterCode(int codeLength, int fineNess, int imgWidth, int imgHeight, string fontFamily, int fontSize, FontStyleType fontStyle, int pointX, int pointY, string sessionKey, CodeStyleType codeStyle)
        {
            this.codeLen = codeLength;
            this.fineness = fineNess;
            this.imgWidth = imgWidth;
            this.imgHeight = imgHeight;
            this.fontFamily = fontFamily;
            this.fontSize = fontSize;
            this.fontStyle = fontStyle;
            this.posX = pointX;
            this.posY = pointY;
            this.sessionkey = sessionKey;
            this.codestyle = codeStyle;
            return WriterCode();
        }

        /**
         * @ 指定要生成的验证码类型，然后使用默认的设置生成验证码
         * @ codeStyle 验证码类型，0英文字母，1中文字符，3数字
         * */
        public byte[] WriterCode(CodeStyleType codeStyle)
        {
            codestyle = codeStyle;
            return WriterCode();
        }

        /**
         * @ 使用默认的设置生成验证
         * */
        public byte[] WriterCode()
        {
            sessionkey = CreateCode();
            Bitmap bitmap = new Bitmap(imgWidth, imgHeight);
            // 给图像设置干扰
            DisturbBitmap(bitmap);
            // 绘制验证码图像
            DrawCode(bitmap, sessionCode);
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Jpeg);
            return ms.ToArray();
        }

        /*
         * @ 开始生成
         * */
        private string CreateCode()
        {
            Random random = new Random();
            switch (codestyle)
            {
                case CodeStyleType.CHAR:
                    for (int i = 0; i < codeLen; i++)
                    {
                        // 26: a - z
                        int n = random.Next(26);
                        // 将小写字母转换成大写字母
                        sessionCode += (char)(n + 65);
                    }
                    break;
                case CodeStyleType.ZH_CN:
                    sessionCode = CodeRandom.GetRand_CN(4);
                    break;
                case CodeStyleType.NUMBER:
                    sessionCode += CodeRandom.GetRandom(0, 9, codeLen);
                    break;
            }
            return sessionCode;
        }

        /**
         * @ 为图片设置干扰点
         * */
        private void DisturbBitmap(Bitmap bitmap)
        {
            Random random = new Random();
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    if (random.Next(100) <= this.fineness)
                        bitmap.SetPixel(i, j, Color.White);
                }
            }
        }

        /**
         * @ 绘制验证码图片
         * */
        private void DrawCode(Bitmap bitmap, string code)
        {
            Graphics g = Graphics.FromImage(bitmap);
            Font font = new Font(fontFamily, fontSize, GetFontStyle());
            Pen myPen = new Pen(Color.Black, 2);

            ////随机画两条线
            //Random r = new Random();
            //float x1 = r.Next(imgWidth);
            //float x2 = r.Next(imgWidth);
            //float y1 = r.Next(imgHeight);
            //float y2 = r.Next(imgHeight);
            //g.DrawLine(myPen, x1, y1, x2, y2);
            //x1 = r.Next(imgWidth);
            //x2 = r.Next(imgWidth);
            //y1 = r.Next(imgHeight);
            //y2 = r.Next(imgHeight);
            //g.DrawLine(myPen, x1, y1, x2, y2);
            // 绘制验证码图像
            g.DrawString(code, font, Brushes.Black, posX, posY);

        }

        /**
         * @ 换算验证码字体样式：1 粗体 2 斜体 3 粗斜体，默认为普通字体
         * */
        private FontStyle GetFontStyle()
        {
            if (fontStyle == FontStyleType.BOLD)
                return FontStyle.Bold;
            else if (fontStyle == FontStyleType.ITALIC)
                return FontStyle.Italic;
            else if (fontStyle == FontStyleType.BOLD_ITALIC)
                return FontStyle.Bold | FontStyle.Italic;
            else
                return FontStyle.Regular;
        }

        #region Properties
        private string sessionkey = "code";
        /**
         * @ 保存验证码值Session的名字 默认是 "code"
         * */
        public string SessionKey
        {
            get { return sessionkey; }
            set { sessionkey = value; }
        }

        private string sessionCode = string.Empty;
        /**
         * @ 当前生成的验证码
         * */
        public string SessionCode
        {
            get { return sessionCode; }
            set { sessionCode = value; }
        }
        #endregion
    }

    /*
     * @ 验证码随机数
     * */
    public class CodeRandom
    {
        /*
         * @ 生成随机n位数字符串
         * @ len 随机数长度
         * */
        public static string GetRandStrNumber(int len)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            StringBuilder str = new StringBuilder(len);
            for (int i = 0; i < len; i++)
            {
                int n = rnd.Next(10);
                str.Append(n);
            }
            return str.ToString();
        }

        /*
         * @ 生成随机n位数字符串
         * @ len 随机数长度
         * */
        public static int GetRandNumber(int len)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            int n = rnd.Next(len);
            return n;
        }

        /*
          * @ 生成随机n位数字符串
          * @ len 随机数长度
          * @ rand 指定种子
          * */
        public static int GetRandNumber(int len, Random rand)
        {
            int number = rand.Next(len);
            return number;
        }

        /**
         * @ 返回随机字母
         * @ len 长度
         * */
        public static string GetRandChar(int len)
        {
            string code = string.Empty;
            Random random = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < len; i++)
            {
                // 26: a - z
                int n = random.Next(26);
                // 将小写字母转换成大写字母
                code += (char)(n + 65);
            }
            return code;
        }

        /**
         * @ 随机产生颜色
         * */
        public static Color GetRandColor()
        {
            Random rnf = new Random(Guid.NewGuid().GetHashCode());
            //  对于C#的随机数，没什么好说的
            Thread.Sleep(rnf.Next(50));
            Random rns = new Random((int)DateTime.Now.Ticks);

            //  为了在白色背景上显示，尽量生成深色
            int int_Red = rnf.Next(256);
            int int_Green = rns.Next(256);
            int int_Blue = (int_Red + int_Green > 400) ? 0 : 400 - int_Red - int_Green;
            int_Blue = (int_Blue > 255) ? 255 : int_Blue;

            return Color.FromArgb(int_Red, int_Green, int_Blue);
        }

        /*
         * @ 随机生成中文
         * @ len 长度
         * */
        public static string GetRand_CN(int len)
        {
            string allcn = "们以我到他会作时要动国产的一是工就年阶义发成部民可出能方进在了不和有大这主中人上为来分生对于学下级地个用同行面说种过命度革而多子后自社加小机也经力线本电高量长党得实家定深法表着水理化争现所二起政三好十战无农使性前等反体合斗路图把结第里正新开论之物从当两些还天资事队批点育重其思与间内去因件日利相由压员气业代全组数果期导平各基或月毛然如应形想制心样干都向变关问比展那它最及外没看治提五解系林者米群头意只明四道马认次文通但条较克又公孔领军流入接席位情运器并飞原油放立题质指建区验活众很教决特此常石强极土少已根共直团统式转别造切九你取西持总料连任志观调七么山程百报更见必真保热委手改管处己将修支识病象几先老光专什六型具示复安带每东增则完风回南广劳轮科北打积车计给节做务被整联步类集号列温装即毫知轴研单色坚据速防史拉世设达尔场织历花受求传口断况采精金界品判参层止边清至万确究书术状厂须离再目海交权且儿青才证低越际八试规斯近注办布门铁需走议县兵固除般引齿千胜细影济白格效置推空配刀叶率述今选养德话查差半敌始片施响收华觉备名红续均药标记难存测士身紧液派准斤角降维板许破述技消底床田势端感往神便贺村构照容非搞亚磨族火段算适讲按值美态黄易彪服早班麦削信排台声该击素张密害侯草何树肥继右属市严径螺检左页抗苏显苦英快称坏移约巴材省黑武培著河帝仅针怎植京助升王眼她抓含苗副杂普谈围食射源例致酸旧却充足短划剂宣环落首尺波承粉践府鱼随考刻靠够满夫失包住促枝局菌杆周护岩师举曲春元超负砂封换太模贫减阳扬江析亩木言球朝医校古呢稻宋听唯输滑站另卫字鼓刚写刘微略范供阿块某功套友限项余倒卷创律雨让骨远帮初皮播优占死毒圈伟季训控激找叫云互跟裂粮粒母练塞钢顶策双留误础吸阻故寸盾晚丝女散焊功株亲院冷彻弹错散商视艺灭版烈零室轻血倍缺厘泵察绝富城冲喷壤简否柱李望盘磁雄似困巩益洲脱投送奴侧润盖挥距触星松送获兴独官混纪依未突架宽冬章湿偏纹吃执阀矿寨责熟稳夺硬价努翻奇甲预职评读背协损棉侵灰虽矛厚罗泥辟告卵箱掌氧恩爱停曾溶营终纲孟钱待尽俄缩沙退陈讨奋械载胞幼哪剥迫旋征槽倒握担仍呀鲜吧卡粗介钻逐弱脚怕盐末阴丰雾冠丙街莱贝辐肠付吉渗瑞惊顿挤秒悬姆烂森糖圣凹陶词迟蚕亿矩康遵牧遭幅园腔订香肉弟屋敏恢忘编印蜂急拿扩伤飞露核缘游振操央伍域甚迅辉异序免纸夜乡久隶缸夹念兰映沟乙吗儒杀汽磷艰晶插埃燃欢铁补咱芽永瓦倾阵碳演威附牙芽永瓦斜灌欧献顺猪洋腐请透司危括脉宜笑若尾束壮暴企菜穗楚汉愈绿拖牛份染既秋遍锻玉夏疗尖殖井费州访吹荣铜沿替滚客召旱悟刺脑措贯藏敢令隙炉壳硫煤迎铸粘探临薄旬善福纵择礼愿伏残雷延烟句纯渐耕跑泽慢栽鲁赤繁境潮横掉锥希池败船假亮谓托伙哲怀割摆贡呈劲财仪沉炼麻罪祖息车穿货销齐鼠抽画饲龙库守筑房歌寒喜哥洗蚀废纳腹乎录镜妇恶脂庄擦险赞钟摇典柄辩竹谷卖乱虚桥奥伯赶垂途额壁网截野遗静谋弄挂课镇妄盛耐援扎虑键归符庆聚绕摩忙舞遇索顾胶羊湖钉仁音迹碎伸灯避泛亡答勇频皇柳哈揭甘诺概宪浓岛袭谁洪谢炮浇斑讯懂灵蛋闭孩释乳巨徒私银伊景坦累匀霉杜乐勒隔弯绩招绍胡呼痛峰零柴簧午跳居尚丁秦稍追梁折耗碱殊岗挖氏刃剧堆赫荷胸衡勤膜篇登驻案刊秧缓凸役剪川雪链渔啦脸户洛孢勃盟买杨宗焦赛旗滤硅炭股坐蒸凝竟陷枪黎救冒暗洞犯筒您宋弧爆谬涂味津臂障褐陆啊健尊豆拔莫抵桑坡缝警挑污冰柬嘴啥饭塑寄赵喊垫丹渡耳刨虎笔稀昆浪萨茶滴浅拥穴覆伦娘吨浸袖珠雌妈紫戏塔锤震岁貌洁剖牢锋疑霸闪埔猛诉刷狠忽灾闹乔唐漏闻沈熔氯荒茎男凡抢像浆旁玻亦忠唱蒙予纷捕锁尤乘乌智淡允叛畜俘摸锈扫毕璃宝芯爷鉴秘净蒋钙肩腾枯抛轨堂拌爸循诱祝励肯酒绳穷塘燥泡袋朗喂铝软渠颗惯贸粪综墙趋彼届墨碍启逆卸航衣孙龄岭骗休借";
            string rel = "";
            for (int i = 0; i < len; i++)
            {
                int key = GetRandNumber(allcn.Length);
                rel += allcn[key];
            }
            return rel;
        }

        /**
         * @ 生成随机字母字符串(数字字母混和)
         * @ len 待生成的位数
         * @ 生成的字母字符串
         * */
        public static string GenerateCode(int len)
        {
            int rep = 0;
            string str = string.Empty;
            long num2 = DateTime.Now.Ticks + rep;
            rep++;
            Random random = new Random(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> rep)));
            for (int i = 0; i < len; i++)
            {
                char ch;
                int num = random.Next();
                if ((num % 2) == 0)
                    ch = (char)(0x30 + ((ushort)(num % 10)));
                else
                    ch = (char)(0x41 + ((ushort)(num % 0x1a)));
                str = str + ch.ToString();
            }
            return str;
        }

        /*
         * @ 产生随机数
         * @ minValue 最小值
         * @ maxValue 最大值
         * @ len 长度
         * */
        public static string GetRandom(int minValue, int maxValue, int len)
        {
            int[] intList = new int[maxValue];
            for (int i = 0; i < maxValue; i++)
            {
                intList[i] = i + minValue;
            }
            int[] intRet = new int[len];
            string sRet = string.Empty;
            int n = maxValue;
            Random rand = new Random();
            for (int i = 0; i < len; i++)
            {
                int index = rand.Next(0, n);
                sRet += intList[index].ToString();
                intList[index] = intList[--n];
            }

            return sRet;
        }
    }

    /*
     * @ 验证码样式类型
     * */
    public enum CodeStyleType
    {
        /**
         * @ 英文字母
         * */
        CHAR = 10,
        /**
         * @ 中文字符
         * */
        ZH_CN = 20,
        /**
         * @ 数字
         * */
        NUMBER = 30
    }

    /*
     * @ 验证码字体样式类型
     * */
    public enum FontStyleType
    {
        /**
         * @ 普通文本
         * */
        NORMAL = 0,
        /**
         * @ 粗体
         * */
        BOLD = 10,
        /**
         * @ 斜体
         * */
        ITALIC = 20,
        /**
         * @ 粗斜体
         * */
        BOLD_ITALIC = 30
    }
}
