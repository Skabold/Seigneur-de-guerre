using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    /// Les constantes d'une créature 
    /// </summary>
    class CreatureDescription {
        /// <summary>
        /// Bonus selon que l'on est en ville
        /// </summary>
        public enum BonusUrbain {
            AUCUN,
            VILLE,
            EXTERIEUR
        }

        /// <summary>
        /// Bonus selon le type d'environnement extérieur
        /// </summary>
        public enum BonusEnvironnement {
            AUCUN,
            COLLINES,
            MARAIS,
            FORÊT,
            HERBE,
            EAU
        }

        /// <summary>
        /// Nom constant de la créature (le nom du héros est une caractéristique de Creature, pas CreatureDescription, comme toutes les variables)
        /// </summary>
        private string _nom;
        /// <summary>
        /// Force de 1 à 10
        /// </summary>
        private int _force;
        /// <summary>
        /// Mouvement max
        /// </summary>
        private int _mouvement;
        /// <summary>
        /// Cout de l'unité
        /// </summary>
        private int _cout;
        /// <summary>
        /// Peut voler
        /// </summary>
        private bool _vol;
        /// <summary>
        /// Peut nager
        /// </summary>
        private bool _nage;
        /// <summary>
        /// Est un héros (peut porter des objets, peut être transporté dans les airs ou l'eau)
        /// </summary>
        private bool _heros;
        /// <summary>
        /// +1 dans ces conditions
        /// </summary>
        private BonusUrbain _bonusAttaque;
        /// <summary>
        /// +1 dans ces conditions
        /// </summary>
        private BonusUrbain _bonusDefense;
        /// <summary>
        /// /2 le malus de terrain dans ces conditions
        /// </summary>
        private BonusEnvironnement _bonusMouvement;
        
        /// <summary>
        /// Position de la créature dans la planche de texture
        /// </summary>
        private Point _positionPlanche;

        /// <summary>
        /// Identifiant du type de créature
        /// </summary>
        private int _indexCreature;

        // Constructeur ----------------------------------------------------------------
                
        public CreatureDescription(string lnom, Point positionPlanche, int index) {
            _nom = lnom;
            _positionPlanche = positionPlanche;
            _indexCreature = index;
            // Valeurs par défaut pour le reste
            _force = 10;
            _mouvement = 10;
            _heros = false;
            _vol = false;
            _nage = false;
            _bonusAttaque = BonusUrbain.AUCUN;
            _bonusDefense = BonusUrbain.AUCUN;
            _bonusMouvement = BonusEnvironnement.AUCUN;
        }
        /// <summary>
        /// Retourne une ligne décrivant les caractéristiques de la créature
        /// </summary>
        /// <returns></returns>
        public string profilAsStr() {
            StringBuilder sb = new StringBuilder();
            sb.Append("F:").Append(force);
            sb.Append(" - M:").Append(mouvement);
            sb.Append(vol ? " - Vol" : "");
            sb.Append(nage ? " - Mer" : "");
            if (bonusAttaque != BonusUrbain.AUCUN) {
                sb.Append(" - A+ " + bonusAttaque.ToString().ToLower());
            }
            if (bonusDefense != BonusUrbain.AUCUN) {
                sb.Append(" - D+ " + bonusDefense.ToString().ToLower());
            }
            if (bonusMouvement != BonusEnvironnement.AUCUN) {
                sb.Append(" - M+ " + bonusMouvement.ToString().ToLower());
            }

            return sb.ToString();
        }

        // Accesseurs--------------------------------------------------------------------
        public string nom {
            get { return _nom; }
            set { _nom = value; }
        }
        public int force {
            get { return _force; }
            set { _force = value; }
        }
        /// <summary>
        /// Mouvement max
        /// </summary>
        public int mouvement {
            get { return _mouvement; }
            set { _mouvement = value; }
        }
        /// <summary>
        /// cout unité
        /// </summary>
        public int cout {
            get { return _cout; }
            set { _cout = value; }
        }
        /// <summary>
        /// Peut voler
        /// </summary>
        public bool vol {
            get { return _vol; }
            set { _vol = value; }
        }
        /// <summary>
        /// Peut nager
        /// </summary>
        public bool nage {
            get { return _nage; }
            set { _nage = value; }
        }
        /// <summary>
        /// Est un héros (peut porter des objets, peut être transporté dans les airs ou l'eau)
        /// </summary>
        public bool heros {
            get { return _heros; }
            set { _heros = value; }
        }
        /// <summary>
        /// +1 dans ces conditions
        /// </summary>
        public BonusUrbain bonusAttaque {
            get { return _bonusAttaque; }
            set { _bonusAttaque = value; }
        }
        /// <summary>
        /// +1 dans ces conditions
        /// </summary>
        public BonusUrbain bonusDefense {
            get { return _bonusDefense; }
            set { _bonusDefense = value; }
        }
        /// <summary>
        /// /2 le malus de terrain dans ces conditions
        /// </summary>
        public BonusEnvironnement bonusMouvement {
            get { return _bonusMouvement; }
            set { _bonusMouvement = value; }
        }

        /// <summary>
        /// position dans la planche de texture
        /// </summary>
        public Point positionPlanche {
            get { return _positionPlanche; }
        }

        /// <summary>
        /// Identifiant du type de créature
        /// </summary>
        public int indexCreature {
            get { return _indexCreature; }
        }

      



    }
}
