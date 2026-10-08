using AgroEco.Core;
using AgroEco.Core.Jobs;
using AgroEco.Core.Triggers;
using AgroEco.Data;

namespace AgroEco.Data.Repositories
{
    // El repositorio maneja la entidad abstracta Trigger y opera sobre el DataContext
    public class TriggerRepository : RepositoryBase<Trigger, DataContext>
    {
        public TriggerRepository(DataContext context) : base(context)
        {
        }

        // ApplyChanges es requerido por la clase base para saber cómo actualizar una entidad existente
        protected override void ApplyChanges(Trigger existingEntity, Trigger newEntity)
        {
            Result result = existingEntity.UpdateDetails(newEntity.Name);
            if (!result.Success)
            {
                throw new InvalidOperationException(result.Message);
            }
        }
    }
}