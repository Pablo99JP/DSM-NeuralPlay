using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using NeuralPlay.Models;

namespace NeuralPlay.Assemblers
{
    public static class InvitacionAssembler
    {
        public static InvitacionViewModel ConvertENToViewModel(Invitacion en)
        {
            if (en == null) throw new System.ArgumentNullException(nameof(en));

            return new InvitacionViewModel
            {
                Id = (int)en.IdInvitacion,
                Tipo = en.Tipo,
                Estado = en.Estado,
                FechaEnvio = en.FechaEnvio,
                FechaRespuesta = en.FechaRespuesta,
                EmisorId = en.Emisor != null ? (int)en.Emisor.IdUsuario : 0,
                DestinatarioId = en.Destinatario != null ? (int)en.Destinatario.IdUsuario : 0,
                ComunidadId = en.Comunidad != null ? (int)en.Comunidad.IdComunidad : 0,
                EquipoId = en.Equipo != null ? (int)en.Equipo.IdEquipo : 0
            };
        }

        public static IList<InvitacionViewModel> ConvertListENToViewModel(IEnumerable<Invitacion> ens)
        {
            if (ens == null) return new List<InvitacionViewModel>();
            return ens.Select(ConvertENToViewModel).ToList();
        }
    }
}
