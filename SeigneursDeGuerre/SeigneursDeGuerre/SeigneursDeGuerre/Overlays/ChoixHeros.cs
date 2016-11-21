using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SeigneursDeGuerre.Moteur;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SeigneursDeGuerre.Overlays {
    class ChoixHeros : PopWindow {

        private bool _femme;
        private string _nomHeros;
        private BoiteLabel _editNom;
        private Keys[] _keys;

        public ChoixHeros(Jeu _jeu)
            : base(_jeu, Overlay.Position.CENTRE, 360+16+16, 550, 0, 0, "Une nouvelle héroïne souhaite rejoindre votre cause") {

             
            _keys = new Keys[] {};
            _modalOverlay = ModalOverlay.CHOIX_HEROS;
            _femme = (_jeu.rnd.Next(2) == 1);
            if (!_femme) {
                _titre = _titre.Replace("nouvelle héroïne", "nouvel héros");
            }
            _nomHeros = nomAuHasard(_jeu.rnd, _femme);

            // Boutons de choix du sexe
            Bouton homme = new Bouton(_jeu, _xoverlay + 16, _yoverlay + 450, "Homme", null, _jeu.isoFont);
            Bouton femme = new Bouton(_jeu, _xoverlay + 16, _yoverlay + 495, "Femme", null, _jeu.isoFont);
            homme.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                if (released) {
                    _femme = false;
                    _titre = _titre.Replace("nouvelle héroïne", "nouvel héros");
                }
            };
            femme.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                if (released) {
                    _femme = true;
                    _titre = _titre.Replace("nouvel héros","nouvelle héroïne");
                }
            };
            _controles.Add(homme);
            _controles.Add(femme);

            // Bouton de validation
            Bouton ok = new Bouton(_jeu, _xoverlay + 16 + 270, _yoverlay + 495, "Valider", null, _jeu.isoFont);
            ok.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                if (released) {
                    // Création du héros et des troupes associées 
                    _jeu.creatures.createHeros(_editNom.texte, _femme, _jeu.noTour != 1);
                    _jeu.popOverlay();
                }
            };
            _controles.Add(ok);


            // Zone de saisie
            _editNom = new BoiteLabel(_jeu, _xoverlay + 100, _yoverlay + 450, 275, _nomHeros, _jeu.font);
            _controles.Add(_editNom);


            // Bouton de nom au hasard
            Bouton nomHasard = new Bouton(_jeu, _xoverlay + 100, _yoverlay + 495, "Hasard", null, _jeu.isoFont);
            nomHasard.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                if (released) {
                    _nomHeros = nomAuHasard(_jeu.rnd,_femme);
                    _editNom.texte = _nomHeros;
                }
            };
            _controles.Add(nomHasard);

            drawCallBack = delegate(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime) {                
                // Image de héros à gauche
                spriteBatch.Draw(_femme ? _jeu.texHeroine : _jeu.texHeros, new Rectangle(xoverlay + 16, yoverlay + 64, 360, 360), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.8f);                
            };
        }

        /// <summary>
        /// Retourne un nom au hasard pour un héros
        /// </summary>
        /// <param name="femme">nom de femme</param>
        /// <returns></returns>
        static public string nomAuHasard(Random rnd, bool femme) {
            string[] nomsHommes = { "Martin", "Jack", "Guy", "Maxime", "Moloch", "Skabold", "Leo", "Alban", "Abraham", "Guillaume", "Hjerin", "Wazam", "Paul","Erwann","Pierre","Loris","aurélien","Mathieu","Kenzo", };
            string[] nomsFemmes = { "Karine", "Gabrielle", "Manon", "Valérie", "Yennifer","Suzon","Lucie","Léna","Anna","Emma","Candyce","Audrey","Lisa","Emilie","Wladyslava", };
            return femme ? nomsFemmes[rnd.Next(nomsFemmes.Length)] : nomsHommes[rnd.Next(nomsHommes.Length)];
        }

        /// <summary>
        /// Call back appelée lorsque la souris est dans l'overlay
        /// </summary>
        /// <param name="x">x souris</param>
        /// <param name="y">y souris</param>
        /// <param name="lost">focus perdu</param>
        public override void focused(KeyboardState kbd, int x, int y, bool lost) {
            // taille max du nom
            const int MAXLENGTH = 16;
            // Controles
            basefocused(kbd, x, y, lost);
            // Saisies clavier : uniquement les nouvelles touches
            Keys[] keys = kbd.GetPressedKeys();
            foreach (Keys key in keys) {
                if (!_keys.Contains(key)) {
                    // Touche Back
                    if (key == Keys.Back) {
                        if (_editNom.texte.Length > 0) {
                            _editNom.texte = _editNom.texte.Substring(0, _editNom.texte.Length - 1);
                        }
                    }
                    else {
                        string keystr = key.ToString();
                        if (keystr.Equals("Space")) { 
                            keystr = " "; 
                        }
                        if ((keystr.Length == 1) && (_editNom.texte.Length < MAXLENGTH)) {
                            _editNom.texte += kbd.IsKeyDown(Keys.LeftShift)||kbd.IsKeyDown(Keys.RightShift) ? keystr.ToUpper() : keystr.ToLower();                                
                        }
                    }
                }
            }
            _keys = keys;

        }
    }
}
