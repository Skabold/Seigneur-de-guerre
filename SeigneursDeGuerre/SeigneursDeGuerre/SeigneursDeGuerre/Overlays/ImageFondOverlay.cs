using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SeigneursDeGuerre.Moteur;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SeigneursDeGuerre.Overlays {
    class ImageFondOverlay : PopWindow {
        
        Texture2D _fond;

        public ImageFondOverlay(Jeu _jeu, Texture2D fond, string titre)
            : base(_jeu, Overlay.Position.CENTRE, _jeu.resX,  _jeu.resY , 0, 0, titre) {
                _modalOverlay = ModalOverlay.IMAGE_FOND;         
                _fond = fond;

        }

           /// <summary>
        /// Affiche l'overlay
        /// Appelé au sein d'un spriteBatch begin/end
        /// </summary>
        /// <param name="spriteBatch">spriteBatch du jeu</param>
        /// <param name="GraphicsDevice">gd du jeu</param>
        /// <param name="gameTime">temps du jeu</param>
        public override void draw(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime) {
            
            Rectangle rectDest = new Rectangle(0, 0, _jeu.resX, _jeu.resY);
            // La source est éventuellement tronquée à gauche et à droite pour s'adapter à tout l'écran
            int w,h;
            Rectangle rectSource;

            h = (_fond.Width * _jeu.resY) / _jeu.resX;
            if (h > _fond.Height) {
                h = _fond.Height;
            }
            // 1600 x 1200 / 800x600 => w=800 h=600 
            // 1920 x 1200 / 800x600 => w=800 h=500  

            w = (_fond.Height * _jeu.resX) / _jeu.resY;
            if (w > _fond.Width) {
                w = _fond.Width;
            }
            rectSource = new Rectangle((_fond.Width - w) / 2, (_fond.Height - h) / 2, w, h);

            spriteBatch.Draw(_fond, rectDest, rectSource, Color.White,0,Vector2.Zero,SpriteEffects.None,0.9f);
            Vector2 taille = _jeu.bigFont.MeasureString(_titre);
            drawOutlinedString(spriteBatch, _jeu.bigFont, _titre, new Vector2((_jeu.resX - taille.X) / 2, (_jeu.resY- taille.Y) / 2), _jeu.factions.getFaction(_jeu.tourFaction).couleur, 
                _jeu.tourFaction == 8 ? Color.Red : Color.Black);
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
            // ferme au click
            if (released) {
                if (_jeu.getTopOverlay().modalOverlay == ModalOverlay.IMAGE_FOND) {
                    _jeu.popOverlay();
                }
            }
        }


        /// <summary>
        /// Call back appelée lorsque la souris est dans l'overlay
        /// </summary>
        /// <param name="x">x souris</param>
        /// <param name="y">y souris</param>
        /// <param name="lost">focus perdu</param>
        public override void focused(KeyboardState kbd, int x, int y, bool lost) {    
            if (kbd.IsKeyDown(Keys.Space) || kbd.IsKeyDown(Keys.Enter)) {
                if (_jeu.getTopOverlay().modalOverlay == ModalOverlay.IMAGE_FOND) {
                    _jeu.popOverlay();
                }
            }
        }
    }

    /// <summary>
    /// Les overlays fils : nouveau tour, victoire, défaite...
    /// </summary>
    class NouveauTour : ImageFondOverlay {
        public NouveauTour(Jeu _jeu)
            : base(_jeu, _jeu.texChgTour, "Tour " + _jeu.noTour + " - " + _jeu.factions.getFaction(_jeu.tourFaction).nom) {
        }
    }

    class Victoire : ImageFondOverlay {
        public Victoire(Jeu _jeu)
            : base(_jeu, _jeu.texVictoire, _jeu.factions.getFaction(_jeu.tourFaction).nom + ", vous êtes victorieux !") {
        }
    }
    class Defaite : ImageFondOverlay {
        public Defaite(Jeu _jeu)
            : base(_jeu, _jeu.texDefaite, "Tous les joueurs humains ont été vaincus") {
        }
    }

}
