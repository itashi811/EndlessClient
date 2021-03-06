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
    public class BootsRenderer : BaseCharacterPropertyRenderer
    {
        private readonly ISpriteSheet _bootsSheet;

        public override bool CanRender => _bootsSheet.HasTexture && _renderProperties.BootsGraphic != 0;

        public BootsRenderer(ICharacterRenderProperties renderProperties,
                             ISpriteSheet bootsSheet)
            : base(renderProperties)
        {
            _bootsSheet = bootsSheet;
        }

        public override void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea)
        {
            var offsets = GetOffsets(parentCharacterDrawArea);
            var drawLoc = new Vector2(parentCharacterDrawArea.X + offsets.X,
                                      // Center the Y coordinate over the bottom half of the character sprite
                                      parentCharacterDrawArea.Y + offsets.Y);
            Render(spriteBatch, _bootsSheet, drawLoc);
        }

        private Vector2 GetOffsets(Rectangle parentCharacterDrawArea)
        {
            var resX = -(float)Math.Floor(Math.Abs((float)_bootsSheet.SourceRectangle.Width - parentCharacterDrawArea.Width) / 2);
            var resY = (int)Math.Floor(parentCharacterDrawArea.Height / 3f) * 2 - 1;

            if (_renderProperties.IsActing(CharacterActionState.Walking))
            {
                resY -= 2;// * factor;
            }
            else if (_renderProperties.AttackFrame == 2)
            {
                var isDownOrLeft = _renderProperties.IsFacing(EODirection.Down, EODirection.Left);
                var factor = isDownOrLeft ? -1 : 1;
                var extra = !isDownOrLeft ? 2*_renderProperties.Gender : 0;

                resX += 2 * factor;
                resY += 1 * factor - extra;
            }

            return new Vector2(resX, resY);
        }
    }
}
