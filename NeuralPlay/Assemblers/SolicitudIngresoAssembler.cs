using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using NeuralPlay.Models;

namespace NeuralPlay.Assemblers
{
    public static class SolicitudIngresoAssembler
    {
        public static SolicitudIngresoViewModel ConvertENToViewModel(SolicitudIngreso en)
        {
            if (en == null) throw new System.ArgumentNullException(nameof(en));

            return new SolicitudIngresoViewModel
            {
                Id = (int)en.IdSolicitud,
                Tipo = en.Tipo,
                Estado = en.Estado,
                FechaSolicitud = en.FechaSolicitud,
                FechaResolucion = en.FechaResolucion,
                SolicitanteId = en.Solicitante != null ? (int)en.Solicitante.IdUsuario : 0,
                ComunidadId = en.Comunidad != null ? (int)en.Comunidad.IdComunidad : 0,
                EquipoId = en.Equipo != null ? (int)en.Equipo.IdEquipo : 0
            };
        }

        public static IList<SolicitudIngresoViewModel> ConvertListENToViewModel(IEnumerable<SolicitudIngreso> ens)
        {
            if (ens == null) return new List<SolicitudIngresoViewModel>();
            return ens.Select(ConvertENToViewModel).ToList();
        }
    }
}
