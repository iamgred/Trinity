using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using MudBlazor;
using Client.Services;
using Campaign = Trinity.Shared.Models.Campaign;
using Operator = Trinity.Shared.Models.Operator;

namespace Client.Components.Pages.Admin;

public partial class Admin
{
    [Inject] private CampaignService CampaignService { get; set; } = null!;
    [Inject] private OperatorService OperatorService { get; set; } = null!;
    [Inject] private IHttpClientFactory HttpFactory { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private int _activeTab;
    private readonly DialogOptions _dialogOpts = new() { MaxWidth = MaxWidth.Small, FullWidth = true };

    private HttpClient Http => HttpFactory.CreateClient("TeamServer");

    // Campaign state
    private List<Campaign> _campaigns = [];
    private string _campaignSearch = string.Empty;
    private bool _loadingCampaigns;
    private bool _createCampaignOpen;
    private bool _savingCampaign;
    private string _newCampaignName = string.Empty;
    private string _newCampaignDesc = string.Empty;
    private string? _campaignError;

    // Operator state
    private List<Operator> _operators = [];
    private string _operatorSearch = string.Empty;
    private bool _loadingOperators;
    private bool _createOperatorOpen;
    private bool _savingOperator;
    private string _newOperatorName = string.Empty;
    private string _newOperatorPass = string.Empty;
    private string _newOperatorRole = "Operator";
    private string? _operatorError;

    private IEnumerable<Campaign> FilteredCampaigns =>
        string.IsNullOrWhiteSpace(_campaignSearch)
            ? _campaigns
            : _campaigns.Where(c => c.Name?.Contains(_campaignSearch, StringComparison.OrdinalIgnoreCase) == true);

    private IEnumerable<Operator> FilteredOperators =>
        string.IsNullOrWhiteSpace(_operatorSearch)
            ? _operators
            : _operators.Where(o => o.Username?.Contains(_operatorSearch, StringComparison.OrdinalIgnoreCase) == true);

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(LoadCampaignsAsync(), LoadOperatorsAsync());
    }

    private async Task LoadCampaignsAsync()
    {
        _loadingCampaigns = true;
        try { _campaigns = await CampaignService.GetTasksAsync(); }
        catch { _campaigns = []; }
        finally { _loadingCampaigns = false; }
    }

    private async Task LoadOperatorsAsync()
    {
        _loadingOperators = true;
        try { _operators = await OperatorService.GetTasksAsync(); }
        catch { _operators = []; }
        finally { _loadingOperators = false; }
    }

    private async Task CreateCampaignAsync()
    {
        if (string.IsNullOrWhiteSpace(_newCampaignName))
        {
            _campaignError = "Campaign name is required.";
            return;
        }
        _savingCampaign = true;
        _campaignError = null;
        try
        {
            var response = await Http.PostAsJsonAsync("/api/v1/campaigns",
                new { Name = _newCampaignName.Trim(), Description = _newCampaignDesc.Trim() });
            if (!response.IsSuccessStatusCode)
            {
                _campaignError = $"Server refused ({(int)response.StatusCode}).";
                return;
            }
            _createCampaignOpen = false;
            _newCampaignName = string.Empty;
            _newCampaignDesc = string.Empty;
            await LoadCampaignsAsync();
        }
        catch (HttpRequestException e) { _campaignError = $"Could not reach the server: {e.Message}"; }
        finally { _savingCampaign = false; }
    }

    private async Task DeleteCampaignAsync(Campaign campaign)
    {
        try
        {
            var response = await Http.DeleteAsync($"/api/v1/campaigns/{campaign.ID}");
            if (!response.IsSuccessStatusCode)
            {
                Snackbar.Add($"Delete failed ({(int)response.StatusCode}).", Severity.Error);
                return;
            }
            Snackbar.Add($"Campaign '{campaign.Name}' deleted.", Severity.Warning);
            await LoadCampaignsAsync();
        }
        catch (HttpRequestException e) { Snackbar.Add($"Could not reach the server: {e.Message}", Severity.Error); }
    }

    private async Task CreateOperatorAsync()
    {
        if (string.IsNullOrWhiteSpace(_newOperatorName) || string.IsNullOrWhiteSpace(_newOperatorPass))
        {
            _operatorError = "Username and password are required.";
            return;
        }
        _savingOperator = true;
        _operatorError = null;
        try
        {
            var response = await Http.PostAsJsonAsync("/api/v1/operators",
                new { Username = _newOperatorName.Trim(), Password = _newOperatorPass, Role = _newOperatorRole });
            if (!response.IsSuccessStatusCode)
            {
                _operatorError = $"Server refused ({(int)response.StatusCode}).";
                return;
            }
            _createOperatorOpen = false;
            _newOperatorName = string.Empty;
            _newOperatorPass = string.Empty;
            _newOperatorRole = "Operator";
            await LoadOperatorsAsync();
        }
        catch (HttpRequestException e) { _operatorError = $"Could not reach the server: {e.Message}"; }
        finally { _savingOperator = false; }
    }

    private async Task DeleteOperatorAsync(Operator op)
    {
        try
        {
            var response = await Http.DeleteAsync($"/api/v1/operators/{op.ID}");
            if (!response.IsSuccessStatusCode)
            {
                Snackbar.Add($"Delete failed ({(int)response.StatusCode}).", Severity.Error);
                return;
            }
            Snackbar.Add($"Operator '{op.Username}' removed.", Severity.Warning);
            await LoadOperatorsAsync();
        }
        catch (HttpRequestException e) { Snackbar.Add($"Could not reach the server: {e.Message}", Severity.Error); }
    }

    private static string FirstChar(string? username) =>
        string.IsNullOrWhiteSpace(username) ? "?" : username[0].ToString().ToUpperInvariant();
}