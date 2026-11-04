namespace Client.Components.Pages.Admin;

public sealed record OperatorRow(int Id, string Username, string Password, bool IsAdmin, IReadOnlyList<int> CampaignIds, DateTime? LastLogin);

public enum OperatorDialogMode
{
    Create,
    Edit,
    ResetPassword,
}
