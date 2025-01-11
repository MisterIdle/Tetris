using System;
using Newtonsoft.Json;
using System.IO;

namespace Tetris {
    public static class Settings {
        public static bool shadow = true;
        public static bool placedBlocks = true;
        public static bool nextBlock = true;
        public static bool randomMovement = false;

        public static int level = 1;

        public static float masterVolume = 1.0f;
        public static float musicVolume = 1.0f;
        public static float sfxVolume = 1.0f;

        private static readonly string directoryPath = "json";
        private static readonly string filePath = Path.Combine(directoryPath, "settings.json");

        public static void Switch(ref bool value) {
            value = !value;
            SaveSettings();
        }

        public static void SaveSettings() {
            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }

            string json = JsonConvert.SerializeObject(new SettingsData {
                shadow = shadow,
                placedBlocks = placedBlocks,
                nextBlock = nextBlock,
                randomMovement = randomMovement,
                level = level,
                masterVolume = masterVolume,
                musicVolume = musicVolume,
                sfxVolume = sfxVolume
            }, Formatting.Indented);

            File.WriteAllText(filePath, json);
        }

        public static void LoadSettings() {
            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(filePath)) {
                SaveSettings();
                return;
            }

            try {
                string json = File.ReadAllText(filePath);
                SettingsData data = JsonConvert.DeserializeObject<SettingsData>(json);

                shadow = data.shadow;
                placedBlocks = data.placedBlocks;
                nextBlock = data.nextBlock;
                randomMovement = data.randomMovement;
                level = data.level;
                masterVolume = data.masterVolume;
                musicVolume = data.musicVolume;
                sfxVolume = data.sfxVolume;
            } catch (Exception e) {
                Console.WriteLine($"Error loading settings: {e.Message}");
                SaveSettings();
            }
        }

        public class SettingsData {
            public bool shadow { get; set; }
            public bool placedBlocks { get; set; }
            public bool nextBlock { get; set; }
            public bool randomMovement { get; set; }
            public int level { get; set; }
            public float masterVolume { get; set; }
            public float musicVolume { get; set; }
            public float sfxVolume { get; set; }
        }
    }
}