using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Text.Json;
using Client.Components.Layout;
using Client.Components.Pages.Dialogs;
using Client.Components.Shell;
using Client.Components.UI;
using Client.Services;
using MudBlazor;
using Campaign = Trinity.Shared.Models.Campaign;
using Operator = Trinity.Shared.Models.Operator;

namespace Client.Components.Pages;

public partial class Campaigns
{
    [CascadingParameter] private AdminLayout Layout { get; set; } = default!;

    private List<Campaign> _campaigns = [];
    private List<Operator> _operators = [];
    private readonly Dictionary<int, List<Operator>> _assigned = [];
    private Campaign? _selected;
    private string? _error;
    private bool _loading;
    private bool _assignOpen;

    private bool _renameOpen;
    private string _renameValue = "";
    private string? _renameError;

    private HttpClient Http => HttpFactory.CreateClient("TeamServer");

    private IEnumerable<Operator> AssignedOperators =>
        _selected is not null && _assigned.TryGetValue(_selected.ID, out var ops) ? ops : [];

    private IEnumerable<Operator> UnassignedOperators =>
        _operators.Where(o => AssignedOperators.All(a => a.ID != o.ID));

    private int OperatorCount(Campaign campaign) =>
        _assigned.TryGetValue(campaign.ID, out var ops) ? ops.Count : 0;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        try
        {
            _campaigns = await CampaignService.GetTasksAsync();
            _operators = await OperatorService.GetTasksAsync();
            _assigned.Clear();
            foreach (var campaign in _campaigns)
                _assigned[campaign.ID] = await LoadAssignedAsync(campaign.ID);

            _selected = _campaigns.FirstOrDefault(c => c.ID == _selected?.ID) ?? _campaigns.FirstOrDefault();
            _error = null;
            Layout.SetCounts(campaigns: _campaigns.Count, operators: _operators.Count);
        }
        catch (Exception e) when (e is HttpRequestException or JsonException or TaskCanceledException)
        {
            _error = ServerDown.Is(e) ? ServerDown.Message : $"Could not load campaigns: {e.Message}";
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task<List<Operator>> LoadAssignedAsync(int campaignId) =>
        await Http.GetFromJsonAsync<List<Operator>>($"/api/v1/campaigns/{campaignId}/operators") ?? [];

    private Task SelectAsync(Campaign campaign)
    {
        _selected = campaign;
        _assignOpen = false;
        return Task.CompletedTask;
    }

    private async Task NewCampaignAsync()
    {
        var dialog = await DialogService.ShowAsync<CreateNewCampaignDialog>(
            string.Empty,
            new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true });
        await dialog.Result;
        await LoadAsync();
    }

    private async Task AssignAsync(Operator op) => await ChangeAssignmentAsync(op, assign: true);
    private async Task UnassignAsync(Operator op) => await ChangeAssignmentAsync(op, assign: false);

    private async Task ChangeAssignmentAsync(Operator op, bool assign)
    {
        if (_selected is null) return;

        var url = $"/api/v1/campaigns/{_selected.ID}/operators/{op.ID}";
        try
        {
            var response = assign
                ? await Http.PostAsync(url, null)
                : await Http.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _error = $"The team server refused the change ({(int)response.StatusCode}).";
                return;
            }

            _assigned[_selected.ID] = await LoadAssignedAsync(_selected.ID);
            _assignOpen = false;
            _error = null;
        }
        catch (HttpRequestException e)
        {
            _error = ServerDown.Is(e) ? ServerDown.Message : $"Could not reach the team server: {e.Message}";
        }
    }

    private void OpenRename()
    {
        _renameValue = _selected?.Name ?? "";
        _renameError = null;
        _renameOpen = true;
    }

    private async Task RenameAsync()
    {
        if (_selected is null || string.IsNullOrWhiteSpace(_renameValue))
        {
            _renameError = "Enter a name.";
            return;
        }

        try
        {
            var response = await Http.PutAsJsonAsync(
                $"/api/v1/campaigns/{_selected.ID}",
                new Campaign { ID = _selected.ID, Name = _renameValue.Trim() });

            if (!response.IsSuccessStatusCode)
            {
                _renameError = $"The team server refused the rename ({(int)response.StatusCode}).";
                return;
            }

            _renameOpen = false;
            await LoadAsync();
        }
        catch (HttpRequestException e)
        {
            _renameError = ServerDown.Is(e) ? ServerDown.Message : $"Could not reach the team server: {e.Message}";
        }
    }
}