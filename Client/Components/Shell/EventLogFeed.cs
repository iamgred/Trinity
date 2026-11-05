using System.Net.Http.Json;
using System.Text.Json;
using Client.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Trinity.Shared.DTOs.Notification;
using Campaign = Trinity.Shared.Models.Campaign;
using Operator = Trinity.Shared.Models.Operator;

namespace Client.Components.Shell;

// live event log from the team server's SignalR hub (/register)
// owned by WorkspaceLayout so events keep coming in while you are on other tabs
public sealed class EventLogFeed : IAsyncDisposable
{
    private readonly WorkspaceState _state;
    private readonly IHttpClientFactory _httpFactory;
    private readonly string _baseUrl;
    private HubConnection? _hub;
    private List<int> _campaignIds = [];

    public EventLogFeed(WorkspaceState state, IHttpClientFactory httpFactory, string baseUrl)
    {
        _state = state;
        _httpFactory = httpFactory;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task StartAsync(ShellUser user)
    {
        try
        {
            _campaignIds = await FindMyCampaignsAsync(user);
            if (_campaignIds.Count == 0)
            {
                _state.EventsError = "You are not on any campaign yet, so there are no events to show.";
                _state.Notify();
                return;
            }

            _hub = new HubConnectionBuilder()
                .WithUrl($"{_baseUrl}/register")
                .WithAutomaticReconnect()
                .Build();

            _hub.On<EventLog>("EventLogNotification", OnEvent);

            _hub.Reconnected += async _ =>
            {
                await JoinAllAsync();
                SetLive(true);
            };
            _hub.Reconnecting += _ =>
            {
                SetLive(false);
                return Task.CompletedTask;
            };
            _hub.Closed += _ =>
            {
                SetLive(false);
                return Task.CompletedTask;
            };

            await _hub.StartAsync();
            await JoinAllAsync();
            _state.EventsError = null;
            SetLive(true);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException or InvalidOperationException)
        {
            _state.EventsError = ServerDown.Is(ex) ? ServerDown.Message : $"Couldn't connect to the live event log: {ex.Message}";
            SetLive(false);
        }
    }

    private void OnEvent(EventLog entry)
    {
        _state.Events.Add(entry);

        if (_state.Events.Count > 500)
        {
            _state.Events.RemoveRange(0, _state.Events.Count - 500);
        }

        _state.Notify();
    }

    private async Task JoinAllAsync()
    {
        foreach (var id in _campaignIds)
        {
            await _hub!.InvokeAsync("SubscribeToEventLog", id.ToString());
        }
    }

    private void SetLive(bool live)
    {
        _state.EventsLive = live;
        _state.Notify();
    }

    // admins see every campaign, operators only the ones they are assigned to
    // TODO(connect to backend): sign-in doesn't keep the operator ID or their campaigns, so work it out from the lists
    private async Task<List<int>> FindMyCampaignsAsync(ShellUser user)
    {
        var http = _httpFactory.CreateClient("TeamServer");
        var campaigns = await http.GetFromJsonAsync<List<Campaign>>("/api/v1/campaigns") ?? [];
        if (user.IsAdmin)
        {
            return campaigns.Select(c => c.ID).ToList();
        }

        var mine = new List<int>();
        foreach (var campaign in campaigns)
        {
            var operators = await http.GetFromJsonAsync<List<Operator>>($"/api/v1/campaigns/{campaign.ID}/operators") ?? [];
            if (operators.Any(o => o.Username.Equals(user.Name, StringComparison.OrdinalIgnoreCase)))
            {
                mine.Add(campaign.ID);
            }
        }

        return mine;
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub is null)
        {
            return;
        }

        try
        {
            if (_hub.State == HubConnectionState.Connected)
            {
                foreach (var id in _campaignIds)
                {
                    await _hub.InvokeAsync("UnsubscribeFromEventLog", id.ToString());
                }
            }
        }
        catch (Exception)
        {
            // leaving anyway, the server drops the rooms when the connection closes
        }

        await _hub.DisposeAsync();
    }
}
