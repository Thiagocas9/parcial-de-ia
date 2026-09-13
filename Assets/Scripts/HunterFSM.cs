using UnityEngine;

namespace HuntingSimulation
{
    
    public sealed class HunterFSM
    {
        public enum State { Patrol, Attack, Gather }
        public State Current { get; private set; } = State.Patrol;
        public float GatherProgress { get; private set; }
        readonly HunterAgent hunter;
        public HunterFSM(HunterAgent owner) => hunter = owner;
        void Transition(State next, BoidAgent target = null)
        {
            Current = next; hunter.Target = target; GatherProgress = 0;
        }
        public void Tick(float dt)
        {
            if (Current == State.Attack && (!hunter.Visible(hunter.Target) || hunter.Target.Dead))
                Transition(State.Patrol);
            if (Current == State.Gather && (!hunter.Visible(hunter.Target) || !hunter.Target.Dead))
                Transition(State.Patrol);

            
            BoidAgent corpse = hunter.Nearest(true);
            if (Current != State.Gather && corpse != null) Transition(State.Gather, corpse);
            if (Current == State.Patrol && hunter.Cooldown <= 0)
            {
                BoidAgent prey = hunter.Nearest(false);
                if (prey != null) Transition(State.Attack, prey);
            }
            switch (Current)
            {
                case State.Patrol: hunter.Patrol(); break;
                case State.Attack:
                    hunter.AttackFeedback();
                    float distance = hunter.TargetDistance;
                    if (distance <= hunter.MeleeAttackRadius)
                    {
                        if (distance <= hunter.meleeContactRadius) Attack(true);
                        else hunter.Pursue();
                    }
                    else if (distance <= hunter.RangeAttackRadius) Attack(false);
                    else hunter.Pursue();
                    break;
                case State.Gather:
                    hunter.GatherFeedback();
                    if (hunter.TargetDistance > 1.45f) { GatherProgress = 0; hunter.Pursue(); }
                    else
                    {
                        hunter.Stop(); GatherProgress += dt;
                        if (GatherProgress >= hunter.gatherDuration)
                        { hunter.FinishGather(); Transition(State.Patrol); }
                    }
                    break;
            }
        }
        void Attack(bool melee)
        {
            hunter.PerformAttack(melee);
            Transition(State.Patrol);
        }
    }
}
