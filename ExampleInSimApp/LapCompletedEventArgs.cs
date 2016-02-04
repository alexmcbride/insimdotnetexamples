using System;

namespace ExampleInSimApp {
    public class LapCompletedEventArgs : EventArgs {
        public string PlayerName { get; private set; }
        public string Track { get; private set; }
        public string Car { get; private set; }
        public int Laps { get; private set; }
        public TimeSpan LapTime { get; private set; }
        public TimeSpan[] Splits { get; private set; }

        public LapCompletedEventArgs(string playerName, string track, string car, int laps, TimeSpan lapTime, TimeSpan[] splits) {
            PlayerName = playerName;
            Track = track;
            Car = car;
            Laps = laps;
            LapTime = lapTime;
            Splits = splits;
        }
    }
}
