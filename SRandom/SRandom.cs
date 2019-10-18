using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

namespace SuperRandom {

    /// <summary>
    ///     Static multi-threading optimized random number generator implementing MT19937-64(Mersenne Twister).
    ///     https://en.wikipedia.org/wiki/Mersenne_Twister#C.23_implementation used for c# implementation reference.
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
        private static SRandom sharedRandom = new SRandom((ulong)DateTime.Now.Ticks);
        private static object sharedRandomLock = new object();


        /// <summary>
        ///     Creates SRandom seeded with the specified seed.
        /// </summary>
        private SRandom(ulong seed) {
            mt[0] = seed;
            for(ulong i = 1; i < n; ++i) {
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
            lock(sharedRandomLock) {
                entropySources.Add(sharedRandom._Next());
            }

            //use process info as a source of entropy
            using(Process proc = Process.GetCurrentProcess()) {
                entropySources.Add((ulong)proc.PrivateMemorySize64);
                entropySources.Add((ulong)proc.Id);
            }

            //use time spent generating as a source of entropy
            stopwatch.Stop();
            entropySources.Add((ulong)stopwatch.ElapsedTicks);

            ulong seed = (ulong)DateTime.Now.Ticks;
            foreach(ulong source in entropySources) {
                //if source of entropy erroneously evaluating to zero was used, it would negate the usefullness of the others, so just ignore it
                if(source != 0) {
                    seed *= source;
                }
            }

            return new SRandom(seed);
        });

        /// <summary>
        ///     Returns a random ULong.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
        private ulong _Next() {
            if(index >= n) {
                Twist();
            }

            ulong next = mt[index++];
            next = next ^ ((next >> u) & d);
            next = next ^ ((next << s) & b);
            next = next ^ ((next << t) & c);
            next = next ^ (next >> l);

            return next;
        }

        /// <summary>
        ///     Reset index to zero and twist generator variable.
        /// </summary>
        private void Twist() {
            for(ulong i = 0; i < n; ++i) {
                ulong x = (mt[i] & upper_mask) + (mt[(i + 1) % n] & lower_mask);
                ulong xA = x >> 1;

                if(x % 2 != 0) {
                    xA = xA ^ a;
                }

                mt[i] = mt[(i + m) % n] ^ xA;
            }

            index = 0;
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
        ///     Returns a random long.
        /// </summary>
        public static long NextLong() {
            return unchecked((long)threadLocalRandom.Value._Next());
        }

        /// <summary>
        ///     Returns a random uint.
        /// </summary>
        public static uint NextUInt(){
            return unchecked((uint)threadLocalRandom.Value._Next());
        }

        /// <summary>
        ///     Returns a random int.
        /// </summary>
        public static int NextInt() {
            return unchecked((int)threadLocalRandom.Value._Next());
        }

        /// <summary>
        ///     Returns a random ushort.
        /// </summary>
        public static ushort NextUShort() {
            return unchecked((ushort)threadLocalRandom.Value._Next());
        }

        /// <summary>
        ///     Returns a random short.
        /// </summary>
        public static short NextShort() {
            return unchecked((short)threadLocalRandom.Value._Next());
        }

        /// <summary>
        ///     Returns a random byte.
        /// </summary>
        public static byte NextByte() {
            return unchecked((byte)threadLocalRandom.Value._Next());
        }

        /// <summary>
        ///     Returns a random sbyte.
        /// </summary>
        public static sbyte NextsByte() {
            return unchecked((sbyte)threadLocalRandom.Value._Next());
        }

    }
}
