using MMO_Client.Common.Logger;
using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;

namespace MMO_Client.Client.Net.NetworkManager
{
    class NetworkManager
    {
        private WebSocket websocket;
        private void InitEvents()
        {
            websocket.Opened += new EventHandler(Net_Opened);
            websocket.Error += new EventHandler<ErrorEventArgs>(Net_Error);
            websocket.Closed += new EventHandler(Net_Closed);
            websocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(Net_Recv);
        }
        private void Net_Opened(object sender, EventArgs e)
        {
            Logger.Debug("Websocket open");
        }

        private void Net_Closed(object sender, EventArgs e)
        {
            Logger.Debug("Websocket closed");
        }

        private void Net_Recv(object sender, MessageReceivedEventArgs e)
        {
            Logger.Debug("Websocket recv -> " + e.Message);
        }

        private void Net_Error(object sender, ErrorEventArgs e)
        {
            Logger.Error("Websocket error " + e.ToString());
        }

        public void Connect(string host, int port)
        {
            websocket = new WebSocket("ws://" + host + ":" + port.ToString());
            InitEvents();
            websocket.Open();
            Logger.Debug("Connecting to " + host);
        }
    }
}
