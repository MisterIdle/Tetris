using System;

namespace Tetris
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a new instance of the GameLoop class
            GameLoop game = new GameLoop();
            
            // Start the game loop
            game.Run();
        }
    }
}
