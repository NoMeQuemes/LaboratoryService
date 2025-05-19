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
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
                        Debug.WriteLine($"Cliente conectado desde: {((IPEndPoint)client.Client.RemoteEndPoint).Address}:{((IPEndPoint)client.Client.RemoteEndPoint).Port}");
                        logger.Info($"Cliente conectado desde: {((IPEndPoint)client.Client.RemoteEndPoint).Address}:{((IPEndPoint)client.Client.RemoteEndPoint).Port}");
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
            var client = obj as System.Net.Sockets.TcpClient; ;
            if (client == null) return;

            try
            {
                using NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                StringBuilder messageBuilder = new();
                bool messageStarted = false;

                while (client.Connected)
                {
                    if (stream.DataAvailable)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            bool invalidMessageDetected = false;

                            for (int i = 0; i < bytesRead; i++)
                            {
                                byte currentByte = buffer[i];

                                if (currentByte == 0x0B) // Inicio de mensaje MLLP
                                {
                                    messageStarted = true;
                                    messageBuilder.Clear();
                                }
                                else if (currentByte == 0x1C) // Fin de mensaje
                                {
                                    if (i + 1 < bytesRead && buffer[i + 1] == 0x0D)
                                    {
                                        string hl7Message = messageBuilder.ToString();
                                        Debug.WriteLine($"Servidor: mensaje HL7 recibido:\n{hl7Message}");
                                        logger.Info($"Servidor: mensaje HL7 recibido: {hl7Message}");

                                        string idHl7Message = ConvertHL7.ObtenerMessageControlId(hl7Message);

                                        //  Construcción del mensaje ACK
                                        string fechaActual = DateTime.Now.ToString("yyyyMMddHHmmss");
                                        string MSH = $"MSH|^~\\&|HOSTStandardHL7^5.2.0||LIS||{fechaActual}||ACK|MSG00001|P|2.5\x0D";
                                        string MSA = $"MSA|CA|{idHl7Message}\x0D";

                                        var ackBuilder = new StringBuilder();
                                        ackBuilder.Append(MSH)
                                                  .Append(MSA);

                                        string ackMessage = ackBuilder.ToString();

                                        string mllpAck = $"{(char)0x0B}{ackMessage}{(char)0x1C}{(char)0x0D}";
                                        byte[] ackBytes = Encoding.ASCII.GetBytes(mllpAck);
                                        stream.Write(ackBytes, 0, ackBytes.Length);

                                        messageStarted = false;
                                        i++; // Saltar 0x0D
                                    }
                                }
                                else if (messageStarted)
                                {
                                    messageBuilder.Append((char)currentByte);
                                }
                                else
                                {
                                    // Si no se ha detectado el comienzo correcto (0x0B), y llega otro byte, es inválido
                                    invalidMessageDetected = true;
                                    break; // Detenemos el procesamiento de este bloque
                                }
                            }

                            if (invalidMessageDetected)
                            {
                                string errorMessage = "ERROR|Mensaje no válido\r";
                                string mllpError = $"{(char)0x0B}{errorMessage}{(char)0x1C}{(char)0x0D}";
                                byte[] errorBytes = Encoding.ASCII.GetBytes(mllpError);
                                stream.Write(errorBytes, 0, errorBytes.Length);
                                Debug.WriteLine("Se recibió un mensaje que no cumple con el protocolo MLLP. Se envió respuesta de error.");
                                logger.Info("Se recibió un mensaje que no cumple con el protocolo MLLP. Se envió respuesta de error.");
                            }
                        }
                    }

                    Thread.Sleep(50); // reduce el uso excesivo de la CPU
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SERVIDOR MLLP] Error en cliente: {ex.Message}");
                logger.Error($"Error en el cliente: {ex}");
            }
            finally
            {
                IPEndPoint remoteEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                client.Close();
                Debug.WriteLine($"Cliente desconectado desde: {remoteEndPoint.Address}:{remoteEndPoint.Port}");
                logger.Info($"Cliente desconectado desde: {remoteEndPoint.Address}:{remoteEndPoint.Port}");
            }
        }

        public void StopListening()
        {
            _isRunning = false;
            _listener?.Stop();
            Debug.WriteLine("El servidor se detuvo");
            logger.Info("El servidor se detuvo");
        }
    }
}
