using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FileTransfer
{
    public enum RequestType
    {
        Upload = 1,
        
        Download
    }

    public sealed class FileTransferFactory
    {

        IPEndPoint m_ep;
     
         Socket clientSocket;
        Socket receivingSock;
       const int bufLength = 1024 * 5000;
       byte[] BufferSize = new byte[bufLength];


        private FileTransferFactory() 
        {
            
        }

  
        private static FileTransferFactory _instance;

  
        public static FileTransferFactory GetInstance()
        {
            if (_instance == null)
            {
                _instance = new FileTransferFactory();
            }
            return _instance;
        }

        public  void Init(IPAddress ep_, int port,int timeout=5000)
        {
            if (m_ep==null)
            {
                m_ep = new IPEndPoint(ep_, port); ;
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                clientSocket.ReceiveTimeout = timeout == 0 ? 3000 : timeout;
                
            }

            
        }

        private void SendFileToRequester(string fileName)
        {



            var stream = File.Open(fileName, FileMode.Open);
            var reader = new BinaryReader(stream);

            byte[] fileData = reader.ReadBytes((int)stream.Length);

            byte[] clientData = new byte[bufLength];

            for (int i = 0; i < fileData.Length; i++)
            {
                clientData[i] = fileData[i];
            }



            receivingSock.Send(clientData);


        }


        private void MakeFileInServer(string fileName,
            int count,
            int NameSizeCount,
            byte[] bytesz)
        {


            byte[] sizeofDataInBytes = new byte[4];
            count += NameSizeCount;
            for (int i = 0; i < sizeofDataInBytes.Length; i++)
            {
                sizeofDataInBytes[i] = bytesz[count];
                count++;
            }

            byte[] remainder = new byte[4];
            for (int i = 0; i < remainder.Length; i++)
            {
                remainder[i] = bytesz[count];
                count++;
            }

            int sizeOfDataCount = BitConverter.ToInt32(sizeofDataInBytes, 0);// *255;
            int sizeOfDataCountRemainder = BitConverter.ToInt32(remainder, 0);
            int totalSize = sizeOfDataCountRemainder + (sizeOfDataCount * 255);






            BinaryWriter binWriter =
new BinaryWriter(new FileStream(fileName, FileMode.Create)); ; ;
            binWriter.Write(bytesz, count, totalSize);

            binWriter.Close();


        }


        private void handleRequest(RequestType reqType,
            string fileName,
            int count=0,
            int NameSizeCount=0,
            byte []arr=null
            )
        {
            switch (reqType)
            {
                case RequestType.Upload:
                    MakeFileInServer(fileName, count, NameSizeCount, arr);
                    break;
                case RequestType.Download:
                    SendFileToRequester(fileName);
                    break;
                default:
                    break;
            }


        }

        private bool isFinishingData(byte[] data,Socket sock)
        {


            if (data!=null)
            {
                var addr =(IPEndPoint)sock.RemoteEndPoint;
               var add_= addr.Address.GetAddressBytes();
                int indForAddress = 4;
                for (int i = 0; i < 4; i++)
                {
                    if (data[indForAddress] != add_[i])
                    {
                        return false;
                    }
                    indForAddress++;
                }


                for (int i = data.Length-20; i < data.Length; i++)
                {
                    if (data[i]!=(byte)'$')
                    {
                        return false;
                    }
                }



            }
            return true;
        }

        public void OpenReceivingConnection()
        {
            Console.WriteLine("opening port on {0} on ip {1}",m_ep.Address,m_ep.Port);

            clientSocket.Bind(m_ep);
            clientSocket.Listen(100);
            receivingSock = clientSocket.Accept();
            int bytesRec = 0;
            byte[] bytes = null;

           

                while (true)
                {
                try
                {
                    List<(byte[], int)> byts = new List<(byte[], int)>();

                    byte[] buffTocheck= new byte[bufLength];
                    while (true)
                    {

                           bytes = new byte[bufLength];
                            bytesRec += receivingSock.Receive(bytes);
                        if (bytesRec== bufLength)
                        {
                            
                            if (isFinishingData(bytes, receivingSock))
                            {
                                buffTocheck = bytes;
                                break;
                            }
                        }




                    }






                    
                   

                    bytesRec = 0;


                    int count = 0;

                    byte[] requestType = new byte[4];

                    for (int i = 0; i < requestType.Length; i++)
                    {
                        requestType[i] = buffTocheck[count];
                        count++;
                    }

                    int requestTypeInt = BitConverter.ToInt32(requestType, 0);

                    byte[] NameSize = new byte[4];
                    count += 4;
                    for (int i = 0; i < NameSize.Length; i++)
                    {
                        NameSize[i] = buffTocheck[count];
                        count++;
                    }
                    int NameSizeCount = BitConverter.ToInt32(NameSize, 0);
                    string FileName = Encoding.UTF8.GetString(buffTocheck, count, NameSizeCount);
                    handleRequest((RequestType)requestTypeInt, FileName, count, NameSizeCount, buffTocheck); ;
                }
                catch (Exception e)
                {

                    ;
                }
            }


           



        }

        private  void ConnectToFTP()
        {
            if (!clientSocket.Connected)
            {
                clientSocket.Connect(m_ep);
            }
            


        }
        private void DisconnectFromFTP()
        {
            //clientSocket.Disconnect();


        }

        public void DownloadFile(string FileName)
        {

            ConnectToFTP();
            byte[] nameAs = new byte[4];
            nameAs = Encoding.ASCII.GetBytes(FileName);
        








        int nextStartPoint = 0;
        byte[] nameSize = BitConverter.GetBytes(nameAs.Length);

        //this is a header for type of request 2=fetch file

       

        byte[] ReqType = BitConverter.GetBytes((int)RequestType.Download);
                    for (int i = 0; i<ReqType.Length; i++)
                    {
                BufferSize[nextStartPoint] = ReqType[i];
                        nextStartPoint++;
                    }



                    for (int i = 0; i<nameSize.Length; i++)
                    {
                BufferSize[nextStartPoint] = nameSize[i];
                        nextStartPoint++;
                    }

//nextStartPoint += 1;
            for (int i = 0; i < nameAs.Length; i++)
    {
                BufferSize[nextStartPoint] = nameAs[i];
    nextStartPoint++;
    }
            clientSocket.Send(BufferSize);
            int bytesRec = 0;
            byte[] bytes = null;
            List<(byte[], int)> byts = new List<(byte[], int)>();
            while (bytesRec < bufLength)
            {
                bytes = new byte[bufLength];
                bytesRec += clientSocket.Receive(bytes);
                int dat = bytesRec;
                if (byts.Count > 0)
                {
                    dat = bytesRec - byts[byts.Count - 1].Item2;
                }

                byts.Add((bytes, dat));

            }

            byte[] bytesz = new byte[1024 * 5000];
            for (int i = 0; i < bytesz.Length; i++)
            {
                foreach (var arr in byts)
                {
                    for (int j = 0; j < arr.Item2; j++)
                    {
                        bytesz[i] = arr.Item1[j];
                        i++;
                    }
                }
                break;
            }

            bytesRec = 0;

            BinaryWriter binWriter =
new BinaryWriter(new FileStream(FileName, FileMode.Create)); ; ;
            binWriter.Write(bytesz, 0, bytesz.Length);

            binWriter.Close();



            //  handleRequest(RequestType.Download,FileName);
        }

        public void UploadFile(string FilePath)
        {
           

            ConnectToFTP();


            string[] nameOFfile;
            string fname;
            byte[] nameAs = new byte[4];
            byte[] ddataAsByees = new byte[4];
            
            FileStream stream;
            BinaryReader reader;
            byte[] dataAsByees = new byte[4];

            nameOFfile = FilePath.Split("\\");
            fname = nameOFfile[nameOFfile.Length - 1];
            nameAs = Encoding.ASCII.GetBytes(fname);
            ddataAsByees = File.ReadAllBytes(FilePath);
            stream = File.Open(FilePath, FileMode.Open);
            reader = new BinaryReader(stream);

            dataAsByees = reader.ReadBytes(ddataAsByees.Length);
            if (dataAsByees.Length>bufLength)
            {
                return;
            }

            reader.Close();

            int nextStartPoint = 0;
            byte[] nameSize = BitConverter.GetBytes(nameAs.Length);

            //this is a header for type of request 2=fetch file

            

            byte[] ReqType = BitConverter.GetBytes((int)RequestType.Upload);
            for (int i = 0; i < ReqType.Length; i++)
            {
                BufferSize[nextStartPoint] = ReqType[i];
                nextStartPoint++;
            }

            byte[] ipaddress = m_ep.Address.GetAddressBytes(); //new byte[4];
            for (int i = 0; i < ipaddress.Length; i++)
            {
                BufferSize[nextStartPoint] = ipaddress[i];
                nextStartPoint++;
            }

            for (int i = 0; i < nameSize.Length; i++)
            {
                BufferSize[nextStartPoint] = nameSize[i];
                nextStartPoint++;
            }

            //nextStartPoint += 1;
            for (int i = 0; i < nameAs.Length; i++)
            {
                BufferSize[nextStartPoint] = nameAs[i];
                nextStartPoint++;
            }

          





            byte[] dataAsByedes = BitConverter.GetBytes((dataAsByees.Length / 255));//.ToByte() //Encoding.ASCII.GetBytes();
                                                                                        //setting the size buffer
                                                                                        //nextStartPoint += 1;
                for (int i = 0; i < dataAsByedes.Length; i++)
                {
                    BufferSize[nextStartPoint] = dataAsByedes[i];
                    nextStartPoint++;
                }
                byte[] Remainder = BitConverter.GetBytes((dataAsByees.Length % 255));
                for (int i = 0; i < Remainder.Length; i++)
                {
                    BufferSize[nextStartPoint] = Remainder[i];
                    nextStartPoint++;
                }

            for (int i = 0; i < dataAsByees.Length; i++)
            {
                BufferSize[nextStartPoint] = dataAsByees[i];
                nextStartPoint++;
            }

            const byte ClosingChar = (byte)('$');

            while (nextStartPoint< BufferSize.Length)
            {
                BufferSize[nextStartPoint] = ClosingChar;
                nextStartPoint++;
            }


            clientSocket.Send(BufferSize);

           // DisconnectFromFTP();
        }


        public void UploadFolder(string FolderPath)
        {
            Directory_S directory_S = new Directory_S(FolderPath);
            getDirectotiresRecoursevly(ref directory_S);
        }
        private void FillFileSize(ref File_S file)
        {
            FileStream stream;
            BinaryReader reader;


            file.FileSize = File.ReadAllBytes(file.FilePath);
            


             stream = File.OpenRead(file.FilePath);
            reader = new BinaryReader(stream);

            file.FileSize = reader.ReadBytes(file.FileSize.Length);


            reader.Close();



        }
        private void getDirectotiresRecoursevly(ref Directory_S folder)
        {
            string[] files = Directory.GetFiles(folder.dirPath);

            if (files.Length > 0)
            {
                Console.WriteLine("in folder {0} there are {1} files", folder.dirPath, files.Length);
                foreach (var file in files)
                {
                    
                    File_S fileObj = new File_S(file);
                    FillFileSize(ref fileObj);
                    folder.files.Add(fileObj);
                    Console.WriteLine("\t{0}", file);
                }
            }



            var dirs = Directory.GetDirectories(folder.dirPath);

            if (dirs.Length > 0)
            {
                Console.WriteLine("in folder {0} there are {1} directories", folder.dirPath, dirs.Length);
                foreach (var directory in dirs)
                {
                    Directory_S directory_ = new Directory_S(directory);


                    Console.WriteLine("\t{0}directory:", directory);

                    getDirectotiresRecoursevly(ref directory_);

                    folder.directories.Add(directory_);




                }
            }



        }




        class Directory_S
        {
            public string dirPath { get; }
            public string dirname { get; }
            public List<Directory_S> directories;
            public List<File_S> files;

            public Directory_S(string dirPath)
            {
                this.dirPath = dirPath;
                string[] splitter = dirPath.Split("\\");
                dirname = splitter[splitter.Length - 1];


                if (directories == null)
                {
                    directories = new List<Directory_S>();
                }
                if (files == null)
                {
                    files = new List<File_S>();
                }

            }

        }

         class File_S
        {
           public string nameFile { get; }
            public string FilePath { get; }
            public byte[] FileSize;
            public File_S(string FilePath) 
            {
                this.FilePath = FilePath;
                string[] splitter = FilePath.Split("\\");
                nameFile = splitter[splitter.Length - 1];
                
            }
        }


    }
   
}
