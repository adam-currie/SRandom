using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SuperRandom {

    /// <summary>
    ///     Static multi-threading optimized random number generator implementing MT19937-64(Mersenne Twister).
    ///     https://en.wikipedia.org/wiki/Mersenne_Twister used for reference.
    /// </summary>
    public class SRandom {
        private const int w = 64;
        private const ulong n = 312;
        private const ulong m = 156;
        private const ulong r = 31;
        private const ulong a = 0xB5026F5AA96619E9;
        private const int u = 29;
        private const ulong d = 0x5555555555555555;
        private const int s = 17;
        private const ulong b = 0x71D67FFFEDA60000;
        private const int t = 37;
        private const ulong c = 0xFFF7EEE000000000;
        private const int l = 43;
        private const ulong f = 6364136223846793005;
        public const ulong lower_mask = 0x7FFFFFFF;
        public const ulong upper_mask = ~lower_mask;

        private ulong index = n;
        private ulong[] mt = new ulong[n];

        //RNG shared between threads to create seeds.
        private static readonly SRandom sharedRandom = new SRandom((ulong)DateTime.Now.Ticks);
        private static readonly object sharedRandomLock = new object();

        /// <summary>
        ///     Creates SRandom seeded with the specified seed.
        /// </summary>
        private SRandom(ulong seed) {
            mt[0] = seed;
            for (ulong i = 1; i < n; ++i) {
                mt[i] = (f * (mt[i - 1] ^ (mt[i - 1] >> (w - 2))) + i);
            }
        }

        //create thread local rngs using shared rng.
        private static ThreadLocal<SRandom> threadLocalRandom = new ThreadLocal<SRandom>(() => {
            List<ulong> entropySources = new List<ulong>();

            Stopwatch stopwatch = Stopwatch.StartNew();

            /*
             * use shared seed to create thread local seeds so that if other seeds are created close together,
             * they wont use the same time as their main source of entropy
             */
            lock (sharedRandomLock) {
                entropySources.Add(sharedRandom._Next());
            }

            //use process info as a source of entropy
            using (Process proc = Process.GetCurrentProcess()) {
                entropySources.Add((ulong)proc.PrivateMemorySize64);
                entropySources.Add((ulong)proc.Id);
            }

            //use time spent generating as a source of entropy
            stopwatch.Stop();
            entropySources.Add((ulong)stopwatch.ElapsedTicks);

            ulong seed = (ulong)DateTime.Now.Ticks;
            foreach (ulong source in entropySources) {
                //if source of entropy erroneously evaluating to zero was used, it would negate the usefullness of the others, so just ignore it
                if (source != 0) {
                    seed *= source;
                }
            }

            return new SRandom(seed);
        });

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
        private ulong _Next() {
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
        private ulong Range(ulong min, ulong max) {
            if (min >= max) throw new ArgumentException("Max is not greater than min.");
            return Range(max - min) + min;
        }

        /// <summary>
        ///     Returns an unbiased random ULongin the specified range.
        /// </summary>
        /// <param name="max"> Exclusive maximum value. </param>
        private ulong Range(ulong max) {
            if (max == 0) throw new ArgumentException("Max cannot be 0.");
            ulong raw, rand;

            //if raw comes after N where N is the largest multiple of max that fits in ulong.MaxValue
            //then we need to try again, this should be rare
            //checking by comparing raw and rand in order to avoid calculating N with extra division 
            do {
                raw = _Next();
                rand = raw % max;
            } while (raw - rand > ulong.MaxValue - max);

            return rand;
        }

        /// <summary>
        ///     Causes early initialization of thread local rng, otherwise initialized when first used.
        /// </summary>
        public static void Init() {
            //access the field to force initialization
            _ = threadLocalRandom.Value;
        }


        /// <summary>
        ///     Returns a random ulong.
        /// </summary>
        public static ulong Next() {
            return threadLocalRandom.Value._Next();
        }

        /// <summary>
        ///     Returns a random ULong in the specified range.
        /// </summary>
        /// <remarks>
        ///     Range mapped outputs are unbiased.
        /// </remarks>
        /// <param name="max"> exclusive maximum value. </param>
        /// <param name="min"> Inclusive minimum value. </param>
        /// <exception cref="ArgumentException">
        ///     Thrown when min >= max.
        /// </exception>
        public static ulong Next(ulong min, ulong max) {
            return threadLocalRandom.Value.Range(min, max);
        }

        /// <summary>
        ///     Returns a random ULong in the specified range.
        /// </summary>
        /// <remarks>
        ///     Range mapped outputs are unbiased.
        /// </remarks>
        /// <param name="max"> exclusive maximum value. </param>
        public static ulong Next(ulong max) {
            return threadLocalRandom.Value.Range(max);
        }

        /// <summary>
        ///     Returns a random long.
        /// </summary>
        public static long NextLong() => UncheckedConvert<long>(Next());

        /// <summary>
        ///     Returns a random uint.
        /// </summary>
        public static uint NextUInt() => UncheckedConvert<uint>(Next());

        /// <summary>
        ///     Returns a random int.
        /// </summary>
        public static int NextInt() => UncheckedConvert<int>(Next());

        /// <summary>
        ///     Returns a random ushort.
        /// </summary>
        public static ushort NextUShort() => UncheckedConvert<ushort>(Next());

        /// <summary>
        ///     Returns a random short.
        /// </summary>
        public static short NextShort() => UncheckedConvert<short>(Next());

        /// <summary>
        ///     Returns a random byte.
        /// </summary>
        public static byte NextByte() => UncheckedConvert<byte>(Next());

        /// <summary>
        ///     Returns a random sbyte.
        /// </summary>
        public static sbyte NextSByte() => UncheckedConvert<sbyte>(Next());

        /// <summary>
        ///     Fills the specified array with random values.
        /// </summary>
        /// <remarks>
        ///     This method provides better performance than generating values individually
        ///     because it pulls multiple random values from one ulong via bit-shifting.
        /// </remarks>
        /// <param name="buffer">The array to fill with random numbers.</param>
        public static void FillArray(byte[] buffer) => FillArray(buffer, sizeof(byte));

        /// <summary>
        ///     Fills the specified array with random values.
        /// </summary>
        /// <remarks>
        ///     This method provides better performance than generating values individually
        ///     because it pulls multiple random values from one ulong via bit-shifting.
        /// </remarks>
        /// <param name="buffer">The array to fill with random numbers.</param>
        public static void FillArray(sbyte[] buffer) => FillArray(buffer, sizeof(sbyte));

        /// <summary>
        ///     Fills the specified array with random values.
        /// </summary>
        /// <remarks>
        ///     This method provides better performance than generating values individually
        ///     because it pulls multiple random values from one ulong via bit-shifting.
        /// </remarks>
        /// <param name="buffer">The array to fill with random numbers.</param>
        public static void FillArray(ushort[] buffer) => FillArray(buffer, sizeof(ushort));

        /// <summary>
        ///     Fills the specified array with random values.
        /// </summary>
        /// <remarks>
        ///     This method provides better performance than generating values individually
        ///     because it pulls multiple random values from one ulong via bit-shifting.
        /// </remarks>
        /// <param name="buffer">The array to fill with random numbers.</param>
        public static void FillArray(short[] buffer) => FillArray(buffer, sizeof(short));

        /// <summary>
        ///     Fills the specified array with random values.
        /// </summary>
        /// <remarks>
        ///     This method provides better performance than generating values individually
        ///     because it pulls multiple random values from one ulong via bit-shifting.
        /// </remarks>
        /// <param name="buffer">The array to fill with random numbers.</param>
        public static void FillArray(uint[] buffer) => FillArray(buffer, sizeof(uint));

        /// <summary>
        ///     Fills the specified array with random values.
        /// </summary>
        /// <remarks>
        ///     This method provides better performance than generating values individually
        ///     because it pulls multiple random values from one ulong via bit-shifting.
        /// </remarks>
        /// <param name="buffer">The array to fill with random numbers.</param>
        public static void FillArray(int[] buffer) => FillArray(buffer, sizeof(int));

        /// <summary>
        ///     Fills the specified array with random values.
        /// </summary>
        /// <param name="buffer">The array to fill with random numbers.</param>
        public static void FillArray(long[] buffer) => FillArray(buffer, sizeof(long));

        /// <summary>
        ///     Fills the specified array with random values.
        /// </summary>
        /// <remarks>
        ///     This method bit-shifts the ulong random numbers to get multiple target-type RNGS from a single ulong.
        ///     Need to use other methods as an interface to this to force only allowed types.
        /// </remarks>
        /// <typeparam name="T">Target type.</typeparam>
        /// <param name="buffer">The array to fill with random numbers.</param>
        /// <param name="typeSize">Size(in bytes) of the target type(must be a factor of ulong size).</param>
        private static void FillArray<T>(T[] buffer, int typeSize) {
            Debug.Assert(0 == sizeof(ulong) % typeSize);

            for (int i = 0; i < buffer.Length;) {
                ulong n = threadLocalRandom.Value._Next();
                for (int shiftAmount = 0; i < buffer.Length && shiftAmount < sizeof(ulong); shiftAmount += typeSize) {
                    buffer[i++] = UncheckedConvert<T>(n << shiftAmount);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T UncheckedConvert<T>(ulong n) {
            unchecked {
                if (typeof(T) == typeof(byte)) return (T)(object)(byte)(ulong)(object)n;
                if (typeof(T) == typeof(sbyte)) return (T)(object)(sbyte)(ulong)(object)n;
                if (typeof(T) == typeof(ushort)) return (T)(object)(ushort)(ulong)(object)n;
                if (typeof(T) == typeof(short)) return (T)(object)(short)(ulong)(object)n;
                if (typeof(T) == typeof(uint)) return (T)(object)(uint)(ulong)(object)n;
                if (typeof(T) == typeof(int)) return (T)(object)(int)(ulong)(object)n;
                if (typeof(T) == typeof(long)) return (T)(object)(long)(ulong)(object)n;
                if (typeof(T) == typeof(ulong)) return (T)(object)n;

                //THIS WILL ALWAYS THROW A CAST EXCEPTION 
                //throwing one manually would be pointless and when tested prevented some compiler/jit optimization
                return (T)(object)n;
            }
        }
    }
}
