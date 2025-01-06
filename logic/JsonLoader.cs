
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Raylib_CsLo;  

namespace Tetris {
    class JsonLoader
    {
        public static List<Block> LoadFromJson(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            List<TetrominoData> tetrominoData = JsonConvert.DeserializeObject<List<TetrominoData>>(jsonString);

            List<Block> blocks = new List<Block>();

            foreach (TetrominoData data in tetrominoData)
            {
                blocks.Add(new Block(0, 0, data.Shape, new Color(data.Color[0], data.Color[1], data.Color[2], 255)));
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