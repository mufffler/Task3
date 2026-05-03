using System;
using System.Threading;

namespace RacingSimulator.Models
{
    /// <summary>
    /// Класс механика
    /// </summary>
    public class Mechanic
    {
        private Random random = new Random();
        private bool isBusy = false;
        private Timer pitStopTimer;

        public string Name { get; private set; }

        public event EventHandler<RaceEventArgs> PitStopStarted;
        public event EventHandler<RaceEventArgs> PitStopCompleted;

        public Mechanic(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Выполнить пит-стоп (замена колёс)
        /// </summary>
        public void PerformPitStop(Car car)
        {
            if (isBusy)
            {
                OnPitStopStarted(new RaceEventArgs(car.Name, 
                    $"Механик {Name} занят другим болидом!", car.Distance));
                return;
            }

            isBusy = true;
            
            OnPitStopStarted(new RaceEventArgs(car.Name, 
                $"Механик {Name} начал замену колёс", car.Distance));

            // Симуляция времени пит-стопа (1-3 секунды)
            int pitTime = random.Next(1000, 3000);
            
            pitStopTimer = new Timer(_ =>
            {
                car.CompletePitStop();
                
                OnPitStopCompleted(new RaceEventArgs(car.Name, 
                    $"Механик {Name} завершил замену колёс за {pitTime/1000.0:F1}с", 
                    car.Distance));
                
                isBusy = false;
                pitStopTimer?.Dispose();
            }, null, pitTime, Timeout.Infinite);
        }

        protected virtual void OnPitStopStarted(RaceEventArgs e)
        {
            PitStopStarted?.Invoke(this, e);
        }

        protected virtual void OnPitStopCompleted(RaceEventArgs e)
        {
            PitStopCompleted?.Invoke(this, e);
        }
    }
}