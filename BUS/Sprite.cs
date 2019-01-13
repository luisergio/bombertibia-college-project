using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BUS
{
    /// <summary>
    /// Represents Game Sprite
    /// </summary>
    public class Sprite
    {
        #region Private Variables

        protected List<Texture2D> textures;
        protected Vector2 positon;

        protected float frameRate;
        protected float ellapsedTime;
        protected int currentIndex;
        protected float layer;
        protected bool isWalkable = true;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets and Sets the Sprite Position
        /// </summary>
        public Vector2 Position
        {
            get { return this.positon; }
            set { this.positon = value; }
        }

        /// <summary>
        /// Gets and Sets the Sprite animation Frame Rate
        /// </summary>
        public float FrameRate
        {
            get { return this.frameRate; }
            set { this.frameRate = value; }
        }

        /// <summary>
        /// Gets and Sets the Sprite Layer Depth to draw
        /// </summary>
        public float LayerDepth
        {
            get { return this.layer; }
            set { this.layer = value; }
        }

        /// <summary>
        /// Gets and Sets if this Sprite is Wakable
        /// </summary>
        public bool IsWalkable
        {
            get { return this.isWalkable; }
            set { this.isWalkable = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new Sprite
        /// </summary>
        /// <param name="Texture">Texture</param>
        /// <param name="X">X position on Map</param>
        /// <param name="Y">Y position on Map</param>
        /// <param name="LayerDepth">Layer Depth to use on Draw Method</param>
        public Sprite(Texture2D Texture, int X, int Y,float LayerDepth)
            : this(new List<Texture2D>() { Texture }, new Vector2(X, Y), 0, LayerDepth)
        {
        }

        /// <summary>
        /// Constructs a new Sprite
        /// </summary>
        /// <param name="Texture">Texture</param>
        /// <param name="Position">Vector position on Map</param>
        /// <param name="LayerDepth">Layer Depth to use on Draw Method</param>
        public Sprite(Texture2D Texture, Vector2 Position, float LayerDepth)
            : this(new List<Texture2D>() { Texture }, Position, 0, LayerDepth)
        {
        }

        /// <summary>
        /// Constructs a new Sprite
        /// </summary>
        /// <param name="Textures">Textures list</param>
        /// <param name="X">X position on Map</param>
        /// <param name="Y">Y position on Map</param>
        /// <param name="FrameRate">Rate to perform sprite animation</param>
        /// <param name="LayerDepth">Layer Depth to use on Draw Method</param>
        public Sprite(List<Texture2D> Textures, int X, int Y, float FrameRate, float LayerDepth)
            :this(Textures,new Vector2(X,Y),FrameRate,LayerDepth)
        {

        }

        /// <summary>
        /// Constructs a new Sprite
        /// </summary>
        /// <param name="Textures">Textures list</param>
        /// <param name="Position">Vector position on map</param>
        /// <param name="FrameRate">Rate to perform sprite animation</param>
        /// <param name="LayerDepth">Layer Depth to use on Draw Method</param>
        public Sprite(List<Texture2D> Textures, Vector2 Position, float FrameRate, float LayerDepth)
        {
            this.textures = Textures;
            this.positon = Position;
            this.frameRate = FrameRate;
            this.currentIndex = 0;
            this.layer = LayerDepth;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new texture to this Sprite
        /// </summary>
        public void AddTexture(Texture2D Texture, int Position)
        {
            if (Position > textures.Count - 1)
                this.textures.Add(Texture);
            else
                this.textures.Insert(Position, Texture);
        }

        /// <summary>
        /// Updates the Sprite. In case of just one texture on sprite, it's not needed to call this method
        /// </summary>
        /// <param name="Time">Game Time to calculate the ellapsed time</param>
        public virtual void Update(GameTime Time)
        {
            if (frameRate > 0)
            {
                ellapsedTime += Time.ElapsedGameTime.Milliseconds;
                if (ellapsedTime > frameRate)
                {
                    ellapsedTime -= frameRate;
                    currentIndex++;
                    if (currentIndex >= textures.Count)
                        currentIndex = 0;
                }
            }
        }

        /// <summary>
        /// Draws the Sprite on screen
        /// </summary>
        /// <param name="spriteBatch">Sprite Batch game Object</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(textures[currentIndex], this.positon, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None,layer);
        }

        #endregion
    }

    /// <summary>
    /// Animates the Sprite just once
    /// </summary>
    public class PipelineSprite : Sprite
    {
        #region Constructors

        /// <summary>
        /// Constructs a new Pipeline Sprite
        /// </summary>
        /// <param name="Textures">Textures list</param>
        /// <param name="X">X position on Map</param>
        /// <param name="Y">Y position on Map</param>
        /// <param name="FrameRate">Rate to perform sprite animation</param>
        /// <param name="LayerDepth">Layer Depth to use on Draw Method</param>
        public PipelineSprite(List<Texture2D> Textures, int X, int Y, float FrameRate, float LayerDepth)
            :base(Textures,new Vector2(X,Y),FrameRate,LayerDepth)
        {

        }

        /// <summary>
        /// Constructs a new Sprite
        /// </summary>
        /// <param name="Textures">Textures list</param>
        /// <param name="Position">Vector position on map</param>
        /// <param name="FrameRate">Rate to perform sprite animation</param>
        /// <param name="LayerDepth">Layer Depth to use on Draw Method</param>
        public PipelineSprite(List<Texture2D> Textures, Vector2 Position, float FrameRate, float LayerDepth)
            :base(Textures,Position,FrameRate,LayerDepth)
        {
        }


        #endregion

        #region Public Properties

        /// <summary>
        /// Gets and Sets the current index
        /// </summary>
        public int CurrentIndex
        {
            get { return this.currentIndex; }
            set { this.currentIndex = value; ellapsedTime = 0; }
        }

        #endregion

        #region Public Methods

        public override void Update(GameTime Time)
        {
            if (currentIndex < textures.Count - 1)
            {
                if (frameRate > 0)
                {
                    ellapsedTime += Time.ElapsedGameTime.Milliseconds;
                    if (ellapsedTime > frameRate)
                    {
                        ellapsedTime -= frameRate;
                        currentIndex++;
                    }
                }
            }
            else
                ellapsedTime = Time.ElapsedGameTime.Milliseconds;
        }

        #endregion
    }
}
