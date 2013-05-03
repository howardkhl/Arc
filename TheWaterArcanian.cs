/******************************************************************
* This class is a child class of Arcanian. It handles features
* specific to the Water type arcanian character.
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
using Innovades_Namespace._Game._Skills._Water;
using Innovades_Namespace._Game._GameUtility;
using XNACS1Lib;

namespace Innovades_Namespace._Game._Arcanian
{
    public class TheWaterArcanian : Arcanian
    {
        private float kMinAimerAngle = 0.0f;
        private float kMaxAimerAngle = 85.0f;
        private int kReqHPRegenTime = 4;
        private int mHPRegenTimer;
        private bool mPassiveSkillEnabled = true;

        public TheWaterArcanian(Vector2 position, PlayerIndex thePlayerIndex)
            : base(position, thePlayerIndex)
        {
            // Initialize texture
            texArcanianRight = "Arcanian/waterTurtleRight";
            texArcanianLeft = "Arcanian/waterTurtleLeft";
            texDyingRight = "Arcanian/waterTurtleDead_right";
            texDyingLeft = "Arcanian/waterTurtleDead_left";
            texShield = "Arcanian/watershieldsprite";
            Texture = texArcanianRight;

            // Initialize name
            mName = "Water Arcanian";

            // Initialize shield
            mShieldArt.SetTextureSpriteSheet(texShield, 4, 1, 0);
            mShieldArt.UseSpriteSheet = true;

            // Initliaze water skills
            SingleStream waterStream = new SingleStream();
            DoubleWaterStream doubleWaterStream = new DoubleWaterStream();
            UltimateWaterStream ultimateWaterStream = new UltimateWaterStream();

            // Initialize skill set with water skills
            mSkillSet = new SkillSet(waterStream, doubleWaterStream, ultimateWaterStream, null);

            // Add skills to global list of skills
            //G.ListOfSkills.Add(waterStream);
            //G.ListOfSkills.Add(doubleWaterStream);
            //G.ListOfSkills.Add(ultimateWaterStream);

            // Initialize HP Regen
            mHPRegenTimer = 0;
        }

        public override void Update(GamePadState playerController, ref int playerLives)
        {
            UpdateTexture(playerController);
            UpdateAimer(playerController, kMinAimerAngle, kMaxAimerAngle);
            base.Update(playerController, ref playerLives);
            if (mPassiveSkillEnabled)
                RegenerateHealth();
        }

        private void UpdateTexture(GamePadState playerController)
        {
            float input = playerController.ThumbSticks.Left.X + playerController.ThumbSticks.Right.X;
            if (input < 0.5 && input > -0.5)
            {
                input = 0;
            }
            if (input > 0)
            {
                if (mFacingLeft)
                {
                    Texture = texArcanianRight;
                    FlipAimerAngle();
                }
            }
            if (input < 0)
            {
                if (!mFacingLeft)
                {
                    Texture = texArcanianLeft;
                    FlipAimerAngle();
                }
            }
        }

        protected override Skill MakeSkill(GamePadState playerController)
        {
            Skill newSkill = new Skill();

            if (playerController.Buttons.A == ButtonState.Pressed)
            {
                newSkill = new SingleStream();
            }
            else if (playerController.Buttons.B == ButtonState.Pressed)
            {
                newSkill = new DoubleWaterStream();
            }
            else if (playerController.Buttons.Y == ButtonState.Pressed)
            {
                newSkill = new UltimateWaterStream();
            }

            return newSkill;
        }

        protected override void SetDyingTexture()
        {
            if (mFacingLeft)
            {
                Texture = texDyingLeft;
            }
            else
            {
                Texture = texDyingRight;
            }
        }

        protected override void SetSpawningTexture()
        {
            if (mFacingLeft)
            {
                Texture = texArcanianLeft;
            }
            else
            {
                Texture = texArcanianRight;
            }
        }

        private void RegenerateHealth()
        {
            if (mHealth < kHealth)
            {
                if (mHPRegenTimer == kReqHPRegenTime * G.UPDATE_RATE)
                {
                    mHealth++;
                    mHPRegenTimer = 0;
                }
                mHPRegenTimer++;
            }
        }

        protected override void PlayHitSound()
        {
            World.PlayACue("waterArcHit");
        }

        public override void ShowArcanianOnly()
        {
            base.ShowArcanianOnly();
        }

        public override void HideArcanian()
        {
            base.HideArcanian();
        }

        public override void ShowArcanian()
        {
            base.ShowArcanian();
        }

        public override void ShowBound()
        {
            mBoundMin.RotateAngle = kMinAimerAngle;
            mBoundMax.RotateAngle = kMaxAimerAngle;
            base.ShowBound();
        }

        public override void RemoveBound()
        {
            base.RemoveBound();
        }
    }
}
