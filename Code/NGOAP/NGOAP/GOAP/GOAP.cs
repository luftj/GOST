using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOAP
{
    static class GOAP
    {
        static int cutoff;

        /// <summary>
        /// Does an optimised IDA* search
        /// </summary>
        /// <param name="startModel"></param>
        /// <param name="goal"></param>
        /// <param name="maxDepth"></param>
        /// <returns></returns>
        public static List<Action> planActions(WorldModel startModel, Goal goal, int maxDepth, int ttSize)
        {
            List<Action> path = new List<Action>();

            cutoff = startModel.estimateHeuristic(goal); // set heuristic of startModel as optimisation cutoff

            TranspositionTable transpositionTable = new TranspositionTable(ttSize);

            while (cutoff >= 0 && cutoff != int.MaxValue)   //TODO: passiert >=0 überhaupt?
            {
                //get actionplan
                path = doDepthFirst(startModel, goal, transpositionTable, maxDepth);

                if (path != null)
                    return path;    //if a valid plan was found, return it
            }
            return null;
        }

        /// <summary>
        /// IDA* iteration
        /// </summary>
        /// <param name="worldModel"></param>
        /// <param name="goal"></param>
        /// <param name="transpositionTable"></param>
        /// <param name="maxDepth"></param>
        /// <returns></returns>
        static List<Action> doDepthFirst( WorldModel worldModel, Goal goal, TranspositionTable transpositionTable, int maxDepth)
        {
            //storage for models at each depth ans corresponding actions and their cost
            WorldModel[] models = new WorldModel[maxDepth + 1];
            Action[] actions = new Action[maxDepth];
            float[] costs = new float[maxDepth];

            //initialize
            models[0] = worldModel.copy();
            int curDepth = 0;

            //keep track of minimal cutoff for optimization
            int smallestCutoff = int.MaxValue;

            //iterate until there's no more possibilities at depth 0
            while (curDepth >= 0)
            {
                if (curDepth == 0)
                    smallestCutoff = int.MaxValue;

                //check wether goal is reached
                if (goal.isFulfilled(models[curDepth]))
                {
                    List<Action> ret = new List<Action>();
                    for (int i = 0; i < actions.Length; ++i)
                    {
                        if (actions[i] == null)
                            break;
                        ret.Add(actions[i]);
                    }
                    return ret; //return current plan
                }

                //if maximum depth is reached
                if (curDepth >= maxDepth)
                {   //continue in lower depth
                    --curDepth;
                    continue;
                }

                //calculate total cost of current plan
                float cost = models[curDepth].estimateHeuristic(goal) + costs[curDepth];

                //check if we need to prune based on cost
                if (cost > cutoff)
                {
                    //
                    if (cutoff < smallestCutoff)
                        smallestCutoff = cutoff;

                    //drop back
                    --curDepth;
                    continue;
                }

                //otherwise try the next action
                Action nextAction = models[curDepth].nextAction();
                if (nextAction != null)
                {
                    //copy current model
                    models[curDepth + 1] = models[curDepth].copy();

                    //apply action
                    actions[curDepth] = nextAction;
                    models[curDepth + 1].applyAction(nextAction);
                    costs[curDepth + 1] = costs[curDepth] + nextAction.cost;

                    if (!transpositionTable.has(models[curDepth + 1]))
                    {
                        ++curDepth;
                        transpositionTable.add(models[curDepth], curDepth - 1);
                    }
                    else
                        transpositionTable.add(models[curDepth+1], curDepth);
                }
                else
                    --curDepth;
            }
            cutoff = smallestCutoff;
            return null;
        }
    }
}
