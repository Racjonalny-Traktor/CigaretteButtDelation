using System;

namespace microserv.Models
{
    public class Litter
    {
        public int Id { get; set; }
        public double Long { get; set; }
        public double Lat { get; set; }
        public int CigarettesNum { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
    }
}
