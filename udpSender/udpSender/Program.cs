using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static IPAddress localAddress = IPAddress.Parse("127.0.0.1");
    static int remotePort = 0;
    static long bitRate = 0;
    static long createPackTime = 0;
    static long numOfPacks = 0;
    static long real_bitRate = 0;
    static void Main(string[] args)
    {

        Console.WriteLine("Введите порт для отправки пакетов: ");
        remotePort = Convert.ToInt32(Console.ReadLine());

        Console.WriteLine("Введите скорость передачи (байт/cек): ");
        bitRate = Convert.ToInt32(Console.ReadLine());

        //remotePort = 50000;
        //bitRate = 10_000_000;
        SendMessage();
    }

    static async void SendMessage()
    {
        using UdpClient sender = new UdpClient();
        byte[] data = new byte[1000];

        Stopwatch stopwatchAllTime = new Stopwatch();



        long delaySend_ticks = 10_000_000 / (bitRate / 1_000); // получим задержку между пакетами в тиках (1 тик = 100 нс)
        stopwatchAllTime.Start();
        TimerCallback tm = new TimerCallback(PrintData);
        Timer timer = new Timer(tm, null, 0, 100);
        TimeSpan tsStart, tsStop;

        while (true)
        {
            createPackTime = (uint)stopwatchAllTime.ElapsedTicks;
            tsStart = TimeSpan.FromTicks(createPackTime);
            //================== Номер пакета ==================//
            data[0] = (byte)(numOfPacks & 0x000000ff);
            data[1] = (byte)((numOfPacks & 0x0000ff00) >> 8);
            data[2] = (byte)((numOfPacks & 0x00ff0000) >> 16);
            data[3] = (byte)((numOfPacks & 0xff000000) >> 24);
            //==================================================//

            //================== Время создания пакета ==================//
            data[4] = (byte)(createPackTime & 0x000000ff);
            data[5] = (byte)((createPackTime & 0x0000ff00) >> 8);
            data[6] = (byte)((createPackTime & 0x00ff0000) >> 16);
            data[7] = (byte)((createPackTime & 0xff000000) >> 24);
            //===========================================================//

            await sender.SendAsync(data, new IPEndPoint(localAddress, remotePort));


            numOfPacks++;
            tsStop = TimeSpan.FromTicks(stopwatchAllTime.ElapsedTicks);
            while ((tsStop.Ticks - tsStart.Ticks) < delaySend_ticks)
            {
                tsStop = TimeSpan.FromTicks(stopwatchAllTime.ElapsedTicks);
            }
        }
    }

    public static void PrintData(object obj)
    {

        //Console.Clear();
        Console.WriteLine((numOfPacks - real_bitRate) * 1000 + "bps " + "timeStamp: " + TimeSpan.FromTicks(createPackTime) + " | " + numOfPacks);
        real_bitRate = numOfPacks;
    }

}