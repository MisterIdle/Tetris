using System.Numerics;
using Raylib_CsLo;
using System.IO;
using System;

using Tetris.Block;
using Tetris.Audio;
using Tetris.Options;
using Tetris.UI;

namespace Tetris.Scene {
    public class GameMenu : Scene {
        private GameLoop gameLoop;
        private GameScene gameScene;
        private Font font;

        // Color definitions for various elements in the menu
        private Color gridColor = new Color(35, 53, 89, 255);
        private Color borderColor = new Color(25, 43, 79, 255);

        private Color menuButtonColor = new Color(75, 100, 125, 255);
        private Color menuButtonOverColor = new Color(65, 85, 110, 255);

        private Color howToPlayButtonColor = new Color(75, 125, 75, 255);
        private Color howToPlayButtonOverColor = new Color(65, 110, 65, 255);

        private Color quitButtonColor = new Color(125, 75, 75, 255);
        private Color quitButtonOverColor = new Color(110, 65, 65, 255);

        private Color overColor = new Color(65, 85, 110, 255);

        private Color trueColor = new Color(75, 125, 75, 255);
        private Color falseColor = new Color(125, 75, 75, 255);

        private Color textColor = new Color(255, 255, 255, 255);
        private Color titleColor = new Color(255, 215, 0, 255);
        private Color sideBlockColor = new Color(15, 25, 45, 255);

        private float animationOffset = 0;
        private float animationSpeed = 0.002f;  // Speed for the title animation

        private MenuState menuState = MenuState.Main;  // Current state of the menu
        private MenuState nextMenuState = MenuState.Main;  // Next state after transition
        private float transitionAlpha = 0;  // Transition effect opacity
        private bool isTransitioning = false;  // Flag for menu transition

        // Enum for different menu states
        enum MenuState {
            Main,
            Settings,
            AudioSettings,
            HowToPlay,
            Credits,
            Play,
        }

        // Constructor for the GameMenu class
        public GameMenu(GameLoop gameLoop) {
            this.gameLoop = gameLoop;
        }

        // Load the scene, initialize the font and stop all audio
        public override void LoadScene() {
            font = Raylib.LoadFont("font/Font.ttf");

            SoundManager.StopAllAudio();

            gameScene = new GameScene(gameLoop);
        }

        // Update the scene by checking transitions and looping background music
        public override void UpdateScene() {
            UpdateTransition();
            SoundManager.LoopAudio(SoundManager.backgroundMusicMenu);
        }

        // Draw the scene, depending on the menu state
        public override void DrawScene() {
            DrawMenu();

            // Draw specific screen content based on the menu state
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
                    DrawAudioSettings();
                    break;

                case MenuState.HowToPlay:
                    DrawHowToPlaySettings();
                    break;

                case MenuState.Credits:
                    DrawCredits();
                    break;
            }

            // Draw transition effect if transitioning between states
            if (isTransitioning) {
                Raylib.DrawRectangle(GameLoop.SCREEN_WIDTH / 3 - 35, 120, GameLoop.SCREEN_WIDTH / 2 - 35, GameLoop.SCREEN_HEIGHT, new Color(gridColor.r, gridColor.g, gridColor.b, (byte)(transitionAlpha * 255)));
            }
        }

        // Start the transition effect to a new menu state
        private void StartTransition(MenuState newState) {
            isTransitioning = true;
            nextMenuState = newState;
        }

        // Update the transition effect opacity during scene change
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

        // Draw the main menu layout with title and borders
        private void DrawMenu() {
            animationOffset += animationSpeed;
           
            if (animationOffset > 1) animationOffset = 0;

            Raylib.DrawRectangle(GameLoop.SCREEN_WIDTH / 3 - 50, 0, GameLoop.SCREEN_WIDTH / 2, GameLoop.SCREEN_HEIGHT, gridColor);
            Raylib.DrawTextEx(font, "Tetris", new Vector2(GameLoop.SCREEN_WIDTH / 2 - 265 / 2, 50 + (float)Math.Sin(animationOffset * Math.PI * 2) * 10), 45, 0, titleColor);
            Raylib.DrawTextEx(font, "Ver 1.0", new Vector2(GameLoop.SCREEN_WIDTH / 2 - 100 / 2, 30 + (float)Math.Sin(animationOffset * Math.PI * 2) * 5), 15, 0, textColor);
            Raylib.DrawTextEx(font, "by MisterIdle / Alexy", new Vector2(GameLoop.SCREEN_WIDTH / 2 - 120, 100 + (float)Math.Sin(animationOffset * Math.PI * 2) * 5), 12, 0, textColor);

            // Draw borders for the game screen
            for (int i = 0; i < 20; i++) {
                Tetromino rightBorder = new Tetromino(gameScene, 50, 0, new int[1, 1], borderColor);
                rightBorder.DrawBlock(2, i, borderColor);

                Tetromino leftBorder = new Tetromino(gameScene, 50, 0, new int[1, 1], borderColor);
                leftBorder.DrawBlock(13, i, borderColor);
            }
        }

        // Draw the main menu buttons
        private void DrawMain() {
            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 250 / 2, 165, 250, 50, "Play", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.Play);
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 250 / 2, 235, 250, 50, "Continue", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                if (File.Exists("json/save/game.save")) {
                    Animation.FadeOut(1);
                    gameLoop.ChangeState(GameState.Continue);
                } else {
                    StartTransition(MenuState.Play);
                }
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 250 / 2, 305, 250, 50, "How to play", 20, howToPlayButtonColor, howToPlayButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.HowToPlay);
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 250 / 2, 375, 250, 50, "Settings", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.Settings);
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 250 / 2, 445, 250, 50, "Credits", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.Credits);
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 250 / 2, 530, 250, 50, "Quit", 20, quitButtonColor, quitButtonOverColor, textColor, font, () => {
                Raylib.CloseWindow();
            });
        }

        // Draw the play settings page
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

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 150 / 2 - 67, 510, 140, 50, "Back", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.Main);
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 150 / 2 + 83, 510, 140, 50, "Start", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                if (!Settings.normal && !Settings.gnowius) {
                    StartTransition(MenuState.Settings);
                } else {
                    Animation.FadeOut(1);
                    gameLoop.ChangeState(GameState.Loading);
                }
            });
        }

        // Draw the settings page with audio and challenge settings
        private void DrawSettings() {
            Raylib.DrawTextEx(font, "Settings", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "Settings", 20, 0).X / 2, 170), 20, 0, textColor);

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 250 / 2, 210, 250, 50, "Audio Settings", 15, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.AudioSettings);
            });

            Raylib.DrawTextEx(font, "Challenge", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "Challenge", 20, 0).X / 2, 310), 20, 0, textColor);
            
            CustomElements.SwitchButton(GameLoop.SCREEN_WIDTH / 2 - 250 / 2, 355, 250, 50, "Basic Tetromino", 15, trueColor, falseColor, overColor, textColor, font, ref Settings.normal, (value) => {
                Settings.normal = value;
                Settings.SaveSettings();
            });

            CustomElements.SwitchButton(GameLoop.SCREEN_WIDTH / 2 - 250 / 2, 425, 250, 50, "Gnowius Tetromino", 15, trueColor, falseColor, overColor, textColor, font, ref Settings.gnowius, (value) => {
                Settings.gnowius = value;
                Settings.SaveSettings();
            });

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 150 / 2, 500, 150, 50, "Back", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.Main);
            });
        }

        // Draw the audio settings page with volume sliders
        private void DrawAudioSettings() {
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

        // Draw the how to play settings page with control instructions
        private void DrawHowToPlaySettings() {
            Raylib.DrawTextEx(font, "Rotate", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "Rotate", 20, 0).X / 2, 155), 20, 0, textColor);
            Raylib.DrawTextEx(font, "< UP Arrow >", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "< UP Arrow >", 14, 0).X / 2, 185), 14, 0, textColor);

            Raylib.DrawTextEx(font, "Move", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "Move", 20, 0).X / 2, 215), 20, 0, textColor);
            Raylib.DrawTextEx(font, "< LEFT/RIGHT Arrow >", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "< LEFT/RIGHT Arrow >", 14, 0).X / 2, 245), 14, 0, textColor);

            Raylib.DrawTextEx(font, "Soft Drop", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "Soft Drop", 20, 0).X / 2, 275), 20, 0, textColor);
            Raylib.DrawTextEx(font, "< DOWN Arrow >", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "< DOWN Arrow >", 14, 0).X / 2, 305), 14, 0, textColor);

            Raylib.DrawTextEx(font, "Hard Drop", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "Hard Drop", 20, 0).X / 2, 335), 20, 0, textColor);
            Raylib.DrawTextEx(font, "< SPACE >", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "< SPACE >", 14, 0).X / 2, 365), 14, 0, textColor);

            Raylib.DrawTextEx(font, "Pause", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "Pause", 20, 0).X / 2, 395), 20, 0, textColor);
            Raylib.DrawTextEx(font, "< P / ESC >", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "< P / ESC >", 14, 0).X / 2, 425), 14, 0, textColor);

            Raylib.DrawTextEx(font, "Bot", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "Bot", 20, 0).X / 2, 455), 20, 0, textColor);
            Raylib.DrawTextEx(font, "< F9 / F10 >", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "< F9 / F10 >", 14, 0).X / 2, 485), 14, 0, textColor);
            
            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 150 / 2, 515, 150, 50, "Back", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.Main);
            });
        }

        // Draw the credits page with technology information
        private void DrawCredits() {
            Raylib.DrawTextEx(font, "Technologies", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "Technologies", 20, 0).X / 2, 200), 20, 0, textColor);
            Raylib.DrawTextEx(font, "Raylib", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "Raylib", 20, 0).X / 2, 260), 17, 0, textColor);
            Raylib.DrawTextEx(font, "Raylib-CsLo", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "Raylib-CsLo", 17, 0).X / 2, 300), 17, 0, textColor);
            Raylib.DrawTextEx(font, "Newtonsoft.Json", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "Newtonsoft.Json", 17, 0).X / 2, 340), 17, 0, textColor);
            Raylib.DrawTextEx(font, "Chiptone", new Vector2(GameLoop.SCREEN_WIDTH / 2 - Raylib.MeasureTextEx(font, "Chiptone", 17, 0).X / 2, 380), 17, 0, textColor);

            CustomElements.Button(GameLoop.SCREEN_WIDTH / 2 - 150 / 2, 500, 150, 50, "Back", 20, menuButtonColor, menuButtonOverColor, textColor, font, () => {
                StartTransition(MenuState.Main);
            });
        }
    }
}
