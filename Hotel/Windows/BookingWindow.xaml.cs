using Hotel.Data;
using Hotel.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Application = Hotel.Data.Application;

namespace Hotel.Windows
{
    public partial class BookingWindow : Window
    {
        public Room Room { get; set; }
        public string GuestFullName { get; set; }
        public string GuestPassport { get; set; }
        public string GuestPhone { get; set; }
        public string GuestEmail { get; set; }
        public DateTime CheckInDate { get; set; } = DateTime.Today;
        public DateTime CheckOutDate { get; set; } = DateTime.Today.AddDays(1);
        public decimal TotalPrice => CalculateTotalPrice();
        public bool IsEditMode { get; private set; }
        public Booking CurrentBooking { get; private set; }

        public ObservableCollection<ServiceSelection> AvailableServices { get; set; } = new ObservableCollection<ServiceSelection>();

        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public BookingWindow(Room room) : this(null, false)
        {
            Room = room;
            InitializeComponent();
            LoadUserData(); // Загружаем данные пользователя
            LoadServices();
        }

        public BookingWindow(Booking booking, bool isEditMode)
        {
            InitializeComponent();
            IsEditMode = isEditMode;
            CurrentBooking = booking;

            if (isEditMode && booking != null)
            {
                Room = booking.Room;
                GuestFullName = booking.Guest.FullName;
                GuestPassport = booking.Guest.PassportData;
                GuestPhone = booking.Guest.ContactPhone;
                GuestEmail = booking.Guest.ContactEmail;
                CheckInDate = booking.CheckInDate.ToDateTime(TimeOnly.MinValue);
                CheckOutDate = booking.CheckOutDate.ToDateTime(TimeOnly.MinValue);
                LoadServices();
            }
            else
            {
                LoadUserData(); // Загружаем данные пользователя для нового бронирования
            }

            DataContext = this;
        }

        private void LoadUserData()
        {
            if (Application.CurrentGuest != null)
            {
                GuestFullName = Application.CurrentGuest.FullName;
                GuestPassport = Application.CurrentGuest.PassportData;
                GuestPhone = Application.CurrentGuest.ContactPhone;
                GuestEmail = Application.CurrentGuest.ContactEmail ?? string.Empty;

                // Делаем поля только для чтения, если пользователь авторизован
                FullNameTextBox.IsReadOnly = true;
                PassportTextBox.IsReadOnly = true;
                PhoneTextBox.IsReadOnly = true;
            }
        }

        private void LoadServices()
        {
            _context.Services.Load();
            var services = _context.Services.Local.ToList();

            var selectedServiceIds = new HashSet<int>();
            if (IsEditMode && CurrentBooking != null)
            {
                selectedServiceIds = new HashSet<int>(
                    CurrentBooking.Serviceorders.Select(so => so.ServiceId));
            }

            foreach (var service in services)
            {
                var serviceOrder = IsEditMode ?
                    CurrentBooking?.Serviceorders.FirstOrDefault(so => so.ServiceId == service.ServiceId) :
                    null;

                AvailableServices.Add(new ServiceSelection
                {
                    Service = service,
                    IsSelected = selectedServiceIds.Contains(service.ServiceId),
                    ServiceDate = serviceOrder != null ?
                        serviceOrder.ServiceDate.ToDateTime(TimeOnly.MinValue) :
                        DateTime.Today,
                    ServiceTime = serviceOrder?.ServiceTime ?? new TimeOnly(12, 0),
                    AvailableTimes = GetAvailableTimes()
                });
            }

            ServicesDataGrid.ItemsSource = AvailableServices;
        }

        private void ConfirmBooking_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GuestFullName) ||
                string.IsNullOrWhiteSpace(GuestPassport) ||
                string.IsNullOrWhiteSpace(GuestPhone))
            {
                MessageBox.Show("Заполните все обязательные поля!");
                return;
            }

            try
            {
                if (IsEditMode && CurrentBooking != null)
                {
                    // Редактирование существующего бронирования
                    CurrentBooking.CheckInDate = DateOnly.FromDateTime(CheckInDate);
                    CurrentBooking.CheckOutDate = DateOnly.FromDateTime(CheckOutDate);
                    UpdateSelectedServices(CurrentBooking);
                }
                else
                {
                    // Создание нового бронирования
                    Guest guest;

                    if (Application.CurrentGuest != null)
                    {
                        // Используем существующего гостя (текущего пользователя)
                        guest = _context.Guests.Find(Application.CurrentGuest.GuestId);
                    }
                    else
                    {
                        // Создаем нового гостя (для неавторизованных пользователей)
                        guest = new Guest
                        {
                            FullName = GuestFullName,
                            PassportData = GuestPassport,
                            ContactPhone = GuestPhone,
                            ContactEmail = string.IsNullOrWhiteSpace(GuestEmail) ? null : GuestEmail
                        };
                        _context.Guests.Add(guest);
                    }

                    var booking = new Booking
                    {
                        Guest = guest,
                        RoomId = Room.RoomId,
                        CheckInDate = DateOnly.FromDateTime(CheckInDate),
                        CheckOutDate = DateOnly.FromDateTime(CheckOutDate)
                    };

                    AddSelectedServices(booking);
                    Room.Status = "занят";
                    _context.Bookings.Add(booking);
                    _context.Rooms.Update(Room);
                }

                _context.SaveChanges();
                MessageBox.Show("Бронирование успешно оформлено!");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при бронировании: {ex.Message}");
            }
        }

        // Остальные методы остаются без изменений...
        private Dictionary<TimeOnly, string> GetAvailableTimes()
        {
            return new Dictionary<TimeOnly, string>
            {
                { new TimeOnly(9, 0), "09:00" },
                { new TimeOnly(11, 0), "11:00" },
                { new TimeOnly(13, 0), "13:00" },
                { new TimeOnly(15, 0), "15:00" },
                { new TimeOnly(17, 0), "17:00" }
            };
        }

        private decimal CalculateTotalPrice()
        {
            decimal total = Room.Category.PricePerNight * (CheckOutDate - CheckInDate).Days;

            foreach (var service in AvailableServices.Where(s => s.IsSelected))
            {
                total += service.Service.Price;
            }

            return total;
        }

        private void UpdateSelectedServices(Booking booking)
        {
            var servicesToRemove = booking.Serviceorders
                .Where(so => !AvailableServices.Any(s => s.IsSelected && s.Service.ServiceId == so.ServiceId))
                .ToList();

            foreach (var service in servicesToRemove)
            {
                booking.Serviceorders.Remove(service);
            }

            foreach (var selectedService in AvailableServices.Where(s => s.IsSelected))
            {
                var existingOrder = booking.Serviceorders
                    .FirstOrDefault(so => so.ServiceId == selectedService.Service.ServiceId);

                if (existingOrder != null)
                {
                    existingOrder.ServiceDate = DateOnly.FromDateTime(selectedService.ServiceDate);
                    existingOrder.ServiceTime = selectedService.ServiceTime;
                }
                else
                {
                    booking.Serviceorders.Add(new Serviceorder
                    {
                        ServiceId = selectedService.Service.ServiceId,
                        ServiceDate = DateOnly.FromDateTime(selectedService.ServiceDate),
                        ServiceTime = selectedService.ServiceTime
                    });
                }
            }
        }

        private void AddSelectedServices(Booking booking)
        {
            foreach (var selectedService in AvailableServices.Where(s => s.IsSelected))
            {
                booking.Serviceorders.Add(new Serviceorder
                {
                    ServiceId = selectedService.Service.ServiceId,
                    ServiceDate = DateOnly.FromDateTime(selectedService.ServiceDate),
                    ServiceTime = selectedService.ServiceTime
                });
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class ServiceSelection
    {
        public Service Service { get; set; }
        public bool IsSelected { get; set; }
        public DateTime ServiceDate { get; set; }
        public TimeOnly ServiceTime { get; set; }
        public Dictionary<TimeOnly, string> AvailableTimes { get; set; }
    }
}