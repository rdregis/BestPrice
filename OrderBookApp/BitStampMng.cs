using System.Globalization;
using System;
using System.Threading;
using Websocket.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


class BitStampMng
    {
        public BitStampMng()
        {
        }
        public void connect(string url)
        {
            try {
                System.Uri uri = new System.Uri(url);
                this.wsClient = new WebsocketClient(uri);
                this.wsClient.ReconnectTimeout = TimeSpan.FromSeconds(10);
                this.wsClient.ReconnectionHappened.Subscribe(info =>
                    {
                        Console.WriteLine("Reconnection happened, type: " + info.Type);
                        for (int idx = 0; idx < channelSubs.Count ; ++idx) {
                            doSubscribe(channelSubs[idx]); 
                        } 
                    });             
            }
            catch (Exception ex) {
                Console.WriteLine("ERROR: " + ex.ToString());
                System.Environment.Exit(1);
            }
            // System.Console.CursorVisible = true;
            // Console.ReadKey();
        }

        public void subscribe(string channel)
        {
            doSubscribe(channel);
            channelSubs.Add(channel);

        }
        public void doSubscribe(string channel)
        {
            string data = "{ " + "\"" + "event\": \"bts:subscribe\", \"data\": { \"channel\":\"" + channel + "\" }}";
            Console.WriteLine("Subscribe: " +data);

            try {           
                this.wsClient!.Send(data);
            }
            catch (Exception ex) {
                Console.WriteLine("ERROR: " + ex.ToString());
                System.Environment.Exit(1);
            }

        }
         
        public void start(Action<ResponseMessage> handleMessage )
        {
            try {
                var exitEvent = new ManualResetEvent(false);
                this.wsClient!.MessageReceived.Subscribe(handleMessage);

                this.wsClient.Start();
                exitEvent.WaitOne();
            }
            catch (Exception ex) {
                Console.WriteLine("ERROR: " + ex.ToString());
                System.Environment.Exit(1);

            }
        }
        private Websocket.Client.WebsocketClient? wsClient;
        private List<string> channelSubs = new List<string>();
        
       
}