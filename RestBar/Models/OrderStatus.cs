namespace RestBar.Models
{
    public enum OrderStatus
    {
        Pending,        // Pendiente de preparación
        SentToKitchen, // Enviado a cocina
        Preparing,     // En preparación
        Ready,         // Listo para servir
        ReadyToPay,    // Listo para pagar (todos los items listos)
        Served,        // Servido al cliente
        Cancelled,     // Cancelado
        Completed      // Completado (pagado y cerrado)
    }
} 