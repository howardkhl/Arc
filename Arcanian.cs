/******************************************************************
* This class handles the main character (Arcanian). Features include
* hit detection, interactive with game world, character physics,
* projectile formation, and current state of the character. 
*
* Last modified: 6/11/2011
* Author: Howard Lee (howardkhl@gmail.com)
* Class: CSS490 Game Development
*******************************************************************/

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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Innovades_Namespace._Core;
using Innovades_Namespace._Global;
using Innovades_Namespace._Game._Skills;
using Innovades_Namespace._Game._GameUtility;
using XNACS1Lib;

namespace Innovades_Namespace._Game._Arcanian
{
    public class Arcanian : SolidCircle
    {
        #region Data members
        // Arcanian
        protected const float kRadius = 7.0f;           // Default Arcanian radius
        protected const int kHealth = 200;              // Arcanian health
        protected const int kShield = 40;               // Shield strength
        protected const float kShieldRadius = kRadius * 1.2f;
        protected const int kBlinkRate = 5;             // Blink rate
        protected const int kMaxNumBlink = 40;          // Max blink in ticks
        protected const float kMaxPower = 70;           // Maximum power limit
        protected const float kMinPower = 5;            // Minimum power
        protected const int kMaxUltimate = kHealth / 2; // Ultimate Capacity
        protected const int kShieldRegenRate = 5;       // Shield regen cycle
        protected const int kShieldArtCol = 4;          // Shield art columns
        protected const float kSpeed = 0.5f;            // Default speed
        protected Vector2 kVelocity = new Vector2(0f, -0.1f);     // Falling velocity
        protected float kJumpStrength = 1.4f;
        protected int kJumpTP = 10;
        
        protected ArcanianState mCurrentState;          // Current state of Arcanian
        protected float prevVelocityY = 0.0f;           // Previous velocity
        protected bool mFacingLeft;                     // Arcanian face direction
        protected string mName;                         // Arcanian name
        protected int mHealth;                          // Arcanian health
        protected int mUltimateBar;                     // Ultimate time
        protected float mPower;                         // Power to shoot projectile
        protected XNACS1Circle mShieldArt;              // Shield art
        protected bool mIsBlinking;                     // Is Arcanian blinking
        protected int mBlinkTimer;                      // Blink timer
        protected int mNumBlink;                        // Num blinks
        protected bool mPlayShieldRegenCue;
        protected PlayerIndex mPlayerIndex;           // Who's the controller of this arcanian

        // Texture
        protected string texArcanianRight;
        protected string texArcanianLeft;
        protected string texDyingRight;
        protected string texDyingLeft;
        protected string texShield;

        // Aimer
        protected const float kAimerAngle = 25.0f;      // Default aimer angle
        protected const float kAimerBarWidth = 20.0f;   // Aimer bar width
        protected const float kAimerBarHeight = 2.0f;   // Aimer bar height
        protected const float kAimerRadius = 2.0f;      // Aimer radius
        protected const float kAimerSpeed = 3.0f;       // Aimer rotation speed
        protected XNACS1Rectangle mAimerBar;            // Aimer bar for aimer
        protected XNACS1Circle mAimer;                  // directional aimer
        protected float kBoundWidth = 34.0f;
        protected float kBoundHeight = 0.5f;
        protected XNACS1Rectangle mBoundMin;
        protected XNACS1Rectangle mBoundMax;
        protected string texBound = "Arcanian/simplebound";

        // Skills
        protected SkillSet mSkillSet;           // Arcanian skill set
        protected Skill mCurSkill;              // Arcanian skill

        // Minimap
        protected float mMiniMapXPosition;      // Minimap x position
        protected float mMiniMapYPosition;      // Minimap y position

        // Technical Points System
        protected const int kMaxTechnicalPoints = 100;  // Default cool down time
        protected const int kRequiredRechargeTime = 4;  // Req Rechange Time in Secs
        protected int mTechnicalPoints;                 // Technical Points (TP)
        protected int mShield;                          // Arcanian shield
        //protected bool mSkill1Available;                // Skill1
        //protected bool mSkill2Available;                // Skill2
        protected int mRechargeTimer;                   // Time to fully restore TP
        protected bool mRechargeTP;                     // Whether to recharge TP
        protected int mWalkTimer;                       // Walk timer
        protected int mTPRegenTimer;                    // TP regeneration timer
        protected int mSpawnTimer;                      // Spawn timer
        protected int mFiringDelay;
        protected _Utility.InputState myInput;
        #endregion

        /// <summary>
        /// These enumerations will determine the current state of the Arcanian
        /// </summary>
        protected enum ArcanianState
        {
            ArcanianMoving = 0,
            ArcanianResting = 1,
            ArcanianFalling = 2,
            ArcanianDying = 3,
            ArcanianSpawning = 4,
            ArcanianFiring = 5,
            ArcanianInMenu = 6,
            ArcanianJumping = 7
        }

        /// <summary>
        /// This constructor will initialize the Arcanian to a position and a radius.
        /// </summary>
        /// <param name="position">Initial position of the Arcanian</param>
        protected Arcanian(Vector2 position, PlayerIndex thePlayerIndex)
            : base(position, kRadius)
        {
            mPlayerIndex = thePlayerIndex;
            Initialize();
            myInput = new _Utility.InputState();
        }

        public virtual PlayerIndex getPlayerIndex()
        {
            return mPlayerIndex;
        }

        private void Initialize()
        {
            // Technical Points;
            mTechnicalPoints = kMaxTechnicalPoints;
            mRechargeTimer = 0;
            mRechargeTP = false;
            mWalkTimer = 0;
            mFiringDelay = 0;
            //mSkill1Available = true;
            //mSkill2Available = true;

            // Initialize name
            mName = "Arcanian";

            // Initialize Arcanian
            mCurrentState = ArcanianState.ArcanianInMenu;
            Velocity = kVelocity;
            mHealth = kHealth;
            mFacingLeft = false;
            mIsBlinking = false;
            mBlinkTimer = 0;
            mNumBlink = 0;
            mPlayShieldRegenCue = false;

            // Initialize aimer
            mAimerBar = new XNACS1Rectangle(Center, kAimerBarWidth, kAimerBarHeight);
            mAimerBar.Color = Color.Transparent;
            mAimerBar.RotateAngle = kAimerAngle;
            mAimer = new XNACS1Circle(Center, kAimerRadius);
            mAimer.Texture = "Arcanian/aimer";
            mAimer.RotateAngle = kAimerAngle;

            // Initialize bound
            mBoundMin = new XNACS1Rectangle(Center, kBoundWidth, kBoundHeight);
            mBoundMax = new XNACS1Rectangle(Center, kBoundWidth, kBoundHeight);
            mBoundMin.Texture = texBound;
            mBoundMax.Texture = texBound;
            mBoundMin.RemoveFromAutoDrawSet();
            mBoundMax.RemoveFromAutoDrawSet();

            // Initialize shield
            mShield = kShield;
            mShieldArt = new XNACS1Circle(Center, kShieldRadius);

            // Hide arcanian at menu/selection screen
            HideArcanian();

            // Temporary til we have a minimap view
            mMiniMapXPosition = World.World.WorldMax.X / 2;
        }

        // Do we use this function to free up memory?
        protected void Unload()
        {
            this.RemoveFromAutoDrawSet();
            mAimerBar.RemoveFromAutoDrawSet();
            mAimer.RemoveFromAutoDrawSet();
            mShieldArt.RemoveFromAutoDrawSet();
        }

        /// <summary>
        /// Update Arcanian state.
        /// </summary>
        /// <param name="playerController">Current player controller</param>
        /// <param name="playerLives">Player lives</param>
        public virtual void Update(GamePadState playerController, ref int playerLives)
        {
            myInput.Update();
            #region Update current state
            switch (mCurrentState)
            {
                case ArcanianState.ArcanianResting:
                    ArcanianRestingState(playerController);
                    break;

                case ArcanianState.ArcanianMoving:
                    ArcanianMovingState(playerController);
                    break;

                case ArcanianState.ArcanianFalling:
                    ArcanianFallingState(playerController);
                    break;

                case ArcanianState.ArcanianDying:
                    ArcanianDyingState(ref playerLives);
                    break;

                case ArcanianState.ArcanianSpawning:
                    ArcanianSpawningState(playerController, playerLives);
                    break;

                case ArcanianState.ArcanianFiring:
                    ArcanianFiringState(playerController);
                    break;

                case ArcanianState.ArcanianInMenu:
                    ArcanianInMenuState();
                    break;

                case ArcanianState.ArcanianJumping:
                    ArcanianJumpingState(playerController);
                    break;
            }
            #endregion

            #region Update aimer, facing, shield, blink
            UpdateAimer(playerController);
            UpdateFaceDirection(playerController);
            UpdateShieldArt();
            UpdateBlink();
            if(mCurrentState != ArcanianState.ArcanianMoving)
            {
                UpdateTPRegen();
            }
            #endregion

            #region Echo top status
            //World.EchoToTopStatus(mCurrentState.ToString() +
            //                         " CvY: " + VelocityY.ToString() +
            //                         " PvY: " + prevVelocityY.ToString());
            //World.EchoToTopStatus(mCurrentState.ToString() +
            //                      " FaceLeft: " + mFacingLeft.ToString());
            #endregion
        }

        /// <summary>
        /// Arcanian resting state.
        /// </summary>
        protected void ArcanianRestingState(GamePadState playerController)
        {
            if (mHealth <= 0)                           // Check if arcanian is dying
            {
                mCurrentState = ArcanianState.ArcanianDying;
            }
            else if (playerController.Buttons.X == ButtonState.Pressed && mTechnicalPoints >= kJumpTP) // Check if arcanian is jumping
            {
                mTechnicalPoints -= kJumpTP;
                VelocityY = kJumpStrength;
                mCurrentState = ArcanianState.ArcanianJumping;
            }
            else if (VelocityY != prevVelocityY)        // Checks if player is falling
            {
                mCurrentState = ArcanianState.ArcanianFalling;
            }
            else
            {
                ShouldTravel = false;
                VelocityY = 0;

                // Arcanian actions
                if (mTechnicalPoints > 0 && mFiringDelay <= 0)  // Check if Arcanian have TP
                {
                    float input = playerController.ThumbSticks.Left.X;
                    if (input < 0.5 && input > -0.5)
                    {
                        input = 0;
                    }

                    // Checks if player is using a skill
                    if (myInput.IsKeyCDown(PlayerIndex.One) ||
                        playerController.Buttons.A == ButtonState.Pressed)
                    {
                        //mCurSkill = mSkillSet.GetSkill1();
                        if (mTechnicalPoints >= mSkillSet.GetSkill1().GetTechnicalPointsRequired())
                        {
                            mCurSkill = mSkillSet.GetSkill1();
                            mPower = kMinPower;
                            mCurrentState = ArcanianState.ArcanianFiring;
                            mRechargeTimer = 0;
                            mRechargeTP = false;
                        }
                    }
                    else if (playerController.Buttons.B == ButtonState.Pressed)
                    {
                        //mCurSkill = mSkillSet.GetSkill2();
                        if (mTechnicalPoints >= mSkillSet.GetSkill2().GetTechnicalPointsRequired())
                        {
                            mCurSkill = mSkillSet.GetSkill2();
                            mPower = kMinPower;
                            mCurrentState = ArcanianState.ArcanianFiring;
                            mRechargeTimer = 0;
                            mRechargeTP = false;
                        }
                    }
                    else if (playerController.Buttons.Y == ButtonState.Pressed && mUltimateBar == kMaxUltimate)
                    {
                        mCurSkill = mSkillSet.GetUltimateSkill();
                        //mCurSkill = MakeSkill(playerController);
                        mPower = kMinPower;
                        mCurrentState = ArcanianState.ArcanianFiring;
                        mRechargeTimer = 0;
                        mRechargeTP = false;
                    }
                    else if (input != 0)        // Checks if player is trying to move
                    {
                        mRechargeTimer = 0;
                        mCurrentState = ArcanianState.ArcanianMoving;
                        //mRechargeTP = false;
                    }
                }
                else
                {
                    mFiringDelay--;
                }
            }
            UpdateRechargeTimer();
        }

        /// <summary>
        /// Arcanian moving state.
        /// </summary>
        protected void ArcanianMovingState(GamePadState playerController)
        {
            float input = playerController.ThumbSticks.Left.X;
            
            if (input < 0.5 && input > -0.5)
            {
                input = 0;
            }

            if (mHealth <= 0)                       // Change to dying state when health goes below zero
            {
                mCurrentState = ArcanianState.ArcanianDying;
            }
            else if (playerController.Buttons.X == ButtonState.Pressed && mTechnicalPoints >= kJumpTP) // Check if arcanian is jumping
            {
                mTechnicalPoints -= kJumpTP;
                VelocityY = kJumpStrength;
                mCurrentState = ArcanianState.ArcanianJumping;            
            }
            else if (input == 0 || mTechnicalPoints <= 0)    // Change to resting state when stop moving
            {
                mCurrentState = ArcanianState.ArcanianResting;
            }
            //else if (VelocityY != prevVelocityY)    // Determine if Arcanian is falling
            //{
            //    mCurrentState = ArcanianState.ArcanianFalling;
            //}
            else                                    // Keep moving!
            {
                ArcanianMove(input);
                ShouldTravel = true;
                prevVelocityY = VelocityY;
                VelocityY = Gravity.CalcVeloCurrentState(VelocityY);
            }
        }

        /// <summary>
        /// Arcanian falling state.
        /// </summary>
        protected void ArcanianFallingState(GamePadState playerController)
        {
            if (mHealth <= 0 || CenterY < World.World.WorldMin.Y - kRadius * 3) // Health reaches zero or falls below world bottom 
            {
                mCurrentState = ArcanianState.ArcanianDying;
            }
            else if (VelocityY == prevVelocityY || VelocityY == float.Epsilon || VelocityY == 0.0)        // Reaches solid, transit to resting state
            {
                VelocityY = 0;
                prevVelocityY = 0;
                mCurrentState = ArcanianState.ArcanianResting;
            }
            else
            {
                ShouldTravel = true;
                prevVelocityY = VelocityY;
                ArcanianFall(playerController);
                VelocityX = 0;
            }
        }

        /// <summary>
        /// Arcanian dying state.
        /// </summary>
        protected void ArcanianDyingState(ref int lives)
        {
            if (CenterY >= World.World.WorldMax.Y + 3f)
            {
                SetSpawningTexture();
                HideArcanian();
                lives--;
                mCurrentState = ArcanianState.ArcanianSpawning;
            }
            else
            {
                SetDyingTexture();
                mAimer.RemoveFromAutoDrawSet();
                mShieldArt.RemoveFromAutoDrawSet();
                ShouldTravel = false;
                CenterY += 2.0f;
                VelocityX = 0;
            }
        }

        protected virtual void SetDyingTexture() { }
        protected virtual void SetSpawningTexture() { }

        /// <summary>
        /// Arcanian firing state
        /// </summary>
        protected void ArcanianFiringState(GamePadState playerController)
        {
            // Health below zero, transit to dying state
            if (mHealth <= 0)
            {
                mPower = 0;
                mCurrentState = ArcanianState.ArcanianDying;
            }
            else if (VelocityY != prevVelocityY)    // Determine if Arcanian is falling
            {
                mPower = 0;
                mCurrentState = ArcanianState.ArcanianFalling;
            }
            else if (ReleaseFire(playerController) || mPower >= kMaxPower) // or when max power reached
            {
                mTechnicalPoints -= mCurSkill.GetTechnicalPointsRequired();
                mSkillSet.useSkill(mCurSkill, mPower, mAimer.RotateAngle, mAimer.Center);
                //mCurSkill.FireSkill(mPower, mAimer.RotateAngle, mAimer.Center);
                mPower = 0;
                /*
                if (mCurSkill == mSkillSet.GetSkill1() || mTechnicalPoints < mSkillSet.GetSkill1().GetTechnicalPointsRequired())
                {
                    mSkill1Available = false;
                }
                if (mCurSkill == mSkillSet.GetSkill2() || mTechnicalPoints < mSkillSet.GetSkill2().GetTechnicalPointsRequired())
                {
                    mSkill2Available = false;
                }
                 */
                if (mCurSkill == mSkillSet.GetUltimateSkill())
                {
                    mUltimateBar = 0;
                }
                mAimer.Radius = kAimerRadius;
                mFiringDelay = G.UPDATE_RATE;
                mCurrentState = ArcanianState.ArcanianResting;
            }
            else
            {
                mPower++;
                UpdateAimerSize();
            }
        }

        protected bool ReleaseFire(GamePadState playerController)
        {
            if (G.PC_DEBUG_MODE)
            {
                return (myInput.IsKeyCUp(null));
            }
            else
            {
                return (playerController.IsButtonUp(Buttons.A) &&      // Shoot when buttons are released
                         playerController.IsButtonUp(Buttons.B) &&
                         playerController.IsButtonUp(Buttons.Y));
            }
        }

        /// <summary>
        /// Arcanian spawning state
        /// </summary>
        protected void ArcanianSpawningState(GamePadState playerController, int lives)
        {
            // Spawn at location indicated by spawn arrow
            if ((myInput.IsKeyCDown(null)|| playerController.Buttons.A == ButtonState.Pressed || mSpawnTimer == 10 * G.UPDATE_RATE) && lives > 0)
            {
                ShowArcanian();
                SetSpawningTexture();
                mHealth = kHealth;
                RestoreShield();
                mTechnicalPoints = kMaxTechnicalPoints;
                //mSkill1Available = true;
                //mSkill2Available = true;
                mUltimateBar = 0;
                Velocity = kVelocity;
                mSpawnTimer = 0;
                mCurrentState = ArcanianState.ArcanianFalling;
            }
            else if (lives > 0)
            {
                mSpawnTimer++;
                CenterX += playerController.ThumbSticks.Left.X * 2;
            }
            else
            {
                CenterX = 0;
            }
        }

        /// <summary>
        /// Arcanian menu state
        /// </summary>
        protected void ArcanianInMenuState()
        {
            HideArcanian();
        }

        /// <summary>
        /// Arcanian jumping state
        /// </summary>
        protected void ArcanianJumpingState(GamePadState playerController)
        {
            if (mHealth <= 0 || CenterY < World.World.WorldMin.Y - kRadius * 3)   // Check if arcanian is dying
            {
                mCurrentState = ArcanianState.ArcanianDying;
            }
            else
            {
                ShouldTravel = true;
                VelocityY = Gravity.CalcVeloCurrentState(VelocityY);
                CenterY += VelocityY;
                VelocityX = playerController.ThumbSticks.Left.X / 2;
                CenterX += VelocityX;
            }
        }

        #region Update functions
        protected void UpdateAimer(GamePadState playerController, float minAimerAngle, float maxAimerAngle)
        {
            float input = playerController.ThumbSticks.Right.Y;

            if (input != 0)
            {
                mBoundMin.Center = Center;
                mBoundMax.Center = Center;
                if (mFacingLeft)
                {
                    mBoundMin.RotateAngle = 180 - minAimerAngle;
                    mBoundMax.RotateAngle = 180 - maxAimerAngle;
                }
                else
                {
                    mBoundMin.RotateAngle = minAimerAngle;
                    mBoundMax.RotateAngle = maxAimerAngle;
                }
                mBoundMin.AddToAutoDrawSet();
                mBoundMax.AddToAutoDrawSet();
            }
            else
            {
                mBoundMin.RemoveFromAutoDrawSet();
                mBoundMax.RemoveFromAutoDrawSet();
            }

            // Rotate aimer using y-axis of right thumbstick, max and min angle bounded
            if (mFacingLeft)    // Arcanian facing left
            {
                mAimerBar.RotateAngle -= input * kAimerSpeed;
                mAimer.RotateAngle -= input * kAimerSpeed;

                if (input > 0 && 180 - mAimer.RotateAngle > maxAimerAngle)
                {
                    mAimerBar.RotateAngle = 180 - maxAimerAngle;
                    mAimer.RotateAngle = 180 - maxAimerAngle;
                }
                if (input < 0 && 180 - mAimer.RotateAngle < minAimerAngle)
                {
                    mAimerBar.RotateAngle = 180 - minAimerAngle;
                    mAimer.RotateAngle = 180 - minAimerAngle;
                }
            }
            else                // Arcanian facing right
            {
                mAimerBar.RotateAngle += input * kAimerSpeed;
                mAimer.RotateAngle += input * kAimerSpeed;
                if (input > 0 && mAimer.RotateAngle > maxAimerAngle)
                {
                    mAimerBar.RotateAngle = maxAimerAngle;
                    mAimer.RotateAngle = maxAimerAngle;
                }
                if (input < 0 && mAimer.RotateAngle < minAimerAngle)
                {
                    mAimerBar.RotateAngle = minAimerAngle;
                    mAimer.RotateAngle = minAimerAngle;
                }
            }

            // Update aimer center
            mAimerBar.Center = Center;
            mAimer.Center = mAimerBar.Center + mAimerBar.FrontDirection * (mAimerBar.Width / 2f + 20f * ((float)mPower / kMaxPower));
        }

        protected void UpdateFaceDirection(GamePadState playerController)
        {
            float input = playerController.ThumbSticks.Left.X + playerController.ThumbSticks.Right.X;
            if (input < 0.5 && input > -0.5)
            {
                input = 0;
            }
            if (input < 0)
                mFacingLeft = true;
            if (input > 0)
                mFacingLeft = false;
        }

        protected void UpdateShieldArt()
        {
            mShieldArt.Center = Center;
            if (mShield == 0)
            {
                mShieldArt.RemoveFromAutoDrawSet();
            }
            else
            {
                int col = mShield / 10 - 1;
                if (col < 0)
                    col = 0;
                if (col >= kShieldArtCol)
                    col = kShieldArtCol - 1;
                mShieldArt.SetTextureSpriteAnimationFrames(col, 0, col, 0, 0, SpriteSheetAnimationMode.AnimateForward);
                mShieldArt.UseSpriteSheetAnimation = true;
            }
        }

        protected void UpdateBlink()
        {
            if (mIsBlinking)
            {
                if (mNumBlink < kMaxNumBlink)
                {
                    if (mBlinkTimer == kBlinkRate)
                    {
                        if (Visible == true)
                            Visible = false;
                        else
                            Visible = true;
                        mBlinkTimer = 0;
                    }
                    else
                    {
                        mBlinkTimer++;
                    }
                    mNumBlink++;
                }
                else
                {
                    mIsBlinking = false;
                    Visible = true;
                    mNumBlink = 0;
                }
            }
        }

        protected void UpdateTPRegen()
        {
            mTPRegenTimer++;
            if (mTPRegenTimer >= (G.UPDATE_RATE * (1f/10f)))
            {
                mTPRegenTimer = 0;
                if (mTechnicalPoints < kMaxTechnicalPoints)
                {
                    mTechnicalPoints++;
                }
            }
        }

        protected void UpdateRechargeTimer()
        {
            mRechargeTimer++;

            // Recharge TP and Skills
            if (mRechargeTimer == kRequiredRechargeTime * G.UPDATE_RATE)
            {
                mRechargeTP = true;
                mRechargeTimer = 0;
            }

            if (mRechargeTP)
            {
                if (mTechnicalPoints >= kMaxTechnicalPoints && mShield < kShield)
                {
                    if (!mPlayShieldRegenCue)
                    {
                        World.PlayACue("shieldrecover");
                        mPlayShieldRegenCue = true;
                    }
                    RestoreShield();
                }
                /*
                if (mTechnicalPoints >= mSkillSet.GetSkill1().GetTechnicalPointsRequired())
                {
                    mSkill1Available = true;
                }
                if (mTechnicalPoints >= mSkillSet.GetSkill2().GetTechnicalPointsRequired())
                {
                    mSkill2Available = true;
                }
                 */
                if (mTechnicalPoints >= kMaxTechnicalPoints)
                {
                    mRechargeTP = false;
                    mRechargeTimer = 0;
                }
                else
                {
                    if ((mTechnicalPoints + 2) < kMaxTechnicalPoints)
                    {
                        mTechnicalPoints += 2;
                    }
                    else
                    {
                        mTechnicalPoints++;
                    }
                }
            }
        }
        #endregion

        #region Aimer functions
        protected virtual void UpdateAimer(GamePadState playerController)
        {
            // Rotate aimer using y-axis of right thumbstick
            if (mFacingLeft)    // Arcanian facing left
            {
                mAimerBar.RotateAngle -= playerController.ThumbSticks.Right.Y * kAimerSpeed;
                mAimer.RotateAngle -= playerController.ThumbSticks.Right.Y * kAimerSpeed;
            }
            else                // Arcanian facing right
            {
                mAimerBar.RotateAngle += playerController.ThumbSticks.Right.Y * kAimerSpeed;
                mAimer.RotateAngle += playerController.ThumbSticks.Right.Y * kAimerSpeed;
            }

            // Update aimer center
            mAimerBar.Center = Center;
            mAimer.Center = mAimerBar.Center + mAimerBar.FrontDirection * (mAimerBar.Width / 2f + 10f * ((float)mPower/kMaxPower));
        }

        protected void UpdateAimerSize()
        {
            mAimer.Radius = mAimer.Radius + 0.1f;
        }

        protected void FlipAimerAngle()
        {
            float prevAngle = mAimerBar.RotateAngle;
            mAimerBar.RotateAngle += 180 - prevAngle * 2;
            mAimer.RotateAngle += 180 - prevAngle * 2;
        }
        #endregion

        #region Elemental specific functions
        protected virtual Skill MakeSkill(GamePadState playerController)
        {
            return null; 
        }

        protected virtual void ArcanianMove(float input)
        {
            VelocityX = input * kSpeed;
            CenterX += VelocityX;
        }

        protected virtual void ArcanianFall(GamePadState playerController)
        {
            VelocityY = Gravity.CalcVeloCurrentState(VelocityY);
        }

        protected virtual void RestoreShield()
        {
            mShield = kShield;
            mShieldArt.AddToAutoDrawSet();
        }

        protected virtual void CalcShieldDamage(float dmg)
        {
            mShield -= (int)dmg;
            if (mShield < 0)
                mShield = 0;
        }

        protected virtual void PlayHitSound()
        {
            World.PlayACue("electric");
        }
        #endregion

        #region Hide / Show functions
        public virtual void HideArcanian()
        {
            RemoveFromAutoDrawSet();
            mAimerBar.RemoveFromAutoDrawSet();
            mAimer.RemoveFromAutoDrawSet();
            mShieldArt.RemoveFromAutoDrawSet();
        }

        public virtual void ShowArcanian()
        {
            AddToAutoDrawSet();
            mAimerBar.AddToAutoDrawSet();
            mAimer.AddToAutoDrawSet();
            mShieldArt.AddToAutoDrawSet();
        }

        public virtual void ShowArcanianOnly()
        {
            AddToAutoDrawSet();
        }

        public virtual void ShowBound()
        {
            mBoundMax.Center = Center;
            mBoundMin.Center = Center;
            mBoundMax.AddToAutoDrawSet();
            mBoundMin.AddToAutoDrawSet();
        }

        public virtual void RemoveBound()
        {
            mBoundMax.RemoveFromAutoDrawSet();
            mBoundMin.RemoveFromAutoDrawSet();
        }
        #endregion

        #region Get functions
        public string getName()
        {
            return mName;
        }

        public String getSkillName(int slotNumber)
        {
            return mSkillSet.GetSkillName(slotNumber);
        }

        public float getRadius()
        {
            return Radius;
        }

        public int getMaxHealth()
        {
            return kHealth;
        }

        public int getHealth()
        {
            return mHealth;
        }

        public int getMaxTP()
        {
            return kMaxTechnicalPoints;
        }

        public int getTP()
        {
            return mTechnicalPoints;
        }

        public float getMaxPower()
        {
            return kMaxPower;
        }

        public float getPower()
        {
            return mPower;
        }

        public int getSkill1Status()
        {
            return mSkillSet.GetSkill1().GetTechnicalPointsRequired();
        }

        public int getSkill2Status()
        {
            return mSkillSet.GetSkill2().GetTechnicalPointsRequired();
        }

        public int getUltimateStatus()
        {
            return mUltimateBar;
        }

        public int getMaxUltimate()
        {
            return kMaxUltimate;
        }

        public float getAngle()
        {
            if (mFacingLeft)
            {
                return 180 - mAimer.RotateAngle;
            }
            else
            {
                return mAimer.RotateAngle;
            }
        }

        public int getShield()
        {
            return mShield;
        }

        public int getSpawnTime()
        {
            return mSpawnTimer;
        }
        #endregion

        #region Public functions
        public void EnterFallingState()
        {
            mCurrentState = ArcanianState.ArcanianFalling;
        }

        public void TransitToSpawningState()
        {
            ShowArcanian();
            mCurrentState = ArcanianState.ArcanianSpawning;
            mHealth = kHealth;
        }

        public void TakeDamage(float dmg)
        {
            if (mShield > 0)
            {
                SetVirbration(10, 0.5f, 0.5f);
                World.PlayACue("shieldhit");
                CalcShieldDamage(dmg);
                mPlayShieldRegenCue = false;
            }
            else
            {
                SetVirbration(20);
                PlayHitSound();
                mIsBlinking = true;
                mShieldArt.RemoveFromAutoDrawSet();
                mHealth -= (int)dmg;
            }

            mUltimateBar += (int)dmg;
            if (mUltimateBar > kMaxUltimate)
            {
                mUltimateBar = kMaxUltimate;
            }
        }

        public void SetVirbration(int time)
        {
            World.SetVirbration(mPlayerIndex, time);
        }

        public void SetVirbration(int time, float L, float R)
        {
            World.SetVirbration(mPlayerIndex, time, L, R);
        }

        public override bool IsOnGround(SolidRectangle incRect)
        {
            float distanceBetween = CenterY - incRect.CenterY;
            if (Radius + incRect.Height / 2 <= Math.Abs(distanceBetween) + ERROR_RATE && Radius + incRect.Height / 2 >= Math.Abs(distanceBetween) - ERROR_RATE)
            {
                mCurrentState = ArcanianState.ArcanianResting;
                return true;
            }
            else
                return false;
        }

        public bool IsDyingOrSpawning()
        {
            if (mCurrentState == ArcanianState.ArcanianDying || mCurrentState == ArcanianState.ArcanianSpawning)
                return true;
            else
                return false;
        }
        #endregion
    }
}
