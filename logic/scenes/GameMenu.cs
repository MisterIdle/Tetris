using System.Numerics;
using Raylib_CsLo;

namespace Tetris {
    public class GameMenu : Scene {
        private GameLoop gameLoop;
        private Font font;

        private Color gridColor = new Color(35, 53, 89, 255);
        private Color borderColor = new Color(39, 61, 89, 255);

        private Color menuButtonColor = new Color(75, 100, 125, 255);
        private Color menuButtonOverColor = new Color(65, 85, 110, 255);

        private Color quitButtonColor = new Color(125, 75, 75, 255);
        private Color quitButtonOverColor = new Color(110, 65, 65, 255);

        private Color textColor = new Color(255, 255, 255, 255);
        private Color titleColor = new Color(255, 215, 0, 255);
        private Color sideBlockColor = new Color(15, 25, 45, 255);

        public GameMenu(GameLoop gameLoop) {
            this.gameLoop = gameLoop;
        }

        public override void LoadScene() {
            font = Raylib.LoadFont("font/Font.ttf");
            Raylib.PlaySound(SoundManager.backgroundMusicGame);
        }

        public override void UpdateScene() {
        }

        public override void DrawScene() {
            DrawMenu();
        }

        private void DrawMenu() {
            int menuWidth = GameLoop.SCREEN_WIDTH / 2 + 150;
            int menuX = GameLoop.SCREEN_WIDTH / 2 - menuWidth / 2;

            Raylib.DrawRectangle(menuX + 3, 3, menuWidth, GameLoop.SCREEN_HEIGHT, borderColor);
            Raylib.DrawRectangle(menuX, 0, menuWidth, GameLoop.SCREEN_HEIGHT, gridColor);

            Vector2 titlePosition = new Vector2(GameLoop.SCREEN_WIDTH / 2 - GameLoop.SCREEN_HEIGHT / 4, 50);
            Raylib.DrawTextEx(font, "Tetris", titlePosition, 60, 0, titleColor);

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 150 / 2, 200, 150, 50, "Play", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                gameLoop.ChangeState(GameState.Loading);
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 150 / 2, 270, 150, 50, "Settings", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 150 / 2, 340, 150, 50, "Quit", 20, quitButtonColor, quitButtonOverColor, textColor, font, () => {
                Raylib.CloseWindow();
            });

            CustomElements.Slider(GameLoop.SCREEN_WIDTH / 2 - 150 / 2, 410, 150, 25, "Music Volume", ref gameLoop.musicVolume, 0, 1, 20, textColor, font, menuButtonColor, menuButtonOverColor, 10, sideBlockColor, (value) => {
                SoundManager.ChangeVolumeMusic(value);
            });

            CustomElements.Slider(GameLoop.SCREEN_WIDTH / 2 - 150 / 2, 480, 150, 25, "SFX Volume", ref gameLoop.sfxVolume, 0, 1, 20, textColor, font, menuButtonColor, menuButtonOverColor, 10, sideBlockColor, (value) => {
                SoundManager.ChangeSFXVolume(value);
            });
        }
    }
}
