﻿using System;
using Microsoft.Xna.Framework.Graphics;

namespace EOLib.Graphics
{
	interface IGraphicsLoader : IDisposable
	{
		/// <summary>
		/// Returns a byte array of image data from a single image within an endless online *.egf file
		/// Image is specified by the library file (GFXTypes) and the resourceName (number)
		/// </summary>
		/// <param name="resourceVal">Name (number) of the image resource</param>
		/// <param name="file">File type to load from</param>
		/// <param name="transparent">Whether or not to make the background black color transparent</param>
		/// <param name="reloadFromFile">True to force reload the gfx from the gfx file, false to use the in-memory cache</param>
		/// <returns>Texture2D containing the image from the *.egf file</returns>
		Texture2D TextureFromResource(GFXTypes file, int resourceVal, bool transparent = false, bool reloadFromFile = false);
	}
}
