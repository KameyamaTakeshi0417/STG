namespace Alpha.Core.Utils
{
    public interface IBombDestructible
    {
        bool canDestructByBomb { get; }
        void OnBombDestruct();
    }
}
