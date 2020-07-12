// MD5Utilities.cs - 12/26/2019

using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CopyDiff
{
    public class MD5Utilities
    {
        public static string CalcMD5FromFile(string filepath)
        {
            using (FileStream fs = File.OpenRead(filepath))
            {
                return CalcMD5FromStream(fs);
            }
        }

        public static string CalcMD5FromString(string value)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(value);
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                return CalcMD5FromStream(ms);
            }
        }

        public static string CalcMD5FromStream(Stream value)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] result = md5Hasher.ComputeHash(value);
            StringBuilder hexResult = new StringBuilder();
            foreach (byte b in result)
            {
                hexResult.Append(b.ToString("x2"));
            }
            return hexResult.ToString();
        }
    }
}
