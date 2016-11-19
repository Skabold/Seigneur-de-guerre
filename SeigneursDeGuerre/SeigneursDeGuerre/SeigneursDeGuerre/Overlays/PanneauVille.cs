using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SeigneursDeGuerre.Moteur;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SeigneursDeGuerre.Overlays {
    class PanneauVille : PopWindow {
        
        private VilleDescription _ville;
        private Texture2D[] texProd;

        public PanneauVille(Jeu _jeu, VilleDescription ville)
            : base(_jeu, Overlay.Position.CENTRE, 360 + 16 + 16, 550, 0, 0, "Gestion de ") {
                _ville = ville;
                _titre += ville.nom;
                _modalOverlay = ModalOverlay.INFO_VILLE;
            
                // Textures des créatures produites dans la ville
                texProd = new Texture2D[ville.typeCreatures.Length];
                Bouton[] productions = new Bouton[ville.typeCreatures.Length];
                for (int i=0;i<texProd.Length;i++) {
                    texProd[i] = new Texture2D(_jeu.graphicsDevice, Creatures.CREATURE_SIZE, Creatures.CREATURE_SIZE);
                    // Copie la créature concernée dedans                    
                    texProd[i].SetData(_jeu.creatures.getTextureData(ville.typeCreatures[i]));
                    productions[i] = new Bouton(_jeu,_xoverlay + 16, _yoverlay + 64 + (12 + Creatures.CREATURE_SIZE) * i,"",texProd[i],_jeu.isoFont);
                    productions[i].userData = i;
                    productions[i].click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                        if (released) {
                            ville.productionCourante = ((Bouton)clicked).userData;                            
                        }
                    };
                    _controles.Add(productions[i]);
                }

                Bouton stopProd = new Bouton(_jeu, _xoverlay + 116, _yoverlay + 64 + (12 + Creatures.CREATURE_SIZE) * texProd.Length + 20 + 40 + 28 
                    , "Ne rien produire", null, _jeu.font);
                stopProd.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                    if (released) {
                        ville.productionCourante = -1;
                    }
                };
                _controles.Add(stopProd);

                Bouton plusDef = new Bouton(_jeu, _xoverlay + 16, _yoverlay + 64 + (12 + Creatures.CREATURE_SIZE) * texProd.Length + 20 + 40 + 28 + 20 + Creatures.CREATURE_SIZE + 15 
                    , "Augmenter la défense et la productivité", null, _jeu.font);
                plusDef.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                    if (released) {
                        // Augmente le niveau de défense
                        if ((_ville.niveauDefense < 40) && (_jeu.factions.getFaction(_ville.faction).or >= _ville.orPourNiveauSuivant)) {
                            _jeu.factions.getFaction(_ville.faction).or -= _ville.orPourNiveauSuivant;
                            _ville.niveauDefense = _ville.niveauDefense+10;
                            _ville.niveauProductivite = ville.niveauProductivite + 1;
                        }
                    }
                };
                _controles.Add(plusDef);


                Bouton OK = new Bouton(_jeu, _xoverlay + 16+116, _yoverlay + 64 + (12 + Creatures.CREATURE_SIZE) * texProd.Length + 20 + 40 + 28 + 20 + Creatures.CREATURE_SIZE + 28 + 10 + 40
                , " Valider ", null, _jeu.isoFont);
                OK.click = delegate(Controle clicked, int x, int y, bool leftClick, bool rightClick, bool released) {
                    if (released) {
                        _jeu.popOverlay();
                    }
                };
                _controles.Add(OK);

                _height = 25 + 64 + (12 + Creatures.CREATURE_SIZE) * texProd.Length + 20 + 40 + 28 + 20 + Creatures.CREATURE_SIZE + 28 + 10 + 60;

                drawCallBack = delegate(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime) {
                    // Texte                    
                    drawOutlinedString(spriteBatch,_jeu.font, "~ Productions disponibles ~",new Vector2(_xoverlay + 8 , _yoverlay + 30) , Color.White, Color.DarkBlue);

                    for (int i = 0; i < texProd.Length; i++) {
                        // texte et profil de l'armée en production
                        drawOutlinedString(spriteBatch,_jeu.font, _jeu.creatures.description[_ville.typeCreatures[i]].nom,
                             new Vector2(_xoverlay + 16 + Creatures.CREATURE_SIZE + 8, _yoverlay + 64 + (12 + Creatures.CREATURE_SIZE) * i), Color.White, Color.Crimson);

                        spriteBatch.DrawString(_jeu.font, _jeu.creatures.description[_ville.typeCreatures[i]].profilAsStr(),
                             new Vector2(_xoverlay + 16 + Creatures.CREATURE_SIZE + 8, _yoverlay + 64 + (12 + Creatures.CREATURE_SIZE) * i + 20), Color.White);
                        
                        int nbTours = _ville.geNbToursPourCreatures(_jeu.creatures.description[_ville.typeCreatures[i]].cout);
                        spriteBatch.DrawString(_jeu.font, "Coût : " + _jeu.creatures.description[_ville.typeCreatures[i]].cout
                            + " - " +  nbTours + " tour" + (nbTours > 1 ? "s" : ""),
                             new Vector2(_xoverlay + 16 + Creatures.CREATURE_SIZE + 8, _yoverlay + 64 + (12 + Creatures.CREATURE_SIZE) * i + 40), Color.White);

                    }
                    int ynext = _yoverlay + 64 + (12 + Creatures.CREATURE_SIZE) * texProd.Length + 20;
                    drawOutlinedString(spriteBatch, _jeu.font, "~ Production courante ~", new Vector2(_xoverlay + 8, ynext), Color.White, Color.DarkBlue);
                    
                    ynext += 20;
                    int revenusVille = _ville.getRevenus();
                    spriteBatch.DrawString(_jeu.font, "Revenus de la ville : " + revenusVille,
                                 new Vector2(_xoverlay + 16 + 100, ynext), Color.White);

                    ynext += 20;
                    if (_ville.productionCourante != -1) {
                        spriteBatch.Draw(texProd[_ville.productionCourante], new Rectangle(_xoverlay + 16, ynext, Creatures.CREATURE_SIZE, Creatures.CREATURE_SIZE), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.9f);
                        int nbToursPourCrea = _ville.geNbToursPourCreatures(_jeu.creatures.description[_ville.typeCreatures[_ville.productionCourante]].cout);
                        int toursRestants = (nbToursPourCrea - _ville.productionPoints);
                        if (toursRestants < 1) {
                            toursRestants = 1;
                        }
                        spriteBatch.DrawString(_jeu.font, "Tours restants : " + toursRestants,
                                     new Vector2(_xoverlay + 16 + 100, ynext), Color.White);

                    }
                    else {
                        drawOutlinedString(spriteBatch, _jeu.font, "Aucune", new Vector2(_xoverlay + 16, ynext), Color.White, Color.DarkCyan);
                    }


                    ynext += Creatures.CREATURE_SIZE + 28;
                    drawOutlinedString(spriteBatch, _jeu.font, "~ Niveau de défense actuel : " + _ville.niveauDefense + " ~", new Vector2(_xoverlay + 8, ynext), Color.White, Color.DarkBlue);
                    ynext += 30;

                    spriteBatch.DrawString(_jeu.font, "Coût : " + _ville.orPourNiveauSuivant,
                         new Vector2(_xoverlay + 16 + 190, ynext), Color.White);

                    ynext += 20;
                    spriteBatch.DrawString(_jeu.font, "Or disponible : " + _jeu.factions.getFaction(_ville.faction).or,
                         new Vector2(_xoverlay + 16 + 190, ynext), Color.White);

                };
                _jeu.curseur.forme = Cursor.FormeCurseur.FLECHE;
        }
        
 
    }
}
