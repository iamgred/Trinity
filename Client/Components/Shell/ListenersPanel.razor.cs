using Microsoft.AspNetCore.Components;
using MudBlazor;
using Client.Services;
using Client.Components.Pages.Dialogs;
using SysTask = System.Threading.Tasks.Task;

namespace Client.Components.Shell;

public partial class ListenersPanel : ComponentBase
{
    [Inject] ListenerApiClient ListenerClient { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;

    private IReadOnlyList<ListenerResponse> _listeners = [];
    private ListenerResponse? _selected;

    protected override async SysTask OnInitializedAsync() => await RefreshAsync();

    private async SysTask RefreshAsync()
    {
        _listeners = await ListenerClient.GetListenersAsync();
        _selected = null;
        StateHasChanged();
    }

    private async SysTask HandleAdd()
    {
        var dialog = await DialogService.ShowAsync<NewListenerDialog>(
            string.Empty,
            new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true });
        var result = await dialog.Result;
        if (result is { Canceled: false, Data: NewListenerDialog.Result r })
        {
            bool ok;
            if (r.IsHttps)
                ok = await ListenerClient.CreateHttpsListenerAsync(r.Name, r.Hosts, r.C2Port, r.UserAgent, r.Header, r.RotationStrategy);
            else
                ok = await ListenerClient.CreateHttpListenerAsync(r.Name, r.Hosts, r.BindPort, r.C2Port, r.UserAgent, r.Header, r.RotationStrategy);

            if (ok) await RefreshAsync();
        }
    }

    private async SysTask HandleEdit()
    {
        if (_selected == null) return;
        var details = await ListenerClient.GetListenerDetailsAsync(_selected.id);
        if (details == null) return;

        var parameters = new DialogParameters<NewListenerDialog>
        {
            { x => x.IsEdit,          true            },
            { x => x.InitialName,     details.Name    },
            { x => x.InitialType,     _selected.protocol },
            { x => x.InitialHosts,    details.Hosts   },
            { x => x.InitialC2Port,   details.C2Port  },
            { x => x.InitialBindPort, details.BindPort },
            { x => x.InitialRotation, details.RotationStrategy },
            { x => x.InitialUserAgent, details.UserAgent },
            { x => x.InitialHeader,   details.Header  },
        };

        var dialog = await DialogService.ShowAsync<NewListenerDialog>(
            string.Empty, parameters,
            new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true });
        var result = await dialog.Result;
        if (result is { Canceled: false, Data: NewListenerDialog.Result r })
        {
            bool ok;
            if (r.IsHttps)
                ok = await ListenerClient.UpdateHttpsAsync(_selected.id, r.Hosts, r.C2Port, r.UserAgent, r.Header, r.RotationStrategy);
            else
                ok = await ListenerClient.UpdateHttpAsync(_selected.id, r.Hosts, r.BindPort, r.C2Port, r.UserAgent, r.Header, r.RotationStrategy);

            if (ok) await RefreshAsync();
        }
    }

    private async SysTask HandleRestart()
    {
        if (_selected == null) return;
        await ListenerClient.RestartListenerAsync(_selected.id);
        await RefreshAsync();
    }

    private async SysTask HandleRemove()
    {
        if (_selected == null) return;
        await ListenerClient.DeleteListenerAsync(_selected.id);
        await RefreshAsync();
    }

    private static string TypeIcon(ListenerResponse l)
    {
        var proto = l.protocol?.ToUpperInvariant() ?? "";
        return proto switch
        {
            "HTTPS" =>
                "<svg class=\"lp-type-ico lp-ico-https\" xmlns=\"http://www.w3.org/2000/svg\" width=\"14\" height=\"14\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><rect x=\"3\" y=\"11\" width=\"18\" height=\"11\" rx=\"2\" ry=\"2\"/><path d=\"M7 11V7a5 5 0 0 1 10 0v4\"/></svg>",
            "HTTP" =>
                "<svg class=\"lp-type-ico lp-ico-http\" xmlns=\"http://www.w3.org/2000/svg\" width=\"14\" height=\"14\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M1 6l4 6-4 6\"/><path d=\"M23 6l-4 6 4 6\"/><line x1=\"8\" y1=\"12\" x2=\"16\" y2=\"12\"/></svg>",
            _ =>
                "<svg class=\"lp-type-ico lp-ico-other\" xmlns=\"http://www.w3.org/2000/svg\" width=\"14\" height=\"14\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><circle cx=\"12\" cy=\"12\" r=\"10\"/><line x1=\"12\" y1=\"8\" x2=\"12\" y2=\"12\"/><line x1=\"12\" y1=\"16\" x2=\"12.01\" y2=\"16\"/></svg>"
        };
    }
}
