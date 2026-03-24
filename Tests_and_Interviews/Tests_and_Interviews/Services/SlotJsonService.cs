using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Tests_and_Interviews.Models;

namespace Tests_and_Interviews.Services
{
    public static class SlotJsonService
    {
        private static string filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "slots.json"
        );

        public static List<Slot> LoadSlots()
        {
            if (!File.Exists(filePath))
                return new List<Slot>();

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<Slot>>(json) ?? new List<Slot>();
        }

        public static void SaveSlots(List<Slot> slots)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(slots, options);
            File.WriteAllText(filePath, json);

            System.Diagnostics.Debug.WriteLine("JSON saved at: " + filePath);
        }
    }
}