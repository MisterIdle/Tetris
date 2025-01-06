using Raylib_CsLo;

namespace Tetris {
    public class Score {
        public static int score = 0;
        public static int level = 0;
        public static int linesCleared = 0;

        public static void DrawScore()
        {
            Raylib.DrawText($"Score: {score}", GameLoop.SCREEN_WIDTH - 180, 20, GameLoop.textSize, GameLoop.textColor);
            Raylib.DrawText($"Level: {level}", GameLoop.SCREEN_WIDTH - 180, 50, GameLoop.textSize, GameLoop.textColor);
        }

        public static void IncreaseScore(int lineCount)
        {
            int baseScore = 0;

            switch (lineCount)
            {
                case 1: baseScore = 40; break;
                case 2: baseScore = 100; break;
                case 3: baseScore = 300; break;
                case 4: baseScore = 1200; break;
            }

            score += baseScore * (level + 1);
            linesCleared += lineCount;

            level = linesCleared / 10;
        }
    }
}