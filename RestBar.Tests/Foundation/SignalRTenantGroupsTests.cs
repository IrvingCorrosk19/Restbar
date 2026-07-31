using RestBar.Infrastructure.Foundation;

namespace RestBar.Tests.Foundation;

public class SignalRTenantGroupsTests
{
    [Fact]
    public void Kitchen_IsCompanyScoped_AndDistinctPerCompany()
    {
        var a = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var b = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        Assert.Equal($"c_{a:N}_kitchen", SignalRTenantGroups.Kitchen(a));
        Assert.NotEqual(SignalRTenantGroups.Kitchen(a), SignalRTenantGroups.Kitchen(b));
        Assert.StartsWith("c_", SignalRTenantGroups.Kitchen(a));
        Assert.EndsWith("_kitchen", SignalRTenantGroups.Kitchen(a));
    }

    [Fact]
    public void Station_IncludesTypeAndCompany()
    {
        var c = Guid.NewGuid();
        Assert.Equal($"c_{c:N}_station_bar", SignalRTenantGroups.Station(c, "Bar"));
    }

    [Fact]
    public void Orders_Stock_Cash_TableAll_AreScoped()
    {
        var c = Guid.NewGuid();
        Assert.Contains(c.ToString("N"), SignalRTenantGroups.Orders(c));
        Assert.Contains(c.ToString("N"), SignalRTenantGroups.Stock(c));
        Assert.Contains(c.ToString("N"), SignalRTenantGroups.CashDashboard(c));
        Assert.Contains(c.ToString("N"), SignalRTenantGroups.TableAll(c));
    }
}
