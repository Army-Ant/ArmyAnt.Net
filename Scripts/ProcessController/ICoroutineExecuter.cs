namespace ArmyAnt.ProcessController {
    public interface ICoroutineTask : System.Collections.IEnumerator {
        System.Collections.IEnumerator Target { get; }
    }

    public interface ICoroutineExecuter {
        ICoroutineTask RunCoroutine(System.Collections.IEnumerator process);

        System.Collections.IEnumerator CreateSecondsDelay(float seconds);
    }
}
