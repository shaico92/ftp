using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using FileTransfer;
namespace ftpsrv
{





    //FILE TRANSFER USING C#.NET SOCKET - SERVER
    class Program
    {


     


        
        static void Main(string[] args)
        {

            //testing how to extract all the directories



            //getDirectotiresRecoursevly(ref dir);


            //  FileTransferFactory.GetInstance().UploadFolder("C:\\Users\\shaic\\Downloads\\fix");


            // 
            var add = IPAddress.Parse("127.0.0.1");

            FileTransferFactory.GetInstance().Init(add, 5032,15000);
            //FileTransferFactory.GetInstance().UploadFile("C:\\Users\\shaic\\Downloads\\fix\\SharedMemory.sln", false);


            //FileTransferFactory.GetInstance().SetDesigna//tedFolder("C:\\Users\\shaic\\Documents\\uploads");


            FileTransferFactory.GetInstance().OpenReceivingConnection();
        }

        
    }
}