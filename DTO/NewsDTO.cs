﻿using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DTO
{
    [Serializable]
    public class NewsDTO
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime DateOfPublication { get; set; }
        // public string Description { get; set; }
        public string Url { get; set; }
        //public bool Like { get; set; }
        //public bool Dislike { get; set; }

        public override string ToString()
        {
            return string.Format("{0}{1}Title:  {2}{1}Author:  {3}{1}Time:  {4}{1}   {0}",
                "----------------------------",
                Environment.NewLine,
                Title,
                Author,
                // Description,
                DateOfPublication.ToLongTimeString()
                // Like,
                // Dislike
           );
        }

        public string GetStr()
        {
            return $"{ID}{Environment.NewLine}" +
                $"{Title} {Environment.NewLine}" +
                $"{Author} {Environment.NewLine}" +
                // $"{Description} {Environment.NewLine}" +
                $"{DateOfPublication.ToLongTimeString()} {Environment.NewLine}" +
                // $"{Like} {Environment.NewLine}" +
                // $"{Dislike} {Environment.NewLine}" +
                $"---";
        }


    }

    public class BinaryConverter
    {
        public static byte[] ObjectToByteArray<T>(T obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static T ByteArrayToObject<T>(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream(arrBytes))
            {
                var binForm = new BinaryFormatter();
                var obj = (T)binForm.Deserialize(memStream);
                return obj;
            }
        }
    }
}
