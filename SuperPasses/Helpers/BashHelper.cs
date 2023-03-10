using System.Collections.Generic;
using DynamicData;

namespace SuperPasses.Helpers;

public class BashHelper
{
    // 返回的格式： 00:0C:29:A1:63:66
    public static string GetMacAddress(string deviceName)
    {
        var command = @"ifconfig | grep DEVICENAME | awk '{ print $5}'";
        command = command.Replace("DEVICENAME", deviceName);
        return command;
    }

    public static string GetIpAddress(string interfaceName)
    {
        var command = "ifstatus INTERFACENAME |  jsonfilter -e '@[\"ipv4-address\"][0].address'";
        command = command.Replace("INTERFACENAME", interfaceName);
        return command;
    }

    public static IEnumerable<string> GetOpenWrtBashFileStringList(string cmdLine)
    {
        var s = new List<string>
        {
            @"#!/bin/sh",
            "",
            "logger -t web-login \"SuperPasses Execute Bash File\"",
            @$"LOGIN_STATUS={cmdLine}",
            @"logger -t web-login ${LOGIN_STATUS}",
            @"echo ${LOGIN_STATUS}"
        };
        return s;
    }
}