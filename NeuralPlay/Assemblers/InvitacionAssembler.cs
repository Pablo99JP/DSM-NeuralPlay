using ApplicationCore.Domain.EN;
using NeuralPlay.Models;

namespace NeuralPlay.Assemblers
{
 public static class InvitacionAssembler
 {
 public static InvitacionViewModel ConvertENToViewModel(Invitacion en)
 {
 if (en == null) return null!;
 return new InvitacionViewModel
 {
 IdInvitacion = en.IdInvitacion,
 Tipo = en.Tipo,
 Estado = en.Estado,
 FechaEnvio = en.FechaEnvio,
 FechaRespuesta = en.FechaRespuesta,
 EmisorId = en.Emisor?.IdUsuario,
 EmisorNick = en.Emisor?.Nick,
 DestinatarioId = en.Destinatario?.IdUsuario,
 DestinatarioNick = en.Destinatario?.Nick,
 ComunidadId = en.Comunidad?.IdComunidad,
 ComunidadNombre = en.Comunidad?.Nombre,
 EquipoId = en.Equipo?.IdEquipo,
 EquipoNombre = en.Equipo?.Nombre
 };
 }
 }
}
