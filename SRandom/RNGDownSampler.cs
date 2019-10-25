using System;

namespace SuperRandom {

    /// <summary>
    ///     Adapts ulong rng down to smaller types using bit-shifting to reduce calls to the source RNG.
    /// </summary>
    internal class RNGDownSampler<T> : RNG<T> {
        private static readonly int sourceSize = sizeof(ulong);

        private readonly Converter<ulong, T> convert;
        private readonly int targetSize;
        private readonly RNG<ulong> source;

        private ulong sample;
        private int shiftAmount = 0;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RNGDownSampler"/> class.
        /// </summary>
        /// <param name="convert"> Converter to change from source to target type. </param>
        /// <param name="typeSize"> Size in bytes of target type. </param>
        /// <param name="source"> Underlying rng to pull from. </param>
        public RNGDownSampler(Converter<ulong, T> convert, int typeSize, RNG<ulong> source) {
            if (0 != sourceSize % typeSize) {
                throw new ArgumentOutOfRangeException("target size must be a factor of source size");
            }

            this.convert = convert;
            this.source = source;
            targetSize = typeSize;
        }

        /// <summary>
        ///     Returns a random value.
        /// </summary>
        public T Next() {
            if (shiftAmount == 0) {
                shiftAmount = sourceSize;
                sample = source.Next();
            }

            shiftAmount -= targetSize;

            return convert(sample >> shiftAmount*8);
        }

    }
}
