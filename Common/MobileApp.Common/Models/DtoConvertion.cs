using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Entities;
using MobileApp.Common.Models.Enums;
using MobileApp.Common.Utilities;

namespace MobileApp.Common.Models {
    public static class DtoConvertion {

        public static IEnumerable<ModuleInfoDto> ToDtos(this IEnumerable<ModuleInfo> moduleInfos) {
            var objects = new List<ModuleInfoDto>();

            if (moduleInfos != null) {
                foreach (var module in moduleInfos) {
                    objects.Add(ToDto(module));
                }
            }

            return objects;
        }

        public static ModuleInfoDto ToDto(this ModuleInfo module) {
            return new ModuleInfoDto() {
                ModuleId = Utils.ConvertByteToHex(module.ModuleId),
                Name = module.Name, 
                ModuleTypeName = (module.ModuleType == ModuleType.Sensor) ? ModuleTypeNames.SENSOR : ModuleTypeNames.VALVE,
                AssociatedModules = module.AssociatedModules,
                InformationTimestamp = module.InformationTimestamp,
                EnabledForManualIrrigation = module.EnabledForManualIrrigation,
                LastWaterings = module.LastWaterings,
                SignalStrength = module.SignalStrength,
                SoilMoistureMeasurements = module.SoilMoistureMeasurements,
                TemperatureMeasurements = module.TemperatureMeasurements,
                BatteryLevel = module.BatteryLevel
            };
        }

        public static ModuleInfo FromDto(this ModuleInfoDto moduleInfoDto) {
            return new ModuleInfo {
                ModuleId = Utils.ConvertHexToByte(moduleInfoDto.ModuleId),
                AssociatedModules = moduleInfoDto.AssociatedModules,
                InformationTimestamp = moduleInfoDto.InformationTimestamp,
                LastWaterings = moduleInfoDto.LastWaterings,
                ModuleType = (moduleInfoDto.ModuleTypeName == ModuleTypeNames.SENSOR) ? ModuleType.Sensor : ModuleType.Valve,
                Name = moduleInfoDto.Name,
                EnabledForManualIrrigation = moduleInfoDto.EnabledForManualIrrigation,
                TemperatureMeasurements = moduleInfoDto.TemperatureMeasurements,
                SoilMoistureMeasurements = moduleInfoDto.SoilMoistureMeasurements,
                SignalStrength = moduleInfoDto.SignalStrength,
                BatteryLevel = moduleInfoDto.BatteryLevel
            };
        }

        public static ConnectRequestDto ToDto(this ConnectRequest connectRequest) {
            return new ConnectRequestDto() {
                BasestationId = connectRequest.BasestationId.ToByteArray(),
                ForceRelay = connectRequest.ForceRelay
            };
        }

        public static ApplicationSettingsDto ToDto(this ApplicationSettings applicationSettings) {
            return new ApplicationSettingsDto() {
                AesIV = applicationSettings.AesIV,
                AesKey = applicationSettings.AesKey,
                BasestationCert = (applicationSettings.BasestationCert != null) ? new X509Certificate(applicationSettings.BasestationCert) : null,
                BasestationId = applicationSettings.BasestationId,
                BaseStationIP = applicationSettings.BaseStationIP,
                Id = applicationSettings.Id,
                SessionAPIToken = applicationSettings.SessionAPIToken
            };
        }

        public static ConnectRequestResult FromDto(this ConnectRequestResultDto relayRequestResultDto) {
            return new ConnectRequestResult() {
                BasestationNotReachable = relayRequestResultDto.BasestationNotReachable,
                BasestaionEndPoint = IpUtils.IPEndPoint_TryParse(relayRequestResultDto.BasestaionEndPoint)
            };
        }

        public static SystemStatus FromDto(this SystemStatusDto systemStatusDto) {
            string wateringStatus = Enum.GetName(systemStatusDto.WateringStatus.GetType(), systemStatusDto.WateringStatus);

            return new SystemStatus {
                SystemUpTime = TimeSpan.FromMinutes(systemStatusDto.SystemUpMinutes).ToReadableString(),
                Temperature = systemStatusDto.Temperature,
                WateringStatus = wateringStatus
            };
        }

        public static ApplicationSettings FromDto(this ApplicationSettingsDto applicationSettingsDto) {
            return new ApplicationSettings() {
                AesIV = applicationSettingsDto.AesIV,
                AesKey = applicationSettingsDto.AesKey,
                BasestationCert = (applicationSettingsDto.BasestationCert != null) ? applicationSettingsDto.BasestationCert.GetRawCertData() : null,
                BasestationId = applicationSettingsDto.BasestationId,
                BaseStationIP = applicationSettingsDto.BaseStationIP,
                Id = applicationSettingsDto.Id,
                SessionAPIToken = applicationSettingsDto.SessionAPIToken
            };
        }
    }
}
