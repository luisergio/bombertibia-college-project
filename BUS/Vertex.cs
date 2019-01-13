using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BUS
{
    public class Vertex
    {
        #region Variables

        private float distance;
        private Vertex lastVertex;
        private Vector2 position;
        private float weightToHere;

        #endregion

        #region Properties

        public float Distance
        {
            set { this.distance = value; }
            get { return this.distance; }
        }

        public Vertex LastVertex
        {
            set { this.lastVertex = value; }
            get { return this.lastVertex; }
        }

        public Vector2 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        public float WeightToHere
        {
            set { this.weightToHere = value; }
            get { return this.weightToHere; }
        }

        #endregion

        #region Contructors

        public Vertex(Vertex Vertex)
        {
            this.position = Vertex.Position;
            this.lastVertex = Vertex.LastVertex;
            this.distance = Vertex.Distance;
            this.weightToHere = Vertex.WeightToHere;
        }

        public Vertex(Vector2 Position)
        {
            this.distance = float.MaxValue;
            this.lastVertex = null;
            this.position = Position;
        }

        public Vertex(int X, int Y)
        {
            this.distance = float.MaxValue;
            this.lastVertex = null;
            this.position = new Vector2(X,Y);
        }

        #endregion

        #region Public Methods

        public Vertex Clone()
        {
            return new Vertex(this);
        }

        #endregion
    }
}