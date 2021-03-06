﻿<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="MpsWfmHub" generation="1" functional="0" release="0" Id="37f61633-a5a6-4ac3-8bd7-3d03faccf65a" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="MpsWfmHubGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="WfmHubWorker:Endpoint1" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/MpsWfmHub/MpsWfmHubGroup/LB:WfmHubWorker:Endpoint1" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="ProcessMpsListenerMsg:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/MpsWfmHub/MpsWfmHubGroup/MapProcessMpsListenerMsg:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="ProcessMpsListenerMsgInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/MpsWfmHub/MpsWfmHubGroup/MapProcessMpsListenerMsgInstances" />
          </maps>
        </aCS>
        <aCS name="WfmHubWorker:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/MpsWfmHub/MpsWfmHubGroup/MapWfmHubWorker:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="WfmHubWorkerInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/MpsWfmHub/MpsWfmHubGroup/MapWfmHubWorkerInstances" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:WfmHubWorker:Endpoint1">
          <toPorts>
            <inPortMoniker name="/MpsWfmHub/MpsWfmHubGroup/WfmHubWorker/Endpoint1" />
          </toPorts>
        </lBChannel>
      </channels>
      <maps>
        <map name="MapProcessMpsListenerMsg:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/MpsWfmHub/MpsWfmHubGroup/ProcessMpsListenerMsg/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapProcessMpsListenerMsgInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/MpsWfmHub/MpsWfmHubGroup/ProcessMpsListenerMsgInstances" />
          </setting>
        </map>
        <map name="MapWfmHubWorker:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/MpsWfmHub/MpsWfmHubGroup/WfmHubWorker/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapWfmHubWorkerInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/MpsWfmHub/MpsWfmHubGroup/WfmHubWorkerInstances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="ProcessMpsListenerMsg" generation="1" functional="0" release="0" software="C:\Wfm_Git\WfmHubServer\MpsWfmHub\csx\Debug\roles\ProcessMpsListenerMsg" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaWorkerHost.exe " memIndex="-1" hostingEnvironment="consoleroleadmin" hostingEnvironmentVersion="2">
            <settings>
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;ProcessMpsListenerMsg&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;ProcessMpsListenerMsg&quot; /&gt;&lt;r name=&quot;WfmHubWorker&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/MpsWfmHub/MpsWfmHubGroup/ProcessMpsListenerMsgInstances" />
            <sCSPolicyUpdateDomainMoniker name="/MpsWfmHub/MpsWfmHubGroup/ProcessMpsListenerMsgUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/MpsWfmHub/MpsWfmHubGroup/ProcessMpsListenerMsgFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
        <groupHascomponents>
          <role name="WfmHubWorker" generation="1" functional="0" release="0" software="C:\Wfm_Git\WfmHubServer\MpsWfmHub\csx\Debug\roles\WfmHubWorker" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaWorkerHost.exe " memIndex="-1" hostingEnvironment="consoleroleadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Endpoint1" protocol="http" portRanges="80" />
            </componentports>
            <settings>
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;WfmHubWorker&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;ProcessMpsListenerMsg&quot; /&gt;&lt;r name=&quot;WfmHubWorker&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/MpsWfmHub/MpsWfmHubGroup/WfmHubWorkerInstances" />
            <sCSPolicyUpdateDomainMoniker name="/MpsWfmHub/MpsWfmHubGroup/WfmHubWorkerUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/MpsWfmHub/MpsWfmHubGroup/WfmHubWorkerFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyUpdateDomain name="WfmHubWorkerUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyUpdateDomain name="ProcessMpsListenerMsgUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyFaultDomain name="ProcessMpsListenerMsgFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyFaultDomain name="WfmHubWorkerFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="ProcessMpsListenerMsgInstances" defaultPolicy="[1,1,1]" />
        <sCSPolicyID name="WfmHubWorkerInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="72d4417c-13fc-4925-b5fd-694e37ff76fd" ref="Microsoft.RedDog.Contract\ServiceContract\MpsWfmHubContract@ServiceDefinition">
      <interfacereferences>
        <interfaceReference Id="0896063f-d5fc-4f74-918a-386280c8856d" ref="Microsoft.RedDog.Contract\Interface\WfmHubWorker:Endpoint1@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/MpsWfmHub/MpsWfmHubGroup/WfmHubWorker:Endpoint1" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>