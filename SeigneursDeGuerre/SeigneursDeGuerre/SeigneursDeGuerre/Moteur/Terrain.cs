using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    /// Service de gestion des terrains
    /// </summary>
    class Terrain {
        /// <summary>
        /// La description sous-jacente
        /// </summary>
        private TerrainDescription description;
        /// <summary>
        /// Instance partagée du jeu
        /// </summary>
        private Jeu jeu;

        /// <summary>
        /// La première case affichée dans la carte, en pixels (diviser par blockSize pour la pos dans la map)
        /// </summary>
        public Point offsetCarte;

        private Texture2D texCollines;
        private Texture2D texEau1;
        private Texture2D texEau2;
        private Texture2D texHerbe;
        private Texture2D texMarais;
        private Texture2D texMontagne;
        private Texture2D texSapin;
        private Texture2D texArbre;
        private Texture2D texRoseau1;
        private Texture2D texRoseau2;
        private Texture2D texPlageVH;
        private Texture2D texPlageVM;
        private Texture2D texPlageVB;
        private Texture2D texPlageHG;
        private Texture2D texPlageHM;
        private Texture2D texPlageHD;

        private Texture2D texCheminG;
        private Texture2D texCheminD;
        private Texture2D texCheminH;
        private Texture2D texCheminB;
        private Texture2D texCheminGD;
        private Texture2D texCheminHB;
        private Texture2D texCheminGH;
        private Texture2D texCheminGB;
        private Texture2D texCheminDH;
        private Texture2D texCheminDB;
        private Texture2D texCheminGDH;
        private Texture2D texCheminGDB;
        private Texture2D texCheminGHB;
        private Texture2D texCheminDHB;
        private Texture2D texCheminGDHB;

        private Texture2D texRuine;

        private Texture2D texFondMer;

        /// <summary>
        /// image de l'eau à afficher
        /// </summary>
        private double waterAnimationOffset;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="leJeu"></param>
        public Terrain(Jeu leJeu) {
            jeu = leJeu;
            offsetCarte.X = 0;
            offsetCarte.Y = 0;
            waterAnimationOffset = 0;
        }

        /// <summary>
        /// Accesseurs sur la descritption
        /// </summary>
        public TerrainDescription terrainDesc {
            get { return description; }
            set { description = value; }
        }


        /// <summary>
        /// Loader de contenu 
        /// </summary>
        /// <param name="content"></param>
        public void load(ContentManager content) {
            texCollines = content.Load<Texture2D>("terrain/collines1");
            texEau1 = content.Load<Texture2D>("terrain/eau1");
            texEau2 = content.Load<Texture2D>("terrain/eau2");
            texHerbe = content.Load<Texture2D>("terrain/herbe");
            texMarais = content.Load<Texture2D>("terrain/marais");
            texMontagne = content.Load<Texture2D>("terrain/montagne");
            texSapin = content.Load<Texture2D>("terrain/sapin");
            texArbre = content.Load<Texture2D>("terrain/arbre");
            texRoseau1 = content.Load<Texture2D>("terrain/roseaux");
            texRoseau2 = content.Load<Texture2D>("terrain/roseaux2");
            

            texPlageVH = content.Load<Texture2D>("terrain/plageVH");
            texPlageVM = content.Load<Texture2D>("terrain/plageVM");
            texPlageVB = content.Load<Texture2D>("terrain/plageVB");
            texPlageHG = content.Load<Texture2D>("terrain/plageHG");
            texPlageHM = content.Load<Texture2D>("terrain/plageHM");
            texPlageHD = content.Load<Texture2D>("terrain/plageHD");

            texCheminG = content.Load<Texture2D>("terrain/routeG");
            texCheminD = content.Load<Texture2D>("terrain/routeD");
            texCheminH = content.Load<Texture2D>("terrain/routeH");
            texCheminB = content.Load<Texture2D>("terrain/routeB");
            texCheminGD = content.Load<Texture2D>("terrain/routeGD");
            texCheminHB = content.Load<Texture2D>("terrain/routeHB");
            texCheminGH = content.Load<Texture2D>("terrain/routeGH");
            texCheminGB = content.Load<Texture2D>("terrain/routeGB");
            texCheminDH = content.Load<Texture2D>("terrain/routeDH");
            texCheminDB = content.Load<Texture2D>("terrain/routeDB");
            texCheminGDH = content.Load<Texture2D>("terrain/routeGDH");
            texCheminGDB = content.Load<Texture2D>("terrain/routeGDB");
            texCheminGHB = content.Load<Texture2D>("terrain/routeGHB");
            texCheminDHB = content.Load<Texture2D>("terrain/routeDHB");
            texCheminGDHB = content.Load<Texture2D>("terrain/routeGDHB");

            texRuine = content.Load<Texture2D>("terrain/ruine");

            texFondMer = content.Load<Texture2D>("terrain/fondMer");            

        }


        /// <summary>
        /// Affiche le fond de carte (sans les villes, sans les unités, avec les routes & les ruines)
        /// Appelé au sein d'un spriteBatch begin/end
        /// </summary>
        /// <param name="spriteBatch">spriteBatch du jeu</param>
        /// <param name="GraphicsDevice">gd du jeu</param>
        /// <param name="gameTime">temps du jeu</param>
        public void draw(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime) {

            // Fonde de mer
             spriteBatch.Draw(texFondMer, new Rectangle(-offsetCarte.X, -offsetCarte.Y, jeu.blockSize * terrainDesc.getLargeur(), jeu.blockSize * terrainDesc.getHauteur()),
                null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.99f);

            // Affiche la carte à partir de offsetCarte
            for (int y = offsetCarte.Y - jeu.blockSize *2 ; y < (offsetCarte.Y + jeu.resY + jeu.blockSize *3); y+= jeu.blockSize) {
                for (int x = offsetCarte.X - jeu.blockSize *2; x < (offsetCarte.X + jeu.resX + jeu.blockSize * 3); x += jeu.blockSize) {

                    Texture2D texture;
                    Texture2D[] textureSpray = null;
                    bool eau = false;
                    int sprayNumber = 0;
                    float sprayScale = 1f;
                    int xCell = x / jeu.blockSize;
                    int yCell = y / jeu.blockSize;

                    TerrainDescription.TerrainCell cellM = terrainDesc.getCellAt(xCell, yCell);
                    TerrainDescription.TerrainCell cellG = terrainDesc.getCellAt(xCell - 1, yCell);
                    TerrainDescription.TerrainCell cellD = terrainDesc.getCellAt(xCell + 1, yCell);
                    TerrainDescription.TerrainCell cellH = terrainDesc.getCellAt(xCell, yCell - 1);
                    TerrainDescription.TerrainCell cellB = terrainDesc.getCellAt(xCell, yCell + 1);

                    TerrainDescription.TypeTerrain typeM = cellM.type;
                    TerrainDescription.TypeTerrain typeG = cellG.type;
                    TerrainDescription.TypeTerrain typeH = cellH.type;

                    switch (typeM) {
                        case TerrainDescription.TypeTerrain.COLLINE :
                            texture = texHerbe; //texCollinesFond;
                            textureSpray = new Texture2D[] {texCollines};
                            sprayNumber = 3;
                            sprayScale = 2;
                            break;
                        case TerrainDescription.TypeTerrain.EAU :
                            eau = true;
                            texture = texEau1;
                            break;
                        case TerrainDescription.TypeTerrain.FORET:
                            texture = texHerbe;
                            textureSpray = new Texture2D[] {texSapin,texArbre};
                            sprayNumber = 8;
                            sprayScale = 1.5f;
                            break;
                        case TerrainDescription.TypeTerrain.HERBE:
                            texture = texHerbe;
                            break;                    
                        case TerrainDescription.TypeTerrain.MARECAGE:
                            texture = texMarais;
                            textureSpray = new Texture2D[] {texRoseau1,texRoseau2};
                            sprayNumber = 6;
                            sprayScale = 1;
                            break;
                        case TerrainDescription.TypeTerrain.MONTAGNE:
                            texture = texHerbe;
                            textureSpray = new Texture2D[] {texMontagne};
                            sprayNumber = 2;
                            sprayScale = 2.5f;
                            break;
                        default:
                            texture = jeu.pixelBlanc;
                            break;
                    }
                    // Détermine le rectangle source. 256 = 4 blocs (texture.Width / jeu.blockSize)

                    Rectangle sourceRectangle;
                    int decalageX = ((x / jeu.blockSize) % (texture.Width / jeu.blockSize)) * jeu.blockSize;
                    int decalageY = ((y / jeu.blockSize) % (texture.Width / jeu.blockSize)) * jeu.blockSize;


                    if (eau) {
                        int ytexture;
                        if (waterAnimationOffset < 16) {
                            texture = texEau1;
                            ytexture = (int)waterAnimationOffset * 256;
                        }
                        else {
                            texture = texEau2;
                            ytexture = ((int)waterAnimationOffset-16) * texture.Width;
                        }
                        sourceRectangle = new Rectangle(decalageX, decalageY + ytexture, jeu.blockSize, jeu.blockSize);
                    }
                    else {
                        sourceRectangle = new Rectangle(decalageX, decalageY, jeu.blockSize, jeu.blockSize);
                    }
                    int drawX = x - offsetCarte.X - (offsetCarte.X % jeu.blockSize);
                    int drawY = y - offsetCarte.Y - (offsetCarte.Y % jeu.blockSize);
                    spriteBatch.Draw(texture, new Rectangle(drawX, drawY, jeu.blockSize, jeu.blockSize), sourceRectangle, Color.White, 0,Vector2.Zero, SpriteEffects.None,0.9f);
                 
                    // Affiche des arbres ou des collines / montagnes (spray)
                    if (sprayNumber != 0) {
                        Random rnd = new Random((x / jeu.blockSize) + 10000 * (y / jeu.blockSize));
                        for (int i = 0; i < sprayNumber; i++) {
                            int xspray = rnd.Next(jeu.blockSize);
                            int yspray = rnd.Next(jeu.blockSize);
                            int  wspray = (int)(sprayScale * (float)(jeu.blockSize / 2 + rnd.Next(jeu.blockSize / 2)));
                            int itex = rnd.Next(textureSpray.Length);
                            SpriteEffects flip = rnd.Next(2) == 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                            spriteBatch.Draw(textureSpray[itex], new Rectangle(drawX + xspray - wspray / 2, drawY + yspray - wspray, wspray, wspray), null, Color.White, 0, Vector2.Zero,
                                flip, getZPos(drawX + xspray - wspray / 2, drawY + yspray - wspray, 0.7f, 0.8f));
                        }
                    }

                    // Plage Verticale gauche
                    if (((typeM == TerrainDescription.TypeTerrain.EAU) && (typeG != TerrainDescription.TypeTerrain.EAU))
                        || ((typeM != TerrainDescription.TypeTerrain.EAU) && (typeG == TerrainDescription.TypeTerrain.EAU))) {

                            spriteBatch.Draw(texPlageVM, new Rectangle(drawX - jeu.blockSize / 2, drawY,
                                            jeu.blockSize, jeu.blockSize), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.85f);


                            spriteBatch.Draw(texPlageVH, new Rectangle(drawX - jeu.blockSize / 2, drawY - jeu.blockSize / 2, 
                                            jeu.blockSize, jeu.blockSize), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.85f);

                            spriteBatch.Draw(texPlageVB, new Rectangle(drawX - jeu.blockSize / 2, drawY + jeu.blockSize / 2,
                                jeu.blockSize, jeu.blockSize), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.85f);

                    }
                    // Plage horizontale haute
                    if (((typeM == TerrainDescription.TypeTerrain.EAU) && (typeH != TerrainDescription.TypeTerrain.EAU))
                        || ((typeM != TerrainDescription.TypeTerrain.EAU) && (typeH == TerrainDescription.TypeTerrain.EAU))) {

                            spriteBatch.Draw(texPlageHM, new Rectangle(drawX, drawY - jeu.blockSize / 2,
                                jeu.blockSize, jeu.blockSize), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.85f);

                            spriteBatch.Draw(texPlageHG, new Rectangle(drawX - jeu.blockSize / 2, drawY - jeu.blockSize / 2,
                                jeu.blockSize, jeu.blockSize), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.85f);

                            spriteBatch.Draw(texPlageHD, new Rectangle(drawX + jeu.blockSize / 2, drawY - jeu.blockSize / 2,
                                jeu.blockSize, jeu.blockSize), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.85f);
                    }

                    // Routes
                    if (cellM.route) {
                        // Type de route
                        Texture2D texRoute = null;
                        if (cellG.route && cellD.route && cellH.route && cellB.route) {
                            texRoute = texCheminGDHB;
                        }
                        else if (cellG.route && !cellD.route && cellH.route && cellB.route) {
                            texRoute = texCheminGHB;
                        }
                        else if (cellG.route && cellD.route && !cellH.route && cellB.route) {
                            texRoute = texCheminGDB;
                        }
                        else if (cellG.route && cellD.route && cellH.route && !cellB.route) {
                            texRoute = texCheminGDH;
                        }
                        else if (!cellG.route && cellD.route && cellH.route && cellB.route) {
                            texRoute = texCheminDHB;
                        }
                        else if (cellG.route && !cellD.route && !cellH.route && cellB.route) {
                            texRoute = texCheminGB;
                        }
                        else if (cellG.route && !cellD.route && cellH.route && !cellB.route) {
                            texRoute = texCheminGH;
                        }
                        else if (cellG.route && cellD.route && !cellH.route && !cellB.route) {
                            texRoute = texCheminGD;
                        }
                        else if (!cellG.route && !cellD.route && cellH.route && cellB.route) {
                            texRoute = texCheminHB;
                        }
                        else if (!cellG.route && cellD.route && !cellH.route && cellB.route) {
                            texRoute = texCheminDB;
                        }
                        else if (!cellG.route && cellD.route && cellH.route && !cellB.route) {
                            texRoute = texCheminDH;
                        }
                        else if (cellG.route && !cellD.route && !cellH.route && !cellB.route) {
                            texRoute = texCheminG;
                        }
                        else if (!cellG.route && cellD.route && !cellH.route && !cellB.route) {
                            texRoute = texCheminD;
                        }
                        else if (!cellG.route && !cellD.route && !cellH.route && cellB.route) {
                            texRoute = texCheminB;
                        }
                        else if (!cellG.route && !cellD.route && cellH.route && !cellB.route) {
                            texRoute = texCheminH;
                        }
                        else if (!cellG.route && !cellD.route && !cellH.route && !cellB.route) {
                            texRoute = null; // route n'allant nulle part
                        }
                        if (texRoute != null) {
                            spriteBatch.Draw(texRoute, new Rectangle(drawX, drawY,jeu.blockSize, jeu.blockSize), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.69f);
                        }

                    }
                    // Ruines
                    if (cellM.ruine) {
                        spriteBatch.Draw(texRuine, new Rectangle(drawX, drawY, jeu.blockSize, jeu.blockSize), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.68f);
                    }

                }
            }
            // animation de l'eau
            waterAnimationOffset += (double)(gameTime.ElapsedGameTime.Milliseconds) / 60.0;
            waterAnimationOffset %= 32;
        }

        /// <summary>
        /// S'assure de la visibilité du point
        /// </summary>
        /// <param name="pos"></param>
        public void zoomSur(Point pos) {
            offsetCarte.X = pos.X - jeu.resX / 2;
            offsetCarte.Y = pos.Y - jeu.resY / 2;
            normalizeScrolling();            
        }

        /// <summary>
        /// récupère la position Z en fonction de x et y
        /// </summary>
        /// <param name="y">x</param>
        /// <param name="y">y</param>
        /// <param name="max">z max (arrière)</param>
        /// <returns>le z</returns>
        private float getZPos(float x, float y, float min, float max) {
            return max - ((y + 300.0f + ((x + 300.0f)/(jeu.resX + 600.0f))) / (jeu.resY + 601.0f) * (max - min));
        }


        /// <summary>
        /// S'assure qu'on ne scrolle pas en dehors de la carte
        /// </summary>
        public void normalizeScrolling() {
            // limites - on commence par les limites max car les limites min sont plus importantes
            if (offsetCarte.X > (terrainDesc.getLargeur() * jeu.blockSize) - jeu.resX) {
                offsetCarte.X = (terrainDesc.getLargeur() * jeu.blockSize) - jeu.resX;
            }
            if (offsetCarte.Y > (terrainDesc.getHauteur() * jeu.blockSize) - jeu.resY) {
                offsetCarte.Y = (terrainDesc.getHauteur() * jeu.blockSize) - jeu.resY;
            }

            if (offsetCarte.X < 0) {
                offsetCarte.X = 0;
            }
            if (offsetCarte.Y < 0) {
                offsetCarte.Y = 0;
            }
        }
    }
}
