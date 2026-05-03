using System.Windows;
using System.Windows.Data;
using System.Globalization;
using RacingSimulator.Models.Enums;
using System.Linq;

namespace RacingSimulator.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }

    // Конвертер для подсчёта гоночных болидов
    public class RacingCountConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Collections.ObjectModel.ObservableCollection<Models.Car> cars)
            {
                return cars.Count(c => c.State == CarState.Racing);
            }
            return 0;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}