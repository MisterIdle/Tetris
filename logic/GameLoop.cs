using Raylib_CsLo;

namespace Tetris
{
    public class GameLoop
    {
        public const int SCREEN_WIDTH = 650;
        public const int SCREEN_HEIGHT = 600;
        public Color backgroundColor = new Color(17, 24, 38, 255);
        public Font font;
        private Sound backgroundMusic;
        public GameState currentState;

        private void Init()
        {
            Raylib.InitWindow(SCREEN_WIDTH, SCREEN_HEIGHT, "Tetris");
            Raylib.SetTargetFPS(60);

            Raylib.InitAudioDevice();

            backgroundMusic = Raylib.LoadSound("audio/Theme.mp3");

            font = Raylib.LoadFont("font/Font.ttf");

            currentState = GameState.Loading;
            SceneManager.SetScene(new GameScene(this));
        }

        private void SwitchSceneGameStates()
        {
            switch (currentState)
            {
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

                //LoopAudio(backgroundMusic);

                Raylib.EndDrawing();
            }

            Raylib.CloseAudioDevice();

            Raylib.CloseWindow();
        }

        public void LoopAudio(Sound sound)
        {
            if (!Raylib.IsSoundPlaying(sound))
            {
                Raylib.PlaySound(sound);
            }
        }
    }

    public enum GameState
    {
        Menu,
        Loading,
        Playing,
        Paused,
        GameOver
    }
}
