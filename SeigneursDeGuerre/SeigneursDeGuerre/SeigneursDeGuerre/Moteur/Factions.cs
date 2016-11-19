using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    /// Classe donnant des infos sur les factions (en dur dans le code !)
    /// </summary>
    class Factions {
        /// <summary>
        /// Une faction
        /// </summary>
        public class Faction {
            /// <summary>
            /// Nom de la faction
            /// </summary>
            public string nom;
            /// <summary>
            /// Couleur de base
            /// </summary>
            public Color couleur;
            /// <summary>
            /// Numéro de faction
            /// </summary>
            public int numero;
            /// <summary>
            /// Joué par humain ou IA
            /// </summary>
            public bool humanPlayer;

            /// <summary>
            /// Indique si on a déjà dit que la faction était morte
            /// </summary>
            public bool dejaMort;

            /// <summary>
            /// Montant de l'or disponible
            /// </summary>
            public int or;

            /// <summary>
            /// Constructeur
            /// </summary>
            /// <param name="_nom">nom</param>
            /// <param name="_couleur">couleur</param>
            /// <param name="_numero">indice</param>
            public Faction(string _nom, Color _couleur, int _numero, int _or) {
                nom = _nom;
                couleur = _couleur;
                numero = _numero;
                humanPlayer = true;
                or = _or;
                dejaMort = false;
            }
        }
        // Nbre de factions
        private const int NB_FACTIONS = 8;

        /// <summary>
        /// Les factions en question
        /// </summary>
        private Faction[] _factions;
        /// <summary>
        /// Le jeu
        /// </summary>
        private Jeu _jeu;

        // Textures
        private Texture2D texMat;
        private Texture2D texFlag;

        /// <summary>
        /// Constructeur
        /// </summary>
        public Factions(Jeu leJeu) {
            const int OR_INITIAL = 1000;
            _jeu = leJeu;
            _factions = new Faction[NB_FACTIONS + 1] {
                new Faction("Neutres",Color.FromNonPremultiplied(140,140,140,255),0,OR_INITIAL),
                new Faction("Sirylléens",Color.White,1,OR_INITIAL),
                new Faction("Géants de Pierre",Color.Gold,2,OR_INITIAL),
                new Faction("Nains Blancs",Color.Orange,3,OR_INITIAL),
                new Faction("Orcs d'Usyrie",Color.Red,4,OR_INITIAL),
                new Faction("Elfes Sombres",Color.FromNonPremultiplied(0,255,68,255),5,OR_INITIAL),
                new Faction("Royaumes Libres",Color.Blue,6,OR_INITIAL),
                new Faction("Seigneurs du Cheval",Color.FromNonPremultiplied(0,192,192,255),7,OR_INITIAL),
                new Faction("Le Roi Liche",Color.Black,8,OR_INITIAL)
            };
            _factions[0].humanPlayer = false;
        }


        /// <summary>
        /// Loader de contenu 
        /// </summary>
        /// <param name="content"></param>
        public void load(ContentManager content) {
            texMat = content.Load<Texture2D>("creatures/mat");
            texFlag = content.Load<Texture2D>("creatures/drapeau");            
        }

        public int nbFactions {
            get { return NB_FACTIONS; }
        }

        /// <summary>
        /// retourne la faction demandée
        /// </summary>
        /// <param name="index">n° de faction</param>
        /// <returns>la faction</returns>
        public Faction getFaction(int index) {
            return _factions[index];
        }

        /// <summary>
        /// Appelé chaque début de tour pour savoir si un nouveau héros se propose
        /// </summary>
        /// <returns>true si un héros arrive</returns>
        public bool nouvelHeros() {
            bool nouveauHeros;
            if (_jeu.noTour == 1) {
                nouveauHeros = true;
            }
            else {
                // en fonction du hasard et de la richesse
                int proba;
                int orfac = getFaction(_jeu.tourFaction).or ;
                if (orfac > 6000) {
                    proba = 5;
                }
                else if (orfac > 4000) {
                    proba = 10;
                }
                else if (orfac > 2000) {
                    proba = 20;
                }
                else {
                    proba = 30;
                }
                nouveauHeros = (_jeu.rnd.Next(proba) == 0);
            }
            return nouveauHeros;
        }

        /// <summary>
        /// Retourne la liste des armées de la faction
        /// </summary>
        /// <returns></returns>
        public List<Armee> getArmada(int faction) {
            List<Armee> armada = new List<Armee>();
            foreach (Armee armee in _jeu.armees) {
                if (armee.faction == faction) {
                    armada.Add(armee);
                }
            }
            return armada; 
        }

        /// <summary>
        /// Retourne la somme des dépenses de la faction
        /// </summary>
        /// <returns></returns>
        public int getTotalDep(int faction) {
            int totalDep = 0;
            foreach (Armee armee in _jeu.armees) {
                if (armee.faction == faction) {
                    totalDep += armee.totalDep;
                }
            }
            return totalDep;
        }

        /// <summary>
        /// Retourne la somme des revenus de la faction
        /// </summary>
        /// <returns></returns>
        public int getTotalRev(int faction) {
            int totalRev = 0;
            foreach (VilleDescription vd in _jeu.villes.villeDesc) {                
                if (vd.faction == faction) {
                    totalRev += vd.getRevenus();
                }
            }
            return totalRev;
        }



        /// <summary>
        /// Affiche un drapeau. A appeler dans un spriteBatch
        /// </summary>
        /// <param name="spriteBatch">spriteBatch du jeu</param>
        /// <param name="GraphicsDevice">gd du jeu</param>
        /// <param name="gameTime">temps du jeu</param>
        /// <param name="x">position x du coin gauche</param>
        /// <param name="y">position y du coin gauche</param>
        /// <param name="size">nombre d'unités représenté, de 1 à 8</param>
        /// <param name="noFaction">indice de la faction</param>
        /// <param name="z">profondeur</param>
        public void drawFlag(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime,int x, int y, int size, int noFaction, float z) {
            int hauteurMat= _jeu.blockSize;
            int largeurMat = hauteurMat / 8;
            // Mat
            spriteBatch.Draw(texMat, new Rectangle(x, y, largeurMat, hauteurMat), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, z);
            // Drapeau
            int largeurDrapeau = ((3 + size) * _jeu.blockSize) / 10;
            int hauteurDrapeau = largeurDrapeau / 3;

            spriteBatch.Draw(texFlag, new Rectangle(x+largeurMat/2, y+largeurMat, largeurDrapeau, hauteurDrapeau), null, _factions[noFaction].couleur, 0, Vector2.Zero, SpriteEffects.None, z-0.001f);
        }


        /// <summary>
        /// Sauvegarde les informations de faction
        /// </summary>
        /// <param name="file">flux de sortie</param>
        internal void save(System.IO.StreamWriter file) {
            // Nbre de factions
            file.WriteLine(NB_FACTIONS);
            foreach (Faction fac in _factions) {
                file.WriteLine(fac.humanPlayer);
                file.WriteLine(fac.or);
            }
        }
        /// <summary>
        /// Charge les infos de faction
        /// </summary>
        /// <param name="file">flux d'entrée</param>
        /// <returns> true si ok, false si ko</returns>
        internal bool load(System.IO.StreamReader file) {
            int nbFac = int.Parse(file.ReadLine());
            if (nbFac != NB_FACTIONS) {
                return false;
            }
            foreach (Faction fac in _factions) {
                fac.humanPlayer = bool.Parse(file.ReadLine());
                fac.or = int.Parse(file.ReadLine());
            }
            return true;

        }
    }
}
