using AgroEco.Data;
using AgroEco.Data.Repositories;

public class ActionRepository : RepositoryBase<AgroEco.Core.Jobs.Actions.Action, DataContext>
{
    public ActionRepository(DataContext context) : base(context) { }

    protected override void ApplyChanges(AgroEco.Core.Jobs.Actions.Action existing, AgroEco.Core.Jobs.Actions.Action next)
    {
        existing.ChangeStatus(next.Status);
    }
}