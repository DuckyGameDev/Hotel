using Hotel.Data;
using Hotel.Models.Entities;
using Hotel.Windows;
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

namespace Hotel
{
    /// <summary>
    /// Логика взаимодействия для MainWidow.xaml
    /// </summary>
    public partial class MainWidow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        public MainWidow()
        {
            InitializeComponent();
            LoadRooms();
        }

        private void LoadRooms()
        {
            _context.Rooms
                .Include(r => r.Category)
                .Load();

            RoomsListView.ItemsSource = _context.Rooms.Local.ToObservableCollection();
        }

        private void AddRoom_Click(object sender, RoutedEventArgs e)
        {
            var editForm = new RoomEditWindow(new Room());
            if (editForm.ShowDialog() == true)
            {
                _context.Rooms.Add(editForm.Room);
                _context.SaveChanges();
                LoadRooms();
            }
        }

        private void EditRoom_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsListView.SelectedItem is Room selectedRoom)
            {
                var editForm = new RoomEditWindow(selectedRoom);
                if (editForm.ShowDialog() == true)
                {
                    _context.SaveChanges();
                    LoadRooms();
                }
            }
            else
            {
                MessageBox.Show("Выберите номер для редактирования");
            }
        }

        private void DeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsListView.SelectedItem is Room selectedRoom)
            {
                if (MessageBox.Show("Удалить этот номер?", "Подтверждение",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _context.Rooms.Remove(selectedRoom);
                    _context.SaveChanges();
                    LoadRooms();
                }
            }
            else
            {
                MessageBox.Show("Выберите номер для удаления");
            }
        }

        private void BookRoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsListView.SelectedItem is Room selectedRoom)
            {
                var bookingForm = new BookingWindow(selectedRoom);
                if (bookingForm.ShowDialog() == true)
                {
                    // Обновляем список номеров после бронирования
                    LoadRooms();
                }
            }
            else
            {
                MessageBox.Show("Выберите номер для бронирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
