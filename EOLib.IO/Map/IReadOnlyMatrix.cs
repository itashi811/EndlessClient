// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;

namespace EOLib.IO.Map
{
    public interface IReadOnlyMatrix<T> : IEnumerable<IList<T>>
    {
        int Rows { get; }

        int Cols { get; }

        T[] GetRow(int rowIndex);

        T this[int row, int col] { get; }
    }
}