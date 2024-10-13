﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace EquinoxsModUtils
{
    public static partial class EMU 
    {
        /// <summary>
        /// Contains const strings of in-game names
        /// </summary>
        public static class Names 
        {
            /// <summary>
            /// Contains const strings of in-game names of Resources
            /// </summary>
            public static class Resources 
            {
                public const string ConveyorBelt = "Conveyor Belt";
                public const string ConveyorBeltMKII = "Conveyor Belt MKII";
                public const string ConveyorBeltMKIII = "Conveyor Belt MKIII";
                public const string Inserter = "Inserter";
                public const string LongInserter = "Long Inserter";
                public const string FilterInserter = "Filter Inserter";
                public const string FastInserter = "Fast Inserter";
                public const string StackInserter = "Stack Inserter";
                public const string StackFilterInserter = "Stack Filter Inserter";
                public const string Container = "Container";
                public const string MonorailDepot = "Monorail Depot";
                public const string MonorailPole = "Monorail Pole";
                public const string MonorailTrack = "Monorail Track";
                public const string MiningDrill = "Mining Drill";
                public const string BlastDrill = "Blast Drill";
                public const string MiningDrillMKII = "Mining Drill MKII";
                public const string MiningCharge = "Mining Charge";
                public const string Smelter = "Smelter";
                public const string SmelterMKII = "Smelter MKII";
                public const string BlastSmelter = "Blast Smelter";
                public const string Assembler = "Assembler";
                public const string AssemblerMKII = "Assembler MKII";
                public const string Thresher = "Thresher";
                public const string ThresherMKII = "Thresher MKII";
                public const string Planter = "Planter";
                public const string CrankGenerator = "Crank Generator";
                public const string CrankGeneratorMKII = "Crank Generator MKII";
                public const string WaterWheel = "Water Wheel";
                public const string CoreComposer = "Core Composer";
                public const string ResearchCore380nmPurple = "Research Core 380nm (Purple)";
                public const string ResearchCore480nmBlue = "Research Core 480nm (Blue)";
                public const string Accumulator = "Accumulator";
                public const string VoltageStepper = "Voltage Stepper";
                public const string HighVoltageCable = "High Voltage Cable";
                public const string Beacon = "Beacon";
                public const string Biobrick = "Biobrick";
                public const string BiobrickDieselUnrefined = "Biobrick Diesel (Unrefined)";
                public const string BiobrickDieselInfusionPack = "Biobrick Diesel Infusion Pack";
                public const string BiobrickDieselImpure = "Biobrick Diesel (Impure)";
                public const string BiobrickDieselPure = "Biobrick Diesel (Pure)";
                public const string KindlevineStems = "Kindlevine Stems";
                public const string KindlevineStemsWashed = "Kindlevine Stems (Washed)";
                public const string Plantmatter = "Plantmatter";
                public const string KindlevineSeed = "Kindlevine Seed";
                public const string ShiverthornSeed = "Shiverthorn Seed";
                public const string Kindlevine = "Kindlevine";
                public const string Shiverthorn = "Shiverthorn";
                public const string ShiverthornBuds = "Shiverthorn Buds";
                public const string ShiverthornBudsNeutralized = "Shiverthorn Buds (Neutralized)";
                public const string KindlevineExtract = "Kindlevine Extract";
                public const string ShiverthornExtract = "Shiverthorn Extract";
                public const string CarbonPowder = "Carbon Powder";
                public const string Quicklime = "Quicklime";
                public const string Cement = "Cement";
                public const string AtlantumPowder = "Atlantum Powder";
                public const string Sand = "Sand";
                public const string Gravel = "Gravel";
                public const string Limestone = "Limestone";
                public const string PlantmatterFiber = "Plantmatter Fiber";
                public const string Dirt = "Dirt";
                public const string IronOre = "Iron Ore";
                public const string IronOrePowder = "Iron Ore Powder";
                public const string IronChunk = "Iron Chunk";
                public const string RefinedIronChunk = "Refined Iron Chunk";
                public const string CopperOre = "Copper Ore";
                public const string CopperOrePowder = "Copper Ore Powder";
                public const string CopperChunk = "Copper Chunk";
                public const string RefinedCopperChunk = "Refined Copper Chunk";
                public const string AtlantumOre = "Atlantum Ore";
                public const string AtlantumChunk = "Atlantum Chunk";
                public const string IronIngot = "Iron Ingot";
                public const string IronSlab = "Iron Slab";
                public const string CopperIngot = "Copper Ingot";
                public const string CopperSlab = "Copper Slab";
                public const string SteelMixture = "Steel Mixture";
                public const string SteelIngot = "Steel Ingot";
                public const string SteelSlab = "Steel Slab";
                public const string AtlantumMixture = "Atlantum Mixture";
                public const string AtlantumIngot = "Atlantum Ingot";
                public const string AtlantumSlab = "Atlantum Slab";
                public const string LimestoneBrick = "Limestone Brick";
                public const string CarbonPowderBrick = "Carbon Powder Brick";
                public const string KindlevineExtractBrick = "Kindlevine Extract Brick";
                public const string AtlantumPowderBrick = "Atlantum Powder Brick";
                public const string AtlantumMixtureBrick = "Atlantum Mixture Brick";
                public const string PlantmatterBrick = "Plantmatter Brick";
                public const string Clay = "Clay";
                public const string Concrete = "Concrete";
                public const string DecorativeConcrete = "Decorative Concrete";
                public const string Glass = "Glass";
                public const string PlantmatterFrame = "Plantmatter Frame";
                public const string CeramicParts = "Ceramic Parts";
                public const string IronFrame = "Iron Frame";
                public const string IronComponents = "Iron Components";
                public const string IronMechanism = "Iron Mechanism";
                public const string CopperWire = "Copper Wire";
                public const string CopperFrame = "Copper Frame";
                public const string CopperComponents = "Copper Components";
                public const string CopperMechanism = "Copper Mechanism";
                public const string ShiverthornCoolant = "Shiverthorn Coolant";
                public const string CoolingSystem = "Cooling System";
                public const string MechanicalComponents = "Mechanical Components";
                public const string ElectricalComponents = "Electrical Components";
                public const string ProcessorUnit = "Processor Unit";
                public const string CeramicTiles = "Ceramic Tiles";
                public const string SteelFrame = "Steel Frame";
                public const string WireSpindle = "Wire Spindle";
                public const string Gearbox = "Gearbox";
                public const string ReinforcedIronFrame = "Reinforced Iron Frame";
                public const string ReinforcedCopperFrame = "Reinforced Copper Frame";
                public const string ElectricalSet = "Electrical Set";
                public const string ElectricFrame = "Electric Frame";
                public const string ElectricMotor = "Electric Motor";
                public const string ProcessorArray = "Processor Array";
                public const string AdvancedCircuit = "Advanced Circuit";
                public const string RelayCircuit = "Relay Circuit";
                public const string ShiverthornExtractGel = "Shiverthorn Extract Gel";
                public const string IronInfusedLimestone = "Iron-Infused Limestone";
                public const string CopperInfusedLimestone = "Copper-Infused Limestone";
                public const string AtlantumInfusedLimestone = "Atlantum-Infused Limestone";
                public const string PowerFloor = "Power Floor";
                public const string CalycitePlatform1x1 = "Calycite Platform (1x1)";
                public const string CalycitePlatform3x3 = "Calycite Platform (3x3)";
                public const string CalycitePlatform5x5 = "Calycite Platform (5x5)";
                public const string DiscoBlock1x1 = "Disco Block (1x1)";
                public const string GlowBlock1x1 = "Glow Block (1x1)";
                public const string Catwalk3x9 = "Catwalk (3x9)";
                public const string Catwalk5x9 = "Catwalk (5x9)";
                public const string CalyciteWall3x3 = "Calycite Wall (3x3)";
                public const string CalyciteWall5x3 = "Calycite Wall (5x3)";
                public const string CalyciteWall5x5 = "Calycite Wall (5x5)";
                public const string CalyciteGate5x2 = "Calycite Gate (5x2)";
                public const string CalyciteGate5x5 = "Calycite Gate (5x5)";
                public const string CalyciteGate10x5 = "Calycite Gate (10x5)";
                public const string CalyciteWallCap3x1 = "Calycite Wall Cap (3x1)";
                public const string CalyciteWallCap5x1 = "Calycite Wall Cap (5x1)";
                public const string CalyciteWallCorner1x1 = "Calycite Wall Corner (1x1)";
                public const string CalyciteVerticalWallCap3x1 = "Calycite Vertical Wall Cap (3x1)";
                public const string CalyciteVerticalWallCap5x1 = "Calycite Vertical Wall Cap (5x1)";
                public const string CalyciteVerticalWallCorner1x1 = "Calycite Vertical Wall Corner (1x1)";
                public const string CalyciteWallCutaway2x2 = "Calycite Wall Cutaway (2x2)";
                public const string CalyciteWallCutaway3x3 = "Calycite Wall Cutaway (3x3)";
                public const string CalyciteWallCutaway5x3 = "Calycite Wall Cutaway (5x3)";
                public const string CalyciteWallCutaway5x5 = "Calycite Wall Cutaway (5x5)";
                public const string MetalStairs1x1 = "Metal Stairs (1x1)";
                public const string MetalStairs3x1 = "Metal Stairs (3x1)";
                public const string MetalStairs3x3 = "Metal Stairs (3x3)";
                public const string MetalStairs3x5 = "Metal Stairs (3x5)";
                public const string LightStick = "Light Stick";
                public const string RedLightStick = "Red Light Stick";
                public const string GreenLightStick = "Green Light Stick";
                public const string BlueLightStick = "Blue Light Stick";
                public const string OverheadLight = "Overhead Light";
                public const string StandingLamp1x5 = "Standing Lamp (1x5)";
                public const string WallLight1x1 = "Wall Light (1x1)";
                public const string WallLight3x1 = "Wall Light (3x1)";
                public const string HangingLamp1x1 = "Hanging Lamp (1x1)";
                public const string FanLamp7x2 = "Fan Lamp (7x2)";
                public const string Railing1x1 = "Railing (1x1)";
                public const string Railing3x1 = "Railing (3x1)";
                public const string Railing5x1 = "Railing (5x1)";
                public const string RailingCorner1x1 = "Railing Corner (1x1)";
                public const string RailingCorner3x3 = "Railing Corner (3x3)";
                public const string MetalPillar1x1 = "Metal Pillar (1x1)";
                public const string MetalPillar1x3 = "Metal Pillar (1x3)";
                public const string MetalPillar1x5 = "Metal Pillar (1x5)";
                public const string CalycitePillar1x1 = "Calycite Pillar (1x1)";
                public const string CalycitePillar1x3 = "Calycite Pillar (1x3)";
                public const string CalycitePillar1x5 = "Calycite Pillar (1x5)";
                public const string CalyciteAngleSupport3x3 = "Calycite Angle Support (3x3)";
                public const string MetalAngleSupport5x2 = "Metal Angle Support (5x2)";
                public const string MetalRibBase1x2 = "Metal Rib Base (1x2)";
                public const string MetalRibMiddle1x3 = "Metal Rib Middle (1x3)";
                public const string CalyciteBeam1x1 = "Calycite Beam (1x1)";
                public const string CalyciteBeam3x1 = "Calycite Beam (3x1)";
                public const string CalyciteBeam5x1 = "Calycite Beam (5x1)";
                public const string MetalBeam1x1 = "Metal Beam (1x1)";
                public const string MetalBeam1x3 = "Metal Beam (1x3)";
                public const string MetalBeam1x5 = "Metal Beam (1x5)";
                public const string CalyciteRamp1x1 = "Calycite Ramp (1x1)";
                public const string CalyciteRamp1x3 = "Calycite Ramp (1x3)";
                public const string CalyciteRamp1x5 = "Calycite Ramp (1x5)";
                public const string Crate = "Crate";
                public const string SmallFloorPot = "Small Floor Pot";
                public const string WallPot = "Wall Pot";
                public const string MediumFloorPot = "Medium Floor Pot";
                public const string CeilingPlant1x1 = "Ceiling Plant (1x1)";
                public const string CeilingPlant3x3 = "Ceiling Plant (3x3)";
                public const string WallPlant1x1 = "Wall Plant (1x1)";
                public const string WallPlant3x3 = "Wall Plant (3x3)";
                public const string SectionalCorner2x2 = "Sectional Corner (2x2)";
                public const string StandardPickaxe = "Standard Pickaxe";
                public const string Scanner = "Scanner";
                public const string Omniseeker = "Omniseeker";
                public const string TheMOLE = "The M.O.L.E.";
                public const string HoverPack = "Hover Pack";
                public const string Railrunner = "Railrunner";
                public const string Replacer = "Replacer";
                public const string LaserPistol = "Laser Pistol";
                public const string SonarTower = "Sonar Tower";
                public const string Nexus = "Nexus";
                public const string SpectralCubeGarnet = "Spectral Cube (Garnet)";
                public const string SpectralCubeEmerald = "Spectral Cube (Emerald)";

                /// <summary>
                /// A spoiler-friendly list of all item names.
                /// </summary>
                public static readonly List<string> SafeResources = new List<string>() {
                    ConveyorBelt,
                    ConveyorBeltMKII,
                    ConveyorBeltMKIII,
                    Inserter,
                    LongInserter,
                    FilterInserter,
                    FastInserter,
                    StackInserter,
                    StackFilterInserter,
                    Container,
                    MonorailDepot,
                    MonorailPole,
                    MonorailTrack,
                    MiningDrill,
                    BlastDrill,
                    MiningDrillMKII,
                    MiningCharge,
                    Smelter,
                    SmelterMKII,
                    BlastSmelter,
                    Assembler,
                    AssemblerMKII,
                    Thresher,
                    ThresherMKII,
                    Planter,
                    CrankGenerator,
                    CrankGeneratorMKII,
                    WaterWheel,
                    CoreComposer,
                    ResearchCore380nmPurple,
                    ResearchCore480nmBlue,
                    Accumulator,
                    VoltageStepper,
                    HighVoltageCable,
                    Beacon,
                    Biobrick,
                    BiobrickDieselUnrefined,
                    BiobrickDieselInfusionPack,
                    BiobrickDieselImpure,
                    BiobrickDieselPure,
                    KindlevineStems,
                    KindlevineStemsWashed,
                    Plantmatter,
                    KindlevineSeed,
                    ShiverthornSeed,
                    Kindlevine,
                    Shiverthorn,
                    ShiverthornBuds,
                    ShiverthornBudsNeutralized,
                    KindlevineExtract,
                    ShiverthornExtract,
                    CarbonPowder,
                    Quicklime,
                    Cement,
                    AtlantumPowder,
                    Sand,
                    Gravel,
                    Limestone,
                    PlantmatterFiber,
                    Dirt,
                    IronOre,
                    IronOrePowder,
                    IronChunk,
                    RefinedIronChunk,
                    CopperOre,
                    CopperOrePowder,
                    CopperChunk,
                    RefinedCopperChunk,
                    AtlantumOre,
                    AtlantumChunk,
                    IronIngot,
                    IronSlab,
                    CopperIngot,
                    CopperSlab,
                    SteelMixture,
                    SteelIngot,
                    SteelSlab,
                    AtlantumMixture,
                    AtlantumIngot,
                    AtlantumSlab,
                    LimestoneBrick,
                    CarbonPowderBrick,
                    KindlevineExtractBrick,
                    AtlantumPowderBrick,
                    AtlantumMixtureBrick,
                    PlantmatterBrick,
                    Clay,
                    Concrete,
                    DecorativeConcrete,
                    Glass,
                    PlantmatterFrame,
                    CeramicParts,
                    IronFrame,
                    IronComponents,
                    IronMechanism,
                    CopperWire,
                    CopperFrame,
                    CopperComponents,
                    CopperMechanism,
                    ShiverthornCoolant,
                    CoolingSystem,
                    MechanicalComponents,
                    ElectricalComponents,
                    ProcessorUnit,
                    CeramicTiles,
                    SteelFrame,
                    WireSpindle,
                    Gearbox,
                    ReinforcedIronFrame,
                    ReinforcedCopperFrame,
                    ElectricalSet,
                    ElectricFrame,
                    ElectricMotor,
                    ProcessorArray,
                    AdvancedCircuit,
                    RelayCircuit,
                    ShiverthornExtractGel,
                    IronInfusedLimestone,
                    CopperInfusedLimestone,
                    AtlantumInfusedLimestone,
                    PowerFloor,
                    CalycitePlatform1x1,
                    CalycitePlatform3x3,
                    CalycitePlatform5x5,
                    DiscoBlock1x1,
                    GlowBlock1x1,
                    Catwalk3x9,
                    Catwalk5x9,
                    CalyciteWall3x3,
                    CalyciteWall5x3,
                    CalyciteWall5x5,
                    CalyciteGate5x2,
                    CalyciteGate5x5,
                    CalyciteGate10x5,
                    CalyciteWallCap3x1,
                    CalyciteWallCap5x1,
                    CalyciteWallCorner1x1,
                    CalyciteVerticalWallCap3x1,
                    CalyciteVerticalWallCap5x1,
                    CalyciteVerticalWallCorner1x1,
                    CalyciteWallCutaway2x2,
                    CalyciteWallCutaway3x3,
                    CalyciteWallCutaway5x3,
                    CalyciteWallCutaway5x5,
                    MetalStairs1x1,
                    MetalStairs3x1,
                    MetalStairs3x3,
                    MetalStairs3x5,
                    LightStick,
                    RedLightStick,
                    GreenLightStick,
                    BlueLightStick,
                    OverheadLight,
                    StandingLamp1x5,
                    WallLight1x1,
                    WallLight3x1,
                    HangingLamp1x1,
                    FanLamp7x2,
                    Railing1x1,
                    Railing3x1,
                    Railing5x1,
                    RailingCorner1x1,
                    RailingCorner3x3,
                    MetalPillar1x1,
                    MetalPillar1x3,
                    MetalPillar1x5,
                    CalycitePillar1x1,
                    CalycitePillar1x3,
                    CalycitePillar1x5,
                    CalyciteAngleSupport3x3,
                    MetalAngleSupport5x2,
                    MetalRibBase1x2,
                    MetalRibMiddle1x3,
                    CalyciteBeam1x1,
                    CalyciteBeam3x1,
                    CalyciteBeam5x1,
                    MetalBeam1x1,
                    MetalBeam1x3,
                    MetalBeam1x5,
                    CalyciteRamp1x1,
                    CalyciteRamp1x3,
                    CalyciteRamp1x5,
                    Crate,
                    SmallFloorPot,
                    WallPot,
                    MediumFloorPot,
                    CeilingPlant1x1,
                    CeilingPlant3x3,
                    WallPlant1x1,
                    WallPlant3x3,
                    SectionalCorner2x2,
                    StandardPickaxe,
                    Scanner,
                    Omniseeker,
                    TheMOLE,
                    HoverPack,
                    Railrunner,
                    Replacer,
                };
            }

            /// <summary>
            /// Contains const strings of in-game names of Unlocks
            /// </summary>
            public static class Unlocks 
            {
                public const string Accumulator = "Accumulator";
                public const string AccumulationII = "Accumulation II";
                public const string AccumulationIII = "Accumulation III";
                public const string AccumulationIV = "Accumulation IV";
                public const string AssemblerMKII = "Assembler MKII";
                public const string AtlantumSlab = "Atlantum Slab";
                public const string AdvancedComponents = "Advanced Components";
                public const string AdvancedElectronics = "Advanced Electronics";
                public const string AdvancedForging = "Advanced Forging";
                public const string AdvancedSlabThreshing = "Advanced Slab Threshing";
                public const string AdvancedOptimization = "Advanced Optimization";
                public const string AdvancedPowderBricks = "Advanced Powder Bricks";
                public const string AdvancedBrickThreshing = "Advanced Brick Threshing";
                public const string SteelSlab = "Steel Slab";
                public const string AdvancedClayProduction = "Advanced Clay Production";
                public const string Assembler = "Assembler";
                public const string ASMPowerTrimII = "ASM PowerTrim II";
                public const string ASMPowerTrimIII = "ASM PowerTrim III";
                public const string AssemblingSpeedII = "Assembling Speed II";
                public const string AssemblingSpeedIII = "Assembling Speed III";
                public const string AtlantumOreThreshing = "Atlantum Ore Threshing";
                public const string AtlantumIngot = "Atlantum Ingot";
                public const string AtlantumMixture = "Atlantum Mixture";
                public const string AtlantumPowderBricks = "Atlantum Powder Bricks";
                public const string AtlantumThreshing = "Atlantum Threshing";
                public const string AtlantumInfusion = "Atlantum Infusion";
                public const string InfusedAtlantumThreshing = "Infused Atlantum Threshing";
                public const string Smelter = "Smelter";
                public const string CrankGenerator = "Crank Generator";
                public const string CrankSpan = "CrankSpan";
                public const string BasicForging = "Basic Forging";
                public const string BasicSlabThreshing = "Basic Slab Threshing";
                public const string BasicIncineration = "Basic Incineration";
                public const string BasicLogistics = "Basic Logistics";
                public const string BasicManufacturing = "Basic Manufacturing";
                public const string BasicMechanisms = "Basic Mechanisms";
                public const string BasicOptimization = "Basic Optimization";
                public const string BasicPowderBricks = "Basic Powder Bricks";
                public const string BasicBrickThreshing = "Basic Brick Threshing";
                public const string BasicSlabs = "Basic Slabs";
                public const string Beacon = "Beacon";
                public const string VBHeightIII = "VB-Height III";
                public const string VBHeightII = "VB-Height II";
                public const string Biobrick = "Biobrick";
                public const string BioDenseII = "BioDense II";
                public const string BioDenseIII = "BioDense III";
                public const string BioDenseIV = "BioDense IV";
                public const string BioDenseV = "BioDense V";
                public const string BDRMultiBlastIV = "BDR-MultiBlast IV";
                public const string BDRMultiBlastV = "BDR-MultiBlast V";
                public const string BDRMultiBlastII = "BDR-MultiBlast II";
                public const string BDRMultiBlastIII = "BDR-MultiBlast III";
                public const string BlastSmelter = "Blast Smelter";
                public const string BSMMultiBlastIV = "BSM-MultiBlast IV";
                public const string BSMMultiBlastV = "BSM-MultiBlast V";
                public const string BSMMultiBlastII = "BSM-MultiBlast II";
                public const string BSMMultiBlastIII = "BSM-MultiBlast III";
                public const string CarbonPowder = "Carbon Powder";
                public const string StairsAndWalkwaysI = "Stairs & Walkways I";
                public const string StairsAndWalkwaysII = "Stairs & Walkways II";
                public const string StairsAndWalkwaysIII = "Stairs & Walkways III";
                public const string Cement = "Cement";
                public const string Ceramics = "Ceramics";
                public const string CeramicsThreshing = "Ceramics Threshing";
                public const string LightStickPrimaryColors = "Light Stick (Primary Colors)";
                public const string Concrete = "Concrete";
                public const string FloraCompression = "Flora Compression";
                public const string PlantmatterBrickThreshing = "Plantmatter Brick Threshing";
                public const string CoolantCompression = "Coolant Compression";
                public const string ShiverthornGelThreshing = "Shiverthorn Gel Threshing";
                public const string CoolDenseII = "CoolDense II";
                public const string CoolDenseIII = "CoolDense III";
                public const string CoolDenseIV = "CoolDense IV";
                public const string CoolDenseV = "CoolDense V";
                public const string CoolingSystems = "Cooling Systems";
                public const string CoreComposer = "Core Composer";
                public const string CoreClustering = "Core Clustering";
                public const string Decomposition = "Decomposition";
                public const string DECOSeriesI = "DECO Series I";
                public const string DECOSeriesII = "DECO Series II";
                public const string DECOSeriesIII = "DECO Series III";
                public const string DECOPlantsAndCeilings = "DECO Plants & Ceilings";
                public const string DECOPlantsAndWalls = "DECO Plants & Walls";
                public const string DecorativeMaterials = "Decorative Materials";
                public const string MiningCharge = "Mining Charge";
                public const string MassProductionMiningCharge = "Mass Production (Mining Charge)";
                public const string Do_not_use_tech = "Do_not_use_tech";
                public const string ElectricComponents = "Electric Components";
                public const string MiningDrillMKII = "Mining Drill MKII";
                public const string MDRPowerTrimII = "MDR PowerTrim II";
                public const string MDRPowerTrimIII = "MDR PowerTrim III";
                public const string ElectricMotor = "Electric Motor";
                public const string SmelterMKII = "Smelter MKII";
                public const string ThresherMKII = "Thresher MKII";
                public const string THRPowerTrimII = "THR PowerTrim II";
                public const string THRPowerTrimIII = "THR PowerTrim III";
                public const string VerticalBelts = "Vertical Belts";
                public const string BlastDrill = "Blast Drill";
                public const string ConveyorBeltMKII = "Conveyor Belt MKII";
                public const string ConveyorBeltMKIII = "Conveyor Belt MKIII";
                public const string FastInserter = "Fast Inserter";
                public const string FiberDecomposition = "Fiber Decomposition";
                public const string FiberExtraction = "Fiber Extraction";
                public const string FilterInserter = "Filter Inserter";
                public const string StackFilterInserter = "Stack Filter Inserter";
                public const string CoreBoostAssembly = "Core Boost (Assembly)";
                public const string CoreBoostMining = "Core Boost (Mining)";
                public const string CoreBoostPower = "Core Boost (Power)";
                public const string CoreBoostSmelting = "Core Boost (Smelting)";
                public const string CoreBoostThreshing = "Core Boost (Threshing)";
                public const string GatesI = "Gates I";
                public const string GatesII = "Gates II";
                public const string GatesIII = "Gates III";
                public const string GatesIV = "Gates IV";
                public const string GravelThreshing = "Gravel Threshing";
                public const string OptimizationBlueResearchCore = "Optimization (Blue Research Core)";
                public const string BiobrickDiesel = "Biobrick Diesel";
                public const string BiobrickDieselInfusion = "Biobrick Diesel Infusion";
                public const string BiobrickDieselRecycling = "Biobrick Diesel Recycling";
                public const string BiobrickDieselRefinement = "Biobrick Diesel Refinement";
                public const string HoverPack = "Hover Pack";
                public const string HighVoltageCable = "High Voltage Cable";
                public const string HVCReachII = "HVC Reach II";
                public const string HVCReachIII = "HVC Reach III";
                public const string HVCReachIV = "HVC Reach IV";
                public const string HVCReachV = "HVC Reach V";
                public const string PackSizeII = "PackSize II";
                public const string PackSizeIII = "PackSize III";
                public const string PackSizeIV = "PackSize IV";
                public const string PackSizeV = "PackSize V";
                public const string BlastIncineration = "Blast Incineration";
                public const string KindlevineThreshing = "Kindlevine Threshing";
                public const string StemThreshing = "Stem Threshing";
                public const string LightStick = "Light Stick";
                public const string LimestoneExtraction = "Limestone Extraction";
                public const string LongInserter = "Long Inserter";
                public const string MassProductionBasicFrames = "Mass Production (Basic Frames)";
                public const string MassProductionConveyorBelts = "Mass Production (Conveyor Belts)";
                public const string MassProductionBiobrick = "Mass Production (Biobrick)";
                public const string MassProductionClay = "Mass Production (Clay)";
                public const string MassCollect = "MassCollect";
                public const string MassCollectIII = "MassCollect III";
                public const string MassCollectIV = "MassCollect IV";
                public const string MassCollectII = "MassCollect II";
                public const string MassProductionCoolant = "Mass Production (Coolant)";
                public const string MassDeconstruct = "MassDeconstruct";
                public const string MassDeconstructIII = "MassDeconstruct III";
                public const string MassDeconstructIV = "MassDeconstruct IV";
                public const string MassDeconstructII = "MassDeconstruct II";
                public const string MassProductionInserters = "Mass Production (Inserters)";
                public const string MassProductionSteelFrames = "Mass Production (Steel Frames)";
                public const string MHaulerCapII = "M-HaulerCap II";
                public const string MHaulerCapIII = "M-HaulerCap III";
                public const string MTrackReachII = "M-TrackReach II";
                public const string MTrackReachIII = "M-TrackReach III";
                public const string MTrackReachIV = "M-TrackReach IV";
                public const string MTrackReachV = "M-TrackReach V";
                public const string MonorailSystem = "Monorail System";
                public const string MetalPowderThreshing = "Metal Powder Threshing";
                public const string MetalPowderSmelting = "Metal Powder Smelting";
                public const string MDRSpeedII = "MDR Speed II";
                public const string MDRSpeedIII = "MDR Speed III";
                public const string TheMOLE = "The M.O.L.E.";
                public const string MOLEMaximization = "M.O.L.E. Maximization";
                public const string OreInfusion = "Ore Infusion";
                public const string InfusedOreThreshing = "Infused Ore Threshing";
                public const string OverheadLight = "Overhead Light";
                public const string OverheadLightII = "Overhead Light II";
                public const string Planter = "Planter";
                public const string PlantmatterFrames = "Plantmatter Frames";
                public const string PlantmatterFramesOptimized = "Plantmatter Frames (Optimized)";
                public const string PlantmatterFiberSeparation = "Plantmatter Fiber (Separation)";
                public const string PlatformsI = "Platforms I";
                public const string BeamsI = "Beams I";
                public const string PlatformsII = "Platforms II";
                public const string BeamsII = "Beams II";
                public const string CraftSpeedII = "CraftSpeed II";
                public const string CraftSpeedIII = "CraftSpeed III";
                public const string CraftSpeedIV = "CraftSpeed IV";
                public const string HPFlightII = "HP-Flight II";
                public const string HPFlightIII = "HP-Flight III";
                public const string CrankGeneratorMKII = "Crank Generator MKII";
                public const string CrankConnect = "CrankConnect";
                public const string BasicConstruction = "Basic Construction";
                public const string PowerAmpII = "PowerAmp II";
                public const string PowerAmpIII = "PowerAmp III";
                public const string PowerAmpIV = "PowerAmp IV";
                public const string PrecisionDisassembly = "Precision Disassembly";
                public const string ProcessorUnit = "Processor Unit";
                public const string ProductionOptimization = "Production Optimization";
                public const string OptimizationPurpleResearchCore = "Optimization (Purple Research Core)";
                public const string QuicklimeBlasting = "Quicklime Blasting";
                public const string Quicklime = "Quicklime";
                public const string MonorailSpikeTrim = "Monorail Spike Trim";
                public const string MonorailSpikeTrimII = "Monorail Spike Trim II";
                public const string MonorailSpikeTrimIII = "Monorail Spike Trim III";
                public const string MonorailSpikeTrimIV = "Monorail Spike Trim IV";
                public const string BasicScience = "Basic Science";
                public const string BasicRefinement = "Basic Refinement";
                public const string ReinforcedFrames = "Reinforced Frames";
                public const string ReinforcedFramesThreshing = "Reinforced Frames Threshing";
                public const string RelayCircuit = "Relay Circuit";
                public const string Replacer = "Replacer";
                public const string ResearchCoreBlue = "Research Core (Blue)";
                public const string SandThreshing = "Sand Threshing";
                public const string Omniseeker = "Omniseeker";
                public const string ShiverthornNeutralization = "Shiverthorn Neutralization";
                public const string ShiverthornProcessing = "Shiverthorn Processing";
                public const string ShiverthornCoolant = "Shiverthorn Coolant";
                public const string Railrunner = "Railrunner";
                public const string RailRushII = "RailRush II";
                public const string RailRushIII = "RailRush III";
                public const string RailRushIV = "RailRush IV";
                public const string RailRushV = "RailRush V";
                public const string SmeltingSpeedII = "Smelting Speed II";
                public const string SmeltingSpeedIII = "Smelting Speed III";
                public const string StackInserter = "Stack Inserter";
                public const string StackCapII = "StackCap II";
                public const string StackCapIII = "StackCap III";
                public const string StackCapIV = "StackCap IV";
                public const string SteelProduction = "Steel Production";
                public const string StemWashing = "Stem Washing";
                public const string SupportsI = "Supports I";
                public const string SupportsII = "Supports II";
                public const string SupportsIII = "Supports III";
                public const string SupportsIV = "Supports IV";
                public const string CoreReassignment = "Core Reassignment";
                public const string TheMOLEFlatten = "The M.O.L.E. (Flatten)";
                public const string MOLEF77Flatten = "M.O.L.E. F-77 (Flatten)";
                public const string MOLEF99Flatten = "M.O.L.E. F-99 (Flatten)";
                public const string MOLEF1212Flatten = "M.O.L.E. F-1212 (Flatten)";
                public const string MOLESpeedII = "M.O.L.E. Speed II";
                public const string MOLESpeedIII = "M.O.L.E. Speed III";
                public const string MOLESpeedIV = "M.O.L.E. Speed IV";
                public const string MOLET59Tunneling = "M.O.L.E. T-59 (Tunneling)";
                public const string MOLET33Tunneling = "M.O.L.E. T-33 (Tunneling)";
                public const string MOLET99Tunneling = "M.O.L.E. T-99 (Tunneling)";
                public const string MiningDrill = "Mining Drill";
                public const string Thresher = "Thresher";
                public const string ThreshingSpeedII = "Threshing Speed II";
                public const string ThreshingSpeedIII = "Threshing Speed III";
                public const string ToolbeltII = "Toolbelt II";
                public const string ToolbeltIII = "Toolbelt III";
                public const string ToolbeltIV = "Toolbelt IV";
                public const string VoltageAutoStep = "Voltage AutoStep";
                public const string SuitSpeedII = "SuitSpeed II";
                public const string SuitSpeedIII = "SuitSpeed III";
                public const string SuitSpeedIV = "SuitSpeed IV";
                public const string WallLights = "Wall Lights";
                public const string WallsI = "Walls I";
                public const string WallsII = "Walls II";
                public const string WallsIII = "Walls III";
            }
        }
    }
}