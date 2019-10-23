using System;
using System.Collections.Generic;
using System.Text;

namespace SuperRandom {
    internal interface RNG<T> {

        /// <summary>
        ///     Returns a random value in the specified range.
        /// </summary>
        T Next();

        //todo: add range stuff here

    }
}
