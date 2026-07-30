using System.Text.RegularExpressions;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.Copilot;

public class CopilotIntentClassifier : ICopilotIntentClassifier
{
    public CopilotIntent Classify(string message)
    {
        var m = message.ToLowerInvariant();

        if (Regex.IsMatch(m, @"ayuda|help|qu[eé] puedes|what can you"))
            return CopilotIntent.Help;

        if (Regex.IsMatch(m, @"qu[eé] deber[ií]a|what should i|decisiones|prioriz|hoy debo|atender primero"))
            return CopilotIntent.WhatShouldIDo;

        if (Regex.IsMatch(m, @"c[oó]mo est[aá] mi empresa|resumen ejecutivo|executive|command center|estado general|how is (my )?business"))
            return CopilotIntent.ExecutiveBriefing;

        if (Regex.IsMatch(m, @"food\s*cost|costo de comida|por qu[eé] aument|porque aument|aument[oó] el (costo|food)|varianza"))
            return CopilotIntent.FoodCostWhy;

        if (Regex.IsMatch(m, @"desperdicio|waste|merma"))
            return CopilotIntent.WasteStatus;

        if (Regex.IsMatch(m, @"proveedor|supplier|negociar contrato"))
            return CopilotIntent.SupplierAdvice;

        if (Regex.IsMatch(m, @"compra|purchase|po\b|orden de compra|reposici[oó]n|qu[eé] comprar"))
            return CopilotIntent.PurchasingWhat;

        if (Regex.IsMatch(m, @"crear\s+(solicitud|pr)|draft\s+purchase|solicitud de compra"))
            return CopilotIntent.DraftPurchaseRequest;

        if (Regex.IsMatch(m, @"caja|cash|arqueo|sesi[oó]n de caja|z-?report"))
            return CopilotIntent.CashStatus;

        if (Regex.IsMatch(m, @"alerta|alert|riesgo|cr[ií]tico|critico"))
            return CopilotIntent.AlertsNow;

        if (Regex.IsMatch(m, @"men[uú]|menu|receta|plato|eliminar producto|promociona"))
            return CopilotIntent.RecommendMenu;

        if (Regex.IsMatch(m, @"venta|gan[eé]|revenue|cu[aá]nto dinero|ticket|[oó]rdenes hoy|ordenes hoy"))
            return CopilotIntent.SalesToday;

        return CopilotIntent.Unknown;
    }
}
