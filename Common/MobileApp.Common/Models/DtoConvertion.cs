﻿using System;
using System.Collections.Generic;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Entities;
using MobileApp.Common.Models.Enums;

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
    }
}
