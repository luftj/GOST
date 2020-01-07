using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOAP
{
    class TranspositionTable
    {
        class Entry
        {
            public WorldModel model = null;    //holds the model for this entry
            public int depth = int.MaxValue;   //holds the depth this model was found at
        }

        Entry[] entries;
        int size;

        public TranspositionTable(int size)
        {
            this.size = size;
            entries = new Entry[size];
        }
        
        public bool has(WorldModel model)
        {
            //get hash value
            int hashValue = model.getHash();

            //find entry
            Entry entry = entries[hashValue % size];

            if (entry == null)
                return false;

            //check if it's the right one
            return entry.model.equals(model);
        }

        public void add(WorldModel model, int depth)
        {
            //get hash value
            int hashValue = model.getHash();

            //find entry
            Entry entry = entries[hashValue % size];

            if (entry == null)
            {
                entries[hashValue % size] = new Entry();
                entries[hashValue % size].model = model;
                entries[hashValue % size].depth = depth;
                return;
            }

            //check if it's the right one
            if (entry.model.equals(model))
            {
                //use the new depth if it is lower
                if (depth < entry.depth)
                    entry.depth = depth;
            }
            else
                //replace this slot if the new depth is lower
                if (depth < entry.depth)
                {
                    entry.model = model;
                    entry.depth = depth;
                }
        }
    }
}
