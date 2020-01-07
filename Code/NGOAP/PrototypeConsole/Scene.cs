using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrototypeConsole
{
    class Scene
    {
        public string id { get; private set; }   //don't use underscores in this string!!!

        public string output { get; private set; }

        public Dictionary<string, bool> conditions { get; private set; }
        public Dictionary<string, Dictionary<string, bool>> outcomes { get; private set; }


        public Scene(string id, string output, Dictionary<string, bool> conditions, Dictionary<string, Dictionary<string, bool>> outcomes)
        {
            if (id.Contains('_'))
                throw new ArgumentException("Scene ID may not contain underscore ('_') characters!");
            this.id = id;
            this.output = output;
            this.conditions = new Dictionary<string, bool>(conditions);
            this.outcomes = new Dictionary<string, Dictionary<string, bool>>(outcomes);
        }
    }
}
