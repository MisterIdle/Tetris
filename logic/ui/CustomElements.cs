using System;
using System.Numerics;
using Raylib_CsLo;

namespace Tetris
{
    public class CustomElements
    {
        public static void Button(int x, int y, int width, int height, string text, int size, Color color, Color overColor, Color textColor, Font font, Action onClick)
        {
            bool isHovered = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), new Rectangle(x, y, width, height));
            if (isHovered)
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

        public static void Slider(int x, int y, int width, int height, string labelText, ref float value, float minValue, float maxValue, int size, Color textColor, Font font, Color sliderColor, Color handleColor, int handleRadius, Color backgroundColor, Action<float> onValueChanged)
        {
            Vector2 labelSize = Raylib.MeasureTextEx(font, labelText, size, 0);
            Raylib.DrawTextEx(font, labelText, new Vector2(x + width / 2 - labelSize.X / 2, y - labelSize.Y - 5), size, 0, textColor);

            Raylib.DrawRectangle(x, y, width, height, backgroundColor);

            float sliderWidth = (value - minValue) / (maxValue - minValue) * width;
            Raylib.DrawRectangle(x, y, (int)sliderWidth, height, sliderColor);

            int handleX = (int)(x + sliderWidth);
            Raylib.DrawCircle(handleX, y + height / 2, handleRadius, handleColor);

            string valueText = $"{value:F2}";
            Vector2 valueSize = Raylib.MeasureTextEx(font, valueText, size, 0);
            Raylib.DrawTextEx(font, valueText, new Vector2(x + width + 10, y + height / 2 - valueSize.Y / 2), size, 0, textColor);

            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), new Rectangle(x, y, width, height)) && Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                float newValue = (Raylib.GetMouseX() - x) / (float)width * (maxValue - minValue) + minValue;
                newValue = Math.Clamp(newValue, minValue, maxValue);
                
                if (newValue != value)
                {
                    value = newValue;
                    onValueChanged(value);
                }
            }
        }
    }
}