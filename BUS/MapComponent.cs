using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BUS
{
    /// <summary>
    /// Abstract class that represents a Map Component
    /// </summary>
    public abstract class MapComponent
    {
        #region Protected Variables

        protected List<Sprite> sprites = new List<Sprite>();
        protected float ellapsedTime = 0;
        protected bool isStackable = true;
        protected Map map;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the Component Map
        /// </summary>
        public Map Map
        {
            get { return this.map; }
        }

        /// <summary>
        /// Gets if this map component can be add togheter another one of the same type
        /// </summary>
        public bool IsStackable
        {
            get { return this.isStackable; }
        }

        /// <summary>
        /// Gets the component Sprites
        /// </summary>
        public List<Sprite> Sprites
        {
            get { return this.sprites; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Deafult constructor with the Component Map reference
        /// </summary>
        /// <param name="Map"></param>
        public MapComponent(Map Map)
        {
            this.map = Map;
        }

        #endregion

        #region Public Virtual Methods

        /// <summary>
        /// Draws the Map Component
        /// </summary>
        /// <param name="SpriteBatch">Sprite Batch to render Sprites</param>
        public virtual void Draw(SpriteBatch SpriteBatch)
        {
        }

        /// <summary>
        /// Updates the Map Component
        /// </summary>
        /// <param name="Time">Game Time to calculate the ellapsed time</param>
        public virtual void Update(GameTime Time)
        {
        }

        /// <summary>
        /// Called when a fire reach the map component
        /// </summary>
        /// <param name="position"></param>
        public virtual void FireReached()
        {
        }

        #endregion

        #region Public Method

        /// <summary>
        /// Check if the Map Component has a sprite in a specific position
        /// </summary>
        /// <returns></returns>
        public bool IsInPosition(Vector2 Position)
        {
            switch (this.GetType().ToString().Split('.')[1])
            {
                case "Machine": return ((Machine)this).RealPosition.Equals(Position);
                case "Player": return ((Player)this).RealPosition.Equals(Position);
                default:
                    foreach (Sprite sprite in this.sprites)
                        if (sprite.Position.Equals(Position))
                            return true;
                    return false;
            }

            //foreach (Sprite sprite in this.sprites)
            //    if (sprite.Position.Equals(Position))
            //        return true;
            //return false;
        }

        /// <summary>
        /// Gets the component sprite at specific position
        /// </summary>
        /// <param name="position">Position to find the sprite</param>
        /// <returns>Return the sprite at position specified or null in case not exist sprite at that position</returns>
        public Sprite this[Vector2 position]
        {
            get
            {
                foreach (Sprite sprite in sprites)
                    if (sprite.Position.Equals(position))
                        return sprite;
                return null;
            }
        }

        #endregion
    }
}
