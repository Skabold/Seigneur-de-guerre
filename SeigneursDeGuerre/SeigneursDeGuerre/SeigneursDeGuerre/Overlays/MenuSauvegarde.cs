using SeigneursDeGuerre.Moteur;

namespace SeigneursDeGuerre.Overlays {

   
    class MenuSauvegarde  : PopWindow {
        
        
        public MenuSauvegarde(Jeu _jeu, string nomFichier, suiteTraitement suite)
            : base(_jeu, Overlay.Position.CENTRE, 120, 200, 0, 0, "Menu") {
        
            _modalOverlay = ModalOverlay.MENU_SAUVE;

            Bouton sauver = new Bouton(_jeu, _xoverlay + 16, _yoverlay + _height - 48 - 40*2, "Sauver ", null, _jeu.isoFont);
            sauver.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                if (released) {
                    sauverJeu(nomFichier);
                }
            };
            _controles.Add(sauver);

            Bouton charger = new Bouton(_jeu, _xoverlay + 16, _yoverlay + _height - 48 - 40*3, "Charger", null, _jeu.isoFont);
            charger.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                if (released) {
                    chargerJeu(nomFichier);
                }
            };
            _controles.Add(charger);

            Bouton quitter = new Bouton(_jeu, _xoverlay + 16, _yoverlay + _height - 48 - 40*1, "Quitter", null, _jeu.isoFont);
            quitter.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                if (released) {
                    suite();
                }
            };
            _controles.Add(quitter);

            Bouton annuler = new Bouton(_jeu, _xoverlay + 16, _yoverlay + _height - 48 - 40 * 0, "Annuler", null, _jeu.isoFont);
            annuler.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                if (released) {
                    _jeu.popOverlay();
                }
            };
            _controles.Add(annuler);

        }

        /// <summary>
        /// Sauver le jeu
        /// </summary>
        private void sauverJeu(string nomFichier) {
            _jeu.saveGame(nomFichier);
            _jeu.popOverlay();
        }

        /// <summary>
        /// Charger le jeu
        /// </summary>
        private void chargerJeu(string nomFichier) {
            _jeu.loadGame(nomFichier);
            _jeu.popOverlay();
        }


        
    }
}
