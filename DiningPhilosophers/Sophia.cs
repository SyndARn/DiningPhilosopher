using Actor.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SophiaFuture
{
    class Fork : BaseActor
    {
        public int Number { get; private set; }
        public string Name { get { return "Fork " + Number.ToString(); } }
        private IActor InHand { get; set; }


        public Fork(int forkNumber) : base()
        {
            Number = forkNumber;
            Become(new Behavior<IActor>(DoRelease));
            AddBehavior(new Behavior<IActor, Future<bool>>(DoGetInHand));
        }

        public void Release(IActor actor)
        {
            SendMessage(actor);
        }

        private void DoRelease(IActor actor)
        {
            if (InHand == actor)
            {
                InHand = null;
            }
        }

        private void DoGetInHand(IActor actor, Future<bool> future)
        {
            if (InHand == null)
            {
                InHand = actor;
            }
            future.SendMessage(InHand == actor);
        }

        public bool GetInHand(IActor actor)
        {
            Future<bool> future = new Future<bool>();
            this.SendMessage(actor, future);
            return future.Result();
        }

    }


    enum PhilosopherState { Thinking, Eating };

    class Philosopher : BaseActor
    {
        private PhilosopherState CurrentState { get; set; }
        private string Name { get; set; }
        private Fork LeftFork { get; set; }
        private Fork RightFork { get; set; }
        private int Round { get; set; }
        private int Dinners { get; set; }
        private IActor Catcher { get; set; }
        public Philosopher(string name, Fork left, Fork right, IActor catcher) : base()
        {
            LeftFork = left;
            RightFork = right;
            Name = name;
            Catcher = catcher;
            CurrentState = PhilosopherState.Thinking;
            Become(new Behavior<int>(
                i =>
                {
                    Round = i;
                    switch (CurrentState)
                    {
                        case PhilosopherState.Eating: ReleaseForks(); break;
                        case PhilosopherState.Thinking: SearchForks(); break;
                    }
                }));
            AddBehavior(new Behavior<string>(
                s =>
                {
                    Catcher.SendMessage(string.Format("Philosopher {0} status : round {1} dinners {2} state {3}",
                        Name, Round, Dinners, CurrentState));
                })) ;
        }

        private void ReleaseForks()
        {
            LeftFork.Release((IActor)this);
            RightFork.Release((IActor)this);
            CurrentState = PhilosopherState.Thinking;
            Catcher.SendMessage(string.Format("Philo {0} thinks and has eaten for {1} times", this.Name, this.Dinners));
            if (Round > 0)
            {
                SendMessage(Round - 1);
            }
        }

        private void SearchForks()
        {
            if (LeftFork.GetInHand((IActor)this) && RightFork.GetInHand((IActor)this))
            {
                CurrentState = PhilosopherState.Eating;
                Dinners++;
                Catcher.SendMessage("Philo {0} eats for the {1} times", this.Name, this.Dinners);
            }
            else
            {
                LeftFork.Release((IActor)this);
                RightFork.Release((IActor)this);
            }
            SendMessage(Round);
        }

    }

    public class Table
    {
        private List<Philosopher> PhiloList { get; set; }
        public Table(int attendees,int rounds, IActor catcher)
        {
            PhiloList = new List<Philosopher>();
            var ForkList = new List<Fork>();
            for (int i = 0; i < attendees; i++)
            {
                ForkList.Add(new Fork(i));
            }
            for (int i = 0; i < attendees; i++)
            {
                PhiloList.Add(new Philosopher(i.ToString(), ForkList[i], 
                    (i+1) >= attendees ? ForkList[0] :  ForkList[i+1], catcher));
            }
            for (int i = 0; i < attendees; i++)
            {
                PhiloList[i].SendMessage(rounds); 
            }
        }
        public void Status()
        {
            foreach(var item in PhiloList)
            {
                item.SendMessage("Status");
            }
        }
    }
}
