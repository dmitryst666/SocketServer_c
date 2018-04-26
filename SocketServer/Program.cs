using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using NLog;

namespace SocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            int maxConn = 10;
            logger.Trace("--------------------------------------------------------------  New Instance started...");

            // Устанавливаем для сокета локальную конечную точку
            //IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            //IPAddress ipAddr = ipHost.AddressList[0];
            //int size = ipHost.AddressList.Length;
            // logger.Trace("host array size is: {0}", size);
            // IPAddress ipAddr = IPAddress.Parse("127.0.0.1");


            string ipHost = System.Net.Dns.GetHostName();
            IPAddress ipAddr = System.Net.Dns.GetHostByName(ipHost).AddressList[0];
            int port = 11000;
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);
            
            Console.WriteLine("Host name is: {0} ", ipHost);
          //  Console.WriteLine("host array size is: {0}", size);
            Console.WriteLine("IP is " + ipAddr);
            logger.Trace("Endpoint Host is {0} on Port {1} at Host {2}", ipAddr, port.ToString(), ipHost);
            
            // Создаем сокет Tcp/Ip
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(maxConn);
            

                // Начинаем слушать соединения
                while (true)
                {
                    Console.WriteLine("Waiting on port {0}", ipEndPoint);
                    logger.Trace("start listening with {0} max connection", maxConn);
                    // Программа приостанавливается, ожидая входящее соединение
                    Socket handler = sListener.Accept();
                    string data = null;

                    // Мы дождались клиента, пытающегося с нами соединиться

                    byte[] bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);

                    data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                    // Показываем данные на консоли
                    Console.Write("Received Message text: " + data + "\n\n");
                    logger.Trace("Received Message text: {0}", data);

                    // Отправляем ответ клиенту\
                    string reply = "Echo of request of " + data.Length.ToString()  + " chars length";
                    byte[] msg = Encoding.UTF8.GetBytes(reply);
                    handler.Send(msg);

                    if (data.IndexOf("<EOF>") > -1)
                    {
                        Console.WriteLine("Server is shutting down...");
                        logger.Trace("Shutting down...");
                        break;
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                
                Console.ReadLine();
            }

            LogManager.Shutdown();
        }  //// end of Main
    }
}
