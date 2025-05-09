using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NLog;

namespace LaboratoryService_Api.Utilities
{
    public class TcpServer
    {
        private readonly int _listeningPort;
        private TcpListener _listener;
        private bool _isRunning;
        private static  readonly Logger logger = LogManager.GetCurrentClassLogger();

        public TcpServer(int port)
        {
            _listeningPort = port;
        }

        public void StartListening()
        {
            _listener = new TcpListener(IPAddress.Any, _listeningPort);
            _listener.Start();
            _isRunning = true;

            Thread listenerThread = new Thread(() =>
            {
                while (_isRunning)
                {
                    if (_listener.Pending())
                    {
                        var client = _listener.AcceptTcpClient();
                        ThreadPool.QueueUserWorkItem(HandleClient, client);
                    }
                    Thread.Sleep(100);
                }
            });
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }

        private void HandleClient(object obj)
        {
            var client = obj as System.Net.Sockets.TcpClient;
            if (client == null) return;

            try
            {
                using NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];

                while (client.Connected)
                {
                    if (stream.DataAvailable)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            Console.WriteLine($"[SERVIDOR] Mensaje recibido: {receivedMessage}");
                            Debug.WriteLine($"[SERVIDOR] Mensaje recibido: {receivedMessage}");
                            logger.Info($"Mensaje ercibido: {receivedMessage}");
                            logger.Debug("Acá llegó el mensaje");
                            // Aquí puedes manejar el mensaje recibido como quieras
                            string responseMessage = "Mensaje recibido correctamente";
                            byte[] responseData = Encoding.ASCII.GetBytes(responseMessage);
                            stream.Write(responseData, 0, responseData.Length);
                        }
                    }

                    Thread.Sleep(100); // Evita uso excesivo de CPU
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SERVIDOR] Error en cliente: {ex.Message}");
            }
            finally
            {
                client.Close();
                Debug.WriteLine("[SERVIDOR] Cliente desconectado");
            }
        }

        public void StopListening()
        {
            _isRunning = false;
            _listener?.Stop();
        }
    }
}
