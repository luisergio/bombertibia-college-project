using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BUS
{
    public class StatusBar : MapComponent
    {
        #region Private Variables

        Dictionary<AddOnType, Sprite> lstSprites;
        Dictionary<AddOnType, int> lstQuantity;
        string strStatus = "Level 1: Easy";

        private SpriteFont font;
        private SpriteFont font12;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the Addons Quantity
        /// </summary>
        public Dictionary<AddOnType, int> Quantity
        {
            get
            {
                return this.lstQuantity;
            }
        }

        /// <summary>
        /// Sets the Status
        /// </summary>
        public string Status
        {
            set { this.strStatus = value; }
        }

        #endregion

        #region Constructor

        public StatusBar(Map map, ContentManager content) : base(map)
        {
            font = content.Load<SpriteFont>("Matura");
            font12 = content.Load<SpriteFont>("Kootenay");
            lstSprites = new Dictionary<AddOnType, Sprite>();
            lstSprites.Add(AddOnType.BombQuantity, new Sprite(content.Load<Texture2D>(@"Images\AddOn\BombQuantity"), new Vector2(152, 512), 0.5f));
            lstSprites.Add(AddOnType.BombSize, new Sprite(content.Load<Texture2D>(@"Images\AddOn\BombSize"), new Vector2(216, 512), 0.5f));
            lstSprites.Add(AddOnType.Speed, new Sprite(content.Load<Texture2D>(@"Images\AddOn\Speed"), new Vector2(280, 512), 0.5f));
            lstSprites.Add(AddOnType.FullBombSize, new Sprite(content.Load<Texture2D>(@"Images\AddOn\FullBombSize"), new Vector2(344, 512), 0.5f));
            

            lstQuantity = new Dictionary<AddOnType, int>();
        }

        #endregion

        #region Public Methods

        public override void Update(GameTime Time)
        {
            
        }

        public override void Draw(SpriteBatch SpriteBatch)
        {
            SpriteBatch.DrawString(font, strStatus, new Vector2( (576 - strStatus.Length * 12) / 2, 0), Color.DarkRed, 0, Vector2.Zero, 1, SpriteEffects.None, 1);

            SpriteBatch.DrawString(font12, lstQuantity[AddOnType.BombQuantity].ToString(), new Vector2(184, 517), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            SpriteBatch.DrawString(font12, lstQuantity[AddOnType.BombSize].ToString(), new Vector2(248, 517), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            SpriteBatch.DrawString(font12, lstQuantity[AddOnType.Speed] > 0 ? "Yes" : "No", new Vector2(312, 517), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            SpriteBatch.DrawString(font12, lstQuantity[AddOnType.FullBombSize] > 0 ? "Yes" : "No", new Vector2(376, 517), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            

            foreach (Sprite spr in lstSprites.Values)
                spr.Draw(SpriteBatch);
        }

        #endregion
    }
}