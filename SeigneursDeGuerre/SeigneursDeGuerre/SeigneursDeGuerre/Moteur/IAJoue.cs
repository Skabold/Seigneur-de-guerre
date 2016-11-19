using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SeigneursDeGuerre.Moteur.Trajectoires;
using Microsoft.Xna.Framework;

namespace SeigneursDeGuerre.Moteur {
    class IAJoue {

        public enum Etat {
            PHASE_INIT,
            PHASE_CHOIX_DEST,
            PHASE_MOUVEMENT,
            PHASE_CHOIX_PRODUCTION,
            PHASE_ATTAQUE,
            PHASE_MOUVEMENT_2
        };

        /// <summary>
        /// Jeu
        /// </summary>
        private Jeu _jeu;

        /// <summary>
        /// Etat courant de la réflexion
        /// </summary>
        private Etat _etat;

        /// <summary>
        /// IA Joue
        /// </summary>
        /// <param name="jeu"></param>
        public IAJoue(Jeu jeu) {
            this._jeu = jeu;
            this._etat = Etat.PHASE_INIT;
        }

        /// <summary>
        /// Joue !
        /// </summary>
        private int idxChoixDest = 0;
        private int idxChoixDestVD = 0;
        private int idxChoixDestArmee = 0;

        private List<Armee> armada;
        private int minChemin;
        private int minCheminArmee;
        private Armee armeeCible;
        private VilleDescription vdcible;

        /// <summary>
        /// Réinitialise après chargement
        /// </summary>
        public void reinit() {
            this._etat = Etat.PHASE_INIT;
        }

        public void cEstMonTour() {
            switch (this._etat) {
                case Etat.PHASE_INIT:
                    idxChoixDest = 0;
                    idxChoixDestVD = 0;
                    minChemin = Int32.MaxValue;
                    minCheminArmee = Int32.MaxValue;
                    armeeCible = null;
                    vdcible = null;
                    armada = _jeu.factions.getArmada(_jeu.tourFaction);
                    _etat = Etat.PHASE_CHOIX_DEST;
                    break;

                case Etat.PHASE_CHOIX_DEST:
                    // Itère plusieurs fois par update() sur cette phase
                    for (int iter = 1; iter < 10; iter++) {
                        // Passe en revue toutes les armées et choisit ce qu'il faut faire
                        if (idxChoixDest < armada.Count) {
                            // Pour toutes les armées de l'armada, regarde la ville la plus proche à attataquer
                            if (idxChoixDestVD < _jeu.villes.villeDesc.Length) {
                                choisitCibleArmee(armada[idxChoixDest], _jeu.villes.villeDesc[this.idxChoixDestVD++]);
                                if (minChemin == 0) {
                                    // on ne peut pas faire mieux, on coupe court à la recherche
                                    idxChoixDestVD = _jeu.villes.villeDesc.Length;
                                }
                            }
                            else {
                                // Plus ville, Maintenant passe aux armées cibles
                                if (idxChoixDestArmee < _jeu.armees.Count) {
                                    choisitCibleArmee(armada[idxChoixDest], _jeu.armees[idxChoixDestArmee++]);
                                    if (minCheminArmee == 0) {
                                        // On ne peut pas faire mieux, on coupe court à la recherche
                                        idxChoixDestArmee = _jeu.armees.Count;
                                    }
                                }
                                else {
                                    // Plus de ville, plus d'armées, regarde le plus proche si on a trouvé une ville avant de passer à l'armée à bouger suivante
                                    idxChoixDestVD = 0;
                                    idxChoixDestArmee = 0;

                                    // Choisit-on l'armée ou la ville ?
                                    if (minCheminArmee < minChemin) {
                                        // l'armée cible est plus proche
                                        vdcible = null;
                                    }
                                    else {
                                        // la ville cible est plus proche
                                        armeeCible = null;
                                    }

                                    if (armeeCible != null) {
                                        // La cible est une armée
                                        armada[idxChoixDest].realTarget = armeeCible.positionCarte;
                                        // Se déplace jusqu'à la position de la ville                                   
                                        Point? target = getPointPresDeArmee(armada[idxChoixDest], armeeCible.positionCarte);
                                        // Si l'armée est inaccessible, rentre en ville !
                                        if (target == null) {
                                            choisitCibleVilleRefuge(armada[idxChoixDest]);
                                        }
                                        else if (armada[idxChoixDest].moveTarget != target) {
                                            armada[idxChoixDest].moveTarget = target;
                                        }
                                    }
                                    else if (vdcible != null) {
                                        // La cible est une ville
                                        armada[idxChoixDest].realTarget = vdcible.positionMap;
                                        // Se déplace jusqu'à la position de la ville                                   
                                        Point? target = getPointPresDeVille(armada[idxChoixDest], vdcible.positionMap);
                                        // Si la ville est inaccessible, rentre en ville !
                                        if (target == null) {
                                            choisitCibleVilleRefuge(armada[idxChoixDest]);
                                        }
                                        else if (armada[idxChoixDest].moveTarget != target) {
                                            armada[idxChoixDest].moveTarget = target;
                                        }
                                    }
                                    else {
                                        // Pas de ville ni d'armée à attaquer, se retire
                                        // Trouve la ville amie la plus proche 
                                        choisitCibleVilleRefuge(armada[idxChoixDest]);
                                    }
                                    // Passe à l'armée suivante
                                    minChemin = Int32.MaxValue;
                                    minCheminArmee = Int32.MaxValue;
                                    armeeCible = null;
                                    vdcible = null;
                                    idxChoixDest++;
                                }
                            }
                        }
                        else {
                            _etat = Etat.PHASE_CHOIX_PRODUCTION;
                            break;
                        }
                    }
                    break;

                case Etat.PHASE_CHOIX_PRODUCTION:
                    // Passe en revue toutes les villes et choisit ce qu'il faut construire
                    foreach (VilleDescription vd in _jeu.villes.villeDesc) {
                        if (vd.faction == _jeu.tourFaction) {
                            choisirProduction(vd);
                        }
                    }
                    _etat = Etat.PHASE_MOUVEMENT;
                    _jeu.selectedArmee = null;
                    _jeu.selectedArmeeGO = false;
                    break;

                case Etat.PHASE_MOUVEMENT:
                case Etat.PHASE_MOUVEMENT_2:
                    // Trouve la prochaine armée qui peut bouger
                    if ((_jeu.selectedArmee != null) && !_jeu.selectedArmeeGO) {
                        _jeu.selectedArmee = null;
                        armada = _jeu.factions.getArmada(_jeu.tourFaction);
                    }
                    if ((_jeu.selectedArmee == null)) {
                        foreach (Armee armee in armada) {
                            //armee.reinitTrajectoire();
                            if (deplaceArmee(armee)) {
                                _jeu.selectedArmee = armee;
                                _jeu.selectedArmeeGO = true;
                                break;
                            }
                        }
                        // Si rien n'est sélectionné, phase d'après
                        if (_jeu.selectedArmee == null) {
                            if (_etat == Etat.PHASE_MOUVEMENT) {
                                _etat = Etat.PHASE_ATTAQUE;
                            }
                            else {
                                // Fini, fin du tour
                                // reinit pour la suite
                                _etat = Etat.PHASE_INIT;
                                _jeu.finTour();
                            }
                            idxChoixDest = 0;
                        }
                    }
                    break;
                case Etat.PHASE_ATTAQUE:
                    // Si un combat est en cours ; on continue;
                    if (_jeu.getTopOverlay().modalOverlay != Overlays.Overlay.ModalOverlay.COMBAT) {
                        armada = _jeu.factions.getArmada(_jeu.tourFaction);
                        // Si on peut attaquer une ville, on le fait.
                        if (idxChoixDest < armada.Count) {
                            // Peut-on attaquer ?
                            Point? vqm = armada[idxChoixDest].realTarget;
                            if (vqm != null) {
                                Point v = vqm.Value;
                                Point a = armada[idxChoixDest].positionCarte;

                                // Attaque de ville ou d'armée ?
                                VilleDescription villeAttaquee = null;
                                foreach (VilleDescription vd in _jeu.villes.villeDesc) {
                                    if (vd.positionMap == v) {
                                        villeAttaquee = vd;
                                        break;
                                    }
                                }
                                if (villeAttaquee != null) {
                                    if ((a.X >= (v.X - 1)) && (a.X <= (v.X + 2)) && (a.Y >= (v.Y - 1)) && (a.Y <= (v.Y + 2))) {
                                        // Oui ! A l'attaaaaaaaaaaaaaaaaaaaaaaaaque !
                                        _jeu.terrain.zoomSur(armada[idxChoixDest].positionEcran);
                                        attaqueVille(armada[idxChoixDest], villeAttaquee);
                                    }
                                }
                                else {
                                    // Attaque d'armée ?
                                    foreach (Armee cible in _jeu.armees) {
                                        if (cible.positionCarte == v && cible.faction != _jeu.tourFaction) {
                                            // oui ! attaque l'armée !
                                            _jeu.terrain.zoomSur(armada[idxChoixDest].positionEcran);
                                            attaqueArmee(armada[idxChoixDest], cible);
                                            break;
                                        }
                                    }
                                }
                            }

                            idxChoixDest++;
                        }
                        else {
                            // phase de mouvement suivante (jusque dans les villes prises)
                            _etat = Etat.PHASE_MOUVEMENT_2;
                        }
                    }
                    break;
            }
        }



        /// <summary>
        /// L'armée doit rentrer  dans la ville la plus proche
        /// </summary>
        /// <param name="armee"></param>
        private void choisitCibleVilleRefuge(Armee armee) {
            int minChemin = Int32.MaxValue;
            VilleDescription vdcible = null;
            foreach (VilleDescription vd in _jeu.villes.villeDesc) {
                if (vd.faction == _jeu.tourFaction) {
                    Point cible = vd.positionMap;
                    if (armee.positionCarte == cible) {
                        minChemin = 0;
                        vdcible = vd;
                        break;
                    }
                    else {
                        armee.moveTarget = cible;
                        if ((armee.trajectoire != null) && (armee.trajectoire.chemin != null)) {
                            int lenChem = armee.trajectoire.chemin.Count;
                            if (lenChem != 0 && lenChem < minChemin) {
                                minChemin = lenChem;
                                vdcible = vd;
                            }
                        }
                    }
                }
            }
            if (vdcible != null) {
                armee.moveTarget = vdcible.positionMap;
            }
            else {
                armee.moveTarget = null;
            }
        }

        /// <summary>
        /// L'armée attaque la ville
        /// </summary>
        /// <param name="armee">armée</param>
        /// <param name="v">ville</param>
        private void attaqueVille(Armee armee, VilleDescription vd) {
            _jeu.selectedArmee = armee;
            if (_jeu.interactionJoueur.attaqueVille(vd)) {
                // Si on a gagné, se déplace dans la ville si on a encore du mouvement
                armee.moveTarget = vd.positionMap;
            }
        }

        /// <summary>
        /// L'armée attaque l'armée ennemie
        /// </summary>
        /// <param name="armee">armée</param>
        /// <param name="cible">armée ennemie</param>

        private void attaqueArmee(Armee armee, Armee cible) {
            _jeu.selectedArmee = armee;
            List < Armee > cibles = new List<Armee>();
            cibles.Add(cible);
            if (_jeu.interactionJoueur.attaqueArmee(cibles)) {
                // Si on a gagné, se déplace à la place de l'armée si on a encore du mouvement
                armee.moveTarget = cible.positionCarte;
            }            
        }


        /// <summary>
        /// Trouve un point accessible pour l'armée près de l'armée cible
        /// </summary>
        /// <param name="posArmee">position de l'armée souhaitée</param>
        /// <returns>le point cible ou null si pas trouvé</returns>

        private Point? getPointPresDeArmee(Armee armee, Point posArmee) {
            // Prend le pt le + proche parmi les 8 points filtrés selon accessiblité
            List<Point> choix = new List<Point>();
            for (int y = -1; y <= 1; y++) {
                for (int x = -1; x <= 1; x++) {
                    // ne fait que les abords de la ville
                    if (!((x==0)&&(y==0))) {
                        Point tente = new Point(posArmee.X + x, posArmee.Y + y);
                        if (armee.getMouvementCost(tente) != Creature.MVTINFINI) {
                            choix.Add(tente);
                        }
                    }
                }
            }
            // Ville inaccessible
            if (choix.Count == 0) {
                return null;
            }
            else {
                // prend le point le plus proche au sens de la distance X/Y (pas forcément le plus proche mais bon...)
                int minDistance = Int32.MaxValue;
                int minIndice = 0;
                for (int i = 0; i < choix.Count; i++) {
                    int d = Math.Max(Math.Abs(armee.positionCarte.X - choix[i].X), Math.Abs(armee.positionCarte.Y - choix[i].Y));
                    if (minDistance > d) {
                        minDistance = d;
                        minIndice = i;
                    }
                }
                return choix[minIndice];
            }
        }

        /// <summary>
        /// Trouve un point accessible pour l'armée près de la ville
        /// </summary>
        /// <param name="posVille">position de la ville souhaitée</param>
        /// <returns>le point cible ou null si pas trouvé</returns>
        private Point? getPointPresDeVille(Armee armee, Point posVille) {
            // Prend un point au hasard parmi les 12 points filtrés selon accessiblité
            List<Point> choix = new List<Point>();
            for (int y = -1; y <= 2; y++) {
                for (int x = -1; x <= 2; x++) {
                    // ne fait que les abords de la ville
                    if (!((x >= 0) && (x <= 1) && (y >= 0) && (y <= 1))) {
                        Point tente = new Point(posVille.X + x, posVille.Y + y);
                        if (armee.getMouvementCost(tente) != Creature.MVTINFINI) {
                            choix.Add(tente);
                        }
                    }
                }
            }
            // Ville inaccessible
            if (choix.Count == 0) {
                return null;
            }
            else {
                // prend le point le plus proche au sens de la distance X/Y (pas forcément le plus proche mais bon...)
                int minDistance = Int32.MaxValue;
                int minIndice = 0;
                for (int i = 0; i < choix.Count; i++) {
                    int d = Math.Max(Math.Abs(armee.positionCarte.X - choix[i].X),Math.Abs(armee.positionCarte.Y - choix[i].Y));
                    if (minDistance > d) {
                        minDistance = d;
                        minIndice = i;
                    }
                }
                return choix[minIndice];
            }
        }

        /// <summary>
        /// Déplacement des armées qui le peuvent
        /// </summary>
        /// <param name="armee">armee</param>
        /// <returns>true si cette armée bouge, false sinon</returns>
        private bool deplaceArmee(Armee armee) {
            return (armee.moveTarget != armee.positionCarte) && (armee.trajectoire != null) && (armee.trajectoire.chemin.Count != 0) &&
                (armee.trajectoire.chemin[0].tourCourant) && (armee.getMouvementRestantMin() > 0);
        }

        /// <summary>
        /// Choisit la production
        /// </summary>
        /// <param name="vd"></param>
        private void choisirProduction(VilleDescription vd) {
            // Je prend au hasard !
            vd.productionCourante = _jeu.rnd.Next(vd.typeCreatures.Length);
        }

        /// <summary>
        /// Déplacement des armées : met à jour vdcible avec la meilleure cible pour une armée & une armée ennemie choisie
        /// </summary>
        /// <param name="armee">armee</param>
        /// <param name="cible">armée ciblée</param>
        private void choisitCibleArmee(Armee armee, Armee cible) {
            // Choisit la ville la plus proche et l'attaque si on peut gagner
            if (cible.faction != _jeu.tourFaction) {
                // Regarde d'abord si elle n'est pas trop loin... Taille max
                const int DISTANCE_MAX = 25;
                if (Math.Max(Math.Abs(cible.positionCarte.X - armee.positionCarte.X), Math.Abs(cible.positionCarte.Y - armee.positionCarte.Y)) < DISTANCE_MAX) {
                    // Regarde ensuite si elle est dans une ville, si oui on l'ignore car c'est pris en compte dans le choix des cibles villes
                    bool dansVille = false;
                    foreach (VilleDescription vd in _jeu.villes.villeDesc) {
                        if (vd.contains(cible.positionCarte)) {
                            dansVille = true;
                            break;
                        }
                    }
                    if (!dansVille) {
                        // Regarde d'abord si on gagnerait pour éviter de calculer des trajectoires pour rien
                        // peut-on gagner le combat ?
                        bool gagne;
                        List<Armee> garnison = new List<Armee>();
                        garnison.Add(cible);
                        Combat combat = new Combat(_jeu, armee, garnison);
                        gagne = combat.simulationCombat();

                        if (gagne) {
                            // Accessible ?
                            // choisit un point accessible près de la cible 
                            Point? pcible = getPointPresDeArmee(armee, cible.positionCarte);
                            if (pcible != null) { // sinon inaccessible
                                // Déjà là ?
                                if (armee.positionCarte == pcible) {
                                    minCheminArmee = 0;
                                    armeeCible = cible;
                                }
                                else {
                                    armee.moveTarget = pcible;
                                    if ((armee.trajectoire != null) && (armee.trajectoire.chemin != null)) {
                                        int lenChem = armee.trajectoire.chemin.Count;
                                        if (lenChem != 0 && lenChem < minCheminArmee) {
                                            if (minCheminArmee > lenChem) {
                                                minCheminArmee = lenChem;
                                                armeeCible = cible;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }



        /// <summary>
        /// Déplacement des armées : met à jour vdcible avec la meilleure cible pour une armée & une ville choisie
        /// </summary>
        /// <param name="armee">armee</param>
        /// <param name="vd">ville</param>
        private void choisitCibleArmee(Armee armee, VilleDescription vd) {
            // Choisit la ville la plus proche et l'attaque si on peut gagner
            if (vd.faction != _jeu.tourFaction) {
                // Regarde d'abord si elle n'est pas trop loin... Taille max
                const int DISTANCE_MAX = 25;
                if (Math.Max(Math.Abs(vd.positionMap.X - armee.positionCarte.X), Math.Abs(vd.positionMap.Y - armee.positionCarte.Y)) < DISTANCE_MAX) {

                    // Regarde d'abord si on gagnerait pour éviter de calculer des trajectoires pour rien
                    // peut-on gagner le combat ?
                    List<Armee> garnison = new List<Armee>();
                    foreach (Armee arme in _jeu.armees) {
                        if (vd.contains(arme.positionCarte)) {
                            garnison.Add(arme);
                        }
                    }
                    bool gagne;
                    if (garnison.Count != 0) {
                        Combat combat = new Combat(_jeu, armee, garnison);
                        gagne = combat.simulationCombat();
                    }
                    else {
                        gagne = true;
                    }
                    if (gagne) {
                        // Accessible ?
                        // choisit un point accessible près de la ville 
                        Point? cible = getPointPresDeVille(armee, vd.positionMap);
                        if (cible != null) { // sinon inaccessible
                            // Déjà là ?
                            if (armee.positionCarte == cible) {
                                minChemin = 0;
                                vdcible = vd;
                            }
                            else {
                                armee.moveTarget = cible;
                                if ((armee.trajectoire != null) && (armee.trajectoire.chemin != null)) {
                                    int lenChem = armee.trajectoire.chemin.Count;
                                    if (lenChem != 0 && lenChem < minChemin) {
                                        if (minChemin > lenChem) {
                                            minChemin = lenChem;
                                            vdcible = vd;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
