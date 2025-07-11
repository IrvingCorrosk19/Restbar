namespace RestBar.Models
{
    public enum OrderItemStatus
    {
        Pending,        // Pendiente de preparación
        Preparing,      // En preparación
        Ready,          // Listo
        Served,         // Servido
        Cancelled       // Cancelado
    }

    public enum KitchenStatus
    {
        Pending,        // Aún no enviado a cocina
        Sent,           // Enviado pero no preparado
        Ready,          // Ya preparado
        Cancelled       // Eliminado o anulado
    }
} 