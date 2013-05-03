/******************************************************************
* This class is a child class of Arcanian. It handles features
* specific to the Earth type arcanian character.
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
using Innovades_Namespace._Game._Skills._Earth;
using Innovades_Namespace._Game._GameUtility;
using XNACS1Lib;

namespace Innovades_Namespace._Game._Arcanian
{
    public class TheEarthArcanian : Arcanian
    {
        private float kMinAimerAngle = -90.0f;
        private float kMaxAimerAngle = 10.0f;
        private int kDoubleShield = kShield * 2;
        private bool mPassiveSkillEnabled = true;

        public TheEarthArcanian(Vector2 position, PlayerIndex thePlayerIndex)
            : base(position, thePlayerIndex)
        {
            // Initialize texture
            texArcanianRight = "Arcanian/earthMoleRight";
            texArcanianLeft = "Arcanian/earthMoleLeft";
            texDyingRight = "Arcanian/earthMoleDead_right";
            texDyingLeft = "Arcanian/earthMoleDead_left";
            texShield = "Arcanian/earthshieldsprite";
            Texture = texArcanianRight;

            // Initialize name
            mName = "Earth Arcanian";

            // Initialize shield
            mShieldArt.SetTextureSpriteSheet(texShield, 4, 1, 0);
            mShieldArt.UseSpriteSheet = true;

            #region skills borrowed from fire skills

            // Initialize skill set with fire skills
            mSkillSet = new SkillSet(new EarthMole(), new EarthMoleLauncher(), new EarthMoleLauncher(), null);

            // Add skills to global list of skills
            //G.ListOfSkills.Add(fireball);
            //G.ListOfSkills.Add(multipleFireBall);
            //G.ListOfSkills.Add(megaFireBall);
            #endregion
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
                newSkill = new EarthMole();
            }
            else if (playerController.Buttons.B == ButtonState.Pressed)
            {
                newSkill = new EarthMoleLauncher();
            }
            else if (playerController.Buttons.Y == ButtonState.Pressed)
            {
                newSkill = new EarthMoleProjectile();
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

        protected override void RestoreShield()
        {
            if (!mPassiveSkillEnabled)
            {
                base.RestoreShield();
            }
            else
            {
                if (mCurrentState == ArcanianState.ArcanianSpawning)
                {
                    mShield = kDoubleShield;
                }
                if (mShield < kDoubleShield)
                    mShield += kShieldRegenRate;
                mShieldArt.AddToAutoDrawSet();
            }
        }

        protected override void PlayHitSound()
        {
            World.PlayACue("earthArcHit");
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
