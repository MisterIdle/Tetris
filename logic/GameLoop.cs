using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Raylib_CsLo;

namespace Tetris
{
    class GameLoop
    {
        public const int screenWidth = 800;
        public const int screenHeight = 600;
        public const int gridWidth = 10;
        public const int gridHeight = 20;
        public const int cellSize = 30;
        public const int marginX = 50;
        public const int marginY = 0;

        public List<Block> blocks;
        public Block currentBlock;
        public Block nextBlock;


        public GameLoop()
        {
            blocks = Block.LoadFromJson("json/blocks.json");
            currentBlock = GenerateRandomBlock();
            nextBlock = GenerateRandomBlock();
        }

        public void Run()
        {
            Raylib.InitWindow(screenWidth, screenHeight, "Tetris Game");

            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Raylib.RAYWHITE);

                Draw();

                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }

        public void Draw() {
            DrawGrid();
            currentBlock.DrawTetromino();
        }

        //////////
        // Grid //
        //////////
        public void DrawGrid()
        {
            for (int i = 0; i < gridWidth; i++)
            {
                for (int j = 0; j < gridHeight; j++)
                {
                    Raylib.DrawRectangle(marginX + i * cellSize, marginY + j * cellSize, cellSize, cellSize, Raylib.LIGHTGRAY);
                    Raylib.DrawRectangleLines(marginX + i * cellSize, marginY + j * cellSize, cellSize, cellSize, Raylib.DARKGRAY);
                }
            }
        }

        ///////////////
        // Tetromino //
        ///////////////
        public Block GenerateRandomBlock()
        {
            Random random = new Random();
            int index = random.Next(blocks.Count);
            Block randomBlock = blocks[index];

            return new Block(gridWidth / 2 - randomBlock.shape.GetLength(1) / 2, 0, randomBlock.shape, randomBlock.color);
        }

    }

    class Block
    {
        public int x;
        public int y;
        public int[,] shape;
        public Color color;

        public Block(int x, int y, int[,] shape, Color color)
        {
            this.x = x;
            this.y = y;
            this.shape = shape;
            this.color = color;
        }

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

        // .NET 5.0 requires a conversion because [,] is not supported. (Developer note 😉)
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


        public void DrawTetromino()
        {
            for (int i = 0; i < shape.GetLength(0); i++)
            {
                for (int j = 0; j < shape.GetLength(1); j++)
                {
                    if (shape[i, j] == 1)
                    {
                        Raylib.DrawRectangle(GameLoop.marginX + (x + j) * GameLoop.cellSize, GameLoop.marginY + (y + i) * GameLoop.cellSize, GameLoop.cellSize, GameLoop.cellSize, color);
                        Raylib.DrawRectangleLines(GameLoop.marginX + (x + j) * GameLoop.cellSize, GameLoop.marginY + (y + i) * GameLoop.cellSize, GameLoop.cellSize, GameLoop.cellSize, Raylib.DARKGRAY);
                    }
                }
            }
        }
    }


    class TetrominoData
    {
        public string Name { get; set; }
        public int[] Color { get; set; }
        public int[][] Shape { get; set; }
    }
}