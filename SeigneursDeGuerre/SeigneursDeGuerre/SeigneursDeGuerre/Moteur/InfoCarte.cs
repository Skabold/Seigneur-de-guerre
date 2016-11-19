using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SeigneursDeGuerre.Overlays;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    /// Classe gérant les clicks droit sur la carte (info sur terrain ou armée)
    /// </summary>
    class InfoCarte {
        /// <summary>
        /// Le jeu
        /// </summary>
        private Jeu _jeu;

        /// <summary>
        /// Créateur
        /// </summary>
        /// <param name="leJeu">jeu</param>
        public InfoCarte(Jeu leJeu) {
            _jeu = leJeu;
        }

        /// <summary>
        /// Créer une pop up d'info sur click droit
        /// </summary>
        /// <param name="mouseX"></param>
        /// <param name="mouseY"></param>
        public void createInfoPopup(int mouseX, int mouseY) {
            // Détermine où on a cliqué : terrain, ville, armée...

            float extraHeight = 0;
            int extraWidth = 0;
            Armee armeeClick = null;
            VilleDescription villeClick = null;
            TerrainDescription.TerrainCell cellClick = getClicked(mouseX, mouseY, ref armeeClick, ref villeClick);
            string titre = "(bug)";

            // Priorité aux armées
            if (armeeClick != null) {
                titre = armeeClick.proue.vraiNom + " (" + _jeu.factions.getFaction(armeeClick.faction).nom + ")";
                extraHeight = Creatures.CREATURE_SIZE + PopWindow.MARGE_TITRE;
                extraWidth = Creatures.CREATURE_SIZE * Armee.MAX_TAILLE + PopWindow.MARGE_TITRE * 2;
            }
            // puis les villes
            else if (villeClick != null) {
                titre = villeClick.nom  +" (" + _jeu.factions.getFaction(villeClick.faction).nom + ")";
                extraHeight = -2.5f;
                extraWidth = 300;
            }
            // Enfin le terrain
            else {
                // Ruines
                if (cellClick.ruine) {
                    int carteX = (mouseX + _jeu.terrain.offsetCarte.X) / _jeu.blockSize;
                    int carteY = (mouseY + _jeu.terrain.offsetCarte.Y) / _jeu.blockSize;
                    Ruines.RuineDescription desc = _jeu.ruines.ruines[new Point(carteX,carteY)];
                    titre = "Ruine de " + desc.nom + " (" + (desc.visite ? "déjà visité" : "non exploré") + ")";

                }
                else if (cellClick.route) {
                    titre = "Route - le chemin le plus rapide";
                }
                else {
                    switch (cellClick.type) {
                        case TerrainDescription.TypeTerrain.COLLINE:
                            titre = "Collines - difficile de les traverser";
                            break;
                        case TerrainDescription.TypeTerrain.EAU:
                            titre = "Eau - il vous faut ou voler, nager ou prendre un bateau";
                            break;
                        case TerrainDescription.TypeTerrain.FORET:
                            titre = "Forêt - difficile de la traverser";
                            break;
                        case TerrainDescription.TypeTerrain.HERBE:
                            titre = "Plaine - le terrain le plus rapide après la route";
                            break;
                        case TerrainDescription.TypeTerrain.MARECAGE:
                            titre = "Marécages - difficile de les traverser";
                            break;
                        case TerrainDescription.TypeTerrain.MONTAGNE:
                            titre = "Montagnes - A moins de les survoler, on ne peut les traverser";
                            break;
                    }
                }
            }
            // Taille du texte
            Vector2 fontSize = _jeu.font.MeasureString(titre);

            Point taille = PopWindow.getPopWindowSize(_jeu, titre);
            
            // Ajoute un bord
            if (taille.X < extraWidth) {
                taille.X = extraWidth;
            }
            if (extraHeight < 0) {
                taille.Y = (int)((float)taille.Y * (-extraHeight));
            }
            else {
                taille.Y = (int)((float)taille.Y + extraHeight);
            }
            PopWindow info = new PopWindow(_jeu, Overlay.Position.SOURIS, taille.X, taille.Y, mouseX, mouseY, titre);
            
            // Custom Draw pour afficher l'armée s'il le faut
            if (armeeClick != null) {
                info.drawCallBack = delegate(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime) {
                    int x = info.xoverlay + _jeu.terrain.offsetCarte.X + PopWindow.MARGE_TITRE/2;
                    int y = info.yoverlay + info.height - Creatures.CREATURE_SIZE - PopWindow.MARGE_TITRE/2 + _jeu.terrain.offsetCarte.Y;
                    for (int i = 0; i< armeeClick.getTaille(); i++) {
                        Creature crea = armeeClick.getCreature(i);
                        _jeu.creatures.draw(spriteBatch, GraphicsDevice, gameTime, x, y, 0f, crea.typeCreature);
                        x += Creatures.CREATURE_SIZE;
                    }
                };
            }
            // Custom draw pour les villes
            else if (villeClick != null) {
                info.drawCallBack = delegate(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime) {
                    float x = info.xoverlay + PopWindow.MARGE_TITRE/2;
                    float y = info.yoverlay + PopWindow.MARGE_TITRE + fontSize.Y;
                    spriteBatch.DrawString(_jeu.font, "Défense :" + villeClick.niveauDefense, new Vector2(x, y), Color.BlanchedAlmond);
                    if (villeClick.capitaleFaction != 0) {
                        y += fontSize.Y;
                        spriteBatch.DrawString(_jeu.font, "Capitale de : " + _jeu.factions.getFaction(villeClick.capitaleFaction).nom, new Vector2(x, y),
                            _jeu.factions.getFaction(villeClick.capitaleFaction).couleur);
                    }
                };
            }

            _jeu.addOverlay(info);
            _jeu.curseur.forme = Cursor.FormeCurseur.INTERROGATION;
        }

        /// <summary>
        /// Supprime la pop up
        /// </summary>
        public void destroyInfoPopup() {
            _jeu.popOverlay();
            _jeu.curseur.forme = Cursor.FormeCurseur.FLECHE;
        }

        /// <summary>
        /// Retourne ce sur quoi le curseur de souris a cliqué
        /// </summary>
        /// <param name="mouseX">position X du curseur</param>
        /// <param name="mouseY">position Y du curseur</param>
        /// <param name="armeeClick">si non null, l'armée cliquée</param>
        /// <param name="villeClick">si non null, la ville cliquée</param>
        /// <returns>la cellule du terrain cliqué</returns>
        public TerrainDescription.TerrainCell getClicked(int mouseX, int mouseY, ref Armee armeeClick, ref VilleDescription villeClick) {
            int carteX = (mouseX + _jeu.terrain.offsetCarte.X) / _jeu.blockSize;
            int carteY = (mouseY + _jeu.terrain.offsetCarte.Y) / _jeu.blockSize;
            armeeClick = null;
            villeClick = null;

            // Une armée se trouve ici ?

            foreach (Armee armee in _jeu.armees) {
                if (armee.positionCarte.X == carteX && armee.positionCarte.Y == carteY) {
                    armeeClick = armee;
                    break;
                }
            }
            // Une ville se trouve ici ?
            foreach (VilleDescription vd in _jeu.villes.villeDesc) {
                if (carteX >= vd.positionMap.X && carteX <= (vd.positionMap.X + 1) &&
                    carteY >= vd.positionMap.Y && carteY <= (vd.positionMap.Y + 1)) {
                    villeClick = vd;
                    break;
                }
            }

            return _jeu.terrain.terrainDesc.getCellAt(carteX, carteY);
        }

    }
}
