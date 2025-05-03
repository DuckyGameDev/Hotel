using Hotel.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Hotel.Windows
{
    /// <summary>
    /// Логика взаимодействия для ServiceEditWindow.xaml
    /// </summary>
    public partial class ServiceEditWindow : Window
    {
        public Service CurrentService { get; set; }
        public List<string> ServiceTypes { get; } = new List<string>
        {
            "SPA", "Ресторан", "Трансфер", "Экскурсия", "Прачечная", "Другое"
        };

        public ServiceEditWindow(Service service)
        {
            InitializeComponent();
            CurrentService = service ?? new Service();
            DataContext = this;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CurrentService.ServiceName))
            {
                MessageBox.Show("Введите название услуги");
                return;
            }

            if (CurrentService.Price <= 0)
            {
                MessageBox.Show("Цена должна быть больше нуля");
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
