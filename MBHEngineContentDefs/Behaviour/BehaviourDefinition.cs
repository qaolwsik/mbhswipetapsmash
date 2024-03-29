﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace MBHEngineContentDefs
{
    public class BehaviourDefinition
    {
        /// <summary>
        /// Allows game objects to only be updated or rendered during specific "Passes" which
        /// can be switched at runtime.
        /// </summary>
        public enum Passes
        {
            MAIN_MENU = 0,
            CREDITS,
            TUTORIAL_PAUSE,
            GAME_PLAY,
            GAME_OVER,
            GAME_OVER_LOSS,
            GAME_PLAY_PAUSED,
            TRIAL_LIMIT_REACHED,
            QUIT,
        }

        /// <summary>
        /// If specified the object will only be updated during this pass.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public List<Passes> mUpdatePasses;

        /// <summary>
        /// Do not render when the current GameObject pass is in this list.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public List<Passes> mRenderPasses;

        /// <summary>
        /// When true the behaviour is updated and rendered every frame.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Boolean mIsEnabled = true;
    }
}
