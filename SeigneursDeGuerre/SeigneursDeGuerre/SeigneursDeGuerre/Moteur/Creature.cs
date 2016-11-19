using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    ///  Une créature isolée
    /// </summary>
    class Creature :  IComparable<Creature> {

        /// <summary>
        /// Mouvement infini
        /// </summary>
        public const int MVTINFINI = 1000000;

        /// <summary>
        /// le jeu
        /// </summary>
        private Jeu _jeu;
        /// <summary>
        /// index du type de la créature
        /// </summary>
        private int _typeCreature;

        /// <summary>
        /// raccourci vers la description du type de créature
        /// </summary>
        private CreatureDescription _desc;

        /// <summary>
        /// Le vrai nom de la créature
        /// </summary>
        private string _vraiNom;

        /// <summary>
        /// force de la créature
        /// </summary>
        private int _vraieforce;
        /// <summary>
        /// Mouvement max
        /// </summary>
        private int _vraimouvement;
        /// <summary>
        /// Cout de l'unité
        /// </summary>
        private int _vraicout;
        /// <summary>
        /// Peut voler
        /// </summary>
        private bool _vraivol;
        /// <summary>
        /// Peut nager
        /// </summary>
        private bool _vrainage;


        /// <summary>
        /// Combien il reste de mouvement à cette créature
        /// </summary>
        private int _mouvementCourant;

        /// <summary>
        /// Faction de la créature
        /// </summary>
        private int _faction;

        /// <summary>
        /// Position dans la carte (au pixel près, type offset terrain)
        /// </summary>
        private Point _positionEcran;

        /// <summary>
        /// Position officielle dans la carte (unité = carte)
        /// </summary>
        private Point _positionCarte;

        /// <summary>
        /// Possessions de la créature
        /// </summary>
        private List<ItemDescription> _items;

        // TODO : expérience, bonuses...

        // Créateur -----------------------------------------------------------
        /// <summary>
        /// Crée une créature
        /// </summary>
        /// <param name="leJeu">jeu</param>
        /// <param name="typeCreature">type de créature</param>
        /// <param name="faction">n° de faction</param>
        /// <param name="position">position dans la carte (position de carte)</param>
        public Creature(Jeu leJeu, int typeCreature, int faction, Point position) {
            _jeu = leJeu;
            _typeCreature = typeCreature;
            _faction = faction;
            _positionEcran.X = position.X * _jeu.blockSize;
            _positionEcran.Y = position.Y * _jeu.blockSize;
            _positionCarte = position;

            _items = new List<ItemDescription>();
            // cache la description
            _desc = _jeu.creatures.description[typeCreature];
            _vraiNom = _desc.nom;
            _vraicout = _desc.cout;
            _vraieforce = _desc.force;
            _vraimouvement = _desc.mouvement;
            _vrainage = _desc.nage;
            _vraivol = _desc.vol;
            reinitTour();
        }

        // Accesseurs --------------------------------------------------------
        public CreatureDescription description {
            get { return _desc; }
        }
        public int typeCreature {
            get { return _typeCreature; }
        }
        public int faction {
            get { return _faction; }
        }
        public int mouvementCourant {
            get { return _mouvementCourant; }
            set { 
                _mouvementCourant = value;
                if (value < 0) {
                    _mouvementCourant = 0;
                }
           }
        }
        public Point positionEcran {
            get { return _positionEcran; }
            set { _positionEcran = value; }
        }
        public Point positionCarte {
            get { return _positionCarte; }
            set { _positionCarte = value; }
        }
        public string vraiNom {
            get { return _vraiNom; }
            set { _vraiNom = value; }                 
        }
        /// <summary>
        /// Pour parcours seulement
        /// </summary>
        public List<ItemDescription> items {
            get { return _items; }
        }

        /// <summary>
        /// Ajoute un item à la créature (et modifie son profil)
        /// </summary>
        /// <param name="item"></param>
        public void addItem(ItemDescription item) {
            _items.Add(item);
            // Applique le bonus de l'item immédiatement
            switch (item.effet) {
                case ItemDescription.Effet.BONUS_FORCE:
                    _vraieforce += item.amplitude;
                    break;
                case ItemDescription.Effet.BONUS_MOUVEMENT:
                    _vraimouvement += item.amplitude;
                    break;
                case ItemDescription.Effet.BONUS_OR:
                    _vraicout -= item.amplitude;
                    break;
                case ItemDescription.Effet.NAGE:
                    _vrainage = true;
                    break;
                case ItemDescription.Effet.VOL:
                    _vraivol = true;
                    break;
            }

        }
        /// <summary>
        /// Ajoute un item sans modifier le profil (lors du reload)
        /// </summary>
        /// <param name="itemDescription">item</param>
        public void addItemNoEffect(ItemDescription itemDescription) {
            _items.Add(itemDescription);
        }

        public int force {
            get { return _vraieforce; }
            set { _vraieforce = value; }
        }
        /// <summary>
        /// Mouvement max
        /// </summary>
        public int mouvement {
            get { return _vraimouvement; }
            set { _vraimouvement = value; }
        }
        /// <summary>
        /// cout unité
        /// </summary>
        public int cout {
            get { return _vraicout; }
            set { _vraicout = value; }
        }
        /// <summary>
        /// Peut voler
        /// </summary>
        public bool vol {
            get { return _vraivol; }
            set { _vraivol = value; }
        }
        /// <summary>
        /// Peut nager
        /// </summary>
        public bool nage {
            get { return _vrainage; }
            set { _vrainage = value; }
        }


        /// <summary>
        /// Retourne une ligne décrivant les caractéristiques de la créature
        /// </summary>
        /// <returns></returns>
        public string profilAsStr() {
            StringBuilder sb = new StringBuilder();
            sb.Append("F:").Append(force);
            sb.Append(" - M:").Append(this.mouvementCourant).Append("/").Append(mouvement);
            sb.Append(vol ?  " - Vol" : "");
            sb.Append(nage ? " - Mer" : "");
            if (description.bonusAttaque != CreatureDescription.BonusUrbain.AUCUN) {
                sb.Append(" - A+ " + description.bonusAttaque.ToString().ToLower());
            }
            if (description.bonusDefense != CreatureDescription.BonusUrbain.AUCUN) {
                sb.Append(" - D+ " + description.bonusDefense.ToString().ToLower());
            }
            if (description.bonusMouvement != CreatureDescription.BonusEnvironnement.AUCUN) {
                sb.Append(" - M+ " + description.bonusMouvement.ToString().ToLower());
            }
                
            return sb.ToString();
        }

        /// <summary>
        /// Force pour l'affichage en proue
        /// </summary>
        /// <param name="troop">troupe</param>
        /// <returns></returns>
        public int forcePourProue() {
            if (nage) {
                return 100000;
            }
            else if (description.heros) {
                return 99999;
            }
            else {
                return force;
            }
        }

        /// <summary>
        /// Comparateur de créatures pour les tri de combat
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Creature other) {
            int diff = forcePourProue() - other.forcePourProue();
            if (diff == 0) {
                diff = cout - other.cout;
            }
            return diff;
        }

        /// <summary>
        /// Donne le coût en points de mouvements pour une cellule de terrain donnée
        /// </summary>
        /// <param name="target">cellule visée</param>
        /// <param param name="groupe">armée accompagnant cette créature pour les bonus de groupe</param>
        /// <returns>le coût en points de mouvements</returns>
        public int getMouvementCost(Point target, Armee groupe) {
            // Villes
            foreach (VilleDescription vd in _jeu.villes.villeDesc) {
                if (vd.contains(target)) {
                    return (vd.faction != this.faction)  ? MVTINFINI : 1;
                }
            }
            return getMouvementCost(_jeu.terrain.terrainDesc.getCellAt(target.X, target.Y), groupe);
        }
        /// <summary>
        /// Donne le coût en points de mouvements pour une cellule de terrain donnée. Les villes ne sont pas prises en compte.
        /// </summary>
        /// <param name="cell">cellule de terrain</param>
        /// <param param name="groupe">armée accompagnant cette créature pour les bonus de groupe</param>
        /// <returns>le coût en points de mouvements</returns>
        public int getMouvementCost(TerrainDescription.TerrainCell cell, Armee groupe) {            
            // Les bateaux ne font QUE nager
            if ((cell.type != TerrainDescription.TypeTerrain.EAU) && nage && !description.heros) {
                return MVTINFINI;
            }
            if (cell.route) {
                return 1;
            }
            switch (cell.type) {
                case TerrainDescription.TypeTerrain.COLLINE:
                    return description.bonusMouvement == CreatureDescription.BonusEnvironnement.COLLINES ? 3 : 5;
                case TerrainDescription.TypeTerrain.EAU:
                    return groupe.vol ? 3 : (groupe.nage ? ((description.bonusMouvement == CreatureDescription.BonusEnvironnement.EAU) ? 1 : 2) : MVTINFINI);
                case TerrainDescription.TypeTerrain.FORET:
                    return description.bonusMouvement == CreatureDescription.BonusEnvironnement.FORÊT ? 3 : 5;
                case TerrainDescription.TypeTerrain.HERBE:
                    return description.bonusMouvement == CreatureDescription.BonusEnvironnement.HERBE ? 2 : 3;
                case TerrainDescription.TypeTerrain.MARECAGE:
                    return description.bonusMouvement == CreatureDescription.BonusEnvironnement.MARAIS ? 3 : 5;
                case TerrainDescription.TypeTerrain.MONTAGNE:
                    return groupe.vol ? 3 : MVTINFINI;
            }
            return MVTINFINI;
        }

        // -------------------------------------------------------------------
        /// <summary>
        /// Appelée au début de chaque tour sur chaque créature
        /// </summary>
        public void reinitTour() {
            _mouvementCourant = mouvement;
        }



    }
}
