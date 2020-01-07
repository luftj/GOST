using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOAP
{
    public class Goal
    {
        public string id {get; private set;}

        Dictionary<string, bool> conditions;

        public Goal(string id, Dictionary<string, bool> conditions)
        {
            this.id = id;
            this.conditions = new Dictionary<string, bool>(conditions);
        }

        /// <summary>
        /// Checks wether the given WorldModel complies with this goals conditions.
        /// </summary>
        /// <returns>"true" if goal is fulfilled.</returns>
        public bool isFulfilled(WorldModel model)
        {
            foreach (KeyValuePair<string, bool> item in conditions)
                if (item.Value != model.getCondition(item.Key))
                    return false;
            return true;
        }

        public int getNumUnmetConditions(WorldModel model)
        {
            int numUnmetConditions = 0;
            foreach (KeyValuePair<string, bool> item in conditions)
                if (model.getCondition(item.Key) != item.Value)
                    ++numUnmetConditions;
            return numUnmetConditions;
        }
    }
}
