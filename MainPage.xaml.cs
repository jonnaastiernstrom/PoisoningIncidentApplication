using Microsoft.Maui.Controls;


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
            // Use the text from the SearchBar for the query
            string searchTerm = ProductSearchBar.Text;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var productName = await _databaseService.GetProductByNameAsync(searchTerm);
                if (productName != null)
                {
                    SearchResultsLabel.Text = $"Product found: {productName}";
                }
                else
                {
                    SearchResultsLabel.Text = "Product not found.";
                }
            }
            else
            {
                SearchResultsLabel.Text = "Please enter a product name to search.";
            }
        }
    }
}
