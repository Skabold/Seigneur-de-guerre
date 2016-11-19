using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SeigneursDeGuerre.Moteur;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SeigneursDeGuerre.Overlays {

    delegate void FinDuCombat(bool attaquantVainqueur);


    class ZoneCombat : Overlay {
        private enum Etat {
            NOUVEAU_ROUND,
            AFFICHE_MORT_ATK,
            AFFICHE_MORT_DEF            
        };
        private Combat _combat;
        private FinDuCombat _callBack;

        private Etat _etat;
        private bool _atkGagne;

        private double _tempsmiseajour;
        private const double TEMPO = 750;

        public ZoneCombat(Jeu _jeu, Combat combat, FinDuCombat callBack)
            : base(_jeu, Overlay.Position.CENTRE, Creatures.CREATURE_SIZE * (Armee.MAX_TAILLE + 2),
                                                  Creatures.CREATURE_SIZE * 6, 0, 0) {
            _modalOverlay = ModalOverlay.COMBAT;
            _combat = combat;
            _callBack = callBack;

            _colorFond = Color.FromNonPremultiplied(140, 100, 100, 220);
            _tempsmiseajour = TEMPO*2;
            _etat = Etat.NOUVEAU_ROUND;
        }

        /// <summary>
        /// Affiche l'overlay
        /// Appelé au sein d'un spriteBatch begin/end
        /// </summary>
        /// <param name="spriteBatch">spriteBatch du jeu</param>
        /// <param name="GraphicsDevice">gd du jeu</param>
        /// <param name="gameTime">temps du jeu</param>
        public override void draw(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime) {
            baseDraw(spriteBatch, GraphicsDevice, gameTime);
            // Armées en défense
            for (int i = 0; i < _combat.defense.getTaille(); i++) {
                Creature crea = _combat.defense.getCreature(i);
                int x = xoverlay + Creatures.CREATURE_SIZE * (i + 1) + _jeu.terrain.offsetCarte.X;
                int y = yoverlay + Creatures.CREATURE_SIZE * 1 + _jeu.terrain.offsetCarte.Y;
                _jeu.creatures.draw(spriteBatch, GraphicsDevice, gameTime, x, y, 0.8f, crea.typeCreature);
            }

            // Armées en attaque
            for (int i = 0; i < _combat.attaque.getTaille(); i++) {
                Creature crea = _combat.attaque.getCreature(i);
                int x = xoverlay + Creatures.CREATURE_SIZE * (i + 1) + _jeu.terrain.offsetCarte.X;
                int y = yoverlay + Creatures.CREATURE_SIZE * 4 + _jeu.terrain.offsetCarte.Y;
                _jeu.creatures.draw(spriteBatch, GraphicsDevice, gameTime, x, y, 0.8f, crea.typeCreature);
            }

            if (_etat == Etat.AFFICHE_MORT_ATK) {
                spriteBatch.Draw(PanneauControle._texFintour, new Rectangle(xoverlay + Creatures.CREATURE_SIZE, yoverlay + Creatures.CREATURE_SIZE * 4,
                    Creatures.CREATURE_SIZE, Creatures.CREATURE_SIZE),
                    null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.7f);
            }
            else if (_etat == Etat.AFFICHE_MORT_DEF) {
                spriteBatch.Draw(PanneauControle._texFintour, new Rectangle(xoverlay + Creatures.CREATURE_SIZE, yoverlay + Creatures.CREATURE_SIZE * 1,
                                Creatures.CREATURE_SIZE, Creatures.CREATURE_SIZE),
                                null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.7f);

            }

            // Bonus en présence
            int bonusDefense = _combat.bonusDefense;
            int bonusAttaque = _combat.bonusAttaque;
            spriteBatch.Draw(_jeu.pixelBlanc,new Rectangle(xoverlay,yoverlay,8,(height*bonusDefense)/(bonusDefense+bonusAttaque)),
                null,_jeu.factions.getFaction(_combat.defense.faction).couleur*0.5f,0,Vector2.Zero,SpriteEffects.None,0.6f);

            spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(xoverlay, yoverlay + (height*bonusDefense)/(bonusDefense+bonusAttaque), 8, (height * bonusAttaque) / (bonusDefense + bonusAttaque)),
                null, _jeu.factions.getFaction(_combat.attaque.faction).couleur*0.5f, 0, Vector2.Zero, SpriteEffects.None, 0.6f);



            // Pour simplifier, l'update se fait aussi dans le draw, à la fin 
            _tempsmiseajour -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_tempsmiseajour < 0) {
                _tempsmiseajour = TEMPO;

                switch (_etat) {
                    case Etat.NOUVEAU_ROUND:
                        _atkGagne = _combat.unRound(true);
                        _etat = _atkGagne ? Etat.AFFICHE_MORT_DEF : Etat.AFFICHE_MORT_ATK;
                        break;
                    case Etat.AFFICHE_MORT_ATK:
                    case Etat.AFFICHE_MORT_DEF:
                        bool combatFini = _combat.roundSuivant(_atkGagne);
                        _etat = Etat.NOUVEAU_ROUND;
                        if (combatFini) {
                            Armee perdante = _atkGagne ? _combat.defense : _combat.attaque;
                            _jeu.removeArmee(perdante);
                            // Si l'attaquant a perdu, on arrête, sinon on continue 
                            if (_atkGagne) {
                                bool finCombat = !_combat.armeeSuivante(false);
                                if (finCombat) {
                                    // Attaquant gagné
                                    _callBack(true);
                                    _jeu.popOverlay();
                                }
                            }
                            else {
                                // Attaquant perdu
                                _callBack(false);
                                _jeu.popOverlay();
                            }
                        }


                        break;
                }

            }
        }

    }
}
