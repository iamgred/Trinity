using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;
using Client.Services;
using Client.Components.Pages.Dialogs;
using Tool = Trinity.Shared.Models.Tool;
using SysTask = System.Threading.Tasks.Task;

namespace Client.Components.Shell;

public partial class ArmouryPanel : ComponentBase
{
    [Inject] ArmouryService ArmouryService { get; set; } = null!;
    [Inject] IJSRuntime JS { get; set; } = null!;
    [Inject] IConfiguration Config { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;
    [Inject] EventLogService EventLog { get; set; } = null!;

    private List<Tool> _tools = [];
    private Tool? _selected;
    private int _page = 1;
    private int _rows = 8;
    private readonly HashSet<int> _copied = [];

    private int TotalPages => Math.Max(1, (int)Math.Ceiling(_tools.Count / (double)_rows));
    private IEnumerable<Tool> PagedTools => _tools.Skip((_page - 1) * _rows).Take(_rows);

    protected override async SysTask OnInitializedAsync() => await RefreshAsync();

    private async SysTask RefreshAsync()
    {
        _tools = await ArmouryService.GetToolsAsync();
        _selected = null;
        StateHasChanged();
    }

    private async SysTask CopyPath(Tool tool)
    {
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", tool.Path);
        _copied.Add(tool.ID);
        StateHasChanged();
        await SysTask.Delay(1800);
        _copied.Remove(tool.ID);
        StateHasChanged();
    }

    private bool IsCopied(Tool tool) => _copied.Contains(tool.ID);

    private async SysTask HandleAdd()
    {
        var dialog = await DialogService.ShowAsync<AddToolDialog>(
            string.Empty,
            new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true });
        var result = await dialog.Result;
        if (result is { Canceled: false, Data: (string name, IBrowserFile file) })
        {
            await using var stream = file.OpenReadStream(maxAllowedSize: 100 * 1024 * 1024);
            await ArmouryService.UploadAsync(name, 1, stream, file.Name);
            EventLog.Add("SUCCESS", $"Tool '{name}' added to armoury.");
            await RefreshAsync();
        }
    }

    private async SysTask HandleRemove()
    {
        if (_selected == null) return;
        var name = _selected.Name;
        await ArmouryService.RemoveAsync(_selected.ID);
        EventLog.Add("WARN", $"Tool '{name}' removed from armoury.");
        await RefreshAsync();
    }

    private static string TypeLabel(Tool tool)
    {
        var ext = Path.GetExtension(tool.Name).ToLowerInvariant();
        return ext switch
        {
            ".o" or ".obj" => "Object File",
            ".exe" or ".dll" => ".NET Assembly",
            ".ps1" => "PowerShell Script",
            ".py" => "Python Script",
            _ => ".NET Assembly"
        };
    }

    private static string TypeIcon(Tool tool)
    {
        var ext = Path.GetExtension(tool.Name).ToLowerInvariant();
        return ext switch
        {
            ".o" or ".obj" =>
                "<svg class=\"arm-type-ico arm-ico-bof\" xmlns=\"http://www.w3.org/2000/svg\" width=\"14\" height=\"14\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><polyline points=\"16 18 22 12 16 6\"/><polyline points=\"8 6 2 12 8 18\"/></svg>",
            ".ps1" =>
                "<svg class=\"arm-type-ico arm-ico-ps\" xmlns=\"http://www.w3.org/2000/svg\" width=\"14\" height=\"14\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><polyline points=\"4 17 10 11 4 5\"/><line x1=\"12\" y1=\"19\" x2=\"20\" y2=\"19\"/></svg>",
            _ =>
                "<svg class=\"arm-type-ico arm-ico-asm\" xmlns=\"http://www.w3.org/2000/svg\" width=\"14\" height=\"14\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><circle cx=\"12\" cy=\"12\" r=\"10\"/><circle cx=\"12\" cy=\"12\" r=\"4\"/><line x1=\"4.93\" y1=\"4.93\" x2=\"9.17\" y2=\"9.17\"/><line x1=\"14.83\" y1=\"14.83\" x2=\"19.07\" y2=\"19.07\"/><line x1=\"14.83\" y1=\"9.17\" x2=\"19.07\" y2=\"4.93\"/><line x1=\"4.93\" y1=\"19.07\" x2=\"9.17\" y2=\"14.83\"/></svg>"
        };
    }

    private static string FormatSize(double bytes)
    {
        if (bytes < 1024) return $"{bytes:F0} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024:F1} KB";
        return $"{bytes / (1024 * 1024):F1} MB";
    }
}
