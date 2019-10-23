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
        //RNG shared between threads to create seeds.
        private static readonly RNGSource sharedRandom = new RNGSource((ulong)DateTime.Now.Ticks);
        private static readonly object sharedRandomLock = new object();

        //create thread local rngs using shared rng.
        private static ThreadLocal<RNGSource> threadLocalRNG = new ThreadLocal<RNGSource>(() => {
            List<ulong> entropySources = new List<ulong>();

            Stopwatch stopwatch = Stopwatch.StartNew();

            /*
             * use shared seed to create thread local seeds so that if other seeds are created close together,
             * they wont use the same time as their main source of entropy
             */
            lock (sharedRandomLock) {
                entropySources.Add(sharedRandom.Next());
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

            return new RNGSource(seed);
        });

        private static ThreadLocal<RNG<byte>> byteRNG = new ThreadLocal<RNG<byte>>(() => 
            new RNGDownSampler<byte>(((ulong n) => (byte)n), sizeof(byte), threadLocalRNG.Value));

        private static ThreadLocal<RNG<sbyte>> sbyteRNG = new ThreadLocal<RNG<sbyte>>(() =>
            new RNGDownSampler<sbyte>(((ulong n) => (sbyte)n), sizeof(sbyte), threadLocalRNG.Value));

        private static ThreadLocal<RNG<uint>> uintRNG = new ThreadLocal<RNG<uint>>(() =>
            new RNGDownSampler<uint>(((ulong n) => (uint)n), sizeof(uint), threadLocalRNG.Value));

        private static ThreadLocal<RNG<int>> intRNG = new ThreadLocal<RNG<int>>(() => 
            new RNGDownSampler<int>(((ulong n) => (int)n), sizeof(int), threadLocalRNG.Value));

        private static ThreadLocal<RNG<ushort>> ushortRNG = new ThreadLocal<RNG<ushort>>(() =>
            new RNGDownSampler<ushort>(((ulong n) => (ushort)n), sizeof(ushort), threadLocalRNG.Value));

        private static ThreadLocal<RNG<short>> shortRNG = new ThreadLocal<RNG<short>>(() =>
            new RNGDownSampler<short>(((ulong n) => (short)n), sizeof(short), threadLocalRNG.Value));

        private static ThreadLocal<RNG<float>> floatRNG = new ThreadLocal<RNG<float>>(() =>
            new RNGDownSampler<float>(((ulong n) => UncheckedConvert<float>(n)), sizeof(float), threadLocalRNG.Value));

        /// <summary>
        ///     Causes early initialization of thread local rng, otherwise initialized when first used.
        /// </summary>
        public static void Init() {
            //access the field to force initialization
            _ = threadLocalRNG.Value;
        }


        /// <summary>
        ///     Returns a random ulong.
        /// </summary>
        public static ulong Next() {
            return threadLocalRNG.Value.Next();
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
            return threadLocalRNG.Value.Range(min, max);
        }

        /// <summary>
        ///     Returns a random ULong in the specified range.
        /// </summary>
        /// <remarks>
        ///     Range mapped outputs are unbiased.
        /// </remarks>
        /// <param name="max"> exclusive maximum value. </param>
        public static ulong Next(ulong max) {
            return threadLocalRNG.Value.Range(max);
        }

        /// <summary>
        ///     Returns a random long.
        /// </summary>
        public static long NextLong() => UncheckedConvert<long>(Next());//don't need as much conversion for 64bit types

        /// <summary>
        ///     Returns a random double.
        /// </summary>
        public static double NextDouble() => UncheckedConvert<double>(Next());//don't need as much conversion for 64bit types

        /// <summary>
        ///     Returns a random float.
        /// </summary>
        public static float NextFloat() => floatRNG.Value.Next();

        /// <summary>
        ///     Returns a random uint.
        /// </summary>
        public static uint NextUInt() => uintRNG.Value.Next();

        /// <summary>
        ///     Returns a random int.
        /// </summary>
        public static int NextInt() => intRNG.Value.Next();

        /// <summary>
        ///     Returns a random ushort.
        /// </summary>
        public static ushort NextUShort() => ushortRNG.Value.Next();

        /// <summary>
        ///     Returns a random short.
        /// </summary>
        public static short NextShort() => shortRNG.Value.Next();

        /// <summary>
        ///     Returns a random byte.
        /// </summary>
        public static byte NextByte() => byteRNG.Value.Next();

        /// <summary>
        ///     Returns a random sbyte.
        /// </summary>
        public static sbyte NextSByte() => sbyteRNG.Value.Next();

        /// <summary>
        ///     Fills the specified array with random values.
        /// </summary>
        /// <remarks>
        ///     This method provides better performance than generating values individually.
        /// </remarks>
        /// <param name="buffer">The array to fill with random numbers.</param>
        public static void FillArray(byte[] buffer) => FillArray(buffer, sizeof(byte));

        /// <summary>
        ///     Fills the specified array with random values.
        /// </summary>
        /// <remarks>
        ///     This method provides better performance than generating values individually.
        /// </remarks>
        /// <param name="buffer">The array to fill with random numbers.</param>
        public static void FillArray(sbyte[] buffer) => FillArray(buffer, sizeof(sbyte));

        /// <summary>
        ///     Fills the specified array with random values.
        /// </summary>
        /// <remarks>
        ///     This method provides better performance than generating values individually.
        /// </remarks>
        /// <param name="buffer">The array to fill with random numbers.</param>
        public static void FillArray(ushort[] buffer) => FillArray(buffer, sizeof(ushort));

        /// <summary>
        ///     Fills the specified array with random values.
        /// </summary>
        /// <remarks>
        ///     This method provides better performance than generating values individually.
        /// </remarks>
        /// <param name="buffer">The array to fill with random numbers.</param>
        public static void FillArray(short[] buffer) => FillArray(buffer, sizeof(short));

        /// <summary>
        ///     Fills the specified array with random values.
        /// </summary>
        /// <remarks>
        ///     This method provides better performance than generating values individually.
        /// </remarks>
        /// <param name="buffer">The array to fill with random numbers.</param>
        public static void FillArray(uint[] buffer) => FillArray(buffer, sizeof(uint));

        /// <summary>
        ///     Fills the specified array with random values.
        /// </summary>
        /// <remarks>
        ///     This method provides better performance than generating values individually.
        /// </remarks>
        /// <param name="buffer">The array to fill with random numbers.</param>
        public static void FillArray(int[] buffer) => FillArray(buffer, sizeof(int));

        /// <summary>
        ///     Fills the specified array with random values.
        /// </summary>
        /// <remarks>
        ///     This method provides better performance than generating values individually.
        /// </remarks>
        /// <param name="buffer">The array to fill with random numbers.</param>
        public static void FillArray(float[] buffer) => FillArray(buffer, sizeof(float));

        /// <summary>
        ///     Fills the specified array with random values.
        /// </summary>
        /// <remarks>
        ///     This method provides better performance than generating values individually.
        /// </remarks>
        /// <param name="buffer">The array to fill with random numbers.</param>
        public static void FillArray(double[] buffer) => FillArray(buffer, sizeof(double));

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
        ///     Using this instead of RNGAdapter because UncheckedConvert optimizes better than injected converter functions.
        /// </remarks>
        /// <typeparam name="T">Target type.</typeparam>
        /// <param name="buffer">The array to fill with random numbers.</param>
        /// <param name="typeSize">Size(in bytes) of the target type(must be a factor of ulong size).</param>
        private static void FillArray<T>(T[] buffer, int typeSize) {
            Debug.Assert(0 == sizeof(ulong) % typeSize);

            var rng = threadLocalRNG.Value;

            for (int i = 0; i < buffer.Length;) {
                ulong n = rng.Next();
                for (int shiftAmount = 0; i < buffer.Length && shiftAmount < sizeof(ulong); shiftAmount += typeSize) {
                    buffer[i++] = UncheckedConvert<T>(n >> shiftAmount*8);
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe T UncheckedConvert<T>(ulong n) {
            unchecked {
                if (typeof(T) == typeof(byte)) return (T)(object)(byte)(ulong)(object)n;
                if (typeof(T) == typeof(sbyte)) return (T)(object)(sbyte)(ulong)(object)n;
                if (typeof(T) == typeof(ushort)) return (T)(object)(ushort)(ulong)(object)n;
                if (typeof(T) == typeof(short)) return (T)(object)(short)(ulong)(object)n;
                if (typeof(T) == typeof(uint)) return (T)(object)(uint)(ulong)(object)n;
                if (typeof(T) == typeof(int)) return (T)(object)(int)(ulong)(object)n;
                if (typeof(T) == typeof(long)) return (T)(object)(long)(ulong)(object)n;
                if (typeof(T) == typeof(float)) return (T)(object)*((float*)&n);
                if (typeof(T) == typeof(double)) return (T)(object)*((double*)&n);
                if (typeof(T) == typeof(ulong)) return (T)(object)n;

                //THIS WILL ALWAYS THROW A CAST EXCEPTION 
                //throwing one manually would be pointless and when tested prevented some compiler/jit optimization
                return (T)(object)n;
            }
        }

    }
}
