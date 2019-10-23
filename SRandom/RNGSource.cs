using System;

namespace SuperRandom {

    /// <summary>
    ///     Random number generator implementing MT19937-64(Mersenne Twister).
    ///     https://en.wikipedia.org/wiki/Mersenne_Twister used for reference.
    /// </summary>
    internal class RNGSource : RNG<ulong> {
        private const int w = 64;
        private const ulong n = 312;
        private const ulong m = 156;
        private const ulong a = 0xB5026F5AA96619E9;
        private const int u = 29;
        private const ulong d = 0x5555555555555555;
        private const int s = 17;
        private const ulong b = 0x71D67FFFEDA60000;
        private const int t = 37;
        private const ulong c = 0xFFF7EEE000000000;
        private const int l = 43;
        private const ulong f = 6364136223846793005;
        private const ulong lower_mask = 0x7FFFFFFF;
        private const ulong upper_mask = ~lower_mask;

        private ulong index = n;
        private readonly ulong[] mt = new ulong[n];

        /// <summary>
        ///     Creates SRandom seeded with the specified seed.
        /// </summary>
        public RNGSource(ulong seed) {
            mt[0] = seed;
            for (ulong i = 1; i < n; ++i) {
                mt[i] = (f * (mt[i - 1] ^ (mt[i - 1] >> (w - 2))) + i);
            }
        }

        /// <summary>
        ///     Reset index to zero and twist generator variable.
        /// </summary>
        private void Twist() {
            for (ulong i = 0; i < n; ++i) {
                ulong x = (mt[i] & upper_mask) + (mt[(i + 1) % n] & lower_mask);
                ulong xA = x >> 1;

                if (x % 2 != 0) {
                    xA ^= a;
                }

                mt[i] = mt[(i + m) % n] ^ xA;
            }

            index = 0;
        }

        /// <summary>
        ///     Returns a random ULong.
        /// </summary>
        public ulong Next() {
            if (index >= n) {
                Twist();
            }

            ulong next = mt[index++];
            next ^= ((next >> u) & d);
            next ^= ((next << s) & b);
            next ^= ((next << t) & c);
            next ^= (next >> l);

            return next;
        }

        /// <summary>
        ///     Returns an unbiased random ULong in the specified range.
        /// </summary>
        /// <param name="max"> Exclusive maximum value. </param>
        /// <param name="min"> Inclusive minimum value. </param>
        /// <exception cref="ArgumentException">
        ///     Thrown when min >= max.
        /// </exception>
        public ulong Range(ulong min, ulong max) {
            if (min >= max) throw new ArgumentException("Max is not greater than min.");
            return Range(max - min) + min;
        }

        /// <summary>
        ///     Returns an unbiased random ULongin the specified range.
        /// </summary>
        /// <param name="max"> Exclusive maximum value. </param>
        public ulong Range(ulong max) {
            if (max == 0) throw new ArgumentException("Max cannot be 0.");
            ulong raw, rand;

            //if raw comes after N where N is the largest multiple of max that fits in ulong.MaxValue
            //then we need to try again, this should be rare
            //checking by comparing raw and rand in order to avoid calculating N with extra division 
            do {
                raw = Next();
                rand = raw % max;
            } while (raw - rand > ulong.MaxValue - max);

            return rand;
        }
    }
}
