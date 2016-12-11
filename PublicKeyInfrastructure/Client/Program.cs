﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ServiceModel;
using Common.Client;
using System.Security.Principal;
using Cryptography.AES;
using Common.Proxy;
using Client.Database;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            Console.CancelKeyPress += CurrentDomain_ProcessExit;

            Console.WriteLine("Client node\n\n");
            Console.Write("Host service port: ");
            string port = Console.ReadLine();
            string address = string.Format("net.tcp://localhost:{0}/Client", port);
            IDatabaseWrapper dbWrapper = new SQLiteWrapper();

            ServiceHost host = new ServiceHost(new ClientService(address, dbWrapper));
            NetTcpBinding binding = new NetTcpBinding();
            binding.SendTimeout = new TimeSpan(0, 5, 5);
            binding.ReceiveTimeout = new TimeSpan(0, 5, 5);
            binding.OpenTimeout = new TimeSpan(0, 5, 5);
            binding.CloseTimeout = new TimeSpan(0, 5, 5);

            host.AddServiceEndpoint(typeof(IClientContract), binding, address);
                    
            host.Open();
            Console.WriteLine("Service is started...");


            ClientProxy proxy = new ClientProxy(
            new EndpointAddress(string.Format("net.tcp://localhost:{0}/Client", port)),
            new NetTcpBinding(), new ClientService());

            while(true)
            {
                Console.WriteLine("\n1.Connect to other client");
                Console.WriteLine("2.Send message");
                Console.WriteLine("3.Connected to...");
                Console.WriteLine("4.End");

                string option = Console.ReadLine();
                if (option.Equals("4")) break;

                switch(option)
                {
                    case "1":
                        Console.WriteLine("Client port:");
                        string clientAddress = Console.ReadLine();
                        proxy.StartComunication(clientAddress);

                        break;

                    case "2":
                        Console.WriteLine("Address: ");
                        string clientAddres = Console.ReadLine();
                        Console.WriteLine("Message: ");
                        string message = Console.ReadLine();

                        proxy.CallPay(System.Text.Encoding.UTF8.GetBytes(message), clientAddres);

                        break;
                    case "3":
                        Console.WriteLine("Connected Clients:");
                        dbWrapper.ListAllRecordsFromTable();
                        break;
                }
            }

            Console.ReadKey();
            host.Close();
            
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            string address = "net.tcp://localhost:10002/RegistrationAuthorityService";
            using (RAProxy raProxy = new RAProxy(address, binding))
            {
                //caProxy.RemoveMeFromList();
            }
        }
    }
}
