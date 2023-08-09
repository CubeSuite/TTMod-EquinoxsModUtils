using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EquinoxsModUtils
{
    public class NewUnlockDetails
    {
        public Unlock.TechCategory category;
        public ResearchCoreDefinition.CoreType coreTypeNeeded;
        public int coreCountNeeded;
        public List<string> dependencyNames = new List<string>();
        public string description;
        public string displayName;
        public int numScansNeeded;
        public TechTreeState.ResearchTier requiredTier;
        public Sprite sprite;
        public int treePosition;
    }
}
