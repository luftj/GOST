using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOAP
{
    public class WorldModel
    {
        static bool useNullHeuristic = false;


        SortedDictionary<string, bool> state;

        public List<Action> remainingActions;
        int curActionPtr = 0;


        public WorldModel(Dictionary<string, bool> state, List<Action> actions)
        {
            this.state = new SortedDictionary<string, bool>(state);
            remainingActions = new List<Action>(actions);
            curActionPtr = 0;
        }
        public WorldModel(SortedDictionary<string, bool> state, List<Action> actions)
        {
            this.state = new SortedDictionary<string, bool>(state);
            remainingActions = new List<Action>(actions);
            curActionPtr = 0;
        }

        /// <summary>
        /// calculates a heuristic of how far the goal is.
        /// </summary>
        /// <param name="goal">goal the heuristic is calculated for</param>
        /// <returns>calculated heuristic</returns>
        public int estimateHeuristic(Goal goal)
        {
            if(useNullHeuristic)
                return 0; //null-heuristic

            return goal.getNumUnmetConditions(this);
        }

        /// <summary>
        /// get next action possible for this state
        /// </summary>
        /// <returns>next action or 'null' if no possible action left</returns>
        public Action nextAction()
        {
            while (curActionPtr < remainingActions.Count)
            {
                Action ret = remainingActions[curActionPtr];
                
                ++curActionPtr;

                if (ret.isPossible(this))
                    return ret;
            }
            return null;
        }

        /// <summary>
        /// apply effect of given action to this worldmodel and delete this action from the pool of remaining actions
        /// </summary>
        /// <param name="action">action to apply</param>
        public void applyAction(Action action)
        {
            remainingActions.RemoveAt(curActionPtr);

            foreach (KeyValuePair<string, bool> item in action.effects)
                state[item.Key] = item.Value;
        }

        /// <summary>
        /// calculate hash as bitflag from set conditions
        /// </summary>
        /// <returns>hash value for this worldstate</returns>
        public int getHash()
        {
            int hash = 0;

            for (int i = 0; i < state.Count; ++i)
                if (state.ElementAt(i).Value)
                    hash |= 1 << i;

            return hash;
        }

        public void reset()
        {
            curActionPtr = 0;
        }

        /// <summary>
        /// Copies this WorldModel
        /// </summary>
        /// <returns>A copy of this WorldModel</returns>
        public WorldModel copy()
        {
            return new WorldModel(state, remainingActions);
        }

        public bool equals(WorldModel other)
        {
            if (!state.SequenceEqual(other.state))
                return false;
            if (!remainingActions.SequenceEqual(other.remainingActions))
                return false;
            return true;
        }

        public bool? getCondition(string key)
        {
            if (!state.ContainsKey(key))
                return null;
            return state[key];
        }

        public string stateToString()
        {
            string ret = "";
            foreach (KeyValuePair<string, bool> item in state)
                ret += item.Key + " is " + item.Value + "\n";
            return ret;
        }
    }
}
