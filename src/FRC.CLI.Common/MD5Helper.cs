using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FRC.CLI.Common
{
    public static class MD5Helper
    {
        public static async Task<string> Md5SumAsync(string fileName)
        {
            byte[] fileMd5Sum = null;


            if (File.Exists(fileName))
            {
                using (FileStream stream = new FileStream(fileName, FileMode.Open))
                {
                    using (MD5 md5 = MD5.Create())
                    {
                        await Task.Run(() =>
                        {
                            fileMd5Sum = md5.ComputeHash(stream);
                        }).ConfigureAwait(false);
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