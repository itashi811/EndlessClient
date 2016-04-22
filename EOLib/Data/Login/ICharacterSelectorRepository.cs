﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Data.BLL;

namespace EOLib.Data.Login
{
	public interface ICharacterSelectorRepository
	{
		IReadOnlyList<ICharacter> Characters { get; set; }
	}

	public interface ICharacterSelectorProvider
	{
		IReadOnlyList<ICharacter> Characters { get; }
	}

	public class CharacterSelectorRepository : ICharacterSelectorRepository, ICharacterSelectorProvider
	{
		public IReadOnlyList<ICharacter> Characters { get; set; }
	}
}
