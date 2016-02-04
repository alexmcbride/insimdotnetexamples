using InSimDotNet;
using InSimDotNet.Helpers;
using InSimDotNet.Packets;
using System;
using System.Collections.Generic;

namespace ExampleInSimApp {
    public class RaceControl {
        private InSim insim;
        private Dictionary<byte, Player> players;
        private string currentTrack;

        public event EventHandler<InitializeEventArgs> Initialized {
            add { insim.Initialized += value; }
            remove { insim.Initialized -= value; }
        }

        public event EventHandler<DisconnectedEventArgs> Disconnected {
            add { insim.Disconnected += value; }
            remove { insim.Disconnected -= value; }
        }

        public event EventHandler<InSimErrorEventArgs> InSimError {
            add { insim.InSimError += value; }
            remove { insim.InSimError -= value; }
        }

        public event EventHandler<LapCompletedEventArgs> LapCompleted;

        public bool IsConnected {
            get { return insim.IsConnected; }
        }

        public RaceControl() {
            insim = new InSim();

            // bind packet events.
            insim.Bind<IS_ISM>(HandlePacket);
            insim.Bind<IS_STA>(HandlePacket);
            insim.Bind<IS_NPL>(HandlePacket);
            insim.Bind<IS_PLL>(HandlePacket);
            insim.Bind<IS_LAP>(HandlePacket);
            insim.Bind<IS_SPX>(HandlePacket);

            players = new Dictionary<byte, Player>(32);
        }

        public void Initialize(string host, int port, string admin) {
            // connect to LFS
            insim.Initialize(new InSimSettings {
                Host = host,
                Port = port,
                Admin = admin,
                IName = "^3LapTimes",
            });

            // request ISM multiplayer packet be sent
            insim.Send(new IS_TINY { ReqI = 1, SubT = TinyType.TINY_ISM });
        }

        public void Disconnect() {
            insim.Disconnect();
        }

        private void HandlePacket(InSim insim, IS_ISM packet) {
            // joined multiplayer host, request players and state be sent
            insim.Send(new[] {
                new IS_TINY { ReqI = 1, SubT= TinyType.TINY_NPL }, // request player packets
                new IS_TINY { ReqI = 1, SubT= TinyType.TINY_SST } // request state packet
            });
        }

        private void HandlePacket(InSim insim, IS_STA packet) {
            // occurs when LFS state changes, store current track
            currentTrack = TrackHelper.GetFullTrackName(packet.Track);
        }

        private void HandlePacket(InSim insim, IS_NPL packet) {
            // add new player to player list
            players[packet.PLID] = new Player(
                packet.PLID,
                StringHelper.StripColors(packet.PName),
                CarHelper.GetFullCarName(packet.CName));
        }

        private void HandlePacket(InSim insim, IS_PLL packet) {
            // remove player from list
            players.Remove(packet.PLID);
        }

        private void HandlePacket(InSim insim, IS_SPX packet) {
            // get player from players list and add new split
            var player = players[packet.PLID];

            player.AddSplit(packet.Split, packet.STime);
        }

        private void HandlePacket(InSim insim, IS_LAP packet) {
            // player has completed lap, get player from list, and raise lapcompleted event
            var player = players[packet.PLID];

            var e = new LapCompletedEventArgs(
                player.PlayerName,
                currentTrack,
                player.Car,
                packet.LapsDone,
                packet.LTime,
                player.GetSplits());

            OnLapCompleted(e);

            player.ClearSplits(); // do this last
        }

        protected virtual void OnLapCompleted(LapCompletedEventArgs e) {
            var temp = LapCompleted;
            if (temp != null) {
                temp(this, e);
            }
        }
    }
}
