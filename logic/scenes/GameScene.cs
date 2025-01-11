using System;
using System.Numerics;
using Raylib_CsLo;
using Tetris;

public class GameScene : Scene
{
    public const int GRID_WIDTH = 10;
    public const int GRID_HEIGHT = 20;
    public const int CELL_SIZE = 30;
    
    public int MARGIN_X;
    public int MARGIN_Y;

    public int[,] grid = new int[GRID_HEIGHT, GRID_WIDTH];
    public Color[,] colorGrid = new Color[GRID_HEIGHT, GRID_WIDTH];

    public Tetromino currentTetromino;
    public Tetromino nextTetromino;

    private Color gridColor = new Color(35, 53, 89, 255);
    private Color borderColor = new Color(39, 61, 89, 255);
    private Color blinkColor = Raylib.RED;

    private Color textColor = new Color(255, 255, 255, 255);

    private int score = 0;
    public int level = 1;
    private int lines = 0;

    private GameLoop gameLoop;

    public GameScene(GameLoop gameLoop)
    {
        this.gameLoop = gameLoop;
        MARGIN_X = GameLoop.SCREEN_WIDTH / 3 + 20 - GRID_WIDTH * CELL_SIZE / 2;
        MARGIN_Y = 0;
    }

    public override void LoadScene()
    {
        InitializeGrid();

        currentTetromino = GenerateRandomTetromino();
        nextTetromino = GenerateRandomTetromino();
    }

    public override void UpdateScene()
    {
        if (gameLoop.currentState == GameState.Playing)
        {
            currentTetromino.Update();
            currentTetromino.HandleInput();
        }

        CheckLines();
        CheckGameOver();
    }

    public override void DrawScene()
    {
        DrawGrid();
        DrawHUD();

        if (gameLoop.currentState == GameState.GameOver)
        {
            DrawGameOver();
            HandleGameOverInput();
        } 
        else
        {
            currentTetromino.DrawShadowTetromino();
            currentTetromino.DrawPlacedTetromino();
            currentTetromino.DrawTetromino();
        }
    }


    private void InitializeGrid()
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

    private void DrawGrid()
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
            Tetromino rightBorder = new Tetromino(this, GRID_WIDTH, 0, new int[1, 1], borderColor);
            rightBorder.DrawBlock(GRID_WIDTH, i, borderColor);

            Tetromino leftBorder = new Tetromino(this, -1, 0, new int[1, 1], borderColor);
            leftBorder.DrawBlock(-1, i, borderColor);
        }
    }

    public void BlinkLine(int lineIndex, int blinkCount, float blinkDuration)
    {
        for (int blink = 0; blink < blinkCount; blink++)
        {
            for (int j = 0; j < GRID_WIDTH; j++)
            {
                colorGrid[lineIndex, j] = (blink % 2 == 0) ? blinkColor : gridColor;
            }

            DrawScene();

            Raylib.EndDrawing();
            Raylib.BeginDrawing();
            Raylib.ClearBackground(gameLoop.backgroundColor);
            Raylib.WaitTime(blinkDuration);
        }
    }

    private float gameOverAnimationTime = 0;
    private float gameOverAnimationDuration = 1.5f;

    public void CheckLines()
    {
        int linesCleared = 0;

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
                linesCleared++;

                BlinkLine(i, 3, 0.1f);
                RemoveLine(i);
                MoveLinesDown(i);

                Console.WriteLine("Line " + i + " cleared");

                i++;
            }
        }

        if (linesCleared > 0)
        {
            ScorePoints(linesCleared);
            lines += linesCleared;

            if (lines >= level * 10)
            {
                level++;
            }
        }
    }

    private void RemoveLine(int lineIndex)
    {
        for (int j = 0; j < GRID_WIDTH; j++)
        {
            grid[lineIndex, j] = 0;
            colorGrid[lineIndex, j] = gridColor;
        }
    }

    private void MoveLinesDown(int lineIndex)
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

    private void CheckGameOver()
    {
        // Si le tetromino actuel est en collision dès le spawn alors c'est game over
        if (currentTetromino.CheckCollision())
        {
            gameLoop.ChangeState(GameState.GameOver);
        }
    }

    public Tetromino GenerateRandomTetromino()
    {
        Random random = new Random();
        int index = random.Next(JsonLoader.LoadFromJson("json/blocks.json").Count);
        Tetromino randomBlock = JsonLoader.LoadFromJson("json/blocks.json")[index];
        return new Tetromino(this, GRID_WIDTH / 2 - randomBlock.shape.GetLength(1) / 2, 0, randomBlock.shape, randomBlock.color);
    }

    private void ScorePoints(int lines)
    {
        switch (lines)
        {
            case 1:
                score += 40 * level;
                break;
            case 2:
                score += 100 * level;
                break;
            case 3:
                score += 300 * level;
                break;
            case 4:
                score += 1200 * level;
                break;
        }
    }

    // HUD
    private void DrawHUD()
    {
        DrawDegradedBackground(GameLoop.SCREEN_WIDTH / 2 + 100, 0, GameLoop.SCREEN_WIDTH / 2 - 100, GameLoop.SCREEN_HEIGHT, gridColor);
        DrawNextTetrominoHUD();
        DrawScoreHUD();
        DrawLevelHUD();
        DrawLinesHUD();
    }

    private void DrawDegradedBackground(int x, int y, int width, int height, Color color)
    {
        Raylib.DrawRectangleGradientV(x, y, width, height, Raylib.ColorAlpha(color, 0.5f), Raylib.ColorAlpha(color, 0.2f));
    }

    private void DrawNextTetrominoHUD()
    {
        Raylib.DrawTextEx(gameLoop.font, "NEXT", new Vector2(GameLoop.SCREEN_WIDTH / 2 + 130, 440), 40, 0, textColor);

        nextTetromino.DrawNextTetromino(13, 16);

    }

    private void DrawScoreHUD()
    {
        Raylib.DrawTextEx(gameLoop.font, "SCORE", new Vector2(GameLoop.SCREEN_WIDTH / 2 + 130, 30), 30, 0, textColor);
        Raylib.DrawTextEx(gameLoop.font, score.ToString(), new Vector2(GameLoop.SCREEN_WIDTH / 2 + 130, 80), 30, 0, textColor);
    }

    private void DrawLevelHUD()
    {
        Raylib.DrawTextEx(gameLoop.font, "LEVEL", new Vector2(GameLoop.SCREEN_WIDTH / 2 + 130, 150), 30, 0, textColor);
        Raylib.DrawTextEx(gameLoop.font, level.ToString(), new Vector2(GameLoop.SCREEN_WIDTH / 2 + 130, 200), 30, 0, textColor);
    }

    private void DrawLinesHUD()
    {
        Raylib.DrawTextEx(gameLoop.font, "LINES", new Vector2(GameLoop.SCREEN_WIDTH / 2 + 130, 270), 30, 0, textColor);
        Raylib.DrawTextEx(gameLoop.font, lines.ToString(), new Vector2(GameLoop.SCREEN_WIDTH / 2 + 130, 320), 30, 0, textColor);
    }

    private void DrawGameOver()
    {
        Raylib.DrawTextEx(gameLoop.font, "GAME OVER", new Vector2(GameLoop.SCREEN_WIDTH / 2 - 215, GameLoop.SCREEN_HEIGHT / 2 - 70), 30, 0, textColor);
    }

    private void HandleGameOverInput()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
        {
            gameLoop.ChangeState(GameState.Playing);
            InitializeGrid();
            currentTetromino = GenerateRandomTetromino();
            nextTetromino = GenerateRandomTetromino();
            score = 0;
            level = 1;
            lines = 0;

            Console.WriteLine("Game restarted");
        }
    }
}
