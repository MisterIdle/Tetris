using Raylib_CsLo;

namespace Tetris {
    public class Animation {

        public static void FadeOut(float duration)
        {
            float alpha = 0.0f;

            while (alpha < 1.0f)
            {
                Raylib.DrawRectangle(0, 0, GameLoop.SCREEN_WIDTH, GameLoop.SCREEN_HEIGHT, Raylib.ColorAlpha(Raylib.BLACK, alpha));
                alpha += Raylib.GetFrameTime() / duration;
                Raylib.EndDrawing();
            }
                Raylib.DrawRectangle(0, 0, GameLoop.SCREEN_HEIGHT, GameLoop.SCREEN_WIDTH, Raylib.ColorAlpha(Raylib.BLACK, alpha));
        }
    }
}