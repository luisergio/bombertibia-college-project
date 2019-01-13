using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using Microsoft.Xna.Framework;
using System.Collections;

namespace BUS
{
    public delegate void DelegateNextMove(params Directions[] d);

    public class Machine : Player
    {
        #region Variables

        Thread IAThread;
        Queue<Directions> NextMove;
        private Player Target;
        private bool WaitBomb;
        private bool Stop;

        private Graph auxGraph;

        public DelegateNextMove m_DelegateNextMove;

        #endregion

        #region Contructors

        public Machine(Char CharName, Phase Phase, float PlayerSpeed, int BombQuantity, int BombSize)
            : base(CharName,Phase,PlayerSpeed,BombQuantity,BombSize)
        {
            Stop = false;
            NextMove = new Queue<Directions>();
            IAThread = new Thread(new ThreadStart(this.IABestWay));
        }

        #endregion

        #region Public Overrided Method

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (!isDead && !Stop)
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
                        string keyPressed = GetMovement();
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

                currentSprite.Position = position;
                currentSprite.Update(gameTime);

                #endregion
            }
        }

        private string GetMovement()
        {
            if (this.map.isDangerPosition(this.position))
            {
                if (!IAThread.IsAlive)
                {
                    IAThread = new Thread(new ThreadStart(this.IAEscape));

                    this.auxGraph = map.Graph.Clone();
                    IAThread.Start();
                }

                if (NextMove.Count > 0)
                    return Walk(NextMove.Dequeue());
            }

            if (this.WaitBomb)
                if (this.bombs.Count == 0)
                    this.WaitBomb = false;
                else
                    return "";

            if (!this.map.CanEscape(this.position))
                return "";

            //Busca player vivo mais próximo
            List<Player> lstOpon = new List<Player>();

            foreach (Player p in map.Players)
            {
                if (p.GetType().ToString().Split('.')[1] != "Machine" && !p.IsDead)
                    lstOpon.Add(p);
            }

            //Se não existir oponente
            if (lstOpon.Count() == 0)
                return "";

            float distancia = float.MaxValue;
            float Xdif = float.MaxValue;
            float Ydif = float.MaxValue;

            foreach (Player objPly in lstOpon)
            {
                //if (!map.isDangerPosition(objPly.Position))
                //{
                    if (objPly.Position.X > this.Position.X)
                        Xdif = objPly.Position.X - this.position.X;
                    else
                        Xdif = objPly.Position.X - this.position.X;

                    if (objPly.Position.Y > this.Position.Y)
                        Ydif = objPly.Position.Y - this.position.Y;
                    else
                        Ydif = objPly.Position.Y - this.position.Y;

                    if (distancia > Xdif + Ydif)
                    {
                        distancia = Xdif + Ydif;
                        Target = objPly;
                    }
                //}
            }

            if (Target == null) return "";

            if (Xdif < 0) Xdif = Xdif * -1;
            if (Ydif < 0) Ydif = Ydif * -1;

            if ((Xdif <= this.bombSize * 32 && Ydif == 0) || (Ydif <= this.bombSize * 32 && Xdif == 0))
            {
                if (this.map.WillHitTarget(this.position, Target.Position, this.BombSize))
                    if (this.map.CanEscape(this.position, this.position, this.bombSize))
                        this.AddBomb(this.position);
            }
            else
                if (Target.Position != this.Position)
                {
                    if (map.isDangerPosition(Target.Position))
                        return "";

                    if (!IAThread.IsAlive)
                    {
                        IAThread = new Thread(new ThreadStart(this.IABestWay));

                        this.auxGraph = map.Graph.Clone();
                        IAThread.Start();
                    }

                    if(NextMove.Count>0)
                        return Walk(NextMove.Dequeue());
                }

            return "";

        }

        private string Walk(Directions direct)
        {
            if (direct != Directions.Stopped)
            {
                Vector2 newPosition = this.position + Utility.GetDirections()[direct] * 32;
                
                bool notMove = false;

                if (this.map.IsInFire(newPosition))
                    notMove = true;
                else
                foreach (MapComponent mapComponent in this.map.GetComponents(newPosition))
                {
                    try
                    {
                        if (!notMove)
                            notMove = !mapComponent[newPosition].IsWalkable;
                    }
                    catch { }
                    switch (mapComponent.GetType().ToString().Split('.')[1])
                    {
                        case "Obstacle":
                            if (this.map.CanEscape(this.position, this.position, this.bombSize))
                            {
                                this.WaitBomb = true;
                                this.AddBomb(this.position);
                            }
                            break;
                    }
                }

                if (notMove)
                    return "";//this.Stop = true;
                else
                    return direct.ToString();
            }
            return "";
        }

        private void IABestWay()
        {
            Directions direct = this.GetBestWay(this.position, Target.Position);

            typeof(Machine).GetMethod("AddMove").Invoke(this, new object[] { new Directions[] {direct} });
        }

        private void IAEscape()
        {
            if (this.position.X % 32 == 0 && this.position.Y % 32 == 0)
            {
                Directions direct = this.map.GetBestEscape(this.position);

                typeof(Machine).GetMethod("AddMove").Invoke(this, new object[] { new Directions[] { direct } });
            }
        }

        public void Terminate()
        {
            this.IAThread.Abort();
        }

        public void AddMove(params Directions[] directions)
        {
            foreach(Directions direct in directions)
                NextMove.Enqueue(direct);
        }

        public Directions GetBestWay(Vector2 From, Vector2 To)
        {
            if (From.X % 32 == 0 && From.Y % 32 == 0 && To.X % 32 == 0 && To.Y % 32 == 0)
            {
                Vector2 cFrom = new Vector2(From.X, From.Y);
                Vector2 cTo = new Vector2(To.X, To.Y);

                return this.auxGraph.GetBestWay(cFrom, cTo);
            }
            else
                return Directions.Stopped;
        }
        
        #endregion
    }

}