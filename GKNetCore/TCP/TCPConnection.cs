﻿/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GKNet.TCP
{
    public class TCPConnection
    {
        protected internal string fId;

        private byte[] fBuffer = new byte[65535];
        private TCPDuplexClient fDuplexClient;
        private Encoding fEncoding = Encoding.UTF8;
        private Socket fSocket;

        public IPEndPoint EndPoint
        {
            get { return (IPEndPoint)fSocket.RemoteEndPoint; }
        }

        public TCPConnection(TCPDuplexClient client, Socket socket, bool receive = true)
        {
            fId = Guid.NewGuid().ToString();
            fDuplexClient = client;
            fDuplexClient.AddConnection(this);

            fSocket = socket;

            // Start listening for incoming data.  (If you want a multi-
            // threaded service, you can start this method up in a separate
            // thread.)

            if (!receive) return;
            BeginReceive();
        }

        // Call this method to set this connection's socket up to receive data.
        private void BeginReceive()
        {
            fSocket.BeginReceive(fBuffer, 0, fBuffer.Length, SocketFlags.None,
                    new AsyncCallback(OnBytesReceived), this);
        }

        // This is the method that is called whenever the socket receives
        // incoming bytes.
        protected void OnBytesReceived(IAsyncResult result)
        {
            try {
                // End the data receiving that the socket has done and get
                // the number of bytes read.
                int nBytesRec = fSocket.EndReceive(result);
                // If no bytes were received, the connection is closed (at
                // least as far as we're concerned).
                if (nBytesRec <= 0) {
                    fSocket.Close();
                    return;
                }

                byte[] data = new byte[nBytesRec];
                Array.Copy(fBuffer, 0, data, 0, nBytesRec);
                fDuplexClient.RaiseDataReceive(data, (IPEndPoint)fSocket.RemoteEndPoint);

                // Whenever you decide the connection should be closed, call 
                // sock.Close() and don't call sock.BeginReceive() again.  But as long 
                // as you want to keep processing incoming data...

                // Set up again to get the next chunk of data.
                fSocket.BeginReceive(fBuffer, 0, fBuffer.Length, SocketFlags.None,
                    new AsyncCallback(OnBytesReceived), this);
            } catch (ObjectDisposedException ex) {

            } catch (SocketException ex) {

            }
        }

        public void Send(byte[] data)
        {
            fSocket.Send(data);
        }

        public void Close()
        {
            if (fSocket != null && fSocket.Connected) {
                fSocket.Shutdown(SocketShutdown.Both);
                fSocket.Close();
            }
            fDuplexClient.RemoveConnection(fId);
        }
    }
}