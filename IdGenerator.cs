using System;
using System.Security.Cryptography;

namespace Backend
{
    public class IdGenerator
    {
        public static string CreateId()
        {
            var randomGenerator = RandomNumberGenerator.Create();
            byte[] data1 = new byte[16];
            byte[] data2 = new byte[16];
            randomGenerator.GetBytes(data1);
            randomGenerator.GetBytes(data2);
            var guid1 = new Guid(data1);
            var guid2 = new Guid(data2);
            var encodedGUID1 = Convert.ToBase64String(guid1.ToByteArray()).Replace("/", "_").Replace("+", "-").Replace("=", "");
            var encodedGUID2 = Convert.ToBase64String(guid2.ToByteArray()).Replace("/", "_").Replace("+", "-").Replace("=", "");
            return encodedGUID1 + encodedGUID2;
        }
    }
}