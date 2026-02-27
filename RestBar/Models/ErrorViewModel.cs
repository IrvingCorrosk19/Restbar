namespace RestBar.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>Mensaje de la excepción para mostrar en la vista de error.</summary>
        public string? Message { get; set; }
    }
}
