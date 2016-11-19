using System;
using System.Windows.Forms;

namespace SeigneursDeGuerre {
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            // Détermine un emplacement de fichier..
            SaveFileDialog file = new SaveFileDialog();
            file.DefaultExt = "SDG";
            file.AddExtension = true;
            file.CheckPathExists = true;
            file.CreatePrompt = false;
            file.OverwritePrompt = false;
            file.Filter = "Fichiers de sauvegarde|*.SDG";
            file.Title = "Choix de l'emplacement de la sauvegarde";
            if (file.ShowDialog() == DialogResult.OK) {
                
                using (MainGame game = new MainGame()) {
                    game.saveFile = file.FileName;                   
                    game.Run();
                }
            }
        }
    }
#endif
}

