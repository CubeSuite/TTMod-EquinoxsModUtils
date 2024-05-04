using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using UnityEngine;

namespace EquinoxsModUtils
{
    public class NewResourceDetails
    {
        public string parentName;

        public CraftingMethod craftingMethod;
        public int craftTierRequired;
        public string description;
        public float fuelAmount;
        public string subHeaderTitle;
        public int maxStackCount = 500;
        public int miningTierRequired;
        public GameObject model3D;
        public GameObject rawConveyorResourcePrefab;
        public string name;
        public Sprite sprite;
        public int sortPriority;
        public string unlockName;

        internal ResourceInfo ConvertToResourceInfo() {
            ResourceInfo newResource = (ResourceInfo)ScriptableObject.CreateInstance("ResourceInfo");
            newResource.craftingMethod = craftingMethod;
            newResource.craftTierRequired = craftTierRequired;
            newResource.description = description;
            newResource.fuelAmount = fuelAmount;
            newResource.maxStackCount = maxStackCount;
            newResource.miningTierRequired = miningTierRequired;
            newResource.model3D = model3D;
            newResource.rawConveyorResourcePrefab = rawConveyorResourcePrefab;
            newResource.rawName = name;
            newResource.rawSprite = sprite;
            newResource.sortPriority = sortPriority;

            return newResource;
        }
    }
}
