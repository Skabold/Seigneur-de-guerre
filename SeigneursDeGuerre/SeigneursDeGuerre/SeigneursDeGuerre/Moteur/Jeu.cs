using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using SeigneursDeGuerre.Overlays;
using System.IO;
using System.Windows.Forms;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    /// Classe gérant les principaux paramètres globaux du jeu
    /// </summary>
    class Jeu {
        // Constantes imporantes
        const int BLOCK_SIZE = 64;

        // Instance unique de curseur
        private Cursor _curseur;

        // Factions
        private Factions _factions;

        // Terrain courant
        private Terrain _terrain;

        // Villes actuelles
        private Villes _villes;
        // Types de Créatures
        private Creatures _creatures;

        // Armées en présence
        private List<Armee> _armees;

        // Objets du jeu
        private Items _items;

        // Ruines (à ajouter après les items)
        private Ruines _ruines;

        // Gestionnaires d'info de carte
        private InfoCarte _infoCarte;

        // IA du jeu
        private IAJoue _iajoue;

        // Texures de base
        private Texture2D _pixelBlanc;
        private Texture2D _texSelection;
        private Texture2D _texMarqueur;
        private Texture2D _texMarqueurKO;
        private Texture2D _texChgTour;
        private Texture2D _texHeros;
        private Texture2D _texHeroine;
        private Texture2D _texRuine;
        private Texture2D _texHumain;
        private Texture2D _texCPU;
        private Texture2D _texVictoire;
        private Texture2D _texDefaite;

        // Polices
        private SpriteFont _font;
        private SpriteFont _bigFont;
        private SpriteFont _isoFont;

        // Dimensions des blocks & écran
        private int _resX;
        private int _resY;
        private int _blockSize;
        
        // Matrice de mise à l'échelle
        private Matrix _scaleSpriteBatch;

        // ratio de mise à l'échelle
        private float _scaleX;

        // Overlays courants
        private List<Overlay> _overlays;

        // Interaction joueur
        private InteractionJoueur _interactionJoueur;

        // Faction dont c'est le tour
        private int _tourFaction;
        // Armée sélectionnée par le joueur courant
        private Armee _selectedArmee;
        /// <summary>
        /// L'armée sélectionnée bouge !
        /// </summary>
        private bool _selectedArmeeGO;

        /// <summary>
        /// N° du tour
        /// </summary>
        private int _noTour;

        // Message de debug à afficher
        private string _messageDebug;

        // Message d'info à afficher
        private string _messageInfo;
        // Durée d'affichage restante du message d'info
        private double _messageInfoDuree;

        public GraphicsDevice graphicsDevice;
        public Random rnd = new Random();

        // Accesseurs
        public Terrain terrain {
            get { return _terrain; }
            set { _terrain = value; }
        }
        public Villes villes {
            get { return _villes; }
            set { _villes = value; }
        }
        public Factions factions {
            get { return _factions; }
            set { _factions = value; }
        }
        public Creatures creatures {
            get { return _creatures; }
            set { _creatures = value; }
        }
        public Items items {
            get { return _items; }
            set { _items = value; }
        }
        public Ruines ruines {
            get { return _ruines; }
            set { _ruines = value; }
        }

        public InfoCarte infoCarte {
            get { return _infoCarte; }
            set { _infoCarte = value; }
        }
        public InteractionJoueur interactionJoueur {
            get { return _interactionJoueur; }
            set { _interactionJoueur = value; }
        }

        public IAJoue IAJoue {
            get { return _iajoue; }
            set { _iajoue = value; }
        }

        public Matrix scaleSpriteBatch {
            get { return _scaleSpriteBatch; }
        }

        public Texture2D pixelBlanc {
            get { return _pixelBlanc; }
        }
        public Texture2D texSelection {
            get { return _texSelection; }
        }
        public Texture2D texMarqueur {
            get { return _texMarqueur; }
        }
        public Texture2D texMarqueurKO {
            get { return _texMarqueurKO; }
        }
        public Texture2D texChgTour {
            get { return _texChgTour; }
        }
        public Texture2D texHeros {
            get { return _texHeros; }
        }
        public Texture2D texHeroine {
            get { return _texHeroine; }
        }
        public Texture2D texRuine {
            get { return _texRuine; }
        }
        public Texture2D texHumain {
            get { return _texHumain; }
        }
        public Texture2D texCPU {
            get { return _texCPU; }
        }
        public Texture2D texVictoire {
            get { return _texVictoire; }
        }
        public Texture2D texDefaite {
            get { return _texDefaite; }
        }

                
        public SpriteFont font {
            get { return _font; }
        }
        public SpriteFont bigFont {
            get { return _bigFont; }
        }
        public SpriteFont isoFont {
            get { return _isoFont; }
        }
        public int resX {
            get { return _resX; }
        }
        public int resY {
            get { return _resY; }
        }
        public int blockSize {
            get { return _blockSize; }
        }
        public float scaleX {
            get { return _scaleX; }
        }
        public Cursor curseur {
            get { return _curseur; }
        }
        // Pour itérer seulement
        public List<Armee> armees{
            get { return _armees; }
        }
        public int tourFaction {
            get { return _tourFaction; }
            set { _tourFaction = value; }
        }



        public Armee selectedArmee {
            get { return _selectedArmee; }
            set {
                if (_selectedArmee != value) {
                    _selectedArmee = value;
                    // Recalcul trajectoire
                    if (value != null) {
                        _selectedArmee.reinitTrajectoire();
                    }
                    // TODO : ne pas forcément faire ça
                    interactionJoueur.reinitDoubleClick();

                }
            }
        }

        public bool selectedArmeeGO {
            get { return _selectedArmeeGO; }
            set { _selectedArmeeGO = value; }
        }
        public string messageDebug {
            get { return _messageDebug; }
            set { _messageDebug = value; }
        }
        
        public string messageInfo {
            get { return _messageInfo; }
            set { _messageInfo = value;
            _messageInfoDuree = 5000;
            }
        }
        public double messageInfoDuree {
            get { return _messageInfoDuree; }
            set { _messageInfoDuree = value; }
        }

        public int noTour {
            get { return _noTour; }
            set { _noTour = value; }
        }

        // Ajoute un overlay
        public void addOverlay(Overlay overlay) {
            _overlays.Add(overlay);
        }
        // Enlève le dernier overlay
        public void popOverlay() {
            if (_overlays.Count != 0) {
                _overlays.RemoveAt(_overlays.Count - 1);
            }
        }
        /// <summary>
        /// Retourne le dernier overlay
        /// </summary>
        /// <returns></returns>
        public Overlay getTopOverlay() {
            if (_overlays.Count != 0) {
                return _overlays[_overlays.Count - 1];
            }
            else {
                return null;
            }
        }

        // pour itérer seulement
        public List<Overlay> getOverlays() {
            return _overlays;
        }

        // pour itérer seulement, mais on peut enlever un overlay pendant l'itération
        public List<Overlay> getClonedOverlays() {
            List<Overlay> lo = new List<Overlay>();
            foreach (Overlay over in _overlays) {
                lo.Add(over);
            }
            return lo;
        }


        // Ajoute une armée nouvellement créée
        public void addArmee(Armee armee) {
            // fusion des armées si une armée est déjà à cet emplacement
            foreach (Armee autre in _armees) {
                if (autre.positionCarte == armee.positionCarte) {
                    // Fusionne les deux si c'est possible
                    if (autre.getTaille() + armee.getTaille() > Armee.MAX_TAILLE) {
                        Point pt = armee.getPositionLibre(armee.positionCarte, false, false).Value;
                        armee.positionCarte = pt;
                        // récursif
                        addArmee(armee);
                        return;
                    }
                    else {
                        autre.ajouteTroupes(armee);
                    }
                    return;
                }
            }
            // Toute seule
            _armees.Add(armee);            
        }


        // retire une armée perdante
        public void removeArmee(Armee armee) {
            if (_armees.Contains(armee)) {
                _armees.Remove(armee);
            }
        }
        // retire toutes les armées ! (pour reload)
        public void removeAllArmee() {
            _armees.Clear();
        }


       
        // Constructeur
        public Jeu(GraphicsDeviceManager graphics, int lresX, int lresY) {
            graphics.PreferredBackBufferWidth = lresX;
            graphics.PreferredBackBufferHeight = lresY;

            _scaleX = lresX / 1920.0f;
            _scaleSpriteBatch = Matrix.CreateScale(_scaleX);

            _resX = 1920;
            _resY = (int)((float)lresY / _scaleX);
            _blockSize = BLOCK_SIZE;

            _curseur = new Cursor();

            _overlays = new List<Overlay>();

            _armees = new List<Armee>();

            _selectedArmee = null;
            _selectedArmeeGO = false;
            _messageDebug = null;

            _noTour = 1;
        }

        // Loader de contenu
        public void load(ContentManager content) {
            _pixelBlanc = content.Load<Texture2D>("PixelBlanc");
            _texSelection = content.Load<Texture2D>("selection");
            _texMarqueur = content.Load<Texture2D>("creatures/marqueur_chemin_32x32");
            _texMarqueurKO = content.Load<Texture2D>("creatures/marqueur_chemin_ko_32x32");
            _texChgTour = content.Load<Texture2D>("Nicaise_de_Keyser");
            _texHeros = content.Load<Texture2D>("creatures/heros");
            _texHeroine = content.Load<Texture2D>("creatures/heroine");
            _texRuine = content.Load<Texture2D>("ruine");
            _texHumain = content.Load<Texture2D>("humain");
            _texCPU = content.Load<Texture2D>("cpu");
            _texVictoire = content.Load<Texture2D>("victoire_1");
            _texDefaite = content.Load<Texture2D>("victoire_2");
        

            _font = content.Load<SpriteFont>("font");
            _bigFont = content.Load<SpriteFont>("bigFont");
            _isoFont = content.Load<SpriteFont>("isoFont");

            _curseur.load(content);
            _terrain.load(content);
            _villes.load(content);
            _factions.load(content);
            _creatures.load(content);
        }

        /// <summary>
        /// Appelée à chaque fin de tour
        /// </summary>
        public void finTour() {
            if ((selectedArmee != null) && (selectedArmeeGO)) {
                selectedArmee.stop();
            }
            selectedArmee = null;
            selectedArmeeGO = false;
            int nbMorts = 0;
            do {
                tourFaction++;
                if (tourFaction > factions.nbFactions) {
                    tourFaction = 1;
                    noTour++;
                    // Gestion des productions dans les villes
                    villes.productions();
                }
                // Vérification si la faction est décédée = pas d'armée, pas de ville
                if ((factions.getArmada(tourFaction).Count == 0) && (villes.getPays(tourFaction).Count == 0)) {
                    if (!factions.getFaction(tourFaction).dejaMort) {
                        messageInfo = factions.getFaction(tourFaction).nom + ", vous avez perdu...";
                        factions.getFaction(tourFaction).dejaMort = true;
                    }
                    // Fin du tour encore
                    nbMorts++;
                    continue;
                }
                else {
                    // Actions de début de tour
                    debutTour();
                    break;
                }
            } while (nbMorts < (factions.nbFactions - 1));
            if (nbMorts >= (factions.nbFactions - 1)) {
                // Plus qu'un seul joueur ! le retrouve
                for (tourFaction = 1; tourFaction <= factions.nbFactions; tourFaction++) {
                    if (!((factions.getArmada(tourFaction).Count == 0) && (villes.getPays(tourFaction).Count == 0))) {
                        break;
                    }
                }
                if (factions.getFaction(tourFaction).humanPlayer) {
                    // Victoire
                    addOverlay(new Victoire(this));
                }
                else {
                    // Défaite
                    addOverlay(new Defaite(this));
                }

            }
        }

        /// <summary>
        /// Sauve une partie
        /// </summary>
        /// <param name="nomFichier">nom fichier sauvegarde</param>
        public void saveGame(string nomFichier) {
            StreamWriter file = new StreamWriter(nomFichier);
            // Sauvegarde du jeu...
            // Factions
            factions.save(file);
            // Villes
            villes.save(file);
            // Armées & Créatures
            creatures.save(file);
            // Ruines
            ruines.save(file);
            // Tour Faction
            file.WriteLine(tourFaction.ToString());
            file.WriteLine(noTour.ToString());
            file.Close();
            messageInfo = "Jeu sauvegardé (" + nomFichier + ")";
        }

        /// <summary>
        /// Charge une partie
        /// </summary>
        /// <param name="nomFichier">nom fichier sauvegarde</param>
        public void loadGame(string nomFichier) {
            StreamReader file = new StreamReader(nomFichier);
            bool ok = false;
            // Charge le jeu...
            ok = factions.load(file);
            // Villes
            ok &= villes.load(file);
            // Armées & Créatures
            ok &= creatures.load(file);
            // Ruines
            ok &= ruines.load(file);
            // Tour faction
            tourFaction = int.Parse(file.ReadLine());
            noTour = int.Parse(file.ReadLine());
            file.Close();
            if (!ok) {
                // Foutu
                MessageBox.Show("Mauvais format de fichier... Je ne supporte pas cela !", "Erreur fatale", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new InvalidDataException("Erreur au chargement");
            }
            selectedArmee = null;
            selectedArmeeGO = false;
            creatures.selectNext();
            IAJoue.reinit();

            messageInfo = "Jeu chargé";
        }


        /// <summary>
        /// Appelée à chaque changement de tour d'une faction
        /// </summary>
        public void debutTour() {
            if (factions.nouvelHeros()) {
                // Affiche un overlay de personalisation du héros
                if (factions.getFaction(tourFaction).humanPlayer) {
                    addOverlay(new ChoixHeros(this));
                }
                // Ordinateur : au hasard
                else {
                    bool sexe = (rnd.Next(2) == 0);
                    creatures.createHeros(ChoixHeros.nomAuHasard(rnd, sexe), sexe, noTour != 1);
                }
            }
            // Réinit le mouvement & liste noire
            foreach (Armee armee in armees) {
                armee.reinitMouvement();
                armee.blackListed = false;
            }

            // Sélectionne la première armée et met la carte dessus
            creatures.selectNext();

            // Gestion de l'argent
            int totalDep = factions.getTotalDep(tourFaction);
            int totalRev = factions.getTotalRev(tourFaction);
            factions.getFaction(tourFaction).or += totalRev - totalDep;
            // Plus de sous !
            if (factions.getFaction(tourFaction).or < 0) {
                // On est sympa...
                factions.getFaction(tourFaction).or = 0;
            }


            // Ecran de changement de toour
            if (factions.getFaction(tourFaction).humanPlayer) {
                addOverlay(new NouveauTour(this));
            }
        }


    }
}
