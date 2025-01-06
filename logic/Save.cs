using Raylib_CsLo;
using System.IO;
using Newtonsoft.Json;

namespace Tetris {
    public class Save
    {
        public static void SaveGame(int score, int level, int linesCleared, int[,] grid, Color[,] colorGrid, Block currentBlock, Block nextBlock)
        {
            SaveData saveData = new SaveData
            {
                Score = score,
                Level = level,
                LinesCleared = linesCleared,
                Grid = grid,
                ColorGrid = colorGrid,
                CurrentBlock = currentBlock,
                NextBlock = nextBlock
            };

            string jsonString = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText("save.json", jsonString);
        }

        public static SaveData LoadGame()
        {
            string jsonString = File.ReadAllText("save.json");
            SaveData saveData = JsonConvert.DeserializeObject<SaveData>(jsonString);
            return saveData;
        }
    }

    public class SaveData
    {

        public int Score { get; set; }
        public int Level { get; set; }
        public int LinesCleared { get; set; }
        public int[,] Grid { get; set; }
        public Color[,] ColorGrid { get; set; }
        public Block CurrentBlock { get; set; }
        public Block NextBlock { get; set; }
    }
}
