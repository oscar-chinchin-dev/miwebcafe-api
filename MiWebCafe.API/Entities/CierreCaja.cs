namespace MiWebCafe.API.Entities
{
    public class CierreCaja
    {
        public int CierreCajaId { get; set; }

        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }   // nullable

        public decimal MontoInicial { get; set; }
        public decimal? MontoFinalDeclarado { get; set; }

        public decimal TotalVentas { get; set; }
        public int CantidadVentas { get; set; }

        public string Estado { get; set; } = "ABIERTA"; // ABIERTA / CERRADA

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
    }
}
