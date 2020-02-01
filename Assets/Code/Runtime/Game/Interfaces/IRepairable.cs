namespace Code.Runtime.Game.Interfaces
{
    public interface IRepairable : ITargetable
    {
        void Repair(int repairAmount);
    }
}