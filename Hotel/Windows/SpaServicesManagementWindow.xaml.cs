using Hotel.Data;
using Hotel.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace Hotel.Windows
{
    public partial class SpaServicesManagementWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public SpaServicesManagementWindow()
        {
            InitializeComponent();
            LoadSpaServices();
        }

        private void LoadSpaServices()
        {
            try
            {
                _context.Spaservices
                    .Include(s => s.Spaserviceorders)
                    .Load();

                SpaServicesListView.ItemsSource = _context.Spaservices.Local.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки SPA-услуг: {ex.Message}");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddSpaService_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditSpaServiceWindow();
            if (editWindow.ShowDialog() == true)
            {
                try
                {
                    _context.Spaservices.Add(editWindow.Service);
                    _context.SaveChanges();
                    LoadSpaServices();
                    MessageBox.Show("SPA-услуга успешно добавлена", "Успех");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка");
                }
            }
        }

        private void EditSpaService_Click(object sender, RoutedEventArgs e)
        {
            if (SpaServicesListView.SelectedItem is Spaservice selectedService)
            {
                var editWindow = new EditSpaServiceWindow(selectedService);
                if (editWindow.ShowDialog() == true)
                {
                    try
                    {
                        _context.SaveChanges();
                        LoadSpaServices();
                        MessageBox.Show("Изменения сохранены", "Успех");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите SPA-услугу для редактирования", "Внимание");
            }
        }

        private void DeleteSpaService_Click(object sender, RoutedEventArgs e)
        {
            if (SpaServicesListView.SelectedItem is Spaservice selectedService)
            {
                if (selectedService.Spaserviceorders.Any())
                {
                    MessageBox.Show("Невозможно удалить услугу, так как есть связанные заказы", "Ошибка");
                    return;
                }

                if (MessageBox.Show("Вы уверены, что хотите удалить эту SPA-услугу?",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Spaservices.Remove(selectedService);
                        _context.SaveChanges();
                        LoadSpaServices();
                        MessageBox.Show("SPA-услуга удалена", "Успех");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите SPA-услугу для удаления", "Внимание");
            }
        }
    }
}