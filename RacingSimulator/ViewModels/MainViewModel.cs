using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using RacingSimulator.Models;
using RacingSimulator.Models.Enums;

namespace RacingSimulator.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private DispatcherTimer updateTimer;
        private ObservableCollection<Car> cars;
        private ObservableCollection<Mechanic> mechanics;
        private ObservableCollection<Loader> loaders;
        private ObservableCollection<RaceEventArgs> raceLog;
        private string selectedCarName;

        public ObservableCollection<Car> Cars
        {
            get => cars;
            set
            {
                cars = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<RaceEventArgs> RaceLog
        {
            get => raceLog;
            set
            {
                raceLog = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCarName
        {
            get => selectedCarName;
            set
            {
                selectedCarName = value;
                OnPropertyChanged();
            }
        }

        // Команды
        public ICommand StartRaceCommand { get; }
        public ICommand StopRaceCommand { get; }
        public ICommand AddCarCommand { get; }
        public ICommand RemoveCarCommand { get; }
        public ICommand ClearLogCommand { get; }

        public MainViewModel()
        {
            Cars = new ObservableCollection<Car>();
            mechanics = new ObservableCollection<Mechanic>();
            loaders = new ObservableCollection<Loader>();
            RaceLog = new ObservableCollection<RaceEventArgs>();

            // Инициализация механиков и погрузчиков
            mechanics.Add(new Mechanic("Иван"));
            mechanics.Add(new Mechanic("Пётр"));
            loaders.Add(new Loader("Эвакуатор-1"));
            loaders.Add(new Loader("Эвакуатор-2"));

            // Команды
            StartRaceCommand = new RelayCommand(StartRace, CanStartRace);
            StopRaceCommand = new RelayCommand(StopRace, CanStopRace);
            AddCarCommand = new RelayCommand(AddCar);
            RemoveCarCommand = new RelayCommand(RemoveCar, CanRemoveCar);
            ClearLogCommand = new RelayCommand(ClearLog);

            // Таймер для обновления UI
            updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            updateTimer.Tick += (s, e) => UpdateUI();
            updateTimer.Start();

            // Добавляем тестовые болиды
            AddCar();
            AddCar();
        }

        private void AddCar()
        {
            var newCar = new Car($"Болид-{Cars.Count + 1}", 1000);
            
            // Подписка на события
            newCar.TireWornOut += OnCarTireWornOut;
            newCar.Crash += OnCarCrash;
            newCar.PositionChanged += OnCarPositionChanged;
            newCar.RaceFinished += OnCarRaceFinished;
            newCar.StateChanged += OnCarStateChanged;
            
            Cars.Add(newCar);
            AddToLog($"Добавлен {newCar.Name}");
        }

        private void RemoveCar()
        {
            if (SelectedCarName != null)
            {
                var car = Cars.FirstOrDefault(c => c.Name == SelectedCarName);
                if (car != null)
                {
                    car.StopRace();
                    Cars.Remove(car);
                    AddToLog($"Удалён {car.Name}");
                }
            }
        }

        private bool CanRemoveCar()
        {
            return SelectedCarName != null && Cars.Any();
        }

        private void StartRace()
        {
            foreach (var car in Cars)
            {
                if (car.State == CarState.Racing || car.State == CarState.Pitting || car.State == CarState.Crashed)
                {
                    car.StartRace();
                }
            }
            AddToLog("ГОНКА НАЧАЛАСЬ!");
        }

        private bool CanStartRace()
        {
            return Cars.Any(c => c.State != CarState.Finished);
        }

        private void StopRace()
        {
            foreach (var car in Cars)
            {
                car.StopRace();
            }
            AddToLog("ГОНКА ОСТАНОВЛЕНА!");
        }

        private bool CanStopRace()
        {
            return Cars.Any();
        }

        private void ClearLog()
        {
            RaceLog.Clear();
        }

        private void OnCarTireWornOut(object sender, RaceEventArgs e)
        {
            AddToLog(e);
            
            var car = sender as Car;
            if (car != null)
            {
                // Находим свободного механика
                var freeMechanic = mechanics.FirstOrDefault(m => true);
                freeMechanic?.PerformPitStop(car);
            }
        }

        private void OnCarCrash(object sender, RaceEventArgs e)
        {
            AddToLog(e);
            
            var car = sender as Car;
            if (car != null)
            {
                // Используем рефлексию для получения информации о типе аварии
                var crashType = e.GetType();
                var messageProperty = crashType.GetProperty("Message");
                var crashMessage = messageProperty?.GetValue(e)?.ToString() ?? "Неизвестная авария";
                
                AddToLog(new RaceEventArgs(car.Name, $"Анализ аварии: {crashMessage}", car.Distance));
                
                // Находим свободный погрузчик
                var freeLoader = loaders.FirstOrDefault(l => true);
                freeLoader?.LoadCar(car);
            }
        }

        private void OnCarPositionChanged(object sender, RaceEventArgs e)
        {
            // Обновляем UI без логирования каждой позиции
            OnPropertyChanged(nameof(Cars));
        }

        private void OnCarRaceFinished(object sender, RaceEventArgs e)
        {
            AddToLog(e);
        }

        private void OnCarStateChanged(object sender, RaceEventArgs e)
        {
            AddToLog(e);
            OnPropertyChanged(nameof(Cars));
        }

        private void AddToLog(RaceEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                RaceLog.Insert(0, e);
                if (RaceLog.Count > 100)
                    RaceLog.RemoveAt(RaceLog.Count - 1);
            });
        }

        private void AddToLog(string message)
        {
            AddToLog(new RaceEventArgs("Система", message, 0));
        }

        private void UpdateUI()
        {
            OnPropertyChanged(nameof(Cars));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute();
        }

        public void Execute(object parameter)
        {
            execute();
        }
    }
}