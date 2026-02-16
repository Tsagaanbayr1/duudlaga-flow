using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace DuudlagaFlow.Views.History;

public partial class HistoryWindow : Window
{
    public HistoryWindow()
    {
        InitializeComponent();
        try { Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/icon_32.png", UriKind.Absolute)); } catch { }
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RefreshList();
    }

    private void RefreshList()
    {
        var query = SearchBox.Text.Trim();
        var records = string.IsNullOrEmpty(query)
            ? App.Instance.HistoryStore.AllRecords
            : App.Instance.HistoryStore.Search(query);

        HistoryList.ItemsSource = records;
        CountText.Text = $"{records.Count} бичлэг";
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        RefreshList();
    }

    private void OnCopyClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch { }
        }
    }

    private void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            App.Instance.HistoryStore.Delete(id);
            RefreshList();
        }
    }

    private void OnDeleteAllClick(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Бүх түүхийг устгах уу?",
            "Түүх устгах",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            App.Instance.HistoryStore.DeleteAll();
            RefreshList();
        }
    }
}
