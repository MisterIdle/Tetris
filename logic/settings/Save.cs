using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Raylib_CsLo;

using Tetris.Block;
using Tetris.Scene;

namespace Tetris.Options
{
    public class Save
    {
        public static string path = "json/save/game.save";

        // Not in a .env file for simplicity ;)
        private static readonly byte[] key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
        private static readonly byte[] iv = Encoding.UTF8.GetBytes("1234567890123456");

        // Saves the game state to a file
        public static void SaveGame(GameScene gameScene)
        {
            SaveData saveData = new SaveData
            {
                score = gameScene.score,
                level = gameScene.level,
                lines = gameScene.lines,

                grid = gameScene.grid,
                shadow = gameScene.shadow,
                placedBlocks = gameScene.placedBlocks,
                nextBlock = gameScene.nextBlock,
                randomMovement = gameScene.randomMovement,

                normal = gameScene.normal,
                gnowius = gameScene.gnowius,

                colorGrid = gameScene.colorGrid,
                currentTetromino = gameScene.currentTetromino,
                nextTetromino = gameScene.nextTetromino
            };

            string jsonString = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            string encryptedData = EncryptString(jsonString);
            File.WriteAllText(path, encryptedData);
        }

        // Loads the game state from a file
        public static SaveData LoadGame()
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Save file not found!");

            string encryptedData = File.ReadAllText(path);
            string jsonString = DecryptString(encryptedData);
            SaveData saveData = JsonConvert.DeserializeObject<SaveData>(jsonString);
            return saveData;
        }

        // Encrypts a string using AES encryption
        private static string EncryptString(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
        
        // Decrypts a string using AES encryption
        private static string DecryptString(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

    public class SaveData
    {
        public int score { get; set; }
        public int level { get; set; }
        public int lines { get; set; }
        public int[,] grid { get; set; }
        public bool shadow { get; set; }
        public bool placedBlocks { get; set; }
        public bool nextBlock { get; set; }
        public bool randomMovement { get; set; }
        public bool normal { get; set; }
        public bool gnowius { get; set; }
        public Color[,] colorGrid { get; set; }
        public Tetromino currentTetromino { get; set; }
        public Tetromino nextTetromino { get; set; }
    }
}
