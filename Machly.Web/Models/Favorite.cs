namespace Machly.Web.Models
{
    public class Favorite
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string MachineId { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Propiedad opcional para mostrar detalles de la máquina si se hace un join o se rellena manualmente
        public Machine? Machine { get; set; }
    }
}
