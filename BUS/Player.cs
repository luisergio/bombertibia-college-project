using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Collections;
using System.Xml;

namespace BUS
{
    public enum Char
    {
        Warrior,
        Orc
    }

    /// <summary>
    /// Represents a Game Player
    /// </summary>
    public class Player : MapComponent
    {
        protected const string XmlsPath = @"..\..\..\..\BUS\Char\";

        #region Private Variables

        //Walk Variables
        protected float playerSpeed = 250;
        protected float walkPerFrame,walkedTime;
        protected int WalkInt;
        protected Dictionary<Directions, PipelineSprite> PipeSprites;
        protected Dictionary<Directions, Vector2> directions;
        protected Sprite currentSprite;
        protected Vector2 position, direction;
        protected Vector2 realPosition;

        //Die
        protected Sprite dieSprite;
        
        //Bomb Variables
        protected List<Bomb> bombs;
        protected float bombEllapsed, bombPerFrame = 50;
        protected int bombQuantity;
        protected int bombSize;
        
        //General Variables
        protected Phase phase;
        protected ContentManager contentManager;
        protected Char charName;
        protected bool isDead = false;
        protected bool isFlamming = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets and Sets the Player bomb size
        /// </summary>
        public int BombSize
        {
            get { return this.bombSize; }
            set { this.bombSize = value; }
        }
        
        /// <summary>
        /// Gets the Player Bombs Collection
        /// </summary>
        public List<Bomb> Bombs
        {
            get { return this.bombs; }
        }

        /// <summary>
        /// Gets the Player Phase
        /// </summary>
        public Phase Phase
        {
            get { return this.phase; }
        }

        /// <summary>
        /// Gets and Sets how many bombs the player can drop at the same time
        /// </summary>
        public int BombQuantity
        {
            get { return this.bombQuantity; }
            set { this.bombQuantity = value; }
        }

        /// <summary>
        /// Gets and Sets the player speed
        /// </summary>
        public float Speed
        {
            get { return this.playerSpeed; }
            set {
                walkPerFrame = value / 8;
                this.playerSpeed = value; 
            }
        }

        /// <summary>
        /// Getst the player current Direction
        /// </summary>
        public Directions Direction
        {
            get
            {
                if(direction.X == 0 && direction.Y == 0)
                    return Directions.Stopped;

                if (this.direction.X > 0)
                    return Directions.Right;
                else if (this.direction.X < 0)
                    return Directions.Left;
                else if (this.direction.Y > 0)
                    return Directions.Down;
                else
                    return Directions.Up;
            }
        }

        /// <summary>
        /// Get Position
        /// </summary>
        public Vector2 Position
        {
            get { return this.position; }
        }

        /// <summary>
        /// Gets the real position
        /// </summary>
        public Vector2 RealPosition
        {
            get { return realPosition; }
        }

        /// <summary>
        /// Checks if the player is dead
        /// </summary>
        public bool IsDead
        {
            get { return this.isDead; }
        }

        #endregion

        #region Contructor

        /// <summary>
        /// Constructs a new Player
        /// </summary>
        public Player(Char CharName, Phase Phase,float PlayerSpeed, int BombQuantity,int BombSize) : base(Phase.Map)
        {
            this.charName = CharName;
            this.playerSpeed = PlayerSpeed;
            this.bombQuantity = BombQuantity;
            this.bombSize = BombSize;
            this.phase = Phase;
        }

        #endregion

        #region Public Mehods

        /// <summary>
        /// 
        /// </summary>
        public void Initialize(Vector2 StartPosition)
        {
            PipeSprites = new Dictionary<Directions, PipelineSprite>();
            directions = new Dictionary<Directions, Vector2>();
            bombs = new List<Bomb>();

            position = StartPosition;
            realPosition = StartPosition;
            walkPerFrame = playerSpeed / 8;
        }

        /// <summary>
        /// Loads Sprite Contents
        /// </summary>
        public void LoadContent(ContentManager contentManager)
        {
            this.contentManager = contentManager;
            
            //Movement Dictionaries
            LoadXML();

            directions = Utility.GetDirections();
            PipeSprites[Directions.Down].CurrentIndex = 2;
            currentSprite = PipeSprites[Directions.Down];

            this.sprites.Add(currentSprite);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (!isDead)
            {
                #region Walk Routine

                //New Movement
                if (WalkInt == 0)
                {
                    if (isFlamming || isDead)
                    {
                        this.isDead = true;
                        currentSprite = dieSprite;
                    }
                    else
                    {
                        string keyPressed = Utility.KeyPressed(new Keys[4] { Keys.Up, Keys.Down, Keys.Right, Keys.Left });
                        if (!string.IsNullOrEmpty(keyPressed))
                            Move((Directions)Enum.Parse(typeof(Directions), keyPressed));
                    }
                }

                //Current Movement
                walkedTime += gameTime.ElapsedGameTime.Milliseconds;
                if (walkedTime > walkPerFrame && WalkInt > 0)
                {
                    walkedTime -= walkPerFrame;
                    this.position += direction;
                    WalkInt--;
                }

                try
                {
                    currentSprite.Position = position;
                    currentSprite.Update(gameTime);
                }
                catch { }

                #endregion

                #region Bomb Routine

                bombEllapsed += gameTime.ElapsedGameTime.Milliseconds;
                if (bombEllapsed > bombPerFrame)
                {
                    bombEllapsed = 0;
                    if (Keyboard.GetState().IsKeyDown(Keys.Space) && bombs.Count < bombQuantity)
                    {
                        Vector2 bomPosition = Utility.GetVecInSqm(this.position, this.Direction);
                        this.AddBomb(bomPosition);
                    }
                }

                #endregion

                //this.sprites.Clear();
                try
                {
                    this.sprites[0] = currentSprite;
                }
                catch { }
            }
        }

        /// <summary>
        /// Draws Sprites
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            try
            {
                currentSprite.Draw(spriteBatch);
            }
            catch { }
        }

        /// <summary>
        /// Kills this Player
        /// </summary>
        public void Kill()
        {
            this.isFlamming = true;
        }
        
        #endregion

        #region Private Methods

        protected void AddBomb(Vector2 bomPosition)
        {
            Bomb newBomb = new Bomb(bomPosition, this.contentManager, this);
            if (phase.Map.AddMapComponent(newBomb))
                bombs.Add(newBomb);
        }

        /// <summary>
        /// Loads all information needed from XML
        /// </summary>
        protected void LoadXML()
        {
            XmlDocument xmlConfiguration = new XmlDocument();
            xmlConfiguration.Load(XmlsPath + charName.ToString() + ".xml");

            XmlNode charNode = xmlConfiguration.GetElementsByTagName("Char")[0];
            string imagesPath = charNode.Attributes["ImagesPath"].Value;

            XmlNodeList movImagesNodes = charNode.ChildNodes[0].ChildNodes[0].ChildNodes;
            Directions enDirection;
            foreach (XmlNode movImageNode in movImagesNodes)
            {
                enDirection = (Directions)Enum.Parse(typeof(Directions), movImageNode.Attributes["Direction"].Value);
                if (!PipeSprites.Keys.Contains<Directions>(enDirection))
                    PipeSprites.Add(enDirection, new PipelineSprite(new List<Texture2D>(), this.position, this.playerSpeed, 0.5f));
                Texture2D newTexture = contentManager.Load<Texture2D>(imagesPath + movImageNode.Attributes["Name"].Value);
                PipeSprites[enDirection].AddTexture(newTexture, int.Parse(movImageNode.Attributes["Step"].Value));
            }

            XmlNodeList dieImagesNodes = charNode.ChildNodes[0].ChildNodes[1].ChildNodes;
            dieSprite = new PipelineSprite(new List<Texture2D>(), this.position, 600, 0.5f);
            foreach (XmlNode dieImageNode in dieImagesNodes)
            {
                dieSprite.AddTexture(contentManager.Load<Texture2D>(imagesPath + dieImageNode.Attributes["Name"].Value)
                    , int.Parse(dieImageNode.Attributes["Step"].Value));
            }
        }

        /// <summary>
        /// Moves the Player to a Specific Position
        /// </summary>
        /// <param name="Direction">Direction to move the player</param>
        protected void Move(Directions Direction)
        {
            try
            {
                Vector2 newPosition = this.position + Utility.Sqm * directions[Direction];
                if (this.map.IsWalkable(newPosition))
                {
                    bool notMove = false;
                    foreach (MapComponent mapComponent in this.map.GetComponents(newPosition))
                    {
                        if (!notMove && mapComponent[newPosition] != null)
                            notMove = !mapComponent[newPosition].IsWalkable;

                        switch (mapComponent.GetType().ToString().Split('.')[1])
                        {
                            case "Bomb":
                                if (((Bomb)mapComponent).IsExploding)
                                    isFlamming = true;
                                break;
                            case "AddOn":
                                ((AddOn)mapComponent).ApplyInPlayer(this);
                                break;
                        }
                    }

                    if (!notMove)
                    {
                        realPosition += (directions[Direction] * 32);
                        direction = 2 * directions[Direction];
                        position += direction;
                        PipeSprites[Direction].CurrentIndex = 0;

                        WalkInt = 15;
                        walkedTime = 0;
                    }
                    else
                        PipeSprites[Direction].CurrentIndex = 2;
                }
                else
                    PipeSprites[Direction].CurrentIndex = 2;

                PipeSprites[Direction].Position = position;
                this.currentSprite = PipeSprites[Direction];
            }
            catch { }
        }

        /// <summary>
        /// Gets a texture based in it's name
        /// </summary>
        /// <param name="CharNumber">Texture name</param>
        /// <returns></returns>
        protected Texture2D GetTexture(int CharNumber)
        {
            return contentManager.Load<Texture2D>("Images\\Char\\" + CharNumber);
        }

        #endregion
    }
}