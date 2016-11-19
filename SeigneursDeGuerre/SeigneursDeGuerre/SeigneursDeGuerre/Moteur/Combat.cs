using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    /// Combat entre 2 armées
    /// </summary>
    class Combat {
        private Armee _attaque;
        private Armee _defense;
        private List<Armee> _garnison;
        private int _indexDefense;
        private Jeu _jeu;

        private bool _dansVille;
        private int _defenseVille;

        private int _bonusAttaque;
        private int _bonusDefense;

        /// <summary>
        /// Nombre de coups pour un combat entre 2 créatures avant de décider qui gagne
        /// </summary>
        private const int NB_COUPS_PAR_ROUND = 3;


        public Armee attaque {
            get { return _attaque; }
        }
        public Armee defense {
            get { return _defense; }
        }
        public int bonusAttaque {
            get { return _bonusAttaque; }
        }
        public int bonusDefense {
            get { return _bonusDefense; }
        }


        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="jeu">jeu</param>
        /// <param name="attaque">attaquant</param>
        /// <param name="garnison">défenseurs</param>
        public Combat(Jeu jeu, Armee attaque, List<Armee> garnison) {
            _jeu = jeu;
            _attaque = attaque;
            _garnison = garnison;
            _indexDefense = 0;
            _defense = garnison[0];
            // Ordonne les armées dans l'ordre qui va bien (normalement inutile, mais bon)
            _attaque.ordonnePourCombat();
            _defense.ordonnePourCombat();
            // Détermine si l'armée de défense est dans une ville
            foreach (VilleDescription vd in jeu.villes.villeDesc) {
                if (vd.contains(_defense.positionCarte)) {
                    _dansVille = true;
                    _defenseVille = vd.niveauDefense;
                    break;
                }
            }
            // Calcul du 1er bonus pour l'affichage
            _bonusAttaque = calculBonusAttaque(_attaque.getCreature(0), _attaque);
            _bonusDefense = calculBonusDefense(_defense.getCreature(0), _defense);

        }


        /// <summary>
        /// Combat jusqu'au bout, en mode non graphique
        /// l'armée perdante est retirée du jeu
        /// </summary>
        /// <returns>true si l'attaquant gagne, false sinon</returns>
        public bool combatAuto() {
            bool res = false;
            do {
                do {
                    res = unRound(true);
                } while (!roundSuivant(res));

                Armee perdante = res ? _defense : _attaque;
                _jeu.removeArmee(perdante);
                // Si l'attaquant a perdu, on arrête
                if (!res) {
                    break;
                }
            } while (armeeSuivante(false));
            return res;
        }

        /// <summary>
        /// Simulation du combat jusqu'au bout, mais sans faire de mal aux armées en place
        /// </summary>
        /// <returns>true si l'attaquant gagne, false sinon</returns>
        public bool simulationCombat() {
            // Clone les armées
            Armee sovAtk = _attaque;
            Armee sovDef = _defense;
            _attaque = _attaque.Clone();
            _defense = _defense.Clone();
            bool res;
            do {
                do {
                    res = unRound(false);
                } while (!roundSuivant(res));
                if (!res) {
                    break;
                }
            } while (armeeSuivante(true));

            _attaque = sovAtk;
            _defense = sovDef;
            return res;
        }


        /// <summary>
        /// Transition vers le round suivant : supprime l'armée perdante
        /// </summary>
        /// <param name="resultatRound">résultat du round précédent : true si l'attaquant gagne, false sinon</param>
        /// <returns>true si le combat est fini, false si des rounds supplémentaires sont nécessaires</returns>
        public bool roundSuivant(bool resultatRound) {
            Armee perdante = resultatRound ? _defense : _attaque;
            bool retVal;
            if (perdante.getTaille() == 1) {
                // Une armée a été décimée
                retVal = true;
            }
            else {
                perdante.removeCreature(perdante.getCreature(0));
                retVal = false;
            }
            _bonusAttaque = calculBonusAttaque(_attaque.getCreature(0), _attaque);
            _bonusDefense = calculBonusDefense(_defense.getCreature(0), _defense);
            return retVal;
        }

        /// <summary>
        /// Transition vers la sélection de l'armée suivante
        /// </summary>
        /// <returns>false s'il n'y a plus d'armée à combattre</returns>
        public bool armeeSuivante(bool simulation) {
            _indexDefense++;
            if (_indexDefense < _garnison.Count) {
                if (simulation) {
                    _defense = _garnison[_indexDefense].Clone();
                }
                else {
                    _defense = _garnison[_indexDefense];
                }
                _defense.ordonnePourCombat();
                _bonusDefense = calculBonusDefense(_defense.getCreature(0), _defense);
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Fait un round de combat 
        /// </summary>
        /// <param name="des">si true, tire aux dés, sinon, pure évaluation des stats (pour simu)</param>
        /// <returns>true si l'attaquant gagne, false si c'est le défenseur</returns>
        public bool unRound(bool des) {
            bool retVal = true;
            Creature attaque = _attaque.getCreature(0);
            Creature defense = _defense.getCreature(0);

            // Bonus - recalcul au cas où
            _bonusAttaque = calculBonusAttaque(attaque, _attaque);
            _bonusDefense = calculBonusDefense(defense, _defense);

            if (des) {
                // Attaque
                int scoreAtq = 0;
                int scoreDef = 0;
                for (int i = 0; i < NB_COUPS_PAR_ROUND; i++) {
                    scoreAtq += _jeu.rnd.Next(_bonusAttaque);
                    scoreDef += _jeu.rnd.Next(_bonusDefense);
                }

                if (scoreAtq > scoreDef) {
                    // L'attaquant gagne
                    retVal = true;
                }
                else {
                    // Le défenseur gagne
                    retVal = false;
                }
            }
            else {
                retVal = (_bonusAttaque > _bonusDefense);
            }
            return retVal;
        }

        /// <summary>
        /// Calcule le bonus d'attaque
        /// </summary>
        /// <param name="crea">créature en combat</param>
        /// <param name="arme">groupe dont elle fait partie</param>
        /// <returns>bonus de force en attaque</returns>
        private int calculBonusAttaque(Creature crea, Armee arme) {
            int bonus = 0;
            // +1 par héros dans l'armée
            for (int i = 0; i < arme.getTaille(); i++) {
                Creature copain = arme.getCreature(i);
                if (copain.description.heros) {
                    bonus=bonus+5;
                }
            }
            if ((crea.description.bonusAttaque == CreatureDescription.BonusUrbain.EXTERIEUR) && (!_dansVille)) {
                bonus += 2;
            }
            else if ((crea.description.bonusAttaque == CreatureDescription.BonusUrbain.VILLE) && _dansVille) {
                // Annule le bonus de défense de la ville
                bonus += _defenseVille;
            }
            // la taille compte
            bonus += (2 * arme.getTaille()) / 5;

            // Enfin la force de la créature...
            bonus += crea.description.force;
            return bonus;
        }

        /// <summary>
        /// Calcule le bonus de défense
        /// </summary>
        /// <param name="crea">créature en combat</param>
        /// <param name="arme">groupe dont elle fait partie</param>
        /// <returns>bonus de force en défense</returns>
        private int calculBonusDefense(Creature crea, Armee arme) {
            int bonus = 0;
            // +1 par héros dans l'armée
            for (int i = 0; i < arme.getTaille(); i++) {
                Creature copain = arme.getCreature(i);
                if (copain.description.heros) {
                    bonus++;
                }
            }
            if ((crea.description.bonusDefense == CreatureDescription.BonusUrbain.EXTERIEUR) && (!_dansVille)) {
                bonus += 2;
            }
            else if ((crea.description.bonusDefense == CreatureDescription.BonusUrbain.VILLE) && _dansVille) {
                // Augmente le bonus de défense de 2
                bonus += 2;
            }
            // Si dans ville, bonus de la ville
            if (_dansVille) {
                bonus += _defenseVille;
            }
            // la taille compte
            bonus += (2 * arme.getTaille()) / 5;

            // Enfin la force de la créature...
            bonus += crea.description.force;

            return bonus;
        }

    }
}
