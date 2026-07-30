using RestBar.Domain.Cash;
using RestBar.Infrastructure.Cash;
using RestBar.Models;

namespace RestBar.Tests.Cash;

public class CashSessionStateMachineTests
{
    [Theory]
    [InlineData(CashSessionStatus.Prepared, CashSessionStatus.Open, true)]
    [InlineData(CashSessionStatus.Open, CashSessionStatus.Operating, true)]
    [InlineData(CashSessionStatus.Operating, CashSessionStatus.Counting, true)]
    [InlineData(CashSessionStatus.Counting, CashSessionStatus.Reconciling, true)]
    [InlineData(CashSessionStatus.Reconciling, CashSessionStatus.Closed, true)]
    [InlineData(CashSessionStatus.Closed, CashSessionStatus.Open, true)]
    [InlineData(CashSessionStatus.Closed, CashSessionStatus.Operating, false)]
    [InlineData(CashSessionStatus.Historical, CashSessionStatus.Open, false)]
    public void CanTransition_MatchesDesign(CashSessionStatus from, CashSessionStatus to, bool expected)
    {
        Assert.Equal(expected, CashSessionStateMachine.CanTransition(from, to));
    }

    [Theory]
    [InlineData(CashSessionStatus.Open, true)]
    [InlineData(CashSessionStatus.Operating, true)]
    [InlineData(CashSessionStatus.Counting, false)]
    [InlineData(CashSessionStatus.Closed, false)]
    public void AllowsPayments_OnlyOpenAndOperating(CashSessionStatus status, bool expected)
    {
        Assert.Equal(expected, CashSessionStateMachine.AllowsPayments(status));
    }

    [Fact]
    public void EnsureTransition_Invalid_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CashSessionStateMachine.EnsureTransition(CashSessionStatus.Historical, CashSessionStatus.Open));
    }
}

public class CashHashChainTests
{
    [Fact]
    public void MovementHash_IsDeterministic()
    {
        var movement = new CashMovement
        {
            CashSessionId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            SequenceNumber = 1,
            MovementType = CashMovementType.OpeningFloat,
            Direction = CashMovementDirection.In,
            Amount = 100m,
            CreatedAtUtc = new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc)
        };

        var h1 = CashHashChainBuilder.ComputeMovementHash(movement, null);
        var h2 = CashHashChainBuilder.ComputeMovementHash(movement, null);
        Assert.Equal(h1, h2);
        Assert.Equal(64, h1.Length);
    }

    [Fact]
    public void MovementHash_ChangesWithPreviousHash()
    {
        var movement = new CashMovement
        {
            CashSessionId = Guid.NewGuid(),
            SequenceNumber = 2,
            MovementType = CashMovementType.SaleCash,
            Direction = CashMovementDirection.In,
            Amount = 25m,
            CreatedAtUtc = DateTime.UtcNow
        };

        var h1 = CashHashChainBuilder.ComputeMovementHash(movement, "abc");
        var h2 = CashHashChainBuilder.ComputeMovementHash(movement, "def");
        Assert.NotEqual(h1, h2);
    }
}
