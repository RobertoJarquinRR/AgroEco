using AgroEco.Core.Jobs;
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
            // Como las propiedades tienen private set, actualizamos los campos permitidos 
            // mediante la lógica o métodos que exponga tu entidad Trigger.
            // Por ejemplo, si añadieras un método en Trigger como 'UpdateDetails', lo llamarías aquí:
            // existingEntity.UpdateDetails(newEntity.Name, newEntity.Type);
        }
    }
}