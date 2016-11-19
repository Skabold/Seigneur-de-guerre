using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SeigneursDeGuerre.Moteur;

namespace SeigneursDeGuerre.Overlays {
    /// <summary>
    /// Classe de base pour une petite fenêtre affichée sur bouton droit
    /// </summary>
    class PopWindow : Overlay {

        public const int MARGE_TITRE = 16;

        /// <summary>
        /// Titre de la fenêtre
        /// </summary>
        protected string _titre;

        /// <summary>
        /// Permet d'avoir la taille de la fenêtre minimum d'après le titre
        /// avant de créer la pop window
        /// </summary>
        /// <param name="titre">titre de la pop window</param>
        /// <returns>la taille X,Y minimum de la fenêtre</returns>
        public static Point getPopWindowSize(Jeu leJeu, string titre) {
            // Taille de la fenêtre redimensionnée si trop peu large
            Vector2 tailleTitre = leJeu.font.MeasureString(titre);
            return new Point((int)tailleTitre.X + MARGE_TITRE, (int)tailleTitre.Y + MARGE_TITRE);
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="leJeu">jeu</param>
        /// <param name="position">position de l'overlay</param>
        /// <param name="height"> hauteur</param>
        /// <param name="width">largeur</param>
        public PopWindow(Jeu leJeu, Position position, int width, int height, int xsouris, int ysouris, string titre) : base(leJeu, position, width, height, (int)xsouris, (int)ysouris) {
            _titre = titre;            
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
            // Titre                        
            drawOutlinedString(spriteBatch, _jeu.font, _titre, new Vector2((float)_xoverlay + MARGE_TITRE / 2, (float)_yoverlay + MARGE_TITRE / 2), Color.LightGoldenrodYellow, Color.Black);
        }
        
    }
}
