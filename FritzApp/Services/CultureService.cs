using System.Globalization;
using Microsoft.JSInterop;

namespace FritzApp.Services;

public class CultureService
{
    private readonly IJSRuntime _jsRuntime;
    private const string StorageKey = "FritzAppCulture";
    
    public event Action? CultureChanged;
    
    public CultureService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    public async Task<string> GetCultureAsync()
    {
        try
        {
            // Try to get stored culture preference
            var storedCulture = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            
            if (!string.IsNullOrEmpty(storedCulture))
            {
                return storedCulture;
            }
            
            // If no stored preference, try to get browser language
            var browserLanguage = await _jsRuntime.InvokeAsync<string>("eval", "navigator.language || navigator.userLanguage");
            
            // Map browser language to supported cultures
            if (browserLanguage.StartsWith("de", StringComparison.OrdinalIgnoreCase))
            {
                return "de";
            }
            
            // Default to English
            return "en";
        }
        catch
        {
            // Fallback to English if anything fails
            return "en";
        }
    }
    
    public async Task SetCultureAsync(string culture)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, culture);
        
        var cultureInfo = new CultureInfo(culture);
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        
        CultureChanged?.Invoke();
    }
    
    public async Task InitializeCultureAsync()
    {
        var culture = await GetCultureAsync();
        var cultureInfo = new CultureInfo(culture);
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }
}
