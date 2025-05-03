using Hotel.Data;
using Hotel.Models.Entities;
using Microsoft.EntityFrameworkCore;
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
    /// Логика взаимодействия для ServicesManagementWindow.xaml
    /// </summary>
    public partial class ServicesManagementWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public ServicesManagementWindow()
        {
            InitializeComponent();
            LoadServices();
        }

        private void LoadServices()
        {
            _context.Services
                .Include(s => s.Serviceorders)
                .Load();

            ServicesListView.ItemsSource = _context.Services.Local.ToObservableCollection();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void AddService_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new ServiceEditWindow(new Service());
            if (editWindow.ShowDialog() == true)
            {
                _context.Services.Add(editWindow.CurrentService);
                _context.SaveChanges();
                LoadServices();
            }
        }

        private void EditService_Click(object sender, RoutedEventArgs e)
        {
            if (ServicesListView.SelectedItem is Service selectedService)
            {
                var editWindow = new ServiceEditWindow(selectedService);
                if (editWindow.ShowDialog() == true)
                {
                    _context.SaveChanges();
                    LoadServices();
                }
            }
            else
            {
                MessageBox.Show("Выберите услугу для редактирования");
            }
        }

        private void DeleteService_Click(object sender, RoutedEventArgs e)
        {
            if (ServicesListView.SelectedItem is Service selectedService)
            {
                if (MessageBox.Show($"Удалить услугу '{selectedService.ServiceName}'?",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _context.Services.Remove(selectedService);
                    _context.SaveChanges();
                    LoadServices();
                }
            }
        }
    }
}
