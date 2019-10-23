using System;
using System.Collections.Generic;
using System.Text;

namespace SuperRandom {
    internal interface RNG<T> {

        /// <summary>
        ///     Returns a random value.
        /// </summary>
        T Next();

    }
}
