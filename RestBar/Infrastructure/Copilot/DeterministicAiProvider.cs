using System.Text;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Infrastructure.Copilot;

/// <summary>
/// v1 provider: sintetiza respuestas a partir de tool results. Sin vendor LLM.
/// Preparado para sustituir por OpenAI/Azure/Claude vía IAiProvider.
/// </summary>
public class DeterministicAiProvider : IAiProvider
{
    public string Name => "Deterministic";

    public Task<string> CompleteAsync(
        string systemPrompt,
        string userMessage,
        IReadOnlyList<CopilotToolResult> tools,
        CancellationToken ct = default)
    {
        var sb = new StringBuilder();
        sb.AppendLine("### Director Operativo RestBar");
        sb.AppendLine();

        var denied = tools.Where(t => !t.Allowed).ToList();
        var ok = tools.Where(t => t.Allowed).ToList();

        if (ok.Count == 0 && denied.Count > 0)
        {
            sb.AppendLine("No tengo permiso para consultar esos datos con tu rol actual.");
            sb.AppendLine("Solicita acceso ReportAccess / CostingAccess / PurchasingAccess / CashAccess según corresponda.");
            return Task.FromResult(sb.ToString());
        }

        if (ok.Count == 0)
        {
            sb.AppendLine("Puedo ayudarte a dirigir la operación. Pregunta, por ejemplo:");
            sb.AppendLine("- ¿Cómo está mi empresa hoy?");
            sb.AppendLine("- ¿Por qué subió el Food Cost?");
            sb.AppendLine("- ¿Qué debería hacer ahora?");
            sb.AppendLine("- ¿Cómo está la caja?");
            sb.AppendLine("- ¿Qué compras tengo pendientes?");
            return Task.FromResult(sb.ToString());
        }

        foreach (var t in ok)
        {
            sb.AppendLine(t.PayloadMarkdown);
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine("*Cifras obtenidas solo de motores RestBar (tools). No inventadas.*");
        return Task.FromResult(sb.ToString().Trim());
    }
}
