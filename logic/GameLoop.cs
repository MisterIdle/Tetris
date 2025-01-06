using System;
using System.IO;
using System.Text.Json;
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
        public static int[,] grid = new int[GRID_HEIGHT, GRID_WIDTH];
        public static Color[,] colorGrid = new Color[GRID_HEIGHT, GRID_WIDTH];

        public static Block currentBlock;
        public static Block nextBlock;

        public GameLoop()
        {
            blocks = new List<Block>();
            currentBlock = GenerateRandomBlock();
            nextBlock = GenerateRandomBlock();
            InitializeGrid();
        }

        public static void InitializeGrid()
        {
            for (int i = 0; i < GRID_HEIGHT; i++)
            {
                for (int j = 0; j < GRID_WIDTH; j++)
                {
                    grid[i, j] = 0;
                    colorGrid[i, j] = Raylib.LIGHTGRAY;
                }
            }
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

        // INPUT ET MOUVEMENT
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
            CheckLines();
        }

        public void Draw()
        {
            DrawGrid();
            DrawPlacedBlocks();
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

            Raylib.DrawRectangle(SCREEN_WIDTH - 200, 0, 200, SCREEN_HEIGHT, Raylib.DARKGRAY);
            Raylib.DrawRectangleLines(SCREEN_WIDTH - 200, 0, 200, SCREEN_HEIGHT, Raylib.BLACK);
        }

        public void DrawPlacedBlocks()
        {
            for (int i = 0; i < GRID_HEIGHT; i++)
            {
                for (int j = 0; j < GRID_WIDTH; j++)
                {
                    if (grid[i, j] != 0) 
                    {
                        Raylib.DrawRectangle(MARGIN_X + j * CELL_SIZE, MARGIN_Y + i * CELL_SIZE, CELL_SIZE, CELL_SIZE, colorGrid[i, j]);
                        Raylib.DrawRectangleLines(MARGIN_X + j * CELL_SIZE, MARGIN_Y + i * CELL_SIZE, CELL_SIZE, CELL_SIZE, Raylib.DARKGRAY);
                    }
                }
            }
        }

        public void CheckLines()
        {
            for (int i = GRID_HEIGHT - 1; i >= 0; i--)
            {
                bool isLineFull = true;
                for (int j = 0; j < GRID_WIDTH; j++)
                {
                    if (grid[i, j] == 0)
                    {
                        isLineFull = false;
                        break;
                    }
                }

                if (isLineFull)
                {
                    RemoveLine(i);
                    ShiftLinesDown(i);
                    i++;
                }
            }
        }

        public void RemoveLine(int lineIndex)
        {
            for (int j = 0; j < GRID_WIDTH; j++)
            {
                grid[lineIndex, j] = 0;
                colorGrid[lineIndex, j] = Raylib.LIGHTGRAY;
            }
        }

        public void ShiftLinesDown(int startLine)
        {
            for (int i = startLine; i > 0; i--)
            {
                for (int j = 0; j < GRID_WIDTH; j++)
                {
                    grid[i, j] = grid[i - 1, j];
                    colorGrid[i, j] = colorGrid[i - 1, j];
                }
            }
        }

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
                int newY = y + 1;

                if (newY < GameLoop.GRID_HEIGHT && !CheckCollision(newY))
                {
                    y++;
                }
                else
                {
                    PlaceBlock();
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

        public void PlaceBlock()
        {
            for (int i = 0; i < shape.GetLength(0); i++)
            {
                for (int j = 0; j < shape.GetLength(1); j++)
                {
                    if (shape[i, j] == 1)
                    {
                        int newX = x + j;
                        int newY = y + i;

                        if (newX >= 0 && newX < GameLoop.GRID_WIDTH && newY >= 0 && newY < GameLoop.GRID_HEIGHT)
                        {
                            GameLoop.grid[newY, newX] = 1;
                            GameLoop.colorGrid[newY, newX] = color;
                        }
                    }
                }
            }

            GameLoop.currentBlock = GameLoop.nextBlock;
            GameLoop.nextBlock = GameLoop.GenerateRandomBlock();
        }

        public bool CheckCollision(int? newY = null)
        {
            int checkY = newY ?? y;

            for (int i = 0; i < shape.GetLength(0); i++)
            {
                for (int j = 0; j < shape.GetLength(1); j++)
                {
                    if (shape[i, j] == 1)
                    {
                        int checkX = x + j;
                        int checkYLocal = checkY + i;

                        if (checkYLocal >= GameLoop.GRID_HEIGHT || checkX < 0 || checkX >= GameLoop.GRID_WIDTH || GameLoop.grid[checkYLocal, checkX] != 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    public class TetrominoData
    {
        public string Name { get; set; }
        public int[] Color { get; set; }
        public int[][] Shape { get; set; }
    }
}
