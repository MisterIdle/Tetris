using Raylib_CsLo;

namespace Tetris
{
    class GameLoop
    {
        public void Run()
        {
            const int screenWidth = 800;
            const int screenHeight = 600;

            Raylib.InitWindow(screenWidth, screenHeight, "Tetris Game");

            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Raylib.RAYWHITE);
                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }
    }
}