using Orchestrator.Driver.Config;
using Orchestrator.Driver.Config.ColorSensor;
using ServiceLayer.Services;
using ServiceLayer.Types;

namespace Orchestrator.Driver;

public class Worker(IRobotService robotService, IConfiguration configuration) : BackgroundService
{
    private readonly RobotVariablesOptions _options = configuration.GetSection(RobotVariablesOptions.RobotVariables).Get<RobotVariablesOptions>()!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await robotService.MoveBelt(new MoveBeltContinuousMessage() { Running = true });
            while (!await ObjectAtBarrierAsync()) { }
            await robotService.MoveBelt(new MoveBeltContinuousMessage() { Running = false });
            await robotService.MoveBelt(new MoveBeltMessage() {Distance = _options.BarrierColorSensorDistance}); //by 20mm
            
            await HandleObject();
            // if (stopProcess == true)
            // {
            //     break;
            // }
        }
    }

    private async Task<bool> ObjectAtBarrierAsync()
    {
        int depthSensorRange = (await robotService.ReadDepthSensorMessage()).Range;
        if (depthSensorRange <= _options.DepthSensor.ObjectUpperBound)
        {
            return true;
        }
        return false;
    }

    private async Task OpenBarrierAsync(bool open = true)
    {
        await robotService
            .SetServoProgressions(
                new SetServoProgressionsMessage()
                {
                    Progressions = new List<ServoProgressionDto>()
                    {
                        new ServoProgressionDto()
                            {ServoId = 0, Progression = open ? (byte)0 : (byte)255}
                    }
                });
        await Task.Delay(_options.InterOperationDelayMs);
    }

    int TranslationTimeMs(int distance)
    {
        return (int) ((distance / (float)_options.Motor.Speed) * 1000);
    }
    
    private async Task MoveBeltAsync(int distance)
    {
        await robotService.MoveBelt(new MoveBeltMessage() { Distance = distance });
        await Task.Delay(TranslationTimeMs(_options.BarrierPassingDistance)); 
        await Task.Delay(_options.InterOperationDelayMs);
    }

    private bool inDeviation(ReadColorSensorMessage message, IdentifiableObject identifiableObject)
    {
        return Math.Abs(message.Red - identifiableObject.Red) <= identifiableObject.Deviation &&
               Math.Abs(message.Green - identifiableObject.Green) <= identifiableObject.Deviation &&
               Math.Abs(message.Blue - identifiableObject.Blue) <= identifiableObject.Deviation;
    }
    
    private async Task<string> ClassifyObjectWithColorSensorAsync()
    {
        ReadColorSensorMessage colorSensorMessage = await robotService.ReadColorSensorData();
        foreach (var identifiableObject in _options.ColorSensor.Objects)
        {
            if (inDeviation(colorSensorMessage, identifiableObject))
                return identifiableObject.Name;
        }

        return "none";
    }

    private async Task HandleObject()
    {
        await OpenBarrierAsync();
        await MoveBeltAsync(_options.BarrierPassingDistance);
        await OpenBarrierAsync(false);
        await MoveBeltAsync(_options.BarrierColorSensorDistance - _options.BarrierPassingDistance);
        
        string objectType = await ClassifyObjectWithColorSensorAsync();//get the color
        if (objectType == "white_disc")
        {
            // int minimalWeight = getMinimalWeight(weight1, weight2, weight3, 10);
            // switch (minimalWeight)
            // {
            //     case 0: RobotService.WriteToDisplay();//Write "Containers full. Empty!"
            //         stopProcess = true;
            //         break;
            //     case 1: RobotService.MoveBelt();//by 45mm
            //         RobotService.SetServoPos();//move servo1 forward
            //         RobotService.SetServoPos();//move servo1 backward
            //         weight1 += 10;
            //         if (weight1 == 40)
            //         {
            //             RobotService.SetLEDOn();//set led1 on
            //         }
            //         break;
            //     case 2: RobotService.MoveBelt();//by 120mm
            //         RobotService.SetServoPos();//move servo2 forward
            //         RobotService.SetServoPos();//move servo2 backward
            //         weight2 += 10;
            //         if (weight2 == 40)
            //         {
            //             RobotService.SetLEDOn();//set led2 on
            //         }
            //         break;
            //     case 3: RobotService.MoveBelt();//by 180mm
            //         RobotService.SetServoPos();//move servo2 forward
            //         RobotService.SetServoPos();//move servo2 backward
            //         weight3 += 10;
            //         if (weight3 == 40)
            //         {
            //             RobotService.SetLEDOn();//set led3 on
            //         }
            //         break;
            // }
        } 
        // else if (colorCurrObj == "Black")
        // {
        //     int minimalWeight = getMinimalWeight(weight1, weight2, weight3, 10);
        //     switch (minimalWeight)
        //     {
        //         case 0: RobotService.WriteToDisplay();//Write "Containers full. Empty!"
        //             stopProcess = true;
        //             break;
        //         case 1: RobotService.MoveBelt();//by 30mm
        //             RobotService.SetServoPos();//move servo1 forward
        //             RobotService.SetServoPos();//move servo1 backward
        //             weight1 += 20;
        //             if (weight1 == 40)
        //             {
        //                 RobotService.SetLEDOn();//set led1 on
        //             }
        //             break;
        //         case 2: RobotService.MoveBelt();//by 120mm
        //             RobotService.SetServoPos();//move servo2 forward
        //             RobotService.SetServoPos();//move servo2 backward
        //             weight2 += 20;
        //             if (weight2 == 40)
        //             {
        //                 RobotService.SetLEDOn();//set led2 on
        //             }
        //             break;
        //         case 3: RobotService.MoveBelt();//by 180mm
        //             RobotService.SetServoPos();//move servo2 forward
        //             RobotService.SetServoPos();//move servo2 backward
        //             weight3 += 20;
        //             if (weight3 == 40)
        //             {
        //                 RobotService.SetLEDOn();//set led3 on
        //             }
        //             break;
        //     }
        // }
        // else
        // {
        //     RobotService.MoveBelt();//by 170mm
        // }
        //
        // //Check for full containers
        // if (weight1 == 40 && weight2 == 40 && weight3 == 40)
        // {
        //     RobotService.WriteToDisplay();//Write "Containers full. Empty!"
        //     stopProcess = true;
        // }
    }
    //
    // int getMinimalWeight(int weight1, int weight2, int weight3, int discWeight)
    // {
    //     if (weight1 <= weight2 && weight1 <= weight3)
    //     {
    //         if (discWeight + weight1 > 40)
    //         {
    //             return 0;
    //         }
    //         return 1;
    //     } 
    //     else if (weight2 <= weight1 && weight2 <= weight3)
    //     {
    //         if (discWeight + weight2 > 40)
    //         {
    //             return 0;
    //         }
    //         return 2;
    //     } 
    //     if (discWeight + weight3 > 40)
    //     {
    //         return 0;
    //     }
    //     return 3;
    // }
}