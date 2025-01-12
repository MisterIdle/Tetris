// Represents an abstract base class for a scene in the Tetris game.
namespace Tetris.Scene
{
    public abstract class Scene
    {
        // Loads the scene. This method must be implemented by derived classes.
        public abstract void LoadScene();

        // Updates the scene. This method can be overridden by derived classes.
        public virtual void UpdateScene() {}

        // Draws the scene. This method can be overridden by derived classes.
        public virtual void DrawScene() {}
    }
}
