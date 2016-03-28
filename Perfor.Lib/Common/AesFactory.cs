using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;

using Perfor.Lib.Extension;
using System.Security.Cryptography;
using System.IO;

namespace Perfor.Lib.Common
{
    /**
     * @ AES 对称加密算法自定义管理类
     * @ 该类使用 AesManaged 作为加密工厂
     * */
    public class AesFactory : Aes
    {
        #region Identity
        // 默认 key（32位）注意：生产环境请勿使用默认 key
        private byte[] crypt_key = { 0xe8, 0xbf, 0x99, 0xe6, 0x98, 0xaf, 0x64, 0x61, 0x6e, 0x6e, 0x79, 0xe5, 0x88, 0x9b, 0xe5, 0xbb, 0xba, 0xe7, 0x9a, 0x84, 0xe5, 0x8a, 0xa0, 0xe5, 0xaf, 0x86, 0xe7, 0x9a, 0x84, 0x4b, 0x45, 0x59 };
        // 默认 向量（16位）注意：生产环境请勿使用默认 iv
        private byte[] crypt_iv = { 0xe8, 0xbf, 0x99, 0xe6, 0x98, 0xaf, 0x64, 0x61, 0x6e, 0x6e, 0x79, 0xe5, 0x88, 0x9b, 0xe5, 0xbb };
        #endregion

        /**
         * @ 默认构造函数
         * @ useDefaultKey 是否使用默认的 key 和 iv，为避免埋雷，这里设置默认不使用
         * */
        public AesFactory(bool useDefaultKey = false)
        {
            if (useDefaultKey == false)
            {
                new Random(32).NextBytes(this.crypt_key);
                new Random(16).NextBytes(this.crypt_iv);
            }
        }
        
        /**
         * @ 构造函数第三次重载，使用指定的 key 和 iv 填充加密工厂
         * @ key 钥匙
         * @ iv 向量
         * */
        public AesFactory(string key, string iv)
        {
            if (key.IsNullOrEmpty())
                throw new ArgumentException("参数 key 不能为空");
            if (iv.IsNullOrEmpty())
                throw new ArgumentException("参数 key 不能为空并");

            byte[] b_key = Encoding.UTF8.GetBytes(key);
            byte[] b_iv = Encoding.UTF8.GetBytes(iv);
            int len = this.crypt_key.Length;
            int b_len = b_key.Length;
            for (int i = 0; i < len; i++)
            {
                this.crypt_key[i] = b_key[i];
                if (i + 1 == b_len) break;
            }

            len = this.crypt_iv.Length;
            b_len = b_iv.Length;
            for (int i = 0; i < len; i++)
            {
                this.crypt_iv[i] = b_iv[i];
                if (i + 1 == b_len) break;
            }
        }

        /**
         * @ 构造函数第二次重载，使用指定的 key 和 iv 填充加密工厂
         * @ key 钥匙，长度为 32 位 byte 数组
         * @ iv 向量，长度为 16 位 byte 数组
         * */
        public AesFactory(byte[] key, byte[] iv)
        {
            if (key.IsNotNullAndEq(32) == false)
                throw new ArgumentException("参数 key 不能为空并且长度必须等于32");
            if (iv.IsNotNullAndEq(16) == false)
                throw new ArgumentException("参数 iv 不能为空并且长度必须等于16");

            this.crypt_key = key;
            this.crypt_iv = iv;
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
                CryptoStream csCrypto = new CryptoStream(msCrypto, transform, CryptoStreamMode.Write);
                csCrypto.Write(crypt, 0, crypt.Length);
                csCrypto.FlushFinalBlock();
                byte[] bytes = msCrypto.ToArray();
                result = bytes.ToBase64();
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
                CryptoStream csCrypto = new CryptoStream(msCrypto, transform, CryptoStreamMode.Read);
                StreamReader swCrypto = new StreamReader(csCrypto);
                result = swCrypto.ReadToEnd();
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
            new Random(16).NextBytes(this.crypt_iv);
        }

        /**
         * @ 生成用于该算法的随机 System.Security.Cryptography.SymmetricAlgorithm.Key。
         * */
        public override void GenerateKey()
        {
            new Random(32).NextBytes(this.crypt_key);
        }

        #region Properties
        /**
         * @ 向量
         * */
        public override byte[] IV
        {
            get
            {
                return crypt_iv;
            }
            set
            {
                crypt_iv = value;
            }
        }


        /**
         * @ 钥匙
         * */
        public override byte[] Key
        {
            get
            {
                return crypt_key;
            }
            set
            {
                crypt_key = value;
            }
        }
        #endregion
    }
}
