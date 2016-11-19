using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeigneursDeGuerre.Moteur {
    /// <summary>
    /// Exception custom très simple pour les erreurs de conf / le debuggage
    /// </summary>
    public class SdGException : Exception {
        // Résumé :
        //     Initialise une nouvelle instance de la classe System.Exception.
        public SdGException() : base() {}
        //
        // Résumé :
        //     Initialise une nouvelle instance de la classe System.Exception avec un message
        //     d'erreur spécifié.
        //
        // Paramètres :
        //   message:
        //     Message décrivant l'erreur.
        public SdGException(string message) : base(message) {}

        //
        // Résumé :
        //     Initialise une nouvelle instance de la classe System.Exception avec un message
        //     d'erreur spécifié et une référence à l'exception interne qui est à l'origine
        //     de cette exception.
        //
        // Paramètres :
        //   message:
        //     Message d'erreur expliquant la raison de l'exception.
        //
        //   innerException:
        //     Exception à l'origine de l'exception en cours ou référence null (Nothing
        //     en Visual Basic) si aucune exception interne n'est spécifiée.
        public SdGException(string message, Exception innerException) : base(message, innerException) { }
    }
}
