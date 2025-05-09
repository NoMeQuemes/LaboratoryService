﻿using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LaboratoryService_Api.Utilities
{
    public class TcpClient
    {
        private readonly string _serverIp;
        private readonly int _serverPort;
        private System.Net.Sockets.TcpClient _client;
        private NetworkStream _stream;
        private bool _isListening;

        public TcpClient(string serverIp, int serverPort)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            Connect();
        }

        private void Connect()
        {
            try
            {
                _client = new System.Net.Sockets.TcpClient(_serverIp, _serverPort);
                _stream = _client.GetStream();
                _isListening = true;
                Thread listenerThread = new Thread(ListenForResponses);
                listenerThread.IsBackground = true;
                listenerThread.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error connecting to server: {ex.Message}");
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                if (_client == null || !_client.Connected)
                {
                    Debug.WriteLine("Reconnecting...");
                    Connect();
                }

                byte[] data = Encoding.ASCII.GetBytes(message);
                _stream.Write(data, 0, data.Length);

                // Iniciar la escucha después de enviar un mensaje
                Task.Run(() => ListenForResponses());

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        private void ListenForResponses()
        {
            try
            {
                byte[] buffer = new byte[1024];
                while (_isListening && _client.Connected)
                {
                    if (_stream.DataAvailable) // Verificar si hay datos antes de leer
                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            Console.WriteLine($"Received: {response}");
                            Debug.WriteLine($"Received: {response}");
                        }
                    }
                    Thread.Sleep(100); // Pequeño delay para evitar uso excesivo de CPU
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving response: {ex.Message}");
                Debug.WriteLine($"Error receiving response: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            _isListening = false;
            _stream?.Close();
            _client?.Close();
        }
    }
}
