﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient.Controllers.Repositories
{
    public interface ICharacterManagementControllerRepository
    {
        ICharacterManagementController CharacterManagementController { get; set; }
    }

    public class CharacterManagementRepository : ICharacterManagementControllerRepository
    {
        public ICharacterManagementController CharacterManagementController { get; set; }
    }
}
