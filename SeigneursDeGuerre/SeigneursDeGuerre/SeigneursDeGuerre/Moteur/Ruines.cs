using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    /// Gestion des ruines
    /// </summary>
    class Ruines {
        public class RuineDescription {
            public string nom;
            public bool visite;
            public ItemDescription item;
            public Point position;
        }
        /// <summary>
        /// Ruines du jeu
        /// </summary>
        private Dictionary<Point, RuineDescription> _ruines;

        /// <summary>
        /// Jeu
        /// </summary>
        private Jeu _jeu;

        public Ruines(Jeu jeu) {
            _jeu = jeu;
            _ruines = new Dictionary<Point, RuineDescription>();
            // Distribue les items d'abords
            List<ItemDescription> itemDesc = _jeu.items.itemDesc;
            List<RuineDescription> listRuines = new List<RuineDescription>();

            // Utilise la description pour créer les ruines
            for (int y = 0; y < _jeu.terrain.terrainDesc.getHauteur(); y++) {
                for (int x = 0; x < _jeu.terrain.terrainDesc.getLargeur(); x++) {
                    if (_jeu.terrain.terrainDesc.getCellAt(x, y).ruine) {
                        RuineDescription ruinedesc = new RuineDescription();
                        ruinedesc.nom = getNomRuine();
                        ruinedesc.visite = false;
                        ruinedesc.position = new Point(x, y);
                        ruinedesc.item = null;
                        _ruines.Add(ruinedesc.position, ruinedesc);
                        listRuines.Add(ruinedesc);
                    }
                }
            }
            int nbRuines = listRuines.Count;
            int nbItems = itemDesc.Count;
            if (nbRuines > nbItems) {
                throw new ArgumentException("Il n'y a pas assez d'items décrits dans la liste pour le nombre de ruines de la carte");
            }
            // Place les items
            for (int itemNb = 1; itemNb < nbRuines; itemNb++) {
                int idx;
                do {
                    idx = _jeu.rnd.Next(listRuines.Count);
                } while (listRuines[idx].item != null);
                listRuines[idx].item = itemDesc[itemNb];
            }

        }

        /// <summary>
        /// Ne pas modifier le dico en lui même
        /// </summary>
        public Dictionary<Point, RuineDescription> ruines {
            get { return _ruines; }
        }

        /// <summary>
        /// donne un nom de ruine au hasard
        /// </summary>
        /// <returns>nom au hasard</returns>
        string getNomRuine() {
            string[] syllabes = { "kra", "gor", "mi", "lar", "vor", "klech", "pol", "pyr", "marg", "hyt", "loor", "nub", "cry", "wulch", "xam", "ques", "leyf", "jyk" };
            int nbSyllabes = 2 + _jeu.rnd.Next(3);
            string nom = "";
            for (int i = 0; i < nbSyllabes; i++) {
                nom += syllabes[_jeu.rnd.Next(syllabes.Length)];
            }
            return nom.Substring(0, 1).ToUpper() + nom.Substring(1);
        }

        /// <summary>
        /// Fouille une ruine
        /// </summary>
        /// <param name="heros">héros visiteur</param>
        /// <param name="ruine">ruine visitée</param>
        /// <param name="nomEnnemi">en sortie, le nom de l'ennemi rencontré</param>
        /// <param name="nomRecompense">en sortie (si vainqueur), le nom de la récompense</param>
        /// <returns>true si le héros gagne, false s'il meurt</returns>
        /// 
        public bool fouilleRuine(Creature heros, RuineDescription ruine, ref string nomEnnemi, ref string nomRecompense) {
            // Détermine l'ennemi à combattre
            Creature ennemi;
            switch (_jeu.rnd.Next(3)) {
                case 0:
                    ennemi = new Creature(_jeu, Creatures.TYPE_GRIFFON, 0, heros.positionCarte);
                    nomEnnemi = "un griffon";
                    break;
                case 1:
                    ennemi = new Creature(_jeu, Creatures.TYPE_GEANT, 0, heros.positionCarte);
                    nomEnnemi = "un géant des montagnes";
                    break;
                default:
                    ennemi = new Creature(_jeu, Creatures.TYPE_SORCIERE, 0, heros.positionCarte);
                    nomEnnemi = "une sorcière";
                    break;
            }
            Armee aheros = new Armee(_jeu);
            aheros.addCreature(heros);

            Armee aennemi = new Armee(_jeu);
            aennemi.addCreature(ennemi);
            List<Armee> garnison = new List<Armee>();
            garnison.Add(aennemi);

            Combat combat = new Combat(_jeu, aheros, garnison);
            bool herosGagne = combat.combatAuto();

            // Détermine la récompense : item, troupe ou argent
            nomRecompense = "";
            if (herosGagne) {
                ruine.visite = true;
                if (ruine.item != null) {
                    nomRecompense = "l'objet : " + ruine.item.nom;
                    heros.addItem(ruine.item);
                }
                else if (_jeu.rnd.Next(4) == 0) {
                    int montant = _jeu.rnd.Next(1500) + 500;
                    nomRecompense = montant + " pièces d'or";
                    _jeu.factions.getFaction(_jeu.tourFaction).or += montant;
                }
                else {
                    // Alliés
                    Armee allies = new Armee(_jeu);
                    int typeAllie = _jeu.rnd.Next(Creatures.TYPE_ALLIEMAX + 1 - Creatures.TYPE_ALLIEMIN) + Creatures.TYPE_ALLIEMIN;
                    int nbAllie = _jeu.rnd.Next(2) + 2;
                    for (int i = 0; i < nbAllie; i++) {
                        allies.addCreature(new Creature(_jeu, typeAllie, _jeu.tourFaction, heros.positionCarte));
                    }
                    _jeu.addArmee(allies);
                    nomRecompense = nbAllie + " alliés";
                }
            }
            else {
                // Suppression du héros du jeu
                if (_jeu.selectedArmee.getTaille() == 1) {
                    _jeu.removeArmee(_jeu.selectedArmee);
                    _jeu.selectedArmee = null;
                }
                else {
                    _jeu.selectedArmee.removeCreature(heros);
                }
            }

            return herosGagne;
        }

        /// <summary>
        /// Sauvegarde les informations sur les ruines
        /// </summary>
        /// <param name="file">flux de sortie</param>     
        internal void save(System.IO.StreamWriter file) {
            file.WriteLine(_ruines.Count);
            foreach (RuineDescription rd in _ruines.Values) {
                file.WriteLine(rd.position.X);
                file.WriteLine(rd.position.Y);
                file.WriteLine(rd.visite);
                file.WriteLine(rd.nom);
                file.WriteLine(rd.item != null);
                if (rd.item != null) {
                    file.WriteLine(_jeu.items.itemDesc.IndexOf(rd.item));
                }
            }
        }
        /// <summary>
        /// Charge les infos sur les ruines
        /// </summary>
        /// <param name="file">flux d'entrée</param>
        /// <returns>true si ok, false si ko</returns>

        internal bool load(System.IO.StreamReader file) {
            int nbRuines = int.Parse(file.ReadLine());
            if (nbRuines != _ruines.Count) {
                return false;
            }
            // Rince tout
            _ruines.Clear();
            for (int i = 0; i < nbRuines; i++) {
                RuineDescription rd = new RuineDescription();
                rd.position = new Point(int.Parse(file.ReadLine()), int.Parse(file.ReadLine()));
                rd.visite = bool.Parse(file.ReadLine());
                rd.nom = file.ReadLine();
                bool item = bool.Parse(file.ReadLine());
                if (item) {
                    rd.item = _jeu.items.itemDesc[int.Parse(file.ReadLine())];
                }
                _ruines.Add(rd.position, rd);
            }
            return true;
        }
    }
}
