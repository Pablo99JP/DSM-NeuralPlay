# ApplicationCore

Contiene el dominio de la aplicación (EN, enums, interfaces de repositorio, CEN y CP futuros).

Estructura relevante creada automáticamente:

- Domain/EN: Entidades POCO (sin referencias a EF/NHibernate).
- Domain/Enums: Enums del dominio.
- Domain/Repositories: Interfaces síncronas de repositorio.

Siguientes pasos recomendados:

1. Implementar las interfaces de repositorio en `Infrastructure` (NHibernate/EF Core).
2. Generar CENs (Componentes por Entidad) en `ApplicationCore/Domain/CEN` que consuman las interfaces de repositorio.
3. Implementar CPs (casos de uso) en `ApplicationCore/Domain/CP`.
