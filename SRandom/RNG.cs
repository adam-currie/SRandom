namespace SuperRandom {
    internal interface RNG<T> {

        /// <summary>
        ///     Returns a random value.
        /// </summary>
        T Next();

    }
}
