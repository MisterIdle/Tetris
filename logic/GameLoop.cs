using System;
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

        public static int score = 0;
        public static int level = 0;
        public static int linesCleared = 0;

        public static int blinkCount = 6;
        public static float blinkDuration = 0.1f;

        public static float normalFallSpeed = 1.0f;
        public static float fastFallSpeed = 0.07f;

        public static Color gridColor = Raylib.LIGHTGRAY;
        public static Color gridLineColor = Raylib.DARKGRAY;
        public static Color sidePanelColor = Raylib.DARKGRAY;
        public static Color textColor = Raylib.DARKBLUE;
        public static Color menuTitleColor = Raylib.DARKGREEN;

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
                Raylib.ClearBackground(Raylib.RAYWHITE);
                deltaTime = Raylib.GetFrameTime();
                switch (currentState)
                {
                    case GameState.Menu:
                        DrawMenu();
                        HandleMenuInput();
                        break;
                    case GameState.Playing:
                        Input();
                        Update();
                        Draw();
                        break;
                    case GameState.GameOver:
                        DrawGameOver();
                        HandleGameOverInput();
                        break;
                }
                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }

        public void Input()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_UP))
                currentBlock.RotateTetromino();

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_LEFT))
                currentBlock.MoveTetromino(-1, 0);

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_RIGHT))
                currentBlock.MoveTetromino(1, 0);

            if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN))
                currentBlock.SetFallSpeed(fastFallSpeed);
            else
                currentBlock.SetFallSpeed(normalFallSpeed);

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
                currentBlock.PlaceBlockAtBottom();

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
                Save.SaveGame(score, level, linesCleared, grid, colorGrid, currentBlock, nextBlock);

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_L))
            {
                SaveData saveData = Save.LoadGame();
                score = saveData.Score;
                level = saveData.Level;
                linesCleared = saveData.LinesCleared;
                grid = saveData.Grid;
                colorGrid = saveData.ColorGrid;
                currentBlock = saveData.CurrentBlock;
                nextBlock = saveData.NextBlock;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_R))
                isRobotPlaying = !isRobotPlaying;
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
            DrawPlacedBlocks();
            DrawShadow();


            currentBlock.DrawTetromino();
            nextBlock.DrawNextBlock(SCREEN_WIDTH - 200 + 50, 50);

            Raylib.DrawText($"Score: {score}", SCREEN_WIDTH - 180, 20, textSize, textColor);
            Raylib.DrawText($"Level: {level}", SCREEN_WIDTH - 180, 50, textSize, textColor);
        }


        public void DrawGrid()
        {
            for (int i = 0; i < GRID_HEIGHT; i++)
            {
                for (int j = 0; j < GRID_WIDTH; j++)
                {
                    Raylib.DrawRectangle(MARGIN_X + j * CELL_SIZE, MARGIN_Y + i * CELL_SIZE, CELL_SIZE, CELL_SIZE, gridColor);
                    Raylib.DrawRectangleLines(MARGIN_X + j * CELL_SIZE, MARGIN_Y + i * CELL_SIZE, CELL_SIZE, CELL_SIZE, gridLineColor);
                }
            }
            Raylib.DrawRectangle(SCREEN_WIDTH - 200, 0, 200, SCREEN_HEIGHT, sidePanelColor);
            Raylib.DrawRectangleLines(SCREEN_WIDTH - 200, 0, 200, SCREEN_HEIGHT, Raylib.BLACK);
        }

        public void DrawShadow()
        {
            Block shadowBlock = new Block(currentBlock.x, currentBlock.y, currentBlock.shape, new Color(0, 0, 0, 100));
            while (!shadowBlock.CheckCollision(shadowBlock.y + 1))
            {
                shadowBlock.y++;
            }
            shadowBlock.DrawTetromino();
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
                        Raylib.DrawRectangleLines(MARGIN_X + j * CELL_SIZE, MARGIN_Y + i * CELL_SIZE, CELL_SIZE, CELL_SIZE, gridLineColor);
                    }
                }
            }
        }

        public void BlinkLine(int lineIndex, int blinkCount, float blinkDuration)
        {
            for (int blink = 0; blink < blinkCount; blink++)
            {
                for (int j = 0; j < GRID_WIDTH; j++)
                {
                    colorGrid[lineIndex, j] = (blink % 2 == 0) ? Raylib.RED : gridColor;
                }
                Draw();
                Raylib.EndDrawing();
                Raylib.BeginDrawing();
                Raylib.WaitTime(blinkDuration);
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
                    BlinkLine(i, blinkCount, blinkDuration);
                    RemoveLine(i);
                    ShiftLinesDown(i);
                    IncreaseScore(1);
                }
            }
        }

        public void RemoveLine(int lineIndex)
        {
            for (int j = 0; j < GRID_WIDTH; j++)
            {
                grid[lineIndex, j] = 0;
                colorGrid[lineIndex, j] = gridColor;
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
        public bool IsOccupied(int x, int y)
        {
            if (x < 0 || x >= GRID_WIDTH || y < 0 || y >= GRID_HEIGHT)
            {
                return false;
            }

            return grid[y, x] != 0;
        }


        public bool IsLineCleared(int y)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                if (grid[y, x] == 0)
                {
                    return false;
                }
            }
            return true;
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

        public void HandleGameOverInput()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
            {
                score = 0;
                level = 0;
                linesCleared = 0;
                InitializeGrid();
                currentState = GameState.Playing;
            }
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

        public static void IncreaseScore(int lineCount)
        {
            switch (lineCount)
            {
                case 1: score += 40 * (level + 1); break;
                case 2: score += 100 * (level + 1); break;
                case 3: score += 300 * (level + 1); break;
                case 4: score += 1200 * (level + 1); break;
            }
            linesCleared += lineCount;
            level = linesCleared / 10;
        }
    }
}
