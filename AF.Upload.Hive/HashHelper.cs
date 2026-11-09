using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AF.Upload.Hive
{
    public class HashHelper
    {
        /// <summary>
        /// 计算文件的MD5哈希值
        /// </summary>
        public static string GetFileMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        /// <summary>
        /// 计算文件的SHA1哈希值
        /// </summary>
        public static string GetFileSHA1(string filePath)
        {
            using (var sha1 = SHA1.Create())
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    byte[] hash = sha1.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        /// <summary>
        /// 计算文件的SHA256哈希值
        /// </summary>
        public static string GetFileSHA256(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    byte[] hash = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        /// <summary>
        /// 通用的文件哈希计算方法
        /// </summary>
        public static string GetFileHash(string filePath, HashAlgorithm algorithm)
        {
            using (var stream = System.IO.File.OpenRead(filePath))
            {
                byte[] hash = algorithm.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }


        # region 高性能哈希计算
        //高性能哈希计算（避免重复创建HashAlgorithm实例）
        private static readonly ThreadLocal<MD5> _md5 = new ThreadLocal<MD5>(() => MD5.Create());
        private static readonly ThreadLocal<SHA1> _sha1 = new ThreadLocal<SHA1>(() => SHA1.Create());
        private static readonly ThreadLocal<SHA256> _sha256 = new ThreadLocal<SHA256>(() => SHA256.Create());

        public static string FastFileMD5(string filePath)
        {
            using (var stream = System.IO.File.OpenRead(filePath))
            {
                byte[] hash = _md5.Value.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        public static string FastFileSHA1(string filePath)
        {
            using (var stream = System.IO.File.OpenRead(filePath))
            {
                byte[] hash = _sha1.Value.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        public static string FastStringSHA256(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = _sha256.Value.ComputeHash(inputBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        #endregion




        /// <summary>
        /// 计算字符串的MD5哈希值
        /// </summary>
        public static string GetStringMD5(string input)
        {
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// 计算字符串的SHA1哈希值
        /// </summary>
        public static string GetStringSHA1(string input)
        {
            using (var sha1 = SHA1.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha1.ComputeHash(inputBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// 计算字符串的SHA256哈希值
        /// </summary>
        public static string GetStringSHA256(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha256.ComputeHash(inputBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// 通用的字符串哈希计算方法
        /// </summary>
        public static string GetStringHash(string input, HashAlgorithm algorithm)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

    }
}


