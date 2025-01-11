namespace Tetris
{
    public static class SceneManager
    {
        private static Scene currentScene;

        public static void SetScene(Scene scene)
        {
            currentScene = scene;
            currentScene.LoadScene();
        }

        public static void UpdateScene()
        {
            currentScene?.UpdateScene();
        }

        public static void DrawScene()
        {
            currentScene?.DrawScene();
        }
    }
}
