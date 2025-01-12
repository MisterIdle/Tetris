using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Raylib_CsLo;

namespace Tetris.Block
{
    public class JsonLoader
    {
        // Load Tetrominos from a JSON file
        public static List<Tetromino> LoadFromJson(string filePath)
        {
            // Read JSON content
            string jsonString = File.ReadAllText(filePath);

            // Deserialize JSON content
            List<TetrominoData> tetrominoData = JsonConvert.DeserializeObject<List<TetrominoData>>(jsonString);

            // List to store Tetromino objects
            List<Tetromino> blocks = new List<Tetromino>();

            // Iterate through TetrominoData objects
            foreach (TetrominoData data in tetrominoData)
            {
                // Validate data
                if (data.Shape == null || data.Color == null || data.Color.Length != 3)
                {
                    continue;
                }

                // Create Tetromino object
                Color color = new Color(data.Color[0], data.Color[1], data.Color[2], 255);
                blocks.Add(new Tetromino(null, 0, 0, data.Shape, color));
            }

            return blocks;
        }
    }

    // Data class for Tetrominos
    public class TetrominoData
    {
        // Property name is intended for use in a future piece editor
        public string Name { get; set; }
        public int[] Color { get; set; } 
        public int[,] Shape { get; set; }
    }
}
