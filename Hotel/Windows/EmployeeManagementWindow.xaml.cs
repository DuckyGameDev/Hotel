using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Hotel.Data;
using Hotel.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Windows
{
    /// <summary>
    /// Логика взаимодействия для EmployeeManagementWindow.xaml
    /// </summary>
    public partial class EmployeeManagementWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private ObservableCollection<Employee> _employees;
        private Employee _currentEmployee;

        public EmployeeManagementWindow()
        {
            InitializeComponent();
            _employees = new ObservableCollection<Employee>();
            EmployeesDataGrid.ItemsSource = _employees;
            LoadEmployees();
            SetFormState(false);
            HireDatePicker.SelectedDate = DateTime.Today;
        }

        private void LoadEmployees()
        {   
            
            _context.Employees.Load();

            _employees.Clear();

            foreach(var employee in _context.Employees.Local)
                _employees.Add(employee);
        }

        private void SetFormState(bool isEditing)
        {
            AddButton.IsEnabled = !isEditing;
            SaveButton.IsEnabled = isEditing;
            CancelButton.IsEnabled = isEditing;
            EmployeesDataGrid.IsEnabled = !isEditing;

            if (!isEditing)
            {
                ClearForm();
            }
        }

        private void ClearForm()
        {
            FullNameTextBox.Text = "";
            PositionTextBox.Text = "";
            PhoneTextBox.Text = "";
            EmailTextBox.Text = "";
            HireDatePicker.SelectedDate = DateTime.Today;
            _currentEmployee = null;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _currentEmployee = new Employee
            {
                HireDate = DateOnly.FromDateTime(DateTime.Today)
            };
            FillFormFromEmployee();
            SetFormState(true);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(PositionTextBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                HireDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentEmployee == null) return;

            _currentEmployee.FullName = FullNameTextBox.Text.Trim();
            _currentEmployee.Position = PositionTextBox.Text.Trim();
            _currentEmployee.ContactPhone = PhoneTextBox.Text.Trim();
            _currentEmployee.ContactEmail = EmailTextBox.Text.Trim();
            _currentEmployee.HireDate = DateOnly.FromDateTime(HireDatePicker.SelectedDate.Value);

            try
            {
                if (_currentEmployee.EmployeeId == 0) // Новый сотрудник
                {
                    _context.Employees.Add(_currentEmployee);
                }

                _context.SaveChanges();
                LoadEmployees();
                SetFormState(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SetFormState(false);
        }

        private void EmployeesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmployeesDataGrid.SelectedItem is Employee selectedEmployee)
            {
                _currentEmployee = selectedEmployee;
                FillFormFromEmployee();
                SetFormState(true);
            }
        }

        private void FillFormFromEmployee()
        {
            if (_currentEmployee == null) return;

            FullNameTextBox.Text = _currentEmployee.FullName;
            PositionTextBox.Text = _currentEmployee.Position;
            PhoneTextBox.Text = _currentEmployee.ContactPhone;
            EmailTextBox.Text = _currentEmployee.ContactEmail;
            HireDatePicker.SelectedDate = new DateTime(
                _currentEmployee.HireDate.Year,
                _currentEmployee.HireDate.Month,
                _currentEmployee.HireDate.Day);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text.ToLower();
            EmployeesDataGrid.ItemsSource = _context.Employees.Local
                .Where(emp => emp.FullName.ToLower().Contains(searchText) ||
                               emp.Position.ToLower().Contains(searchText) ||
                               emp.ContactPhone.Contains(searchText) ||
                               (emp.ContactEmail != null && emp.ContactEmail.ToLower().Contains(searchText)))
                .ToList();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesDataGrid.SelectedItem is Employee selectedEmployee)
            {
                if (MessageBox.Show($"Удалить сотрудника {selectedEmployee.FullName}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Employees.Remove(selectedEmployee);
                        _context.SaveChanges();
                        LoadEmployees();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
