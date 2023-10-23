
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class AsyncUdpClient
{
    private const int Port = 8001;
    private const string ServerIp = "127.0.0.1";

    static void Main()
    {
        UdpClient udpClient = new UdpClient();

        while (true)
        {
            Console.WriteLine("Введите команду:");
            Console.WriteLine("getall - Вывести все записи");
            Console.WriteLine("get <номер> - Вывести запись по номеру");
            Console.WriteLine("delete <номер> - Удалить запись по номеру");
            Console.WriteLine("add <данные> - Добавить запись");
            Console.WriteLine("exit - Выйти из программы");


            string command = Console.ReadLine();
            string response = "";

            if (command == "exit")
            {
                break;
            }

            byte[] requestData = Encoding.UTF8.GetBytes(command);
            udpClient.Send(requestData, requestData.Length, ServerIp, Port);

            IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Any, Port);
            byte[] responseData = udpClient.Receive(ref serverEndpoint);

            response = Encoding.UTF8.GetString(responseData);
            Console.WriteLine("Ответ сервера:");
            Console.WriteLine(response);

        }

        udpClient.Close();
    }
}