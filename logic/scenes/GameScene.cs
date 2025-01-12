using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_CsLo;

namespace Tetris
{
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

        public Tetromino IATetromino;

        private Color gridColor = new Color(35, 53, 89, 255);
        private Color borderColor = new Color(39, 61, 89, 255);
        private Color blinkColor = Raylib.RED;

        private Color textColor = new Color(255, 255, 255, 255);

        private Color menuButtonColor = new Color(75, 100, 125, 255);
        private Color menuButtonOverColor = new Color(65, 85, 110, 255);

        private Color quitButtonColor = new Color(125, 75, 75, 255);
        private Color quitButtonOverColor = new Color(110, 65, 65, 255);

        private int score = 0;
        private int scoreMultiplier = 1;

        public int level = 1;
        private int lines = 0;

        private bool aiPlaying = false;

        private GameLoop gameLoop;

        public GameScene(GameLoop gameLoop)
        {
            this.gameLoop = gameLoop;
            MARGIN_X = GameLoop.SCREEN_WIDTH / 3 + 20 - GRID_WIDTH * CELL_SIZE / 2;
            MARGIN_Y = 0;
        }

        public override void LoadScene()
        {
            if (gameLoop.currentState == GameState.Loading)
            {
                InitializeGrid();
                ScoreMultiplier();

                SoundManager.StopAllAudio();

                currentTetromino = GenerateRandomTetromino();
                nextTetromino = GenerateRandomTetromino();

                level = Settings.level;

                gameLoop.ChangeState(GameState.Playing);
            }
        }

        public override void UpdateScene()
        {    
            if (gameLoop.currentState == GameState.Playing)
            {
                currentTetromino.Update();
                currentTetromino.HandleInput();

                if (aiPlaying)
                {
                    (int bestX, int bestY, int bestRotation) = GetBestMove(grid, currentTetromino);
                    currentTetromino.MoveTo(bestX, bestY, bestRotation);
                }
            }

            SoundManager.LoopAudio(SoundManager.backgroundMusicGame);

            CheckLines();
            CheckGameOver();

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_P) && gameLoop.currentState == GameState.Playing)
            {
                gameLoop.ChangeState(GameState.Paused);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_F9))
            {
                aiPlaying = !aiPlaying;
            }
        }

        public override void DrawScene()
        {
            if (gameLoop.currentState == GameState.Loading)
            {
                Raylib.ClearBackground(gameLoop.backgroundColor);
                Raylib.DrawText("Loading...", GameLoop.SCREEN_WIDTH / 2 - 50, GameLoop.SCREEN_HEIGHT / 2 - 10, 20, textColor);
                return;
            }

            DrawGrid();
            DrawHUD();

            if (gameLoop.currentState == GameState.GameOver)
            {
                DrawGameOver();
                return;
            }

            if (gameLoop.currentState == GameState.Paused)
            {
                DrawPause();
                return;
            }

            if (Settings.shadow)
                currentTetromino.DrawShadowTetromino();

            if (Settings.placedBlocks)
                currentTetromino.DrawPlacedTetromino();

            if (Settings.randomMovement)
                currentTetromino.MoveTetrominoRandomly();

            currentTetromino.DrawTetromino();
        }

        private (int, int, int) GetBestMove(int[,] board, Tetromino piece)
        {
            int bestX = 0;
            int bestY = 0;
            int bestRotation = 0;
            float bestScore = float.MinValue;

            for (int rotation = 0; rotation < 4; rotation++)
            {
                Tetromino rotatedPiece = piece.Clone();
                rotatedPiece.Rotate(rotation);

                for (int x = -rotatedPiece.shape.GetLength(1) + 1; x < GRID_WIDTH; x++)
                {
                    int dropY = GetDropY(board, rotatedPiece, x);
                    if (dropY != -1)
                    {
                        int[,] simulatedBoard = SimulateMove(board, rotatedPiece, x, dropY);
                        float score = EvaluateBoard(simulatedBoard);

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestX = x;
                            bestY = dropY;
                            bestRotation = rotation;
                        }
                    }
                }
            }

            return (bestX, bestY, bestRotation);
        }

        private int GetDropY(int[,] board, Tetromino piece, int x)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                if (IsCollision(board, piece, x, y))
                {
                    return y - 1;
                }
            }
            return -1;
        }

        private bool IsCollision(int[,] board, Tetromino piece, int x, int y)
        {
            int pieceHeight = piece.shape.GetLength(0);
            int pieceWidth = piece.shape.GetLength(1);

            for (int py = 0; py < pieceHeight; py++)
            {
                for (int px = 0; px < pieceWidth; px++)
                {
                    if (piece.shape[py, px] != 0)
                    {
                        int boardX = x + px;
                        int boardY = y + py;

                        if (boardX < 0 || boardX >= GRID_WIDTH || boardY >= GRID_HEIGHT || 
                            (boardY >= 0 && board[boardY, boardX] != 0))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private float EvaluateBoard(int[,] board)
        {
            float score = 0.0f;

            int height = CalculateHeight(board);
            int holes = CalculateHoles(board);
            int completedLines = CalculateCompletedLines(board);
            int bumpiness = CalculateBumpiness(board);

            score -= 0.510066f * height;
            score -= 0.35663f * holes;
            score += 0.760666f * completedLines;
            score -= 0.184483f * bumpiness;

            return score;
        }

        private int[] PrecalculateHeights(int[,] board)
        {
            int[] columnHeights = new int[GRID_WIDTH];

            for (int x = 0; x < GRID_WIDTH; x++)
            {
                columnHeights[x] = GetColumnHeight(board, x);
            }

            return columnHeights;
        }

        private int GetColumnHeight(int[,] board, int column)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                if (board[y, column] != 0)
                {
                    return GRID_HEIGHT - y;
                }
            }
            return 0;
        }

        private int CalculateHeight(int[,] board)
        {
            int height = 0;
            int[] columnHeights = PrecalculateHeights(board);

            for (int x = 0; x < GRID_WIDTH; x++)
            {
                height += columnHeights[x];
            }

            return height;
        }

        private int CalculateHoles(int[,] board)
        {
            int holes = 0;

            for (int x = 0; x < GRID_WIDTH; x++)
            {
                bool blockFound = false;

                for (int y = 0; y < GRID_HEIGHT; y++)
                {
                    if (board[y, x] != 0)
                    {
                        blockFound = true;
                    }
                    else if (blockFound && board[y, x] == 0)
                    {
                        holes++;
                    }
                }
            }

            return holes;
        }

        private int CalculateCompletedLines(int[,] board)
        {
            int completedLines = 0;

            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                bool isLineComplete = true;

                for (int x = 0; x < GRID_WIDTH; x++)
                {
                    if (board[y, x] == 0)
                    {
                        isLineComplete = false;
                        break;
                    }
                }

                if (isLineComplete)
                {
                    completedLines++;
                }
            }

            return completedLines;
        }

        private int CalculateBumpiness(int[,] board)
        {
            int bumpiness = 0;
            int[] columnHeights = PrecalculateHeights(board);

            for (int x = 0; x < GRID_WIDTH - 1; x++)
            {
                bumpiness += Math.Abs(columnHeights[x] - columnHeights[x + 1]);
            }

            return bumpiness;
        }

        private int[,] SimulateMove(int[,] board, Tetromino piece, int x, int y)
        {
            int[,] simulatedBoard = (int[,])board.Clone();
            int pieceHeight = piece.shape.GetLength(0);
            int pieceWidth = piece.shape.GetLength(1);

            for (int py = 0; py < pieceHeight; py++)
            {
                for (int px = 0; px < pieceWidth; px++)
                {
                    if (piece.shape[py, px] != 0)
                    {
                        int boardX = x + px;
                        int boardY = y + py;

                        if (boardX >= 0 && boardX < GRID_WIDTH && boardY >= 0 && boardY < GRID_HEIGHT)
                        {
                            simulatedBoard[boardY, boardX] = piece.shape[py, px];
                        }
                    }
                }
            }

            return simulatedBoard;
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

               Raylib.PlaySound(SoundManager.lineclear);

               Raylib.EndDrawing();
               Raylib.BeginDrawing();
               Raylib.ClearBackground(gameLoop.backgroundColor);
               Raylib.WaitTime(blinkDuration);
           }
       }

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
                   Raylib.PlaySound(SoundManager.levelup);
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
           if (currentTetromino.CheckCollision())
           {
               if (gameLoop.currentState != GameState.GameOver)
               {
                   Raylib.PlaySound(SoundManager.lose);
               }

               gameLoop.ChangeState(GameState.GameOver);
           }
       }

       public Tetromino GenerateRandomTetromino()
       {
           var random = new Random();
           List<Tetromino> combinedBlocks = new List<Tetromino>();

           if (Settings.normal)
           {
               combinedBlocks.AddRange(JsonLoader.LoadFromJson("json/blocks.json"));
           }

           if (Settings.gnowius)
           {
               combinedBlocks.AddRange(JsonLoader.LoadFromJson("json/gnowius.json"));
           }

           if (combinedBlocks.Count == 0)
           {
               throw new InvalidOperationException("No Tetromino configuration is active.");
           }

           int index = random.Next(combinedBlocks.Count);
           var randomBlock = combinedBlocks[index];

           return new Tetromino(
               this,
               GRID_WIDTH / 2 - randomBlock.shape.GetLength(1) / 2,
               0,
               randomBlock.shape,
               randomBlock.color
           );
       }


       private void ScorePoints(int lines)
       {
           switch (lines)
           {
               case 1:
                   score += 40 * level * scoreMultiplier;
                   break;
               case 2:
                   score += 100 * level * scoreMultiplier;
                   break;
               case 3:
                   score += 300 * level * scoreMultiplier;
                   break;
               case 4:
                   score += 1200 * level * scoreMultiplier;
                   break;
               case 5:
                   score += 5000 * level * scoreMultiplier;
                   break;
           }
       }

       private void ScoreMultiplier()
       {
           if (!Settings.nextBlock)
           {
               scoreMultiplier *= 2;
           }

           if (!Settings.shadow)
           {
               scoreMultiplier *= 2;
           }

           if (!Settings.placedBlocks)
           {
               scoreMultiplier *= 5;
           }

           if (Settings.randomMovement)
           {
               scoreMultiplier *= 3;
           }

           Console.WriteLine("Score Multiplier: " + scoreMultiplier);
       }

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
           if (Settings.nextBlock)
           {
               Raylib.DrawTextEx(gameLoop.font, "NEXT", new Vector2(GameLoop.SCREEN_WIDTH / 2 + 130, 440), 40, 0, textColor);

               nextTetromino.DrawNextTetromino(13, 16);
           }
       }

       private void DrawScoreHUD()
       {
           Raylib.DrawTextEx(gameLoop.font, "SCORE", new Vector2(GameLoop.SCREEN_WIDTH / 2 + 130, 30), 30, 0, textColor);
           string scoreText = score.ToString().Length > 6 ? "GLITCH" : score.ToString();
           Raylib.DrawTextEx(gameLoop.font, scoreText, new Vector2(GameLoop.SCREEN_WIDTH / 2 + 130, 80), 30, 0, textColor);
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

       private void DrawPause()
       {
           Raylib.DrawRectangle(0, 0, GameLoop.SCREEN_WIDTH, GameLoop.SCREEN_HEIGHT, Raylib.ColorAlpha(Raylib.BLACK, 0.5f));

           string pauseText = "PAUSED";
           int textSize = 40;

           Vector2 textSizeVector = Raylib.MeasureTextEx(gameLoop.font, pauseText, textSize, 0);
           Color animatedColor = Raylib.GetFrameTime() % 1.0f > 0.5f ? Raylib.GRAY : textColor;

           Raylib.DrawTextEx(gameLoop.font, pauseText, new Vector2(GameLoop.SCREEN_WIDTH / 2 - textSizeVector.X / 2 - 80, GameLoop.SCREEN_HEIGHT / 2 - 150), textSize, 0, animatedColor);

           CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 180, GameLoop.SCREEN_HEIGHT / 2 - 50, 200, 50, "RESUME", 20, menuButtonColor, menuButtonOverColor, textColor, gameLoop.font, () =>
           {
               gameLoop.ChangeState(GameState.Playing);
           });

           CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 180, GameLoop.SCREEN_HEIGHT / 2 + 20, 200, 50, "MAIN MENU", 20, menuButtonColor, menuButtonOverColor, textColor, gameLoop.font, () =>
           {
               gameLoop.ChangeState(GameState.Menu);
           });

           CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 180, GameLoop.SCREEN_HEIGHT / 2 + 150, 200, 50, "QUIT GAME", 20, quitButtonColor, quitButtonOverColor, textColor, gameLoop.font, () =>
           {
               Raylib.CloseWindow();
           });
       }

       private void DrawGameOver()
       {
           Raylib.DrawRectangle(0, 0, GameLoop.SCREEN_WIDTH, GameLoop.SCREEN_HEIGHT, Raylib.ColorAlpha(Raylib.BLACK, 0.5f));

           string pauseText = "GAME OVER";
           int textSize = 40;

           Vector2 textSizeVector = Raylib.MeasureTextEx(gameLoop.font, pauseText, textSize, 0);
           Color animatedColor = Raylib.GetFrameTime() % 1.0f > 0.5f ? Raylib.GRAY : textColor;

           Raylib.DrawTextEx(gameLoop.font, pauseText, new Vector2(GameLoop.SCREEN_WIDTH / 2 - textSizeVector.X / 2 - 85, GameLoop.SCREEN_HEIGHT / 2 - 150), textSize, 0, animatedColor);

           CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 180, GameLoop.SCREEN_HEIGHT / 2 - 50, 200, 50, "RESTART", 20, menuButtonColor, menuButtonOverColor, textColor, gameLoop.font, () =>
           {
               gameLoop.ChangeState(GameState.Loading);
           });

           CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 180, GameLoop.SCREEN_HEIGHT / 2 + 20, 200, 50, "MAIN MENU", 20, menuButtonColor, menuButtonOverColor, textColor, gameLoop.font, () =>
           {
               gameLoop.ChangeState(GameState.Menu);
           });

           CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 180, GameLoop.SCREEN_HEIGHT / 2 + 150, 200, 50, "QUIT GAME", 20, quitButtonColor, quitButtonOverColor, textColor, gameLoop.font, () =>
           {
               Raylib.CloseWindow();
           });
       }
    }
}
