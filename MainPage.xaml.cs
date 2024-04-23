﻿using Microsoft.Maui.Controls;
using System.Globalization;


namespace PoisoningIncidentApplication
{
    public partial class MainPage : ContentPage
    {
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


        private async Task PerformSearch(string searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var productName = await _databaseService.GetProductByNameAsync(searchTerm);
                if (productName != null)
                {
                    string description = await _databaseService.GetProductDescriptionByNameAsync(productName);
                    DescriptionHeaderLabel.IsVisible = true;
                    SearchResultsLabel.Text = $"Hittade 1 träff på din sökning: {productName}";
                    DescriptionLabel.IsVisible = true;
                    DescriptionLabel.Text = description;
                }
                else
                {
                    SearchResultsLabel.Text = $"Hittade 0 träffar på din sökning: {searchTerm}";
                    DescriptionHeaderLabel.IsVisible = false;
                    DescriptionLabel.IsVisible = false;
                }
            }
        }



    }
}
