using System;
using System.Diagnostics;
using System.Threading;
using SuperRandom;

namespace SRandomTest
{
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("press any key...");
            Console.ReadKey();

            SpeedTest();

            Console.WriteLine("press any key...");
            Console.ReadKey();
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
