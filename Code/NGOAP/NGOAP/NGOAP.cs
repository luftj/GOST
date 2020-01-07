using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GOAP;

namespace NGOAP
{
    public class NGOAP
    {
        int maxDepth = 4;   // Constant - cuts off search at certain depth for optimisation
        int ttSize = 10;    // Constant - determines size of transposition table. bigger size -> more memory usage, faster search and vice versa


        List<GOAP.Action> actions;

        SortedList<int, Goal> goals;
        int curGoalPtr = 0;

        public List<GOAP.Action> actionPlan { get; private set; }
        public WorldModel currentState { get; private set; }


        public NGOAP()
        {
            currentState = null;
            actions = new List<GOAP.Action>();
            goals = new SortedList<int, Goal>();  
        }

        public void createStartState(Dictionary<string, bool> conditions)
        {
            currentState = new WorldModel(conditions, actions);
        }

        public bool createGoal(string id, Dictionary<string, bool> conditions, int priority)
        {
            Goal goal = new Goal(id,conditions);
            if (goals.ContainsKey(priority) || goals.ContainsValue(goal))
                return false;

            goals.Add(priority, goal);

            return true;
        }

        public void createAction(string id, Dictionary<string,bool> preconditions, Dictionary<string,bool> effects)
        {
            GOAP.Action action = new GOAP.Action(id, preconditions, effects);
            addAction(action);
        }

        void addAction(GOAP.Action action)
        {
            if (actions.Contains(action))
                return;

            actions.Add(action);

            if (currentState != null)
                currentState.remainingActions.Add(action);
        }

        public int makePlan()
        {
            while (curGoalPtr<goals.Count())
            {
                List<GOAP.Action> currentActionPlan = GOAP.GOAP.planActions(currentState, goals.Values[curGoalPtr], maxDepth, ttSize);

                if (currentActionPlan == null)
                {   //goal can't be reached
                    ++curGoalPtr;
                    continue;
                }
                else
                {   //valid plan
                    actionPlan = currentActionPlan;
                    return 0;
                }
            }
            return -1;
        }

        public void invalidatePlan()
        {
            actionPlan.Clear();
            currentState.reset();
        }

        public bool checkGoalReached()
        {
            //check for every possible goal
            foreach (Goal item in goals.Values)
                if (item.isFulfilled(currentState))
                {
                    curGoalPtr = goals.Values.IndexOf(item);
                    return true;
                }
            return false;   //no goal is reached
        }

        public Goal getCurrentGoal()
        {
            return goals.Values[curGoalPtr];
        }

        public void applyAction(string key)
        {
            foreach (GOAP.Action item in currentState.remainingActions)
            {
                if (item.id == key)                     //find action in remaining actions
                {
                    currentState.applyAction(item);     //apply effects
                    actionPlan.RemoveAt(0);             //step forward in plan
                    return;
                }
            }
            //else: action not found
            throw new ArgumentException("Can't apply action " + key + " because it wasn't found for current worldstate!");
        }

        public string getNextAction()
        {
            return actionPlan[0].id;
        }

        public string planToString()
        {
            string ret = "";
            ret += "Current state:\n" + currentState.stateToString();
            ret += "Current plan:\n";
            int i = 0;
            foreach (GOAP.Action item in actionPlan)
                ret += "Step " + i++ + ": " + item.id + "\n";  //print plan
            ret+="Current goal: " + getCurrentGoal().id;

            return ret;
        }
    }
}
