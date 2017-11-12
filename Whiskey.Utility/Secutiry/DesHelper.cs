
//  <copyright file="AbstractBuilder.cs" company="优维拉软件设计工作室">



//  <last-date>2014:07:05 13:48</last-date>


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Whiskey.Utility.Extensions;
using Whiskey.Utility.Properties;


namespace Whiskey.Utility.Secutiry
{
    /// <summary>
    /// DES / TripleDES加密解密操作类
    /// </summary>
    public class DesHelper
    {
        private const int BufferAppendSize = 64;
        private const string SectionSign = "?SECTION?";
        private readonly bool _isTriple;
        private readonly byte[] _key;

        /// <summary>
        /// 使用随机密码初始化一个<see cref="DesHelper"/>类的新实例
        /// </summary>
        /// <param name="isTriple">是否使用TripleDES方式，否则为DES方式</param>
        public DesHelper(bool isTriple = false)
            : this(isTriple
                ? new TripleDESCryptoServiceProvider().Key
                : new DESCryptoServiceProvider().Key)
        {
            _isTriple = isTriple;
        }

        /// <summary>
        /// 获取 密钥
        /// </summary>
        public byte[] Key { get { return _key; } }

        #region 实例方法

        /// <summary>
        /// 使用指定8位密码初始化一个<see cref="DesHelper"/>类的新实例
        /// </summary>
        public DesHelper(byte[] key)
        {
            key.CheckNotNull("key");
            key.Required(k => k.Length == 8 || k.Length == 24, string.Format(Resources.Security_DES_KeyLenght, key.Length));
            _key = key;
            _isTriple = key.Length == 24;
        }

        /// <summary>
        /// 加密字节数组
        /// </summary>
        /// <param name="source">要加密的字节数组</param>
        /// <returns>加密后的字节数组</returns>
        public byte[] Encrypt(byte[] source)
        {
            source.CheckNotNull("source");
            SymmetricAlgorithm provider = _isTriple
                ? (SymmetricAlgorithm)new TripleDESCryptoServiceProvider { Key = _key, Mode = CipherMode.ECB }
                : new DESCryptoServiceProvider { Key = _key, Mode = CipherMode.ECB };

            MemoryStream ms = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(ms, provider.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(source, 0, source.Length);
                cs.FlushFinalBlock();
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 解密字节数组
        /// </summary>
        /// <param name="source">要解密的字节数组</param>
        /// <returns>解密后的字节数组</returns>
        public byte[] Decrypt(byte[] source)
        {
            source.CheckNotNull("source");

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            SymmetricAlgorithm provider = _isTriple
                ? (SymmetricAlgorithm)new TripleDESCryptoServiceProvider { Key = _key, Mode = CipherMode.ECB }
                : new DESCryptoServiceProvider { Key = _key, Mode = CipherMode.ECB };
            MemoryStream ms = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(ms, provider.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(source, 0, source.Length);
                cs.FlushFinalBlock();
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 加密字符串，输出BASE64编码字符串
        /// </summary>
        /// <param name="source">要加密的明文字符串</param>
        /// <returns>加密的BASE64编码的字符串</returns>
        public string Encrypt(string source)
        {
            source.CheckNotNull("source");
            byte[] bytes = Encoding.UTF8.GetBytes(source);
            return Convert.ToBase64String(Encrypt(bytes));
        }

        /// <summary>
        /// 解密字符串，输入为BASE64编码字符串
        /// </summary>
        /// <param name="source">要解密的BASE64编码的字符串</param>
        /// <returns>明文字符串</returns>
        public string Decrypt(string source)
        {
            source.CheckNotNullOrEmpty("source");
            byte[] bytes = Convert.FromBase64String(source);
            return Encoding.UTF8.GetString(Decrypt(bytes));
        }

        /// <summary>
        /// 整体加密文件
        /// </summary>
        /// <param name="sourceFile">待加密的文件名</param>
        /// <param name="targetFile">保存加密文件名</param>
        public void EncryptFile(string sourceFile, string targetFile)
        {
            sourceFile.CheckFileExists("sourceFile");
            targetFile.CheckNotNullOrEmpty("targetFile");

            using (FileStream ifs = new FileStream(sourceFile, FileMode.Open, FileAccess.Read),
                ofs = new FileStream(targetFile, FileMode.Create, FileAccess.Write))
            {
                long length = ifs.Length;
                byte[] sourceBytes = new byte[length];
                ifs.Read(sourceBytes, 0, sourceBytes.Length);
                byte[] targetBytes = Encrypt(sourceBytes);
                ofs.Write(targetBytes, 0, targetBytes.Length);
            }
        }

        /// <summary>
        /// 分段加密文件
        /// </summary>
        /// <param name="sourceFile">待加密的文件名</param>
        /// <param name="targetFile">保存加密文件名</param>
        /// <param name="sectionLength">分段大小（字节）</param>
        public void EncryptFile(string sourceFile, string targetFile, int sectionLength)
        {
            sourceFile.CheckFileExists("sourceFile");
            targetFile.CheckNotNullOrEmpty("targetFile");
            sectionLength.CheckGreaterThan("sectionLength", 0);

            using (FileStream ifs = new FileStream(sourceFile, FileMode.Open, FileAccess.Read),
                ofs = new FileStream(targetFile, FileMode.Create, FileAccess.Write))
            {
                //追加附加数据到加密文件开关
                long decryptFileSize = ifs.Length;
                byte[] appendBytes = new byte[BufferAppendSize];

                //0位为加密分段大小，1位为未加密文件的长度
                //附加信息格式为：{分段长度}|{明文文件长度}|{结束0标记}
                string appendStr = "{0}|{1}|{2}".FormatWith(sectionLength, decryptFileSize, 0);
                appendStr = Encrypt(appendStr);
                int sectionSignSize = (SectionSign + "|").Length;
                //密文附加信息长度
                int appendStrSize = appendStr.Length + sectionSignSize;
                //附加信息格式为：{附加信息长度}|{分段长度}|{明文文件长度}|{结束0标记}
                appendStr = "{0}|{1}".FormatWith(appendStrSize, appendStr);

                //在文件最开关添加分段标记Section_Sign，说明文件是分段加密文件
                //附加串信息格式为：{分段加密标记}|{附加信息长度}|{分段长度}|{明文文件长度}|{结束0标记}
                appendStr = "{0}|{1}".FormatWith(SectionSign, appendStr);
                appendStr = appendStr.Replace("|" + appendStrSize.ToString(CultureInfo.InvariantCulture) + "|",
                    "|" + appendStr.Length.ToString(CultureInfo.InvariantCulture) + "|");
                byte[] tmpBytes = Encoding.UTF8.GetBytes(appendStr);
                using (MemoryStream ms = new MemoryStream(appendBytes))
                {
                    ms.Write(tmpBytes, 0, tmpBytes.Length);
                    appendBytes = ms.ToArray();
                }

                ofs.Seek(0, SeekOrigin.Begin);
                ofs.Write(appendBytes, 0, appendBytes.Length);

                long fileSize = ifs.Length;
                long sectionCount = fileSize / sectionLength;
                int lastLength = (int)(fileSize % sectionLength);

                int length;
                byte[] sourceBytes = new byte[sectionLength];
                if (sectionCount > 0)
                {
                    length = ifs.Read(sourceBytes, 0, sourceBytes.Length);
                }
                else
                {
                    sourceBytes = new byte[lastLength];
                    length = ifs.Read(sourceBytes, 0, sourceBytes.Length);
                }
                while (length > 0)
                {
                    byte[] targetBytes = Encrypt(sourceBytes);
                    ofs.Write(targetBytes, 0, targetBytes.Length);
                    sectionCount--;
                    if (sectionCount > 0)
                    {
                        length = ifs.Read(sourceBytes, 0, sourceBytes.Length);
                    }
                    else
                    {
                        sourceBytes = new byte[lastLength];
                        length = ifs.Read(sourceBytes, 0, sourceBytes.Length);
                    }
                }
            }
        }

        /// <summary>
        /// 对文件内容进行DES解密，能自动识别并处理是否为分段加密
        /// </summary>
        /// <param name="sourceFile">待加密的文件名</param>
        /// <param name="targetFile">保存加密文件名</param>
        public void DecryptFile(string sourceFile, string targetFile)
        {
            sourceFile.CheckFileExists("sourceFile");
            targetFile.CheckNotNullOrEmpty("targetFile");

            using (FileStream ifs = new FileStream(sourceFile, FileMode.Open, FileAccess.Read),
                ofs = new FileStream(targetFile, FileMode.Create, FileAccess.Write))
            {
                //判断是否分段加密
                bool isSection = CheckSectionSign(ifs);
                if (!isSection)
                {
                    ifs.Seek(0, SeekOrigin.Begin);
                    long length = ifs.Length;
                    byte[] sourceBytes = new byte[length];
                    ifs.Read(sourceBytes, 0, sourceBytes.Length);
                    byte[] targetBytes = Decrypt(sourceBytes);
                    ofs.Write(targetBytes, 0, targetBytes.Length);
                }
                else
                {
                    //从加密文件中读取附加信息，获取加密时分段大小
                    ifs.Seek(0, SeekOrigin.Begin);
                    byte[] appendBytes = new byte[BufferAppendSize];
                    ifs.Read(appendBytes, 0, appendBytes.Length);
                    string appendStr = Encoding.UTF8.GetString(appendBytes);
                    string tmpAppend = appendStr.Substring(SectionSign.Length + 1);
                    int appendStrSize = Convert.ToInt32(tmpAppend.Substring(0, tmpAppend.IndexOf("|", StringComparison.Ordinal)));
                    appendStr = appendStr.Substring(0, appendStrSize);
                    tmpAppend = appendStr.Substring(appendStr.LastIndexOf("|", StringComparison.Ordinal) + 1,
                        appendStr.Length - appendStr.LastIndexOf("|", StringComparison.Ordinal) - 1);
                    appendStr = Decrypt(tmpAppend);
                    int sectionLength = Convert.ToInt32(appendStr.Split('|')[0]);
                    ifs.Seek(BufferAppendSize, SeekOrigin.Begin); //把文件读取指针移到附加信息后面
                    sectionLength = Encrypt(new byte[sectionLength]).Length;
                    long fileSize = ifs.Length;
                    fileSize -= BufferAppendSize;
                    long sectionCount = fileSize / sectionLength; //段数
                    int laseLength = (int)(fileSize % sectionLength); //最后一段长度
                    int length;
                    byte[] sourceBytes = new byte[sectionLength]; //加密数据缓冲区
                    if (sectionCount > 0)
                    {
                        length = ifs.Read(sourceBytes, 0, sourceBytes.Length);
                    }
                    else
                    {
                        sourceBytes = new byte[laseLength];
                        length = ifs.Read(sourceBytes, 0, sourceBytes.Length);
                    }
                    while (length > 0)
                    {
                        byte[] targetBytes = Decrypt(sourceBytes);
                        ofs.Write(targetBytes, 0, targetBytes.Length);
                        sectionCount--;
                        if (sectionCount > 0)
                        {
                            length = ifs.Read(sourceBytes, 0, sourceBytes.Length);
                        }
                        else if (sectionCount == 0)
                        {
                            sourceBytes = new byte[laseLength];
                            length = ifs.Read(sourceBytes, 0, sourceBytes.Length);
                        }
                        else
                        {
                            length = 0;
                        }
                    }
                }
            }
        }

        private static bool CheckSectionSign(Stream ifs)
        {
            int sectionSignSize = SectionSign.Length;
            ifs.Seek(0, SeekOrigin.Begin);
            byte[] sectionSignBytes = new byte[sectionSignSize];
            ifs.Read(sectionSignBytes, 0, sectionSignBytes.Length);
            string sectionSignString = Encoding.UTF8.GetString(sectionSignBytes);
            return sectionSignString == SectionSign;
        }

        #endregion

        #region 静态方法

        /// <summary>
        /// 加密字节数组
        /// </summary>
        /// <param name="source">要加密的字节数组</param>
        /// <param name="key">密钥字节数组，长度为8或者24</param>
        /// <returns>加密后的字节数组</returns>
        public static byte[] Encrypt(byte[] source, byte[] key)
        {
            DesHelper des = new DesHelper(key);
            return des.Encrypt(source);
        }

        /// <summary>
        /// 解密字节数组
        /// </summary>
        /// <param name="source">要解密的字节数组</param>
        /// <param name="key">密钥字节数组，长度为8或者24</param>
        /// <returns>解密后的字节数组</returns>
        public static byte[] Decrypt(byte[] source, byte[] key)
        {
            DesHelper des = new DesHelper(key);
            return des.Decrypt(source);
        }

        /// <summary>
        /// 加密字符串，输出BASE64编码字符串
        /// </summary>
        /// <param name="source">要加密的明文字符串</param>
        /// <param name="key">密钥字符串，长度为8或者24</param>
        /// <returns>加密的BASE64编码的字符串</returns>
        public static string Encrypt(string source, string key)
        {
            source.CheckNotNull("source");
            key.CheckNotNullOrEmpty("key");
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            DesHelper des = new DesHelper(keyBytes);
            return des.Encrypt(source);
        }

        /// <summary>
        /// 解密字符串，输入BASE64编码字符串
        /// </summary>
        /// <param name="source">要解密的BASE64编码字符串</param>
        /// <param name="key">密钥字符串，长度为8或者24</param>
        /// <returns>解密的明文字符串</returns>
        public static string Decrypt(string source, string key)
        {
            source.CheckNotNullOrEmpty("source");
            key.CheckNotNullOrEmpty("key");
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            DesHelper des = new DesHelper(keyBytes);
            return des.Decrypt(source);
        }

        /// <summary>
        /// 整体加密文件
        /// </summary>
        /// <param name="sourceFile">待加密的文件名</param>
        /// <param name="targetFile">保存加密文件名</param>
        /// <param name="key">密钥字符串，长度为8或者24</param>
        public static void EncryptFile(string sourceFile, string targetFile, string key)
        {
            sourceFile.CheckFileExists("sourceFile");
            targetFile.CheckNotNullOrEmpty("targetFile");
            key.CheckNotNullOrEmpty("key");

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            DesHelper des = new DesHelper(keyBytes);
            des.EncryptFile(sourceFile, targetFile);
        }

        /// <summary>
        /// 分段加密文件
        /// </summary>
        /// <param name="sourceFile">待加密的文件名</param>
        /// <param name="targetFile">保存加密文件名</param>
        /// <param name="sectionLength">分段大小（字节）</param>
        /// <param name="key">密钥字符串，长度为8或者24</param>
        public static void EncryptFile(string sourceFile, string targetFile, int sectionLength, string key)
        {
            sourceFile.CheckFileExists("sourceFile");
            targetFile.CheckNotNullOrEmpty("targetFile");
            key.CheckNotNullOrEmpty("key");
            sectionLength.CheckGreaterThan("sectionLength", 0);

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            DesHelper des = new DesHelper(keyBytes);
            des.EncryptFile(sourceFile, targetFile, sectionLength);
        }

        /// <summary>
        /// 对文件内容进行DES解密，能自动识别并处理是否为分段加密
        /// </summary>
        /// <param name="sourceFile">待加密的文件名</param>
        /// <param name="targetFile">保存加密文件名</param>
        /// <param name="key">密钥字符串，长度为8或者24</param>
        public static void DecryptFile(string sourceFile, string targetFile, string key)
        {
            sourceFile.CheckFileExists("sourceFile");
            targetFile.CheckNotNullOrEmpty("targetFile");
            key.CheckNotNullOrEmpty("key");

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            DesHelper des = new DesHelper(keyBytes);
            des.DecryptFile(sourceFile, targetFile);
        }

        #endregion
    }
}

namespace System
{
    public class DESFileHelper
    {
        private const ulong FC_TAG = 0xFC010203040506CF;

        private const int BUFFER_SIZE = 128 * 1024;

        /// <summary>
        /// 检验两个Byte数组是否相同
        /// </summary>
        /// <param name="b1">Byte数组</param>
        /// <param name="b2">Byte数组</param>
        /// <returns>true－相等</returns>
        private static bool CheckByteArrays(byte[] b1, byte[] b2)
        {
            if (b1.Length == b2.Length)
            {
                for (int i = 0; i < b1.Length; ++i)
                {
                    if (b1[i] != b2[i])
                        return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 创建DebugLZQ
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="salt"></param>
        /// <returns>加密对象</returns>
        private static SymmetricAlgorithm CreateRijndael(string password, byte[] salt)
        {
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, salt, "SHA256", 1000);

            SymmetricAlgorithm sma = Rijndael.Create();
            sma.KeySize = 256;
            sma.Key = pdb.GetBytes(32);
            sma.Padding = PaddingMode.PKCS7;
            return sma;
        }

        /// <summary>
        /// 加密文件随机数生成
        /// </summary>
        private static RandomNumberGenerator rand = new RNGCryptoServiceProvider();

        /// <summary>
        /// 生成指定长度的随机Byte数组
        /// </summary>
        /// <param name="count">Byte数组长度</param>
        /// <returns>随机Byte数组</returns>
        private static byte[] GenerateRandomBytes(int count)
        {
            byte[] bytes = new byte[count];
            rand.GetBytes(bytes);
            return bytes;
        }

        /// <summary>
        /// 加密文件
        /// </summary>
        /// <param name="inFile">待加密文件</param>
        /// <param name="outFile">加密后输入文件</param>
        /// <param name="password">加密密码</param>
        public static bool EncryptFile(string inFile, string outFile, string password)
        {
            try
            {
                using (FileStream fin = File.OpenRead(inFile),
                    fout = File.OpenWrite(outFile))
                {
                    long lSize = fin.Length; // 输入文件长度
                    int size = (int)lSize;
                    byte[] bytes = new byte[BUFFER_SIZE]; // 缓存
                    int read = -1; // 输入文件读取数量
                    int value = 0;

                    // 获取IV和salt
                    byte[] IV = GenerateRandomBytes(16);
                    byte[] salt = GenerateRandomBytes(16);

                    // 创建加密对象
                    SymmetricAlgorithm sma = CreateRijndael(password, salt);
                    sma.IV = IV;

                    // 在输出文件开始部分写入IV和salt
                    fout.Write(IV, 0, IV.Length);
                    fout.Write(salt, 0, salt.Length);

                    // 创建散列加密
                    HashAlgorithm hasher = SHA256.Create();
                    using (CryptoStream cout = new CryptoStream(fout, sma.CreateEncryptor(), CryptoStreamMode.Write),
                        chash = new CryptoStream(Stream.Null, hasher, CryptoStreamMode.Write))
                    {
                        BinaryWriter bw = new BinaryWriter(cout);
                        bw.Write(lSize);

                        bw.Write(FC_TAG);

                        // 读写字节块到加密流缓冲区
                        while ((read = fin.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            cout.Write(bytes, 0, read);
                            chash.Write(bytes, 0, read);
                            value += read;
                        }
                        // 关闭加密流
                        chash.Flush();
                        chash.Close();

                        // 读取散列
                        byte[] hash = hasher.Hash;

                        // 输入文件写入散列
                        cout.Write(hash, 0, hash.Length);

                        // 关闭文件流
                        cout.Flush();
                        cout.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 解密文件
        /// </summary>
        /// <param name="inFile">待解密文件</param>
        /// <param name="outFile">解密后输出文件</param>
        /// <param name="password">解密密码</param>
        public static bool DecryptFile(string inFile, string outFile, string password)
        {
            try
            {
                // 创建打开文件流
                using (FileStream fin = File.OpenRead(inFile),
                    fout = File.OpenWrite(outFile))
                {
                    int size = (int)fin.Length;
                    byte[] bytes = new byte[BUFFER_SIZE];
                    int read = -1;
                    int value = 0;
                    int outValue = 0;

                    byte[] IV = new byte[16];
                    fin.Read(IV, 0, 16);
                    byte[] salt = new byte[16];
                    fin.Read(salt, 0, 16);

                    SymmetricAlgorithm sma = CreateRijndael(password, salt);
                    sma.IV = IV;

                    value = 32;
                    long lSize = -1;

                    // 创建散列对象, 校验文件
                    HashAlgorithm hasher = SHA256.Create();

                    using (CryptoStream cin = new CryptoStream(fin, sma.CreateDecryptor(), CryptoStreamMode.Read),
                        chash = new CryptoStream(Stream.Null, hasher, CryptoStreamMode.Write))
                    {
                        // 读取文件长度
                        BinaryReader br = new BinaryReader(cin);
                        lSize = br.ReadInt64();
                        ulong tag = br.ReadUInt64();

                        if (FC_TAG != tag)
                        {
                            //MessageBox.Show("文件被破坏或密码错误");
                            return false;
                        }

                        long numReads = lSize / BUFFER_SIZE;

                        long slack = (long)lSize % BUFFER_SIZE;

                        for (int i = 0; i < numReads; ++i)
                        {
                            read = cin.Read(bytes, 0, bytes.Length);
                            fout.Write(bytes, 0, read);
                            chash.Write(bytes, 0, read);
                            value += read;
                            outValue += read;
                        }

                        if (slack > 0)
                        {
                            read = cin.Read(bytes, 0, (int)slack);
                            fout.Write(bytes, 0, read);
                            chash.Write(bytes, 0, read);
                            value += read;
                            outValue += read;
                        }

                        chash.Flush();
                        chash.Close();

                        fout.Flush();
                        fout.Close();

                        byte[] curHash = hasher.Hash;

                        // 获取比较和旧的散列对象
                        byte[] oldHash = new byte[hasher.HashSize / 8];
                        read = cin.Read(oldHash, 0, oldHash.Length);
                        if ((oldHash.Length != read) || (!CheckByteArrays(oldHash, curHash)))
                        {
                            //MessageBox.Show("文件被破坏或密码错误");
                            return false;
                        }
                    }

                    if (outValue != lSize)
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}