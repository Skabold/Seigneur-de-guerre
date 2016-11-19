using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SeigneursDeGuerre.Moteur.Trajectoires {

    /// ----------------------------------------------------------------------------------------
    /// <summary>
    /// Construit une structure de coût pour la recherche du meilleur chemin à partir d'une carte et 
    /// D'une armée
    /// </summary>
    /// ----------------------------------------------------------------------------------------
    class Map {
        private int[,] _costs = null;
        private Point _startPt;
        private Point _endPt;
        private Jeu _jeu;

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Create a new map.
        /// </summary>
        /// <param name="fullMapFile"> The map's path to load. </param>
        /// ----------------------------------------------------------------------------------------
        public Map(Jeu leJeu, Point startPt, Point endPt, Armee creas) {
            // Calcul des coûts de façon optimisée

            _jeu = leJeu;
            _startPt = startPt;
            _endPt = endPt;
            _costs = new int[Largeur, Hauteur];
            this.AssignCost(creas);
            
            foreach (VilleDescription vd in leJeu.villes.villeDesc) {
                if (vd.faction != creas.faction) {
                    _costs[vd.positionMap.X, vd.positionMap.Y] = Creature.MVTINFINI;
                    _costs[vd.positionMap.X + 1, vd.positionMap.Y] = Creature.MVTINFINI;
                    _costs[vd.positionMap.X, vd.positionMap.Y + 1] = Creature.MVTINFINI;
                    _costs[vd.positionMap.X + 1, vd.positionMap.Y + 1] = Creature.MVTINFINI;
                }
                else {
                    // Si la ville est chez nous, toute position "dans la ville" est peu chère
                    _costs[vd.positionMap.X, vd.positionMap.Y] = 1;
                    _costs[vd.positionMap.X + 1, vd.positionMap.Y] = 1;
                    _costs[vd.positionMap.X, vd.positionMap.Y + 1] = 1;
                    _costs[vd.positionMap.X + 1, vd.positionMap.Y + 1] = 1;
                }
            }
            
            
            // On contourne les créatures ennemies
            foreach (Armee armee in leJeu.armees) {
                if (armee.faction != creas.faction) {
                    _costs[armee.positionCarte.X, armee.positionCarte.Y] = Creature.MVTINFINI;
                }
            }


        }



        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the length of the map.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public int Largeur {
            get { return _jeu.terrain.terrainDesc.getLargeur(); }
        }
        public int Hauteur {
            get { return _jeu.terrain.terrainDesc.getHauteur(); }
        }


        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the size of the map.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public int Size {
            get { return Largeur * Hauteur; }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the start point.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public Point StartPoint {
            get { return this._startPt; }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the end Point.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public Point EndPoint {
            get { return this._endPt; }
        }

        
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Assign the costs.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        private void AssignCost(Armee creas) {
            // Cache les valeurs de terrain possible pour aller plus vite
            int cCol = creas.getMouvementCost(new TerrainDescription.TerrainCell(TerrainDescription.TypeTerrain.COLLINE));
            int cEau = creas.getMouvementCost(new TerrainDescription.TerrainCell(TerrainDescription.TypeTerrain.EAU));
            int cFor = creas.getMouvementCost(new TerrainDescription.TerrainCell(TerrainDescription.TypeTerrain.FORET));
            int cHer = creas.getMouvementCost(new TerrainDescription.TerrainCell(TerrainDescription.TypeTerrain.HERBE));
            int cMar = creas.getMouvementCost(new TerrainDescription.TerrainCell(TerrainDescription.TypeTerrain.MARECAGE));
            int cMon = creas.getMouvementCost(new TerrainDescription.TerrainCell(TerrainDescription.TypeTerrain.MONTAGNE));
            TerrainDescription.TerrainCell cell = new TerrainDescription.TerrainCell(TerrainDescription.TypeTerrain.HERBE);
            cell.route = true;
            int cRou = creas.getMouvementCost(cell );

            for (int i = 0; i < this.Hauteur; i++) {
                for (int j = 0; j < this.Largeur; j++) {
                    cell = _jeu.terrain.terrainDesc.getCellAt(j,i);
                    int cost = Creature.MVTINFINI;
                    if (cell.route) {
                        cost = cRou;
                    }
                    else {
                        switch (cell.type) {
                            case TerrainDescription.TypeTerrain.COLLINE:
                                cost = cCol;
                                break;
                            case TerrainDescription.TypeTerrain.EAU:
                                cost = cEau;
                                break;
                            case TerrainDescription.TypeTerrain.FORET:
                                cost = cFor;
                                break;
                            case TerrainDescription.TypeTerrain.HERBE:
                                cost = cHer;
                                break;
                            case TerrainDescription.TypeTerrain.MARECAGE:
                                cost = cMar;
                                break;
                            case TerrainDescription.TypeTerrain.MONTAGNE:
                                cost = cMon;
                                break;
                        }
                    }
                    this._costs[j, i] = cost;
                }
            }
        }
        
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Check if a point is valid.
        /// </summary>
        /// <param name="labyPt"> The point to check. </param>
        /// <returns> True if the point is valid, otherwise false. </returns>
        /// ----------------------------------------------------------------------------------------
        public bool IsPointValid(Point labyPt) {
            return (this.Largeur > labyPt.X) && (labyPt.X >= 0) && (labyPt.Y >= 0) && (this.Hauteur > labyPt.Y);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Check if the current point is a wall (outside point = wall).
        /// </summary>
        /// <param name="labyPt"> The point. </param>
        /// <returns> True if it is a wall. </returns>
        /// ----------------------------------------------------------------------------------------
        public bool IsWall(Point labyPt) {
            return this.GetCost(labyPt) >= Creature.MVTINFINI;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the cost of a Point.
        /// </summary>
        /// <param name="labyPt"> The point. </param>
        /// <returns> The cost. </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetCost(Point labyPt) {
            if (this.IsPointValid(labyPt)) {
                return this._costs[labyPt.X, labyPt.Y];
            }
            return Creature.MVTINFINI;
        }
    }
}

