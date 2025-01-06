using Raylib_CsLo;

namespace Tetris {
    class Input {

        public static void InputManager()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_UP))
                GameLoop.currentBlock.RotateTetromino();

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_LEFT))
                GameLoop.currentBlock.MoveTetromino(-1, 0);

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_RIGHT))
                GameLoop.currentBlock.MoveTetromino(1, 0);

            if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN))
                GameLoop.currentBlock.SetFallSpeed(GameLoop.fastFallSpeed);
            else
                GameLoop.currentBlock.SetFallSpeed(GameLoop.normalFallSpeed);

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
                GameLoop.currentBlock.PlaceBlockAtBottom();

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_R))
                GameLoop.isRobotPlaying = !GameLoop.isRobotPlaying;
        }

        public static void HandleGameOverInput()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
            {
                Score.score = 0;
                Score.level = 0;
                Score.linesCleared = 0;
                GameLoop.InitializeGrid();
                GameLoop.currentState = GameState.Playing;
            }
        }

    }
}