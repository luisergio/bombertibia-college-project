using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace BUS
{
    /// <summary>
    /// Represents the Game Bomb
    /// </summary>
    public class Bomb : MapComponent
    {
        #region Private Variables

        private const float animRate = 150;
        private const float layerDepth = 0.4f;

        private Sprite sprBomb;
        private List<Sprite> fireSprites;
        private List<Texture2D> textures;
        private Player player;

        float explodeTime = 5000;
        private static float explodeDuration = 2000;
        private int explodeSize = 1;

        private bool isExploding = false;
        private bool isExploded = false;

        private SoundEffect mySoundEffect;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets how much time the bomb keep exploding
        /// </summary>
        public static float ExplodeDuration
        {
            get { return Bomb.explodeDuration; }
        }

        /// <summary>
        /// Checks if the Bomb is already exploded
        /// </summary>
        public bool IsExploded
        {
            get { return this.isExploded; }
        }

        /// <summary>
        /// Checks if the bomb is currently Exploding
        /// </summary>
        public bool IsExploding
        {
            get { return this.isExploding; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new Bomb
        /// </summary>
        /// <param name="Position">Position to add the Bomb on Map</param>
        /// <param name="Content">Game content property to load images</param>
        public Bomb(Vector2 Position, ContentManager Content, Player player):base(player.Map)
        {
            this.isStackable = false;
            this.player = player;
            this.explodeSize = player.BombSize; //Define when the bom is create or when exploded? :D

            textures = new List<Texture2D>();

            //Loading Bomb Images
            textures.Add(Content.Load<Texture2D>(@"Images\Bomb\largeBomb"));
            textures.Add(Content.Load<Texture2D>(@"Images\Bomb\mediumBomb"));
            textures.Add(Content.Load<Texture2D>(@"Images\Bomb\smallBomb"));
            textures.Add(Content.Load<Texture2D>(@"Images\Bomb\mediumBomb"));
            sprBomb = new Sprite(textures, Position, 100, 0.4f);
            sprBomb.IsWalkable = false;

            //Loading Fire Images
            textures = new List<Texture2D>();
            textures.Add(Content.Load<Texture2D>(@"Images\Bomb\494"));
            textures.Add(Content.Load<Texture2D>(@"Images\Bomb\495"));

            this.sprites.Add(sprBomb);

            this.mySoundEffect = Content.Load<SoundEffect>("Sounds\\bomb3");
        }

        #endregion

        #region Public Methods

        public override void Update(GameTime Time)
        {
            if (!isExploded)
            {
                if (!isExploding)
                {
                    this.sprBomb.Update(Time);
                    ellapsedTime += Time.ElapsedGameTime.Milliseconds;
                    if (ellapsedTime >= explodeTime)
                    {
                        LoadFires();
                        this.sprBomb.IsWalkable = true;
                        this.isExploding = true;
                        player.Bombs.Remove(this);
                        ellapsedTime = 0;

                        mySoundEffect.Play(0.2f);
                    }
                }
                else
                {
                    foreach (Sprite fire in fireSprites)
                        fire.Update(Time);
                    ellapsedTime += Time.ElapsedGameTime.Milliseconds;
                    if (ellapsedTime >= explodeDuration)
                    {
                        isExploded = true;
                        map.RemoveMapComponent(this);
                    }
                }
            }
        }

        public override void Draw(SpriteBatch SpriteBatch)
        {
            if (!isExploding)
                this.sprBomb.Draw(SpriteBatch);
            else if (!isExploded)
                foreach (Sprite fire in fireSprites)
                    fire.Draw(SpriteBatch);
        }

        /// <summary>
        /// Explodes the Bomb
        /// </summary>
        public void Explode()
        {
            ellapsedTime = explodeTime;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads the Fires Sprites
        /// </summary>
        private void LoadFires()
        {
            //Load Fire Spries
            fireSprites = new List<Sprite>();
            fireSprites.Add(new Sprite(textures, new Vector2(sprBomb.Position.X, sprBomb.Position.Y), 150, 0.4f));

            this.map.AddInFirePosition(sprBomb.Position);

            List<Vector2> directions = new List<Vector2>() { new Vector2(32, 0), new Vector2(-32, 0), new Vector2(0, 32), new Vector2(0, -32) };
            List<Vector2> removeDirections = new List<Vector2>();

            Vector2 firePos;
            for (int i = 1; i <= explodeSize; i++)
            {
                foreach (Vector2 direction in directions)
                {
                    firePos = sprBomb.Position + (i * direction);
                    if (map.IsWalkable(firePos))
                    {
                        bool stopFire = false;
                        foreach (MapComponent mapComponent in map.GetComponents(firePos))
                        {
                            switch (mapComponent.GetType().ToString().Split('.')[1])
                            {
                                case "Bomb":
                                    if (!((Bomb)mapComponent).isExploding)
                                        ((Bomb)mapComponent).Explode();
                                    break;
                                case "Obstacle":
                                    stopFire = true;
                                    mapComponent.FireReached();
                                    break;
                                case "Player":
                                    if(!((Player)mapComponent).IsDead)
                                    ((Player)mapComponent).Kill();
                                    break;
                                case "Machine":
                                    if (!((Machine)mapComponent).IsDead)
                                        ((Machine)mapComponent).Kill();
                                    break;
                                default:
                                    mapComponent.FireReached();
                                    break;
                            }
                        }

                        if (!stopFire)
                        {
                            fireSprites.Add(new Sprite(textures, firePos, animRate, layerDepth));
                            this.map.AddInFirePosition(firePos);
                        }
                        else
                            removeDirections.Add(direction);
                    }
                    else
                        removeDirections.Add(direction);
                }
                foreach (Vector2 direction in removeDirections)
                    directions.Remove(direction);
            }

            this.sprites.Remove(sprBomb);
            this.sprites.AddRange(fireSprites);
        }

        #endregion

        public static List<Vector2> WillExplodePositions(Map map, Bomb bomb)
        {
            return WillExplodePositions(map, bomb.Sprites[0].Position, bomb.explodeSize);
        }

        public static List<Vector2> WillExplodePositions(Map map, Vector2 InitialPosition, int ExplodeSize)
        {
            List<Vector2> WillExplode = new List<Vector2>();

            WillExplode.Add(InitialPosition);

            List<Vector2> directions = new List<Vector2>() { new Vector2(32, 0), new Vector2(-32, 0), new Vector2(0, 32), new Vector2(0, -32) };
            List<Vector2> removeDirections = new List<Vector2>();

            Vector2 firePos;

            for (int i = 1; i <= ExplodeSize; i++)
            {
                foreach (Vector2 direction in directions)
                {
                    firePos = InitialPosition + (i * direction);
                    if (map.IsWalkable(firePos))
                    {
                        bool stopFire = false;

                        foreach (MapComponent mapComponent in map.GetComponents(firePos))
                            if (mapComponent is Obstacle) stopFire = true;

                        if (!stopFire)
                            WillExplode.Add(firePos);
                        else
                            removeDirections.Add(direction);
                    }
                    else
                        removeDirections.Add(direction);
                }

                foreach (Vector2 direction in removeDirections)
                    directions.Remove(direction);
            }

            return WillExplode;
        }
    }
}
