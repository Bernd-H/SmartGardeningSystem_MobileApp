using System;
using System.Collections.Generic;
using System.Net;
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
                Id = module.Id,
                Name = module.Name,
                Type = new ModuleTypes(module.ModuleTyp),
                CorrespondingValves = module.AssociatedModules,
                InformationTimestamp = DateTime.Now
            };
        }

        public static ConnectRequestDto ToDto(this ConnectRequest connectRequest) {
            return new ConnectRequestDto() {
                BasestationId = connectRequest.BasestationId.ToByteArray(),
                ForceRelay = connectRequest.ForceRelay
            };
        }

        public static ConnectRequestResult FromDto(this ConnectRequestResultDto relayRequestResultDto) {
            return new ConnectRequestResult() {
                BasestationNotReachable = relayRequestResultDto.BasestationNotReachable,
                BasestaionEndPoint = IpUtils.IPEndPoint_TryParse(relayRequestResultDto.BasestaionEndPoint)
            };
        }
    }
}
