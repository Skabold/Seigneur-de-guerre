using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SeigneursDeGuerre.Moteur;
using SeigneursDeGuerre.Overlays;
using System.IO;




namespace SeigneursDeGuerre {
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MainGame : Microsoft.Xna.Framework.Game {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Jeu jeu;

        // Chemin de sauvegarde
        public string saveFile;

        // Dernière position connue de la souris lors du précédent update()
        private int oldMouseY = 0;
        private int oldMouseX = 0;
        // Permet de savoir si on a cliqué (appuyé puis relaché)
        private Overlay overlayClicked = null;
        // Pareil pour le focus souris
        private Overlay overlayFocused = null;
        // Permet de savoir si une fenête pop d'info est affichée
        private bool popWindowInfo = false;
        // Permet de savoir si on relache le bouton gauche dans le terrain
        private bool leftClickOnTerrain = false;


        // gère le passage fullscreen
        bool fullScreentoggled = false;

        public MainGame() {
            graphics = new GraphicsDeviceManager(this);

            //this.graphics.IsFullScreen = true;
            System.Drawing.Rectangle resolution = System.Windows.Forms.Screen.PrimaryScreen.Bounds;            
            jeu = new Jeu(graphics, resolution.Width, resolution.Height);
            
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            base.Initialize();
        }
        //*****************************************************************************************************************************
        //*****************************************************************************************************************************
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        //*****************************************************************************************************************************
        //*****************************************************************************************************************************
        protected override void LoadContent() {
            jeu.graphicsDevice = GraphicsDevice;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Factions -----------------------------------------------------------
            Factions fact = new Factions(jeu);
            jeu.factions = fact;

            // Création de la carte ------------------------------------------------
            // Carte de test
            System.Drawing.Bitmap carteTest = new System.Drawing.Bitmap(
                                System.Reflection.Assembly.GetEntryAssembly().
                                GetManifestResourceStream("SeigneursDeGuerre.Rsc.Cartes.Carte_Campagne1.png"));

            TerrainDescription td = new TerrainDescription(carteTest);
            Terrain t = new Terrain(jeu);
            t.terrainDesc = td;
            jeu.terrain = t;
            carteTest.Dispose();

            // Création des villes -------------------------------------------------------
            Villes v = new Villes(jeu, "Cartes.Ville_Campagne1.txt");
            jeu.villes = v;

            // Ruines & liste d'item disponible pour la carte ----------------------------
            jeu.items = new Items(jeu, "RuinesItems.items.txt");
            jeu.ruines = new Ruines(jeu);


            // Créatures -----------------------------------------------------------------
            Creatures c = new Creatures(jeu, "Creatures.CreaturesBase.txt");
            jeu.creatures = c;


            // Armées --------------------------------------------------------------------
            // Ajoute une armée pour chaque ville du type max de construction (sauf bateau)
            foreach (VilleDescription vil in jeu.villes.villeDesc) {

                int typecrea;
                if ((vil.typeCreatures[vil.typeCreatures.Length - 1] == Creatures.TYPE_BATEAU) && (vil.typeCreatures.Length > 1)) {
                    typecrea = vil.typeCreatures[vil.typeCreatures.Length - 2];
                }
                else {
                    typecrea = vil.typeCreatures[vil.typeCreatures.Length - 1];
                }
                Creature crea = new Creature(jeu, typecrea, vil.faction, vil.positionMap);
                Armee armee = new Armee(jeu);
                armee.addCreature(crea);
                jeu.addArmee(armee);
            }

            // Gestionnaires --------------------------------------------------------------
            InfoCarte infoC = new InfoCarte(jeu);
            jeu.infoCarte = infoC;
            InteractionJoueur ijoueur = new InteractionJoueur(jeu);
            jeu.interactionJoueur = ijoueur;
            IAJoue iajoue = new IAJoue(jeu);
            jeu.IAJoue = iajoue;

            // Chargement des textures utilitaires de base
            jeu.load(Content);
            // Chargements complémentaires statiques
            PanneauControle.load(Content);

            // Si fichier existe, le charger
            if (File.Exists(this.saveFile)) {
                // Minimap
                jeu.addOverlay(new Minimap(jeu, Overlay.Position.HAUT_DROITE));
                // Paneau de contrôle
                jeu.addOverlay(new PanneauControle(jeu));
                // charge le jeu
                jeu.loadGame(saveFile);
            }
            else {
                // Choix des factions --------------------------------------------------------
                jeu.addOverlay(new FactionOverlay(jeu, delegate() {
                    // Overlays de base ----------------------------------------------------------
                    jeu.tourFaction = 1;
                    // Minimap
                    jeu.addOverlay(new Minimap(jeu, Overlay.Position.HAUT_DROITE));
                    // Paneau de contrôle
                    jeu.addOverlay(new PanneauControle(jeu));
                    // Actions de début de tour
                    jeu.debutTour();
                }));
                // Personne ne joue encore
                jeu.tourFaction = 0;
            }

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent() {            
        }


        //*****************************************************************************************************************************
        //*****************************************************************************************************************************
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        //*****************************************************************************************************************************
        //*****************************************************************************************************************************
        protected override void Update(GameTime gameTime) {

            int newMouseX = (int)((float)Mouse.GetState().X / jeu.scaleX);
            int newMouseY = (int)((float)Mouse.GetState().Y / jeu.scaleX);

            // fullscreen toggle
            if (!fullScreentoggled && Keyboard.GetState().IsKeyDown(Keys.Enter) && Keyboard.GetState().IsKeyDown(Keys.LeftAlt)) {
                this.graphics.IsFullScreen = !graphics.IsFullScreen;
                this.graphics.ApplyChanges();
                fullScreentoggled = true;
            }
            else {
                fullScreentoggled = false;
            }

            // Comportements automatiques (humain & ia) ---------------------------------------------------------------------------
            // mouvements 
            if (jeu.selectedArmeeGO) {
                jeu.selectedArmee.go(gameTime);
            }

            // Comportement humain ------------------------------------------------------------------------------------------------
            

            // Bouton G ou D appuyés
            if ((Mouse.GetState().LeftButton == ButtonState.Pressed) ||
                (Mouse.GetState().RightButton == ButtonState.Pressed)) {

                // Dans un overlay ? (si pas déjà dans autre chose)
                if (!popWindowInfo && !leftClickOnTerrain) {
                    for (int ovidx = jeu.getOverlays().Count-1;ovidx>=0;ovidx--) {
                        Overlay overlay = jeu.getOverlays()[ovidx];
                        if (overlay.isPointInOverlay(newMouseX, newMouseY)) {
                            // yes
                            overlay.clicked(newMouseX, newMouseY, Mouse.GetState().LeftButton == ButtonState.Pressed, Mouse.GetState().RightButton == ButtonState.Pressed, false);
                            overlayClicked = overlay;
                            break;
                        }
                    }
                }

                // Pas dans un overlay, mais overlay modal -> on ne fait rien
                if ((jeu.getTopOverlay().modalOverlay == Overlay.ModalOverlay.AUCUN) && (jeu.factions.getFaction(jeu.tourFaction).humanPlayer)) {
                       
                    // Pas dans un overlay - click droit = info si pas déjà fait
                    if ((Mouse.GetState().RightButton == ButtonState.Pressed) && !popWindowInfo) {
                        jeu.infoCarte.createInfoPopup(newMouseX, newMouseY);
                        popWindowInfo = true;
                    }
                    // Click gauche !
                    if ((overlayClicked == null) && (Mouse.GetState().LeftButton == ButtonState.Pressed)) {
                        jeu.interactionJoueur.leftClick(newMouseX, newMouseY, false);
                        leftClickOnTerrain = (jeu.getTopOverlay().modalOverlay == Overlay.ModalOverlay.AUCUN);
                    }
                }
            }
            else { // Boutons G & D pas appuyés
                if (overlayClicked != null) {
                    // Boutons G/D non pressés, mais relachés dans un overlay
                    overlayClicked.clicked(newMouseX, newMouseY, Mouse.GetState().LeftButton == ButtonState.Pressed, Mouse.GetState().RightButton == ButtonState.Pressed, true);
                    overlayClicked = null;
                }

                // Pas dans un overlay, mais overlay modal -> on ne fait rien
                if (jeu.getTopOverlay().modalOverlay == Overlay.ModalOverlay.AUCUN) {

                    // Allows the game to exit
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                        jeu.addOverlay(new MenuSauvegarde(jeu, saveFile, delegate() {
                            this.Exit();
                        }));
                    }

                    // Non modal
                    if (leftClickOnTerrain) {
                        // Bouton G relaché dans le terrain
                        if (jeu.factions.getFaction(jeu.tourFaction).humanPlayer) {
                            jeu.interactionJoueur.leftClick(newMouseX, newMouseY, true);
                        }
                        leftClickOnTerrain = false;
                    }
                    if (popWindowInfo) {
                        // Bouton D relaché alors qu'une fenêtre pop d'info est ouverte
                        if (jeu.factions.getFaction(jeu.tourFaction).humanPlayer) {
                            jeu.infoCarte.destroyInfoPopup();
                        }
                        popWindowInfo = false;
                    }
                    // Scrolling dans la carte sur bouton du milieu pressé
                    if (jeu.factions.getFaction(jeu.tourFaction).humanPlayer) {
                        if (Mouse.GetState().MiddleButton == ButtonState.Pressed) {
                            // Bouton M pressé

                            jeu.curseur.forme = Cursor.FormeCurseur.MAIN;

                            int deltaX = newMouseX - oldMouseX;
                            int deltaY = newMouseY - oldMouseY;
                            // bouge l'offset de la map
                            jeu.terrain.offsetCarte.X -= deltaX;
                            jeu.terrain.offsetCarte.Y -= deltaY;
                            // limites - on commence par les limites max car les limites min sont plus importantes
                            jeu.terrain.normalizeScrolling();
                        }
                        else {
                            // Bouton M non pressé
                            jeu.curseur.forme = Cursor.FormeCurseur.FLECHE;
                        }
                    }
                }
            }

            // Gestion du focus des overlays
            bool oneOverlayFocused = false;
            for (int ovidx = jeu.getOverlays().Count - 1; ovidx >= 0; ovidx--) {
                Overlay overlay = jeu.getOverlays()[ovidx];
                if (overlay.isPointInOverlay(newMouseX, newMouseY)) {
                    // yes
                    overlay.focused(Keyboard.GetState(),newMouseX, newMouseY, false);
                    oneOverlayFocused = true;
                    overlayFocused = overlay;
                    break;
                }
                // perte de focus
                else if (overlayFocused == overlay) {
                    overlayFocused = null;
                    overlay.focused(Keyboard.GetState(),newMouseX, newMouseY, true);
                }
            }
            // Focus sur la carte (changement de forme du curseur)
            if ((!oneOverlayFocused) && (jeu.getTopOverlay().modalOverlay == Overlay.ModalOverlay.AUCUN) && (Mouse.GetState().MiddleButton != ButtonState.Pressed)
                && (jeu.factions.getFaction(jeu.tourFaction).humanPlayer)) {
                jeu.interactionJoueur.focusCarte(newMouseX, newMouseY);
            }


            
            // CPU joue ---------------------------------------------
            if ((jeu.tourFaction != 0) && (!jeu.factions.getFaction(jeu.tourFaction).humanPlayer)) {
                if (jeu.getTopOverlay().modalOverlay == Overlay.ModalOverlay.AUCUN) {
                    jeu.IAJoue.cEstMonTour();
                }
            }

            // Dernière position connue de la souris lors du précédent update()
            oldMouseX = (int)((float)Mouse.GetState().X / jeu.scaleX);
            oldMouseY = (int)((float)Mouse.GetState().Y / jeu.scaleX);

            base.Update(gameTime);
        }
        //*****************************************************************************************************************************
        //*****************************************************************************************************************************
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        //*****************************************************************************************************************************
        //*****************************************************************************************************************************
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1f, 0);
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, jeu.scaleSpriteBatch);

            // Affiche la map
            jeu.terrain.draw(spriteBatch, GraphicsDevice, gameTime);

            // Affiche les villes
            jeu.villes.draw(spriteBatch, GraphicsDevice, gameTime);

            spriteBatch.End();

            // Armées - nouveau spritebatch car au dessus de la carte
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, jeu.scaleSpriteBatch);

            foreach (Armee armee in jeu.armees) {
                armee.draw(spriteBatch, GraphicsDevice, gameTime, 0.8f);
            }

            spriteBatch.End();


            // Overlays - ont leur propre spriteBatch pour être forcément au dessus
            foreach (Overlay overlay in jeu.getClonedOverlays()) {
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, jeu.scaleSpriteBatch);
                overlay.draw(spriteBatch, GraphicsDevice, gameTime);
                spriteBatch.End();
            }



            // Affiche la souris (tjrs à la fin)
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, jeu.scaleSpriteBatch);

            // Message de debug
            if (jeu.messageDebug != null) {
                spriteBatch.DrawString(jeu.font, jeu.messageDebug, new Vector2(0, 0), Color.White);
            }
            // Message d'info
            if (jeu.messageInfo != null && (jeu.messageInfoDuree>0)) {
                Vector2 size = jeu.font.MeasureString(jeu.messageInfo);
                spriteBatch.DrawString(jeu.isoFont, jeu.messageInfo, new Vector2(3, jeu.resY - size.Y - 1), Color.Black);
                spriteBatch.DrawString(jeu.isoFont, jeu.messageInfo, new Vector2(5, jeu.resY - size.Y - 1), Color.Black);
                spriteBatch.DrawString(jeu.isoFont, jeu.messageInfo, new Vector2(4, jeu.resY - size.Y - 2), Color.Black);
                spriteBatch.DrawString(jeu.isoFont, jeu.messageInfo, new Vector2(4, jeu.resY - size.Y), Color.Black);
                spriteBatch.DrawString(jeu.isoFont, jeu.messageInfo, new Vector2(4, jeu.resY - size.Y - 1), Color.Yellow);
                jeu.messageInfoDuree -= gameTime.ElapsedGameTime.TotalMilliseconds;
            }
            // Curseur            
            jeu.curseur.draw(spriteBatch, GraphicsDevice, gameTime, oldMouseX, oldMouseY);
            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
