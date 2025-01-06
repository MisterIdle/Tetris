
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Raylib_CsLo;    

namespace Tetris {
    class JsonLoader
    {
        public static List<Block> LoadFromJson(string filePath)
        {
            string jsonContent = File.ReadAllText(filePath);
            var blocksData = JsonSerializer.Deserialize<List<TetrominoData>>(jsonContent);
            var blocks = new List<Block>();
            foreach (var data in blocksData)
            {
                Color blockColor = new Color(data.Color[0], data.Color[1], data.Color[2], 255);
                int[,] shape = ConvertTo2DArray(data.Shape);
                blocks.Add(new Block(0, 0, shape, blockColor));
            }
            return blocks;
        }
        
        private static int[,] ConvertTo2DArray(int[][] jaggedArray)
        {
            int rows = jaggedArray.Length;
            int cols = jaggedArray[0].Length;
            int[,] array = new int[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    array[i, j] = jaggedArray[i][j];
                }
            }
            return array;
        }
    }
    
    
    public class TetrominoData
    {
        public string Name { get; set; }
        public int[] Color { get; set; }
        public int[][] Shape { get; set; }
    }
}