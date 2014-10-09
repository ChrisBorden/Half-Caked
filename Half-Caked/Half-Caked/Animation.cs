#region File Description
//-----------------------------------------------------------------------------
// Animation.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

//-------------------------------
//
// Adapted for Half_Caked 3/31/12
//
//-------------------------------

#endregion

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Half_Caked
{
    /// <summary>
    /// Represents an animated texture.
    /// </summary>
    /// <remarks>
    /// Currently, this class assumes that each frame of animation is
    /// as wide as each animation is tall. The number of frames in the
    /// animation are inferred from this.
    /// </remarks>
    class Animation
    {
        /// <summary>
        /// All frames in the animation arranged horizontally.
        /// </summary>
        public Texture2D Texture
        {
            get { return texture; }
        }
        Texture2D texture;

        /// <summary>
        /// Duration of time to show each frame.
        /// </summary>
        /*public float FrameTime
        {
            get { return frameTime; }
        }
        float frameTime;*/

		private float[] frameTime;

		public float getFrameTime(int frameInd) {
			if( frameInd > -1 && frameInd < frameTime.Length )
				return frameTime[frameInd];
			else
				return -1.0f;
		}

        /// <summary>
        /// When the end of the animation is reached, should it
        /// continue playing from the beginning?
        /// </summary>
        public bool IsLooping
        {
            get { return isLooping; }
        }
        bool isLooping;

        /// <summary>
        /// Gets the number of frames in the animation.
        /// </summary>
        public int FrameCount
        {
            get { return frameCount; }
			set { if(value > 0) this.frameCount = value; }
        }
		private int frameCount;

        /// <summary>
        /// Gets the width of a frame in the animation.
        /// </summary>
        public float FrameWidth
        {
            get { return Texture.Width / (float) frameCount; }
        }

        /// <summary>
        /// Gets the height of a frame in the animation.
        /// </summary>
        public int FrameHeight
        {
            get { return Texture.Height; }
        }

        /// <summary>
        /// New animation constructor with variable frame times.
		/// frameTime contains timings for each frame, should be equal
		/// in length to the number of frames.
        /// </summary>        
        public Animation(Texture2D texture, float[] frameTime, bool isLooping)
        {
            this.texture = texture;
			this.frameTime = frameTime;
			this.frameCount = frameTime.Length;
            this.isLooping = isLooping;
        }

		/// <summary>
		/// New animation constructor with constant frame times.
		/// </summary>
		public Animation(Texture2D texture, float frameTime, int frameCount, 
			bool isLooping)
		{
			this.texture = texture;
			this.frameTime = new float[frameCount];
			for (int i = 0; i < frameCount; i++)
				this.frameTime[i] = frameTime;
			this.frameCount = frameCount;
			this.isLooping = isLooping;
		}
    }
}
