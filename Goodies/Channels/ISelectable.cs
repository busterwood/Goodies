namespace BusterWood.Channels
{
    public interface ISelectable
    {
        /// <summary>Adds a waiter for a <see cref="Select"/></summary>
        void AddWaiter(Waiter waiter);

        /// <summary>Removes a waiter for a <see cref="Select"/></summary>
        void RemoveWaiter(Waiter waiter);
    }
}