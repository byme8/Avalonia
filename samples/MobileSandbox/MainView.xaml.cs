using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ControlCatalog.Controls;

namespace MobileSandbox
{
    public class MainView : UserControl
    {
        public MainView()
        {
            AvaloniaXamlLoader.Load(this);
            FillButton = this.Get<Button>(nameof(FillButton));
            FillLazyButton = this.Get<Button>(nameof(FillLazyButton));
            ContentPresenter2 = this.Get<ContentPresenter>(nameof(ContentPresenter2));
        }

        public Button FillLazyButton { get; }

        public Button FillButton { get; }

        public ContentPresenter ContentPresenter2 { get; }

        private async void Fill(object? sender, TappedEventArgs e)
        {
            var stopwatch = Stopwatch.StartNew();

            var stack = new StackPanel();
            ContentPresenter2.Content = stack;

            var chips = Enumerable.Range(0, 10).Select(
                i => new Chips
                {
                    Items = Enumerable.Range(0, 10).Select(j => $"Chip {i}-{j}").ToArray()
                });

            foreach (var chip in chips)
            {
                stack.Children.Add(chip);
            }

            stopwatch.Stop();

            FillButton.Content = $"Fill: {stopwatch.ElapsedMilliseconds}";
        }

        private async void FillLazy(object? sender, TappedEventArgs e)
        {
            var stopwatch = Stopwatch.StartNew();

            var stack = new StackPanel();
            ContentPresenter2.Content = stack;

            var chips = Enumerable.Range(0, 10).Select(
                i => new Chips
                {
                    Items = Enumerable.Range(0, 10).Select(j => $"Chip {i}-{j}").ToArray()
                });

            foreach (var chip in chips)
            {
                await Task.Yield();
                stack.Children.Add(chip);
            }

            stopwatch.Stop();

            FillLazyButton.Content = $"Fill Lazy: {stopwatch.ElapsedMilliseconds}";
        }
    }
}
