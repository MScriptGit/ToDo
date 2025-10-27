using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Views;
using ToDo;
using Android.Graphics.Pdf.Models;
using Plugin.LocalNotification;

namespace ToDo
{
    public static class Constants
    {
        public static readonly string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TodoList.json");
    }

    public class todoItem : INotifyPropertyChanged
    {
        private string _listItem;
        private bool _isCompleted;

        public string listItem
        {
            get => _listItem;
            set
            {
                _listItem = value;
                OnPropertyChanged(nameof(listItem));
            }
        }

        public bool isCompleted
        {
            get => _isCompleted;
            set
            {
                _isCompleted = value;
                OnPropertyChanged(nameof(isCompleted));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{listItem} - {isCompleted}";
        }
    }

    public class MyPopup : Popup
    {
        private Entry itemEntry;

        public MyPopup(MainPage mainPage)
        {
            itemEntry = new Entry
            {
                Placeholder = "Enter new item",
                PlaceholderColor = Colors.White,
                TextColor = Colors.White
            };


            var border = new Border
            {
                BackgroundColor = Colors.Black,
                HeightRequest = 200,
                WidthRequest = 300,
                Stroke = Colors.Grey,
                StrokeThickness = 2,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(15),
                },
                Padding = new Thickness(20),


                Content = new VerticalStackLayout
                {
                    Children =
                {
                    new Label { Text = "" },
                    new Label { Text = "" },
                    itemEntry,
                    new Button
                    {
                        Text = "Add item",
                        Command = new Command(() =>
                        {
                            try
                            {
                                string userInput = itemEntry.Text;
                                mainPage.addNewItem(userInput);
                                Close();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error: " + ex.Message);
                            }
                        })

                    }
                }
                }
            };

            Color = Colors.Transparent;

            Content = border;
        }
    }

    public class editPopup : Popup
    {
        private Entry editEntry;

        public editPopup(todoItem item, MainPage mainPage)
        {
            editEntry = new Entry
            {
                Text = item.listItem,
                Placeholder = "Enter new text",
                PlaceholderColor = Colors.White,
                TextColor = Colors.White,
                FontSize = 18
            };

            var saveButton = new ImageButton
            {
                Source = ImageSource.FromFile("save50x50.png"),
                BackgroundColor = Colors.Transparent,
                WidthRequest = 50,
                HeightRequest = 50,
                Margin = new Thickness(0, 10, 0, 0),
                Command = new Command(() =>
                {
                    try
                    {
                        item.listItem = editEntry.Text; // Wijzig direct het item
                        mainPage.writeFile(); // Sla wijzigingen op
                        Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                })
            };

            var deleteButton = new ImageButton
            {
                Source = ImageSource.FromFile("trashcan50x50.png"),
                BackgroundColor = Colors.Transparent,
                WidthRequest = 50,
                HeightRequest = 50,
                Margin = new Thickness(0, 10, 0, 0),
                Command = new Command(() =>
                {
                    Close();
                    mainPage.viewModel.Items.Remove(item);
                    mainPage.writeFile();
                })
            };


            var border = new Border
            {
                BackgroundColor = Colors.Black,
                HeightRequest = 150,
                WidthRequest = 300,
                Stroke = Colors.Grey,
                StrokeThickness = 2,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(15),
                },
                Padding = new Thickness(20),

                Content = new VerticalStackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        editEntry,
                        new HorizontalStackLayout
                        {
                            Spacing = 20,
                            HorizontalOptions = LayoutOptions.Center,
                            Children =
                            {
                                saveButton,
                                deleteButton
                            }
                        }
                    }
                }
            };

            Color = Colors.Transparent;

            Content = border;
        }
    }



    public class MainViewModel
    {
        public ObservableCollection<todoItem> Items { get; set; }

        public MainViewModel()
        {
            if (File.Exists(Constants.filePath))
            {
                var json = File.ReadAllText(Constants.filePath);
                Items = new ObservableCollection<todoItem>(
                    JsonSerializer.Deserialize<List<todoItem>>(json)
                );
            }
            else
            {
                Items = new ObservableCollection<todoItem>();
            }
        }
    }

    public partial class MainPage : ContentPage
    {
        public MainViewModel viewModel = new MainViewModel();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = viewModel;

            // Luister naar property changes van bestaande items (alleen nodig bij eerste load)
            foreach (var item in viewModel.Items)
            {
                SubscribeToItem(item);
            }
        }

        private void SubscribeToItem(todoItem item)
        {
            item.PropertyChanged += Item_PropertyChanged;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Als een property zoals 'isCompleted' of 'listItem' wijzigt
            writeFile();
        }

        private void btnClicked(object sender, EventArgs e)
        {
            var popup = new MyPopup(this);
            Application.Current.MainPage.ShowPopup(popup);
        }

        private void editBtnClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.BindingContext is todoItem item)
            {
                var popup = new editPopup(item, this);
                Application.Current.MainPage.ShowPopup(popup);
            }
        }

        public void addNewItem(string userInput)
        {
            //add new item to ObservableList
            var newItem = new todoItem { listItem = userInput, isCompleted = false };
            SubscribeToItem(newItem);
            viewModel.Items.Add(newItem);

            //save new ObservableList to JSON file
            writeFile();
        }

        public void readFile()
        {
            string jsonString = File.ReadAllText(Constants.filePath);
            List<todoItem> items = JsonSerializer.Deserialize<List<todoItem>>(jsonString);
            foreach (var item in items)
            {
                viewModel.Items.Add(item);
            }
        }

        public void writeFile()
        {
            string jsonString = JsonSerializer.Serialize(viewModel.Items);
            File.WriteAllText(Constants.filePath, jsonString);
        }

        public void deleteItem(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.BindingContext is todoItem item)
            {
                viewModel.Items.Remove(item);
                writeFile();
            }
        }

        private async void menuBtnClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.BindingContext is todoItem item)
            {
                var popup = new editPopup(item, this);
                Application.Current.MainPage.ShowPopup(popup);
            }
        }

        private void OnReorderCompleted(object sender, EventArgs e)
        {
            writeFile();
        }



        /*protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Zorg ervoor dat deze methode met 'await' wordt aangeroepen als hij 'Task' retourneert
            // In dit geval roepen we hem aan met await in de 'async void' OnAppearing.
            await CheckAndRequestNotificationPermission();
        }

        private async Task CheckAndRequestNotificationPermission()
        {
            // FOUT OPGELOST: Gebruik 'await' om de bool-waarde uit de Task<bool> te halen.
            bool isEnabled = await LocalNotificationCenter.Current.AreNotificationsEnabled();

            if (!isEnabled)
            {
                // FOUT OPGELOST: Gebruik 'await' om de bool-waarde uit de Task<bool> te halen.
                var requestResult = await LocalNotificationCenter.Current.RequestNotificationPermission();

                if (requestResult != true)
                {
                    // Toon een uitleg waarom de toestemming nodig is
                    await DisplayAlert("Wekkerfunctie vereist",
                                       "Om de wekker te laten afgaan wanneer de app gesloten is, hebben we toestemming nodig voor het versturen van meldingen.",
                                       "OK");
                }
            }
        }*/


    }
}