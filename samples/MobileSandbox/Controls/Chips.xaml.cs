using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ControlCatalog.Controls;

public partial class Chips : ItemsRepeater
{
    public Chips()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
