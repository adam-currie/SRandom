using System;

namespace SuperRandom {
    internal class RNGDownSampler<T> : RNG<T> {
        private static readonly int sourceSize = sizeof(ulong);

        private readonly Converter<ulong, T> convert;
        private readonly int targetSize;
        private readonly RNGSource source;

        private ulong sample;
        private int shiftAmount = 0;

        public RNGDownSampler(Converter<ulong, T> convert, int typeSize, RNGSource source) {
            if (0 != sourceSize % typeSize) {
                throw new ArgumentOutOfRangeException("target size must be a factor of source size");
            }

            this.convert = convert;
            this.source = source;
            targetSize = typeSize;
        }

        public T Next() {
            if (shiftAmount == 0) {
                shiftAmount = sourceSize;
                sample = source.Next();
            }

            shiftAmount -= targetSize;

            return convert(sample << shiftAmount);
        }

    }
}
