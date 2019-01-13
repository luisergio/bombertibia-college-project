using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace BUS
{
    /// <summary>
    /// Types of Players Add Ons
    /// </summary>
    public enum AddOnType
    {
        Speed, 
        BombQuantity,
        FullBombSize,
        BombSize
    }

    public class AddOn : MapComponent
    {
        #region Private Variables

        private AddOnType type;
        private SoundEffect AddOnUsed;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="map"></param>
        public AddOn(Map map, AddOnType Type, Vector2 Position, ContentManager contentManager)
            : base(map)
        {
            this.type = Type;

            List<Texture2D> imgAddOn = new List<Texture2D>() { 
                contentManager.Load<Texture2D>(@"Images\AddOn\" + type.ToString())
                ,contentManager.Load<Texture2D>(@"Images\AddOn\" + type.ToString() + "2") 
                //,contentManager.Load<Texture2D>(@"Images\AddOn\" + type.ToString() + "3")
                //,contentManager.Load<Texture2D>(@"Images\AddOn\" + type.ToString() + "4")
                //,contentManager.Load<Texture2D>(@"Images\AddOn\" + type.ToString() + "3") 
                //,contentManager.Load<Texture2D>(@"Images\AddOn\" + type.ToString() + "2")
            };
            this.sprites.Add(new Sprite(imgAddOn, Position,150, 0.4f));

            this.AddOnUsed = contentManager.Load<SoundEffect>("Sounds\\addon");
        }

        #endregion

        #region Public Methods

        public override void FireReached()
        {
            this.map.RemoveMapComponent(this);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch SpriteBatch)
        {
            sprites[0].Draw(SpriteBatch);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime Time)
        {
            sprites[0].Update(Time);
        }

        /// <summary>
        /// Applies this AddOn to a Player
        /// </summary>
        /// <param name="player"></param>
        public void ApplyInPlayer(Player player)
        {
            switch (type)
            {
                case AddOnType.Speed:
                    player.Speed=150;
                    break;
                case AddOnType.BombSize:
                    player.BombSize++;
                    break;
                case AddOnType.FullBombSize:
                    player.BombSize = 17;
                    break;
                case AddOnType.BombQuantity:
                    player.BombQuantity++;
                    break;
            }

            if(!(player is Machine))
                map.StatusBar.Quantity[type]++;

            this.map.RemoveMapComponent(this);
            this.AddOnUsed.Play();
        }

        #endregion
    }
}
