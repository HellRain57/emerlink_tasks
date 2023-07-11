using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static IPAddress localAddress = IPAddress.Parse("127.0.0.1");
    static int remotePort = 0;
    static uint packNum = 0;
    static uint timeStamp = 0;
    static uint numRecPacks = 0;
    static uint lostPacks = 0;
    static uint real_bitRate = 0;
    static TimeSpan ts;

    static void Main(string[] args)
    {
        //Stopwatch stopwatchAllTime = new Stopwatch();
        //long tsStart, tsStop;
        //stopwatchAllTime.Start();
        //List<long> packets = new List<long>();
        //for (long i = 0; ; i++)
        //{
        // tsStart = stopwatchAllTime.ElapsedMilliseconds;
        // tsStop = stopwatchAllTime.ElapsedMilliseconds - tsStart;
        // if (tsStop > 14)
        // {
        // packets.Add(tsStop);
        // packets.Add(i);
        // packets.Add(4444444444);
        // }
        //}

        Console.WriteLine("Введите порт для приема пакетов: ");
        remotePort = Convert.ToInt32(Console.ReadLine());
        //remotePort = 50001;

        ReceiveMessageAsync();
    }

    static void ReceiveMessageAsync()
    {
        byte[] data = new byte[1000];
        using Socket receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        receiver.Bind(new IPEndPoint(localAddress, remotePort));

        TimerCallback tm = new TimerCallback(PrintData);
        Timer timer = new Timer(tm, null, 0, 100);

        while (true)
        {
            receiver.Receive(data);

            packNum = data[0];
            packNum += (uint)data[1] << 8;
            packNum += (uint)data[2] << 16;
            packNum += (uint)data[3] << 24;

            timeStamp = data[4];
            timeStamp += (uint)data[5] << 8;
            timeStamp += (uint)data[6] << 16;
            timeStamp += (uint)data[7] << 24;
            ts = TimeSpan.FromTicks(timeStamp);

            if (numRecPacks != packNum)
            {
                lostPacks = packNum - numRecPacks;
            }
            numRecPacks++;
        }
    }

    public static void PrintData(object obj)
    {
        //Console.Clear();
        Console.WriteLine((numRecPacks - real_bitRate) * 1000 + "bps " + "timeStamp: " + ts + " LostPacks: " + lostPacks);
        real_bitRate = numRecPacks;
    }

}