using Raylib_CsLo;

namespace Tetris {
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

        public void DrawNextBlock(int x, int y)
        {
            for (int i = 0; i < shape.GetLength(0); i++)
            {
                for (int j = 0; j < shape.GetLength(1); j++)
                {
                    if (shape[i, j] == 1)
                    {
                        Raylib.DrawRectangle(x + j * GameLoop.CELL_SIZE, y + i * GameLoop.CELL_SIZE, GameLoop.CELL_SIZE, GameLoop.CELL_SIZE, color);
                        Raylib.DrawRectangleLines(x + j * GameLoop.CELL_SIZE, y + i * GameLoop.CELL_SIZE, GameLoop.CELL_SIZE, GameLoop.CELL_SIZE, Raylib.DARKGRAY);
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

        public void PlaceBlockAtBottom()
        {
            while (!CheckCollision(y + 1))
            {
                y++;
            }

            PlaceBlock();
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
}