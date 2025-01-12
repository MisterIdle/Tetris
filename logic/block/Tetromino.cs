using System;
using Raylib_CsLo;

namespace Tetris
{

    public class Tetromino
    {
        public int x;
        public int y;

        public int[,] shape;
        public Color color;

        public int rotation = 0;

        private float fallSpeed = 1.0f;
        private float fastFallSpeed = 20f;

        private float fallDelta = 0.0f;

        private float moveDelta = 0.0f;
        private float moveInterval = Raylib.GetRandomValue(1, 3);

        private GameScene gameScene;

        public Tetromino(GameScene gameScene, int x, int y, int[,] shape, Color color)
        {
            this.gameScene = gameScene;
            this.x = x;
            this.y = y;
            this.shape = shape;
            this.color = color;
        }

        public void DrawBlock(int x, int y, Color color)
        {
            int drawX = gameScene.MARGIN_X + x * GameScene.CELL_SIZE;
            int drawY = gameScene.MARGIN_Y + y * GameScene.CELL_SIZE;

            Raylib.DrawRectangle(drawX + 3, drawY + 3, GameScene.CELL_SIZE, GameScene.CELL_SIZE, Raylib.ColorAlpha(Raylib.BLACK, 0.5f));
            Raylib.DrawRectangle(drawX, drawY, GameScene.CELL_SIZE, GameScene.CELL_SIZE, color);

            DrawBlockOutline(drawX, drawY);
            DrawBlockLighting(drawX, drawY);
        }

        private void DrawBlockOutline(int drawX, int drawY)
        {
            Raylib.DrawRectangleLinesEx(new Rectangle(drawX, drawY, GameScene.CELL_SIZE, GameScene.CELL_SIZE), 3, Raylib.BLACK);
        }

        private void DrawBlockLighting(int drawX, int drawY)
        {
            Raylib.DrawRectangle(drawX + 3, drawY + 3, GameScene.CELL_SIZE - 6, 3, Raylib.ColorAlpha(Raylib.WHITE, 0.3f));
            Raylib.DrawRectangle(drawX + 3, drawY + 3, 3, GameScene.CELL_SIZE - 6, Raylib.ColorAlpha(Raylib.WHITE, 0.3f));
            Raylib.DrawRectangle(drawX + GameScene.CELL_SIZE - 6, drawY + 3, 3, GameScene.CELL_SIZE - 6, Raylib.ColorAlpha(Raylib.BLACK, 0.3f));
            Raylib.DrawRectangle(drawX + 3, drawY + GameScene.CELL_SIZE - 6, GameScene.CELL_SIZE - 6, 3, Raylib.ColorAlpha(Raylib.BLACK, 0.3f));
        }

        public void DrawTetromino()
        {
            for (int i = 0; i < shape.GetLength(0); i++)
            {
                for (int j = 0; j < shape.GetLength(1); j++)
                {
                    if (shape[i, j] == 1)
                    {
                        {
                            DrawBlock(x + j, y + i, color);
                        }
                    }
                }
            }
        }

        public void DrawNextTetromino(int x, int y)
        {
            for (int i = 0; i < shape.GetLength(0); i++)
            {
                for (int j = 0; j < shape.GetLength(1); j++)
                {
                    if (shape[i, j] == 1)
                    {
                        if (shape.GetLength(0) == 2 && shape.GetLength(1) == 2)
                        {
                            DrawBlock(x + j, y + i + 1, color);
                        }
                        else
                        {
                            DrawBlock(x + j, y + i, color);
                        }
                    }
                }
            }
        }

        public void DrawShadowTetromino()
        {
            int shadowY = y;

            while (!CheckCollision(shadowY + 1))
            {
                shadowY++;
            }

            for (int i = 0; i < shape.GetLength(0); i++)
            {
                for (int j = 0; j < shape.GetLength(1); j++)
                {
                    if (shape[i, j] == 1)
                    {
                        DrawBlock(x + j, shadowY + i, Raylib.ColorAlpha(Raylib.BLACK, 0.1f));
                    }
                }
            }
        }

        public void DrawIATetromino()
        {
            for (int i = 0; i < shape.GetLength(0); i++)
            {
                for (int j = 0; j < shape.GetLength(1); j++)
                {
                    if (shape[i, j] == 1)
                    {
                        DrawBlock(x + j, y + i, color);
                    }
                }
            }
        }

        public void DrawPlacedTetromino()
        {
            for (int i = 0; i < GameScene.GRID_HEIGHT; i++)
            {
                for (int j = 0; j < GameScene.GRID_WIDTH; j++)
                {
                    if (gameScene.grid[i, j] != 0)
                    {
                        DrawBlock(j, i, gameScene.colorGrid[i, j]);
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

        public void MoveTetrominoRandomly()
        {
            moveDelta += Raylib.GetFrameTime();

            if (moveDelta >= moveInterval)
            {

                int random = Raylib.GetRandomValue(0, 5);

                if (random == 0)
                    MoveTetromino(-1, 0);

                else if (random == 1)
                    MoveTetromino(1, 0);

                else if (random == 2)
                    RotateTetromino();

                else if (random == 3)
                    SetFallSpeed(fastFallSpeed);

                else if (random == 4)
                    PlaceTetrominoAsBottom();

                moveDelta = 0.0f;
                moveInterval = Raylib.GetRandomValue(1, 3);
            }
        }

        private void PlaceTetrominoAsBottom()
        {
            while (!CheckCollision(y + 1))
            {
                y++;
            }

            PlaceBlock();

            gameScene.currentTetromino = gameScene.nextTetromino;
            gameScene.nextTetromino = gameScene.GenerateRandomTetromino();
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
            
            else
            {
                rotation = (rotation + 1) % 4;
            }
        }

        public Tetromino Rotate(int rotation)
        {
            for (int i = 0; i < rotation; i++)
            {
                RotateTetromino();
            }
            return this;
        }

        public void MoveTo(int x, int y, int rotation, bool ultraSpeed)
        {
            int deltaX = x - this.x;
            int deltaY = y - this.y;

            MoveTetromino(deltaX, 0);

            for (int i = 0; i < rotation; i++)
            {
                RotateTetromino();
            }

            SetFallSpeed(fastFallSpeed);
            
            if (ultraSpeed)
                MoveTetromino(0, deltaY);
        }

        public Tetromino Clone()
        {
            return new Tetromino(gameScene, x, y, shape, color);
        }


        private void PlaceBlock()
        {
            for (int i = 0; i < shape.GetLength(0); i++)
            {
                for (int j = 0; j < shape.GetLength(1); j++)
                {
                    if (shape[i, j] == 1)
                    {
                        int newX = x + j;
                        int newY = y + i;

                        if (newX >= 0 && newX < GameScene.GRID_WIDTH && newY >= 0 && newY < GameScene.GRID_HEIGHT)
                        {
                            gameScene.grid[newY, newX] = 1;
                            gameScene.colorGrid[newY, newX] = color;
                        }
                    }
                }
            }

            Raylib.PlaySound(SoundManager.blockplace);
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

                        if (checkYLocal < 0 || checkYLocal >= GameScene.GRID_HEIGHT || checkX < 0 || checkX >= GameScene.GRID_WIDTH || gameScene.grid[checkYLocal, checkX] != 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void Update()
        {
            fallDelta += Raylib.GetFrameTime() * fallSpeed;

            if (fallDelta >= 1.0f)
            {
                if (!CheckCollision(y + 1))
                {
                    y++;
                }
                else
                {
                    PlaceBlock();
                    gameScene.currentTetromino = gameScene.nextTetromino;
                    gameScene.nextTetromino = gameScene.GenerateRandomTetromino();
                }

                fallDelta = 0.0f;
            }
        }

        private void SetFallSpeed(float fallSpeed)
        {
            this.fallSpeed = fallSpeed * gameScene.level;
        } 

        public void HandleInput()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_UP))
                RotateTetromino();

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_LEFT))
                MoveTetromino(-1, 0);

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_RIGHT))
                MoveTetromino(1, 0);

            if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN)) {
                SetFallSpeed(fastFallSpeed);
            }
            else {
                SetFallSpeed(1.0f);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
                PlaceTetrominoAsBottom();
        }
    }
}
