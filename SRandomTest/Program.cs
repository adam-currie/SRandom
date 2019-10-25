using System;
using System.Diagnostics;
using System.Threading;
using SuperRandom;

namespace SRandomTest {
    class Program {
        static void Main() {
            Console.WriteLine("press any key...");
            Console.ReadKey();

            SpeedTest();

            Console.WriteLine("press any key...");
            Console.ReadKey();

            SpeedTestFillArray();

            Console.WriteLine("press any key...");
            Console.ReadKey();

            SpeedTestRange();

            Console.WriteLine("press any key...");
            Console.ReadKey();
        }

        private static void SpeedTestRange() {
            int count = 50000000;

            ulong[] arr = new ulong[count];

            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < arr.Length; i++) {
                arr[i] = SRandom.Next(10000, 100000);
            }
            stopwatch.Stop();
            var time = stopwatch.ElapsedMilliseconds;

            Console.WriteLine(count + " ulongs generated on 1 thread with range method: " + time + "ms");
        }

        private static void SpeedTestFillArray() {
            int count = 10000000;


            //bytes
            byte[] bytes = new byte[count];

            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < bytes.Length; i++) {
                bytes[i] = SRandom.NextByte();
            }
            stopwatch.Stop();
            var nextByteTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Reset();
            stopwatch.Start();
            SRandom.FillArray(bytes);
            stopwatch.Stop();
            var fillArrayTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Reset();
            stopwatch.Start();
            byte[] b = new byte[8];
            for (int i = 0; i < bytes.Length/8; i++) {
                SRandom.FillArray(b);
            }
            stopwatch.Stop();
            var fillArraySplitTime = stopwatch.ElapsedMilliseconds;

            //ints
            var intsCount = count / 2;
            int[] ints = new int[intsCount];

            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < ints.Length; i++) {
                ints[i] = SRandom.NextInt();
            }
            stopwatch.Stop();
            var nextIntTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Reset();
            stopwatch.Start();
            SRandom.FillArray(ints);
            stopwatch.Stop();
            var fillArrayIntTime = stopwatch.ElapsedMilliseconds;


            //longs
            var longsCount = count / 4;
            long[] longs = new long[longsCount];

            stopwatch.Reset();
            stopwatch.Start();
            for (long i = 0; i < longs.Length; i++) {
                longs[i] = SRandom.NextLong();
            }
            stopwatch.Stop();
            var nextLongTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Reset();
            stopwatch.Start();
            SRandom.FillArray(longs);
            stopwatch.Stop();
            var fillArrayLongTime = stopwatch.ElapsedMilliseconds;


            Console.WriteLine(count + " bytes generated on 1 thread with SRandom.NextByte: " + nextByteTime + "ms");
            Console.WriteLine(count + " bytes generated on 1 thread with SRandom.FillArray: " + fillArrayTime + "ms");
            Console.WriteLine(count + " bytes generated on 1 thread with SRandom.FillSplitArray: " + fillArraySplitTime + "ms");
            Console.WriteLine(intsCount + " ints generated on 1 thread with SRandom.NextInt: " + nextIntTime + "ms");
            Console.WriteLine(intsCount + " ints generated on 1 thread with SRandom.FillArray: " + fillArrayIntTime + "ms");
            Console.WriteLine(longsCount + " longs generated on 1 thread with SRandom.Nextlong: " + nextLongTime + "ms");
            Console.WriteLine(longsCount + " longs generated on 1 thread with SRandom.FillArray: " + fillArrayLongTime + "ms");
        }

        private static void SpeedTest() {
            int countPerThread = 10000;
            int threadCount = 10;

            Thread[] threads = new Thread[threadCount];

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < threads.Length; i++) {
                threads[i] = new Thread(() => {
                    for(int j = 0; j < countPerThread; j++) {
                        SRandom.Next();
                    }
                });
            }

            foreach(Thread thread in threads) {
                thread.Start();
            }

            foreach(Thread thread in threads) {
                thread.Join();
            }

            stopwatch.Stop();

            Console.WriteLine("Generated "+ threadCount * countPerThread + " ulongs accross " + threadCount + " threads :" + stopwatch.ElapsedMilliseconds + "ms");
        }

    }
}
