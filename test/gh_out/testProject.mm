<?xml version="1.0" standalone="yes" ?>
<!---->
<DataRoot FormatVersion="1" ContentVersion="8.0.9.0" FileName="">
    <Objects>
        <Circulate:test>
            <Attributes>
                <AttrCirculateEventCirculatePortals>
                    <Data>
                        <GlobalIDs v="[b7e7faff-a8d9-4780-adff-9db9a3d46cf4]" t="3" />
                        <WeightedType v="WeightedNone" t="3" />
                        <Weights v="[1.000000]" t="2" />
                    </Data>
                    <Type v="DataTypeVectorWeightedGlobalID" t="3" />
                </AttrCirculateEventCirculatePortals>
                <AttrCirculateEventCirculateWaitDistributionScheme>
                    <Data>
                        <FallbackDistribution>
                            <!--Constant: [value,,,], Uniform: [min,max,,], Normal: [min,max,mean,std], Triangular: [min,max,mode,], Exponential: [min,max,lambda]-->
                            <Type v="Uniform" t="3" />
                            <Values v="[30.000000,300.000000]" t="2" />
                        </FallbackDistribution>
                        <Rules />
                    </Data>
                    <Type v="DataTypeDistributionScheme" t="3" />
                </AttrCirculateEventCirculateWaitDistributionScheme>
                <AttrCirculateEventCirculateWaitOnStart>
                    <Data v="0" t="0" />
                    <Type v="DataTypeBool" t="3" />
                </AttrCirculateEventCirculateWaitOnStart>
                <AttrCirculateEventCirculateWaitStyle>
                    <Data>
                        <EnumString v="WaitSpreadOut" t="3" />
                        <EnumValue v="2" t="1" />
                    </Data>
                    <Type v="DataTypeEnum" t="3" />
                </AttrCirculateEventCirculateWaitStyle>
                <AttrCirculateEventLifetimeCountDistribution>
                    <Data>
                        <!--Constant: [value,,,], Uniform: [min,max,,], Normal: [min,max,mean,std], Triangular: [min,max,mode,], Exponential: [min,max,lambda]-->
                        <Type v="Uniform" t="3" />
                        <Values v="[0.000000,10.000000]" t="2" />
                    </Data>
                    <Type v="DataTypeDistribution" t="3" />
                </AttrCirculateEventLifetimeCountDistribution>
                <AttrCirculateEventLifetimeDurationDistribution>
                    <Data>
                        <!--Constant: [value,,,], Uniform: [min,max,,], Normal: [min,max,mean,std], Triangular: [min,max,mode,], Exponential: [min,max,lambda]-->
                        <Type v="Uniform" t="3" />
                        <Values v="[0.000000,960.000000]" t="2" />
                    </Data>
                    <Type v="DataTypeDistribution" t="3" />
                </AttrCirculateEventLifetimeDurationDistribution>
                <AttrCirculateEventLifetimeEndTime>
                    <Data>
                        <GlobalID v="00000000-0000-0000-0000-000000000000" t="3" />
                        <TimeInSeconds v="00:05:00" t="3" />
                        <TimeType v="TimeSimulationStart" t="3" />
                    </Data>
                    <Type v="DataTypeTimeReference" t="3" />
                </AttrCirculateEventLifetimeEndTime>
                <AttrCirculateEventLifetimeType>
                    <Data>
                        <EnumString v="LifetimeUntilTime" t="3" />
                        <EnumValue v="4" t="1" />
                    </Data>
                    <Type v="DataTypeEnum" t="3" />
                </AttrCirculateEventLifetimeType>
                <AttrCirculateEventLifetimeWaitAfterCount>
                    <Data v="0" t="0" />
                    <Type v="DataTypeBool" t="3" />
                </AttrCirculateEventLifetimeWaitAfterCount>
                <AttrEnabled>
                    <Data v="1" t="0" />
                    <Type v="DataTypeBool" t="3" />
                </AttrEnabled>
                <AttrEventBirthAction>
                    <Data>
                        <ActionType v="ActionNone" t="3" />
                    </Data>
                    <Type v="DataTypeAction" t="3" />
                </AttrEventBirthAction>
                <AttrEventBirthProfile>
                    <Data v="a83e5662-81c5-4e8d-9823-a6e9fb979967" t="3" />
                    <Type v="DataTypeGlobalID" t="3" />
                </AttrEventBirthProfile>
                <AttrEventColorScheme>
                    <Data>
                        <FallbackColor v="[1.000000,0.500000,0.000000,1.000000]" t="2" />
                        <Rules />
                        <UseFallbackColor v="1" t="0" />
                    </Data>
                    <Type v="DataTypeColorScheme" t="3" />
                </AttrEventColorScheme>
                <AttrEventColorType>
                    <Data>
                        <EnumString v="EventColorObjectEqual" t="3" />
                        <EnumValue v="0" t="1" />
                    </Data>
                    <Type v="DataTypeEnum" t="3" />
                </AttrEventColorType>
                <AttrEventDemandCurveData>
                    <Data v="[]" t="2" />
                    <Type v="DataTypeVectorDouble" t="3" />
                </AttrEventDemandCurveData>
                <AttrEventDemandType>
                    <Data>
                        <EnumString v="DemandDistribution" t="3" />
                        <EnumValue v="2" t="1" />
                    </Data>
                    <Type v="DataTypeEnum" t="3" />
                </AttrEventDemandType>
                <AttrEventDestinationType>
                    <Data>
                        <EnumString v="DestinationAssigned" t="3" />
                        <EnumValue v="1" t="1" />
                    </Data>
                    <Type v="DataTypeEnum" t="3" />
                </AttrEventDestinationType>
                <AttrEventDurationDistribution>
                    <Data>
                        <!--Constant: [value,,,], Uniform: [min,max,,], Normal: [min,max,mean,std], Triangular: [min,max,mode,], Exponential: [min,max,lambda]-->
                        <Type v="Uniform" t="3" />
                        <Values v="[0.000000,1800.000000]" t="2" />
                    </Data>
                    <Type v="DataTypeDistribution" t="3" />
                </AttrEventDurationDistribution>
                <AttrEventMultiDestination>
                    <Data>
                        <GlobalIDs v="[d64ff063-75dd-4285-b262-ec1df6df9835]" t="3" />
                        <WeightedType v="WeightedNone" t="3" />
                        <Weights v="[1.000000]" t="2" />
                    </Data>
                    <Type v="DataTypeVectorWeightedGlobalID" t="3" />
                </AttrEventMultiDestination>
                <AttrEventMultiOrigin>
                    <Data>
                        <GlobalIDs v="[29412593-6916-4d93-a67b-03f2385ccd15]" t="3" />
                        <WeightedType v="WeightedNone" t="3" />
                        <Weights v="[1.000000]" t="2" />
                    </Data>
                    <Type v="DataTypeVectorWeightedGlobalID" t="3" />
                </AttrEventMultiOrigin>
                <AttrEventPopulation>
                    <Data v="10" t="1" />
                    <Type v="DataTypeInt" t="3" />
                </AttrEventPopulation>
                <AttrEventStartTime>
                    <Data>
                        <GlobalID v="00000000-0000-0000-0000-000000000000" t="3" />
                        <TimeInSeconds v="00:00:00" t="3" />
                        <TimeType v="TimeSimulationStart" t="3" />
                    </Data>
                    <Type v="DataTypeTimeReference" t="3" />
                </AttrEventStartTime>
                <AttrObjectColor>
                    <Data v="[1.000000,0.500000,0.000000,1.000000]" t="2" />
                    <Type v="DataTypeColor" t="3" />
                </AttrObjectColor>
            </Attributes>
            <GlobalID v="a76ab934-a9e8-4f6f-bf8e-57af7b3f51fc" t="3" />
            <ID v="2" t="1" />
            <Name v="Circulate:test" t="3" />
            <ObjectSubType v="EventCirculate" t="3" />
            <ObjectType v="Event" t="3" />
        </Circulate:test>      
    </Objects>   
</DataRoot>
