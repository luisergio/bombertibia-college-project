using System;

namespace BomberMan
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (BomberMan game = new BomberMan())
            {
                game.Run();
            }
        }
    }
}

