using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SeigneursDeGuerre.Moteur;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace SeigneursDeGuerre.Overlays {
    /// <summary>
    /// Panneau de contrôle principal
    /// </summary>
    class PanneauControle : Overlay {

        private const int LARGBTN = 148;
        private Creature _focusCreature;
        private HashSet<Creature> _group;
        private Armee _groupOwner;

        private static Texture2D _texContinuer;
        private static Texture2D _texDeselection;
        public static Texture2D _texFintour;
        private static Texture2D _texFouiller;
        private static Texture2D _texPasser;
        private static Texture2D _texSuivant;
        private static Texture2D _texScinder;


        public PanneauControle(Jeu _jeu)
            : base(_jeu, Position.BAS_GAUCHE, LARGBTN * 6 + 16, 160, null, null) {

            // Ajout de boutons ----

            Bouton boutonArmeeSuivante = new Bouton(_jeu, xoverlay + 16 + LARGBTN * 0, yoverlay + 16, "   Suivant ", _texSuivant,_jeu.isoFont);
            boutonArmeeSuivante.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                if (released && _jeu.getTopOverlay().modalOverlay == ModalOverlay.AUCUN) {
                    boutonArmeeSuivanteClick();
                }
            };
            _controles.Add(boutonArmeeSuivante);

            Bouton boutonSuiteMouvement = new Bouton(_jeu, xoverlay + 16 + LARGBTN * 1, yoverlay + 16, "   Continue",_texContinuer, _jeu.isoFont);
            boutonSuiteMouvement.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                if (released && _jeu.getTopOverlay().modalOverlay == ModalOverlay.AUCUN) {
                    boutonSuiteMouvementClick();
                }
            };
            _controles.Add(boutonSuiteMouvement);

            Bouton boutonDeselection = new Bouton(_jeu, xoverlay + 16 + LARGBTN * 2, yoverlay + 16, "   Déselect",_texDeselection, _jeu.isoFont);
            boutonDeselection.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                if (released && _jeu.getTopOverlay().modalOverlay == ModalOverlay.AUCUN) {
                    boutonDeselectionClick();
                }
            };
            _controles.Add(boutonDeselection);

            Bouton boutonOublieArmee = new Bouton(_jeu, xoverlay + 16 + LARGBTN * 3, yoverlay + 16, "    Passer ", _texPasser,_jeu.isoFont);
            boutonOublieArmee.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                if (released && _jeu.getTopOverlay().modalOverlay == ModalOverlay.AUCUN) {
                    boutonOublieArmeeClick();
                }
            };
            _controles.Add(boutonOublieArmee);

            Bouton boutonFouillerRuine = new Bouton(_jeu, xoverlay + 16 + LARGBTN * 4, yoverlay + 16, "   Fouille ",_texFouiller, _jeu.isoFont);
            boutonFouillerRuine.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                if (released && _jeu.getTopOverlay().modalOverlay == ModalOverlay.AUCUN) {
                    boutonFouillerRuineClick();
                }
            };
            _controles.Add(boutonFouillerRuine);

            Bouton boutonFinTour = new Bouton(_jeu, xoverlay + 16 + LARGBTN * 5, yoverlay + 16, "   Fin Tour",_texFintour, _jeu.isoFont);
            boutonFinTour.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                if (released && _jeu.getTopOverlay().modalOverlay == ModalOverlay.AUCUN) {
                    boutonFinTourClick();
                }
            };
            _controles.Add(boutonFinTour);

            Bouton scinder = new Bouton(_jeu, xoverlay + 16 + Creatures.CREATURE_SIZE * 9, yoverlay + height - 64 - 16, "",_texScinder, _jeu.isoFont);
            scinder.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                if (released && _jeu.getTopOverlay().modalOverlay == ModalOverlay.AUCUN) {
                    boutonScinderClick();
                }
            };
            _controles.Add(scinder);

            _focusCreature = null;
            _group = new HashSet<Creature>();
            _groupOwner = null;

        }

        /// <summary>
        /// Affiche l'overlay
        /// Appelé au sein d'un spriteBatch begin/end
        /// </summary>
        /// <param name="spriteBatch">spriteBatch du jeu</param>
        /// <param name="GraphicsDevice">gd du jeu</param>
        /// <param name="gameTime">temps du jeu</param>
        public override void draw(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime) {
            // petite portion "update"... :(
            if (_jeu.selectedArmee != _groupOwner) {
                _groupOwner = _jeu.selectedArmee;
                _group.Clear();
            }
            // vrai draw
            baseDraw(spriteBatch, GraphicsDevice, gameTime);
            // Si une armée est sélectionnée, affiche son contenu
            if (_jeu.selectedArmee != null) {
                int x = xoverlay + 16 ;
                int y = yoverlay + height - Creatures.CREATURE_SIZE - 16*2 ;

                for (int i = 0; i < _jeu.selectedArmee.getTaille(); i++) {
                    Creature crea = _jeu.selectedArmee.getCreature(i);
                    _jeu.creatures.draw(spriteBatch, GraphicsDevice, gameTime, x + _jeu.terrain.offsetCarte.X, y + _jeu.terrain.offsetCarte.Y, 0f, crea.typeCreature);
                     if (_group.Contains(crea)) {
                         // Marqueur de sélection
                         spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(x + 3, y - 3, Creatures.CREATURE_SIZE - 6, 2), null,
                             _jeu.factions.getFaction(_jeu.tourFaction).couleur, 0, Vector2.Zero, SpriteEffects.None, 0.5f);
                         spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(x + 3, y + 3 + Creatures.CREATURE_SIZE, Creatures.CREATURE_SIZE - 6, 2), null,
                             _jeu.factions.getFaction(_jeu.tourFaction).couleur, 0, Vector2.Zero, SpriteEffects.None, 0.5f);


                     }
                     x += Creatures.CREATURE_SIZE;
                }
                
            }
            
            // Paneau d'information sur les créatures
            int xpano = xoverlay + 16 + Creatures.CREATURE_SIZE * 10;
            int ypano = yoverlay + height - 64 - 16*2;
            spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(xpano, ypano, 369-56, 64), null,Color.DarkGray,0,Vector2.Zero,SpriteEffects.None,0.5f);

            // Info sur une créature
            if (_focusCreature != null) {
                spriteBatch.DrawString(_jeu.font, _focusCreature.vraiNom, new Vector2(xpano+4, ypano+4), Color.Black,0,Vector2.Zero,1f,SpriteEffects.None,0.49f);
                spriteBatch.DrawString(_jeu.font, _focusCreature.profilAsStr(), new Vector2(xpano+4, ypano + 4 +18), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.49f);
                spriteBatch.DrawString(_jeu.font, "Cout : " + _focusCreature.description.cout, new Vector2(xpano + 4, ypano + 4 + 18*2), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.49f);
            }
            // or restant
            int totalDep = _jeu.factions.getTotalDep(_jeu.tourFaction);
            int totalRev = _jeu.factions.getTotalRev(_jeu.tourFaction);
            spriteBatch.DrawString(_jeu.font, "Or disponible : " + _jeu.factions.getFaction(_jeu.tourFaction).or
                                            + " ~ Dépenses : " + totalDep
                                            + " ~ Revenus : " + totalRev, new Vector2(_xoverlay, _yoverlay + _height - 22), Color.White);
        }

        /// <summary>
        /// Call back appelée lorsque la souris est dans l'overlay
        /// </summary>
        /// <param name="x">x souris</param>
        /// <param name="y">y souris</param>
        /// <param name="lost">focus perdu</param>
        public override void focused(KeyboardState kbd, int x, int y, bool lost) {
            if (_jeu.factions.getFaction(_jeu.tourFaction).humanPlayer) {
                // Controles
                basefocused(kbd, x, y, lost);
                // Position sur les troupes
                _focusCreature = null;
                if ((!lost) && (_jeu.selectedArmee != null)) {
                    int xc = xoverlay + 16;
                    int yc = yoverlay + height - Creatures.CREATURE_SIZE - 16;
                    for (int i = 0; i < _jeu.selectedArmee.getTaille(); i++) {
                        Creature crea = _jeu.selectedArmee.getCreature(i);
                        if (new Rectangle(xc, yc, Creatures.CREATURE_SIZE, Creatures.CREATURE_SIZE).Contains(x, y)) {
                            _focusCreature = crea;
                            break;
                        }
                        xc += Creatures.CREATURE_SIZE;
                    }
                }
            }
        }



        /// <summary>
        /// Call back appelée lorsqu'on clique dans l'overlay
        /// </summary>
        /// <param name="x">position X dans l'overlay (dans l'écran)</param>
        /// <param name="y">position Y dans l'overlay (dans l'écran)</param>
        /// <param name="leftClick">true si click avec bouton gauche</param>
        /// <param name="rightClick">true si click avec bouton droit</param>
        /// <param name="released">true si le user a relaché le bouton de souris après avoir cliqué (si true, le bouton est tjrs appuyé)</param>
        public override void clicked(int x, int y, bool leftClick, bool rightClick, bool released) {
            if (_jeu.factions.getFaction(_jeu.tourFaction).humanPlayer) {
                baseClicked(x, y, leftClick, rightClick, released);
                if (released && _focusCreature != null && _jeu.getTopOverlay().modalOverlay == ModalOverlay.AUCUN) {
                    // Sélectionne une armée pour la scinder
                    if (_group.Contains(_focusCreature)) {
                        _group.Remove(_focusCreature);
                    }
                    else {
                        _group.Add(_focusCreature);
                    }
                }
            }
        }


        // Boutons -------------------------------------------------------------------------------------------------------
        private void boutonScinderClick() {
            if ((_jeu.selectedArmee != null) && (_jeu.selectedArmee.getTaille() != _group.Count) && (_group.Count > 0)) {
                // Crée une autre armée

                Armee nouvelle = new Armee(_jeu);
                // Déplace les créatures sélectionnées dedans
                foreach (Creature crea in _group) {
                    _jeu.selectedArmee.removeCreature(crea);
                    nouvelle.addCreature(crea);                    
                }
                // Déselectionne les armées sorites
                for (int i = 0; i < nouvelle.getTaille() ; i++) {
                    Creature crea = nouvelle.getCreature(i);
                    _group.Remove(crea);
                }

                // Place la nouvelle armée à un emplacement libre à coté si il y a de la place
                Point? posNouvelle = nouvelle.getPositionLibre(_jeu.selectedArmee.positionCarte, true, true);
                // Si pas de place, annule tout !
                if (posNouvelle == null) {
                    _jeu.selectedArmee.ajouteTroupes(nouvelle);
                    _jeu.messageInfo = "Pas de place pour placer cette armée";
                }
                else {
                    // Regarde si on a assez de mouvement pour aller jusqu'à la nouvelle position
                    bool ok = true;
                    for (int i = 0; i < nouvelle.getTaille() ; i++) {
                        Creature crea = nouvelle.getCreature(i);
                        int mvtCost = crea.getMouvementCost(posNouvelle.Value, nouvelle);
                        if (crea.mouvementCourant < mvtCost) {
                            ok = false;
                            break;
                        }
                    }
                    if (!ok) {
                        // annule tout !
                        _jeu.selectedArmee.ajouteTroupes(nouvelle);
                        _jeu.messageInfo = "Plus assez de mouvement pour se séparer";
                    }
                    else {
                        // enlève le mouvement cette fois... Sinon on peut tricher avec des déplacements à coût 0 !
                        for (int i = 0; i < nouvelle.getTaille(); i++) {
                            Creature crea = nouvelle.getCreature(i);
                            int mvtCost = crea.getMouvementCost(posNouvelle.Value, nouvelle);
                            crea.mouvementCourant -= mvtCost;
                        }
                        nouvelle.positionCarte = posNouvelle.Value;
                        _jeu.addArmee(nouvelle);
                        // change sans doute le mouvement et donc la trajectoire de l'armée sélectionnée
                        _jeu.selectedArmee.reinitTrajectoire();
                    }
                }
            }
        }




        private void boutonArmeeSuivanteClick() {
            _jeu.creatures.selectNext();
        }         

        private void boutonSuiteMouvementClick() {
            if (_jeu.selectedArmee != null) {
                _jeu.selectedArmeeGO = true;
            }
        }
        private void boutonDeselectionClick() {
            if ((_jeu.selectedArmee != null) && (_jeu.selectedArmeeGO)) {
                _jeu.selectedArmee.stop();
            }
            _jeu.selectedArmee = null;
            _jeu.selectedArmeeGO = false;            
        }

        private void boutonOublieArmeeClick() {
            if (_jeu.selectedArmee != null) {
                _jeu.selectedArmee.blackListed = true;
                _jeu.creatures.selectNext();
            }
        }
        /// <summary>
        /// Fouille une ruine
        /// </summary>
        private void boutonFouillerRuineClick() {
            // Il faut que l'armée sélectionnée comprenne un héros et soit sur une ruine...
            if (_jeu.selectedArmee != null) {
                Creature crea = null;
                for (int i = 0; i < _jeu.selectedArmee.getTaille(); i++) {
                    crea = _jeu.selectedArmee.getCreature(i);
                    if (crea.description.heros) {
                        break;
                    }
                    else {
                        crea = null;
                    }
                }
                if (crea != null) {
                    // un héros, mais il y a-t-il une ruine ?
                    if (_jeu.ruines.ruines.ContainsKey(crea.positionCarte)) {
                        Ruines.RuineDescription rd = _jeu.ruines.ruines[crea.positionCarte];
                        if (!rd.visite) {
                            // Ouvre le panneau de visite de la ruine
                            _jeu.addOverlay(new PanneauRuine(_jeu, crea, rd));
                        }
                        else {
                            _jeu.messageInfo = "Cette ruine a déjà été fouillée";
                        }
                    }
                    else {
                        _jeu.messageInfo = "Vous ne pouvez fouiller que les ruines";
                    }
                }
                else {
                    _jeu.messageInfo = "Seul un héros peut visiter une ruine";
                }
                
            }

        }
        /// <summary>
        /// Fin du tour
        /// </summary>
        private void boutonFinTourClick() {
            _jeu.finTour();
        }

        /// <summary>
        /// Textures de l'overlay
        /// </summary>
        /// <param name="content"></param>
        public static void load(ContentManager content) {            
            _texContinuer = content.Load<Texture2D>("boutons/continuer");
            _texDeselection = content.Load<Texture2D>("boutons/deselection");
            _texFintour = content.Load<Texture2D>("boutons/fintour");
            _texFouiller = content.Load<Texture2D>("boutons/fouiller");
            _texPasser = content.Load<Texture2D>("boutons/passer");
            _texSuivant = content.Load<Texture2D>("boutons/suivant");
            _texScinder = content.Load<Texture2D>("boutons/scinder");
        }

    }
}
