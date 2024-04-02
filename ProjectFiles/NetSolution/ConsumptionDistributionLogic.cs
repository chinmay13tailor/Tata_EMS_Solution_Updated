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
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.Metrics;
using System.Threading;
#endregion


public class ConsumptionDistributionLogic : BaseNetLogic
{
   

    public override void Start()
    {
        var owner = (ConsumptionDistributionLogger)LogicObject.Owner;
        dateVariable = owner.DateVariable;
        buttonVariable = owner.ButtonVariable;
        countVariable = owner.CountVariable;
        monthyearVariable = owner.MonthYearVariable;
        yearVariable = owner.YearVariable;
        jaceVariable = owner.JaceVariable;
        jace1Variable = owner.Jace1Variable;
        consumption1Variable = owner.Consumption1Variable;
        consumption2Variable = owner.Consumption2Variable;
        consumptionVariable = owner.ConsumptionVariable;



        periodicTask = new PeriodicTask(IncrementDecrementTask, 1000, LogicObject);
        periodicTask.Start();

    }

    public override void Stop()
    {

        periodicTask.Dispose();
        periodicTask = null;
    }

    public void IncrementDecrementTask()

    {
        String date = dateVariable.Value;
        int count = countVariable.Value;
        string jace = jaceVariable.Value;
        string jace1 = jace1Variable.Value;
        float consumption1 = consumption1Variable.Value;
        float consumption2 = consumption2Variable.Value;
        float consumption = consumptionVariable.Value;



        string monthyear = monthyearVariable.Value;
        string year = yearVariable.Value;

        bool button = buttonVariable.Value;
        var project = FTOptix.HMIProject.Project.Current;

        var myStore1 = project.GetObject("DataStores").Get<Store.Store>("ODBCDatabase");///Delete 
        var myStore2 = project.GetObject("DataStores").Get<Store.Store>("ODBCDatabase");///Jace
        var myStore3 = project.GetObject("DataStores").Get<Store.Store>("ODBCDatabase");///Consumption1
        var myStore4 = project.GetObject("DataStores").Get<Store.Store>("ODBCDatabase");///Consumption2


        object[,] resultSet1;
        string[] header1;
        object[,] resultSet2;
        string[] header2;
        object[,] resultSet3;
        string[] header3;
        object[,] resultSet4;
        string[] header4;

        if (button == true)
        {

            if (count < 1)

            {
                DateTime currentTime = DateTime.Now;
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                int currentHour = currentTime.Hour;

                // Calculate start and end times for the current day
                DateTime startTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 8, 0, 0);
                DateTime endTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 7, 59, 59).AddDays(1);
                var date1 = startTime.ToString("dd-MM-yyyy");
                // Adjust the start time if the current hour is before 8 AM
                if (currentHour < 8)
                {
                    startTime = startTime.AddDays(-1);
                    endTime = endTime.AddDays(-1);
                }

               

                String jacee = jace1.ToString();
                
                
                string st = startTime.ToString("yyyy-MMM-dd");
                string et = endTime.ToString("yyyy-MMM-dd");
                monthyear = startTime.ToString("yyyy-MM");
                year = startTime.ToString("yyyy");

                if (count == 0)
                {

                    string query1 = $"DELETE FROM ConsumptionDistribution WHERE LocalTimestamp BETWEEN '" + st + " 08:00:00' AND '" + et + " 07:59:59' AND Jace = '" + jacee + "'";
                    string query2 = $" SELECT Jace FROM DailyConsumptionAgg WHERE LocalTimestamp  BETWEEN '" + st + " 08:00:00' AND '" + et + " 07:59:59'  AND Jace = '" + jacee + "' AND Meter = 'J1_INCOMER1'";
                    string query3 = $" SELECT Consumption  FROM DailyConsumptionAgg WHERE LocalTimestamp  BETWEEN '" + st + " 08:00:00' AND '" + et + " 07:59:59'  AND Jace = '" + jacee + "' AND Meter = 'J1_INCOMER1'";
                    string query4 = $" SELECT Consumption  FROM DailyConsumptionAgg WHERE LocalTimestamp  BETWEEN '" + st + " 08:00:00' AND '" + et + " 07:59:59'  AND Jace = '" + jacee + "' AND Meter = 'J1_INCOMER2'";
                   
                    myStore1.Query(query1, out header1, out resultSet1);
                    myStore2.Query(query2, out header2, out resultSet2);
                    myStore3.Query(query3, out header3, out resultSet3);
                    myStore4.Query(query4, out header4, out resultSet4);

                    var rowCount2 = resultSet2 != null ? resultSet2.GetLength(0) : 0;
                    var columnCount2 = header2 != null ? header2.Length : 0;
                    if (rowCount2 > 0 && columnCount2 > 0)
                    {
                        var column1 = Convert.ToString(resultSet2[0, 0]);
                        jace = column1;

                    }

                    var rowCount3 = resultSet3 != null ? resultSet3.GetLength(0) : 0;
                    var columnCount3 = header3 != null ? header3.Length : 0;
                    if (rowCount3 > 0 && columnCount3 > 0)
                    {
                        var column1 = Convert.ToInt32(resultSet3[0, 0]);
                        consumption1 = column1;

                    }







                    date = date1;
                    count = count + 1;

                }

                if (count == 1)
                {

                    string query1 = $"DELETE FROM ConsumptionDistribution WHERE LocalTimestamp BETWEEN '" + st + " 08:00:00' AND '" + et + " 07:59:59' AND Jace = '" + jacee + "'";
                    string query2 = $" SELECT Jace FROM DailyConsumptionAgg WHERE LocalTimestamp  BETWEEN '" + st + " 08:00:00' AND '" + et + " 07:59:59'  AND Jace = '" + jacee + "'";
                    string query3 = $" SELECT SUM(Consumption)  FROM DailyConsumptionAgg WHERE LocalTimestamp  BETWEEN '" + st + " 08:00:00' AND '" + et + " 07:59:59'  AND Jace = '" + jacee + "'";

                    myStore1.Query(query1, out header1, out resultSet1);
                    myStore2.Query(query2, out header2, out resultSet2);
                    myStore3.Query(query3, out header3, out resultSet3);

                    var rowCount2 = resultSet2 != null ? resultSet2.GetLength(0) : 0;
                    var columnCount2 = header2 != null ? header2.Length : 0;
                    if (rowCount2 > 0 && columnCount2 > 0)
                    {
                        var column1 = Convert.ToString(resultSet2[0, 0]);
                        jace = column1;

                    }

                    var rowCount3 = resultSet3 != null ? resultSet3.GetLength(0) : 0;
                    var columnCount3 = header3 != null ? header3.Length : 0;
                    if (rowCount3 > 0 && columnCount3 > 0)
                    {
                        var column1 = Convert.ToInt32(resultSet3[0, 0]);
                        consumption = column1;

                    }
                    date = date1;
                    count = count + 1;

                }





            }
        

            else
            {
                count = 0;
            }


        }

        dateVariable.Value = date;
        countVariable.Value = count;
        jaceVariable.Value = jace;
        consumptionVariable.Value = consumption;
        monthyearVariable.Value = monthyear;
        yearVariable.Value = year;




    }
    private IUAVariable dateVariable;
    private IUAVariable buttonVariable;
    private IUAVariable countVariable;
    private IUAVariable monthyearVariable;
    private IUAVariable yearVariable;
    private IUAVariable jaceVariable;
    private IUAVariable jace1Variable;
    private IUAVariable consumption1Variable;
    private IUAVariable consumption2Variable;
    private IUAVariable consumptionVariable;
    private PeriodicTask periodicTask;
}