﻿using System;
using System.Collections.Generic;
using System.Linq;
using Calamari.Extensibility.Docker.Commands;
using Newtonsoft.Json;

namespace Calamari.Extensibility.Docker
{
    public static class ScriptBuilder
    {
        public static string Run(IVariableDictionary variables)
        {
            var image = variables.Get(SpecialVariables.Tentacle.CurrentDeployment.PackageFilePath);
            var runCommand = new DockerRunCommand(image);

            //Restart Policy
            runCommand.RestartPolicy = variables.Get(SpecialVariables.Action.Docker.RestartPolicy, string.Empty);            
            runCommand.RestartPolicyMax = variables.GetInt32(SpecialVariables.Action.Docker.RestartPolicyMax);
            
            // Labels
            runCommand.Labels.Add(SpecialVariables.Release.Number, variables.Get(SpecialVariables.Release.Number));
            runCommand.Labels.Add(SpecialVariables.Project.Id, variables.Get(SpecialVariables.Project.Id));
            runCommand.Labels.Add(SpecialVariables.Environment.Id, variables.Get(SpecialVariables.Environment.Id));
            runCommand.Labels.Add(SpecialVariables.Deployment.Id, variables.Get(SpecialVariables.Deployment.Id));
            runCommand.Labels.Add(SpecialVariables.Deployment.Tenant.Id, variables.Get(SpecialVariables.Deployment.Tenant.Id));
            runCommand.Labels.Add(SpecialVariables.Action.Id, variables.Get(SpecialVariables.Deployment.Tenant.Id));

            //Port Bindings
            runCommand.PortAutoMap = variables.GetFlag(SpecialVariables.Action.Docker.PortAutoMap, false);
            runCommand.PortMappings = JsonObjectToDictionary(variables.Get(SpecialVariables.Action.Docker.PortMapping));

            // Networking
            runCommand.NetworkType = variables.Get(SpecialVariables.Action.Docker.NetworkType);
            runCommand.NetworkContainer = variables.Get(SpecialVariables.Action.Docker.NetworkContainer);
            runCommand.NetworkName = variables.Get(SpecialVariables.Action.Docker.NetworkName);
            runCommand.NetworkAliases = CommaDelimtedToList(variables.Get(SpecialVariables.Action.Docker.NetworkAlias));

            // Hosts file entries
            runCommand.AddedHosts = JsonObjectToDictionary(variables.Get(SpecialVariables.Action.Docker.AddedHost));

            // Volumes
            runCommand.VolumeDriver = variables.Get(SpecialVariables.Action.Docker.VolumeDriver);
            var rawVolumeBindingJson = variables.Get(SpecialVariables.Action.Docker.VolumeBindings);
            if (!string.IsNullOrWhiteSpace(rawVolumeBindingJson))
            {

               runCommand.VolumeBindings = JsonConvert.DeserializeObject<Dictionary<string, DockerRunCommand.VolumeBinding>>(rawVolumeBindingJson);
            }
            runCommand.VolumesFrom = CommaDelimtedToList(variables.Get(SpecialVariables.Action.Docker.VolumesFrom));


            // Environment Variables
            runCommand.EnvironmentVariables = JsonObjectToDictionary(variables.Get(SpecialVariables.Action.Docker.EnvVariable));
            
            runCommand.DontRun = variables.GetFlag(SpecialVariables.Action.Docker.DontRun, false);
            runCommand.OtherArgs = variables.Get(SpecialVariables.Action.Docker.Args);
            runCommand.EntryCommand = variables.Get(SpecialVariables.Action.Docker.Command);

            return runCommand.ToString();
        }

        static List<string> CommaDelimtedToList(string commaDelimited)
        {
            return (commaDelimited ?? "")
                .Split(',')
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim(' '))
                .ToList();
        }

        static Dictionary<string, string> JsonObjectToDictionary(string rawJson)
        {
            return string.IsNullOrWhiteSpace(rawJson) 
                ? new Dictionary<string, string>() 
                : JsonConvert.DeserializeObject<Dictionary<string, string>>(rawJson);
        }
    }
}