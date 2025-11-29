using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Machly.Api.Models
{
    public class GlobalSettings
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        // Lista de categorías permitidas para máquinas
        public List<string> AllowedCategories { get; set; } = new();

        // Comisión por reserva (por ejemplo 0.10 = 10%)
        public decimal CommissionRate { get; set; } = 0.10m;

        // Texto de términos y condiciones
        public string TermsAndConditions { get; set; } = string.Empty;

        // Email de soporte
        public string SupportEmail { get; set; } = string.Empty;
    }
}
