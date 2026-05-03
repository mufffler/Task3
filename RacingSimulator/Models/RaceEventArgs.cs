using System;

namespace RacingSimulator.Models
{
    /// <summary>
    /// Аргументы событий гонки
    /// </summary>
    public class RaceEventArgs : EventArgs
    {
        public string CarName { get; set; }
        public string Message { get; set; }
        public DateTime EventTime { get; set; }
        public double Distance { get; set; }

        public RaceEventArgs(string carName, string message, double distance)
        {
            CarName = carName;
            Message = message;
            EventTime = DateTime.Now;
            Distance = distance;
        }

        public override string ToString()
        {
            return $"[{EventTime:HH:mm:ss.fff}] {CarName}: {Message} (Дистанция: {Distance:F1}м)";
        }
    }
}