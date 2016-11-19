using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SeigneursDeGuerre.Moteur;

namespace SeigneursDeGuerre.Overlays {
    /// <summary>
    /// Callback pour poursuivre le traitement à la fin de l'overlay
    /// </summary>
    delegate void suiteTraitement();

    /// <summary>
    /// Choix des factions : qui est humain, qui ne l'est pas
    /// </summary>
    class FactionOverlay : PopWindow {
        public FactionOverlay(Jeu _jeu, suiteTraitement suite)
            : base(_jeu, Overlay.Position.CENTRE, 400, 680+80, 0, 0,"Seigneurs de Guerre - Choisissez les factions") {
               
            _modalOverlay = ModalOverlay.FACTIONS;
            
            // Bouton Démarrer
            Bouton demarrer = new Bouton(_jeu, _xoverlay + 16, _yoverlay + _height - 48, "Démarrer", null, _jeu.isoFont);
            demarrer.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                if (released) {
                    _jeu.popOverlay();
                    // Suite du traitement - début du jeu
                    suite();
                }
            };
            _controles.Add(demarrer);
            
            // Boutons des factions, 2 colonnes
            Bouton[] factions = new Bouton[_jeu.factions.nbFactions];
            int xbouton = _xoverlay + 16;
            int ybouton = _yoverlay + 48;
            for (int i = 1; i <= factions.Length; i++) {
                Factions.Faction f = _jeu.factions.getFaction(i);
                string nom = ("           " + f.nom + "                           ").Substring(0,32);
                factions[i-1] = new Bouton(_jeu, xbouton, ybouton, nom, f.humanPlayer ? _jeu.texHumain : _jeu.texCPU, _jeu.isoFont);
                factions[i-1].userData = i;
                factions[i-1].click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                    if (released) {
                        Factions.Faction fac = _jeu.factions.getFaction(clicked.userData);
                        fac.humanPlayer ^= true;
                        ((Bouton)clicked).picture = fac.humanPlayer ? _jeu.texHumain : _jeu.texCPU;
                    }
                };
                _controles.Add(factions[i-1]);
                ybouton += 80;
            }         
         }

    }
}
