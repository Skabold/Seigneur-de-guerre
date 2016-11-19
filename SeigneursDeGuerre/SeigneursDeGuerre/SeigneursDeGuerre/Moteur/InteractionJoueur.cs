using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SeigneursDeGuerre.Overlays;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    /// Classe gérant les interactions du joueur (actions click gauche)
    /// </summary>
    class InteractionJoueur {

        /// <summary>
        /// Jeu
        /// </summary>
        private Jeu _jeu;

        /// <summary>
        /// Booléen indiquant si on a relache le bouton en changeant de cible
        /// </summary>
        private bool relacheApresChgtCible;
        /// <summary>
        /// cible définie pour déterminer le dble click
        /// </summary>
        private Point? targetSet;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="leJeu">jeu</param>
        public InteractionJoueur(Jeu leJeu) {
            _jeu = leJeu;
            reinitDoubleClick();
        }

        public void reinitDoubleClick() {
            relacheApresChgtCible = false;
            targetSet = null;
        }

        /// <summary>
        /// Click gauche - point d'entrée
        /// </summary>
        /// <param name="mouseX"></param>
        /// <param name="mouseY"></param>
        /// <param name="released">true si cle bouton est relaché avant d'être cliqué</param>
        public void leftClick(int mouseX, int mouseY, bool released) {
            // Si une armée bouge, cliquer = arrêter le mouvement
            if (_jeu.selectedArmeeGO) {
                _jeu.selectedArmee.stop();
                return;
            }
            // L'armée ne bouge pas

            // Détermine sur quoi on clique
            Armee clickA = null;
            VilleDescription clickV = null;
            TerrainDescription.TerrainCell clickC = _jeu.infoCarte.getClicked(mouseX, mouseY, ref clickA, ref clickV);
            int carteX = (mouseX + _jeu.terrain.offsetCarte.X) / _jeu.blockSize;
            int carteY = (mouseY + _jeu.terrain.offsetCarte.Y) / _jeu.blockSize;

            // Déclencheur de mvt : 2nd click
            if (released && relacheApresChgtCible) {
                relacheApresChgtCible = false;
                Point? target = _jeu.selectedArmee == null ? null : _jeu.selectedArmee.moveTarget;
                if (targetSet == target) {
                    _jeu.selectedArmeeGO = true;
                }
            }

            // Sélection, ville ou mouvement.
            if (clickA != null) {
                //Click sur armée
                leftClickOnArmee(clickA, released);
            }
            else if (clickV != null) {
                // Click sur ville
                leftClickOnVille(new Point(carteX, carteY), clickV, released);
            }
            else {
                // Click sur terrain
                leftClickOnCell(new Point(carteX, carteY), released);
            }

            // Déclencheur de mvt : 1er click
            if (released && !relacheApresChgtCible) {
                Point? target = _jeu.selectedArmee == null ? null : _jeu.selectedArmee.moveTarget;
                if (target != null && target != targetSet) {
                    relacheApresChgtCible = true;
                }
                targetSet = target;
            }


        }

        /// <summary>
        /// Click gauche sur une armée
        /// </summary>
        /// <param name="clickA">l'armée</param>
        /// <param name="released">si on a clické ou relaché</param>
        private void leftClickOnArmee(Armee clickA, bool released) {
            // si aucune armée n'est sélectionnée et que c'est notre armée, on la sélectionne
            if ((_jeu.selectedArmee == null) && (clickA.faction == _jeu.tourFaction)) {
                _jeu.selectedArmee = clickA;
            }
            // si une armée est sélectionné et qu'on clique sur une armée amie, c'est un mouvement + merge
            // on calcul une nouvelle trajectoire
            else if ((_jeu.selectedArmee != null) && (clickA.faction == _jeu.tourFaction)) {
                setTarget(clickA.positionCarte);
            }
            // si une armée est sélectionné et qu'on clique sur une armée ennemie, c'est une attaque
            else if ((_jeu.selectedArmee != null) && (clickA.faction != _jeu.tourFaction)) {
                // est ce que l'armée fait partie d'une ville ?
                bool dansVille = false;
                foreach (VilleDescription vd in _jeu.villes.villeDesc) {
                    if (vd.contains(clickA.positionCarte)) {
                        // Attaque d'une ville si on est à coté
                        Point v = vd.positionMap;
                        Point a = _jeu.selectedArmee.positionCarte;
                        if ((a.X >= (v.X - 1)) && (a.X <= (v.X + 2)) && (a.Y >= (v.Y - 1)) && (a.Y <= (v.Y + 2))) {
                            // Oui ! A l'attaaaaaaaaaaaaaaaaaaaaaaaaque !
                            attaqueVille(vd);
                        }
                        dansVille = true;
                        break;
                    }
                }
                // Armée isolée
                if (!dansVille) {
                    if ((Math.Abs(clickA.positionCarte.X - _jeu.selectedArmee.positionCarte.X) <= 1) &&
                         (Math.Abs(clickA.positionCarte.Y - _jeu.selectedArmee.positionCarte.Y) <= 1)) {
                        //A l'attaaaaaaaaaaaaaaaaaaaaaaaaque !
                        attaqueArmee(new List<Armee> { clickA });
                    }
                }

            }
        }

        /// <summary>
        /// Click gauche sur une ville
        /// </summary>
        /// <param name="posCell">position de la map clickée</param>
        /// <param name="clickV">la ville</param>
        /// <param name="released">si on a clické ou relaché</param>
        private void leftClickOnVille(Point posCell, VilleDescription clickV, bool released) {
            // Si aucune armée n'est sélectionnée et que c'est notre ville, ouvre le panneau de gestion
            if ((_jeu.selectedArmee == null) && (clickV.faction == _jeu.tourFaction)) {
                _jeu.addOverlay(new PanneauVille(_jeu, clickV));
            }
            // Si une armée est sélectionnée, on calcul une nouvelle trajectoire si c'est une ville amie
            else if ((_jeu.selectedArmee != null) && (clickV.faction == _jeu.tourFaction)) {
                setTarget(posCell);
            }
            // si c'est une ville ennemie, c'est une attaque
            else if ((_jeu.selectedArmee != null) && (clickV.faction != _jeu.tourFaction)) {
                // raccourci d'écriture pour vérifier si on est à coté
                Point v = clickV.positionMap;
                Point a = _jeu.selectedArmee.positionCarte;
                if ((a.X >= (v.X - 1)) && (a.X <= (v.X + 2)) && (a.Y >= (v.Y - 1)) && (a.Y <= (v.Y + 2))) {
                    // Oui ! A l'attaaaaaaaaaaaaaaaaaaaaaaaaque !
                    attaqueVille(clickV);
                }
            }
        }

        /// <summary>
        /// Click gauche sur terrain
        /// </summary>
        /// <param name="posCell">poisition de la map clickée</param>
        /// <param name="released">si on a clické ou relaché</param>
        private void leftClickOnCell(Point posCell, bool released) {
            // Si aucune armée n'est sélectionnée, ne fait rien
            // Sinon on calcul une nouvelle trajectoire
            if (_jeu.selectedArmee != null) {
                setTarget(posCell);
            }
        }

        /// <summary>
        /// Définie une nouvelle destination (si elle est accessible)
        /// </summary>
        /// <param name="targetPos"></param>
        private void setTarget(Point targetPos) {
            if (_jeu.selectedArmee.getMouvementCost(targetPos) != Creature.MVTINFINI) {
                _jeu.selectedArmee.moveTarget = targetPos;
            }
            else {
                targetSet = null;
            }
        }

        /// <summary>
        /// Focus sur carte sans cliquer. Change le curseur de souris
        /// </summary>
        /// <param name="newMouseX">x souris</param>
        /// <param name="newMouseY">y souris</param>
        public void focusCarte(int mouseX, int mouseY) {
            // Seulement si pas modal & si pas click droit en cours

            Armee focusA = null;
            VilleDescription focusV = null;
            TerrainDescription.TerrainCell focusC = _jeu.infoCarte.getClicked(mouseX, mouseY, ref focusA, ref focusV);
            // Si sélection : peut etre l'épée pour attaquer unité ou ville
            if (_jeu.selectedArmee != null) {
                if (((focusA != null) && (focusA.faction != _jeu.tourFaction)) ||
                    ((focusV != null) && (focusV.faction != _jeu.tourFaction))) {
                    _jeu.curseur.forme = Cursor.FormeCurseur.EPEE;
                }
                else {
                    _jeu.curseur.forme = Cursor.FormeCurseur.FLECHE;
                }
            }
            // Sinon : peut être info ville si ville alliée
            else {
                if ((focusV != null) && (focusV.faction == _jeu.tourFaction)) {
                    _jeu.curseur.forme = Cursor.FormeCurseur.INTERROGATION;
                }
                else {
                    _jeu.curseur.forme = Cursor.FormeCurseur.FLECHE;
                }
            }
        }

        /// <summary>
        /// Attaque d'une armée ennemie -- appelée aussi par l'IA
        /// </summary>
        /// <param name="cible">armée cible</param>
        /// <returns> true si on a gagné</returns>
        public bool attaqueArmee(List<Armee> cible) {            
            Combat combat = new Combat(_jeu, _jeu.selectedArmee, cible);
            // Mode auto ou graphique
            if (_jeu.factions.getFaction(_jeu.tourFaction).humanPlayer || _jeu.factions.getFaction(cible[0].faction).humanPlayer) {
                // mode graphique
                ZoneCombat zc = new ZoneCombat(_jeu, combat, delegate(bool attaquantVainqueur) {
                    if (!attaquantVainqueur) {
                        _jeu.selectedArmee = null;
                    }
                });
                _jeu.addOverlay(zc);

            }
            else {
                bool resCombat = combat.combatAuto();
                if (!resCombat) {
                    // perdu !
                    _jeu.selectedArmee = null;
                }                
            }
            return (_jeu.selectedArmee != null);                
        }



        /// <summary>
        /// Attaque d'une ville ennemie -- appelée aussi par l'IA
        /// </summary>
        /// <returns> true si on a gagné</returns>
        /// <param name="cible">ville cible</param>
        public bool attaqueVille(VilleDescription cible) {
            bool retVal = false;
            // Détermine toutes les armées de la ville
            List<Armee> garnison = new List<Armee>();
            foreach (Armee arme in _jeu.armees) {
                if (cible.contains(arme.positionCarte)) {
                    garnison.Add(arme);
                }
            }
            // Mode auto ou graphique
            if (_jeu.factions.getFaction(_jeu.tourFaction).humanPlayer || _jeu.factions.getFaction(cible.faction).humanPlayer) {
                if (garnison.Count != 0) {
                    // Mode graphique
                    Combat combat = new Combat(_jeu, _jeu.selectedArmee, garnison);
                    ZoneCombat zc = new ZoneCombat(_jeu, combat, delegate(bool attaquantVainqueur) {
                        if (!attaquantVainqueur) {
                            _jeu.selectedArmee = null;
                        }
                        else {
                            // On gagne
                            cible.faction = _jeu.selectedArmee.faction;
                        }
                    });
                    _jeu.addOverlay(zc);
                }
                else {
                    // On gagne
                    cible.faction = _jeu.selectedArmee.faction;
                }
            }
            else {
                if (garnison.Count != 0) {
                    attaqueArmee(garnison);
                }
                // Si il y a encore un attaquant, c'est la victoire !
                if (_jeu.selectedArmee != null) {
                    // Capture la ville
                    cible.faction = _jeu.selectedArmee.faction;
                    retVal = true;
                }
            }
            return retVal;
        }
        
    }
}
