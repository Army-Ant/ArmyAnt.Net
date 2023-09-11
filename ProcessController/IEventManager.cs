namespace ArmyAnt.ProcessController {
    public interface IEventManager<T_EventID> {
        public interface IEventArgs {
            T_EventID EventId { get; }
        }

        int Listen(T_EventID eventId, System.Action<IEventArgs> callback);
        bool Unlisten(T_EventID eventId, int listenerId);
        bool NotifyAsync(IEventArgs args);
        void NotifySync(IEventArgs args);
    }
}