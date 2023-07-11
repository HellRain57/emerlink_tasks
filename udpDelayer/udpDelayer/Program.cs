using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;


class Program
{
    static IPAddress localAddress = IPAddress.Parse("127.0.0.1");
    static int port_in = 0;
    static int port_out = 0;
    static int delay = 0;
    static uint packNum = 0;
    static uint timeStamp = 0;
    static uint numRecPacks = 0;
    static uint lostPacks = 0;
    static uint real_bitRate = 0;
    static TimeSpan ts;

    static void Main(string[] args)
    {
        Console.WriteLine("Введите порт для приема пакетов: ");
        port_in = Convert.ToInt32(Console.ReadLine());

        Console.WriteLine("Введите порт для отправки пакетов: ");
        port_out = Convert.ToInt32(Console.ReadLine());

        Console.WriteLine("Введите время задержики(в мс): ");
        delay = Convert.ToInt32(Console.ReadLine());

        //port_in = 50000;
        //port_out = 50001;
        //delay = 3400;

        ReceiveSendMessage();
    }

    static async void ReceiveSendMessage()
    {
        byte[] data = new byte[1000];
        using Socket receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        receiver.Bind(new IPEndPoint(localAddress, port_in));

        TimerCallback tm = new TimerCallback(PrintData);
        Timer timer = new Timer(tm, null, 0, 100);
        List<uint> pask = new List<uint>();
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

            pask.Add(packNum);

            numRecPacks++;
            await SendMessage(data);
        }
    }


    async static Task SendMessage(byte[] data)
    {
        using UdpClient sender = new UdpClient();
        Stopwatch sw = new Stopwatch();
        TimeSpan tsStart, tsStop;
        sw.Start();
        tsStart = TimeSpan.FromTicks(sw.ElapsedTicks);
        tsStop = TimeSpan.FromTicks(sw.ElapsedTicks);
        while ((tsStop.Ticks - tsStart.Ticks) < delay)
        {
            tsStop = TimeSpan.FromTicks(sw.ElapsedTicks);
        }
        await sender.SendAsync(data, new IPEndPoint(localAddress, port_out));
    }

    public static void PrintData(object obj)
    {
        //Console.Clear();
        Console.WriteLine("Received packs: " + numRecPacks);
        real_bitRate = numRecPacks;
    }

}