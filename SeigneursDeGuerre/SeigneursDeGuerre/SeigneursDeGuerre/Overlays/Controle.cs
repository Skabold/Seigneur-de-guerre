using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SeigneursDeGuerre.Moteur;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SeigneursDeGuerre.Overlays {

    delegate void CustomClick(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released);

    /// <summary>
    /// Classe pour un controle de fenêtre
    /// </summary>
    abstract class Controle {
        // Constantes
        public const int MARGE = 5;
        // caractéristiques données
        protected Jeu _jeu;
        protected int _x;
        protected int _y;
        protected string _texte;
        protected SpriteFont _police;
        protected CustomClick _click;
        // caractéristiques calculées
        protected int _w;
        protected int _h;
        protected Vector2 _tailleTexte;
        protected int _userData;

        public Controle(Jeu jeu, int x, int y, string texte, SpriteFont police) {
            _jeu = jeu;
            _x = x;
            _y = y;
            _texte = texte;
            _police = police;
            _tailleTexte = _police.MeasureString(texte);
            _w = (int)_tailleTexte.X + MARGE * 2;
            _h = (int)_tailleTexte.Y + MARGE * 2;
            _click = null;
            _userData = 0;
        }
        /// <summary>
        /// Callback de click
        /// </summary>
        public CustomClick click {
            get { return _click; }
            set { _click = value; }
        }
        public string texte {
            get { return _texte; }
            set { _texte = value; }
        }
        public int userData {
            get { return _userData; }
            set { _userData = value; }
        }
        public int x {get { return _x; }}
        public int y { get { return _y; } }
        public int w { get { return _w; } }
        public int h { get { return _h; } }
           

        /// <summary>
        /// Méthode de dessin du bouton
        /// </summary>
        /// <param name="spriteBatch">spritebatch</param>
        /// <param name="GraphicsDevice">gd</param>
        /// <param name="gameTime">gt</param>
        abstract public void draw(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime);

        /// <summary>
        /// Call back appelée lorsqu'on clique dans l'overlay
        /// </summary>
        /// <param name="x">position X dans l'overlay (dans l'écran)</param>
        /// <param name="y">position Y dans l'overlay (dans l'écran)</param>
        /// <param name="leftClick">true si click avec bouton gauche</param>
        /// <param name="rightClick">true si click avec bouton droit</param>
        /// <param name="released">true si le user a relaché le bouton de souris après avoir cliqué (si true, le bouton est tjrs appuyé)</param>
        public virtual void clicked(int x, int y, bool leftClick, bool rightClick, bool released) {
            if (_click != null) {
                _click(this, x, y, leftClick, rightClick, released);
            }
        }

        /// <summary>
        /// Call back appelée lorsque la souris est dans le controle
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="lost"></param>
        public virtual void focused(KeyboardState kbd, int x, int y, bool lost) {
        }

        /// <summary>
        /// Est-ce que le point est dans le controle ?
        /// </summary>
        /// <param name="mx">point x</param>
        /// <param name="my">point y</param>
        /// <returns></returns>
        public bool contains(int mx, int my) {
            return new Rectangle(_x, _y, _w, _h).Contains(mx, my);
        }
    }


    /// <summary>
    /// Classe représentant un bouton
    /// </summary>
    class Bouton : Controle {
        private bool _pressed;
        private Texture2D _picture;
        public Bouton(Jeu jeu, int x, int y, string texte, Texture2D picture, SpriteFont police)
            : base(jeu, x, y, texte, police) {
                _pressed = false;
                _picture = picture;
                if (picture != null) {
                    // redimensionne éventuellement
                    if ((4 + picture.Width) > _w) {
                        _w = picture.Width + 4;
                    }
                    if ((4 + picture.Height) > _h) {
                        _h = picture.Height + 4;
                    }
                }
        }
        /// <summary>
        /// Accesseur sur image
        /// </summary>
        public Texture2D picture {
            get { return _picture; }
            set { _picture = value; }
        }

        /// <summary>
        /// Méthode de dessin du bouton
        /// </summary>
        /// <param name="spriteBatch">spritebatch</param>
        /// <param name="GraphicsDevice">gd</param>
        /// <param name="gameTime">gt</param>
        public override void draw(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime) {
            
            if (!_pressed) {
                // Fond
                spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(_x, _y, _w, _h), null, Color.DarkGray, 0, Vector2.Zero, SpriteEffects.None, 0.8f);
                // Bords
                spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(_x, _y, _w, 1), null, Color.LightGray, 0, Vector2.Zero, SpriteEffects.None, 0.79f);
                spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(_x, _y + _h, _w, 1), null, Color.FromNonPremultiplied(100, 100, 100, 255), 0, Vector2.Zero, SpriteEffects.None, 0.79f);
                spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(_x, _y, 1, _h), null, Color.FromNonPremultiplied(100, 100, 100, 255), 0, Vector2.Zero, SpriteEffects.None, 0.79f);
                spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(_x + _w, _y, 1, _h), null, Color.LightGray, 0, Vector2.Zero, SpriteEffects.None, 0.79f);
                // Image
                if (_picture != null) {
                    spriteBatch.Draw(_picture, new Rectangle(_x + 3, _y + 1, _picture.Width, _picture.Height), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.71f);
                }
                // Texte
                spriteBatch.DrawString(_police, _texte, new Vector2(_x + MARGE, _y + MARGE), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.7f);
            }
            else {
                // Fond
                spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(_x, _y, _w, _h), null, Color.FromNonPremultiplied(128, 128, 128, 255), 0, Vector2.Zero, SpriteEffects.None, 0.8f);
                // Bords

                spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(_x+1, _y+1, _w-2, 1), null,Color.FromNonPremultiplied(100, 100, 100, 255), 0, Vector2.Zero, SpriteEffects.None, 0.79f);
                spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(_x + 1, _y + _h - 1, _w - 2, 1), null, Color.DarkGray, 0, Vector2.Zero, SpriteEffects.None, 0.79f);
                spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(_x + 1, _y + 1, 1, _h - 2), null, Color.DarkGray, 0, Vector2.Zero, SpriteEffects.None, 0.79f);
                spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(_x + _w - 1, _y+1, 1, _h-2), null,Color.FromNonPremultiplied(100, 100, 100, 255), 0, Vector2.Zero, SpriteEffects.None, 0.79f);

                spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(_x, _y, _w, 1), null, Color.Black, 0, Vector2.Zero, SpriteEffects.None, 0.79f);
                spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(_x, _y + _h, _w, 1), null, Color.Black, 0, Vector2.Zero, SpriteEffects.None, 0.79f);
                spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(_x, _y, 1, _h), null, Color.Black, 0, Vector2.Zero, SpriteEffects.None, 0.79f);
                spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(_x + _w, _y, 1, _h), null, Color.Black, 0, Vector2.Zero, SpriteEffects.None, 0.79f);
                // Image
                if (_picture != null) {
                    spriteBatch.Draw(_picture, new Rectangle(_x+2, _y + 2, _picture.Width, _picture.Height), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.71f);
                }
                // Texte
                spriteBatch.DrawString(_police, _texte, new Vector2(_x + MARGE - 2 , _y + MARGE + 2), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.7f);
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
            _pressed = !released;
            if (_click != null) {
                _click(this, x, y, leftClick, rightClick, released);
            }
        }
        /// <summary>
        /// Call back appelée lorsque la souris est dans le controle
        /// </summary>
        /// <param name="x">x souris</param>
        /// <param name="y">y souris</param>
        /// <param name="lost">focus perdu alors qu'il a eu auparavant</param>
        public override void focused(KeyboardState kbd, int x, int y, bool lost) {
            if (lost) {
                _pressed = false;
            }
        }


    }

    /// <summary>
    /// Zone d'édition de texte
    /// </summary>
    class BoiteLabel : Controle {
        public BoiteLabel(Jeu jeu, int x, int y, int w, string texte, SpriteFont police)
            : base(jeu, x, y, texte, police) {
                _w = w;
        }
        /// <summary>
        /// Méthode de dessin du bouton
        /// </summary>
        /// <param name="spriteBatch">spritebatch</param>
        /// <param name="GraphicsDevice">gd</param>
        /// <param name="gameTime">gt</param>
        public override void draw(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime) {
            spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(_x, _y, _w, _h), null, Color.DarkGray, 0, Vector2.Zero, SpriteEffects.None, 0.8f);
            spriteBatch.DrawString(_police, _texte+"|", new Vector2(_x + MARGE, _y + MARGE), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.7f);
        }
    }
}
