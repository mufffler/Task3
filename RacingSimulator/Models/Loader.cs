Actionusing System;

namespace RacingSimulator.Models
{
    /// <summary>
    /// Интерфейс погрузчика
    /// </summary>
    public interface ILoader
    {
        string Name { get; }
        void LoadCar(Car car);
        event EventHandler<RaceEventArgs> LoadCompleted;
    }

    /// <summary>
    /// Реализация погрузчика (эвакуатор)
    /// </summary>
    public class Loader : ILoader
    {
        private Random random = new Random();
        private bool isBusy = false;

        public string Name { get; private set; }

        public event EventHandler<RaceEventArgs> LoadCompleted;

        public Loader(string name)
        {
            Name = name;
        }

        public void LoadCar(Car car)
        {
            if (isBusy)
            {
                OnLoadCompleted(new RaceEventArgs(car.Name, "Погрузчик занят, ожидайте...", car.Distance));
                return;
            }

            isBusy = true;
            
            // Используем рефлексию для получения информации о болиде
            var carType = car.GetType();
            var properties = carType.GetProperties();
            string carInfo = "";
            foreach (var prop in properties)
            {
                if (prop.Name == "Name" || prop.Name == "Speed" || prop.Name == "Distance")
                {
                    carInfo += $"{prop.Name}: {prop.GetValue(car)}, ";
                }
            }

            OnLoadCompleted(new RaceEventArgs(car.Name, 
                $"Погрузчик {Name} начинает эвакуацию. Инфо: {carInfo}", 
                car.Distance));

            // Симуляция погрузки
            System.Threading.Thread.Sleep(2000);
            
            car.CrashRecovered();
            
            OnLoadCompleted(new RaceEventArgs(car.Name, 
                $"Погрузчик {Name} завершил эвакуацию. Болид возвращается в гонку!", 
                car.Distance));
            
            isBusy = false;
        }

        protected virtual void OnLoadCompleted(RaceEventArgs e)
        {
            LoadCompleted?.Invoke(this, e);
        }
    }
}