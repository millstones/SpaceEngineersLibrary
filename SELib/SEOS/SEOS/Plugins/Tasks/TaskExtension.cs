using Sandbox.ModAPI.Ingame;
using System.Collections;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    static class TaskExtension
    {
        #region Common
        public static void Run(this TaskBase enumerator, CancellationToken token = null) =>
            TaskPlugin.Run(enumerator, token);
        public static void Run(this IEnumerator enumerator, CancellationToken token = null)=>
            TaskPlugin.Run(new Task().While(enumerator.MoveNext), token);
        public static TaskBase ToTask(this IEnumerator enumerator) =>
            new Task().While(enumerator.MoveNext);
        #endregion
        #region IMyEntity
        public static Task TriggerIsInventoryFull(this IMyEntity entity) =>
            Task.Trigger(() => entity.GetInventory().IsFull);
        public static Task<double> TriggerInventoryPercent(this IMyEntity entity) =>
            Task.ValueChanged(entity, (_) => entity.EmployedPercent());
        #endregion
        #region IMyCubeBlock
        public static Task TriggerIsBeingHacked(this IMyCubeBlock block) =>
            Task.Trigger(() => block.IsBeingHacked);
        public static Task TriggerIsFunctional(this IMyCubeBlock block) =>
            Task.Trigger(() => block.IsFunctional);
        #endregion
        #region IMyFunctionalBlock
        public static Task<bool> TriggerOnOff(this IMyFunctionalBlock block) =>
            Task.ValueChanged(block, (b) => b.Enabled);
        public static Task TriggerOn(this IMyFunctionalBlock block) =>
            Task.Trigger(() => block.Enabled);
        public static Task TriggerOff(this IMyFunctionalBlock block) =>
            Task.Trigger(() => !block.Enabled);
        #endregion
        #region IMyDoor
        public static Task<DoorStatus> TriggerDoorStatus(this IMyDoor door) =>
            Task.ValueChanged(door, (d) => d.Status);
        public static Task TriggerDoorOpen(this IMyDoor door) =>
            Task.Trigger(() => door.Status == DoorStatus.Open);
        public static Task TriggerDoorClosed(this IMyDoor door) =>
            Task.Trigger(() => door.Status == DoorStatus.Closed);
        #endregion
        #region IMyPistonBase
        public static Task<PistonStatus> TriggerStatus(this IMyPistonBase piston) =>
            Task.ValueChanged(piston, (p) => p.Status);
        public static Task TriggerStatusExtended(this IMyPistonBase piston) =>
            Task.Trigger(() => piston.Status == PistonStatus.Extended);
        public static Task TriggerStatusRetracted(this IMyPistonBase piston) =>
            Task.Trigger(() => piston.Status == PistonStatus.Retracted);
        public static Task TriggerStatusStopped(this IMyPistonBase piston) =>
            Task.Trigger(() => piston.Status == PistonStatus.Stopped);
        public static Task<double> TriggerPositionPercent(this IMyPistonBase piston)=>
            Task.ValueChanged(piston, (p) => p.PositionPercent());
        #endregion
        #region IMyShipToolBase
        public static Task TriggerIsActivatedFromTerminalOrMouseClick(this IMyShipToolBase tool) =>
            Task.Trigger(() => tool.IsActivated);
        #endregion
        #region IMyGyro
        public static Task TriggerOverride(this IMyGyro gyro)=>
            Task.Trigger(() => gyro.GyroOverride);
        #endregion
        #region IMyShipController
        public static Task TriggerDampenersOverride(this IMyShipController controller)=>
            Task.Trigger(() => controller.DampenersOverride);
        public static Task TriggerHandBrake(this IMyShipController controller) =>
             Task.Trigger(() => controller.HandBrake);
        #endregion
        #region IMyShipConnector
        public static Task<MyShipConnectorStatus> TriggerConnectorStatus(this IMyShipConnector connector) =>
            Task.ValueChanged(connector, (c) => c.Status);
        public static Task TriggerConnectorStatusConnectable(this IMyShipConnector connector) =>
            Task.Trigger(() => connector.Status == MyShipConnectorStatus.Connectable);
        public static Task TriggerConnectorStatusConnected(this IMyShipConnector connector) =>
            Task.Trigger(() => connector.Status == MyShipConnectorStatus.Connected);
        public static Task TriggerConnectorStatusUnconnected(this IMyShipConnector connector) =>
            Task.Trigger(() => connector.Status == MyShipConnectorStatus.Unconnected);
        #endregion
        #region IMyBatteryBlock
        public static Task<ChargeMode> TriggerChargeMode(this IMyBatteryBlock battary)=>
            Task.ValueChanged(battary, (b) => b.ChargeMode);
        public static Task TriggerHasCapacityRemaining(this IMyBatteryBlock battary) =>
            Task.Trigger(() => battary.HasCapacityRemaining);
        public static Task TriggerIsCharging(this IMyBatteryBlock battary) =>
            Task.Trigger(() => battary.IsCharging);
        #endregion
        #region IMySensorBlock
        public static Task TriggerIsBeingDetected(this IMySensorBlock sensor) =>
            Task.Trigger(() => sensor.IsActive);
        public static Task<MyDetectedEntityInfo> TriggerOnDetectEntity(this IMySensorBlock sensor)=>
            Task.ValueChanged(sensor, (s) => s.LastDetectedEntity);
        #endregion
        #region IMyCameraBlock
        public static Task TriggerIsUserUsed(this IMyCameraBlock camera) =>
            Task.Trigger(() => camera.IsActive);
        public static Task TriggerIsEnableRaycast(this IMyCameraBlock camera) =>
            Task.Trigger(() => camera.EnableRaycast);
        #endregion
    }
}
