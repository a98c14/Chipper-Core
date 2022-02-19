using System.Collections;
using Unity.EditorCoroutines.Editor;

namespace Chipper.Prefabs.Utils
{
    public class DataCoroutineOwnerless
    {
        public object Result;
        public EditorCoroutine Coroutine { get; private set; }

        private readonly IEnumerator m_Target;

        public DataCoroutineOwnerless(IEnumerator target)
        {
            m_Target = target;
            Coroutine = EditorCoroutineUtility.StartCoroutineOwnerless(Run());
        }

        private IEnumerator Run()
        {
            while (m_Target.MoveNext())
            {
                Result = m_Target.Current;
                yield return Result;
            }
        }
    }
}
