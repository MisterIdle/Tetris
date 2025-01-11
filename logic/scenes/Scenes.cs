namespace Tetris
{
    public abstract class Scene
    {
        public abstract void LoadScene();

        public virtual void UpdateScene() {}

        public virtual void DrawScene() {}
    }
}
