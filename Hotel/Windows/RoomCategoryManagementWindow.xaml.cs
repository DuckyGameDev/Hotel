using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hotel.Data;
using Hotel.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Windows
{
    public partial class RoomCategoryManagementWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private ObservableCollection<Roomcategory> _categories;
        private Roomcategory _currentCategory;

        public RoomCategoryManagementWindow()
        {
            InitializeComponent();
            _categories = new ObservableCollection<Roomcategory>();
            CategoriesDataGrid.ItemsSource = _categories;
            LoadCategories();
            SetFormState(false);
        }

        private void LoadCategories()
        {
            _context.Roomcategories.Load();
            _categories.Clear();

            foreach (var category in _context.Roomcategories.Local)
                _categories.Add(category);
        }

        private void SetFormState(bool isEditing)
        {
            AddButton.IsEnabled = !isEditing;
            SaveButton.IsEnabled = isEditing;
            CancelButton.IsEnabled = isEditing;
            CategoriesDataGrid.IsEnabled = !isEditing;

            if (!isEditing)
            {
                ClearForm();
            }
        }

        private void ClearForm()
        {
            CategoryNameTextBox.Text = "";
            PriceTextBox.Text = "";
            DescriptionTextBox.Text = "";
            _currentCategory = null;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _currentCategory = new Roomcategory();
            FillFormFromCategory();
            SetFormState(true);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CategoryNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(PriceTextBox.Text) ||
                !decimal.TryParse(PriceTextBox.Text, out decimal price))
            {
                MessageBox.Show("Заполните все обязательные поля (название и корректная цена)!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentCategory == null) return;

            _currentCategory.CategoryName = CategoryNameTextBox.Text.Trim();
            _currentCategory.PricePerNight = price;
            _currentCategory.Description = DescriptionTextBox.Text.Trim();

            try
            {
                if (_currentCategory.CategoryId == 0)
                {
                    _context.Roomcategories.Add(_currentCategory);
                }

                _context.SaveChanges();
                LoadCategories();
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

        private void CategoriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoriesDataGrid.SelectedItem is Roomcategory selectedCategory)
            {
                _currentCategory = selectedCategory;
                FillFormFromCategory();
                SetFormState(true);
            }
        }

        private void FillFormFromCategory()
        {
            if (_currentCategory == null) return;

            CategoryNameTextBox.Text = _currentCategory.CategoryName;
            PriceTextBox.Text = _currentCategory.PricePerNight.ToString();
            DescriptionTextBox.Text = _currentCategory.Description;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text.ToLower();
            CategoriesDataGrid.ItemsSource = _context.Roomcategories.Local
                .Where(c => c.CategoryName.ToLower().Contains(searchText) ||
                            (c.Description != null && c.Description.ToLower().Contains(searchText)) ||
                            c.PricePerNight.ToString().Contains(searchText))
                .ToList();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesDataGrid.SelectedItem is Roomcategory selectedCategory)
            {
                // Проверка на наличие связанных номеров
                if (_context.Rooms.Any(r => r.CategoryId == selectedCategory.CategoryId))
                {
                    MessageBox.Show("Нельзя удалить категорию, к которой привязаны номера!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MessageBox.Show($"Удалить категорию {selectedCategory.CategoryName}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Roomcategories.Remove(selectedCategory);
                        _context.SaveChanges();
                        LoadCategories();
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
                MessageBox.Show("Выберите категорию для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}