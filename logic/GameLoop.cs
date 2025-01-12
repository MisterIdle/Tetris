using Raylib_CsLo;
using Tetris.Scene;
using Tetris.Audio;
using Tetris.Options;

namespace Tetris
{
    public class GameLoop
    {
        public const int SCREEN_WIDTH = 650;
        public const int SCREEN_HEIGHT = 600;
        public Color backgroundColor = new Color(17, 24, 38, 255);
        public Font font;
        public Image icon;
        public GameState currentState;

        // Initialize the game
        private void Init()
        {
            Raylib.InitWindow(SCREEN_WIDTH, SCREEN_HEIGHT, "Tetris - By MisterIdle");
            Raylib.SetTargetFPS(60);

            Raylib.InitAudioDevice();
            Raylib.SetExitKey(-1);

            font = Raylib.LoadFont("font/Font.ttf");
            icon = Raylib.LoadImage("icon/icon.png");

            Raylib.SetWindowIcon(icon);

            SoundManager.LoadSound();
            Settings.LoadSettings();

            currentState = GameState.Menu;
            SceneManager.SetScene(new GameMenu(this));
        }

        // Switch scenes based on the current game state
        private void SwitchSceneGameStates()
        {
            switch (currentState)
            {
                case GameState.Menu:
                    SceneManager.SetScene(new GameMenu(this));
                    break;
                case GameState.Continue:
                    SceneManager.SetScene(new GameScene(this));
                    break;
                case GameState.Loading:
                    SceneManager.SetScene(new GameScene(this));
                    break;
            }
        }

        // Change the current game state and switch scenes if necessary
        public void ChangeState(GameState newState)
        {
            if (currentState != newState)
            {
                currentState = newState;
                SwitchSceneGameStates();
            }
        }

        // Main game loop
        public void Run()
        {
            Init();

            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(backgroundColor);

                SceneManager.UpdateScene();
                SceneManager.DrawScene();

                Raylib.EndDrawing();
            }

            Raylib.CloseAudioDevice();
            Raylib.UnloadFont(font);
            Raylib.UnloadImage(icon);
            Raylib.CloseWindow();
        }
    }
}
