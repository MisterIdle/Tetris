namespace Tetris {
    public class Bot {

        GameScene gameScene;

        public Bot(GameScene gameScene) {
            this.gameScene = gameScene;
        }

        public (int, int, int) GetBestMove(int[,] board, Tetromino piece)
        {
            int bestX = 0;
            int bestY = 0;
            int bestRotation = 0;
            float bestScore = float.MinValue;
            for (int rotation = 0; rotation < 4; rotation++) {

                Tetromino rotatedPiece = piece.Clone();
                rotatedPiece.Rotate(rotation);

                for (int x = -rotatedPiece.shape.GetLength(1) + 1; x < GameScene.GRID_WIDTH; x++)
                {
                    int dropY = GetDropY(board, rotatedPiece, x);
                    if (dropY != -1)
                    {
                    int[,] simulatedBoard = SimulateMove(board, rotatedPiece, x, dropY);
                    float score = EvaluateBoard(simulatedBoard);

                        for (int nextRotation = 0; nextRotation < 4; nextRotation++)
                        {
                            Tetromino nextRotatedPiece = gameScene.nextTetromino.Clone();
                            nextRotatedPiece.Rotate(nextRotation);
                            for (int nextX = -nextRotatedPiece.shape.GetLength(1) + 1; nextX < GameScene.GRID_WIDTH; nextX++)
                            {
                            int nextDropY = GetDropY(simulatedBoard, nextRotatedPiece, nextX);
                                if (nextDropY != -1)
                                {
                                    int[,] nextSimulatedBoard = SimulateMove(simulatedBoard, nextRotatedPiece, nextX, nextDropY);
                                    float nextScore = EvaluateBoard(nextSimulatedBoard);
                                        for (int futureRotation = 0; futureRotation < 4; futureRotation++)
                                        {
                                            Tetromino futureRotatedPiece = gameScene.nextTetromino.Clone();
                                            futureRotatedPiece.Rotate(futureRotation);
                                            
                                            for (int futureX = -futureRotatedPiece.shape.GetLength(1) + 1; futureX < GameScene.GRID_WIDTH; futureX++)
                                            {
                                                int futureDropY = GetDropY(nextSimulatedBoard, futureRotatedPiece, futureX);
                                                if (futureDropY != -1)
                                                {
                                                int[,] futureSimulatedBoard = SimulateMove(nextSimulatedBoard, futureRotatedPiece, futureX, futureDropY);
                                                float futureScore = EvaluateBoard(futureSimulatedBoard);
                                                if (futureScore > bestScore)
                                                {
                                                    bestScore = futureScore;
                                                    bestX = x;
                                                    bestY = dropY;
                                                    bestRotation = rotation;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return (bestX, bestY, bestRotation);
        }

        private int GetDropY(int[,] board, Tetromino piece, int x)
        {
            for (int y = 0; y < GameScene.GRID_HEIGHT; y++)
            {
                if (IsCollision(board, piece, x, y))
                {
                    return y - 1;
                }
            }
            return -1;
        }
        private static bool IsCollision(int[,] board, Tetromino piece, int x, int y)
        {
            int pieceHeight = piece.shape.GetLength(0);
            int pieceWidth = piece.shape.GetLength(1);

            for (int py = 0; py < pieceHeight; py++)
            {
                for (int px = 0; px < pieceWidth; px++)
                {
                    if (piece.shape[py, px] != 0) {
                        int boardX = x + px;
                        int boardY = y + py;
                        if (boardX < 0 || boardX >= GameScene.GRID_WIDTH || boardY >= GameScene.GRID_HEIGHT || (boardY >= 0 && board[boardY, boardX] != 0))
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
            int[] columnHeights = new int[GameScene.GRID_WIDTH];

            for (int x = 0; x < GameScene.GRID_WIDTH; x++) {
                columnHeights[x] = GetColumnHeight(board, x);
            }

            return columnHeights;
        }

        private int GetColumnHeight(int[,] board, int column)
        {
            for (int y = 0; y < GameScene.GRID_HEIGHT; y++)
            {
                if (board[y, column] != 0)
                {
                    return GameScene.GRID_HEIGHT - y;
                }
            }

            return 0;
        }

        private int CalculateHeight(int[,] board)
        {
            int height = 0;
            int[] columnHeights = PrecalculateHeights(board);

            for (int x = 0; x < GameScene.GRID_WIDTH; x++)
            {
                height += columnHeights[x];
            }

            return height;
        }

        private static int CalculateHoles(int[,] board)
        {
            int holes = 0;
            for (int x = 0; x < GameScene.GRID_WIDTH; x++) {

                bool blockFound = false;
                for (int y = 0; y < GameScene.GRID_HEIGHT; y++)
                {
                    if (board[y, x] != 0) {
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

        private static int CalculateCompletedLines(int[,] board)
        {
            int completedLines = 0;
            for (int y = 0; y < GameScene.GRID_HEIGHT; y++)
            {
                bool isLineComplete = true;

                for (int x = 0; x < GameScene.GRID_WIDTH; x++) {
                    if (board[y, x] == 0) {
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
            
            for (int x = 0; x < GameScene.GRID_WIDTH - 1; x++) {
                bumpiness += System.Math.Abs(columnHeights[x] - columnHeights[x + 1]);
            }

            return bumpiness;
        }

        private static int[,] SimulateMove(int[,] board, Tetromino piece, int x, int y)
        {
            int[,] simulatedBoard = (int[,])board.Clone();
            int pieceHeight = piece.shape.GetLength(0);
            int pieceWidth = piece.shape.GetLength(1);
            
            for (int py = 0; py < pieceHeight; py++)
            {
                for (int px = 0; px < pieceWidth; px++)
                {
                    if (piece.shape[py, px] != 0) {

                        int boardX = x + px;
                        int boardY = y + py;

                        if (boardX >= 0 && boardX < GameScene.GRID_WIDTH && boardY >= 0 && boardY < GameScene.GRID_HEIGHT)
                        {
                            simulatedBoard[boardY, boardX] = piece.shape[py, px];
                        }
                    }
                }
            }
            return simulatedBoard;
        }
    }
}