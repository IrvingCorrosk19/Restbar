using System;
using System.Collections.Generic;
using RestBar.Models;
using System.Text.Json.Serialization;

namespace RestBar.ViewModel
{
    public class SendOrderDto
    {
        public Guid TableId { get; set; }
        public string OrderType { get; set; } = "DineIn";
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public string? Notes { get; set; }
        public decimal? Discount { get; set; }
        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    public class AddItemsDto
    {
        public Guid OrderId { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class RemoveItemDto
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string? Status { get; set; }
        public Guid? ItemId { get; set; } // ID específico del item para eliminación precisa
    }

    public class UpdateItemQuantityDto
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public string? Status { get; set; }
        public Guid? ItemId { get; set; } // ✅ NUEVO: ItemId específico para actualización precisa
    }

    public class UpdateItemDto
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
    }

    public class UpdateOrderItemDto
    {
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateOrderCompleteDto
    {
        public Guid OrderId { get; set; }
        public List<UpdateOrderItemDto> Items { get; set; } = new List<UpdateOrderItemDto>();
    }

    public class MarkItemReadyDto
    {
        public Guid OrderId { get; set; }
        public Guid ItemId { get; set; }
    }
} 