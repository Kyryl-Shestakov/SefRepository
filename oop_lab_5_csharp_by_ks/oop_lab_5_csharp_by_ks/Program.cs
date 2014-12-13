using System;
using System.Threading;
using System.Threading.Tasks;

namespace oop_lab_5_csharp_by_ks
{
    /// <summary>
    /// A delegate compatible with the used method
    /// </summary>
    /// <param name="firstParameter"></param>
    /// <param name="secondParameter"></param>
    /// <param name="thirdParameter"></param>
    /// <returns></returns>
    delegate int MyDelegate(ref short firstParameter, out long secondParameter, char thirdParameter);

    sealed class Program
    {
        /// <summary>
        /// An entry point of a program
        /// </summary>
        /// <param name="args">
        /// Command line arguments
        /// </param>
        static void Main(string[] args)
        {
            short s = 5;
            long l;

            Console.WriteLine("\nCalling method synchronously:");
            Console.WriteLine("The result is " + MyStaticMethod(ref s, out l, 'a')); //synchronous call of a method

            Console.WriteLine("\nCalling method in another thread:");
            s += 5;

            new Thread(() =>
            {
                Console.WriteLine("The result is " + MyStaticMethod(ref s, out l, 'b')); //synchronous call in a new thread
            }).Start();

            for (int i = 0; i < 6; ++i)
            {
                Thread.Sleep(1000);
                Console.WriteLine("\nMain method running"); //view of concurrent working
            }

            Console.WriteLine("\nCalling method asynchronously (first way):");
            s += 5;

            MyDelegate del = new MyDelegate(MyStaticMethod); 
            IAsyncResult iar = del.BeginInvoke(ref s, out l, 'c', null, null); //usage of asynchronous call on a delegate

            for (int i = 0; i < 3; ++i)
            {
                Thread.Sleep(1000);
                Console.WriteLine("\nMain method running");
            }

            Console.WriteLine("The result is " + del.EndInvoke(ref s, out l, iar));

            Console.WriteLine("\nCalling method asynchronously (second way):");
            s += 5;
            iar = del.BeginInvoke(ref s, out l, 'd', null, null);

            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine("\nMain thread running");

                if (iar.IsCompleted)
                {
                    Console.WriteLine("The result is " + del.EndInvoke(ref s, out l, iar));
                    break;
                }
            }

            Console.WriteLine("\nCalling method asynchronously (third way):");
            s += 5;
            iar = del.BeginInvoke(ref s, out l, 'e', null, null);
            iar.AsyncWaitHandle.WaitOne(10000, false);
            Console.WriteLine("The result is " + del.EndInvoke(ref s, out l, iar));

            Console.WriteLine("\nCalling method asynchronously (fourth way):");
            s += 5;
            iar = del.BeginInvoke(ref s, out l, 'f', (asyncResult) =>
                {
                    Console.WriteLine("The result is " + del.EndInvoke(ref s, out l, asyncResult));
                },
                null);

            iar.AsyncWaitHandle.WaitOne();
            Thread.Sleep(1000);

            Console.WriteLine("\nCalling method asynchronously (fifth way):");
            s += 5;

            Task task = DisplayResult(s, l, 'g');

            for (int i = 0; i < 3; ++i)
            {
                Thread.Sleep(1000);
                Console.WriteLine("\nMain method running");
            }

            task.Wait();
            Console.WriteLine();

            //Several threads competing for one resource

            Console.WriteLine("Illustration of several threads accessing shared resource simultaneously\n");
            Thread[] threads = new Thread[5];
            int count = 0;

            for (int i = 0; i < 5; ++i)
            {
                threads[i] = new Thread(() =>
                {
                    for (int j = 0; j < 5; ++j)
                    {
                        Console.WriteLine("Thread with priority " + Thread.CurrentThread.Priority + " has the resource: " + ++count);
                        Thread.Sleep(10);
                    }
                });
                threads[i].Priority = (ThreadPriority)i;
            }

            for (int i = 0; i < 5; ++i)
            {
                threads[i].Start();
            }

            for (int i = 0; i < 5; ++i)
            {
                threads[i].Join();
            }
            Console.WriteLine();

            //Synchronization of these threads

            Console.WriteLine("Illustration of thread synchronization with a lock:\n");
            count = 0;
            object obj = new object();

            for (int i = 0; i < 5; ++i)
            {
                threads[i] = new Thread(() =>
                {
                    lock (obj)
                    {
                        for (int j = 0; j < 5; ++j)
                        {
                            Console.WriteLine("Thread with priority " + Thread.CurrentThread.Priority + " has the resource: " + ++count);
                            Thread.Sleep(10);
                        }
                    }
                });
                threads[i].Priority = (ThreadPriority)i;
            }

            for (int i = 0; i < 5; ++i)
            {
                threads[i].Start();
            }

            for (int i = 0; i < 5; ++i)
            {
                threads[i].Join();
            }
            Console.WriteLine();

            //Using Mutex

            Console.WriteLine("Illustration of thread synchronization using Mutex:");
            count = 0;
            Mutex mtx = new Mutex();

            for (int i = 0; i < 5; ++i)
            {
                threads[i] = new Thread(() =>
                {
                    mtx.WaitOne();
                    Console.WriteLine("Thread with priority " + Thread.CurrentThread.Priority + " acquired a mutex\n");

                    for (int j = 0; j < 5; ++j)
                    {
                        Console.WriteLine("Thread with priority " + Thread.CurrentThread.Priority + " has the resource: " + ++count);
                        Thread.Sleep(10);
                    }

                    Console.WriteLine("\nThread with priority " + Thread.CurrentThread.Priority + " released a mutex\n");
                    mtx.ReleaseMutex();
                });
                threads[i].Priority = (ThreadPriority)i;
            }

        for (int i = 0; i < 5; ++i)
        {
            threads[i].Start();
        }

        for (int i = 0; i < 5; ++i)
        {
            threads[i].Join();
        }

        //Using Semaphore

        Console.WriteLine("Illustration of thread synchronization using Semaphore:\n");
        count = 0;
        Semaphore smphr = new Semaphore(2, 2);

        for (int i = 0; i < 5; ++i)
        {
            threads[i] = new Thread(() =>
            {
                smphr.WaitOne();
                Console.WriteLine("Thread with priority " + Thread.CurrentThread.Priority + " acquired a semaphore\n");

                    for (int j = 0; j < 5; ++j)
                    {
                        Console.WriteLine("Thread with priority " + Thread.CurrentThread.Priority + " has the resource: " + ++count);
                        Thread.Sleep(10);
                    }

                    Console.WriteLine("\nThread with priority " + Thread.CurrentThread.Priority + " released a semaphore\n");
                    smphr.Release();
                });
                threads[i].Priority = (ThreadPriority)i;
            }

            for (int i = 0; i < 5; ++i)
            {
                threads[i].Start();
            }

            for (int i = 0; i < 5; ++i)
            {
                threads[i].Join();
            }

            Console.ReadKey(true);
        }

        /// <summary>
        /// A method to be called several ways
        /// </summary>
        /// <param name="firstParameter"></param>
        /// <param name="secondParameter"></param>
        /// <param name="thirdParameter"></param>
        /// <returns></returns>
        static int MyStaticMethod(ref short firstParameter, out long secondParameter, char thirdParameter)
        {
            secondParameter = 10L;

            Console.WriteLine("Inside my static method:");
            Console.WriteLine("First parameter is " + firstParameter);
            Console.WriteLine("Second parameter is " + secondParameter);
            Console.WriteLine("Third parameter is " + thirdParameter);
            Console.Write("Wait for a method to complete");
            
            for (int i = 0; i != 50; ++i)
            {
                Console.Write(".");
                Thread.Sleep(100);
            }

            Console.WriteLine("\nThe method is complete");

            return (int) (int) firstParameter + (int) secondParameter;
        }

        /// <summary>
        /// Asynchronous static method lower in the call graph
        /// </summary>
        /// <param name="firstParameter"></param>
        /// <param name="secondParameter"></param>
        /// <param name="thirdParameter"></param>
        /// <returns></returns>
        //static async Task<int> MyStaticMethodAsync(short firstParameter, long secondParameter, char thirdParameter)
        //{
        //    return await Task.Run(async () =>
        //        {
        //            secondParameter = 10L;

        //            Console.WriteLine("Inside my static method:");
        //            Console.WriteLine("First parameter is " + firstParameter);
        //            Console.WriteLine("Second parameter is " + secondParameter);
        //            Console.WriteLine("Third parameter is " + thirdParameter);
        //            Console.Write("Wait for a method to complete");

        //            for (int i = 0; i != 50; ++i)
        //            {
        //                Console.Write(".");
        //                //Thread.Sleep(100);
        //                await Task.Delay(100);
        //            }

        //            Console.WriteLine("\nThe method is complete");

        //            return (int)(int)firstParameter + (int)secondParameter;
        //        });
        //}

        ///// <summary>
        ///// Asynchronous static method
        ///// </summary>
        ///// <param name="firstParameter"></param>
        ///// <param name="secondParameter"></param>
        ///// <param name="thirdParameter"></param>
        ///// <returns></returns>
        //static async Task DisplayResult(short firstParameter, long secondParameter, char thirdParameter)
        //{
        //    secondParameter = 10L;
        //    Console.WriteLine("The result is " + await MyStaticMethodAsync(firstParameter, secondParameter, thirdParameter));
        //}
    }
}
