using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BUS
{
    public class Obstacle : MapComponent
    {
        #region Private Variables

        private bool OnFire;
        private AddOn addOn;
        private Phase objPhase;
        private List<Texture2D> AnimationImages, FinalizationImages;
        private float LayerDepth;
        private Vector2 Position;
        private float elepsedTime;

        #endregion

        #region Public Properties

        #endregion

        #region Constructor

        public Obstacle(Phase phase, Texture2D image, Vector2 position, float layerDepth, AddOn AddOn, List<Texture2D> AniImages):base(phase.Map)
        {
            this.objPhase = phase;

            Sprite obsSprite = new Sprite(image, position, layerDepth);
            obsSprite.IsWalkable = false;
            this.sprites.Add(obsSprite);

            this.addOn = AddOn;

            this.AnimationImages = AniImages;
            //this.FinalizationImages = FinImages;
            this.Position = position;
            this.LayerDepth = layerDepth;

            this.OnFire = false;
            this.elepsedTime = 0f;
        }

        #endregion

        #region Public Overrided Methods

        public override void Draw(SpriteBatch SpriteBatch)
        {
            foreach (Sprite obstacle in this.sprites)
                obstacle.Draw(SpriteBatch);
        }

        public override void Update(GameTime Time)
        {
            foreach (Sprite obstacle in this.sprites)
                obstacle.Update(Time);

            if (this.OnFire)
            {
                elepsedTime += Time.ElapsedGameTime.Milliseconds;
                if (elepsedTime >= Bomb.ExplodeDuration)
                {
                    this.objPhase.Map.RemoveMapComponent(this);
                }
            }
        }

        public override void FireReached()
        {
            if (!OnFire)
            {
                this.sprites.Remove(this.sprites[0]);

                Sprite objsprite = new Sprite(this.AnimationImages, this.Position, 150f, this.LayerDepth);
                objsprite.IsWalkable = false;
                this.sprites.Add(objsprite);
                this.OnFire = true;

                if (this.addOn != null)
                {
                    this.map.AddMapComponent(addOn);
                }
            }
        }

        #endregion

        #region Public Methods
        #endregion
    }
}
