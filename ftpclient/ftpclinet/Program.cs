using System;

using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using FileTransfer;

namespace ftpclient
{
    //FILE TRANSFER USING C#.NET SOCKET - CLIENT
    class Program
    {
        static IPAddress ipEnd;
        static string FTPServer;
        
        static int FTPServerPort;

        static public void Dwnload()
        {

            Console.WriteLine("Please input the name of the file you wish to download");
             string FileName =Console.ReadLine();
            Console.WriteLine("You chose file named {0} checking if exists.....",FileName);
            FileTransferFactory.GetInstance().DownloadFile(FileName);

        }
        static public void Upload()
        {

            Console.WriteLine("Please input the name of the file you wish to upload");
            
            string FileName = Console.ReadLine();
            Console.WriteLine("You chose file named {0} checking if exists.....", FileName);
            FileTransferFactory.GetInstance().UploadFile(FileName);
        }

        static public void setServerFTP()
        {
            bool isValidIp = false;
            bool isValidPort = false;
            Console.WriteLine("Please input the ip of the server");
            while (!isValidIp)
            {
                

               
                FTPServer = Console.ReadLine();

                isValidIp = IPAddress.TryParse(FTPServer, out ipEnd);
                if (isValidIp)
                {
                    break;
                }
                Console.WriteLine("the string {0} is not a valid ip address",FTPServer);
            }
            Console.WriteLine("Please input the port of the server");
            while (!isValidPort)
            {



            string    _FTPServerPort = Console.ReadLine();

                isValidPort = int.TryParse(_FTPServerPort, out FTPServerPort);
                if (isValidPort)
                {
                    break;
                }
                Console.WriteLine("the string {0} is not a valid port", _FTPServerPort);
            }

            FileTransferFactory.GetInstance().Init(ipEnd, FTPServerPort);

        }




        static void Main(string[] args)
        {

            //postFile();


            bool appRunning = true;
            while ( appRunning )

            {
                Console.WriteLine("Choose the following options");
                for (int i = 0; i < 4; i++)
                {

                    switch (i)
                    {
                        case 1:

                            Console.WriteLine($"\t{i}-send file");
                            break;

                        case 2:
                            Console.WriteLine($"\t{i}-download file");
                            break;
                        case 0:
                            Console.WriteLine($"\t{i}-exit");
                            break;
                        case 3:
                            Console.WriteLine($"\t{i}-set FTPServer");
                            break;
                        default:
                            break;
                    }
                }

                var df = Console.ReadKey();
                Console.WriteLine();
                bool howtoGetToDecide = string.IsNullOrEmpty(FTPServer) && FTPServerPort == 0 && df.KeyChar != '3';
                if (howtoGetToDecide)
                {
                    Console.WriteLine("you must set the desired server please choose option 3");
                }
                else
                {
                    switch (df.KeyChar)
                    {
                        case '2':
                            Console.WriteLine("you have chosen to download file");
                            Dwnload();
                            break;

                        case '1':
                            Console.WriteLine("you have chosen to upload file");
                            Upload();
                            break;
                        case '0':
                            Console.WriteLine("exiting application");
                            appRunning = false;
                            break;
                        case '3':
                            setServerFTP();
                            //appRunning = false;
                            break;
                        default:
                            break;
                    }
                }
              

            }








        }

    }
}