using Hotel.Data;
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
using Microsoft.EntityFrameworkCore;

namespace Hotel.Windows
{
    /// <summary>
    /// Логика взаимодействия для RoomEditWindow.xaml
    /// </summary>
    public partial class RoomEditWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        public Room CurrentRoom { get; private set; }

        public RoomEditWindow(Room room)
        {
            InitializeComponent();

            // Создаем новый объект для редактирования (чтобы избежать изменений до сохранения)
            CurrentRoom = new Room
            {
                RoomId = room.RoomId,
                RoomNumber = room.RoomNumber,
                CategoryId = room.CategoryId,
                Status = room.Status
            };

            // Загружаем категории
            _context.Roomcategories.Load();
            CategoryComboBox.ItemsSource = _context.Roomcategories.Local.ToObservableCollection();

            // Устанавливаем начальные значения
            RoomNumberTextBox.Text = CurrentRoom.RoomNumber;
            CategoryComboBox.SelectedValue = CurrentRoom.CategoryId;
            StatusComboBox.SelectedValue = CurrentRoom.Status;

            DataContext = CurrentRoom;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Обновляем значения из полей ввода
            CurrentRoom.RoomNumber = RoomNumberTextBox.Text;
            CurrentRoom.CategoryId = (int)CategoryComboBox.SelectedValue;
            CurrentRoom.Status = StatusComboBox.SelectedValue?.ToString();

            // Очищаем и проверяем ввод
            CurrentRoom.RoomNumber = RoomNumberTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(CurrentRoom.RoomNumber))
            {
                MessageBox.Show("Введите номер комнаты!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CurrentRoom.RoomNumber.Length > 10)
            {
                MessageBox.Show("Номер комнаты не должен превышать 10 символов!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (CurrentRoom.RoomId == 0) // Новый номер
                {
                    _context.Rooms.Add(CurrentRoom);
                }
                else // Редактирование существующего
                {
                    var existingRoom = _context.Rooms.Find(CurrentRoom.RoomId);
                    if (existingRoom != null)
                    {
                        existingRoom.RoomNumber = CurrentRoom.RoomNumber;
                        existingRoom.CategoryId = CurrentRoom.CategoryId;
                        existingRoom.Status = CurrentRoom.Status;
                    }
                }

                _context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.InnerException?.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
