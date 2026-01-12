using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Azure.Storage.Blobs;
using Clipboard = System.Windows.Clipboard;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;


namespace FsAzureStorage.Windows {
    /// <summary>
    /// Interaction logic for PropertiesWindow.xaml
    /// </summary>
    public partial class PropertiesWindow : Window {
        private readonly BlobClient _blob;

        public PropertiesWindow(BlobClient blob)
        {
            _blob = blob;

            InitializeComponent();
            TabControl.Focus();

            Title = $"/{_blob.Name} - [{_blob.BlobContainerName}]";
            Init();

            PreviewKeyDown += (s, e) => {
                if (e.Key == Key.Escape) Close();
            };
        }

        private async void Init()
        {
            var properties = (await _blob.GetPropertiesAsync()).Value;

            Dispatcher.Invoke(() => {
                DataContext = new PropertiesViewModel(properties);
            });
        }

        private void Properties_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control) {
                var selectedItems = Properties.SelectedItems.OfType<PropertyValue>();
                var data = string.Join("\n", selectedItems.Select(_ => _.Value));
                Clipboard.SetDataObject(data);
            }
        }

        private void SaveMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is PropertiesViewModel model) {
                var data = model.Metadata
                    .Where(_ => !string.IsNullOrEmpty(_.Key))
                    .ToList();

                var duplicates = data
                    .GroupBy(_ => _.Key)
                    .Where(_ => _.Count() > 1)
                    .ToList();

                if (duplicates.Any()) {
                    MessageBox.Show(this, $"Duplicate Metadata Keys:\n - {string.Join("\n - ", duplicates.SelectMany(_ => _).Select(_ => $"{_.Key}: {_.Value}"))}", "Error");
                    return;
                }

                var metadata = data
                    .ToDictionary(
                        _ => _.Key!,
                        _ => _.Value!
                    );

                _blob.SetMetadata(metadata);
                this.Close();
            }
        }

        private void ResetMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void DataGridCell_Selected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is DataGridCell && sender is DataGrid grid) {
                grid.BeginEdit(e);
            }
        }
    }
}
