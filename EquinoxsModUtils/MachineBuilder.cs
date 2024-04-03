using FIMSpace.Generating.Planning.PlannerNodes.Cells.Actions;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using TriangleNet.Voronoi.Legacy;
using UnityEngine;

namespace EquinoxsModUtils
{
    internal static class MachineBuilder
    {
        internal static void buildMachine(int resId, GridInfo gridInfo, bool shouldLog, int variationIndex, int recipe, ConveyorBuildInfo.ChainData? chainData, bool reverseConveyor) {
            MachineTypeEnum type = ModUtils.GetMachineTypeFromResID(resId);
            if(type == MachineTypeEnum.NONE) {
                ResourceInfo resourceInfo = SaveState.GetResInfoFromId(resId);
                string name = resourceInfo == null ? "Unknown Resource" : resourceInfo.displayName;
                ModUtils.LogEMUError($"Cannot build machine for invalid resID: {resId} - '{name}'");
                return;
            }

            switch (type) {
                case MachineTypeEnum.Accumulator:
                case MachineTypeEnum.BlastSmelter:
                case MachineTypeEnum.Chest:
                case MachineTypeEnum.LightSticks:
                case MachineTypeEnum.Planter:
                case MachineTypeEnum.Smelter:
                case MachineTypeEnum.Thresher:
                case MachineTypeEnum.TransitDepot:
                case MachineTypeEnum.TransitPole:
                case MachineTypeEnum.VoltageStepper:
                    doSimpleBuild(resId, gridInfo, shouldLog); break;
                
                case MachineTypeEnum.Assembler:
                    if (recipe == -1) doSimpleBuild(resId, gridInfo, shouldLog);
                    else doSimpleBuildWithRecipe(resId, gridInfo, recipe, shouldLog);
                    break;
                    
                case MachineTypeEnum.Inserter: doSimpleBuildWithRecipe(resId, gridInfo, recipe, shouldLog); break;
                case MachineTypeEnum.Conveyor: doConveyorBuild(resId, gridInfo, chainData, reverseConveyor, shouldLog); break;
                case MachineTypeEnum.Drill: doDrillBuild(resId, gridInfo); break;
                case MachineTypeEnum.Floor: doFloorBuild(resId, gridInfo); break;
                case MachineTypeEnum.ResearchCore: doResearchCoreBuild(resId, gridInfo); break;
                case MachineTypeEnum.Stairs: doStairsBuild(resId, gridInfo); break;
                case MachineTypeEnum.Structure: doStructureBuild(resId, gridInfo, variationIndex); break;

                default:
                    ModUtils.LogEMUError($"Sorry, EMU currently doesn't support building {type}");
                    break;
            }
        }

        // doBuild Functions

        private static void doSimpleBuild(int resID, GridInfo gridInfo, bool shouldLog) {
            SimpleBuildInfo simpleBuildInfo = new SimpleBuildInfo() {
                machineType = resID,
                rotation = gridInfo.yawRot,
                minGridPos = gridInfo.minPos
            };
            BuilderInfo builderInfo = (BuilderInfo)SaveState.GetResInfoFromId(resID);
            StreamedHologramData hologram = getHologram(builderInfo, gridInfo);

            MachineInstanceDefaultBuilder builder = (MachineInstanceDefaultBuilder)Player.instance.builder.GetBuilderForType(BuilderInfo.BuilderType.MachineInstanceDefaultBuilder);
            builder.newBuildInfo = (SimpleBuildInfo)simpleBuildInfo.Clone();
            builder = (MachineInstanceDefaultBuilder)setCommonBuilderFields(builder, builderInfo, gridInfo, hologram);

            setPlayerBuilderPrivateFields(builder, builderInfo);
            doBuild(builder, builderInfo, -1);

            if (shouldLog) ModUtils.LogEMUInfo($"Built {builderInfo.displayName} at ({gridInfo.minPos}) with yawRotation {gridInfo.yawRot}");
        }

        private static void doSimpleBuildWithRecipe(int resID, GridInfo gridInfo, int recipe, bool shouldLog) {
            SimpleBuildInfo info = new SimpleBuildInfo() {
                machineType = resID,
                rotation = gridInfo.yawRot,
                minGridPos = gridInfo.minPos,
                recipeId = recipe
            };
            BuilderInfo builderInfo = (BuilderInfo)SaveState.GetResInfoFromId(resID);

            StreamedHologramData hologram = getHologram(builderInfo, gridInfo);
            MachineInstanceDefaultBuilder builder = (MachineInstanceDefaultBuilder)Player.instance.builder.GetBuilderForType(BuilderInfo.BuilderType.MachineInstanceDefaultBuilder);
            builder.newBuildInfo = info;
            builder = (MachineInstanceDefaultBuilder)setCommonBuilderFields(builder, builderInfo, gridInfo, hologram);
            
            setPlayerBuilderPrivateFields(builder, builderInfo);
            doBuild(builder, builderInfo, recipe);

            if (shouldLog) ModUtils.LogEMUInfo($"Built {builderInfo.displayName} with recipe {recipe} at {gridInfo.minPos} with yawRotation {gridInfo.yawRot}");
        }

        private static void doConveyorBuild(int resID, GridInfo gridInfo, ConveyorBuildInfo.ChainData? nullableChainData, bool reverseConveyor, bool shouldLog) {
            if(nullableChainData == null) {
                ModUtils.LogEMUError($"You cannot build a conveyor with null ChainData. Aborting build attempt.");
                return;
            }

            if (shouldLog) ModUtils.LogEMUInfo($"Attempting to build conveyor");

            ConveyorBuildInfo.ChainData chainData = (ConveyorBuildInfo.ChainData)nullableChainData;

            if (shouldLog) {
                ModUtils.LogEMUInfo($"chainData.count: {chainData.count}");
                ModUtils.LogEMUInfo($"chainData.rotation: {chainData.rotation}");
                ModUtils.LogEMUInfo($"chainData.shape: {chainData.shape}");
                ModUtils.LogEMUInfo($"chainData.start: {chainData.start}");
                ModUtils.LogEMUInfo($"chainData.height: {chainData.height}");
            }

            ConveyorBuildInfo conveyorBuildInfo = new ConveyorBuildInfo() {
                machineType = resID,
                chainData = new List<ConveyorBuildInfo.ChainData>() { chainData },
                isReversed = reverseConveyor,
                machineIds = new List<uint>(),
                autoHubsEnabled = false
            };
            BuilderInfo builderInfo = (BuilderInfo)SaveState.GetResInfoFromId(resID);
            StreamedHologramData hologram = getHologram(builderInfo, gridInfo, -1, chainData);

            ConveyorBuilder builder = (ConveyorBuilder)Player.instance.builder.GetBuilderForType(BuilderInfo.BuilderType.ConveyorBuilder);
            builder.beltBuildInfo = conveyorBuildInfo;
            builder = (ConveyorBuilder)setCommonBuilderFields(builder, builderInfo, gridInfo, hologram);

            setPlayerBuilderPrivateFields(builder, builderInfo);

            builder.BuildFromNetworkData(conveyorBuildInfo, false);
            Player.instance.inventory.TryRemoveResources(resID, 1);

            if (shouldLog) ModUtils.LogEMUInfo($"Built {builderInfo.displayName} at {gridInfo.minPos} with yawRotation {gridInfo.yawRot}");
        }

        private static void doDrillBuild(int resID, GridInfo gridInfo) {
            SimpleBuildInfo simpleBuildInfo = new SimpleBuildInfo() {
                machineType = resID,
                rotation = gridInfo.yawRot,
                minGridPos = gridInfo.minPos
            };
            BuilderInfo builderInfo = (BuilderInfo)SaveState.GetResInfoFromId(resID);
            StreamedHologramData hologram = getHologram(builderInfo, gridInfo);

            DrillBuilder builder = (DrillBuilder)Player.instance.builder.GetBuilderForType(BuilderInfo.BuilderType.DrillBuilder);
            builder.newBuildInfo = simpleBuildInfo;
            builder = (DrillBuilder)setCommonBuilderFields(builder, builderInfo, gridInfo, hologram);

            setPlayerBuilderPrivateFields(builder, builderInfo);
            builder.BuildFromNetworkData(simpleBuildInfo, false);
            Player.instance.inventory.TryRemoveResources(resID, 1);
        }

        private static void doFloorBuild(int resID, GridInfo gridInfo) {
            FloorBuildInfo floorBuildInfo = new FloorBuildInfo() {
                machineType = resID,
                rotation = gridInfo.yawRot,
                startCorner = gridInfo.minPos,
                endCorner = gridInfo.minPos
            };
            BuilderInfo builderInfo = (BuilderInfo)SaveState.GetResInfoFromId(resID);
            StreamedHologramData hologram = getHologram(builderInfo, gridInfo);

            FloorBuilder builder = (FloorBuilder)Player.instance.builder.GetBuilderForType(BuilderInfo.BuilderType.FloorBuilder);
            builder.floorBuildInfo = floorBuildInfo;
            builder = (FloorBuilder)setCommonBuilderFields(builder, builderInfo, gridInfo, hologram);

            setPlayerBuilderPrivateFields(builder, builderInfo);
            builder.BuildFromNetworkData(floorBuildInfo, false);
            Player.instance.inventory.TryRemoveResources(resID, 1);
        }

        private static void doResearchCoreBuild(int resID, GridInfo gridInfo) {
            ResearchCoreBuildInfo researchCoreBuildInfo = new ResearchCoreBuildInfo() {
                machineType = resID,
                startCorner = gridInfo.minPos,
                endCorner = gridInfo.minPos,
                rotation = gridInfo.yawRot
            };
            BuilderInfo builderInfo = (BuilderInfo)SaveState.GetResInfoFromId(resID);
            StreamedHologramData hologram = getHologram(builderInfo, gridInfo);

            ResearchCoreBuilder builder = (ResearchCoreBuilder)Player.instance.builder.GetBuilderForType(BuilderInfo.BuilderType.ResearchCoreBuilder);
            builder.coreBuildInfo = researchCoreBuildInfo;
            builder = (ResearchCoreBuilder)setCommonBuilderFields(builder, builderInfo, gridInfo, hologram);

            setPlayerBuilderPrivateFields(builder, builderInfo);
            builder.BuildFromNetworkData(researchCoreBuildInfo, false);
            Player.instance.inventory.TryRemoveResources(resID, 1);
        }

        private static void doStairsBuild(int resID, GridInfo gridInfo) {
            StairsBuildInfo stairsBuildInfo = new StairsBuildInfo() {
                machineType = resID,
                startCorner = gridInfo.minPos,
                dragDiff = Vector3Int.zero,
                rotation = gridInfo.yawRot
            };
            BuilderInfo builderInfo = (BuilderInfo)SaveState.GetResInfoFromId(resID);
            StreamedHologramData hologram = getHologram(builderInfo, gridInfo);

            StairsBuilder builder = (StairsBuilder)Player.instance.builder.GetBuilderForType(BuilderInfo.BuilderType.StairsBuilder);
            builder.stairsBuildInfo = stairsBuildInfo;
            builder = (StairsBuilder)setCommonBuilderFields(builder, builderInfo, gridInfo, hologram);

            setPlayerBuilderPrivateFields(builder, builderInfo);
            builder.BuildFromNetworkData(stairsBuildInfo, false);
            Player.instance.inventory.TryRemoveResources(resID, 1);
        }

        private static void doStructureBuild(int resID, GridInfo gridInfo, int variationIndex) {
            SimpleBuildInfo simpleBuildInfo = new SimpleBuildInfo() {
                machineType = resID,
                rotation = gridInfo.yawRot,
                minGridPos = gridInfo.minPos
            };
            BuilderInfo builderInfo = (BuilderInfo)SaveState.GetResInfoFromId(resID);
            StreamedHologramData hologram = getHologram(builderInfo, gridInfo, variationIndex);

            StructureBuilder builder = (StructureBuilder)Player.instance.builder.GetBuilderForType(BuilderInfo.BuilderType.StructureBuilder);
            builder.newBuildInfo = simpleBuildInfo;
            builder.currentVariationIndex = variationIndex;
            builder = (StructureBuilder)setCommonBuilderFields(builder, builderInfo, gridInfo, hologram);

            setPlayerBuilderPrivateFields(builder, builderInfo);
            doBuild(builder, builderInfo, -1);
        }

        // Private Functions

        private static StreamedHologramData getHologram(BuilderInfo builderInfo, GridInfo gridInfo, int variationIndex = -1, ConveyorBuildInfo.ChainData? nullableChainData = null) {
            StreamedHologramData hologram = null;
            Vector3 thisHologramPos = gridInfo.BottomCenter;
            int yawRotation = 0;
            MachineTypeEnum type = builderInfo.GetInstanceType();

            if(type == MachineTypeEnum.Conveyor) {
                ConveyorBuildInfo.ChainData chainData = (ConveyorBuildInfo.ChainData)nullableChainData;

                ConveyorInstance conveyor = MachineManager.instance.Get<ConveyorInstance, ConveyorDefinition>(0, type);
                ConveyorHologramData conveyorHologram = conveyor.myDef.GenerateUnbuiltHologramData() as ConveyorHologramData;
                conveyorHologram.buildBackwards = false;
                conveyorHologram.curShape = chainData.shape;
                conveyorHologram.numBelts = 1;

                thisHologramPos.x += conveyor.gridInfo.dims.x / 2.0f;
                thisHologramPos.z += conveyor.gridInfo.dims.z / 2.0f;
                yawRotation = gridInfo.yawRot;

                Quaternion conveyorRotation = Quaternion.Euler(0, yawRotation, 0);
                conveyorHologram.SetTransform(thisHologramPos, conveyorRotation);
                conveyorHologram.type = builderInfo;
                return conveyorHologram;
            }

            switch (type) {
                case MachineTypeEnum.Assembler:
                    AssemblerInstance assembler = MachineManager.instance.Get<AssemblerInstance, AssemblerDefinition>(0, type);
                    hologram = assembler.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += assembler.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += assembler.gridInfo.dims.z / 2.0f;
                    yawRotation = assembler.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.Chest:
                    ChestInstance chest = MachineManager.instance.Get<ChestInstance, ChestDefinition>(0, type);
                    hologram = chest.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += chest.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += chest.gridInfo.dims.z / 2.0f;
                    yawRotation = chest.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.Drill:
                    DrillInstance drill = MachineManager.instance.Get<DrillInstance, DrillDefinition>(0, type);
                    hologram = drill.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += drill.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += drill.gridInfo.dims.z / 2.0f;
                    yawRotation = drill.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.Floor:
                    FloorInstance floor = MachineManager.instance.Get<FloorInstance, FloorDefinition>(0, type);
                    hologram = floor.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += floor.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += floor.gridInfo.dims.z / 2.0f;
                    yawRotation = floor.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.Inserter:
                    InserterInstance inserter = MachineManager.instance.Get<InserterInstance, InserterDefinition>(0, type);
                    hologram = inserter.myDef.GenerateUnbuiltHologramData();
                    yawRotation = inserter.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.LightSticks:
                    LightStickInstance lightStick = MachineManager.instance.Get<LightStickInstance, LightStickDefinition>(0, type);
                    hologram = lightStick.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += lightStick.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += lightStick.gridInfo.dims.z / 2.0f;
                    yawRotation = lightStick.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.Planter:
                    PlanterInstance planter = MachineManager.instance.Get<PlanterInstance, PlanterDefinition>(0, type);
                    hologram = planter.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += planter.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += planter.gridInfo.dims.z / 2.0f;
                    yawRotation = planter.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.PowerGenerator:
                    PowerGeneratorInstance generator = MachineManager.instance.Get<PowerGeneratorInstance, PowerGeneratorDefinition>(0, type);
                    hologram = generator.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += generator.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += generator.gridInfo.dims.z / 2.0f;
                    yawRotation = generator.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.ResearchCore:
                    ResearchCoreInstance researchCore = MachineManager.instance.Get<ResearchCoreInstance, ResearchCoreDefinition>(0, type);
                    hologram = researchCore.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += researchCore.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += researchCore.gridInfo.dims.z / 2.0f;
                    yawRotation = researchCore.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.Smelter:
                    SmelterInstance smelter = MachineManager.instance.Get<SmelterInstance, SmelterDefinition>(0, type);
                    hologram = smelter.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += smelter.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += smelter.gridInfo.dims.z / 2.0f;
                    yawRotation = smelter.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.Stairs:
                    StairsInstance stairs = MachineManager.instance.Get<StairsInstance, StairsDefinition>(0, type);
                    hologram = stairs.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += stairs.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += stairs.gridInfo.dims.z / 2.0f;
                    yawRotation = stairs.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.Thresher:
                    ThresherInstance thresher = MachineManager.instance.Get<ThresherInstance, ThresherDefinition>(0, type);
                    hologram = thresher.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += thresher.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += thresher.gridInfo.dims.z / 2.0f;
                    yawRotation = thresher.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.TransitDepot:
                    TransitDepotInstance transitDepot = MachineManager.instance.Get<TransitDepotInstance, TransitDepotDefinition>(0, type);
                    hologram = transitDepot.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += transitDepot.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += transitDepot.gridInfo.dims.z / 2.0f;
                    yawRotation = transitDepot.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.TransitPole:
                    TransitPoleInstance transitPole = MachineManager.instance.Get<TransitPoleInstance, TransitPoleDefinition>(0, type);
                    hologram = transitPole.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += transitPole.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += transitPole.gridInfo.dims.z / 2.0f;
                    yawRotation = transitPole.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.WaterWheel:
                    WaterWheelInstance waterWheel = MachineManager.instance.Get<WaterWheelInstance, WaterWheelDefinition>(0, type);
                    hologram = waterWheel.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += waterWheel.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += waterWheel.gridInfo.dims.z / 2.0f;
                    yawRotation = waterWheel.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.Accumulator:
                    AccumulatorInstance accumulator = MachineManager.instance.Get<AccumulatorInstance, AccumulatorDefinition>(0, type);
                    hologram = accumulator.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += accumulator.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += accumulator.gridInfo.dims.z / 2.0f;
                    yawRotation = accumulator.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.HighVoltageCable:
                    HighVoltageCableInstance hvc = MachineManager.instance.Get<HighVoltageCableInstance, HighVoltageCableDefinition>(0, type);
                    hologram = hvc.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += hvc.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += hvc.gridInfo.dims.z / 2.0f;
                    yawRotation = hvc.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.VoltageStepper:
                    VoltageStepperInstance stepper = MachineManager.instance.Get<VoltageStepperInstance, VoltageStepperDefinition>(0, type);
                    hologram = stepper.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += stepper.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += stepper.gridInfo.dims.z / 2.0f;
                    yawRotation = stepper.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.Structure:
                    StructureInstance structure = MachineManager.instance.Get<StructureInstance, StructureDefinition>(0, type);
                    hologram = structure.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += structure.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += structure.gridInfo.dims.z / 2.0f;
                    yawRotation = structure.gridInfo.yawRot;
                    break;

                case MachineTypeEnum.BlastSmelter:
                    BlastSmelterInstance blastSmelter = MachineManager.instance.Get<BlastSmelterInstance, BlastSmelterDefinition>(0, type);
                    hologram = blastSmelter.myDef.GenerateUnbuiltHologramData();
                    thisHologramPos.x += blastSmelter.gridInfo.dims.x / 2.0f;
                    thisHologramPos.z += blastSmelter.gridInfo.dims.z / 2.0f;
                    yawRotation = blastSmelter.gridInfo.yawRot;
                    break;

                default:
                    Debug.Log($"Skipped rendering hologram for unknown type: {type}");
                    break;
            }

            if (variationIndex != -1) hologram.variationNum = variationIndex;

            Quaternion rotation = Quaternion.Euler(0, yawRotation, 0);
            hologram.SetTransform(thisHologramPos, rotation);
            hologram.type = builderInfo;
            return hologram;
        }

        private static ProceduralBuilder setCommonBuilderFields(ProceduralBuilder builder, BuilderInfo builderInfo, GridInfo gridInfo, StreamedHologramData hologram) {
            builder.curBuilderInfo = builderInfo;
            builder.myNewGridInfo = gridInfo;
            builder.myHolo = hologram;
            builder.recentlyBuilt = true;
            builder.OnShow();
            return builder;
        }

        private static void setPlayerBuilderPrivateFields(ProceduralBuilder builder, BuilderInfo builderInfo) {
            ModUtils.SetPrivateField("_currentBuilder", Player.instance.builder, builder);
            ModUtils.SetPrivateField("_lastBuilderInfo", Player.instance.builder, builderInfo);
            ModUtils.SetPrivateField("_lastBuildPos", Player.instance.builder, builder.curGridPlacement.MinInt);
        }

        private static void doBuild(ProceduralBuilder builder, BuilderInfo builderInfo, int recipeID) {
            BuildMachineAction action = builder.GenerateNetworkData();
            action.recipeId = recipeID;
            action.resourceCostID = builderInfo.uniqueId;
            action.resourceCostAmount = 1;
            NetworkMessageRelay.instance.SendNetworkAction(action);
        }
    }
}
