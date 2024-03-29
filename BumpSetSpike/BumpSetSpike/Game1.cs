using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MBHEngine.GameObject;
using MBHEngine.IO;
using MBHEngine.Math;
using MBHEngine.Input;
using MBHEngine.Debug;
using MBHEngine.Render;
using MBHEngine.Trial;
using MBHEngine.World;
using Microsoft.Xna.Framework.Input;
using MBHEngine.Behaviour;
using Microsoft.Xna.Framework.Input.Touch;
using BumpSetSpike.Behaviour;
using BumpSetSpike.Gameflow;
using Microsoft.Xna.Framework.Media;
#if WINDOWS_PHONE
using Microsoft.Phone.Shell;
#endif
using MBHEngineContentDefs;
using Microsoft.Xna.Framework.GamerServices;

namespace BumpSetSpike
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager mGraphics;
        private SpriteBatch mSpriteBatch;

        /// <summary>
        /// Color of the sky used for flushing the screen to a color which will transition seamlessly
        /// with main menu.
        /// </summary>
        private Color mSkyColor;

        /// <summary>
        /// Whether or not to draw debug information.
        /// </summary>
        private Boolean mDebugDrawEnabled;

        /// <summary>
        /// Debug controls for skipping updates calls to help debug in "slow motion".
        /// </summary>
        private Int32 mFrameSkip = 0;
        private Int32 mFameSkipCount = 0;
        private Boolean mFreeze = false;
#if DEBUG
        private Boolean mSkipKeyIncDown = false;
        private Boolean mSkipKeyDecDown = false;
        private Boolean mFreezeKeyDown = false;
#endif

        public Game1()
        {
            Reset(null);
        }

        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="args">Command-line arguments passed to the executable.</param>
        public Game1(string[] args)
        {
            Reset(args);
        }

        public void Reset(string[] args)
        {
            CommandLineManager.pInstance.pArgs = args;

            mGraphics = new GraphicsDeviceManager(this);

            // WINDOWS_PHONE = 800x480

#if WINDOWS
#if SMALL_WINDOW
            mGraphics.PreferredBackBufferWidth = 640;
            mGraphics.PreferredBackBufferHeight = 360;
#else
            mGraphics.PreferredBackBufferWidth = 1280; // 1366; // 1280;
            mGraphics.PreferredBackBufferHeight = 720; // 768; // 720;
#endif
#endif

#if __ANDROID__
            //mGraphics.PreferredBackBufferWidth = 1280; // 1366; // 1280;
            //mGraphics.PreferredBackBufferHeight = 720; // 768; // 720;
			mGraphics.IsFullScreen = true;
            //mGraphics.PreferredBackBufferHeight = Window.ClientBounds.Width;
            //mGraphics.PreferredBackBufferWidth = Window.ClientBounds.Height;
#endif

#if false
            mGraphics.PreferredBackBufferWidth = 1920; // 1280;
            mGraphics.PreferredBackBufferHeight = 1080; // 720;
            //mGraphics.PreferredBackBufferWidth = mGraphics.GraphicsDevice.DisplayMode.Width;
            //mGraphics.PreferredBackBufferHeight = mGraphics.GraphicsDevice.DisplayMode.Height;
            mGraphics.IsFullScreen = false;
#endif

			Content.RootDirectory = "Content";

            // Avoid the "jitter".
            // http://forums.create.msdn.com/forums/p/9934/53561.aspx#53561
            // Set to TRUE so that we can target 30fps to match windows phone.
            // Should be FALSE to fix jitter issue.
            IsFixedTimeStep = true;

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            mSkyColor = new Color(146, 225, 255);

            //mGraphics.GraphicsDevice.PresentationParameters.MultiSampleType = MultiSampleType.TwoSamples;
            //mGraphics.GraphicsDevice.RenderState.MultiSampleAntiAlias = true;

            // THIS BREAKS ON WP8!
            //mGraphics.PreferMultiSampling = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            mSpriteBatch = new SpriteBatch (GraphicsDevice);        

            GameObjectManager.pInstance.Initialize(Content, mGraphics);
            GameObject.AddBehaviourCreator(new ClientBehaviourCreator());
            GameObjectFactory.pInstance.Initialize();
            CameraManager.pInstance.Initialize(mGraphics.GraphicsDevice);
            CameraManager.pInstance.pNumBlendFrames = 30;
            StopWatchManager.pInstance.Initialize();
            ScoreManager.pInstance.Initialize();
            TutorialManager.pInstance.Initialize();
            DebugShapeDisplay.pInstance.Initialize();
            DebugMessageDisplay.pInstance.Initialize();
            MusicManager.pInstance.Initialize();
            LeaderBoardManager.pInstance.Initialize();
            SaveGameManager.pInstance.Inititalize();
            SaveGameManager.pInstance.ReadSaveGameXML();
            RandomManager.pInstance.Initialize();
            TrialModeManager.pInstance.Initialize();
            AchievementManager.pInstance.Initialize();
            //SaveGameManager.pInstance.WriteSaveGameXML();
            //SaveGameManager.pInstance.ReadSaveGameXML();

            // enable the gestures we care about. you must set EnabledGestures before
            // you can use any of the other mGesture APIs.
            // we use both Tap and DoubleTap to workaround a bug in the XNA GS 4.0 Beta
            // where some Taps are missed if only Tap is specified.
            TouchPanel.EnabledGestures =
                GestureType.Hold |
                GestureType.Tap |
                GestureType.DoubleTap |
                GestureType.FreeDrag |
                GestureType.Flick |
                GestureType.Pinch;

#if DEBUG
            // By default, in DEBUG the debug drawing is enabled.
            mDebugDrawEnabled = true;
#else
            // In release it is not.
            mDebugDrawEnabled = false;
#endif

            //IsMouseVisible = mDebugDrawEnabled;
            IsMouseVisible = true;

#if true
            Guide.SimulateTrialMode = false;
#endif

#if WINDOWS
#if !MONOGL
            System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(this.Window.Handle);
            form.Location = new System.Drawing.Point(70, 0);
#endif
#endif

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Add any objects desired to the Game Object Factory.  These will be allocated now and can
            // be retrived later without any heap allocations.
            //
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\Items\\Blood\\Blood", 4);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\Items\\Dust\\Dust", 64);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\Items\\Kabooom\\Kabooom", 4);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\Items\\Sparks\\Sparks", 64);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\Items\\SparkEmitter\\SparkEmitter", 4);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\Items\\Tutorial\\Faster\\Faster", 4);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\Items\\Tutorial\\Slower\\Slower", 4);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\CreditsButton\\CreditsButton", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\FacebookButton\\FacebookButton", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\FSMPauseScreen\\FSMPauseScreen", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\GoogleLeaderboardButton\\GoogleLeaderboardButton", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\HiScoreLabel\\HiScoreLabel", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\HitCountDisplay\\HitCountDisplay", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\HitCountDisplayRecord\\HitCountDisplayRecord", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\IndieDBButton\\IndieDBButton", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\LeaveCreditsButton\\LeaveCreditsButton", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\MainMenu\\FSMMainMenu\\FSMMainMenu", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\EnduranceModeBG\\EnduranceModeBG", 2);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\EnduranceModeDesc\\EnduranceModeDesc", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\EnduranceModeButton\\EnduranceModeButton", 2);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\GoButton\\GoButton", 2);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\ModeSelectBG\\ModeSelectBG", 2);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\ModeSelectTitle\\ModeSelectTitle", 2);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\ScoreAttackModeBG\\ScoreAttackModeBG", 2);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\ScoreAttackModeButton\\ScoreAttackModeButton", 2);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\ScoreAttackModeDesc\\ScoreAttackModeDesc", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\NewHighScore\\NewHighScore", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\NumFont\\NumFont", 128);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\NumFontUI\\NumFontUI", 128);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\Options\\Tutorial\\Checkbox\\Checkbox", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\Options\\Tutorial\\Checkmark\\Checkmark", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\Options\\Tutorial\\Label\\Label", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\PauseAchievementsButton\\PauseAchievementsButton", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\PauseMainMenuButton\\PauseMainMenuButton", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\PauseQuitButton\\PauseQuitButton", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\PauseResumeButton\\PauseResumeButton", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\PauseTrialModePurchaseButton\\PauseTrialModePurchaseButton", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\PointDisplay\\PointDisplay", 32);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\RecentTrickDisplay\\RecentTrickDisplay", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\ScoreLabel\\ScoreLabel", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\ScoreSummary\\ScoreSummary", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\ScoreSummaryBG\\ScoreSummaryBG", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\TapStart\\TapStart", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\TrialModeLimit\\FSMTrialModeLimit\\FSMTrialModeLimit", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\TrialModeLimit\\TrialModeWatermark\\TrialModeWatermark", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\TrialModeLimit\\TrialModeInputDisabled\\TrialModeInputDisabled", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\TrialModeLimit\\TrialModeLimitReached\\TrialModeLimitReached", 2);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\TrialModeLimit\\TrialModeLimitReachedBG\\TrialModeLimitReachedBG", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\TrialModeLimit\\TrialModePurchaseButton\\TrialModePurchaseButton", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\TrialModeLimit\\TrialModeEndGameButton\\TrialModeEndGameButton", 1);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\Tutorial\\TapToContinue\\TapToContinue", 1);

            // Add objects that exist from the moment the game starts.
            //
            GameObjectManager.pInstance.Add(new GameObject("GameObjects\\Items\\Court\\Court"));
            GameObjectManager.pInstance.Add(new GameObject("GameObjects\\Items\\Net\\Net"));
            GameObjectManager.pInstance.Add(new GameObject("GameObjects\\UI\\Credits\\Backdrop\\Backdrop"));
            GameObjectManager.pInstance.Add(new GameObject("GameObjects\\UI\\Credits\\BG\\BG"));
            //GameObjectManager.pInstance.Add(new GameObject("GameObjects\\UI\\Credits\\CKuklaButton\\CKuklaButton"));
            GameObjectManager.pInstance.Add(new GameObject("GameObjects\\UI\\Credits\\MHughsonButton\\MHughsonButton"));
            GameObjectManager.pInstance.Add(new GameObject("GameObjects\\UI\\Credits\\MImmonenButton\\MImmonenButton"));
            GameObjectManager.pInstance.Add(new GameObject("GameObjects\\UI\\Credits\\SMcGeeButton\\SMcGeeButton"));
            GameObjectManager.pInstance.Add(new GameObject("GameObjects\\UI\\Credits\\SPaxtonButton\\SPaxtonButton"));
            GameObjectManager.pInstance.Add(new GameObject("GameObjects\\UI\\GameOver\\GameOver"));
            GameObjectManager.pInstance.Add(new GameObject("GameObjects\\UI\\PauseButton\\PauseButton"));
            GameObjectManager.pInstance.Add(new GameObject("GameObjects\\UI\\PausedBackdrop\\PausedBackdrop"));
            GameObjectManager.pInstance.Add(new GameObject("GameObjects\\UI\\PausedOverlay\\PausedOverlay"));

            GameObject titleScreen = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\MainMenu\\FSMMainMenu\\FSMMainMenu");
            GameObjectManager.pInstance.Add(titleScreen);

            GameObjectManager.pInstance.Add(GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\TrialModeLimit\\FSMTrialModeLimit\\FSMTrialModeLimit"));
            GameObjectManager.pInstance.Add(GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\TrialModeLimit\\TrialModeWatermark\\TrialModeWatermark"));

            // Disabled recent trick display. Doesn't look very good and seems not very useful.
            //GameObjectManager.pInstance.Add(GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\RecentTrickDisplay\\RecentTrickDisplay"));

            GameObjectManager.pInstance.Add(GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\ScoreLabel\\ScoreLabel"));
            GameObjectManager.pInstance.Add(GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\HitCountDisplay\\HitCountDisplay"));
            GameObjectManager.pInstance.Add(GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\HiScoreLabel\\HiScoreLabel"));
            GameObjectManager.pInstance.Add(GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\HitCountDisplayRecord\\HitCountDisplayRecord"));

            // The tiled background image that travels will the player creating the illusion of
            // an infinite background image.
            GameObject bg = new GameObject();
            MBHEngine.Behaviour.Behaviour t = new InfiniteBG(bg, null);
            bg.AttachBehaviour(t);
            bg.pRenderPriority = 20;
            GameObjectManager.pInstance.Add(bg);
            
            // Create the level.
            WorldManager.pInstance.Initialize();

            // Debug display for different states in the game.  This by creating new behaviours, additional
            // stats can be displayed.
            GameObject debugStatsDisplay = new GameObject();
            MBHEngine.Behaviour.Behaviour fps = new MBHEngine.Behaviour.FrameRateDisplay(debugStatsDisplay, null);
            debugStatsDisplay.AttachBehaviour(fps);
            GameObjectManager.pInstance.Add(debugStatsDisplay);

            // The player himself.
            GameObject player = new GameObject("GameObjects\\Characters\\Player\\Player");
            GameObjectManager.pInstance.Add(player);

            // Store the player for easy access.
            GameObjectManager.pInstance.pPlayer = player;

            GroundShadow.SetTargetMessage setTarg = new GroundShadow.SetTargetMessage();

            GameObject shadow = new GameObject("GameObjects\\Items\\PlayerShadow\\PlayerShadow");
            GameObjectManager.pInstance.Add(shadow);
            setTarg.mTarget_In = player;
            shadow.OnMessage(setTarg);

            GameObject ball = new GameObject("GameObjects\\Items\\Ball\\Ball");
            GameObjectManager.pInstance.Add(ball);
            shadow = new GameObject("GameObjects\\Items\\BallShadow\\BallShadow");
            GameObjectManager.pInstance.Add(shadow);
            setTarg.mTarget_In = ball;
            shadow.OnMessage(setTarg);

            GameObject partner = new GameObject("GameObjects\\Characters\\Partner\\Partner");
            GameObjectManager.pInstance.Add(partner); 
            shadow = new GameObject("GameObjects\\Items\\PlayerShadow\\PlayerShadow");
            GameObjectManager.pInstance.Add(shadow);
            setTarg.mTarget_In = partner;
            shadow.OnMessage(setTarg);

            GameObject opponent = new GameObject("GameObjects\\Characters\\Opponent\\Opponent");
            GameObjectManager.pInstance.Add(opponent);
            shadow = new GameObject("GameObjects\\Items\\PlayerShadow\\PlayerShadow");
            GameObjectManager.pInstance.Add(shadow);
            setTarg.mTarget_In = opponent;
            shadow.OnMessage(setTarg);

            opponent = new GameObject("GameObjects\\Characters\\Opponent\\Opponent");
            opponent.pPosX = 75.0f;
            GameObjectManager.pInstance.Add(opponent);
            shadow = new GameObject("GameObjects\\Items\\PlayerShadow\\PlayerShadow");
            GameObjectManager.pInstance.Add(shadow);
            setTarg.mTarget_In = opponent;
            shadow.OnMessage(setTarg);

            // The vingette effect used to dim out the edges of the screen.
            //GameObject ving = new GameObject("GameObjects\\Interface\\Vingette\\Vingette");
#if SMALL_WINDOW
            //ving.pScale = new Vector2(0.5f, 0.5f);
#endif
            //GameObjectManager.pInstance.Add(ving);

            DebugMessageDisplay.pInstance.AddConstantMessage("Game Load Complete.");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // If someone told us to exit, do it right away.
            if (GameObjectManager.pInstance.pCurUpdatePass == BehaviourDefinition.Passes.QUIT)
            {
                Exit();

                return;
            }

            if (!IsActive)
            {
                // Prevent the debug shape queue from getting too backed up.
                DebugShapeDisplay.pInstance.Update();

                base.Update(gameTime);
                return;
            }

            InputManager.pInstance.UpdateBegin();

            // Bit of a hack to handle pressing back while popup is active.
            if (InputManager.pInstance.CheckAction(InputManager.InputActions.BACK, true))
            {

                if (GameObjectManager.pInstance.pCurUpdatePass == BehaviourDefinition.Passes.GAME_PLAY)
                {
                    GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.GAME_PLAY_PAUSED;
                    GameObjectManager.pInstance.Add(GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\FSMPauseScreen\\FSMPauseScreen"));
                }
            }

            // Toggle the debug drawing with a click of the left stick.
            if (InputManager.pInstance.CheckAction(InputManager.InputActions.L3, true))
            {
                mDebugDrawEnabled ^= true;

                // When debug draw is enabled, turn on the hardware mouse so that things like the
                // GameObjectPicker work better.
                //IsMouseVisible = mDebugDrawEnabled;
            }
#if DEBUG
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.OemPlus))
            {
                if (!mSkipKeyDecDown)
                {
                    mFrameSkip = Math.Max(mFrameSkip - 1, 0);
                }

                mSkipKeyDecDown = true;
            }
            else
            {
                mSkipKeyDecDown = false;
            }

            if (keyboardState.IsKeyDown(Keys.OemMinus))
            {
                if (!mSkipKeyIncDown)
                {
                    mFrameSkip++;
                }

                mSkipKeyIncDown = true;
            }
            else
            {
                mSkipKeyIncDown = false;
            }

            if (keyboardState.IsKeyDown(Keys.D0))
            {
                if (!mFreezeKeyDown)
                {
                    mFreeze ^= true;
                }

                mFreezeKeyDown = true;
            }
            else
            {
                mFreezeKeyDown = false;
            }
#endif
            // If we are skipping frames, check if enough have passed before doing updates.
            if (mFameSkipCount >= mFrameSkip && !mFreeze)
            {
                DebugMessageDisplay.pInstance.ClearDynamicMessages();
                DebugShapeDisplay.pInstance.Update();

                //DebugMessageDisplay.pInstance.AddDynamicMessage("Game-Time Delta: " + gameTime.ElapsedGameTime.TotalSeconds);
                //DebugMessageDisplay.pInstance.AddDynamicMessage("Path Find - Unused: " + MBHEngine.PathFind.GenericAStar.Planner.pNumUnusedNodes);
                //DebugMessageDisplay.pInstance.AddDynamicMessage("Graph Neighbour - Unused: " + MBHEngine.PathFind.GenericAStar.GraphNode.pNumUnusedNeighbours);
                //DebugMessageDisplay.pInstance.AddDynamicMessage("NavMesh - Unused: " + MBHEngine.PathFind.HPAStar.NavMesh.pUnusedGraphNodes);
                //DebugMessageDisplay.pInstance.AddDynamicMessage("Frame Skip: " + mFrameSkip);

                mFameSkipCount = 0;
                StopWatchManager.pInstance.Update();
                GameObjectManager.pInstance.Update(gameTime);
            }
            else
            {
                mFameSkipCount++; 
            }

            if (mDebugDrawEnabled)
            {
                // This does some pretty expensive stuff, so only do it when it is really useful.
                GameObjectPicker.pInstance.Update(gameTime, (mFameSkipCount == 0));
            }

            TutorialManager.pInstance.Update(gameTime);
            InputManager.pInstance.UpdateEnd();
            CameraManager.pInstance.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(mSkyColor);

            // First draw all the objects managed by the game object manager.
            GameObjectManager.pInstance.Render(mSpriteBatch, (mFameSkipCount == 0));

            if (mDebugDrawEnabled)
            {
                DebugShapeDisplay.pInstance.Render();

                // We need to go back to standard alpha blend before drawing the debug layer.
                mSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                DebugMessageDisplay.pInstance.Render(mSpriteBatch);
                mSpriteBatch.End();
            }

            base.Draw(gameTime);
        }
        
        /// <summary>
        /// Call when the application starts or resumes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void OnActivated(object sender, EventArgs args)
        {
            base.OnActivated(sender, args);

#if WINDOWS
            // IsTrialMode does not work on Windows.
            // On android assume we are in trial mode until we are told otherwise.
            TrialModeManager.pInstance.pIsTrialMode = true;
#elif __ANDROID__
#else
            //Guide.SimulateTrialMode ^= true;
            TrialModeManager.pInstance.pIsTrialMode = Guide.IsTrialMode;
#endif
        }

        /// <summary>
        /// Called when the application is shutting down. It is not called when it is temporarily 
        /// deactivating.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void OnExiting(object sender, EventArgs args)
        {
            GameObjectManager.pInstance.BroadcastMessage(new SaveGameManager.ForceUpdateSaveDataMessage());

            // Save the current state of the game on exit.
            SaveGameManager.pInstance.WriteSaveGameXML();

            base.OnExiting(sender, args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void OnDeactivated(object sender, EventArgs args)
        {
            base.OnDeactivated(sender, args);

            if (GameObjectManager.pInstance.pCurUpdatePass == BehaviourDefinition.Passes.GAME_PLAY)
            {
                GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.GAME_PLAY_PAUSED;
                GameObjectManager.pInstance.Add(GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\FSMPauseScreen\\FSMPauseScreen"));
            }
        }
    }
}
