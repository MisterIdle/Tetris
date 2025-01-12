using Raylib_CsLo;

namespace Tetris
{
    public class GameLoop
    {
        public const int SCREEN_WIDTH = 650;
        public const int SCREEN_HEIGHT = 600;
        public Color backgroundColor = new Color(17, 24, 38, 255);
        public Font font;
        public GameState currentState;

        private void Init()
        {
            Raylib.InitWindow(SCREEN_WIDTH, SCREEN_HEIGHT, "Tetris - By MisterIdle");
            Raylib.SetTargetFPS(60);

            Raylib.InitAudioDevice();

            Raylib.SetExitKey(-1);

            font = Raylib.LoadFont("font/Font.ttf");

            SoundManager.LoadSound();
            Settings.LoadSettings();

            currentState = GameState.Menu;
            SceneManager.SetScene(new GameMenu(this));
        }

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

        public void ChangeState(GameState newState)
        {
            if (currentState != newState)
            {
                currentState = newState;
                SwitchSceneGameStates();
            }
        }

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
            Raylib.CloseWindow();
        }

        public void Quit()
        {
            Raylib.EndDrawing();
            Raylib.CloseAudioDevice();
            Raylib.UnloadFont(font);
            Raylib.CloseWindow();
        }
    }
}
