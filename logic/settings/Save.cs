using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Raylib_CsLo;

namespace Tetris
{
    public class Save
    {
        public static string path = "json/save/game.save";

        private static readonly byte[] key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
        private static readonly byte[] iv = Encoding.UTF8.GetBytes("1234567890123456");


        public static void SaveGame(GameScene gameScene)
        {
            SaveData saveData = new SaveData
            {
                score = gameScene.score,
                level = gameScene.level,
                linesCleared = gameScene.lines,
                grid = gameScene.grid,
                colorGrid = gameScene.colorGrid,
                currentBlock = gameScene.currentTetromino,
                nextBlock = gameScene.nextTetromino
            };

            string jsonString = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            string encryptedData = EncryptString(jsonString);
            File.WriteAllText(path, encryptedData);
        }


        public static SaveData LoadGame()
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Save file not found!");

            string encryptedData = File.ReadAllText(path);
            string jsonString = DecryptString(encryptedData);
            SaveData saveData = JsonConvert.DeserializeObject<SaveData>(jsonString);
            return saveData;
        }

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
        public int linesCleared { get; set; }
        public int[,] grid { get; set; }
        public Color[,] colorGrid { get; set; }
        public Tetromino currentBlock { get; set; }
        public Tetromino nextBlock { get; set; }
    }
}
