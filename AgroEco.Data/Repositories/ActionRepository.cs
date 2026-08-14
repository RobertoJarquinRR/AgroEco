using AgroEco.Data;
using AgroEco.Data.Repositories;

public class ActionRepository : RepositoryBase<AgroEco.Core.Jobs.Action, DataContext>
{
    public ActionRepository(DataContext context) : base(context) { }

    protected override void ApplyChanges(AgroEco.Core.Jobs.Action existing, AgroEco.Core.Jobs.Action next)
    {
        existing.ChangeStatus(next.Status);
    }
}