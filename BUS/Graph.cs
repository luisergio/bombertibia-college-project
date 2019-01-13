using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BUS
{
    public class Graph
    {
        #region Variables

        private List<Vertex> lstVertex;
        private List<Edge> lstEdge;

        #endregion
        
        #region Properties

        public List<Vertex> Vertexs
        {
            get { return this.lstVertex; }
        }

        public List<Edge> Edges
        {
            get { return this.lstEdge; }
        }

        #endregion

        #region Contructors

        public Graph()
        {
            this.lstEdge = new List<Edge>();
            this.lstVertex = new List<Vertex>();
        }

        public Graph(Graph Graph)
        {
            this.lstVertex = new List<Vertex>();
            this.lstEdge = new List<Edge>();

            foreach (Vertex vertex in Graph.Vertexs)
                this.lstVertex.Add(vertex.Clone());

            foreach (Edge edge in Graph.Edges)
                this.lstEdge.Add(edge.Clone(lstVertex));
        }

        #endregion

        #region Public Methods

        public Graph Clone()
        {
            return new Graph(this);
        }

        public Directions GetBestWay(Vector2 From, Vector2 To)
        {
            //Zera todos as vertices
            foreach (Vertex vet in lstVertex)
                vet.Distance = float.MaxValue;

            //Busca Vertice inicial
            Vertex fromVertex = lstVertex.First(v => v.Position == From);
            fromVertex.Distance = 0;

            Vertex currentVertex = fromVertex;

            //Busca os vértices não percorridos.
            List<Vertex> TraveledVertexs = new List<Vertex>();
            TraveledVertexs.Add(currentVertex);

            bool NotReachable = false;

            //Enquanto não percorrer todos.
            while (TraveledVertexs.Count() != lstVertex.Count)
            {
                //Busca as arestas que saem do vértice atual
                IEnumerable<Edge> possibleEdge = from edg in lstEdge
                                                where edg.From == currentVertex
                                                && edg.Weight != Graph.WillExplodeWeight
                                                select edg;

                //Pra cada vértice alcançavel, calcula nova distância.
                foreach (Edge possEdge in possibleEdge)
                {
                    if (possEdge.To.Distance > possEdge.Weight + currentVertex.Distance)
                    {
                        possEdge.To.Distance = possEdge.Weight + currentVertex.Distance;
                        possEdge.To.LastVertex = currentVertex;
                    }
                }
                //Pega o vértice com menor peso que ainda não foi expandido.
                IEnumerable<Vertex> OthersV = lstVertex.Except(TraveledVertexs);

                currentVertex = OthersV.First(v => v.Distance == OthersV.Min(vert => vert.Distance));
            
                if (currentVertex.Distance == float.MaxValue)
                {
                    NotReachable = true;
                    break;
                }

                //Adiciona o vertice atual na lista de vertices que foram expandidos.
                TraveledVertexs.Add(currentVertex);

                if (currentVertex.Position == To) break;
            }

            if (NotReachable) return Directions.Stopped;

            Vertex toVertex = this.lstVertex.First(v => v.Position == To);

            currentVertex = toVertex;

            //Volta no melhor caminho
            while (currentVertex.LastVertex != fromVertex)
                currentVertex = currentVertex.LastVertex;

            //Retorna a direção
            return this.lstEdge.First(edg => edg.From == fromVertex && edg.To == currentVertex).Direction;
        }

        public Directions GetBestEscape(Vector2 From)
        {
            //Zera todos as vertices
            foreach (Vertex vet in lstVertex)
                vet.Distance = float.MaxValue;

            //Busca Vertice inicial
            Vertex fromVertex = lstVertex.First(v => v.Position == From);
            fromVertex.Distance = 0;

            Vertex currentVertex = fromVertex;
            //Busca os vértices não percorridos.
            List<Vertex> TraveledVertexs = new List<Vertex>();
            TraveledVertexs.Add(currentVertex);

            //Busca as arestas que saem do vértice atual
            IEnumerable<Edge> possibleEdge = from edg in lstEdge
                                             where edg.From == currentVertex
                                             && (
                                                 edg.Weight == Graph.WillExplodeWeight
                                                 ||
                                                 edg.Weight == Graph.EmptyWeight
                                             )
                                             select edg;

            //Enquanto não percorrer todos.
            while (possibleEdge.Count() != 0)
            {
                bool exit = false;
                //Pra cada vértice alcançavel, calcula nova distância.
                foreach (Edge possEdge in possibleEdge)
                {
                    if (possEdge.Weight == Graph.EmptyWeight)
                    {
                        possEdge.To.LastVertex = currentVertex;
                        currentVertex = possEdge.To;
                        exit = true;
                        break;
                    }
                    
                    float newWeight = possEdge.Weight + currentVertex.Distance;

                    if (possEdge.To.Distance > newWeight)
                    {
                        possEdge.To.Distance = newWeight;
                        possEdge.To.LastVertex = currentVertex;
                    }
                }

                if (exit) break;

                //Pega o vértice com menor peso que ainda não foi expandido.
                IEnumerable<Vertex> OthersV = lstVertex.Except(TraveledVertexs);
                currentVertex = OthersV.First(v => v.Distance == OthersV.Min(vert => vert.Distance));

                //Adiciona o vertice atual na lista de vertices que foram expandidos.
                TraveledVertexs.Add(currentVertex);

                //Busca as arestas que saem do vértice atual
                possibleEdge = from edg in lstEdge
                               where edg.From == currentVertex
                               && (
                                   edg.Weight == Graph.WillExplodeWeight
                                   ||
                                   edg.Weight == Graph.EmptyWeight
                               )
                               select edg;
            }


            //Volta no melhor caminho
            while (currentVertex.LastVertex != fromVertex)
                currentVertex = currentVertex.LastVertex;

            //Retorna a direção
            return this.lstEdge.First(edg => edg.From == fromVertex && edg.To == currentVertex).Direction;
        }

        public List<Vector2> WalkableEmptyPositions(Vector2 From)
        {
            List<Vector2> WEP = new List<Vector2>();
            WEP.Add(From);

            //Busca Vertice inicial
            Vertex fromVertex = lstVertex.First(v => v.Position == From);
            Vertex currentVertex = fromVertex;

            //Vértices não percorridos.
            List<Vertex> NotTraveledVertexs = new List<Vertex>();
            List<Vertex> TraveledVertexs = new List<Vertex>();

            NotTraveledVertexs.Add(currentVertex);

            while (NotTraveledVertexs.Count > 0)
            {
                currentVertex = NotTraveledVertexs[0];
                TraveledVertexs.Add(currentVertex);

                //Busca as arestas que saem do vértice atual
                IEnumerable<Edge> possibleEdge = from edg in lstEdge
                                                 where edg.From == currentVertex
                                                 && edg.Weight == Graph.EmptyWeight
                                                 select edg;

                foreach (Edge edg in possibleEdge)
                    if (!TraveledVertexs.Contains(edg.To))
                    {
                        NotTraveledVertexs.Add(edg.To);
                        WEP.Add(edg.To.Position);
                    }

                NotTraveledVertexs.Remove(currentVertex);
            }

            return WEP;
        }

        public void UpdateWeight(float X, float Y, float NewWeight)
        {
            this.UpdateWeight(new Vector2(X,Y), NewWeight);
        }

        public void UpdateWeight(Vector2 Position, float NewWeight)
        {
            IEnumerable<Edge> lstEdg = from e in this.lstEdge
                                       where e.To.Position == Position
                                       select e;

            foreach (Edge edg in lstEdg)
            {
                edg.Weight = NewWeight;
                edg.To.WeightToHere = NewWeight;
            }
        }

        #endregion

        #region Public Static Properties

        public static float EmptyWeight
        {
            get{ return 0.5f; }
        }

        public static float ObstacleWeight
        {
            get{ return 100f; }
        }

        public static float WillExplodeWeight
        {
            get{ return 5000f; }
        }

        public static float ExplodingWeight
        {
            get { return 10000f; }
        }

        #endregion
    }
}