// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient.Rendering.Sprites
{
	public enum WeaponSpriteType
	{
		Standing = 1, //1/2
		WalkFrame1 = 3, //3/7
		WalkFrame2 = 4, //4/8
		WalkFrame3 = 5, //5/9
		WalkFrame4 = 6, //6/10
		SpellCast = 11, //11/12
		SwingFrame1 = 13, //13/15
		SwingFrame2 = 14, //14/16
		SwingFrame2Spec = 17, //17 - special frame so that it shows well in the hand
		//invalid for non-ranged weapons:
		Shooting = 18, //18/19 AND 21/22 have same gfx
	}
}