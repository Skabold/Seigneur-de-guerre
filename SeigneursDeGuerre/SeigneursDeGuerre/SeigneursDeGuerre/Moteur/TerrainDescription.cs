using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    /// Classe décrivant le terrain (données sans traitement associé)
    /// </summary>
    class TerrainDescription {
        /// <summary>
        /// Les types de terrains possibles pour une case du jeu
        /// </summary>        
        /// 

        // Stockage -------------------------------------------------------------------------------------------------
        public enum TypeTerrain {
            EAU, // mer & fleuve
            HERBE, // par défaut
            COLLINE, // colline
            MONTAGNE, // montagne
            MARECAGE, // marécage
            FORET // forêt
        };
        
        
        /// <summary>
        /// Structure d'élément de carte
        /// </summary>
        public struct TerrainCell {
            // Type du terrain
            public TypeTerrain type;
            // true si une route passe ici
            public bool route;
            // index de la ville (ou -1 si pas de ville à cet endroit)
            public int villeIndex;            
            // index de la ruine ou -1 si pas de ruine à cet endroit
            public int ruineIndex;
            /// <summary>
            /// return true si il y a une ruine à cet endroit
            /// </summary>
            public bool ruine {
                get { return ruineIndex != -1; }
            }
            /// <summary>
            /// return true si il y a une ville à cet endroit
            /// </summary>
            public bool ville {
                get { return villeIndex != -1; }
            }
            public TerrainCell(TypeTerrain typeTerrain) {
                type = typeTerrain;
                route = false;
                villeIndex = -1;
                ruineIndex = -1;
            }
        }

        /// <summary>
        /// Largeur de la map en cases
        /// </summary>
        private int largeur;
        /// <summary>
        /// hauteur de la map en cases
        /// </summary>
        private int hauteur;
        /// <summary>
        /// elements de la map (x,y)
        /// </summary>
        private TerrainCell[,] map;

        /// <summary>
        /// stocke les positions des villes par index
        /// </summary>
        private List<Microsoft.Xna.Framework.Point> _villePos = new List<Microsoft.Xna.Framework.Point>();

        /// <summary>
        /// nombre de villes trouvées sur la carte
        /// </summary>
        private int _nombreVilles;

        /// <summary>
        /// nombre de ruines trouvées sur la carte
        /// </summary>
        private int _nombreRuines;

        
        // Constructeurs -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructeur vide
        /// </summary>
        /// <param name="largeur">largeur du terrain</param>
        /// <param name="hauteur">hauteur du terrain</param>
        public TerrainDescription(int largeur, int hauteur) {
            this.largeur = largeur;
            this.hauteur = hauteur;
            _nombreRuines = 0;
            _nombreVilles = 0;
            map = new TerrainCell[largeur, hauteur];
        }

        /// <summary>
        /// Constructeur à partir d'une carte sous forme de bitmap
        /// Chaque case fait 2 x 2 pixels
        /// 
        /// Code couleurs pixels :
        /// BLEU 0x0000FF = eau
        /// VERT 0x00FF00 = herbe
        /// ORANGE 0xFF8000 = colline
        /// GRIS 0x808080 = montagne
        /// TURQUOISE 0x00FFFF = marécage
        /// VERT FONCE 0x008000 = forêt
        /// 
        /// 
        /// Si un des 4 pixels possède est en plus de couleur ROUGE 0xFF0000 une route passe dessus
        ///                                                   BLANC 0xFFFFFF une ville est positionnée ici
        ///                                                   NOIR 0x000000 une ruine est positionnée ici        
        /// </summary>
        /// <param name="carteTerrain"></param>
        public TerrainDescription(Bitmap carteTerrain) :  this(carteTerrain.Width / 2, carteTerrain.Height / 2) {
            chargeDepuisBitmap(carteTerrain);
        }

        /// <summary>
        /// Charge depuis le bitmap en mode unsafe pour aller + vite
        /// </summary>
        /// <param name="carteTerrain">bitmap de carte </param>
        public unsafe void chargeDepuisBitmap(Bitmap carteTerrain) {
            BitmapData bData = carteTerrain.LockBits(new Rectangle(0, 0, carteTerrain.Width, carteTerrain.Height), ImageLockMode.ReadWrite, carteTerrain.PixelFormat);
            byte bitsPerPixel = (byte) Image.GetPixelFormatSize(bData.PixelFormat);

            if ((bitsPerPixel != 24) && (bitsPerPixel != 32)) {
                throw new SdGException("L'image de carte n'est pas en mode 24 ou 32 bits par pixel");
            }

            // Offset du 1er octet dans le bitmap
            byte* scan0 = (byte*)bData.Scan0.ToPointer();

            for (int y = 0; y < hauteur; y++) {
                for (int x = 0; x < largeur; x++) {

                    byte* data = scan0 + y * 2 * bData.Stride + x * 2 * bitsPerPixel / 8;                    
                    int coul0 = getPlusProche(*data | (*(data + 1) <<8 ) | *(data + 2) << 16);

                    data = scan0 + (y * 2 + 1) * bData.Stride + x * 2 * bitsPerPixel / 8;
                    int coul1 = getPlusProche(*data | (*(data + 1) << 8) | *(data + 2) << 16);

                    data = scan0 + (y * 2  + 1) * bData.Stride + (x * 2 + 1)* bitsPerPixel / 8;
                    int coul2 = getPlusProche(*data | (*(data + 1) << 8) | *(data + 2) << 16);

                    data = scan0 + y * 2 * bData.Stride + (x * 2 + 1) * bitsPerPixel / 8;
                    int coul3 = getPlusProche(*data | (*(data + 1) << 8) | *(data + 2) << 16);
                    
                    // Analyse des couleurs
                    bool route = (coul0 == 6) || (coul1 == 6) || (coul2 == 6) || (coul3 == 6);
                    bool ville = (coul0 == 7) || (coul1 == 7) || (coul2 == 7) || (coul3 == 7);
                    bool ruine = (coul0 == 8) || (coul1 == 8) || (coul2 == 8) || (coul3 == 8);

                    TypeTerrain typeTerr;
                    if ((coul0 == 0) || (coul1 == 0) || (coul2 == 0) || (coul3 == 0)) {
                        typeTerr = TypeTerrain.EAU;
                    }
                    else if ((coul0 == 1) || (coul1 == 1) || (coul2 == 1) || (coul3 == 1)) {
                        typeTerr = TypeTerrain.HERBE;
                    }
                    else if ((coul0 == 2) || (coul1 == 2) || (coul2 == 2) || (coul3 == 2)) {
                        typeTerr = TypeTerrain.COLLINE;
                    }
                    else if ((coul0 == 3) || (coul1 == 3) || (coul2 == 3) || (coul3 == 3)) {
                        typeTerr = TypeTerrain.MONTAGNE;
                    }
                    else if ((coul0 == 4) || (coul1 == 4) || (coul2 == 4) || (coul3 == 4)) {
                        typeTerr = TypeTerrain.MARECAGE;
                    }
                    else if ((coul0 == 5) || (coul1 == 5) || (coul2 == 5) || (coul3 == 5)) {
                        typeTerr = TypeTerrain.FORET;
                    }
                    else {
                        throw new SdGException("Type de couleur non reconnu en position (carte) x = " + x + " y = " + y);
                    }
                    // Affecte la cellule
                    TerrainCell cell = new TerrainCell(typeTerr);
                    cell.route = route;
                    if (ruine) {
                        cell.ruineIndex = _nombreRuines++;
                    }
                    if (ville) {
                        cell.villeIndex = _nombreVilles++;
                        _villePos.Add(new Microsoft.Xna.Framework.Point(x, y));
                    }

                    setCellAt(x, y, cell);
                }
            }

            carteTerrain.UnlockBits(bData);
        }

        /// <summary>
        /// retourne le type le plus proche pour cette couleur
        /// </summary>
        /// <param name="argb">couleur</param>
        /// <returns>0 pour EAU, 1 pour HERBE, 2 pour COLLINE, 3 pour MONTAGNE, 4 pour MARECAGE, 5 pour FORET, 6 pour ROUTE,7 pour ville, 8 pour ruine</returns>
        private int getPlusProche(int argb) {
            int[] couleurs = new int[] {0x0000FF, 0x00FF00, 0xFF8000, 0x808080, 0x00FFFF, 0x008000,0xFF0000,0xFFFFFF,0x000000};
            int minDist = int.MaxValue;
            int minI = -1;
            for (int i = 0; i < couleurs.Length; i++) {
                int d = distance(argb, couleurs[i]);
                if (d < minDist) {
                    minDist = d;
                    minI = i;
                }
            }
            return minI;
        }

        /// <summary>
        /// Donne la distance entre 2 couleurs
        /// </summary>
        /// <param name="argb1">couleur 1</param>
        /// <param name="argb2">couleur 2</param>
        /// <returns>somme des valeurs absolues des différences des composantes R, G, B</returns>
        private int distance(int argb1, int argb2) {
            // optim de perf
            if (argb1 == argb2) {
                return 0;
            }
            else {
                return Math.Abs((argb1 & 0xFF) - (argb2 & 0xFF)) + Math.Abs(((argb1 & 0xFF00) >> 8) - ((argb2 & 0xFF00) >> 8)) + Math.Abs(((argb1 & 0xFF0000) >> 16) - ((argb2 & 0xFF0000) >> 16));
            }
        }

        // Accesseurs -------------------------------------------------------------------------------------------------

        /// <summary>hauteur de la map en cases</summary>
        /// <returns>hauteur de la map en cases</returns>
        public int getHauteur() {
            return hauteur;
        }

        /// <summary>largeur de la map en cases</summary>
        /// <returns>largeur de la map en cases</returns>
        public int getLargeur() {
            return largeur;
        }
        /// <summary>Donne le terrain en position x,y</summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <returns>le terrain si c'est dans la map, de l'eau sinon</returns>
        public TerrainCell getCellAt(int x, int y) {
            if ((x < 0) || (y < 0) || (x >= largeur) || (y >= hauteur)) {
                return new TerrainCell(TypeTerrain.EAU);
            }
            else {
                return map[x, y];
            }
        }
        /// <summary>
        /// Nombre de villes sur la carte
        /// </summary>
        public int nombreVilles {
            get { return _nombreVilles; }
        }
        /// <summary>
        /// Nombre de ruines sur la carte
        /// </summary>
        public int nombreRuines {
            get { return _nombreRuines; }
        }

        /// <summary>
        /// retourne la position de la ville trouvée à la construction, par index
        /// </summary>
        /// <param name="index">n° de ville</param>
        /// <returns>position sur la carte</returns>
        public Microsoft.Xna.Framework.Point getPositionVilles(int index) {
            return _villePos[index];
        }

        /// <summary>Affecte le terrain en position x,y</summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>        
        public void setCellAt(int x, int y, TerrainCell cell) {
            map[x, y] = cell;
        }
    }
}
