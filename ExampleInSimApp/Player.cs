using System;

namespace ExampleInSimApp {
    public class Player {
        private TimeSpan[] splits;

        public byte PlayerId { get; private set; }
        public string Car { get; private set; }
        public string PlayerName { get; private set; }

        public Player(byte playerId, string playerName, string car) {
            PlayerId = playerId;
            PlayerName = playerName;
            Car = car;
            splits = new TimeSpan[3];
        }

        public void AddSplit(int split, TimeSpan time) {
            if (split == 0) {
                throw new ArgumentException("split in incorrect format", "split");
            }

            splits[split - 1] = time;
        }

        public void ClearSplits() {
            for (int i = 0; i < splits.Length; i++) {
                splits[i] = TimeSpan.Zero;
            }
        }

        public TimeSpan[] GetSplits() {
            return (TimeSpan[])splits.Clone();
        }
    }
}
