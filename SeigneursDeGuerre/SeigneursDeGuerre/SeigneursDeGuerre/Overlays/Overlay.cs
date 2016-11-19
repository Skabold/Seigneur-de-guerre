using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SeigneursDeGuerre.Moteur;
using Microsoft.Xna.Framework.Input;

namespace SeigneursDeGuerre.Overlays {

    delegate void CustomDraw(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime);

    /// <summary>
    /// Classe de base pour les fenêtres au dessus de la carte "aventure"
    /// </summary>
    abstract class Overlay {

        public enum ModalOverlay {
            AUCUN,
            IMAGE_FOND,
            CHOIX_HEROS,
            INFO_VILLE,
            FOUILLE_RUINE,
            FACTIONS,
            COMBAT,
            MENU_SAUVE
        };

        public enum Position {
            HAUT_GAUCHE,
            HAUT_DROITE,
            BAS_GAUCHE,
            BAS_DROITE,
            CENTRE,
            SOURIS
        }
        /// <summary>
        /// Marge entre l'overlay et le bord de l'écran
        /// </summary>
        protected const int MARGE = 20;

        /// <summary>
        /// Position de l'overlay
        /// </summary>
        protected Position _position;

        /// <summary>
        /// Taille de l'overlay en pixels
        /// </summary>
        protected int _width;
        protected int _height;

        /// <summary>
        /// Coin haut gauche de l'overlay
        /// </summary>
        protected int _xoverlay;
        protected int _yoverlay;

        /// <summary>
        /// Jeu courant
        /// </summary>
        protected Jeu _jeu;

        /// <summary>
        /// Couleur du fond
        /// </summary>
        protected Color _colorFond;

        /// <summary>
        /// Extension de draw
        /// </summary>
        CustomDraw _drawCallBack;

        /// <summary>
        /// Type d'overlay modal
        /// </summary>
        protected ModalOverlay _modalOverlay;

        /// <summary>
        /// Controles
        /// </summary>
        protected List<Controle> _controles;

        /// <summary>
        /// Controle qui a le focus
        /// </summary>
        protected Controle _focusedCtrl;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="leJeu">jeu</param>
        /// <param name="position">position de l'overlay</param>
        /// <param name="height"> hauteur</param>
        /// <param name="width">largeur</param>
        /// <param name="xsouris">x souris (nécessaire si position = souris)</param>
        /// <param name="ysouris">y souris (nécessaire si position = souris)</param>
        public Overlay(Jeu leJeu, Position position, int width, int height, int? xsouris,int? ysouris) {
            _position = position;
            _jeu = leJeu;
            _width = width;
            _height = height;
            // Calcule la position
            switch (position) {
                case Position.BAS_DROITE:
                    _xoverlay = _jeu.resX - _width - MARGE;
                    _yoverlay = _jeu.resY - _height - MARGE;
                    break;
                case Position.BAS_GAUCHE:
                    _xoverlay = MARGE;
                    _yoverlay = _jeu.resY - _height - MARGE;
                    break;
                case Position.HAUT_DROITE:
                    _xoverlay = _jeu.resX - _width - MARGE;
                    _yoverlay = MARGE;
                    break;
                case Position.HAUT_GAUCHE:
                    _xoverlay = MARGE;
                    _yoverlay = _jeu.resY - _height - MARGE;
                    break;
                case Position.CENTRE:
                    _xoverlay = (_jeu.resX - _width) / 2;
                    _yoverlay = (_jeu.resY - _height) / 2;
                    break;
                case Position.SOURIS:
                    _xoverlay = (int)xsouris;
                    _yoverlay = (int)ysouris;
                    // Décale si sort de l'écran
                    if (_xoverlay + _width > _jeu.resX) {
                        _xoverlay = _jeu.resX - _width;
                    }
                    if (_yoverlay + _height > _jeu.resY) {
                        _yoverlay = _jeu.resY - _height;
                    }

                    break;
            }
            // Couleur du fond
            _colorFond = Color.FromNonPremultiplied(128, 128, 128, 200);
            _drawCallBack = null;
            _modalOverlay = ModalOverlay.AUCUN;
            _controles = new List<Controle>();
            _focusedCtrl = null;
        }
        /// <summary>
        /// Donne une callback
        /// </summary>
        public CustomDraw drawCallBack {
            set { _drawCallBack = value; }
        }

        public int xoverlay {
            get { return _xoverlay; }
        }
        public int yoverlay {
            get { return _yoverlay; }
        }
        public int width {
            get { return _width; }
        }
        public int height {
            get { return _height; }
        }
        public ModalOverlay modalOverlay {
            get { return _modalOverlay; }
        }


        /// <summary>
        /// Détermine si le point spécifié est dans l'overlay
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool isPointInOverlay(int x, int y) {
            return new Rectangle(_xoverlay, _yoverlay, _width, _height).Contains(x, y);
        }

        
        /// <summary>
        /// Affiche l'overlay
        /// Appelé au sein d'un spriteBatch begin/end
        /// </summary>
        /// <param name="spriteBatch">spriteBatch du jeu</param>
        /// <param name="GraphicsDevice">gd du jeu</param>
        /// <param name="gameTime">temps du jeu</param>
        public abstract void draw(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime);
            
        /// <summary>
        /// Fournit un comportement de base (affiche le fond)
        /// </summary>
        /// <param name="spriteBatch">spriteBatch du jeu</param>
        /// <param name="GraphicsDevice">gd du jeu</param>
        /// <param name="gameTime">temps du jeu</param>
        protected void baseDraw(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime) {
            // Affiche le fond
            spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(_xoverlay, _yoverlay, _width, _height), null, _colorFond,0,Vector2.Zero,SpriteEffects.None, 1f);
            // Extension
            if (_drawCallBack != null) {
                _drawCallBack(spriteBatch, GraphicsDevice, gameTime);
            }
            // Controles
            foreach (Controle ctrl in _controles) {
                ctrl.draw(spriteBatch, GraphicsDevice, gameTime);
            }
        }

        /// <summary>
        /// Click de base (contrôles)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="leftClick"></param>
        /// <param name="rightClick"></param>
        /// <param name="released"></param>
        /// <return>true si un controle a été cliqué</return>
        protected bool baseClicked(int x, int y, bool leftClick, bool rightClick, bool released) {
            // Controles
            bool retval = false;
            foreach (Controle ctrl in _controles) {
                // Si on clicke dedans
                if (ctrl.contains(x, y)) {
                    ctrl.clicked(x, y, leftClick, rightClick, released);
                    retval = true;
                }
            }
            return retval;
        }
        

        /// <summary>
        /// Call back appelée lorsqu'on clique dans l'overlay
        /// </summary>
        /// <param name="x">position X dans l'overlay (dans l'écran)</param>
        /// <param name="y">position Y dans l'overlay (dans l'écran)</param>
        /// <param name="leftClick">true si click avec bouton gauche</param>
        /// <param name="rightClick">true si click avec bouton droit</param>
        /// <param name="released">true si le user a relaché le bouton de souris après avoir cliqué (si true, le bouton est tjrs appuyé)</param>
        public virtual void clicked(int x, int y, bool leftClick, bool rightClick, bool released) {
            baseClicked(x, y, leftClick, rightClick, released);
        }
        
        /// <summary>
        /// Focus de base (appelle la callback sur les controles)
        /// </summary>
        /// <param name="x">x souris</param>
        /// <param name="y">y souris</param>
        /// <param name="lost">focus perdu ?</param>
        protected void basefocused(KeyboardState kbd, int x, int y, bool lost) {
            // Controles
            foreach (Controle ctrl in _controles) {
                if (!lost) {
                    // Si le focus est dedans
                    if (ctrl.contains(x, y)) {
                        ctrl.focused(kbd, x, y, lost);
                        _focusedCtrl = ctrl;
                    }
                    else if (_focusedCtrl == ctrl) {
                        ctrl.focused(kbd, x, y, true);
                        _focusedCtrl = null;
                    }
                }
                else if (_focusedCtrl != null) {
                    _focusedCtrl.focused(kbd, x, y, lost);
                    _focusedCtrl = null;
                }
            };
        }

        /// <summary>
        /// Call back appelée lorsque la souris est dans l'overlay
        /// </summary>
        /// <param name="x">x souris</param>
        /// <param name="y">y souris</param>
        /// <param name="lost">focus perdu</param>
        public virtual void focused(KeyboardState kbd,int x, int y, bool lost) {
            // Controles
            basefocused(kbd, x, y, lost);
        }
        
        /// <summary>
        /// Utilitaire pour écrire du texte avec un tour
        /// </summary>
        /// <param name="batch">spritebatch</param>
        /// <param name="font">police</param>
        /// <param name="text">texte</param>
        /// <param name="pos">position</param>
        /// <param name="color">couleur texte</param>
        /// <param name="coloroutline">couleur du tour</param>
        public static void drawOutlinedString(SpriteBatch batch, SpriteFont font, String text, Vector2 pos, Color color, Color coloroutline) {

            batch.DrawString(font, text, pos + new Vector2(0, -1), coloroutline, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
            batch.DrawString(font, text, pos + new Vector2(0, 1), coloroutline, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
            batch.DrawString(font, text, pos + new Vector2(1, 0), coloroutline, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
            batch.DrawString(font, text, pos + new Vector2(-1, 0), coloroutline, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);

            batch.DrawString(font, text, pos, color, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
        }
    }
}
