using RestBar.Domain.Procurement;
using RestBar.Models;

namespace RestBar.Tests.Procurement;

public class PurchaseOrderStateMachineTests
{
    [Theory]
    [InlineData(PurchaseOrderStatus.Draft, PurchaseOrderStatus.PendingApproval, true)]
    [InlineData(PurchaseOrderStatus.PendingApproval, PurchaseOrderStatus.Approved, true)]
    [InlineData(PurchaseOrderStatus.Approved, PurchaseOrderStatus.Sent, true)]
    [InlineData(PurchaseOrderStatus.Sent, PurchaseOrderStatus.PartiallyReceived, true)]
    [InlineData(PurchaseOrderStatus.Sent, PurchaseOrderStatus.FullyReceived, true)]
    [InlineData(PurchaseOrderStatus.FullyReceived, PurchaseOrderStatus.Closed, true)]
    [InlineData(PurchaseOrderStatus.Closed, PurchaseOrderStatus.Sent, false)]
    [InlineData(PurchaseOrderStatus.Cancelled, PurchaseOrderStatus.Approved, false)]
    public void CanTransition(PurchaseOrderStatus from, PurchaseOrderStatus to, bool expected)
    {
        Assert.Equal(expected, PurchaseOrderStateMachine.CanTransition(from, to));
    }

    [Theory]
    [InlineData(PurchaseOrderStatus.Sent, true)]
    [InlineData(PurchaseOrderStatus.PartiallyReceived, true)]
    [InlineData(PurchaseOrderStatus.Approved, true)]
    [InlineData(PurchaseOrderStatus.Closed, false)]
    public void CanReceive(PurchaseOrderStatus status, bool expected)
    {
        Assert.Equal(expected, PurchaseOrderStateMachine.CanReceive(status));
    }
}

public class PurchaseRequestStateMachineTests
{
    [Theory]
    [InlineData(PurchaseRequestStatus.Draft, PurchaseRequestStatus.Pending, true)]
    [InlineData(PurchaseRequestStatus.Pending, PurchaseRequestStatus.Approved, true)]
    [InlineData(PurchaseRequestStatus.Approved, PurchaseRequestStatus.Converted, true)]
    [InlineData(PurchaseRequestStatus.Rejected, PurchaseRequestStatus.Approved, false)]
    public void CanTransition(PurchaseRequestStatus from, PurchaseRequestStatus to, bool expected)
    {
        Assert.Equal(expected, PurchaseRequestStateMachine.CanTransition(from, to));
    }
}

public class CostEngineMathTests
{
    [Fact]
    public void WAC_FromZeroStock_EqualsUnitCost()
    {
        Assert.Equal(10m, CostEngineMath.ComputeMovingAverage(0, 5, 100, 10));
    }

    [Fact]
    public void WAC_BlendsCorrectly()
    {
        // 10 @ $5 + 10 @ $7 = 20 @ $6
        Assert.Equal(6m, CostEngineMath.ComputeMovingAverage(10, 5, 10, 7));
    }

    [Fact]
    public void OverallScore_Weights()
    {
        var score = CostEngineMath.ComputeOverallScore(100, 100, 100, 100);
        Assert.Equal(100m, score);
    }

    [Fact]
    public void OverallScore_WeightedMix()
    {
        // 0.25*80 + 0.30*90 + 0.25*70 + 0.20*60 = 20+27+17.5+12 = 76.5
        Assert.Equal(76.5m, CostEngineMath.ComputeOverallScore(80, 90, 70, 60));
    }
}
