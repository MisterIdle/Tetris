using Raylib_CsLo;
using System.IO;
using Newtonsoft.Json;

namespace Tetris {
    public class Save
    {
        public static string path = "json/save/save.json";

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
            File.WriteAllText(path, jsonString);
        }

        public static SaveData LoadGame()
        {
            string jsonString = File.ReadAllText(path);
            SaveData saveData = JsonConvert.DeserializeObject<SaveData>(jsonString);
            return saveData;
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