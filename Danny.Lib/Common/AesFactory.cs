using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;

using Danny.Lib.Extension;
using System.Security.Cryptography;
using System.IO;

namespace Danny.Lib.Common
{
    /**
     * @ AES 对称加密算法自定义管理类
     * @ 该类使用 AesManaged 作为加密工厂
     * */
    public class AesFactory : Aes
    {
        // 初始化标准KEY（32位）
        private static byte[] CRYPT_KEY = { 0xe8, 0xbf, 0x99, 0xe6, 0x98, 0xaf, 0x64, 0x61, 0x6e, 0x6e, 0x79, 0xe5, 0x88, 0x9b, 0xe5, 0xbb, 0xba, 0xe7, 0x9a, 0x84, 0xe5, 0x8a, 0xa0, 0xe5, 0xaf, 0x86, 0xe7, 0x9a, 0x84, 0x4b, 0x45, 0x59 };
        // 向量（16位）
        private static byte[] CRYPT_IV = { 0xe8, 0xbf, 0x99, 0xe6, 0x98, 0xaf, 0x64, 0x61, 0x6e, 0x6e, 0x79, 0xe5, 0x88, 0x9b, 0xe5, 0xbb };

        /**
         * @ 默认构造函数
         * */
        public AesFactory()
        {
            this.IV = CRYPT_IV;
            this.Key = CRYPT_KEY;
        }

        /**
         * @ 加密字符串
         * @ sourceText 待加密的内容
         * */
        public string Encrypt(string crypt)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(crypt);
            string result = Encrypt(bytes);

            return result;
        }

        /**
         * @ 加密字符串
         * @ crypt 待加密的内容
         * */
        public string Encrypt(byte[] crypt)
        {
            string result = string.Empty;
            ICryptoTransform transform = CreateEncryptor(this.Key, this.IV);
            using (MemoryStream msCrypto = new MemoryStream())
            {
                using (CryptoStream csCrypto = new CryptoStream(msCrypto, transform, CryptoStreamMode.Write))
                {
                    csCrypto.Write(crypt, 0, crypt.Length);
                    csCrypto.FlushFinalBlock();
                    byte[] bytes = msCrypto.ToArray();
                    result = bytes.ToBase64();
                }
            }
            return result;
        }

        /**
         * @ 解密字符串
         * @ sourceText 待解密的字符串
         * */
        public string Decrypt(string encrypt)
        {
            byte[] bytes = encrypt.FromBase64();
            string result = Decrypt(bytes);
            return result;
        }

        /**
         * @ 解密字节数组
         * @ 待解密的字节数组
         * */
        public string Decrypt(byte[] encrypt)
        {
            if (encrypt.IsNullOrEmpty())
                return string.Empty;

            string result = string.Empty;
            ICryptoTransform transform = CreateDecryptor(this.Key, this.IV);
            using (MemoryStream msCrypto = new MemoryStream(encrypt))
            {
                using (CryptoStream csCrypto = new CryptoStream(msCrypto, transform, CryptoStreamMode.Read))
                {
                    using (StreamReader swCrypto = new StreamReader(csCrypto))
                    {
                        result = swCrypto.ReadToEnd();
                    }
                }
            }

            return result;
        }

        /**
         * @ 创建解密容器接口
         * @ rgbKey 解密用的 key
         * @ rgbIV 解密用的向量 iv
         * */
        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            ICryptoTransform transform = AesManaged.Create().CreateDecryptor(rgbKey, rgbIV);

            return transform;
        }

        /**
         * @ 创建加密容器接口
         * @ rgbKey 加密用的 key
         * @ rgbIV 加密用的向量 iv
         * */
        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            ICryptoTransform transform = AesManaged.Create().CreateEncryptor(rgbKey, rgbIV);

            return transform;
        }

        /**
         * @ 生成用于该算法的随机初始化向量（System.Security.Cryptography.SymmetricAlgorithm.IV）
         * */
        public override void GenerateIV()
        {
            new Random(16).NextBytes(this.IV);
        }

        /**
         * @ 生成用于该算法的随机 System.Security.Cryptography.SymmetricAlgorithm.Key。
         * */
        public override void GenerateKey()
        {
            new Random(32).NextBytes(this.Key);
        }
    }
}
