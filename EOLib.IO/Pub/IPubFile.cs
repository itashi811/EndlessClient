﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.IO.Services;

namespace EOLib.IO.Pub
{
    public interface IPubFile<out TRecord> : IPubFile
        where TRecord : IPubRecord, new()
    {
        TRecord this[int id] { get; }

        IReadOnlyList<TRecord> Data { get; }
    }

    public interface IPubFile
    {
        string FileType { get; }

        int CheckSum { get; set; }

        int Length { get; }

        byte[] SerializeToByteArray(INumberEncoderService numberEncoderService);

        void DeserializeFromByteArray(byte[] bytes, INumberEncoderService numberEncoderService);
    }
}