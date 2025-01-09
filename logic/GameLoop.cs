using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_CsLo;

namespace Tetris
{
    class GameLoop
    {
        public const int SCREEN_WIDTH = 750;
        public const int SCREEN_HEIGHT = 600;
        public const int GRID_WIDTH = 10;
        public const int GRID_HEIGHT = 20;
        public const int CELL_SIZE = 30;
        public const int MARGIN_X = SCREEN_WIDTH / 2 - GRID_WIDTH * CELL_SIZE / 2;
        public const int MARGIN_Y = 0;

        public static float deltaTime = 0.0f;

        public static int blinkCount = 6;
        public static float blinkDuration = 0.1f;
        public Color blinkColor = new Color(255, 0, 0, 100);

        public static float normalFallSpeed = 1.0f;
        public static float fastFallSpeed = 0.07f;

        public static Color gridColor = new Color(31, 31, 88, 255);
        public static Color borderColor = Raylib.GRAY;
        public static Color backgroundColor = Raylib.BLACK;
        
        public static Color textColor = Raylib.WHITE;
        public static Color scoreColor = Raylib.GOLD;
        public static Color levelColor = Raylib.RED;
        public static Color linesColor = Raylib.BLUE;

        public static Color menuTitleColor = Raylib.DARKGREEN;
        public static Color shadowColor = new Color(0, 0, 0, 50);

        public static Font font;

        public static int textSize = 40;
        public static int titleSize = 40;
        public static int menuTitleOffsetX = -60;
        public static int menuTitleOffsetY = -100;
        public static int menuTextOffsetX = -150;
        public static int menuTextOffsetY = 0;

        public static float gameOverAnimationTime = 0.0f;
        public static float gameOverAnimationDuration = 0.1f;

        public static bool isRobotPlaying = false;

        public static List<Block> blocks;
        public static int[,] grid = new int[GRID_HEIGHT, GRID_WIDTH];
        public static Color[,] colorGrid = new Color[GRID_HEIGHT, GRID_WIDTH];

        public static Block currentBlock;
        public static Block nextBlock;

        public static GameState currentState = GameState.Menu;
        public static int score = 0; 
        public static int level = 1;
        public static int lines = 0;

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
            Raylib.SetTargetFPS(60);
            
            font = Raylib.LoadFont("fonts/8-bitArcadeIn.ttf");

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
                        AnimateGameOver();
                        break;
                }

                Raylib.EndDrawing();
            }

            Raylib.UnloadFont(font);

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
            DrawLines();
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

            for (int i = 0; i < GRID_HEIGHT; i++)
            {
                Block rightBorder = new Block(GRID_WIDTH, 0, new int[1, 1], borderColor);
                rightBorder.DrawBlock(GRID_WIDTH, i, borderColor);

                Block leftBorder = new Block(-1, 0, new int[1, 1], borderColor);
                leftBorder.DrawBlock(-1, i, borderColor);
            }
        }

        public void AnimateGameOver()
        {
            Draw();

            Block fakeBlock = new Block(0, 0, new int[1, 1], Raylib.GRAY);

            if (gameOverAnimationTime < gameOverAnimationDuration)
            {
                int rowsToFill = (int)(gameOverAnimationTime * GRID_HEIGHT / 2);

                for (int i = GRID_HEIGHT - 1; i >= GRID_HEIGHT - rowsToFill; i--)
                {
                    for (int j = 0; j < GRID_WIDTH; j++)
                    {
                        fakeBlock.DrawBlock(j, i, Raylib.GRAY);
                    }
                }

                gameOverAnimationTime += deltaTime;
            }
            else
            {
                DrawGameOver();
                Input.HandleGameOverInput();
            }
        }

        public void DrawScore()
        {
            Raylib.DrawTextEx(font, "Score", new Vector2(SCREEN_WIDTH - 170, 10), textSize, 1, textColor);
            Raylib.DrawTextEx(font, score.ToString(), new Vector2(SCREEN_WIDTH - 170, 50), textSize, 1, scoreColor);
        }

        public void DrawLines() {
            Raylib.DrawTextEx(font, "Lines", new Vector2(SCREEN_WIDTH - 165, 100), textSize, 1, textColor);
            Raylib.DrawTextEx(font, lines.ToString(), new Vector2(SCREEN_WIDTH - 165, 140), textSize, 1, linesColor);
        }

        public void DrawLevel()
        {
            Raylib.DrawTextEx(font, "Level", new Vector2(SCREEN_WIDTH - 165, 190), textSize, 1, textColor);
            Raylib.DrawTextEx(font, level.ToString(), new Vector2(SCREEN_WIDTH - 165, 230), textSize, 1, levelColor);
        }

        public void DrawNextBlock()
        {
            Raylib.DrawTextEx(font, "Next", new Vector2(45, 10), textSize, 1, textColor);
            nextBlock.DrawNextBlock(-6, 2);

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
                Raylib.ClearBackground(backgroundColor);
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
                    score += 100;
                    lines++;

                    if (score % 1000 == 0)
                    {
                        level++;
                    }

                    BlinkLine(i, blinkCount, blinkDuration);
                    ShiftLinesDown(new List<int> { i });
                    i++;
                }
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
            if (currentBlock.CheckCollision())
            {
                currentState = GameState.GameOver;
            }
        }

        public void DrawGameOver()
        {
            Raylib.DrawRectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT, new Color(0, 0, 0, 200));
            
            Raylib.DrawRectangle(SCREEN_WIDTH / 2 - 300, SCREEN_HEIGHT / 2 - 200, 600, 400, Raylib.BLACK);
            Raylib.DrawRectangleLines(SCREEN_WIDTH / 2 - 300, SCREEN_HEIGHT / 2 - 200, 600, 400, Raylib.WHITE);

            // Place le text en haut au milieu de l'écran
            Raylib.DrawTextEx(font, "GAME OVER", new Vector2(SCREEN_WIDTH / 2 - 250, SCREEN_HEIGHT / 2 - 150), textSize + 40, 1, Raylib.RED);

            Raylib.DrawTextEx(font, "Press ENTER to restart", new Vector2(SCREEN_WIDTH / 2 - 230, SCREEN_HEIGHT / 2 - 50), textSize - 10, 1, textColor);

            Raylib.DrawTextEx(font, "Score", new Vector2(SCREEN_WIDTH / 2 - 260, SCREEN_HEIGHT / 2 + 50), textSize, 1, scoreColor);
            Raylib.DrawTextEx(font, score.ToString(), new Vector2(SCREEN_WIDTH / 2 - 260, SCREEN_HEIGHT / 2 + 90), textSize, 1, scoreColor);

            Raylib.DrawTextEx(font, "Lines", new Vector2(SCREEN_WIDTH / 2 - 60, SCREEN_HEIGHT / 2 + 50), textSize, 1, linesColor);
            Raylib.DrawTextEx(font, lines.ToString(), new Vector2(SCREEN_WIDTH / 2 - 60, SCREEN_HEIGHT / 2 + 90), textSize, 1, linesColor);

            Raylib.DrawTextEx(font, "Level", new Vector2(SCREEN_WIDTH / 2 + 120, SCREEN_HEIGHT / 2 + 50), textSize, 1, levelColor);
            Raylib.DrawTextEx(font, level.ToString(), new Vector2(SCREEN_WIDTH / 2 + 120, SCREEN_HEIGHT / 2 + 90), textSize, 1, levelColor);
        
            
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
