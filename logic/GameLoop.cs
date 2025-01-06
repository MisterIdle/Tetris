using System;
using System.Collections.Generic;
using Raylib_CsLo;

namespace Tetris
{
    class GameLoop
    {
        public const int SCREEN_WIDTH = 550;
        public const int SCREEN_HEIGHT = 600;
        public const int GRID_WIDTH = 10;
        public const int GRID_HEIGHT = 20;
        public const int CELL_SIZE = 30;
        public const int MARGIN_X = 50;
        public const int MARGIN_Y = 0;

        public static float deltaTime = 0.0f;

        public static int blinkCount = 6;
        public static float blinkDuration = 0.1f;
        public Color blinkColor = new Color(255, 0, 0, 100);

        public static float normalFallSpeed = 1.0f;
        public static float fastFallSpeed = 0.07f;

        public static Color gridColor = new Color(31, 31, 88, 255);
        public static Color backgroundColor = Raylib.BLACK;
        public static Color textColor = Raylib.DARKBLUE;
        public static Color menuTitleColor = Raylib.DARKGREEN;
        public static Color shadowColor = new Color(0, 0, 0, 50);

        public static int textSize = 20;
        public static int titleSize = 40;
        public static int menuTitleOffsetX = -60;
        public static int menuTitleOffsetY = -100;
        public static int menuTextOffsetX = -150;
        public static int menuTextOffsetY = 0;

        public static bool isRobotPlaying = false;

        public static List<Block> blocks;
        public static int[,] grid = new int[GRID_HEIGHT, GRID_WIDTH];
        public static Color[,] colorGrid = new Color[GRID_HEIGHT, GRID_WIDTH];

        public static Block currentBlock;
        public static Block nextBlock;

        public static GameState currentState = GameState.Menu;
        public static int score = 0; 
        public static int level = 1;

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
                    colorGrid[i, j] = gridColor;
                }
            }
        }

        public void Run()
        {
            Raylib.InitWindow(SCREEN_WIDTH, SCREEN_HEIGHT, "Tetris Game | By MisterIdle");
            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(backgroundColor);
                deltaTime = Raylib.GetFrameTime();
                switch (currentState)
                {
                    case GameState.Menu:
                        DrawMenu();
                        HandleMenuInput();
                        break;
                    case GameState.Playing:
                        Input.InputManager();
                        Update();
                        Draw();
                        break;
                    case GameState.GameOver:
                        DrawGameOver();
                        Input.HandleGameOverInput();
                        break;
                }
                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }

        public void Update()
        {
            currentBlock.FallTetromino();
            CheckLines();
            CheckGameOver();
        }

        public void Draw()
        {
            DrawGrid();
            currentBlock.DrawPlacedBlocks();
            currentBlock.DrawShadow();

            DrawScore();
            DrawLevel();
            DrawNextBlock();

            currentBlock.DrawTetromino();
        }

        public void DrawGrid()
        {
            for (int i = 0; i < GRID_HEIGHT; i++)
            {
            for (int j = 0; j < GRID_WIDTH; j++)
            {
                Raylib.DrawRectangle(MARGIN_X + j * CELL_SIZE + 3, MARGIN_Y + i * CELL_SIZE + 3, CELL_SIZE, CELL_SIZE, gridColor);
            }
            }

            Raylib.DrawRectangle(MARGIN_X - 3, MARGIN_Y, 3, GRID_HEIGHT * CELL_SIZE, Raylib.WHITE);
            Raylib.DrawRectangle(MARGIN_X + GRID_WIDTH * CELL_SIZE, MARGIN_Y + 1, 3, GRID_HEIGHT * CELL_SIZE, Raylib.WHITE);
        }

        public void DrawScore()
        {
            Raylib.DrawText($"Score: {score}", SCREEN_WIDTH - 150, 150, textSize, textColor);
        }

        public void DrawLevel()
        {
            Raylib.DrawText($"Level: {level}", SCREEN_WIDTH - 150, 200, textSize, textColor);
        }

        public void DrawNextBlock()
        {
            Raylib.DrawText("Next Block:", SCREEN_WIDTH - 150, 250, textSize, textColor);

            nextBlock.DrawNextBlock(12, 10);
        }


        public void BlinkLine(int lineIndex, int blinkCount, float blinkDuration)
        {
            for (int blink = 0; blink < blinkCount; blink++)
            {
                for (int j = 0; j < GRID_WIDTH; j++)
                {
                    colorGrid[lineIndex, j] = (blink % 2 == 0) ? blinkColor : gridColor;
                }

                Draw();
                Raylib.EndDrawing();
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Raylib.RAYWHITE);
                Raylib.WaitTime(blinkDuration);
            }
        }

        public void CheckLines()
        {
            List<int> linesToClear = new List<int>();

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
                    linesToClear.Add(i);
                }
            }

            if (linesToClear.Count > 0)
            {
                foreach (int lineIndex in linesToClear)
                {
                    BlinkLine(lineIndex, blinkCount, blinkDuration);
                }

                score += linesToClear.Count * 100;
                level = score / 1000 + 1;
                ShiftLinesDown(linesToClear);
            }
        }

        public void ShiftLinesDown(List<int> linesToClear)
        {
            foreach (int lineIndex in linesToClear)
            {
                for (int i = lineIndex; i > 0; i--)
                {
                    for (int j = 0; j < GRID_WIDTH; j++)
                    {
                        grid[i, j] = grid[i - 1, j];
                        colorGrid[i, j] = colorGrid[i - 1, j];
                    }
                }
            }
        }

        public bool IsOccupied(int x, int y)
        {
            if (x < 0 || x >= GRID_WIDTH || y < 0 || y >= GRID_HEIGHT)
            {
                return false;
            }

            return grid[y, x] != 0;
        }

        public void CheckGameOver()
        {
            for (int j = 0; j < GRID_WIDTH; j++)
            {
                if (grid[0, j] != 0)
                {
                    currentState = GameState.GameOver;
                }
            }
        }

        public void DrawGameOver()
        {
            Raylib.DrawText("GAME OVER", SCREEN_WIDTH / 2 + menuTitleOffsetX, SCREEN_HEIGHT / 2 + menuTitleOffsetY, titleSize, textColor);
            Raylib.DrawText("Press ENTER to restart", SCREEN_WIDTH / 2 + menuTextOffsetX, SCREEN_HEIGHT / 2 + menuTextOffsetY, textSize, textColor);
        }

        public void DrawMenu()
        {
            Raylib.DrawText("TETRIS", SCREEN_WIDTH / 2 + menuTitleOffsetX, SCREEN_HEIGHT / 2 + menuTitleOffsetY, titleSize, menuTitleColor);
            Raylib.DrawText("Press ENTER to start", SCREEN_WIDTH / 2 + menuTextOffsetX, SCREEN_HEIGHT / 2 + menuTextOffsetY, textSize, textColor);
        }

        public void HandleMenuInput()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
            {
                currentState = GameState.Playing;
            }
        }

        public static Block GenerateRandomBlock()
        {
            Random random = new Random();
            int index = random.Next(JsonLoader.LoadFromJson("json/blocks.json").Count);
            Block randomBlock = JsonLoader.LoadFromJson("json/blocks.json")[index];
            return new Block(GRID_WIDTH / 2 - randomBlock.shape.GetLength(1) / 2, 0, randomBlock.shape, randomBlock.color);
        }
    }
}
