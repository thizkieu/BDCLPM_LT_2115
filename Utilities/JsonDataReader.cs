using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace FootballSeleniumTest.Utilities
{
    public class JsonDataReader
    {
        public static List<T> ReadTestData<T>(string fullPath)
        {
            try
            {
                if (!File.Exists(fullPath))
                {
                    Console.WriteLine($"Không tìm thấy file JSON: {fullPath}");
                    return new List<T>();
                }

                string json = File.ReadAllText(fullPath);
                var data = JsonConvert.DeserializeObject<List<T>>(json);

                if (data == null) return new List<T>();

                Console.WriteLine($"--- Đã nạp thành công {data.Count} kịch bản test ---");
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đọc JSON: {ex.Message}");
                return new List<T>();
            }
        }
    }
}