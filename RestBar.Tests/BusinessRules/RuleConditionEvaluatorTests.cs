using RestBar.Domain.BusinessRules;

namespace RestBar.Tests.BusinessRules;

public class RuleConditionEvaluatorTests
{
    [Fact]
    public void Gt_matches_numeric()
    {
        var facts = new Dictionary<string, object?> { ["cash.varianceAbs"] = 60m };
        var conds = new[] { new RuleConditionSpec(0, false, "cash.varianceAbs", BrConditionOp.Gt, "50") };
        Assert.True(RuleConditionEvaluator.EvaluateAll(conds, BrLogicGateRoot.And, facts));
    }

    [Fact]
    public void And_requires_all()
    {
        var facts = new Dictionary<string, object?> { ["a"] = 1m, ["b"] = 0m };
        var conds = new[]
        {
            new RuleConditionSpec(0, false, "a", BrConditionOp.Gt, "0"),
            new RuleConditionSpec(1, false, "b", BrConditionOp.Gt, "0"),
        };
        Assert.False(RuleConditionEvaluator.EvaluateAll(conds, BrLogicGateRoot.And, facts));
        Assert.True(RuleConditionEvaluator.EvaluateAll(conds, BrLogicGateRoot.Or, facts));
    }

    [Fact]
    public void Not_negates()
    {
        var facts = new Dictionary<string, object?> { ["x"] = 5m };
        var conds = new[] { new RuleConditionSpec(0, true, "x", BrConditionOp.Gt, "10") };
        Assert.True(RuleConditionEvaluator.EvaluateAll(conds, BrLogicGateRoot.And, facts));
    }

    [Fact]
    public void Between_works()
    {
        Assert.True(RuleConditionEvaluator.EvaluateOne(15m, BrConditionOp.Between, "[10,20]"));
        Assert.False(RuleConditionEvaluator.EvaluateOne(25m, BrConditionOp.Between, "[10,20]"));
    }

    [Fact]
    public void Bool_eq_true()
    {
        Assert.True(RuleConditionEvaluator.EvaluateOne(true, BrConditionOp.Eq, "true"));
    }

    [Fact]
    public void Empty_conditions_fail_closed()
    {
        Assert.False(RuleConditionEvaluator.EvaluateAll([], BrLogicGateRoot.And, new Dictionary<string, object?>()));
    }

    [Fact]
    public void Compiler_parses_template_stock()
    {
        var tpl = BusinessRuleTemplates.All.First(t => t.Code == "STOCK_CRITICAL");
        var (logic, conds, acts) = RuleFlowCompiler.Compile(tpl.FlowJson);
        Assert.Equal(BrLogicGateRoot.And, logic);
        Assert.NotEmpty(conds);
        Assert.NotEmpty(acts);
        Assert.Contains(acts, a => a.ActionType == "CreateAlert");
    }

    [Fact]
    public void Compiler_rejects_unknown_via_enum_in_engine_validate()
    {
        var json = """{"logic":"AND","conditions":[{"field":"a","op":"gt","value":1}],"actions":[{"type":"DeleteEverything","params":{}}]}""";
        var (_, _, acts) = RuleFlowCompiler.Compile(json);
        Assert.False(Enum.TryParse<RestBar.Models.BrActionType>(acts[0].ActionType, true, out _));
    }
}
