using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SeigneursDeGuerre.Moteur;

namespace SeigneursDeGuerre.Moteur.Trajectoires {
    /// <summary>
    /// Classe décrivant un chemin d'un point à un autre
    /// </summary>
    class Trajectoire {
        public struct Etape {
            /// <summary>
            /// Position sur la carte du point de la trajectoire
            /// </summary>
            public Point point;
            /// <summary>
            /// Si true ce point peut être atteint pendant ce tour par cette armée
            /// </summary>
            public bool tourCourant;
            public Etape(Point _point, bool _tour) {
                point = _point;
                tourCourant = _tour;
            }
        }

        /// <summary>
        /// Jeu
        /// </summary>
        private Jeu _jeu;
        /// <summary>
        /// Armée dont c'est la trajectoire
        /// </summary>
        private Armee _armee;

        /// <summary>
        /// Chemin calculé
        /// </summary>
        private List<Etape> _chemin;

        // Accesseurs ---------------
        public List<Etape> chemin {
            get { return _chemin; }
        }

        // Constructeur -----------------
        public Trajectoire(Jeu leJeu, Armee larmee) {
            _jeu = leJeu;
            _armee = larmee;
            _chemin = new List<Etape>();
            // Pas de trajectoire si pas de destination 
            if (_armee.moveTarget != null) {
                calculTrajectoire();
            }
        }

        // ----------------------------------
        // Calcul de la trajectoire
        // -----------------------------------

        public void calculTrajectoire() {
            Point courant = _armee.positionCarte;
            Point arrivee = _armee.moveTarget.GetValueOrDefault();
            _chemin = new List<Etape>();

            // Seulement si la cible est accessible
            if (_armee.isPlaceLibre(arrivee, false)) {

                int[] mouvements = new int[_armee.getTaille()];
                int mouvementMin = Int32.MaxValue;
                int mouvementMinNage = Int32.MaxValue;
                for (int i = 0; i < mouvements.Length; i++) {
                    mouvements[i] = _armee.getCreature(i).mouvementCourant;
                    if (mouvementMin > mouvements[i]) {
                        mouvementMin = mouvements[i];
                    }
                    if (_armee.getCreature(i).description.nage) {
                        if (mouvementMinNage > mouvements[i]) {
                            mouvementMinNage = mouvements[i];
                        }
                    }
                }

                List<Point> path = CalculateBestPath();
                if (path != null) {
                    for (int idx = path.Count - 1; idx >= 0; idx--) {
                        _chemin.Add(new Etape(path[idx], ((mouvementMin > 0) && (mouvementMinNage == Int32.MaxValue)) || ((mouvementMinNage != Int32.MaxValue)&&(mouvementMinNage>0))));
                        mouvementMin = Int32.MaxValue;
                        mouvementMinNage = Int32.MaxValue;        
                        for (int i = 0; i < mouvements.Length; i++) {
                            mouvements[i] -= _armee.getCreature(i).getMouvementCost(path[idx], _armee);
                            if (mouvementMin > mouvements[i]) {
                                mouvementMin = mouvements[i];
                            }
                            if (_armee.getCreature(i).description.nage) {
                                if (mouvementMinNage > mouvements[i]) {
                                    mouvementMinNage = mouvements[i];
                                }
                            }
                        }

                    }
                }
            }

        }
        //------------------------------------------
        // Utilise A* http://fr.wikipedia.org/wiki/Algorithme_A*
        // --------------------------------------------

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Calculate the shortest path between the start point and the end point.
        /// </summary>
        /// <remarks> The path is reversed. </remarks>
        /// <returns> The shortest path. </returns>
        /// ----------------------------------------------------------------------------------------
        private List<Point> CalculateBestPath() {
            //System.Diagnostics.Stopwatch chrono = new System.Diagnostics.Stopwatch();
            //chrono.Start();


            Map map = new Map(_jeu, _armee.positionCarte, _armee.moveTarget.GetValueOrDefault(), _armee);
            Node.NodeList open = new Node.NodeList();
            Node.NodeList close = new Node.NodeList();
            Node[] possibleNodes = new Node[8];

            Node startNode = new Node(null,map.StartPoint, map);
            open.Add(startNode);
            int nbIter = 0;
            List<Point> sol = null;
            while (open.Count > 0) {
                Node best = open.RemoveFirst(map);           // This is the best node
                if (best.MapPoint == map.EndPoint) {      // We are finished                
                    sol = new List<Point>();  // The solution
                    while (best.Parent != null) {
                        sol.Add(best.MapPoint);
                        best = best.Parent;
                    }
                    // Return the solution when the parent is null (the first point)
                    break;
                }
                close.Add(best);
                this.AddToOpen(open, close, best, possibleNodes, best.GetPossibleNode(map,possibleNodes), map);
                nbIter++;         
            }
            //chrono.Stop();
            //_jeu.messageDebug = "nb iter = " + nbIter + " en " + chrono.ElapsedMilliseconds + " ms" + "taille(close) = " + close.Count + " taille(open) = "+ open.Count;
            return sol;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Add a list of nodes to the open list if needed.
        /// </summary>
        /// <param name="current"> The current nodes. </param>
        /// <param name="nodes"> The nodes to add. </param>
        /// ----------------------------------------------------------------------------------------
        private void AddToOpen(Node.NodeList open, Node.NodeList close, Node current, Node[] nodes, int nbNodes, Map map) {
            for (int i=0;i<nbNodes;i++) {
                Node node = nodes[i];
                if (!open.Contains(node)) {
                    if (!close.Contains(node)) {
                        open.Add(node);
                    }
                }
                // Else really needed ?
                else {
                    if (node.CostWillBe(map) < open[node].Cost) {
                        node.SetParent(current, map);
                    }
                }
            }
        }

    }
}
