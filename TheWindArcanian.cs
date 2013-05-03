/******************************************************************
* This class is a child class of Arcanian. It handles features
* specific to the Wind type arcanian character.
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
using Innovades_Namespace._Game._Skills._Wind;
using Innovades_Namespace._Game._GameUtility;
using XNACS1Lib;

namespace Innovades_Namespace._Game._Arcanian
{
    public class TheWindArcanian : Arcanian
    {
        private float kSlowFallSpeed = 0.01f;
        private float kFastFallSpeed = 0.1f;
        private float kMinAimerAngle = -20.0f;
        private float kMaxAimerAngle = 90.0f;
        private bool mPassiveSkillEnabled = true;

        public TheWindArcanian(Vector2 position, PlayerIndex thePlayerIndex)
            : base(position, thePlayerIndex)
        {
            // Initialize texture
            texArcanianRight = "Arcanian/windBirdRight";
            texArcanianLeft = "Arcanian/windBirdLeft";
            texDyingRight = "Arcanian/windBirdDead_right";
            texDyingLeft = "Arcanian/windBirdDead_left";
            texShield = "Arcanian/windshieldsprite";
            Texture = texArcanianRight;

            // Initialize name
            mName = "Wind Arcanian";

            // Initialize shield
            mShieldArt.SetTextureSpriteSheet(texShield, 4, 1, 0);
            mShieldArt.UseSpriteSheet = true;

            // Initliaze wind skills
            WindBlade windBlade = new WindBlade();
            MultipleWindBlade multipleWindBlade = new MultipleWindBlade();
            MegaWindBlade megaWindBlade = new MegaWindBlade();

            // Initialize skill set with wind skills
            mSkillSet = new SkillSet(windBlade, multipleWindBlade, megaWindBlade, null);

            // Add skills to global list of skills
            //G.ListOfSkills.Add(windBlade);
            //G.ListOfSkills.Add(multipleWindBlade);
            //G.ListOfSkills.Add(megaWindBlade);
        }

        public override void Update(GamePadState playerController, ref int playerLives)
        {
            UpdateTexture(playerController);
            UpdateAimer(playerController, kMinAimerAngle, kMaxAimerAngle);
            base.Update(playerController, ref playerLives);
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
                newSkill = new WindBlade();
            }
            else if (playerController.Buttons.B == ButtonState.Pressed)
            {
                newSkill = new MultipleWindBlade();
            }
            else if (playerController.Buttons.Y == ButtonState.Pressed)
            {
                newSkill = new MegaWindBlade();
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

        protected override void ArcanianMove(float input)
        {
            if (!mPassiveSkillEnabled)
            {
                base.ArcanianMove(input);
            }
            else
            {
                VelocityX = input * kSpeed * 2;
                CenterX += VelocityX;           // Wind passive
            }
        }

        protected override void ArcanianFall(GamePadState playerController)
        {
            if (!mPassiveSkillEnabled)
            {
                base.ArcanianFall(playerController);
            }
            else
            {
                if (playerController.ThumbSticks.Left.Y < 0)
                {
                    VelocityY -= kFastFallSpeed;
                }
                else
                {
                    VelocityY -= kSlowFallSpeed;
                }
                VelocityX = playerController.ThumbSticks.Left.X;
                CenterX += VelocityX;  
            }
        }

        protected override void PlayHitSound()
        {
            World.PlayACue("windArcHit");
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
