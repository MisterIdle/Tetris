namespace Tetris.Scene
{
    public static class SceneManager
    {
        private static Scene currentScene;

        // Sets the current scene and loads it.
        public static void SetScene(Scene scene)
        {
            currentScene = scene;
            currentScene.LoadScene();
        }

        // Updates the current scene.
        public static void UpdateScene() => currentScene?.UpdateScene();

        // Draws the current scene.
        public static void DrawScene() => currentScene?.DrawScene();
    }
}
