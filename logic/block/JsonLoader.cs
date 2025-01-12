using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Raylib_CsLo;  

namespace Tetris {
    public class JsonLoader
    {
        public static List<Tetromino> LoadFromJson(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            List<TetrominoData> tetrominoData = JsonConvert.DeserializeObject<List<TetrominoData>>(jsonString);

            List<Tetromino> blocks = new List<Tetromino>();

            foreach (TetrominoData data in tetrominoData)
            {
                blocks.Add(new Tetromino(null, 0, 0, data.Shape, new Color(data.Color[0], data.Color[1], data.Color[2], 255)));
            }

            return blocks;
        }
    }
    
    
    public class TetrominoData
    {
        public string Name { get; set; }
        public int[] Color { get; set; }
        public int[,] Shape { get; set; }
    }
}