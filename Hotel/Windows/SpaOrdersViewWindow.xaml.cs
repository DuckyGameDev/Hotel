using Hotel.Data;
using Hotel.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using Application = Hotel.Data.Application;

namespace Hotel.Windows
{
    public partial class SpaOrdersViewWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public SpaOrdersViewWindow()
        {
            InitializeComponent();
            LoadSpaOrders();
        }

        private void LoadSpaOrders()
        {
            if (Application.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться");
                Close();
                return;
            }

            try
            {
                _context.Spaserviceorders
                    .Include(o => o.SpaService)
                    .Where(o => o.UserId == Application.CurrentUser.UserId)
                    .OrderBy(o => o.ServiceDate)
                    .ThenBy(o => o.ServiceTime)
                    .Load();

                SpaOrdersListView.ItemsSource = _context.Spaserviceorders.Local.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            if (SpaOrdersListView.SelectedItem is Spaserviceorder selectedOrder)
            {
                if (IsPastOrder(selectedOrder))
                {
                    MessageBox.Show("Нельзя отменить завершенную процедуру");
                    return;
                }

                if (MessageBox.Show("Вы уверены, что хотите отменить эту процедуру?",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Spaserviceorders.Remove(selectedOrder);
                        _context.SaveChanges();
                        LoadSpaOrders();
                        MessageBox.Show("Процедура отменена");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при отмене: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для отмены");
            }
        }

        private void RescheduleOrder_Click(object sender, RoutedEventArgs e)
        {
            if (SpaOrdersListView.SelectedItem is Spaserviceorder selectedOrder)
            {
                if (IsPastOrder(selectedOrder))
                {
                    MessageBox.Show("Нельзя перенести завершенную процедуру");
                    return;
                }

                var rescheduleWindow = new RescheduleSpaOrderWindow(selectedOrder);
                if (rescheduleWindow.ShowDialog() == true)
                {
                    try
                    {
                        selectedOrder.ServiceDate = rescheduleWindow.NewDate;
                        selectedOrder.ServiceTime = rescheduleWindow.NewTime;
                        _context.SaveChanges();
                        LoadSpaOrders();
                        MessageBox.Show("Процедура перенесена");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при переносе: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для переноса");
            }
        }

        private bool IsPastOrder(Spaserviceorder order)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var now = TimeOnly.FromDateTime(DateTime.Now);

            return order.ServiceDate < today ||
                  (order.ServiceDate == today && order.ServiceTime < now);
        }
    }
}