using Hotel.Data;
using Hotel.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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

        private ApplicationDbContext _context;

        public BookingWindow(Room room) : this(null, false)
        {
            Room = room;
            InitializeComponent();
            _context = new ApplicationDbContext();
            LoadUserData();
            LoadServices();
            InitializeDatePickers();
        }

        public BookingWindow(Booking booking, bool isEditMode)
        {
            InitializeComponent();
            IsEditMode = isEditMode;
            _context = new ApplicationDbContext();

            if (isEditMode && booking != null)
            {
                // Получаем бронирование с AsNoTracking и затем присоединяем
                CurrentBooking = _context.Bookings
                    .Include(b => b.Room)
                    .ThenInclude(r => r.Category)
                    .Include(b => b.Guest)
                    .Include(b => b.Serviceorders)
                    .ThenInclude(so => so.Service)
                    .AsNoTracking()
                    .FirstOrDefault(b => b.BookingId == booking.BookingId);

                if (CurrentBooking != null)
                {
                    // Присоединяем и начинаем отслеживать
                    _context.Bookings.Attach(CurrentBooking);
                    _context.Entry(CurrentBooking).State = EntityState.Modified;

                    Room = CurrentBooking.Room;
                    GuestFullName = CurrentBooking.Guest.FullName;
                    GuestPassport = CurrentBooking.Guest.PassportData;
                    GuestPhone = CurrentBooking.Guest.ContactPhone;
                    GuestEmail = CurrentBooking.Guest.ContactEmail;
                    CheckInDate = CurrentBooking.CheckInDate.ToDateTime(TimeOnly.MinValue);
                    CheckOutDate = CurrentBooking.CheckOutDate.ToDateTime(TimeOnly.MinValue);
                    LoadServices();
                }
            }
            else
            {
                LoadUserData();
            }

            DataContext = this;
            InitializeDatePickers();
        }

        public BookingWindow(Room room, DateOnly checkInDate, DateOnly checkOutDate)
            : this(null, false)
        {
            Room = room;
            CheckInDate = checkInDate.ToDateTime(TimeOnly.MinValue);
            CheckOutDate = checkOutDate.ToDateTime(TimeOnly.MinValue);
            InitializeComponent();
            _context = new ApplicationDbContext();
            LoadUserData();
            LoadServices();
            InitializeDatePickers();
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }

        private void InitializeDatePickers()
        {
            CheckInDatePicker.BlackoutDates.Clear();
            CheckOutDatePicker.BlackoutDates.Clear();

            if (Room != null)
            {
                var currentBookingId = CurrentBooking != null ? CurrentBooking.BookingId : 0;
                var bookedDates = _context.Bookings
                    .Where(b => b.RoomId == Room.RoomId && b.BookingId != currentBookingId)
                    .AsNoTracking()
                    .ToList();

                foreach (var booking in bookedDates)
                {
                    var startDate = booking.CheckInDate.ToDateTime(TimeOnly.MinValue);
                    var endDate = booking.CheckOutDate.ToDateTime(TimeOnly.MinValue);

                    for (var date = startDate; date < endDate; date = date.AddDays(1))
                    {
                        CheckInDatePicker.BlackoutDates.Add(new CalendarDateRange(date));
                        CheckOutDatePicker.BlackoutDates.Add(new CalendarDateRange(date));
                    }
                }
            }

            CheckInDatePicker.SelectedDateChanged += (sender, e) =>
            {
                if (CheckInDatePicker.SelectedDate.HasValue)
                {
                    CheckOutDatePicker.DisplayDateStart = CheckInDatePicker.SelectedDate.Value;
                    if (CheckOutDatePicker.SelectedDate < CheckInDatePicker.SelectedDate)
                    {
                        CheckOutDatePicker.SelectedDate = CheckInDatePicker.SelectedDate.Value.AddDays(1);
                    }
                }
            };
        }

        private void LoadUserData()
        {
            if (Application.CurrentGuest != null)
            {
                GuestFullName = Application.CurrentGuest.FullName;
                GuestPassport = Application.CurrentGuest.PassportData;
                GuestPhone = Application.CurrentGuest.ContactPhone;
                GuestEmail = Application.CurrentGuest.ContactEmail ?? string.Empty;

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
                    // Обновляем основные свойства
                    CurrentBooking.CheckInDate = DateOnly.FromDateTime(CheckInDate);
                    CurrentBooking.CheckOutDate = DateOnly.FromDateTime(CheckOutDate);
                    
                    // Обновляем связанные сервисы
                    UpdateSelectedServices(CurrentBooking);
                    
                    // Обновляем статус комнаты
                    Room.Status = "занят";
                    _context.Entry(Room).State = EntityState.Modified;
                }
                else
                {
                    Guest guest;

                    if (Application.CurrentGuest != null)
                    {
                        guest = _context.Guests.Find(Application.CurrentGuest.GuestId);
                    }
                    else
                    {
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
                    _context.Entry(Room).State = EntityState.Modified;
                }

                _context.SaveChanges();
                MessageBox.Show("Бронирование успешно оформлено!");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                string errorDetails = BuildErrorMessage(ex);
                MessageBox.Show($"Ошибка при бронировании: {errorDetails}");
                Debug.WriteLine("FULL ERROR:");
                Debug.WriteLine(ex.ToString());
            }
        }

        private string BuildErrorMessage(Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(ex.Message);
            
            Exception inner = ex.InnerException;
            int level = 1;
            
            while (inner != null)
            {
                sb.AppendLine(new string(' ', level * 2) + "↳ " + inner.Message);
                inner = inner.InnerException;
                level++;
            }
            
            return sb.ToString();
        }

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
            // Загружаем текущие сервисные заказы
            var existingOrders = _context.Serviceorders
                .Where(so => so.BookingId == booking.BookingId)
                .ToList();

            // Удаляем невыбранные сервисы
            var servicesToRemove = existingOrders
                .Where(eo => !AvailableServices.Any(s => s.IsSelected && s.Service.ServiceId == eo.ServiceId))
                .ToList();

            foreach (var service in servicesToRemove)
            {
                _context.Serviceorders.Remove(service);
            }

            // Добавляем или обновляем выбранные сервисы
            foreach (var selectedService in AvailableServices.Where(s => s.IsSelected))
            {
                var existingOrder = existingOrders
                    .FirstOrDefault(so => so.ServiceId == selectedService.Service.ServiceId);

                if (existingOrder != null)
                {
                    existingOrder.ServiceDate = DateOnly.FromDateTime(selectedService.ServiceDate);
                    existingOrder.ServiceTime = selectedService.ServiceTime;
                    _context.Entry(existingOrder).State = EntityState.Modified;
                }
                else
                {
                    var newOrder = new Serviceorder
                    {
                        BookingId = booking.BookingId,
                        ServiceId = selectedService.Service.ServiceId,
                        ServiceDate = DateOnly.FromDateTime(selectedService.ServiceDate),
                        ServiceTime = selectedService.ServiceTime
                    };
                    _context.Serviceorders.Add(newOrder);
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