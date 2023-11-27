using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TranslationRunner
{
    public class Utils
    {
        public static string GetFileMD5(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";

            if (!File.Exists(path))
                return "";

            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    byte[] hashBytes = md5.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
        }

    }

}
