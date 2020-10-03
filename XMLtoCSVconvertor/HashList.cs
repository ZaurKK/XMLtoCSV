using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace XMLtoCSVconvertor
{
    public class HashData
    {
        public DateTime ReceivedDateTime { get; set; }
        public string FileName { get; set; }
        public string HashString { get; set; }
        public string HashType { get; set; }

        public bool IsValid { get; set; } = false;

        private static MD5 md5Hash = MD5.Create();

        public HashData(string data)
        {
            string[] splittedString = data.Replace("\"", "").Split(',');
            if (splittedString.Length > 0)
            {
                DateTime dateTime;
                if (DateTime.TryParse(splittedString[0], out dateTime))
                    ReceivedDateTime = dateTime;
                else
                    return;
            }
            if (splittedString.Length > 1)
                FileName = splittedString[1];
            if (splittedString.Length > 2)
                HashString = splittedString[2];
            if (splittedString.Length > 3)
                HashType = splittedString[3];
            IsValid = true;
        }

        static string GetMd5Hash(byte[] data)
        {
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

        static string GetMd5Hash(string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            return GetMd5Hash(data);
        }

        static string GetMd5Hash(Stream input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(input);

            return GetMd5Hash(data);
        }

        // Verify a hash against a string.
        public static bool VerifyMd5Hash(string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(input);

            //// Create a StringComparer and compare the hashes.
            //StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            //return comparer.Compare(hashOfInput, hash) == 0;
            return string.CompareOrdinal(hashOfInput, hash) == 0;
        }

        // Verify a hash against a string.
        public static bool VerifyMd5Hash(Stream input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(input);

            //// Create a StringComparer and compare the hashes.
            //StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            //return comparer.Compare(hashOfInput, hash) == 0;
            return string.CompareOrdinal(hashOfInput, hash) == 0;
        }
    }

    public class HashList
    {
        DateTime MinDateTime { get; set; } = new DateTime(2020, 09, 03);
        private List<HashData> HashDataList { get; set; } = new List<HashData>();

        public HashList()
        {
            var hashFilePaths = Directory.GetFiles("HashFiles/", "*.csv", SearchOption.TopDirectoryOnly).ToList();
            if (hashFilePaths.Count == 0)
                return;

            DateTime lastCreated = DateTime.MinValue;
            string latestHashFilePath = string.Empty;
            foreach (var path in hashFilePaths)
            {
                var created = File.GetCreationTime(path);
                if (created > lastCreated)
                {
                    lastCreated = created;
                    latestHashFilePath = path;
                }
            }

            var hashStrings = File.ReadAllLines(latestHashFilePath).ToList();
            foreach (var hashString in hashStrings)
            {
                var hashData = new HashData(hashString);
                if (hashData.IsValid)
                    HashDataList.Add(hashData);
            }
        }

        public bool IsFileValid(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var fileName = Path.GetFileName(filePath);
            var hashData = HashDataList.FirstOrDefault(data => data.FileName.Equals(fileName));
            if (hashData == null)
                return false;

            if (hashData.ReceivedDateTime < MinDateTime)
                return false;

            var stream = File.OpenRead(filePath);
            var validMD5Hash = HashData.VerifyMd5Hash(stream, hashData.HashString);
            stream.Close();
            return validMD5Hash;
        }
    }
}
