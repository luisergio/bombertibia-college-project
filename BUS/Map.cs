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
using System.Reflection;

namespace BUS
{
    public class Map
    {
        #region Private Variables

        private List<MapComponent> lstMapComponent;
        private List<MapComponent> lstMapCompToAdd;
        private List<MapComponent> lstMapCompToDelete;
        private List<MapLevel> lstMapLevels;
        private List<Player> lstPlayer;
        private Dictionary<Vector2,int> dicInFirePositions;
        private string ObstacleImage;
        private Phase objPhase;
        private Game Game;
        private int intLines;
        private int intColumns;
        private string mapName;
        private Graph mapGraph;
        private StatusBar statusBar;

        #endregion

        #region Propertis

        public StatusBar StatusBar
        {
            get { return statusBar; }
            set { statusBar = value; }
        }

        public List<Player> Players
        {
            get { return lstPlayer; }
            set { lstPlayer = value; }
        }

        public Graph Graph
        {
            get { return this.mapGraph; }
        }

        #endregion

        #region Contructor

        public Map()
        { 
        }

        public Map(Phase phase, Game game, string MapName)
        {
            this.objPhase = phase;
            lstMapComponent = new List<MapComponent>();
            lstMapCompToDelete = new List<MapComponent>();
            lstMapCompToAdd = new List<MapComponent>();
            lstPlayer = new List<Player>();
            dicInFirePositions = new Dictionary<Vector2, int>();
            this.mapName = MapName;
            this.Game = game;
            // TODO: Construct any child components here
        }

        #endregion

        #region Public Mehods

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize()
        {
            List<Vector2> Positions = new List<Vector2>();
            Positions.Add(new Vector2(64, 64));
            Positions.Add(new Vector2(448, 448));
            Positions.Add(new Vector2(448, 64));
            Positions.Add(new Vector2(64, 448));

            foreach (MapComponent mapCom in this.lstMapComponent)
            {
                if (mapCom is Player)
                {
                    ((Player)mapCom).Initialize(Positions[0]);
                }

                Positions.Remove(Positions[0]);
            }

            foreach (MapComponent mapCom in this.lstMapCompToAdd)
            {
                if (mapCom is Player)
                {
                    ((Player)mapCom).Initialize(Positions[0]);
                }

                Positions.Remove(Positions[0]);
            }
        }

        /// <summary>
        /// Loads Sprite Contents
        /// </summary>
        public void LoadContent(ContentManager contentManager)
        {
            XmlDocument xmlMap = new XmlDocument();
            xmlMap.Load(@"..\..\..\..\BUS\Maps\" + this.mapName);

            this.LoadMap(xmlMap);
            this.LoadMapGraph();
            this.Obstacle(xmlMap);

            foreach (MapComponent mapCom in this.lstMapComponent)
                if (mapCom is Player)
                    ((Player)mapCom).LoadContent(contentManager);

            foreach (MapComponent mapCom in this.lstMapCompToAdd)
                if (mapCom is Player)
                    ((Player)mapCom).LoadContent(contentManager);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            foreach (MapComponent mapComp in lstMapCompToAdd)
                lstMapComponent.Add(mapComp);

            lstMapCompToAdd.Clear();

            foreach (MapComponent mapComp in lstMapCompToDelete)
                lstMapComponent.Remove(mapComp);

            lstMapCompToDelete.Clear();

            foreach (MapComponent mapCom in this.lstMapComponent)
                mapCom.Update(gameTime);
        }

        /// <summary>
        /// Draws Sprites
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (MapLevel mapLevel in this.lstMapLevels)
            {
                for (int i = 0; i < this.intLines; i++)
                {
                    for (int j = 0; j < this.intColumns; j++)
                    {
                        if (mapLevel.MapStruct[i, j] != null)
                            mapLevel.MapStruct[i, j].Draw(spriteBatch);
                    }
                }
            }

            statusBar.Draw(spriteBatch);

            foreach (MapComponent mapCom in this.lstMapComponent)
                mapCom.Draw(spriteBatch);
        }

        /// <summary>
        /// Gets Map Components in a specific Map Position
        /// </summary>
        /// <param name="Position">Position to check on map</param>
        /// <returns>A list of Map Components in the map position</returns>
        public List<MapComponent> GetComponents(Vector2 Position)
        {
            List<MapComponent> lstValidMapComponents = new List<MapComponent>();

            foreach (MapComponent mapComponent in this.lstMapComponent)
                if (mapComponent.IsInPosition(Position))
                    lstValidMapComponents.Add(mapComponent);

            return lstValidMapComponents;
        }

        public bool IsWalkable(Vector2 Position)
        {
            bool isWalkable = true;

            foreach (MapLevel mapLevel in this.lstMapLevels)
            {
                int i = int.Parse(Position.X.ToString()) / 32;
                int j = int.Parse(Position.Y.ToString()) / 32;
                if (mapLevel.MapStruct[i, j] != null)
                    if (!mapLevel.MapStruct[i, j].IsWalkable)
                    {
                        isWalkable = false;
                        break;
                    }
            }
            
            return isWalkable;
        }

        public bool IsWalkable(float X, float Y)
        {
            return IsWalkable(new Vector2(X,Y));
        }

        public bool AddMapComponent(MapComponent mapComponent)
        {
            if (!mapComponent.IsStackable)
            {
                Type compType = mapComponent.GetType();
                foreach (MapComponent mapComp in lstMapComponent)
                {
                    if (compType.Equals(mapComp.GetType()))
                        foreach (Sprite mapCompSprite in mapComp.Sprites) //Review it in case of bomb in fire (quick power)
                            foreach (Sprite mapComponentSprite in mapComponent.Sprites)
                                if (mapComponentSprite.Position.Equals(mapCompSprite.Position))
                                    return false;
                }
            }

            this.lstMapCompToAdd.Add(mapComponent);

            switch (mapComponent.GetType().ToString().Split('.')[1])
            {
                case "Bomb":
                    foreach (Vector2 pos in Bomb.WillExplodePositions(this,(Bomb)mapComponent))
                        this.mapGraph.UpdateWeight(pos, Graph.WillExplodeWeight);
                    break;

                case "Player":
                    this.Players.Add((Player)mapComponent);
                    break;

                case "Machine":
                    this.Players.Add((Player)mapComponent);
                    break;
            }

            return true;
        }

        public void UpdateWeight(Vector2 Position, float Weight)
        {
            this.mapGraph.UpdateWeight(Position, Weight);
        }

        public void RemoveMapComponent(MapComponent mapComponent)
        {
            lstMapCompToDelete.Add(mapComponent);

            switch (mapComponent.GetType().ToString().Split('.')[1])
            {
                case "Bomb":
                    foreach (Sprite spr in mapComponent.Sprites)
                    {
                        this.mapGraph.UpdateWeight(spr.Position, Graph.EmptyWeight);
                        this.RemoveInFirePosition(spr.Position);
                    }

                    break;

                case "Obstacle":
                    this.mapGraph.UpdateWeight(mapComponent.Sprites.First(p => 1 == 1).Position, Graph.EmptyWeight);
                    break;
            }
        }

        public Directions GetBestWay(Vector2 From, Vector2 To)
        {
            if (From.X % 32 == 0 && From.Y % 32 == 0 && To.X % 32 == 0 && To.Y % 32 == 0)
            {
                Vector2 cFrom = new Vector2(From.X, From.Y);
                Vector2 cTo = new Vector2(To.X, To.Y);

                return this.mapGraph.GetBestWay(cFrom, cTo);
            }
            else
                return Directions.Stopped;
        }

        public Directions GetBestEscape(Vector2 From)
        {
            return this.mapGraph.GetBestEscape(From);
        }

        public bool isDangerPosition(Vector2 Position)
        {
            foreach (MapComponent MapCom in this.lstMapComponent)
                if(MapCom is Bomb)
                    foreach(Vector2 pos in Bomb.WillExplodePositions(this,(Bomb)MapCom))
                        if(pos == Position) 
                            return true;

            return false;
        }

        public bool WillHitTarget(Vector2 BombPosition, Vector2 TargetPosition,int ExplodeSize)
        {
            foreach (Vector2 pos in Bomb.WillExplodePositions(this,BombPosition,ExplodeSize))
                if (pos == TargetPosition)
                    return true;

            return false;
        }

        public bool CanEscape(Vector2 FromPosition)
        {
            try
            {
                return this.mapGraph.WalkableEmptyPositions(FromPosition).Count > 0;
            }
            catch { return false; }
        }

        public bool CanEscape(Vector2 FromPosition, Vector2 NewBombPosition, int ExplodeSize)
        {
            List<Vector2> WillExplode = Bomb.WillExplodePositions(this, NewBombPosition, ExplodeSize);
            List<Vector2> WEP = this.mapGraph.WalkableEmptyPositions(FromPosition);
            return WEP.Count > WillExplode.Count;
        }

        public void AddInFirePosition(Vector2 Position)
        {
            if (this.dicInFirePositions.ContainsKey(Position))
                this.dicInFirePositions[Position] += 1;
            else
                this.dicInFirePositions.Add(Position, 1);
        }

        public void RemoveInFirePosition(Vector2 Position)
        {
            if (this.dicInFirePositions[Position] > 1)
                this.dicInFirePositions[Position] -= 1;
            else
                this.dicInFirePositions.Remove(Position);
        }

        public bool IsInFire(Vector2 Position)
        {
            return this.dicInFirePositions.ContainsKey(Position);
        }

        public bool PlayerIsAlive()
        {
            foreach (MapComponent objMapCom in this.lstMapComponent)
            {
                if (objMapCom.GetType().ToString().Split('.')[1] == "Player")
                    if (((Player)objMapCom).IsDead)
                        return false;
                    else
                        return true;
            }
            return false;
        }

        public bool MachinesIsAlive()
        {
            bool isAlive = false;
            foreach (MapComponent objMapCom in this.lstMapComponent)
            {
                if (objMapCom.GetType().ToString().Split('.')[1] == "Machine")
                    if (!((Machine)objMapCom).IsDead)
                        isAlive = true;
            }
            return isAlive;
        }

        /// <summary>
        /// Clone the object, and returning a reference to a cloned object.
        /// </summary>
        /// <returns>Reference to the new cloned object.</returns>
        public object Clone()
        {
            //First we create an instance of this specific type.
            object newObject = Activator.CreateInstance(this.GetType());

            //We get the array of fields for the new type instance.
            FieldInfo[] fields = newObject.GetType().GetFields();

            int i = 0;

            foreach (FieldInfo fi in this.GetType().GetFields())
            {
                //We query if the fiels support the ICloneable interface.
                Type ICloneType = fi.FieldType.GetInterface("ICloneable", true);

                if (ICloneType != null)
                {
                    //Getting the ICloneable interface from the object.
                    ICloneable IClone = (ICloneable)fi.GetValue(this);

                    //We use the clone method to set the new value to the field.
                    fields[i].SetValue(newObject, IClone.Clone());
                }
                else
                {
                    // If the field doesn't support the ICloneable 
                    // interface then just set it.
                    fields[i].SetValue(newObject, fi.GetValue(this));
                }

                //Now we check if the object support the 
                //IEnumerable interface, so if it does
                //we need to enumerate all its items and check if 
                //they support the ICloneable interface.
                Type IEnumerableType = fi.FieldType.GetInterface("IEnumerable", true);

                if (IEnumerableType != null)
                {
                    //Get the IEnumerable interface from the field.
                    IEnumerable IEnum = (IEnumerable)fi.GetValue(this);

                    //This version support the IList and the 
                    //IDictionary interfaces to iterate on collections.
                    Type IListType = fields[i].FieldType.GetInterface("IList", true);
                    Type IDicType = fields[i].FieldType.GetInterface("IDictionary", true);

                    int j = 0;
                    if (IListType != null)
                    {
                        //Getting the IList interface.
                        IList list = (IList)fields[i].GetValue(newObject);

                        foreach (object obj in IEnum)
                        {
                            //Checking to see if the current item 
                            //support the ICloneable interface.
                            ICloneType = obj.GetType().GetInterface("ICloneable", true);

                            if (ICloneType != null)
                            {
                                //If it does support the ICloneable interface,
                                //we use it to set the clone of
                                //the object in the list.
                                ICloneable clone = (ICloneable)obj;
                                list[j] = clone.Clone();
                            }

                            //NOTE: If the item in the list is not 
                            //support the ICloneable interface then in the 
                            //cloned list this item will be the same 
                            //item as in the original list
                            //(as long as this type is a reference type).
                            j++;
                        }
                    }
                    else if (IDicType != null)
                    {
                        //Getting the dictionary interface.
                        IDictionary dic = (IDictionary)fields[i].GetValue(newObject);
                        j = 0;

                        foreach (DictionaryEntry de in IEnum)
                        {
                            //Checking to see if the item 
                            //support the ICloneable interface.

                            ICloneType = de.Value.GetType().GetInterface("ICloneable", true);

                            if (ICloneType != null)
                            {
                                ICloneable clone = (ICloneable)de.Value;

                                dic[de.Key] = clone.Clone();
                            }
                            j++;
                        }
                    }
                }
                i++;
            }
            return newObject;
        }

        #endregion

        #region Private Methods

        private void LoadMap(XmlDocument xmlMap)
        {
            lstMapLevels = new List<MapLevel>();
            XmlNodeList xmlLevels = xmlMap.GetElementsByTagName("Level");

            foreach (XmlNode xmlLevel in xmlLevels)
            {

                MapLevel newMapLevel = new MapLevel();
                newMapLevel.Number = int.Parse(xmlLevel.Attributes["Number"].Value);
                newMapLevel.UpOfPlayer = bool.Parse(xmlLevel.Attributes["UpOfPlayer"].Value);
                newMapLevel.Walkable = bool.Parse(xmlLevel.Attributes["Walkable"].Value);

                string strLevel = xmlLevel.InnerText.Trim();

                string[] vetLines = strLevel.Split('\n');
                string[] vetColumns = vetLines[0].Trim().Split(';');

                this.intLines = vetLines.Length;
                this.intColumns = vetColumns.Length;

                Sprite[,] MapStruct = new Sprite[this.intColumns, this.intLines];

                for (int i = 0; i < vetLines.Length; i++)
                {
                    vetColumns = vetLines[i].Trim().Split(';');
                    for (int j = 0; j < vetColumns.Length; j++)
                        if (vetColumns[j].Trim() != "X")
                        {
                            int image = int.Parse(vetColumns[j].Trim().ToString());

                            float LayerDepth;

                            if (newMapLevel.UpOfPlayer)
                                LayerDepth = float.Parse("0,5" + newMapLevel.Number.ToString());
                            else
                                LayerDepth = float.Parse("0,3" + newMapLevel.Number.ToString());

                            Sprite spriteImage = new Sprite(GetTexture(image), j * 32, i * 32, LayerDepth);

                            if (!newMapLevel.Walkable)
                                spriteImage.IsWalkable = false;

                            MapStruct[i, j] = spriteImage;
                        }
                }

                newMapLevel.MapStruct = MapStruct;

                this.lstMapLevels.Add(newMapLevel);
            }
        }

        private void Obstacle(XmlDocument xmlMap)
        {
            XmlNodeList xmlObstacle = xmlMap.GetElementsByTagName("Obstacle");

            int ObstacleImage = int.Parse(xmlObstacle[0].Attributes["PrincipalImage"].Value);
            string[] AniImages = xmlObstacle[0].Attributes["AnimationImages"].Value.Split(';');
            string[] FinImages = xmlObstacle[0].Attributes["FinalizationImages"].Value.Split(';');
            int percent = int.Parse(xmlObstacle[0].Attributes["Percent"].Value);

            string strObstacle = xmlObstacle[0].InnerText.Trim();
            string[] vetLinesObs = strObstacle.Split('\n');
            string[] vetColumnsObs = vetLinesObs[0].Trim().Split(';');

            List<Vector2> possibleLocation = new List<Vector2>();

            for (int i = 0; i < vetLinesObs.Length; i++)
            {
                vetColumnsObs = vetLinesObs[i].Trim().Split(';');

                for (int j = 0; j < vetColumnsObs.Length; j++)
                    if (vetColumnsObs[j].Trim() == "X")
                        possibleLocation.Add(new Vector2(j * 32, i * 32));
            }


            int NumberOfObstacle = (int)Math.Round((percent / 100f) * possibleLocation.Count);

            int Obstacles = 0;

            Random random = new Random();

            while (Obstacles < NumberOfObstacle)
            {
                Obstacles++;
                int index = random.Next(0, possibleLocation.Count - 1);

                AddOn obtAddOn = null;
                if (objPhase.AddOns.Count > 0)
                {
                    KeyValuePair<AddOnType,int> addOn = objPhase.AddOns.ElementAt(0);
                    obtAddOn = new AddOn(this, addOn.Key, possibleLocation[index], this.Game.Content);
                    objPhase.AddOns.Remove(addOn.Key);
                    if (addOn.Value - 1 > 0)
                        objPhase.AddOns.Add(addOn.Key, addOn.Value - 1);
                }

                List<Texture2D> lstAniImages = new List<Texture2D>();
                foreach (string image in AniImages)
                    lstAniImages.Add(GetTexture(int.Parse(image)));

                //List<Texture2D> lstFinImages = new List<Texture2D>();
                //foreach (string image in FinImages)
                //    lstFinImages.Add(GetTexture(int.Parse(image)));

                Obstacle newObstacle = new Obstacle(this.objPhase, GetTexture(ObstacleImage), possibleLocation[index], 0.501f, obtAddOn, lstAniImages);//, lstFinImages);
                this.AddMapComponent(newObstacle);

                //Adiciona novo peso para as arestas que vão para o local desse obstáculo.
                this.mapGraph.UpdateWeight(possibleLocation[index].X, possibleLocation[index].Y, Graph.ObstacleWeight);
                possibleLocation.RemoveAt(index);
            }
        }

        private void LoadMapGraph()
        {
            this.mapGraph = new Graph();

            for (int i = 1; i <= 13; i++) //Linha
                for (int j = 1; j <= 13; j++) //Coluna
                    if (i % 2 != 0 || j % 2 != 0)
                        this.mapGraph.Vertexs.Add(new Vertex((i * 32) + 32, (j * 32) + 32));

            IEnumerable<Vertex> lstVertex = from vert in this.mapGraph.Vertexs
                                            where ((vert.Position.X - 32) / 32) % 2 != 0
                                            && ((vert.Position.Y - 32) / 32) % 2 != 0
                                            select vert;

            foreach (Vertex objV in lstVertex)
            { 
                float X = objV.Position.X;
                float Y = objV.Position.Y;
                IEnumerable<Vertex> neighborVertex = from vert in this.mapGraph.Vertexs
                                                     where
                                                     (vert.Position.X == X
                                                     && (vert.Position.Y == Y + 32
                                                        || vert.Position.Y == Y - 32)
                                                     )
                                                     || (vert.Position.Y == Y
                                                     && (vert.Position.X == X + 32
                                                        || vert.Position.X == X - 32)
                                                     )
                                                     select vert;

                foreach (Vertex neighborV in neighborVertex)
                { 
                    if(neighborV.Position.X == X)
                        if (neighborV.Position.Y == Y + 32)
                        {
                            this.mapGraph.Edges.Add(new Edge(objV, neighborV, Graph.EmptyWeight, Directions.Down));
                            this.mapGraph.Edges.Add(new Edge(neighborV, objV, Graph.EmptyWeight, Directions.Up));
                        }
                        else
                        {
                            this.mapGraph.Edges.Add(new Edge(objV, neighborV, Graph.EmptyWeight, Directions.Up));
                            this.mapGraph.Edges.Add(new Edge(neighborV, objV, Graph.EmptyWeight, Directions.Down));
                        }
                    else
                    if (neighborV.Position.Y == Y)
                        if (neighborV.Position.X == X + 32)
                        {
                            this.mapGraph.Edges.Add(new Edge(objV, neighborV, Graph.EmptyWeight, Directions.Right));
                            this.mapGraph.Edges.Add(new Edge(neighborV, objV, Graph.EmptyWeight, Directions.Left));
                        }
                        else
                        {
                            this.mapGraph.Edges.Add(new Edge(objV, neighborV, Graph.EmptyWeight, Directions.Left));
                            this.mapGraph.Edges.Add(new Edge(neighborV, objV, Graph.EmptyWeight, Directions.Right));
                        }
                }
            }
        }

        private Texture2D GetTexture(int ImageName)
        {
            return this.Game.Content.Load<Texture2D>("Images\\Maps\\" + ImageName);
        }

        #endregion
    }

    public struct MapLevel
    {
        public Sprite[,] MapStruct;
        public int Number;
        public bool Walkable;
        public bool UpOfPlayer;
    }
}