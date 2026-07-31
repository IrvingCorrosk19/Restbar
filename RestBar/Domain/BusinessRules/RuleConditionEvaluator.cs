using System.Globalization;
using System.Text.Json;

namespace RestBar.Domain.BusinessRules;

public static class RuleConditionEvaluator
{
    public static bool EvaluateAll(
        IReadOnlyList<RuleConditionSpec> conditions,
        BrLogicGateRoot logic,
        IReadOnlyDictionary<string, object?> facts,
        List<string>? trace = null)
    {
        if (conditions.Count == 0)
        {
            trace?.Add("No conditions → match=false (fail-closed)");
            return false;
        }

        var results = new List<bool>(conditions.Count);
        foreach (var c in conditions.OrderBy(x => x.SortOrder))
        {
            facts.TryGetValue(c.FieldKey, out var fact);
            var ok = EvaluateOne(fact, c.Operator, c.ValueJson);
            if (c.Negate) ok = !ok;
            results.Add(ok);
            trace?.Add($"{(c.Negate ? "NOT " : "")}{c.FieldKey} {c.Operator} → {ok} (fact={Format(fact)})");
        }

        return logic == BrLogicGateRoot.And ? results.All(x => x) : results.Any(x => x);
    }

    public static bool EvaluateOne(object? fact, BrConditionOp op, string valueJson)
    {
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(valueJson) ? "null" : valueJson);
        var expected = doc.RootElement;

        return op switch
        {
            BrConditionOp.Eq => Compare(fact, expected) == 0,
            BrConditionOp.Neq => Compare(fact, expected) != 0,
            BrConditionOp.Gt => Compare(fact, expected) > 0,
            BrConditionOp.Gte => Compare(fact, expected) >= 0,
            BrConditionOp.Lt => Compare(fact, expected) < 0,
            BrConditionOp.Lte => Compare(fact, expected) <= 0,
            BrConditionOp.Contains => Contains(fact, expected),
            BrConditionOp.NotContains => !Contains(fact, expected),
            BrConditionOp.Between => Between(fact, expected),
            BrConditionOp.In => InList(fact, expected),
            BrConditionOp.NotIn => !InList(fact, expected),
            _ => false
        };
    }

    static bool Between(object? fact, JsonElement expected)
    {
        if (expected.ValueKind != JsonValueKind.Array || expected.GetArrayLength() < 2) return false;
        var lo = expected[0];
        var hi = expected[1];
        return Compare(fact, lo) >= 0 && Compare(fact, hi) <= 0;
    }

    static bool InList(object? fact, JsonElement expected)
    {
        if (expected.ValueKind != JsonValueKind.Array) return false;
        foreach (var el in expected.EnumerateArray())
            if (Compare(fact, el) == 0) return true;
        return false;
    }

    static bool Contains(object? fact, JsonElement expected)
    {
        var s = Convert.ToString(fact, CultureInfo.InvariantCulture) ?? "";
        var needle = expected.ValueKind == JsonValueKind.String ? expected.GetString() ?? "" : expected.ToString();
        return s.Contains(needle, StringComparison.OrdinalIgnoreCase);
    }

    static int Compare(object? fact, JsonElement expected)
    {
        if (fact is bool fb)
        {
            var eb = expected.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(expected.GetString(), out var b) && b,
                JsonValueKind.Number => expected.TryGetInt32(out var n) && n != 0,
                _ => false
            };
            return fb.CompareTo(eb);
        }
        if (TryDecimal(fact, out var fd) && TryDecimal(expected, out var ed))
            return fd.CompareTo(ed);
        var fs = Convert.ToString(fact, CultureInfo.InvariantCulture) ?? "";
        var es = expected.ValueKind == JsonValueKind.String ? expected.GetString() ?? "" : expected.ToString();
        return string.Compare(fs, es, StringComparison.OrdinalIgnoreCase);
    }

    static bool TryDecimal(object? fact, out decimal d)
    {
        d = 0;
        if (fact is null) return false;
        if (fact is decimal dd) { d = dd; return true; }
        if (fact is int i) { d = i; return true; }
        if (fact is long l) { d = l; return true; }
        if (fact is double db) { d = (decimal)db; return true; }
        if (fact is float f) { d = (decimal)f; return true; }
        return decimal.TryParse(Convert.ToString(fact, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out d);
    }

    static bool TryDecimal(JsonElement el, out decimal d)
    {
        d = 0;
        return el.ValueKind switch
        {
            JsonValueKind.Number => el.TryGetDecimal(out d),
            JsonValueKind.String => decimal.TryParse(el.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out d),
            _ => false
        };
    }

    static string Format(object? v) => v is null ? "null" : Convert.ToString(v, CultureInfo.InvariantCulture) ?? "";
}

public enum BrLogicGateRoot { And, Or }
public enum BrConditionOp { Eq, Neq, Gt, Gte, Lt, Lte, Contains, NotContains, Between, In, NotIn }

public sealed record RuleConditionSpec(int SortOrder, bool Negate, string FieldKey, BrConditionOp Operator, string ValueJson);

public sealed record RuleActionSpec(int SortOrder, string ActionType, string ParametersJson);

public static class RuleFlowCompiler
{
    /// <summary>
    /// Compiles a simple flow JSON into conditions/actions.
    /// Schema: { "logic":"AND|OR", "conditions":[...], "actions":[...] }
    /// </summary>
    public static (BrLogicGateRoot logic, List<RuleConditionSpec> conditions, List<RuleActionSpec> actions) Compile(string flowJson)
    {
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(flowJson) ? "{}" : flowJson);
        var root = doc.RootElement;
        var logic = BrLogicGateRoot.And;
        if (root.TryGetProperty("logic", out var lg) &&
            string.Equals(lg.GetString(), "OR", StringComparison.OrdinalIgnoreCase))
            logic = BrLogicGateRoot.Or;

        var conditions = new List<RuleConditionSpec>();
        if (root.TryGetProperty("conditions", out var conds) && conds.ValueKind == JsonValueKind.Array)
        {
            var i = 0;
            foreach (var c in conds.EnumerateArray())
            {
                var field = c.TryGetProperty("field", out var f) ? f.GetString() ?? "" : "";
                var opStr = c.TryGetProperty("op", out var o) ? o.GetString() ?? "eq" : "eq";
                var negate = c.TryGetProperty("not", out var n) && n.ValueKind == JsonValueKind.True;
                var value = c.TryGetProperty("value", out var v) ? v.GetRawText() : "null";
                conditions.Add(new RuleConditionSpec(i++, negate, field, ParseOp(opStr), value));
            }
        }

        var actions = new List<RuleActionSpec>();
        if (root.TryGetProperty("actions", out var acts) && acts.ValueKind == JsonValueKind.Array)
        {
            var i = 0;
            foreach (var a in acts.EnumerateArray())
            {
                var type = a.TryGetProperty("type", out var t) ? t.GetString() ?? "" : "";
                var parameters = a.TryGetProperty("params", out var p) ? p.GetRawText() : "{}";
                actions.Add(new RuleActionSpec(i++, type, parameters));
            }
        }

        return (logic, conditions, actions);
    }

    public static BrConditionOp ParseOp(string op) => op.Trim().ToLowerInvariant() switch
    {
        "eq" or "==" or "igual" => BrConditionOp.Eq,
        "neq" or "!=" or "distinto" => BrConditionOp.Neq,
        "gt" or ">" or "mayor" => BrConditionOp.Gt,
        "gte" or ">=" => BrConditionOp.Gte,
        "lt" or "<" or "menor" => BrConditionOp.Lt,
        "lte" or "<=" => BrConditionOp.Lte,
        "contains" or "contiene" => BrConditionOp.Contains,
        "notcontains" => BrConditionOp.NotContains,
        "between" or "entre" => BrConditionOp.Between,
        "in" => BrConditionOp.In,
        "notin" => BrConditionOp.NotIn,
        _ => BrConditionOp.Eq
    };
}
