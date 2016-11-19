using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SeigneursDeGuerre.Outils;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    /// Décrit une ville
    /// </summary>
    class Villes {
        /// <summary>
        /// La description sous-jacente
        /// </summary>
        private VilleDescription[] _description;
        /// <summary>
        /// Instance partagée du jeu
        /// </summary>
        private Jeu _jeu;

        private Texture2D[] _texVillesNormales;


        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="leJeu">jeu en cours</param>
        public Villes(Jeu leJeu) {
            _jeu = leJeu;
            _texVillesNormales = new Texture2D[_jeu.factions.nbFactions + 1];
        }
        /// <summary>
        /// Créateur à partir de fichier de description des villes
        /// </summary>
        /// <param name="leJeu">jeu en cours</param>
        /// <param name="nomFichierRessource">nom du fichier texte (sans préfixe SeigneursDeGuerre.)</param>
        public Villes(Jeu leJeu, string nomFichierRessource) :  this(leJeu) {
            // Format du fichier :
            // Indice;Nom;NiveauDefense;NiveauProductivtié;CapitaleFaction;Faction;Productions possibles (indexes séparées par |)
            // (header présent dans le fichier)
            _description = new VilleDescription[leJeu.terrain.terrainDesc.nombreVilles];

            CSVReader csv = new CSVReader(nomFichierRessource, ";");
            // header
            if (!csv.readLine()) {
                throw new SdGException("Le fichier des villes est vide");
            }; 
            try {
                while (csv.readLine()) {
                    VilleDescription ville = new VilleDescription(Int32.Parse(csv.getField(0)));
                    ville.nom = csv.getField(1);
                    ville.niveauDefense = Int32.Parse(csv.getField(2));
                    ville.niveauProductivite = Int32.Parse(csv.getField(3));
                    ville.capitaleFaction = Int32.Parse(csv.getField(4));
                    ville.faction = Int32.Parse(csv.getField(5));
                    _description[ville.indice] = ville;
                    ville.positionMap = leJeu.terrain.terrainDesc.getPositionVilles(ville.indice);
                    string[] types = csv.getField(6).Split(new char[]{'|'}, StringSplitOptions.RemoveEmptyEntries);
                    ville.typeCreatures = new int[types.Length];
                    for (int i = 0; i < types.Length; i++) {
                        ville.typeCreatures[i] = Int32.Parse(types[i]);
                    }
                    // La faction neutre ne produit rien
                    ville.productionCourante = (ville.faction == 0) ? -1 : 0;
                }
            }
            finally {
                csv.dispose();
            }


        }

        /// <summary>
        /// Accesseurs sur la descritption
        /// </summary>
        public VilleDescription[] villeDesc {
            get { return _description; }         
        }


        /// <summary>
        /// Production des villes
        /// </summary>
        public void productions() {
            foreach (VilleDescription vd in villeDesc) {
                
                if (vd.productionCourante != -1) {
                    vd.productionPoints++;
                    int coutPP = vd.geNbToursPourCreatures(_jeu.creatures.description[vd.typeCreatures[vd.productionCourante]].cout);
                    if (vd.productionPoints >= coutPP) {
                        vd.productionPoints -= coutPP;
                        // Ajoute la créature
                        Creature crea = new Creature(_jeu, vd.typeCreatures[vd.productionCourante], vd.faction,vd.positionMap);
                        Armee armee = new Armee(_jeu);
                        armee.addCreature(crea);
                        _jeu.addArmee(armee);
                    }
                }
            }
        }
        /// <summary>
        /// Retourne la liste des villes de la faction
        /// </summary>
        /// <param name="faction">faction</param>
        /// <returns>liste des villes</returns>
        public List<VilleDescription> getPays(int faction) {
            List<VilleDescription> ret = new List<VilleDescription>();
            foreach (VilleDescription vd in villeDesc) {
                if (vd.faction == faction) {
                    ret.Add(vd);
                }
            }
            return ret;
        }

        /// <summary>
        /// Loader de contenu 
        /// </summary>
        /// <param name="content"></param>
        public void load(ContentManager content) {
            for (int i = 0; i < _texVillesNormales.Length; i++) {
                _texVillesNormales[i] = content.Load<Texture2D>("villes/ville" + i);
            }
        }


        /// <summary>
        /// Affiche les villes sur le fond de carte 
        /// Appelé au sein d'un spriteBatch begin/end
        /// </summary>
        /// <param name="spriteBatch">spriteBatch du jeu</param>
        /// <param name="GraphicsDevice">gd du jeu</param>
        /// <param name="gameTime">temps du jeu</param>
        public void draw(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime) {
            
            foreach (VilleDescription vd in villeDesc) {

                int drawX = (vd.positionMap.X * _jeu.blockSize) - _jeu.terrain.offsetCarte.X;
                int drawY = (vd.positionMap.Y * _jeu.blockSize) - _jeu.terrain.offsetCarte.Y;

                spriteBatch.Draw(_texVillesNormales[vd.faction], new Rectangle(drawX, drawY, _jeu.blockSize * 2, _jeu.blockSize * 2), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.68f);
            }
        }

        /// <summary>
        /// Sauvegarde les informations de villes
        /// </summary>
        /// <param name="file">flux de sortie</param>
        internal void save(System.IO.StreamWriter file) {
            file.WriteLine(_description.Length);
            foreach (VilleDescription vd in _description) {
                file.WriteLine(vd.faction);
                file.WriteLine(vd.niveauDefense);
                file.WriteLine(vd.productionCourante);
                file.WriteLine(vd.productionPoints);
            }
        }
        /// <summary>
        /// Charge les infos de ville
        /// </summary>
        /// <param name="file">flux d'entrée</param>
        /// <returns>true si ok, false si ko</returns>

        internal bool load(System.IO.StreamReader file) {
            int descLen = int.Parse(file.ReadLine());
            if (descLen != _description.Length) {
                return false;
            }
            foreach (VilleDescription vd in _description) {
                vd.faction = int.Parse(file.ReadLine());
                vd.niveauDefense = int.Parse(file.ReadLine());
                vd.productionCourante = int.Parse(file.ReadLine());
                vd.productionPoints = int.Parse(file.ReadLine());
            }
            return true;
        }
    }
}
