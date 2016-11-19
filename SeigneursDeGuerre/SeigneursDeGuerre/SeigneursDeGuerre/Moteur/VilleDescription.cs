using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    /// Décrit une ville (indice, position, défense, productions...)
    /// </summary>
    class VilleDescription {
        /// <summary>
        /// Indice de la ville
        /// </summary>
        private int _indice;

        /// <summary>
        /// position dans le terrain (x,y) du point haut, gauche
        /// </summary>
        private Point _positionMap;

        /// <summary>
        /// Nom de la ville
        /// </summary>
        private string _nom;


        /// <summary>
        /// Défense : 0 = nul à 9 = super fort
        /// </summary>
        private int _niveauDefense;

        /// <summary>
        /// Productivité de la ville : 0 = null à 9 = super fort
        /// </summary>
        private int _niveauProductivite;

        /// <summary>
        /// Indique si c'est une capitale (!=0) et le n° de faction si c'est le cas
        /// </summary>
        private int _capitaleFaction;

        /// <summary>
        /// Faction actuelle
        /// </summary>
        private int _faction;

        /// <summary>
        /// Créatures pouvant être construites, par ordre de force
        /// </summary>
        private int[] _typeCreatures;
        
        // production en cours
        private int _productionCourante;

        // production accumulée dans la ville (réinitialisée à chaque production)
        private int _productionPoints;
        

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        /// <param name="lindice"></param>
        public VilleDescription(int lindice) {
            _indice = lindice;
            _positionMap.X = 0;
            _positionMap.Y = 0;
            _niveauDefense = 0;
            _niveauProductivite = 0;
            _capitaleFaction = 0;
            _faction = 0;
            _nom = "Ville sans nom";
            // Paysan
            _typeCreatures = new int[] { 11 };
            _productionCourante = 0;
            _productionPoints = 0;
        }

        // Accesseurs ------------------------------------------------------------------
        public int indice {
            get {return _indice;}            
        }
        public Point positionMap {
            get { return _positionMap; }
            set { _positionMap = value; }
        }
        public int niveauDefense {
            get { return _niveauDefense; }
            set { _niveauDefense = value; }
        }
        public int niveauProductivite {
            get { return _niveauProductivite; }
            set { _niveauProductivite = value; }
        }
        public int capitaleFaction {
            get { return _capitaleFaction; }
            set { _capitaleFaction = value; }
        }
        public int faction {
            get { return _faction; }
            set { _faction = value; }
        }
        public string nom {
            get { return _nom; }
            set { _nom = value; }
        }
        public int[] typeCreatures {
            get { return _typeCreatures; }
            set { _typeCreatures = value; }
        }

        public int productionCourante {
            get {return _productionCourante;}
            set { _productionCourante = value; }
        }

        public int orPourNiveauSuivant {
            get { return _niveauDefense * 250; }
        }

        public int productionPoints {
            get { return _productionPoints; }
            set { _productionPoints = value; }
        }

        public int geNbToursPourCreatures(int coutCreature) {
            int nbTours = coutCreature / (niveauProductivite * 3);
            if (nbTours == 0) {
                nbTours = 1;
            }
            return nbTours;
        }

        public int getRevenus() {
            return (niveauProductivite * 50) + (productionCourante == -1 ? (niveauProductivite * 25) : 0);
        }
        
        /// <summary>
        /// Indique si la position de la carte fait partie de la ville
        /// </summary>
        /// <param name="p">position</param>
        /// <returns>true si partie de ville</returns>
        public bool contains(Point p) {
            return (p == positionMap) || (p == new Point(positionMap.X, positionMap.Y + 1)) ||
                    (p == new Point(positionMap.X + 1, positionMap.Y + 1)) || (p == new Point(positionMap.X + 1, positionMap.Y));
        }


    }
}
