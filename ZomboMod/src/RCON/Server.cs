using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using Zombo.Logging;
using UnityEngine;

namespace ZomboMod.RCON
{
    public class RCONServer : MonoBehaviour
    {
        private static List<RCONConnection> clients = new List<RCONConnection>();
        private TcpListener listener;
        private bool exiting = false;
        private Thread waitingThread;

        public void Awake()
        {
            listener = new TcpListener(IPAddress.Any, R.Settings.Instance.RCON.Port);
            listener.Start();

            // Logger.Log("Waiting for new connection");

            waitingThread = new Thread(() =>
            {
                while (!exiting)
                {
                    RCONConnection newclient = new RCONConnection(listener.AcceptTcpClient());
                    clients.Add(newclient);
                    newclient.Send("ZomboMod Rcon" + Assembly.GetExecutingAssembly().GetName().Version + "\r\n");
                    ThreadPool.QueueUserWorkItem(handleConnection, newclient);
                }
            });
            waitingThread.Start();
        }

        private static void handleConnection(object obj)
        {
            try
            {
                RCONConnection newclient = (RCONConnection)obj;
                string command = "";
                while (newclient.Client.Client.Connected)
                {
                    Thread.Sleep(100);
                    command = newclient.Read();
                    if (command == "") break;
                    command = command.TrimEnd('\n', '\r', ' ');
                    if (command == "quit") break;
                    if (command == "ia")
                    {
                        //newclient.Send("Toggled interactive mode");
                        newclient.Interactive = !newclient.Interactive;
                    }
                    if (command == "") continue;
                    if (command == "Connect")
                    {
                        if (newclient.Authenticated)
                            newclient.Send("You Are Already Login!\r\n");
                        else
                            newclient.Send("Connect <password>");
                        continue;
                    }
                    if (command.Split(' ').Length > 1 && command.Split(' ')[0] == "login")
                    {
                        if (newclient.Authenticated)
                        {
                            newclient.Send("You are Already Login!\r\n");
                            continue;
                        }
                        else
                        {

                            if (command.Split(' ')[1] == R.Settings.Instance.RCON.Password)
                            {
                                newclient.Authenticated = true;
                                //newclient.Send("You have Login!\r\n");
                                //Logger.Log("Client has Login!");
                                continue;
                            }
                            else
                            {
                                newclient.Send("Error: Invalid password!\r\n");
                                Logger.Log("Client has failed to Connect. IP: " + newclient.Client.Client.RemoteEndPoint);
                                break;
                            }
                        }
                    }

                    if (command == "set")
                    {
                        newclient.Send("Syntax: set [option] [value]");
                        continue;
                    }
                    if (!newclient.Authenticated)
                    {
                        newclient.Send("Error: You have not Connected !\r\n");
                        continue;
                    }
                    if (command != "ia")
                        Logger.Log(command);
                    R.Commands.Execute(new ConsolePlayer(), command);
                    command = "";
                }

                clients.Remove(newclient);
                newclient.Send("Good bye!");
                Thread.Sleep(1500);
                Logger.Log("Client has Disconnected! (IP: " + newclient.Client.Client.RemoteEndPoint + ")");
                newclient.Close();

            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static void Broadcast(string message)
        {
            foreach (RCONConnection client in clients)
            {
                if (client.Authenticated)
                    client.Send(message);
            }
        }

        private void OnDestroy()
        {
            exiting = true;
            // Force all connected clients to Disconnect from the server on Restart . will stuck shutdown process until all clients disconnect.
            foreach (RCONConnection client in clients)
            {
                client.Close();
            }
            clients.Clear();
            waitingThread.Abort();
            listener.Stop();
        }

        public static string Read(TcpClient client)
        {
            byte[] _data = new byte[1];
            string data = "";
            NetworkStream _stream = client.GetStream();

            while (true)
            {
                try
                {
                    int k = _stream.Read(_data, 0, 1);
                    if (k == 0)
                        return "";
                    char kk = Convert.ToChar(_data[0]);
                    data += kk;
                    if (kk == '\n')
                        break;
                }
                catch
                {
                    return "";
                }
            }
            return data;
        }

        public static void Send(TcpClient client, string text)
        {
            byte[] data = new UTF8Encoding().GetBytes(text);
            client.GetStream().Write(data, 0, data.Length);
        }
    }
}