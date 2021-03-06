﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class ShieldRenderer : BaseCharacterPropertyRenderer
    {
        private readonly ISpriteSheet _shieldSheet;
        private readonly bool _isShieldOnBack;

        public override bool CanRender => _shieldSheet.HasTexture && _renderProperties.ShieldGraphic != 0;

        public ShieldRenderer(ICharacterRenderProperties renderProperties,
                              ISpriteSheet shieldSheet,
                              bool isShieldOnBack)
            : base(renderProperties)
        {
            _shieldSheet = shieldSheet;
            _isShieldOnBack = isShieldOnBack;
        }

        public override void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea)
        {
            var offsets = GetOffsets(parentCharacterDrawArea);
            var drawLoc = new Vector2(parentCharacterDrawArea.X + offsets.X, parentCharacterDrawArea.Y + offsets.Y);
            Render(spriteBatch, _shieldSheet, drawLoc);
        }

        private Vector2 GetOffsets(Rectangle parentCharacterDrawArea)
        {
            float resX, resY;

            if (_isShieldOnBack)
            {
                resX = -(float)Math.Floor(Math.Abs((float)_shieldSheet.SourceRectangle.Width - parentCharacterDrawArea.Width) / 2);
                resY = -(float)Math.Floor(parentCharacterDrawArea.Height / 3f) - _renderProperties.Gender;

                if (_renderProperties.AttackFrame == 2)
                    resX += _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? 2 : -2;
            }
            else
            {
                resX = (float)Math.Floor(Math.Abs((float)_shieldSheet.SourceRectangle.Width - parentCharacterDrawArea.Width) / 2);
                resY = (float)Math.Floor(Math.Abs((float)_shieldSheet.SourceRectangle.Height - parentCharacterDrawArea.Height) / 2) + 5;

                if (_renderProperties.IsFacing(EODirection.Down, EODirection.Left))
                {
                    resX -= parentCharacterDrawArea.Width * 1.5f;

                    if (_renderProperties.AttackFrame == 2)
                        resX -= 2;

                    resX += 2;
                }
                else
                {
                    resX -= parentCharacterDrawArea.Width / 1.5f;

                    if (_renderProperties.AttackFrame == 2)
                        resX += 2;

                    resX -= 3;
                }

                resY -= _renderProperties.Gender;
                if (_renderProperties.IsActing(CharacterActionState.Walking))
                    resY -= 1;
            }

            return new Vector2(resX, resY);
        }
    }
}
