using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using SeigneursDeGuerre.Outils;
using Microsoft.Xna.Framework;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    /// Service de gestion des créatures
    /// </summary>
    class Creatures {
        /// <summary>
        /// taille en pixels d'une créature
        /// </summary>
        public const int CREATURE_SIZE = 56;

        public const int TYPE_HEROS = 29;
        public const int TYPE_HEROINE = 28;
        public const int TYPE_ALLIEMIN = 19;
        public const int TYPE_ALLIEMAX = 27;


        public const int TYPE_BATEAU = 5;


        // Ennemis à combattre dans les ruines
        public const int TYPE_SORCIERE = 23;
        public const int TYPE_GEANT = 0;
        public const int TYPE_GRIFFON = 9;

        /// <summary>
        /// Descriptions des créatures disponibles
        /// </summary>
        private List<CreatureDescription> _description;
        /// <summary>
        /// Jeu
        /// </summary>
        private Jeu _jeu;

        /// <summary>
        /// Planche avec ttes les textures de créature
        /// </summary>
        private Texture2D texPlancheCreatures;

        /// <summary>
        /// Constructeur vide
        /// </summary>
        /// <param name="leJeu"></param>
        public Creatures(Jeu leJeu) {
            _jeu = leJeu;
        }


        /// <summary>
        /// Constructeur avec fichier CSV
        /// </summary>
        /// <param name="leJeu">jeu</param>
        /// <param name="nomFichierRessource">nom du fichier sans SeigneurDeGuerre.rsc.</param>
        public Creatures(Jeu leJeu, string nomFichierRessource)
            : this(leJeu) {

            // Format du fichier :
            // Index;NomCreature;PositionPlancheX;PositionPlancheY;Force;Mouvement;Cout;Heros?;Vol?;Nage?;BonusAttaque:AUCUN/VILLE/EXTERIEUR;BonusDefense;BonusMouvement:AUCUN/FORET/COLLINES/MARAIS/HERBE/EAU
            // (header présent dans le fichier)
            _description= new List<CreatureDescription>();

            CSVReader csv = new CSVReader(nomFichierRessource, ";");
            // header
            if (!csv.readLine()) {
                throw new SdGException("Le fichier des créatures est vide");
            };
            try {
                while (csv.readLine()) {
                    CreatureDescription crea = new CreatureDescription(csv.getField(1), new Point(Int32.Parse(csv.getField(2)), Int32.Parse(csv.getField(3))), Int32.Parse(csv.getField(0)));
                    crea.force = Int32.Parse(csv.getField(4));
                    crea.mouvement = Int32.Parse(csv.getField(5));
                    crea.cout  = Int32.Parse(csv.getField(6));
                    crea.heros = csv.getField(7).StartsWith("O", StringComparison.InvariantCultureIgnoreCase);
                    crea.vol = csv.getField(8).StartsWith("O", StringComparison.InvariantCultureIgnoreCase);
                    crea.nage = csv.getField(9).StartsWith("O", StringComparison.InvariantCultureIgnoreCase);
                    
                    crea.bonusAttaque = (CreatureDescription.BonusUrbain) Enum.Parse(typeof(CreatureDescription.BonusUrbain), csv.getField(10), true);
                    crea.bonusDefense = (CreatureDescription.BonusUrbain) Enum.Parse(typeof(CreatureDescription.BonusUrbain), csv.getField(11), true);
                    crea.bonusMouvement = (CreatureDescription.BonusEnvironnement)Enum.Parse(typeof(CreatureDescription.BonusEnvironnement), csv.getField(12), true);
                    
                    // Attention : si l'indexe ne suit pas l'ordre du fichier, pb... 
                    _description.Add(crea);
                }
            }
            finally {
                csv.dispose();
            }
        }


        /// <summary>
        /// Accesseur descriptions
        /// </summary>
        public List<CreatureDescription> description {
            get { return _description; }
        }

        /// <summary>
        /// Change de sélection pour la faction courante
        /// </summary>
        public void selectNext() {
            if ((_jeu.selectedArmee != null) && (_jeu.selectedArmeeGO)) {
                _jeu.selectedArmee.stop();
            }
            // Construit la liste des armées de la faction
            List<Armee> armada = _jeu.factions.getArmada(_jeu.tourFaction);
            Armee nextArmee = _jeu.selectedArmee;

            if (armada.Count != 0) {
                int idx;
                if (nextArmee == null) {
                    idx = 0;
                }
                else {
                    idx = armada.IndexOf(nextArmee);
                }
                int compteur = 0;
                do {
                    idx++;
                    compteur++;
                    if (idx >= armada.Count) {
                        idx = 0;
                    }
                    nextArmee = armada[idx];
                } while ((nextArmee.blackListed) && (compteur <= armada.Count));
                if (compteur > armada.Count) {
                    nextArmee = null;
                }
                
                // Focus
                _jeu.selectedArmee = nextArmee;
                if (_jeu.selectedArmee != null) {
                    _jeu.terrain.zoomSur(_jeu.selectedArmee.positionEcran);
                }
                
                
            }
        }

        /// <summary>
        /// Création d'un nouvel héros pour la faction courante
        /// </summary>
        /// <param name="nom">nom du héros</param>
        /// <param name="femme">true si femme</param>
        /// <param name="allies">true si on ajoute des alliés</param>
        public void createHeros(string nom, bool femme, bool allies) {            
            // Choix de la ville
            List<VilleDescription> villesFaction = new List<VilleDescription>();
            foreach (VilleDescription vd in _jeu.villes.villeDesc) {
                if (vd.faction == _jeu.tourFaction) {
                    villesFaction.Add(vd);
                }
            }
            if (villesFaction.Count != 0) {
                VilleDescription vd = villesFaction[_jeu.rnd.Next(villesFaction.Count)];
                int herosType = femme ? TYPE_HEROS : TYPE_HEROINE;
                Creature heros = new Creature(_jeu, herosType, vd.faction, vd.positionMap);
                heros.vraiNom = nom;
                // Possède un étendard
                heros.addItem(_jeu.items.itemDesc[Items.ETENDARD]);

                Armee armee = new Armee(_jeu);
                armee.addCreature(heros);
                // Alliés éventuels : types 19-27
                int nbAllie = 0;
                if (allies) {
                    int typeAllie = _jeu.rnd.Next(TYPE_ALLIEMAX + 1 - TYPE_ALLIEMIN) + TYPE_ALLIEMIN;
                    nbAllie = _jeu.rnd.Next(3) + 1;
                    for (int i = 0; i < nbAllie; i++) {
                        armee.addCreature(new Creature(_jeu, typeAllie, vd.faction, vd.positionMap));
                    }
                }                
                armee.positionCarte = armee.getPositionLibre(armee.positionCarte, false, false).Value;
                _jeu.addArmee(armee);
                if (allies) {
                    _jeu.messageInfo = nom + " est accompagné" + (femme ? "e" : "") + " de " + nbAllie + " allié" + (nbAllie > 1 ? "s" : "") + " et se trouve à " + vd.nom;
                }
                else {
                    _jeu.messageInfo = nom + " se trouve à " + vd.nom;
                }

            }
        }

        /// <summary>
        /// Loader de contenu 
        /// </summary>
        /// <param name="content"></param>
        public void load(ContentManager content) {
            texPlancheCreatures = content.Load<Texture2D>("creatures/planche_w56-57_16x4");
        }


        /// <summary>
        /// Affiche une créature à l'endroit désiré
        /// Appelé au sein d'un spriteBatch begin/end
        /// </summary>
        /// <param name="spriteBatch">spriteBatch du jeu</param>
        /// <param name="GraphicsDevice">gd du jeu</param>
        /// <param name="gameTime">temps du jeu</param>
        /// <param name="x">x coin haut gauche (coordonnées de type offet de terrain)</param>
        /// <param name="y">y coin haut gauche (coordonnées de type offet de terrain)</param>
        /// <param name="z">z </param>
        /// <param name="typeCreature">type de créature à afficher</param>
        /// 
        public void draw(SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, GameTime gameTime, int x,int y,float z,int typeCreature) {
            CreatureDescription desc = _description[typeCreature];

            int drawX = x - _jeu.terrain.offsetCarte.X;
            int drawY = y - _jeu.terrain.offsetCarte.Y;

            spriteBatch.Draw(texPlancheCreatures, new Rectangle(drawX, drawY, CREATURE_SIZE, CREATURE_SIZE), 
                                                  new Rectangle(desc.positionPlanche.X * (CREATURE_SIZE + 1), desc.positionPlanche.Y * (CREATURE_SIZE + 1), CREATURE_SIZE, CREATURE_SIZE),
                                                  Color.White, 0, Vector2.Zero, SpriteEffects.None, z);
            
        }

        /// <summary>
        /// Retourne le contenu de la texture d'une créature extraite de la planche
        /// </summary>
        /// <param name="output">tableau de sortie</param>
        /// <param name="typeCreature">type de la créature</param>
        public Color[] getTextureData( int typeCreature) {
            CreatureDescription desc = _description[typeCreature];
            Color[] output = new Color[Creatures.CREATURE_SIZE * Creatures.CREATURE_SIZE];
            texPlancheCreatures.GetData<Color>(0, new Rectangle(desc.positionPlanche.X * (CREATURE_SIZE + 1),
                desc.positionPlanche.Y * (CREATURE_SIZE + 1), CREATURE_SIZE, CREATURE_SIZE), output, 0, CREATURE_SIZE * CREATURE_SIZE);
            return output;
        }


        /// <summary>
        /// Sauvegarde les informations des armées et créatures
        /// </summary>
        /// <param name="file">flux de sortie</param>     
        internal void save(System.IO.StreamWriter file) {
            file.WriteLine(_jeu.armees.Count);
            foreach (Armee arme in _jeu.armees) {
                file.WriteLine(arme.faction);
                file.WriteLine(arme.positionCarte.X);
                file.WriteLine(arme.positionCarte.Y);
                file.WriteLine(arme.positionEcran.X);
                file.WriteLine(arme.positionEcran.Y);
                file.WriteLine(arme.moveTarget.HasValue);
                if (arme.moveTarget.HasValue) {
                    file.WriteLine(arme.moveTarget.Value.X);
                    file.WriteLine(arme.moveTarget.Value.Y);
                }
                file.WriteLine(arme.getTaille());
                for (int i = 0; i < arme.getTaille(); i++) {
                    Creature crea = arme.getCreature(i);
                    file.WriteLine(crea.typeCreature);
                    file.WriteLine(crea.vraiNom);
                    file.WriteLine(crea.cout);
                    file.WriteLine(crea.force);
                    file.WriteLine(crea.mouvement);
                    file.WriteLine(crea.mouvementCourant);
                    file.WriteLine(crea.nage);
                    file.WriteLine(crea.vol);
                    file.WriteLine(crea.items.Count);
                    foreach (ItemDescription it in crea.items) {
                        file.WriteLine(_jeu.items.itemDesc.IndexOf(it));
                    }
                }
            }
        }
        /// <summary>
        /// Charge les infos de créatures et d'armées
        /// </summary>
        /// <param name="file">flux d'entrée</param>
        /// <returns>true si ok, false si ko</returns>

        internal bool load(System.IO.StreamReader file) {
            int armeesCount = int.Parse(file.ReadLine());
            // rince toutes les armées...
            _jeu.removeAllArmee();
            for (int i=0;i<armeesCount;i++) {
                Armee arme = new Armee(_jeu);
                int faction = int.Parse(file.ReadLine());
                Point posCarte = new Point(int.Parse(file.ReadLine()),int.Parse(file.ReadLine()));
                arme.positionEcran = new Point(int.Parse(file.ReadLine()), int.Parse(file.ReadLine()));
                bool hasTarget = bool.Parse(file.ReadLine());
                arme.moveTarget = null; 
                if (hasTarget) {
                    arme.moveTarget = new Point(int.Parse(file.ReadLine()), int.Parse(file.ReadLine()));
                }
                int nbCreatures = int.Parse(file.ReadLine());
                for (int j = 0; j < nbCreatures; j++) {
                    Creature crea = new Creature(_jeu, int.Parse(file.ReadLine()), faction, posCarte);
                    crea.vraiNom = file.ReadLine();
                    crea.cout = int.Parse(file.ReadLine());
                    crea.force = int.Parse(file.ReadLine());
                    crea.mouvement = int.Parse(file.ReadLine());
                    crea.mouvementCourant = int.Parse(file.ReadLine());
                    crea.nage = bool.Parse(file.ReadLine());
                    crea.vol = bool.Parse(file.ReadLine());
                    int nbItems = int.Parse(file.ReadLine());
                    for (int k = 0; k < nbItems; k++) {
                        crea.addItemNoEffect(_jeu.items.itemDesc[int.Parse(file.ReadLine())]); // TODO n'applique pas les bonus
                    }
                    arme.addCreature(crea);
                }
                arme.blackListed = false;
                _jeu.addArmee(arme);
            }
            return true;
        }
    }
}
