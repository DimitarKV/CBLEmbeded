﻿using Orchestrator.Driver.Config.ColorSensor;
using Orchestrator.Driver.Config.DepthSensor;
using Orchestrator.Driver.Config.Motor;
using Orchestrator.Driver.Config.Pushers;
using ServiceLayer.Services.Implementation;

namespace Orchestrator.Driver.Config;

public class RobotVariablesOptions
{
    public const string RobotVariables = "RobotVariables";
    public MotorConfig Motor { get; set; }
    public DepthSensorConfig DepthSensor { get; set; }
    public ColorSensorConfig ColorSensor { get; set; }
    public List<ServoConfig> Pushers { get; set; }

    public int BarrierPassingDistance { get; set; }
    public int BarrierColorSensorDistance { get; set; }
    public int InterOperationDelayMs { get; set; }
    public int BarrierAdjustmentDistance { get; set; }
    public int ColorSensorPusherDistance { get; set; }
    public int InterPusherDistance { get; set; }
    public int PusherMoveDelayMs { get; set; }

    public int ColorSensorToTrashDistance { get; set; }

    // Hristo
    public Dictionary<string, string> Audio { get; set; }
}