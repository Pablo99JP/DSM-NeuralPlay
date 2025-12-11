using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using NeuralPlay.Models;

namespace NeuralPlay.Assemblers
{
    public static class NotificacionAssembler
    {
        public static NotificacionViewModel ConvertENToViewModel(Notificacion en)
        {
            if (en == null) throw new System.ArgumentNullException(nameof(en));

            return new NotificacionViewModel
            {
                Id = (int)en.IdNotificacion,
                Texto = en.Mensaje ?? string.Empty,
                Fecha = en.FechaCreacion,
                Leido = en.Leida,
                Tipo = en.Tipo,
                UsuarioId = en.Destinatario != null ? (int)en.Destinatario.IdUsuario : 0
            };
        }

        public static IList<NotificacionViewModel> ConvertListENToModel(IEnumerable<Notificacion> ens)
        {
            if (ens == null) return new List<NotificacionViewModel>();
            return ens.Select(ConvertENToViewModel).ToList();
        }
    }
}
