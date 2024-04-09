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
    private IUAVariable guageMaxVariable;
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
        guageMaxVariable = owner.GuageMaxVariable;

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
        float monthconsumption = monthConsumptionVariable.Value;
        float monthconsumptioncost = monthConsumptionCostVariable.Value;
        float todayconsumptioncost = todayConsumptionCostVariable.Value;
        float averagefrequency = averageFrequencyVariable.Value;
        float avgpowerfactor = avgPowerFactorVariable.Value;
        bool button = buttonVariable.Value;
        int guagemax = guageMaxVariable.Value;
        float IN1Consumption = 0; // Declare outside the if block
        float IN2Consumption = 0; // Declare outside the if block
        float MIN1Consumption = 0; // Declare outside the if block
        float MIN2Consumption = 0; // Declare outside the if block

        var project = FTOptix.HMIProject.Project.Current;
        var myStore1 = project.GetObject("DataStores").Get<Store.Store>("ODBCDatabase");
        var myStore2 = project.GetObject("DataStores").Get<Store.Store>("ODBCDatabase");
        var myStore3 = project.GetObject("DataStores").Get<Store.Store>("ODBCDatabase");
        var myStore4 = project.GetObject("DataStores").Get<Store.Store>("ODBCDatabase");
        var myStore5 = project.GetObject("DataStores").Get<Store.Store>("ODBCDatabase");
        var myStore6 = project.GetObject("DataStores").Get<Store.Store>("ODBCDatabase");

        object[,] resultSet1;
        string[] header1;

        object[,] resultSet2;
        string[] header2;

        object[,] resultSet3;
        string[] header3;

        object[,] resultSet4;
        string[] header4;

        object[,] resultSet5;
        string[] header5;

        object[,] resultSet6;
        string[] header6;

        if (button)
        {
            DateTime currentTime = DateTime.Now;
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

            // Calculate start and end times for the current day
            DateTime startTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 0, 0, 0);
            DateTime endTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 7, 59, 59).AddDays(1);

            string new123 = startTime.ToString("yyyy-MM-dd");
            string new12 = startTime.ToString("yyyy-MM");

            string query1 = $"SELECT SUM(Consumption) FROM DailyConsumptionAgg WHERE LocalDate = '" + new123 + " 00:00:00' AND Jace = '33KV' AND Meter = 'INCOMER1'";
            string query2 = $"SELECT SUM(Consumption) FROM DailyConsumptionAgg WHERE LocalDate = '" + new123 + " 00:00:00' AND Jace = '33KV' AND Meter = 'INCOMER2'";
            string query3 = $"SELECT Frequency FROM DailyConsumptionAgg WHERE LocalDate = '" + new123 + " 00:00:00' AND Jace = '33KV' AND Meter = 'INCOMER1'";
            string query4 = $"SELECT Avg_PF FROM DailyConsumptionAgg WHERE LocalDate = '" + new123 + " 00:00:00' AND Jace = '33KV' AND Meter = 'INCOMER1'";
            string query5 = $"SELECT SUM(Consumption) FROM DailyConsumptionAgg WHERE MonthYear = '" + new12 + "' AND Jace = '33KV' AND Meter = 'INCOMER1'";
            string query6 = $"SELECT SUM(Consumption) FROM DailyConsumptionAgg WHERE MonthYear = '" + new12 + "' AND Jace = '33KV' AND Meter = 'INCOMER2'";

            myStore1.Query(query1, out header1, out resultSet1);
            myStore2.Query(query2, out header2, out resultSet2);
            myStore3.Query(query3, out header3, out resultSet3);
            myStore4.Query(query4, out header4, out resultSet4);
            myStore5.Query(query5, out header5, out resultSet5);
            myStore6.Query(query6, out header6, out resultSet6);


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

            if (resultSet5 != null && resultSet5.GetLength(0) > 0 && header5 != null && header5.Length > 0)
            {
                float.TryParse(resultSet5[0, 0]?.ToString(), out MIN1Consumption);
            }

            if (resultSet6 != null && resultSet6.GetLength(0) > 0 && header6 != null && header6.Length > 0)
            {
                float.TryParse(resultSet6[0, 0]?.ToString(), out MIN2Consumption);
            }

            // Calculate today consumption based on IN1Consumption and IN2Consumption
            todayconsumption = IN1Consumption + IN2Consumption;
            monthconsumption = MIN1Consumption + MIN2Consumption;

            // Calculate today consumption cost
            todayconsumptioncost = todayconsumption * 752;
            todayconsumptioncost = todayconsumptioncost / 100 + 1608.4f;
            monthconsumptioncost = monthconsumption * 752;
            monthconsumptioncost = monthconsumptioncost / 100 + 1608.4f;

            if (monthconsumption < 99)
            {
                guagemax = 100;
            }
            else if (monthconsumption > 99 && monthconsumption < 999)
            {
                guagemax = 1000;
            }
            else if (monthconsumption > 999 && monthconsumption < 9999)
            {
                guagemax = 10000;
            }
            else if (monthconsumption > 9999 && monthconsumption < 99999)
            {
                guagemax = 100000;
            }
            else if (monthconsumption > 99999 && monthconsumption < 999999)
            {
                guagemax = 1000000;
            }
            else if (monthconsumption > 999999 && monthconsumption < 9999999)
            {
                guagemax = 1000000;
            }
            else if (monthconsumption > 9999999 && monthconsumption < 99999999)
            {
                guagemax = 100000000;
            }
                  
        }


        // Update variable values
        todayConsumptionVariable.Value = todayconsumption;
        todayConsumptionCostVariable.Value = todayconsumptioncost;
        averageFrequencyVariable.Value = averagefrequency;
        avgPowerFactorVariable.Value = avgpowerfactor;
        monthConsumptionVariable.Value = monthconsumption;  
        monthConsumptionCostVariable.Value =monthconsumptioncost;
        guageMaxVariable.Value = guagemax;
    }

}


