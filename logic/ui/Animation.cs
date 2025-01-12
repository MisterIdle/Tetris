using Raylib_CsLo;

namespace Tetris.UI {
    public class Animation {

        // Method to perform a fade-out animation over a specified duration
        public static void FadeOut(float duration)
        {
            float alpha = 0.0f; // Initial alpha value (fully transparent)
            float increment = Raylib.GetFrameTime() / duration; 

            // Loop until the alpha value reaches 1.0 (fully opaque)
            while (alpha < 1.0f)
            {
                Raylib.BeginDrawing();
                Raylib.DrawRectangle(0, 0, GameLoop.SCREEN_WIDTH, GameLoop.SCREEN_HEIGHT, Raylib.ColorAlpha(Raylib.BLACK, alpha));
                Raylib.EndDrawing();

                alpha += increment;
                Raylib.WaitTime(0.01f);
            }
        }
    }
}