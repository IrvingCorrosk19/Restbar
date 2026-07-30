using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using RestBar.Infrastructure.Foundation;

namespace RestBar.Tests.Foundation;

public class TenantScopeTests
{
    [Fact]
    public void FromPrincipal_Unauthenticated_ReturnsEmptyScope()
    {
        var scope = TenantScope.FromPrincipal(new ClaimsPrincipal());
        Assert.False(scope.IsAuthenticated);
        Assert.Null(scope.CompanyId);
    }

    [Fact]
    public void CanAccessCompany_SameCompany_Allows()
    {
        var companyId = Guid.NewGuid();
        var scope = Scope("admin", companyId, Guid.NewGuid());
        Assert.True(scope.CanAccessCompany(companyId));
    }

    [Fact]
    public void CanAccessCompany_OtherCompany_Denies()
    {
        var scope = Scope("admin", Guid.NewGuid(), Guid.NewGuid());
        Assert.False(scope.CanAccessCompany(Guid.NewGuid()));
    }

    [Fact]
    public void CanAccessCompany_SuperAdmin_AllowsAny()
    {
        var scope = Scope("superadmin", Guid.NewGuid(), Guid.NewGuid());
        Assert.True(scope.CanAccessCompany(Guid.NewGuid()));
    }

    [Fact]
    public void CanAccessCompany_NullResource_FailsClosed()
    {
        var scope = Scope("admin", Guid.NewGuid(), Guid.NewGuid());
        Assert.False(scope.CanAccessCompany(null));
    }

    [Fact]
    public void EnsureCompanyAccess_ThrowsWhenDenied()
    {
        var scope = Scope("waiter", Guid.NewGuid(), Guid.NewGuid());
        Assert.Throws<UnauthorizedAccessException>(() => scope.EnsureCompanyAccess(Guid.NewGuid()));
    }

    private static TenantScope Scope(string role, Guid companyId, Guid branchId)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("UserRole", role),
            new Claim("CompanyId", companyId.ToString()),
            new Claim("BranchId", branchId.ToString()),
            new Claim("UserId", Guid.NewGuid().ToString())
        }, authenticationType: "Test");

        return TenantScope.FromPrincipal(new ClaimsPrincipal(identity));
    }
}

public class SeedEnvironmentGateTests
{
    [Theory]
    [InlineData("Development", true)]
    [InlineData("Production", false)]
    [InlineData("Staging", false)]
    public void IsSeedAllowed_OnlyDevelopment(string envName, bool expected)
    {
        var env = new FakeEnv(envName);
        Assert.Equal(expected, SeedEnvironmentGate.IsSeedAllowed(env));
    }

    private sealed class FakeEnv : IWebHostEnvironment
    {
        public FakeEnv(string name) => EnvironmentName = name;
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "RestBar";
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

public class FeatureFlagsTests
{
    [Fact]
    public void Defaults_KeepFutureModulesOff()
    {
        var flags = new FeatureFlags();
        Assert.False(flags.EnableSupplierUi);
        Assert.False(flags.EnableCashModule);
        Assert.False(flags.EnablePurchasingModule);
        Assert.False(flags.EnableFoodCostModule);
        Assert.False(flags.EnableCommandCenter);
        Assert.False(flags.EnableCopilot);
        Assert.False(flags.EnableReportExports);
        Assert.False(flags.EnableBackupExecution);
    }
}
