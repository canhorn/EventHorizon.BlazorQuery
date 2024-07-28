namespace EventHorizon.BlazorQuery.Web.Components;

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

public partial class BlazorQuery<TArg, TResult>
    where TResult : new()
{
    [Parameter]
    public required BlazorDataQuery<TArg, TResult> DataQuery { get; set; }

    [Parameter]
    public TArg? Args { get; set; }

    [Parameter]
    public BlazorDataQueryOptions Options { get; set; } = new();

    [Parameter]
    public RenderFragment<BlazorData<TArg, TResult>>? ErrorTemplate { get; set; }

    [Parameter]
    public RenderFragment<BlazorData<TArg, TResult>>? LoadingTemplate { get; set; }

    [Parameter]
    public required RenderFragment<BlazorData<TArg, TResult>> ChildContent { get; set; }

    protected BlazorData<TArg, TResult> Data { get; set; } = BlazorData<TArg, TResult>.Empty;

    protected override async Task OnInitializedAsync()
    {
        Data = DataQuery.Query(Args);
        DataQuery.StateChanged += StateHasChanged;

        if (Options.Trigger == DataQueryTrigger.OnInitialized)
        {
            await Data.LoadData();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Options.Trigger == DataQueryTrigger.OnFirstRender)
        {
            await Data.LoadData();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        await Data.SetArgs(Args);
    }
}

public record BlazorDataQueryOptions
{
    public TimeSpan CacheDuration { get; init; } = TimeSpan.FromMinutes(5);
    public TimeSpan DefaultStaleDuration { get; init; } = TimeSpan.Zero;

    public DataQueryTrigger Trigger { get; init; } = DataQueryTrigger.OnInitialized;
}

public enum DataQueryTrigger
{
    Manual,
    OnInitialized,
    OnFirstRender,
}

public class BlazorDataQuery<TArgs, TResult>(Func<TArgs?, Task<TResult>> queryFunc)
    where TResult : new()
{
    private readonly Func<TArgs?, Task<TResult>> _queryFunc = queryFunc;
    public event Action StateChanged = () => { };

    public BlazorData<TArgs, TResult> Query(TArgs? args)
    {
        return new BlazorData<TArgs, TResult>(_queryFunc) { Args = args, };
    }
}

public record BlazorData<TArgs, TResult>(Func<TArgs?, Task<TResult>> QueryFunc)
    where TResult : new()
{
    public static readonly BlazorData<TArgs, TResult> Empty =
        new(static _ => Task.FromResult(new TResult()));

    public TArgs? Args { get; set; }
    public TResult Data { get; set; } = new();

    public bool IsError { get; set; }
    public bool IsLoading { get; set; } = true;
    public bool HasData => Data is not null;

    [MemberNotNullWhen(true, nameof(ErrorMessage))]
    public string? ErrorMessage { get; set; } = string.Empty;

    [MemberNotNullWhen(true, nameof(ErrorMessage))]
    public Exception? Exception { get; set; }

    public async Task LoadData()
    {
        IsLoading = true;
        IsError = false;

        Exception = null;
        ErrorMessage = string.Empty;

        try
        {
            Data = await QueryFunc(Args);
        }
        catch (Exception ex)
        {
            Exception = ex;
            ErrorMessage = ex.Message;
            IsError = true;
        }

        IsLoading = false;
    }

    public async Task SetArgs(TArgs? args)
    {
        Args = args;
        await LoadData();
    }
}
