namespace ArmyAnt.ProcessController {
    public interface IEventManager<T_EventID> {
        public interface IEventArgs {
            T_EventID EventId { get; }
        }

        int Listen(T_EventID eventId, System.Action<IEventArgs> callback, int siblingIndex = -1);
        bool Unlisten(T_EventID eventId, int listenerId);
        bool Unlisten(T_EventID eventId, System.Action<IEventArgs> callback);
        bool NotifyAsync(IEventArgs args);
        void NotifySync(IEventArgs args);
    }
}