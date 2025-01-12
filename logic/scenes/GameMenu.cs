using System.Numerics;
using Raylib_CsLo;
using System;

namespace Tetris {
    public class GameMenu : Scene {
        private GameLoop gameLoop;
        private GameScene gameScene;
        private Font font;

        private Color gridColor = new Color(35, 53, 89, 255);
        private Color borderColor = new Color(25, 43, 79, 255);

        private Color menuButtonColor = new Color(75, 100, 125, 255);
        private Color menuButtonOverColor = new Color(65, 85, 110, 255);

        private Color quitButtonColor = new Color(125, 75, 75, 255);
        private Color quitButtonOverColor = new Color(110, 65, 65, 255);

        private Color overColor = new Color(65, 85, 110, 255);

        private Color trueColor = new Color(75, 125, 75, 255);
        private Color falseColor = new Color(125, 75, 75, 255);

        private Color textColor = new Color(255, 255, 255, 255);
        private Color titleColor = new Color(255, 215, 0, 255);
        private Color sideBlockColor = new Color(15, 25, 45, 255);

        private float animationOffset = 0;
        private float animationSpeed = 0.002f;

        private MenuState menuState = MenuState.Main;
        private MenuState nextMenuState = MenuState.Main;
        private float transitionAlpha = 0;
        private bool isTransitioning = false;

        enum MenuState {
            Main,
            Settings,
            AudioSettings,
            ModdedSettings,
            Credits,
            Play,
        }

        public GameMenu(GameLoop gameLoop) {
            this.gameLoop = gameLoop;
        }

        public override void LoadScene() {
            font = Raylib.LoadFont("font/Font.ttf");

            SoundManager.StopAllAudio();

            gameScene = new GameScene(gameLoop);
        }

        public override void UpdateScene() {
            UpdateTransition();
            SoundManager.LoopAudio(SoundManager.backgroundMusicMenu);
        }

        public override void DrawScene() {
            DrawMenu();

            switch (menuState) {
                case MenuState.Main:
                    DrawMain();
                    break;

                case MenuState.Play:
                    DrawPlay();
                    break;

                case MenuState.Settings:
                    DrawSettings();
                    break;

                case MenuState.AudioSettings:
                    AudioSettings();
                    break;

                case MenuState.ModdedSettings:
                    ModdedSettings();
                    break;

                case MenuState.Credits:
                    DrawCredits();
                    break;
            }

            if (isTransitioning) {
                Raylib.DrawRectangle(GameLoop.SCREEN_WIDTH / 3 - 35, 120, GameLoop.SCREEN_WIDTH / 2 - 35, GameLoop.SCREEN_HEIGHT, new Color(gridColor.r, gridColor.g, gridColor.b, (byte)(transitionAlpha * 255)));
            }
        }

        private void StartTransition(MenuState newState) {
            isTransitioning = true;
            nextMenuState = newState;
        }

        private void UpdateTransition() {
            if (isTransitioning) 
            {
                transitionAlpha += 0.05f;

                if (transitionAlpha >= 1) {
                    transitionAlpha = 1;
                    isTransitioning = false;
                    menuState = nextMenuState;
                }
            } 
            else 
            {
                transitionAlpha -= 0.05f;
    
                if (transitionAlpha <= 0) {
                    transitionAlpha = 0;
                }
            }
        }

        private void DrawMenu() {
            animationOffset += animationSpeed;
           
            if (animationOffset > 1) animationOffset = 0;

            Raylib.DrawRectangle(GameLoop.SCREEN_WIDTH / 3 - 50, 0, GameLoop.SCREEN_WIDTH / 2, GameLoop.SCREEN_HEIGHT, gridColor);
            Raylib.DrawTextEx(font, "Tetris", new Vector2(GameLoop.SCREEN_WIDTH / 2 - 265 / 2, 50 + (float)Math.Sin(animationOffset * Math.PI * 2) * 10), 45, 0, titleColor);
            Raylib.DrawTextEx(font, "Ver 1.0", new Vector2(GameLoop.SCREEN_WIDTH / 2 - 100 / 2, 30 + (float)Math.Sin(animationOffset * Math.PI * 2) * 5), 15, 0, textColor);
            Raylib.DrawTextEx(font, "by MisterIdle / Alexy", new Vector2(GameLoop.SCREEN_WIDTH / 2 - 120, 100 + (float)Math.Sin(animationOffset * Math.PI * 2) * 5), 12, 0, textColor);

            for (int i = 0; i < 20; i++) {
                Tetromino rightBorder = new Tetromino(gameScene, 50, 0, new int[1, 1], borderColor);
                rightBorder.DrawBlock(2, i, borderColor);

                Tetromino leftBorder = new Tetromino(gameScene, 50, 0, new int[1, 1], borderColor);
                leftBorder.DrawBlock(13, i, borderColor);
            }
        }

        private void DrawMain() {
            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 190 / 2, 200, 200, 50, "Play", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.Play);
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 190 / 2, 270, 200, 50, "Settings", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.Settings);
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 190 / 2, 340, 200, 50, "Credits", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.Credits);
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 190 / 2, 500, 200, 50, "Quit", 20, quitButtonColor, quitButtonOverColor, textColor, font, () => {
                Raylib.CloseWindow();
            });
        }

        private void DrawPlay() {
            CustomElements.SwitchButton(GameLoop.SCREEN_WIDTH / 2 - 250 / 2, 135, 250, 50, "Shadow", 17, trueColor, falseColor, overColor, textColor, font, ref Settings.shadow, (value) => {
                Settings.shadow = value;
                Settings.SaveSettings();
            });

            CustomElements.SwitchButton(GameLoop.SCREEN_WIDTH / 2 - 250 / 2, 205, 250, 50, "Next Piece", 17, trueColor, falseColor, overColor, textColor, font, ref Settings.nextBlock, (value) => {
                Settings.nextBlock = value;
                Settings.SaveSettings();
            });

            CustomElements.SwitchButton(GameLoop.SCREEN_WIDTH / 2 - 250 / 2, 275, 250, 50, "Placed Blocks", 17, trueColor, falseColor, overColor, textColor, font, ref Settings.placedBlocks, (value) => {
                Settings.placedBlocks = value;
                Settings.SaveSettings();
            });

            CustomElements.SwitchButton(GameLoop.SCREEN_WIDTH / 2 - 250 / 2, 345, 250, 50, "Random Movement", 15, trueColor, falseColor, overColor, textColor, font, ref Settings.randomMovement, (value) => {
                Settings.randomMovement = value;
                Settings.SaveSettings();
            });

            CustomElements.SliderInt(GameLoop.SCREEN_WIDTH / 2 - 150 / 2, 445, 150, 25, "Level", ref Settings.level, 1, 25, 20, textColor, font, menuButtonColor, menuButtonOverColor, 10, sideBlockColor, (value) => {
                gameScene.level = value;
                Settings.level = value;
                Settings.SaveSettings();
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 150 / 2 - 70, 510, 140, 50, "Back", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.Main);
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 150 / 2 + 85, 510, 140, 50, "Start", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                if (!Settings.normal && !Settings.gnowius) {
                    StartTransition(MenuState.ModdedSettings);
                } else {
                    Animation.FadeOut(1);
                    gameLoop.ChangeState(GameState.Loading);
                }
            });
        }

        private void DrawSettings() {
            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 150 / 2, 250, 150, 50, "Audio", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.AudioSettings);
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 150 / 2, 320, 150, 50, "Modded", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.ModdedSettings);
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 150 / 2, 500, 150, 50, "Back", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.Main);
            });
        }

        private void AudioSettings() {
            CustomElements.SliderFloat(GameLoop.SCREEN_WIDTH / 2 - 240 / 2, 200, 150, 25, "Master Volume", ref Settings.masterVolume, 0, 1, 20, textColor, font, menuButtonColor, menuButtonOverColor, 10, sideBlockColor, (value) => {
                SoundManager.ChangeVolumeMaster(value);
            });

            CustomElements.SliderFloat(GameLoop.SCREEN_WIDTH / 2 - 240 / 2, 300, 150, 25, "Music Volume", ref Settings.musicVolume, 0, 1, 20, textColor, font, menuButtonColor, menuButtonOverColor, 10, sideBlockColor, (value) => {
                SoundManager.ChangeVolumeMusic(value);
            });

            CustomElements.SliderFloat(GameLoop.SCREEN_WIDTH / 2 - 240 / 2, 400, 150, 25, "SFX Volume", ref Settings.sfxVolume, 0, 1, 20, textColor, font, menuButtonColor, menuButtonOverColor, 10, sideBlockColor, (value) => {
                SoundManager.ChangeVolumeSFX(value);
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 150 / 2, 500, 150, 50, "Back", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.Settings);
            });
        }

        private void ModdedSettings() {
            CustomElements.SwitchButton(GameLoop.SCREEN_WIDTH / 2 - 250 / 2, 135, 250, 50, "Basic Tetromino", 15, trueColor, falseColor, overColor, textColor, font, ref Settings.normal, (value) => {
                Settings.normal = value;
                Settings.SaveSettings();
            });

            CustomElements.SwitchButton(GameLoop.SCREEN_WIDTH / 2 - 250 / 2, 205, 250, 50, "Gnowius Tetromino", 15, trueColor, falseColor, overColor, textColor, font, ref Settings.gnowius, (value) => {
                Settings.gnowius = value;
                Settings.SaveSettings();
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 150 / 2, 500, 150, 50, "Back", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.Settings);
            });
        }

        private void DrawCredits() {
            Raylib.DrawTextEx(font, "Technologies", new Vector2(GameLoop.SCREEN_WIDTH / 2 - 110, 200), 20, 0, textColor);
            Raylib.DrawTextEx(font, "Raylib", new Vector2(GameLoop.SCREEN_WIDTH / 2 - 50, 260), 20, 0, textColor);
            Raylib.DrawTextEx(font, "Raylib-CsLo", new Vector2(GameLoop.SCREEN_WIDTH / 2 - 100, 300), 20, 0, textColor);
            Raylib.DrawTextEx(font, "Newtonsoft.Json", new Vector2(GameLoop.SCREEN_WIDTH / 2 - 140, 340), 20, 0, textColor);
            Raylib.DrawTextEx(font, "Chiptone", new Vector2(GameLoop.SCREEN_WIDTH / 2 - 70, 380), 20, 0, textColor);

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 150 / 2, 500, 150, 50, "Back", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.Main);
            });
        }
    }
}
