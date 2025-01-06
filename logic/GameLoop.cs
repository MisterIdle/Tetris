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
        public const int SCREEN_WIDTH = 800;
        public const int SCREEN_HEIGHT = 600;
        public const int GRID_WIDTH = 10;
        public const int GRID_HEIGHT = 20;
        public const int CELL_SIZE = 30;
        public const int MARGIN_X = 50;
        public const int MARGIN_Y = 0;

        public static float deltaTime = 0.0f;

        public static List<Block> blocks;
        public static Block currentBlock;
        public static Block nextBlock;

        public GameLoop()
        {
            blocks = new List<Block>();
            currentBlock = GenerateRandomBlock();
            nextBlock = GenerateRandomBlock();
        }

        public void Run()
        {
            Raylib.InitWindow(SCREEN_WIDTH, SCREEN_HEIGHT, "Tetris Game");

            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Raylib.RAYWHITE);

                deltaTime = Raylib.GetFrameTime();

                Input();
                Update();
                Draw();

                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }

        // INPUT AND DRAWING //
        public void Input()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_UP))
                currentBlock.RotateTetromino();

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_LEFT))
                currentBlock.MoveTetromino(-1, 0);

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_RIGHT))
                currentBlock.MoveTetromino(1, 0);

            if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN))
                currentBlock.SetFallSpeed(0.07f);
            else
                currentBlock.SetFallSpeed(1.0f);
        }

        public void Update()
        {
            currentBlock.FallTetromino();
        }

        public void Draw()
        {
            DrawGrid();

            foreach (var block in blocks)
            {
                block.DrawTetromino();
            }

            currentBlock.DrawTetromino();
        }

        public void DrawGrid()
        {
            for (int i = 0; i < GRID_HEIGHT; i++)
            {
                for (int j = 0; j < GRID_WIDTH; j++)
                {
                    Raylib.DrawRectangle(MARGIN_X + j * CELL_SIZE, MARGIN_Y + i * CELL_SIZE, CELL_SIZE, CELL_SIZE, Raylib.LIGHTGRAY);
                    Raylib.DrawRectangleLines(MARGIN_X + j * CELL_SIZE, MARGIN_Y + i * CELL_SIZE, CELL_SIZE, CELL_SIZE, Raylib.DARKGRAY);
                }
            }
        }

        // BLOCK GENERATION //
        public static Block GenerateRandomBlock()
        {
            Random random = new Random();
            int index = random.Next(Block.LoadFromJson("json/blocks.json").Count);
            Block randomBlock = Block.LoadFromJson("json/blocks.json")[index];

            return new Block(GRID_WIDTH / 2 - randomBlock.shape.GetLength(1) / 2, 0, randomBlock.shape, randomBlock.color);
        }
    }

    class Block
    {
        public int x;
        public int y;
        public int[,] shape;
        public Color color;

        public float fallTimer = 0.0f;
        public float fallInterval = 1.0f;

        public Block(int x, int y, int[,] shape, Color color)
        {
            this.x = x;
            this.y = y;
            this.shape = shape;
            this.color = color;
        }

        // JSON LOADING //
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

        // DRAWING AND MOVEMENT //
        public void DrawTetromino()
        {
            for (int i = 0; i < shape.GetLength(0); i++)
            {
                for (int j = 0; j < shape.GetLength(1); j++)
                {
                    if (shape[i, j] == 1)
                    {
                        Raylib.DrawRectangle(GameLoop.MARGIN_X + (x + j) * GameLoop.CELL_SIZE, GameLoop.MARGIN_Y + (y + i) * GameLoop.CELL_SIZE, GameLoop.CELL_SIZE, GameLoop.CELL_SIZE, color);
                        Raylib.DrawRectangleLines(GameLoop.MARGIN_X + (x + j) * GameLoop.CELL_SIZE, GameLoop.MARGIN_Y + (y + i) * GameLoop.CELL_SIZE, GameLoop.CELL_SIZE, GameLoop.CELL_SIZE, Raylib.DARKGRAY);
                    }
                }
            }
        }

        public void MoveTetromino(int dx, int dy)
        {
            int prevX = x;
            int prevY = y;

            x += dx;
            y += dy;

            if (CheckCollision())
            {
                x = prevX;
                y = prevY;
            }
        }

        public void FallTetromino()
        {
            fallTimer += GameLoop.deltaTime;

            if (fallTimer >= fallInterval)
            {
                y++;

                if (CheckCollision())
                {
                    y--;
                    LockTetromino();
                    GameLoop.blocks.Add(this);
                    GameLoop.currentBlock = GameLoop.GenerateRandomBlock();
                }

                fallTimer = 0.0f;
            }
        }

        public void SetFallSpeed(float speed)
        {
            fallInterval = speed;
        }

        public void RotateTetromino()
        {
            int[,] rotatedShape = new int[shape.GetLength(1), shape.GetLength(0)];

            for (int i = 0; i < shape.GetLength(0); i++)
            {
                for (int j = 0; j < shape.GetLength(1); j++)
                {
                    rotatedShape[j, shape.GetLength(0) - 1 - i] = shape[i, j];
                }
            }

            int[,] prevShape = shape;
            shape = rotatedShape;

            if (CheckCollision())
            {
                shape = prevShape;
            }
        }

        // COLLISION DETECTION //
        public bool CheckCollision()
        {
            for (int i = 0; i < shape.GetLength(0); i++)
            {
                for (int j = 0; j < shape.GetLength(1); j++)
                {
                    if (shape[i, j] == 1)
                    {
                        int newX = x + j;
                        int newY = y + i;

                        if (newX < 0 || newX >= GameLoop.GRID_WIDTH || newY >= GameLoop.GRID_HEIGHT)
                        {
                            return true;
                        }

                        if (IsCellOccupied(newX, newY))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool IsCellOccupied(int x, int y)
        {
            foreach (var block in GameLoop.blocks)
            {
                for (int i = 0; i < block.shape.GetLength(0); i++)
                {
                    for (int j = 0; j < block.shape.GetLength(1); j++)
                    {
                        if (block.shape[i, j] == 1)
                        {
                            int blockX = block.x + j;
                            int blockY = block.y + i;
                            if (blockX == x && blockY == y)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public void LockTetromino()
        {
            GameLoop.blocks.Add(this);
            ClearCompleteLines();
        }

        public void ClearCompleteLines()
        {
            for (int row = 0; row < GameLoop.GRID_HEIGHT; row++)
            {
                bool isLineComplete = true;

                for (int col = 0; col < GameLoop.GRID_WIDTH; col++)
                {
                    if (IsCellOccupied(col, row) == false)
                    {
                        isLineComplete = false;
                        break;
                    }
                }

                if (isLineComplete)
                {
                    RemoveLine(row);
                }
            }
        }

        public void RemoveLine(int row)
        {
            for (int i = row; i > 0; i--)
            {
                for (int j = 0; j < GameLoop.GRID_WIDTH; j++)
                {
                    foreach (var block in GameLoop.blocks)
                    {
                        if (block.y == i - 1 && block.x == j)
                        {
                            block.y++;
                        }
                    }
                }
            }

            foreach (var block in GameLoop.blocks)
            {
                if (block.y == 0 && block.x == row)
                {
                    GameLoop.blocks.Remove(block);
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
