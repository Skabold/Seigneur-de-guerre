using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SeigneursDeGuerre.Moteur.Trajectoires {
    /// ----------------------------------------------------------------------------------------
    /// <summary>
    /// Define a node.
    /// </summary>
    /// <remarks> 
    /// Remember: F = Cost + Heuristic! 
    /// Read the html file in the documentation directory (AStarAlgo project) for more informations.
    /// </remarks>
    /// ----------------------------------------------------------------------------------------
    class Node {
        // Carte
        private int _costG = 0; // From start point to here
        private Node _parent = null;
        private Point _currentPoint;

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Creation des noeuds suivants
        /// </summary>
        /// <param name="parent"> The parent node. </param>
        /// <param name="currentPoint"> The current point. </param>
        /// ----------------------------------------------------------------------------------------
        public Node(Node parent, Point currentPoint, Map map) {
            this._currentPoint = currentPoint;
            this.SetParent(parent, map);
            
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get or set the parent.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public Node Parent {
            get { return this._parent; }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the cost.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public int Cost {
            get { return this._costG; }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the F distance (Cost + Heuristic).
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public int getF(Map map) {
            return this._costG + this.GetHeuristic(map);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the location of the node.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public Point MapPoint {
            get { return this._currentPoint; }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Set the parent.
        /// </summary>
        /// <param name="parent"> The parent to set. </param>
        /// ----------------------------------------------------------------------------------------
        public void SetParent(Node parent, Map _map) {
            this._parent = parent;

            // Refresh the cost : the cost of the parent + the cost of the current point
            if (parent != null) {
                this._costG = this._parent.Cost + _map.GetCost(this._currentPoint);
            }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// The cost if you move to this.
        /// </summary>
        /// <returns> The futur cost. </returns>
        /// --------- -------------------------------------------------------------------------------
        public int CostWillBe(Map _map) {
            return (this._parent != null ? this._parent.Cost + _map.GetCost(this._currentPoint) : 0);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Calculate the heuristic. (absolute x and y displacement).
        /// </summary>
        /// <returns> The heuristic. </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetHeuristic(Map _map) {
            return (Math.Abs(this._currentPoint.X - _map.EndPoint.X) + Math.Abs(this._currentPoint.Y - _map.EndPoint.Y));
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the possible node.
        /// </summary>
        /// <param name="_map"> carte</param>
        /// <param name="array">tableau d'au moins 8 noeuds</param>
        /// <returns> le nombre de  noeuds utilisés dans le tableau</returns>
        /// ----------------------------------------------------------------------------------------
        public int GetPossibleNode(Map _map, Node[] array) {
            int idx = 0;
            Point mapPt = new Point();

            // Top
            mapPt.X = _currentPoint.X;
            mapPt.Y = _currentPoint.Y - 1;
            if (!_map.IsWall(mapPt)) {
                array[idx++] = new Node(this, mapPt, _map);
            }

            // Right
            mapPt.X = _currentPoint.X + 1;
            mapPt.Y = _currentPoint.Y;
            if (!_map.IsWall(mapPt)) {
                array[idx++] = new Node(this, mapPt, _map);
            }

            // Left
            mapPt.X = _currentPoint.X - 1;
            mapPt.Y = _currentPoint.Y;
            if (!_map.IsWall(mapPt)) {
                array[idx++] = new Node(this, mapPt, _map);
            }

            // Bottom
            mapPt.X = _currentPoint.X;
            mapPt.Y = _currentPoint.Y + 1;
            if (!_map.IsWall(mapPt)) {
                array[idx++] = new Node(this, mapPt, _map);
            }

            /// Top Left
            mapPt.X = _currentPoint.X - 1;
            mapPt.Y = _currentPoint.Y - 1;
            if (!_map.IsWall(mapPt)) {
                array[idx++] = new Node(this, mapPt, _map);
            }

            /// Top Right
            mapPt.X = _currentPoint.X + 1;
            mapPt.Y = _currentPoint.Y - 1;
            if (!_map.IsWall(mapPt)) {
                array[idx++] = new Node(this, mapPt, _map);
            }

            /// Bottom Left
            mapPt.X = _currentPoint.X - 1;
            mapPt.Y = _currentPoint.Y + 1;
            if (!_map.IsWall(mapPt)) {
                array[idx++] = new Node(this, mapPt, _map);
            }

            /// Bottom right
            mapPt.X = _currentPoint.X + 1;
            mapPt.Y = _currentPoint.Y + 1;
            if (!_map.IsWall(mapPt)) {
                array[idx++] = new Node(this, mapPt, _map);
            }
            return idx;
        }


        /// <summary>
        /// Structure optimisée pour "Close" : basée sur hashmap
        /// </summary>
        internal class NodeList : Dictionary<Point, Node> {
            public void Add(Node node) {
                this[node.MapPoint] = node;
            }
            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Chek if the collection contains a Node (the MapPoint are compared by value!).
            /// </summary>
            /// <param name="node"> The node to check. </param>
            /// <returns> True if it's contained, otherwise false. </returns>
            /// ----------------------------------------------------------------------------------------
            public bool Contains(Node node) {
                return this.ContainsKey(node.MapPoint);
            }
            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Get a node from the collection (the MapPoint are compared by value!).
            /// </summary>
            /// <param name="node"> The node to get. </param>
            /// <returns> The node with the same MapPoint. </returns>
            /// ----------------------------------------------------------------------------------------
            public Node this[Node node] {
                get {
                    return this[node.MapPoint];
                }
            }
            /// <summary>
            /// Enlève le 1er noeud au sens de la distance F
            /// </summary>
            /// <param name="_map"></param>
            /// <returns></returns>
            public Node RemoveFirst(Map _map) {
                // Recherche le 1er
                int min = Int32.MaxValue;
                Node first = null;
                foreach (Node n in this.Values) {
                    if (n.getF(_map) < min) {
                        min = n.getF(_map);
                        first = n;
                    }
                }
                this.Remove(first.MapPoint);
                return first;
            }
        }
    }
}