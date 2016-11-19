using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SeigneursDeGuerre.Outils;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    ///  Gestion des items
    /// </summary>
    class Items {
        /// <summary>
        /// N° d'item correspondant à l'étendard (possession de base du héros)
        /// </summary>
        public const int ETENDARD = 0;
        /// <summary>
        /// La description sous-jacente
        /// </summary>
        private List<ItemDescription> _description;
        /// <summary>
        /// Instance partagée du jeu
        /// </summary>
        private Jeu _jeu;

        
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="leJeu">jeu en cours</param>
        public Items(Jeu leJeu) {
            _jeu = leJeu;         
        }
        /// <summary>
        /// Créateur à partir de fichier de description des villes
        /// </summary>
        /// <param name="leJeu">jeu en cours</param>
        /// <param name="nomFichierRessource">nom du fichier texte (sans préfixe SeigneursDeGuerre.)</param>
        public Items(Jeu leJeu, string nomFichierRessource) : this(leJeu) {
            // Format du fichier :
            // nom;effet;amplitude
            // (header présent dans le fichier)
            _description = new List<ItemDescription>();

            CSVReader csv = new CSVReader(nomFichierRessource, ";");
            // header
            if (!csv.readLine()) {
                throw new SdGException("Le fichier des items est vide");
            }; 
            try {
                while (csv.readLine()) {
                    ItemDescription id = new ItemDescription();
                    id.nom = csv.getField(0);
                    id.effet = (ItemDescription.Effet)Enum.Parse(typeof(ItemDescription.Effet), csv.getField(1), true);
                    
                    id.amplitude = Int32.Parse(csv.getField(2));
                    _description.Add(id);
                }
            }
            finally {
                csv.dispose();
            }


        }

        /// <summary>
        /// Accesseurs sur la description
        /// </summary>
        public List<ItemDescription> itemDesc {
            get { return _description; }         
        }



    }
}
