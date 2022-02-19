using System.Collections;
using UnityEngine;

namespace Chipper.Prefabs.Utils
{
    public class DataCoroutine
    {
        public object Result;
        public Coroutine Coroutine { get; private set; }

        private readonly IEnumerator m_Target;

        public DataCoroutine(MonoBehaviour owner, IEnumerator target)
        {
            m_Target = target;
            Coroutine = owner.StartCoroutine(Run());
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
