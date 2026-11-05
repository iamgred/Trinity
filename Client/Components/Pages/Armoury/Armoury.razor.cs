using Microsoft.AspNetCore.Components;

namespace Client.Components.Pages;

public partial class Armoury : ComponentBase
{
    private record ArmouryItem(string Name, string Type, string Path, string Size, string AddedAt);

    private List<ArmouryItem> _items = new();
}