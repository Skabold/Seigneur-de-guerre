using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    /// Pointeur de souris
    /// </summary>
    class Cursor {

        /// <summary>
        /// Les différents types de curseurs possibles
        /// </summary>
        public enum FormeCurseur {
            FLECHE,
            EPEE,
            MAIN,
            INTERROGATION
        };
        // constantes
        private const int CURSOR_SIZE = 48;
        
        // textures
        private Texture2D _cursorEpee;
        private Texture2D _cursorHand;
        private Texture2D _cursorQuestion;
        private Texture2D _cursorArrow;

        // Curseur courant
        private FormeCurseur _forme;
        private Texture2D _texCourante;
        
        // Accesseurs
        public int mouseSize {
            get { return CURSOR_SIZE; }
        }

        // change la forme du cursur
        public FormeCurseur forme {
            get { return _forme; }
            set {
                _forme = value;
                switch (_forme) {
                    case FormeCurseur.EPEE:
                        _texCourante = _cursorEpee;
                        break;
                    case FormeCurseur.FLECHE:
                        _texCourante = _cursorArrow;
                        break;
                    case FormeCurseur.INTERROGATION:
                        _texCourante = _cursorQuestion;
                        break;
                    case FormeCurseur.MAIN:
                        _texCourante = _cursorHand;
                        break;
                    default:
                        _texCourante = _cursorArrow;
                        break;
                }
            }
        }

        // Créateur
        public Cursor() {
            _forme = FormeCurseur.FLECHE;            
        }

        // Callbacks
        public void load(ContentManager content) {
            _cursorEpee = content.Load<Texture2D>("cursor");
            _cursorHand = content.Load<Texture2D>("glove");
            _cursorQuestion = content.Load<Texture2D>("Question-Mark");
            _cursorArrow = content.Load<Texture2D>("arrow");

            _texCourante = _cursorArrow;
        }
        
        /// <summary>
        /// Affiche le curseur
        /// Appelé au sein d'un spriteBatch begin/end
        /// </summary>
        /// <param name="spriteBatch">spriteBatch du jeu</param>
        /// <param name="GraphicsDevice">gd du jeu</param>
        /// <param name="gameTime">temps du jeu</param>
        /// <param name="mouseX">position X du curseur</param>
        /// <param name="mouseY">position Y du curseur</param>
        public void draw(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime, int mouseX, int mouseY) {
            // Affiche la souris            
            spriteBatch.Draw(_texCourante, new Rectangle(mouseX, mouseY, mouseSize, mouseSize), Color.White);
        }

    }
}
