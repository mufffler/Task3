using System;
using System.Threading;
using System.Threading.Tasks;
using RacingSimulator.Models.Enums;

namespace RacingSimulator.Models
{
    /// <summary>
    /// Класс болида
    /// </summary>
    public class Car
    {
        private Random random = new Random();
        private CancellationTokenSource cancellationTokenSource;
        private Task raceTask;
        
        // Свойства болида
        public string Name { get; private set; }
        public double Speed { get; private set; } // м/с
        public double Distance { get; private set; } // пройденная дистанция в метрах
        public double TotalDistance { get; private set; } // общая дистанция гонки
        public CarState State { get; private set; }
        public int TireWear { get; private set; } // износ шин 0-100
        public bool IsRacing => State == CarState.Racing;

        // События
        public event EventHandler<RaceEventArgs> TireWornOut;    // Стерлись покрышки
        public event EventHandler<RaceEventArgs> Crash;          // Столкновение
        public event EventHandler<RaceEventArgs> PositionChanged; // Изменение позиции
        public event EventHandler<RaceEventArgs> RaceFinished;    // Финиш
        public event EventHandler<RaceEventArgs> StateChanged;    // Изменение состояния

        public Car(string name, double totalDistance = 1000)
        {
            Name = name;
            TotalDistance = totalDistance;
            Speed = 20 + random.NextDouble() * 15; // 20-35 м/с
            Distance = 0;
            State = CarState.Racing;
            TireWear = 0;
        }

        /// <summary>
        /// Старт гонки
        /// </summary>
        public void StartRace()
        {
            if (raceTask != null && !raceTask.IsCompleted)
                return;

            cancellationTokenSource = new CancellationTokenSource();
            State = CarState.Racing;
            OnStateChanged(new RaceEventArgs(Name, "Старт гонки!", Distance));
            
            raceTask = Task.Run(() => RaceLoop(cancellationTokenSource.Token), cancellationTokenSource.Token);
        }

        /// <summary>
        /// Остановить гонку
        /// </summary>
        public void StopRace()
        {
            cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Основной цикл гонки
        /// </summary>
        private async Task RaceLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && State == CarState.Racing && Distance < TotalDistance)
            {
                // Движение
                double step = Speed * 0.1; // шаг 100мс
                Distance += step;
                
                // Износ шин увеличивается
                TireWear += random.Next(1, 5);
                
                OnPositionChanged(new RaceEventArgs(Name, $"Скорость: {Speed:F1} м/с, Износ шин: {TireWear}%", Distance));

                // Проверка на износ шин (вероятность 30% при износе > 80)
                if (TireWear > 80 && random.NextDouble() < 0.3)
                {
                    State = CarState.Pitting;
                    OnStateChanged(new RaceEventArgs(Name, "Износ шин критический! Заезд в боксы!", Distance));
                    OnTireWornOut(new RaceEventArgs(Name, "Покрышки стерлись! Нужна замена!", Distance));
                    break;
                }

                // Проверка на столкновение (вероятность 5% на каждом шаге)
                if (random.NextDouble() < 0.05)
                {
                    State = CarState.Crashed;
                    OnStateChanged(new RaceEventArgs(Name, "АВАРИЯ! Столкновение!", Distance));
                    OnCrash(new RaceEventArgs(Name, "Произошло столкновение! Вызван погрузчик.", Distance));
                    break;
                }

                // Проверка финиша
                if (Distance >= TotalDistance)
                {
                    State = CarState.Finished;
                    Distance = TotalDistance;
                    OnStateChanged(new RaceEventArgs(Name, $"ФИНИШ! Время: {DateTime.Now:HH:mm:ss}", Distance));
                    OnRaceFinished(new RaceEventArgs(Name, "Гонка завершена!", Distance));
                    break;
                }

                await Task.Delay(100, cancellationToken);
            }
        }

        /// <summary>
        /// Завершение пит-стопа
        /// </summary>
        public void CompletePitStop()
        {
            if (State == CarState.Pitting)
            {
                TireWear = 0;
                State = CarState.Racing;
                OnStateChanged(new RaceEventArgs(Name, "Пит-стоп завершён! Новые шины установлены.", Distance));
                
                // Возобновляем гонку
                StartRace();
            }
        }

        /// <summary>
        /// Восстановление после аварии (вызвано погрузчиком)
        /// </summary>
        public void CrashRecovered()
        {
            if (State == CarState.Crashed)
            {
                State = CarState.Racing;
                OnStateChanged(new RaceEventArgs(Name, "Авария устранена! Болид возвращается в гонку.", Distance));
                StartRace();
            }
        }

        protected virtual void OnTireWornOut(RaceEventArgs e)
        {
            TireWornOut?.Invoke(this, e);
        }

        protected virtual void OnCrash(RaceEventArgs e)
        {
            Crash?.Invoke(this, e);
        }

        protected virtual void OnPositionChanged(RaceEventArgs e)
        {
            PositionChanged?.Invoke(this, e);
        }

        protected virtual void OnRaceFinished(RaceEventArgs e)
        {
            RaceFinished?.Invoke(this, e);
        }

        protected virtual void OnStateChanged(RaceEventArgs e)
        {
            StateChanged?.Invoke(this, e);
        }
    }
}