using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XMLtoCSVconvertor
{
    class HashData
    {
        public DateTime ReceivedDateTime { get; set; }
        public string FileName { get; set; }
        public string HashString { get; set; }
        public string HashType { get; set; }

        private static MD5 md5Hash = MD5.Create();

        public HashData(string data)
        {
            string[] splittedString = data.Split(',');
            if (splittedString.Length > 0)
                ReceivedDateTime = DateTime.Parse(splittedString[0]);
            if (splittedString.Length > 1)
                FileName = splittedString[1];
            if (splittedString.Length > 2)
                HashString = GetMd5Hash(splittedString[2]);
            if (splittedString.Length > 3)
                HashType = splittedString[3];
        }

        static string GetMd5Hash(string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        static bool VerifyMd5Hash(string input, string hash)    
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(input);

            // Create a StringComparer and compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return (0 == comparer.Compare(hashOfInput, hash));
        }
    }

    public class HashList
    {
        //private 

        public HashList(string []hashStrings)
        {
            foreach (var hashString in hashStrings)
            {

            }
        }
    }
}
