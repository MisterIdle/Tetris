// NOTE
// This YouTube video helped me understand the logic: https://www.youtube.com/watch?v=DqEirMq7sD0&t

using Tetris.Scene;

namespace Tetris.Block {
    public class Bot {

        // Reference to the game scene, used to access elements like the next Tetrominos
        GameScene gameScene;

        // Constructor initializing the game scene
        public Bot(GameScene gameScene) {
            this.gameScene = gameScene;
        }

        // Main method that determines the best move for the bot
        public (int, int, int) GetBestMove(int[,] board, Tetromino piece)
        {
            // Variables to store the best coordinates and rotation
            int bestX = 0;
            int bestY = 0;
            int bestRotation = 0;
            float bestScore = float.MinValue; // Highest score found

            // Test all possible rotations of the current piece
            for (int rotation = 0; rotation < 4; rotation++) {

                // Create a copy of the piece and apply the current rotation
                Tetromino rotatedPiece = piece.Clone();
                rotatedPiece.Rotate(rotation);

                // Test all possible horizontal positions for this rotation
                for (int x = -rotatedPiece.shape.GetLength(1) + 1; x < GameScene.GRID_WIDTH; x++)
                {
                    // Find the vertical drop position for this horizontal position
                    int dropY = GetDropY(board, rotatedPiece, x);
                    if (dropY != -1)
                    {
                        // Simulate the board state after this move
                        int[,] simulatedBoard = SimulateMove(board, rotatedPiece, x, dropY);
                        float score = EvaluateBoard(simulatedBoard);

                        // Perform a deeper search to account for the next Tetromino
                        for (int nextRotation = 0; nextRotation < 4; nextRotation++)
                        {
                            Tetromino nextRotatedPiece = gameScene.nextTetromino.Clone();
                            nextRotatedPiece.Rotate(nextRotation);
                            for (int nextX = -nextRotatedPiece.shape.GetLength(1) + 1; nextX < GameScene.GRID_WIDTH; nextX++)
                            {
                                int nextDropY = GetDropY(simulatedBoard, nextRotatedPiece, nextX);
                                if (nextDropY != -1)
                                {
                                    // Simulate the board after the next Tetromino's move
                                    int[,] nextSimulatedBoard = SimulateMove(simulatedBoard, nextRotatedPiece, nextX, nextDropY);
                                    float nextScore = EvaluateBoard(nextSimulatedBoard);

                                    // Continue with further lookahead for future Tetromino
                                    for (int futureRotation = 0; futureRotation < 4; futureRotation++)
                                    {
                                        Tetromino futureRotatedPiece = gameScene.nextTetromino.Clone();
                                        futureRotatedPiece.Rotate(futureRotation);
                                        
                                        for (int futureX = -futureRotatedPiece.shape.GetLength(1) + 1; futureX < GameScene.GRID_WIDTH; futureX++)
                                        {
                                            int futureDropY = GetDropY(nextSimulatedBoard, futureRotatedPiece, futureX);
                                            if (futureDropY != -1)
                                            {
                                                // Simulate the board after placing the future Tetromino
                                                int[,] futureSimulatedBoard = SimulateMove(nextSimulatedBoard, futureRotatedPiece, futureX, futureDropY);
                                                float futureScore = EvaluateBoard(futureSimulatedBoard);

                                                // Update the best move if the current one is better
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
        
        // Calculates the Y-coordinate where the piece will land when dropped
        private int GetDropY(int[,] board, Tetromino piece, int x)
        {
            for (int y = 0; y < GameScene.GRID_HEIGHT; y++)
            {
                if (IsCollision(board, piece, x, y))
                {
                    return y - 1; // Return the position just before collision
                }
            }
            return -1; // Indicates no valid drop position
        }

        // Checks if placing the piece at the specified position causes a collision
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
                            return true; // Collision detected
                        }
                    }
                }
            }
            return false; // No collision
        }

        // Evaluates the board state and assigns a score based on heuristics
        private float EvaluateBoard(int[,] board)
        {
            float score = 0.0f;

            // Calculate features to evaluate the board
            int height = CalculateHeight(board);
            int holes = CalculateHoles(board);
            int completedLines = CalculateCompletedLines(board);
            int bumpiness = CalculateBumpiness(board);

            // Weighted scoring system
            // The coefficients were determined by research papers not conducted by me

            score -= 0.510066f * height;          // Penalize height
            score -= 0.35663f * holes;           // Penalize holes
            score += 0.760666f * completedLines; // Reward completed lines
            score -= 0.184483f * bumpiness;      // Penalize bumpiness

            return score;
        }

        // Precomputes the height of each column in the board
        private int[] PrecalculateHeights(int[,] board)
        {
            int[] columnHeights = new int[GameScene.GRID_WIDTH];

            for (int x = 0; x < GameScene.GRID_WIDTH; x++) {
                columnHeights[x] = GetColumnHeight(board, x);
            }

            return columnHeights;
        }

        // Calculates the height of a specific column
        private int GetColumnHeight(int[,] board, int column)
        {
            for (int y = 0; y < GameScene.GRID_HEIGHT; y++)
            {
                if (board[y, column] != 0)
                {
                    return GameScene.GRID_HEIGHT - y; // Return height
                }
            }

            return 0; // No blocks in the column
        }

        // Calculates the total height of all columns
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

        // Counts the number of holes in the board (empty cells with blocks above them)
        private static int CalculateHoles(int[,] board)
        {
            int holes = 0;
            for (int x = 0; x < GameScene.GRID_WIDTH; x++) {

                bool blockFound = false;
                for (int y = 0; y < GameScene.GRID_HEIGHT; y++)
                {
                    if (board[y, x] != 0) {
                        blockFound = true; // Block found, start counting holes
                    }
                    
                    else if (blockFound && board[y, x] == 0)
                    {
                        holes++;
                    }
                }
            }
            return holes;
        }

        // Counts the number of completed lines on the board
        private static int CalculateCompletedLines(int[,] board)
        {
            int completedLines = 0;
            for (int y = 0; y < GameScene.GRID_HEIGHT; y++)
            {
                bool isLineComplete = true;

                for (int x = 0; x < GameScene.GRID_WIDTH; x++) {
                    if (board[y, x] == 0) {
                        isLineComplete = false;
                        break; // Stop checking if the line is incomplete
                    }
                }
                if (isLineComplete)
                {
                    completedLines++;
                }
            }
            return completedLines;
        }

        // Calculates the bumpiness (height difference between adjacent columns)
        private int CalculateBumpiness(int[,] board)
        {
            int bumpiness = 0;
            int[] columnHeights = PrecalculateHeights(board);
            
            for (int x = 0; x < GameScene.GRID_WIDTH - 1; x++) {
                bumpiness += System.Math.Abs(columnHeights[x] - columnHeights[x + 1]);
            }

            return bumpiness;
        }

        // Simulates placing a Tetromino on the board and returns the resulting board
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
