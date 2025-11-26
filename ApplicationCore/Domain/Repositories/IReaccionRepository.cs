using System.Collections.Generic;
using ApplicationCore.Domain.EN;

namespace ApplicationCore.Domain.Repositories
{
 public interface IReaccionRepository : IRepository<Reaccion>
 {
 Reaccion? GetByPublicacionAndAutor(long publicacionId, long autorId);
 int CountByPublicacion(long publicacionId);
 Reaccion? GetByComentarioAndAutor(long comentarioId, long autorId);
 int CountByComentario(long comentarioId);
 }
}
