﻿using Microsoft.Maui.Controls;


namespace PoisoningIncidentApplication
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public MainPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            
            ProductSearchBar.Unfocus();
            // Use the text from the SearchBar for the query
            string searchTerm = ProductSearchBar.Text;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var productName = await _databaseService.GetProductByNameAsync(searchTerm);
                if (productName != null)
                {
                    string description = await _databaseService.GetProductDescriptionByNameAsync(productName);
                    DescriptionHeaderLabel.IsVisible = true;
                    SearchResultsLabel.Text = $"Hittade 1 träff på din sökning: {productName}";
                    DescriptionLabel.IsVisible = true;
                    DescriptionLabel.Text = $"{description}";
                }
                else
                {
                    SearchResultsLabel.Text = $"Hittade 0 träffar på din sökning: {searchTerm}";
                    DescriptionHeaderLabel.IsVisible =false;
                    DescriptionLabel.IsVisible =false;
                }
            }
          

        }
    }
}
