using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SeigneursDeGuerre.Moteur;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SeigneursDeGuerre.Overlays {
    class PanneauRuine : PopWindow {
        public PanneauRuine(Jeu _jeu, Creature heros, Ruines.RuineDescription _ruine)
            : base(_jeu, Overlay.Position.CENTRE, 400, 440, 0, 0,
               heros.vraiNom + " fouille la Ruine de " + _ruine.nom + "...") {

            _modalOverlay = ModalOverlay.FOUILLE_RUINE;
            // Bouton Fermer
            Bouton fermer = new Bouton(_jeu, _xoverlay + 16, _yoverlay + _height - 48, "Fermer", null, _jeu.isoFont);
            fermer.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                if (released) {
                    _jeu.popOverlay();
                }
            };
            _controles.Add(fermer);

            string nomEnnemi = null;
            string nomRecompense = null;
            bool herosGagne = _jeu.ruines.fouilleRuine(heros, _ruine, ref nomEnnemi, ref nomRecompense);
            

            // Affichage
            drawCallBack = delegate(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime) {
                spriteBatch.Draw(_jeu.texRuine, new Rectangle(_xoverlay + (_width - 256) / 2, _yoverlay + 32, 256, 256), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.7f);
                spriteBatch.DrawString(_jeu.font, heros.vraiNom + " rencontre " + nomEnnemi + "...", new Vector2(_xoverlay + 16, _yoverlay + 256+48), Color.White);
                spriteBatch.DrawString(_jeu.font, "... et " + (herosGagne?" en sort vainqueur !" : " a été massacré(e) !"), new Vector2(_xoverlay + 16, _yoverlay + 256+48+20), Color.White);
                if (herosGagne) {
                    spriteBatch.DrawString(_jeu.font, heros.vraiNom + " trouve " + nomRecompense, new Vector2(_xoverlay + 16, _yoverlay + 256 + 48 + 20*2), Color.White);
                }
            };
        }
    }
}
