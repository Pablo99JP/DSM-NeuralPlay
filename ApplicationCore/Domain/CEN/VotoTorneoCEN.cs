using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad VotoTorneo.
    /// Expone operaciones CRUD para gestionar votos en propuestas de torneo.
    /// Los miembros del equipo votan para aceptar o rechazar unirse a un torneo.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class VotoTorneoCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly IVotoTorneoRepository _votoTorneoRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="votoTorneoRepository">Implementación del repositorio de votos de torneo</param>
        public VotoTorneoCEN(IVotoTorneoRepository votoTorneoRepository)
        {
            _votoTorneoRepository = votoTorneoRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea un nuevo voto en una propuesta de torneo.
        /// REGLA DE NEGOCIO: FechaVoto se establece automáticamente a DateTime.Now.
        /// </summary>
        /// <param name="valor">Valor del voto (true = a favor, false = en contra)</param>
        /// <returns>ID del voto creado</returns>
        public long Crear(bool valor)
        {
            // Construye la entidad de dominio aplicando reglas de negocio
            var votoTorneo = new VotoTorneo
            {
                Valor = valor,
                FechaVoto = DateTime.Now  // ← REGLA: Siempre fecha actual
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/VotoTorneoRepository.cs → GenericRepository.New()
            _votoTorneoRepository.New(votoTorneo);
            
            return votoTorneo.IdVoto;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica un voto existente.
        /// Permite cambiar el voto antes de que se cierre la propuesta.
        /// </summary>
        /// <param name="idVoto">ID del voto a modificar</param>
        /// <param name="valor">Nuevo valor del voto (true/false)</param>
        /// <param name="fechaVoto">Fecha del voto</param>
        public void Modificar(long idVoto, bool valor, DateTime fechaVoto)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/VotoTorneoRepository.cs → GenericRepository.DamePorOID()
            var votoTorneo = _votoTorneoRepository.DamePorOID(idVoto);
            
            // Actualiza las propiedades
            votoTorneo.Valor = valor;
            votoTorneo.FechaVoto = fechaVoto;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/VotoTorneoRepository.cs → GenericRepository.Modify()
            _votoTorneoRepository.Modify(votoTorneo);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina un voto por su ID.
        /// Se usa cuando un usuario retira su voto antes del cierre de la propuesta.
        /// </summary>
        /// <param name="idVoto">ID del voto a eliminar</param>
        public void Eliminar(long idVoto)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/VotoTorneoRepository.cs → GenericRepository.Destroy()
            _votoTorneoRepository.Destroy(idVoto);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene un voto por su identificador único.
        /// </summary>
        /// <param name="idVoto">ID del voto</param>
        /// <returns>Entidad VotoTorneo o null si no existe</returns>
        public VotoTorneo DamePorOID(long idVoto)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/VotoTorneoRepository.cs → GenericRepository.DamePorOID()
            return _votoTorneoRepository.DamePorOID(idVoto);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todos los votos del sistema.
        /// </summary>
        /// <returns>Lista de todos los votos</returns>
        public IList<VotoTorneo> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/VotoTorneoRepository.cs → GenericRepository.DameTodos()
            return _votoTorneoRepository.DameTodos();
        }
    }
}
