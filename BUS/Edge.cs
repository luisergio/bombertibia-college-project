using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BUS
{
    public class Edge
    {
        #region Variables

        private Vertex from;
        private Vertex to;
        private float weight;
        private Directions direction;

        #endregion

        #region Public Methods

        public Vertex From
        {
            get { return this.from; }
        }

        public Vertex To
        {
            get { return this.to; }
        }

        public float Weight
        {
            get { return this.weight; }
            set { this.weight = value; }
        }

        public Directions Direction
        {
            get { return this.direction; }
        }

        #endregion

        #region Contructors

        public Edge(Vertex From, Vertex To, float Weight, Directions Direction)
        {
            this.from = From;
            this.to = To;
            this.weight = Weight;
            this.direction = Direction;
        }

        public Edge(Edge Edge, List<Vertex> Vertex)
        {
            this.from = Vertex.Where(v => v.Position == Edge.From.Position).First();
            this.to = Vertex.Where(v => v.Position == Edge.To.Position).First();
            this.weight = Edge.Weight;
            this.direction = Edge.Direction;
        }

        #endregion

        #region Public Methods

        public Edge Clone(List<Vertex> lstVertex)
        {
            return new Edge(this, lstVertex);
        }

        #endregion
    }
}