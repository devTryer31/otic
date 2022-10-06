using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Zipper
{
    class LigthComparator<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equalFunc;
        private readonly Func<T, int> _getHashFunc;

        public LigthComparator(Func<T, T, bool> equalFunc, Func<T, int> getHashFunc = null)
        {
            _equalFunc = equalFunc;
            _getHashFunc = getHashFunc;
        }

        public bool Equals(T? x, T? y)
            => _equalFunc(x, y);

        public int GetHashCode([DisallowNull] T obj)
            => _getHashFunc?.Invoke(obj) ?? obj.GetHashCode();
    }
}
