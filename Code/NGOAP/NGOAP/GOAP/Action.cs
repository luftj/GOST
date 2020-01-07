using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOAP
{
    public class Action
    {
        public string id { get; private set; }

        Dictionary<string, bool> preconditions;
        public Dictionary<string, bool> effects;

        public float cost { get; private set; }

        public Action(string id, Dictionary<string, bool> preconditions, Dictionary<string, bool> effects)
        {
            this.id = id;
            this.preconditions = new Dictionary<string, bool>(preconditions);
            this.effects = new Dictionary<string, bool>(effects);
            cost = 1.0f;
        }

        public bool isPossible(WorldModel model)
        {
            foreach (KeyValuePair<string, bool> item in preconditions)
            {
                bool? modelIs = model.getCondition(item.Key);
                if (modelIs == null)
                    continue;
                if (item.Value != modelIs)
                    return false;
            }
            return true;
        }
    }
}
