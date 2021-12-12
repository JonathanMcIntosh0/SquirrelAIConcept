using Squirrel;

namespace Actions
{
    public abstract class SquirrelAction : ISquirrelAction
    {
        protected SquirrelController _controller;

        protected SquirrelAction(SquirrelController controller)
        {
            _controller = controller;
        }

        public abstract bool PreCondition(WorldVector state);
        public abstract bool PostCondition(WorldVector state);
        public abstract bool Execute();

        public abstract WorldVector Simulate(WorldVector state);
    }
}