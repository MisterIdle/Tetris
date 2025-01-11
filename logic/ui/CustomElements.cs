using System;
using Raylib_CsLo;

namespace Tetris
{
    public class CustomElements
    {
        public static void Button(int x, int y, int width, int height, string text, Color color, Action action)
        {
            Raylib.DrawRectangleRounded(new Rectangle(x, y, width, height), 0.2f, 10, color);
            Raylib.DrawText(text, x + 10, y + 10, 20, Raylib.WHITE);

            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), new Rectangle(x, y, width, height)))
            {
                Raylib.DrawRectangleRounded(new Rectangle(x, y, width, height), 0.2f, 10, Raylib.ColorAlpha(Raylib.WHITE, 0.3f));

                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT) || Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    action();
                }
            }
        }

        public static void UpDownButton(int x, int y, int width, int height, string text, Color color, Action action)
        {
            Raylib.DrawRectangleRounded(new Rectangle(x, y, width, height), 0.2f, 10, color);
            Raylib.DrawText(text, x + 10, y + 10, 20, Raylib.WHITE);

            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), new Rectangle(x, y, width, height)))
            {
                Raylib.DrawRectangleRounded(new Rectangle(x, y, width, height), 0.2f, 10, Raylib.ColorAlpha(Raylib.WHITE, 0.3f));

                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT) || Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    action();
                }
            }
        }
    }
}