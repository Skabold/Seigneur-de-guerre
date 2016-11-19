using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    /// Classe décrivant un objet caché dans une ruine
    /// </summary>
    class ItemDescription {
        public enum Effet {
            VOL,
            NAGE,
            BONUS_FORCE,
            BONUS_MOUVEMENT,
            BONUS_OR
        };

        /// <summary>
        /// Nom de l'item
        /// </summary>
        public string nom;

        /// <summary>
        /// Effet de l'item
        /// </summary>
        public Effet effet;
        /// <summary>
        /// Amplitude de l'effet (applicable aux boni seulement)
        /// </summary>
        public int amplitude;

        /// <summary>
        /// Nom de l'effet sous forme de chaîne
        /// </summary>
        public string description {
            get {
                string ret = "inconnu";
                switch (effet) {
                    case Effet.VOL:
                        ret = "fait voler l'armée";
                        break;
                    case Effet.BONUS_FORCE:
                        ret = "force +" + amplitude;
                        break;
                    case Effet.BONUS_MOUVEMENT:
                        ret = "mouvement +" + amplitude;
                        break;
                    case Effet.BONUS_OR:
                        ret = "rapporte " + amplitude + " p.o.";
                        break;
                    case Effet.NAGE:
                        ret = "fait naviguer l'armée";
                        break;
                }
                return ret;
            }
        }

    }

}
