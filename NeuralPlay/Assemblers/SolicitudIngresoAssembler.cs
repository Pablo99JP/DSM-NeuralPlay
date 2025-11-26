using ApplicationCore.Domain.EN;
using NeuralPlay.Models;

namespace NeuralPlay.Assemblers
{
 public static class SolicitudIngresoAssembler
 {
 public static SolicitudIngresoViewModel ConvertENToViewModel(SolicitudIngreso en)
 {
 if (en == null) return null!;
 return new SolicitudIngresoViewModel
 {
 IdSolicitud = en.IdSolicitud,
 Tipo = en.Tipo,
 Estado = en.Estado,
 FechaSolicitud = en.FechaSolicitud,
 FechaResolucion = en.FechaResolucion,
 SolicitanteId = en.Solicitante?.IdUsuario,
 SolicitanteNick = en.Solicitante?.Nick,
 ComunidadId = en.Comunidad?.IdComunidad,
 ComunidadNombre = en.Comunidad?.Nombre,
 EquipoId = en.Equipo?.IdEquipo,
 EquipoNombre = en.Equipo?.Nombre
 };
 }
 }
}
