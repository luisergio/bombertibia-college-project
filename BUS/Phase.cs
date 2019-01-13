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
using System.Threading;

namespace BUS
{
    /// <summary>
    /// This is a Game Phase
    /// </summary>
    public class Phase : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Private Variables

        private Dictionary<AddOnType,int> PhaseAddOns;
        private Player objPlayer;
        private Machine objMachine1;
        private Machine objMachine2;
        private Machine objMachine3;
        private Sprite sptMenu;
        private Dictionary<MenuOption, Texture2D> Imagens;
        private MenuOption slcMenu;
        private Map objMap;
        private Thread trdWait = new Thread(new ThreadStart(Utility.Sleep));
        private GameLevel CurrentGameLevel = GameLevel.Easy;
        private SoundEffect menuEffect;
        private SoundEffect inicialEffect;
        private bool IsWaiting = false;
        private StatusBar statusBar;

        #endregion

        #region Public Properties

        public Map Map
        {
            get { return this.objMap; }
        }

        public Dictionary<AddOnType,int> AddOns
        {
            get { return this.PhaseAddOns; }
        }

        #endregion

        #region Constructor

        public Phase(Game game): base(game)
        {
            Imagens = new Dictionary<MenuOption, Texture2D>();
            Imagens.Add(MenuOption.Start, this.GetTexture("Menu-Start"));
            Imagens.Add(MenuOption.About, this.GetTexture("Menu-About"));
            Imagens.Add(MenuOption.AboutPage, this.GetTexture("Menu-About-Page"));
            Imagens.Add(MenuOption.Close, this.GetTexture("Menu-Close"));
            Imagens.Add(MenuOption.Retry, this.GetTexture("Menu-Retry"));
            Imagens.Add(MenuOption.Cancel, this.GetTexture("Menu-Cancel"));
            Imagens.Add(MenuOption.YouWin, this.GetTexture("Menu-You-Win"));

            this.menuEffect = this.Game.Content.Load<SoundEffect>("Sounds\\menu");
            this.inicialEffect = this.Game.Content.Load<SoundEffect>("Sounds\\temple");
        }

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            ChangeMenuOption(MenuOption.Start, false);
            //inicialEffect.Play();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = new SpriteBatch(this.Game.GraphicsDevice);
            
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend,SpriteSortMode.FrontToBack,SaveStateMode.None);

            if (objMap != null)
                objMap.Draw(spriteBatch);

            if (sptMenu != null)
            {
                sptMenu.Draw(spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            try
            {
                if (objMap != null)
                {
                    objMap.Update(gameTime);
                    if (objMap != null)
                    {
                        if (!objMap.PlayerIsAlive() && !IsWaiting)
                        {
                            Utility.ExecuteMethod(this, "ChangeMenuOption", 2500, MenuOption.Retry, false);
                            IsWaiting = true;
                        }

                        if (!objMap.MachinesIsAlive() && !IsWaiting)
                        {
                            switch (CurrentGameLevel)
                            {
                                case GameLevel.Easy:
                                    CurrentGameLevel = GameLevel.Medium;
                                    Utility.ExecuteMethod(this, "CreateLevel", 2500, Game);
                                    IsWaiting = true;
                                    break;
                                case GameLevel.Medium:
                                    CurrentGameLevel = GameLevel.Hard;
                                    Utility.ExecuteMethod(this, "CreateLevel", 2500, Game);
                                    IsWaiting = true;
                                    break;
                                case GameLevel.Hard:
                                    CurrentGameLevel = GameLevel.Expert;
                                    Utility.ExecuteMethod(this, "CreateLevel", 2500, Game);
                                    IsWaiting = true;
                                    break;
                                case GameLevel.Expert:
                                    Utility.ExecuteMethod(this, "ChangeMenuOption", 2500, MenuOption.YouWin, false);
                                    IsWaiting = true;
                                    break;
                            }
                        }
                    }
                }
                else if (sptMenu != null)
                {

                    objMap = null;
                    sptMenu.Update(gameTime);

                    string KeyPressed = Utility.KeyPressed(new Keys[] { Keys.Up, Keys.Down, Keys.Enter, Keys.Right, Keys.Left });

                    switch (KeyPressed)
                    {
                        case "Up":
                            switch (slcMenu)
                            {
                                case MenuOption.About: this.ChangeMenuOption(MenuOption.Start, true); break;
                                case MenuOption.Close: this.ChangeMenuOption(MenuOption.About, true); break;
                                case MenuOption.Start: this.ChangeMenuOption(MenuOption.Close, true); break;
                            }
                            break;
                        case "Down":
                            switch (slcMenu)
                            {
                                case MenuOption.About: this.ChangeMenuOption(MenuOption.Close, true); break;
                                case MenuOption.Start: this.ChangeMenuOption(MenuOption.About, true); break;
                                case MenuOption.Close: this.ChangeMenuOption(MenuOption.Start, true); break;
                            }
                            break;
                        case "Left":
                            switch (slcMenu)
                            {
                                case MenuOption.Retry: this.ChangeMenuOption(MenuOption.Cancel, true); break;
                                case MenuOption.Cancel: this.ChangeMenuOption(MenuOption.Retry, true); break;
                            }
                            break;
                        case "Right":
                            switch (slcMenu)
                            {
                                case MenuOption.Retry: this.ChangeMenuOption(MenuOption.Cancel, true); break;
                                case MenuOption.Cancel: this.ChangeMenuOption(MenuOption.Retry, true); break;
                            }
                            break;
                        case "Enter":
                            switch (slcMenu)
                            {
                                case MenuOption.Start:
                                    if (Utility.CanI(this.trdWait, out this.trdWait))
                                    {
                                        sptMenu = null;
                                        this.CurrentGameLevel = GameLevel.Easy;
                                        CreateLevel(Game);
                                    }
                                    break;

                                case MenuOption.About:
                                    this.ChangeMenuOption(MenuOption.AboutPage, true);
                                    break;

                                case MenuOption.Close:
                                    Game.Exit();
                                    break;

                                case MenuOption.Retry:
                                    sptMenu = null;
                                    CreateLevel(Game);
                                    break;

                                case MenuOption.Cancel:
                                    this.ChangeMenuOption(MenuOption.Start, true);
                                    break;

                                case MenuOption.YouWin:
                                    this.ChangeMenuOption(MenuOption.Start, true);
                                    break;

                                case MenuOption.AboutPage:
                                    this.ChangeMenuOption(MenuOption.About, true);
                                    break;
                            }
                            break;
                    }
                }

                base.Update(gameTime);
            }
            catch { }
        }

        public void ChangeMenuOption(MenuOption newOption, bool withSoundEffect)
        {
            IsWaiting = false;
            if (Utility.CanI(trdWait, out trdWait))
            {
                this.TerminateGame();
                this.slcMenu = newOption;
                this.sptMenu = new Sprite(Imagens[this.slcMenu], new Vector2(0f, 0f), 0f);
                if (withSoundEffect) this.menuEffect.Play();
            }
        }

        public void CreateLevel(Game game)
        {
            this.TerminateGame();
            IsWaiting = false;
            //CurrentGameLevel = (GameLevel)(((int)CurrentGameLevel)!= 4?((int)CurrentGameLevel)+1:4);
            PhaseAddOns = new Dictionary<AddOnType, int>();

            switch (CurrentGameLevel)
            {
                case GameLevel.Easy:
                    objMap = new Map(this, game, "Map1.xml");
                    PhaseAddOns.Add(AddOnType.BombSize, 10);
                    PhaseAddOns.Add(AddOnType.FullBombSize, 5);
                    PhaseAddOns.Add(AddOnType.Speed, 10);
                    PhaseAddOns.Add(AddOnType.BombQuantity, 15);

                    objMachine1 = new Machine(Char.Orc, this, 300, 2, 1);

                    objMap.StatusBar = new StatusBar(objMap, game.Content) { Status = "Level 1: Easy" };

                    break;

                case GameLevel.Medium:
                    objMap = new Map(this, game, "Map3.xml");
                    PhaseAddOns.Add(AddOnType.BombSize, 5);
                    PhaseAddOns.Add(AddOnType.FullBombSize, 5);
                    PhaseAddOns.Add(AddOnType.Speed, 5);
                    PhaseAddOns.Add(AddOnType.BombQuantity, 10);

                    objMachine1 = new Machine(Char.Orc, this, 300, 2, 1);
                    objMachine2 = new Machine(Char.Orc, this, 300, 2, 1);

                    objMap.StatusBar = new StatusBar(objMap, game.Content) { Status = "Level 2: Medium" };

                    break;

                case GameLevel.Hard:
                    objMap = new Map(this, game, "Map2.xml");
                    PhaseAddOns.Add(AddOnType.BombSize, 5);
                    PhaseAddOns.Add(AddOnType.FullBombSize, 3);
                    PhaseAddOns.Add(AddOnType.Speed, 10);
                    PhaseAddOns.Add(AddOnType.BombQuantity, 5);

                    objMachine1 = new Machine(Char.Orc, this, 300, 2, 1);
                    objMachine2 = new Machine(Char.Orc, this, 300, 2, 1);
                    objMachine3 = new Machine(Char.Orc, this, 300, 2, 1);

                    objMap.StatusBar = new StatusBar(objMap, game.Content) { Status = "Level 3: Hard" };
                    break;

                case GameLevel.Expert:
                    objMap = new Map(this, game, "Map4.xml");
                    PhaseAddOns.Add(AddOnType.BombSize, 3);
                    PhaseAddOns.Add(AddOnType.FullBombSize, 1);
                    PhaseAddOns.Add(AddOnType.Speed, 3);
                    PhaseAddOns.Add(AddOnType.BombQuantity, 3);

                    objMachine1 = new Machine(Char.Orc, this, 300, 2, 1);
                    objMachine2 = new Machine(Char.Orc, this, 300, 2, 1);
                    objMachine3 = new Machine(Char.Orc, this, 300, 2, 1);

                    objMap.StatusBar = new StatusBar(objMap, game.Content) { Status = "Level 4: Expert" };

                    break;
            }

            objMap.StatusBar.Quantity.Add(AddOnType.BombSize, 0);
            objMap.StatusBar.Quantity.Add(AddOnType.FullBombSize, 0);
            objMap.StatusBar.Quantity.Add(AddOnType.Speed, 0);
            objMap.StatusBar.Quantity.Add(AddOnType.BombQuantity, 2);

            objPlayer = new Player(Char.Warrior, this, 300, 2, 1);
            objMap.AddMapComponent(objPlayer);

            objMap.AddMapComponent(objMachine1);
            if (objMachine2 != null) objMap.AddMapComponent(objMachine2);
            if (objMachine3 != null) objMap.AddMapComponent(objMachine3);

            objMap.Initialize();
            objMap.LoadContent(Game.Content);
        }

        #endregion

        #region Private Methods

        private void TerminateGame()
        {
            if (objMachine1 != null) this.objMachine1.Terminate();
            if (objMachine2 != null) this.objMachine2.Terminate();
            if (objMachine3 != null) this.objMachine3.Terminate();

            this.objPlayer = null;
            this.objMachine1 = null;
            this.objMachine2 = null;
            this.objMachine3 = null;
            this.objMap = null;
        }

        private Texture2D GetTexture(string ImageName)
        {
            return this.Game.Content.Load<Texture2D>("Images\\Menu\\" + ImageName);
        }

        #endregion
    }

    public enum MenuOption
    {
        Start,
        About,
        Close,
        Retry,
        Cancel,
        AboutPage,
        YouWin
    }

    public enum GameLevel
    { 
        Easy = 1,
        Medium = 2,
        Hard = 3,
        Expert = 4
    }
}