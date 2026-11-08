using Moba.Core;

namespace Moba.Units
{
    /// <summary>
    /// Миньон: при смерти сразу убирается со сцены.
    /// </summary>
    public class Minion : Unit
    {
        protected override void OnDeath(Unit killer)
        {
            base.OnDeath(killer);
            Destroy(gameObject);
        }
    }
}
