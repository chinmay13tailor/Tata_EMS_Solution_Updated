#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.DataLogger;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.NativeUI;
using FTOptix.UI;
using FTOptix.CoreBase;
using FTOptix.Store;
using FTOptix.ODBCStore;
using FTOptix.Report;
using FTOptix.RAEtherNetIP;
using FTOptix.Retentivity;
using FTOptix.CommunicationDriver;
using FTOptix.Core;
using Store = FTOptix.Store;
using System.Text.RegularExpressions;
using FTOptix.SQLiteStore;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Reflection.Emit;
using FTOptix.MicroController;
using FTOptix.AuditSigning;
using FTOptix.Alarm;
#endregion

public class HomePageTagsLogic : BaseNetLogic
{
    private IUAVariable todayConsumptionVariable;
    private IUAVariable todayConsumptionCostVariable;
    private IUAVariable averageFrequencyVariable;
    private IUAVariable avgPowerFactorVariable;
    private IUAVariable maxDemandVariable;
    private IUAVariable monthConsumptionVariable;
    private IUAVariable monthConsumptionCostVariable;
    private IUAVariable buttonVariable;
    private PeriodicTask periodicTask;

    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        var owner = (HomePageTags)LogicObject.Owner;

        todayConsumptionVariable = owner.TodayConsumptionVariable;
        todayConsumptionCostVariable = owner.TodayConsumptionCostVariable;
        averageFrequencyVariable = owner.AverageFrequencyVariable;
        avgPowerFactorVariable = owner.AvgPowerFactorVariable;
        maxDemandVariable = owner.MaxDemandVariable;
        monthConsumptionVariable = owner.MonthConsumptionVariable;
        monthConsumptionCostVariable = owner.MonthConsumptionCostVariable;
        buttonVariable = owner.ButtonVariable;

        periodicTask = new PeriodicTask(HomePageCalculationTask, 10000, LogicObject);
        periodicTask.Start();
    }

    public override void Stop()
    {
        periodicTask.Dispose();
        periodicTask = null;
    }

    public void HomePageCalculationTask()
    {
        float todayconsumption = todayConsumptionVariable.Value;
        float todayconsumptioncost = todayConsumptionCostVariable.Value;
        float averagefrequency = averageFrequencyVariable.Value;
        float avgpowerfactor = avgPowerFactorVariable.Value;
        bool button = buttonVariable.Value;
        float IN1Consumption = 0; // Declare outside the if block
        float IN2Consumption = 0; // Declare outside the if block

        var project = FTOptix.HMIProject.Project.Current;
        var myStore1 = project.GetObject("DataStores").Get<Store.Store>("ODBCDatabase");
        var myStore2 = project.GetObject("DataStores").Get<Store.Store>("ODBCDatabase");
        var myStore3 = project.GetObject("DataStores").Get<Store.Store>("ODBCDatabase");
        var myStore4 = project.GetObject("DataStores").Get<Store.Store>("ODBCDatabase");

        object[,] resultSet1;
        string[] header1;

        object[,] resultSet2;
        string[] header2;

        object[,] resultSet3;
        string[] header3;

        object[,] resultSet4;
        string[] header4;

        if (button)
        {
            DateTime currentTime = DateTime.Now;
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

            // Calculate start and end times for the current day
            DateTime startTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 0, 0, 0);
            DateTime endTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 7, 59, 59).AddDays(1);

            string new123 = startTime.ToString("yyyy-MM-dd");

            string query1 = $"SELECT SUM(Consumption) FROM DailyConsumptionAgg WHERE LocalDate = '" + new123 + " 00:00:00' AND Jace = '33KV' AND Meter = 'INCOMER1'";
            string query2 = $"SELECT SUM(Consumption) FROM DailyConsumptionAgg WHERE LocalDate = '" + new123 + " 00:00:00' AND Jace = '33KV' AND Meter = 'INCOMER2'";
            string query3 = $"SELECT Frequency FROM DailyConsumptionAgg WHERE LocalDate = '" + new123 + " 00:00:00' AND Jace = '33KV' AND Meter = 'INCOMER1'";
            string query4 = $"SELECT Avg_PF FROM DailyConsumptionAgg WHERE LocalDate = '" + new123 + " 00:00:00' AND Jace = '33KV' AND Meter = 'INCOMER1'";

            myStore1.Query(query1, out header1, out resultSet1);
            myStore2.Query(query2, out header2, out resultSet2);
            myStore3.Query(query3, out header3, out resultSet3);
            myStore4.Query(query4, out header4, out resultSet4);

            if (resultSet1 != null && resultSet1.GetLength(0) > 0 && header1 != null && header1.Length > 0)
            {
                float.TryParse(resultSet1[0, 0]?.ToString(), out IN1Consumption);
            }

            if (resultSet2 != null && resultSet2.GetLength(0) > 0 && header2 != null && header2.Length > 0)
            {
                float.TryParse(resultSet2[0, 0]?.ToString(), out IN2Consumption);
            }
            // Process resultSet3 (Frequency)
            if (resultSet3 != null && resultSet3.GetLength(0) > 0 && header3 != null && header3.Length > 0)
            {
                float.TryParse(resultSet3[0, 0]?.ToString(), out averagefrequency);
            }

            // Process resultSet4 (Avg_PF)
            if (resultSet4 != null && resultSet4.GetLength(0) > 0 && header4 != null && header4.Length > 0)
            {
                float.TryParse(resultSet4[0, 0]?.ToString(), out avgpowerfactor);
            }

            // Calculate today consumption based on IN1Consumption and IN2Consumption
            todayconsumption = IN1Consumption + IN2Consumption;

            // Calculate today consumption cost
            todayconsumptioncost = todayconsumption * 752;
            todayconsumptioncost = todayconsumptioncost / 100 + 1608.4f;
        }

        // Update variable values
        todayConsumptionVariable.Value = todayconsumption;
        todayConsumptionCostVariable.Value = todayconsumptioncost;
        averageFrequencyVariable.Value = averagefrequency;
        avgPowerFactorVariable.Value = avgpowerfactor;
    }

}


