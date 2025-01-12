using Raylib_CsLo;
using System;

namespace Tetris {
    public class Animation {

        public static void FadeOut(float duration)
        {
            float alpha = 0.0f;
            float increment = Raylib.GetFrameTime() / duration;

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