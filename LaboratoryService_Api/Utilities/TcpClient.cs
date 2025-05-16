using NLog;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LaboratoryService_Api.Utilities
{
    public class TcpClient
    {
        private readonly string _serverIp;
        private readonly int _serverPort;
        private System.Net.Sockets.TcpClient _client;
        private NetworkStream _stream;
        private bool _isListening;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public TcpClient(string serverIp, int serverPort)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            Connect();
        }

        private bool Connect()
        {
            try
            {
                _client = new System.Net.Sockets.TcpClient(_serverIp, _serverPort);
                _stream = _client.GetStream();
                _isListening = true;

                Thread listenerThread = new Thread(ListenForResponses)
                {
                    IsBackground = true
                };
                listenerThread.Start();

                Debug.WriteLine("Conexión con el servidor exitosa");
                logger.Info("Conexión con el servidor exitosa");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al conectar con el servidor: {ex.Message}");
                logger.Error($"Error al conectar con el servidor: {ex.Message}");
                _stream = null;
                return false;
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                if (_client == null || !_client.Connected)
                {
                    Debug.WriteLine("Cliente desconectado. Intentando reconectar...");
                    logger.Info("Cliente desconectado. Intentando reconectar...");

                    if (!Connect())
                    {
                        Debug.WriteLine("No se pudo reconectar al servidor.");
                        logger.Error("No se pudo reconectar al servidor.");
                        return;
                    }
                }

                if (_stream == null)
                {
                    Debug.WriteLine("Stream no disponible. No se puede enviar el mensaje.");
                    logger.Error("Stream no disponible. No se puede enviar el mensaje.");
                    return;
                }

                byte[] data = Encoding.ASCII.GetBytes(message);
                _stream.Write(data, 0, data.Length);

                // Iniciar escucha en segundo plano (opcional, ya está en Connect)
                Task.Run(() => ListenForResponses());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al enviar el mensaje: {ex.Message}");
                logger.Error($"Error al enviar el mensaje: {ex.Message}");
            }
        }

        private void ListenForResponses()
        {
            try
            {
                byte[] buffer = new byte[1024];
                while (_isListening && _client != null && _client.Connected)
                {
                    if (_stream != null && _stream.DataAvailable)
                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            Debug.WriteLine($"Respuesta recibida desde el servidor: {response}");
                            logger.Info($"Respuesta recibida desde el servidor: {response}");
                        }
                    }
                    Thread.Sleep(100); // Evitar uso excesivo de CPU
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al recibir la respuesta: {ex.Message}");
                logger.Error($"Error al recibir la respuesta: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            _isListening = false;
            try
            {
                _stream?.Close();
                _client?.Close();
                Debug.WriteLine("Desconectado del servidor.");
                logger.Info("Desconectado del servidor.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cerrar la conexión: {ex.Message}");
                logger.Error($"Error al cerrar la conexión: {ex.Message}");
            }
        }
    }
}
