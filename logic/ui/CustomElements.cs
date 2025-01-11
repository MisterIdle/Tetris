using System;
using System.Numerics;
using Raylib_CsLo;

namespace Tetris
{
    public class CustomElements
    {
        public static void Button(int x, int y, int width, int height, string text, int size, Color color, Color overColor, Color textColor, Font font, Action onClick)
        {
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), new Rectangle(x, y, width, height)))
            {
                Raylib.DrawRectangle(x, y, width, height, overColor);
                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    onClick();
                }
            }
            else
            {
                Raylib.DrawRectangle(x, y, width, height, color);
            }
            Vector2 textSize = Raylib.MeasureTextEx(font, text, size, 0);
            Raylib.DrawTextEx(font, text, new Vector2(x + width / 2 - textSize.X / 2, y + height / 2 - textSize.Y / 2), size, 0, textColor);
        }
    }
}