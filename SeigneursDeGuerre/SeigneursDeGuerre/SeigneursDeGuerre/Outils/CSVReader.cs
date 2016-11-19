using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeigneursDeGuerre.Outils {
    class CSVReader {
        
        /// <summary>
        /// Le flux
        /// </summary>
        private System.IO.StreamReader _file;
        /// <summary>
        /// le séparateur
        /// </summary>
        private string _separator;
        /// <summary>
        /// Les champs de la ligne courante
        /// </summary>
        private string[] _currentFields = null;
        
        /// <summary>
        /// Crée un lecteur CSV à partir d'un fichier de ressource. N'oubliez pas d'appeler dispose() quand la classe n'est plus utile.
        /// </summary>
        /// <param name="ressourceFileName">chemin du fichier dans les ressources (le préfixe "SeigneursDeGuerre." est ajouté automatiquement)</param>
        /// <param name="separator">séparateur du fichier CSV</param>
        public CSVReader(string ressourceFileName, string separator) {
            _file = new System.IO.StreamReader(System.Reflection.Assembly.GetEntryAssembly().
                                GetManifestResourceStream("SeigneursDeGuerre.Rsc." + ressourceFileName));
            _separator = separator;

        }

        /// <summary>
        /// Lit une ligne du fichier
        /// </summary>
        /// <returns>true si une ligne nouvelle a été lue, false si la fin du fichier est atteinte</returns>
        public bool readLine() {
            bool retVal;
            string line = _file.ReadLine();
            if (line != null) {
                _currentFields = line.Split(new string[] { _separator }, StringSplitOptions.None);
                retVal = true;
            }
            else {
                _currentFields = null;
                retVal = false;
            }
            return retVal;
        }

        /// <summary>
        /// Retourne une colonne de la ligne courante
        /// </summary>
        /// <param name="fieldNumber">numéro de champ (commence à 0)</param>
        /// <returns>le champ ou null si il n'y a pas de champ de cet indice</returns>
        public string getField(int fieldNumber) {
            if (fieldNumber < getFieldsNumber()) {
                return _currentFields[fieldNumber];
            }
            else {
                return null;
            }
        }

        /// <summary>
        /// Retourne le nombre de colonnes dans la ligne courante
        /// </summary>
        /// <returns>le nombre de colonnes dans la ligne courante</returns>
        public int getFieldsNumber() {
            if (_currentFields != null) {
                return _currentFields.Length;
            }
            else {
                return 0;
            }
        }

        /// <summary>
        /// A appeler pour fermer le fichier
        /// </summary>
        public void dispose() {
            _file.Close();
        }

    }
}
