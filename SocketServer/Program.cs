using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using NLog;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace SocketServer
{

    [DataContract]
    public class Client
    {
        [DataMember]
        public string Account { get; set; }

        [DataMember]
        public string Name { get; set; }


        public Client(string account, string name)
        {
            Account = account;
            Name = name;
        }

    }



    class Program
    {
        static void Main(string[] args)
        {

            char delimiter = '|';
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

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Host name is: {0} ", ipHost);
          //  Console.WriteLine("host array size is: {0}", size);
            Console.WriteLine("IP is " + ipAddr);
            Console.ResetColor();
            logger.Trace("Endpoint Host is {0} on Port {1} at Host {2}", ipAddr, port.ToString(), ipHost);

            //string delimiter = " | ";
            
            // Создаем сокет Tcp/Ip
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(maxConn);


                Client cli1 = new Client("1410000000", "Central WH");
                Client cli2 = new Client("1410000001", "სან სთორზ");
                Client cli3 = new Client("1410000002", "СкладЪ");
                Client cli4 = new Client("1410000003", "Batumi");
                Client cli5 = new Client("1410000004", "Kutaisi");
                Client cli6 = new Client("1410000005", "Rustavi");
                Client cli7 = new Client("1410000006", "Kakheti");
                Client cli8 = new Client("1410000007", "Poti");
                Client cli9 = new Client("1410000008", "ახალციხე");
                Client cli10 = new Client("1410000009", "Gori");


                Client[] people = new Client[] { cli1, cli2, cli3, cli4, cli5, cli6, cli7, cli8, cli9, cli10 };

                DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Client[]));
                MemoryStream mem = new MemoryStream();
                jsonFormatter.WriteObject(mem, people);


                // Начинаем слушать соединения
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Waiting on port {0}", ipEndPoint);
                    Console.ResetColor();
                    logger.Trace("start listening with {0} max connection", maxConn);
                    // Программа приостанавливается, ожидая входящее соединение
                    Socket handler = sListener.Accept();
                    string data = null;

                    // Мы дождались клиента, пытающегося с нами соединиться

                    byte[] bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);

                    byte[] bytez = new byte[1024];
                    data += Encoding.Default.GetString(bytes, 0, bytesRec); ////  not   Encoding.UTF8.GetString   !!!!!

                    // Показываем данные на консоли
                    Console.Write("Received Message text: " + data + "\n\n");
                    logger.Trace("Received Message text: {0}", data);

                    // Отправляем ответ клиенту\
                    //string reply = "Echo of request of " + data.Length.ToString()  + " chars length";
                    //string reply = "1410123456" + delimiter + "Test name of account";
                    //byte[] msg = Encoding.UTF8.GetBytes(reply);

                    if (data.Substring(0,5) == "CHECK") /// 'CHECK' 
                    {


                        string[] parts = data.Split(delimiter);
                        int size = parts.Length;
                        

                        Console.WriteLine("parts count: {0}", size);
                        string reply = "fail";
                        
                        if (size == 3) {/// process CHECK request
                           
                             reply = "pass";
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("CHECK request received");

                            Console.Write("Credentials: user = ");
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(parts[1]);
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write(" and pass = ");
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(parts[2]);
                            Console.ResetColor();
                            Console.WriteLine("\n");
                            Console.Write("RESULT: ");
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(reply);
                            Console.WriteLine("\n");
                            Console.ResetColor();
                            bytez = System.Text.Encoding.Default.GetBytes(reply);
                        } else
                        {
                            ///////   serialized JSON
                            bytez = mem.ToArray();
                            var str = System.Text.Encoding.Default.GetString(bytez);  ///  default! not UTF8!!!!
                            logger.Debug("Reply: {0}", str);
                            Console.WriteLine("Reply: {0}",str);
                        }
                    }
      
                    /// send bytez
                    
                    handler.Send(bytez);

                    if (data.IndexOf("<EOF>") > -1)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
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
