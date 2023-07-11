using System.Diagnostics;
using System;
using System.IO;
using System.Text;

record class ParamsForThreads(int startNum, int endNum);

class Program
{
    static FileStream fstream = new FileStream("primes.txt", FileMode.Create);
    static object locker = new();

    static void Main(string[] args)
    {
        Console.Write("Введите Nt: ");
        int Nt = Convert.ToInt32(Console.ReadLine());

        Console.Write("Введите IMax: ");
        int IMax = Convert.ToInt32(Console.ReadLine());

        //int Nt = 7;
        //int IMax = 1011;


        double iterForThread = (double)IMax / (double)Nt; // считаем сколько приходится на каждый поток
        int extraIter = IMax - (Nt * (int)iterForThread); // порой числа между потоками поровну не делится. считаем сколько потоков остается

        int start = 0;
        int stop = 0;

        fstream.Close(); //перед запуском, файл пересоздается и открывается. Вот здесь он закрывается

        List<Thread> threads = new List<Thread>();

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for (int i = 0; i < Nt; i++)
        {
            start = stop;
            stop = start + (int)iterForThread;
            if (extraIter > 0)
            {
                stop++; // если есть числа, которые по ровну между потоками не делятся, то раскидываем по одному числу каждому потоку начиная с первого
                extraIter--;
            }

            ParamsForThreads paramsForThreads = new ParamsForThreads(start, stop); // указываем каждому потоку, с какого числа по какое проверять
            Thread myThread = new(checkPrimeNums);
            threads.Add(myThread);
            threads[i].Name = $"Поток {i}";
            threads[i].Start(paramsForThreads);
        }
        for (int i = 0; i < Nt; i++)
        {
            threads[i].Join();
        }
        //останавливаем счётчик
        stopwatch.Stop();
        //смотрим сколько тактов было затрачено на выполнение
        Console.WriteLine("Время выполнения: " + stopwatch.ElapsedMilliseconds + " мс");
    }

    static bool isPrime(int n)
    {
        for (int i = 2; i <= Math.Sqrt(n); i++)
        {
            if (n % i == 0)
            {
                //n не простое
                return false;
            }
        }
        // n простое.
        return true;
    }

    static void checkPrimeNums(object? obj)
    {
        string text = "";
        if (obj is ParamsForThreads paramsForThreads)
        {
            for (int i = paramsForThreads.startNum; i < paramsForThreads.endNum; i++)
            {
                if (isPrime(i))
                {
                    Console.WriteLine(Thread.CurrentThread.Name + ": " + i);
                    text += Thread.CurrentThread.Name + ": " + i + "| "; // строка для записи
                }
            }
            text += "\n";
            // преобразуем строку в байты
            byte[] buffer = Encoding.Default.GetBytes(text);
            lock (locker)
            {
                fstream = new FileStream("primes.txt", FileMode.Append);

                fstream.Write(buffer, 0, buffer.Length);
                fstream.Close();
                Console.WriteLine("Текст записан в файл ==== " + Thread.CurrentThread.Name);
            }

        }
    }

}