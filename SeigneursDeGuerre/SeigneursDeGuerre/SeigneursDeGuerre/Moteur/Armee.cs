using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SeigneursDeGuerre.Moteur.Trajectoires;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    /// Un ensemble de 1 à 8 creatures assemblées en un même endroit
    /// </summary>
    class Armee {
        /// <summary>
        /// Le jeu
        /// </summary>
        private Jeu _jeu;

        /// <summary>
        /// Taille max d'une armée
        /// </summary>
        public const int MAX_TAILLE = 8;
        /// <summary>
        /// Position commune de l'armée, coordoonnées écran
        /// </summary>
        private Point _positionEcran;
        /// <summary>
        /// Position officielle sur la carte
        /// </summary>
        private Point _positionCarte;
        /// <summary>
        /// Faction commune de l'armée
        /// </summary>
        private int _faction;
        /// <summary>
        /// Troupe
        /// </summary>
        private List<Creature> _troupe;


        /// <summary>
        /// Créature de la troupe la plus forte (ou héros) à afficher
        /// </summary>
        private Creature _proue;

        /// <summary>
        /// Destination de l'armée sur la carte (coordonnées de carte). Null si pas de destination.
        /// </summary>
        private Point? _moveTarget;

        /// <summary>
        /// Position réelle de l'armée (genre ville à attaquer) - utilisé par l'IA seulement
        /// </summary>
        private Point? _realTarget;

        /// <summary>
        /// Chemin courant
        /// </summary>
        private Trajectoire _trajectoire;

        /// <summary>
        /// Si black listé, ne revient plus lors de la sélection d'après
        /// </summary>
        private bool _blackList;

        // Constructeur -----------------------------------------------------

        public Armee(Jeu leJeu) {
            _jeu = leJeu;
            _troupe = new List<Creature>();
            _moveTarget = null;
            _trajectoire = null;
            _blackList = false;
        }

        // Accesseurs & modifieurs -------------------------------------------------------

        /// <summary>
        /// Récupère l'indice dans la troup
        /// </summary>
        /// <param name="index">indice</param>
        /// <returns>la créature</returns>
        public Creature getCreature(int index) {
            return _troupe[index];
        }

        /// <summary>
        /// Taille de l'armée
        /// </summary>
        /// <returns></returns>
        public int getTaille() {
            return _troupe.Count;
        }

        /// <summary>
        /// Position commune
        /// </summary>
        public Point positionEcran {
            get { return _positionEcran; }
            set {
                foreach (Creature crea in _troupe) {
                    crea.positionEcran = value;
                }
                _positionEcran = value;
            }
        }

        /// <summary>
        /// Position commune
        /// </summary>
        public Point positionCarte {
            get { return _positionCarte; }
            set {
                foreach (Creature crea in _troupe) {
                    crea.positionCarte = value;
                }
                _positionCarte = value;
                // fixe la position écran
                positionEcran = new Point(_positionCarte.X *_jeu.blockSize, _positionCarte.Y * _jeu.blockSize);
            }
        }

        /// <summary>
        /// Cible sur carte
        /// </summary>
        public Point? moveTarget {
            get { return _moveTarget; }
            set { 
                // Si on change de cible, recalcule une trajectoire
                if ((_moveTarget != value) && (_positionCarte != value)) {
                    if (value.HasValue && (getMouvementCost(value.Value) != Creature.MVTINFINI)) {
                        _moveTarget = value;
                        _trajectoire = new Trajectoire(_jeu, this);
                    }
                    else {
                        _moveTarget = null;
                    }
                }
            }
        }

        /// <summary>
        /// Cible sur carte
        /// </summary>
        public Point? realTarget {
            get { return _realTarget; }
            set {_realTarget= value;}
        }

        
        /// <summary>
        /// Faction commune. Ne peut pas changer !
        /// </summary>
        public int faction {
            get { return _faction; }
        }

        public Creature proue {
            get { return _proue; }
        }

        public bool blackListed {
            get { return _blackList; }
            set { _blackList = value; }
        }
        /// <summary>
        /// Dépenses totale pour cette armée
        /// </summary>
        public int totalDep {
            get {
                int total = 0;
                foreach (Creature crea in _troupe) {
                    total += crea.cout;
                }
                return total;
            }
        }
        
        /// <summary>
        /// L'armée vole-t-elle ?
        /// </summary>
        public bool vol {
            get {
                // Vol = le héros vole, toute l'armée vole, ou toute l'armée sauf le héros vol, mais pas le héros qui ne vole pas seul
                bool allVol = true;
                bool auMoins1 = false;
                bool herosVol = false;
                foreach (Creature crea in _troupe) {
                    if (!crea.description.heros) {
                        allVol &= crea.vol;
                        auMoins1 = true;
                    }
                    else if (crea.vol) {
                        herosVol = true;
                        break;
                    }
                }
                return herosVol || (allVol && auMoins1);
             }
        }
        /// <summary>
        /// L'armée nage-t-elle ?
        /// </summary>
        public bool nage {
            get {
                // Nage = au moins 1 nage
                foreach (Creature crea in _troupe) {
                    if (crea.nage) {
                        return true;
                    }
                }
                return false;
             }
        }

        /// <summary>
        /// donne le mouvement minimum des troupes
        /// </summary>
        /// <returns></returns>
        public int getMouvementRestantMin() {
            int mvt = Int32.MaxValue;
            foreach (Creature crea in _troupe) {
                // Si on a un bateau, c'est lui qui compte car il porte les autres !
                if (crea.nage) {
                    if (crea.mouvementCourant < mvt) {
                        mvt = crea.mouvementCourant;
                    }                   
                }
            }
            // Si personne ne nage, on prend le mouvement min des unités
            if (mvt == Int32.MaxValue) {
                foreach (Creature crea in _troupe) {
                    if (crea.mouvementCourant < mvt) {
                        mvt = crea.mouvementCourant;
                    }
                }
            }
            return mvt;
        }

        /// <summary>
        /// Ajoute une créature à l'armée
        /// </summary>
        /// <param name="crea"></param>
        public void addCreature(Creature crea) {
            _troupe.Add(crea);
            _faction = crea.faction;
            _positionEcran = crea.positionEcran;
            _positionCarte = crea.positionCarte;
            // Détermine la figure de proue
            determineProue();
            ordonnePourCombat();
        }
        /// <summary>
        /// Calcule la figure de proue
        /// </summary>
        private void determineProue() {
            int force = -1;
            foreach (Creature troop in _troupe) {
                if ((troop.forcePourProue() > force)) {
                    force = troop.forcePourProue();
                    _proue = troop;
                }
            }
        }

        
        /// <summary>
        /// Ordonne les troupes dans l'ordre pour un combat (force inverse)
        /// </summary>
        public void ordonnePourCombat() {
            _troupe.Sort();
        }

        /// <summary>
        /// Enlève une créature à l'armée
        /// </summary>
        /// <param name="crea"></param>
        ///             
        
        static bool stopsi1 = false;
        public void removeCreature(Creature crea) {

            _troupe.Remove(crea);
            // Détermine la figure de proue
            determineProue();
            ordonnePourCombat();
        }


        public Trajectoire trajectoire {
            get { return _trajectoire; }
        }

        /// <summary>
        /// Déplace l'armée (appelé pendant update)
        /// </summary>
        /// <param name="gameTime"></param>
        private float interpolation = 0f;
        public void go(GameTime gameTime) {
            // Suit la trajectoire
            if ((_trajectoire != null) && (_trajectoire.chemin.Count != 0) &&
                (_trajectoire.chemin[0].tourCourant) && (this.getMouvementRestantMin() > 0)) { 
                Point nextPos = _trajectoire.chemin[0].point;
                // Interpolation entre positions
                interpolation += ((float)gameTime.ElapsedGameTime.Milliseconds) / 100f;
                positionEcran = new Point((int)((float)_jeu.blockSize * ((float)positionCarte.X + interpolation * (float)(nextPos.X - positionCarte.X))),
                                          (int)((float)_jeu.blockSize * ((float)positionCarte.Y + interpolation * (float)(nextPos.Y - positionCarte.Y))));

                // Centre sur créature
                _jeu.terrain.zoomSur(positionEcran);

                if (interpolation >= 1) {
                    positionCarte = nextPos;
                    // prépare la suite
                    _trajectoire.chemin.RemoveAt(0);
                    // enlève le mouvement adéquat
                    foreach (Creature crea in _troupe) {
                        crea.mouvementCourant -= crea.getMouvementCost(positionCarte, this);
                    }
                    interpolation = 0;
                }
            }
            else {                
                _jeu.selectedArmeeGO = false;
                // Si à la position cible il y a une armée amie, les fusionne
                // fusion des armées si une armée est déjà à cet emplacement                
                foreach (Armee autre in _jeu.armees) {
                    if ((autre != this) && (autre.positionCarte == positionCarte)) {
                        // Que si il y a la place
                        if ((autre.getTaille() + getTaille()) <= Armee.MAX_TAILLE) {
                            ajouteTroupes(autre);
                            // Suppression de autre
                            _jeu.removeArmee(autre);
                        }
                        break;
                    }
                }

            }
        }

        /// <summary>
        /// Arrête le mouvement
        /// </summary>
        public void stop() {
            if ((_trajectoire.chemin.Count != 0) &&
               (_trajectoire.chemin[0].tourCourant)) {

                positionCarte = _trajectoire.chemin[0].point;
                interpolation = 0;
                _jeu.selectedArmeeGO = false;
                // enlève le mouvement adéquat
                foreach (Creature crea in _troupe) {
                    crea.mouvementCourant -= crea.getMouvementCost(positionCarte, this);
                }

            }
        }
        /// <summary>
        /// Le cout d'une armée est le coût maximal (sauf vol et nage de héros)
        /// </summary>
        /// <param name="target">point visé</param>
        /// <returns></returns>
        public int getMouvementCost(Point target) {
            int maxCost = 0;
            foreach (Creature crea in _troupe) {
                int cost = crea.getMouvementCost(target, this);
                if (cost > maxCost) {
                    maxCost = cost;
                }
            }
            return maxCost;
        }

        /// <summary>
        /// Le cout d'une armée est le coût maximal (sauf vol & nage de héros)
        /// </summary>
        /// <param name="target">point visé</param>
        /// <returns></returns>
        public int getMouvementCost(TerrainDescription.TerrainCell cell) {
            int maxCost = 0;
            foreach (Creature crea in _troupe) {
                int cost = crea.getMouvementCost(cell, this);
                if (cost > maxCost) {
                    maxCost = cost;
                }
            }
            return maxCost;
        }


        /// <summary>
        /// Réinitialise le mouvement en début de tour
        /// </summary>
        public void reinitMouvement() {
            foreach (Creature crea in _troupe) {
                crea.mouvementCourant = crea.mouvement;             
            }            
        }
        /// <summary>
        /// Réinitialise la trajectoire
        /// </summary>
        public void reinitTrajectoire() {
            // Refait trajectoire
            if (_trajectoire != null) {
                _trajectoire.calculTrajectoire();
            }
        }

        /// <summary>
        /// Trouve une position libre et accessible autour du point donné
        /// (pas saturé d'armées)
        /// </summary>
        /// <param name="autourDe">point de départ</param>
        /// <param name="totalementLibre">si true, ne mélange pas les armées amies</param>
        /// <param name="tresProche">si true, retourne null si on n'a pas plu pacer l'armée sur une case adjacente</param>
        /// <returns></returns>
        public Point? getPositionLibre(Point autourDe, bool totalementLibre, bool tresProche) {
            
            // essaye de façon "circulaire" autour de autourDe
            int rayonM = 0; // rayon "moins"
            int rayonP = 0; // rayon "plus"            
            while (true) {
                for (int x = autourDe.X - rayonM; x <= autourDe.X + rayonP; x++) {
                    for (int y = autourDe.Y - rayonM; y <= autourDe.Y + rayonP; y++) {
                        if ((x >= 0) && (y >= 0) && (x < _jeu.terrain.terrainDesc.getLargeur()) && (y < _jeu.terrain.terrainDesc.getHauteur())) {
                            Point pt = new Point(x, y);
                            if (isPlaceLibre(pt, totalementLibre)) {
                                return pt;
                            }
                        }
                    }
                }
                // Change le rayon. On finira bien par tomber sur un emplacement libre 
                if ((rayonM + rayonP) % 2 == 0) {
                    rayonP++;
                }
                else {
                    rayonM++;
                }
                // Si pas de place proche du point, retourne null pour annuler l'action (si c'est autorisé)
                if (tresProche && ((rayonP >= 2) || (rayonM >= 2))) {
                    // Pas de place                    
                    return null;
                }
                if (rayonM > _jeu.terrain.terrainDesc.getLargeur()) {
                    throw new SdGException("Carte pourrie : pas de place pour placer toutes les armées !");
                }
            }
        }

        /// <summary>
        /// indique si le point de la carte est libre pour cette armée (pas inaccessible et pas saturé d'armées)
        /// </summary>
        /// <param name="place">emplacement considéré</param>
        /// <param name="totalementLibre">si true, ne mélange pas les armées amies</param>
        /// <returns>true si libre</returns>
        public bool isPlaceLibre(Point place, bool totalementLibre) {
            // Si inaccessible, pas bon
            if (getMouvementCost(place) == Creature.MVTINFINI) {
                return false;
            }
            // Si ville ennemie, pas bon
            foreach (VilleDescription vd in _jeu.villes.villeDesc) {
                if (vd.contains(place) && (vd.faction != this.faction)) {
                    return false;
                }
            }
            // Armée à cet emplacement ?
            foreach (Armee armee in _jeu.armees) {
                if ((armee != this) && (armee.positionCarte == place)) {
                    // Ennemie : pas bon
                    if (armee.faction != faction) {
                        return false;
                    }
                    // Amie : taille totale < 8
                    return (!totalementLibre) && (armee.getTaille() + getTaille() <= MAX_TAILLE);
                }
            }
            // sinon ok
            return true;
        }

        // Fusionne une armée avec this
        public void ajouteTroupes(Armee autre) {
            if ((autre.getTaille() + getTaille()) > MAX_TAILLE) {
                throw new SdGException("Bug : tentative de fusion d'armées volumineuses !");
            }
            foreach (Creature crea in autre._troupe) {
                this.addCreature(crea);
            }            
        }
        /// <summary>
        /// Clone l'armée (uniquement les troupes, pas la trajectoire, et ne clone pas les créatures)
        /// </summary>
        /// <returns></returns>
        public Armee Clone() {
            Armee clone = new Armee(_jeu);
            foreach (Creature crea in _troupe) {
                clone.addCreature(crea);
            }            
            return clone;
        }

        /// <summary>
        /// Affiche une armée
        /// Appelé au sein d'un spriteBatch begin/end
        /// </summary>
        /// <param name="spriteBatch">spriteBatch du jeu</param>
        /// <param name="GraphicsDevice">gd du jeu</param>
        /// <param name="gameTime">temps du jeu</param>
        /// <param name="z">z </param>
        /// 
        public void draw(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime, float z) {

            // Drapeau - derrière
            int marge = (_jeu.blockSize - Creatures.CREATURE_SIZE);
            int drapeauX = _positionEcran.X - _jeu.terrain.offsetCarte.X;
            int drapeauY = _positionEcran.Y - _jeu.terrain.offsetCarte.Y - marge;

            _jeu.factions.drawFlag(spriteBatch, GraphicsDevice, gameTime, drapeauX, drapeauY, _troupe.Count, _faction, z);

            // Proue - on considère que l'armée fait blocSize mais ce n'est pas le cas => décalage
            _jeu.creatures.draw(spriteBatch, GraphicsDevice, gameTime, 
                _positionEcran.X + (_jeu.blockSize - Creatures.CREATURE_SIZE) / 2, _positionEcran.Y + (_jeu.blockSize - Creatures.CREATURE_SIZE) / 2, z - 0.01f, _proue.typeCreature);
            
            // Sélection - si c'est l'armée sélectionnée, ajoute un cadre et affiche la trajectoire
            if (_jeu.selectedArmee == this) {
                // intensité couleur (animation)
                float intensite = 0.66f + (float)Math.Cos(2.0 * Math.PI * (double)((int)gameTime.TotalGameTime.TotalMilliseconds % 2000) / 2000.0) * 0.33f;
                spriteBatch.Draw(_jeu.texSelection, new Rectangle(drapeauX - marge ,drapeauY,
                    _jeu.blockSize + marge * 2, _jeu.blockSize + marge * 2), _jeu.factions.getFaction(faction).couleur * intensite);

                // Trajectoire (si humain)
                if ((_trajectoire != null) && (_jeu.factions.getFaction(_jeu.tourFaction).humanPlayer)) {
                    foreach (Trajectoire.Etape etape in _trajectoire.chemin) {
                        spriteBatch.Draw(etape.tourCourant ? _jeu.texMarqueur : _jeu.texMarqueurKO,
                                         new Rectangle(etape.point.X * _jeu.blockSize - _jeu.terrain.offsetCarte.X + _jeu.blockSize/4,
                                             etape.point.Y * _jeu.blockSize - _jeu.terrain.offsetCarte.Y + _jeu.blockSize/4, _jeu.blockSize/2, _jeu.blockSize/2),
                                             null, Color.White, 0, Vector2.Zero, SpriteEffects.None, z - 0.02f);
                    }
                }

            }
        }
    }
}
