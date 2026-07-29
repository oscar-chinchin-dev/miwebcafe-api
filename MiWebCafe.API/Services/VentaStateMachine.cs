using MiWebCafe.API.Entities;

namespace MiWebCafe.API.Services
{
    public static class VentaEstados
    {
        public const string Abierta = "ABIERTA";
        public const string Confirmada = "CONFIRMADA";
        public const string Anulada = "ANULADA";
    }

    public class VentaStateMachineResult
    {
        public bool EsValida { get; set; }
        public string? MensajeError { get; set; }

        public static VentaStateMachineResult Ok() => new VentaStateMachineResult { EsValida = true };
        public static VentaStateMachineResult Error(string mensaje) => new VentaStateMachineResult { EsValida = false, MensajeError = mensaje };
    }

    /// <summary>
    /// Servicio centralizado para la máquina de estados de la entidad Venta.
    /// Valida las transiciones permitidas durante el ciclo de vida de una venta.
    /// </summary>
    public static class VentaStateMachine
    {
        public static string ObtenerEstadoActual(Venta venta)
        {
            if (venta.Anulada)
                return VentaEstados.Anulada;

            return string.IsNullOrWhiteSpace(venta.Estado) 
                ? VentaEstados.Abierta 
                : venta.Estado.ToUpperInvariant();
        }

        public static VentaStateMachineResult ValidarTransicion(string estadoOrigen, string estadoDestino)
        {
            var origen = estadoOrigen?.ToUpperInvariant() ?? VentaEstados.Abierta;
            var destino = estadoDestino?.ToUpperInvariant();

            if (origen == destino)
            {
                return VentaStateMachineResult.Error($"La venta ya se encuentra en estado '{origen}'.");
            }

            if (origen == VentaEstados.Anulada)
            {
                return VentaStateMachineResult.Error("Una venta ANULADA no puede realizar transiciones de estado ni ser reactivada.");
            }

            if (destino == VentaEstados.Confirmada)
            {
                if (origen != VentaEstados.Abierta)
                {
                    return VentaStateMachineResult.Error($"No se puede confirmar una venta en estado '{origen}'. Solo las ventas en estado 'ABIERTA' pueden ser confirmadas.");
                }
                return VentaStateMachineResult.Ok();
            }

            if (destino == VentaEstados.Anulada)
            {
                if (origen == VentaEstados.Abierta || origen == VentaEstados.Confirmada)
                {
                    return VentaStateMachineResult.Ok();
                }
                return VentaStateMachineResult.Error($"No se puede anular una venta en estado '{origen}'.");
            }

            return VentaStateMachineResult.Error($"Transición de estado no permitida de '{origen}' a '{destino}'.");
        }

        public static VentaStateMachineResult ValidarModificacion(Venta venta)
        {
            var estadoActual = ObtenerEstadoActual(venta);
            if (estadoActual != VentaEstados.Abierta)
            {
                return VentaStateMachineResult.Error($"No se pueden agregar detalles a una venta en estado '{estadoActual}'. Solo se permiten modificaciones en estado 'ABIERTA'.");
            }
            return VentaStateMachineResult.Ok();
        }
    }
}
