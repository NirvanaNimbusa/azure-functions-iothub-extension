﻿using Microsoft.Azure.Devices;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.IoTHub.Config
{
    public class IoTSetDeviceTwinExtension : IExtensionConfigProvider
    {
        private Dictionary<string, RegistryManager> _manager;
        private string connectionString;
        private RegistryManager registryManager;

        public void Initialize(ExtensionConfigContext context)
        {
            _manager = new Dictionary<string, RegistryManager>();

            // This allows a user to bind to IAsyncCollector<string>, and the sdk
            // will convert that to IAsyncCollector<IoTCloudToDeviceItem>
            context.AddConverter<string, IoTSetDeviceTwinItem>(ConvertToItem);

            // This is useful on input. 
            context.AddConverter<IoTSetDeviceTwinItem, string>(ConvertToString);

            // Create 2 binding rules for the Sample attribute.
            var rule = context.AddBindingRule<IoTSetDeviceTwinAttribute>();

            //rule.BindToInput<IoTSetDeviceTwinItem>(BuildItemFromAttr);
            rule.BindToCollector<IoTSetDeviceTwinItem>(BuildCollector);
        }

        private string ConvertToString(IoTSetDeviceTwinItem item)
        {
            return JsonConvert.SerializeObject(item);
        }

        private IoTSetDeviceTwinItem ConvertToItem(string str)
        {
            //return JsonConvert.DeserializeObject<IoTSetDeviceTwinItem>(str);
            var item = JsonConvert.DeserializeObject<Dictionary<string, object>>(str);

            return new IoTSetDeviceTwinItem
            {
                DeviceId = (string)item["DeviceId"],
                Patch = JsonConvert.SerializeObject(item["Patch"])
            };
        }

        private IAsyncCollector<IoTSetDeviceTwinItem> BuildCollector(IoTSetDeviceTwinAttribute attribute)
        {
            connectionString = attribute.Connection;
            if (_manager.ContainsKey(connectionString))
            {
                registryManager = _manager[connectionString];
            }
            else
            {
                registryManager = RegistryManager.CreateFromConnectionString(connectionString);
                _manager.Add(connectionString, registryManager);
            }

            return new IoTSetDeviceTwinAsyncCollector(registryManager, attribute);
        }

    }
}