﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class HairRenderLocationCalculator
    {
        private readonly ICharacterRenderProperties _renderProperties;

        public HairRenderLocationCalculator(ICharacterRenderProperties renderProperties)
        {
            _renderProperties = renderProperties;
        }

        public Vector2 CalculateDrawLocationOfCharacterHair(Rectangle hairRectangle, Rectangle parentCharacterDrawArea)
        {
            var resX = -(float)Math.Floor(Math.Abs((float)hairRectangle.Width - parentCharacterDrawArea.Width) / 2) - 1;
            var resY = -(float)Math.Floor(Math.Abs(hairRectangle.Height - (parentCharacterDrawArea.Height / 2f)) / 2) - _renderProperties.Gender;

            if (_renderProperties.AttackFrame == 2)
            {
                resX += _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? 4 : -4;
                resX += _renderProperties.IsFacing(EODirection.Up)
                    ? _renderProperties.Gender * -2
                    : _renderProperties.IsFacing(EODirection.Left)
                        ? _renderProperties.Gender * 2
                        : 0;

                resY += _renderProperties.IsFacing(EODirection.Up, EODirection.Left) ? 1 : 5;
                resY -= _renderProperties.IsFacing(EODirection.Right, EODirection.Down) ? _renderProperties.Gender : 0;
            }

            var flippedOffset = _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? 2 : 0;
            return parentCharacterDrawArea.Location.ToVector2() + new Vector2(resX + flippedOffset, resY);
        }
    }
}
