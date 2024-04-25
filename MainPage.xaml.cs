using Microsoft.Maui.Controls;
using System.Globalization;
using Microsoft.Maui.ApplicationModel.Communication;
using Android.Content;
using System.ComponentModel;
namespace PoisoningIncidentApplication
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var changed = PropertyChanged;
            if (changed != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private readonly DatabaseService _databaseService;
        private bool _isSuggestionsVisible;
        public bool IsSuggestionsVisible
        {
            get => _isSuggestionsVisible;
            set
            {
                if (_isSuggestionsVisible != value)
                {
                    _isSuggestionsVisible = value;
                    OnPropertyChanged(nameof(IsSuggestionsVisible));
                }
            }
        }

        public MainPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            SuggestionsCollection.IsVisible = false;
            SuggestionFrame.IsVisible = false;
            BindingContext = this;
        }

       
        private async void ProductSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            const int itemHeight = 18; // The estimated height of each item
            const int maxHeight = 100; // The maximum height you want for the CollectionView

            if (!string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                var suggestions = await _databaseService.GetProductSuggestionsAsync(e.NewTextValue);
                SuggestionsCollection.ItemsSource = suggestions;
                SuggestionsCollection.IsVisible = suggestions.Any();
                SuggestionFrame.IsVisible = suggestions.Any();

                // Calculate the height based on the number of items, but do not exceed the maximum height
                var desiredHeight = suggestions.Count() * itemHeight;
                SuggestionsCollection.HeightRequest = Math.Min(desiredHeight, maxHeight);
            }
            else
            {
                SuggestionsCollection.IsVisible = false;
                SuggestionFrame.IsVisible= false;
            }

            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                DescriptionLabel.Text = "";
                SearchResultsLabel.Text = "";
                DescriptionHeaderLabel.IsVisible = false;
                DescriptionLabel.IsVisible = false;

            }


        }

    
        private async void OnSearchClicked(object sender, EventArgs e)
        {
            ProductSearchBar.Unfocus();
            SuggestionsCollection.IsVisible = false;
            SuggestionFrame.IsVisible = false;
           
            string searchTerm = ProductSearchBar.Text;
            if (!string.IsNullOrEmpty(searchTerm))
            {
                DescriptionHeaderLabel.IsVisible = true;
                searchTerm = char.ToUpper(searchTerm[0], CultureInfo.CurrentCulture) + searchTerm.Substring(1).ToLower(CultureInfo.CurrentCulture);
                await PerformSearch(searchTerm);
            }
        }


        //GÖR SÅ ATT MAN KAN KLICKA FLERA GÅNGER PÅ SAMMA SUGGESTION

        private async void OnSuggestionSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = e.CurrentSelection.FirstOrDefault() as string;
            if (selectedItem != null)
            {
                ProductSearchBar.Text = selectedItem; // Set the selected item as the search bar text
                DescriptionHeaderLabel.IsVisible = true;

                // Clear selection
                ((CollectionView)sender).SelectedItem = null;

                // Trigger the search
                await PerformSearch(selectedItem);

                // Ensure we are on the main thread when we update the UI
                Dispatcher.Dispatch(() =>
                {
                    SuggestionsCollection.IsVisible = false; // Hide the suggestions
                    SuggestionFrame.IsVisible= false;
                    ProductSearchBar.Unfocus();
                });
            }
        }


        //private async Task PerformSearch(string searchTerm)
        //{
        //    if (!string.IsNullOrWhiteSpace(searchTerm))
        //    {
        //        var productName = await _databaseService.GetProductByNameAsync(searchTerm);
        //        if (productName != null)
        //        {
        //            string description = await _databaseService.GetProductDescriptionByNameAsync(productName);
        //            DescriptionHeaderLabel.IsVisible = true;
        //            SearchResultsLabel.Text = $"Hittade 1 träff på din sökning: {productName}";
        //            DescriptionLabel.IsVisible = true;
        //            DescriptionLabel.Text = description;
        //        }
        //        else
        //        {
        //            SearchResultsLabel.Text = $"Hittade 0 träffar på din sökning: {searchTerm}";
        //            DescriptionHeaderLabel.IsVisible = false;
        //            DescriptionLabel.IsVisible = false;
        //        }
        //    }
        //}
        // This property is used to bind the background color in XAML.
        private string GetColorByDangerLevel(int dangerLevel)
        {
            return dangerLevel switch
            {
                0 => "#ebedec",  
                1 => "#ddecc5",  
                2 => "#d2e6d2",   
                3 => "#cddcf0",  
                4 => "#fee4a5",  
                5 => "#d68c45",   
                6 => "#c41b1b",   
                _ => "#808080",   
            };
        }


        public string DangerLevelColor { get; set; }
        private FormattedString CreateFormattedDescription(string description)
        {
            var formattedDescription = new FormattedString();
            var wordsToBold = new HashSet<string> { "inte", "genast", "omedelbart", "112" };
            var phrasesToBold = new List<string> { "Riskabel produkt!", "Frätande produkt!", "Ring 112" };

            string[] lines = description.Split('\n');

            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    if (formattedDescription.Spans.Count > 0)
                    {
                        formattedDescription.Spans.Add(new Span { Text = "\n" });
                    }

                    string trimmedLine = line.TrimStart('•', ' ').TrimEnd();
                    string bulletPoint = line.StartsWith("•") ? "• " : "";

                    int currentIndex = 0;
                    bool isFirstToken = true;

                    while (currentIndex < trimmedLine.Length)
                    {
                        if (isFirstToken && bulletPoint != "")
                        {
                            formattedDescription.Spans.Add(new Span { Text = bulletPoint });
                            isFirstToken = false;
                        }

                        int nextSpace = trimmedLine.IndexOf(' ', currentIndex);
                        if (nextSpace == -1) nextSpace = trimmedLine.Length;

                        string word = trimmedLine.Substring(currentIndex, nextSpace - currentIndex);
                        string remainingText = trimmedLine.Substring(currentIndex);

                        // Match phrases first
                        var matchedPhrase = phrasesToBold.FirstOrDefault(phrase => remainingText.StartsWith(phrase));
                        if (matchedPhrase != null)
                        {
                            formattedDescription.Spans.Add(new Span
                            {
                                Text = matchedPhrase + " ",
                                FontAttributes = FontAttributes.Bold
                            });
                            currentIndex += matchedPhrase.Length + 1; // Skip past the phrase
                        }
                        else
                        {
                            // Match single bold words or normal text
                            FontAttributes attrs = wordsToBold.Contains(word) ? FontAttributes.Bold : FontAttributes.None;
                            formattedDescription.Spans.Add(new Span { Text = word + " ", FontAttributes = attrs });
                            currentIndex = nextSpace + 1;
                        }
                    }
                }
            }

            return formattedDescription;
        }











        private async Task PerformSearch(string searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var productName = await _databaseService.GetProductByNameAsync(searchTerm);
                if (productName != null)
                {
                    string description = await _databaseService.GetProductDescriptionByNameAsync(productName);
                    int dangerLevel = await _databaseService.GetProductDangerLevelByNameAsync(productName); // Get the danger level
                    DangerLevelColor = GetColorByDangerLevel(dangerLevel); // Set the color based on the danger level

                    //DescriptionHeaderLabel.IsVisible = true;
                    SearchResultsLabel.Text = $"Hittade 1 träff på din sökning: {productName}";
                    DescriptionLabel.IsVisible = true;
                    DescriptionLabel.FormattedText = CreateFormattedDescription(description);

                    OnPropertyChanged(nameof(DangerLevelColor)); // Notify the UI of the color change
                }
                else
                {
                    SearchResultsLabel.Text = $"Hittade 0 träffar på din sökning: {searchTerm}";
                    DescriptionHeaderLabel.IsVisible = false;
                    DescriptionLabel.IsVisible = false;
                }
            }
        }

        private async void OnEmergencyButtonClicked(object sender, EventArgs e)
        {
            bool isConfirmed = await DisplayAlert("Call Emergency", "Are you sure you want to call emergency services?", "Yes", "No");
            if (isConfirmed)
            {
                try
                {
                    MakePhoneCall("112");
                }
                catch (Exception ex)
                {
                    // Consider showing the exception message to understand what's going wrong
                    await DisplayAlert("Error", ex.Message, "OK");
                }
            }
        }


        private async void OnNonEmergencyButtonClicked(object sender, EventArgs e)
        {
            bool isConfirmed = await DisplayAlert("Call Non-Emergency", "Are you sure you want to call the non-emergency number?", "Yes", "No");
            if (isConfirmed)
            {
                MakePhoneCall("010-456 6700");
            }
        }

#if ANDROID
        private void MakePhoneCall(string number)
        {
            var intent = new Intent(Intent.ActionCall);
            intent.SetData(Android.Net.Uri.Parse($"tel:{number}"));
            // Add the FLAG_ACTIVITY_NEW_TASK flag as required for calling from outside an Activity context.
            intent.AddFlags(ActivityFlags.NewTask);
            Android.App.Application.Context.StartActivity(intent);
        }
#endif
    }


}

