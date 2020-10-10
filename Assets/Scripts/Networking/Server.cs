﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using System.Linq;

namespace Networking {
    public class Server {

        public class Connection : IConnection {
            public const int MAX_FRAME_SIZE = 256;

            private readonly Server server;
            public readonly Guid connectionId;
            private readonly Socket socket;
            private readonly ByteFramer byteFramer;
            private readonly ByteSender byteSender;

            public ConnectionStatus Status { get; private set; } = ConnectionStatus.CONNECTED;

            public Socket GetSocket() {
                return socket;
            }

            public string GetConnectedWithIdentifier() {
                return string.Format("connection-{0}", connectionId.ToString());
            }

            public bool IsConnected() {
                return Status == ConnectionStatus.CONNECTED;
            }

            public Connection(Server server, Guid connectionId, Socket socket) {
                this.server = server;
                this.connectionId = connectionId;
                this.socket = socket;

                byteFramer = new ByteFramer(MAX_FRAME_SIZE, (byte[] bytes) => {
                    server.OnFrameReceived(connectionId, bytes);
                });

                new ByteReceiver(this, (bytes) => {
                    byteFramer.Append(bytes);
                });

                byteSender = new ByteSender(this);
            }

            public void Disconnect() {
                if (Status == ConnectionStatus.DISCONNECTED) {
                    Debug.LogError(string.Format("Server cannot disconnect connection with connection {0} when it has already been disconnected", connectionId));
                    return;
                }
                Debug.Log(string.Format("Disconnecting connection {0}", connectionId));
                Status = ConnectionStatus.DISCONNECTED;
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                server.OnDisconnected(connectionId);
                Debug.Log(string.Format("Successfully disconnected connection {0}", connectionId));
            }

            public void Send(byte[] data) {
                byte[] frame = byteFramer.Frame(data);
                byteSender.Send(frame);
            }
        }

        private ManualResetEvent acceptDone = new ManualResetEvent(false);
        private readonly Socket rootSocket;
        private readonly Dictionary<Guid, Connection> connections = new Dictionary<Guid, Connection>();
        private readonly Action<Guid> OnConnected;
        private readonly Action<Guid> OnDisconnected;
        private readonly Action<Guid, byte[]> OnFrameReceived;
        private readonly PacketFactory packetFactory;

        public ServerStatus Status { get; private set; } = ServerStatus.RUNNING;

        public Server(int port, Action<Guid> OnConnected, Action<Guid> OnDisconnected, Action<Guid, Packet> OnPacketReceived) {
            Debug.Log(string.Format("Start of setting up server on port {0}", port));
            packetFactory = PacketFactory.BuildFromAllAssemblies();
            Debug.Log(packetFactory.ToString());

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            ThreadManager.Activate();

            this.OnConnected = (Guid connectionId) => {
                ThreadManager.ExecuteOnMainThread(() => {
                    OnConnected(connectionId);
                });
            };
            this.OnDisconnected = (Guid connectionId) => {
                connections.Remove(connectionId);
                ThreadManager.ExecuteOnMainThread(() => {
                    OnDisconnected(connectionId);
                });
            };
            OnFrameReceived = (Guid connectionId, byte[] bytes) => {
                ThreadManager.ExecuteOnMainThread(() => {
                    OnPacketReceived(connectionId, packetFactory.FromBytes(bytes));
                });
            };
            ;

            rootSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            rootSocket.Bind(localEndPoint);
            rootSocket.Listen(100);

            var thread = new Thread(() => {
                Debug.Log(string.Format("Server is ready for connections on {0}", localEndPoint));
                InitiateAcceptLoop();
            });
            thread.Start();
        }

        public void InitiateAcceptLoop() {
            while (true) {
                acceptDone.Reset();
                if (Status == ServerStatus.SHUTDOWN) {
                    Debug.Log("Breaking out of accepting new connections loop since server has shutdown");
                    break;
                }

                // TODO: When server is full, either stop accepting new connection or allow them and notify them instantly after connection was setup
                rootSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

                acceptDone.WaitOne();
            }
        }

        public void AcceptCallback(IAsyncResult ar) {
            try {
                if (Status == ServerStatus.SHUTDOWN) {
                    Debug.Log("Server cannot handle an accept callback after it was shutdown");
                    return;
                }

                Socket socket = rootSocket.EndAccept(ar);
                Guid connectionId = Guid.NewGuid();
                Debug.Log(string.Format("Accepted incoming connection from {0} and assigned id {1} to the connection", socket.RemoteEndPoint, connectionId));
                Connection connection = new Connection(this, connectionId, socket);
                connections.Add(connectionId, connection);
                OnConnected(connectionId);

            } catch (Exception) {

            } finally {
                acceptDone.Set();
            }
        }

        public void Shutdown() {
            if (Status == ServerStatus.SHUTDOWN) {
                Debug.LogError("Server cannot shutdown when it has already been shutdown");
                return;
            }
            Debug.Log("Shutting down server");
            Status = ServerStatus.SHUTDOWN;
            var connectionIds = connections.Keys.ToList();
            foreach (var connectionId in connectionIds) {
                Connection connection = connections[connectionId];
                connection.Disconnect();
            }
            rootSocket.Close();
            Debug.Log("Server shutdown completed");
        }

        public void Disconnect(Guid connectionId) {
            if (connections.TryGetValue(connectionId, out Connection connection)) {
                connection.Disconnect();
            } else {
                throw new InvalidOperationException(string.Format("Cannot disconnect connection {0} because that connection does not exist", connectionId));
            }
        }

        public void Broadcast(Packet packet) {
            byte[] bytes = packetFactory.GetBytes(packet);
            foreach (var connection in connections.Values) {
                connection.Send(bytes);
            }
        }

        public void Send(Guid connectionId, Packet packet) {
            byte[] bytes = packetFactory.GetBytes(packet);
            if (connections.TryGetValue(connectionId, out Connection connection)) {
                connection.Send(bytes);
            } else {
                throw new InvalidOperationException(string.Format("Cannot send a message to connection {0} because that connection does not exist", connectionId));
            }
        }
    }
}