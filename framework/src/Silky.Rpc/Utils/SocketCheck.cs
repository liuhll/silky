using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Rpc.Endpoint.Monitor;

namespace Silky.Rpc.Utils
{
    public static class SocketCheck
    {
        private static ILogger<IRpcEndpointMonitor> _logger = NullLogger<IRpcEndpointMonitor>.Instance;

        public static bool TestConnection(string host, int port, int millisecondsTimeout = 500)
        {
            Socket socket = null;
            try
            {
                var timeoutObject = new ManualResetEvent(false);
                timeoutObject.Reset();
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ar = socket.BeginConnect(host, port, null, null);
                ar.AsyncWaitHandle.WaitOne(millisecondsTimeout);
                return socket.Connected;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{host}: {port} connection is abnormal, reason: {ex.Message}");
                return false;
            }
            finally
            {
                if (socket != null)
                {
                    socket.Dispose();
                }
            }
        }

        public static bool TestConnection(IPEndPoint ipEndPoint, int millisecondsTimeout = 500)
        {
            Socket socket = null;
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ar = socket.BeginConnect(ipEndPoint, null, null);
                ar.AsyncWaitHandle.WaitOne(millisecondsTimeout);
                return socket.Connected;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ipEndPoint}Connection abnormal, reason=>{ex.Message}");
                return false;
            }
            finally
            {
                if (socket != null)
                {
                    socket.Dispose();
                }
            }
        }

        public static bool TestConnection(IPAddress iPAddress, int port, int millisecondsTimeout = 500)
        {
            Socket socket = null;
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ar = socket.BeginConnect(iPAddress, port, null, null);
                ar.AsyncWaitHandle.WaitOne(millisecondsTimeout);
                return socket.Connected;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{iPAddress.ToString()}:{port}Connection abnormal, reason=>{ex.Message}");
                return false;
            }
            finally
            {
                if (socket != null)
                {
                    socket.Dispose();
                }
            }
        }
    }
}