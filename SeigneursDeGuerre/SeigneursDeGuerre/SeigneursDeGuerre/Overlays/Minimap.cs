using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SeigneursDeGuerre.Moteur;

namespace SeigneursDeGuerre.Overlays {
    class Minimap : Overlay {
        
        // taille d'une cellule de carte
        public const int TAILLE_CELL = 3;
        private const int ALPHA = 160;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="leJeu">jeu</param>
        /// <param name="position">position</param>
        /// <param name="width">largeur</param>
        /// <param name="height">hauteur</param>
        public Minimap(Jeu leJeu, Position position) : base(leJeu, position,
                                        leJeu.terrain.terrainDesc.getLargeur() * TAILLE_CELL, 
                                        leJeu.terrain.terrainDesc.getHauteur() * TAILLE_CELL, null, null) {

            // Change la couleur du fond (bleu)
            this._colorFond = Color.FromNonPremultiplied(0, 50, 128, 200);
        }


        /// <summary>
        /// Affiche l'overlay
        /// Appelé au sein d'un spriteBatch begin/end
        /// </summary>
        /// <param name="spriteBatch">spriteBatch du jeu</param>
        /// <param name="GraphicsDevice">gd du jeu</param>
        /// <param name="gameTime">temps du jeu</param>
        public override void draw(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime) {
            // Appelle la base
            baseDraw(spriteBatch, GraphicsDevice, gameTime);
            // Affiche la carte - ignore l'eau
            for (int y = 0; y < _jeu.terrain.terrainDesc.getHauteur(); y++) {
                for (int x = 0; x < _jeu.terrain.terrainDesc.getLargeur(); x++) {
                    bool drawSomething = true;
                    Color color1 = _colorFond;
                    int taille = TAILLE_CELL;
                    TerrainDescription.TerrainCell cell = _jeu.terrain.terrainDesc.getCellAt(x,y);
                    if (cell.ville) {
                        // dépend de la faction
                        color1 = _jeu.factions.getFaction(_jeu.villes.villeDesc[cell.villeIndex].faction).couleur;
                        taille = TAILLE_CELL * 2;
                    }
                    else if (cell.ruine) {
                        color1 = Color.FromNonPremultiplied(255, 255, 255, 255);
                    }
                    else if (cell.route) {
                        color1 = Color.FromNonPremultiplied(165, 42, 42, ALPHA);
                    }
                    else {
                        switch (cell.type) {
                            case TerrainDescription.TypeTerrain.COLLINE:
                                color1 = Color.FromNonPremultiplied(255, 165, 0, ALPHA);
                                break;
                            case TerrainDescription.TypeTerrain.FORET:
                                color1 = Color.FromNonPremultiplied(0, 100, 0, ALPHA);
                                break;
                            case TerrainDescription.TypeTerrain.HERBE:
                                color1 = Color.FromNonPremultiplied(0, 128, 0, ALPHA);
                                break;
                            case TerrainDescription.TypeTerrain.MARECAGE:
                                color1 = Color.FromNonPremultiplied(143, 188, 139, ALPHA);
                                break;
                            case TerrainDescription.TypeTerrain.MONTAGNE:
                                color1 = Color.FromNonPremultiplied(128, 128, 128, ALPHA);
                                break;
                            case TerrainDescription.TypeTerrain.EAU:
                                drawSomething = false;
                                break;
                        }
                    }
                    if (drawSomething) {
                        spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(_xoverlay + x * TAILLE_CELL, 
                            _yoverlay + y * TAILLE_CELL, taille, taille), null, color1,0,Vector2.Zero,SpriteEffects.None,(taille == TAILLE_CELL ? 0.99f : 0.98f));
                    }
                }
            }
            // Affiche le cadre autour de la vision courante
            int xecran = (_jeu.terrain.offsetCarte.X / _jeu.blockSize) * TAILLE_CELL + _xoverlay;
            int yecran = (_jeu.terrain.offsetCarte.Y / _jeu.blockSize) * TAILLE_CELL + _yoverlay;
            int wecran = (_jeu.resX / _jeu.blockSize) * TAILLE_CELL + 1;
            int hecran = (_jeu.resY / _jeu.blockSize) * TAILLE_CELL + 1;

            spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(xecran, yecran, wecran, 1), null, Color.LightCoral,0, Vector2.Zero, SpriteEffects.None, 0.97f);
            spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(xecran, yecran, 1, hecran), null, Color.LightCoral, 0, Vector2.Zero, SpriteEffects.None, 0.97f);
            spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(xecran, yecran + hecran, wecran + 1, 1), null, Color.LightCoral, 0, Vector2.Zero, SpriteEffects.None, 0.97f);
            spriteBatch.Draw(_jeu.pixelBlanc, new Rectangle(xecran + wecran, yecran, 1, hecran + 1), null, Color.LightCoral, 0, Vector2.Zero, SpriteEffects.None, 0.97f);


            
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
            if (_jeu.factions.getFaction(_jeu.tourFaction).humanPlayer) {
                // scrolling au milieu
                if (!released && leftClick) {
                    _jeu.terrain.offsetCarte.X = ((x - _xoverlay) / TAILLE_CELL) * _jeu.blockSize - _jeu.resX / 2;
                    _jeu.terrain.offsetCarte.Y = ((y - _yoverlay) / TAILLE_CELL) * _jeu.blockSize - _jeu.resY / 2;
                    _jeu.terrain.normalizeScrolling();
                }
            }
        }

    }
}
