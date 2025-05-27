using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NLog;
using NHapi.Base.Parser;
using NHapi.Model.V25.Message;
using NHapi.Model.V25.Segment;
using NHapi.Base.Model;

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
            var client = obj as System.Net.Sockets.TcpClient;
            if (client == null) return;

            using (client)
            using (NetworkStream stream = client.GetStream())
            {
                try
                {
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

                                            string idHl7Message = "UNKNOWN";
                                            string respuesta = "";

                                            try
                                            {
                                                string tipoMensaje = ParserHL7.ObtenerTipoMensaje(hl7Message);

                                                PipeParser parser = new PipeParser();
                                                var parsedMsg = parser.Parse(hl7Message);
                                                var msh = parsedMsg.GetStructure("MSH") as MSH;
                                                idHl7Message = msh.MessageControlID.Value;

                                                switch (tipoMensaje)
                                                {
                                                    case "OUL^R22":
                                                        respuesta = ParserHL7.DecodificarOULR22(hl7Message);
                                                        break;
                                                    case "SSU^U03":
                                                        respuesta = ParserHL7.DecodificarSSUUO3(hl7Message);
                                                        break;
                                                    default:
                                                        throw new Exception($"Tipo de mensaje no soportado: {tipoMensaje}");
                                                }

                                                // Enviar ACK
                                                string mllpAck = ParserHL7.ConstruirACK(idHl7Message);
                                                byte[] ackBytes = Encoding.ASCII.GetBytes(mllpAck);
                                                if (client.Connected) stream.Write(ackBytes, 0, ackBytes.Length);
                                            }
                                            catch (Exception ex)
                                            {
                                                logger.Error($"Error procesando mensaje HL7: {ex.Message}");

                                                string nack = ParserHL7.ConstruirACKError(idHl7Message, ex.Message, "AE");
                                                byte[] nackBytes = Encoding.ASCII.GetBytes(nack);
                                                if (client.Connected) stream.Write(nackBytes, 0, nackBytes.Length);
                                            }

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
                                        invalidMessageDetected = true;
                                        break;
                                    }
                                }

                                if (invalidMessageDetected)
                                {
                                    string errorMessage = "ERROR|Mensaje no válido\r";
                                    string mllpError = $"\x0B{errorMessage}\x1C\r";
                                    byte[] errorBytes = Encoding.ASCII.GetBytes(mllpError);

                                    try
                                    {
                                        if (client.Connected)
                                        {
                                            stream.Write(errorBytes, 0, errorBytes.Length);
                                        }
                                    }
                                    catch (ObjectDisposedException)
                                    {
                                        logger.Warn("No se pudo escribir en el stream (error) porque ya fue cerrado.");
                                    }

                                    Debug.WriteLine("Se recibió un mensaje que no cumple con el protocolo MLLP. Se envió respuesta de error.");
                                    logger.Info("Se recibió un mensaje que no cumple con el protocolo MLLP. Se envió respuesta de error.");
                                }
                            }
                        }

                        Thread.Sleep(50);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SERVIDOR MLLP] Error en cliente: {ex.Message}");
                    logger.Error($"Error en el cliente: {ex}");
                }
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
