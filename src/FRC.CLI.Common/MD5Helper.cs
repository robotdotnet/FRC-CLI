using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FRC.CLI.Common
{
    public static class MD5Helper
    {
        public static string Md5Sum(string fileName)
        {
            byte[] fileMd5Sum = null;


            if (File.Exists(fileName))
            {
                using (FileStream stream = new FileStream(fileName, FileMode.Open))
                {
                    using (MD5 md5 = MD5.Create())
                    {
                        fileMd5Sum = md5.ComputeHash(stream);
                    }
                }
            }

            if (fileMd5Sum == null)
            {
                return null;
            }

            StringBuilder builder = new StringBuilder();
            foreach (var b in fileMd5Sum)
            {
                builder.Append(b);
            }
            return builder.ToString();
        }
    }
}