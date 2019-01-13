using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Threading;
using System.Reflection;

namespace BUS
{
    public static class Utility
    {
        private class ExecuteMethodThread
        {
            #region Private Methods

            public object source;
            public string methodName;
            public int timeMiliSeconds;
            public object[] parameters;

            #endregion

            #region Public Methods

            public void Execute()
            {
                Thread.Sleep(timeMiliSeconds);
                source.GetType().GetMethod(methodName).Invoke(source, parameters);
            }

            #endregion
        }


        #region Properties

        /// <summary>
        /// Gets the Sprites size
        /// </summary>
        public static int Sqm
        {
            get{return 32;}
        }

        #endregion

        #region Keyboard Help Methods

        /// <summary>
        /// Checks if some of the specified keys were pressed
        /// </summary>
        /// <param name="keys">Keys collection to check</param>
        /// <returns>Returns the pressed key or empty in case none of keys were pressed</returns>
        public static string KeyPressed(Keys[] KeysToCheck)
        {
            string keyPressed = "";
            foreach (Keys key in Keyboard.GetState().GetPressedKeys())
                foreach (Keys keyToCheck in KeysToCheck)
                    if (key == keyToCheck)
                    {
                        keyPressed = key.ToString();
                        break;
                    }
            
            return keyPressed;
        }

        public static bool CanI(Thread WaitThread, out Thread NewWaitThread)
        {
            if (!WaitThread.IsAlive)
            {
                WaitThread = new Thread(new ThreadStart(Utility.Sleep));
                WaitThread.Start();
                NewWaitThread = WaitThread;
                return true;
            }
            else
            {
                NewWaitThread = WaitThread;
                return false;
            }
        }

        public static void Sleep()
        {
            Thread.Sleep(100);
        }

        #endregion

        #region Vectors Methods

        /// <summary>
        /// Gets a corresponding Vector in the SQM system
        /// </summary>
        /// <param name="Position">Current Position</param>
        /// <returns></returns>
        public static Vector2 GetVecInSqm(Vector2 Position,Directions Direction)
        {
            float xRest = Position.X % Sqm;
            float yRest = Position.Y % Sqm;

            if (xRest == 0 && yRest == 0)
                return Position;

            switch (Direction)
            {
                case Directions.Down:
                    return new Vector2(Position.X, Position.Y - yRest);
                case Directions.Up:
                    return new Vector2(Position.X, Position.Y + Sqm - yRest);
                case Directions.Right:
                    return new Vector2(Position.X - xRest, Position.Y);
                case Directions.Left:
                    return new Vector2(Position.X + Sqm - xRest, Position.Y);
                default: return Position;
            }
        }

        /// <summary>
        /// Gets the Vectors Directions
        /// </summary>
        /// <returns></returns>
        public static Dictionary<Directions, Vector2> GetDirections()
        {
            Dictionary<Directions, Vector2> dicDirections = new Dictionary<Directions, Vector2>(4);
            dicDirections.Add(Directions.Up,new Vector2(0,-1));
            dicDirections.Add(Directions.Down, new Vector2(0, 1));
            dicDirections.Add(Directions.Left, new Vector2(-1, 0));
            dicDirections.Add(Directions.Right, new Vector2(1, 0));

            return dicDirections;
        }

        public static Directions NextDirection(Directions direct)
        {
            if (direct == Directions.Up)
                return Directions.Down;

            if (direct == Directions.Right)
                return Directions.Left;

            if (direct == Directions.Down)
                return Directions.Up;

            if (direct == Directions.Left)
                return Directions.Right;

            return Directions.Stopped;
        }

        #endregion

        #region General Methods

        public static void ExecuteMethod(object Source, string MethodName, int TimeMiliSeconds, params object[] Parameters)
        {
            ExecuteMethodThread objExecMethod = new ExecuteMethodThread()
            {
                methodName = MethodName,
                source = Source,
                timeMiliSeconds = TimeMiliSeconds,
                parameters = Parameters
            };

            Thread objThread = new Thread(new ThreadStart(objExecMethod.Execute));
            objThread.Start();
        }

        #endregion
    }
}
