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

        private Color gridColor = new Color(35, 53, 89, 255);
        private Color gridIAColor = new Color(89, 35, 35, 255);
        private Color gridIAUltraSpeedColor = new Color(139, 0, 0, 255);
        private Color borderColor = new Color(39, 61, 89, 255);
        private Color blinkColor = new Color(255, 0, 0, 255);

        private Color textColor = new Color(255, 255, 255, 255);
        private Color textIAColor = new Color(255, 0, 0, 255);

        private Color menuButtonColor = new Color(75, 100, 125, 255);
        private Color menuButtonOverColor = new Color(65, 85, 110, 255);

        private Color quitButtonColor = new Color(125, 75, 75, 255);
        private Color quitButtonOverColor = new Color(110, 65, 65, 255);

        public int score = 0;
        private int scoreMultiplier = 1;

        public int level = 1;
        public int lines = 0;

        public bool aiPlaying = false;
        public bool iaPlayingUltraSpeed = false;

        private GameLoop gameLoop;
        private Bot bot;

        public GameScene(GameLoop gameLoop)
        {
            this.gameLoop = gameLoop;
            bot = new Bot(this);
            MARGIN_X = GameLoop.SCREEN_WIDTH / 3 + 20 - GRID_WIDTH * CELL_SIZE / 2;
            MARGIN_Y = 0;
        }

        public override void LoadScene()
        {
            InitializeGrid();
            ScoreMultiplier();
            SoundManager.StopAllAudio();

            if (gameLoop.currentState == GameState.Continue)
            {
                SaveData saveData = Save.LoadGame();

                score = saveData.score;
                level = saveData.level;
                lines = saveData.linesCleared;

                grid = saveData.grid;
                colorGrid = saveData.colorGrid;

                currentTetromino = GenerateSavedTetromino(new TetrominoData
                {
                    Shape = saveData.currentBlock.shape,
                    Color = new int[] { saveData.currentBlock.color.r, saveData.currentBlock.color.g, saveData.currentBlock.color.b, saveData.currentBlock.color.a }
                });
                
                nextTetromino = GenerateSavedTetromino(new TetrominoData
                {
                    Shape = saveData.nextBlock.shape,
                    Color = new int[] { saveData.nextBlock.color.r, saveData.nextBlock.color.g, saveData.nextBlock.color.b, saveData.nextBlock.color.a }
                });
            }
            
            else if (gameLoop.currentState == GameState.Loading)
            {
                currentTetromino = GenerateRandomTetromino();
                nextTetromino = GenerateRandomTetromino();
    
                level = Settings.level;
            }

            gameLoop.ChangeState(GameState.Playing);
        }

        public override void UpdateScene()
        {    
            if (gameLoop.currentState == GameState.Playing)
            {
            currentTetromino.Update();
            currentTetromino.HandleInput();

            if (aiPlaying)
            {
                var (bestX, bestY, bestRotation) = bot.GetBestMove(grid, currentTetromino);
                currentTetromino.MoveTo(bestX, bestY, bestRotation, iaPlayingUltraSpeed);
            }
            }

            SoundManager.LoopAudio(SoundManager.backgroundMusicGame);

            CheckLines();
            CheckGameOver();

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_P) || Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE))
            {
                gameLoop.ChangeState(gameLoop.currentState == GameState.Playing ? GameState.Paused : GameState.Playing);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_F9))
            {
                aiPlaying = !aiPlaying;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_F10))
            {
                iaPlayingUltraSpeed = !iaPlayingUltraSpeed;
            }
        }

        public override void DrawScene()
        {
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
                    Raylib.DrawRectangle(MARGIN_X + j * CELL_SIZE, MARGIN_Y + i * CELL_SIZE, CELL_SIZE, CELL_SIZE, gridColor);

                    if (aiPlaying)
                        Raylib.DrawRectangle(MARGIN_X + j * CELL_SIZE, MARGIN_Y + i * CELL_SIZE, CELL_SIZE, CELL_SIZE, gridIAColor);
                    
                    if (iaPlayingUltraSpeed)
                        Raylib.DrawRectangle(MARGIN_X + j * CELL_SIZE, MARGIN_Y + i * CELL_SIZE, CELL_SIZE, CELL_SIZE, gridIAUltraSpeedColor);
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

            string pathNormal = "json/block/blocks.json";
            string pathGnowius = "json/block/gnowius.json";

            if (Settings.normal)
            {
                combinedBlocks.AddRange(JsonLoader.LoadFromJson(pathNormal));
            }

            if (Settings.gnowius)
            {
                combinedBlocks.AddRange(JsonLoader.LoadFromJson(pathGnowius));
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

        public Tetromino GenerateSavedTetromino(TetrominoData tetrominoData)
        {
            return new Tetromino(
                this,
                GRID_WIDTH / 2 - tetrominoData.Shape.GetLength(1) / 2,
                0,
                tetrominoData.Shape,
                new Color(tetrominoData.Color[0], tetrominoData.Color[1], tetrominoData.Color[2], tetrominoData.Color[3])
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
            DrawIAMessageHUD();
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

        private void DrawIAMessageHUD()
        {
            if (aiPlaying) {
                Raylib.DrawTextEx(gameLoop.font, "AI ON", new Vector2(GameLoop.SCREEN_WIDTH / 2 + 130, 370), 20, 0, textIAColor);
                if (iaPlayingUltraSpeed) {
                    Raylib.DrawTextEx(gameLoop.font, "ULTRA SPEED (MAY BUG)", new Vector2(GameLoop.SCREEN_WIDTH / 2 + 110, 400), 10, 0, textIAColor);
                }
            }

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
            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 180, GameLoop.SCREEN_HEIGHT / 2 + 20, 200, 50, "SAVE", 20, menuButtonColor, menuButtonOverColor, textColor, gameLoop.font, () =>
            {
                Save.SaveGame(this);
                
                Raylib.DrawTextEx(gameLoop.font, "Game saved!", new Vector2(GameLoop.SCREEN_WIDTH / 2 - 180, GameLoop.SCREEN_HEIGHT / 2 - 90), 20, 0, textColor);
                Raylib.BeginDrawing();
                Raylib.EndDrawing();
                Raylib.WaitTime(0.5f);
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 180, GameLoop.SCREEN_HEIGHT / 2 + 90, 200, 50, "MAIN MENU", 20, menuButtonColor, menuButtonOverColor, textColor, gameLoop.font, () =>
            {
                gameLoop.ChangeState(GameState.Menu);
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 180, GameLoop.SCREEN_HEIGHT / 2 + 200, 200, 50, "QUIT GAME", 20, quitButtonColor, quitButtonOverColor, textColor, gameLoop.font, () =>
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
